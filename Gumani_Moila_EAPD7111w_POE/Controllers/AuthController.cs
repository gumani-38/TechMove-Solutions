using Microsoft.AspNetCore.Mvc;
using Gumani_Moila_EAPD7111w_POE.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Gumani_Moila_EAPD7111w_POE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _authFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _authFactory = httpClientFactory;
        }
        private HttpClient ApiUser => _authFactory.CreateClient("TechMoveAPI");

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await ApiUser.PostAsJsonAsync("/api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse =
           await response.Content.ReadFromJsonAsync<LoginResponse>();

                HttpContext.Session.SetString(
                    "AuthToken",
                    loginResponse.Token
                );
                Console.WriteLine(HttpContext.Session.GetString("AuthToken"));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
