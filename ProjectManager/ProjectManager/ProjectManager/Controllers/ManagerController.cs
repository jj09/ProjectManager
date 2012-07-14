using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ProjectManager.Models;
using ProjectManager.ViewModels;
using org.pdfbox.util;
using org.pdfbox.pdmodel;
using ProjectManager.Helpers;
using System.IO;

namespace ProjectManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagerController : Controller
    {
        ProjectManagerEntities db = new ProjectManagerEntities();
        List<bool> chechBoxForAdd;

        //
        // GET: /Manager/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Store/GenreMenu
        [ChildActionOnly]
        public ActionResult ManagerMenu()
        {
            return PartialView();
        }

        /**********************Courses****************************************/
        //
        // GET: /Manager/Courses
        public ActionResult Courses()
        {
            var courses = from n in db.Courses
                          orderby n.Name
                          select n;

            return View(courses.ToList());
        }

        //
        // GET: /Manager/UserCourses
        public ActionResult UserCourses(String userName)
        {
            UserCourses uc = new UserCourses();
            ViewBag.userName = userName;

            var have = from c in db.Courses
                       join e in db.Enrollments on c.ID equals e.CourseId
                       where e.UserName == userName
                       orderby c.Name
                       select c;
            uc.Have = have.ToList();

            var rest = from c in db.Courses
                       orderby c.Name
                       select c;
            uc.Rest = rest.ToList();

            foreach (var restc in rest.ToList())
            {
                foreach (var havec in uc.Have)
                {
                    if (restc.ID == havec.ID)
                        uc.Rest.Remove(restc);
                }
            }

            return View(uc);
        }

        [HttpPost]
        public ActionResult UserCourses(string userName, UserCourses uc)
        {
            Enrollment en = new Enrollment();
            en.UserName = userName;
            en.CourseId = uc.Add;
            db.Enrollments.Add(en);
            db.SaveChanges();

            this.UserCourses(userName);
            return View();
        }

        //
        // GET: /Manager/UsersCourses
        public ActionResult UsersCourses()
        {            
            UsersCourses uc = new UsersCourses();

            var all = from c in db.Courses
                      orderby c.Name
                      select c;

            ViewData["Courses"] = new SelectList(all.ToList(), "Id", "Name");

            var users = from u in db.Users
                        where u.Role != "admin"
                        orderby u.UserName
                      select u.UserName;
            uc.Users = users.ToList();            

            List<bool> add = new List<bool>();

            foreach(var u in users.ToList())
            {
                int n = all.ToList().First().ID;
                var enroll = from e in db.Enrollments
                            where e.CourseId == n && e.UserName == u
                            select e.ID;

                if (enroll.Count() == 0)
                    add.Add(false);
                else
                    add.Add(true);
            }
            uc.Check = add;

            return View(uc);
        }

        [HttpPost]
        public ActionResult UsersCourses(UsersCourses model)
        {
            var users = from u in db.Users
                        where u.Role != "admin"
                        orderby u.UserName
                        select u.UserName;
            model.Users = users.ToList();

            List<bool> add = new List<bool>();

            foreach (var u in users.ToList())
            {
                int n = model.CourseID;
                var enroll = from e in db.Enrollments
                             where e.CourseId == n && e.UserName == u
                             select e.ID;

                if (enroll.Count() == 0)
                    add.Add(false);
                else
                    add.Add(true);
            }
            model.Check = add;
            
            return View(model);
        }

        //
        // GET: /Manager/CourseCreate
        public ActionResult CourseCreate()
        {
            return View();
        }

        //
        // POST: /Manager/CourseCreate
        [HttpPost]
        public ActionResult CourseCreate(Course newCourse)
        {
            if (ModelState.IsValid)
            {
                db.Courses.Add(newCourse);
                db.SaveChanges();
                return RedirectToAction("Courses");
            }
            else
            {
                return View(newCourse);
            }
        }

        // 
        // GET: /Manager/CourseEdit
        public ActionResult CourseEdit(int id)
        {
            Course course = db.Courses.Find(id);
            if (course == null)
                return RedirectToAction("Courses");
            return View(course);
        }

        // 
        // POST: /Manager/CourseEdit 
        [HttpPost]
        public ActionResult CourseEdit(Course model)
        {
            try
            {
                var course = db.Courses.Find(model.ID);

                UpdateModel(course);
                db.SaveChanges();
                return RedirectToAction("Courses", new { id = model.ID });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Edit Failure, see inner exception");
            }

            return View(model);
        }

        // 
        // GET: /Manager/UserCourseDelete
        public ActionResult UserCourseDelete(int id, String userName)
        {
            var id2 = from e in db.Enrollments
                      where e.CourseId == id && e.UserName == userName
                      select e.ID;
            db.Enrollments.Remove(db.Enrollments.Find(id2.ToList().First()));
            db.SaveChanges();

            var ids = from pd in db.ProjectsData
                      join p in db.Projects on pd.ProjectId equals p.ID
                      where p.CourseId == id && pd.UserName == userName
                      select pd.ID;

            foreach (var id3 in ids.ToList())
            {
                var temp = db.ProjectsData.Find(id3);
                db.ProjectsData.Remove(temp);
            }

            db.SaveChanges();

            return RedirectToAction("UserCourses", new { userName = userName });
        }

        // 
        // GET: /Manager/CourseDelete
        public ActionResult CourseDelete(int id)
        {
            Course course = db.Courses.Find(id);
            if (course == null)
                return RedirectToAction("Courses");
            return View(course);
        }

        // 
        // POST: /Manager/CourseDelete 
        [HttpPost]
        public RedirectToRouteResult CourseDelete(int id, FormCollection collection)
        {
            Course course = db.Courses.Find(id);
            db.Courses.Remove(course);
            db.SaveChanges();

            var id2 = from e in db.Enrollments
                      where e.CourseId == id
                      select e.ID;

            foreach (var i in id2.ToList())
            {
                db.Enrollments.Remove(db.Enrollments.Find(i));
            }

            var ids = from pd in db.ProjectsData
                      join p in db.Projects on pd.ProjectId equals p.ID
                      where p.CourseId == id
                      select pd.ID;

            foreach (var id3 in ids.ToList())
            {
                var temp = db.ProjectsData.Find(id3);
                db.ProjectsData.Remove(temp);
            }

            var id4 = from p in db.Projects
                      where p.CourseId == id
                      select p.ID;

            foreach (var i in id4.ToList())
            {
                db.Projects.Remove(db.Projects.Find(i));
            }

            db.SaveChanges();

            return RedirectToAction("Courses");
        }

        /**********************Projects***************************************/
        //
        // GET: /Manager/Projects
        public ActionResult Projects(int id = 0)
        {
            var projects = from n in db.Projects
                           join c in db.Courses on n.CourseId equals c.ID
                           where (id > 0 ? c.ID == id : c.ID > 0)
                           orderby c.Name, n.ProjectNo
                           select new ManagerModel { Project = n, Course = c };

            var courses = from c in db.Courses
                          orderby c.Name
                          select c;
            ViewBag.courses = courses.ToList();

            return View(projects);
        }

        ////
        //// POST: /Manager/Projects
        //[HttpPost]
        //public ActionResult Projects(ManagerModel model)
        //{
        //    var projects = from n in db.Projects
        //                   join c in db.Courses on n.CourseId equals c.ID
        //                   where (model.Course.ID > 0 ? model.Course.ID == n.CourseId : n.CourseId > 0)
        //                   orderby c.Name, n.ProjectNo
        //                   select new ManagerModel { Project = n, Course = c };

        //    var courses = from c in db.Courses
        //                  orderby c.Name
        //                  select c;
        //    ViewBag.courses = courses.ToList();

        //    return View(projects);
        //}

        //
        // GET: /Manager/ProjectCreate
        public ActionResult ProjectCreate()
        {
            var courses = from c in db.Courses
                          orderby c.Name
                          select c;
            ViewBag.courses = courses.ToList();
            return View();
        }

        //
        // POST: /Manager/ProjectCreate
        [HttpPost]
        public ActionResult ProjectCreate(Project newProject)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(newProject);
                db.SaveChanges();
                return RedirectToAction("Projects");
            }
            else
            {
                return View(newProject);
            }
        }

        // 
        // GET: /Manager/ProjectEdit
        public ActionResult ProjectEdit(int id)
        {
            Project project = db.Projects.Find(id);
            if (project == null)
                return RedirectToAction("Projects");
            var courses = from c in db.Courses
                          orderby c.Name
                          select c;
            ViewBag.courses = courses.ToList();
            return View(project);
        }

        // 
        // POST: /Manager/ProjectEdit 
        [HttpPost]
        public ActionResult ProjectEdit(Project model)
        {
            try
            {
                var project = db.Projects.Find(model.ID);

                UpdateModel(project);
                db.SaveChanges();
                return RedirectToAction("Projects", new { id = model.CourseId });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Edit Failure, see inner exception");
            }

            return View(model);
        }

        // 
        // GET: /Manager/ProjectDelete
        public ActionResult ProjectDelete(int id)
        {
            Project project = db.Projects.Find(id);
            if (project == null)
                return RedirectToAction("Projects");

            var projectInfo = from p in db.Projects
                              join c in db.Courses on p.CourseId equals c.ID
                              where p.ID == id
                              select new ManagerModel { Course = c, Project = p };

            return View(projectInfo.Single());
        }

        // 
        // POST: /Manager/ProjectDelete 
        [HttpPost]
        public RedirectToRouteResult ProjectDelete(int id, FormCollection collection)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();

            var ids = from p in db.ProjectsData
                      where p.ProjectId == project.ID
                      select p.ID;

            foreach (var id2 in ids.ToList())
            {
                var temp = db.ProjectsData.Find(id2);
                db.ProjectsData.Remove(temp);
            }

            db.SaveChanges();

            return RedirectToAction("Projects");
        }

        // 
        // GET: /Manager/ProjectData/ProjectId/CourseId
        public ActionResult ProjectsData(int id = 0, int courseId = 0)
        {
            ViewBag.cid = courseId;
            //creating dropdown list
            List<SelectListItem> notes = new List<SelectListItem>();
            notes.Add(new SelectListItem
            {
                Text = "-",
                Value = "0"
            });
            notes.Add(new SelectListItem
            {
                Text = "2,0",
                Value = "2",
            });
            for (double f = 3.0; f < 6; f += 0.5)
            {
                notes.Add(new SelectListItem
                {
                    Text = String.Format("{0:0.0}", f),
                    Value = f.ToString()
                });
            }
            ViewData["notes"] = new SelectList(notes, "Value", "Text");
            ViewBag.notes = notes;

            if (id == 0)
            {
                var projectsData = from pd in db.ProjectsData
                                   join p in db.Projects on pd.ProjectId equals p.ID
                                   join c in db.Courses on p.CourseId equals c.ID
                                   orderby c.Name, p.ProjectNo, pd.UserName
                                   select new ManagerModel { ProjectData = pd, Project = p, Course = c };
                ViewBag.Title = "Projects";
                ViewBag.showNames = true;
                return View(projectsData);
            }
            else
            {
                var projectsData = from pd in db.ProjectsData
                                   join p in db.Projects on pd.ProjectId equals p.ID
                                   where pd.ProjectId == id
                                   orderby pd.UserName
                                   select new ManagerModel { ProjectData = pd, Project = p };

                var projectName = from p in db.Projects
                                  join c in db.Courses on p.CourseId equals c.ID
                                  where p.ID == id
                                  select new ManagerModel { Course = c, Project = p };
                if(projectName.Count()>0)
                    ViewBag.Title = "Projects: " + projectName.First().Project.Name + " (" + projectName.First().Course.Name + ")";
                ViewBag.showNames = false;
                return View(projectsData);
            }
        }

        // 
        // GET: /Manager/ProjectData/ProjectId
        public ActionResult ProjectsDataDelete(int id)
        {
            ProjectData pd = db.ProjectsData.Find(id);
            if (pd == null)
                return RedirectToAction("ProjectsData");
            else
            {
                var projectData = from projd in db.ProjectsData
                                  where projd.ID == id
                                  select projd;
                if (projectData.Single().Pdf != null)
                {
                    var path = Server.MapPath("~/Uploads/Pdf/" + projectData.Single().Pdf);
                    FileInfo pdfFile = new FileInfo(path.ToString());
                    pdfFile.Delete();
                }
                if (projectData.Single().Code != null)
                {
                    var path = Server.MapPath("~/Uploads/Code/" + projectData.Single().Code.ToString());
                    FileInfo codeFile = new FileInfo(path);
                    codeFile.Delete();
                }
                db.ProjectsData.Remove(pd);
                db.SaveChanges();

                return RedirectToAction("ProjectsData");
            }
        }

        public ActionResult ProjectCheck(int id, int projectId)
        {
            //get the project
            var project1 = from pd in db.ProjectsData
                           where pd.ID == id
                           select pd;
            PDDocument pdf1 = PDDocument.load(Server.MapPath("~/Uploads/PDF/" + project1.Single().Pdf.ToString()));

            //get other projects (the same type)
            var otherProjects = from pd in db.ProjectsData
                                where pd.ID != id && pd.ProjectId == projectId
                                select pd;

            //stuff for pdf compare
            LinkedList<PlagiarismModel> results = new LinkedList<PlagiarismModel>();

            PDDocument pdf2;
            PDFTextStripper stripper = new PDFTextStripper();
            string text;
            string text1;
            string tmp;
            Random gen = new Random();
            int j;
            int f;
            int g;
            double yes;
            double plag;
            int iter = 80;



            foreach (var item in otherProjects)
            {
                yes = j = 0;
                pdf2 = PDDocument.load(Server.MapPath("~/Uploads/PDF/" + item.Pdf.ToString()));
                text = stripper.getText(pdf1);
                text1 = stripper.getText(pdf2);
                j = text1.Length;
                for (int i = 0; i < iter; i++)
                {
                    g = gen.Next(1, 50);
                    f = gen.Next(1, j - g);
                    tmp = text1.Substring(f, g);
                    if (HtmlHelpers.search(text, tmp))
                        yes++;
                }
                plag = (yes / iter) * 100;
                results.AddLast(new PlagiarismModel(plag, item));
            }

            ViewBag.checkedFile = new PlagiarismModel(0, project1.Single());


            return View(results);
        }

        // 
        // POST: /Manager/CourseEdit 
        [HttpPost]
        public ActionResult SetNote(NoteModel model)
        {

            try
            {
                var pd = db.ProjectsData.Find(model.pdID);
                pd.Note = model.Note;
                db.SaveChanges();
                if (Request.IsAjaxRequest())
                    return Content("Success");
                else
                    return Redirect(Request.UrlReferrer.ToString());


            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Edit Failure, see inner exception");
            }

            //return View(model);
            return RedirectToAction("ProjectsData");
        }

    }
}
