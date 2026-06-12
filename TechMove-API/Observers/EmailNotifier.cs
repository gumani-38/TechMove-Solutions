namespace TechMove_API.Observers
{
    public class EmailNotifier : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine($"EMAIL: {message}");
        }
    }
}
