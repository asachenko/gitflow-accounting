using Accounting.Contracts;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Models;
using Accounting.Contracts.Security;
using Accounting.Service.Contracts;
using Accounting.Service.Models;
using NLog;
using System;

namespace Accounting.Service.Services
{
    public class AccountingAdministrationService : IAccountingAdministrationService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISignInManager _signInManager;
        private readonly IAccountingAdministrationManager _accountingAdministrationManager;

        public AccountingAdministrationService(ISignInManager signInManager, IAccountingAdministrationManager accountingAdministrationManager)
        {
            _signInManager = signInManager;
            _accountingAdministrationManager = accountingAdministrationManager;
        }

        public OperationCreationResult<AccountModel> Create(Login login, AccountModel account)
        {
            var operationCreationResult = new OperationCreationResult<AccountModel> {Status = OperationStatus.Failure};

            var operationResult = Perform(login, () =>
            {
                var creationResult = _accountingAdministrationManager.Create(Map(account));

                operationCreationResult.Result = Map(creationResult.Result);

                return creationResult.Status;
            });

            operationCreationResult.Status = operationResult.Status;

            return operationCreationResult;
        }

        public OperationResult Edit(Login login, AccountModel account)
        {
            return Perform(login, () => _accountingAdministrationManager.Edit(Map(account)));
        }

        public OperationResult Delete(Login login, int accountId)
        {
            return Perform(login, () => _accountingAdministrationManager.Delete(accountId));
        }

        private Account Map(AccountModel accountModel)
        {
            // Better to replace on using AutoMapper
            return new Account
            {
                Balance = accountModel.Balance,
                Frozen = accountModel.Frozen,
                Id = accountModel.Id,
                Type = accountModel.Type
            };
        }

        private AccountModel Map(Account account)
        {
            // Better to replace on using AutoMapper
            return new AccountModel
            {
                Balance = account.Balance,
                Frozen = account.Frozen,
                Id = account.Id,
                Type = account.Type
            };
        }

        private OperationResult Perform(Login login, Func<OperationStatus> operation)
        {
            var signInStatus = _signInManager.Login(login.Name, login.Pin);
            if (signInStatus == SignInStatus.Failure) return new OperationResult { Status = OperationStatus.AccessDenied };

            try
            {
                var status = operation();

                return new OperationResult { Status = status };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return new OperationResult { Status = OperationStatus.Unknown };
            }
        }
    }
}
