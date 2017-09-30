using Accounting.Contracts;
using Accounting.Contracts.Data;
using Accounting.Contracts.Models;
using Accounting.Core.Managers;
using Accounting.Core.Security;
using Accounting.Service.Models;
using Accounting.Service.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Transactions;

namespace Accounting.ServiceTests
{
    [TestClass]
    public class AccountingServiceTests
    {
        private static readonly Login Login = new Login {Name = "test", Pin = "1111"};

        private static readonly IDictionary<AccountType, decimal> Rates = new Dictionary<AccountType, decimal>
            {
                {AccountType.Current, 0.015m},
                {AccountType.Savings, 0.03m}

            };

        private static AccountingManager CreateAccountingManager(Account sourceAccount, Account destinationAccount = null)
        {
            var repoMock = new Mock<IAccountRepository>();

            repoMock.Setup(x => x.Get(sourceAccount.Id)).Returns(sourceAccount);

            if (destinationAccount != null)
            {
                repoMock.Setup(x => x.Get(destinationAccount.Id)).Returns(destinationAccount);
            }

            var unitOfWork = new Mock<IAccountUnitOfWork>();
            unitOfWork.SetupGet(x => x.AccountRepository).Returns(repoMock.Object);

            var unitOfWorkFactory = new Mock<IAccountUnitOfWorkFactory>();
            unitOfWorkFactory.Setup(x => x.Create(IsolationLevel.RepeatableRead)).Returns(unitOfWork.Object);

            var accountingService = new AccountingManager(unitOfWorkFactory.Object, Rates);
            return accountingService;
        }

        private static AccountingService CreateAccountingService(Account sourceAccount, Account destinationAccount = null)
        {
            var accountingManager = CreateAccountingManager(sourceAccount, destinationAccount);

            var signInManager = new SignInManager(new Dictionary<string, string> {{Login.Name, Login.Pin}});
            
            return new AccountingService(signInManager, accountingManager);
        }

        [TestMethod]
        public void AllOperations_Login_Failure()
        {
            var originalBalance = 1m;
            var delta = 0.01m;

            var account = new Account { Id = 1, Balance = originalBalance };
            var login = new Login { Name = "test", Pin = "1234" };

            var accountingService = CreateAccountingService(account);

            var debitResult = accountingService.Debit(login, account.Id, delta);
            Assert.AreEqual(debitResult.Status, OperationStatus.AccessDenied);
            Assert.AreEqual(account.Balance, originalBalance);

            var creditResult = accountingService.Credit(login, account.Id, delta);
            Assert.AreEqual(creditResult.Status, OperationStatus.AccessDenied);
            Assert.AreEqual(account.Balance, originalBalance);

            var transferResult = accountingService.Transfer(login, account.Id, 2, delta);
            Assert.AreEqual(transferResult.Status, OperationStatus.AccessDenied);
            Assert.AreEqual(account.Balance, originalBalance);

            var freezeResult = accountingService.Freeze(login, account.Id);
            Assert.AreEqual(freezeResult.Status, OperationStatus.AccessDenied);
            Assert.AreEqual(account.Balance, originalBalance);

            var addIntrestResult = accountingService.AddIntrest(login, account.Id);
            Assert.AreEqual(addIntrestResult.Status, OperationStatus.AccessDenied);
            Assert.AreEqual(account.Balance, originalBalance);
        }
        
