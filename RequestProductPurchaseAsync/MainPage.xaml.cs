using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RequestProductPurchaseAsync
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var unfulfilled = await CurrentApp.GetUnfulfilledConsumablesAsync();
            if (unfulfilled.Count > 0)
            {
                foreach (var transaction in unfulfilled)
                {
                    var fulfillStatus = await CurrentApp.ReportConsumableFulfillmentAsync("MyConsumable", transaction.TransactionId);
                    if (fulfillStatus != FulfillmentResult.Succeeded)
                    {
                        Debug.WriteLine($"Fulfillment error: {fulfillStatus}");
                    }
                }
            }


            LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
            if (!licenseInformation.ProductLicenses["MyConsumable"].IsActive)
            {
                var res = await CurrentApp.RequestProductPurchaseAsync("MyConsumable");
                if (res.Status == ProductPurchaseStatus.Succeeded)
                {
                    var el = XElement.Parse(res.ReceiptXml);
                    var items = el.Elements();
                    foreach (var item in items)
                    {
                        if (item.Name.LocalName == "ProductReceipt")
                        {
                            var at = item.Attribute("Id").Value;

                            Debug.Assert(new Guid(at) == res.TransactionId);
                        }

                    }
                } else
                {
                    Debug.WriteLine($"Purchase faied: {res.Status}");
                }

            } else

            {
                Debug.WriteLine("Could not purchase since license is still active.");
            }

        }
    }
}
