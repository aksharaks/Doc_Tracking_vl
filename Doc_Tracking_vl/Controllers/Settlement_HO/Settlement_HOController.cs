using Doc_Tracking_vl.App_Code;
using Doc_Tracking_vl.Filters;
using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Doc_Tracking_vl.Controllers.Settlement_HO
{



    [SessionTimeout]
    public class Settlement_HOController : Controller
    {
        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;

        //-------------------------Loan numbers fetch-------------------------------------------------------//


        // GET: Settlement_HO
        public ActionResult Settle_loan_doc()
        {
            Settle_HO_Direct model = new Settle_HO_Direct();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_tracking_vl d, employee_master e, branch_master s where d.branch_id = s.branch_id and s.branch_id = e.branch_id and d.status=5 and d.checker4_ho='" + id + "' and d.settle_status=0";
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
        public ActionResult search_no(Settle_HO_Direct model)
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

                string sql1 = "select distinct l.loan_id, l.branch_id, s.branch_name, l.customer_name from maafin_lms.loan_master l, branch_master s, employee_master e, documents_tracking_vl d where l.branch_id = s.branch_id and l.branch_id = e.branch_id and d.branch_id = l.branch_id and l.loan_id = '" + loan_id + "'";
                OracleCommand cmd = new OracleCommand(sql1, con);

                // Execute the SQL query and fetch branch names
                OracleDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    TempData["loan_id"] = reader["loan_id"].ToString();
                    Session["loan_id"] = reader["loan_id"].ToString();
                    TempData["customer_name"] = reader["customer_name"].ToString();
                    TempData["branch_name"] = reader["branch_name"].ToString();
                    Session["loan_branch_id"] = reader["branch_id"].ToString();
                }

                // Fetch settle types
                string settleTypeSql = "select settle_id, settle_type from DOCVL_SETTLE_TYPE";
                OracleCommand settleTypeCmd = new OracleCommand(settleTypeSql, con);
                OracleDataReader settleTypeReader = settleTypeCmd.ExecuteReader();
                List<SelectListItem> settleTypes = new List<SelectListItem>();

                // Add "Select" as the first option
                settleTypes.Add(new SelectListItem { Text = "Select", Value = "" });

                while (settleTypeReader.Read())
                {
                    string settleId = settleTypeReader["settle_id"].ToString();
                    string settleType = settleTypeReader["settle_type"].ToString();

                    settleTypes.Add(new SelectListItem { Text = settleType, Value = settleId });
                }


                con.Close();

                // Pass loan details and settle types to the partial view
                ViewBag.SettleTypes = settleTypes;
                //ViewBag.ApplicantTypes = applicantTypes;
                //ViewBag.IdTypes= idTypes;
                return PartialView("_Loan_dtl_customer_settle");
            }
            else
            {
                TempData["Message"] = "Select the value!!";
                return PartialView("_Loan_dtl_customer_settle");
            }
        }



        //--------------------------------fetch settled details------------------------------------------//

        [HttpPost]
        public ActionResult FetchDetails(string settlementTypeValue, string settlementTypeText, HttpPostedFileBase[] files, SettleFormModel model)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            try
            {

                con.Open();
                Session["Settle_type"] = settlementTypeText;
                // Fetch settle person type (ApplicantTypes)
                //  List<SelectListItem> applicantTypes = GetApplicantTypes(con) ?? new List<SelectListItem>();

                // Fetch ID types (IdTypes)
                //  List<SelectListItem> idTypes = GetIdTypes(con) ?? new List<SelectListItem>();
                //List<SelectListItem>branchNames=GetBranchName(con)??new List<SelectListItem>();

                //----------------------sales writeoff--------------------------------//


                if (int.TryParse(settlementTypeValue, out int intValue) && intValue == 1)
                {
                    // Fetch direct documents based on settlementTypeValue
                    string directSql = "SELECT s.doc_list_id, s.list_items FROM DOCVL_SETTLE_TYPE d, VL_DOC_SETTLE_LIST s WHERE s.settle_id=d.settle_id AND d.settle_id=:settleId";
                    OracleCommand directCmd = new OracleCommand(directSql, con);
                    directCmd.Parameters.Add(new OracleParameter("settleId", settlementTypeValue));
                    OracleDataReader reader = directCmd.ExecuteReader();

                    var directDocs = new List<SettlementItem>();

                    while (reader.Read())
                    {
                        directDocs.Add(new SettlementItem
                        {
                            DocListId = reader["doc_list_id"].ToString(),
                            ListItems = reader["list_items"].ToString(),
                            IsChecked = null,
                            Details = null,

                            //   ApplicantTypes = applicantTypes, // Add dropdown values for ApplicantTypes
                            // IdTypes = idTypes // Add dropdown values for IdTypes
                        });
                    }

                    con.Close();

                    // Handle file uploads
                    if (files != null)
                    {
                        for (int i = 0; i < directDocs.Count && i < files.Length; i++)
                        {
                            var file = files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                // Save the file to the server or perform further processing
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                                file.SaveAs(path);

                                // Update the SettlementItem with the file information
                                directDocs[i].ImageFile = file;
                            }
                        }
                    }

                    // Create the model and pass it to the partial view
                    var settleFormModel = new SettleFormModel
                    {
                        DirectDocs = directDocs
                    };

                    if (settleFormModel != null)
                    {
                        return PartialView("Sales_writeoff", settleFormModel);
                    }
                }




                //----------------------------Normal settlement--------------------------------------//
                else if (int.TryParse(settlementTypeValue, out int intValue3) && intValue3 == 2)
                {
                    // Fetch direct documents based on settlementTypeValue


                    string courSql = "SELECT s.doc_list_id, s.list_items FROM DOCVL_SETTLE_TYPE d JOIN VL_DOC_SETTLE_LIST s ON d.settle_id = s.settle_id WHERE s.doc_list_id IN ('8', '7', '6', '5') AND d.settle_id =:settleId ";


                    //string courSql = "SELECT s.doc_list_id, s.list_items FROM doc_track_settle_type d, doc_settle_list s  WHERE s.doc_list_id in ('14','15','16','17' ,'18') AND d.settle_id=:settleId";


                    OracleCommand courCmd = new OracleCommand(courSql, con);
                    courCmd.Parameters.Add(new OracleParameter("settleId", settlementTypeValue));
                    OracleDataReader reader = courCmd.ExecuteReader();

                    var courDocs = new List<SettlementItem_Normal>();

                    while (reader.Read())
                    {
                       // var branchName = GetBranchName(con);
                        //var branchText = !string.IsNullOrEmpty(branchName) ? branchName : ""; // Check if the branch name is not null or empty

                        courDocs.Add(new SettlementItem_Normal
                        {
                            DocListId = reader["doc_list_id"].ToString(),
                            ListItems = reader["list_items"].ToString(),
                            IsChecked = null,
                            Details = null,
                            // Assign branch name directly to the BranchName property
                           // BranchName = branchName// Get the branch name from the GetBranchName method
                            //BranchNames = branchNames // Add dropdown values for ApplicantTypes
                            //IdTypes = idTypes // Add dropdown values for IdTypes
                        });
                    }

                    con.Close();

                    // Handle file uploads
                    if (files != null)
                    {
                        for (int i = 0; i < courDocs.Count && i < files.Length; i++)
                        {
                            var file = files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                // Save the file to the server or perform further processing
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                                file.SaveAs(path);

                                // Update the SettlementItem with the file information
                                courDocs[i].ImageFile = file;
                            }
                        }
                    }

                    // Create the model and pass it to the partial view
                    var settleFormModel = new SettleFormModel_Normal
                    {
                        CourDocs = courDocs
                    };

                    if (settleFormModel != null)
                    {
                        return PartialView("_Sales_Partial", settleFormModel);
                    }
                }


                else
                {
                    // Handle the case where settleFormModel is null
                    return HttpNotFound();
                }
                return View("Settle_loan_doc");
            }

            catch (Exception ex)
            {
                // Log the exception details
                Debug.WriteLine($"Exception in FetchDetails: {ex.Message}");
                throw; // Rethrow the exception for now; you may handle it differently in a production environment
            }
        }

        //-----------------------------save sales writeoff-------------------------------------------------------------//

        [HttpPost]
        public ActionResult SaveDetails(SettleFormModel settlementModel)
        {
            //// Set IsChecked property of each item to null before processing the form submission
            //foreach (var item in settlementModel.DirectDocs)
            //{
            //    item.IsChecked = null;
            //}
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1; // Ensure to declare 'constr' variable

            if (Session["user_id"] != null)
            {
                try
                {
                    using (OracleConnection con = new OracleConnection(constr))
                    {
                        con.Open();
                        var user_id = Session["user_id"];
                        var loan_id = Session["loan_id"].ToString();
                        var settle_type = Session["Settle_type"];
                        var branch_id = Session["loan_branch_id"].ToString();
                        var settle_branch_id = Session["branch_id"].ToString();
                        // Check if the loan number is already settled
                        if (IsLoanAlreadySettled(con, loan_id))
                        {
                            TempData["Message"] = "Loan number is already settled.";
                            return RedirectToAction("Settle_loan_doc");
                        }

                        // Insert data into settlement_main table with the next value from the sequence
                        string insertMainSql = @"
INSERT INTO VL_SETTLEMENT_MAIN

    (setle_id, loan_number, settlement_type, created_by, created_date,branch_id,settle_branch_id)
VALUES
    (vl_settlement_seq.NEXTVAL, :loanNumber, :settlementType, :createdBy, :createdDate,:branchId,:settleBranchId)
RETURNING setle_id INTO :setId";

                        using (OracleCommand mainCmd = new OracleCommand(insertMainSql, con))
                        {
                            mainCmd.Parameters.Add(new OracleParameter("loanNumber", loan_id));
                            mainCmd.Parameters.Add(new OracleParameter("settlementType", settle_type));
                            mainCmd.Parameters.Add(new OracleParameter("createdBy", user_id));
                            mainCmd.Parameters.Add(new OracleParameter("createdDate", DateTime.Now));
                            mainCmd.Parameters.Add(new OracleParameter("branchId", branch_id));
                            mainCmd.Parameters.Add(new OracleParameter("settleBranchId", settle_branch_id));

                            // Define output parameter for set_id
                            OracleParameter setIdParameter = new OracleParameter();
                            setIdParameter.ParameterName = "setId";
                            setIdParameter.OracleDbType = OracleDbType.Int32;
                            setIdParameter.Direction = ParameterDirection.Output;
                            mainCmd.Parameters.Add(setIdParameter);

                            mainCmd.ExecuteNonQuery();

                            // After executing the command
                            OracleDecimal setIdDecimal = (OracleDecimal)setIdParameter.Value;
                            int setId = setIdDecimal.ToInt32();


                            //Insert data into SETTLEMENT_DOC_TRACK table
                            foreach (var item in settlementModel.DirectDocs)
                            {
                                string isCheckedValue = item.IsChecked.HasValue ? (item.IsChecked.Value ? "Y" : "N") : "N"; // Handle null value
                                string insertDocSql = "INSERT INTO SettlementVl_Doc_Track (sl_no, set_id, list_items, is_checked,upload,branch_id, consignment_no,handover_to,loan_branch_name,courier_date ) VALUES (SETTLE_DOC_TRACK.nextval, :setId, :listItems, :isChecked, :upload,:handoverto, :branchId,:courierConsignmentNo,:branchName,:courierDate)";
                                using (OracleCommand docCmd = new OracleCommand(insertDocSql, con))
                                {
                                    docCmd.Parameters.Add(new OracleParameter("setId", setId));
                                    docCmd.Parameters.Add(new OracleParameter("listItems", item.ListItems));
                                    //docCmd.Parameters.Add(new OracleParameter("isChecked", item.IsChecked.HasValue && item.IsChecked.Value ? "Y" : "N"));
                                    docCmd.Parameters.Add(new OracleParameter("isChecked", isCheckedValue)); // Use the modified value here                                 
                                    docCmd.Parameters.Add(new OracleParameter("upload", ConvertFileToByteArray(item.ImageFile)));
                                    docCmd.Parameters.Add(new OracleParameter("branchId", branch_id));
                                    docCmd.Parameters.Add(new OracleParameter("courierConsignmentNo", item.CourierConsignmentNo));
                                    docCmd.Parameters.Add(new OracleParameter("branchName", item.BranchName));
                                    docCmd.Parameters.Add(new OracleParameter("courierComanyName", item.handoverto));
                                    docCmd.Parameters.Add(new OracleParameter("courierDate", item.CourierDate));

                                    docCmd.ExecuteNonQuery();
                                }
                            }

                            string updtesql = @"UPDATE documents_tracking_vl t 
                     SET t.settle_status=1,t.settle_date=sysdate
                     WHERE t.loan_no = :LoanNumber";
                            using (OracleCommand docupdate = new OracleCommand(updtesql, con))
                            {
                                docupdate.Parameters.Add(new OracleParameter("LoanNumber", loan_id));
                                docupdate.ExecuteNonQuery();
                            }



                        }
                    }
                    // Redirect to a success page or return a success message
                    TempData["Message"] = "Loan number has been settled successfully.";
                    // Redirect to a success page or return a success message
                    return RedirectToAction("Settle_loan_doc");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    return RedirectToAction("Error");
                }
            }

            return RedirectToAction("Settle_loan_doc");
        }

        // Function to check if the loan number is already settled
        private bool IsLoanAlreadySettled(OracleConnection con, string loanNumber)
        {
            string query = "SELECT COUNT(*) FROM VL_SETTLEMENT_MAIN WHERE loan_number = :loanNumber";
            using (OracleCommand cmd = new OracleCommand(query, con))
            {
                cmd.Parameters.Add(new OracleParameter("loanNumber", loanNumber));
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }


        // Helper method to convert HttpPostedFileBase to byte array
        private byte[] ConvertFileToByteArray(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    return binaryReader.ReadBytes(file.ContentLength);
                }
            }
            return null;
        }

        //-----------------------------save Normal settlement-----------------------------------------------------------//

        [HttpPost]
        public ActionResult SaveDetails_Normal(SettleFormModel_Normal settlementModel)
        {
            //// Set IsChecked property of each item to null before processing the form submission
            //foreach (var item in settlementModel.DirectDocs)
            //{
            //    item.IsChecked = null;
            //}
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1; // Ensure to declare 'constr' variable

            if (Session["user_id"] != null)
            {
                try
                {
                    using (OracleConnection con = new OracleConnection(constr))
                    {
                        con.Open();
                        var user_id = Session["user_id"];
                        var loan_id = Session["loan_id"].ToString();
                        var settle_type = Session["Settle_type"];
                        var branch_id = Session["loan_branch_id"].ToString();
                        var settle_branch_id = Session["branch_id"].ToString();
                        // Check if the loan number is already settled
                        if (IsLoanAlreadySettled1(con, loan_id))
                        {
                            TempData["Message"] = "Loan number is already settled.";
                            return RedirectToAction("Settle_loan_doc");
                        }

                        // Insert data into settlement_main table with the next value from the sequence
                        string insertMainSql = @"
INSERT INTO VL_SETTLEMENT_MAIN

    (setle_id, loan_number, settlement_type, created_by, created_date,branch_id,settle_branch_id)
VALUES
    (vl_settlement_seq.NEXTVAL, :loanNumber, :settlementType, :createdBy, :createdDate,:branchId,:settleBranchId)
RETURNING setle_id INTO :setId";

                        using (OracleCommand mainCmd = new OracleCommand(insertMainSql, con))
                        {
                            mainCmd.Parameters.Add(new OracleParameter("loanNumber", loan_id));
                            mainCmd.Parameters.Add(new OracleParameter("settlementType", settle_type));
                            mainCmd.Parameters.Add(new OracleParameter("createdBy", user_id));
                            mainCmd.Parameters.Add(new OracleParameter("createdDate", DateTime.Now));
                            mainCmd.Parameters.Add(new OracleParameter("branchId", branch_id));
                            mainCmd.Parameters.Add(new OracleParameter("settleBranchId", settle_branch_id));

                            // Define output parameter for set_id
                            OracleParameter setIdParameter = new OracleParameter();
                            setIdParameter.ParameterName = "setId";
                            setIdParameter.OracleDbType = OracleDbType.Int32;
                            setIdParameter.Direction = ParameterDirection.Output;
                            mainCmd.Parameters.Add(setIdParameter);

                            mainCmd.ExecuteNonQuery();

                            // After executing the command
                            OracleDecimal setIdDecimal = (OracleDecimal)setIdParameter.Value;
                            int setId = setIdDecimal.ToInt32();


                            //Insert data into SETTLEMENT_DOC_TRACK table
                            foreach (var item in settlementModel.CourDocs)
                            {
                                string isCheckedValue = item.IsChecked.HasValue ? (item.IsChecked.Value ? "Y" : "N") : "N"; // Handle null value
                                string insertDocSql = "INSERT INTO SettlementVl_Doc_Track (sl_no, set_id, list_items, is_checked,upload,branch_id, consignment_no,handover_to,loan_branch_name,courier_date ) VALUES (SETTLE_DOC_TRACK.nextval, :setId, :listItems, :isChecked, :upload,:handoverto, :branchId,:courierConsignmentNo,:branchName,:courierDate)";
                                using (OracleCommand docCmd = new OracleCommand(insertDocSql, con))
                                {
                                    docCmd.Parameters.Add(new OracleParameter("setId", setId));
                                    docCmd.Parameters.Add(new OracleParameter("listItems", item.ListItems));
                                    //docCmd.Parameters.Add(new OracleParameter("isChecked", item.IsChecked.HasValue && item.IsChecked.Value ? "Y" : "N"));
                                    docCmd.Parameters.Add(new OracleParameter("isChecked", isCheckedValue)); // Use the modified value here                                 
                                    docCmd.Parameters.Add(new OracleParameter("upload", ConvertFileToByteArray1(item.ImageFile)));
                                    docCmd.Parameters.Add(new OracleParameter("branchId", branch_id));
                                    docCmd.Parameters.Add(new OracleParameter("courierConsignmentNo", item.CourierConsignmentNo));
                                    docCmd.Parameters.Add(new OracleParameter("branchName", item.BranchName));
                                    docCmd.Parameters.Add(new OracleParameter("courierComanyName", item.handoverto));
                                    docCmd.Parameters.Add(new OracleParameter("courierDate", item.CourierDate));

                                    docCmd.ExecuteNonQuery();
                                }
                            }

                            string updtesql = @"UPDATE documents_tracking_vl t 
                     SET t.settle_status=1,t.settle_date=sysdate
                     WHERE t.loan_no = :LoanNumber";
                            using (OracleCommand docupdate = new OracleCommand(updtesql, con))
                            {
                                docupdate.Parameters.Add(new OracleParameter("LoanNumber", loan_id));
                                docupdate.ExecuteNonQuery();
                            }



                        }
                    }
                    // Redirect to a success page or return a success message
                    TempData["Message"] = "Loan number has been settled successfully.";
                    // Redirect to a success page or return a success message
                    return RedirectToAction("Settle_loan_doc");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    return RedirectToAction("Error");
                }
            }

            return RedirectToAction("Settle_loan_doc");
        }

        // Function to check if the loan number is already settled
        private bool IsLoanAlreadySettled1(OracleConnection con, string loanNumber)
        {
            string query = "SELECT COUNT(*) FROM VL_SETTLEMENT_MAIN WHERE loan_number = :loanNumber";
            using (OracleCommand cmd = new OracleCommand(query, con))
            {
                cmd.Parameters.Add(new OracleParameter("loanNumber", loanNumber));
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }


        // Helper method to convert HttpPostedFileBase to byte array
        private byte[] ConvertFileToByteArray1(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    return binaryReader.ReadBytes(file.ContentLength);
                }
            }
            return null;
        }

        //----------------------------settled loan numbers view-----------------------------//

        [HttpPost]

        public ActionResult View_settleloan()
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            string constr = mdh.conStr1;

            List<settle_loan_no> settleData = new List<settle_loan_no>(); // Assuming Settle_HO_Direct is your model class

            if (Session["user_id"] != null)
            {
                try
                {
                    using (OracleConnection con = new OracleConnection(constr))
                    {
                        con.Open();

                        // Your SQL query to fetch data
                        string query = @"
                   SELECT DISTINCT
     s.loan_number,
     b.branch_name AS loan_branch_name,
     b1.branch_name AS settle_branch_name,
     s.settlement_type AS settled_Type,
     CASE 
         WHEN d.settle_status = 1 THEN 'settled'
         ELSE 'not settled'
     END AS settle_status,
     TO_CHAR(d.settle_date, 'YYYY-MM-DD') AS settle_date
 FROM 
    vl_settlement_main s
     JOIN documents_tracking_vl d ON s.loan_number = d.loan_no
     JOIN branch_master b ON b.branch_id = s.branch_id
     JOIN branch_master b1 ON b1.branch_id = s.branch_id
 WHERE 
     d.settle_status = 1";

                        using (OracleCommand cmd = new OracleCommand(query, con))
                        {
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    settle_loan_no settleItem = new settle_loan_no();
                                    settleItem.loan_number = reader["loan_number"].ToString();
                                    settleItem.loan_branch_name = reader["loan_branch_name"].ToString();
                                    settleItem.settlement_type = reader["settled_Type"].ToString();
                                    settleItem.settle_branch_name = reader["settle_branch_name"].ToString();
                                    settleItem.settle_status = reader["settle_status"].ToString();
                                   
                                    // Convert string date to DateTime
                                    if (DateTime.TryParse(reader["settle_date"].ToString(), out DateTime settleDate))
                                    {
                                        settleItem.settle_date = settleDate;
                                    }
                                    else
                                    {
                                        // Handle invalid date format if needed
                                        settleItem.settle_date = DateTime.MinValue; // or set to null
                                    }
                                    settleData.Add(settleItem);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine(ex.Message);
                }
            }

            return View(settleData); // Pass the list of data to the view
        }

    }
}


   


        
