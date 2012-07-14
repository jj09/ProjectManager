using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;

namespace ProjectManager.Models
{
    public class Enrollment
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "User is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
    }
}