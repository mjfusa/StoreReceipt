using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //DoPurchase("9N1HSVGN8D4V"); // This consumable is free
            DoPurchase("MyConsumable"); // This consumable is free

        }

        static StoreContext storeContext = StoreContext.GetDefault();
        private async void DoPurchase1(string productId)
        {
            var res = await storeContext.RequestPurchaseAsync(productId);
            if (res.Status == StorePurchaseStatus.Succeeded)
            {
            } else
            {
                // Display failure reason
            }
        }


        private async void DoPurchase(string productId)
        {
            txtStatus.Text = "";
            string[] productKinds = { "UnmanagedConsumable" };
            List<String> filterList = new List<string>(productKinds);
            var result = await storeContext.GetAssociatedStoreProductsAsync(filterList);
            StoreProduct sp = null;
            foreach (var p in result.Products.Values)
            {
                if (p.InAppOfferToken == productId)
                {
                    sp = p;
                    break;
                }
            }

            if (sp != null)
            {
                try
                {
                    var guid = Guid.NewGuid();
                    if (sp.IsInUserCollection)
                    {
                        var fulfillStatus = await storeContext.ReportConsumableFulfillmentAsync(sp.StoreId, 1, guid);
                        if (fulfillStatus.Status != StoreConsumableStatus.Succeeded)
                        {
                            txtStatus.Text = txtStatus.Text += "\r" + ($"Fulfillment error: {fulfillStatus}. Product Id: {productId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    txtStatus.Text = txtStatus.Text += "\r" + ($"Exception during fulfill: {ex.Message} Product Id: {productId}");
                }
            }

            //LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
            //if (!licenseInformation.ProductLicenses[productId].IsActive)
            //{
                var res = await CurrentApp.RequestProductPurchaseAsync(productId);
                if (res.Status == ProductPurchaseStatus.Succeeded)
                {
                    var el = XElement.Parse(res.ReceiptXml);
                    var items = el.Elements();
                    foreach (var item in items)
                    {
                        if (item.Name.LocalName == "ProductReceipt")
                        {
                            var at = item.Attribute("Id").Value;
                            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                            
                            if (!localSettings.Values.ContainsKey(at))
                            {
                                localSettings.Values[at] = 1;
                            } else
                            {
                                txtStatus.Text = txtStatus.Text += "\r" + ($"Receipt invalid. ProductReceipt Id {at} not unique.\r" +
                                    $"{res.TransactionId}     {at}\r" +
                                    $"Time now (UTC): {DateTime.UtcNow}    Time in receipt: {item.Attribute("PurchaseDate")}");
                        
                            foreach (var k in localSettings.Values.Keys)
                            {
                                Debug.WriteLine(k);
                            }
                        }
                            var guid = new Guid(at);
                            //Debug.Assert( guid == res.TransactionId);
                            if (guid != res.TransactionId)  // Failure case
                                txtStatus.Text = txtStatus.Text += "\r" + ($"Receipt invalid. Transaction Id does not match ProductReceipt Id\r" +
                                    $"{res.TransactionId}     {at}\r" +
                                    $"Time now (UTC): {DateTime.UtcNow}    Time in receipt: {item.Attribute("PurchaseDate")}");
                        }

                    }
                    txtStatus.Text = txtStatus.Text += "\r" + $"Purchase of {productId} succeeded!";

                }
                else
                {
                    txtStatus.Text = txtStatus.Text += "\r" + ($"Purchase of {productId} failed: {res.Status}");
                }
            //}
        }
        private void Button_Click_Paid(object sender, RoutedEventArgs e)
        {
            DoPurchase("MyConsumablePaid");
        }
    }    
}
