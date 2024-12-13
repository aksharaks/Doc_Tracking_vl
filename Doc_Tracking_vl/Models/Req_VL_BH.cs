using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Req_VL_BH
    {

        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public List<SelectListItem> DocTypes { get; set; }
        public List<ChecklistItem_gl_bh> ChecklistItems { get; set; }
        public List<SelectListItem> States { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string SelectedState { get; set; }
        public string SelectedBranch { get; set; }
        public string SelectedLoanType { get; set; }
        public string SelectedDocType { get; set; }
        public LoanDocRequ_Mortgage_BH_VL LoanDocsReq_vl_bh { get; set; }
    }
    public class LoanDocRequ_Mortgage_BH_VL
    {
        public List<LoanDoc_BH_vl> LoanDocs { get; set; }
    }
    public class LoanDoc_BH_vl
    {
        public string LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public bool IsChecked { get; set; }
        public string Remarks { get; set; }
    }

    // 
    // to save request loan doc

    public class Req_LoanDoc_Mort_vl_bh
    {
        public int LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public string IsChecked { get; set; }
        public string Remark { get; set; }
        public string SelectedLoanNumber { get; set; }
        public string SelectedDocType { get; set; }

    }
}