using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookCovers.API.Controllers
{
    [Route("api/bookcovers")]
    [ApiController]
    public class BookCoversController : ControllerBase
    {
        [HttpGet("{name}")]
        public async Task<IActionResult> GetBookCover(string name, bool returnFault = false)
        {
            // if returnFault is true, wait 500ms and
            // return an Internal Server Error
            if (returnFault)
            {
                // to mimic that something is wrong
                await Task.Delay(500);
                return new StatusCodeResult(500);
            }

            // generate a "book cover" (byte array) between 2 and 10MB
            // this random generation of bytes to mimic long running action of getting book cover from external service
            var random = new Random();
            int fakeCoverBytes = random.Next(2097152, 10485760);            
            byte[] fakeCover = new byte[fakeCoverBytes];
            random.NextBytes(fakeCover);

            return Ok(new
            {
                Name = name,
                Content = fakeCover
            });
        }
    }
}
