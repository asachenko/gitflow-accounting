using Accounting.Contracts.Models;

namespace Accounting.Service.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        
        public AccountType Type { get; set; }
        
        public decimal Balance { get; set; }
        
        public bool Frozen { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Type: {Type}, Balance: {Balance}, Frozen: {Frozen}";
        }
    }
}
