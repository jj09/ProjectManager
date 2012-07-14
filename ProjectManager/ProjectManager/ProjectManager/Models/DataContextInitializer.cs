using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Database;

namespace ProjectManager.Models
{
    public class DataContextInitializer : DropCreateDatabaseIfModelChanges<ProjectManagerEntities>
    {
        protected override void Seed(ProjectManagerEntities context)
        {
        }
    }

}