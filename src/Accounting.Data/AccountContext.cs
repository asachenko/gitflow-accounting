using Accounting.Contracts.Models;
using System.Data.Entity;

namespace Accounting.Data
{
    public class AccountContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public AccountContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().Property(x => x.Balance).HasPrecision(19, 4);
        }
    }
}
