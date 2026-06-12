using TechMove_API.Models;

namespace TechMove_API.Requests
{
    public class PickupRequest: ServiceRequest
    {
        public override string Process()
        {
            return "Pickup Request Processed";
        }
    }
}
