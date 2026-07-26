using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Arlecchino.Packages.Scanning;

internal sealed class ListReport
{
    public List<ReportProblem>? Problems { get; set; }

    public List<ReportProject>? Projects { get; set; }
}

internal sealed class ReportProblem
{
    public string? Text { get; set; }

    public string? Level { get; set; }
}

internal sealed class ReportProject
{
    public string? Path { get; set; }

    public List<ReportFramework>? Frameworks { get; set; }
}

internal sealed class ReportFramework
{
    public string? Framework { get; set; }

    public List<ReportPackage>? TopLevelPackages { get; set; }

    public List<ReportPackage>? TransitivePackages { get; set; }
}

internal sealed class ReportPackage
{
    public string? Id { get; set; }

    public string? RequestedVersion { get; set; }

    public string? ResolvedVersion { get; set; }

    public string? LatestVersion { get; set; }

    public List<ReportVulnerability>? Vulnerabilities { get; set; }

    public List<string>? DeprecationReasons { get; set; }

    public ReportAlternative? AlternativePackage { get; set; }
}

internal sealed class ReportVulnerability
{
    public string? Severity { get; set; }

    [JsonPropertyName("advisoryurl")]
    public string? AdvisoryUrl { get; set; }
}

internal sealed class ReportAlternative
{
    public string? Id { get; set; }

    public string? VersionRange { get; set; }
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ListReport))]
internal sealed partial class ReportJson : JsonSerializerContext;
