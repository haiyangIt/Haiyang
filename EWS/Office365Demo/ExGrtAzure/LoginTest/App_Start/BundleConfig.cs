using System.Web;
using System.Web.Optimization;

namespace LoginTest
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

            bundles.Add(new ScriptBundle("~/bundles/bootstrapwizard").Include(
                      "~/Scripts/jquery.bootstrap.wizard.*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").
                Include("~/Scripts/jquery-ui*").
                Include("~/Scripts/mustache.js"));

            bundles.Add(new StyleBundle("~/bundles/jqueryuicss").
                IncludeDirectory("~/Content/themes/base", "*.css", true).
                IncludeDirectory("~/Content/themes/base", "*.png", true));

            //bundles.Add(new StyleBundle("~/bundles/jqueryuicss").
            //                IncludeDirectory("~/Content/themes/ui_bootstrap", "*.css", true).
            //                IncludeDirectory("~/Content/themes/ui_bootstrap", "*.png", true));

            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                    "~/Scripts/jquery.unobtrusive*"));

            bundles.Add(new ScriptBundle("~/bundles/arcserveutil").Include("~/Scripts/Restore/jquery_data_protect_util.js"));

            bundles.Add(new ScriptBundle("~/bundles/three-status-select").
                Include("~/Scripts/jquery.tristate.js").
                Include("~/Scripts/Restore/restore-selection.js"));

            bundles.Add(new ScriptBundle("~/bundles/three-status-select-nav").
                Include("~/Scripts/Restore/jquery-ui-customer-restore-nav.js"));

            bundles.Add(new ScriptBundle("~/bundles/customerMailbox").
                Include("~/Scripts/Restore/jquery-mailbox-detail.js").
                Include("~/Scripts/Restore/jquery-ui-customer-maildetail.js").
                Include("~/Scripts/Restore/jquery-ui-customer-catalogselection.js"));

            bundles.Add(new StyleBundle("~/bundles/customerMailboxcss").Include("~/Content/Restore/customernav.css"));

            bundles.Add(new ScriptBundle("~/bundles/customerPaginator").
                Include("~/Scripts/Restore/bootstrap-paginator.js"));

            bundles.Add(new StyleBundle("~/bundles/bootstrapselectcss").Include("~/Content/bootstrap-select.css"));

            bundles.Add(new ScriptBundle("~/bundles/bottstrapselect").Include("~/Scripts/bootstrap-select.js"));
        }
    }
}
