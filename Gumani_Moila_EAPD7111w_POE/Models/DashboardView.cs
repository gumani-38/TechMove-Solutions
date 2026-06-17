namespace Gumani_Moila_EAPD7111w_POE.Models
{
    public class DashboardView
    {
            public int TotalClients { get; set; }
            public int ActiveContracts { get; set; }
            public int ExpiredContracts { get; set; }
            public int PendingRequests { get; set; }
            public int CompletedRequests { get; set; }

            public List<int> MonthlyRequests { get; set; } = new();
            public List<int> MonthlyContracts { get; set; } = new();
      

    }
}
