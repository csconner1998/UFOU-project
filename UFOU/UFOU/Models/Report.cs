using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static UFOU.Models.ShapeUtility;

namespace UFOU.Models
{
    /// <summary>
    /// Represents individual reports submitted to NUFORC
    /// </summary>
    public class Report
    {
        [Display(Name = "Report ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty]
        public int ReportId { get; set; }

        [JsonProperty]
        public string Location { get; set; }

        [JsonProperty]
        public Shape Shape { get; set; }

        [JsonProperty]
        public string Duration { get; set; }
        
        [MaxLength(5000, ErrorMessage="Limit description to 5000 characters")]
        [JsonProperty]
        public string Description { get; set; }
         
        [Display(Name = "Date Occurred")]
        [DataType(DataType.Date)]
        [JsonProperty]
        public DateTime DateOccurred { get; set; }

        [Display(Name = "Date Submitted")]
        [DataType(DataType.Date)]
        [JsonProperty]
        public DateTime DateSubmitted { get; set; }

        [Display(Name = "Date Posted")]
        [DataType(DataType.Date)]
        [JsonProperty]
        public DateTime? DatePosted { get; set; }

        [JsonProperty]
        public bool Approved { get; set; }
    }


    
    

    
}
