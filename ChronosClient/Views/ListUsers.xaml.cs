using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
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
                            string user = data.users[i].email.ToString();
                            listView_Users.Items.Add(user);
                        }

                        next_Button.IsEnabled = false;
                    }
                }
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

        private void listView_Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            next_Button.IsEnabled = listView_Users.SelectedItems.Count > 0;
            DataContainer.Recipient = listView_Users.SelectedItems[0].ToString();

        }

        private void next_Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}