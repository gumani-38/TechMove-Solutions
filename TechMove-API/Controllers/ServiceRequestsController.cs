using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove_API.Data;
using TechMove_API.Factory;
using TechMove_API.Models;

namespace TechMove_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly TechMoveDbContext _context;
        private readonly HttpClient _httpClient;

        public ServiceRequestsController(TechMoveDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        // get all service requests
        [HttpGet]
        public async Task<IActionResult> GetAllServiceRequests()
        {
            try
            {
                var serviceRequests = await _context.ServiceRequests.Include(s => s.Contract).ToListAsync();
                return Ok(serviceRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving service requests.", details = ex.Message });
            }
            }

        // get service request by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceRequestById(int id)
        {
            try
            {
                var serviceRequest = await _context.ServiceRequests
                    .Include(s => s.Contract)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (serviceRequest == null)
                {
                    return NotFound();
                }
                return Ok(serviceRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the service request.", details = ex.Message });
            }
            }

        // get the currency conversion result using exchangerate api passes in the amount which is always in USD and covert it to ZAR
        // GET: api/Currency/Convert
        [HttpGet("Convert")] 
        public async Task<IActionResult> GetConvertedCurrency([FromQuery] decimal amount) 
        {
            try
            {
               
                var response = await _httpClient.GetAsync("https://v6.exchangerate-api.com/v6/26e1f641a6dd35fe3871f17a/latest/USD");
          
        

                        response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonDocument.Parse(json);

                var rate = data.RootElement
                    .GetProperty("conversion_rates")
                    .GetProperty("ZAR")
                    .GetDecimal();

                var convertedAmount = amount * rate;

               
                return Ok(new
                {
                    SourceCurrency = "USD",
                    TargetCurrency = "ZAR",
                    OriginalAmount = amount,
                    Rate = rate,
                    ConvertedAmount = convertedAmount
                });
            }
            catch (HttpRequestException ex)
            {
           
                return StatusCode(502, new { message = "Failed to fetch current exchange rates from provider.", details = ex.Message });
            }
        }


        // POST: api/ServiceRequests
        [HttpPost]
  
        public async Task<IActionResult> Create([FromBody] ServiceRequest serviceRequest) 
        {

            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "The specified contract does not exist.");
                return BadRequest(ModelState); 
            }

            if (contract.Status == Contract.ContractStatus.expired || contract.Status == Contract.ContractStatus.onHold)
            {
                ModelState.AddModelError("ContractId", "Cannot create a service request for an expired or on-hold contract.");
                return BadRequest(ModelState);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync("https://v6.exchangerate-api.com/v6/26e1f641a6dd35fe3871f17a/latest/USD");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonDocument.Parse(json);

                    var rate = data.RootElement.GetProperty("conversion_rates").GetProperty("ZAR").GetDecimal();

                    serviceRequest.Cost = serviceRequest.Cost * rate;

                    // Execute pattern matching via your Factory
                    var processingMessage = RequestFactory.ProcessRequest(serviceRequest.Type);

                    _context.Add(serviceRequest);
                    await _context.SaveChangesAsync();

                 return CreatedAtAction("GetServiceRequestById", new { id = serviceRequest.Id }, new { serviceRequest, statusMessage = processingMessage });
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(502, new { message = "Failed to convert currency because the currency exchange service is unavailable.", details = ex.Message });
                }
            }
            return BadRequest(ModelState);
        }

        // PUT: api/ServiceRequests/5
        [HttpPut("{id}")] 
        public async Task<IActionResult> Edit(int id, [FromBody] ServiceRequest serviceRequest) 
        {
            if (id != serviceRequest.Id)
            {
                return BadRequest(new { message = "Mismatched Service Request ID between URL route and payload body." }); 
            }

            if (ModelState.IsValid)
            {
                try
                {
             
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync("https://v6.exchangerate-api.com/v6/26e1f641a6dd35fe3871f17a/latest/USD");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonDocument.Parse(json);

                    var rate = data.RootElement.GetProperty("conversion_rates").GetProperty("ZAR").GetDecimal();

           
                    serviceRequest.Cost = serviceRequest.Cost * rate;

                    // Re-invoke tracking logic from your Factory pattern
                    var processingMessage = RequestFactory.ProcessRequest(serviceRequest.Type);

                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();

  
                    return Ok(new { serviceRequest, statusMessage = processingMessage });
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(502, new { message = "Failed to convert currency because the exchange service is down.", details = ex.Message });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.Id))
                    {
                        return NotFound(new { message = $"Service Request with ID {id} not found during concurrency validation." });
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return BadRequest(ModelState);
        }

        // delete: api/ServiceRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound(new { message = $"Service Request with ID {id} not found." });
            }
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.Id == id);
        }
    }
}
