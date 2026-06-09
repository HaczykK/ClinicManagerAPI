using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Auth;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć minimum 6 znaków.")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(100)]
        [Display(Name = "Imię")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100)]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "Specjalizacja (opcjonalnie)")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Rola jest wymagana.")]
        [Display(Name = "Rola")]
        public string Role { get; set; } = "Rejestratorka";
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Walidacja roli — Admin nie może być przypisany przez rejestrację
        var allowedRoles = new[] { "Lekarz", "Rejestratorka" };
        if (!allowedRoles.Contains(Input.Role))
        {
            ErrorMessage = "Niedozwolona rola. Wybierz Lekarz lub Rejestratorka.";
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(Input.Role))
        {
            ErrorMessage = $"Rola '{Input.Role}' nie istnieje w systemie.";
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Specialization = Input.Specialization
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Input.Role);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/Index");
    }
}
