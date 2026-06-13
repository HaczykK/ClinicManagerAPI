using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;

namespace ClinicManagerAPI.PerformanceTests;

public static class VisitsLoadTest
{
    public static void Run()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7050";
        var httpClient = Http.CreateDefaultClient();

        var scenario = Scenario.Create("get_active_visits", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/visits/active");
            return await Http.Send(httpClient, request);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(30)));

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv, ReportFormat.Txt)
            .Run();
    }
}
