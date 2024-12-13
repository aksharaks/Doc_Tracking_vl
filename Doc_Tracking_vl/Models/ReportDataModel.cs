using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Doc_Tracking_vl.Models
{
    public class ReportDataModel
    { 
        public string LoanNo { get; set; }
        public string BranchName { get; set; }
        public string TypeOfDocs { get; set; }
        public string Checker4Ho { get; set; }
        public string CourierCompanyName { get; set; }
        public string ConsignmentNo { get; set; }
        public DateTime? DateOfCourier { get; set; }
        public DateTime? DateOfReceival { get; set; }
    }


    public class ReportDataModel_Total
    {
        public string LoanNo { get; set; }
        public string BranchName { get; set; }
        public string TypeOfDocs { get; set; }
        public string CustName { get; set; }
        public DateTime? LoanDate { get; set; }
        public string LoanAmount { get; set; }
        public string DocsCustody { get; set; }
        public string Maker { get; set; }
        public DateTime? MakerDate { get; set; }
        public string Checker1Smebh { get; set; }
        public DateTime? Checker1Date { get; set; }
        public string Checker2Glabh { get; set; }
        public DateTime? Checker2Date { get; set; }
        public string Checker3Glbh { get; set; }
        public DateTime? Checker3Date { get; set; }
        public string Checker4Ho { get; set; }
        public DateTime? Checker4Date { get; set; }
    }

    public class ReportDataModel_Daily
    {
        public string LoanNo { get; set; }
        public string ApplicationId { get; set; }
        public string CustomerId { get; set; }
        public string BranchName { get; set; }
        public string TypeOfDocs { get; set; }
        public string CustName { get; set; }
        public DateTime? LoanDate { get; set; }
        public string LoanAmount { get; set; }
        public string RecedByLoanConfirmation { get; set; }
        public string UnderCustodyAfterDisb { get; set; }
        public string RecedByAfterDisb { get; set; }
        public string UnderCustodyFromBranch { get; set; }
        public string RecedByTransferFromBranch { get; set; }
        public string CourIdTranBranch { get; set; }
        public string CourIdRecvdBy { get; set; }
        public string UnderCustodyCourRcvdBy { get; set; }
        public string CourierRecvdBy { get; set; }

        public string DocsCustody { get; set; }
        public string Maker { get; set; }
        public DateTime? MakerDate { get; set; }
        public string Checker1Smebh { get; set; }
        public DateTime? Checker1Date { get; set; }
        public string Checker2Glabh { get; set; }
        public DateTime? Checker2Date { get; set; }
        public string Checker3Glbh { get; set; }
        public DateTime? Checker3Date { get; set; }
        public string Checker4Ho { get; set; }
        public DateTime? Checker4Date { get; set; }
    }
}
