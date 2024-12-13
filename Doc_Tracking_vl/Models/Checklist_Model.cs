using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Models
{
    public class Checklist_Model
    {
        public string loan_id { get; set; }
        public string SelectedLoanNumber { get; set; }
        public List<SelectListItem> LoanNumbers { get; set; }
        public List<ChecklistItem> ChecklistItems { get; set; }
        public List<SelectListItem> States { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string SelectedState { get; set; }
        public string SelectedBranch { get; set; }
        public string SelectedLoanType { get; set; }
        //public PropertyDocModel PropertyDocsModel { get; set; }




    }

    public class ChecklistItem
    {
        public string DocumentsEntry { get; set; }
        public bool IsChecked { get; set; }
        public int Index { get; set; }
        public int Status { get; set; }

    }
}