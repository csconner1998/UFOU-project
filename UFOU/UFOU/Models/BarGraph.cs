using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static UFOU.Models.ShapeUtility;

namespace UFOU.Models
{
    public class BarGraph
    {
        public int ID { get; set; }

        public Shape Shape { get; set; }

        public int Quantity { get; set; }
        
        public string Location { get; set; }
    }
}
