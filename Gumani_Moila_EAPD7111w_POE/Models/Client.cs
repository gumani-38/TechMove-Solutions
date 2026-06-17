using System.ComponentModel.DataAnnotations;

namespace Gumani_Moila_EAPD7111w_POE.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "client name is required")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "contact details are required")]
        public string ContactDetails { get; set; }

        [Required(ErrorMessage = "region is required")]
        public string ClientRegion { get; set; }

    }
}
