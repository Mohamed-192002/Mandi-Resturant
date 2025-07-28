using System.ComponentModel.DataAnnotations;

namespace Core.Common
{
    public class EntityBase
    {
        [Key]
        public int Id { get; set; }
        public Guid CreatedUser { get; set; }
        public Guid LastEditUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public bool IsDeleted { get; set; }

    }
}
