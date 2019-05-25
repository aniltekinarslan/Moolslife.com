using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace MoolsPayment.DAL
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            if (role.IndexOf(roles) > -1)
                return true;
            else if(role == "User" && roles == "Admin" || role == "User" && roles == "Editor")
                return true;

            return false;
        }

        public CustomPrincipal(string Username)
        {
            this.Identity = new GenericIdentity(Username);
        }
        public int AccountID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public decimal? Balance { get; set; }
        public string DealerCode { get; set; }
        public bool? IsActive { get; set; }
        public string roles { get; set; }
        public string IPAddress { get; set; }
    }

    public class CustomPrincipalSerializeModel
    {
        public int AccountID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public decimal? Balance { get; set; }
        public string DealerCode { get; set; }
        public bool? IsActive { get; set; }
        public string roles { get; set; }
        public string IPAddress { get; set; }
    }
}