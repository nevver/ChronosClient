using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;



namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListUsers : Page
    {
        public ListUsers()
        {
            this.InitializeComponent();
            GetUsers("https://www.chronoschat.co/conversations/index");
        }

        async void GetUsers(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                string accessToken = DataContainer.Token.ToString();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        RootObject data = JsonConvert.DeserializeObject<RootObject>(mycontent);
                        
                        for (int i = 0; i < data.users.Count; i++)
                        {
                            if (data.users[i].email != DataContainer.User)
                            {
                                string user = data.users[i].email.ToString();
                                listView_Users.Items.Add(user);
                            }
                           
                        }
                        
                        next_Button.IsEnabled = false;
                    }
                }
              
            }
            
        }

        /// <summary>
        /// Get Conversation ID
        /// </summary>
        private async void GetConversation()
        {

            try
            {
                //Need headers
                string accessToken = DataContainer.Token.ToString();
                HttpResponseMessage responseMessage = await "https://chronoschat.co/conversations/create".WithHeader("Authorization", "Bearer " + accessToken).PostUrlEncodedAsync(new
                {

                    recipient_email = DataContainer.Recipient

                });

                string createConversationResponse = await responseMessage.Content.ReadAsStringAsync();
                Debug.WriteLine("Json response: " + createConversationResponse);
                ConversationObject json = JsonConvert.DeserializeObject<ConversationObject>(createConversationResponse);
                DataContainer.ConversationID = json.id.ToString();
                Debug.WriteLine("ConversationID: " + DataContainer.ConversationID);

            }
            catch (HttpRequestException hre)
            {
                Debug.WriteLine(hre.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        // json user class representation for JsonConvert
        public class User
        {
            public string email { get; set; }
        }

        // json ROOT class representation for JsonConvert
        public class RootObject
        {
            public List<User> users { get; set; }
        }

        /// <summary>
        /// Conversation Json class representation for JsonConvert
        /// </summary>
        public class ConversationObject
        {
            public int id { get; set; }
        }


        private void listView_Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            next_Button.IsEnabled = listView_Users.SelectedItems.Count > 0;
            DataContainer.Recipient = listView_Users.SelectedItems[0].ToString();
            Debug.WriteLine(DataContainer.Recipient);
            GetConversation();

        }

        private void next_Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            Frame.Navigate(typeof(ImportRecipientsPublicKey));
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

        private async void generatePublicKey_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
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

        }

        private void Logout_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }
    }
}