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
    public class AccountingAdministrationServiceTests
    {
        private static readonly Login Login = new Login { Name = "test", Pin = "1111" };
        private static readonly AccountModel Account = new AccountModel { Id = 1, Balance = 1m, Frozen = false, Type = AccountType.Current };

        private static AccountingAdministrationManager CreateAccountingAdministrationManager(IAccountRepository accountRepository)
        {
            var unitOfWork = new Mock<IAccountUnitOfWork>();
            unitOfWork.SetupGet(x => x.AccountRepository).Returns(accountRepository);

            var unitOfWorkFactory = new Mock<IAccountUnitOfWorkFactory>();
            unitOfWorkFactory.Setup(x => x.Create(It.IsAny<IsolationLevel>())).Returns(unitOfWork.Object);
            
            return new AccountingAdministrationManager(unitOfWorkFactory.Object);
        }

        private static AccountingAdministrationService CreateAccountingAdministrationService(IAccountRepository accountRepository)
        {
            var accountingAdministrationManager = CreateAccountingAdministrationManager(accountRepository);

            var signInManager = new SignInManager(new Dictionary<string, string> { { Login.Name, Login.Pin } });

            return new AccountingAdministrationService(signInManager, accountingAdministrationManager);
        }

        [TestMethod]
        public void AllOperations_Login_Failure()
        {
            var login = new Login {Name = "test", Pin = "1234"};

            var repoMock = new Mock<IAccountRepository>();

            var service = CreateAccountingAdministrationService(repoMock.Object);

            var createResult = service.Create(login, Account);
            Assert.AreEqual(createResult.Status, OperationStatus.AccessDenied);
            repoMock.Verify(x => x.Insert(It.IsAny<Account>()), Times.Never);

            var editResult = service.Edit(login, Account);
            Assert.AreEqual(editResult.Status, OperationStatus.AccessDenied);
            repoMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);
            
            var deleteResult = service.Delete(login, Account.Id);
            Assert.AreEqual(deleteResult.Status, OperationStatus.AccessDenied);
            repoMock.Verify(x => x.Delete(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void Create_Account_Success()
        {
            var repoMock = new Mock<IAccountRepository>();

            var service = CreateAccountingAdministrationService(repoMock.Object);

            var createResult = service.Create(Login, Account);
            Assert.AreEqual(createResult.Status, OperationStatus.Success);
            repoMock.Verify(x => x.Insert(It.IsAny<Account>()), Times.Once);
        }

        [TestMethod]
        public void Edit_Account_Success()
        {
            var repoMock = new Mock<IAccountRepository>();

            var service = CreateAccountingAdministrationService(repoMock.Object);
            
            var editResult = service.Edit(Login, Account);
            Assert.AreEqual(editResult.Status, OperationStatus.Success);
            repoMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        }

        [TestMethod]
        public void Delete_Account_Success()
        {
            var repoMock = new Mock<IAccountRepository>();

            var service = CreateAccountingAdministrationService(repoMock.Object);
            
            var deleteResult = service.Delete(Login, Account.Id);
            Assert.AreEqual(deleteResult.Status, OperationStatus.Success);
            repoMock.Verify(x => x.Delete(It.IsAny<int>()), Times.Once);
        }
    }
}
