using Doc_Tracking_vl.App_Code;
using Doc_Tracking_vl.Filters;
using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Controllers.Branch
{

    [SessionTimeout]
    public class vl_Branch_HeadController : Controller
    {

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        // GET: vl_Branch_Head
        public ActionResult Dashboard()
        {
            return View();
        }


        public ActionResult handover_glbh()
        {
             Checklist_Model_VL_BH model = new Checklist_Model_VL_BH();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_tracking_vl d, employee_master e, branch_master s where d.branch_id = s.branch_id and s.branch_id = e.branch_id and d.status = 1 and e.department_id = 456 and e.emp_code = '" + id + "' ";
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
                // Query for document types
                string docTypeSql = "select distinct d.type_of_docs from documents_tracking_vl d where d.loan_no='" + Loan_Id + "'";
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

                con.Close();

                // Populate the properties of the model
                model.LoanNumbers = loanid;
                model.DocTypes = docTypes;


            }
            // Extract the selected loan number from the request parameters
            string loanNumber = Request.QueryString["loanNumber"];
            // Call DetermineLoanDocType to get the loanDocType
            //-------------------
            string loanDocType = DetermineLoanDocType(loanNumber);

            // Set the loanDocType in ViewBag
            //--------------------------------
            ViewBag.LoanMode = loanDocType;
            return View(model);
        }
        public ActionResult Search_doc(Checklist_Model_VL_BH v, string loanMode)
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
                    if (v.SelectedDocType == "Loan Document" && loanMode == "Mortgage")
                    {
                        string loanDocSql = "SELECT l.track_id, l.loan_doc_id, l.docu_name, l.maker_status FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id = l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' ORDER BY l.loan_doc_id ASC";
                        OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
                        loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

                        OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

                        v.LoanDocsModel = new LoanDocModel_Mortgage_vl_bh
                        {
                            LoanDocs = new List<LoanDoc_vl_bh>() // Initialize the list
                        };

                        while (loanDocReader.Read())
                        {
                            Session["track_id_loan"] = loanDocReader["track_id"].ToString();
                            // Populate the properties for Loan Document and add it to the list
                            var loanDoc = new LoanDoc_vl_bh
                            {
                                LoanDocId = loanDocReader["loan_doc_id"].ToString(),
                                LoanDocName = loanDocReader["docu_name"].ToString(),
                                IsChecked = true,
                                Maker_Status = loanDocReader["maker_status"].ToString()
                            };

                            v.LoanDocsModel.LoanDocs.Add(loanDoc);
                        }

                        con.Close();
                        return PartialView("_Mortgage_loan_check", v);
                    }

                    // If the conditions are not met, close the connection
                    con.Close();
                }

                return View("handover_glbh");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return View("Error"); // Return an error view
            }
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
            string loanDocType = "Mortgage"; // Default value, replace it with your actual logic

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
        //----------------------------------Get loan model completed---------------------------//


        //----------------------Save Checklist Loan document--------------------------//


        [HttpPost]
        public ActionResult Save_Loan_Mortgage_VL_bh(List<LoanDocumentsMortgage_vl_bh> model, string selectedLoanNumber, string selectedDocType)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            try
            {

                string sqlUpdateDocumentsBranchTracking = "UPDATE DOCUMENTS_TRACKING_VL t SET t.checker1_vlbh = :userid, t.checker1_date = sysdate, t.status = 2 WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";
                string sqlUpdateCheckLoanDocuMaster = "UPDATE CHECKLIST_LOAN_MASTER c SET c.checker1_status = :checker1_status, c.checker1_remarks = :checker1_remarks, c.checker1_vlbh   = :userid, c.checker1_date = sysdate WHERE c.loan_no = :loan_number  AND c.loan_doc_id = :loan_doc_id";

                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();

                    // Retrieve data from the model
                    var user_id = Session["user_id"];
                    var track_id = Session["track_id"];
                    string sqlCheckDocumentsBranchTracking = "SELECT t.status FROM DOCUMENTS_TRACKING_VL t WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";
                    // Check the status of documents
                    using (OracleCommand cmdCheckTracking = new OracleCommand(sqlCheckDocumentsBranchTracking, con))
                    {
                        cmdCheckTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                        cmdCheckTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;

                        // Execute the query to check the status
                        var status = cmdCheckTracking.ExecuteScalar();

                        if (status != null && status.ToString() == "2")
                        {
                            // Status is already 2, return appropriate response indicating that the documents have already been verified
                            return Json(new { success = false, message = "Documents are already verified" });
                        }
                        else
                        {

                            // Update DOCUMENTS_BRANCH_TRACKING
                            using (OracleCommand cmdUpdateTracking = new OracleCommand(sqlUpdateDocumentsBranchTracking, con))
                            {
                                cmdUpdateTracking.Parameters.Add(":userid", OracleDbType.Int32).Value = user_id;
                                cmdUpdateTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                                cmdUpdateTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;
                                cmdUpdateTracking.ExecuteNonQuery();
                            }

                            // Update CHECK_PROP_DOCU_MASTER
                            using (OracleCommand cmdUpdateCheckLoanDocuMaster = new OracleCommand(sqlUpdateCheckLoanDocuMaster, con))
                            {
                                // Loop through each record in 'model'
                                foreach (var item in model)
                                {
                                    // Set parameters for the current record
                                    cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker1_status", OracleDbType.Varchar2).Value = item.IsChecked;
                                    cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker1_remarks", OracleDbType.Varchar2).Value = item.Remark;
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
                            return Json(new { success = true, message = "Documents Transfer to Safe" });
                        }
                    }
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