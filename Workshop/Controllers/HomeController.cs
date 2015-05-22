using Microsoft.Security.Application;
using Newtonsoft.Json.Linq;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Workshop.Models;

namespace Workshop.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index(Guid? id, int? Page = 1)
        {
            using (WorkshopDb db = new WorkshopDb())
            {
                IQueryable<GuestBook> query = null;

                if (id == null)
                {
                    query = db.GuestBooks.Where(i => i.IdReply == Guid.Empty);
                    var pageQuery = query.OrderByDescending(i => i.DateCreated).ToPagedList(Page.Value, 5);

                    if (Request.IsAjaxRequest())
                        return PartialView("_ThreadView", pageQuery);
                    else
                        return View("Thread", pageQuery);
                }
                else
                {
                    query = db.GuestBooks.Where(i => i.Id == id.Value);
                    ViewBag.PostId = id.Value.ToString();

                    if (query.Any())
                        return View("ThreadReplyView", query.First());
                    else
                        return HttpNotFound();
                }
            }
        }

        // implement Partial View to display replies.
        public ActionResult IndexReply(Guid PostId)
        {
            using (WorkshopDb db = new WorkshopDb())
            {
                if (PostId == null)
                    return new EmptyResult();
                else
                {
                    var query = db.GuestBooks
                        .Where(i => i.IdReply == PostId)
                        .OrderByDescending(i => i.DateCreated);

                    return PartialView("_RepliesView", query.ToList());
                }
            }
        }

        #region Create Message and Reply

        public ActionResult Create(Guid? id = null)
        {
            if (id == null)
                ViewBag.PostId = "";
            else
                ViewBag.PostId = id.Value.ToString();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create(GuestBook Model, Guid? id = null)
        public ActionResult Create(Guid? id = null, string Magic = null)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View();
            //}

            var Model = new GuestBook();
            var identity = Request.RequestContext.HttpContext.User.Identity as FormsIdentity;
            var o = JObject.Parse(identity.Ticket.UserData);

            // create data.
            using (WorkshopDb db = new WorkshopDb())
            {
                if (id == null)
                {
                    Model.Id = Guid.NewGuid();
                    Model.DateCreated = DateTime.Now;
                    Model.Subject = Request.Form["Subject"];
                    Model.Name = o.Property("name").ToObject(typeof(string)).ToString();
                    Model.Email = o.Property("email").ToObject(typeof(string)).ToString();
                    Model.Body = Sanitizer.GetSafeHtmlFragment(Request.Unvalidated.Form["Body"]);

                    db.GuestBooks.Add(Model);
                }
                else
                {
                    Model.Id = Guid.NewGuid();
                    Model.IdReply = id.Value;
                    Model.Subject = Request.Form["Subject"];
                    Model.Name = o.Property("name").ToObject(typeof(string)).ToString();
                    Model.Email = o.Property("email").ToObject(typeof(string)).ToString();
                    Model.Body = Sanitizer.GetSafeHtmlFragment(Request.Unvalidated.Form["Body"]);
                    Model.DateCreated = DateTime.Now;

                    db.GuestBooks.Add(Model);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Edit Message and Reply

        public ActionResult Edit(Guid id)
        {
            using (WorkshopDb db = new WorkshopDb())
            {
                var query = db.GuestBooks.Where(i => i.Id == id);

                if (!query.Any())
                    return HttpNotFound();

                return View("Edit", query.First());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit(GuestBook Model, Guid id)
        public ActionResult Edit(Guid id, string Magic = null)
        {
            //if (!ModelState.IsValid)
            //{
            //    if (string.IsNullOrEmpty(Model.Subject) ||
            //        string.IsNullOrEmpty(Model.Body))
            //        return View();
            //}

            if (string.IsNullOrEmpty(Request.Form["Subject"]) ||
                string.IsNullOrEmpty(Request.Unvalidated.Form["Body"]))
                return View();

            // create data.
            using (WorkshopDb db = new WorkshopDb())
            {
                // query item.
                var query = db.GuestBooks.Where(i => i.Id == id);

                if (!query.Any())
                    return HttpNotFound();

                var dbModel = query.First();

                // modify data.
                dbModel.Subject = Request.Form["Subject"];
                dbModel.Body = Sanitizer.GetSafeHtmlFragment(Request.Unvalidated.Form["Body"]);

                // change tracking service lets it valid.
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Delete Message and Reply

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            // create data.
            using (WorkshopDb db = new WorkshopDb())
            {
                // query item.
                var query = db.GuestBooks.Where(i => i.Id == id);

                if (!query.Any())
                    return HttpNotFound();

                var dbModel = query.First();
                db.GuestBooks.Remove(dbModel);

                // change tracking service lets it valid.
                db.SaveChanges();
            }

            return new EmptyResult();
        }

        #endregion

        #region Login and Logout

        public ActionResult Login()
        {
            string redirectUrl = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope=email";
            return Redirect(string.Format(redirectUrl,
                ConfigurationManager.AppSettings["fb:key"],
                ConfigurationManager.AppSettings["fb:redirectUri"]));
        }

        public ActionResult Logout()
        {
            // revoke cookie
            FormsAuthentication.SignOut();

            // overwrite exist cookie to force logout.
            HttpCookie cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName, FormsAuthentication.FormsCookiePath);
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);

            Request.RequestContext.HttpContext.User = null;

            return RedirectToAction("Index");
        }

        #endregion

        #region Change Layout

        public ActionResult ChangeLayout(string type)
        {
            HttpCookie cookie = Request.Cookies["layout"];

            if (cookie == null)
                cookie = new HttpCookie("layout");

            cookie.Value = type;

            Response.Cookies.Add(cookie);

            return RedirectToAction("Index");
        }

        #endregion
    }
}