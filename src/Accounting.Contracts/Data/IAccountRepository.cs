using Accounting.Contracts.Models;

namespace Accounting.Contracts.Data
{
    public interface IAccountRepository
    {
        Account Get(int id);

        void Insert(Account account);

        void Update(Account account);

        void Delete(int id);
    }
}