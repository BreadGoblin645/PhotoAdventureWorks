using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PhotoAdventureWorks.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        [Required(ErrorMessage = "El campo Asunto es requerido")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "El campo Cuerpo es requerido")]
        public string Body { get; set; }

        [Required(ErrorMessage = "El campo FotoID es requerido")]
        public int PhotoID { get; set; }

        public IdentityUser User { get; set; }

        public Photo Photo { get; set; }  // Asegúrate de que esta propiedad esté definida correctamente
    }
}
