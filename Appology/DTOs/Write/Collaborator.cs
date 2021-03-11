using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appology.Write.DTOs
{
    public class Collaborator
    {
        public Guid CollaboratorId { get; set; }
        public bool ShowOnTree { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }
}
