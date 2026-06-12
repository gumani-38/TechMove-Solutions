using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove_API.Models
{
    public  class ServiceRequest
    {
        public enum ServiceRequestStatus
        {
            pending,
            inProgress,
            completed,
            cancelled
        }
        public enum RequestType
        {
            delivery,
            pickup,
            returnRequest
        }
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Display(Name = "Cost in $ (dollar)")]
        [Range(0.01, 9999999999)]
        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Cost is required")]
        public decimal Cost { get; set; }
        public RequestType Type { get; set; }

        public ServiceRequestStatus Status { get; set; }
        [ForeignKey("Contract")]
        [Required(ErrorMessage = "contract id is required")]
        [Display(Name = "Select a contract")]
         public int ContractId { get; set; }

        // Navigation property for the related Contract entity
        public Contract? Contract { get; set; }

        // FACTORY PATTERN
        public virtual string Process()
        {
            return "Processed";
        }
    }
}
