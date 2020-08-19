using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExampleWebAPI.Models;
using ExampleWebAPI.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ExampleWebAPI.Controllers {

    [ApiController]
    [Authorize]
    [Route("events")]
    public class EventsController : ControllerBase {

        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context) {
            _context = context;
        }


        // GET: /events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents([FromQuery(Name = "start-date")] string stringStartDate, [FromQuery(Name = "end-date")] string stringEndDate) {


            // min value is used as the null value (min value is 00:00:00.0000000 UTC, January 1, 0001)
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;

            if (stringStartDate != null) {
                if (!DateTime.TryParse(stringStartDate, out startDate)) {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "invalid start-date format" });
                }
            }
            if (stringEndDate != null) {
                if (!DateTime.TryParse(stringEndDate, out endDate)) {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "invalid end-date format" });
                }
            }




            List<Event> eventsData = await _context.Events.ToListAsync();

            if (startDate == null && endDate == null) {
                return eventsData;
            }

            List<Event> filteredData = eventsData.FindAll(e => (startDate == DateTime.MinValue || e.EventDate > startDate) && (endDate == DateTime.MinValue || e.EventDate < endDate));

            return filteredData;
        }
    }
}
