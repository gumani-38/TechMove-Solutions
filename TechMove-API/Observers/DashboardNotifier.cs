namespace TechMove_API.Observers
{
    public class DashboardNotifier : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine($"DASHBOARD: {message}");
        }
    }
}
