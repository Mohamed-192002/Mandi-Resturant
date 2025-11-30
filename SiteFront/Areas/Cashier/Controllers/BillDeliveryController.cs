using AutoMapper;
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.HoleVM;
using Core.ViewModels.SaleBillPrintVM;
using Core.ViewModels.SaleBillVM;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SiteFront.Services;
using System.Diagnostics;

namespace SiteFront.Areas.Cashier.Controllers
{
    [Area("Cashier")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class BillDeliveryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Delivery> _deliveryRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly IRepository<PrinterRegistration> _printerRegistration;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<MeatFilling> _meatFillingRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<User> _userRepo;

        public BillDeliveryController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Delivery> deliveryRepo,
            IRepository<Customer> customerRepo,
            IRepository<Product> productRepo,
            IRepository<Hole> holeRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<MeatFilling> meatFillingRepo,
            IRepository<PrinterRegistration> printerRegistration,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            UserManager<User> userManager,
            IRepository<User> userRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _deliveryRepo = deliveryRepo;
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _holeRepo = holeRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
            _printerRegistration = printerRegistration;
            _meatHoleMovementRepo = meatHoleMovementRepo;
            _chickenFillingRepo = chickenFillingRepo;
            _meatFillingRepo = meatFillingRepo;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _userManager = userManager;
            _userRepo = userRepo;
        }

