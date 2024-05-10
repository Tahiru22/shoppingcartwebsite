﻿using System.ComponentModel.DataAnnotations;

namespace shoppingcartwebsite.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First name not specified")]
        public string FirstName { get; set; }
        
        public string? SecondName { get; set; }

        [Required(ErrorMessage = "Last name not specified")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number not specified")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email not specified")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password not specified")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password entered incorrectly")]
        public string ConfirmPassword { get; set; }
    }
}
