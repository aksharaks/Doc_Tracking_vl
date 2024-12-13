using Doc_Tracking_vl.App_Code;
using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Controllers.Settlement_Branch
{
    public class Settlement_BranchController : Controller
    {
        // GET: Settlement_Branch

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;
        public ActionResult Settle_loan_doc_branch()
        {
            Settle_Branch_Direct model = new Settle_Branch_Direct();
            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;
            OracleConnection con = new OracleConnection(constr);



            if (Session["user_id"] != null)
            {

                var id = Session["user_id"].ToString();
                var branch_id = Session["branch_id"].ToString();
                con.Open();
                string sql = "select distinct d.loan_no from documents_tracking_vl d, employee_master e,  branch_master s where d.branch_id = s.branch_id and s.branch_id = e.branch_id and d.status=5 and d.maker='" + id + "' and d.branch_id='" + branch_id + "' and d.settle_status NOT IN (1,2)";
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

    }
}