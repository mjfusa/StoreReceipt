using System;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
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
            DoPurchase("MyConsumable"); // This consumable is free
        }
        private async void DoPurchase(string productId)
        {
            txtStatus.Text = "";
            var unfulfilled = await CurrentApp.GetUnfulfilledConsumablesAsync();
            if (unfulfilled.Count > 0)
            {
                foreach (var transaction in unfulfilled)
                {
                    try
                    {
                        var fulfillStatus = await CurrentApp.ReportConsumableFulfillmentAsync(transaction.ProductId, transaction.TransactionId);
                        if (fulfillStatus != FulfillmentResult.Succeeded)
                        {
                            txtStatus.Text = txtStatus.Text += "\r" + ($"Fulfillment error: {fulfillStatus}. Product Id: {transaction.ProductId}");
                        }
                    } catch (Exception ex )
                    {
                        txtStatus.Text = txtStatus.Text += "\r" + ($"Exception during fulfill: {ex.Message} Product Id: {transaction.ProductId}");
                    }
                }
            }

            LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
            if (!licenseInformation.ProductLicenses[productId].IsActive)
            {
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

            }
            else

            {
                txtStatus.Text = txtStatus.Text += "\r" + ($"Could not purchase since license is still active: {productId}");
            }

        }

        private void Button_Click_Paid(object sender, RoutedEventArgs e)
        {
            DoPurchase("MyConsumablePaid");
        }
    }    
}
