using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PhotoAdventureWorks.Models
{
    public class Photo
    {
        [Key]
        public int PhotoID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The PhotoFile field is required")]
        public string PhotoFile { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public IdentityUser User { get; set; }
    }
}
