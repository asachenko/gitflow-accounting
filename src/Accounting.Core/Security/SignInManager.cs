using Accounting.Contracts.Security;
using System.Collections.Generic;

namespace Accounting.Core.Security
{
    public class SignInManager : ISignInManager
    {
        private readonly IDictionary<string, string> _users;

        public SignInManager(IDictionary<string, string> users)
        {
            _users = users;
        }

        public SignInStatus Login(string name, string password)
        {
            if (_users.ContainsKey(name) && _users[name] == password)
            {
                return SignInStatus.Success;
            }

            return SignInStatus.Failure;
        }
    }
}
