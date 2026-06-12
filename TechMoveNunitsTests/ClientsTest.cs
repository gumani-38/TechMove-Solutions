using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechMove_API.Controllers;
using TechMove_API.Data;
using TechMove_API.Models;

namespace TechMoveNunitsTests
{
    [TestFixture]
    public class ClientsTest
    {
        private TechMoveDbContext _context;
        private ClientsController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TechMoveDbContext>()
             .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
             .Options;

            _context = new TechMoveDbContext(options);
            _controller = new ClientsController(_context);

            // Seed test data
            _context.Clients.AddRange(new List<Client>
            {
                new Client { ClientId = 1, ClientName = "Alice",  
                ClientRegion = "Gauteng", ContactDetails = "0618272791"},
                new Client { ClientId = 2, ClientName  = "Bob", ClientRegion = "Western Cape", ContactDetails = "0791234567"}
            });
            _context.SaveChanges();
        }

        // ✅ GET: api/Clients
        [Test]
        public async Task Index_ReturnsOkResult_WithClientsList()
        {
            var result = await _controller.Index();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var clients = okResult?.Value as List<Client>;

            Assert.NotNull(clients);
            Assert.AreEqual(2, clients.Count);
        }

        // ✅ GET: api/Clients/{id}
        [Test]
        public async Task GetClient_ReturnsOkResult_WhenClientExists()
        {
            var result = await _controller.GetClient(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var client = okResult?.Value as Client;

            Assert.NotNull(client);
            Assert.AreEqual("Alice", client.ClientName);
        }

        [Test]
        public async Task GetClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            var result = await _controller.GetClient(99);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // ✅ POST: api/Clients
        [Test]
        public async Task Create_ReturnsCreatedAtActionResult_WhenValid()
        {
            var newClient = new Client { ClientId = 3, ClientName = "Charlie", ClientRegion = "Eastern Cape", ContactDetails = "0612345678" };

            var result = await _controller.Create(newClient);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            var client = createdResult?.Value as Client;

            Assert.NotNull(client);
            Assert.AreEqual("Charlie", client.ClientName);
        }

        // ✅ PUT: api/Clients/{id}
        [Test]
        public async Task Edit_ReturnsNoContent_WhenUpdateSuccessful()
        {
            var existingClient = new Client
            {
                ClientId = 1,
                ClientName = "Alice Updated",
                ClientRegion = "Gauteng",
                ContactDetails = "0000"
            };

            // Detach tracked entity to avoid duplicate tracking
            var tracked = _context.Clients.Local.FirstOrDefault(c => c.ClientId == existingClient.ClientId);
            if (tracked != null)
            {
                _context.Entry(tracked).State = EntityState.Detached;
            }

            var result = await _controller.Edit(1, existingClient);

            Assert.IsInstanceOf<NoContentResult>(result);
            var updatedClient = await _context.Clients.FindAsync(1);
            Assert.AreEqual("Alice Updated", updatedClient.ClientName);
        }

        [Test]
        public async Task Edit_ReturnsBadRequest_WhenIdMismatch()
        {
            var client = new Client { ClientId = 5, ClientName = "Mismatch", ClientRegion = "Unknown", ContactDetails = "0000000000" };
            var result = await _controller.Edit(1, client);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // ✅ DELETE: api/Clients/{id}
        [Test]
        public async Task DeleteClient_ReturnsNoContent_WhenClientDeleted()
        {
            var result = await _controller.DeleteClient(2);

            Assert.IsInstanceOf<NoContentResult>(result);
            var deletedClient = await _context.Clients.FindAsync(2);
            Assert.Null(deletedClient);
        }

        [Test]
        public async Task DeleteClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            var result = await _controller.DeleteClient(99);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
