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
    
    public partial class LoginLog
    {
        public int LoginID { get; set; }
        public int AccountID { get; set; }
        public string Ip { get; set; }
        public string SessionID { get; set; }
        public System.DateTime Date { get; set; }
        public bool Status { get; set; }
        public Nullable<decimal> Balance1 { get; set; }
        public Nullable<decimal> Balance2 { get; set; }
    
        public virtual Accounts Accounts { get; set; }
    }
}
