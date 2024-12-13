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

namespace Doc_Tracking_vl.Controllers.Transfer_to_HO
{

    [SessionTimeout]
    public class VL_op_headController : Controller
    {
        // GET: VL_op_head

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
       
        
        
      
        
        public ActionResult Op_Head_Entry()

        {
            Courier_entry model = new Courier_entry();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1; // Assuming conStr1 is your Oracle connection string
            try
            {
                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();

                    if (Session["user_id"] != null)
                    {
                        var id = Session["user_id"].ToString();

                        // Retrieve loan numbers based on the user's session ID
                        string sql = "SELECT DISTINCT d.loan_no FROM documents_tracking_vl d, employee_master e, branch_master s " +
                                     "WHERE d.branch_id = s.branch_id AND s.branch_id = e.branch_id " +
                                     "AND d.return_status = 4 AND e.emp_code = :EmpCode";

                        using (OracleCommand cmd = new OracleCommand(sql, con))
                        {
                            cmd.Parameters.Add(new OracleParameter("EmpCode", id));

                            // Execute the SQL command to get loan numbers
                            OracleDataReader reader = cmd.ExecuteReader();
                            List<SelectListItem> loanNumbers = new List<SelectListItem>();

                            while (reader.Read())
                            {
                                string loanNumber = reader["loan_no"].ToString();
                                loanNumbers.Add(new SelectListItem
                                {
                                    Text = loanNumber,
                                    Value = loanNumber
                                });
                            }

                            con.Close();

                            // Create a SelectList from the list of loan numbers
                            SelectList loanNumbersSelectList = new SelectList(loanNumbers, "Value", "Text");

                            // Assign the SelectList to the model's LoanNumbers property
                            model.LoanNumbers = loanNumbersSelectList;
                        }
                    }
                }

                // Set other ViewBag or model properties as needed
                //ViewBag.LoanMode = Session["doc_type"]?.ToString(); // Example usage

                return View(model);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                ViewBag.ErrorMessage = "Error retrieving loan numbers: " + ex.Message;
                return View(model); // Return view with model even in case of error
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
                         "FROM documents_tracking_vl d, CHECKLIST_LOAN_MASTER c " +
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
                            if (loanMode == "vehicle")
                            {
                                loanDocType = "vehicle";
                            }
                            // You can add more conditions as needed
                        }
                    }
                }
            }

            return loanDocType;
        }

        // POST: SME_Op_Head/UpdateDocument
        [HttpPost]

        public ActionResult Submit_entry(Courier_entry model)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1; // Assuming conStr1 is your Oracle connection string

            try
            {
                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();

                    // Loop through each selected loan number and document type
                    for (int i = 0; i < model.SelectedLoanNumber.Count; i++)
                    {
                        // Define your SQL query for each selected loan number and document type
                        string query = @"UPDATE documents_tracking_vl t 
                                 SET t.courier_company_name = :CourierName, 
                                     t.consignment_no = :ConsignmentNo, 
                                     t.date_of_courier = :CourierDate,
                                     t.return_status=5
                                 WHERE t.loan_no = :LoanNumber";

                        using (OracleCommand command = new OracleCommand(query, con))
                        {
                            // Add parameters to the query for the current loan number and document type
                            command.Parameters.Add(new OracleParameter("CourierName", model.courier_company_name));
                            command.Parameters.Add(new OracleParameter("ConsignmentNo", model.consignment_no));
                            command.Parameters.Add(new OracleParameter("CourierDate", model.date_of_courier));
                            command.Parameters.Add(new OracleParameter("LoanNumber", model.SelectedLoanNumber[i]));
                            // command.Parameters.Add(new OracleParameter("DocType", model.SelectedDocTypes[i]));

                            try
                            {
                                // Execute query for the current loan number and document type
                                int rowsAffected = command.ExecuteNonQuery();

                                // Check if any rows were affected
                                if (rowsAffected <= 0)
                                {
                                    // Handle case where no rows were updated
                                    ViewBag.Message = "No rows were updated for loan number: " + model.SelectedLoanNumber[i];
                                }
                            }
                            catch (Exception ex)
                            {
                                // Handle specific loan number and document type update error
                                ViewBag.Message = "An error occurred while updating loan number " + model.SelectedLoanNumber[i] + ": " + ex.Message;
                            }
                        }
                    }

                    // Redirect to a view or return JSON response as needed after processing all updates
                    return Json(new { success = true, message = "Courier data submitted successfully." });
                }
            }
            catch (Exception ex)
            {
                // Handle connection errors
                ViewBag.Message = "Connection error: " + ex.Message;
                return Json(new { success = false, message = "An error occurred while processing the request." });
            }
        }

    }
}