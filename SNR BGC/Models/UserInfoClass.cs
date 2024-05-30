using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class UserInfoClass
    {
        [Key]
        public int userId { get; set; }

        public string username { get; set; }
        public string userFullname { get; set; }
        public string userRole { get; set; }
        public string userModule { get; set; }
        public string userSubModule { get; set; }
        public string userAccess { get; set; }
        public string userStatus { get; set; }
        public DateTime? lastEditDate { get; set; }
        public string lastEditUser { get; set; }
        public string lastEditColumn { get; set; }
        public string lastEditValue { get; set; }

    }
}
