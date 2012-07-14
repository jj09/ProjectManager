using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectManager.Models;
using ProjectManager.ViewModels;
using System.IO;

namespace ProjectManager.Controllers
{
    public class HomeController : Controller
    {
        //NewsDBContext newsDb = new NewsDBContext();
        ProjectManagerEntities db = new ProjectManagerEntities();

        public ActionResult Index()
        {
            //var news = from n in newsDb.News
            var news = from n in db.News
                       where n.Date < DateTime.Now
                       orderby n.Date descending
                       select n;

            return View(news.ToList());
        }

        //
        // GET: /Home/Info
        public ActionResult Info()
        {
            return View();
        }

        //
        // GET: /Home/InfoEdit
        public ActionResult InfoEdit()
        {
            if (Request.IsAuthenticated && HttpContext.User.IsInRole("admin"))
            {
                return View();
            }
            else
            {
                return View("AccessDenied");
            }
        }

        //
        // POST: /Home/InfoEdit
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult InfoEdit(Editor e)
        {
            if (Request.IsAuthenticated && HttpContext.User.IsInRole("admin"))
            {
                var path = Server.MapPath("~/Views/Home/InfoContent.cshtml");
                System.IO.StreamWriter file = new System.IO.StreamWriter(path, false, System.Text.Encoding.UTF8);
                file.WriteLine(e.content);
                file.Close();

                return View("Info");
            }
            else
            {
                return View("AccessDenied");
            }
        }

        public ActionResult InfoContent()
        {
            return PartialView();
        }

        // 
        // GET: /Home/Project/CourseId
        public ActionResult Projects(int id=0)
        {
            if (!Request.IsAuthenticated && !HttpContext.User.IsInRole("user"))
                return View("AccessDenied");
            
            var subquery = from e in db.Enrollments
                           where e.UserName == this.HttpContext.User.Identity.Name && (id > 0 ? e.ID == id : e.ID > 0)
                           select e.CourseId;

            var projectsData = from c in db.Courses
                               join p in db.Projects on c.ID equals p.CourseId
                               where subquery.Contains(p.CourseId)
                               orderby c.Name, p.ProjectNo
                               select new ManagerModel { Course = c, Project = p, ProjectData = null };

            ProjectManagerEntities db2 = new ProjectManagerEntities();

            LinkedList<ManagerModel> projectsList = new LinkedList<ManagerModel>();

            foreach (var item in projectsData)
            {
                var projData = from pd in db2.ProjectsData
                               join p in db2.Projects on pd.ProjectId equals p.ID
                               where pd.UserName == this.HttpContext.User.Identity.Name && pd.ProjectId == item.Project.ID
                               select new ManagerModel { Project = p, ProjectData = pd };
                if (projData.SingleOrDefault() != null)
                {
                    item.ProjectData = projData.Single().ProjectData;
                    item.Project = projData.Single().Project;
                }
                projectsList.AddLast(item);
            }

            db2.Dispose();

            return View(projectsList);
        }

        //
        // POST: /Home/Project
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Projects(UploadProject up)
        {
            if (!Request.IsAuthenticated && !HttpContext.User.IsInRole("user"))
                return View("AccessDenied");
            if (up.type == "Pdf")
            {
                var query = from pd in db.ProjectsData
                            where pd.UserName == this.HttpContext.User.Identity.Name && pd.ProjectId == up.ProjectId
                            select pd;

                if (up.ufile == null)
                {
                    var path = Server.MapPath("~/Uploads/Pdf/" + query.Single().Pdf);
                    FileInfo pdfFile = new FileInfo(path);
                    pdfFile.Delete();
                    if (query.Single().Code == null)
                    {
                        db.ProjectsData.Remove(query.Single());
                        db.SaveChanges();
                    }
                    else
                    {
                        query.Single().Pdf = null;
                        db.SaveChanges();
                    }
                }
                else
                {
                    var projectData = from c in db.Courses
                                      join p in db.Projects on c.ID equals p.CourseId
                                      where p.ID == up.ProjectId
                                      select new ManagerModel { Course = c, Project = p };
                    string fileName = projectData.Single().Course.Name + "_" + projectData.Single().Project.ProjectNo + "_" + this.HttpContext.User.Identity.Name.ToString() + ".pdf";
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads/PDF"), fileName);
                    up.ufile.SaveAs(filePath);

                    if (query.Count() == 1)
                    {
                        query.Single().Pdf = fileName;
                        query.Single().Received = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        // Create a new Order object.
                        ProjectData pd = new ProjectData
                        {
                            ProjectId = up.ProjectId,
                            UserName = this.HttpContext.User.Identity.Name,
                            Pdf = fileName,
                            Received = DateTime.Now,
                            Note = 0
                        };

                        // Add the new object to the Orders collection.
                        db.ProjectsData.Add(pd);

                        // Submit the change to the database.
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            //
                        }

                    }
                }
            }
            else //if (up.type == "Code")
            {
                var query = from pd in db.ProjectsData
                            where pd.UserName == this.HttpContext.User.Identity.Name && pd.ProjectId == up.ProjectId
                            select pd;

                if (up.ufile == null)
                {
                    var path = Server.MapPath("~/Uploads/Code/" + query.Single().Code);
                    FileInfo codeFile = new FileInfo(path);
                    codeFile.Delete();
                    if (query.Single().Pdf == null)
                    {
                        db.ProjectsData.Remove(query.Single());
                        db.SaveChanges();
                    }
                    else
                    {
                        query.Single().Code = null;
                        db.SaveChanges();
                    }
                }

                else
                {
                    var projectData = from c in db.Courses
                                      join p in db.Projects on c.ID equals p.CourseId
                                      where p.ID == up.ProjectId
                                      select new ManagerModel { Course = c, Project = p };
                    string fileName = projectData.Single().Course.Name + "_" + projectData.Single().Project.ProjectNo + "_" + this.HttpContext.User.Identity.Name.ToString() + "_" + up.ufile.FileName;
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads/Code"), fileName);
                    up.ufile.SaveAs(filePath);

                    if (query.Count() == 1)
                    {
                        query.Single().Code = fileName;
                        query.Single().Received = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        // Create a new Order object.
                        ProjectData pd = new ProjectData
                        {
                            ProjectId = up.ProjectId,
                            UserName = this.HttpContext.User.Identity.Name,
                            Code = fileName,
                            Received = DateTime.Now,
                            Note = 0
                        };

                        // Add the new object to the Orders collection.
                        db.ProjectsData.Add(pd);

                        // Submit the change to the database.
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            //
                        }

                    }
                }
            }


            return Projects();
        }
    }
}
