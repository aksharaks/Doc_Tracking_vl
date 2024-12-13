using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Doc_Tracking_vl.Models
{
    public class LoanDocModel
    {

    }

    public class LoanDoc
    {
        public string LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public bool IsChecked { get; set; }
        public string Remarks { get; set; }
    }

    public class LoanDocModel_Mortgage
    {
        public List<LoanDoc> LoanDocs { get; set; }
    }

    public class LoanDocumentsMortgage
    {
        public int LoanDocId { get; set; }
        public string LoanDocName { get; set; }
        public string IsChecked { get; set; }
        public string Remark { get; set; }
    }

}