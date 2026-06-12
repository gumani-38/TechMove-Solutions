using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TechMove_API.Controllers;
using TechMove_API.Data;
using TechMove_API.Models;

namespace TechMoveNunitsTests
{
    [TestFixture]
    public class ContractsTest
    {
        private TechMoveDbContext _context;
        private ContractsController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TechMoveDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TechMoveDbContext(options);
            _controller = new ContractsController(_context);

            // Seed test data
            _context.Clients.Add(new Client { ClientId = 1, ClientName = "Alice", ClientRegion = "Gauteng", ContactDetails = "0618272791" });
            _context.Contracts.AddRange(new List<Contract>
            {
                new Contract { ContractId = 1, ClientId = 1, StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(10), Status = Contract.ContractStatus.active },
                new Contract { ContractId = 2, ClientId = 2, StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-5), Status = Contract.ContractStatus.expired }
            });
            _context.SaveChanges();
        }

        // ✅ GET: api/Contracts with filters
        [Test]
        public async Task GetAllContracts_ReturnsFilteredContracts_ByStatus()
        {
            var result = await _controller.GetAllContracts(null, null, "active");

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var contracts = okResult?.Value as List<Contract>;

            Assert.NotNull(contracts);
            Assert.AreEqual(1, contracts.Count);
            Assert.AreEqual(Contract.ContractStatus.active, contracts[0].Status);
        }

        // ✅ GET: api/Contracts/{id}
        [Test]
        public async Task GetContractById_ReturnsOk_WhenContractExists()
        {
            var result = await _controller.GetContractById(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var contract = okResult?.Value as Contract;

            Assert.NotNull(contract);
            Assert.AreEqual(1, contract.ContractId);
        }

        [Test]
        public async Task GetContractById_ReturnsNotFound_WhenContractDoesNotExist()
        {
            var result = await _controller.GetContractById(2);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        // ✅ POST: api/Contracts
        [Test]
        public async Task Create_ReturnsCreatedAtActionResult_WhenValidPdfUploaded()
        {
            var contract = new Contract
            {
                ClientId = 3,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                Status = Contract.ContractStatus.draft
            };

            var pdfBytes = Encoding.UTF8.GetBytes("Fake PDF content");
            var formFile = new FormFile(new MemoryStream(pdfBytes), 0, pdfBytes.Length, "agreementPdf", "test.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var result = await _controller.Create(contract, formFile);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.NotNull(createdResult.Value);
        }

        [Test]
        public async Task Create_ReturnsBadRequest_WhenNoFileUploaded()
        {
            var contract = new Contract
            {
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                Status = Contract.ContractStatus.draft
            };

            var result = await _controller.Create(contract, null);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // ✅ PUT: api/Contracts/{id}
        [Test]
        public async Task Edit_ReturnsOk_WhenUpdateSuccessful()
        {
            var contract = new Contract
            {
                ContractId = 1,
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10),
                Status = Contract.ContractStatus.active
            };

            // Detach tracked entity to avoid duplicate tracking
            var tracked = _context.Contracts.Local.FirstOrDefault(c => c.ContractId == contract.ContractId);
            if (tracked != null)
            {
                _context.Entry(tracked).State = EntityState.Detached;
            }

            var result = await _controller.Edit(1, contract);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }


        [Test]
        public async Task Edit_ReturnsBadRequest_WhenIdMismatch()
        {
            var contract = new Contract { ContractId = 5, ClientId = 1 };
            var result = await _controller.Edit(1, contract);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // ✅ DELETE: api/Contracts/{id}
        [Test]
        public async Task DeleteContract_ReturnsNoContent_WhenContractDeleted()
        {
            var result = await _controller.DeleteContract(2);

            Assert.IsInstanceOf<NoContentResult>(result);
            var deletedContract = await _context.Contracts.FindAsync(2);
            Assert.Null(deletedContract);
        }

        [Test]
        public async Task DeleteContract_ReturnsNotFound_WhenContractDoesNotExist()
        {
            var result = await _controller.DeleteContract(99);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
