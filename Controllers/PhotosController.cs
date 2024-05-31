using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoAdventureWorks.Data;
using PhotoAdventureWorks.Models;
using X.PagedList;
using PhotoAdventureWorks.ViewModels;



namespace PhotoAdventureWorks.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PhotosController> _logger;
        private readonly string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        public PhotosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<PhotosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult SlideShow()
        {
            var photos = _context.Photos
                .Include(p => p.User)
                .OrderByDescending(p => p.PhotoID)
                .ToList();

            return View(photos);
        }

        public async Task<IActionResult> UserGallery(int? page)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var photos = await _context.Photos
                .Where(p => p.UserID == user.Id)
                .OrderByDescending(p => p.PhotoID)
                .ToListAsync();

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(photos.ToPagedList(pageNumber, pageSize));
        }

        public IActionResult Upload()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile upload, Photo photo)
        {
            if (upload != null && upload.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found");
                    return View(photo);
                }

                var extension = Path.GetExtension(upload.FileName);
                var newFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(rootPath, newFileName);

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                photo.PhotoFile = $"/uploads/{newFileName}";
                photo.UserID = user.Id;
                photo.CreatedDate = photo.CreatedDate;
                photo.Title ??= "Default Title"; 
                photo.Description ??= "Default Description"; 

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return RedirectToAction("UserGallery");
            }

            ModelState.AddModelError("upload", "Please select a file");
            return View(photo);
        }



        [AllowAnonymous]
        public IActionResult Index(string searchString, int? page)
        {
            ViewBag.CurrentFilter = searchString;

            IQueryable<Photo> photos = _context.Photos.Include(p => p.User);

            if (!String.IsNullOrEmpty(searchString))
            {
                photos = photos.Where(p => p.Title.Contains(searchString) || p.Description.Contains(searchString));
            }

            photos = photos.OrderByDescending(p => p.PhotoID);

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(photos.ToPagedList(pageNumber, pageSize));
        }



        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PhotoID == id);

            if (photo == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PhotoID == id)
                .ToListAsync();

            var viewModel = new PhotoDetailsViewModel
            {
                Photo = photo,
                Comments = comments ?? new List<Comment>(),
                Comment = new Comment
                {
                    PhotoID = photo.PhotoID,
                    Subject = "",
                    Body = "",
                    User = photo.User,
                }
            };

            return View(viewModel);
        }

        private bool PhotoExists(int id)
        {
            return _context.Photos.Any(e => e.PhotoID == id);
        }

        // Método GET para cargar la vista de edición
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // Método POST para guardar los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhotoID,Title,Description,CreatedDate,UserID,PhotoFile,User")] Photo photo)
        {
            if (id != photo.PhotoID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPhoto = await _context.Photos.Include(p => p.User).FirstOrDefaultAsync(p => p.PhotoID == id);
                    if (existingPhoto == null)
                    {
                        return NotFound();
                    }

                    existingPhoto.Title = photo.Title ?? "Default Title";
                    existingPhoto.Description = photo.Description ?? "Default Description";
                    existingPhoto.CreatedDate = photo.CreatedDate != default ? photo.CreatedDate : existingPhoto.CreatedDate;
                    existingPhoto.UserID = existingPhoto.UserID;
                    existingPhoto.User = existingPhoto.User;

                    _context.Update(existingPhoto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = photo.PhotoID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhotoExists(photo.PhotoID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(photo);
        }



        public async Task<IActionResult> Delete(int? id, bool? saveChangeError = false)
        {
            if (id == null)
            {
                return BadRequest();
            }

            if (saveChangeError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var photo = await _context.Photos.FindAsync(id);
                if (photo == null)
                {
                    _logger.LogWarning($"Photo with ID {id} not found.");
                    return NotFound($"Photo with ID {id} not found.");
                }

                _context.Photos.Remove(photo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id, saveChangeError = true });
            }

            return RedirectToAction(nameof(UserGallery));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
