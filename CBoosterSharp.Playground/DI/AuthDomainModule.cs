using CBoosterSharp.Generator.Attributes;
using CBoosterSharp.Generator.Model;
using CBoosterSharp.Playground.Util;

namespace CBoosterSharp.Playground.DI;

[Module]
public class AuthDomainModule
{
    [Provides(DIScope.Singleton)]
    public static FilePathService ProvideFilePathService()
    {
        return new FilePathService();
    }
}
