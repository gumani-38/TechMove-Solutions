
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gumani_Moila_EAPD7111w_POE.Models
{
    public class Contract 
    {
        public enum ContractStatus
        {
            draft,
            active,
            expired,
            onHold
        }

        [Key]
        public int ContractId { get; set; }
        [Required(ErrorMessage = "contract start date is required")]
        public DateTime  StartDate { get; set; }
        [Required(ErrorMessage = "contract end date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "contract status is required")]
        public ContractStatus Status { get; set; }
        [ForeignKey("Client")]
        [DisplayName("Select a client")]
        public int ClientId { get; set; }

        // Navigation property for the related Client entity
        public Client? Client { get; set; }

       


    }
}
