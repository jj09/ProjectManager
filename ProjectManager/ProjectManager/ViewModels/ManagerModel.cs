using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProjectManager.Models;

namespace ProjectManager.ViewModels
{
    public class ManagerModel
    {
        public Course Course { get; set; }
        public ProjectData ProjectData { get; set; }
        public Project Project { get; set; }
    }
}