        //Save Bill By Holes
        [HttpPost("SaveSaleDelivery")]
        public async Task<IActionResult> SaveSaleDelivery([FromBody] BillDeliveryRegisterVM model)
        {
            try
            {
                var saleBillDb = new SaleBill
                {
                    CreatedDate = DateTime.Now,
                    LastEditDate = DateTime.Now,
                    CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    DeliveryId = model.DeliveryId,
                    CustomerId = model.CustomerId,
                    BillType = BillType.Delivery,
                    Date = DateTime.Now,
                    Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
                    Discount = model.Discount,
                    Vat = model.Vat,
                    FinalTotal = model.FinalTotal,
                    DeliveryPrice = model.DeliveryPrice,
                    OrderDeliveredTime = model.OrderDeliveredTime != "" ? TimeOnly.Parse(model.OrderDeliveredTime) : null,
                    Notes = model.Notes,
                    MoneyDelivered = false,
                    CustomerAddress = model.CustomerAddress,
                    OrderNumber = model.OrderNumber
                };
                _saleBillRepo.Add(saleBillDb);
                await _saleBillRepo.SaveAllAsync();

                //All Selected Holes
                var holes = new List<Hole>();
                foreach (var holeId in model.HoleIds)
                {
                    var hole = await _holeRepo.GetByIdAsync(Convert.ToInt32(holeId));
                    holes.Add(hole);
                }
                //BillDetail
                foreach (var saleDetail in model.BillDetailRegisterVM)
                {
                    var saleDetailDb = _mapper.Map<SaleBillDetail>(saleDetail);
                    saleDetailDb.SaleBillId = saleBillDb.Id;
                    saleDetailDb.CreatedDate = DateTime.Now;
                    saleDetailDb.LastEditDate = DateTime.Now;
                    saleDetailDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    saleDetailDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _saleBillDetailRepo.Add(saleDetailDb);
                    await _saleBillDetailRepo.SaveAllAsync();

                    //Handle Holes
                    await handleDagag(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, holes);
                    await HandleMeatNafr(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, holes);
                    await HandleMeatHalfNafr(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, holes);
                }

                //For Print AllBill
                var billHallPrintVM = new BillHallPrintVM
                {
                    Billnumber = saleBillDb.Id.ToString(),
                    Date = saleBillDb.Date,
                    BillDetailRegisterVM = model.BillDetailRegisterVM,
                    TotalPrice = model.FinalTotal,
                    Discount = model.Discount,
                    Vat = model.Vat,
                    DeliveryPrice = model.DeliveryPrice,
                    Notes = model.Notes,
                    CashierName = _userRepo.GetByIdAsync(saleBillDb.CreatedUser).Result.Name
                };
                if (model.CustomerAddress != null)
                {
                    billHallPrintVM.CustomerAddress = model.CustomerAddress;
                }
                if (model.CustomerId != null)
                {
                    billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Phone;
                    billHallPrintVM.CustomerName = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Name;
                }
                if (model.DeliveryId != 0)
                {
                    billHallPrintVM.DeliveryName = _deliveryRepo.GetByIdAsync((int)model.DeliveryId).Result.Name;
                }
                try
                {
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user == null)
                        throw new Exception("User not found.");
                    // Get the printer names from configuration
                    var print = await _printerRegistration.SingleOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
                    if (print == null)
                        throw new Exception("Printer not registered for user.");
                    //var printerName = _configuration["CashierPrinterName"];
                    var printerName = print.Name;
                    var printerName2 = _configuration["DeliveryPrinterName"];
                    //  await PrintPdfAsync(filePathBill, printerName);
                    await ApiHelper.SendToApi(filePathBill, printerName, print.IpAddress);
                    await PrintPdfAsync(filePathBill, printerName2);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "TRANSACTION_ERROR", message = "Transaction failed: " + ex.Message });
            }
        }

        //Save Bill By Time
        [HttpPost("SaveSaleDeliveryByTime")]
        public async Task<IActionResult> SaveSaleDeliveryByTime([FromBody] BillDeliveryRegisterVM model)
        {
            try
            {
                var saleBillDb = new SaleBill
                {
                    CreatedDate = DateTime.Now,
                    LastEditDate = DateTime.Now,
                    CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    DeliveryId = model.DeliveryId,
                    CustomerId = model.CustomerId,
                    BillType = BillType.Delivery,
                    Date = DateTime.Now,
                    Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
                    Discount = model.Discount,
                    Vat = model.Vat,
                    DeliveryPrice = model.DeliveryPrice,
                    FinalTotal = model.FinalTotal,
                    OrderDeliveredTime = string.IsNullOrEmpty(model.OrderDeliveredTime) ? null : TimeOnly.Parse(model.OrderDeliveredTime),
                    Notes = model.Notes,
                    MoneyDelivered = false,
                    CustomerAddress = model.CustomerAddress,
                    OrderNumber = model.OrderNumber,
                };
                _saleBillRepo.Add(saleBillDb);
                await _saleBillRepo.SaveAllAsync();

                var today = DateTime.Today;
                //All Holes
                var dagagHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
                         .Select(h => new DagagHoleDataVM
                         {
                             Id = h.Id,
                             Name = h.Name,
                             HoleType = h.HoleType,
                             EndTime = _chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result
                                           .LastOrDefault()?.EndTime,
                             Amount = _chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result
                                           .Sum(c => c.AmountIn - c.AmountOut)
                         })
                        .Where(d => d.EndTime.HasValue
                                     && !string.IsNullOrWhiteSpace(model.OrderDeliveredTime)
                                     && d.EndTime.Value < TimeOnly.Parse(model.OrderDeliveredTime))

                         .ToList();


                var meatHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                   .Select(h => new MeatHoleDataVM
                   {
                       Id = h.Id,
                       Name = h.Name,
                       HoleType = h.HoleType,
                       EndTime = _meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result
                                      .LastOrDefault()?.EndTime,
                       NafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result
                                      .Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                       HalfNafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result
                                      .Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
                   })
                   .Where(d => d.EndTime.HasValue
                                 && !string.IsNullOrWhiteSpace(model.OrderDeliveredTime)
                                 && d.EndTime.Value < TimeOnly.Parse(model.OrderDeliveredTime))

                   .ToList();

                //BillDetail
                foreach (var saleDetail in model.BillDetailRegisterVM)
                {
                    var saleDetailDb = _mapper.Map<SaleBillDetail>(saleDetail);
                    saleDetailDb.SaleBillId = saleBillDb.Id;
                    saleDetailDb.CreatedDate = DateTime.Now;
                    saleDetailDb.LastEditDate = DateTime.Now;
                    saleDetailDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    saleDetailDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _saleBillDetailRepo.Add(saleDetailDb);
                    await _saleBillDetailRepo.SaveAllAsync();

                    //Handle Holes
                    await handleDagagByTime(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, dagagHoleDataVM);
                    await HandleMeatNafrByTime(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, meatHoleDataVM);
                    await HandleMeatHalfNafrByTime(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id, meatHoleDataVM);
                }

                //For Print AllBill
                var billHallPrintVM = new BillHallPrintVM
                {
                    Billnumber = saleBillDb.Id.ToString(),
                    Date = saleBillDb.Date,
                    BillDetailRegisterVM = model.BillDetailRegisterVM,
                    TotalPrice = model.FinalTotal,
                    Discount = model.Discount,
                    Vat = model.Vat,
                    DeliveryPrice = model.DeliveryPrice,
                    Notes = model.Notes,
                    CashierName = _userRepo.GetByIdAsync(saleBillDb.CreatedUser).Result.Name
                };
                if (model.CustomerAddress != null)
                {
                    billHallPrintVM.CustomerAddress = model.CustomerAddress;
                }
                if (model.CustomerId != null)
                {
                    billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Phone;
                    billHallPrintVM.CustomerName = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Name;
                }
                if (model.DeliveryId != 0)
                {
                    billHallPrintVM.DeliveryName = _deliveryRepo.GetByIdAsync((int)model.DeliveryId).Result.Name;
                }
                try
                {
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user == null)
                        throw new Exception("User not found.");
                    // Get the printer names from configuration
                    var print = await _printerRegistration.SingleOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
                    if (print == null)
                        throw new Exception("Printer not registered for user.");
                    //var printerName = _configuration["CashierPrinterName"];
                    var printerName = print.Name;
                    var printerName2 = _configuration["DeliveryPrinterName"];
                    //await PrintPdfAsync(filePathBill, printerName);
                    await ApiHelper.SendToApi(filePathBill, printerName, print.IpAddress);
                    await PrintPdfAsync(filePathBill, printerName2);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "TRANSACTION_ERROR", message = "Transaction failed: " + ex.Message });
            }
        }

        //Check HolesAmount By Holes
        [HttpPost("CheckDeliveryHolesAmount")]
        public async Task<IActionResult> CheckDeliveryHolesAmount(CheckDeliveryHoleAmountVM model)
        {
            //All Selected Holes
            var holes = new List<Hole>();
            foreach (var holeId in model.HoleIds)
            {
                var hole = await _holeRepo.GetByIdAsync(Convert.ToInt32(holeId));
                holes.Add(hole);
            }
            var dagagHoleIds = holes.Where(h => h.HoleType == HoleType.دجاج).Select(h => h.Id).ToList();
            var meatHoleIds = holes.Where(h => h.HoleType == HoleType.لحم).Select(h => h.Id).ToList();
            double allDagagAmount = 0.0;
            int allNafrAmount = 0;
            int allHalfNafrAmount = 0;
            foreach (var item in model.BillDetailRegisterVM)
            {
                var productById = await _productRepo.GetByIdAsync(item.ProductId);
                if (productById.Dagag != null || productById.HalfDagag != null)
                {
                    double dagag = productById.Dagag ?? 0.0;
                    double halfDagag = productById.HalfDagag ?? 0.0;
                    var dagagValue = (dagag + halfDagag * 0.5) * item.Amount;
                    allDagagAmount = allDagagAmount + dagagValue;
                }
                if (productById.Nafr != null)
                {
                    var nafrValue = (int)productById.Nafr * item.Amount;
                    allNafrAmount = allNafrAmount + nafrValue;
                }
                if (productById.HalfNafr != null)
                {
                    var halfNafrValue = (int)productById.HalfNafr * item.Amount;
                    allHalfNafrAmount = allHalfNafrAmount + halfNafrValue;
                }
            }
            var today = DateTime.Today;
            if (allDagagAmount > 0)
            {
                var realDagagAmount = _chickenHoleMovementRepo.GetAllAsync(c => dagagHoleIds.Contains(c.HoleId)).Result
                    .Where(c => c.Date.Date == today.Date).Sum(c => c.AmountIn - c.AmountOut);
                if (allDagagAmount > realDagagAmount || realDagagAmount == 0)
                    return BadRequest();
            }
            if (allNafrAmount > 0 || allHalfNafrAmount > 0)
            {
                var realNafrAmount = _meatHoleMovementRepo.GetAllAsync(c => meatHoleIds.Contains(c.HoleId)).Result
                    .Where(c => c.Date.Date == today.Date).Sum(c => c.NafrAmountIn - c.NafrAmountOut);
                var realHalfNafrAmount = _meatHoleMovementRepo.GetAllAsync(c => meatHoleIds.Contains(c.HoleId)).Result
                    .Where(c => c.Date.Date == today.Date).Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut);
                if (allNafrAmount > realNafrAmount || allHalfNafrAmount > realHalfNafrAmount || realNafrAmount == 0 || realHalfNafrAmount == 0)
                    return BadRequest();
            }
            return Ok();
        }

        //Check HolesAmount By Time
        [HttpPost("CheckDeliveryHolesAmountByTime")]
        public async Task<IActionResult> CheckDeliveryHolesAmountByTime(CheckDeliveryHoleAmountVM model)
        {
            var today = DateTime.Today;
            //All Holes
            var dagagHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
             .Select(h => new DagagHoleDataVM
             {
                 Id = h.Id,
                 Name = h.Name,
                 HoleType = h.HoleType,
                 EndTime = _chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                 Amount = _chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut)
             }).Where(d => d.EndTime < TimeOnly.Parse(model.OrderDeliveredTime)).ToList();

            var meatHoleDataVM = _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                .Select(h => new MeatHoleDataVM
                {
                    Id = h.Id,
                    Name = h.Name,
                    HoleType = h.HoleType,
                    EndTime = _meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                    NafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                    HalfNafrAmount = (int)_meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
                }).Where(d => d.EndTime < TimeOnly.Parse(model.OrderDeliveredTime)).ToList();

            double allDagagAmount = 0.0;
            int allNafrAmount = 0;
            int allHalfNafrAmount = 0;
            foreach (var item in model.BillDetailRegisterVM)
            {
                var productById = await _productRepo.GetByIdAsync(item.ProductId);
                if (productById.Dagag != null || productById.HalfDagag != null)
                {
                    double dagag = productById.Dagag ?? 0.0;
                    double halfDagag = productById.HalfDagag ?? 0.0;
                    var dagagValue = (dagag + halfDagag * 0.5) * item.Amount;
                    allDagagAmount = allDagagAmount + dagagValue;
                }
                if (productById.Nafr != null)
                {
                    var nafrValue = (int)productById.Nafr * item.Amount;
                    allNafrAmount = allNafrAmount + nafrValue;
                }
                if (productById.HalfNafr != null)
                {
                    var halfNafrValue = (int)productById.HalfNafr * item.Amount;
                    allHalfNafrAmount = allHalfNafrAmount + halfNafrValue;
                }
            }

            if (allDagagAmount > 0)
            {
                var realDagagAmount = dagagHoleDataVM.Sum(d => d.Amount);
                if (allDagagAmount > realDagagAmount)
                    return BadRequest();
            }
            if (allNafrAmount > 0 || allHalfNafrAmount > 0)
            {
                var realNafrAmount = meatHoleDataVM.Sum(m => m.NafrAmount);
                var realHalfNafrAmount = meatHoleDataVM.Sum(m => m.HalfNafrAmount);
                if (allNafrAmount > realNafrAmount || allHalfNafrAmount > realHalfNafrAmount)
                    return BadRequest();
            }
            return Ok();
        }

        //For SaleBill By Holes
        private async Task handleDagag(int productId, int detailAmount, int saleBillId, List<Hole> holes)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.Dagag != null || productById.HalfDagag != null)
            {
                // Default to 0 if the value is null
                double dagag = productById.Dagag ?? 0.0;
                double halfDagag = productById.HalfDagag ?? 0.0;
                var dagagValue = (dagag + halfDagag * 0.5) * detailAmount;

                foreach (var dagagHole in holes.Where(h => h.HoleType == HoleType.دجاج))
                {
                    var amount = _chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == dagagHole.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut);

                    if (amount >= dagagValue)
                    {
                        //ChickenHoleMovement
                        var chickenHoleMovement = new ChickenHoleMovement
                        {
                            HoleId = dagagHole.Id,
                            Date = DateTime.Now,
                            AmountIn = 0,
                            AmountOut = dagagValue,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _chickenHoleMovementRepo.Add(chickenHoleMovement);
                        await _chickenHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //ChickenHoleMovement
                        var chickenHoleMovement = new ChickenHoleMovement
                        {
                            HoleId = dagagHole.Id,
                            Date = DateTime.Now,
                            AmountIn = 0,
                            AmountOut = amount,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _chickenHoleMovementRepo.Add(chickenHoleMovement);
                        await _chickenHoleMovementRepo.SaveAllAsync();
                        dagagValue = dagagValue - amount;
                    }
                }
            }
        }

        private async Task HandleMeatNafr(int productId, int detailAmount, int saleBillId, List<Hole> holes)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.Nafr != null)
            {
                var nafrValue = (double)productById.Nafr * detailAmount;
                foreach (var meatHole in holes.Where(h => h.HoleType == HoleType.لحم))
                {
                    var amount = _meatHoleMovementRepo.GetAllAsync(c => c.HoleId == meatHole.Id && c.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut);
                    if (amount >= nafrValue)
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = nafrValue,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = 0,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = amount,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = 0,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        nafrValue = nafrValue - amount;
                    }
                }
            }
        }

        private async Task HandleMeatHalfNafr(int productId, int detailAmount, int saleBillId, List<Hole> holes)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.HalfNafr != null)
            {
                var halfNafrValue = (double)productById.HalfNafr * detailAmount;
                foreach (var meatHole in holes.Where(h => h.HoleType == HoleType.لحم))
                {
                    var amount = _meatHoleMovementRepo.GetAllAsync(c => c.HoleId == meatHole.Id && c.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut);
                    if (amount >= halfNafrValue)
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = 0,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = halfNafrValue,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = 0,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = amount,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        halfNafrValue = halfNafrValue - amount;
                    }
                }
            }
        }


        //For SaleBill By Time
        private async Task handleDagagByTime(int productId, int detailAmount, int saleBillId, List<DagagHoleDataVM> dagagHoles)
        {
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.Dagag != null || productById.HalfDagag != null)
            {
                // Default to 0 if the value is null
                double dagag = productById.Dagag ?? 0.0;
                double halfDagag = productById.HalfDagag ?? 0.0;
                var dagagValue = (dagag + halfDagag * 0.5) * detailAmount;

                foreach (var dagagHole in dagagHoles.OrderByDescending(d => d.EndTime))
                {
                    if (dagagHole.Amount >= dagagValue)
                    {
                        //ChickenHoleMovement
                        var chickenHoleMovement = new ChickenHoleMovement
                        {
                            HoleId = dagagHole.Id,
                            Date = DateTime.Now,
                            AmountIn = 0,
                            AmountOut = dagagValue,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _chickenHoleMovementRepo.Add(chickenHoleMovement);
                        await _chickenHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //ChickenHoleMovement
                        var chickenHoleMovement = new ChickenHoleMovement
                        {
                            HoleId = dagagHole.Id,
                            Date = DateTime.Now,
                            AmountIn = 0,
                            AmountOut = dagagHole.Amount,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _chickenHoleMovementRepo.Add(chickenHoleMovement);
                        await _chickenHoleMovementRepo.SaveAllAsync();
                        dagagValue = dagagValue - dagagHole.Amount;
                    }
                }
            }
        }

        private async Task HandleMeatNafrByTime(int productId, int detailAmount, int saleBillId, List<MeatHoleDataVM> meatHoles)
        {
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.Nafr != null)
            {
                var nafrValue = (double)productById.Nafr * detailAmount;
                foreach (var meatHole in meatHoles.OrderByDescending(m => m.EndTime))
                {
                    if (meatHole.NafrAmount >= nafrValue)
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = nafrValue,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = 0,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = meatHole.NafrAmount,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = 0,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        nafrValue = nafrValue - meatHole.NafrAmount;
                    }
                }
            }
        }

        private async Task HandleMeatHalfNafrByTime(int productId, int detailAmount, int saleBillId, List<MeatHoleDataVM> meatHoles)
        {
            var productById = await _productRepo.GetByIdAsync(productId);
            if (productById.HalfNafr != null)
            {
                var halfNafrValue = (double)productById.HalfNafr * detailAmount;
                foreach (var meatHole in meatHoles.OrderByDescending(m => m.EndTime))
                {
                    if (meatHole.HalfNafrAmount >= halfNafrValue)
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = 0,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = halfNafrValue,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        break;
                    }
                    else
                    {
                        //MeatHoleMovement
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = meatHole.Id,
                            Date = DateTime.Now,
                            NafrAmountIn = 0,
                            NafrAmountOut = 0,
                            HalfNafrAmountIn = 0,
                            HalfNafrAmountOut = meatHole.HalfNafrAmount,
                            HoleMovementType = HoleMovementType.Sale,
                            HoleMovementTypeId = saleBillId,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                        await _meatHoleMovementRepo.SaveAllAsync();
                        halfNafrValue = halfNafrValue - meatHole.HalfNafrAmount;
                    }
                }
            }
        }


        [HttpPost("UpdateSaleDelivery")]
        public async Task<IActionResult> UpdateSaleDelivery([FromBody] SaleBillRefundVM model)
        {
            try
            {
                var saleBillById = await _saleBillRepo.GetByIdAsync(model.Id);
                if (saleBillById == null)
                    return NotFound();
                saleBillById.Total = model.BillDetailRegisterVM.Sum(b => b.TotalPrice);
                saleBillById.LastEditDate = DateTime.Now;
                saleBillById.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                saleBillById.FinalTotal = Math.Round((model.BillDetailRegisterVM.Sum(b => b.TotalPrice) * (1 + saleBillById.Vat * 0.01) - saleBillById.Discount) + saleBillById.DeliveryPrice, 1);
                _saleBillRepo.Update(saleBillById);
                await _saleBillRepo.SaveAllAsync();

                //SaleBillDetail
                var oldSaleDetail = await _saleBillDetailRepo.GetAllAsync(s => s.SaleBillId == model.Id);
                if (oldSaleDetail != null)
                {
                    _saleBillDetailRepo.DeletelistRange(oldSaleDetail.ToList());
                }
                foreach (var saleDetail in model.BillDetailRegisterVM)
                {
                    var saleDetailDb = _mapper.Map<SaleBillDetail>(saleDetail);
                    saleDetailDb.SaleBillId = model.Id;
                    saleDetailDb.CreatedDate = DateTime.Now;
                    saleDetailDb.LastEditDate = DateTime.Now;
                    saleDetailDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    saleDetailDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _saleBillDetailRepo.Add(saleDetailDb);
                    await _saleBillDetailRepo.SaveAllAsync();
                }

                //For Print AllBill
                var billHallPrintVM = new BillHallPrintVM
                {
                    Billnumber = saleBillById.Id.ToString(),
                    Date = saleBillById.Date,
                    BillDetailRegisterVM = model.BillDetailRegisterVM,
                    TotalPrice = saleBillById.FinalTotal,
                    Discount = saleBillById.Discount,
                    Vat = saleBillById.Vat,
                    Notes = saleBillById.Notes
                };
                if (saleBillById.CustomerId != null)
                {
                    billHallPrintVM.CustomerAddress = _customerRepo.GetByIdAsync((int)saleBillById.CustomerId).Result.Address;
                }
                try
                {
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user == null)
                        throw new Exception("User not found.");
                    // Get the printer names from configuration
                    var print = await _printerRegistration.SingleOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
                    if (print == null)
                        throw new Exception("Printer not registered for user.");
                    //var printerName = _configuration["CashierPrinterName"];
                    var printerName = print.Name;
                    //await PrintPdfAsync(filePathBill, printerName);
                    await ApiHelper.SendToApi(filePathBill, printerName, print.IpAddress);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "TRANSACTION_ERROR", message = "Transaction failed: " + ex.Message });
            }
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

            // حساب المحتوى الإضافي (الخصم، التوصيل، الملاحظات)
            float extraContentHeight = 0f;
            if (model.Discount != 0) extraContentHeight += 20f;
            if (model.DeliveryPrice != 0) extraContentHeight += 20f;
            if (!string.IsNullOrEmpty(model.Notes)) extraContentHeight += 20f;
            //extraContentHeight += 1f; // للسعر الكلي والفواصل

            float pageHeight = headerHeight + (rowCount * rowHeight) + extraContentHeight + footerHeight;
            Document document = new Document(new Rectangle(pageWidth, pageHeight), 1, 1, 0, 0); // Margins (left, right, top, bottom)

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
                Font arabicFont = new Font(bfArialUnicode, 12, Font.BOLD, BaseColor.BLACK);   // كان 14
                Font arabicFont22 = new Font(bfArialUnicode, 8, Font.BOLD, BaseColor.BLACK); // كان 10

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
                if (!string.IsNullOrEmpty(model.DeliveryName))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" شركة التوصيل : {model.DeliveryName}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                if (!string.IsNullOrEmpty(model.CustomerName))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase($" اسم العميل : {model.CustomerName}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
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
                if (model.DeliveryPrice != 0)
                {
                    document.Add(new Paragraph(al.Process($" التوصيل : {model.DeliveryPrice}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
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
