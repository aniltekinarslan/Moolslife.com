using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
using Resources;
using MoolsPayment.Controllers;

namespace MoolsPayment.Models
{
    public class ShowMethodsViewModel
    {
        public PaypalViewModel paypal { get; set; }
    }

    public class PaypalViewModel
    {
        [StringLength(100)]
        public string business { get; set; }

        [StringLength(100)]
        public string cmd { get; set; }

        [StringLength(100)]
        public string currency_code { get; set; }

        [StringLength(255)]
        public string return_url { get; set; }
     
        [StringLength(255)]
        public string cancel_url { get; set; }

        [StringLength(255)]
        public string ipn_notification_url { get; set; }
 
        [StringLength(140)]
        public string item_name { get; set; }
 
        [StringLength(10)]
        public string amount { get; set; }

        [StringLength(100)]
        public string item_number { get; set; }

        [StringLength(100)]
        public string custom { get; set; }
    }
}