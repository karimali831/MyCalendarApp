using System;


namespace Appology.Website.ViewModels
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ErrorMsg { get; set; }
        public Guid? InviteeId { get; set; } 
        public Guid? DocId { get; set; }
    }
}