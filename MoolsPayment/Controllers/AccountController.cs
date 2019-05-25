using MoolsPayment.DAL;
using MoolsPayment.Models;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.WebPages;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Resources;
using System.Data.Entity;
using System.Data;
using System.Text;

namespace MoolsPayment.Controllers
{
    [CustomAuthorize(Roles = "User")]
    public class AccountController : BaseController
    {
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (IsAuthenticated())
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl = "")
        {
            if (IsAuthenticated())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = Home.InformationTryAgain;
                return View();
            }

            var password = GetHashedPassword(model.password);
            var user = _db.Accounts.FirstOrDefault(u => u.Username.ToLower() == model.username.ToLower() || u.Email.ToLower() == model.username.ToLower());
            if (user == null)
            {
                ViewBag.ErrorMessage = Home.CantFindAccount;
                return View(model);
            }

            bool status = user.Password == password;

            LoginLog log = new LoginLog()
            {
                Date = DateTime.Now,
                Ip = RealIpAddress(),
                Status = status,
                SessionID = System.Web.HttpContext.Current.Session.SessionID,
                AccountID = user.AccountID,
                Balance1 = user.Balance,
                Balance2 = _db.Payments.Where(p => p.AccountID == user.AccountID && p.Status == 1).Sum(p => (decimal?)p.Amount)
            };

            _db.LoginLog.Add(log);
            _db.SaveChanges();

            if (status == false)
            {
                ViewBag.ErrorMessage = Home.InformationInvalidEmailOrPassword;
                return View(model);
            }

            if (user.IsActive == false)
            {
                ViewBag.ErrorMessage = Home.InformationUnActivated;
                return View(model);
            }

            CustomPrincipalSerializeModel serializeModel = new CustomPrincipalSerializeModel();

            serializeModel.AccountID = user.AccountID;
            serializeModel.Email = user.Email;
            serializeModel.Username = user.Username;
            serializeModel.Password = user.Password;
            serializeModel.Name = user.Name;
            serializeModel.Balance = user.Balance;
            serializeModel.DealerCode = user.DealerCode;
            serializeModel.IsActive = user.IsActive;
            serializeModel.IPAddress = RealIpAddress();
            serializeModel.roles = user.Role;

