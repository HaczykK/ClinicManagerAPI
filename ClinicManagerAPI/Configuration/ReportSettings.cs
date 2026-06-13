namespace ClinicManagerAPI.Configuration;

public class ReportSettings
{
    public string OutputDirectory { get; set; } = "Reports";
    public string FileName { get; set; } = "raport-nadchodzace-wizyty.pdf";
    public int IntervalMinutes { get; set; } = 1440;
}
