
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string UwpAppId = @"57012MikeFrancis.TimeZonesConverter_08pddan6qgb4e!app";

        protected static WindowsDriver<WindowsElement> session;
        private TestContext _testContext;

        public TestContext TestContext
        {
            get { return _testContext; }
            set { _testContext = value; }
        }

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            if (session == null)
            {
                var appiumOptions = new AppiumOptions();
                appiumOptions.AddAdditionalCapability("app", UwpAppId);
                appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appiumOptions);

            }
        }

  

        [TestMethod]
        public void ClickBuyFreeButton()
        {
            int cnt = 0;
            do
            {
                var btnBuyFree = session.FindElementByAccessibilityId("BuyFree");
                btnBuyFree.Click();
                
                Thread.Sleep(12 * 1000);
                var btnOKBuy = session.FindElementByClassName("ApplicationFrameInputSinkWindow");
                btnOKBuy.SendKeys(Keys.Enter);
                
                Thread.Sleep(12 * 1000);
                var txtResult = session.FindElementByAccessibilityId("txtResult");
                var productId = "MyConsumable";
                Assert.AreEqual(txtResult.Text, $"\rPurchase of {productId} succeeded!");
                cnt++;
                TestContext.WriteLine($"Completed {cnt} tests!");
            } while (cnt < 10);


        }
    }
}
