using System;
using System.Collections.Generic;

namespace Appology.Website.ViewModels
{
    public class AvatarVM
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public IEnumerable<string> PreAvatars { get; set; }
    }
}