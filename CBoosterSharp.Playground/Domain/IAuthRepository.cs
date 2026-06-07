using CBoosterSharp.Generator.Attributes;
using CBoosterSharp.Generator.Model;

namespace CBoosterSharp.Playground.Domain;

[Usecaseable(
    GenerateDI = true,
    GenerateWrapper = true,
    DIScope = DIScope.Singleton
)]
[Injectable(implementation: nameof(AuthRepositoryImpl))]
public interface IAuthRepository
{
    Task<Int32> Login(
        string email,
        string password,
        CancellationToken cancellationToken = default
    );

    Task Logout();
}
