using Accounting.Contracts;
using Accounting.Contracts.Data;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Models;
using System.Transactions;

namespace Accounting.Core.Managers
{
    public class AccountingAdministrationManager : IAccountingAdministrationManager
    {
        private readonly IAccountUnitOfWorkFactory _accountUnitOfWorkFactory;

        public AccountingAdministrationManager(IAccountUnitOfWorkFactory unitOfWorkFactory)
        {
            _accountUnitOfWorkFactory = unitOfWorkFactory;
        }

        public CreationResult<Account> Create(Account account)
        {
            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.ReadCommitted))
            {
                unitOfWork.AccountRepository.Insert(account);
                unitOfWork.Commit();
                
                return new CreationResult<Account>(OperationStatus.Success, account);
            }
        }

        public OperationStatus Edit(Account account)
        {
            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                unitOfWork.AccountRepository.Update(account);
                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        public OperationStatus Delete(int accountId)
        {
            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                unitOfWork.AccountRepository.Delete(accountId);
                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }
    }
}
