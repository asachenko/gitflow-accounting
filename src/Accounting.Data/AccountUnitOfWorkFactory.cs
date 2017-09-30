using Accounting.Contracts.Data;
using System.Transactions;

namespace Accounting.Data
{
    public class AccountUnitOfWorkFactory : IAccountUnitOfWorkFactory
    {
        private readonly string _connectionString;

        public AccountUnitOfWorkFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IAccountUnitOfWork Create(IsolationLevel isolationLevel)
        {
            var transactionOptions = new TransactionOptions {IsolationLevel = isolationLevel};

            return new AccountUnitOfWork(new AccountContext(_connectionString), new TransactionScope(TransactionScopeOption.Required, transactionOptions));
        }
    }
}