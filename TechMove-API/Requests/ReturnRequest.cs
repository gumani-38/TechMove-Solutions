using TechMove_API.Models;

namespace TechMove_API.Requests
{
    public class ReturnRequest : ServiceRequest
    {
        public override string Process()
        {
            return "Return Request Processed";
        }
    }
}
