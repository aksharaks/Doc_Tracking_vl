using System;
using System.Collections.Generic;
using Doc_Tracking_vl.App_Code;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using Doc_Tracking_vl.Filters;
using Oracle.ManagedDataAccess.Client;
using System.Data;                  
using Doc_Tracking_vl.Models;

namespace Doc_Tracking_vl.Controllers.HeadOffice
{
    [SessionTimeout]
    //ghdgsjhgjh
    public class Head_OfficeController : Controller
    {

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        public ActionResult Dashboard_HO()
        {
            Dashboard_Model model = new Dashboard_Model();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = " select distinct d.loan_no from documents_tracking_vl d";
                OracleCommand cmd = new OracleCommand(sql, con);

                // Execute the SQL query and fetch branch names
                OracleDataReader reader = cmd.ExecuteReader();
                List<SelectListItem> loanid = new List<SelectListItem>();

                while (reader.Read())
                {
                    string loan_id = reader["loan_no"].ToString();
                    Session["loan_id"] = loan_id;
                    //ViewBag.LoanId = loan_id;
                    // Session["branch_id"] = reader["branch_id"].ToString();
                    var item = new SelectListItem
                    {
                        Text = loan_id,
                        Value = loan_id
                    };
                    loanid.Add(item);
                }

                var Loan_Id = Session["loan_id"];

                con.Close();

                // Populate the properties of the model
                model.LoanNumbers = loanid;
                // model.DocTypes = docTypes;


            }
            // Extract the selected loan number from the request parameters
            string loanNumber = Request.QueryString["loanNumber"];

            return View(model);
        }



