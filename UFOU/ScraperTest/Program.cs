using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using UFOU.Models;
using Newtonsoft.Json;
using System.Net;
using HtmlAgilityPack;
using System.Threading;
using System.Text.RegularExpressions;

namespace ScraperTest
{
    class Program
    {
        static string baseUrl = "http://www.nuforc.org/";

        /// <summary>
        /// Maximum number of requests allowed to be sent before throttling begins
        /// </summary>
        static int maxInFlight = 500;

        /// <summary>
        /// Wait this many milliseconds whenever the maxInFlight ceiling is hit
        /// </summary>
        static int throttleTimeout = 1000;

        /// <summary>
        /// The total number of reports you want from the site
        /// Set to -1 for all of thems
        /// </summary>
        static int maxReportBatch = 30;

        static void Main(string[] args)
        {

            var reports = GetReports();
            foreach (Report r in reports)
            {
                Console.WriteLine(JsonConvert.SerializeObject(r));
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        /// <summary>
        /// Scrapes data from the NUFORC web portal, serializes it into 
        /// Multithreaded, takes a lot of CPU and memory to run, ensure parameters are properly set before starting
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Report> GetReports()
        {
            var newReports = new HashSet<Report>();

            var web = new HtmlWeb();
            var homeDoc = web.Load(baseUrl + "webreports.html");

            // from front page, go to "by state"
            var stateIndexLink = baseUrl + homeDoc.QuerySelector(Selectors.indexSelector).Attributes["href"].Value;
            var stateIndexDoc = web.Load(stateIndexLink);

            // number of pending requests to the NUFORC site, decrements once the web page is received
            int requestsInFlight = 0;

            object stopRequests = false;

            // get list of links to state pages
            var stateLinkNodes = stateIndexDoc.QuerySelectorAll(Selectors.tableLinkSelector);
            do
            {
                foreach (var s in stateLinkNodes)
                {
                    web = new HtmlWeb();

                    var stateLink = baseUrl + "webreports/" + s.Attributes["href"].Value;

                    // throttle requests when too many are in flight
                    while (requestsInFlight > maxInFlight)
                        Thread.Sleep(throttleTimeout);
                    requestsInFlight++;

                    // spin off threads for each state
                    web.LoadFromWebAsync(stateLink).ContinueWith((state_task) =>
                    {
                        try { state_task.Wait(); }
                        catch { return; }

                        requestsInFlight--;

                        if (!state_task.IsCompletedSuccessfully)
                            return;

                        var stateDoc = state_task.Result;

                        // get list of links to individual reports (same selector)
                        var reportLinkNodes = stateDoc.QuerySelectorAll(Selectors.tableLinkSelector);

                        foreach (var r in reportLinkNodes)
                        {

                            var webTemp = new HtmlWeb();
                            var reportLink = baseUrl + "webreports/" + r.Attributes["href"].Value;

                            // extract ID from url, use that as the ID
                            var reportId = int.Parse(reportLink.Substring(reportLink.LastIndexOf('/') + 2).Replace(".html", ""));

                            // throttle requests when to many are in flight
                            while (requestsInFlight >= maxInFlight)
                                Thread.Sleep(throttleTimeout);
                            requestsInFlight++;

                            // spin off *another* thread to load the report pages
                            webTemp.LoadFromWebAsync(reportLink).ContinueWith((report_task) =>
                            {
                                try
                                { report_task.Wait(); }
                                catch { return; }
                                finally { requestsInFlight--; }

                                // bail out if the request failed
                                if (!report_task.IsCompletedSuccessfully)
                                    return;

                                var reportDoc = report_task.Result;

                                // TODO: get hashset of all IDs from DB into memory before running this method, check for duplicates here


                                // get the details and description of the report
                                var reportTable = reportDoc.QuerySelectorAll("tr td");

                                // report wasn't found, got a different page
                                if (reportTable.Count < 2)
                                    return;

                                var details = reportTable[0].InnerHtml;
                                var description = reportTable[1];

                                // bail out if we've hit the max batch in any thread
                                if (stopRequests.Equals(true) || (maxReportBatch != -1 && newReports.Count >= maxReportBatch))
                                {
                                    stopRequests = true;
                                    return;
                                }

                                Console.WriteLine(newReports.Count);


                                // for this row: the left most portion is the date, the rightmost is the time, the middle is usually useless
                                var reportDateTimes = Regex.Match(details, RegularExpressions.dateReported).Value.Split(' ');

                                newReports.Add(new Report()
                                {
                                    ReportId = reportId,
                                    DateOccurred = DateTime.Parse(Regex.Match(details, RegularExpressions.dateOccured).Value),
                                    DateSubmitted = DateTime.Parse(reportDateTimes[0] + reportDateTimes[reportDateTimes.Length-1]),
                                    DatePosted = DateTime.Parse(Regex.Match(details, RegularExpressions.datePosted).Value),
                                    Location = Regex.Match(details, RegularExpressions.location).Value,
                                    Shape = ShapeUtility.ShapeAliases(Regex.Match(details, RegularExpressions.shape).Value),
                                    Duration = Regex.Match(details, RegularExpressions.duration).Value,
                                    Description = description.InnerText
                                });

                            });

                            if (stopRequests.Equals(true))
                                break;

                        }
                    });
                    if (stopRequests.Equals(true))
                        break;
                }
            } while (requestsInFlight > 0 && stopRequests.Equals(false));

            return newReports;
        }

        /// <summary>
        /// CSS selectors for the web scraper to properly navigate
        /// </summary>
        public static class Selectors
        {
            public static string tableLinkSelector = "tr td a";
            public static string indexSelector = "p a[href*=\"ndxloc\"]";
            public static string reportDetailsSelector = "tr:nth-child(1) td font";
            public static string reportDescriptionSelector = "tr:nth-child(2) td font";
        }


        /// <summary>
        /// Regular expressions for parsing of irregularly formatted data
        /// </summary>
        public static class RegularExpressions
        {
            // match to innerHtml, not innerText
            public static string dateOccured = @"(?<=Occurred :) ?(\d{1,2}/){2}(\d{4}|\d{2}) \d{1,2}:\d\d";
            public static string dateReported = @"(?<=Reported:) ?(\d{1,2}/){2}(\d{4}|\d{2})(.+M) \d\d:\d\d"; 
            public static string datePosted = @"(?<=Posted:) ?(\d{1,2}/){2}(\d{4}|\d{2})";
            public static string location = @"(?<=Location:) ?([^<>\n\r$])+";
            public static string shape = @"(?<=Shape:) ?([^<>\n\r$])+";
            public static string duration = @"(?<=Duration:) ?([^<>\n\r$])+";

        }

    }
}
