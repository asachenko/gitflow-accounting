using Accounting.ConsoleApp.DI.Autofac;
using Accounting.Contracts;
using Accounting.Contracts.Models;
using Accounting.Data;
using Accounting.Service.Contracts;
using Accounting.Service.Models;
using Autofac;
using Ninject;
using System;
using System.Configuration;

namespace Accounting.ConsoleApp
{
    public class Program
    {

        static void Main(string[] args)
        {
            try
            {
                CreateDatabaseIfNotExists();

                using (var scope = CreateContainer().BeginLifetimeScope())
                {
                    //var kernel = new StandardKernel(new AccountingModule());

                    var login = new Login {Name = "test", Pin = "1111"};

                    //var adminService = CreateAccountingAdministrationServiceByNinject(kernel);
                    var adminService = CreateAccountingAdministrationServiceByAutofac(scope);

                    var account1 = CreateAccount(adminService, login,
                        new AccountModel {Type = AccountType.Current, Balance = 10m});
                    var account2 = CreateAccount(adminService, login,
                        new AccountModel {Type = AccountType.Savings, Balance = 200m});

                    if (account1 == null || account2 == null)
                    {
                        Console.WriteLine("An error occured during creation accounts.");
                        Console.ReadKey();

                        return;
                    }

                    //var service = CreateAccountingServiceByNinject(kernel);
                    var service = CreateAccountingServiceByAutofac(scope);

                    Debit(service, login, account1.Id, 0.5m);

                    Credit(service, login, account2.Id, 0.5m);

                    Transfer(service, login, account1.Id, account2.Id, 1m);

                    AddIntrest(service, login, account1.Id);

                    Freeze(service, login, account1.Id);

                    account2.Type = AccountType.Current;
                    account2.Frozen = true;

                    EditAccount(adminService, login, account2);

                    DeleteAccount(adminService, login, account1.Id);
                    DeleteAccount(adminService, login, account2.Id);

                    Console.WriteLine("==== The end ====");
                }
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
            }
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<AutofacAccountingModule>();

            return builder.Build();
        }

        private static void Debit(IAccountingService service, Login login, int accountId, decimal delta)
        {
            Perform(() => service.Debit(login, accountId, delta), "Debit");
        }

        private static void Credit(IAccountingService service, Login login, int accountId, decimal delta)
        {
            Perform(() => service.Credit(login, accountId, delta), "Credit");
        }

        private static void Transfer(IAccountingService service, Login login, int fromId, int toId, decimal delta)
        {
            Perform(() => service.Transfer(login, fromId, toId,  delta), "Transfer");
        }

        private static void AddIntrest(IAccountingService service, Login login, int accountId)
        {
            Perform(() => service.AddIntrest(login, accountId), "AddIntrest");
        }

        private static void Freeze(IAccountingService service, Login login, int accountId)
        {
            Perform(() => service.Freeze(login, accountId), "Freeze");
        }

        private static AccountModel CreateAccount(IAccountingAdministrationService service, Login login, AccountModel account)
        {
            var operationResult = Perform(() => service.Create(login, account), "CreateNewAccount");

            return operationResult.Status == OperationStatus.Success ? ((OperationCreationResult<AccountModel>) operationResult).Result : null;
        }

        private static void EditAccount(IAccountingAdministrationService service, Login login, AccountModel account)
        {
            Perform(() => service.Edit(login, account), "EditAccount");
        }

        private static void DeleteAccount(IAccountingAdministrationService service, Login login, int accountId)
        {
            Perform(() => service.Delete(login, accountId), "DeleteAccount");
        }

        private static IAccountingAdministrationService CreateAccountingAdministrationServiceByNinject(IKernel kernel)
        {
            return kernel.Get<IAccountingAdministrationService>();
        }

        private static IAccountingService CreateAccountingServiceByNinject(IKernel kernel)
        {
            return kernel.Get<IAccountingService>();
        }

        private static IAccountingAdministrationService CreateAccountingAdministrationServiceByAutofac(ILifetimeScope scope)
        {
            return scope.Resolve<IAccountingAdministrationService>();
        }

        private static IAccountingService CreateAccountingServiceByAutofac(ILifetimeScope scope)
        {
            return scope.Resolve<IAccountingService>();
        }

        private static OperationResult Perform(Func<OperationResult> operation, string operationName)
        {
            try
            {
                Console.WriteLine($"Operation: {operationName}");
                var result = operation();

                Console.WriteLine($"Operation: { operationName} has been finished with status {result.Status}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The error occured during executing th operation '{operationName}'. ErrorMessage: {ex.Message}");

                return new OperationResult {Status = OperationStatus.Failure};
            }
        }

        /// <summary>
        /// Creation database by Entity Framework Code First
        /// </summary>
        private static void CreateDatabaseIfNotExists()
        {
            using (var context = new AccountContext(GetConnectionString()))
            {
                context.Database.CreateIfNotExists();
            }
        }
        
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["accounting-db"].ConnectionString;
        }
    }
}