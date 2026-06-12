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
            // 2. Changed from return View() to return Ok() with data
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        // POST: api/Clients
        [HttpPost]
        // 3. Removed [ValidateAntiForgeryToken] and [Bind] (not used in REST APIs)
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();

                // 4. Returns a standard HTTP 201 Created response pointing to the resource
                return CreatedAtAction(nameof(Index), new { id = client.ClientId }, client);
            }
            return BadRequest(ModelState);
        }
        // Get Client by ID: api/Clients/5
        [HttpGet("{id}")] // 5. Added route parameter to get client by
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
        [HttpPut("{id}")] // 5. Changed verb to HttpPut and added route parameter to resolve Swagger conflicts
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
                return NoContent(); // 6. Standard API response for a successful update (HTTP 204)
            }
            return BadRequest(ModelState);
        }


        // delete client by id: api/Clients/5
        [HttpDelete("{id}")] // 9. Added route parameter to delete client by
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent(); // 10. Standard API response for successful deletion
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
