namespace CBoosterSharp.Generator.Model;

internal sealed class ModuleModel
{
    public string Namespace { get; set; } = "";
    public string FeatureName { get; set; } = "";

    public List<string> Registrations { get; } = new();
    public List<string> Providers { get; } = new();
}
