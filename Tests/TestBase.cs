using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Selenium_xunit_template.Helpers;
using Xunit.Abstractions;
using System.Diagnostics;
using Xunit;
using System.Net;
using Selenium_xunit_template.Helpers;
using System.IO;

namespace Selenium_xunit_template.Tests
{
    public class TestBase : IDisposable
    {
        public IWebDriver driver;
        public ChromeDriverService service = ChromeDriverService.CreateDefaultService();

        protected readonly ITestOutputHelper _testOutput;
        protected readonly CustomLogger logger;


        readonly string username = ConfigurationManager.AppSettings["login:username"];
        readonly string password = ConfigurationManager.AppSettings["login:password"];
        readonly bool DropDownOptionsTestDisabled = (ConfigurationManager.AppSettings["test:drop-down-options"] ?? "").ToLower() == "disabled";
        readonly bool DirectNavTestDisabled = (ConfigurationManager.AppSettings["test:direct-nav"] ?? "").ToLower() == "disabled";
        readonly bool LeaveBrowserOpen = (ConfigurationManager.AppSettings["browser:leave-open"] ?? "").ToLower() == "true";
        readonly bool KillDllhost = (ConfigurationManager.AppSettings["dllhost:kill"] ?? "").ToLower() == "true";

        public TestBase(ITestOutputHelper output)
        {
            logger = new CustomLogger(output);
            this._testOutput = output;

            if (ConfigurationManager.AppSettings["RanOneTimeSetup"] == "false")
            {
                SetOneTimeSetup(true);
                CreateTestRunDirectory();
            }
            try
            {
                //DB.Initialize(
                //    ConfigurationManager.ConnectionStrings["ApplicationServices"].ProviderName == "System.Data.SqlClient" ,
                //    ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString
                //);

                var options = new ChromeOptions();
                // Set all options from the app.config
                foreach (string key in ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("chrome:setting")))
                    options.AddArgument(ConfigurationManager.AppSettings[key]);

                driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options);
                Debug.WriteLine("Process ID for driver: " + service.ProcessId);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public bool InitializeTest(string testdescription)
        {
            logger.WriteLine(testdescription);
            var loadTime = driver.GetPageLoadTime();
            logger.WriteLine($"Page Load Time: {loadTime}ms ");


            string status = "";
            bool result = TestStatus(driver.Url, ref status);
            logger.WriteLine(status);

            if (driver.Title.Contains("404"))
            {
                logger.WriteLine($"Error: {driver.Title} URL: {driver.Url}", false);
                Assert.True(false);
                return false;
            }

            return true;
        }
        private bool TestStatus(string url, ref string status)
        {
            bool result = false;
            try
            {
                // Creates an HttpWebRequest for the specified URL. 
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                // Releases the resources of the response.
                myHttpWebResponse.Close();

                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    result = true;

                status = $"{myHttpWebResponse.StatusCode} | {myHttpWebResponse.StatusDescription}";
            }
            catch (WebException e)
            {
                status = ($"\r\nWebException Raised. The following error occured : {e.Status}");
            }
            catch (Exception e)
            {
                status = ($"The following Exception was raised : {e.Message}");
            }
            return result;
        }
        public static void SetOneTimeSetup(bool ran)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["RanOneTimeSetup"].Value = ran.ToString();
            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
            Debug.WriteLine($"Set configuration: {ConfigurationManager.AppSettings["RanOneTimeSetup"]}");
        }
        public static void CreateTestRunDirectory()
        {
            try
            {
                var date = DateTime.Now.ToString("MMddyyyy_HHmmss");

                string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string folderName = Path.Combine(projectPath, date);
                System.IO.Directory.CreateDirectory(folderName);

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["TestRunFolder"].Value = folderName;
                config.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("appSettings");
                Debug.WriteLine($"Created test run directory: {ConfigurationManager.AppSettings["TestRunFolder"]}");

                // create file for oneliner results
                System.IO.File.WriteAllText($"{folderName}\\results.txt", $"TEST RUN {date}\n");
                System.IO.File.WriteAllText($"{folderName}\\spelling.txt", $"TEST RUN {date}\n");
            }
            catch
            {
                Debug.WriteLine("could not create results.txt location");
            }
        }
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (!LeaveBrowserOpen)
                        {
                            var driverProcessIds = new List<int> { service.ProcessId };

                            driver.Close();
                            driver.Quit();
                            driver.Dispose();

                            foreach (var id in driverProcessIds)
                            {
                                System.Diagnostics.Process.GetProcessById(id).Kill();
                            }

                            if (ConfigurationManager.AppSettings["RanOneTimeSetup"] == "true")
                            {
                                SetOneTimeSetup(false);
                                CreateTestRunDirectory();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                }
                disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
