
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Gumani_Moila_EAPD7111w_POE.Models;

namespace Gumani_Moila_EAPD7111w_POE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IHttpClientFactory _contractFactory;

        public ContractsController(IHttpClientFactory contractFactory)
        {
            _contractFactory = contractFactory;
        }
        private HttpClient ApiContract
        {
            get
            {
                var client = _contractFactory.CreateClient("TechMoveAPI");
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                return client;
            }
}

        // GET: Contracts
        public async Task<IActionResult> Index(DateOnly? fromDate, DateOnly? toDate, string status)
        {
          
            var queryParameters = new List<string>();

            if (fromDate.HasValue)
                queryParameters.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParameters.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrEmpty(status))
                queryParameters.Add($"status={Uri.EscapeDataString(status)}");

            string queryString = queryParameters.Count > 0
                ? "?" + string.Join("&", queryParameters)
                : string.Empty;

            var response = await ApiContract.GetAsync($"api/contracts{queryString}");

            List<Contract> contracts = new List<Contract>();

            if (response.IsSuccessStatusCode)
            {
                contracts = await response.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable to load contracts from the server.");
            }

      
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status ?? "All";

            return View(contracts);
        }



        // GET: Contracts/Create
      public async Task<IActionResult> Create()
        {
           
            var response = await ApiContract.GetAsync("api/clients");

            List<Client> clients = new List<Client>();

            if (response.IsSuccessStatusCode)
            {
                clients = await response.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable to load clients for selection.");
            }

            // 2. Bind the fetched list to the SelectList for the View
            ViewData["ClientId"] = new SelectList(clients, "ClientId", "ClientName");
            return View();
        }


        // POST: Contracts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartDate,EndDate,Status,ClientId")] Contract contract, IFormFile agreementPdf)
        {
            // Frontend validation
            if (agreementPdf == null || agreementPdf.Length == 0)
            {
                ModelState.AddModelError("agreementFile", "The signed agreement file is required");
                return View(contract);
            }

            var extension = Path.GetExtension(agreementPdf.FileName).ToLowerInvariant();
            if (agreementPdf.ContentType != "application/pdf" || extension != ".pdf")
            {
                ModelState.AddModelError("agreementFile", "Only PDF files are allowed.");
                return View(contract);
            }

            if (ModelState.IsValid)
            {
                using var content = new MultipartFormDataContent();

                // Pass fields using their raw string representations
                content.Add(new StringContent(contract.StartDate.ToString()), nameof(contract.StartDate));
                content.Add(new StringContent(contract.EndDate.ToString()), nameof(contract.EndDate));
                content.Add(new StringContent(contract.Status.ToString()), nameof(contract.Status));
                content.Add(new StringContent(contract.ClientId.ToString()), nameof(contract.ClientId));

                // Add file stream
                using var fileStream = agreementPdf.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(fileContent, "agreementPdf", agreementPdf.FileName);

                var response = await ApiContract.PostAsync("api/contracts", content);
            var apiResult = await response.Content.ReadFromJsonAsync<ApiResponseMessage>();
                if (apiResult != null && !string.IsNullOrEmpty(apiResult.StateInfo))
                {
                    TempData["Info"] = apiResult.StateInfo;
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The backend server failed to process and save the contract.");
            }

            // Load dropdown options via API
            var clientsResponse = await ApiContract.GetAsync("api/clients");
            var clients = clientsResponse.IsSuccessStatusCode
                ? await clientsResponse.Content.ReadFromJsonAsync<List<Client>>()
                : new List<Client>();
            
            ViewData["ClientId"] = new SelectList(clients, "ClientId", "ClientName", contract.ClientId);
            return View(contract);
        }

   
        public async Task<IActionResult> DownloadAgreement(int id)
        {
           var response =  await ApiContract.GetAsync($"api/contracts/download/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound("Agreement pdf not found.");
            if (!response.IsSuccessStatusCode)
                return NotFound("Agreement pdf not found.");

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"Contract_{id}_Agreement.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

    
            var contractResponse = await ApiContract.GetAsync($"api/contracts/{id}");

            if (!contractResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var contract = await contractResponse.Content.ReadFromJsonAsync<Contract>();
            if (contract == null)
            {
                return NotFound();
            }

            var clientsResponse = await ApiContract.GetAsync("api/clients");
            var clients = clientsResponse.IsSuccessStatusCode
                ? await clientsResponse.Content.ReadFromJsonAsync<List<Client>>()
                : new List<Client>();

            ViewData["ClientId"] = new SelectList(clients, "ClientId", "ClientName", contract.ClientId);

            return View(contract);
        }


        // POST: Contracts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,StartDate,EndDate,Status,ClientId")] Contract contract)
        {
            if (id != contract.ContractId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var response = await ApiContract.PutAsJsonAsync($"api/contracts/{id}", contract);

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<ApiResponseMessage>();
                    if (apiResult != null && !string.IsNullOrEmpty(apiResult.StateInfo))
                    {
                        TempData["Info"] = apiResult.StateInfo;
                    }

                    return RedirectToAction(nameof(Index));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "Unable to save updates on the server.");
            }

            var clientsResponse = await ApiContract.GetAsync("api/clients");
            var clients = clientsResponse.IsSuccessStatusCode
                ? await clientsResponse.Content.ReadFromJsonAsync<List<Client>>()
                : new List<Client>();

            ViewData["ClientId"] = new SelectList(clients, "ClientId", "ClientName", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var response = await ApiContract.GetAsync($"api/contracts/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }


        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await ApiContract.DeleteAsync($"api/contracts/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Contract deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Unable to delete the contract from the server.");
            return RedirectToAction(nameof(Index));
        }


        // Helper class to match the API's JSON response structure
        public class ApiResponseMessage
        {
            public Contract Contract { get; set; }
            public string StateInfo { get; set; }
        }

    }
}
