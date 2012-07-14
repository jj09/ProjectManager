using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectManager.Models
{
    public class UploadProject
    {
        public int ProjectId { get; set; }
        public HttpPostedFileBase ufile { get; set; }
        public string type { get; set; }
    }
}