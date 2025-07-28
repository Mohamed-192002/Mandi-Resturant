
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.HoleVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SiteFront.Hubs;

namespace SiteFront.Services
{
    public class EndTimeCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EndTimeCheckerService> _logger;
        private readonly IHubContext<DataHub> _hubContext;

        public EndTimeCheckerService(IServiceScopeFactory scopeFactory,
            ILogger<EndTimeCheckerService> logger,
            IHubContext<DataHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope()) // Create scope
                    {
                        var holeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Hole>>();
                        var chickenFillingRepo = scope.ServiceProvider.GetRequiredService<IRepository<ChickenFilling>>();
                        var chickenHoleMovementRepo = scope.ServiceProvider.GetRequiredService<IRepository<ChickenHoleMovement>>();
                        var meatFillingRepo = scope.ServiceProvider.GetRequiredService<IRepository<MeatFilling>>();
                        var meatHoleMovementRepo = scope.ServiceProvider.GetRequiredService<IRepository<MeatHoleMovement>>();
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                        DateTime now = DateTime.Now;
                        TimeOnly nowTime = TimeOnly.FromDateTime(now);
                        DateTime today = now.Date;

                        // Get all holes with their EndTime and Amount
                        var dagagHoleDataVM = (await holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true))
                            .Select(h => new
                            {
                                Id = h.Id,
                                Name = h.Name,
                                EndTime = chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today).Result
                                               .OrderByDescending(f => f.EndTime)
                                               .FirstOrDefault()?.EndTime,
                                Amount = chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today).Result
                                               .Sum(c => c.AmountIn - c.AmountOut)
                            })
                            .Where(h => h.EndTime.HasValue)
                            .OrderBy(h => h.EndTime)
                            .ToList();

                        foreach (var hole in dagagHoleDataVM)
                        {
                            if (hole.EndTime <= nowTime)
                            {
                                var previousHole = dagagHoleDataVM
                                    .Where(h => h.EndTime < hole.EndTime)
                                    .OrderByDescending(h => h.EndTime)
                                    .FirstOrDefault();

                                if (previousHole != null && previousHole.Amount > 0)
                                {
                                    // Deduct from previous hole
                                    var deduction = new ChickenHoleMovement
                                    {
                                        HoleId = previousHole.Id,
                                        Date = now,
                                        AmountIn = 0,
                                        AmountOut = previousHole.Amount,
                                        HoleMovementType = HoleMovementType.Transfer,
                                        HoleMovementTypeId = 0,
                                        CreatedDate = now,
                                        LastEditDate = now,
                                        CreatedUser = Guid.NewGuid(),
                                        LastEditUser = Guid.NewGuid()
                                    };
                                    chickenHoleMovementRepo.Add(deduction);
                                    // Add to the current hole
                                    var addition = new ChickenHoleMovement
                                    {
                                        HoleId = hole.Id,
                                        Date = now,
                                        AmountIn = previousHole.Amount,
                                        AmountOut = 0,
                                        HoleMovementType = HoleMovementType.Transfer,
                                        HoleMovementTypeId = 0,
                                        CreatedDate = now,
                                        LastEditDate = now,
                                        CreatedUser = Guid.NewGuid(),
                                        LastEditUser = Guid.NewGuid()
                                    };
                                    chickenHoleMovementRepo.Add(addition);
                                    await chickenHoleMovementRepo.SaveAllAsync();
                                    // Send real-time update
                                    var dagagHoleDataVMNew = holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
                                        .Select(h => new DagagHoleDataVM
                                        {
                                            Id = h.Id,
                                            Name = h.Name,
                                            HoleType = h.HoleType,
                                            EndTime = chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                                            Amount = chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut)
                                        }).ToList();

                                    var meatHoleDataVMNew = holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                                        .Select(h => new MeatHoleDataVM
                                        {
                                            Id = h.Id,
                                            Name = h.Name,
                                            HoleType = h.HoleType,
                                            EndTime = meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                                            NafrAmount = (int)meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                                            HalfNafrAmount = (int)meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
                                        }).ToList();
                                    await _hubContext.Clients.All.SendAsync("ReceiveData", dagagHoleDataVMNew, meatHoleDataVMNew);

                                }
                            }
                        }

                        // Get all holes with their EndTime and Amount
                        var meatHoleDataVM = (await holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true))
                            .Select(h => new
                            {
                                Id = h.Id,
                                Name = h.Name,
                                EndTime = meatFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today).Result
                                               .OrderByDescending(f => f.EndTime)
                                               .FirstOrDefault()?.EndTime,
                                NafrAmount = meatHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today).Result
                                               .Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                                HalfNafrAmount = meatHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today).Result
                                               .Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut)
                            })
                            .Where(h => h.EndTime.HasValue)
                            .OrderBy(h => h.EndTime)
                            .ToList();

                        foreach (var hole in meatHoleDataVM)
                        {
                            if (hole.EndTime <= nowTime)
                            {
                                var previousHole = meatHoleDataVM
                                    .Where(h => h.EndTime < hole.EndTime)
                                    .OrderByDescending(h => h.EndTime)
                                    .FirstOrDefault();

                                if (previousHole != null && (previousHole.NafrAmount > 0 || previousHole.HalfNafrAmount > 0))
                                {
                                    // Deduct from previous hole
                                    var deduction = new MeatHoleMovement
                                    {
                                        HoleId = previousHole.Id,
                                        Date = now,
                                        NafrAmountIn = 0,
                                        NafrAmountOut = previousHole.NafrAmount,
                                        HalfNafrAmountIn = 0,
                                        HalfNafrAmountOut = previousHole.HalfNafrAmount,
                                        HoleMovementType = HoleMovementType.Transfer,
                                        HoleMovementTypeId = 0,
                                        CreatedDate = now,
                                        LastEditDate = now,
                                        CreatedUser = Guid.NewGuid(),
                                        LastEditUser = Guid.NewGuid()
                                    };
                                    meatHoleMovementRepo.Add(deduction);
                                    // Add to the current hole
                                    var addition = new MeatHoleMovement
                                    {
                                        HoleId = hole.Id,
                                        Date = now,
                                        NafrAmountIn = previousHole.NafrAmount,
                                        NafrAmountOut = 0,
                                        HalfNafrAmountIn = previousHole.HalfNafrAmount,
                                        HalfNafrAmountOut = 0,
                                        HoleMovementType = HoleMovementType.Transfer,
                                        HoleMovementTypeId = 0,
                                        CreatedDate = now,
                                        LastEditDate = now,
                                        CreatedUser = Guid.NewGuid(),
                                        LastEditUser = Guid.NewGuid()
                                    };
                                    meatHoleMovementRepo.Add(addition);
                                    await meatHoleMovementRepo.SaveAllAsync();
                                    // Send real-time update
                                    var dagagHoleDataVMNew = holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.دجاج, true).Result
                                        .Select(h => new DagagHoleDataVM
                                        {
                                            Id = h.Id,
                                            Name = h.Name,
                                            HoleType = h.HoleType,
                                            EndTime = chickenFillingRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                                            Amount = chickenHoleMovementRepo.GetAllAsync(c => c.HoleId == h.Id && c.Date.Date == today.Date).Result.Sum(c => c.AmountIn - c.AmountOut)
                                        }).ToList();

                                    var meatHoleDataVMNew = holeRepo.GetAllAsync(c => !c.IsDeleted && c.HoleType == HoleType.لحم, true).Result
                                        .Select(h => new MeatHoleDataVM
                                        {
                                            Id = h.Id,
                                            Name = h.Name,
                                            HoleType = h.HoleType,
                                            EndTime = meatFillingRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.LastOrDefault()?.EndTime ?? null,
                                            NafrAmount = (int)meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.NafrAmountIn - c.NafrAmountOut),
                                            HalfNafrAmount = (int)meatHoleMovementRepo.GetAllAsync(m => m.HoleId == h.Id && m.Date.Date == today.Date).Result.Sum(c => c.HalfNafrAmountIn - c.HalfNafrAmountOut),
                                        }).ToList();
                                    await _hubContext.Clients.All.SendAsync("ReceiveData", dagagHoleDataVMNew, meatHoleDataVMNew);

                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in EndTimeCheckerService: {ex}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
