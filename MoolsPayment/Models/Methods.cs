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
    
    public partial class Methods
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Methods()
        {
            this.MethodCommissionTree = new HashSet<MethodCommissionTree>();
            this.MethodPreference = new HashSet<MethodPreference>();
            this.Methods1 = new HashSet<Methods>();
            this.Payments = new HashSet<Payments>();
        }
    
        public int MethodID { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string AccountHolder { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string IbanCode { get; set; }
        public bool IsAtmActive { get; set; }
        public decimal AtmFee { get; set; }
        public string AtmDescription { get; set; }
        public int SortOrder { get; set; }
        public decimal Commission { get; set; }
        public int Currency { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> MainMethodID { get; set; }
        public string Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MethodCommissionTree> MethodCommissionTree { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MethodPreference> MethodPreference { get; set; }
        public virtual PaymentCurrency PaymentCurrency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Methods> Methods1 { get; set; }
        public virtual Methods Methods2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payments> Payments { get; set; }
    }
}
