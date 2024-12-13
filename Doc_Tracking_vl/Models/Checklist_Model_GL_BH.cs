using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Checklist_Model_GL_BH
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
     //   public PropertyDocModel_gl_bh PropertyDocsModel { get; set; }
        public LoanDocModel_Mortgage_gl_bh LoanDocsModel { get; set; }
        //public LoanDocModel_Tranche_gl_bh LoanDocsModel_tranche { get; set; }
       // public PropertyDocModel_gl_bh_tranche PropertyDocsModel_tranche { get; set; }
    }


    public class ChecklistItem_gl_bh
    {
        public string DocumentsEntry { get; set; }
        public bool IsChecked { get; set; }
        public int Index { get; set; }
        public int Status { get; set; }
    }

    public class LoanDocModel_Mortgage_gl_bh
    {
        public List<LoanDoc_gl_bh> LoanDocs { get; set; }
    }
    public class LoanDoc_gl_bh
    {
        public string LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public bool IsChecked { get; set; }
        public string Remarks { get; set; }
        public string Checker2_Status { get; set; }
    }
}
