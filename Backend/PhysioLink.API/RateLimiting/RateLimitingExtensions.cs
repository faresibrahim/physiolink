using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace PhysioLink.API.RateLimiting;

/// <summary>
/// Central configuration for the API's rate limiting. Uses the built-in
/// <c>Microsoft.AspNetCore.RateLimiting</c> middleware (no extra package on .NET 9).
///
/// Two layers:
///   1. A global limiter that caps every request per client, guarding the API
///      against a single noisy client / scraper.
///   2. A stricter named policy (<see cref="AuthPolicy"/>) for the login endpoint
///      to slow credential-stuffing / brute-force attempts.
///
/// Both partition on the client IP — which only makes sense for UNTRUSTED clients
/// that each have their own IP (the Flutter app hitting the API directly). The
/// admin panel is a trusted server-side client: every clinic admin's request comes
/// from the one admin-panel server IP, so IP partitioning would throttle all admins
/// collectively. Such first-party traffic identifies itself with a shared secret
/// header (<see cref="InternalApiKeyHeader"/>) and is exempted entirely.
///
/// IP note: behind a reverse proxy (Railway / Docker) the socket address is the
/// proxy for every request, so we read the left-most <c>X-Forwarded-For</c> entry
/// first. That header is client-supplied and spoofable — adequate for coarse abuse
/// protection, not a hard security boundary.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>Named policy applied to authentication endpoints (login).</summary>
    public const string AuthPolicy = "auth";

    /// <summary>Header a trusted first-party server sends to bypass rate limiting.</summary>
    public const string InternalApiKeyHeader = "X-Internal-Api-Key";

    /// <param name="internalApiKey">
    /// Shared secret that exempts first-party callers. When null/empty, no caller is
    /// exempt (fail closed) — set INTERNAL_API_KEY in production on both the API and
    /// the admin panel so admin traffic is not IP-throttled.
    /// </param>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, string? internalApiKey)
    {
        services.AddRateLimiter(options =>
        {
            // Return 429 (not the default 503) and tell well-behaved clients when to retry.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = static (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return ValueTask.CompletedTask;
            };

            // Layer 1: global baseline — every request passes through this limiter.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (IsTrustedInternalClient(httpContext, internalApiKey))
                {
                    return RateLimitPartition.GetNoLimiter("internal");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Layer 2: stricter per-IP limit for login (opt-in via [EnableRateLimiting(AuthPolicy)]).
            options.AddPolicy(AuthPolicy, httpContext =>
            {
                if (IsTrustedInternalClient(httpContext, internalApiKey))
                {
                    return RateLimitPartition.GetNoLimiter("internal");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    private static bool IsTrustedInternalClient(HttpContext httpContext, string? internalApiKey)
    {
        if (string.IsNullOrEmpty(internalApiKey))
        {
            return false;
        }

        var provided = httpContext.Request.Headers[InternalApiKeyHeader].FirstOrDefault();
        return provided is not null && string.Equals(provided, internalApiKey, StringComparison.Ordinal);
    }

    private static string GetClientIp(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // X-Forwarded-For may be a comma-separated chain; the original client is left-most.
            var first = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
