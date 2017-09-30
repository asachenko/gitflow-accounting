Database will be created automatically by Entity Framework Code First after executing Accounting.ConsoleApp. For this it is necessary to set correct connection string with name accounting-db in the project Accounting.ConsoleApp in App.config.

Classes Accounting.Service.Services.AccountingAdministrationService and Accounting.Service.Services.AccountingService contain all use cases from requirements 3, 4, 5, 7.

Implementation of business logic is located in classes Accounting.Core.Managers.AccountingAdministrationManager and Accounting.Core.Managers.AccountingManager.

I use patterns the repository and unit of work patterns in the application for creation an abstraction layer between the data access layer and the business logic.
Implementing these patterns can help insulate the application from changes in the data store and can facilitate automated unit testing.

I chose a decimal type to represent money in C# and in data storage in SQL Server. The decimal type has more precision and a smaller range, which makes it appropriate for financial and monetary calculations.

In operation debit, credit and etc. I use an isolation level RepeatableRead, because it guarantees that any data read cannot change, if the transaction reads 
the same data again.