using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class CourierTypes
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

    }
}
