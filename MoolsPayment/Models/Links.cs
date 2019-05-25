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
    
    public partial class Links
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Links()
        {
            this.Payments1 = new HashSet<Payments>();
        }
    
        public int LinkID { get; set; }
        public int AccountID { get; set; }
        public string Token { get; set; }
        public System.DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public string IpAddress { get; set; }
        public string OrderID { get; set; }
        public string product_Code { get; set; }
        public string product_Name { get; set; }
        public System.DateTime GeneratedDate { get; set; }
        public bool IsDead { get; set; }
        public Nullable<int> PaymentID { get; set; }
        public string customer_Identity { get; set; }
        public string customer_Phone { get; set; }
        public string customer_NameSurname { get; set; }
        public string customer_Email { get; set; }
        public string customer_PhoneCountryCode { get; set; }
        public string product_ImageUrl { get; set; }
    
        public virtual Accounts Accounts { get; set; }
        public virtual PaymentCurrency PaymentCurrency { get; set; }
        public virtual Payments Payments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payments> Payments1 { get; set; }
    }
}
