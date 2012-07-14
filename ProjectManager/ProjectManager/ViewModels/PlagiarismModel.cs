using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProjectManager.Models;

namespace ProjectManager.ViewModels
{
    public class PlagiarismModel
    {
        public PlagiarismModel(double result, ProjectData pd)
        {
            this.result = result;
            this.ProjectData = pd;
        }

        public double result { get; set; }
        public ProjectData ProjectData { get; set; }
    }
}