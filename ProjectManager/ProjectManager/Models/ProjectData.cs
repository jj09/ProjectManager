using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using System;

namespace ProjectManager.Models
{
    public class ProjectData
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "User is required")]
        public string UserName { get; set; }

        public string Pdf { get; set; }

        public string Code { get; set; }

        public DateTime Received { get; set; }

        public double Note { get; set; }
    }
}