using Accounting.Contracts.Data;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Models;
using Accounting.Contracts.Security;
using Accounting.Core.Managers;
using Accounting.Core.Security;
using Accounting.Data;
using Accounting.Service.Contracts;
using Accounting.Service.Services;
using Ninject.Modules;
using System.Collections.Generic;
using System.Configuration;

namespace Accounting.ConsoleApp.DI
{
    public class AccountingModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAccountingService>()
                .To<AccountingService>();

            Bind<IAccountingAdministrationService>()
                .To<AccountingAdministrationService>();

            Bind<ISignInManager>()
                .ToConstructor(ctx => new SignInManager(new Dictionary<string, string> { {"test", "1111"} }))
                .InSingletonScope();

            Bind<IAccountingManager>()
                .ToConstructor(
                    ctx =>
                        new AccountingManager(ctx.Inject<IAccountUnitOfWorkFactory>(),
                            new Dictionary<AccountType, decimal>
                            {
                                {AccountType.Current, 0.02m},
                                {AccountType.Savings, 0.03m}
                            }))
                .InSingletonScope();

            Bind<IAccountingAdministrationManager>()
                .To<AccountingAdministrationManager>()
                .InSingletonScope();

            Bind<IAccountUnitOfWorkFactory>()
                .ToConstructor(
                    ctx => new AccountUnitOfWorkFactory(
                        ConfigurationManager.ConnectionStrings["accounting-db"].ConnectionString))
                .InSingletonScope();
        }
    }
}
