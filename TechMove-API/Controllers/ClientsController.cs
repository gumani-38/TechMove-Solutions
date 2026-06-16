using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove_API.Data;
using TechMove_API.Models;

namespace TechMove_API.Controllers
{
    [Authorize]

    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase 
    {
        private readonly TechMoveDbContext _context;

        public ClientsController(TechMoveDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        // POST: api/Clients
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Index), new { id = client.ClientId }, client);
            }
            return BadRequest(ModelState);
        }
        // Get Client by ID: api/Clients/5
        [HttpGet("{id}")] 
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        // PUT: api/Clients/5
        [HttpPut("{id}")] 
        public async Task<IActionResult> Edit(int id, [FromBody] Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest("ID mismatch");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return NoContent(); 
            }
            return BadRequest(ModelState);
        }


        // delete client by id: api/Clients/5
        [HttpDelete("{id}")] 
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
