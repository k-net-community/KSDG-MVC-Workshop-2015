using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Models
{
    [MetadataType(typeof(GuestBookMetadata))]
    public partial class GuestBook
    {
        public class GuestBookMetadata
        {
            public string Id { get; set; }
            [Required]
            [Display(Name = "使用者名稱")]
            public string Name { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            [Display(Name = "主題")]
            public string Subject { get; set; }
            [Display(Name = "內文")]
            public string Body { get; set; }
            public DateTime DateCreated { get; set; }
        }
    }
}
