
namespace PhysioLink.AdminPanel.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Tokens refreshed during THIS request. ApiClient is registered scoped, so these
    // live for exactly one HTTP request. The refresh token rotates on every use, and
    // Response.Cookies written mid-request are NOT reflected back into Request.Cookies —
    // so without this, a page that makes several API calls would re-read the same expired
    // access token and re-send the already-rotated (now dead) refresh token on every call
    // after the first, failing them all. Once one call refreshes, the rest reuse these.
    private string? _refreshedAccessToken;
    private string? _refreshedRefreshToken;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // -------------------------------------------------------------------------
    // Infrastructure
    // -------------------------------------------------------------------------

    // Prefer the token refreshed earlier in this request over the (stale) cookie.
    private string? CurrentAccessToken =>
        _refreshedAccessToken ?? _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];

    private string? CurrentRefreshToken =>
        _refreshedRefreshToken ?? _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient("PhysioLinkApi");
        var token  = CurrentAccessToken;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static CookieOptions TokenCookieOptions() => new CookieOptions
    {
        HttpOnly = true,
        Secure   = false,
        SameSite = SameSiteMode.Strict,
        Expires  = DateTimeOffset.UtcNow.AddHours(8)
    };

    // -------------------------------------------------------------------------
    // Token refresh core
    // -------------------------------------------------------------------------

    // Public entry kept for the login flow / callers that just want fresh tokens.
    public async Task<LoginResponse?> RefreshAsync()
    {
        var refreshToken = CurrentRefreshToken;
        if (string.IsNullOrEmpty(refreshToken)) return null;

        var client   = _httpClientFactory.CreateClient("PhysioLinkApi");   // unauthenticated
        var response = await client.PostAsJsonAsync("api/v1/auth/refresh", new { RefreshToken = refreshToken });
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    // Refreshes the session at most once per request. `expiredAccessToken` is the token
    // the caller was holding when it got a 401; if another call already rotated past it
    // while we waited on the lock, we skip a second (double-rotating) refresh.
    // Returns true if we now hold a usable access token; false if the session is dead.
    private async Task<bool> TryRefreshAsync(string? expiredAccessToken)
    {
        await _refreshLock.WaitAsync();
        try
        {
            if (_refreshedAccessToken is not null && _refreshedAccessToken != expiredAccessToken)
                return true;

            var refreshed = await RefreshAsync();
            if (refreshed?.AccessToken is null || refreshed.RefreshToken is null)
                return false;

            _refreshedAccessToken  = refreshed.AccessToken;
            _refreshedRefreshToken = refreshed.RefreshToken;

            // Persist for the NEXT request too.
            var opts = TokenCookieOptions();
            var http = _httpContextAccessor.HttpContext!;
            http.Response.Cookies.Append("auth_token",    refreshed.AccessToken,  opts);
            http.Response.Cookies.Append("refresh_token", refreshed.RefreshToken, opts);
            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    // Runs a call, refreshing once on 401. Throws SessionExpiredException when the
    // session can no longer be refreshed. Returns the (possibly retried) response.
    private async Task<HttpResponseMessage> ExecuteRawWithRefreshAsync(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var usedToken = CurrentAccessToken;
        var response  = await call(CreateAuthenticatedClient());

        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            return response;

        if (!await TryRefreshAsync(usedToken))
            throw new SessionExpiredException();

        var retry = await call(CreateAuthenticatedClient());
        if (retry.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new SessionExpiredException();

        return retry;
    }

    // Returns a deserialized T, or null on a non-auth failure. Throws on expired session.
    private async Task<T?> ExecuteWithRefreshAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var response = await ExecuteRawWithRefreshAsync(call);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>();
    }

    // Returns true on success, false on a non-auth failure. Throws on expired session.
    private async Task<bool> ExecuteWithRefreshAsync(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var response = await ExecuteRawWithRefreshAsync(call);
        return response.IsSuccessStatusCode;
    }

    // -------------------------------------------------------------------------
    // New-pattern overloads — lambda handles response checking + deserialization.
    // These can't see the 401 themselves, so a null/false result triggers one refresh
    // attempt: if the session is still alive the retry runs; if the refresh fails the
    // session is genuinely gone and we throw. Disambiguated from the HttpResponseMessage
    // overloads by lambda return type.
    // -------------------------------------------------------------------------

    // lambda returns T? (null = failure)
    private async Task<T?> ExecuteWithRefreshAsync<T>(Func<HttpClient, Task<T?>> call) where T : class
    {
        var usedToken = CurrentAccessToken;
        var result    = await call(CreateAuthenticatedClient());
        if (result is not null) return result;

        if (!await TryRefreshAsync(usedToken))
            throw new SessionExpiredException();

        return await call(CreateAuthenticatedClient());
    }

    // lambda returns bool (false = failure)
    private async Task<bool> ExecuteWithRefreshAsync(Func<HttpClient, Task<bool>> call)
    {
        var usedToken = CurrentAccessToken;
        if (await call(CreateAuthenticatedClient())) return true;

        if (!await TryRefreshAsync(usedToken))
            throw new SessionExpiredException();

        return await call(CreateAuthenticatedClient());
    }

    // -------------------------------------------------------------------------
    // Auth
    // -------------------------------------------------------------------------

    public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
    {
        var client   = _httpClientFactory.CreateClient("PhysioLinkApi");
        var response = await client.PostAsJsonAsync("api/v1/auth/login", loginRequest);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    // Re-authenticates the signed-in user before a destructive action. The API resolves
    // the account from the bearer token, so only the password travels. Returns false for
    // a wrong password (API answers 400, which keeps 401 meaning "token expired").
    public Task<bool> VerifyPasswordAsync(string password)
        => ExecuteWithRefreshAsync(
               c => c.PostAsJsonAsync("api/v1/auth/verify-password", new { Password = password }));

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var client   = CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync("api/v1/auth/logout", new { RefreshToken = refreshToken });
        return response.IsSuccessStatusCode;
    }

    // -------------------------------------------------------------------------
    // Dashboard
    // -------------------------------------------------------------------------

    public Task<DashboardStatsResponse?> GetDashboardStatsAsync()
        => ExecuteWithRefreshAsync<DashboardStatsResponse>(
               c => c.GetAsync("api/v1/admin/dashboard"));

    // -------------------------------------------------------------------------
    // Therapists
    // -------------------------------------------------------------------------

    public Task<PagedResult<TherapistResponse>?> GetTherapistsAsync(int page, int pageSize)
        => ExecuteWithRefreshAsync<PagedResult<TherapistResponse>>(
               c => c.GetAsync($"api/v1/admin/therapists?page={page}&pageSize={pageSize}"));

    public Task<TherapistResponse?> GetTherapistByIdAsync(Guid id)
        => ExecuteWithRefreshAsync<TherapistResponse>(
               c => c.GetAsync($"api/v1/admin/therapists/{id}"));

    public Task<bool> CreateTherapistAsync(CreateTherapistRequest request)
        => ExecuteWithRefreshAsync(
               c => c.PostAsJsonAsync("api/v1/admin/therapists", request));

    public Task<bool> UpdateTherapistAsync(Guid id, UpdateTherapistRequest request)
        => ExecuteWithRefreshAsync(
               c => c.PutAsJsonAsync($"api/v1/admin/therapists/{id}", request));

    public Task<bool> DeactivateTherapistAsync(Guid id)
        => ExecuteWithRefreshAsync(
               c => c.DeleteAsync($"api/v1/admin/therapists/{id}"));

    // -------------------------------------------------------------------------
    // Patients
    // -------------------------------------------------------------------------

    public Task<PagedResult<PatientResponse>?> GetPatientsAsync(int page, int pageSize, Guid? therapistId, string? search = null)
    {
        var url = $"api/v1/admin/patients?page={page}&pageSize={pageSize}";
        if (therapistId.HasValue)             url += $"&therapistId={therapistId.Value}";
        if (!string.IsNullOrEmpty(search))    url += $"&search={Uri.EscapeDataString(search)}";
        return ExecuteWithRefreshAsync<PagedResult<PatientResponse>>(c => c.GetAsync(url));
    }

    public Task<PatientDetailResponse?> GetPatientByIdAsync(Guid id)
        => ExecuteWithRefreshAsync<PatientDetailResponse>(
               c => c.GetAsync($"api/v1/admin/patients/{id}"));

    public Task<PatientResponse?> CreatePatientAsync(CreatePatientRequest request)
        => ExecuteWithRefreshAsync<PatientResponse>(
               c => c.PostAsJsonAsync("api/v1/admin/patients", request));

    public Task<bool> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
        => ExecuteWithRefreshAsync(
               c => c.PutAsJsonAsync($"api/v1/admin/patients/{id}", request));

    public Task<bool> DeactivatePatientAsync(Guid id)
        => ExecuteWithRefreshAsync(
               c => c.DeleteAsync($"api/v1/admin/patients/{id}"));

    // -------------------------------------------------------------------------
    // Appointments / Recent
    // -------------------------------------------------------------------------

    public Task<PagedResult<AppointmentSummaryResponse>?> GetUpcomingAppointmentsAsync(int count)
    {
        var from = Uri.EscapeDataString(DateTime.UtcNow.ToString("o"));
        // Dashboard "Upcoming Appointments" — confirmed only, so a pending request
        // or a rejected/cancelled/expired one doesn't show as if it were happening.
        return ExecuteWithRefreshAsync<PagedResult<AppointmentSummaryResponse>>(
                   c => c.GetAsync($"api/v1/admin/appointments?page=1&pageSize={count}&from={from}&status=Confirmed"));
    }

    public Task<PagedResult<PatientResponse>?> GetRecentPatientsAsync(int count)
        => ExecuteWithRefreshAsync<PagedResult<PatientResponse>>(
               c => c.GetAsync($"api/v1/admin/patients?page=1&pageSize={count}"));

    // -------------------------------------------------------------------------
    // Exercises
    // Backend returns List<AdminExerciseDto> — no pagination wrapper.
    // search/difficulty are forwarded for future backend support; currently ignored by the API.
    // -------------------------------------------------------------------------

    public async Task<List<ExerciseResponse>?> GetExercisesAsync(string? search = null, string? difficulty = null, string? category = null)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var url = "api/v1/admin/exercises";
            var qs  = new List<string>();
            if (!string.IsNullOrEmpty(search))     qs.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrEmpty(difficulty)) qs.Add($"difficulty={Uri.EscapeDataString(difficulty)}");
            if (!string.IsNullOrEmpty(category))   qs.Add($"category={Uri.EscapeDataString(category)}");
            if (qs.Count > 0) url += "?" + string.Join("&", qs);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            try
            {
                return await response.Content.ReadFromJsonAsync<List<ExerciseResponse>>();
            }
            catch (System.Text.Json.JsonException)
            {
                // Deserialization failed (e.g. unexpected enum format from API).
                // Return null so the caller can handle gracefully.
                return null;
            }
        });
    }

    public Task<ExerciseResponse?> GetExerciseByIdAsync(Guid id)
        => ExecuteWithRefreshAsync<ExerciseResponse>(
               c => c.GetAsync($"api/v1/admin/exercises/{id}"));

    public async Task<bool> CreateExerciseAsync(CreateExerciseRequest request)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.PostAsJsonAsync("api/v1/admin/exercises", request);
            return response.IsSuccessStatusCode;
        });
    }

    // -------------------------------------------------------------------------
    // Assignments
    // -------------------------------------------------------------------------

    public Task<AssignmentResponse?> GetAssignmentByIdAsync(Guid assignmentId)
        => ExecuteWithRefreshAsync<AssignmentResponse>(
               c => c.GetAsync($"api/v1/admin/assignments/{assignmentId}"));

    public async Task<bool> AssignExerciseAsync(Guid patientId, AssignExerciseRequest request)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.PostAsJsonAsync($"api/v1/admin/patients/{patientId}/assignments", request);
            return response.IsSuccessStatusCode;
        });
    }

    public async Task<bool> UpdateAssignmentAsync(Guid assignmentId, UpdateAssignmentRequest request)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.PutAsJsonAsync($"api/v1/admin/assignments/{assignmentId}", request);
            return response.IsSuccessStatusCode;
        });
    }

    public async Task<bool> DeleteAssignmentAsync(Guid assignmentId)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.DeleteAsync($"api/v1/admin/assignments/{assignmentId}");
            return response.IsSuccessStatusCode;
        });
    }

    // -------------------------------------------------------------------------
    // Appointments
    // -------------------------------------------------------------------------

    public async Task<PagedResult<AppointmentSummaryResponse>?> GetAppointmentsAsync(
        int page, int pageSize, Guid? patientId = null, string? status = null,
        DateTime? from = null, DateTime? to = null)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var qs = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (patientId.HasValue)          qs.Add($"patientId={patientId.Value}");
            if (!string.IsNullOrEmpty(status)) qs.Add($"status={Uri.EscapeDataString(status)}");
            if (from.HasValue)               qs.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
            if (to.HasValue)                 qs.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");

            var response = await client.GetAsync($"api/v1/admin/appointments?{string.Join("&", qs)}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PagedResult<AppointmentSummaryResponse>>();
        });
    }

    // Archive of past-due appointments (Completed / Rejected / Expired / Cancelled).
    public async Task<PagedResult<AppointmentSummaryResponse>?> GetAppointmentHistoryAsync(
        int page, int pageSize, string? status = null, string? therapistName = null)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var qs = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrEmpty(status))        qs.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(therapistName)) qs.Add($"therapistName={Uri.EscapeDataString(therapistName)}");

            var response = await client.GetAsync($"api/v1/admin/appointments/history?{string.Join("&", qs)}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PagedResult<AppointmentSummaryResponse>>();
        });
    }

    public async Task<AppointmentSummaryResponse?> GetAppointmentByIdAsync(Guid id)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.GetAsync($"api/v1/admin/appointments/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AppointmentSummaryResponse>();
        });
    }

    public async Task<(bool Success, string? Error)> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        var response = await ExecuteRawWithRefreshAsync(
            c => c.PostAsJsonAsync("api/v1/admin/appointments", request));
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }

    public async Task<(bool Success, string? Error)> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
    {
        var response = await ExecuteRawWithRefreshAsync(
            c => c.PutAsJsonAsync($"api/v1/admin/appointments/{id}", request));
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }

    public async Task<bool> CancelAppointmentAsync(Guid id)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var response = await client.DeleteAsync($"api/v1/admin/appointments/{id}");
            return response.IsSuccessStatusCode;
        });
    }

    // -------------------------------------------------------------------------
    // Slot scheduling (spec Phase 2 / 6)
    // -------------------------------------------------------------------------

    public Task<SlotGridResponse?> GetTherapistScheduleAsync(Guid therapistId, DateTime weekStart)
        => ExecuteWithRefreshAsync<SlotGridResponse>(
               c => c.GetAsync($"api/v1/admin/therapists/{therapistId}/slots?weekStart={weekStart:yyyy-MM-dd}"));

    // scheduledAt is sent as a bare wall-clock string (no offset) so it is never
    // shifted by a server timezone; the API interprets it as UTC.
    public Task<bool> CreateSlotAsync(Guid therapistId, DateTime scheduledAtUtc)
        => ExecuteWithRefreshAsync(
               c => c.PostAsJsonAsync($"api/v1/admin/therapists/{therapistId}/slots",
                   new { scheduledAt = scheduledAtUtc.ToString("yyyy-MM-ddTHH:mm:ss") }));

    public Task<bool> DeleteSlotAsync(Guid therapistId, DateTime scheduledAtUtc)
        => ExecuteWithRefreshAsync(
               c => c.DeleteAsync($"api/v1/admin/therapists/{therapistId}/slots?scheduledAt={Uri.EscapeDataString(scheduledAtUtc.ToString("yyyy-MM-ddTHH:mm:ss"))}"));

    // -------------------------------------------------------------------------
    // Appointment requests queue + decisions (spec Phase 4 / 6)
    // -------------------------------------------------------------------------

    public Task<List<AppointmentRequestResponse>?> GetAppointmentRequestsAsync(Guid? therapistId)
    {
        var url = "api/v1/admin/appointments/requests";
        if (therapistId.HasValue) url += $"?therapistId={therapistId.Value}";
        return ExecuteWithRefreshAsync<List<AppointmentRequestResponse>>(c => c.GetAsync(url));
    }

    public Task<bool> AcceptAppointmentRequestAsync(Guid id)
        => ExecuteWithRefreshAsync(c => c.PutAsync($"api/v1/admin/appointments/{id}/accept", null));

    public Task<bool> RejectAppointmentRequestAsync(Guid id)
        => ExecuteWithRefreshAsync(c => c.PutAsync($"api/v1/admin/appointments/{id}/reject", null));

    // Cancels a previously-confirmed appointment (-> CancelledByClinic, slot freed).
    // Distinct from CancelAppointmentAsync which soft-deletes via the legacy endpoint.
    public Task<bool> CancelConfirmedAppointmentAsync(Guid id)
        => ExecuteWithRefreshAsync(c => c.PutAsync($"api/v1/admin/appointments/{id}/cancel", null));
}
