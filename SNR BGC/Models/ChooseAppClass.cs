using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SNR_BGC.Models
{
    public class ChooseAppClass
    {
        public int? SelectedOptionId { get; set; }
        public List<SelectListItem>? Options { get; set; }
    }
}
