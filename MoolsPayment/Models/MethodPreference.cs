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
    
    public partial class MethodPreference
    {
        public int MethodPreferenceID { get; set; }
        public int AccountID { get; set; }
        public int MethodID { get; set; }
    
        public virtual Accounts Accounts { get; set; }
        public virtual Methods Methods { get; set; }
    }
}
