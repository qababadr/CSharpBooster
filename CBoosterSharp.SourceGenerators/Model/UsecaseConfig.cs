namespace CBoosterSharp.Generator.Model;

public sealed class UsecaseConfig
{
    public bool PrefixWithRepo { get; set; }
    public bool GenerateWrapper { get; set; }
    public bool GenerateDI { get; set; }
    public DIScope DIScope { get; set; }
    public Visibility Visibility { get; set; }
}
