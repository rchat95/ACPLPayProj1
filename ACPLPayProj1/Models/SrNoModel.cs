using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ACPLPayProj1.Models
{
    public class SrNoModel
    {
        [Required]
        public string SrNo { get; set; }
    }
}