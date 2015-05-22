using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Workshop.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Authorize()
        {
            if (Request.QueryString.AllKeys.Contains("error"))
            {
                ViewData.Add("error", Request.Url.Query);
                ViewData.Add("error_type", "authorization_failed");
                return RedirectToAction("Index", "Home");
            }

            string code = Request.QueryString["code"].Split('#')[0];

            var url = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

            // request to Facebook.    
            var client = new WebClient();
            var accessToken = "";
            var expires = DateTime.Now;

            try
            {
                var result = client.DownloadString(string.Format(url,
                    ConfigurationManager.AppSettings["fb:key"],
                    ConfigurationManager.AppSettings["fb:redirectUri"],
                    ConfigurationManager.AppSettings["fb:secret"],
                    code));

                accessToken = HttpUtility.ParseQueryString(result)["access_token"];
            }
            catch (WebException webex)
            {
                var errorReader = new StreamReader(webex.Response.GetResponseStream());
                ViewData.Add("error_type", "accesstoken_failed");
                ViewData.Add("error", errorReader.ReadToEnd());
                errorReader.Close();

                return RedirectToAction("Index", "Home");
            }

            // get user information.
            var profileClient = new WebClient();
            var profileString = profileClient.DownloadString("https://graph.facebook.com/me?access_token=" + accessToken);

            // authenticated.
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                 (JObject.Parse(profileString)).SelectToken("name").Value<string>(), 
                 DateTime.Now,
                 DateTime.Now.AddHours(4), 
                 false,
                 profileString);

            string ticketStr = FormsAuthentication.Encrypt(ticket);

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Value = ticketStr;
            cookie.Expires = ticket.Expiration;
            Response.Cookies.Add(cookie);

            Request.RequestContext.HttpContext.User =
                new GenericPrincipal(new FormsIdentity(ticket), "candidate".Split(','));

            return RedirectToAction("Index", "Home");
        }
    }
}