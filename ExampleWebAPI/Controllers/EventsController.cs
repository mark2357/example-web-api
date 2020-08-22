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
            /**
             * supported date formats (not exclusive) more info here https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tryparse?view=netcore-3.1
             * YYYY-MM-DD
             * YYYY-MM-DD HH:MM:SS
             * YYYY-MM-DDTHH:MM:SS
             * DD/MM/YYYY
            /*
            */

            // min value is used as the null value (min value is 00:00:00.0000000 UTC, January 1, 0001)
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;

            // returns error message specifying how to format date
            if (stringStartDate != null) {
                if (!DateTime.TryParse(stringStartDate, out startDate)) {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "invalid start-date format, use date format YYYY-MM-DD HH:MM:SS" });
                }
            }
            if (stringEndDate != null) {
                if (!DateTime.TryParse(stringEndDate, out endDate)) {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "invalid end-date format, use date format YYYY-MM-DD HH:MM:SS" });
                }
            }



            // gets data from database
            List<Event> eventsData = await _context.Events.ToListAsync();


            if (startDate == null && endDate == null) {
                return eventsData;
            }

            List<Event> filteredData = eventsData.FindAll(e => (startDate == DateTime.MinValue || e.EventDate > startDate) && (endDate == DateTime.MinValue || e.EventDate < endDate));

            return filteredData;
        }
    }
}
