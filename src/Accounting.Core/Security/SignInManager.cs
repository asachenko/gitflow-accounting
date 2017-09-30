using System.Collections.Generic;
using Accounting.Contracts.Security;

namespace Accounting.Core.Security
{
    public class SignInManager : ISignInManager
    {
        private readonly IDictionary<string, string> _users;

        public SignInManager(IDictionary<string, string> users)
        {
            _users = users;
        }

        public SignInStatus Login(string name, string pin)
        {
            if (_users.ContainsKey(name) && _users[name] == pin)
            {
                return SignInStatus.Success;
            }

            return SignInStatus.Failure;
        }
    }
}
