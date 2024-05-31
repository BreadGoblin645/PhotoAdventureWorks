using PhotoAdventureWorks.Models;
using System.Collections.Generic;

namespace PhotoAdventureWorks.ViewModels
{
    public class PhotoDetailsViewModel
    {
        public Photo Photo { get; set; }
        public Comment Comment { get; set; } = new Comment();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
