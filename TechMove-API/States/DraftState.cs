namespace TechMove_API.States
{
    public class DraftState : ContractState
    {
        public override string Handle()
        {
            return "Contract is currently in draft state.";
        }
    }
}
