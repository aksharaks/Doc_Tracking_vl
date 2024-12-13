using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class HO_entry
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public List<SelectListItem> DocTypes { get; set; }
        public string SelectedDocType { get; set; }
        public string courier_company_name { get; set; }
        public string consignment_no { get; set; }
        public DateTime? date_of_courier { get; set; }
        public DateTime? date_of_receival { get; set; }
        public string SelectedLoanType { get; set; }
       
        public LoanDocModel_Mortgage_ho LoanDocsModel { get; set; }
        
    }
    public class LoanDocModel_Mortgage_ho
    {
        public List<LoanDoc_ho> LoanDocs { get; set; }
    }
    public class LoanDoc_ho
    {
        public string LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public bool IsChecked { get; set; }
        public string Remarks { get; set; }
    }


    // to save loan 
    public class LoanDocumentsMortgage_ho
    {
        public int LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public string IsChecked { get; set; }
        public string Remark { get; set; }
        public string SelectedLoanNumber { get; set; }
        public string SelectedDocType { get; set; }
    }
}