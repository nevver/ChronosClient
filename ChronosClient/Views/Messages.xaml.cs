using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
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
        private static string strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaOaepSha512;
        private string encData;

        // Initialize the symmetric encryption
        private static String strSymAlgName = SymmetricAlgorithmNames.AesGcm;
        private static UInt32 keyLength = 64;  // Length of the key, in bytes

        // encoding for symmetric message encryption
        private BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;
        // buffer for newly generated AES key
        private IBuffer keyMaterialSender;
        // string from newly generated AES key buffer
        private string keyMaterialStringSender;
        // key from newly generated AES key
        private CryptographicKey keySender;
        // buffer for newly generated nonce (IV)
        private IBuffer buffNonceSender;
        // encrypted message data with AES key
        private IBuffer encryptedMessageDataSender;
        // authentication tag for AES encrypted data
        private IBuffer authenticationTagSender;
        // string equivalent of the buffer containing the encrypted data
        private string encryptedMessageDataStringSender;
        // string equivalent of the buffer containing the tag for AES encrypted data
        private string authenticationTagStringSender;
        // string equivalent of the buffer containing the nonce
        private string nonceStringSender;
        // buffer with encrypted AES key 
        private IBuffer encryptedAESKeyBuffSender;
        // string equivalent of encrypted key
        private string encryptedKeyStringSender;

        // buffer for newly generated AES key
        private IBuffer keyMaterialRecipient;
        // string from newly generated AES key buffer
        private string keyMaterialStringRecipient;
        // key from newly generated AES key
        private CryptographicKey keyRecipient;
        // buffer for newly generated nonce (IV)
        private IBuffer buffNonceRecipient;
        // encrypted message data with AES key
        private IBuffer encryptedMessageDataRecipient;
        // authentication tag for AES encrypted data
        private IBuffer authenticationTagRecipient;
        // string equivalent of the buffer containing the encrypted data
        private string encryptedMessageDataStringRecipient;
        // string equivalent of the buffer containing the tag for AES encrypted data
        private string authenticationTagStringRecipient;
        // string equivalent of the buffer containing the nonce
        private string nonceStringRecipient;
        // buffer for encrypted AES key for recipient 
        private IBuffer encryptedAESKeyBuffRecipient;
        // string equivalent for encrypted key
        private string encryptedKeyStringRecipient;

        // decrypted data contents
        private IBuffer buffDecrypted;
        // string representation of decrypted data contents
        private string strDecrypted;

        // Initialize a static nonce value.
        static byte[] NonceBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };





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

        // fetches encrypted messages from the server
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
                                //if a message was sent from the current user
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
                                    nonceStringSender = data.messages[i].nc2;
                                    encryptedKeyStringSender = data.messages[i].key2;
                                    authenticationTagStringSender = data.messages[i].tag2;

                                    //decrypt data if the user sent
                                    AuthenticatedDecryptionSender(strSymAlgName, encryptedKeyStringSender, encData, nonceStringSender, authenticationTagStringSender);
                                    body.Text = strDecrypted;
                                    listView_Messages.Items.Add(body);
                                    listView_Messages.Items.Add(space);

                                }
                                //if the message was sent to the current user
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
                                    nonceStringSender = data.messages[i].nc;
                                    encryptedKeyStringSender = data.messages[i].key;
                                    authenticationTagStringSender = data.messages[i].tag;

                                    //decrypt the data sent to the user
                                    AuthenticatedDecryptionSender(strSymAlgName, encryptedKeyStringSender, encData, nonceStringSender, authenticationTagStringSender);
                                    body.Text = strDecrypted;
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
            public string nc { get; set; }
            public string tag { get; set; }
            public string key { get; set; }
            public string body2 { get; set; }
            public string nc2 { get; set; }
            public string tag2 { get; set; }
            public string key2 { get; set; }
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

        // Encryption and authentication method for sender
        private void AuthenticatedEncryptionSender(
            String strMsg,
            String strAlgName,
            UInt32 keyLength)
        {
            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a buffer that contains the data to be encrypted.
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Generate a symmetric key.
            keyMaterialSender = CryptographicBuffer.GenerateRandom(keyLength);
            keyMaterialStringSender = CryptographicBuffer.EncodeToBase64String(keyMaterialSender);
            keySender = objAlgProv.CreateSymmetricKey(keyMaterialSender);

            // Generate a new nonce value.
            buffNonceSender = GetNonce();

            // Encrypt and authenticate the message.
            EncryptedAndAuthenticatedData objEncrypted = CryptographicEngine.EncryptAndAuthenticate(
                keySender,
                buffMsg,
                buffNonceSender,
                null);

            encryptedMessageDataSender = objEncrypted.EncryptedData;
            authenticationTagSender = objEncrypted.AuthenticationTag;
            encryptedMessageDataStringSender = CryptographicBuffer.EncodeToBase64String(encryptedMessageDataSender);
            authenticationTagStringSender = CryptographicBuffer.EncodeToBase64String(authenticationTagSender);
            nonceStringSender = CryptographicBuffer.EncodeToBase64String(buffNonceSender);

        }

        // Encryption and authentication method for recipient
        private void AuthenticatedEncryptionRecipient(
            String strMsg,
            String strAlgName,
            UInt32 keyLength)
        {
            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a buffer that contains the data to be encrypted.
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Generate a symmetric key.
            keyMaterialRecipient = CryptographicBuffer.GenerateRandom(keyLength);
            keyMaterialStringRecipient = CryptographicBuffer.EncodeToBase64String(keyMaterialRecipient);
            keyRecipient = objAlgProv.CreateSymmetricKey(keyMaterialRecipient);

            // Generate a new nonce value.
            buffNonceRecipient = GetNonce();

            // Encrypt and authenticate the message.
            EncryptedAndAuthenticatedData objEncrypted = CryptographicEngine.EncryptAndAuthenticate(
                keyRecipient,
                buffMsg,
                buffNonceRecipient,
                null);

            encryptedMessageDataRecipient = objEncrypted.EncryptedData;
            authenticationTagRecipient = objEncrypted.AuthenticationTag;
            encryptedMessageDataStringRecipient = CryptographicBuffer.EncodeToBase64String(encryptedMessageDataRecipient);
            authenticationTagStringRecipient = CryptographicBuffer.EncodeToBase64String(authenticationTagRecipient);
            nonceStringRecipient = CryptographicBuffer.EncodeToBase64String(buffNonceRecipient);

        }

        // method to decrypt ciphertext of AES key
        public async void asymmetricDecryptAESKeySender(
    String strAsymmetricAlgName,
    IBuffer buffEncryptedAESKey)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            CryptographicKey pair = objAsymmAlgProv.ImportKeyPair(DataContainer.senderKeyPair);
            try
            {
                // Use the private key embedded in the key pair to decrypt the session key.
                keyMaterialSender = CryptographicEngine.Decrypt(pair, buffEncryptedAESKey, null);
            }
            catch (System.ArgumentException ar)
            {
                Debug.WriteLine(ar.ToString());
                var dialog = new MessageDialog("Error: Key Mismatch. Unable to display new messages.");
                await dialog.ShowAsync();
                // CoreApplication.Exit();
            }


            //convert to string
            keyMaterialStringSender = CryptographicBuffer.EncodeToBase64String(keyMaterialSender);

        }

       

        public void AuthenticatedDecryptionSender(
           String strAlgName, string key, string message, string nonce, string tag)
        {

            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);


            IBuffer encryptedAESKey = CryptographicBuffer.DecodeFromBase64String(key);

            asymmetricDecryptAESKeySender(strAsymmetricAlgName, encryptedAESKey);

            IBuffer decryptedAESKeyBuff = CryptographicBuffer.DecodeFromBase64String(keyMaterialStringSender);
            CryptographicKey keyFromEncryptedString = objAlgProv.CreateSymmetricKey(decryptedAESKeyBuff);

            IBuffer encryptedDataFromStringBuff = CryptographicBuffer.DecodeFromBase64String(message);
            IBuffer nonceFromString = CryptographicBuffer.DecodeFromBase64String(nonce);
            IBuffer authenticationTagFromString = CryptographicBuffer.DecodeFromBase64String(tag);

            // The input key must be securely shared between the sender of the encrypted message
            // and the recipient. The nonce must also be shared but does not need to be shared
            // in a secure manner. If the sender encodes the message string to a buffer, the
            // binary encoding method must also be shared with the recipient.
            // The recipient uses the DecryptAndAuthenticate() method as follows to decrypt the 
            // message, authenticate it, and verify that it has not been altered in transit.
            buffDecrypted = CryptographicEngine.DecryptAndAuthenticate(
                keyFromEncryptedString,
                encryptedDataFromStringBuff,
                nonceFromString,
                authenticationTagFromString,
                null);

 
            strDecrypted = CryptographicBuffer.ConvertBinaryToString(encoding, buffDecrypted);


        }

        IBuffer GetNonce()
        {
            // Security best practises require that an ecryption operation not
            // be called more than once with the same nonce for the same key.
            // A nonce value can be predictable, but must be unique for each
            // secure session.

            NonceBytes[0]++;
            for (int i = 0; i < NonceBytes.Length - 1; i++)
            {
                if (NonceBytes[i] == 255)
                {
                    NonceBytes[i + 1]++;
                }
            }

            return CryptographicBuffer.CreateFromByteArray(NonceBytes);
        }

        // method to encrypt AES key for sender
        public void asymemtricEncryptAESKeyforSender(
            String strAsymmetricAlgName)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(DataContainer.senderPublicKey);

            IBuffer plainKey = CryptographicBuffer.DecodeFromBase64String(keyMaterialStringSender);

            // Encrypt message by using the public key.
            encryptedAESKeyBuffSender = CryptographicEngine.Encrypt(publicKey, plainKey, null);

            //convert to cipher string
            encryptedKeyStringSender = CryptographicBuffer.EncodeToBase64String(encryptedAESKeyBuffSender);

        }

        // method to encrypt AES key for recipient
        public void asymemtricEncryptAESKeyforRecipient(
            String strAsymmetricAlgName)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(DataContainer.recipientPublicKey);

            IBuffer plainKey = CryptographicBuffer.DecodeFromBase64String(keyMaterialStringRecipient);

            // Encrypt message by using the public key.
            encryptedAESKeyBuffRecipient = CryptographicEngine.Encrypt(publicKey, plainKey, null);

            //convert to cipher string
            encryptedKeyStringRecipient = CryptographicBuffer.EncodeToBase64String(encryptedAESKeyBuffRecipient);

        }

        private async void sendMessage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {


            if (message != null)
            {
                sendMessageAsync.IsEnabled = false;

                // AES encrypt message for current user to decrypt 
                AuthenticatedEncryptionSender(
                message,
                strSymAlgName,
                keyLength);

                // Encrypting AES key with rsa public key of sender
                asymemtricEncryptAESKeyforSender(strAsymmetricAlgName);

                // AES encrypt message for recipient to decrypt 
                AuthenticatedEncryptionRecipient(
                message,
                strSymAlgName,
                keyLength);

                // Encrypting AES key with rsa public key of recipient
                asymemtricEncryptAESKeyforRecipient(strAsymmetricAlgName);
                try
                {
                    string accessToken = DataContainer.Token;
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/messages/create".WithHeader("Authorization", "Bearer " + accessToken).PostUrlEncodedAsync(new
                    {

                        conversation_id = DataContainer.ConversationID,
                        body = encryptedMessageDataStringRecipient,
                        body2 = encryptedMessageDataStringSender,
                        nc = nonceStringRecipient, 
                        nc2 = nonceStringSender, 
                        tag = authenticationTagStringRecipient, 
                        tag2 = authenticationTagStringSender, 
                        key = encryptedKeyStringRecipient, 
                        key2 = encryptedKeyStringSender

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


                // encode buffer into a ASCII string 
                string publicKeyEncded = encodeBuffTo64BaseString(DataContainer.senderPublicKey);


                // write to the folder selected by user
                await Windows.Storage.FileIO.WriteTextAsync(pubKeyFile, publicKeyEncded);

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
