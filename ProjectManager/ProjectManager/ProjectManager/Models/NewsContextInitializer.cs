using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Database;

namespace ProjectManager.Models
{
    public class NewsContextInitializer : DropCreateDatabaseIfModelChanges<NewsDBContext>
    {
        protected override void Seed(NewsDBContext context)
        {
        }
    }
}