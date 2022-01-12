using Selenium_xunit_template.Attributes;
using Selenium_xunit_template.Tests;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Selenium_xunit_template
{
    [TestCaseOrderer("Selenium_xunit_template.PriorityOrderer", "Selenium_xunit_template")]
    public class TestYwapom : TestBase
    {
        public TestYwapom(ITestOutputHelper output) : base(output) { }

        [Fact, TestPriority(0)]
        private void OneTimeSetup()
        {
            TestBase.CreateTestRunDirectory();
        }
        [Fact, TestPriority(9999999)]
        public void EndOfTests()
        {
            logger.WriteCount();
        }

        [Fact]
        public void Test1()
        {
            bool bSuccess = false;
            bSuccess = InitializeTest(MethodBase.GetCurrentMethod().Name);
            if (!bSuccess)
            {
                Assert.True(bSuccess);
                return;
            }
        }
    }
}