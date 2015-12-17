using System.Web;
using System.Web.Optimization;

namespace Demo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                    "~/Scripts/jquery.unobtrusive*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").
               Include("~/Scripts/jquery-ui*").
               Include("~/Scripts/mustache.js"));

            bundles.Add(new StyleBundle("~/bundles/jqueryuicss").
                IncludeDirectory("~/Content/themes", "*.css", true).
                IncludeDirectory("~/Content/themes", "*.png", true));

            bundles.Add(new ScriptBundle("~/bundles/customerMailbox").Include("~/Scripts/Demo/restore-selection.js").Include("~/Scripts/Demo/jquery-ui-customer-mailboxes.js").
                Include("~/Scripts/Demo/jquery-mailbox-detail.js").
                Include("~/Scripts/Demo/jquery-ui-customer-mailbox-nav.js").
                Include("~/Scripts/Demo/jquery-ui-customer-restore-nav.js").
                Include("~/Scripts/Demo/jquery-ui-customer-maildetail.js"));

            bundles.Add(new StyleBundle("~/bundles/customerMailboxcss").Include("~/Content/customernav.css"));

            bundles.Add(new ScriptBundle("~/bundles/customerPaginator").
                Include("~/Scripts/Demo/bootstrap-paginator.js"));

            
        }
    }
}
