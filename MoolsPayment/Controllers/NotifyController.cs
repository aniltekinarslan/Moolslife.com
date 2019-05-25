using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using MoolsPayment.Models;
using Newtonsoft.Json;

namespace MoolsPayment.Controllers
{
    public class NotifyController : BaseController
    {
        //
        // POST: PayByMe
        [HttpPost]
        public ActionResult PayByMe(string key)
        {
            if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Wrong Key: " + key));



            AddNotifyLog("PayByMe", "merchant_oid=");



            return ResponseWrite("OK");
        }





        //
        // POST: PayTR
        [HttpPost]
        public ActionResult PayTR(string key)
        {
            if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + " Wrong Key: " + key));

            var merchant_oid = Request.Form["merchant_oid"];
            var PayTR_Hash = Request.Form["hash"];
            var status = Request.Form["status"];
            var total_amount = Request.Form["total_amount"];
            var merchant_key = WebConfigurationManager.AppSettings["PayTR_merchant_key"];
            var merchant_salt = WebConfigurationManager.AppSettings["PayTR_merchant_salt"];

            var failed_reason_code = Request.Form["failed_reason_code"];
            var failed_reason_msg = Request.Form["failed_reason_msg"];

            var hash = SHACrypt(merchant_oid + merchant_salt + status + total_amount, merchant_key);

            AddNotifyLog("PayTR", "merchant_oid=" + merchant_oid + "  /PayTR_Hash=" + PayTR_Hash + "  /hash=" + hash +
                           "  /status=" + status + "  /total_amount=" + total_amount + "  /merchant_key=" + merchant_key +
                           "  /merchant_salt=" + merchant_salt + "  /failed_reason_code=" + failed_reason_code +
                           "  /failed_reason_msg=" + failed_reason_msg);

            if (hash != PayTR_Hash)
            {
                AddErrorLog(null, "43", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + " Hash Failed! Hash:" + hash + "  / PayTR_Hash:" + PayTR_Hash);
                return ResponseWrite("OK");
            }


            // CHECKING PAYMENT START
            var p = CheckIsValidPayment(merchant_oid);
            if (!(p is Payments))
            {
                //AddErrorLog(null, "invalid payment", p.ToString());
                return ResponseWrite("OK");
            }

            // CHECKING PAYMENT END

            // payment
            var payment = ((Payments)p);

            var statusS = (status == "success") ? PaymentStatus.Approved : PaymentStatus.Rejected;

            if (payment.Status == (short)statusS)
            {
                AddErrorLog(payment, "43", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + ")Approved before. Status:" + status);
                return ResponseWrite("OK");
            }

