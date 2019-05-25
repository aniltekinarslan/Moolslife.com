using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MoolsPayment.Controllers;

namespace MoolsPayment
{
    public class RouteConfig
    {
        public class RootRouteConstraint<T> : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var rootMethodNames = typeof(T).GetMethods().Select(x => x.Name.ToLower());
                return rootMethodNames.Contains(values["action"].ToString().ToLower());
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Home", "{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new { isMethodInHomeController = new RootRouteConstraint<HomeController>() });

            routes.MapRoute("Account", "{action}/{id}/{page}", new { controller = "Account", action = "Index", id = UrlParameter.Optional, page = UrlParameter.Optional }, new { isMethodInAccountController = new RootRouteConstraint<AccountController>() });

            routes.MapRoute("ShowMethods", "{action}/{token}", new { controller = "ShowMethods", action = "ShowMethods", token = UrlParameter.Optional}, new { isMethodInShowMethodsController = new RootRouteConstraint<ShowMethodsController>() });

            routes.MapRoute("Notify", "Payment/{controller}/{action}/{id}", new { controller = "Notify", action = "Index", id = UrlParameter.Optional });
        }
    }
}