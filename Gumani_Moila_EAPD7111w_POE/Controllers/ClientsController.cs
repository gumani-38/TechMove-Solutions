
using Gumani_Moila_EAPD7111w_POE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Gumani_Moila_EAPD7111w_POE.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ClientsController(IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
        }
        private HttpClient ApiClient
        {
            get
            {
                var client = _clientFactory.CreateClient("TechMoveAPI");
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return client;
            }
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {

            var response = await ApiClient.GetAsync("api/clients");

            if (response.IsSuccessStatusCode)
            {
                var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
                return View(clients);
            }

            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            return View(new List<Client>());
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,ClientName,ContactDetails,ClientRegion")] Client client)
        {
            if (ModelState.IsValid)
            {
                var response = await ApiClient.PostAsJsonAsync("api/clients", client);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }


                var errorMessage = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    $"Status: {(int)response.StatusCode} - {errorMessage}"
                );
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var client = await ApiClient.GetFromJsonAsync<Client>($"api/clients/{id}");
            if (client == null)
            {
                    return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,ClientName,ContactDetails,ClientRegion")] Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var response = await ApiClient.PutAsJsonAsync($"api/clients/{id}", client);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "Unable to save updates. Please try again later.");
            }

            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = await ApiClient.GetFromJsonAsync<Client>($"api/clients/{id}");

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(id <= 0)
            {
                return NotFound();
            }
            var response = await ApiClient.DeleteAsync($"api/clients/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Unable to delete the client. Ensure they have no active dependencies.");
            return RedirectToAction(nameof(Index));
        }


    }
}
