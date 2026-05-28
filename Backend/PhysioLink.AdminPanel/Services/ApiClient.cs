
namespace PhysioLink.AdminPanel.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // -------------------------------------------------------------------------
    // Infrastructure
    // -------------------------------------------------------------------------

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient("PhysioLinkApi");
        var token  = _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
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
    // Step 1 — RefreshAsync
    // -------------------------------------------------------------------------

    public async Task<LoginResponse?> RefreshAsync()
    {
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken)) return null;

        var client   = _httpClientFactory.CreateClient("PhysioLinkApi");   // unauthenticated
        var response = await client.PostAsJsonAsync("api/v1/auth/refresh", new { RefreshToken = refreshToken });
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    // -------------------------------------------------------------------------
    // Step 2 — Token refresh core
    // -------------------------------------------------------------------------

    // Attempts to refresh tokens, updates cookies, and retries the call.
    // Returns the retry HttpResponseMessage, or null if refresh failed (SessionExpired flag is set).
    private async Task<HttpResponseMessage?> RetryAfterRefresh(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var refreshed = await RefreshAsync();
        if (refreshed == null) return null;

        // Persist new tokens for subsequent requests in this session
        var opts = TokenCookieOptions();
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("auth_token",    refreshed.AccessToken!,  opts);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refresh_token", refreshed.RefreshToken!, opts);

        // Must build a new client with the fresh token — Request.Cookies won't reflect
        // Response.Cookies within the same HTTP request
        var retryClient = _httpClientFactory.CreateClient("PhysioLinkApi");
        retryClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshed.AccessToken!);

        return await call(retryClient);
    }

    // Returns a deserialized T, or null on failure / expired session.
    private async Task<T?> ExecuteWithRefreshAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var client   = CreateAuthenticatedClient();
        var response = await call(client);

        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>();
        }

        var retryResponse = await RetryAfterRefresh(call);
        if (retryResponse == null || !retryResponse.IsSuccessStatusCode) return default;
        return await retryResponse.Content.ReadFromJsonAsync<T>();
    }

    // Returns true on success, false on failure / expired session.
    private async Task<bool> ExecuteWithRefreshAsync(Func<HttpClient, Task<HttpResponseMessage>> call)
    {
        var client   = CreateAuthenticatedClient();
        var response = await call(client);

        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            return response.IsSuccessStatusCode;

        var retryResponse = await RetryAfterRefresh(call);
        return retryResponse?.IsSuccessStatusCode == true;
    }

    // -------------------------------------------------------------------------
    // New-pattern overloads — lambda handles response checking + deserialization
    // Disambiguated from the HttpResponseMessage overloads by lambda return type.
    // -------------------------------------------------------------------------

    // lambda returns T? (null = failure)
    private async Task<T?> ExecuteWithRefreshAsync<T>(Func<HttpClient, Task<T?>> call) where T : class
    {
        var result = await call(CreateAuthenticatedClient());
        if (result is not null) return result;

        var refreshed = await RefreshAsync();
        if (refreshed is null) return default;

        var opts = TokenCookieOptions();
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("auth_token",    refreshed.AccessToken!,  opts);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refresh_token", refreshed.RefreshToken!, opts);

        var retryClient = _httpClientFactory.CreateClient("PhysioLinkApi");
        retryClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshed.AccessToken!);

        return await call(retryClient);
    }

    // lambda returns bool (false = failure)
    private async Task<bool> ExecuteWithRefreshAsync(Func<HttpClient, Task<bool>> call)
    {
        if (await call(CreateAuthenticatedClient())) return true;

        var refreshed = await RefreshAsync();
        if (refreshed is null) return false;

        var opts = TokenCookieOptions();
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("auth_token",    refreshed.AccessToken!,  opts);
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refresh_token", refreshed.RefreshToken!, opts);

        var retryClient = _httpClientFactory.CreateClient("PhysioLinkApi");
        retryClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshed.AccessToken!);

        return await call(retryClient);
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
        return ExecuteWithRefreshAsync<PagedResult<AppointmentSummaryResponse>>(
                   c => c.GetAsync($"api/v1/admin/appointments?page=1&pageSize={count}&from={from}"));
    }

    public Task<PagedResult<PatientResponse>?> GetRecentPatientsAsync(int count)
        => ExecuteWithRefreshAsync<PagedResult<PatientResponse>>(
               c => c.GetAsync($"api/v1/admin/patients?page=1&pageSize={count}"));

    // -------------------------------------------------------------------------
    // Exercises
    // Backend returns List<AdminExerciseDto> — no pagination wrapper.
    // search/difficulty are forwarded for future backend support; currently ignored by the API.
    // -------------------------------------------------------------------------

    public async Task<List<ExerciseResponse>?> GetExercisesAsync(string? search = null, string? difficulty = null)
    {
        return await ExecuteWithRefreshAsync(async client =>
        {
            var url = "api/v1/admin/exercises";
            var qs  = new List<string>();
            if (!string.IsNullOrEmpty(search))     qs.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrEmpty(difficulty)) qs.Add($"difficulty={Uri.EscapeDataString(difficulty)}");
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
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync("api/v1/admin/appointments", request);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }

    public async Task<(bool Success, string? Error)> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PutAsJsonAsync($"api/v1/admin/appointments/{id}", request);
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
}
