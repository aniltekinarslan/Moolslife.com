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
    
    public partial class MoolsEpinUsedLog
    {
        public int MoolsEpinUsedLogID { get; set; }
        public int MoolsEpinID { get; set; }
        public decimal OldEpinBalance { get; set; }
        public decimal NewEpinBalance { get; set; }
        public System.DateTime Date { get; set; }
        public int PaymentID { get; set; }
    
        public virtual MoolsEpin MoolsEpin { get; set; }
        public virtual Payments Payments { get; set; }
    }
}
