using MoolsPayment.DAL;
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web.Mvc;
using MoolsPayment.Models;
using System.Text;
using Resources;
using System.Threading;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using System.Data.Entity;
using System.Web;

namespace MoolsPayment.Controllers
{
    public class BaseController : Controller
    {
        static public int PerPage = 100;
        static public int PerPageNews = 10;
        protected static int checkSessionTime = 60000; // minutes

        public string ToEng(string word)
        {
            word = word.Replace('ö', 'o');
            word = word.Replace('ü', 'u');
            word = word.Replace('ğ', 'g');
            word = word.Replace('ş', 's');
            word = word.Replace('ı', 'i');
            word = word.Replace('ç', 'c');
            word = word.Replace('Ö', 'O');
            word = word.Replace('Ü', 'U');
            word = word.Replace('Ğ', 'G');
            word = word.Replace('Ş', 'S');
            word = word.Replace('İ', 'I');
            word = word.Replace('Ç', 'C');

            return word;
        }

        public string GetErrorMessage(string errorCode, bool addNewline = true)
        {
            string errorMessage = null;

            switch (errorCode)
            {
                case "1":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid user. Please check your dealer code";
                    break;
                case "2":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=" + Home.ErrorMessage_2 + "<a href=\"/CancelPayment/\"><< " + Home.GoBack + "</a>";
                    break;
                case "3":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Token date expired. Please go back to payment page again." + "<a href=\"/CancelPayment/\"><< " + Home.GoBack + "</a>";
                    break;
                case "4":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Type";
                    break;
                case "5":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Different IpAddress";
                    break;
                case "6":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Token";
                    break;
                case "7":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Sequence length. (Max 10 character)";
                    break;
                case "51":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Identity cannot be null or whitespace";
                    break;
                case "52":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Identity length. Must be 11 character for Turkey";
                    break;
                case "61":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Phone length. Must be 10 character for Turkey. (Ex: 5323051199)";
                    break;
                case "62":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Phone input. Must be start with 5 for Turkey. (Ex: 5323051199)";
                    break;
                case "81":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid BankID. (Cannot be null or whitespace)";
                    break;
                case "82":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid BankID length. (Max 5 character)";
                    break;
                case "91":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Date. (Cannot be null or whitespace)";
                    break;
                case "92":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Date length. (Max 25 character)";
                    break;
                case "93":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Date format. (ex: 12.05.2016 17:32)";
                    break;
                case "94":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Session date expired. Please go back to your payment page again.";
                    break;
                case "99":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=You have 1 waiting payments. Please try again later.";
                    break;
                case "101":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid PayerName. (Cannot be null or whitespace)";
                    break;
                case "102":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid PayerName length. (Max 100 character)";
                    break;
                case "111":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Amount. (Cannot be null, whitespace, zero or decimal)";
                    break;
                case "112":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Amount length. (Max 8 character)";
                    break;
                case "113":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Identity. (must be only number";
                    break;
                case "114":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Identity length. (Must be last 5 chars or 11 chars of Identity)";
                    break;
                case "115":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_PhoneCountryCode. (Must be min 1, max 5 chars)";
                    break;
                case "116":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Phone. (Cannot be null or whitespace)";
                    break;
                case "117":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Phone length. (Must be Only 10 chars)";
                    break;
                case "118":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Phone length. (Must be last 5 chars or 11 chars of Identity)";
                    break;
                case "119":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_NameSurname. (Must be contains space char)";
                    break;
                case "120":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_NameSurname length. (min 6 chars and must be contains space char)";
                    break;
                case "121":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid customer_Email. (must be contains '@', '.' or can't contains space char)";
                    break;
                case "122":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid CustomerAccount. (Cannot be null or whitespace or zero)";
                    break;
                case "123":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid CustomerAccount length. (Max 140 character)";
                    break;
                case "124":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid CurrencyCode. (Cannot be null or whitespace or zero) (only [1=TRY], [2=USD], [3=EUR])";
                    break;
                case "125":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid CurrencyCode length. (Max 3 character) (only [1=TRY], [2=USD], [3=EUR])";
                    break;
                case "126":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=E-PIN kodu hatalı, lütfen tekrar deneyin.";
                    break;
                case "127":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Bu E-PIN kodunun parasal değeri, ödemeniz gerekenden miktardan daha düşük.";
                    break;
                case "128":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Bu E-PIN kodu ile satın almaya çalıştığınız ürün farklı. Lütfen doğru ürünü seçip bu kodu kullanınız.";
                    break;
                case "129":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Bu E-PIN kodu daha önce kullanılmış. Lütfen yeni bir E-PIN deneyin.";
                    break;
                case "136":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Code. (Cannot be null or whitespace or zero)";
                    break;
                case "137":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Code length. (Max 100 character)";
                    break;
                case "141":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Email. (Cannot be null or whitespace or zero)";
                    break;
                case "142":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid Email length. (Max 70 character)";
                    break;
                case "151":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid IP Address. (Cannot be null or whitespace or zero)";
                    break;
                case "152":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Invalid IP Address length. (Max 150 character)";
                    break;
                case "159":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Payment cannot be confirmed";
                    break;
                case "160":
                    errorMessage = "ErrorCode=" + errorCode + ", Message=Ödeme sırasında bir sorun oluştu, lütfen tekrar deneyiniz.";
                    break;
                default:
                    errorMessage = "";
                    break;
            }


            return errorMessage + (addNewline == false ? "" : " nnnn");
        }

        public string PostData(string link, string postData)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = (HttpWebRequest)WebRequest.Create(link);
                var data = Encoding.UTF8.GetBytes(postData);
                request.UserAgent = "MoolsLife";
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                        request.KeepAlive = false;

                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK && response.GetResponseStream() != null)
                    return new StreamReader(response.GetResponseStream()).ReadToEnd();
                else
                    return "Error: " + response.StatusCode;
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }       
        }

