namespace Accounting.Contracts.Security
{
    public interface ISignInManager
    {
        SignInStatus Login(string name, string pin);
    }
}
