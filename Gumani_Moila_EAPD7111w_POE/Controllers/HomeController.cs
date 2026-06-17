using Gumani_Moila_EAPD7111w_POE.Models;
using Microsoft.AspNetCore.Mvc;


namespace Gumani_Moila_EAPD7111w_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _dashboardFactory;

        public HomeController(IHttpClientFactory dashboardFactory)
        {
            _dashboardFactory = dashboardFactory;
        }
        private HttpClient DashboardClient
        {
            get
            {
               var client = _dashboardFactory.CreateClient("TechMoveAPI");
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                return client;
            }

        }

        public async Task<IActionResult> Index()
        {
            var response = await DashboardClient.GetAsync("api/dashboard");

            DashboardView model;

            if (response.IsSuccessStatusCode)
            {
                model = await response.Content.ReadFromJsonAsync<DashboardView>() ?? new DashboardView();
            }
            else
            {
                model = new DashboardView();
                ModelState.AddModelError(string.Empty, "Could not fetch active live statistics from the backend server.");
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

      
    }
}
