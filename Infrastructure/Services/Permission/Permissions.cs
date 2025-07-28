using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common.Enums;

namespace Infrastracture.Services.Permission
{
    public static class Permissions
    {

        public static List<string> GeneratePermissionsForModule(string module)
        {
            return new List<string>()
                                   {
                                    $"Permissions.{module}.Create",
                                    $"Permissions.{module}.Index",
                                    $"Permissions.{module}.Edit",
                                    $"Permissions.{module}.Delete",

                                   };

        }


        public static List<SchemaOfRole> GenerateAllPermissionsForModule()
        {
            var allpermissions = new List<SchemaOfRole>();

            foreach (var item in RoleModule.GetController())
            {
                allpermissions.Add(new SchemaOfRole
                {
                    en = $"Permissions.{item.Key}",
                    ar = $"{item.Value}"
                });
            }

            return allpermissions;
        }
    }

    public class SchemaOfRole
    {
        public string ar { get; set; }
        public string en { get; set; }
    }

}
