using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Courier_entry
    {
        public string loan_id { get; set; }
        public List<string> SelectedLoanNumber { get; set; }
        public SelectList LoanNumbers { get; set; }
        public List<SelectListItem> DocTypes { get; set; }
        public string SelectedDocType { get; set; }
        public string courier_company_name { get; set; }
        public string consignment_no { get; set; }
        public DateTime date_of_courier { get; set; }
    }

}