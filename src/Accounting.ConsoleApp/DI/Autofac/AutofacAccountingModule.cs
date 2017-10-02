using Accounting.Contracts.Data;
using Accounting.Contracts.Managers;
using Accounting.Contracts.Models;
using Accounting.Contracts.Security;
using Accounting.Core.Managers;
using Accounting.Core.Security;
using Accounting.Data;
using Accounting.Service.Contracts;
using Accounting.Service.Services;
using Autofac;
using System.Collections.Generic;
using System.Configuration;

namespace Accounting.ConsoleApp.DI.Autofac
{
    public class AutofacAccountingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AccountingService>()
                .As<IAccountingService>();

            builder.RegisterType<AccountingAdministrationService>()
                .As<IAccountingAdministrationService>();

            builder.Register(ctx => new SignInManager(new Dictionary<string, string> {{"test", "1111"}}))
                .As<ISignInManager>().SingleInstance();

            builder.Register(
                    ctx =>
                        new AccountingManager(ctx.Resolve<IAccountUnitOfWorkFactory>(),
                            new Dictionary<AccountType, decimal>
                            {
                                {AccountType.Current, 0.015m},
                                {AccountType.Savings, 0.03m}
                            }))
                .As<IAccountingManager>()
                .SingleInstance();

            builder.RegisterType<AccountingAdministrationManager>()
                .As<IAccountingAdministrationManager>()
                .SingleInstance();

            builder.Register(
                ctx => new AccountUnitOfWorkFactory(
                    ConfigurationManager.ConnectionStrings["accounting-db"].ConnectionString))
                .As<IAccountUnitOfWorkFactory>()
                .SingleInstance();
        }
    }
}
