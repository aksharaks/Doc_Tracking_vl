using Doc_Tracking_vl.App_Code;
using Doc_Tracking_vl.Filters;
using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Controllers.Branch
{

    [SessionTimeout]
    public class Vl_operation_HeadController : Controller

    {

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;

        // GET: Vl_operationhead_
        public ActionResult Dashboard()
        {
            Dashboard_Model_Branch model = new Dashboard_Model_Branch();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);


            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_branch_tracking d";
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




          

        public ActionResult Checklist_OpHead() 
        
        
        {
            Checklist_Model model = new Checklist_Model();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);
            if (Session["user_id"] != null)
            {
                var id = Session["user_id"].ToString();
                // Fetch states data
                con.Open();
                string statesSql = "SELECT DISTINCT s.state_id, s.state_name FROM state_master s, branch_master bm, maafin_lms.loan_master lm, employee_master em WHERE s.state_id = bm.state_id AND bm.branch_id = lm.branch_id AND lm.product_id IN (43) AND em.branch_id = bm.branch_id and em.emp_code =  '" + id + "'";
                OracleCommand statesCmd = new OracleCommand(statesSql, con);
                OracleDataReader statesReader = statesCmd.ExecuteReader();

                List<SelectListItem> states = new List<SelectListItem>();
                List<SelectListItem> loanid = new List<SelectListItem>();
                while (statesReader.Read())
                {
                    string stateId = statesReader["state_id"].ToString();
                    Session["state_id"] = statesReader["state_id"].ToString();
                    string stateName = statesReader["state_name"].ToString();
                    states.Add(new SelectListItem { Text = stateName, Value = stateId });
                }

                con.Close();

                model.States = states;
                // Populate the LoanNumbers property of the model
                model.LoanNumbers = loanid;
                // Initialize Branches property
                model.Branches = new List<SelectListItem>(); // Add this line to initialize the Branches property
                model.LoanNumbers = new List<SelectListItem>();

            }
            return View(model);


        }

        public ActionResult GetBranches(string stateId)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);
            var id = Session["user_id"].ToString();
            // Fetch branches based on the selected state
            con.Open();
            string branchesSql = "SELECT DISTINCT bm.branch_id, bm.branch_name FROM state_master s, branch_master bm, maafin_lms.loan_master lm, employee_master em WHERE s.state_id = bm.state_id AND bm.branch_id = lm.branch_id AND lm.product_id IN (43) AND s.state_id = :stateId AND em.branch_id = bm.branch_id and em.emp_code = :id";
            OracleCommand branchesCmd = new OracleCommand(branchesSql, con);
            branchesCmd.Parameters.Add(new OracleParameter("stateId", stateId));
            branchesCmd.Parameters.Add(new OracleParameter("empcode", id));
            OracleDataReader branchesReader = branchesCmd.ExecuteReader();

            List<SelectListItem> branches = new List<SelectListItem>();
            while (branchesReader.Read())
            {
                string branchId = branchesReader["branch_id"].ToString();
                //Session["branch_id"]=branchId;
                string branchName = branchesReader["branch_name"].ToString();
                branches.Add(new SelectListItem { Text = branchName, Value = branchId });
            }

            con.Close();

            return Json(branches);
        }

        public ActionResult GetLoanNumbers(string loanType, string stateId)
        {
            // Implement this method to fetch loan numbers based on state, branch, and loan type
            // Make sure to return data in the format needed by the JavaScript
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);
            var id = Session["user_id"].ToString();
            // Fetch branches based on the selected state
            con.Open();
            string loanSql = "SELECT lm.loan_id, lm.product_id FROM state_master s, branch_master bm, maafin_lms.loan_master lm, employee_master em WHERE s.state_id = bm.state_id AND bm.branch_id = lm.branch_id AND lm.product_id = :loanType AND s.state_id = :stateId AND em.branch_id = bm.branch_id AND em.emp_code = :id";
            //string loanSql = "SELECT lm.loan_id FROM state_master s, branch_master bm, sme_branch_master_internal sm, maafin_lms.loan_master lm, employee_master em WHERE s.state_id = bm.state_id AND bm.branch_id = sm.branch_id AND sm.branch_id = lm.branch_id AND lm.product_id = :loanType AND s.state_id = :stateId AND em.branch_id = sm.branch_id AND em.emp_code = :id AND bm.branch_name = :branch";
            OracleCommand cmd = new OracleCommand(loanSql, con);

            cmd.Parameters.Add(new OracleParameter("loanType", OracleDbType.Int32)).Value = Convert.ToInt32(loanType);
            cmd.Parameters.Add(new OracleParameter("stateId", OracleDbType.Int32)).Value = Convert.ToInt32(stateId);
            cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Varchar2)).Value = id;
            // cmd.Parameters.Add(new OracleParameter("branch", OracleDbType.Varchar2)).Value = branch;
            OracleDataReader reader = cmd.ExecuteReader();

            List<SelectListItem> LoanNumbers = new List<SelectListItem>();
            while (reader.Read())
            {
                string loan_id = reader["loan_id"].ToString();
                Session["loan_type"] = reader["product_id"].ToString();
                LoanNumbers.Add(new SelectListItem { Text = loan_id, Value = loan_id });
            }
            con.Close();
            return Json(LoanNumbers);
        }

        public ActionResult search_no(Checklist_Model model)
        {
            // Access selected loan number using model.SelectedLoanNumber
            var loan_id = model.SelectedLoanNumber;

            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);
            if (Session["user_id"] != null && loan_id != null)
            {
                con.Open();
                // Get the selected value from the dropdown

                string sql1 = "SELECT DISTINCT l.loan_id, l.branch_id, l.customer_name, l.disbursed_amount, TO_CHAR(l.loan_date) AS loan_date FROM maafin_lms.loan_master l, branch_master b, employee_master e WHERE l.branch_id = b.branch_id AND l.branch_id = e.branch_id AND l.product_id IN (43) AND l.loan_id = '" + loan_id + "'";
                OracleCommand cmd = new OracleCommand(sql1, con);

                // Execute the SQL query and fetch branch names
                OracleDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    TempData["loan_id"] = reader["loan_id"].ToString();
                    Session["loan_id"] = reader["loan_id"].ToString();
                    TempData["customer_name"] = reader["customer_name"].ToString();
                    TempData["disbursed_amount"] = reader["disbursed_amount"].ToString();
                    TempData["loan_date"] = reader["loan_date"].ToString();
                    Session["branch_id"] = reader["branch_id"].ToString();

                }
                con.Close();

            }
            else
            {
                TempData["Message"] = "Select the value!!";
            }
            return PartialView("_Branch_Operation_Head");
        }

        //-------------------------Search Part Completed-------------------------------//

        //---------------------------Load vehicle  documents mortgage--------------------------------//
        public ActionResult Mortgage_LoanDoc()
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);
            try
            {
                var loan_id = Session["loan_id"];
                con.Open();
                //string mort_loan_sql = "select distinct l.loan_doc_id, l.loan_doc_name from loan_doc_master l,documents_branch_tracking d where l.loan_type=d.loan_mode and d.loan_no=:loan_no order by l.loan_doc_id asc";
                string mort_loan_sql = "select distinct l.loan_doc_id, l.loan_doc_name from LOAN_VL_MASTER l order by l.loan_doc_id asc";
                OracleCommand cmd = new OracleCommand(mort_loan_sql, con);

                cmd.Parameters.Add(new OracleParameter("loan_no", OracleDbType.Varchar2)).Value = loan_id;
                // Execute the SQL query and fetch branch names
                OracleDataReader reader = cmd.ExecuteReader();

                var loanDocs = new List<LoanDoc>();

                while (reader.Read())
                {
                    loanDocs.Add(new LoanDoc
                    {
                        LoanDocId = reader["loan_doc_id"].ToString(),
                        LoanDocName = reader["loan_doc_name"].ToString(),
                        IsChecked = true // Set your logic for determining whether it's checked or not
                    });
                }

                con.Close();

                // Create the model and pass it to the partial view
                var model = new LoanDocModel_Mortgage
                {
                    LoanDocs = loanDocs
                };

                return PartialView("_Check_Loan_Mortgage", model);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Debug.WriteLine($"Exception in Mortgage_Checklist: {ex.Message}");
                throw; // Rethrow the exception for now; you may handle it differently in a production environment
            }
        }


        //--------------------------------------Save Loan Document mortgage -----------------------//

        [HttpPost]
        public ActionResult Save_Loan_Mortgage(List<LoanDocumentsMortgage> model)
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
                    var loan_id = Session["loan_id"];
                    var user_id = Session["user_id"];
                    var branch_id = Session["branch_id"];
                    var state_id = Session["state_id"];
                    var type_of_loan = Session["loan_type"];
                    // Determine type_of_loan and type_of_docs based on some conditions
                    // int type_of_loan = DetermineLoanType(loan_id);
                    // string type_of_docs = DetermineDocsType(model);
                    // Insert into DOCUMENTS_BRANCH_TRACKING

                    string sqlInsertTracking = @"
                INSERT INTO documents_tracking_vl (track_id,loan_no, state_code, branch_id, maker, maker_date,type_of_loan,type_of_docs,loan_mode,status) VALUES (seq_documents_tracking_vl.nextval,:loan_no, :state_code, :branch_id, :maker, :maker_date,:type_of_loan,'Loan Document','vehicle',1) RETURNING track_id INTO :track_id";

                    using (OracleCommand cmdInsertTracking = new OracleCommand(sqlInsertTracking, con))
                    {
                        cmdInsertTracking.Parameters.Add("loan_no", OracleDbType.Varchar2).Value = loan_id;
                        cmdInsertTracking.Parameters.Add("state_code", OracleDbType.Int32).Value = state_id;
                        cmdInsertTracking.Parameters.Add("branch_id", OracleDbType.Int32).Value = branch_id;
                        cmdInsertTracking.Parameters.Add("maker", OracleDbType.Int32).Value = user_id;
                        cmdInsertTracking.Parameters.Add("maker_date", OracleDbType.Date).Value = DateTime.Now;
                        cmdInsertTracking.Parameters.Add("type_of_loan", OracleDbType.Int32).Value = type_of_loan;
                        // cmdInsertTracking.Parameters.Add("type_of_docs", OracleDbType.Varchar2).Value = type_of_docs;

                        // Output parameter for getting track_id
                        OracleParameter trackIdParameter = new OracleParameter("track_id", OracleDbType.Int32);
                        trackIdParameter.Direction = ParameterDirection.ReturnValue;
                        cmdInsertTracking.Parameters.Add(trackIdParameter);

                        // Execute the command
                        cmdInsertTracking.ExecuteNonQuery();

                        // Get the track_id
                        int trackId = 0; // Initialize with a default value
                        if (trackIdParameter.Value is OracleDecimal oracleDecimalValue)
                        {
                            trackId = oracleDecimalValue.ToInt32();
                        }







                        // Insert into CHECK_PROP_DOCU_MASTER
                        string sqlInsertCheckPropDocuMaster = @"
              insert into CHECKLIST_loan_master(Ch_Loan_Id,Track_Id,Loan_Doc_Id,Loan_No,State_Code,Branch_Id,Docu_Name,Maker_Status,maker_remarks,Maker,Maker_Date) values (Ch_Loan_Id.Nextval,:track_id, :loan_doc_id, :loan_no, :state_code, :branch_id, :docu_name, :maker_status, :maker_remarks, :maker, :maker_date)";
                        foreach (var item in model)
                        {
                            using (OracleCommand cmdInsertCheckPropDocuMaster = new OracleCommand(sqlInsertCheckPropDocuMaster, con))
                            {
                                cmdInsertCheckPropDocuMaster.Parameters.Add("track_id", OracleDbType.Int32).Value = trackId;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("loan_doc_id", OracleDbType.Int32).Value = item.LoanDocId;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("loan_no", OracleDbType.Varchar2).Value = loan_id;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("state_code", OracleDbType.Int32).Value = state_id;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("branch_id", OracleDbType.Int32).Value = branch_id;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("docu_name", OracleDbType.Varchar2).Value = item.LoanDocName;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("maker_status", OracleDbType.Varchar2).Value = item.IsChecked;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("maker_remarks", OracleDbType.Varchar2).Value = item.Remark;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("maker", OracleDbType.Int32).Value = user_id;
                                cmdInsertCheckPropDocuMaster.Parameters.Add("maker_date", OracleDbType.Date).Value = DateTime.Now;

                                // Execute the second command
                                cmdInsertCheckPropDocuMaster.ExecuteNonQuery();
                            }
                            // Break out of the loop after the first iteration

                            // Continue with any other code or return statements as needed
                        }
                    }

                    // Additional code after the database operations if needed

                    // Return an appropriate response
                    return Json(new { success = true, message = "Documents Transfer to Safe" });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
                return Json(new { success = false, message = "Error saving data: " + ex.Message });
            }
        }
        //-----------------Save Loan Document mortgage Completed----------------------------//


        public ActionResult SessionTimeout()
        {
            return View();
        }







    }
}