using Newtonsoft.Json;
using MoolsPayment.DAL;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Net;

namespace MoolsPayment
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session == null)
                return;

            var cultureInfo = (CultureInfo) Session["Culture"];

            if (cultureInfo == null)
            {
                var languageName = "en";
                if (Thread.CurrentThread.CurrentUICulture.Name.Contains("tr") ||
                    Thread.CurrentThread.CurrentUICulture.Name.Contains("TR"))
                    languageName = "tr";

                cultureInfo = new CultureInfo(languageName);
                Session["Culture"] = cultureInfo;
            }
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
        }

        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var serializeModel = JsonConvert.DeserializeObject<CustomPrincipalSerializeModel>(authTicket.UserData);
                var newUser = new CustomPrincipal(authTicket.Name);

                newUser.AccountID = serializeModel.AccountID;
                newUser.Email = serializeModel.Email;
                newUser.Username = serializeModel.Username;
                newUser.Password = serializeModel.Password;
                newUser.Name = serializeModel.Name;
                newUser.Balance = serializeModel.Balance;
                newUser.DealerCode = serializeModel.DealerCode;
                newUser.IsActive = serializeModel.IsActive;
                newUser.IPAddress = serializeModel.IPAddress;
                newUser.roles = serializeModel.roles;

                HttpContext.Current.User = newUser;
            }
        }
    }
}