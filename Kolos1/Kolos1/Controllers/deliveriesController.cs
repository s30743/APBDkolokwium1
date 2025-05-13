using Kolos1.Exceptions;
using Kolos1.Models;
using Kolos1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kolos1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class deliveriesController : ControllerBase
    {
        IdeliverService _deliverService;

        public deliveriesController(IdeliverService deliverService)
        {
            _deliverService = deliverService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeliversById(int id)
        {
            try
            {
                var res = _deliverService.GetDeliversById(id);
                return Ok(res);
            }
            catch (NotFoundEx ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> addDeliver([FromBody] PostDeliverModel deliver)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _deliverService.addNewDeliver(deliver);
            }
            catch (NotFoundEx e)
            {
                return NotFound(e.Message);
            }
            catch (ConflictEx e)
            {
                return Conflict(e.Message);
            }

            return Created("","Utworzono nowy rekord");
        }
    }
}
