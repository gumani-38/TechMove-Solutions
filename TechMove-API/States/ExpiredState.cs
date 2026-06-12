namespace TechMove_API.States
{
    public class ExpiredState : ContractState
    {
        public override string Handle()
        {
            return "Contract has expired.";
        }
    }
}
