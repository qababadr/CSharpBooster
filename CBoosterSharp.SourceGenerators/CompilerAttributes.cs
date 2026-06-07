namespace System.Runtime.CompilerServices;

// Minimal stubs to satisfy older target frameworks / compilers that expect these attributes.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event, Inherited = false)]
public sealed class RequiredMemberAttribute : Attribute
{
    public RequiredMemberAttribute()
    {
    }
}

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public string FeatureName { get; }
    public CompilerFeatureRequiredAttribute()
    {
        FeatureName = string.Empty;
    }

    public CompilerFeatureRequiredAttribute(string featureName)
    {
        FeatureName = featureName;
    }

    public CompilerFeatureRequiredAttribute(string featureName, string url)
    {
        FeatureName = featureName;
    }

    public CompilerFeatureRequiredAttribute(Type featureType)
    {
        FeatureName = featureType?.FullName ?? string.Empty;
    }
}
