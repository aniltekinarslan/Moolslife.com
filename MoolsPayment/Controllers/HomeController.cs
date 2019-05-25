using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MoolsPayment.Controllers
{
    [Authorize(Roles = "User")]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var user = _db.Accounts.First(a => a.Email == User.Email);
            var payments = _db.Payments.Where(p => p.AccountID == user.AccountID);

            ViewBag.CurrentBalance = user.Balance;
            ViewBag.CurrentUsableBalance = user.UsableBalance;

            ViewBag.WaitingPayments = payments.Where(p => p.Status == 0).Sum(p => (decimal?)p.Amount);
            ViewBag.WaitingPaymentsCount = payments.Count(p => p.Status == 0);

            ViewBag.ApprovedPayments = payments.Where(p => p.Status == 1).Sum(p => (decimal?)p.Amount);
            ViewBag.ApprovedPaymentsCount = payments.Count(p => p.Status == 1);

            ViewBag.RejectedPayments = payments.Where(p => p.Status == -1).Sum(p => (decimal?)p.Amount);
            ViewBag.RejectedPaymentsCount = payments.Count(p => p.Status == -1);

            return View();
        }

        [AllowAnonymous]
        public ActionResult ChangeCulture(string lang, string returnUrl)
        {
            if(lang != "en" && lang != "tr")
                lang = "tr";

            Session["Culture"] = new CultureInfo(lang);
            return Redirect(returnUrl);
        }
    }
}