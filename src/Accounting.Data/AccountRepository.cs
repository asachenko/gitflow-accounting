using Accounting.Contracts.Data;
using Accounting.Contracts.Models;

namespace Accounting.Data
{
    public class AccountRepository : GenericRepository<AccountContext, Account>, IAccountRepository
    {
        public AccountRepository(AccountContext context) : base(context) {}
    }
}
