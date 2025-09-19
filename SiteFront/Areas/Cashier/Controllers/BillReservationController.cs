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
using System.Diagnostics;

namespace SiteFront.Areas.Cashier.Controllers
{
    [Area("Cashier")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class BillReservationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<DeliveryBill> _deliveryBillRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<MeatFilling> _meatFillingRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<User> _userRepo;

        public BillReservationController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<DeliveryBill> deliveryBillRepo,
            IRepository<Customer> customerRepo,
            IRepository<Product> productRepo,
            IRepository<Hole> holeRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<MeatFilling> meatFillingRepo,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            UserManager<User> userManager,
            IRepository<User> userRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _deliveryBillRepo = deliveryBillRepo;
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _holeRepo = holeRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
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
                    CustomerId = model.CustomerId,
                    BillType = BillType.Reservation,
                    Date = DateTime.Now,
                    Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
                    Discount = model.Discount,
                    Vat = model.Vat,
                    FinalTotal = model.FinalTotal,
                    OrderDeliveredTime = model.OrderDeliveredTime != "" ? TimeOnly.Parse(model.OrderDeliveredTime) : null,
                    Notes = model.Notes,
                    MoneyDelivered = false,
                    CustomerAddress = model.CustomerAddress,
                    OrderNumber = model.OrderNumber,
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

                ////For Print AllBill
                //var billHallPrintVM = new BillHallPrintVM
                //{
                //    Billnumber = saleBillDb.Id.ToString(),
                //    Date = saleBillDb.Date,
                //    BillDetailRegisterVM = model.BillDetailRegisterVM,
                //    TotalPrice = model.FinalTotal,
                //    Discount = model.Discount,
                //    Vat = model.Vat,
                //    Notes = model.Notes,
                //    OrderDeliveredTime = model.OrderDeliveredTime != "" ? TimeOnly.Parse(model.OrderDeliveredTime) : null,
                //    CashierName = _userRepo.GetByIdAsync(saleBillDb.CreatedUser).Result.Name
                //};
                //if (model.CustomerAddress != null)
                //{
                //    billHallPrintVM.CustomerAddress = model.CustomerAddress;
                //}
                //if (model.CustomerId != null)
                //{
                //    billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Phone;
                //}
                //try
                //{
                //    var filePathBill = GenerateReceipt(billHallPrintVM);
                //    var printerName = _configuration["CashierPrinterName"];
                //    var printerName2 = _configuration["DeliveryPrinterName"];
                //    await PrintPdfAsync(filePathBill, printerName);
                //    await PrintPdfAsync(filePathBill, printerName2);
                //}
                //catch (Exception ex)
                //{
                //    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                //}
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
                    CustomerId = model.CustomerId,
                    BillType = BillType.Reservation,
                    Date = DateTime.Now,
                    Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
                    Discount = model.Discount,
                    Vat = model.Vat,
                    FinalTotal = model.FinalTotal,
                    OrderDeliveredTime = TimeOnly.Parse(model.OrderDeliveredTime),
                    Notes = model.Notes,
                    MoneyDelivered = false,
                    CustomerAddress = model.CustomerAddress,
                    OrderNumber = model.OrderNumber
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

                ////For Print AllBill
                //var billHallPrintVM = new BillHallPrintVM
                //{
                //    Billnumber = saleBillDb.Id.ToString(),
                //    Date = saleBillDb.Date,
                //    BillDetailRegisterVM = model.BillDetailRegisterVM,
                //    TotalPrice = model.FinalTotal,
                //    Discount = model.Discount,
                //    Vat = model.Vat,
                //    Notes = model.Notes,
                //    OrderDeliveredTime = model.OrderDeliveredTime != "" ? TimeOnly.Parse(model.OrderDeliveredTime) : null,
                //    CashierName = _userRepo.GetByIdAsync(saleBillDb.CreatedUser).Result.Name
                //};
                //if (model.CustomerAddress != null)
                //{
                //    billHallPrintVM.CustomerAddress = model.CustomerAddress;
                //}
                //if (model.CustomerId != null)
                //{
                //    billHallPrintVM.CustomerPhone = _customerRepo.GetByIdAsync((int)model.CustomerId).Result.Phone;
                //}
                //try
                //{
                //    var filePathBill = GenerateReceipt(billHallPrintVM);
                //    var printerName = _configuration["CashierPrinterName"];
                //    var printerName2 = _configuration["DeliveryPrinterName"];
                //    await PrintPdfAsync(filePathBill, printerName);
                //    await PrintPdfAsync(filePathBill, printerName2);
                //}
                //catch (Exception ex)
                //{
                //    return BadRequest(new { error = "PRINT_ERROR", message = "Printing failed: " + ex.Message });
                //}
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "TRANSACTION_ERROR", message = "Transaction failed: " + ex.Message });
            }
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
    }
}
