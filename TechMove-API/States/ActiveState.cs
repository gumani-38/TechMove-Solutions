namespace TechMove_API.States
{
    public class ActiveState : ContractState
    {
        public override string Handle()
        {
            return "Contract is active.";
        }
    }
}
