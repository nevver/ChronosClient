using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Xaml.Controls;




namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Messages : Page
    {
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
                        RootError failure = JsonConvert.DeserializeObject<RootError>(jsonResponse);
                        //string errorMessage = failure.error.ToString();
                        
                            for (int i = 0; i < data.messages.Count; i++)
                            {
                                string user = data.messages[i].user_email;
                                string createdAt = data.messages[i].created_at;
                                string body = data.messages[i].body;
                                listView_Messages.Items.Add(user + ": " + body + " at " + createdAt);
                            }
                    }
                }
            }
        }

        private async void ErrorNotAuthenticated(string s)
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
            public string created_at { get; set; }
        }

        /// <summary>
        /// Messages Json class representation for JsonConvert
        /// </summary>
        public class RootObject
        {
            public List<Message> messages { get; set; }
        }
    }

}