            // bank
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == payment.MethodID && b.IsActive == true);
            if (bank == null)
            {
                AddErrorLog(payment, "Bank error", "PaymentID=" + payment.PaymentID);
                return ResponseWrite("OK");
            }


            var Amount = Convert.ToDecimal(total_amount) / Convert.ToDecimal(100);


            if (!PaymentProcess(payment, statusS, status, Amount))
            {
                AddErrorLog(payment, "15_1", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot be " + statusS + ", please contact to MoolsLife Administrator.");
                return ResponseWrite("OK");
            }

            return ResponseWrite("OK");
        }


        private class ShopierResponse
        {
            public string email { get; set; }
            public string orderid { get; set; }
            public int currency { get; set; }
            public decimal price { get; set; }
            public string buyername { get; set; }
            public string buyersurname { get; set; }
            public int productcount { get; set; }
            public int productid { get; set; }
            public string customernote { get; set; }
            public int istest { get; set; }
        }

        //
        // POST: Shopier
        [HttpPost]
        public ActionResult Shopier(string SecurityKey)
        {
            var res = Request.Form["res"];
            var hash = Request.Form["hash"];
            var SecurityKey_orj = WebConfigurationManager.AppSettings["SecurityKey"].ToString();
            var key = WebConfigurationManager.AppSettings["Shopier_key"].ToString();
            var username = WebConfigurationManager.AppSettings["Shopier_username"].ToString();

            if (SecurityKey != SecurityKey_orj)
            {
                AddErrorLog(null, "14", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + " Key Failed! SecurityKey:" + SecurityKey);
                return ResponseWrite("missing parameter");
            }

            if (res == null || hash == null)
            {
                AddErrorLog(null, "13", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + " missing parameter!");
                return ResponseWrite("missing parameter");
            }

            if (hash != HashHmac(res + username, key))
            {
                AddErrorLog(null, "43", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + " Hash Failed! Hash:" + hash);
                return ResponseWrite("OK");
            }

            AddNotifyLog("Shopier", "username : " + username + " // res : " + res + " // ourhash : " + HashHmac(res + username, key) + " // hash : " + hash + " // Response JSON : " + Base64Decode(res));

            ShopierResponse resObj = JsonConvert.DeserializeObject<ShopierResponse>(Base64Decode(res));

            /*
            // CHECKING PAYMENT START
            var p = CheckIsValidPayment(merchant_oid);
            if (!(p is Payments))
            {
                //AddErrorLog(null, "invalid payment", p.ToString());
                return ResponseWrite("success");
            }

            // CHECKING PAYMENT END

            // payment
            var payment = ((Payments)p);

            var statusS = (status == "success") ? PaymentStatus.Approved : PaymentStatus.Rejected;

            if (payment.Status == (short)statusS)
            {
                AddErrorLog(payment, "43", "Message=(" + GetCurrentControllerName() + " /)" + GetCurrentActionName() + ")Approved before. Status:" + status);
                return ResponseWrite("success");
            }

            // bank
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == payment.MethodID && b.IsActive == true);
            if (bank == null)
            {
                AddErrorLog(payment, "Bank error", "PaymentID=" + payment.PaymentID);
                return ResponseWrite("success");
            }


            var Amount = Convert.ToDecimal(total_amount) / Convert.ToDecimal(100);


            if (!PaymentProcess(payment, statusS, status, Amount))
            {
                AddErrorLog(payment, "15_1", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot be " + statusS + ", please contact to MoolsLife Administrator.");
                return ResponseWrite("success");
            }*/

            return ResponseWrite("success");
        }



        
        //
        // GET: Sonteklif
        [HttpGet]
        public ActionResult Sonteklif(string key)
        {
            if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + " Wrong Key: " + key));

            var servis_id = WebConfigurationManager.AppSettings["Sonteklif_servis_id"];
            var siparis_no = Request["siparis_no"] ?? "";
            var satis_kodu = Request["satis_kodu"] ?? "";
            var hash = Request["hash"] ?? "";
            var kullanici_id = Request["kullanici"] ?? "";
            var urun_id = Request["urun"] ?? "";
            var adet = Request["adet"] ?? "";

            AddNotifyLog("Sonteklif", "siparis_no=" + siparis_no + "  /satis_kodu=" + satis_kodu + "  /hash=" + hash +
                           "  /kullanici_id=" + kullanici_id + "  /urun_id=" + urun_id + "  /adet=" + adet);

            if (satis_kodu.IsNullOrWhiteSpace() || hash.IsNullOrWhiteSpace() || kullanici_id.IsNullOrWhiteSpace() ||
                urun_id.IsNullOrWhiteSpace() || adet.IsNullOrWhiteSpace())
            {
                AddErrorLog(null, "invalid null", "");
                return ResponseWrite("FAILED1");
            }


            string hash_ = SonteklifHash(urun_id, satis_kodu, adet, servis_id);
            if (hash_ != hash)
            {
                AddErrorLog(null, "invalid hash    hash_:" + hash_ + " /hash:" + hash, "");
                return ResponseWrite("FAILED2");
            }



            // CHECKING PAYMENT START
            var p = CheckIsValidPayment(siparis_no);
            if (!(p is Payments))
            {
                AddErrorLog(null, "invalid payment siparis_no:" + siparis_no, p.ToString());
                return ResponseWrite("OK");
            }
            // CHECKING PAYMENT END

            // payment
            var payment = ((Payments)p);

            if (payment.Status == (short)PaymentStatus.Approved)
                return ResponseWrite(AddErrorLog(payment, "43", "Message=(Sonteklif)Approved before. Status:" + PaymentStatus.Approved));

            // bank
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == payment.MethodID && b.IsActive == true);
            if (bank == null)
            {
                AddErrorLog(payment, "Bank error", "PaymentID=" + payment.PaymentID);
                return ResponseWrite("OK");
            }

            var count = Convert.ToDecimal(adet);

            if (!PaymentProcess(payment, PaymentStatus.Approved, "Satis_kodu=" + satis_kodu, count))
            {
                AddErrorLog(payment, "15_1", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot be approved, please contact to MoolsLife Administrator.");
                return ResponseWrite("OK");
            }

            return ResponseWrite("OK");
        }




        //
        // POST: Paypal
        [HttpPost]
        public ActionResult Paypal(string key)
        {
            if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Wrong Key: " + key));

            string apiCode = Request.Form["custom"] ?? "";
            string payer_email = Request.Form["payer_email"] ?? "";
            string payment_status = Request.Form["payment_status"] ?? "";
            string first_name = Request.Form["first_name"] ?? "TEKİNARSLAN";
            string last_name = Request.Form["last_name"] ?? "Gürol";
            string mc_currency = Request.Form["mc_currency"] ?? "";
            string address_country = Request.Form["address_country"] ?? "";
            string address_city = Request.Form["address_city"] ?? "";

            //string c = Request.Form["mc_fee"] ?? "";

            //decimal commission = Convert.ToDecimal((c).Replace(".", ","));
            decimal amount = Convert.ToDecimal((Request.Form["mc_gross"] ?? "").Replace(".", ",")); //- commission;

            AddNotifyLog("Paypal", "apiCode=" + apiCode + "  /payer_email=" + payer_email + "  /payment_status=" + payment_status + "  /first_name=" + HttpContext.Server.UrlDecode(first_name) + "  /last_name=" + HttpContext.Server.UrlDecode(last_name) + "  /mc_currency" + mc_currency + "  /address_country=" + HttpContext.Server.UrlDecode(address_country) + "  /address_city=" + HttpContext.Server.UrlDecode(address_city) + "  /commission" /*+ commission*/ + "  /amount=" + amount);

            // CHECKING PAYMENT START
            var p = CheckIsValidPayment(apiCode);
            if (!(p is Payments))
                return ResponseWrite(AddErrorLog(null, "invalid payment", p.ToString()));
            // CHECKING PAYMENT END

            // payment
            var payment = ((Payments)p);
            var status = (payment_status == "confirmed" || payment_status == "Completed") ? PaymentStatus.Approved : PaymentStatus.Waiting;

            if (payment.Status == (short)status)
                return ResponseWrite(AddErrorLog(payment, "43", "Message=(Paypal)Approved before. Status:" + payment_status));


            // bank
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == payment.MethodID && b.IsActive == true);
            if (bank == null)
                return ResponseWrite(AddErrorLog(payment, "Bank error", "PaymentID=" + payment.PaymentID));


            if (!PaymentProcess(payment, status, payment_status, amount, "Payer Email:" + payer_email + "|Payer Name:" + first_name + " " + last_name + "|CurrencyCode:" + mc_currency + "|Country:" + address_country + "|City:" + address_city))
                return ResponseWrite(AddErrorLog(payment, "15_1", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot be " + status + ", please contact to MoolsLife Administrator."));

            return ResponseWrite("OK");
        }

        //
        // POST: G2A bitmedi
        [HttpPost]
        public ActionResult G2A(string key)
        {
            if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Wrong Key: " + key));

            string apiCode = Request.Form["userOrderId"] ?? "";
            string payer_email = Request.Form["payer_email"] ?? "";
            string status = Request.Form["status"] ?? "";
            string transactionId = Request.Form["transactionId"] ?? "";
            string last_name = Request.Form["last_name"] ?? "";
            string currency = Request.Form["currency"] ?? "";
            string orderCompleteAt = Request.Form["orderCompleteAt"] ?? "";
            decimal amount = Convert.ToDecimal(Request.Form["amount"] ?? "".Replace(".", ","));

            AddNotifyLog("G2A", "apiCode=" + apiCode + "  /payer_email=" + payer_email + "  /status=" + status + "  /transactionId=" + transactionId + "  /last_name=" + last_name + "  /currency" + currency + "  /orderCompleteAt=" + orderCompleteAt + "  /amount=" + amount);

            // CHECKING PAYMENT START
            var p = CheckIsValidPayment(apiCode);
            if (!(p is Payments))
                return ResponseWrite(AddErrorLog(null, "invalid payment", p.ToString()));
            // CHECKING PAYMENT END

            // payment
            var payment = ((Payments)p);
            if (payment.Status == (short)PaymentStatus.Waiting /*&& status != PaymentStatus.Approved*/)
                return ResponseWrite(AddErrorLog(payment, "43", "Message=(G2A)Approved before. Status:" + status));

            return ResponseWrite("OK");
        }



        //
        // POST: Bank
        [HttpPost]
        public ActionResult Bank(string key)
        {
            try
            {
                if (key != WebConfigurationManager.AppSettings["SecurityKey"])
                    return ResponseWrite(AddErrorLog(null, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Wrong Key: " + key));

                string ApiCode = Request["ApiCode"] ?? "";
                string Message = Request["Message"] ?? "";
                string Status = Request["Status"] ?? "";

                AddNotifyLog("Bank", "ApiCode=" + ApiCode + "  /Message=" + Message + "  /Status=" + Status);

                var payment = _db.Payments.FirstOrDefault(p => p.ApiCode == ApiCode);
                if (payment == null || ApiCode.IsNullOrWhiteSpace())
                    return ResponseWrite(AddErrorLog(payment, "13", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot found, please check your apicode."));


                var status = Status == "1" ? PaymentStatus.Approved : PaymentStatus.Rejected;

                if (payment.Status == (short)status)
                    return ResponseWrite(AddErrorLog(payment, "14", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cancelled or confirmed before."));

                if (!PaymentProcess(payment, status, Message))
                    return ResponseWrite(AddErrorLog(payment, "15_1", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Payment cannot be " + status + ", please contact to MoolsLife Administrator."));

                return ResponseWrite("OK");
            }
            catch (Exception e)
            {
                return ResponseWrite(AddErrorLog(null, "X", "(" + GetCurrentControllerName() + "/" + GetCurrentActionName() + ")" + "  Failed msg:" + e.Message));
            }
        }


        //
        // GENERAL API POST: SendAllPayments
        [HttpPost]
        public ActionResult SendAllPayments()
        {
            try
            {
                var allPayments = _db.Payments.Where(p => p.IsSend == false).ToList();
                foreach (var p in allPayments)
                {
                    var date = (DateTime)p.Date;

                    if (p.Status == (short)PaymentStatus.Waiting && date.AddHours(12) < DateTime.Now)
                    {
                        try
                        {
                            p.IsCancelled = true;
                            p.Status = (short)PaymentStatus.Rejected;
                            p.Message = "Date expired:  " + DateTime.Now;
                            p.Links1.IsDead = true;

                            _db.Payments.Attach(p);
                            _db.Entry(p).State = EntityState.Modified;
                            _db.SaveChanges();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }



                var payments = allPayments.Where(p => p.Status != (short)PaymentStatus.Waiting);
                payments = payments.Where(p => !p.Accounts.NotifyUrl.IsNullOrWhiteSpace() && !p.ApiCode.IsNullOrWhiteSpace()).ToList();

                var responseText = "";
                foreach (var p in payments)
                {
                    try
                    {
                        var postData = "AccountID=" + p.AccountID;
                        postData += "&Amount=" + ((int)(p.Amount * 100));
                        postData += "&Commission=" + ((int)(p.Commission * 100));
                        postData += "&NetAmount=" + ((int)((p.Amount - p.Commission) * 100));
                        postData += "&Currency=" + p.Links1.PaymentCurrency.Name;
                        postData += "&Status=" + p.Status;
                        postData += "&ProcessDate=" + p.ProcessDate;
                        postData += "&Message=" + p.Message;
                        postData += "&Description=" + p.Description;
                        postData += "&ApiCode=" + p.ApiCode;
                        postData += "&OrderID=" + p.Links1.OrderID;
                        postData += "&product_Code=" + p.Links1.product_Code;
                        postData += "&customer_Email=" + p.Links1.customer_Email;
                        postData += "&paymentType=" + p.Type;


                        var responseString = PostData(p.Accounts.NotifyUrl, postData);
                        if (responseString == "OK" || responseString == "OK\r\n")
                        {
                            p.IsSend = true;
                            p.SendDate = DateTime.Now;

                            _db.Payments.Attach(p);
                            _db.Entry(p).State = EntityState.Modified;
                            _db.SaveChanges();

                            responseText += "\n\n" + "PaymentID = " + p.PaymentID + ", Status = 1, Message = OK";
                        }
                        else
                            responseText += AddErrorLog(p, "Notify Error", "Wrong response. ResponseString:" + responseString);
                    }
                    catch (Exception)
                    {
                        responseText += "\n\n" + "PaymentID = " + p.PaymentID + ", Status = -1, Message = Failed to sent.";
                    }
                }

                return ResponseWrite(responseText);
            }
            catch (Exception e)
            {
                return ResponseWrite(new { ErrorCode = "X", Message = e.Message, Detail = e.TargetSite });
            }
        }



        //
        // GENERAL API GET: GetPaymentNotification
        [HttpPost]
        public ActionResult ExampleGetPaymentNotification()
        {
            var AccountID = Request.Form["AccountID"] ?? "";
            string DealerCode = Request.Form["DealerCode"] ?? "";

            if (AccountID != "10009" && DealerCode != "r12dsj1cg")
                return ResponseWrite("Wrong id");

            string Commission = Request.Form["Commission"] ?? "";
            string PaymentID = Request.Form["PaymentID"] ?? "";
            string Type = Request.Form["Type"] ?? "";
            string PayerName = Request.Form["PayerName"] ?? "";
            string Amount = Request.Form["Amount"] ?? "";
            string Description = Request.Form["Description"] ?? "";
            string Date = Request.Form["Date"] ?? "";
            string Status = Request.Form["Status"] ?? "";
            string ProcessDate = Request.Form["ProcessDate"] ?? "";
            string Message = Request.Form["Message"] ?? "";
            string BankID = Request.Form["BankID"] ?? "";
            string ApiCode = Request.Form["ApiCode"] ?? "";
            string CustomerAccount = Request.Form["CustomerAccount"] ?? "";
            string NetAmount = Request.Form["NetAmount"] ?? "";



            var payment = _db.Payments.FirstOrDefault(p => p.ApiCode == ApiCode);
            if (payment != null && Status != "-2")
            {
                // adderror("This payment processed before.")  hata mesagını dbye ekleyebilirsiniz
                // 
                return ResponseWrite("OK");
            }



            if (Status == "-2") // ödeme geri çekilmişse
            {
                if (payment != null && payment.Status != -2) //bu ödemede daha önce geri çekme işlemi yapılmamışsa
                {
                    // burada kullanıcının hesabından geri çekilen tutar değerinde ürünü silebilirsiniz ya da oyuncuyu banlayabilirsiniz
                    // geri çekme işlemi, daha önce ödemeyi onaylatıp sonra iptal etmesiyle oluyor, örneğin paypalda bu sorunlar olabiliyor.

                    // update payments set status=-2 where apicode=Apicode
                    // Update Accounts Set AccountStatus='Banned' where CustomerID=CustomerAccount
                }

                return ResponseWrite("OK");
            }
            else if (Status == "-1") // ödeme reddedilmişse
            {
                // insert into payments (customerid, description, message, status, amount, netamount, commission, payername, apicode, date) values ('CustomerAccount','Description', 'Message', '-2', 'Amount', 'NetAmount', 'Commission', 'PayerName', 'ApiCode', 'Date')
                // gelen bilgileri bu şekilde kendi ödeme tablonuza kaydedebilirsiniz geriye dönük kontrol yapmak için

                return ResponseWrite("OK");
            }
            else if (Status == "1") // ödeme onaylanmışsa
            {
                // Son aşama ödeme onaylanmışsa oyuncuya ürününü vermelisiniz ve daha sonra gelecek bildirimleri engellemek için ödemeyi kaydetmelisniz

                // insert into payments (customerid, description, message, status, amount, netamount, commission, payername, apicode, date) values ('CustomerAccount','Description', 'Message', '1', 'Amount', 'NetAmount', 'Commission', 'PayerName', 'ApiCode', 'Date')



                // NetAmount a göre ürün miktarını belirleyebilirsiniz

                // var urun = 0;
                // if(NetAmount > 50 && NetAmount < 100)
                //      urun = 10000;
                // else if(NetAmount > 100 && NetAmount < 250)
                //      urun = 25000;

                // Update Accounts Set Gamepoints += urun where CustomerID=CustomerAccount

                return ResponseWrite("OK");
            }
            return ResponseWrite("OK");
        }

    }
}