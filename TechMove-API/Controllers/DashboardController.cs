using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove_API.Data;
using TechMove_API.Models; 


namespace TechMove_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly TechMoveDbContext _context;

        public DashboardController(TechMoveDbContext context)
        {
            _context = context;
        }
        // GET: api/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var totalClients = await _context.Clients.CountAsync();
                var activeContracts = await _context.Contracts.CountAsync(c => c.Status == Contract.ContractStatus.active);
                var expiredContracts = await _context.Contracts.CountAsync(c => c.Status == Contract.ContractStatus.expired);
                var pendingRequests = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequest.ServiceRequestStatus.pending);
                var completedRequests = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequest.ServiceRequestStatus.completed);

                var contracts = await _context.Contracts.Select(c => c.StartDate.Month).ToListAsync();
                var monthlyContracts = Enumerable.Range(1, 12)
                    .Select(month => contracts.Count(m => m == month))
                    .ToList();

                var metrics = new
                {
                    TotalClients = totalClients,
                    ActiveContracts = activeContracts,
                    ExpiredContracts = expiredContracts,
                    PendingRequests = pendingRequests,
                    CompletedRequests = completedRequests,
                    MonthlyContracts = monthlyContracts
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error calculating dashboard metrics: {ex.Message}");
            }
        }
    }
}
