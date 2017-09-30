using System.ComponentModel.DataAnnotations;

namespace Accounting.Contracts.Models
{
    public class Account
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public decimal Balance { get; set; }

        [Required]
        public bool Frozen { get; set; }
    }
}
