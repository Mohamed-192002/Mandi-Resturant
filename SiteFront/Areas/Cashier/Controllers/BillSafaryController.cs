using AutoMapper;
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.SaleBillPrintVM;
using Core.ViewModels.SaleBillVM;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SiteFront.Areas.Cashier.Controllers
{
    [Area("Cashier")]
    [Route("[area]/[controller]")]
    [ApiController]
    public class BillSafaryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<User> _userRepo;

        public BillSafaryController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Product> productRepo,
            IRepository<Hole> holeRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            UserManager<User> userManager,
            IRepository<User> userRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _productRepo = productRepo;
            _holeRepo = holeRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
            _meatHoleMovementRepo = meatHoleMovementRepo;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _userManager = userManager;
            _userRepo = userRepo;
        }

        [HttpPost("SaveSaleSafary")]
        public async Task<IActionResult> SaveSaleSafary([FromBody] BillSafaryRegisterVM model)
        {
            try
            {
                var saleBillDb = new SaleBill
                {
                    CreatedDate = DateTime.Now,
                    LastEditDate = DateTime.Now,
                    CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                    BillType = BillType.Safary,
                    Date = DateTime.Now,
                    Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
                    Discount = model.Discount,
                    Vat = model.Vat,
                    FinalTotal = model.FinalTotal,
                    Notes = model.Notes,
                    Gift = model.Gift,
                    CustomerId = model.CustomerId,
                    CustomerAddress = model.CustomerAddress
                };
                _saleBillRepo.Add(saleBillDb);
                await _saleBillRepo.SaveAllAsync();
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
                    await handleDagag(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id);
                    await HandleMeatNafr(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id);
                    await HandleMeatHalfNafr(saleDetail.ProductId, saleDetail.Amount, saleBillDb.Id);
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
                    Notes = model.Notes,
                    CashierName = _userRepo.GetByIdAsync(saleBillDb.CreatedUser).Result.Name
                };
                try
                {
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var printerName = _configuration["CashierPrinterName"];
                    var printerName2 = _configuration["SafaryPrinterName"];
                    await PrintPdfAsync(filePathBill, printerName);
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

        [HttpPost("CheckHolesAmount")]
        public async Task<IActionResult> CheckHolesAmount(List<BillDetailRegisterVM> BillDetailRegisterVM)
        {
            var today = DateTime.Today;
            double allDagagAmount = 0.0;
            int allNafrAmount = 0;
            int allHalfNafrAmount = 0;
            foreach (var item in BillDetailRegisterVM)
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
            var realDagagAmount = _chickenHoleMovementRepo.GetAllAsync(c=>c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut);
            var realNafrAmount = _meatHoleMovementRepo.GetAllAsync(c => c.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut);
            var realHalfNafrAmount = _meatHoleMovementRepo.GetAllAsync(c => c.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut);
            if (allDagagAmount > realDagagAmount || allNafrAmount > realNafrAmount || allHalfNafrAmount > realHalfNafrAmount)
                return BadRequest();
            return Ok();
        }

        private async Task handleDagag(int productId, int detailAmount, int saleBillId)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            var dagagHoles = await _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true);
            if (productById.Dagag != null || productById.HalfDagag != null)
            {
                // Default to 0 if the value is null
                double dagag = productById.Dagag ?? 0.0;
                double halfDagag = productById.HalfDagag ?? 0.0;
                var dagagValue = (dagag + halfDagag * 0.5) * detailAmount;

                foreach (var dagagHole in dagagHoles)
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

        private async Task HandleMeatNafr(int productId, int detailAmount, int saleBillId)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            var meatHoles = await _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true);
            if (productById.Nafr != null)
            {
                var nafrValue = (double)productById.Nafr * detailAmount;
                foreach (var meatHole in meatHoles)
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

        private async Task HandleMeatHalfNafr(int productId, int detailAmount, int saleBillId)
        {
            var today = DateTime.Today;
            var productById = await _productRepo.GetByIdAsync(productId);
            var meatHoles = await _holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true);
            if (productById.HalfNafr != null)
            {
                var halfNafrValue = (double)productById.HalfNafr * detailAmount;
                foreach (var meatHole in meatHoles)
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


        [HttpPost("UpdateSaleSafary")]
        public async Task<IActionResult> UpdateSaleSafary([FromBody] SaleBillRefundVM model)
        {
            try
            {
                var saleBillById = await _saleBillRepo.GetByIdAsync(model.Id);
                if (saleBillById == null)
                    return NotFound();
                saleBillById.Total = model.BillDetailRegisterVM.Sum(b => b.TotalPrice);
                saleBillById.LastEditDate = DateTime.Now;
                saleBillById.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                saleBillById.FinalTotal = Math.Round(model.BillDetailRegisterVM.Sum(b => b.TotalPrice) * (1 + saleBillById.Vat * 0.01) - saleBillById.Discount, 1);
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
                    Notes = saleBillById.Notes,
                    CashierName = _userRepo.GetByIdAsync(saleBillById.CreatedUser).Result.Name
                };
                try
                {
                    var filePathBill = GenerateReceipt(billHallPrintVM);
                    var printerName = _configuration["CashierPrinterName"];
                    await PrintPdfAsync(filePathBill, printerName);
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
            float pageHeight = 297f * 2.8346f;
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
                Font arabicFont = new Font(bfArialUnicode, 17, Font.BOLD, BaseColor.BLACK);
                Font arabicFont22 = new Font(bfArialUnicode, 10, Font.BOLD, BaseColor.BLACK);

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
                if(model.Discount != 0)
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

        [HttpGet("PrintDaySales")]
        public async Task<IActionResult> PrintDaySales()
        {
            var currentUserId = _userManager.GetUserAsync(HttpContext.User).Result.Id;
            if (currentUserId != Guid.Empty)
            {
                var daySales = await _saleBillRepo.GetAllAsync(s => s.CreatedUser == currentUserId && s.Date.Date == DateTime.Now.Date);
                var allSaleBillDetails = new List<SaleBillDetail>();
                foreach (var saleBill in daySales)
                {
                    var saleBillDetail = await _saleBillDetailRepo.GetAllAsync(s => s.SaleBillId == saleBill.Id);
                    allSaleBillDetails.AddRange(saleBillDetail);
                }
                var billDetailRegisterVM = allSaleBillDetails.GroupBy(d => d.ProductId).Select(d => new BillDetailRegisterVM
                {
                    ProductId = d.Key,
                    PName = _productRepo.GetByIdAsync(d.Key).Result.Name,
                    Amount = d.Sum(d => d.Amount),
                    Price = d.Average(d => d.Price),
                    TotalPrice = d.Sum(d => d.TotalPrice)
                }).ToList();
                var billHallPrintVM = new BillHallPrintVM
                {
                    Date = DateTime.Now,
                    BillDetailRegisterVM = billDetailRegisterVM,
                    TotalPrice = billDetailRegisterVM.Sum(b => b.TotalPrice),
                };
                var filePathBill = GenerateAllDayReceipt(billHallPrintVM);
                var printerName = _configuration["CashierPrinterName"];
                await PrintPdfAsync(filePathBill, printerName);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        private string GenerateAllDayReceipt(BillHallPrintVM model)
        {
            // Define the custom page size (80 mm wide)
            float pageWidth = 80f * 2.8346f; // 80mm to points (1mm = 2.8346 points)
            float pageHeight = 297f * 2.8346f;
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
                Font arabicFont = new Font(bfArialUnicode, 14, Font.BOLD, BaseColor.BLACK);
                Font arabicFont22 = new Font(bfArialUnicode, 10, Font.BOLD, BaseColor.BLACK);

                var al = new ArabicLigaturizer();
                float cellHeight = 30f;
                // Create a table with 2 columns to display two items per row
                PdfPTable infoTable = new PdfPTable(1);
                infoTable.WidthPercentage = 100;
                infoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // Set Right-to-Left direction

                // Add receipt details (Branch and Customer Info) in two columns
                infoTable.AddCell(new PdfPCell(new Phrase($" التاريخ : {model.Date}", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                document.Add(infoTable); // Add the info table to the document

                PdfPTable infoTable2 = new PdfPTable(1);
                infoTable2.WidthPercentage = 100;
                infoTable2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                infoTable2.AddCell(new PdfPCell(new Phrase("الاصناف", arabicFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, FixedHeight = cellHeight });

                document.Add(infoTable2);
                //document.Add(new Paragraph(al.Process("الاصناف"), arabicFont) { Alignment = Element.ALIGN_CENTER });

                // Create Table for Items (Arabic headers)
                PdfPTable table = new PdfPTable(4); // 3 columns: Item, Quantity, Price
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // Set Right-to-Left direction for the table
                table.AddCell(new PdfPCell(new Phrase("الصنف", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("العدد", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(" سعر الوحدة", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(" السعر الكلي", arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                // Add data to table
                foreach (var item in model.BillDetailRegisterVM)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.PName.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(item.Amount.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(item.Price.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(item.TotalPrice.ToString(), arabicFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(table);
                document.Add(new Paragraph(al.Process($"المجموع النهائي  : {model.TotalPrice}"), arabicFont) { Alignment = Element.ALIGN_CENTER });
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

        //[HttpPost("SaveTempSafary")]
        //public async Task<IActionResult> SaveTempSafary([FromBody] BillSafaryRegisterVM model)
        //{
        //    try
        //    {
        //        var saleBillDb = new SaleBill
        //        {
        //            CreatedDate = DateTime.Now,
        //            LastEditDate = DateTime.Now,
        //            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
        //            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
        //            BillType = BillType.Safary,
        //            Date = DateTime.Now,
        //            Total = model.BillDetailRegisterVM.Sum(s => s.TotalPrice),
        //            Discount = model.Discount,
        //            Vat = model.Vat,
        //            FinalTotal = model.FinalTotal,
        //            Notes = model.Notes,
        //            Temporary = true
        //        };
        //        _saleBillRepo.Add(saleBillDb);
        //        await _saleBillRepo.SaveAllAsync();
        //        foreach (var saleDetail in model.BillDetailRegisterVM)
        //        {
        //            var saleDetailDb = _mapper.Map<SaleBillDetail>(saleDetail);
        //            saleDetailDb.SaleBillId = saleBillDb.Id;
        //            saleDetailDb.CreatedDate = DateTime.Now;
        //            saleDetailDb.LastEditDate = DateTime.Now;
        //            saleDetailDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
        //            saleDetailDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
        //            _saleBillDetailRepo.Add(saleDetailDb);
        //            await _saleBillDetailRepo.SaveAllAsync();
        //        }
        //        return Ok();
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpPost("EditTempSafary")]
        //public async Task<IActionResult> EditTempSafary([FromBody] BillSafaryRegisterVM model)
        //{
        //    try
        //    {
        //        var saleBillById = await _saleBillRepo.GetByIdAsync((int)model.Id);
        //        if (saleBillById == null)
        //            return NotFound();
        //        saleBillById.Total = model.BillDetailRegisterVM.Sum(b => b.TotalPrice);
        //        saleBillById.LastEditDate = DateTime.Now;
        //        saleBillById.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
        //        saleBillById.Discount = model.Discount;
        //        saleBillById.Vat = model.Vat;
        //        saleBillById.FinalTotal = model.FinalTotal;
        //        saleBillById.Notes = model.Notes;
        //        saleBillById.Temporary = false;
        //        _saleBillRepo.Update(saleBillById);
        //        await _saleBillRepo.SaveAllAsync();

        //        //SaleBillDetail
        //        var oldSaleDetail = await _saleBillDetailRepo.GetAllAsync(s => s.SaleBillId == saleBillById.Id);
        //        if (oldSaleDetail != null)
        //        {
        //            _saleBillDetailRepo.DeletelistRange(oldSaleDetail.ToList());
        //        }
        //        foreach (var saleDetail in model.BillDetailRegisterVM)
        //        {
        //            var saleDetailDb = _mapper.Map<SaleBillDetail>(saleDetail);
        //            saleDetailDb.SaleBillId = saleBillById.Id;
        //            saleDetailDb.CreatedDate = DateTime.Now;
        //            saleDetailDb.LastEditDate = DateTime.Now;
        //            saleDetailDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
        //            saleDetailDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
        //            _saleBillDetailRepo.Add(saleDetailDb);
        //            await _saleBillDetailRepo.SaveAllAsync();
        //        }

        //        var DetailGroupByCatId = model.BillDetailRegisterVM.GroupBy(s => s.CategoryId);

        //        //For Print CategoryBill
        //        foreach (var item in DetailGroupByCatId)
        //        {
        //            var CategoryId = item.Key;
        //            var category = await _categoryRepo.GetByIdAsync(CategoryId);
        //            var categoryBillPrintVM = new CategoryBillPrintVM
        //            {
        //                CategoryName = category.Name,
        //                Billnumber = saleBillById.Id.ToString(),
        //                Date = saleBillById.Date,
        //                Notes = saleBillById.Notes,
        //                BillDetailRegisterVM = item.ToList()
        //            };
        //            var filePath = GenerateCategoryReceipt(categoryBillPrintVM);
        //            await PrintPdfAsync(filePath, category.PrinterName);
        //        }

        //        //For Print AllBill
        //        var billHallPrintVM = new BillHallPrintVM
        //        {
        //            Billnumber = saleBillById.Id.ToString(),
        //            Date = saleBillById.Date,
        //            BillDetailRegisterVM = model.BillDetailRegisterVM,
        //            TotalPrice = saleBillById.FinalTotal,
        //            Discount = saleBillById.Discount,
        //            Vat = saleBillById.Vat,
        //            Notes = saleBillById.Notes
        //        };
        //        var filePathBill = GenerateReceipt(billHallPrintVM);
        //        var printerName = _configuration["CashierPrinterName"];
        //        await PrintPdfAsync(filePathBill, printerName);
        //        return Ok();
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}

    }
}
