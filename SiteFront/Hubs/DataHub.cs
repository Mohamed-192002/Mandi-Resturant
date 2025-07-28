using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.HoleVM;
using Microsoft.AspNetCore.SignalR;

namespace SiteFront.Hubs
{
    public class DataHub : Hub
    {
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<MeatFilling> _meatFillingRepo;

        public DataHub(IRepository<Hole> holeRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<MeatFilling> meatFillingRepo)
        {
            _holeRepo = holeRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
            _meatHoleMovementRepo = meatHoleMovementRepo;
            _chickenFillingRepo = chickenFillingRepo;
            _meatFillingRepo = meatFillingRepo;
        }
        public async Task SendData()
        {
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
            await Clients.All.SendAsync("ReceiveData", dagagHoleDataVM, meatHoleDataVM);
        }

    }
}
