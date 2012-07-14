using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectManager.Models;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NewsController : Controller
    {
        ProjectManagerEntities db = new ProjectManagerEntities();

        //
        // GET: /News/
        public ActionResult Index()
        {
            var news = from n in db.News
                       orderby n.Date descending
                       select n;

            return View(news.ToList());
        }

        //
        // GET: /News/Add
        public ActionResult Add()
        {
            ViewBag.Now = DateTime.Now;
            return View();
        }

        //
        // POST: /News/Add
        [HttpPost]
        public ActionResult Add(News newNews)
        {
            if (ModelState.IsValid)
            {
                db.News.Add(newNews);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(newNews);
            }
        }

        // 
        // GET: /News/Details
        public ActionResult Details(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
                return RedirectToAction("Index");
            return View("Details", news);
        }

        // 
        // GET: /News/Edit
        public ActionResult Edit(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
                return RedirectToAction("Index");
            return View(news);
        }

        // 
        // POST: /News/Edit 
        [HttpPost]
        public ActionResult Edit(News model)
        {
            try
            {
                var movie = db.News.Find(model.ID);

                UpdateModel(movie);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = model.ID });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Edit Failure, see inner exception");
            }

            return View(model);
        }

        // 
        // GET: /News/Delete
        public ActionResult Delete(int id)
        {
            News news = db.News.Find(id);
            if (news == null)
                return RedirectToAction("Index");
            return View(news);
        }

        // 
        // POST: /News/Delete 
        [HttpPost]
        public RedirectToRouteResult Delete(int id, FormCollection collection)
        {
            News news = db.News.Find(id);
            db.News.Remove(news);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        

    }
}
