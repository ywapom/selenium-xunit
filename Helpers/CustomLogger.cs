using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Selenium_xunit_template.Helpers
{
    public class CustomLogger
    {
        private ITestOutputHelper iTestOutputHelper;
        readonly bool LogPassingTestsDisabled = (ConfigurationManager.AppSettings["log:logPassingTests"] ?? "").ToLower() == "disabled";
        readonly bool LogTestWarningsDisabled = (ConfigurationManager.AppSettings["log:logTestWarnings"] ?? "").ToLower() == "disabled";
        public static int TestID { get; set; } = 0;
        static int testCount = 0;

        public CustomLogger(ITestOutputHelper iTestOutputHelper)
        {
            this.iTestOutputHelper = iTestOutputHelper;
        }

        public void WriteLine(string description)
        {
            testCount++;
            WriteOnelinerResult(description);

            var method = new StackFrame(1).GetMethod();
            var classname = method.DeclaringType.Name;
            var methodname = method.Name;
            var list = new List<string>();
            list.Add($"Description: {description}");
            list.Add($"Class/Method: {method.DeclaringType.Name}.{method.Name}");

            this.iTestOutputHelper.WriteLine(string.Join("\t", list));
        }

        public void WriteLine(string description, string expectedResult, string actualResult, bool bPass)
        {
            testCount++;
            var result = bPass ? "Pass" : "FAIL";
            WriteOnelinerResult(description, result);

            if (LogPassingTestsDisabled && bPass == true)
                return;

            var method = new StackFrame(1).GetMethod();
            var classname = method.DeclaringType.Name;
            var methodname = method.Name;
            var list = new List<string>();

            list.Add($"Result: {result}");
            list.Add($"Description: {description}");
            list.Add($"Expected: {expectedResult}");
            list.Add($"Actual: {actualResult}");
            list.Add($"Class/Method: {method.DeclaringType.Name}.{method.Name}");

            this.iTestOutputHelper.WriteLine(string.Join("\t", list));
        }
        public void WriteLine(string description, bool bPass)
        {
            testCount++;
            var result = bPass ? "Pass" : "FAIL";
            WriteOnelinerResult(description, result);

            if (LogPassingTestsDisabled && bPass == true)
                return;

            var method = new StackFrame(1).GetMethod();
            var classname = method.DeclaringType.Name;
            var methodname = method.Name;
            var list = new List<string>();
            list.Add($"Result: {result}");
            list.Add($"Description: {description}");
            list.Add($"Class/Method: {method.DeclaringType.Name}.{method.Name}");

            this.iTestOutputHelper.WriteLine(string.Join("\t", list));

        }

        public void WriteLine(string description, string customResult)
        {
            testCount++;
            WriteOnelinerResult(description, customResult);

            if (LogTestWarningsDisabled && customResult == "WARNING")
                return;

            var method = new StackFrame(1).GetMethod();
            var classname = method.DeclaringType.Name;
            var methodname = method.Name;

            var list = new List<string>();
            list.Add($"Result: {customResult}");
            list.Add($"Description: {description}");
            list.Add($"Class/Method: {method.DeclaringType.Name}.{method.Name}");

            this.iTestOutputHelper.WriteLine(string.Join("\t", list));
        }

        public void WriteCount()
        {
            WriteLine($"Test Count: {testCount.ToString()}");
        }

        public void WriteOnelinerResult(string description, string result = "")
        {
            string[] path = { ConfigurationManager.AppSettings["TestRunFolder"], "results.txt" };
            string fullPath = Path.Combine(path);
            string count = testCount.ToString();

            if (result == "")
                count = "";
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath, true))
                {
                    file.WriteLine($"{count}\t{result}\t{description}");
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
                Debug.WriteLine("error writing results.txt");
            }
        }
        public void WriteSpellcheckResult(string result)
        {
            string[] path = { ConfigurationManager.AppSettings["TestRunFolder"], "spelling.txt" };
            string fullPath = Path.Combine(path);

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath, true))
                {
                    file.WriteLine(result);
                    file.Close();
                    file.Dispose();
                }
            }
            catch
            {
                Debug.WriteLine("error writing spelling.txt");
            }
        }
    }
}
