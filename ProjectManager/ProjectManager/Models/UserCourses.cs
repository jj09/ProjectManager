using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManager.Models
{
    public class UserCourses
    {
        public List<Course> Have { get; set; }
        public List<Course> Rest { get; set; }
        public int Add { get; set; }
    }

    public class UsersCourses
    {
        public int CourseID { get; set; }
        public List<String> Users { get; set; } 
        public List<bool> Check { get; set; }
    }
}