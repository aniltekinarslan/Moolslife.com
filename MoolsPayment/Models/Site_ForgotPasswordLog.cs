//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MoolsPayment.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Site_ForgotPasswordLog
    {
        public int ForgotPasswordID { get; set; }
        public int AccountID { get; set; }
        public string Email { get; set; }
        public string Ip { get; set; }
        public System.DateTime Date { get; set; }
        public string Hash { get; set; }
        public bool Status { get; set; }
    
        public virtual Accounts Accounts { get; set; }
    }
}