        [TestMethod]
        public void Debit_PositiveValue_Success()
        {
            var originalBalance = 1m;
            var delta = 0.01m;

            var account = new Account { Id = 1, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Debit(Login, account.Id, delta);

            Assert.AreEqual(result.Status, OperationStatus.Success);
            Assert.AreEqual(account.Balance, originalBalance + delta);
        }

        [TestMethod]
        public void Debit_NegativeValue_InvalidArgument()
        {
            var originalBalance = 1m;
            var debitValue = -0.01m;

            var account = new Account { Id = 1, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Debit(Login, account.Id, debitValue);

            Assert.AreEqual(result.Status, OperationStatus.InvalidArgument);
            Assert.AreEqual(account.Balance, originalBalance);
        }

        [TestMethod]
        public void Debit_FrozenTrue_AccountFrozen()
        {
            var originalBalance = 1m;
            var debitValue = 0.01m;

            var account = new Account { Id = 1, Frozen = true, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Debit(Login, account.Id, debitValue);

            Assert.AreEqual(result.Status, OperationStatus.AccountFrozen);
            Assert.AreEqual(account.Balance, originalBalance);
        }

        
        [TestMethod]
        public void Credit_PositiveValue_Success()
        {
            var originalBalance = 1m;
            var delta = 0.01m;

            var account = new Account { Id = 1, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Credit(Login, account.Id, delta);

            Assert.AreEqual(result.Status, OperationStatus.Success);
            Assert.AreEqual(account.Balance, originalBalance - delta);
        }

        [TestMethod]
        public void Credit_NegativeValue_InvalidArgument()
        {
            var originalBalance = 1m;
            var delta = -0.01m;

            var account = new Account { Id = 1, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Credit(Login, account.Id, delta);

            Assert.AreEqual(result.Status, OperationStatus.InvalidArgument);
            Assert.AreEqual(account.Balance, originalBalance);
        }

        [TestMethod]
        public void Credit_FrozenTrue_AccountFrozen()
        {
            var originalBalance = 1m;
            var delta = 0.01m;

            var account = new Account { Id = 1, Frozen = true, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Credit(Login, account.Id, delta);

            Assert.AreEqual(result.Status, OperationStatus.AccountFrozen);
            Assert.AreEqual(account.Balance, originalBalance);
        }

        [TestMethod]
        public void Credit_LessMoney_InsufficientFunds()
        {
            var originalBalance = 1m;
            var delta = 1.01m;

            var account = new Account { Id = 1, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var result = accountingService.Credit(Login, account.Id, delta);

            Assert.AreEqual(result.Status, OperationStatus.InsufficientFunds);
            Assert.AreEqual(account.Balance, originalBalance);
        }

        [TestMethod]
        public void Transfer_PositiveValue_Success()
        {
            var originalBalance = 1m;
            var transferValue = 0.01m;

            var sourceAccount = new Account { Id = 1, Balance = originalBalance };
            var destinationAccount = new Account { Id = 2, Balance = originalBalance };

            var accountingService = CreateAccountingService(sourceAccount, destinationAccount);

            var transferResult = accountingService.Transfer(Login, sourceAccount.Id, destinationAccount.Id, transferValue);

            Assert.AreEqual(transferResult.Status, OperationStatus.Success);
            Assert.AreEqual(sourceAccount.Balance, originalBalance - transferValue);
            Assert.AreEqual(destinationAccount.Balance, originalBalance + transferValue);
        }

        [TestMethod]
        public void Transfer_NegativeValue_InvalidArgument()
        {
            var originalBalance = 1m;
            var transferValue = -0.01m;

            var sourceAccount = new Account { Id = 1, Balance = originalBalance };
            var destinationAccount = new Account { Id = 2, Balance = originalBalance };

            var accountingService = CreateAccountingService(sourceAccount, destinationAccount);

            var transferResult = accountingService.Transfer(Login, sourceAccount.Id, destinationAccount.Id, transferValue);

            Assert.AreEqual(transferResult.Status, OperationStatus.InvalidArgument);
            Assert.AreEqual(sourceAccount.Balance, originalBalance);
            Assert.AreEqual(destinationAccount.Balance, originalBalance);
        }

        [TestMethod]
        public void Transfer_LessMoney_InsufficientFunds()
        {
            var originalBalance = 1m;
            var transferValue = 1.01m;

            var sourceAccount = new Account { Id = 1, Balance = originalBalance };
            var destinationAccount = new Account { Id = 2, Balance = originalBalance };

            var accountingService = CreateAccountingService(sourceAccount, destinationAccount);

            var transferResult = accountingService.Transfer(Login, sourceAccount.Id, destinationAccount.Id, transferValue);
            
            Assert.AreEqual(transferResult.Status, OperationStatus.InsufficientFunds);
            Assert.AreEqual(sourceAccount.Balance, originalBalance);
            Assert.AreEqual(destinationAccount.Balance, originalBalance);
        }

        [TestMethod]
        public void Transfer_Frozen_AccountFrozen()
        {
            var originalBalance = 1m;
            var transferValue = 0.01m;

            var sourceAccount = new Account { Id = 1, Frozen = true, Balance = originalBalance };
            var destinationAccount = new Account { Id = 2, Balance = originalBalance };

            var accountingService = CreateAccountingService(sourceAccount, destinationAccount);

            var transferResult = accountingService.Transfer(Login, sourceAccount.Id, destinationAccount.Id, transferValue);

            Assert.AreEqual(transferResult.Status, OperationStatus.AccountFrozen);
            Assert.AreEqual(sourceAccount.Balance, originalBalance);
            Assert.AreEqual(destinationAccount.Balance, originalBalance);
        }

        [TestMethod]
        public void Freeze_Success()
        {
            var account = new Account { Id = 1 };

            var accountingService = CreateAccountingService(account);

            var freezeResult = accountingService.Freeze(Login, account.Id);

            Assert.AreEqual(freezeResult.Status, OperationStatus.Success);
            Assert.AreEqual(account.Frozen, true);
        }

        [TestMethod]
        public void AddIntrest_Success()
        {
            var originalBalance = 1m;

            var account = new Account { Id = 1, Type = AccountType.Current, Balance = originalBalance };

            var accountingService = CreateAccountingService(account);

            var addIntrestResult = accountingService.AddIntrest(Login, account.Id);

            Assert.AreEqual(addIntrestResult.Status, OperationStatus.Success);
            Assert.AreEqual(account.Balance, originalBalance * (1m + Rates[AccountType.Current]));
        }
    }
}
