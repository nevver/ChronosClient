using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel.Core;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Messages : Page
    {
        private static string message;
        private static string strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaPkcs1;
        private string recipientEncrypted;
        private string senderEncrypted;
        private IBuffer encryptedMessage;
        private IBuffer decryptedMessage;
        private string encData;

        //private object buffPublicKey;

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

        public class MessageStatus
        {
            public string status { get; set; }
        }

        public Messages()
        {
            this.InitializeComponent();

            GetMessages("https://www.chronoschat.co/messages/index?conversation_id=" + DataContainer.ConversationID);

        }

        private void listView_Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }





        async void GetMessages(string url)
        {
            Debug.WriteLine(url);
            using (HttpClient client = new HttpClient())
            {

                string accessToken = DataContainer.Token;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                Debug.WriteLine("Client headers: " + client.DefaultRequestHeaders.ToString());

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {
                        string jsonResponse = await content.ReadAsStringAsync();
                        Debug.WriteLine("Json response: " + jsonResponse);
                        RootObject data = JsonConvert.DeserializeObject<RootObject>(jsonResponse);


                        if (data.messages.Count == 0)
                        {
                            listView_Messages.Items.Add("No messages");
                        }
                        else
                        {
                            for (int i = 0; i < data.messages.Count; i++)
                            {

                                if (data.messages[i].user_email == DataContainer.User)
                                {

                                    TextBlock user = new TextBlock();
                                    user.FontSize = 14;
                                    user.Foreground = new SolidColorBrush(Colors.Gray);
                                    user.Width = 535;
                                    user.TextAlignment = Windows.UI.Xaml.TextAlignment.Right;
                                    user.Text = data.messages[i].user_email + "  at " + data.messages[i].created_at;
                                    listView_Messages.Items.Add(user);


                                    TextBlock space = new TextBlock();

                                    TextBlock body = new TextBlock();
                                    body.FontSize = 26;
                                    body.Width = 535;
                                    body.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                                    body.TextAlignment = Windows.UI.Xaml.TextAlignment.Right;
                                    encData = data.messages[i].body2;
                                    IBuffer userData = CryptographicBuffer.DecodeFromBase64String(encData);
                                    asymmetricDecryptMessageBody(strAsymmetricAlgName, userData);
                                    body.Text = message;
                                    listView_Messages.Items.Add(body);
                                    listView_Messages.Items.Add(space);

                                }
                                else if (data.messages[i].user_email == DataContainer.Recipient)
                                {
                                    TextBlock user = new TextBlock();
                                    user.IsColorFontEnabled = true;
                                    user.FontSize = 14;
                                    user.Foreground = new SolidColorBrush(Colors.Gray);
                                    user.Width = 535;
                                    user.TextAlignment = Windows.UI.Xaml.TextAlignment.Left;
                                    user.Text = data.messages[i].user_email + "  at " + data.messages[i].created_at;
                                    listView_Messages.Items.Add(user);



                                    TextBlock space = new TextBlock();
                                    TextBlock body = new TextBlock();
                                    body.FontSize = 26;
                                    body.Width = 535;
                                    body.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                                    body.TextAlignment = Windows.UI.Xaml.TextAlignment.Left;
                                    encData = data.messages[i].body;
                                    IBuffer userData = CryptographicBuffer.DecodeFromBase64String(encData);
                                    asymmetricDecryptMessageBody(strAsymmetricAlgName, userData);
                                    body.Text = message;
                                    listView_Messages.Items.Add(body);
                                    listView_Messages.Items.Add(space);
                                }



                            }
                        }

                    }
                }
            }
        }

        private async void ErrorMessageDialogBox(string s)
        {
            ContentDialog error = new ContentDialog()
            {
                Title = "Error",
                Content = "Content failed to load, try again. Reason:  " + s,
                PrimaryButtonText = "Ok",

            };

            await error.ShowAsync();
            Frame.Navigate(typeof(ListUsers));
        }


        public class RootError
        {
            public string error { get; set; }
        }

        /// <summary>
        /// Messages Json class representation for JsonConvert
        /// </summary>
        public class Message
        {
            public int user_id { get; set; }
            public string user_email { get; set; }
            public int conversation_id { get; set; }
            public string body { get; set; }
            public string body2 { get; set; }
            public string created_at { get; set; }
        }

        /// <summary>
        /// Messages Json class representation for JsonConvert
        /// </summary>
        public class RootObject
        {
            public List<Message> messages { get; set; }
        }

        private void textToSend_TextChanged(object sender, TextChangedEventArgs e)
        {
            message = textToSend.Text;
        }

        // method to encrypt plain text
        public void asymemtricEncryptMessageBodyForSender(
            String strAsymmetricAlgName)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(DataContainer.senderPublicKey);

            IBuffer plain = CryptographicBuffer.ConvertStringToBinary(message, BinaryStringEncoding.Utf8);

            // Encrypt message by using the public key.
            encryptedMessage = CryptographicEngine.Encrypt(publicKey, plain, null);

            //convert to cipher string
             senderEncrypted = CryptographicBuffer.EncodeToBase64String(encryptedMessage);

        }

        // method to encrypt plain text
        public void asymemtricEncryptMessageBodyRecipient(
            String strAsymmetricAlgName)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(DataContainer.recipientPublicKey);

            IBuffer plain = CryptographicBuffer.ConvertStringToBinary(message, BinaryStringEncoding.Utf8);

            // Encrypt message by using the public key.
            encryptedMessage = CryptographicEngine.Encrypt(publicKey, plain, null);

            //convert to cipher string
            recipientEncrypted = CryptographicBuffer.EncodeToBase64String(encryptedMessage);

        }

        // method to decrypt ciphertext
        public async void asymmetricDecryptMessageBody(
    String strAsymmetricAlgName,
    IBuffer buffEncryptedMessageBody)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            CryptographicKey pair = objAsymmAlgProv.ImportKeyPair(DataContainer.senderKeyPair);
            try
            {
                // Use the private key embedded in the key pair to decrypt the session key.
                decryptedMessage = CryptographicEngine.Decrypt(pair, buffEncryptedMessageBody, null);
            }
            catch (System.ArgumentException ar)
            {
                Debug.WriteLine(ar.ToString());
                var dialog = new MessageDialog("Error: Key Mismatch. Unable to display new messages.");
                await dialog.ShowAsync();
                // CoreApplication.Exit();
            }


            //convert to string
            message = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedMessage);

        }

        private async void sendMessage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {


            if (message != null)
            {
                sendMessageAsync.IsEnabled = false;
                asymemtricEncryptMessageBodyForSender(strAsymmetricAlgName);
                asymemtricEncryptMessageBodyRecipient(strAsymmetricAlgName);
                try
                {
                    string accessToken = DataContainer.Token;
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/messages/create".WithHeader("Authorization", "Bearer " + accessToken).PostUrlEncodedAsync(new
                    {

                        conversation_id = DataContainer.ConversationID,
                        body = recipientEncrypted,
                        body2 = senderEncrypted

                    });

                    string createMessageResponse = await responseMessage.Content.ReadAsStringAsync();
                    string sent = responseMessage.ToString();
                    MessageStatus json = JsonConvert.DeserializeObject<MessageStatus>(createMessageResponse);

                    string status = json.status;
                    if (status == "Message Sent")
                    {
                        Frame.Navigate(typeof(Messages));
                    }
                    else
                    {
                        ErrorMessageDialogBox(status);
                    }

                }
                catch (HttpRequestException hre)
                {
                    ErrorMessageDialogBox(hre.ToString());
                }
                catch (Exception ex)
                {
                    ErrorMessageDialogBox(ex.ToString());
                }

            }

        }

        private void backToUsers_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ListUsers));
        }

        private async void generatePublicKey_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            generatePublicKey.IsEnabled = false;
            generatePublicKey.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".PublicKey");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                 FutureAccessList.AddOrReplace("PickedFolderToken", folder);


                // create pubKeyExport file in chosen directory 
                Windows.Storage.StorageFile pubKeyFile =
                    await folder.CreateFileAsync(DataContainer.User + ".PublicKey",
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // generate asymmetric keys
                // IBuffer buffPublicKey;
                //this.createAsymmetricKeyPair(
                //    strAsymmetricAlgName,
                //    asymmetricKeyLength,
                //    out DataContainer.buffPublicKey);

                // encode buffer into a ASCII string 
                string publicKeyEncded = encodeBuffTo64BaseString(DataContainer.senderPublicKey);


                // write to the folder selected by user
                await Windows.Storage.FileIO.WriteTextAsync(pubKeyFile, publicKeyEncded);

                //// create keypair file for application use only for the user that is logged in
                //string keyPairName = DataContainer.User + ".KeyPair";
                //DataContainer.KeyPairFileName = keyPairName;
                //Windows.Storage.StorageFolder localFolder =
                //    Windows.Storage.ApplicationData.Current.LocalFolder;
                //Windows.Storage.StorageFile keyPair =
                //    await localFolder.CreateFileAsync((DataContainer.KeyPairFileName),
                //        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                //// encode buffer into a ASCII string 
                //string keyPairEncod = encodeBuffTo64BaseString(DataContainer.buffKeyPair);

                //// write to the file the app uses
                //await Windows.Storage.FileIO.WriteTextAsync(keyPair, keyPairEncod);

            }

            generatePublicKey.Visibility = Windows.UI.Xaml.Visibility.Visible;
            generatePublicKey.IsEnabled = true;

        }

        private void Logout_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }
    }
}
