using Accounting.Contracts;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Security;
using Accounting.Core.Exceptions;
using Accounting.Service.Contracts;
using Accounting.Service.Models;
using NLog;
using System;

namespace Accounting.Service.Services
{
    public class AccountingService : IAccountingService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISignInManager _signInManager;
        private readonly IAccountingManager _accountingManager;

        public AccountingService(ISignInManager signInManager, IAccountingManager accountingManager)
        {
            _signInManager = signInManager;
            _accountingManager = accountingManager;
        }

        public OperationResult Debit(Login login, int accountId, decimal value)
        {
            Logger.Debug($"Performing Debit operation: LoginName = {login.Name}, AccountId = {accountId}, Value = {value}");

            return Perform(login, () => _accountingManager.Debit(accountId, value));
        }

        public OperationResult Credit(Login login, int accountId, decimal value)
        {
            Logger.Debug($"Performing Credit operation: LoginName = {login.Name}, AccountId = {accountId}, Value = {value}");

            return Perform(login, () => _accountingManager.Credit(accountId, value));
        }

        public OperationResult Transfer(Login login, int sourceAccountId, int destinationAccountId, decimal value)
        {
            Logger.Debug($"Performing Transfer operation: LoginName = {login.Name}, FromId = {sourceAccountId}, ToId = {destinationAccountId}, Value = {value}");

            return Perform(login, () => _accountingManager.Transfer(sourceAccountId, destinationAccountId, value));
        }

        public OperationResult Freeze(Login login, int accountId)
        {
            Logger.Debug($"Performing Freeze operation: LoginName = {login.Name}, AccountId = {accountId}");

            return Perform(login, () => _accountingManager.Freeze(accountId));
        }

        public OperationResult AddIntrest(Login login, int accountId)
        {
            Logger.Debug($"Performing AddIntrest operation: LoginName = {login.Name}, AccountId = {accountId}");

            return Perform(login, () => _accountingManager.AddIntrest(accountId));
        }

        private OperationResult Perform(Login login, Func<OperationStatus> operation)
        {
            var signInStatus = _signInManager.Login(login.Name, login.Pin);
            if (signInStatus == SignInStatus.Failure) return new OperationResult { Status = OperationStatus.AccessDenied };

            try
            {
                var status = operation();

                return new OperationResult {Status = status};
            }
            catch (AccountNotFoundException ex)
            {
                Logger.Error(ex);

                return new OperationResult {Status = OperationStatus.AccountNotFound, ErrorMessage = ex.Message};
            }
            catch (AccountFrozenException ex)
            {
                Logger.Error(ex);

                return new OperationResult { Status = OperationStatus.AccountFrozen, ErrorMessage = ex.Message };
            }
            catch (InsufficientFundsException ex)
            {
                Logger.Error(ex);

                return new OperationResult { Status = OperationStatus.InsufficientFunds, ErrorMessage = ex.Message };
            }
            catch (NotPositiveValueException ex)
            {
                Logger.Error(ex);

                return new OperationResult { Status = OperationStatus.InvalidArgument, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return new OperationResult { Status = OperationStatus.Unknown };
            }
        }
    }
}
