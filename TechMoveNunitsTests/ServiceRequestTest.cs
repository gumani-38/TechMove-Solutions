using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TechMove_API.Controllers;
using TechMove_API.Data;
using TechMove_API.Models;

namespace TechMoveNunitsTests
{
    [TestFixture]
    public class ServiceRequestsTest
    {
        private TechMoveDbContext _context;
        private ServiceRequestsController _controller;


        public class CurrencyResponse
        {
            public string SourceCurrency { get; set; }
            public string TargetCurrency { get; set; }
            public decimal OriginalAmount { get; set; }
            public decimal Rate { get; set; }
            public decimal ConvertedAmount { get; set; }
        }

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TechMoveDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TechMoveDbContext(options);

            // Mock HttpClient with fake ZAR rate
            var handler = new FakeHttpMessageHandler();
            var httpClient = new HttpClient(handler);

            _controller = new ServiceRequestsController(_context, httpClient);

            // Seed contract
            _context.Contracts.Add(new Contract
            {
                ContractId = 1,
                Status = Contract.ContractStatus.active,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(5)
            });
            _context.SaveChanges();
        }

        // get service requests
        [Test]
        public async Task GetAll_ServiceRequests_Returns_passed()
        {
            _context.ServiceRequests.Add(new ServiceRequest { Id = 1, Description = "Test SR", ContractId = 1 });
            _context.SaveChanges();

            var result = await _controller.GetAllServiceRequests();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var serviceRequests = okResult?.Value as List<ServiceRequest>;
            Assert.NotNull(serviceRequests);
            Assert.AreEqual(1, serviceRequests.Count);
        }

        //  get service request by id
        [Test]
        public async Task GetServiceRequestById_returns_ServiceRequest_WhenExists()
        {
            _context.ServiceRequests.Add(new ServiceRequest { Id = 2, Description = "ById SR", ContractId = 1 });
            _context.SaveChanges();

            var result = await _controller.GetServiceRequestById(2);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var sr = okResult?.Value as ServiceRequest;
            Assert.NotNull(sr);
            Assert.AreEqual("ById SR", sr.Description);
        }

        [Test]
        public async Task GetServiceRequestById_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.GetServiceRequestById(99);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        // get currency conversion 
        [Test]
        public async Task GetConvertedCurrency_Returns_passed_with_ValidAmount()
        {
            var result = await _controller.GetConvertedCurrency(10m);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var data = System.Text.Json.JsonSerializer.Deserialize<CurrencyResponse>(json);

            Assert.AreEqual("USD", data.SourceCurrency);
            Assert.AreEqual("ZAR", data.TargetCurrency);
            Assert.Greater(data.ConvertedAmount, 0);
        }


        // insert new service request
        [Test]
        public async Task Create_ReturnsCreatedAtActionResult_Only_contractExists()
        {
            var sr = new ServiceRequest { Id = 3, Description = "New SR", ContractId = 1, Cost = 100, Type = ServiceRequest.RequestType.delivery };

            var result = await _controller.Create(sr);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var created = result as CreatedAtActionResult;
            Assert.NotNull(created.Value);
        }

        // Invalid contract id should return bad request

        [Test]
        public async Task Create_ReturnsBadRequest_WhenContractInvalid()
        {
            var sr = new ServiceRequest { Id = 4, Description = "Invalid SR", ContractId = 99, Cost = 100 };

            var result = await _controller.Create(sr);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // update existing service request by id
        [Test]
        public async Task Edit_ReturnsOk_WhenUpdateSuccessful()
        {
            _context.ServiceRequests.Add(new ServiceRequest
            {
                Id = 5,
                Description = "Edit SR",
                ContractId = 1,
                Cost = 50,
                Type = ServiceRequest.RequestType.pickup
            });
            _context.SaveChanges();

            var sr = new ServiceRequest
            {
                Id = 5,
                Description = "Updated SR",
                ContractId = 1,
                Cost = 60,
                Type = ServiceRequest.RequestType.returnRequest
            };

            // remove tracked entity to avoid state conflicts during the test
            var tracked = _context.ServiceRequests.Local.FirstOrDefault(s => s.Id == sr.Id);
            if (tracked != null)
            {
                _context.Entry(tracked).State = EntityState.Detached;
            }

            var result = await _controller.Edit(5, sr);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        // return bad request if id of service request doesn't match id in url
        [Test]
        public async Task Edit_Returns_BadRequest_WhenIdMismatch()
        {
            var sr = new ServiceRequest { Id = 10, Description = "Mismatch SR", ContractId = 1 };
            var result = await _controller.Edit(11, sr);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // remove service request by id
        [Test]
        public async Task Delete_Returns_null_WhenDeleted()
        {
            _context.ServiceRequests.Add(new ServiceRequest { Id = 6, Description = "Delete SR", ContractId = 1 });
            _context.SaveChanges();

            var result = await _controller.Delete(6);

            Assert.IsInstanceOf<NoContentResult>(result);
            var deleted = await _context.ServiceRequests.FindAsync(6);
            Assert.Null(deleted);
        }

        // invalid id should return not found for delete

        [Test]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.Delete(99);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }

    //  Fake HttpMessageHandler to mock currency API
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = @"{ ""conversion_rates"": { ""ZAR"": 20.0 } }";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            return Task.FromResult(response);
        }
    }
}
