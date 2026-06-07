namespace CBoosterSharp.Playground.Domain;

internal class AuthRepositoryImpl : IAuthRepository
{
    public Task<int> Login(string email, string password, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task Logout()
    {
        return Task.CompletedTask;
    }
}
