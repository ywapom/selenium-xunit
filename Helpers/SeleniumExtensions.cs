using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selenium_xunit_template.Helpers
{
    public static class SeleniumExtensions
    {
        public static Dictionary<string, object> WebTimings(this IWebDriver driver)
        {
            var webTiming = (Dictionary<string, object>)((IJavaScriptExecutor)driver)
                .ExecuteScript(@"var performance = window.performance || window.webkitPerformance || window.mozPerformance || window.msPerformance || {};
                                 var timings = performance.timing || {};
                                 return timings;");
            //The dictionary returned will contain something like the following.
            //                connectEnd : 1532707796133	
            //              connectStart : 1532707796133	
            //               domComplete : 1532707796291	
            //  domContentLoadedEventEnd : 1532707796290	
            //domContentLoadedEventStart : 1532707796272	
            //            domInteractive : 1532707796272	
            //                domLoading : 1532707796179	
            //           domainLookupEnd : 1532707796133	
            //         domainLookupStart : 1532707796133	
            //                fetchStart : 1532707796133	
            //              loadEventEnd : 1532707796293	
            //            loadEventStart : 1532707796291	
            //           navigationStart : 1532707796133	
            //               redirectEnd : 0	
            //             redirectStart : 0	
            //              requestStart : 1532707796134	
            //               responseEnd : 1532707796271	
            //             responseStart : 1532707796175	
            //     secureConnectionStart : 0	
            //toJSON : System.Collections.Generic.Dictionary`2[System.String,System.Object]	
            //            unloadEventEnd : 1532707796177	
            //          unloadEventStart : 1532707796177	

            return webTiming;
        }
        public static decimal GetPageLoadTime(this IWebDriver driver)
        {
            var dict = driver.WebTimings();
            var start = dict["navigationStart"];
            var end = dict["domComplete"];
            var pageLoadTime = Convert.ToDecimal(end) - Convert.ToDecimal(start);
            return pageLoadTime;
        }
        public static bool jQueryLoaded(this RemoteWebDriver driver)
        {
            bool result = false;
            try
            {
                result = (bool)driver.ExecuteScript("return typeof jQuery == 'function'");
            }
            catch (WebDriverException)
            {
            }

            return result;
        }
        public static void jqueryRemove(this IWebDriver driver, string tag)
        {
            try
            {
                IWebElement element = (driver as RemoteWebDriver).ExecuteScript($"return jQuery(\"{tag}\").remove()") as IWebElement;
            }
            catch
            {
                //
            }
        }

        public static void LoadjQuery(this RemoteWebDriver driver, string version = "any", TimeSpan? timeout = null)
        {
            //Get the url to load jQuery from
            string jQueryURL = "";
            if (version == "" || version.ToLower() == "latest")
                jQueryURL = "http://code.jquery.com/jquery-latest.min.js";
            else
                jQueryURL = "https://ajax.googleapis.com/ajax/libs/jquery/" + version + "/jquery.min.js";

            //Script to load jQuery from external site
            string versionEnforceScript = version.ToLower() != "any" ? string.Format("if (typeof jQuery == 'function' && jQuery.fn.jquery != '{0}') jQuery.noConflict(true);", version)
                                          : string.Empty;
            string loadingScript =
                @"if (typeof jQuery != 'function')
                  {
                      var headID = document.getElementsByTagName('head')[0];
                      var newScript = document.createElement('script');
                      newScript.type = 'text/javascript';
                      newScript.src = '" + jQueryURL + @"';
                      headID.appendChild(newScript);
                  }
                  return (typeof jQuery == 'function');";

            bool loaded = (bool)driver.ExecuteScript(versionEnforceScript + loadingScript);

            if (!loaded)
            {
                //Wait for the script to load
                //Verify library loaded
                if (!timeout.HasValue)
                    timeout = new TimeSpan(0, 0, 30);

                int timePassed = 0;
                while (!driver.jQueryLoaded())
                {
                    Thread.Sleep(500);
                    timePassed += 500;

                    if (timePassed > timeout.Value.TotalMilliseconds)
                        throw new Exception("Could not load jQuery");
                }
            }

            string v = driver.ExecuteScript("return jQuery.fn.jquery").ToString();
        }

        /// <summary>
        /// Overloads the FindElement function to include support for the jQuery selector class
        /// </summary>
        public static IWebElement FindElement(this RemoteWebDriver driver, string jquerySelector)
        {
            //First make sure we can use jQuery functions
            //driver.LoadjQuery();

            //Execute the jQuery selector as a script
            IWebElement element = driver.ExecuteScript($"return jQuery('{jquerySelector}').get(0)") as IWebElement;

            if (element != null)
                return element;
            else
                throw new NoSuchElementException("No element found with jQuery command: jQuery" + jquerySelector);
        }

        /// <summary>
        /// Overloads the FindElement function to include support for the jQuery selector class
        /// </summary>
        public static IWebElement FindElement(this RemoteWebDriver driver, By.jQueryBy by)
        {
            //First make sure we can use jQuery functions
            driver.LoadjQuery();

            //Execute the jQuery selector as a script
            IWebElement element = driver.ExecuteScript("return jQuery" + by.Selector + ".get(0)") as IWebElement;

            if (element != null)
                return element;
            else
                throw new NoSuchElementException("No element found with jQuery command: jQuery" + by.Selector);
        }

        /// <summary>
        /// Overloads the FindElements function to include support for the jQuery selector class
        /// </summary>
        public static ReadOnlyCollection<IWebElement> FindElements(this RemoteWebDriver driver, By.jQueryBy by)
        {
            //First make sure we can use jQuery functions
            driver.LoadjQuery();

            //Execute the jQuery selector as a script
            ReadOnlyCollection<IWebElement> collection = driver.ExecuteScript("return jQuery" + by.Selector + ".get()") as ReadOnlyCollection<IWebElement>;

            //Unlike FindElement, FindElements does not throw an exception if no elements are found
            //and instead returns an empty list
            if (collection == null)
                collection = new ReadOnlyCollection<IWebElement>(new List<IWebElement>()); //empty list

            return collection;
        }
    }
}
