using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SiteFront.Areas.Cashier.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class numberController : ControllerBase
    {
        private readonly IRepository<PhoneNumberRecord> _numberRepo;
        private readonly IRepository<DeviceRegistration> _deviceRegistrationRepo;
        private readonly UserManager<User> _userManager;

        public numberController(IRepository<PhoneNumberRecord> numberRepo, IRepository<DeviceRegistration> deviceRegistrationRepo, UserManager<User> userManager)
        {
            _numberRepo = numberRepo;
            _deviceRegistrationRepo = deviceRegistrationRepo;
            _userManager = userManager;
        }
        [HttpPost("RegistrationDevice")]
        public async Task<IActionResult> RegistrationDevice([FromForm] RegistrationDeviceRequest registration)
        {
            if (string.IsNullOrWhiteSpace(registration.AccessCode))
                return BadRequest(new { message = "AccessCode is required." });
            var isExist = await _deviceRegistrationRepo.SingleOrDefaultAsync(d => d.DeviceId == registration.DeviceId && !d.IsDeleted);
            if (isExist != null)
                return BadRequest("الجهاز مسجل بالفعل");
            var device = await _deviceRegistrationRepo.SingleOrDefaultAsync(d => d.AccessCode == registration.AccessCode && !d.IsDeleted);
            if (device == null)
                return BadRequest("غير موجود");
            if (device.DeviceId != null)
                return BadRequest("الجهاز مسجل بالفعل");

            device.DeviceId = registration.DeviceId;
            _deviceRegistrationRepo.Update(device);
            await _numberRepo.SaveAllAsync();

            return Ok(new { message = "Registration Device Success" });
        }
        [HttpPost]
        public async Task<IActionResult> ReceivePhoneNumber([FromForm] ReceivePhoneNumberRequest receivePhone)
        {
            if (string.IsNullOrWhiteSpace(receivePhone.PhoneNumber))
                return BadRequest(new { message = "Phone number is required." });

            var device = await _deviceRegistrationRepo.SingleOrDefaultAsync(d => d.DeviceId == receivePhone.DeviceId && !d.IsDeleted);
            if (device == null)
                return BadRequest("الجهاز غير مفعل أو غير مسجل");

            var phoneRecord = new PhoneNumberRecord { PhoneNumber = receivePhone.PhoneNumber, UserId = device.UserId };
            _numberRepo.Add(phoneRecord);
            await _numberRepo.SaveAllAsync();

            return Ok(new { message = "Phone number stored successfully.", receivePhone.PhoneNumber });
        }
        [HttpGet()]
        public async Task<IActionResult> GetLastPhoneNumber()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                    return Unauthorized(new { message = "المستخدم غير مسجل الدخول." });

                var phoneRecords = await _numberRepo.GetAllAsync(x => x.UserId == user.Id);

                var latestPhone = phoneRecords
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                if (latestPhone == null)
                    return NotFound(new { message = "لا يوجد رقم هاتف مسجل." });

                return Ok(new { phoneNumber = latestPhone.PhoneNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب رقم الهاتف.", error = ex.Message });
            }
        }

        public class RegistrationDeviceRequest
        {
            public string DeviceId { get; set; }
            public string AccessCode { get; set; }
        }
        public class ReceivePhoneNumberRequest
        {
            public string DeviceId { get; set; }
            public string PhoneNumber { get; set; }
        }

    }
}
