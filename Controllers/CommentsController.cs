using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoAdventureWorks.Data;
using PhotoAdventureWorks.Models;
using PhotoAdventureWorks.ViewModels;
using System.Threading.Tasks;

namespace PhotoAdventureWorks.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<CommentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Comment model)
        {
            if (model.PhotoID == 0)
            {
                return BadRequest("PhotoID is missing");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var photo = await _context.Photos
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PhotoID == model.PhotoID);
            if (photo == null)
            {
                return BadRequest("Photo not found");
            }

            var comment = new Comment
            {
                Subject = model.Subject,
                Body = model.Body,
                PhotoID = model.PhotoID,
                User = user,
                Photo = photo
            };

            ModelState.Remove("User");
            ModelState.Remove("Photo");

            if (TryValidateModel(comment))
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Photos", new { id = photo.PhotoID });
            }

            return View("~/Views/Photos/Details.cshtml", new PhotoDetailsViewModel
            {
                Photo = photo,
                Comments = await _context.Comments.Include(c => c.User).Where(c => c.PhotoID == photo.PhotoID).ToListAsync(),
                Comment = comment
            });
        }

        // Borrar un comentario
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentID == id);

            if (comment == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || comment.User.Id != user.Id)
            {
                return Unauthorized();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Photos", new { id = comment.PhotoID });
        }

    }
}
