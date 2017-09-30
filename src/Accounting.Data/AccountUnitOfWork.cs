using Accounting.Contracts.Data;
using System;
using System.Transactions;

namespace Accounting.Data
{
    public class AccountUnitOfWork : IAccountUnitOfWork
    {
        private readonly AccountContext _context;
        private readonly TransactionScope _transactionScope;

        public AccountUnitOfWork(AccountContext context, TransactionScope transactionScope)
        {
            _context = context;
            _transactionScope = transactionScope;
            AccountRepository = new AccountRepository(context);
        }

        public IAccountRepository AccountRepository { get; }

        public void Commit()
        {
            _context.SaveChanges();
            _transactionScope.Complete();
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                    _transactionScope.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
