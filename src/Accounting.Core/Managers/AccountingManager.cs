using Accounting.Contracts;
using Accounting.Contracts.Data;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Models;
using Accounting.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace Accounting.Core.Managers
{
    public class AccountingManager : IAccountingManager
    {
        private readonly IAccountUnitOfWorkFactory _accountUnitOfWorkFactory;
        private readonly IDictionary<AccountType, decimal> _rates;

        public AccountingManager(IAccountUnitOfWorkFactory unitOfWorkFactory, IDictionary<AccountType, decimal> rates)
        {
            _accountUnitOfWorkFactory = unitOfWorkFactory;
            _rates = rates;
        }

        public OperationStatus Debit(int accountId, decimal value)
        {
            ValidateValueArgument(value);

            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                var account = unitOfWork.AccountRepository.Get(accountId);

                ValidateAccountNotFound(account, accountId);
                ValidateAccountFrozen(account);

                account.Balance += value;

                unitOfWork.AccountRepository.Update(account);

                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        public OperationStatus Credit(int accountId, decimal value)
        {
            ValidateValueArgument(value);

            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                var account = unitOfWork.AccountRepository.Get(accountId);

                ValidateAccountNotFound(account, accountId);
                ValidateAccountFrozen(account);
                ValidateInsufficientFunds(account.Balance - value);

                account.Balance -= value;

                unitOfWork.AccountRepository.Update(account);

                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        public OperationStatus Transfer(int sourceAccountId, int destinationAccountId, decimal value)
        {
            ValidateValueArgument(value);

            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                var sourceAccount = unitOfWork.AccountRepository.Get(sourceAccountId);
                var destinationAccount = unitOfWork.AccountRepository.Get(destinationAccountId);

                ValidateAccountNotFound(sourceAccount, sourceAccountId);
                ValidateAccountNotFound(destinationAccount, destinationAccountId);
                ValidateAccountFrozen(sourceAccount, destinationAccount);
                ValidateInsufficientFunds(sourceAccount.Balance - value);

                sourceAccount.Balance -= value;
                destinationAccount.Balance += value;

                unitOfWork.AccountRepository.Update(sourceAccount);
                unitOfWork.AccountRepository.Update(destinationAccount);

                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        public OperationStatus Freeze(int accountId)
        {
            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                var account = unitOfWork.AccountRepository.Get(accountId);

                ValidateAccountNotFound(account, accountId);
                ValidateAccountFrozen(account);

                account.Frozen = true;

                unitOfWork.AccountRepository.Update(account);

                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        public OperationStatus AddIntrest(int accountId)
        {
            using (var unitOfWork = _accountUnitOfWorkFactory.Create(IsolationLevel.RepeatableRead))
            {
                var account = unitOfWork.AccountRepository.Get(accountId);

                ValidateAccountNotFound(account, accountId);
                ValidateAccountFrozen(account);

                account.Balance = account.Balance*(1m + GetPercent(account.Type));

                unitOfWork.AccountRepository.Update(account);

                unitOfWork.Commit();

                return OperationStatus.Success;
            }
        }

        private decimal GetPercent(AccountType accountType)
        {
            if (_rates.ContainsKey(accountType)) return _rates[accountType];

            throw new Exception($"Account type {accountType} isn't supported.");
        }

        private void ValidateValueArgument(decimal value)
        {
            if (value <= 0)
            {
                throw new NotPositiveValueException($"Value of operation should be more than 0. Current value is {value}");
            }
        }

        private void ValidateInsufficientFunds(decimal value)
        {
            if (value < 0)
            {
                throw new InsufficientFundsException();
            }
        }

        private void ValidateAccountNotFound(Account account, int accountId)
        {
            if (account == null)
            {
                throw new AccountNotFoundException($"Account with id = {accountId} is not found.");
            }
        }

        private void ValidateAccountFrozen(params Account[] accounts)
        {
            foreach (var account in accounts)
            {
                if (account.Frozen)
                {
                    throw new AccountFrozenException($"Account with id = {account.Id} is frozen.");
                }
            }
        }
    }
}
