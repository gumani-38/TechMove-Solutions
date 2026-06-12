using TechMove_API.Models;
using TechMove_API.Requests;

namespace TechMove_API.Factory
{
    public class RequestFactory
    {
        public static string ProcessRequest(ServiceRequest.RequestType type)
        {
            switch (type)
            {
                case ServiceRequest.RequestType.delivery:
                    return "Delivery request created successfully";

                case ServiceRequest.RequestType.pickup:
                    return "Pickup request created successfully";

                case ServiceRequest.RequestType.returnRequest:
                    return "Return request created successfully";

                default:
                    return "Unknown request type";
            }
        }
    }
}
