using Gumani_Moila_EAPD7111w_POE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Gumani_Moila_EAPD7111w_POE.Controllers
{
    public class CurrencyConversionViewModel
    {
        public string SourceCurrency { get; set; } = "USD";
        public string TargetCurrency { get; set; } = "ZAR";
        public decimal OriginalAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal ConvertedAmount { get; set; }
    }
    public class ServiceRequestsController : Controller     
    {
        private readonly IHttpClientFactory _serviceRequestFactory;

        public ServiceRequestsController(IHttpClientFactory httpClientFactory)
        {
            _serviceRequestFactory = httpClientFactory      ;
        }
        private HttpClient ApiServiceRequest
        {
            get
            {
                var client = _serviceRequestFactory.CreateClient("TechMoveAPI");
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                return client;
            }
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var serviceRequest = await ApiServiceRequest.GetFromJsonAsync<List<ServiceRequest>>("api/ServiceRequests");
            return View(serviceRequest);
        }


        // GET: Currency convertor using exchangerate Api

        [HttpGet]
        public async Task<IActionResult> GetConvertedCurrency(decimal amount)
        {
            try
            {
                var response = await ApiServiceRequest.GetAsync($"api/ServiceRequests/Convert?amount={amount}");

                if (response.IsSuccessStatusCode)
                {
                    var conversionResult = await response.Content.ReadFromJsonAsync<CurrencyConversionViewModel>();

                    if (conversionResult == null)
                    {
                        return StatusCode(500, new { error = "Data deserialization failed. ViewModel is null." });
                    }

                    return Ok(new
                    {
                        sourceCurrency = conversionResult.SourceCurrency,
                        targetCurrency = conversionResult.TargetCurrency,
                        rate = conversionResult.Rate,
                        originalAmount = conversionResult.OriginalAmount,
                        convertedAmount = conversionResult.ConvertedAmount
                    });

                }
                return StatusCode((int)response.StatusCode, new
                {
                    error = "Internal Web API returned an error status.",
                    statusCode = response.StatusCode,
                    details = "" + await response.Content.ReadAsStringAsync()
                });
            }
            catch (Exception ex)
            {
                // This sends the exact C# exception message directly to your browser's network/console tab
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
           
            var response = await ApiServiceRequest.GetAsync("api/contracts");

            List<Contract> contracts = new List<Contract>();

            if (response.IsSuccessStatusCode)
            {
                contracts = await response.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable to load contracts for assignment selection.");
            }

            ViewData["ContractId"] = new SelectList(contracts, "ContractId", "ContractId");

            return View();
        }

        // POST: ServiceRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Cost,Status,ContractId,Type")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
             
                var response = await ApiServiceRequest.PostAsJsonAsync("api/servicerequests", serviceRequest);

                if (response.IsSuccessStatusCode)
                {
             
                    var result = await response.Content.ReadFromJsonAsync<ServiceRequestApiResponse>();

                    if (result != null && !string.IsNullOrEmpty(result.StatusMessage))
                    {
                        
                        TempData["Success"] = result.StatusMessage;
                    }

                    return RedirectToAction(nameof(Index));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
               
                    var apiErrors = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, apiErrors);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An unexpected server error occurred while creating the service request.");
                }
            }

            var contractsResponse = await ApiServiceRequest.GetAsync("api/contracts");
            var contracts = contractsResponse.IsSuccessStatusCode
                ? await contractsResponse.Content.ReadFromJsonAsync<List<Contract>>()
                : new List<Contract>();

            ViewData["ContractId"] = new SelectList(contracts, "ContractId", "ContractId", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

     
            var response = await ApiServiceRequest.GetAsync($"api/servicerequests/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (serviceRequest == null)
            {
                return NotFound();
            }

    
            var contractsResponse = await ApiServiceRequest.GetAsync("api/contracts");
            var contracts = contractsResponse.IsSuccessStatusCode
                ? await contractsResponse.Content.ReadFromJsonAsync<List<Contract>>()
                : new List<Contract>();

            ViewData["ContractId"] = new SelectList(contracts, "ContractId", "ContractId", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Cost,Status,ContractId,Type")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var response = await ApiServiceRequest.PutAsJsonAsync($"api/servicerequests/{id}", serviceRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ServiceRequestApiResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.StatusMessage))
                    {
                        TempData["Success"] = result.StatusMessage;
                    }

                    return RedirectToAction(nameof(Index));
                }


                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "Unable to save modified details on the server backend.");
            }

            var contractsResponse = await ApiServiceRequest.GetAsync("api/contracts");
            var contracts = contractsResponse.IsSuccessStatusCode
                ? await contractsResponse.Content.ReadFromJsonAsync<List<Contract>>()
                : new List<Contract>();

            ViewData["ContractId"] = new SelectList(contracts, "ContractId", "ContractId", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

  
            var response = await ApiServiceRequest.GetAsync($"api/servicerequests/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }


        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
          
            var response = await ApiServiceRequest.DeleteAsync($"api/servicerequests/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Service request deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Handle potential API server errors
            ModelState.AddModelError(string.Empty, "Unable to delete the service request from the server.");
            return RedirectToAction(nameof(Index));
        }

        
        // Helper container to match the Web API's returned dynamic structure
        public class ServiceRequestApiResponse
        {
            public ServiceRequest ServiceRequest { get; set; }
            public string StatusMessage { get; set; }
        }

    }
}
