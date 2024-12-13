using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Settle_HO_Direct
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }

    }

    

    public class SettlementItem
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public string DocListId { get; set; }
        public string ListItems { get; set; }
        public bool? IsChecked { get; set; }
        public string Type { get; set; } // Dropdown options for ID proof and signed by
        public List<SelectListItem> ApplicantTypes { get; set; }
        public List<SelectListItem> IdTypes { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public string Details { get; set; }
        public string handoverto { get; set; }
        public DateTime? CourierDate { get; set; } // For storing courier date
        public string CourierConsignmentNo { get; set; }
        public string BranchName { get; set; }
    }

    public class SettleFormModel
    {
        public List<SettlementItem> DirectDocs { get; set; }
    }
    public class SettleFormModel_Normal
    {
        public List<SettlementItem_Normal> CourDocs { get; set; }
    }

    public class SettlementItem_Normal
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public string DocListId { get; set; }
        public string ListItems { get; set; }
        public string Details { get; set; }
        public bool? IsChecked { get; set; }
        public string Type { get; set; } // Dropdown options for ID proof and signed by
        public List<SelectListItem> ApplicantTypes { get; set; }
        public List<SelectListItem> IdTypes { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }



        public string BranchName { get; set; } // For storing branch name
        public DateTime? CourierDate { get; set; } // For storing courier date
        public List<SelectListItem> BranchNames { get; set; }

        public string handoverto { get; set; }
        public string CourierConsignmentNo { get; set; }
        public string CourierCompanyName { get; set; }
    }
    public class settle_loan_no
    {
        public string loan_branch_name { get; set; }
        public string loan_number { get; set; }
        public string settle_branch_name { get; set; }
        public DateTime settle_date { get; set; }
        public string settle_status { get; set; }
        public string settlement_type {  get; set; }
    }

}