using TechMove_API.Observers;
using TechMove_API.States;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove_API.Models
{
    public class Contract : ISubject
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

        // STATE PATTERN
        [NotMapped]
        private ContractState? _state;

        [NotMapped]
        private List<IObserver> observers = new();
        [NotMapped]
        public string NotificationMessage { get; set; } = "";

        public void SetState(ContractState state)
        {
            _state = state;
        }

        public string Request()
        {
            return _state?.Handle() ?? "No state assigned";
        }
        public void Attach(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void Notify()
        {
            foreach (var observer in observers)
            {
                observer.Update(NotificationMessage);
            }
        }


    }
}
