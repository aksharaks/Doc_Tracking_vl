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
    public class GL_BHController : Controller
    {
        // GET: GL_BH

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        public ActionResult Dashboard_GL_BH()
        {
            return View();
        }


        public ActionResult Safe_branch()
        {
            Checklist_Model_GL_BH model = new Checklist_Model_GL_BH();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);

            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_tracking_vl d, employee_master e, branch_master s where d.branch_id = s.branch_id and s.branch_id = e.branch_id and d.status = 3 and e.emp_code='" + id + "'";
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

        public ActionResult Search_doc(Checklist_Model_GL_BH v, string loanMode)
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
                        string loanDocSql = "SELECT l.track_id, l.loan_doc_id, l.docu_name, l.checker2_status FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id = l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' ORDER BY l.loan_doc_id ASC";

                        //string loanDocSql = "SELECT MIN(l.track_id) AS track_id, l.loan_doc_id, l.docu_name, l.checker2_status FROM DOCUMENTS_TRACKING_VL d, CHECKLIST_LOAN_MASTER l WHERE d.track_id = l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' GROUP BY l.loan_doc_id, l.docu_name, l.checker2_status ORDER BY l.loan_doc_id ASC";

                        OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
                        loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

                        OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

                        // Initialize the LoanDocsModel list
                        v.LoanDocsModel = new LoanDocModel_Mortgage_gl_bh
                        {
                            LoanDocs = new List<LoanDoc_gl_bh>()
                        };

                        while (loanDocReader.Read())
                        {
                            Session["track_id_loan"] = loanDocReader["track_id"].ToString();

                            // Populate the properties for Loan Document and add to the list
                            var loanDoc = new LoanDoc_gl_bh
                            {
                                LoanDocId = loanDocReader["loan_doc_id"].ToString(),
                                LoanDocName = loanDocReader["docu_name"].ToString(),
                                IsChecked = true,
                                Checker2_Status = loanDocReader["checker2_status"].ToString()
                            };

                            v.LoanDocsModel.LoanDocs.Add(loanDoc);
                        }

                        // Close the connection after executing the query
                        con.Close();

                        // Return the partial view with the populated data
                        return PartialView("_Loan_Checklist_GL_BH", v);
                    }

                    // If the condition is not met, close the connection
                    con.Close();
                }

                // Return to the main view if no user session exists or condition is not met
                return View("handover_GL_BH");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return View("Error"); // Return an error view
            }
        }




        //public ActionResult Search_doc(Checklist_Model_GL_BH v, string loanMode)
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
        //            Set loanMode in ViewBag
        //            ViewBag.LoanMode = loanMode;
        //            Check the selected document type and execute the corresponding query
        //            if (v.SelectedDocType == "Loan Document")
        //            {
        //                if (loanMode == "Mortgage")
        //                {
        //                    string loanDocSql = "SELECT l.track_id,l.loan_doc_id,l.docu_name,l.checker2_status  FROM documents_branch_tracking d, check_loan_docu_master l WHERE d.track_id=l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' order by l.loan_doc_id asc";
        //                    OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
        //                    loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

        //                    OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

        //                    v.LoanDocsModel = new LoanDocModel_Mortgage_gl_bh
        //                    {
        //                        LoanDocs = new List<LoanDoc_gl_bh>() // Initialize the list
        //                    };

        //                    while (loanDocReader.Read())
        //                    {
        //                        Session["track_id_loan"] = loanDocReader["track_id"].ToString();
        //                        Populate the properties for Loan Document and add it to the list

        //                       var loanDoc = new LoanDoc_gl_bh
        //                       {
        //                           LoanDocId = loanDocReader["loan_doc_id"].ToString(),
        //                           LoanDocName = loanDocReader["docu_name"].ToString(),
        //                           IsChecked = true,
        //                           Checker2_Status = loanDocReader["checker2_status"].ToString()
        //                       };

        //                        v.LoanDocsModel.LoanDocs.Add(loanDoc);
        //                    }
        //                    return PartialView("_Loan_Checklist_GL_BH", v);
        //                }
        //                else if (loanMode == "Tranche")
        //                {
        //                    string loanDocSql = "SELECT l.track_id,l.loan_doc_id_tranche,l.docu_name,l.checker2_status  FROM documents_branch_tracking d, check_loan_docu_master l WHERE d.track_id=l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Loan Document' order by l.loan_doc_id_tranche asc";
        //                    OracleCommand loanDocCmd = new OracleCommand(loanDocSql, con);
        //                    loanDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

        //                    OracleDataReader loanDocReader = loanDocCmd.ExecuteReader();

        //                    v.LoanDocsModel_tranche = new LoanDocModel_Tranche_gl_bh
        //                    {
        //                        LoanDocs = new List<LoanDoc_gl_bh_tranche>() // Initialize the list
        //                    };

        //                    while (loanDocReader.Read())
        //                    {
        //                        Session["track_id_loan"] = loanDocReader["track_id"].ToString();
        //                        Populate the properties for Loan Document and add it to the list

        //                       var loanDoc = new LoanDoc_gl_bh_tranche
        //                       {
        //                           LoanDocId = loanDocReader["loan_doc_id_tranche"].ToString(),
        //                           LoanDocName = loanDocReader["docu_name"].ToString(),
        //                           IsChecked = true,
        //                           Checker2_Status = loanDocReader["checker2_status"].ToString()
        //                       };

        //                        v.LoanDocsModel_tranche.LoanDocs.Add(loanDoc);
        //                    }
        //                    return PartialView("_Loan_Tranche_Check", v);
        //                }
        //            }
        //            else if (v.SelectedDocType == "Property Document")
        //            {
        //                if (loanMode == "Mortgage")
        //                {
        //                    string propertyDocSql = "SELECT l.track_id, l.property_doc_id,l.docu_name,l.checker2_status FROM documents_branch_tracking d, check_prop_docu_master l WHERE d.track_id=l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Property Document' order by l.property_doc_id asc";
        //                    OracleCommand propertyDocCmd = new OracleCommand(propertyDocSql, con);
        //                    propertyDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

        //                    OracleDataReader propertyDocReader = propertyDocCmd.ExecuteReader();

        //                    v.PropertyDocsModel = new PropertyDocModel_gl_bh
        //                    {
        //                        PropertyDocs = new List<PropertyDoc_gl_bh>()
        //                    };// Create a new list to store property documents

        //                    while (propertyDocReader.Read())
        //                    {
        //                        Session["track_id_property"] = propertyDocReader["track_id"].ToString();
        //                        Populate the properties for Property Document and add it to the list

        //                       var propertyDoc = new PropertyDoc_gl_bh
        //                       {
        //                           PropertyDocId = propertyDocReader["property_doc_id"].ToString(),
        //                           PropertyDocName = propertyDocReader["docu_name"].ToString(),
        //                           IsChecked = true,
        //                           Checker2_Status = propertyDocReader["checker2_status"].ToString()

        //                       };

        //                        v.PropertyDocsModel.PropertyDocs.Add(propertyDoc);
        //                    }
        //                    return PartialView("_Property_Checklist_GL_BH", v);
        //                }
        //                else if (loanMode == "Tranche")
        //                {
        //                    string propertyDocSql = "SELECT l.track_id, l.property_doc_id,l.docu_name,l.checker2_status FROM documents_branch_tracking d, check_prop_docu_master l WHERE d.track_id=l.track_id AND d.loan_no = :loanNo AND d.type_of_docs = 'Property Document' order by l.property_doc_id asc";
        //                    OracleCommand propertyDocCmd = new OracleCommand(propertyDocSql, con);
        //                    propertyDocCmd.Parameters.Add(":loanNo", v.SelectedLoanNumber);

        //                    OracleDataReader propertyDocReader = propertyDocCmd.ExecuteReader();

        //                    v.PropertyDocsModel_tranche = new PropertyDocModel_gl_bh_tranche
        //                    {
        //                        PropertyDocs = new List<PropertyDoc_gl_bh_tranche>()
        //                    };// Create a new list to store property documents

        //                    while (propertyDocReader.Read())
        //                    {
        //                        Session["track_id_property"] = propertyDocReader["track_id"].ToString();
        //                        Populate the properties for Property Document and add it to the list

        //                       var propertyDoc = new PropertyDoc_gl_bh_tranche
        //                       {
        //                           PropertyDocId = propertyDocReader["property_doc_id"].ToString(),
        //                           PropertyDocName = propertyDocReader["docu_name"].ToString(),
        //                           IsChecked = true,
        //                           Checker2_Status = propertyDocReader["checker2_status"].ToString()

        //                       };

        //                        v.PropertyDocsModel_tranche.PropertyDocs.Add(propertyDoc);
        //                    }
        //                    return PartialView("_Property_Tranche_Check", v);
        //                }
        //            }
        //            else
        //            {
        //                Handle other document types if needed
        //            }
        //            con.Close();
        //        }
        //        return View("handover_GL_BH");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log the exception or handle it appropriately
        //        Console.WriteLine(ex.Message);
        //        return View("Error"); // Return an error view
        //    }
        //}
        //--------------------------------------Save Checklist property mortgage -----------------------//



        //---------------------------------Get laon mode---------------------------------//


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
                         "FROM DOCUMENTS_TRACKING_VL d,CHECKLIST_LOAN_MASTER c " +
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


        public ActionResult Save_Loan_Mortgage_gl_bh(List<LoanDocumentsMortgage_vl_bh> model, string selectedLoanNumber, string selectedDocType)
        {
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            try
            {
                // Update DOCUMENTS_BRANCH_TRACKING
                string sqlUpdateDocumentsBranchTracking1 = "UPDATE  documents_tracking_vl t SET t.checker3_glbh = :userid, t.checker3_date = sysdate, t.status = 4 WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";
                // Update CHECK_PROP_DOCU_MASTER
                string sqlUpdateCheckLoanDocuMaster = "UPDATE CHECKLIST_LOAN_MASTER c SET c.checker3_status = :checker3_status, c.checker3_remarks = :checker3_remarks, c.checker3_glbh = :userid, c.checker3_date = sysdate WHERE c.loan_no = :loan_number AND c.loan_doc_id = :loan_doc_id";

                using (OracleConnection con = new OracleConnection(constr))
                {
                    con.Open();

                    // Retrieve data from the model
                    var user_id = Session["user_id"];
                    var track_id = Session["track_id"];

                    // Update DOCUMENTS_BRANCH_TRACKING
                    string sqlCheckDocumentsBranchTracking = "SELECT t.status FROM DOCUMENTS_BRANCH_TRACKING t WHERE t.loan_no = :loan_number AND t.type_of_docs = :doc_type";

                    using (OracleCommand cmdCheckTracking = new OracleCommand(sqlCheckDocumentsBranchTracking, con))
                    {
                        cmdCheckTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                        cmdCheckTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;

                        // Execute the query to check the status
                        var status = cmdCheckTracking.ExecuteScalar();

                        if (status != null && status.ToString() == "4")
                        {
                            // Status is already 2, return appropriate response indicating that the documents have already been verified
                            return Json(new { success = false, message = "Documents are already verified" });
                        }
                        else
                        {

                            using (OracleCommand cmdUpdateTracking = new OracleCommand(sqlUpdateDocumentsBranchTracking1, con))
                            {
                                cmdUpdateTracking.Parameters.Add(":userid", OracleDbType.Int32).Value = user_id;
                                cmdUpdateTracking.Parameters.Add(":loan_number", OracleDbType.Varchar2).Value = selectedLoanNumber;
                                cmdUpdateTracking.Parameters.Add(":doc_type", OracleDbType.Varchar2).Value = selectedDocType;
                                cmdUpdateTracking.ExecuteNonQuery();
                            }


                            using (OracleCommand cmdUpdateCheckLoanDocuMaster = new OracleCommand(sqlUpdateCheckLoanDocuMaster, con))
                            {
                                // Loop through each record in 'model'
                                foreach (var item in model)
                                {
                                    // Set parameters for the current record
                                    cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker3_status", OracleDbType.Varchar2).Value = item.IsChecked;
                                    cmdUpdateCheckLoanDocuMaster.Parameters.Add(":checker3_remarks", OracleDbType.Varchar2).Value = item.Remark;
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

        //---------------------------------------------------------------------

    }
}