using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Checklist_Model_VL_BH
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public List<SelectListItem> DocTypes { get; set; }
        public List<ChecklistItem_vl_bh> ChecklistItems { get; set; }
        public List<SelectListItem> States { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string SelectedState { get; set; }
        public string SelectedBranch { get; set; }
        public string SelectedLoanType { get; set; }
        public string SelectedDocType { get; set; }
        //public PropertyDocModel_sme_bh PropertyDocsModel { get; set; }
        public LoanDocModel_Mortgage_vl_bh LoanDocsModel { get; set; }
       // public LoanDocModel_Tranche_sme_bh LoanDocsModel_tranche { get; set; }
      //  public PropertyDocModel_sme_bh_tranche PropertyDocsModel_tranche { get; set; }
    }
}
public class ChecklistItem_vl_bh
{
    public string DocumentsEntry { get; set; }
    public bool IsChecked { get; set; }
    public int Index { get; set; }
    public int Status { get; set; }
}
public class LoanDocModel_Mortgage_vl_bh
{
    public List<LoanDoc_vl_bh> LoanDocs { get; set; }
}

public class LoanDoc_vl_bh
{
    public string LoanDocId { get; set; }
    public string LoanDocName { get; set; }
    public bool IsChecked { get; set; }
    public string Remarks { get; set; }
    public string Maker_Status { get; set; }
}

public class LoanDocumentsMortgage
{
    public int LoanDocId { get; set; }
    public string LoanDocName { get; set; }
    public string IsChecked { get; set; }
    public string Remark { get; set; }
}
public class LoanDocumentsMortgage_vl_bh
{
    public int LoanDocId { get; set; }
    public string LoanDocName { get; set; }
    public string IsChecked { get; set; }
    public string Remark { get; set; }
    public string SelectedLoanNumber { get; set; }
    public string SelectedDocType { get; set; }
}