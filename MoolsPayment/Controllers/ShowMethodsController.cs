using System;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.Ajax.Utilities;
using MoolsPayment.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace MoolsPayment.Controllers
{
    public class ShowMethodsController : BaseController
    {
        [HttpGet]
        public ActionResult ShowMethods(string token)
        {
            Session["Token"] = token;

            var check = Check();
            if (check.ToString() == "-2")
            {
                ReleaseSessions();
                ViewBag.NotFoundUser = GetErrorMessage("3", false);
                return View();
            }

            if (check.ToString() == "-1")
            {
                ReleaseSessions();
                ViewBag.NotFoundUser = GetErrorMessage("6", false);
                return View();
            }

            if (check.ToString() == "0")
            {
                ViewBag.NotFoundUser = GetErrorMessage("99", false);
                return View();
            }

            var link = ((Links)check);


            ViewBag.Methods = _db.Methods.Where(b => b.MethodID > 0 && b.IsActive == true && (b.Currency == link.Currency || b.Currency == (short)PaymentCurrency.ALL) && !_db.MethodPreference.Any(bp => bp.AccountID == link.AccountID && bp.MethodID == b.MethodID)).ToList();
            ViewBag.product_Name = link.product_Name;
            ViewBag.product_ImageUrl = link.product_ImageUrl;

            ViewBag.LogoImageUrl = link.Accounts.LogoImageUrl_;
            ViewBag.CompanyName = link.Accounts.Name;
            ViewBag.Currency = link.PaymentCurrency.Name;
            ViewBag.Amount = link.Amount == 0 ? 0 : link.Amount;
            ViewBag.customer_Identity = link.customer_Identity;
            ViewBag.customer_Phone = link.customer_Phone;
            ViewBag.customer_PhoneCountryCode = link.customer_PhoneCountryCode;
            ViewBag.customer_NameSurname = link.customer_NameSurname;
            ViewBag.customer_Email = link.customer_Email;

            var MoolsEpinPreference = _db.MoolsEpinPreference.FirstOrDefault(t => t.AccountID == link.AccountID);

            if (MoolsEpinPreference != null)
            {
                ViewBag.MoolsEpinImageUrl = MoolsEpinPreference.ImageUrl;
                ViewBag.MoolsEpinName = MoolsEpinPreference.Name;
            }



            var errorCode = (Session["errorCode"] ?? "").ToString();
            if (!errorCode.IsNullOrWhiteSpace())
            {
                try
                {
                    string[] codes = DecryptBasic(errorCode).Split(',');
                    ViewBag.errorMessage = codes.Aggregate<string, string>(null,
                        (current, e) => current + GetErrorMessage(e));
                }
                catch (Exception)
                {
                }
            }

            ViewBag.clickItem = (Session["clickItem"] ?? "").ToString();

            return View();
        }

        [HttpPost] // test
        public ActionResult GenerateLink()
        {
            try
            {
                string DealerCode = Request.Form["DealerCode"] ?? "";
                string Currency = Request.Form["Currency"] ?? "";
                string Amount = (Request.Form["Amount"] ?? "");
                string OrderID = Request.Form["OrderID"] ?? "";
                string product_Code = Request.Form["product_Code"] ?? "";
                string product_Name = Request.Form["product_Name"] ?? "";
                string product_ImageUrl = Request.Form["product_ImageUrl"] ?? "";
                string IpAddress = Request.Form["IpAddress"] ?? "";
                string customer_Identity = Request.Form["customer_Identity"] ?? "";
                string customer_Phone = Request.Form["customer_Phone"] ?? "";
                string customer_PhoneCountryCode = Request.Form["customer_PhoneCountryCode"] ?? "";
                string customer_NameSurname = Request.Form["customer_NameSurname"] ?? "";
                string customer_Email = Request.Form["customer_Email"] ?? "";
                string test = Request.Form["test"] ?? "";


                if (RealIpAddress() == "::1" || RealIpAddress() == "127.0.0.1")
                {
                    DealerCode = "z51fsj34";
                    //Currency = "1";
                    Amount = "100";
                    OrderID = "orders";
                    product_Code = "product_Code";
                    product_Name = "productnames";
                    IpAddress = RealIpAddress();
                    customer_Identity = "24130";
                    customer_Phone = "5321648600";
                    customer_NameSurname = "ANIL TEKİNARSLAN";
                    customer_Email = "aniltekinarslan@msn.com";
                    customer_PhoneCountryCode = "90";
                }


                var u = CheckIsValidUser(DealerCode);
                if (!(u is Accounts))
                    return ResponseWrite(u);
                var user = ((Accounts)u);


                // IpAddress
                if (IpAddress.IsNullOrWhiteSpace())
                    return ResponseWrite(GetErrorMessage("151", false) + " / IpAddress=");
                if (IpAddress.Length > 150)
                    return ResponseWrite(GetErrorMessage("152", false) + " / IpAddress= " + IpAddress + " / IpAddressLength=" + IpAddress.Length);

                //Amount
                if (!Amount.IsInt() || Convert.ToInt32(Amount) <= 0)
                    return ResponseWrite(GetErrorMessage("111", false) + " / Amount=" + Amount);
                if (Amount.Length > 8)
                    return ResponseWrite(GetErrorMessage("112", false) + " / Amount=" + Amount + " / AmountLength=" + Amount.Length);


                // customer_Identity
                if (!customer_Identity.IsNullOrWhiteSpace())
                {
                    if (!customer_Identity.IsDecimal())
                        return ResponseWrite(GetErrorMessage("113", false) + " / customer_Identity=" + customer_Identity);
                    if (customer_Identity.Length != 11 && customer_Identity.Length != 5)
                        return ResponseWrite(GetErrorMessage("114", false) + " / customer_Identity=" + customer_Identity + " / customer_IdentityLength=" + customer_Identity.Length);
                }

                // customer_Phone
                if (!customer_Phone.IsNullOrWhiteSpace())
                {
                    if (customer_PhoneCountryCode.Length > 5 || customer_PhoneCountryCode.Length < 1)
                        return ResponseWrite(GetErrorMessage("115", false) + " / customer_PhoneCountryCode=" + customer_PhoneCountryCode + " / customer_PhoneCountryCodeLength=" + customer_PhoneCountryCode.Length);

                    if (customer_Phone.IsNullOrWhiteSpace())
                        return ResponseWrite(GetErrorMessage("116", false) + " / customer_Phone=");

                    if (customer_PhoneCountryCode == "90")
                    {
                        if (!customer_Phone.IsDecimal())
                            return ResponseWrite(GetErrorMessage("117", false) + " / customer_Phone=" + customer_Phone);
                        if (customer_Phone.Length != 10)
                            return ResponseWrite(GetErrorMessage("118", false) + " / customer_Phone=" + customer_Phone + " / customer_PhoneLength=" + customer_Phone.Length);
                    }
                }

                // customer_NameSurname
                if (!customer_NameSurname.IsNullOrWhiteSpace())
                {
                    if (!customer_NameSurname.Contains(" "))
                        return ResponseWrite(GetErrorMessage("119", false) + " / customer_NameSurname=" + customer_NameSurname);
                    if (customer_NameSurname.Length < 6)
                        return ResponseWrite(GetErrorMessage("120", false) + " / customer_NameSurname=" + customer_NameSurname + " / customer_NameSurnameLength=" + customer_NameSurname.Length);
                }

                // customer_NameSurname
                /*if (!customer_Email.IsNullOrWhiteSpace())
                {
                    if (!customer_Email.Contains("@") || !customer_Email.Contains(".") || customer_Email.Contains(" "))
                        return ResponseWrite(GetErrorMessage("121", false) + " / customer_Email=" + customer_Email);
                }*/


                // Currency
                if (!Currency.IsNullOrWhiteSpace())
                {
                    if (Currency != PaymentCurrency.TRY.ToString() && Currency != PaymentCurrency.USD.ToString() && Currency != PaymentCurrency.EUR.ToString())
                        return ResponseWrite(GetErrorMessage("124", false) + " / Currency=" + Currency);
                }
                else
                    Currency = "TRY";

                short currency = 0;
                if (Currency == "TRY")
                    currency = (short)PaymentCurrency.TRY;
                else if (Currency == "USD")
                    currency = (short)PaymentCurrency.USD;
                else if (Currency == "EUR")
                    currency = (short)PaymentCurrency.EUR;


                var newLink = new Links
                {
                    AccountID = user.AccountID,
                    Date = DateTime.Now,
                    Amount = (Convert.ToDecimal(Amount) / Convert.ToDecimal(100)),
                    OrderID = OrderID,
                    product_Code = product_Code,
                    product_Name = product_Name,
                    product_ImageUrl = product_ImageUrl,
                    GeneratedDate = DateTime.Now,
                    Token = "L" + CreateApiCode(DealerCode, user.AccountID, DateTime.Now),
                    Currency = currency,
                    IpAddress = IpAddress,
                    IsDead = false,
                    PaymentID = null,
                    customer_Identity = customer_Identity,
                    customer_Phone = customer_Phone,
                    customer_PhoneCountryCode = customer_PhoneCountryCode,
                    customer_NameSurname = customer_NameSurname,
                    customer_Email = customer_Email
                };

                _db.Links.Add(newLink);
                _db.SaveChanges();

                if (test == "1")
                    return ResponseWrite("Token:" + newLink.Token + " - Informations: DealerCode=" + DealerCode + "&Currency=" + Currency + "&Amount=" + Amount + "&OrderID=" + OrderID + "&product_Code=" + product_Code + "&product_Name=" + product_Name + "&product_ImageUrl=" + product_ImageUrl + "&IpAddress=" + IpAddress + "&customer_Identity=" + customer_Identity + "&customer_Phone=" + customer_Phone + "&customer_PhoneCountryCode=" + customer_PhoneCountryCode + "&customer_NameSurname=" + customer_NameSurname + "&customer_Email=" + customer_Email);
                else
                    return ResponseWrite("Token:" + newLink.Token);
            }
            catch (Exception e)
            {
                return ResponseWrite("Error:" + e.Message);
            }
        }


        [HttpGet]
        public ActionResult Success()
        {
            ReleaseSessions();
            return View();
        }

        [HttpGet]
        public ActionResult Failed()
        {
            ReleaseSessions();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Paypal()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");

            var errorMessage = "";

            amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? amount.Replace(",", ".") : amount;

            // amount
            if (amount.IsNullOrWhiteSpace() || !amount.IsDecimal() || Convert.ToDecimal(amount) <= 0)
                errorMessage += "111" + ",";
            if (amount.Length > 8)
                errorMessage += "112" + ",";

            if (errorMessage.IsNullOrWhiteSpace())
            {
                var payment = InsertNewPayment(link, Convert.ToDecimal(amount), PaymentTypes.Paypal, MethodIDs.Paypal, null, null, null);

                if (payment != null)
                {
                    var successUrl = payment.Accounts.ReturnUrlSuccess.IsNullOrWhiteSpace()
                        ? WebConfigurationManager.AppSettings["successUrl"]
                        : payment.Accounts.ReturnUrlSuccess;
                    var cancelUrl = payment.Accounts.ReturnUrlFailed.IsNullOrWhiteSpace()
                        ? WebConfigurationManager.AppSettings["cancelUrl"]
                        : payment.Accounts.ReturnUrlFailed;

                    var productName = link.product_Name.IsNullOrWhiteSpace() ? "Balance (Bakiye)" : link.product_Name;

                    var postData = "business=" + WebConfigurationManager.AppSettings["paypalEmail"];
                    postData += "&return_url=" + successUrl;
                    postData += "&cancel_url=" + cancelUrl;
                    postData += "&notify_url=" + WebConfigurationManager.AppSettings["ipn_paypal_notification_url"] + WebConfigurationManager.AppSettings["SecurityKey"];
                    postData += "&cmd=_xclick";
                    postData += "&currency_code=" + link.PaymentCurrency.Name;
                    postData += "&item_name=" + productName;
                    postData += "&amount=" + amount.Replace(",", ".");
                    postData += "&item_number=" + link.OrderID;
                    postData += "&custom=" + payment.ApiCode;

                    ReleaseSessions(true);

                    return RedirectPermanent("https://www.paypal.com/cgi-bin/webscr?" + postData);
                }
                else
                {
                    Response.Write(ShowMessage(link.Token, null, "An error occured. Please contact to MoolsLife Administrator."));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Bank()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var Type = Request.Form["Type"] ?? "";
            var Identity = Request.Form["Identity"] ?? "";
            var Phone = Request.Form["Phone"] ?? "";
            var Sequence = Request.Form["Sequence"] ?? "";
            var BankID = Request.Form["BankID"] ?? "";
            var DateB = Request.Form["Date"] ?? "";
            var PayerName = Request.Form["PayerName"] ?? "";
            var Amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");

            var errorMessage = "";

            // Type
            if (!Type.IsInt() ||
                Convert.ToInt32(Type) != (int)PaymentTypes.ATM && Convert.ToInt32(Type) != (int)PaymentTypes.HavaleEFT)
                errorMessage += "4" + ",";

            // BankID
            if (BankID.IsNullOrWhiteSpace())
                errorMessage += "81" + ",";
            if (BankID.Length > 5)
                errorMessage += "82" + ",";

            // Date
            if (DateB.IsNullOrWhiteSpace())
                errorMessage += "91" + ",";
            if (DateB.Length > 20)
                errorMessage += "92" + ",";

            // PayerName
            if (PayerName.IsNullOrWhiteSpace() || !PayerName.Contains(" "))
                errorMessage += "101" + ",";
            if (PayerName.Length > 100)
                errorMessage += "102" + ",";

            Amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? Amount.Replace(",", ".") : Amount;

            // Amount
            if (Amount.IsNullOrWhiteSpace() || !Amount.IsDecimal() || Convert.ToDecimal(Amount) <= 0)
                errorMessage += "111" + ",";
            if (Amount.Length > 8)
                errorMessage += "112" + ",";

            var Description = "";

            // Identity
            if (Identity.IsNullOrWhiteSpace())
                errorMessage += "51" + ",";
            else
                Description += "TC No:" + Identity;

            if (Identity.Length != 11)
                errorMessage += "52" + ",";

            // Phone
            if (!Phone.IsNullOrWhiteSpace())
            {
                if (Phone.Length != 10)
                    errorMessage += "61" + ",";

                /*if (!Phone.StartsWith("5"))
                    errorMessage += "62" + ",";*/

                Description += "|Telefon Numarası:" + Phone;
            }

            // Sequence
            if (!Sequence.IsNullOrWhiteSpace())
            {
                if (Sequence.Length > 10)
                    errorMessage += "7" + ",";

                Description += "|Sıra No:" + Sequence;
            }

            if (errorMessage.IsNullOrWhiteSpace())
            {
                var payment = InsertNewPayment(link, Convert.ToDecimal(Amount), (PaymentTypes)Convert.ToInt32(Type), (MethodIDs)Convert.ToInt32(BankID), Description, PayerName, Convert.ToDateTime(DateB));
                if (payment != null)
                {
                    Hashtable parameters = new Hashtable();
                    DataAccessor accessor = new DataAccessor();
                    parameters.Add("@BankID", Convert.ToInt32(BankID));
                    parameters.Add("@Date", Convert.ToDateTime(DateB));
                    parameters.Add("@Amount", Convert.ToDecimal(Amount));
                    parameters.Add("@PayerName", PayerName);
                    parameters.Add("@Description", Description);
                    parameters.Add("@NotifyUrl", WebConfigurationManager.AppSettings["ipn_bank_notification_url"] + WebConfigurationManager.AppSettings["SecurityKey"]);
                    parameters.Add("@ApiCode", payment.ApiCode);

                    DataRow row = accessor.Single("ege_bursagb.[Payment].[NewPaymentApiBankTransfer]", parameters,
                        CommandType.StoredProcedure);

                    var paymentId = "";

                    if (row != null)
                        paymentId = row[0].ToString();

                    if (paymentId.IsInt() && Convert.ToInt32(paymentId) > 0)
                    {
                        ReleaseSessions(true);
                        Response.Write(ShowMessage(link.Token, null, "Ödemeniz başarıyla bildirildi. Lütfen ödemenizin sonuçlanmasını bekleyiniz. Ödemeniz onaylandığında ürün hesabınıza otomatik eklenecektir."));
                        return null;
                    }
                    else
                    {
                        try
                        {
                            payment.Status = (short)PaymentStatus.Rejected;
                            payment.Commission = 0;
                            payment.ProcessDate = DateTime.Now;
                            payment.Message = "Auto cancelled, can't notify the central. Please try again.";
                            payment.IsCancelled = true;
                            payment.SendDate = null;

                            _db.Payments.Attach(payment);
                            _db.Entry(payment).State = EntityState.Modified;
                            _db.SaveChanges();

                            Response.Write(ShowMessage(link.Token, null,
                                "Auto cancelled, cant notify the central. Please try again."));
                            return null;
                        }
                        catch (Exception)
                        {
                            Response.Write(ShowMessage(link.Token, null,
                                "An error occured (4). Please contact to MoolsLife Administrator."));
                            return null;
                        }
                    }
                }
                else
                {
                    Response.Write(ShowMessage(link.Token, null,
                        "An error occured (2). Please contact to MoolsLife Administrator."));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Epin()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var errorMessage = "";

            // Code
            var Code = Request.Form["Code"] ?? "";
            if (Code.IsNullOrWhiteSpace())
                errorMessage += "136" + ",";
            if (Code.Length > 100)
                errorMessage += "137" + ",";


            // Bank
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == (int)MethodIDs.MoolsEpin && b.IsActive == true);
            if (bank == null)
                errorMessage += "81" + ",";


            // Trinkpay
            var MoolsLife = _db.MoolsEpin.FirstOrDefault(b => b.Code == Code);
            if (MoolsLife == null)
                errorMessage += "126" + ",";
            else
            {
                if (MoolsLife.GameProductID <= 0)
                {
                    if (MoolsLife.Amount - link.Amount < 0)
                        errorMessage += "127" + ",";
                }
                else
                {
                    if (MoolsLife.MoolsEpinGameProducts.Code != link.product_Code || MoolsLife.MoolsEpinGameProducts.AccountID != link.AccountID)
                        errorMessage += "128" + ",";
                    else if (MoolsLife.Amount - link.Amount < 0)
                        errorMessage += "129" + ",";
                }
            }




            if (errorMessage.IsNullOrWhiteSpace() && MoolsLife != null)
            {
                var payment = InsertNewPayment(link, link.Amount, PaymentTypes.MoolsEpin, MethodIDs.MoolsEpin, "EPIN Code=" + Code, null, null);
                if (payment != null)
                {
                    try
                    {
                        var tpLog = new MoolsEpinUsedLog
                        {
                            Date = DateTime.Now,
                            NewEpinBalance = MoolsLife.Amount - link.Amount,
                            OldEpinBalance = MoolsLife.Amount,
                            MoolsEpinID = MoolsLife.MoolsEpinID,
                            PaymentID = payment.PaymentID
                        };

                        _db.MoolsEpinUsedLog.Add(tpLog);

                        MoolsLife.Amount -= link.Amount;

                        _db.MoolsEpin.Attach(MoolsLife);
                        _db.Entry(MoolsLife).State = EntityState.Modified;

                        _db.SaveChanges();

                        string completeText = "Completed";

                        if (MoolsLife.IsBonus == true)
                            completeText += " (Bonus)";

                        if (!PaymentProcess(payment, PaymentStatus.Approved, completeText, link.Amount, (MoolsLife.IsBonus == true) ? "Bonus Epin" : ""))
                        {
                            ResponseWrite(ShowMessage(link.Token, null, AddErrorLog(payment, null, GetErrorMessage("159"))));
                            return null;
                        }
                        else
                        {
                            ReleaseSessions(true);
                            Response.Write(ShowMessage(link.Token, null, "Your code used successfully. Code:" + Code, false, link.Accounts.ReturnUrlSuccess));
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        Response.Write(ShowMessage(link.Token, null, AddErrorLog(null, null, "An error occured on using Trinkpay(2).Please contact to MoolsLife Administrator.")));
                        return null;
                    }
                }
                else
                {
                    Response.Write(ShowMessage(link.Token, null, AddErrorLog(null, null, "An error occured(7).Please contact to MoolsLife Administrator.")));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult G2A()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var errorMessage = "";

            // Amount
            var Amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");
            Amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? Amount.Replace(",", ".") : Amount;
            if (Amount.IsNullOrWhiteSpace() || !Amount.IsDecimal() || Convert.ToDecimal(Amount) <= 0)
                errorMessage += "111" + ",";
            if (Amount.Length > 8)
                errorMessage += "112" + ",";


            if (errorMessage.IsNullOrWhiteSpace())
            {
                var payment = InsertNewPayment(link, Convert.ToDecimal(Amount), PaymentTypes.G2A, MethodIDs.G2A, null,
                    null, null);
                if (payment != null)
                {
                    Amount = (((CultureInfo)Session["Culture"]).Name == "tr") ? Amount.Replace(",", ".") : Amount;

                    var apiHash = "e35cb4d3-4104-43d5-97fa-ca5ad419353f";
                    var hash = GetSha256FromString(payment.ApiCode + Amount + link.PaymentCurrency.Name + "!g=bCs4@xL$@P_?E?IIqduH@kIXFpbGf231Ev65dR?VpyT+wl6X9-o0W@Ylg%_6&");

                    var successUrl = payment.Accounts.ReturnUrlSuccess.IsNullOrWhiteSpace()
                        ? WebConfigurationManager.AppSettings["successUrl"]
                        : payment.Accounts.ReturnUrlSuccess;

                    var cancelUrl = payment.Accounts.ReturnUrlFailed.IsNullOrWhiteSpace()
                        ? WebConfigurationManager.AppSettings["cancelUrl"]
                        : payment.Accounts.ReturnUrlFailed;

                    var post_link = WebConfigurationManager.AppSettings["G2A_post_link"];
                    var redirect_link = WebConfigurationManager.AppSettings["G2A_redirect_link"];
                    var ProductName = link.product_Name.IsNullOrWhiteSpace() ? "Balance (Bakiye)" : link.product_Name;
                    var product_Code = link.product_Code.IsNullOrWhiteSpace() ? "5432" : link.product_Code;

                    var postData = "api_hash=" + apiHash;
                    postData += "&hash=" + hash;
                    postData += "&order_id=" + payment.ApiCode;
                    postData += "&amount=" + Amount;
                    postData += "&currency=TRY";
                    postData += "&url_ok=" + successUrl;
                    postData += "&url_failure=" + cancelUrl;
                    postData += "&items=" + "[{\"sku\":\"450\",\"name\":\"" + ProductName + "\",\"amount\":" + Amount +
                                ",\"type\":\"balance\",\"qty\":\"1\",\"price\":" + Amount + ",\"id\":\"" + product_Code +
                                "\",\"url\":\"https://www.moolslife.com\"}]";

                    var responseString = PostData(post_link, postData);
                    var linkUrl = responseString.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',');

                    if (!responseString.Contains("token"))
                    {
                        Response.Write(ShowMessage(link.Token, "160", null));
                        return null;
                    }

                    ReleaseSessions(true);
                    return RedirectPermanent(redirect_link + linkUrl[1].Replace("token:", ""));
                }
                else
                {
                    Response.Write(ShowMessage(link.Token, null, "Problem occured, please try again."));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }



        [HttpGet]
        public ActionResult ShopierGetPayment(string ShopierProductID, string ShopierHiddenID)
        {
            var postData = "productId[]=" + ShopierProductID + "&id[]=" + ShopierHiddenID + "&select-quantity[]=1&quantity=1";
            //var responseString = PostData("https://www.shopier.com/ShowProductNew/shippingdetails.php", postData);


            using (var client = new HttpClient())
            {
                try
                {
                    // Next two lines are not required. You can comment or delete that lines without any regrets
                    const string baseUri = "https://www.shopier.com/";
                    client.BaseAddress = new Uri(baseUri);

                    var response = client.PostAsync($"{baseUri}/ShowProductNew/shippingdetails.php?"+ postData, null);
                    response.Wait();
                    if (response.Result.IsSuccessStatusCode)
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }

            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayTR()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var Amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");
            var PayerName = Request.Form["PayerName"] ?? link.customer_NameSurname;
            var Email = Request.Form["Email"] ?? link.customer_Email;
            var Phone = Request.Form["Phone"] ?? link.customer_Phone;

            var errorMessage = "";


            // PayerName
            if (PayerName.IsNullOrWhiteSpace() || !PayerName.Contains(" "))
                errorMessage += "101" + ",";
            if (PayerName.Length > 100)
                errorMessage += "102" + ",";


            // Email
            if (Email.IsNullOrWhiteSpace() || !Email.Contains("@") || !Email.Contains(".") || Email.Contains(" "))
                errorMessage += "141" + ",";
            if (Email.Length > 70)
                errorMessage += "142" + ",";


            // Phone
            if (!Phone.IsNullOrWhiteSpace())
            {
                if (Phone.Length != 10)
                    errorMessage += "61" + ",";

                /*if (!Phone.StartsWith("5"))
                    errorMessage += "62" + ",";*/
            }

            Amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? Amount.Replace(",", ".") : Amount;

            // Amount
            if (Amount.IsNullOrWhiteSpace() || !Amount.IsDecimal() || Convert.ToDecimal(Amount) <= 0)
                errorMessage += "111" + ",";
            if (Amount.Length > 8)
                errorMessage += "112" + ",";


            if (errorMessage.IsNullOrWhiteSpace())
            {
                try
                {
                    link.customer_NameSurname = PayerName;
                    link.customer_Phone = Phone;

                    _db.Links.Attach(link);
                    _db.Entry(link).State = EntityState.Modified;
                    _db.SaveChanges();

                    var payment = InsertNewPayment(link, Convert.ToDecimal(Amount), PaymentTypes.PayTR, MethodIDs.PayTR_KrediKartı, null, null, null);
                    if (payment != null)
                    {
                        var merchant_id = WebConfigurationManager.AppSettings["PayTR_merchant_id"];
                        var merchant_key = WebConfigurationManager.AppSettings["PayTR_merchant_key"];
                        var merchant_salt = WebConfigurationManager.AppSettings["PayTR_merchant_salt"];
                        var no_installment = WebConfigurationManager.AppSettings["PayTR_no_installment"];
                        var max_installment = WebConfigurationManager.AppSettings["PayTR_max_installment"];
                        var get_link = WebConfigurationManager.AppSettings["PayTR_get_link"];
                        var post_link = WebConfigurationManager.AppSettings["PayTR_post_link"];

                        var merchant_ok_url = payment.Accounts.ReturnUrlSuccess.IsNullOrWhiteSpace() ? WebConfigurationManager.AppSettings["successUrl"] : payment.Accounts.ReturnUrlSuccess;
                        var merchant_fail_url = payment.Accounts.ReturnUrlFailed.IsNullOrWhiteSpace() ? WebConfigurationManager.AppSettings["cancelUrl"] : payment.Accounts.ReturnUrlFailed;
                        var user_ip = RealIpAddress();
                        var merchant_oid = payment.ApiCode;
                        var productName = link.product_Name.IsNullOrWhiteSpace() ? "Site Bakiye" : link.product_Name;


                        object[] basket = { productName, Convert.ToDecimal(Amount).ToString().Replace(",", "."), "1" };
                        var user_basket_json = JsonConvert.SerializeObject(basket);
                        string user_basketstr = Convert.ToBase64String(Encoding.ASCII.GetBytes((user_basket_json)));


                        string currency = "TL";
                        string test_mode = "0";

                        var user_address = "MoolsLife Address";
                        Amount = (Convert.ToInt32(Convert.ToDecimal(Amount) * 100)).ToString();
                        //var hash_str = merchant_id + user_ip + merchant_oid + Email + Amount + user_basketstr + no_installment + max_installment + currency + test_mode + merchant_salt;

                        //var paytr_token = PayTRtoken(hash_str, merchant_key);

                        string Birlestir = string.Concat(merchant_id + user_ip + merchant_oid + Email + Amount + user_basketstr + no_installment + max_installment + currency + test_mode + merchant_salt);
                        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
                        var paytr_token = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir)));


                        var postData = "merchant_id=" + merchant_id;
                        postData += "&user_ip=" + user_ip;
                        postData += "&merchant_oid=" + merchant_oid;
                        postData += "&email=" + Email;
                        postData += "&payment_amount=" + Amount;
                        postData += "&user_basket=" + user_basketstr;
                        postData += "&no_installment=" + no_installment;
                        postData += "&max_installment=" + max_installment;
                        postData += "&user_name=" + PayerName;
                        postData += "&merchant_ok_url=" + merchant_ok_url;
                        postData += "&merchant_fail_url=" + merchant_fail_url;
                        postData += "&paytr_token=" + paytr_token;
                        postData += "&user_address=" + user_address;
                        postData += "&user_phone=0" + Phone;
                        postData += "&currency=" + currency;
                        postData += "&test_mode=" + test_mode;
                        postData += "&debug_on=1";
                        postData += "&timeout_limit=30";

                        var responseString = PostData(post_link, postData);


                        var linkUrl = responseString.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',');

                        if (!responseString.Contains("token"))
                        {
                            Response.Write(ShowMessage(link.Token, "160", null));
                            return null;
                        }

                        ReleaseSessions(true);
                        return RedirectPermanent(get_link + linkUrl[1].Replace("token:", ""));
                    }
                    else
                    {
                        Response.Write(ShowMessage(link.Token, null, "An error occured (2). Please contact to MoolsLife Administrator."));
                        return null;
                    }
                }
                catch (Exception)
                {
                    Response.Write(AddErrorLog(null, "X2", "Cant dead link.  Token:" + link.Token));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sonteklif()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var Amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");
            var PayerName = Request.Form["PayerName"] ?? link.customer_NameSurname;
            var Email = Request.Form["Email"] ?? link.customer_Email;
            var Phone = Request.Form["Phone"] ?? link.customer_Phone;

            var errorMessage = "";


            // PayerName
            if (PayerName.IsNullOrWhiteSpace() || !PayerName.Contains(" "))
                errorMessage += "101" + ",";
            if (PayerName.Length > 100)
                errorMessage += "102" + ",";


            // Email
            if (Email.IsNullOrWhiteSpace() || !Email.Contains("@") || !Email.Contains(".") || Email.Contains(" "))
                errorMessage += "141" + ",";
            if (Email.Length > 70)
                errorMessage += "142" + ",";


            // Phone
            if (!Phone.IsNullOrWhiteSpace())
            {
                if (Phone.Length != 10)
                    errorMessage += "61" + ",";

                /*if (!Phone.StartsWith("5"))
                    errorMessage += "62" + ",";*/
            }

            Amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? Amount.Replace(",", ".") : Amount;

            // Amount
            if (Amount.IsNullOrWhiteSpace() || !Amount.IsDecimal() || Convert.ToDecimal(Amount) <= 0)
                errorMessage += "111" + ",";
            if (Amount.Length > 8)
                errorMessage += "112" + ",";


            if (errorMessage.IsNullOrWhiteSpace())
            {
                try
                {
                    link.customer_NameSurname = PayerName;
                    link.customer_Phone = Phone;

                    _db.Links.Attach(link);
                    _db.Entry(link).State = EntityState.Modified;
                    _db.SaveChanges();

                    var payment = InsertNewPayment(link, Convert.ToDecimal(Amount), PaymentTypes.Sonteklif, MethodIDs.Sonteklif, null, null, null);
                    if (payment != null)
                    {
                        var servis_id = WebConfigurationManager.AppSettings["Sonteklif_servis_id"];
                        var productId = WebConfigurationManager.AppSettings["Sonteklif_productid"];

                        string serverip = RealIpAddress();

                        var intAmount = Decimal.ToInt32(Math.Round(Convert.ToDecimal(Amount)));
                        int quantity = intAmount < Convert.ToDecimal(Amount) ? intAmount + 1 : intAmount;
                        var names = link.customer_NameSurname.Split(' ');

                        var fname = names[0] ?? "";
                        var sname = names[names.Length - 1] ?? "";

                        string token = SonteklifToken(link.customer_Email, serverip, servis_id);
                        string url = "http://www.sonteklif.com/guvenli-giris?fp_kullanici=" + link.customer_Email + "&fp_urun[]=" + productId + "&fp_adet_secim[]=" + quantity + "&fp_eposta=" + link.customer_Email + "&fp_ad=" + Uri.EscapeDataString(fname) + "&fp_soyad=" + Uri.EscapeDataString(sname) + "&fp_token=" + token + "&ip=" + serverip + "&fp_siparis_no=" + payment.ApiCode;

                        ReleaseSessions(true);
                        return RedirectPermanent(url);
                    }
                    else
                    {
                        Response.Write(ShowMessage(link.Token, null, "An error occured (2). Please contact to MoolsLife Administrator."));
                        return null;
                    }
                }
                catch (Exception)
                {
                    Response.Write(AddErrorLog(null, "X2", "Cant dead link.  Token:" + link.Token));
                    return null;
                }
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayTRBank()
        {
            Session["clickItem"] = Request.Form["clickItem"] ?? "";
            Session["errorCode"] = null;

            var l = Check();
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);


            var Amount = (link.Amount == 0 ? Request.Form["Amount"] : link.Amount.ToString()).Replace(".", ",");
            var PayerName = Request.Form["PayerName"] ?? link.customer_NameSurname;
            var Email = Request.Form["Email"] ?? link.customer_Email;
            var Phone = Request.Form["Phone"] ?? link.customer_Phone;
            var BankID = Request.Form["BankID"] ?? "";
            var Identity = Request.Form["Identity"] ?? link.customer_Identity;
            var errorMessage = "";


            // PayerName
            if (PayerName.IsNullOrWhiteSpace() || !PayerName.Contains(" "))
                errorMessage += "101" + ",";
            if (PayerName.Length > 100)
                errorMessage += "102" + ",";


            // Identity
            if (!Identity.IsDecimal())
                errorMessage += "113" + ",";
            if (Identity.Length != 11 && Identity.Length != 5)
                errorMessage += "114" + ",";


            // Email
            if (Email.IsNullOrWhiteSpace() || !Email.Contains("@") || !Email.Contains(".") || Email.Contains(" "))
                errorMessage += "141" + ",";
            if (Email.Length > 70)
                errorMessage += "142" + ",";


            // Phone
            if (!Phone.IsNullOrWhiteSpace())
            {
                if (Phone.Length != 10)
                    errorMessage += "61" + ",";

                /*if (!Phone.StartsWith("5"))
                    errorMessage += "62" + ",";*/
            }

            Amount = (((CultureInfo)Session["Culture"]).Name != "tr") ? Amount.Replace(",", ".") : Amount;

            // Amount
            if (Amount.IsNullOrWhiteSpace() || !Amount.IsDecimal() || Convert.ToDecimal(Amount) <= 0)
                errorMessage += "111" + ",";
            if (Amount.Length > 8)
                errorMessage += "112" + ",";

            var bank = "";
            var intBankID = Convert.ToInt32(BankID);
            if (intBankID == (int)MethodIDs.PayTR_ZiraatBankası)
                bank = "ziraat";
            else if (intBankID == (int)MethodIDs.PayTR_İşBankası)
                bank = "isbank";
            else if (intBankID == (int)MethodIDs.PayTR_Akbank)
                bank = "akbank";
            else if (intBankID == (int)MethodIDs.PayTR_DenizBank)
                bank = "denizbank";
            else if (intBankID == (int)MethodIDs.PayTR_FinansBank)
                bank = "finansbank";
            else if (intBankID == (int)MethodIDs.PayTR_HalkBank)
                bank = "halkbank";
            else if (intBankID == (int)MethodIDs.PayTR_PTT)
                bank = "ptt";
            else if (intBankID == (int)MethodIDs.PayTR_TEB)
                bank = "teb";
            else if (intBankID == (int)MethodIDs.PayTR_VakıfBank)
                bank = "vakifbank";
            else if (intBankID == (int)MethodIDs.PayTR_YapıKredi)
                bank = "yapikredi";
            else if (intBankID == (int)MethodIDs.PayTR_KuveytTürk)
                bank = "kuveytturk";


            if (errorMessage.IsNullOrWhiteSpace())
            {
                /*try
                {*/
                link.customer_NameSurname = PayerName;
                link.customer_Phone = Phone;
                link.customer_Identity = Identity;

                _db.Links.Attach(link);
                _db.Entry(link).State = EntityState.Modified;
                _db.SaveChanges();

                var payment = InsertNewPayment(link, Convert.ToDecimal(Amount), PaymentTypes.PayTR, (MethodIDs)intBankID, null, null, null);
                if (payment != null)
                {
                    var productName = link.product_Name.IsNullOrWhiteSpace() ? "Site Bakiye" : link.product_Name;
                    object[][] user_basket = {
                        new object[] { productName, Convert.ToDecimal(Amount).ToString().Replace(",", "."), 1},
                        };
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    string user_basket_json = ser.Serialize(user_basket);
                    string user_basketstr = Convert.ToBase64String(Encoding.UTF8.GetBytes(user_basket_json));


                    var merchant_id = WebConfigurationManager.AppSettings["PayTR_merchant_id"];
                    var merchant_key = WebConfigurationManager.AppSettings["PayTR_merchant_key"];
                    var merchant_salt = WebConfigurationManager.AppSettings["PayTR_merchant_salt"];
                    var get_api_link = WebConfigurationManager.AppSettings["PayTR_get_api_link"];
                    var post_link = WebConfigurationManager.AppSettings["PayTR_post_link"];
                    var user_ip = RealIpAddress();
                    var merchant_oid = payment.ApiCode;
                    Amount = (Convert.ToInt32(Convert.ToDecimal(Amount) * 100)).ToString();
                    var hash_str = merchant_id + user_ip + merchant_oid + Email + Amount + "eft" + merchant_salt;
                    var paytr_token = SHACrypt(hash_str, merchant_key);


                    var postData = "merchant_id=" + merchant_id;
                    postData += "&user_ip=" + user_ip;
                    postData += "&merchant_oid=" + merchant_oid;
                    postData += "&email=" + Email;
                    postData += "&payment_amount=" + Amount;
                    postData += "&user_name=" + PayerName;
                    postData += "&paytr_token=" + paytr_token;
                    postData += "&user_phone=0" + Phone;
                    postData += "&tc_no_last5=" + link.customer_Identity.Substring(link.customer_Identity.Length - 5);
                    postData += "&bank=" + bank;
                    postData += "&payment_type=eft";
                    postData += "&user_basket=" + user_basketstr;
                    //postData += "&debug_on=1";

                    var responseString = PostData(post_link, postData);

                    var linkUrl = responseString.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',');

                    if (!responseString.Contains("token"))
                    {
                        Response.Write(ShowMessage(link.Token, "160", null));
                        return null;
                    }

                    ReleaseSessions(true);
                    return RedirectPermanent(get_api_link + linkUrl[1].Replace("token:", ""));
                }
                else
                {
                    Response.Write(ShowMessage(link.Token, null, "An error occured (2). Please contact to MoolsLife Administrator."));
                    return null;
                }

                /* }
                 catch (Exception)
                 {
                     Response.Write(AddErrorLog(null, "X3", "Cant dead link.Token:" + link.Token));
                     return null;
                 }*/
            }
            else
            {
                Response.Write(ShowMessage(link.Token, errorMessage, null));
                return null;
            }
        }




        [HttpPost]
        public object Check(bool? cancelPayment = false)
        {
            var token = Session["Token"]?.ToString() ?? "";

            var link = _db.Links.FirstOrDefault(l => l.Token == token);
            if (link == null)
                return -1;

            if (cancelPayment == true)
                return link;

            if (link.IsDead == true)
                return -2;

            if (link.GeneratedDate.AddMinutes(checkSessionTime) < DateTime.Now)
                return -2;

            //test
            var paymentCount = 0;//_db.Payments.Count(p =>p.Accounts.AccountID == link.Accounts.AccountID && p.Links1.CustomerAccount == link.CustomerAccount && p.Status == (short) PaymentStatus.Waiting);
            if (paymentCount > 0)
                return 0;

            return link;
        }

        [HttpGet]
        public ActionResult CancelPayment()
        {
            var l = Check(true);
            if (!(l is Links))
                return RedirectToAction("ShowMethods", "ShowMethods", new { token = Session["Token"] });

            var link = ((Links)l);

            try
            {
                //link.IsDead = true; //test
                _db.Links.Attach(link);
                _db.Entry(link).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception)
            {
                AddErrorLog(null, "X2", "Cant dead link.  Token:" + link.Token);
            }

            return RedirectPermanent(link.Accounts.PaymentCancelRedirectUrl_);
        }
    }
}