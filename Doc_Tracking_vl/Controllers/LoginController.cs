using Doc_Tracking_vl.Models;
using Oracle.ManagedDataAccess.Client;
using Doc_Tracking_vl.App_Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Doc_Tracking_vl.Controllers
{
    public class LoginController : Controller
    {

        static DataSet ds = null;
        OracleCommand com = new OracleCommand();

        public string conStr1;
        public string constr;
        OracleDataReader dr;



        public ActionResult Login()
        {
            return View();
        }

        // GET: Login




        [HttpPost]
        public ActionResult Login(LoginModel t)
        {


            MaafinDbHelper mdh = new MaafinDbHelper();
            mdh.Connection();
            constr = mdh.conStr1;

            OracleConnection con = new OracleConnection(constr);
            string check = "select count(*)from logininternal l where l.emp_code = '" + t.Userid + "' and  l.password_int = '" + t.PassWord + "'";
            OracleCommand com = new OracleCommand(check, con);

            con.Open();
            int temp = Convert.ToInt32(com.ExecuteScalar().ToString());
            Session["user_id"] = t.Userid;
            con.Close();

            int id = Convert.ToInt32(Session["user_id"]);
            //branch operation head
            if (temp == 1)
            {
                OracleDataAdapter ada = new OracleDataAdapter();
                DataTable dt = new DataTable();
                con.Open();
                com.Connection = con;
                com.CommandText = "select l.emp_code, l.emp_name, pm.post_name, pm.post_id, em.branch_id, b.BRANCH_NAME from logininternal l, employee_master em, branch_master b, post_mst pm where l.emp_code = em.emp_code and em.branch_id = b.branch_id and em.post_id = pm.post_id and em.post_id in (868, 867, 869) and em.status_id = 1 and em.department_id = 456 and em.emp_code = '" + id + "'";
                com.CommandType = CommandType.Text;
                ada.SelectCommand = com;
                ada.Fill(dt);
                dr = com.ExecuteReader();


                if (dr.HasRows)
                {
                    string empname = dt.Rows[0]["emp_name"].ToString();
                    string post = dt.Rows[0]["post_name"].ToString();
                    string branchname = dt.Rows[0]["branch_name"].ToString();
                    string branchid = dt.Rows[0]["branch_id"].ToString();
                    Session["username"] = empname;
                    Session["Post"] = post;
                    Session["branch_name"] = branchname;
                    Session["branch_id"] = branchid;
                    return this.RedirectToAction("Dashboard", "Vl_operation_Head");

                }
            }
            //branch head
            //if (temp == 1)
            //{
            //    OracleDataAdapter ada = new OracleDataAdapter();
            //    DataTable dt = new DataTable();

            //    com.Connection = con;
            //    com.CommandText = "select b.branch_name,b.branch_id,e.emp_name, p.post_name from employee_master e, branch_master b, post_mst p where e.post_id = 757 and e.department_id = 456 and e.branch_id = b.branch_id and e.status_id = 1 and p.post_id = e.post_id and e.emp_code = '" + id + "'";
            //    com.CommandType = CommandType.Text;
            //    ada.SelectCommand = com;
            //    ada.Fill(dt);
            //    dr = com.ExecuteReader();


            //    if (dr.HasRows)
            //    {
            //        string empname = dt.Rows[0]["emp_name"].ToString();
            //        string post = dt.Rows[0]["post_name"].ToString();
            //        string branchname = dt.Rows[0]["branch_name"].ToString();
            //        string branchid = dt.Rows[0]["branch_id"].ToString();
            //        Session["username"] = empname;
            //        Session["Post"] = post;
            //        Session["branch_name"] = branchname;
            //        Session["branch_id"] = branchid;
            //        return this.RedirectToAction("Dashboard_HO", "Head_Office");

            //    }
            //}
            //-----------------------------------------------------------//


            //branch head
            if (temp == 1)
            {
                OracleDataAdapter ada = new OracleDataAdapter();
                DataTable dt = new DataTable();

                com.Connection = con;
                //com.CommandText = "select b.branch_name, b.branch_id, e.emp_name, p.post_name,p.post_id from employee_master e, branch_master b, post_mst p where e.department_id =456 and e.branch_id = b.branch_id and e.status_id = 1 and e.emp_code = '" + id + "'";

                com.CommandText = "select b.branch_name, b.branch_id, e.emp_name, p.post_name,p.post_id from employee_master e, branch_master b, post_mst p where e.department_id =456 and e.branch_id = b.branch_id and e.status_id = 1 and p.post_id = e.post_id and e.post_id = 936 and e.emp_code = '" + id + "'";
                com.CommandType = CommandType.Text;
                ada.SelectCommand = com;
                ada.Fill(dt);
                dr = com.ExecuteReader();
                //dep id = 456 , post id= 936

                if (dr.HasRows)
                {
                    string empname = dt.Rows[0]["emp_name"].ToString();
                    string post = dt.Rows[0]["post_name"].ToString();
                    string branchname = dt.Rows[0]["branch_name"].ToString();
                    string branchid = dt.Rows[0]["branch_id"].ToString();
                    Session["username"] = empname;
                    Session["Post"] = post;
                    Session["branch_name"] = branchname;
                    Session["branch_id"] = branchid;
                    return this.RedirectToAction("Dashboard", "vl_Branch_Head");


                }
            }
            // gl abh
            if (temp == 1)
            {
                OracleDataAdapter ada = new OracleDataAdapter();
                DataTable dt = new DataTable();

                com.Connection = con;
                com.CommandText = "select b.branch_name, b.branch_id, e.emp_name, p.post_name from employee_master e, branch_master b, post_mst p where e.post_id = 1 and e.department_id = 14 and e.branch_id = b.BRANCH_ID and e.status_id = 1 and p.post_id = e.post_id and e.emp_code = '" + id + "' ";
                com.CommandType = CommandType.Text;
                ada.SelectCommand = com;
                ada.Fill(dt);
                dr = com.ExecuteReader();


                if (dr.HasRows)
                {
                    string empname = dt.Rows[0]["emp_name"].ToString();
                    string post = dt.Rows[0]["post_name"].ToString();
                    string branchname = dt.Rows[0]["branch_name"].ToString();
                    string branchid = dt.Rows[0]["branch_id"].ToString();
                    Session["username"] = empname;
                    Session["Post"] = post;
                    Session["branch_name"] = branchname;
                    Session["branch_id"] = branchid;
                    return this.RedirectToAction("Dashboard", "GL_ABH");

                }
            }


            //GL BH
            
            if (temp == 1)
            {
                OracleDataAdapter ada = new OracleDataAdapter();
                DataTable dt = new DataTable();

                com.Connection = con;
                com.CommandText = "select b.branch_name, b.branch_id, e.emp_name, p.post_name from employee_master e, branch_master b, post_mst p where e.post_id = 10 and e.department_id = 14 and e.branch_id = b.BRANCH_ID and e.status_id = 1 and p.post_id = e.post_id and e.emp_code = '" + id + "' ";
                com.CommandType = CommandType.Text;
                ada.SelectCommand = com;
                ada.Fill(dt);
                dr = com.ExecuteReader();


                if (dr.HasRows)
                {
                    string empname = dt.Rows[0]["emp_name"].ToString();
                    string post = dt.Rows[0]["post_name"].ToString();
                    string branchname = dt.Rows[0]["branch_name"].ToString();
                    string branchid = dt.Rows[0]["branch_id"].ToString();
                    Session["username"] = empname;
                    Session["Post"] = post;
                    Session["branch_name"] = branchname;
                    Session["branch_id"] = branchid;
                    return this.RedirectToAction("Dashboard_GL_BH", "GL_BH");

                }


            }


            //HO

            if (temp == 1)
            {
                OracleDataAdapter ada = new OracleDataAdapter();
                DataTable dt = new DataTable();

                com.Connection = con;
                com.CommandText = "select b.branch_name,b.branch_id,e.emp_name, p.post_name from employee_master e, branch_master b, post_mst p where e.post_id = 757 and e.department_id = 456 and e.branch_id = b.branch_id and e.status_id = 1 and p.post_id = e.post_id and e.emp_code = '" + id + "'";
                com.CommandType = CommandType.Text;
                ada.SelectCommand = com;
                ada.Fill(dt);
                dr = com.ExecuteReader();


                if (dr.HasRows)
                {
                    string empname = dt.Rows[0]["emp_name"].ToString();
                    string post = dt.Rows[0]["post_name"].ToString();
                    string branchname = dt.Rows[0]["branch_name"].ToString();
                    string branchid = dt.Rows[0]["branch_id"].ToString();
                    Session["username"] = empname;
                    Session["Post"] = post;
                    Session["branch_name"] = branchname;
                    Session["branch_id"] = branchid;
                    return this.RedirectToAction("Dashboard_HO", "Head_Office");

                }
                else
            {
                return this.RedirectToAction("Login", "Login");
            }


            }
            else
            {

                con.Close();
                TempData["Message"] = "Incorrect Password";
                return View("Login");

            }
        }
    }


}


