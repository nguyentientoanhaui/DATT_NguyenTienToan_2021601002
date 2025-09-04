using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        public string? PictureUrl { get; set; }

        public string Provider { get; set; } // "Google" or "Facebook"

        public string ProviderKey { get; set; } // Unique ID from the provider
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Gender { get; set; }

        [Range(1, 120, ErrorMessage = "Tuổi phải từ 1 đến 120")]
        public int? Age { get; set; }

        public string? PictureUrl { get; set; }

        public string Provider { get; set; }

        public string ProviderKey { get; set; }
    }
}
