using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhotoAdventureWorks.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel(UserManager<IdentityUser> userManager) : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public required string Username { get; set; }

        public async Task<IActionResult> OnGetAsync(string? username, string? returnUrl = null)
        {
            if (username == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound($"Unable to load user with username '{username}'.");
            }

            Username = username;

            return Page();
        }
    }
}
