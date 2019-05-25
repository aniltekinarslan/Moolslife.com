using System;
using System.Web.Http;

namespace MoolsPayment
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { contoller = "api", action = "BankPayment", id = RouteParameter.Optional });
        }
    }
}
