using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI;
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
                                    body.Text = data.messages[i].body;
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
                                    body.Text = data.messages[i].body;
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

        private async void sendMessage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {


            if (message != null)
            {
                sendMessageAsync.IsEnabled = false;
                try
                {
                    string accessToken = DataContainer.Token;
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/messages/create".WithHeader("Authorization", "Bearer " + accessToken).PostUrlEncodedAsync(new
                    {

                        conversation_id = DataContainer.ConversationID,
                        body = message

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

        private void generatePublicKey_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }

}
