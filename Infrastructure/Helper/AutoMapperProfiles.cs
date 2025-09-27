using AutoMapper;
using Core.Common;
using Core.Dtos;
using Core.Dtos.RolesDto;
using Core.Dtos.UserDto;
using Core.Entities;
using Core.ViewModels.ChickenFillingVM;
using Core.ViewModels.CustomerVM;
using Core.ViewModels.DeliveryBillVM;
using Core.ViewModels.DeliveryVM;
using Core.ViewModels.DriverVM;
using Core.ViewModels.DriverPriceVM;
using Core.ViewModels.ExpenseTypeVM;
using Core.ViewModels.ExpenseVM;
using Core.ViewModels.HoleVM;
using Core.ViewModels.MeatFillingVM;
using Core.ViewModels.ProductVM;
using Core.ViewModels.SaleBillVM;

namespace Infrastructure.Helper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //Roles
            CreateMap<Role, RolesGetDto>();
            CreateMap<Role, RolesRegisterDto>().ReverseMap();
            CreateMap<Role, CommonDto>();

            //Users
            CreateMap<User, UserGetDto>();
            CreateMap<User, UserEditDto>().ReverseMap();
            CreateMap<User, UserRegisterDto>().ReverseMap();
            CreateMap<User, CommonUserDrop>().ReverseMap();

            //Claims 
            CreateMap<Role, EditRolesClaimDto>();

            //ExpenseType
            CreateMap<ExpenseType, ExpenseTypeRegisterVM>().ReverseMap();
            CreateMap<ExpenseType, ExpenseTypeGetVM>();
            CreateMap<ExpenseType, CommonDrop>();

            //Expense
            CreateMap<Expense, ExpenseRegisterVM>().ReverseMap();
            CreateMap<Expense, ExpenseGetVM>();

            //Delivery
            CreateMap<Delivery, DeliveryRegisterVM>().ReverseMap();
            CreateMap<Delivery, DeliveryGetVM>();
            CreateMap<Delivery, CommonDrop>();

            //SaleBill
            CreateMap<SaleBillDetail, BillDetailRegisterVM>().ReverseMap();
            CreateMap<SaleBill, SaleBillGetVM>();
            CreateMap<SaleBill, DeliveryBillGetVM>();

            //Customer
            CreateMap<Customer, CustomerRegisterVM>().ReverseMap();
            CreateMap<Customer, CustomerGetVM>();


            //Product
            CreateMap<Product, ProductRegisterVM>().ReverseMap();
            CreateMap<Product, ProductGetVM>();
            CreateMap<Product, CommonDrop>();

            //Hole
            CreateMap<Hole, HoleRegisterVM>().ReverseMap();
            CreateMap<Hole, HoleGetVM>();
            CreateMap<Hole, CommonDrop>();

            //ChickenFilling
            CreateMap<ChickenFilling, ChickenFillingRegisterVM>().ReverseMap();
            CreateMap<ChickenFilling, ChickenFillingGetVM>();

            //MeatFilling
            CreateMap<MeatFilling, MeatFillingRegisterVM>().ReverseMap();
            CreateMap<MeatFilling, MeatFillingGetVM>();

            //Driver
            CreateMap<Driver, DriverRegisterVM>().ReverseMap();
            CreateMap<Driver, DriverGetVM>();
            CreateMap<Driver, CommonDrop>();

            //DriverPrice
            CreateMap<DriverPrice, DriverPriceRegisterVM>().ReverseMap();
            CreateMap<DriverPrice, DriverPriceGetVM>();

        }
    }
}
