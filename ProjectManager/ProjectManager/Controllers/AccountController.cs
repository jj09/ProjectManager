using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using ProjectManager.Models;
using System.Xml;
using LinqToExcel;
using System.IO;

namespace ProjectManager.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        public UserDBContext userDb = new UserDBContext();
        ProjectManagerEntities db = new ProjectManagerEntities();
        public List<SelectListItem> roles  = new List<SelectListItem>();

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            roles.Add(new SelectListItem
            {
                Value = "admin",
                Text = "Administrator"
            });
            roles.Add(new SelectListItem
            {
                Value = "user",
                Text = "User",
            });

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            return View();
        }

        public ActionResult Index()
        {
            var users = from n in userDb.Users
                       orderby n.UserName ascending
                       select n;

            return View(users.ToList());
        }

        [ChildActionOnly]
        public ActionResult LogOnFast()
        {
            return PartialView();
        }
        
        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Add
        // **************************************

        public ActionResult Add()
        {
            ViewData["Roles"] = new SelectList(roles, "Value", "Text", "user");
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Add(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    Roles.AddUserToRole(model.UserName, model.Role);
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // 
        // GET: /User/Details
        public ActionResult Details(Guid id)
        {
            User user = userDb.Users.Find(id);
            if (user == null)
                return RedirectToAction("Index");
            return View("Details", user);
        }

        // 
        // GET: /User/Delete
        public ActionResult Delete(Guid id)
        {
            User user = userDb.Users.Find(id);
            if (user == null)
                return RedirectToAction("Index");
            return View(user);
        }

        // 
        // POST: /User/Delete 
        [HttpPost]
        public RedirectToRouteResult Delete(Guid id, FormCollection collection)
        {
            User user = userDb.Users.Find(id);
            userDb.Users.Remove(user);
            userDb.SaveChanges();

            var id2 = from e in db.Enrollments
                      where e.UserName == user.UserName
                      select e.ID;

            foreach (var i in id2.ToList())
            {
                db.Enrollments.Remove(db.Enrollments.Find(i));
            }

            var ids = from pd in db.ProjectsData
                      where pd.UserName == user.UserName
                      select pd.ID;

            foreach (var id3 in ids.ToList())
            {
                var temp = db.ProjectsData.Find(id3);
                db.ProjectsData.Remove(temp);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // 
        // POST: /User/Delete 
        [HttpPost]
        public ActionResult DeleteAll()
        {
            foreach(User user in userDb.Users)
            {
                if(user.UserName!=User.Identity.Name)
                {
                    userDb.Users.Remove(user);

                    var id2 = from e in db.Enrollments
                              where e.UserName == user.UserName
                              select e.ID;

                    foreach (var i in id2.ToList())
                    {
                        db.Enrollments.Remove(db.Enrollments.Find(i));
                    }

                    var ids = from pd in db.ProjectsData
                              where pd.UserName == user.UserName
                              select pd.ID;

                    foreach (var id3 in ids.ToList())
                    {
                        var temp = db.ProjectsData.Find(id3);
                        db.ProjectsData.Remove(temp);
                    }
                }                
            }
            userDb.SaveChanges();
            
            return RedirectToAction("Index");
        }

        // 
        // GET: /User/Edit
        public ActionResult Edit(Guid id)
        {
            User user = userDb.Users.Find(id);
            if (user == null)
                return RedirectToAction("Index");

            ViewData["Roles"] = new SelectList(roles, "Value", "Text", user.Role);
            return View(user);
        }

        // 
        // POST: /User/Edit 
        [HttpPost]
        public ActionResult Edit(User model)
        {
            try
            {
                var edited = userDb.Users.Find(model.UserID);
                UpdateModel(edited);
                userDb.SaveChanges();
                return RedirectToAction("Details", new { id = model.UserID });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Edit Failure, see inner exception");
            }

            return View(model);
        }

        public ActionResult Import()
        {
            return View();
        }

        //import users from xml/excel file
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase ufile)
        {
            if (ufile != null)
            {
                if (ufile.ContentType == "text/xml" && ufile.ContentLength > 0)
                {
                    var document = new XmlDocument();
                    document.Load(ufile.InputStream);

                    // Data from xml file
                    XmlNodeList uNames = document.GetElementsByTagName("UserName");
                    XmlNodeList emails = document.GetElementsByTagName("Email");
                    XmlNodeList passwords = document.GetElementsByTagName("Password");
                    XmlNodeList uRoles = document.GetElementsByTagName("Role");

                    for (int i = 0; i < uNames.Count; ++i)
                    {
                        // Attempt to register the user
                        MembershipCreateStatus createStatus = MembershipService.CreateUser(uNames[i].InnerText, passwords[i].InnerText, emails[i].InnerText);

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            Roles.AddUserToRole(uNames[i].InnerText, uRoles[i].InnerText);
                        }
                        else
                            return RedirectToAction("Import", "Account");
                    }
                }
                else if (ufile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filePath = Path.Combine(HttpContext.Server.MapPath("../Uploads"), Path.GetFileName(ufile.FileName));
                    ufile.SaveAs(filePath);

                    var excel = new ExcelQueryFactory(filePath);
                    var users = from x in excel.Worksheet<RegisterModel>(0)
                                select x;

                    foreach (var u in users)
                    {
                        // Attempt to register the user
                        MembershipCreateStatus createStatus = MembershipService.CreateUser(u.UserName, u.Password, u.Email);

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            Roles.AddUserToRole(u.UserName, u.Role);
                        }
                        else
                            return RedirectToAction("Import", "Account");
                    }
                }
            }

            return RedirectToAction("Index", "Account");
        }

        public ActionResult DeleteAllUsers()
        {
            return View();
        }

        // **************************************
        // URL: /Account/Profile
        // **************************************

        [Authorize]
        public ActionResult Profile()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            ViewBag.CreationDate = Membership.GetUser().CreationDate;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Profile(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

    }
}
