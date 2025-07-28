using AutoMapper;
using Core.Dtos.RolesDto;
using Core.Dtos.UserDto;
using Core.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.ViewModels.ExpenseTypeVM;
using Core.ViewModels.ExpenseVM;
using Core.ViewModels.DeliveryVM;
using Core.ViewModels.SaleBillVM;
using Core.ViewModels.CustomerVM;
using Core.ViewModels.DeliveryBillVM;
using Core.ViewModels.HoleVM;
using Core.ViewModels.ProductVM;
using Core.ViewModels.ChickenFillingVM;
using Core.ViewModels.MeatFillingVM;
using Core.ViewModels.DriverVM;

namespace Infrastructure.Helper
{
    public class AutoMapperProfiles:Profile
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

        }
    }
}
