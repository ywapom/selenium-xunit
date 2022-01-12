using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selenium_xunit_template.Helpers
{
    public enum MessageSeverity
    {
        OK = 0,
        Warning = 1,
        Error = 2,
        Catastrophic = 3
    }
    public static class ExtensionMethods
    {
        #region MessageSeverity

        public static bool IsOk(this MessageSeverity ms)
        {
            //return (ms == MessageSeverity.OK || ms == MessageSeverity.Warning);
            return ms == MessageSeverity.OK;
        }

        #endregion

        #region IWebElement

        public static void SetDropDownSelection(this IWebElement el, string value)
        {
            try
            {
                new SelectElement(el).SelectByValue(value);
            }
            catch
            {
                // TODO: log warning that field metadata says it should be a drop down but it is not a select on the screen
                el.SendKeys(value);
            }
        }

        public static void SetDropDownByIndex(this IWebElement el, int index = 0)
        {
            try
            {
                new SelectElement(el).SelectByIndex(index);
            }
            catch
            {
            }
        }

        public static IList<IWebElement> GetDropDownOptions(this IWebElement el)
        {
            IList<IWebElement> allOptions = el.FindElements(By.TagName("option"));
            return allOptions;
        }

        public static void FillToMax(this IWebElement el)
        {
            el.SendKeys("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        }

        public static string GetValue(this IWebElement el)
        {
            return el.GetAttribute("value");
        }

        public static string GetLabelHtml(this IWebElement el, IWebDriver driver)
        {
            var row = GetTableRowFromCellItem(driver, el);
            var cells = GetColInTableRow(driver, row);

            return cells[0].GetAttribute("innerHTML");
        }

        #endregion

        #region IWebDriver

        public static MessageSeverity GetMessageSeverity(this IWebDriver driver) => (MessageSeverity)int.Parse(driver.FindElement("#messageBox").GetAttribute("data-severity"));
        public static string GetMessageText(this IWebDriver driver) => driver.FindElement("#messageBox > span").GetAttribute("innerHTML").Replace("&nbsp;", "").Trim();
        public static int GetMessageNumber(this IWebDriver driver) => int.Parse(driver.FindElement("#messageBox").GetAttribute("data-message-number").Trim());
        public static int GetMessageCount(this IWebDriver driver) => driver.FindElements(By.Id("messageBox")).Count();
        private static List<int> SuccessfulValidateMessageNumbers = new List<int> { 38, 3913, 761, 850, 4172 };
        private static List<int> SuccessfulCommitMessageNumbers = new List<int> { 44, 50, 51, 80, 761, 819, 850, 4158 };

        public static IWebElement SetAttribute(this IWebElement element, string name, string value)
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var jsExecutor = (IJavaScriptExecutor)driver;
            jsExecutor.ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);", element, name, value);

            return element;
        }

        public static bool CheckMessageEnterData(this IWebDriver driver)
        {
            var msg = driver.GetMessageText();
            if (msg.Contains("Enter Data"))
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        public static bool CheckValidateSuccessful(this IWebDriver driver)
        {
            if (!driver.GetMessageSeverity().IsOk())
                return SuccessfulValidateMessageNumbers.Contains(driver.GetMessageNumber()); ;

            return true;
        }
        public static bool CheckValidateFailed(this IWebDriver driver)
        {
            if (driver.GetMessageSeverity().IsOk())
                return false;

            return !SuccessfulValidateMessageNumbers.Contains(driver.GetMessageNumber());

        }

        public static bool CheckPageIsOkay(this IWebDriver driver)
        {
            if (driver.Title.Contains("404"))
            {
                return false;
            }

            if (driver.Url.ToLower().Contains("error.asp") || driver.Url.ToLower().Contains("fatal.asp"))
            {
                return false;
            }

            if (driver.Url.ToLower().Contains("logon.asp"))
            {
                return false;
            }
            else
                return true;
        }

        public static void TestDirectNavSucceeded(this IWebDriver driver, string commandCode, ref bool bSuccess)
        {
            var title = driver.Title;

            if (!title.Contains(commandCode))
            {
                Debug.WriteLine("DirectNav Test", $"Expected {commandCode} in Title", driver.Title, false);
                bSuccess = false;
            }
            else
            {
                Debug.WriteLine("DirectNav Test", $"Expected {commandCode} is in Title", driver.Title, true);
            }
        }

        public static void WaitForElementPresentAndEnabled(this IWebDriver driver, OpenQA.Selenium.By locator, int secondsToWait = 30)
        {
            new WebDriverWait(driver, new TimeSpan(0, 0, secondsToWait))
               .Until(d => d.FindElement(locator).Enabled
                   && d.FindElement(locator).Displayed
                   && d.FindElement(locator).GetAttribute("aria-disabled") == null
               );
        }
        public static Dictionary<string, object> GetAttributesOfElement(this IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var attributes = js.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) " +
                "{ items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", element);
            Dictionary<string, object> a = (Dictionary<string, object>)attributes;
            return a;
        }
        public static IWebElement FindHrefByInnerHTML(this IWebDriver driver, string name)
        {
            IWebElement href = null;
            foreach (var item in driver.FindElements(By.XPath("//a[@href]")))
            {
                if (item.GetAttribute("innerHTML") == name)
                {
                    href = item;
                    break;
                }
            }
            return href;
        }
        public static By SelectorByAttributeValue(this IWebDriver driver, string p_strAttributeName, string p_strAttributeValue)
        {
            return (By)By.XPath(string.Format("//*[@{0} = '{1}']",
                                           p_strAttributeName,
                                           p_strAttributeValue));
        }

        public static IWebElement GetParentElement(this IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var parent = js.ExecuteScript("return arguments[0].parentNode;", element);
            IWebElement p = (IWebElement)parent;
            return p;
        }

        public static IWebElement GetChildElement(this IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var child = js.ExecuteScript("return arguments[0].childNode;", element);
            IWebElement c = (IWebElement)child;
            return c;
        }

        public static IWebElement GetOnclickElement(this IWebDriver driver, string fieldName)
        {
            var onclicks = driver.FindElements(By.XPath("//*[@onclick]"));
            IWebElement element = null;
            foreach (var item in onclicks)
            {
                var html = item.GetAttribute("outerHTML");
                if (html.Contains(fieldName))
                {
                    element = item;
                }
            }
            return element;
        }

        public static IWebElement GetElementById(this IWebDriver driver, string id)
        {
            try
            {
                return driver.FindElement(By.Id(id));
            }
            catch
            {
                return null;
            }
        }

        public static IWebElement GetElementByName(this IWebDriver driver, string name)
        {
            try
            {
                return driver.FindElement(By.Name(name));
            }
            catch
            {
                return null;
            }
        }

        public static IWebElement ByName(this IWebDriver driver, string name)
        {
            return driver.FindElement(By.Name(name));
        }

        public static IWebElement ById(this IWebDriver driver, string id)
        {
            return driver.FindElement(By.Id(id));
        }

        public static IWebElement ByLinkText(this IWebDriver driver, string linktext)
        {
            return driver.FindElement(By.LinkText(linktext));
        }

        public static IWebElement ByPartialLinkText(this IWebDriver driver, string linktext)
        {
            return driver.FindElement(By.PartialLinkText(linktext));
        }

        public static IWebElement ByTagt(this IWebDriver driver, string tag)
        {
            return driver.FindElement(By.TagName(tag));
        }

        public static IWebElement ByClass(this IWebDriver driver, string className)
        {
            return driver.FindElement(By.ClassName(className));
        }

        public static IWebElement ByCss(this IWebDriver driver, string css)
        {
            return driver.FindElement(By.CssSelector(css));
        }

        public static IList<IWebElement> SByName(this IWebDriver driver, string name)
        {
            return driver.FindElements(By.Name(name));
        }

        public static IList<IWebElement> SById(this IWebDriver driver, string id)
        {
            return driver.FindElements(By.Id(id));
        }

        public static IList<IWebElement> SByLinkText(this IWebDriver driver, string linktext)
        {
            return driver.FindElements(By.LinkText(linktext));
        }

        public static IList<IWebElement> SByPartialLinkText(this IWebDriver driver, string linktext)
        {
            return driver.FindElements(By.PartialLinkText(linktext));
        }

        public static IList<IWebElement> SByTagt(this IWebDriver driver, string tag)
        {
            return driver.FindElements(By.TagName(tag));
        }

        public static IList<IWebElement> SByClass(this IWebDriver driver, string className)
        {
            return driver.FindElements(By.ClassName(className));
        }

        public static IList<IWebElement> SByCss(this IWebDriver driver, string css)
        {
            return driver.FindElements(By.CssSelector(css));
        }

        /// <summary>
        /// Overloads the FindElement function to include support for the jQuery selector class
        /// </summary>
        public static IWebElement FindElement(this IWebDriver driver, string jquerySelector)
        {
            //First make sure we can use jQuery functions
            //driver.LoadjQuery();

            //Execute the jQuery selector as a script
            IWebElement element = (driver as RemoteWebDriver).ExecuteScript($"return jQuery('{jquerySelector}').get(0)") as IWebElement;

            if (element != null)
                return element;
            else
                //throw new NoSuchElementException("No element found with jQuery command: jQuery" + jquerySelector);
                return null;
        }

        public static IWebElement GetTableRowFromCellItem(this IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            // first parent is td second parent is tr
            var tr = js.ExecuteScript("return arguments[0].parentNode.parentNode;", element);
            IWebElement a = (IWebElement)tr;
            return a;
        }

        public static IList<IWebElement> GetColInTableRow(this IWebDriver driver, IWebElement tr)
        {
            var tds = tr.FindElements(By.CssSelector("td"));
            return tds;
        }

        public static void ClickHref(this IWebDriver driver, string name)
        {
            IWebElement element = FindHrefByInnerHTML(driver, name);
            if (element != null)
            {
                element.Click();
            }
        }

        public static void DirectNavigate(this IWebDriver driver, string command)
        {
            IWebElement element = driver.GetElementById("CurrComm");
            if (element != null)
            {
                element.SendKeys(command);
                element.SendKeys(Keys.Return);
            }
            else
            {
                driver.UrlNavigate(command);
            }
        }

        public static void UrlNavigate(this IWebDriver driver, string command)
        {
            driver.Navigate().GoToUrl($"{ConfigurationManager.AppSettings["application:url"]}{command}.asp");
        }

        public static bool IsMessageEnterDataOrChangesAndExecute(this IWebDriver driver) => driver.GetMessageNumber() == 40;

        //public static void Login(this IWebDriver driver)
        //{
        //    Logon logon = new Logon(driver);
        //    logon.Login();
        //}

        //public static void LoginAndNavToCommand(this IWebDriver driver, string command)
        //{
        //    driver.Login();
        //    driver.DirectNavigate(command);
        //}

        public static void ExpandAllFilterGroups(this IWebDriver driver)
        {
            (driver as IJavaScriptExecutor).ExecuteScript("$('.group-title > span:contains(\"+\")').parent().click();");
        }

        public static void ClickGetButton(this IWebDriver driver)
        {
            IWebElement button = null;
            try
            {
                button = driver.FindElement(By.Id("GetButton"));
            }
            catch { }

            if (button == null)
            {
                button = driver.FindElement(By.Id("btn-view"));

                if (button == null)
                {
                    string template = "input[type=submit][value=XXXX],button[value=XXXX]";
                    //string selector = string.Join(",", ListOfGetButtonValueAttributes.Select(s => template.Replace("XXXX", s)));
                    //button = driver.FindElement(selector);
                }
            }

            if (button != null)
                driver.SafeClick(button);
        }


        public static void ClickMvcButtonById(this IWebDriver driver, string id)
        {
            var button = driver.FindElement($"#{id}");
            if (button != null)
                driver.SafeClick(button);
        }


        public static void ClickCommitButton(this IWebDriver driver)
        {
            // clicks the button with id=CommitButton
            var button = driver.FindElement("input[type=submit][value=Commit],button[value=Commit]");

            if (button == null)
            {
                try
                {
                    button = driver.FindElement(By.Id("CommitButton"));
                }
                catch
                {
                    //webdriver exception
                }
            }

            if (button == null)
            {
                try
                {
                    button = driver.FindElement(By.Id("btn-commit-1"));
                }
                catch
                {
                    //webdriver exception
                }
            }

            if (button == null)
                try
                {
                    button = driver.FindElement(By.Id("btn-commit-2"));
                }
                catch
                {
                    //webdriver exception
                }


            if (button != null)
                driver.SafeClick(button);
        }


        public static void ClickValidateButton(this IWebDriver driver)
        {
            var button = driver.FindElement("input[type=submit][value=Validate],button[value=Validate]");
            if (button != null)
                driver.SafeClick(button);

            if (button == null)
            {
                try
                {
                    button = driver.FindElement(By.Id("btn-validate"));

                    if (button != null)
                        driver.SafeClick(button);
                }
                catch
                { }

            }

        }

        public static void ClickClearButton(this IWebDriver driver)
        {
            var button = driver.ById("ClearButton");
            if (button != null)
                driver.SafeClick(button);

            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public static void SafeClick(this IWebDriver driver, IWebElement el)
        {
            (driver as RemoteWebDriver).ExecuteScript("arguments[0].click()", el);
        }

        public static void ClickOnClickButton(this IWebDriver driver, string name)
        {
            IReadOnlyCollection<IWebElement> buttons =
            driver.FindElements(By.XPath(".//button[@onclick]"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            foreach (IWebElement button in buttons)
            {
                if (button.Text == name)
                {
                    js.ExecuteScript("arguments[0].click();", button);
                    break;
                }

            }
        }

        public static void ClickTodayOnCalendar(this IWebDriver driver, string fieldName)
        {
            // Handles when parmName is in onclick attribute, or data-input-name attribute
            var elements = driver.FindElements(By.TagName("img"));
            foreach (var elem in elements)
            {
                string onclick = elem.GetAttribute("onclick");
                string dataInput = elem.GetAttribute("data-input-name");

                if (onclick != null && onclick.Contains("showCalendar") && onclick.Contains(fieldName))
                {
                    driver.SafeClick(elem);
                    var today = driver.FindElement(By.ClassName("pcDaySelected"));
                    today.FindElement(By.TagName("a")).Click();
                }
                if (dataInput != null && dataInput.Contains(fieldName))
                {
                    elem.Click();
                    var today = driver.FindElement(By.ClassName("pcDaySelected"));
                    today.FindElement(By.TagName("a")).Click();
                }
            }
        }

        public static IWebElement GetLabel(this IWebDriver driver, string parmName)
        {
            //IWebElement theLabel = null;
            //var labels = driver.FindElements(By.ClassName("label.field-title.popup"));
            //foreach(var label in labels)
            //{
            //    var labelHTML = label.GetAttribute("outerHTML");
            //    if(labelHTML.Contains(parmName))
            //    {
            //        theLabel = label;
            //    }
            //}
            //var theLabel = labels.FirstOrDefault(e => e.GetAttribute("outerHTML").Contains(parmName));
            var theLabel = driver.FindElement(By.CssSelector($"label[for={parmName}"));
            return theLabel;
        }

        public static void TakeScreenshot(this IWebDriver driver, string name)
        {
            try
            {
                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                name += Timestamp.ToString() + ".png";
                string[] path = { ConfigurationManager.AppSettings["TestRunFolder"], name };
                string fullPath = Path.Combine(path);
                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                ss.SaveAsFile(fullPath, ScreenshotImageFormat.Png);
            }
            catch
            {

            }
        }
        #endregion

        #region  IEnumerable<object[]>

        public static void AddItem(this List<object[]> collection, object o)
        {
            collection.Add(new object[] { o });
        }

        #endregion

    }
}
