using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Doc_Tracking_vl.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please enter username")]
        public int Userid { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        public string PassWord { get; set; }
        public string New_PassWord { get; set; }
        public string Confirm_PassWord { get; set; }
    }
}