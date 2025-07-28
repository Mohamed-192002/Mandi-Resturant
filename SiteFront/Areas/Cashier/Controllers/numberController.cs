using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SiteFront.Areas.Cashier.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class numberController : ControllerBase
    {
        private readonly IRepository<PhoneNumberRecord> _numberRepo;

        public numberController(IRepository<PhoneNumberRecord> numberRepo)
        {
            _numberRepo = numberRepo;
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePhoneNumber([FromForm] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest(new { message = "Phone number is required." });

            var phoneRecord = new PhoneNumberRecord { PhoneNumber = phoneNumber };
            _numberRepo.Add(phoneRecord);
            await _numberRepo.SaveAllAsync();

            return Ok(new { message = "Phone number stored successfully.", phoneNumber });
        }

        [HttpGet]
        public async Task<IActionResult> GetLastPhoneNumber()
        {
            var phoneRecord = _numberRepo.GetAllAsync().Result
                .OrderByDescending(p => p.CreatedAt).FirstOrDefault(); 

            if (phoneRecord == null)
                return NotFound(new { message = "No phone number found." });

            return Ok(new { phoneRecord.PhoneNumber });
        }


    }
}
