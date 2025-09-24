using AutoMapper;
using Core.Common;
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.DeliveryBillVM;
using Core.ViewModels.DeliveryVM;
using Core.ViewModels.HoleVM;
using Core.ViewModels.ProductVM;
using Core.ViewModels.ResturantDeliveryVM;
using Core.ViewModels.SaleBillPrintVM;
using Core.ViewModels.SaleBillVM;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SiteFront.Services;
using System.Diagnostics;

namespace SiteFront.Areas.Cashier.Controllers
{
    [Area("Cashier")]
    public class SaleBillController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Delivery> _deliveryRepo;
        private readonly IRepository<DeliveryBill> _deliveryBillRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<MeatFilling> _meatFillingRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IRepository<Driver> _driverRepo;
        private readonly UserManager<User> _userManager;
        private readonly WhatsAppService _whatsAppService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<User> _userRepo;
        private readonly IConfiguration _configuration;
        private readonly IRepository<PrinterRegistration> _printerRegistration;

        public SaleBillController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<SaleBill> saleBillRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Customer> customerRepo,
            IRepository<Delivery> deliveryRepo,
            IRepository<DeliveryBill> deliveryBillRepo,
            IRepository<Product> productRepo,
            IRepository<Hole> holeRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<MeatFilling> meatFillingRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IRepository<Driver> driverRepo,
            UserManager<User> userManager,
            WhatsAppService whatsAppService,
            IWebHostEnvironment webHostEnvironment,
            IRepository<User> userRepo,
            IConfiguration configuration,
            IRepository<PrinterRegistration> printerRegistration)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _saleBillRepo = saleBillRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _customerRepo = customerRepo;
            _deliveryRepo = deliveryRepo;
            _deliveryBillRepo = deliveryBillRepo;
            _productRepo = productRepo;
            _holeRepo = holeRepo;
            _chickenFillingRepo = chickenFillingRepo;
            _meatFillingRepo = meatFillingRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
            _meatHoleMovementRepo = meatHoleMovementRepo;
            _driverRepo = driverRepo;
            _userManager = userManager;
            _whatsAppService = whatsAppService;
            _webHostEnvironment = webHostEnvironment;
            _userRepo = userRepo;
            _configuration = configuration;
            _printerRegistration = printerRegistration;
        }

        [Authorize("Permissions.SaleBillIndex")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAllAsync(c => !c.IsDeleted, true);
            var productGetVM = _mapper.Map<List<ProductGetVM>>(products);
            var holes = await _holeRepo.GetAllAsync(c => !c.IsDeleted, true);
            var holeGetVM = _mapper.Map<List<HoleGetVM>>(holes);
            var deliveries = await _deliveryRepo.GetAllAsync(c => !c.IsDeleted, true);
            var saleBillRefundVM = _saleBillRepo.GetAllAsync().Result
                .Where(s => s.Date.Date == DateTime.Now.Date && s.BillType != BillType.Safary)
                .Select(s => new SaleBillRefundVM
                {
                    Id = s.Id,
                    BillType = s.BillType,
                    BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(d => d.SaleBillId == s.Id).Result.Select(d => new BillDetailRegisterVM
                    {
                        SaleBillId = d.SaleBillId,
                        Amount = d.Amount,
                        Price = d.Price,
                        ProductId = d.ProductId,
                        TotalPrice = d.TotalPrice,
                        Discount = d.Discount,
                        PName = _productRepo.GetByIdAsync(d.ProductId).Result.Name
                    }).ToList()
                }).ToList();

            var saleBills = _saleBillRepo.GetAllAsync().Result
                .Where(s => s.Date.Date == DateTime.Now.Date)
                .Select(s => new CommonDrop
                {
                    Id = s.Id,
                    Name = s.Id.ToString()
                }).ToList();
            var today = DateTime.Today;
            var dagagHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
                .Select(h => new DagagHoleDataVM
                {
                    Id = h.Id,
                    Name = h.Name,
                    HoleType = h.HoleType,
                    EndTime = _chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                    Amount = _chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut)
                }).ToList();

            var meatHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                .Select(h => new MeatHoleDataVM
                {
                    Id = h.Id,
                    Name = h.Name,
                    HoleType = h.HoleType,
                    EndTime = _meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                    NafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                    HalfNafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
                }).ToList();

            var saleBillMainVM = new SaleBillMainVM
            {
                ProductGetVM = productGetVM,
                DagagHoleDataVM = dagagHoleDataVM,
                MeatHoleDataVM = meatHoleDataVM,
                Deliveries = _mapper.Map<List<DeliveryGetVM>>(deliveries),
                Products = _mapper.Map<List<CommonDrop>>(products),
                SaleBillRefundVM = saleBillRefundVM,
                SaleBills = saleBills
            };
            return View(saleBillMainVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepo.GetAllAsync(c => !c.IsDeleted, true);
            var productGetVM = _mapper.Map<List<ProductGetVM>>(products);
            return PartialView("_PartialProducts", productGetVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewCustomer(SaleBillMainVM model)
        {
            if (ModelState.IsValid)
            {
                var checkCustomer = await _customerRepo.SingleOrDefaultAsync(c => c.Phone == model.CustomerRegisterVM.Phone);
                if (checkCustomer == null)
                {
                    var customerDb = _mapper.Map<Customer>(model.CustomerRegisterVM);
                    customerDb.CreatedDate = DateTime.Now;
                    customerDb.LastEditDate = DateTime.Now;
                    customerDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    customerDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _customerRepo.Add(customerDb);
                    await _customerRepo.SaveAllAsync();
                    return Ok(customerDb.Id);
                }
                else
                {
                    checkCustomer.LastEditDate = DateTime.Now;
                    checkCustomer.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    checkCustomer.Name = model.CustomerRegisterVM.Name;
                    checkCustomer.Address = model.CustomerRegisterVM.Address;
                    checkCustomer.Address2 = model.CustomerRegisterVM.Address2;
                    checkCustomer.Address3 = model.CustomerRegisterVM.Address3;
                    checkCustomer.Address4 = model.CustomerRegisterVM.Address4;
                    checkCustomer.AnotherPhone = model.CustomerRegisterVM.AnotherPhone;
                    _customerRepo.Update(checkCustomer);
                    await _customerRepo.SaveAllAsync();
                    return Ok(checkCustomer.Id);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerData(string phone)
        {
            var customerDb = await _customerRepo.SingleOrDefaultAsync(c => c.Phone == phone);
            if (customerDb != null)
            {
                return Json(customerDb);
            }
            else
            {
                return Json(Empty);
            }
        }
        [HttpGet]
        public IActionResult GetCurrentHoles()
        {
            var today = DateTime.Today;
            //var dagagHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
            //   .Select(h => new DagagHoleDataVM
            //   {
            //       Id = h.Id,
            //       Name = h.Name,
            //       HoleType = h.HoleType,
            //       EndTime = _chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
            //       Amount = _chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut)
            //   }).ToList();

            //var meatHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
            //    .Select(h => new MeatHoleDataVM
            //    {
            //        Id = h.Id,
            //        Name = h.Name,
            //        HoleType = h.HoleType,
            //        EndTime = _meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
            //        NafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut),
            //        HalfNafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
            //    }).ToList();


            //return Json(string.Empty);
            var dagagHoles = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
              .Select(h => new
              {
                  id = h.Id,
                  name = h.Name
              });

            var meatHoles = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                .Select(h => new
                {
                    id = h.Id,
                    name = h.Name
                });

            var allHoles = dagagHoles.Concat(meatHoles).ToList();

            return Json(allHoles);
        }

        //All Day Bill
        [HttpGet]
        public async Task<IActionResult> AllDayBill()
        {
            var today = DateTime.Today;
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.Date.Date == today.Date && d.BillType != BillType.Reservation, true, d => d.Delivery, d => d.Customer);
            var deliveryBillGetVM = deliveryBills.Select(b => new DeliveryBillGetVM
            {
                Id = b.Id,
                BillType = b.BillType,
                Date = b.Date,
                FinalTotal = b.FinalTotal,
                OrderNumber = b.OrderNumber,
                DeliveryName = b.DeliveryId != null ? _deliveryRepo.GetByIdAsync((int)b.DeliveryId).Result.Name : null,
                CustomerName = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Name : null,
                CustomerAddress = b.CustomerAddress,
                BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == b.Id).Result.Select(c => new BillDetailRegisterVM
                {
                    PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                    Price = c.Price,
                    Amount = c.Amount,
                    TotalPrice = c.TotalPrice
                }).ToList()
            }).ToList();
            return View(deliveryBillGetVM);
        }

        //Company Delivery
        [HttpGet]
        public async Task<IActionResult> DeliveryBill()
        {
            var today = DateTime.Today;
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.MoneyDelivered && d.Date.Date == today.Date && d.BillType == BillType.Delivery, true, d => d.Delivery, d => d.Customer);
            var deliveryBillGetVM = deliveryBills.Select(b => new DeliveryBillGetVM
            {
                Id = b.Id,
                Date = b.Date,
                FinalTotal = b.FinalTotal,
                DeliveryName = b.DeliveryId != null ? _deliveryRepo.GetByIdAsync((int)b.DeliveryId).Result.Name : null,
                CustomerName = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Name : null,
                CustomerAddress = b.CustomerAddress,
                OrderNumber = b.OrderNumber,
                BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == b.Id).Result.Select(c => new BillDetailRegisterVM
                {
                    PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                    Price = c.Price,
                    Amount = c.Amount,
                    TotalPrice = c.TotalPrice
                }).ToList()
            }).ToList();
            return View(deliveryBillGetVM);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBillAsync(int id)
        {
            var bill = await _saleBillRepo.GetByIdAsync(id);
            if (bill == null)
                return NotFound();

            _saleBillRepo.Delete(bill);
            foreach (var chickenHoleMovement in await _chickenHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale))
            {
                _chickenHoleMovementRepo.Delete(chickenHoleMovement);
            }
            foreach (var meatHoleMovement in await _meatHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale))
            {
                _meatHoleMovementRepo.Delete(meatHoleMovement);
            }
            await _saleBillDetailRepo.SaveAllAsync();
            return Ok();
        }

        //Resturant Delivery
        [HttpGet]
        public async Task<IActionResult> ResturantDelivery()
        {
            var today = DateTime.Today;
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.MoneyDelivered && d.Date.Date == today.Date && d.BillType == BillType.Driver, true, d => d.Driver, d => d.Customer);
            var deliveryBillGetVM = deliveryBills.Select(b => new DeliveryBillGetVM
            {
                Id = b.Id,
                Date = b.Date,
                FinalTotal = b.FinalTotal,
                DriverId = (int)(b.DriverId != null ? b.DriverId : 0),
                CustomerName = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Name : null,
                CustomerAddress = b.CustomerAddress,
                CustomerPhone = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Phone : null,
                BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == b.Id).Result.Select(c => new BillDetailRegisterVM
                {
                    PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                    Price = c.Price,
                    Amount = c.Amount,
                    TotalPrice = c.TotalPrice
                }).ToList(),
                OrderDeliveredTime = b.OrderDeliveredTime
            }).OrderBy(d => d.DriverId != 0).ThenBy(d => d.OrderDeliveredTime == null).ThenBy(d => d.OrderDeliveredTime).ToList();
            var drivers = await _driverRepo.GetAllAsync(d => !d.IsDeleted, true);
            var resturantDeliveryMainVM = new ResturantDeliveryMainVM
            {
                DeliveryBillGetVM = deliveryBillGetVM,
                Drivers = _mapper.Map<List<CommonDrop>>(drivers)
            };
            return View(resturantDeliveryMainVM);
        }

        //Send WhatsApp Message
        public async Task<IActionResult> HandleBillDriver(int driverId, int saleBillId)
        {
            var driverById = await _driverRepo.GetByIdAsync(driverId);
            if (driverById == null)
                return NotFound();
            var saleBillById = await _saleBillRepo.GetByIdAsync(saleBillId);
            if (saleBillById == null)
                return NotFound();
            var customer = await _customerRepo.GetByIdAsync((int)saleBillById.CustomerId);
            var saleBillDetails = await _saleBillDetailRepo.GetAllAsync(s => s.SaleBillId == saleBillId);
            string billDetails = "";
            foreach (var item in saleBillDetails)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                billDetails += $"{product.Name} - الكمية: {item.Amount}, السعر: {item.Price} | ";
            }
            billDetails = billDetails.TrimEnd('|', ' ');

            //string message = $"قم بتوصيل طلب رقم {saleBillById.Id} الي العميل {customer.Name} في عنوان {customer.Address} ورقم هاتفه {customer.Phone}. تفاصيل الطلب : ({billDetails}) ,السعر الكلي :  {saleBillById.FinalTotal}";
            string message = $"قم بتوصيل طلب\n" +
                 $"رقم الطلب : {saleBillById.Id}\n" +
                 $"اسم العميل: {customer.Name}\n" +
                 $"العنوان : {customer.Address}\n" +
                 $"رقم الهاتف: {customer.Phone}\n" +
                 $"تفاصيل الطلب : \n({billDetails})\n" +
                 $"السعر الكلي : {saleBillById.FinalTotal}";

            string phone = driverById.Phone;
            if (phone.StartsWith("0"))
            {
                phone = phone.Substring(1);
            }
            var success = await _whatsAppService.SendMessageAsync(phone, message);
            if (success)
            {
                try
                {
                    //For Print AllBill
                    var billHallPrintVM = new BillHallPrintVM
                    {
                        Billnumber = saleBillById.Id.ToString(),
                        Date = saleBillById.Date,
                        BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == saleBillById.Id).Result.Select(c => new BillDetailRegisterVM
                        {
                            PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                            Price = c.Price,
                            Amount = c.Amount,
                            TotalPrice = c.TotalPrice
                        }).ToList(),
                        TotalPrice = saleBillById.FinalTotal,
                        Discount = saleBillById.Discount,
                        Vat = saleBillById.Vat,
                        Notes = saleBillById.Notes,
                        OrderDeliveredTime = saleBillById.OrderDeliveredTime,
                        CashierName = _userRepo.GetByIdAsync(saleBillById.CreatedUser).Result.Name,
                        CustomerAddress = saleBillById.CustomerAddress
                    };
                    if (saleBillById.CustomerId != null)
                    {
                        billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)saleBillById.CustomerId).Result.Phone;
                    }
                    if (driverId != 0)
                    {
                        billHallPrintVM.DriverName = _driverRepo.GetByIdAsync(driverId).Result.Name;
                    }
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user == null)
                        throw new Exception("User not found.");
                    // Get the printer names from configuration
                    var print = await _printerRegistration.SingleOrDefaultAsync(x => x.UserId == user.Id);
                    if (print == null)
                        throw new Exception("Printer not registered for user.");
                    //var printerName = _configuration["CashierPrinterName"];
                    var printerName = print.Name;
                    var printerName2 = _configuration["DeliveryPrinterName"];
                    await PrintPdfAsync(filePathBill, printerName);
                    await PrintPdfAsync(filePathBill, printerName2);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                }
                saleBillById.DriverId = driverId;
                saleBillById.MoneyDelivered = true;
                saleBillById.DateDelivered = DateTime.Now;
                saleBillById.UserDelivered = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _saleBillRepo.Update(saleBillById);
                await _saleBillRepo.SaveAllAsync();
                return Ok(new { message = "Message sent successfully!" });
            }
            return BadRequest(new { error = "MESSAGE_ERROR" });
        }

        //Customer Recieve
        [HttpGet]
        public async Task<IActionResult> CustomerRecieve()
        {
            var today = DateTime.Today;
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.MoneyDelivered && d.Date.Date == today.Date && d.BillType == BillType.Reservation, true, d => d.Customer);
            var deliveryBillGetVM = deliveryBills.Select(b => new DeliveryBillGetVM
            {
                Id = b.Id,
                Date = b.Date,
                FinalTotal = b.FinalTotal,
                CustomerName = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Name : null,
                CustomerAddress = b.CustomerAddress,
                CustomerPhone = b.CustomerId != null ? _customerRepo.GetByIdAsync((int)b.CustomerId).Result.Phone : null,
                BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == b.Id).Result.Select(c => new BillDetailRegisterVM
                {
                    PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                    Price = c.Price,
                    Amount = c.Amount,
                    TotalPrice = c.TotalPrice
                }).ToList()
            }).ToList();
            return View(deliveryBillGetVM);
        }

        //Print And Delivered Bill Reservation
        public async Task<IActionResult> PrintAndDelivered(int id)
        {
            var SaleBillById = await _saleBillRepo.GetByIdAsync(id);
            if (SaleBillById != null)
            {
                try
                {
                    try
                    {
                        //For Print AllBill
                        var billHallPrintVM = new BillHallPrintVM
                        {
                            Billnumber = SaleBillById.Id.ToString(),
                            Date = SaleBillById.Date,
                            BillDetailRegisterVM = _saleBillDetailRepo.GetAllAsync(c => c.SaleBillId == SaleBillById.Id).Result.Select(c => new BillDetailRegisterVM
                            {
                                PName = _productRepo.GetByIdAsync(c.ProductId).Result.Name,
                                Price = c.Price,
                                Amount = c.Amount,
                                TotalPrice = c.TotalPrice
                            }).ToList(),
                            TotalPrice = SaleBillById.FinalTotal,
                            Discount = SaleBillById.Discount,
                            Vat = SaleBillById.Vat,
                            Notes = SaleBillById.Notes,
                            OrderDeliveredTime = SaleBillById.OrderDeliveredTime,
                            CashierName = _userRepo.GetByIdAsync(SaleBillById.CreatedUser).Result.Name,
                            CustomerAddress = SaleBillById.CustomerAddress
                        };
                        if (SaleBillById.CustomerId != null)
                        {
                            billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)SaleBillById.CustomerId).Result.Phone;
                        }
                        var filePathBill = GenerateReceipt(billHallPrintVM);
                        var user = await _userManager.GetUserAsync(HttpContext.User);
                        if (user == null)
                            throw new Exception("User not found.");
                        // Get the printer names from configuration
                        var print = await _printerRegistration.SingleOrDefaultAsync(x => x.UserId == user.Id);
                        if (print == null)
                            throw new Exception("Printer not registered for user.");
                        //var printerName = _configuration["CashierPrinterName"];
                        var printerName = print.Name;
                        var printerName2 = _configuration["SafaryPrinterName"];
                        await PrintPdfAsync(filePathBill, printerName);
                        await PrintPdfAsync(filePathBill, printerName2);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                    }
                    //Delivered Money
                    SaleBillById.MoneyDelivered = true;
                    SaleBillById.DateDelivered = DateTime.Now;
                    SaleBillById.UserDelivered = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _saleBillRepo.Update(SaleBillById);
                    await _saleBillRepo.SaveAllAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "TRANSACTION_ERROR", message = "Transaction failed: " + ex.Message });
                }
            }
            else
            {
                return NotFound();
            }
        }

        //Retutn Customer Recieve To Driver
        public async Task<IActionResult> ReturnToDriver(int id)
        {
            var deliverySaleBillById = await _saleBillRepo.GetByIdAsync(id);
            if (deliverySaleBillById != null)
            {
                deliverySaleBillById.BillType = BillType.Driver;
                _saleBillRepo.Update(deliverySaleBillById);
                await _saleBillRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم تحويل الفاتورة للتوصيل عبر المطعم");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("لم يتم التحويل بنجاح حاول لاحقا");
            }
            return RedirectToAction(nameof(CustomerRecieve));
        }

        //Cancel Bill Reservation
        public async Task<IActionResult> CancelRerservation(int id)
        {
            var saleBillById = await _saleBillRepo.GetByIdAsync(id);
            if (saleBillById != null)
            {
                var saleBillDetails = await _saleBillDetailRepo.GetAllAsync(s => s.SaleBillId == id);
                if (saleBillDetails.Count() > 0)
                {
                    _saleBillDetailRepo.DeletelistRange(saleBillDetails.ToList());
                }
                var chickenHoleMovement = await _chickenHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale);
                if (chickenHoleMovement.Count() > 0)
                {
                    _chickenHoleMovementRepo.DeletelistRange(chickenHoleMovement.ToList());
                }
                var meatHoleMovement = await _meatHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale);
                if (meatHoleMovement.Count() > 0)
                {
                    _meatHoleMovementRepo.DeletelistRange(meatHoleMovement.ToList());
                }
                _saleBillRepo.Delete(saleBillById);
                await _saleBillRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم إلغاء الحجز بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("لم يتم الالغاء حدث خطأ");
            }

            return RedirectToAction(nameof(CustomerRecieve));
        }

        //Change Delivered For All Bill
        [HttpPost]
        public async Task<IActionResult> ChangeBillDelivered(int id)
        {
            var deliverySaleBillById = await _saleBillRepo.GetByIdAsync(id);
            if (deliverySaleBillById != null)
            {
                deliverySaleBillById.MoneyDelivered = true;
                deliverySaleBillById.DateDelivered = DateTime.Now;
                deliverySaleBillById.UserDelivered = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _saleBillRepo.Update(deliverySaleBillById);
                await _saleBillRepo.SaveAllAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet]
        public async Task<IActionResult> getProductPrice(int productId)
        {
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById == null)
                return NotFound();
            return Json(new { price = productById.SellingPrice });
        }

        [HttpGet]
        public async Task<IActionResult> TemporaryBill()
        {
            var tempSaleBills = await _saleBillRepo.GetAllAsync(d => d.Temporary, true);
            var saleBillGetVM = _mapper.Map<List<SaleBillGetVM>>(tempSaleBills);
            return View(saleBillGetVM);
        }

        public async Task<IActionResult> DeleteBill(int id)
        {
            var saleBillById = await _saleBillRepo.GetByIdAsync(id);
            if (saleBillById != null)
            {
                var chickenHoleMovement = await _chickenHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale);
                if (chickenHoleMovement.Count() > 0)
                {
                    _chickenHoleMovementRepo.DeletelistRange(chickenHoleMovement.ToList());
                }
                var meatHoleMovement = await _meatHoleMovementRepo.GetAllAsync(c => c.HoleMovementTypeId == id && c.HoleMovementType == HoleMovementType.Sale);
                if (meatHoleMovement.Count() > 0)
                {
                    _meatHoleMovementRepo.DeletelistRange(meatHoleMovement.ToList());
                }
                saleBillById.IsDeleted = true;
                _saleBillRepo.Update(saleBillById);
                await _saleBillRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم إلغاء الفاتورة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("لم يتم الالغاء حدث خطأ");
            }
            return RedirectToAction("AllDayBill");
        }

        private string GenerateReceipt(BillHallPrintVM model)
        {
            // Define the custom page size (80 mm wide)
            float pageWidth = 80f * 2.8346f; // 80mm to points (1mm = 2.8346 points)
                                             //float pageHeight = 297f * 2.8346f;
            int rowCount = model.BillDetailRegisterVM.Count;
            float rowHeight = 20f;            // متوسط ارتفاع لكل صف
            float headerHeight = 150f;        // اللوجو + البيانات
            float footerHeight = 100f;        // الفوتر (الشركة - الخطوط)
            float pageHeight = headerHeight + (rowCount * rowHeight) + footerHeight;
            Document document = new Document(new Rectangle(pageWidth, pageHeight), 2, 2, 0, 0); // Margins (left, right, top, bottom)

            // Set up a memory stream to create the PDF
            using (MemoryStream workStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, workStream);
                writer.CloseStream = false;
                document.Open();

                // Load and add the logo image at the top of the PDF
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "NaseerMandi22.jpeg"); // Path to your logo
                if (System.IO.File.Exists(logoPath))
                {
                    Image logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(120f, 120f); // Adjust the size to fit the PDF
                    logo.Alignment = Element.ALIGN_CENTER; // Center the logo
                    document.Add(logo); // Add the logo to the document
                }

                // Set up Arabic font
                string arialFontPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "fonts", "ARIAL.TTF"); // Path to your Arabic font
                BaseFont bfArialUnicode = BaseFont.CreateFont(arialFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font arabicFont = new Font(bfArialUnicode, 12, Font.BOLD, BaseColor.BLACK);
                Font arabicFont22 = new Font(bfArialUnicode, 8, Font.BOLD, BaseColor.BLACK);

                var al = new ArabicLigaturizer();
                float cellHeight = 30f;
                // Create a table with 2 columns to display two items per row
                PdfPTable infoTable = new PdfPTable(1);
                infoTable.WidthPercentage = 100;
                infoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // Set Right-to-Left direction

                // Add receipt details (Branch and Customer Info) in two columns
                infoTable.AddCell(new PdfPCell(new Phrase($"رقم الفاتورة : {model.Billnumber}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                infoTable.AddCell(new PdfPCell(new Phrase($"تاريخ الفاتورة : {model.Date}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                infoTable.AddCell(new PdfPCell(new Phrase($" كاشير : {model.CashierName}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                if (!string.IsNullOrEmpty(model.DriverName))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" السائق : {model.DriverName}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                if (model.OrderDeliveredTime != null)
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" وقت الاستلام : {model.OrderDeliveredTime}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                if (!string.IsNullOrEmpty(model.CustomerAddress))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" عنوان العميل : {model.CustomerAddress}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                if (!string.IsNullOrEmpty(model.CustomerPhone))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" هاتف العميل : {model.CustomerPhone}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(infoTable); // Add the info table to the document

                PdfPTable infoTable2 = new PdfPTable(1);
                infoTable2.WidthPercentage = 100;
                infoTable2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                infoTable2.AddCell(new PdfPCell(new Phrase("الاصناف", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, FixedHeight = cellHeight });

                document.Add(infoTable2);
                //document.Add(new Paragraph(al.Process("الاصناف"), arabicFont) { Alignment = Element.ALIGN_CENTER });

                // Create Table for Items (Arabic headers)
                PdfPTable table = new PdfPTable(3); // 3 columns: Item, Quantity, Price
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // Set Right-to-Left direction for the table
                table.AddCell(new PdfPCell(new Phrase("الصنف", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER, FixedHeight = cellHeight });
                table.AddCell(new PdfPCell(new Phrase("العدد", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER, FixedHeight = cellHeight });
                table.AddCell(new PdfPCell(new Phrase("السعر", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER, FixedHeight = cellHeight });

                // Add data to table
                foreach (var item in model.BillDetailRegisterVM)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.PName.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(item.Amount.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(item.TotalPrice.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(table);
                //document.Add(new Paragraph(al.Process($"الخصم : {model.Discount}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                //document.Add(new Paragraph(al.Process($"الضريبة : {model.Vat}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    document.Add(new Paragraph(al.Process($"ملاحظات : {model.Notes}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                }
                if (model.Discount != 0)
                {
                    document.Add(new Paragraph(al.Process($" الخصم : {model.Discount}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                }
                document.Add(new Paragraph(al.Process($"السعر الكلي : {model.TotalPrice}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph(al.Process("ــــــــــــــــــــــــــــ"), arabicFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph(al.Process("شركة المهد لتكنولوجيا و نظم المعلومات"), arabicFont22) { Alignment = Element.ALIGN_CENTER });

                // Close the document
                document.Close();

                // Save the PDF to the server
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = $"Receipt_{DateTime.Now.Ticks}.pdf"; // Unique file name
                string filePath = Path.Combine(wwwRootPath, "receipts", fileName);

                // Ensure the "receipts" folder exists in wwwroot
                if (!Directory.Exists(Path.Combine(wwwRootPath, "receipts")))
                {
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "receipts"));
                }

                // Write the PDF to the file
                System.IO.File.WriteAllBytes(filePath, workStream.ToArray());

                return filePath; // Return filePath
            }
        }

        static async Task PrintPdfAsync(string pdfFilePath, string printerName)
        {
            // Ensure the PDF file exists
            if (!System.IO.File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException("The specified PDF file was not found.", pdfFilePath);
            }

            string filePath = "C:\\Program Files\\SumatraPDF\\SumatraPDF.exe";

            // Set up the process to run SumatraPDF for silent printing
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                // Path to SumatraPDF
                FileName = filePath,
                Arguments = $"-print-to \"{printerName}\" \"{pdfFilePath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                // Start the print process asynchronously
                await Task.Run(() =>
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit(500);  // Wait for the print process to finish
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to print the PDF using SumatraPDF.", ex);
            }
        }
    }
}