        [HttpPost]
        public ActionResult search_no(Dashboard_Model model)
        {
            var loan_id = model.SelectedLoanNumber;

            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1;

            try
            {
                if (Session["user_id"] != null && loan_id != null)
                {
                    using (OracleConnection con = new OracleConnection(constr))
                    {
                        con.Open();

                        string sql = @"SELECT DISTINCT d.maker,
                to_char(d.maker_date) as maker_date,
                d.checker1_vlbh,
                to_char(d.checker1_date) as checker1_date,
                d.checker2_glabh,
                to_char(d.checker2_date) as checker2_date,
                d.checker3_glbh,
                to_char(d.checker3_date) as checker3_date,
                d.checker4_ho,
                to_char(d.checker4_date) as checker4_date
  FROM documents_tracking_vl d
 WHERE d.loan_no =:loan_id";

                        using (OracleCommand cmd = new OracleCommand(sql, con))
                        {
                            cmd.Parameters.Add("loan_id", OracleDbType.Varchar2).Value = loan_id;

                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    ViewBag.Maker = reader["maker"];
                                    ViewBag.MakerDate = reader["maker_date"];
                                    ViewBag.Checker1Vlbh = reader["checker1_vlbh"];
                                    ViewBag.Checker1Date = reader["checker1_date"];
                                    ViewBag.Checker2Glabh = reader["checker2_glabh"];
                                    ViewBag.Checker2Date = reader["checker2_date"];
                                    ViewBag.Checker3Glbh = reader["checker3_glbh"];
                                    ViewBag.Checker3Date = reader["checker3_date"];
                                    ViewBag.Checker4Ho = reader["checker4_ho"];
                                    ViewBag.Checker4Date = reader["checker4_date"];
                                }
                            }
                        }

                        con.Close();
                    }

                    return PartialView("_LoanProgress");
                }
                else
                {
                    TempData["Message"] = "Please select a loan number.";
                    return PartialView("_LoanProgress");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                return PartialView("_LoanProgress");
            }
        }

        // ------------------Dashboard completed--------------------------------------------//



        // GET: Head_Office
        public ActionResult Head_Office_Entry(string selectedLoanNumber, string selectedDocType)
        {

            HO_entry model = new HO_entry();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {
                var id = Session["user_id"].ToString();
                con.Open();

                // Query to fetch distinct loan numbers
                string sql = "SELECT DISTINCT d.loan_no FROM DOCUMENTS_TRACKING_VL d,employee_master e WHERE d.return_status = 5 AND e.emp_code = '" + id + "'";
                OracleCommand cmd = new OracleCommand(sql, con);
                OracleDataReader reader = cmd.ExecuteReader();
                List<SelectListItem> loanid = new List<SelectListItem>();

                while (reader.Read())
                {
                    string loan_id = reader["loan_no"].ToString();
                    Session["loan_id"] = loan_id;
                    var item = new SelectListItem
                    {
                        Text = loan_id,
                        Value = loan_id
                    };
                    loanid.Add(item);
                }

                var Loan_Id = Session["loan_id"];

                // Query to fetch distinct document types
                string docTypeSql = "SELECT DISTINCT d.type_of_docs FROM DOCUMENTS_TRACKING_VL d";
                OracleCommand docTypeCmd = new OracleCommand(docTypeSql, con);
                OracleDataReader docTypeReader = docTypeCmd.ExecuteReader();
                List<SelectListItem> docTypes = new List<SelectListItem>();

                while (docTypeReader.Read())
                {
                    string docType = docTypeReader["type_of_docs"].ToString();
                    Session["doc_type"] = docType;
                    var item = new SelectListItem
                    {
                        Text = docType,
                        Value = docType
                    };
                    docTypes.Add(item);
                }

                // Query to fetch courier details based on selected loan number and document type
                string courierDetailsSql = @"SELECT DISTINCT d.courier_company_name, d.consignment_no, d.date_of_courier
                                 FROM DOCUMENTS_TRACKING_VL d
                                 WHERE d.loan_no = :loanNumber
                                 AND d.type_of_docs = :docType";

                OracleCommand courierCmd = new OracleCommand(courierDetailsSql, con);
                courierCmd.Parameters.Add(":loanNumber", OracleDbType.Varchar2).Value = selectedLoanNumber;
                courierCmd.Parameters.Add(":docType", OracleDbType.Varchar2).Value = selectedDocType;

                OracleDataReader courierReader = courierCmd.ExecuteReader();

                // Read the result and populate model properties
                if (courierReader.Read())
                {
                    model.courier_company_name = courierReader["courier_company_name"].ToString();
                    model.consignment_no = courierReader["consignment_no"].ToString();
                    model.date_of_courier = Convert.ToDateTime(courierReader["date_of_courier"]);
                }

                con.Close();

                // Populate the LoanNumbers and DocTypes properties of the model
                model.LoanNumbers = loanid;
                model.DocTypes = docTypes;
            }

            // Set the selected loan document type in ViewBag
            ViewBag.SelectedDocType = selectedDocType;
            // Extract the selected loan number from the request parameters
            string loanNumber = Request.QueryString["loanNumber"];
            // Call DetermineLoanDocType to get the loanDocType
            string loanDocType = DetermineLoanDocType(loanNumber);
            // Set the loanDocType in ViewBag
            ViewBag.LoanMode = loanDocType;
            return View(model);
        }
        public JsonResult GetLoanDocType(string loanNumber)
        {
            // Implement your logic to determine the loan document type based on the loan number
            // For example, query the database or use some business rules to determine the type

            string loanDocType = DetermineLoanDocType(loanNumber);

            return Json(loanDocType, JsonRequestBehavior.AllowGet);
        }


        private string DetermineLoanDocType(string loanNumber)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            string loanDocType = "Vehicle"; // Default value, replace it with your actual logic

            string sql = "SELECT DISTINCT d.loan_mode " +
                         "FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER c " +
                         "WHERE d.track_id=c.track_id " +
                         "AND d.loan_no = :loanNo";

            using (OracleConnection con = new OracleConnection(constr))
            {
                con.Open();

                using (OracleCommand cmd = new OracleCommand(sql, con))
                {
                    cmd.Parameters.Add(":loanNo", OracleDbType.Varchar2).Value = loanNumber;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // You may adjust the logic based on the actual values in your database
                            string loanMode = reader["loan_mode"].ToString();

                            // Update the loanDocType based on the fetched loan_mode
                            if (loanMode == "Tranche")
                            {
                                loanDocType = "Tranche";
                            }
                            // You can add more conditions as needed
                        }
                    }
                }
            }

            return loanDocType;
        }
        [HttpPost]
        public ActionResult Submit_entry(HO_entry model)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1; // Assuming conStr1 is your Oracle connection string

            try
            {
                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();
                    // Define your SQL query
                    string query = @"UPDATE DOCUMENTS_TRACKING_VL t 
                     SET t.date_of_receival=:DateReceival,t.return_status=6 ,t.settle_status=0 
                     WHERE t.loan_no = :LoanNumber 
                     AND t.type_of_docs = :DocType 
                     AND t.return_status = 5";

                    // Establish connection to the database and execute the query
                    using (OracleCommand command = new OracleCommand(query, con))
                    {
                        // Add parameters to the query
                        command.Parameters.Add(new OracleParameter("DateReceival", model.date_of_receival));
                        command.Parameters.Add(new OracleParameter("LoanNumber", model.SelectedLoanNumber));
                        command.Parameters.Add(new OracleParameter("DocType", model.SelectedDocType));
                        try
                        {
                            // Execute query
                            int rowsAffected = command.ExecuteNonQuery();

                            // Check if any rows were affected
                            if (rowsAffected > 0)
                            {
                                return Json(new { success = true, message = "Courier data submitted successfully." });
                            }
                            else
                            {
                                return Json(new { success = false, message = "No rows were updated." });
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle any exceptions
                            ViewBag.Message = "An error occurred: " + ex.Message;
                        }
                    }
                }

                // Redirect to a view or return JSON response as needed
                return RedirectToAction("Head_Office_Entry", "Head_Office");
            }
            catch (Exception ex)
            {
                // Handle connection errors
                ViewBag.Message = "Connection error: " + ex.Message;
                return View();
            }
        }




        //[HttpPost]
        //public ActionResult View_docs(HO_entry v, string loanMode)
        //{
        //    try
        //    {
        //        MaafinDbHelper mdh = new MaafinDbHelper();
        //        mdh.Connection();
        //        constr = mdh.conStr1;
        //        OracleConnection con = new OracleConnection(constr);

        //        if (Session["user_id"] != null)
        //        {
        //            var id = Session["user_id"].ToString();
        //            con.Open();
        //            // Set loanMode in ViewBag
        //            ViewBag.LoanMode = loanMode;

        //            // Check if SelectedDocType is "Loan Document" and loanMode is "vehicle"
        //            if (v.SelectedDocType == "Loan Document" && loanMode == "vehicle")
        //            {
        //                string loanDocSql = "SELECT l.track_id,l.loan_doc_id,l.docu_name  FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id=l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' order by l.loan_doc_id asc";
        //                OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
        //                loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

        //                OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

        //                v.LoanDocsModel = new LoanDocModel_Mortgage_ho
        //                {
        //                    LoanDocs = new List<LoanDoc_ho>() // Initialize the list
        //                };

        //                while (loanDocReader.Read())
        //                {
        //                    Session["track_id_loan"] = loanDocReader["track_id"].ToString();
        //                    // Populate the properties for Loan Document and add it to the list
        //                    var loanDoc = new LoanDoc_ho
        //                    {
        //                        LoanDocId = loanDocReader["loan_doc_id"].ToString(),
        //                        LoanDocName = loanDocReader["docu_name"].ToString(),
        //                        IsChecked = true
        //                    };

        //                    v.LoanDocsModel.LoanDocs.Add(loanDoc);
        //                }
        //                con.Close();
        //                return PartialView("_loan_check_HO", v);
        //            }

        //            // Close connection if the condition is not met
        //            con.Close();
        //        }

        //        // Return default view if session is null or condition is not met
        //        return View("Head_Office_Entry");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it appropriately
        //        Console.WriteLine(ex.Message);
        //        return View("Error"); // Return an error view
        //    }
        //}



        //----------------------Save Checklist Loan document--------------------------//

        [HttpPost]
        public ActionResult View_docs(HO_entry v, string loanMode)
        {
            try
            {
                MaafinDbHelper mdh = new MaafinDbHelper();
                mdh.Connection();
                constr = mdh.conStr1;
                OracleConnection con = new OracleConnection(constr);

              
                    if (Session["user_id"] != null)
                    {
                        var id = Session["user_id"].ToString();
                        con.Open();
                        // Set loanMode in ViewBag
                        ViewBag.LoanMode = loanMode;

                        // Check the selected document type and execute the corresponding query
                        if (v.SelectedDocType == "Loan Document" && loanMode == "Vehicle")
                        {
                            string loanDocSql = "SELECT l.track_id, l.loan_doc_id, l.docu_name FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id = l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' ORDER BY l.loan_doc_id ASC";
                            using (OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con))
                            {
                                loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

                                using (OracleDataReader loanDocReader = loanDocCmd.ExecuteReader())
                                {
                                    v.LoanDocsModel = new LoanDocModel_Mortgage_ho
                                    {
                                        LoanDocs = new List<LoanDoc_ho>() // Initialize the list
                                    };

                                    while (loanDocReader.Read())
                                    {
                                        Session["track_id_loan"] = loanDocReader["track_id"].ToString();
                                        // Populate the properties for Loan Document and add it to the list
                                        var loanDoc = new LoanDoc_ho
                                        {
                                            LoanDocId = loanDocReader["loan_doc_id"].ToString(),
                                            LoanDocName = loanDocReader["docu_name"].ToString(),
                                            IsChecked = true
                                        };

                                        v.LoanDocsModel.LoanDocs.Add(loanDoc);
                                    }
                                }
                            }
                            return PartialView("_loan_check_HO", v);
                        }
                    }
                    con.Close();
                
                return View("Head_Office_Entry");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return View("Error"); // Return an error view
            }
        }











        [HttpPost]
        public ActionResult Save_Loan_Mortgage_ho(List<LoanDocumentsMortgage_ho> model, string selectedLoanNumber, string selectedDocType)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            try
            {
                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();

                    // Retrieve data from the model
                    var user_id = Session["user_id"];
                    var track_id = Session["track_id"];

                    // Update DOCUMENTS_BRANCH_TRACKING
                    string sqlUpdateDocumentsBranchTracking1 = "UPDATE DOCUMENTS_TRACKING_VL t SET t.checker4_ho = :userid, t.checker4_date = sysdate, t.status = 5 WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";

                    using (OracleCommand cmdUpdateTracking = new OracleCommand(sqlUpdateDocumentsBranchTracking1, con))
                    {
                        cmdUpdateTracking.Parameters.Add(":userid", OracleDbType.Int32).Value = user_id;
                        cmdUpdateTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                        cmdUpdateTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;
                        cmdUpdateTracking.ExecuteNonQuery();
                    }

                    // Update CHECK_PROP_DOCU_MASTER
                    string sqlUpdateCheckLoanDocuMaster = "UPDATE CHECKLIST_LOAN_MASTER c SET c.checker4_status = :checker4_status, c.checker4_reamarks = :checker4_remarks, c.checker4_ho = :userid, c.checker4_date = sysdate WHERE c.loan_no = :loan_number  AND c.loan_doc_id = :loan_doc_id";

                    using (OracleCommand cmdUpdateCheckLoanDocuMaster = new OracleCommand(sqlUpdateCheckLoanDocuMaster, con))
                    {
                        // Loop through each record in 'model'
                        foreach (var item in model)
                        {
                            // Set parameters for the current record
                            cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker4_status", OracleDbType.Varchar2).Value = item.IsChecked;
                            cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker4_remarks", OracleDbType.Varchar2).Value = item.Remark;
                            cmdUpdateCheckLoanDocuMaster.Parameters.Add(":userid", OracleDbType.Int32).Value = user_id;
                            cmdUpdateCheckLoanDocuMaster.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                            cmdUpdateCheckLoanDocuMaster.Parameters.Add(":loan_doc_id", OracleDbType.Int32).Value = item.LoanDocId;

                            // Execute the update query
                            cmdUpdateCheckLoanDocuMaster.ExecuteNonQuery();

                            // Clear parameters for the next iteration
                            cmdUpdateCheckLoanDocuMaster.Parameters.Clear();
                        }
                    }

                    // Return an appropriate response
                    return Json(new { success = true, message = "Documents Verified by HO" });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
                return Json(new { success = false, message = "Error saving data: " + ex.Message });
            }
        }


    }
}


       


          








                