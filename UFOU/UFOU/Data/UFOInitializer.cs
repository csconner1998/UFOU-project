using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UFOU.Models;

namespace UFOU.Data
{
    public class UFOInitializer
    {
        public static void Initialize(UFOContext context)
        {
            context.Database.Migrate();

            if (!context.Reports.Any())
            {
                // read and deserialize objects from the seed file
                using (var reader = new StreamReader(new FileStream("./Data/Seeds/report_seed.json", FileMode.Open)))
                {
                    var reports = JsonConvert.DeserializeObject<List<Report>>(reader.ReadToEnd());
                    foreach (Report r in reports)
                    {
                        // add to the report database
                        context.Add(r);

                        // add to location database
                        Location l = context.Locations.Where(lo => lo.Name.Equals(r.Location)).FirstOrDefault();

                        if (l == null)
                        {
                            l = new Location
                            {
                                Name = r.Location,
                                MostCommonShape = r.Shape,
                                Sightings = 1
                            };
                            BarGraph b = new BarGraph
                            {
                                Shape = r.Shape,
                                Location = r.Location,
                                Quantity = 1
                            };

                            context.Locations.Add(l);
                            context.BarGraphs.Add(b);
                        }
                        else // location exists so update bargraph and most common shape
                        {
                            // update location sightings
                            l.Sightings++;

                            BarGraph bargraph = context.BarGraphs.Where(b => b.Location.Equals(l.Name) && b.Shape.Equals(r.Shape)).FirstOrDefault();

                            if (bargraph == null)
                            {
                                BarGraph b = new BarGraph
                                {
                                    Shape = r.Shape,
                                    Location = r.Location,
                                    Quantity = 1
                                };

                                l.MostCommonShape = r.Shape;

                                context.BarGraphs.Add(b);
                            }
                            else
                            {
                                // update the correct bargraphs quantity
                                bargraph.Quantity++;

                                // update the most common shape in location
                                var bargraphs = context.BarGraphs.ToList();

                                int max = 0;
                                foreach (BarGraph b in bargraphs)
                                {
                                    if (max < b.Quantity)
                                    {
                                        l.MostCommonShape = b.Shape;
                                    }
                                }
                            }

                        }
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}