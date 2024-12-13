using Doc_Tracking_vl.App_Code;
using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Controllers
{
    public class Reports_HOController : Controller
    {
        // GET: Reports_HO


        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        public ActionResult Daily_File_Rpt()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GenerateReport_Daily(DateTime startDate, DateTime endDate)
        {
            List<ReportDataModel_Daily> reportData = FetchReportData_Daily(startDate, endDate);

            return PartialView("_Daily_file_Rpt_Partial", reportData);
        }


        private List<ReportDataModel_Daily> FetchReportData_Daily(DateTime startDate, DateTime endDate)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            List<ReportDataModel_Daily> reportData = new List<ReportDataModel_Daily>();

            using (OracleConnection connection = new OracleConnection(constr))
            {
                string sqlQuery = @"SELECT
    b.branch_name,
    l.application_id,
    d.loan_no,
    l.customer_id,
    l.customer_name,
    l.loan_date,
    l.loan_amount,
    d.type_of_docs,
    CONCAT(
        COALESCE(e.emp_name, ''),
        CASE WHEN e.emp_name IS NOT NULL AND e1.emp_name IS NOT NULL THEN ' / ' || e1.emp_name ELSE '' END
    ) AS reced_by_loan_confirmation,
    CONCAT(
        COALESCE(e.emp_name, ''),
        CASE WHEN e.emp_name IS NOT NULL AND e1.emp_name IS NOT NULL THEN ' / ' || e1.emp_name ELSE '' END
    ) AS under_custody_after_disb,
    CONCAT(
        COALESCE(e2.emp_name, ''),
        CASE WHEN e2.emp_name IS NOT NULL AND e3.emp_name IS NOT NULL THEN ' / ' || e3.emp_name ELSE '' END
    ) AS reced_by_after_disb,
    CONCAT(
        COALESCE(e2.emp_name, ''),
        CASE WHEN e2.emp_name IS NOT NULL AND e3.emp_name IS NOT NULL THEN ' / ' || e3.emp_name ELSE '' END
    ) AS under_custody_from_branch,
    CONCAT(
        COALESCE(e.emp_name, ''),
        CASE WHEN e.emp_name IS NOT NULL AND e1.emp_name IS NOT NULL THEN ' / ' || e1.emp_name ELSE '' END
    ) AS reced_by_transfer_from_branch,
    d.consignment_no as cour_id_tran_branch,
     d.consignment_no as cour_id_recvd_by,
    CONCAT(
        COALESCE(e.emp_name, ''),
        CASE WHEN e.emp_name IS NOT NULL AND e1.emp_name IS NOT NULL THEN ' / ' || e1.emp_name ELSE '' END
    ) AS under_custody_cour_rcvd_by,
    e4.emp_name AS courier_recvd_by
FROM
    documents_branch_tracking d
JOIN
    branch_master b ON d.branch_id = b.branch_id
JOIN
    maafin_lms.loan_master l ON d.loan_no = l.loan_id
LEFT JOIN
    employee_master e ON e.emp_code = d.maker
LEFT JOIN
    employee_master e1 ON e1.emp_code = d.checker1_smebh
LEFT JOIN
    employee_master e2 ON e2.emp_code = d.checker2_glabh
LEFT JOIN
    employee_master e3 ON e3.emp_code = d.checker3_glbh
LEFT JOIN
    employee_master e4 ON e4.emp_code = d.checker4_ho
WHERE
    d.status >= 1 AND d.status <= 5
    AND d.checker4_date BETWEEN :startDate AND :endDate";


                using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                {
                    command.Parameters.Add("startDate", OracleDbType.Date).Value = startDate;
                    command.Parameters.Add("endDate", OracleDbType.Date).Value = endDate;

                    try
                    {
                        connection.Open();
                        OracleDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            ReportDataModel_Daily data = new ReportDataModel_Daily();
                            data.BranchName = reader["branch_name"].ToString();
                            data.ApplicationId = reader["application_id"].ToString();
                            data.LoanNo = reader["loan_no"].ToString();
                            data.CustomerId = reader["customer_id"].ToString();
                            data.CustName = reader["customer_name"].ToString();
                            // Check for DBNull and assign DateTime values
                            if (reader["loan_date"] != DBNull.Value)
                            {
                                data.LoanDate = Convert.ToDateTime(reader["loan_date"]);
                            }
                            data.LoanAmount = reader["loan_amount"].ToString();
                            data.TypeOfDocs = reader["type_of_docs"].ToString();
                            data.RecedByLoanConfirmation = reader["reced_by_loan_confirmation"].ToString();
                            data.UnderCustodyAfterDisb = reader["under_custody_after_disb"].ToString();
                            data.RecedByAfterDisb = reader["reced_by_after_disb"].ToString();
                            data.UnderCustodyFromBranch = reader["under_custody_from_branch"].ToString();
                            data.RecedByTransferFromBranch = reader["reced_by_transfer_from_branch"].ToString();
                            data.CourIdTranBranch = reader["cour_id_tran_branch"].ToString();
                            data.CourIdRecvdBy = reader["cour_id_recvd_by"].ToString();
                            data.UnderCustodyCourRcvdBy = reader["under_custody_cour_rcvd_by"].ToString();
                            data.CourierRecvdBy = reader["courier_recvd_by"].ToString();


                            reportData.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log, throw, or return empty list)
                        Console.WriteLine("Error fetching report data: " + ex.Message);
                    }
                }
            }

            return reportData;




        }


        //-----------------------------Daily Completed----------------------------//

        //--------------------------------Courier Report-----------------------------//
        public ActionResult Courier_Track_Rpt()
        {
            return View();
        }



        [HttpPost]
        public ActionResult GenerateReport(DateTime startDate, DateTime endDate)
        {
            List<ReportDataModel> reportData = FetchReportData(startDate, endDate);

            return PartialView("_Courier_Rpt_Partial", reportData);
        }

        private List<ReportDataModel> FetchReportData(DateTime startDate, DateTime endDate)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            List<ReportDataModel> reportData = new List<ReportDataModel>();

            using (OracleConnection connection = new OracleConnection(constr))
            {
                string sqlQuery = @"
            SELECT d.loan_no, b.branch_name, d.type_of_docs, d.checker4_ho,
                   d.courier_company_name, d.consignment_no,
                   d.date_of_courier, d.date_of_receival
            FROM documents_branch_tracking d
            JOIN branch_master b ON b.branch_id = d.branch_id
            WHERE d.checker4_date BETWEEN :startDate AND :endDate";

                using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                {
                    command.Parameters.Add("startDate", OracleDbType.Date).Value = startDate;
                    command.Parameters.Add("endDate", OracleDbType.Date).Value = endDate;

                    try
                    {
                        connection.Open();
                        OracleDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            ReportDataModel data = new ReportDataModel();
                            data.LoanNo = reader["loan_no"].ToString();
                            data.BranchName = reader["branch_name"].ToString();
                            data.TypeOfDocs = reader["type_of_docs"].ToString();
                            data.Checker4Ho = reader["checker4_ho"].ToString();
                            data.CourierCompanyName = reader["courier_company_name"].ToString();
                            data.ConsignmentNo = reader["consignment_no"].ToString();

                            // Check for DBNull and assign DateTime values
                            if (reader["date_of_courier"] != DBNull.Value)
                            {
                                data.DateOfCourier = Convert.ToDateTime(reader["date_of_courier"]);
                            }

                            if (reader["date_of_receival"] != DBNull.Value)
                            {
                                data.DateOfReceival = Convert.ToDateTime(reader["date_of_receival"]);
                            }

                            reportData.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log, throw, or return empty list)
                        Console.WriteLine("Error fetching report data: " + ex.Message);
                    }
                }
            }

            return reportData;
        }

        //-----------------------Courier Report Completed---------------//

        //--------------------------------Total inventory Report-----------------------------//

        public ActionResult Total_Invt_Rpt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GenerateReport_Total_Inventory(DateTime startDate, DateTime endDate)
        {
            List<ReportDataModel_Total> reportData = FetchReportData_Total(startDate, endDate);

            return PartialView("_Total_Invt_Rpt_Partial", reportData);
        }

        private List<ReportDataModel_Total> FetchReportData_Total(DateTime startDate, DateTime endDate)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            List<ReportDataModel_Total> reportData = new List<ReportDataModel_Total>();

            using (OracleConnection connection = new OracleConnection(constr))
            {
                string sqlQuery = @"
            SELECT
    b.branch_name,
    d.loan_no,
    l.customer_name,
    to_char(l.loan_date) as loan_date,
    l.loan_amount,
    d.type_of_docs,
    CASE
        WHEN d.status >= 1 AND d.status <= 4 THEN b.branch_name
        WHEN d.status = 5 THEN 'HO'  
        ELSE NULL  
    END AS Documents_Custody,
  e.emp_name AS maker_emp_name,
    e1.emp_name AS checker1_emp_name,
    e2.emp_name AS checker2_emp_name,
    e3.emp_name AS checker3_emp_name,
    e4.emp_name AS checker4_emp_name,
    d.maker_date,
    d.checker1_date,
    d.checker2_date,
    d.checker3_date,
    d.checker4_date
FROM
    documents_branch_tracking d
JOIN
    branch_master b ON d.branch_id = b.branch_id
JOIN
    maafin_lms.loan_master l ON d.loan_no = l.loan_id
    join employee_master e on e.emp_code=d.maker
    join employee_master e1 on e1.emp_code=d.checker1_smebh
    join employee_master e2 on e2.emp_code=d.checker2_glabh
    join employee_master e3 on e3.emp_code=d.checker3_glbh
    join employee_master e4 on e4.emp_code=d.checker4_ho
WHERE
    (d.status >= 1 AND d.status <= 5)
    and d.checker4_date between :startDate AND :endDate";


                using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                {
                    command.Parameters.Add("startDate", OracleDbType.Date).Value = startDate;
                    command.Parameters.Add("endDate", OracleDbType.Date).Value = endDate;

                    try
                    {
                        connection.Open();
                        OracleDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            ReportDataModel_Total data = new ReportDataModel_Total();
                            data.BranchName = reader["branch_name"].ToString();
                            data.LoanNo = reader["loan_no"].ToString();
                            data.CustName = reader["customer_name"].ToString();
                            // Check for DBNull and assign DateTime values
                            if (reader["loan_date"] != DBNull.Value)
                            {
                                data.LoanDate = Convert.ToDateTime(reader["loan_date"]);
                            }
                            data.LoanAmount = reader["loan_amount"].ToString();
                            data.TypeOfDocs = reader["type_of_docs"].ToString();
                            data.DocsCustody = reader["Documents_Custody"].ToString();
                            data.Maker = reader["maker_emp_name"].ToString();
                            if (reader["maker_date"] != DBNull.Value)
                            {
                                data.MakerDate = Convert.ToDateTime(reader["maker_date"]);
                            }
                            data.Checker1Smebh = reader["checker1_emp_name"].ToString();
                            if (reader["checker1_date"] != DBNull.Value)
                            {
                                data.Checker1Date = Convert.ToDateTime(reader["checker1_date"]);
                            }
                            data.Checker2Glabh = reader["checker2_emp_name"].ToString();
                            if (reader["checker2_date"] != DBNull.Value)
                            {
                                data.Checker2Date = Convert.ToDateTime(reader["checker2_date"]);
                            }
                            data.Checker3Glbh = reader["checker3_emp_name"].ToString();
                            if (reader["checker3_date"] != DBNull.Value)
                            {
                                data.Checker3Date = Convert.ToDateTime(reader["checker3_date"]);
                            }
                            data.Checker4Ho = reader["checker4_emp_name"].ToString();
                            if (reader["checker4_date"] != DBNull.Value)
                            {
                                data.Checker4Date = Convert.ToDateTime(reader["checker4_date"]);
                            }

                            reportData.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log, throw, or return empty list)
                        Console.WriteLine("Error fetching report data: " + ex.Message);
                    }
                }
            }

            return reportData;
        }

        //-----------------------Courier Report Completed---------------//

    }


}

    

