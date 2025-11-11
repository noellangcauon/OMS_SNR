using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class UserModelDTO
    {
        public int userId { get; set; }
        public string accessType { get; set; }
        [Required]
        public string fullname { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string employeeId { get; set; }
        public string password { get; set; }
        [Required]
        public string role { get; set; }
        public bool oms { get; set; }
        public bool runnerapp { get; set; }
        public bool pickerapp { get; set; }
        public bool boxerapp { get; set; }
    }
}
