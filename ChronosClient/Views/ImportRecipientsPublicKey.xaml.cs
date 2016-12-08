using System;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportRecipientsPublicKey : Page
    {
        public ImportRecipientsPublicKey()
        {
            this.InitializeComponent();
            this.continueButton.IsEnabled = false;
            this.continueButton.Visibility = Visibility.Collapsed;
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ListUsers));
        }

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Messages));
        }

        public IBuffer decode64BaseString(string str)
        {
            IBuffer buffFromBase64String = CryptographicBuffer.DecodeFromBase64String(str);
            return buffFromBase64String;
        }

        public string encodeBuffTo64BaseString(IBuffer buff)
        {
            string strBase64 = CryptographicBuffer.EncodeToBase64String(buff);
            return strBase64;
        }

        private async void fileImport_Click(object sender, RoutedEventArgs e)
        {
            this.continueButton.Visibility = Visibility.Visible;
            dirSelectorButton.IsEnabled = false;
            dirSelectorButton.Visibility = Visibility.Collapsed;
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add(".PublicKey");

            // Picks folder to write to
            Windows.Storage.StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {

                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                 FutureAccessList.AddOrReplace("PickedFileToken", file);


                string recipientPublicKeyString = await Windows.Storage.FileIO.ReadTextAsync(file);

                // decode string into buffer
                DataContainer.recipientPublicKey = decode64BaseString(recipientPublicKeyString);

                // write the public key import to memory for later access
                Windows.Storage.StorageFolder localFolder =
                   Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile recipientPublicKey =
                    await localFolder.CreateFileAsync((DataContainer.Recipient + ".PublicKey"), Windows.Storage.CreationCollisionOption.ReplaceExisting);

                continueButton.Visibility = Visibility.Visible;
                continueButton.IsEnabled = true;
            }
            else
            {
                dirSelectorButton.IsEnabled = true;
                dirSelectorButton.Visibility = Visibility.Visible;
                continueButton.Visibility = Visibility.Collapsed;
                continueButton.IsEnabled = false;
            }

            
            
        }

        private void Logout_Checked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }
    }
}