            string userData = JsonConvert.SerializeObject(serializeModel);
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(60), false, userData);

            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);

            //So that the user can be referred back to where they were when they click logon
            if (string.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null)
                returnUrl = Server.UrlEncode(Request.UrlReferrer.PathAndQuery);

            if (!Url.IsLocalUrl(returnUrl) || string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";

            ViewBag.ReturnURL = returnUrl;

            return Redirect(returnUrl);
        }
        public ActionResult LoginLog()
        {
            var logList = _db.LoginLog.Where(l => l.AccountID == User.AccountID).OrderByDescending(t => t.LoginID).ToList();
            return View(logList);
        }
        public ActionResult AccountTransactions()
        {
            var transactionList = _db.AccountTransaction.Where(l => l.AccountID == User.AccountID).OrderByDescending(t => t.AccountTransactionID).ToList();
            return View(transactionList);
        }
        public ActionResult ApiSettings()
        {
            var settings = _db.Accounts.FirstOrDefault(a => a.AccountID == User.AccountID);
            return View(settings);
        }
        public ActionResult GeneratedLinks()
        {
            var links = _db.Links.Where(a => a.AccountID == User.AccountID).OrderByDescending(l => l.LinkID);
            return View(links);
        }

        public ActionResult PaymentMethods()
        {
            var methods = _db.Methods.Where(m => m.IsActive == true && (m.Methods2.IsActive == true || m.MainMethodID == null)).OrderBy(m => m.Name).ThenBy(m => m.Currency);
            var methodPreferences = _db.MethodPreference.Where(a => a.AccountID == User.AccountID);

            ViewBag.methodPreferences = methodPreferences;

            return View(methods);
        }
        public ActionResult ChangeMethodPreference(string id)
        {
            var methodId = Convert.ToInt32(id);
            var method = _db.Methods.FirstOrDefault(m => m.MethodID == methodId);

            if (method == null)
                return RedirectToAction("PaymentMethods");

            var methodPreference = _db.MethodPreference.FirstOrDefault(mP => mP.AccountID == User.AccountID && mP.MethodID == methodId);
            try
            {
                if (methodPreference != null)
                    _db.MethodPreference.Remove(methodPreference);
                else
                {
                    MethodPreference m = new MethodPreference()
                    {
                        AccountID = User.AccountID,
                        MethodID = methodId
                    };

                    _db.MethodPreference.Add(m);
                }

                _db.SaveChanges();
            }
            catch (Exception)
            {
                AddErrorLog(null, "paymentpreference error", "User: " + User.AccountID + "  /MethodID=" + id);
            }

            return RedirectToAction("PaymentMethods");
        }



        public ActionResult EditApiSettings()
        {
            var settings = _db.Accounts.FirstOrDefault(a => a.AccountID == User.AccountID);
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditApiSettings(Accounts model)
        {
            var settings = _db.Accounts.FirstOrDefault(a => a.AccountID == User.AccountID);
            if (settings == null || !ModelState.IsValid)
                return View();

            settings.NotifyUrl = model.NotifyUrl;
            settings.ReturnUrlSuccess = model.ReturnUrlSuccess;
            settings.ReturnUrlFailed = model.ReturnUrlFailed;
            settings.PaymentCancelRedirectUrl_ = model.PaymentCancelRedirectUrl_;
            settings.LogoImageUrl_ = model.LogoImageUrl_;

            _db.Accounts.Attach(settings);
            _db.Entry(settings).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("ApiSettings");
        }




        //
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = Home.InformationTryAgain;
                return View();
            }


            var pass = GetHashedPassword(model.CurrentPassword);

            if (pass != User.Password)
            {
                ViewBag.ErrorMessage = Home.InformationInvalidEmailOrPassword;
                return View();
            }

            string cryptpass = GetHashedPassword(model.NewPassword);
            if (cryptpass == "")
            {
                ViewBag.ErrorMessage = Home.InformationTryAgain;
                return View(model);
            }

            var user = _db.Accounts.Where(u => u.Email == User.Email);
            if (user.FirstOrDefault() == null || user.Count() > 1)
            {
                ViewBag.ErrorMessage = Home.InformationEmailNotFoundorMultiple;
                return View(model);
            }

            user.Single().Password = cryptpass;
            _db.Accounts.Attach(user.Single());
            _db.Entry(user.Single()).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();


            Site_ChangePasswordLog change = new Site_ChangePasswordLog()
            {
                AccountID = user.Single().AccountID,
                Email = user.Single().Email,
                Ip = RealIpAddress(),
                Date = DateTime.Now,
                Status = true
            };

            _db.Site_ChangePasswordLog.Add(change);
            _db.SaveChanges();
            User.Password = cryptpass;

            SendEmail(User.Email, EmailTypes.ChangePassword);

            ViewBag.SuccessMessage = Home.InformationPasswordChanged;
            FormsAuthentication.SignOut();
            return View();
        }


        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            if (IsAuthenticated())
                return RedirectToAction("Index", "Home");

            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (IsAuthenticated())
                    return RedirectToAction("Index", "Home");

                if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = Home.InformationTryAgain;
                    return View();
                }

                var user = _db.Accounts.Where(u => u.Email.ToLower() == model.Email.ToLower());
                if (user.FirstOrDefault() == null || user.Count() > 1)
                {
                    ViewBag.ErrorMessage = Home.InformationEmailNotFoundorMultiple;
                    return View(model);
                }

                var usersingle = user.FirstOrDefault();

                var hashed = CreateNewDbHash(usersingle.Email);

                Site_ForgotPasswordLog forgot = new Site_ForgotPasswordLog()
                {
                    AccountID = usersingle.AccountID,
                    Email = usersingle.Email,
                    Ip = RealIpAddress(),
                    Date = DateTime.Now,
                    Status = false,
                    Hash = hashed
                };

                _db.Site_ForgotPasswordLog.Add(forgot);
                _db.SaveChanges();

                SendEmail(model.Email, EmailTypes.ForgotPassword, hashed);

                ViewBag.SuccessMessage = Home.InformationForgotpasswordMailSent;
                return View();

            }
            catch (DbEntityValidationException e)
            {

                foreach (var eve in e.EntityValidationErrors)
                {
                    Response.Write(string.Format("Entity türü \"{0}\" şu hatalara sahip \"{1}\" Geçerlilik hataları:", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Response.Write(string.Format("- Özellik: \"{0}\", Hata: \"{1}\"", ve.PropertyName, ve.ErrorMessage));
                    }
                    Response.End();
                }
                return View();
            }
        }


        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string id)
        {
            if (IsAuthenticated())
                return RedirectToAction("Index", "Home");

            var userID = _db.Site_ForgotPasswordLog.Where(u => u.Hash == id && u.Status == false);
            var userIDfirst = userID.FirstOrDefault();
            if (userIDfirst == null || userID.Count() > 1)
            {
                ViewBag.ErrorMessage = Home.InformationInvalidOrUsedHashCode;
                return View();
            }

            var user = _db.Accounts.Where(u => u.Email == userIDfirst.Email);
            if (user.FirstOrDefault() == null || user.Count() > 1)
            {
                ViewBag.ErrorMessage = Home.InformationEmailNotFoundorMultiple;
                return View();
            }

            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Email = userIDfirst.Email;
            model.Hash = id;

            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (IsAuthenticated())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = Home.InformationTryAgain;
                return View();
            }

            var userID = _db.Site_ForgotPasswordLog.Where(u => u.Hash == model.Hash && u.Status == false);
            var userIDfirst = userID.FirstOrDefault();
            if (userIDfirst == null || userID.Count() > 1)
            {
                ViewBag.ErrorMessage = Home.InformationInvalidOrUsedHashCode;
                return View();
            }

            var user = _db.Accounts.Where(u => u.Email == userIDfirst.Email);
            if (user.FirstOrDefault() == null || user.Count() > 1)
            {
                ViewBag.ErrorMessage = Home.InformationEmailNotFoundorMultiple;
                return View(model);
            }

            var pass = GetHashedPassword(model.NewPassword);

            user.Single().Password = pass;
            _db.Accounts.Attach(user.Single());
            _db.Entry(user.Single()).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();

            userIDfirst.Date = DateTime.Now;
            userIDfirst.Status = true;
            _db.Site_ForgotPasswordLog.Attach(userIDfirst);
            _db.Entry(userIDfirst).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();

            SendEmail(user.Single().Email, EmailTypes.AfterForgotPassword);

            ViewBag.SuccessMessage = Home.InformationPasswordChanged;
            return View();
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult PaymentList(string id, string page)
        {
            var paymentList = _db.Payments.Where(p => p.AccountID == User.AccountID).OrderByDescending(p => p.PaymentID).Take(250).ToList();

            if (!id.IsNullOrWhiteSpace())
            {
                if (id == "Rejected")
                    paymentList = paymentList.Where(p => p.Status == (int)PaymentStatus.Rejected).ToList();
                else if (id == "Waiting")
                    paymentList = paymentList.Where(p => p.Status == (int)PaymentStatus.Waiting).ToList();
                else if (id == "Approved")
                    paymentList = paymentList.Where(p => p.Status == (int)PaymentStatus.Approved).ToList();
                ViewBag.status = id;
            }

            return View(paymentList);
        }


        public ActionResult GeneratedEpins(string id)
        {
            var epinList = _db.MoolsEpin.Where(e => e.CreatedAccountID == User.AccountID).OrderByDescending(e=>e.MoolsEpinID).ToList();

            var totalPage = (epinList.Count() / PerPage) + 1;
            ViewBag.totalPage = totalPage;

            if (id.IsNullOrWhiteSpace())
                epinList = epinList.Take(PerPage).ToList();
            else
                epinList = epinList.Skip(PerPage * (Convert.ToInt32(id) - 1)).Take(PerPage).ToList();

            return View(epinList);
        }

        public ActionResult CreateNewEpin(string id)
        {
            ViewBag.gameProductList = _db.MoolsEpinGameProducts.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult CreateNewEpin()
        {
            ViewBag.gameProductList = _db.MoolsEpinGameProducts.ToList();

            var quantity = Request.Form["Quantity"] ?? "";
            var GameProductID = Request.Form["GameProductID"] ?? "";
            var isBonus = Request.Form["isBonus"] ?? "";

            if (!quantity.IsInt())
            {
                ViewBag.ErrorMessage = "Invalid Quantity";
                return View();
            }

            if (!isBonus.IsInt())
            {
                ViewBag.ErrorMessage = "Invalid Bonus Value (Must be 1 or 0)";
                return View();
            }

            if (!GameProductID.IsInt())
            {
                ViewBag.ErrorMessage = "Invalid Product";
                return View();
            }

            var productId = Convert.ToInt32(GameProductID);
            var q = Convert.ToInt32(quantity);
            var product = _db.MoolsEpinGameProducts.FirstOrDefault(p=>p.MoolsEpinGameProductID == productId);

            try
            {
                var user = _db.Accounts.First(a => a.AccountID == User.AccountID);

                var netAmount = Convert.ToDecimal(quantity)*product.Cost;
                var newAmount = user.UsableBalance - netAmount;

                if (newAmount < 0)
                {
                    ViewBag.ErrorMessage = "EPIN oluşturmak için Kullanılabilir Bakiyeniz yetersiz.";
                    return View();
                }

                /*var transaction = new AccountTransaction
                {
                    AccountID = user.AccountID,
                    Date = DateTime.Now,
                    PaymentID = 811,
                    OldBalance = user.UsableBalance,
                    NewBalance = newAmount,
                    Usable = true
                };*/

                user.UsableBalance = newAmount;
                _db.Accounts.Attach(user);
                _db.Entry(user).State = EntityState.Modified;
                //_db.AccountTransaction.Add(transaction);


                for (int i = 0; i < q; i++)
                {
                    var epin = new MoolsEpin
                    {
                        Amount = product.Cost,
                        Code = CreateEpinCode(GameProductID),
                        Currency = product.Currency,
                        CreatedAccountID = User.AccountID,
                        GameProductID = productId,
                        CreatedDate = DateTime.Now,
                        IsBonus = (isBonus == "1")
                    };
                    _db.MoolsEpin.Add(epin);
                }

                _db.SaveChanges();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Error. Please contact to MoolsLife Administrator";
                return View();
            }

           return RedirectToAction("GeneratedEpins","Account");
        }


        [HttpGet]
        public ActionResult PaymentQuery(string id)
        {
            int paymentId = 0;
            if (id.IsInt())
                paymentId = Convert.ToInt32(id);

            var payment = _db.Payments.Where(p => p.AccountID == User.AccountID).FirstOrDefault(p => p.ApiCode == id || p.PaymentID == paymentId || p.Links1.Token == id);

            return View(payment);
        }
    }
}