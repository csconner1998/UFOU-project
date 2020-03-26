using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFOU.Models;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Threading;
using System.Text.RegularExpressions;

namespace UFOU.Data
{
    /// <summary>
    /// This class facilitates the scraping and parsing of data from NUFORC's website
    /// 
    /// NUFORC houses >87000 individual reports on their website
    /// This cannot be searched through sequentially, so heres the general strategy:
    /// Reports on NUFORC are structured like this:
    ///      index (table of links to states)
    ///          --> states (table of links to reports)
    ///                  --> reports (table of report data, the goal)
    /// 
    /// These are all static pages, which helps in that they're relatively simple, but also can take forever to load
    /// Some tables on this site have an excess of 8000 rows trying to load at once 
    ///     (all of which is being distributed by a less-than-modern server)
    ///     
    /// The library being used to parse the HTML (htmlagilitypack) is way faster than Selenium in this case, but still
    ///     wants to wait until the whole page is received and the DOM is loaded to provide you access
    /// So scraping sequentially can also get you stuck receiving a single page for minutes, 
    /// 
    /// At each level, new threads are forked on each row with a new http connection(think opening each page in a new tab)
    /// Every time a report is processed, the tab is closed
    /// The "requestsInFlight" is the hard upper limit on how many tabs this loop can have open at once
    /// If a thread bumps up against this cap, it waits for the duration set by the "throttleTimeout" before trying again
    /// "batchSize" sets how many reports to return, with -1 acting as the stand-in for "scrape all of them"
    /// 
    /// The number of threads grows exponentially, yes, but this is actually helpful to us
    /// With the "requestsInFlight" set between 300 and 500 this loop keeps a pretty good pace, getting through a couple hundred
    ///     reports per second.
    /// Any faster and the server begins denying requests as it (reasonably) assumes we're conducting a DDOS attack and begins
    ///     rejecting requests.
    /// 
    /// Can this problem be solved with a complex matrix of semaphores and mutexes where the requests are staggered perfectly? Maybe. 
    /// However, this is almost certainly not going to be the bottleneck for a web server, as it won't have to be constantly combing 
    ///     the site for updates.
    /// 
    /// </summary>
    class UFOScraper
    {
        /// <summary>
        /// Check whether a scraper is actively working without reflection
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Every thread is always relatively close to a halt check
        /// Setting to true allows each thread to gracefully exit at their
        ///     earliest convenience
        /// </summary>
        public bool Halt { get; set; }

        /// <summary>
        /// Url of the NUFORC from which all other urls are derived
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Maximum number of requests allowed to be sent before throttling begins
        /// </summary>
        static int MaxInFlight { get; set; }

        /// <summary>
        /// Wait this many milliseconds whenever the maxInFlight ceiling is hit
        /// </summary>
        static int ThrottleTimeout { get; set; }

        /// <summary>
        /// The total number of reports you want from the site
        /// Set to -1 for all of thems
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Private singleton member
        /// </summary>
        private static UFOScraper _ufoScraper = null;

        /// <summary>
        /// Instantiates a web scraper with default parameters
        /// </summary>
        private UFOScraper()
        {
            BaseUrl = "http://www.nuforc.org/";
            MaxInFlight = 500;
            ThrottleTimeout = 1000;
            BatchSize = 100;
            Halt = true;
            IsRunning = false;
        }

        /// <summary>
        /// Returns reference to singleton instance of scraper
        /// No need to ever have more than one
        /// </summary>
        /// <returns></returns>
        public static UFOScraper GetSingletonScraper()
        {
            if (_ufoScraper == null)
                _ufoScraper = new UFOScraper();

            return _ufoScraper;
        }
        

        /// <summary>
        /// Scrapes data from the NUFORC web portal, deserializes it into Report objects
        /// Multithreaded, takes a lot of CPU and memory to run, and is more or less a DDOS if done wrong 
        /// Ensure parameters are properly set before starting
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Report> GetReports(ISet<int> currentIds)
        {
            IsRunning = true;
            Halt = false;
            var newReports = new HashSet<Report>();

            var web = new HtmlWeb();
            var homeDoc = web.Load(BaseUrl + "webreports.html");

            // from front page, go to "by state"
            var stateIndexLink = BaseUrl + homeDoc.QuerySelector(Selectors.indexSelector).Attributes["href"].Value;
            var stateIndexDoc = web.Load(stateIndexLink);

            // number of pending requests to the NUFORC site, decrements once the web page is received
            int requestsInFlight = 0;

            // get list of links to state pages
            var stateLinkNodes = stateIndexDoc.QuerySelectorAll(Selectors.tableLinkSelector);


            // Main loop, forks threads to independently open and close pages as they load
            do
            {
                foreach (var s in stateLinkNodes)
                {
                    web = new HtmlWeb();

                    var stateLink = BaseUrl + "webreports/" + s.Attributes["href"].Value;

                    // throttle requests when too many are in flight
                    while (requestsInFlight > MaxInFlight)
                        Thread.Sleep(ThrottleTimeout);
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
                            // add another short lived web client!
                            var webTemp = new HtmlWeb();
                            var reportLink = BaseUrl + "webreports/" + r.Attributes["href"].Value;

                            // extract ID from url, use that as the ID
                            bool parseResult = int.TryParse(reportLink.Substring(reportLink.LastIndexOf('/') + 2).Replace(".html", ""), out int reportId);

                            // check against duplicates and failed lookups
                            if (!parseResult || currentIds.Contains(reportId))
                                continue;

                            // throttle requests when to many are in flight
                            while (requestsInFlight >= MaxInFlight)
                                Thread.Sleep(ThrottleTimeout);
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

                                // get the details and description of the report
                                var reportTable = reportDoc.QuerySelectorAll("tr td");

                                // report wasn't found, got a different page
                                if (reportTable.Count < 2)
                                    return;

                                var details = reportTable[0].InnerHtml;
                                var description = reportTable[1];

                                // bail out if we've hit the max batch in any thread
                                if (Halt.Equals(true) || (BatchSize != -1 && newReports.Count > BatchSize))
                                {
                                    Halt = true;
                                    return;
                                }

                                // for this row: the left most portion is the date, the rightmost is the time, the middle is usually useless
                                var reportDateTimes = Regex.Match(details, RegularExpressions.dateReported).Value.Split(' ');

                                // there is 
                                newReports.Add(new Report()
                                {
                                    ReportId = reportId,
                                    DateOccurred = DateTime.Parse(Regex.Match(details, RegularExpressions.dateOccured).Value),
                                    DateSubmitted = DateTime.Parse(reportDateTimes[0] + reportDateTimes[reportDateTimes.Length - 1]),
                                    DatePosted = DateTime.Parse(Regex.Match(details, RegularExpressions.datePosted).Value),
                                    Location = Regex.Match(details, RegularExpressions.location).Value,
                                    Shape = ShapeUtility.ShapeAliases(Regex.Match(details, RegularExpressions.shape).Value),
                                    Duration = Regex.Match(details, RegularExpressions.duration).Value,
                                    Description = description.InnerText
                                });

                                Console.WriteLine(newReports.Count);

                            });

                            if (Halt.Equals(true))
                                break;

                        }
                    });
                    if (Halt.Equals(true))
                        break;
                }
            } while (requestsInFlight > 0 || Halt.Equals(false));

            // reset initial conditions
            Halt = false;
            IsRunning = false;

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
        /// NUFORC enforces very few structural requirements on reports submitted to the site, 
        /// so multiple cases have to be covered for a single line of data as it could appear in
        /// many different ways.
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
