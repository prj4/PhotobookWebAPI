using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PhotoBookDatabase.Model;

namespace PhotobookWebAPI.Models
{
    public class GetEventModel
    {
        [Required]
        [Display(Name = "Events")]
        public List<Event> Events { get; set; }
        
    }

    public class CreateEventModel
    {
        [Required]
        public string Location { get; set; }
        public string Description { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
}
