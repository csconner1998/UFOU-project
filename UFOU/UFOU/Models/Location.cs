using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static UFOU.Models.ShapeUtility;

namespace UFOU.Models
{
    public class Location
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public Shape MostCommonShape { get; set; }

        public int Sightings { get; set; }
    }
}
