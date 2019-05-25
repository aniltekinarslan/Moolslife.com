using System.Web;
using System.Web.Optimization;

namespace MoolsPayment
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/stylesheets/styleguide-5.css",
                "~/Content/stylesheets/font_base64-0.css",
                "~/Content/stylesheets/components-34.css",
                "~/Content/stylesheets/pagesal-34.css",
                "~/Content/stylesheets/pagesmz-34.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-{version}.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            /*bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));
            */
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/jquery.carouFredSel-6.1.0.js",
                "~/Scripts/jquery.cslider.js",
                "~/Scripts/modernizr.custom.28468.js"));
         }
    }
}