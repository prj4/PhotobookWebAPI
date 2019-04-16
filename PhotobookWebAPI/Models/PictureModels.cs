using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotobookWebAPI.Models
{
    public class InsertPictureModel
    {

        [Required]
        public string PictureString { get; set; }

        [Required] public string EventPin { get; set; }

    }

    public class RetrievePictureModel
    {
        [Required]
        public string EventPin { get; set; }
        [Required]
        public int PictureId { get; set; }
    }

    public class RequestPicturesModel
    {
        [Required]
        public string EventPin { get; set; }
    }

    public class RequestPicturesAnswerModel
    {
        [Required]
        public List<int> PictureList { get; set; }

    }
}
