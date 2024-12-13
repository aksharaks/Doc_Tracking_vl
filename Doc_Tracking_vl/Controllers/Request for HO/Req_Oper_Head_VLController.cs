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

namespace Doc_Tracking_vl.Controllers.Request_for_HO
{

    [SessionTimeout]
    public class Req_Oper_Head_VLController : Controller
    {


        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        // GET: Req_oper_Head_VL
        public ActionResult Req_VL_Op_Head()
        {
            Req_VL_Op_Head model = new Req_VL_Op_Head();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_tracking_vl d, employee_master e, branch_master s where d.branch_id = s.branch_id and s.branch_id = e.branch_id and d.status = 4 and (d.return_status IS NULL OR d.return_status != 1) and e.emp_code='" + id + "'";


                //and d.return_status != 1
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

                // Populate the LoanNumbers property of the model
                model.LoanNumbers = loanid;
                model.DocTypes = docTypes;


            }
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
            string loanDocType = "vehicle"; // Default value, replace it with your actual logic

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
        public ActionResult Search_doc(Req_VL_Op_Head v, string loanMode)
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

                    // Apply the condition for "Loan Document" and "Mortgage" loanMode
                    if (v.SelectedDocType == "Loan Document" && loanMode == "vehicle")
                    {
                        string loanDocSql = "SELECT l.track_id, l.loan_doc_id, l.docu_name, l.checker3_status,l.checker3_remarks  FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id = l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' ORDER BY l.loan_doc_id ASC";

                        OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
                        loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

                        OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

                        // Initialize the LoanDocsModel list
                        v.LoanDocsReq_vl_ophead = new LoanDocRequ_Mortgage_Op_Head_VL
                        {
                            LoanDocs = new List<LoanDoc_Op_Head_vl>()
                        };

                        while (loanDocReader.Read())
                        {
                            Session["track_id_loan"] = loanDocReader["track_id"].ToString();

                            // Populate the properties for Loan Document and add to the list
                            var loanDoc = new LoanDoc_Op_Head_vl
                            {
                                LoanDocId = loanDocReader["loan_doc_id"].ToString(),
                                LoanDocName = loanDocReader["docu_name"].ToString(),
                                IsChecked = loanDocReader["checker3_status"].ToString() == "Y",
                                Remarks = loanDocReader["checker3_remarks"].ToString() // Keep the remarks as string for now
                            };

                            v.LoanDocsReq_vl_ophead.LoanDocs.Add(loanDoc);
                        }

                        // Close the connection after executing the query
                        con.Close();

                        // Return the partial view with the populated data
                        return PartialView("_Loan_Op_Head_Req", v);
                    }

                    // If the condition is not met, close the connection
                    con.Close();
                }

                // Return to the main view if no user session exists or condition is not met
                return View("Req_VL_Op_Head");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return View("Error"); // Return an error view
            }
        }


        //-------------------------Save Loan Document Request Mortgage-----------------------------//

        [HttpPost]
        public ActionResult Save_LoanReq_Mortgage_Op(List<Req_LoanDoc_Op> model, string selectedLoanNumber, string selectedDocType)
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
                    string sqlCheckDocumentsBranchTracking = "SELECT t.return_status FROM documents_tracking_vl t WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";

                    using (OracleCommand cmdCheckTracking = new OracleCommand(sqlCheckDocumentsBranchTracking, con))
                    {
                        cmdCheckTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                        cmdCheckTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;

                        // Execute the query to check the status
                        var status = cmdCheckTracking.ExecuteScalar();

                        if (status != null && status.ToString() == "1")
                        {
                            // Status is already 2, return appropriate response indicating that the documents have already been verified
                            return Json(new { success = false, message = "Documents are already requested" });
                        }
                        else
                        {

                            // Update DOCUMENTS_BRANCH_TRACKING
                            string sqlUpdateDocumentsBranchTracking1 = "UPDATE documents_tracking_vl t SET t.return_status=1 WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";

                            using (OracleCommand cmdUpdateTracking = new OracleCommand(sqlUpdateDocumentsBranchTracking1, con))
                            {
                                // cmdUpdateTracking.Parameters.Add(":userid", OracleDbType.Int32).Value = user_id;
                                cmdUpdateTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                                cmdUpdateTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;
                                cmdUpdateTracking.ExecuteNonQuery();
                            }



                            // Return an appropriate response
                            return Json(new { success = true, message = "Documents Transfer to HO" });
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