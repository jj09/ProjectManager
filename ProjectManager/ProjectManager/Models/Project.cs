using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using System;

namespace ProjectManager.Models
{
    public class Project
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Project number is required")]
        public int ProjectNo { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        public DateTime Deadline { get; set; }
    }
}