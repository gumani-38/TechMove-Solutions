using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove_API.Data;
using TechMove_API.Models;
using TechMove_API.Observers;
using TechMove_API.States;

namespace TechMove_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly TechMoveDbContext _context;

        public ContractsController(TechMoveDbContext context)
        {
            _context = context;
        }
        [HttpGet] 
        public async Task<IActionResult> GetAllContracts(
          [FromQuery] DateOnly? fromDate,
          [FromQuery] DateOnly? toDate,
          [FromQuery] string? status) 
        {
            IQueryable<Contract> techMoveDbContext = _context.Contracts.Include(c => c.Client);

            // Date filtering
            if (fromDate.HasValue && toDate.HasValue)
            {
                techMoveDbContext = techMoveDbContext
                    .Where(c => DateOnly.FromDateTime(c.StartDate) >= fromDate.Value
                             && DateOnly.FromDateTime(c.EndDate) <= toDate.Value);
            }
            else if (fromDate.HasValue)
            {
                techMoveDbContext = techMoveDbContext
                    .Where(c => DateOnly.FromDateTime(c.StartDate) >= fromDate.Value);
            }
            else if (toDate.HasValue)
            {
                techMoveDbContext = techMoveDbContext
                    .Where(c => DateOnly.FromDateTime(c.EndDate) <= toDate.Value);
            }
            
            // Status filtering
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Contract.ContractStatus>(status, true, out var parsedStatus))
            {
                techMoveDbContext = techMoveDbContext.Where(c => c.Status == parsedStatus);
            }

            var filteredContracts = await techMoveDbContext.ToListAsync();
            return Ok(filteredContracts);
        }

        // POST: api/Contracts
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Contract contract, IFormFile agreementPdf) 
        {
            // Validate file
            if (agreementPdf == null || agreementPdf.Length == 0)
            {
                ModelState.AddModelError("agreementFile", "The signed agreement file is required");
                return BadRequest(ModelState); 
            }

            var extension = Path.GetExtension(agreementPdf.FileName).ToLowerInvariant();
            if (agreementPdf.ContentType != "application/pdf" || extension != ".pdf")
            {
                ModelState.AddModelError("agreementFile", "Only PDF files are allowed.");
                return BadRequest(ModelState);
            }

            if (ModelState.IsValid)
            {
                ApplyContractState(contract);
                if (contract.Request() == "No state assigned")
                {
                    contract.SetState(new DraftState()); // fallback
                }

  
                string executionStateInfo = contract.Request();

     
                _context.Add(contract);
                await _context.SaveChangesAsync();

                // Step 2: Save PDF to local file system using ContractId
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{contract.ContractId}.pdf"; 
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await agreementPdf.CopyToAsync(stream);
                }

                return CreatedAtAction("GetAllContracts", new { id = contract.ContractId }, new { contract, stateInfo = executionStateInfo });
            }
            return BadRequest(ModelState);
        }

        [HttpGet("download/{id}")]
        public IActionResult DownloadAgreement(int id)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts", $"{id}.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Agreement pdf not found.");

            return PhysicalFile(filePath, "application/pdf", $"Contract_{id}_Agreement.pdf");
        }

        // GET: api/Contracts/5
        [HttpGet("{id}")] 
        public async Task<IActionResult> GetContractById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null)
            {
                return NotFound(new { message = $"Contract with ID {id} not found." }); 
            }


            return Ok(contract); 
        }
        // PUT: api/Contracts/5
        [HttpPut("{id}")] 
        public async Task<IActionResult> Edit(int id, [FromBody] Contract contract)
        {
            if (id != contract.ContractId)
            {
                return BadRequest(new { message = "Mismatched Contract ID between URL route and payload body." }); 
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ApplyContractState(contract);
                    contract.Attach(new EmailNotifier());
                    contract.Attach(new DashboardNotifier());

                    contract.NotificationMessage = contract.Request();
                    contract.Notify();

            

                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractId))
                    {
                        return NotFound(new { message = $"Contract with ID {id} not found during concurrency validation." });
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok(new { contract, stateInfo = contract.NotificationMessage });
            }
            return BadRequest(ModelState);
        }

        // delete contract by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound(new { message = $"Contract with ID {id} not found." });
            }
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }



        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
        private void ApplyContractState(Contract contract)
        {
            switch (contract.Status)
            {
                case Contract.ContractStatus.draft:
                    contract.SetState(new DraftState());
                    break;

                case Contract.ContractStatus.active:
                    contract.SetState(new ActiveState());
                    break;

                case Contract.ContractStatus.expired:
                    contract.SetState(new ExpiredState());
                    break;

                case Contract.ContractStatus.onHold:
                    contract.SetState(new OnHoldState());
                    break;
            }
        }
    }
}
