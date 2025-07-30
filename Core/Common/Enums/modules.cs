namespace Core.Common.Enums
{
    public class RoleModule
    {

        private static Dictionary<string, string>? ControllerNames;

        public static Dictionary<string, string> GetController()
        {
            ControllerNames = new Dictionary<string, string>
            {
                //Users
                { "UsersIndex", " المستخدمين " },
                { "GetDeviceRegistrations", " الهواتف " },
                { "DeleteDeviceRegistration", " حذف هاتف " },
                { "AddDeviceRegistration", " اضافه هاتف " },
                { "UsersCreate", " اضافه مستخدم " },
                { "UsersEdit", " تعديل بيانات مستخدم " },
                { "UsersDelete", " حذف مستخدم " },
                { "UsersEditPassword", " تعديل الباسورد " },
                
                //Roles
                { "RolesIndex", " المجموعات و الصلاحيات " },
                { "RolesCreate", " اضافه مجموعه " },
                { "RolesEdit", " تعديل بيانات مجموعه " },
                { "RolesDelete", " حذف مجموعه " },
                { "MangeRolePermition", " صلاحيات مجموعه  " },

                //Product
                { "ProductIndex", "المنتجات" },
                { "ProductCreate", "اضافه منتج" },
                { "ProductEdit", " تعديل بيانات منتج " },
                { "ProductDelete", " حذف منتج " },

                //Hole
                { "HoleIndex", "الحفر" },
                { "HoleCreate", "اضافه حفرة" },
                { "HoleEdit", " تعديل بيانات حفرة " },
                { "HoleDelete", " حذف حفرة " },

                 //ChickenFilling
                { "ChickenFillingIndex", "تجهيز حفر الدجاج" },
                { "ChickenFillingCreate", "اضافه تجهيز دجاج" },
                { "ChickenFillingEdit", " تعديل بيانات تجهيز دجاج " },
                { "ChickenFillingDelete", " حذف تجهيز دجاج " },

                 //MeatFilling
                { "MeatFillingIndex", "تجهيز حفر اللحم" },
                { "MeatFillingCreate", "اضافه تجهيز لحم" },
                { "MeatFillingEdit", " تعديل بيانات تجهيز لحم " },
                { "MeatFillingDelete", " حذف تجهيز لحم " },

                //Expense
                { "ExpenseIndex", "المصروفات" },
                { "ExpenseCreate", " اضافه مصروف " },
                { "ExpenseEdit", " تعديل بيانات مصروف " },
                { "ExpenseDelete", " حذف مصروف " },

                //ExpenseType
                { "ExpenseTypeIndex", "أنواع المصروفات" },
                { "ExpenseTypeCreate", " اضافه نوع مصروف " },
                { "ExpenseTypeEdit", " تعديل بيانات نوع مصروف " },
                { "ExpenseTypeDelete", " حذف نوع مصروف " },

                //Delivery
                { "DeliveryIndex", "شركات التوصيل" },
                { "DeliveryCreate", "اضافه شركة" },
                { "DeliveryEdit", " تعديل بيانات شركة " },
                { "DeliveryDelete", " حذف شركة " },

                //Driver
                { "DriverIndex", "سائقين التوصيل" },
                { "DriverCreate", "اضافه سائق" },
                { "DriverEdit", " تعديل بيانات سائق " },
                { "DriverDelete", " حذف سائق " },

                //Customer
                { "CustomerIndex", "العملاء" },
                { "CustomerCreate", "اضافه عميل" },
                { "CustomerEdit", " تعديل بيانات عميل " },
                { "CustomerDelete", " حذف عميل " },

                //SaleBill
                { "SaleBillIndex", " فاتورة الكاشير" },

                //EarnReport
                { "EarnReportIndex", "تقرير الربح" },

                //DeliveryBillReport
                { "DeliveryBillReportIndex", "تقرير دليفري الشركات" },

                //DriverBillReport
                { "DriverBillReportIndex", "تقرير دليفري المطعم" },

                //CashierSaleReport
                { "CashierSaleReportIndex", "تقرير مبيعات الكاشير" },

            };

            return ControllerNames;

        }


    }

}
