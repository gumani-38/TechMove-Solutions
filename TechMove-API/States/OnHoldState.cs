namespace TechMove_API.States
{
    public class OnHoldState : ContractState
    {
        public override string Handle()
        {
            return "Contract is on hold.";
        }
    }
}
