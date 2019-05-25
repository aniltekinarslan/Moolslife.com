using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
using Resources;

namespace MoolsPayment.Models
{

    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Username", ResourceType = typeof(Resources.Home))]
        public string username { get; set; }

        [Required]
        [StringLength(28, ErrorMessageResourceName = "FailLenghtPassword", ErrorMessageResourceType = typeof(Resources.Home), MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessageResourceName = "FailPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.Home))]
        public string password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Home))]
        public string Email { get; set; }

        [Required]
        [StringLength(28, ErrorMessageResourceName = "FailLenghtPassword", ErrorMessageResourceType = typeof(Resources.Home), MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessageResourceName = "FailPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Home))]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessageResourceName = "FailDoNotMatchPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.Home))]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [StringLength(128)]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Home))]
        public string Email { get; set; }
    }



    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [StringLength(128)]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Home))]
        public string Email { get; set; }

        [Required]
        [StringLength(28, ErrorMessageResourceName = "FailLenghtPassword", ErrorMessageResourceType = typeof(Resources.Home), MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessageResourceName = "FailPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Home))]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessageResourceName = "FailDoNotMatchPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.Home))]
        public string ConfirmNewPassword { get; set; }

        [Required]
        public string Hash { get; set; }
    }


    public class ChangePasswordViewModel
    {
        [Required]
        [StringLength(28, ErrorMessageResourceName = "FailLenghtPassword", ErrorMessageResourceType = typeof(Resources.Home), MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessageResourceName = "FailCurrentPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(Resources.Home))]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(28, ErrorMessageResourceName = "FailLenghtPassword", ErrorMessageResourceType = typeof(Resources.Home), MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9.]+$", ErrorMessageResourceName = "FailPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Home))]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessageResourceName = "FailDoNotMatchPassword", ErrorMessageResourceType = typeof(Resources.Home))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.Home))]
        public string ConfirmNewPassword { get; set; }
    }
}