        public string ShowMessage(string token, string codes, string message, bool isError = true, string redirectUrl = "")
        {
            if (isError)
            {
                if (!codes.IsNullOrWhiteSpace())
                {
                    Response.StatusCode = 307;
                    Session["errorCode"] = EncryptBasic(codes);

                    if (RealIpAddress() == "::1" || RealIpAddress() == "127.0.0.1")
                        Response.RedirectPermanent("http://localhost:11681/ShowMethods/" + token);
                    else
                        Response.RedirectPermanent("https://www.moolslife.com/ShowMethods/" + token);

                    return null;
                }
                else
                    return message;
            }
            else
            {
                if (!redirectUrl.IsNullOrWhiteSpace())
                {
                    Response.StatusCode = 307;
                    Response.RedirectPermanent(redirectUrl);
                    return null;
                }
                else
                    return message;
            }
        }

        public string GetSha256FromString(string strData)
        {
            var message = Encoding.ASCII.GetBytes(strData);
            SHA256Managed hashString = new SHA256Managed();
            string hex = "";

            var hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }

        public static string HashHmac(string message, string secret)
        {
            Encoding encoding = Encoding.UTF8;
            using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(secret)))
            {
                var msg = encoding.GetBytes(message);
                var hash = hmac.ComputeHash(msg);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }

        public string SHACrypt(string value, string merchant_key)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
            var hash = hmac.ComputeHash(valueBytes);
            return Convert.ToBase64String(hash);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string SonteklifToken(string kullaniciId, string ip, string servisId)
        {
            Encoding e = Encoding.UTF8;
            string Birlestir = string.Concat(kullaniciId, ip);
            HMACSHA256 hmac = new HMACSHA256(e.GetBytes(servisId));
            byte[] b = hmac.ComputeHash(e.GetBytes(Birlestir));
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                s.Append(b[i].ToString("x2"));
            }
            string token1 = s.ToString();
            return token1;
        }

        public string SonteklifHash(string urunId, string satisKodu, string adet, string servisId)
        {
            Encoding e = Encoding.UTF8;
            string Birlestir = string.Concat(satisKodu, urunId, adet);
            HMACSHA256 hmac = new HMACSHA256(e.GetBytes(servisId));
            byte[] b = hmac.ComputeHash(e.GetBytes(Birlestir));
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < b.Length; i++)
            {
                s.Append(b[i].ToString("x2"));
            }

            string out1 = s.ToString();
            return out1;
        }

        public object CheckIsValidUser(string DealerCode)
        {
            var user = _db.Accounts.FirstOrDefault(a => a.DealerCode == DealerCode);
            if (user == null)
                return GetErrorMessage("1", false);

            return user;
        }

        public Payments InsertNewPayment(Links link, decimal amount, PaymentTypes type, MethodIDs bankId, string description, string payerName, DateTime? date)
        {
            var bank = _db.Methods.FirstOrDefault(b => b.MethodID == (int)bankId && b.IsActive == true);
            if (bank == null)
            {
                AddErrorLog(null, "bank", "DealerCode=" + link.Accounts.DealerCode);
                return null;
            }

            if (_db.Payments.Any(p=>p.LinkID == link.LinkID)) //test
            {
                AddErrorLog(null, "Same link, 2 payment", " DealerCode=" + link.Accounts.DealerCode);
                return null;
            }

            try
            {
                var payment = new Payments
                {
                    AccountID = link.AccountID,
                    MethodID = (int)bankId,
                    Type = (int)type,
                    Amount = amount,
                    Date = date ?? DateTime.Now,
                    Status = (short)PaymentStatus.Waiting,
                    ApiCode = CreateApiCode(link.Accounts.DealerCode, link.AccountID, DateTime.Now),
                    Description = description.IsNullOrWhiteSpace() ? null : description,
                    PayerName = payerName.IsNullOrWhiteSpace() ? null : payerName,
                    Commission = 0,
                    ProcessDate = null,
                    Message = null,
                    IsSend = false,
                    IsCancelled = false,
                    SendDate = null,
                    LinkID = link.LinkID
                };

                _db.Payments.Add(payment);

                link.PaymentID = payment.PaymentID; //test
                _db.Links.Attach(link);
                _db.Entry(link).State = EntityState.Modified;

                _db.SaveChanges();

                return payment;
            }
            catch (Exception)
            {
                AddErrorLog(null, "Error on db", "DealerCode=" + link.Accounts.DealerCode + " / OrderID=" + link.OrderID + " / Type=" + type + " / BankID=" + bankId + " / Amount=" + amount);
                Response.Write("error");
                return null;
            }
        }

        public string GetStringFromResourceWithName(string name)
        {
            return Home.ResourceManager.GetString(name, CultureInfo.CurrentCulture);
        }

        public string GetCurrentLanguage()
        {
            var lang = "en";
            if (Thread.CurrentThread.CurrentUICulture.Name.Contains("tr") || Thread.CurrentThread.CurrentUICulture.Name.Contains("TR"))
                lang = "tr";

            return lang;
        }

        public string GetCurrentControllerName()
        {
            return Request.RequestContext.RouteData.Values["controller"].ToString();
        }

        public string GetCurrentActionName()
        {
            return Request.RequestContext.RouteData.Values["action"].ToString();
        }

        public bool IsAuthenticated()
        {
            return Request.IsAuthenticated;
        }

        public void ReleaseSessions(bool deadLink = false)
        {
            Session["clickItem"] = null;
            if (deadLink)
            {
                try
                {
                    var token = Session["Token"].ToString();
                    var link = _db.Links.FirstOrDefault(l => l.Token == token);

                    if (link != null && link.IsDead == false)
                    {
                        link.IsDead = true;//test
                        _db.Links.Attach(link);
                        _db.Entry(link).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    Session["Token"] = null;
                }
                catch (Exception)
                {
                }
            }
        }

        public string GetHashedPassword(string pass)
        {
            //get crypted password from db
            Hashtable parameters = new Hashtable();
            DataAccessor accessor = new DataAccessor();
            parameters.Add("@in_Password", pass);
            DataRow rowAccount = accessor.Single("[dbo].[FN_MD5Pass]", parameters, CommandType.StoredProcedure);

            if (rowAccount != null)
                return rowAccount[0].ToString();

            return "";
        }

        public string CreateNewDbHash(string code)
        {
            var md5 = new MD5CryptoServiceProvider();
            var originalBytes = ASCIIEncoding.Default.GetBytes(code + DateTime.Now.Millisecond);
            var encodedBytes = md5.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToUpper(); ;
        }

        protected virtual new CustomPrincipal User
        {
            get { return HttpContext.User as CustomPrincipal; }
        }

        public bool SendEmail(string email, EmailTypes type, string code = "")
        {

            string subj = Home.SiteName + " - ";

            if (type == EmailTypes.ChangePassword)
                subj += Home.ChangePassword + "!";
            else if (type == EmailTypes.ForgotPassword)
                subj += Home.ForgotPassword + "!";
            else if (type == EmailTypes.AfterForgotPassword)
                subj += Home.YourPasswordChanged + "!";
            else if (type == EmailTypes.TicketAnswer)
                subj += code + Home.TicketNumberEmail + Home.YourTicketAnswered + "!";

            string url = "";
            var user = _db.Accounts.Where(u => u.Email == email);

            if (type == EmailTypes.ForgotPassword)
            {
                url = "<a href='http://api." + Home.SiteDomain + "/ResetPassword/" + code + "'>" + Home.AcceptResetPassword + "</a>";
            }


            else if (type == EmailTypes.AfterForgotPassword || type == EmailTypes.ChangePassword)
            {
                if (user.FirstOrDefault() == null || user.Count() > 1)
                {
                    ViewBag.ErrorMessage = Home.CantFindAccount + ": <br> <a href='/Support' target='_blank'>www." + Home.SiteDomain + "/Support</a>";
                    return false;
                }

                url = "<a href='#'>" + Home.NewChangedPasswordInformation + "</a>";
            }


            else if (type == EmailTypes.TicketAnswer)
            {

                if (user.FirstOrDefault() == null || user.Count() > 1)
                {
                    ViewBag.ErrorMessage = Home.InformationEmailNotFoundorMultiple;
                    return false;
                }

                url = "<a href='http://api." + Home.SiteDomain + "/TicketDetail/" + code + "'>" + Home.YourTicketAnsweredInformation;
                url += "</a>";
            }



            string body = "";

            body = @"<!doctype html>
            <html>
            <head>
            <meta charset='utf-8'>
            <title>" + Home.SiteName + @" Email</title>
            <style type='text/css'>
            body { font-family:arial;  font-size:14px; line-height:21px; color:#ffffff; }

            p  {font-size:14px; line-height:21px; color:#ffffff; }
            a { text-decoration:none; color:#ffffff; }

             </style>
            </head>
            <body>
            <p>&nbsp;</p>
            <table width='800' border='0' align='center' cellpadding='0' cellspacing='0' style='margin:0px; padding:0px; font-size: 36px;'>
              <tbody>
                <tr>
                  <td height='100px' colspan='2' bgcolor=''> 
	  
	              <table width='100%' border='0' cellspacing='0' cellpadding='15'>
                    <tbody>
                      <tr>
                        <td>
                            <center>
                              <img src='http://api." + Home.SiteDomain + @"/Content/images/logo_big.png'/>
                            </center>
                        </td>
                      </tr>
                    </tbody>
                  </table>
	  
	              </td>
                </tr>
                <tr>
                  <td colspan='2' valign='top' background='http://api." + Home.SiteDomain + @"/Content/images/mail/back.png'> <table width='100%' border='0' cellspacing='0' cellpadding='20'>
                    <tbody>
                       <tr>
                        <td>
           
                       <center>
		               "
                    +
                   url
                    +
                   @"
		               </center>
              

                       </td>
                      </tr>
                    </tbody>
                  </table></td>
                </tr>
              </tbody>
            </table>
            </body>
            </html>";

            return SendEmail(email, subj, body);
        }

        public bool SendEmail(string email, string subj, string body)
        {
            try
            {
                MailMessage mail = new MailMessage(Home.SiteEmail, email);
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = WebConfigurationManager.AppSettings["smtp_host"];
                client.Port = Convert.ToInt32(WebConfigurationManager.AppSettings["smtp_port"]);
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(WebConfigurationManager.AppSettings["smtp_email"], WebConfigurationManager.AppSettings["smtp_password"]);
                mail.IsBodyHtml = true;
                mail.Subject = subj;
                mail.Body = body;
                client.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string RandomString(int length)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, length).ToUpper();
        }

        public string EncryptBasic(string input)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(input.Replace('+', '-').Replace('/', '_')));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string DecryptBasic(string input)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/')));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string EncryptStrong(string plainText)
        {
            string passPhrase = "2Pa1s5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "MD5";
            int passwordIterations = 2;
            string initVector = "@1B2c3D54e5F6g7H8";
            int keySize = 128;

            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);
            return cipherText;
        }

        public string DecryptStrong(string cipherText)
        {
            string passPhrase = "Pas5pr@se";
            string saltValue = "s@1tValue";
            string hashAlgorithm = "MD5";
            int passwordIterations = 2;
            string initVector = "@1B2c3D4e5F6g7H8";
            int keySize = 128;

            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return plainText;
        }

        public ActionResult ResponseWrite(object Message, bool useJson = false)
        {
            Response.Write(Message);
            return null;
        }

        public decimal CalcCommission(Payments payment)
        {
            decimal netCommission = 0;
            netCommission = Decimal.Round(payment.Methods.Commission * ((100 - payment.Accounts.Discount) / 100), 2);

            var accountMethodCommission = _db.MethodCommissionTree.Where(mc => mc.AccountID == payment.AccountID && mc.MethodID == payment.MethodID).FirstOrDefault();
            if (accountMethodCommission != null)
                netCommission = accountMethodCommission.Commission;

                return payment.Amount <= 0 || payment.Status != (short)PaymentStatus.Approved ? 0 : payment.Amount - Decimal.Round(payment.Amount * ((100 - netCommission) / 100), 2); ;
        }

        public object CheckIsValidPayment(string apiCode)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.ApiCode == apiCode);

            if (payment != null)
                return payment;

            return new { ErrorCode = "1", Message = "Bad payment, please check your apicode. " };
        }

        public string RealIpAddress()
        {
            string ip;
            
            if (!Request.ServerVariables["HTTP_CLIENT_IP"].IsNullOrWhiteSpace())
                ip = Request.ServerVariables["HTTP_CLIENT_IP"];
            else if (!Request.ServerVariables["HTTP_X_FORWARDED_FOR"].IsNullOrWhiteSpace())
                ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',').Last().Trim();
            else
                ip = Request.ServerVariables["REMOTE_ADDR"];

            //ip = "185.124.86.219";
            return ip;
        }

        public string CreateApiCode(string dealerCode, int accountId, DateTime date)
        {
            Random rnd = new Random();
            var md5 = new MD5CryptoServiceProvider();
            var originalBytes = Encoding.Default.GetBytes(dealerCode + rnd.Next(1, 900) + accountId + rnd.Next(1, 900) + date + DateTime.Now.Millisecond + RandomString(3));
            var encodedBytes = md5.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToUpper(); ;
        }


        public string CreateEpinCode(string productId)
        {
            return productId + RandomString(3) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(4);
        }

        public bool PaymentProcess(Payments payment, PaymentStatus status, string message, decimal newAmount = 0, string description = null)
        {
            try
            {
                if (newAmount != 0)
                    payment.Amount = newAmount;

                bool isBonus = (description == "Bonus Epin");

                if (description != null)
                    payment.Description = description;

                payment.Status = Convert.ToInt16(status);
                payment.Message = message;
                payment.ProcessDate = DateTime.Now;
                payment.Commission = CalcCommission(payment);
                payment.IsSend = false;
                payment.SendDate = null;

                _db.Payments.Attach(payment);
                _db.Entry(payment).State = EntityState.Modified;
                _db.SaveChanges();

                if (status == PaymentStatus.Approved || status == PaymentStatus.Refunded)
                {
                    if (!NewTransaction(payment, isBonus))
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool NewTransaction(Payments payment, bool isBonus = false)
        {
            try
            {
                var user = _db.Accounts.FirstOrDefault(a => a.AccountID == payment.AccountID);
                if (user == null)
                    return false;

                var netAmount = payment.Amount <= 0 && payment.Type == (int)PaymentTypes.Paypal ? payment.Amount : payment.Amount - payment.Commission;
                var newAmount = user.Balance;

                if (!isBonus)
                    newAmount += netAmount;

                var transaction = new AccountTransaction
                {
                    AccountID = user.AccountID,
                    Date = DateTime.Now,
                    PaymentID = payment.PaymentID,
                    OldBalance = user.Balance,
                    NewBalance = newAmount,
                    Usable = false
                };


                user.Balance = newAmount;

                _db.Accounts.Attach(user);
                _db.Entry(user).State = EntityState.Modified;

                _db.AccountTransaction.Add(transaction);
                _db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddNotifyLog(string from, string content)
        {
            try
            {
                var l = new NotifyLogs
                {
                    From = from,
                    Content = content,
                    IpAddress = RealIpAddress(),
                    Date = DateTime.Now
                };

                _db.NotifyLogs.Add(l);
                _db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                AddErrorLog(null, "NotifyLog Failed From: " + from, e.Message + " Content: " + content);
            }

            return false;
        }

        public string AddErrorLog(Payments payment, string errorCode, string message)
        {
            var responseText = "";
            try
            {
                if (payment != null)
                {
                    var pE = _db.ErrorLog.Where(p => p.PaymentID == payment.PaymentID && p.ErrorCode == errorCode);

                    if (pE.Any() && pE.Count() == 1)
                    {
                        var peFirst = pE.FirstOrDefault();

                        peFirst.Message_ = message;
                        peFirst.LastDate = DateTime.Now;

                        _db.ErrorLog.Attach(peFirst);
                        _db.Entry(peFirst).State = EntityState.Modified;
                    }
                    else
                    {
                        var error = new ErrorLog
                        {
                            AccountID = payment.AccountID,
                            PaymentID = payment.PaymentID,
                            ErrorCode = errorCode,
                            Message_ = message,
                            LastDate = DateTime.Now,
                        };
                        _db.ErrorLog.Add(error);
                    }
                }
                else
                {
                    var error = new ErrorLog
                    {
                        AccountID = null,
                        PaymentID = null,
                        ErrorCode = errorCode,
                        Message_ = message,
                        LastDate = DateTime.Now,
                    };
                    _db.ErrorLog.Add(error);
                }

                _db.SaveChanges();
            }
            catch (Exception)
            {
                responseText += "Failed on create new error. Can't write the error log on DB.";
            }

            responseText += "\n\n" + "PaymentID=" + (payment?.PaymentID ?? null) + ", ErrorCode=" + errorCode + ", Message=" + message;

            return responseText;
        }

        public enum PaymentTypes
        {
            ATM = 1,
            HavaleEFT = 2,
            BakiyeTransferArtı = 3,
            BakiyeTransferEksi = 4,
            Elden = 5,
            BakiyeÇek = 6,
            KrediKartı = 7,
            G2A = 8,
            GPay = 9,
            Paypal = 10,
            PayTR = 11,
            Sonteklif = 12,
            Mobil = 13,
            MangırKart = 14,
            Teckcard = 15,
            TTNET = 16,
            Xsolla = 17,
            MoolsEpin = 18
        };

        public enum MethodIDs
        {
            GPay_DenizBank = 9,
            GPay_FinansBank = 10,
            GPay_IngBank = 11,   // Pasif
            GPay_KuveytTürk = 12,
            GPay_TEB = 13,
            GPay_VakıfBank = 14,
            GPay_YapıKredı = 15,
            GPay_PTT = 17,
            GPay_Akbank = 19,
            GPay_ZiraatBankası = 50, // Pasif
            GPay_GarantiBankası = 51,    // Pasif
            GPay_İşBankası = 52,
            GPay_İşBankası2 = 56, // Pasif
            Trinkpay_ZiraatBankası = 100,
            Trinkpay_İşBankası = 105,    // Pasif
            Trinkpay_GarantiBankası = 106,
            Hyper_ZiraatBankası = 111,   // Pasif
            Hyper_İşBankası = 112,   // Pasif
            Hyper_GarantiBank = 113,
            Hyper_FinansBank = 114,  // Pasif
            Hyper_PTT = 115,
            Hyper_HalkBankası = 116, // Pasif
            Hyper_YapıKredi = 117,
            Hyper_Akbank = 118,
            PayTR_ZiraatBankası = 201,
            PayTR_İşBankası = 202,
            PayTR_Akbank = 203,
            PayTR_YapıKredi = 204,
            PayTR_DenizBank = 205,
            PayTR_FinansBank = 206,
            PayTR_HalkBank = 207,
            PayTR_PTT = 208,
            PayTR_TEB = 209,
            PayTR_VakıfBank = 210,
            PayTR_KrediKartı = 211,
            PayTR_KuveytTürk = 212,
            KrediKartı = 5000,
            G2A = 5001,
            GPay = 5002,
            Paypal = 5003,
            PayTR = 5004,
            Sonteklif = 5005,
            Mobil = 5006,
            MangırKart = 5007,
            Teckcard = 5008,
            TTNET = 5009,
            Xsolla = 5010,
            MoolsEpin = 5011,
            HyperTeknolojiATMHavaleEFT = 5012,
            TrinkpayTechnologyATMHavaleEFT = 5013
        };

        public enum PaymentStatus
        {
            Refunded = -2,
            Rejected = -1,
            Waiting = 0,
            Approved = 1
        };

        public enum PaymentCurrency
        {
            ALL = 0,
            TRY = 1,
            USD = 2,
            EUR = 3
        };

        public enum EmailTypes
        {
            ForgotPassword,
            ChangePassword,
            AfterForgotPassword,
            TicketAnswer
        };

        public MoolsLifeEntities _db = new MoolsLifeEntities();
    }
}