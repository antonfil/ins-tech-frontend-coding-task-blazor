
using System.Net.Http.Json;
using ins_tech_frontend_coding_task_blazor.Models;

namespace ins_tech_frontend_coding_task_blazor.Services;

public sealed class FleetApiClient
{
    private readonly HttpClient _http;

    public FleetApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<FleetResponse?> GetRandomFleetAsync(CancellationToken ct = default)
        => _http.GetFromJsonAsync<FleetResponse>("https://proxy.corsfix.com/?https://esa.instech.no/api/fleets/random", ct);

}
