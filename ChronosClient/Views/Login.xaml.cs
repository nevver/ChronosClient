using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using Flurl.Http;

namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        /// <summary>
        /// Static variables
        /// </summary>

        private static string built_json;
        private static StringContent stringContent;
        public static string sessionToken { get; set; }
        public string email { get; set; }
        public string password { get; set; }


        /// <summary>
        /// Main
        /// </summary>
        public Login()
        {
            this.InitializeComponent();

        }

        /// <summary>
        /// Login action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void login_Click(object sender, RoutedEventArgs e)
        {
            if (check_Input() == false)
            {
                enable_Buttons(false);
                try
                {
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/auth_user".PostUrlEncodedAsync(new
                    {
                        email = userID_Box.Text.ToString(),
                        password = password_Box.Text.ToString()
                    });
                    string login_Response = await responseMessage.Content.ReadAsStringAsync();
                    string sent = responseMessage.ToString();
                    update_StatusText(login_Response);
                    Debug.WriteLine(sent);
                    Debug.WriteLine(login_Response.ToString());
                }
                catch (HttpRequestException hre)
                {
                    update_StatusText("Error:" + hre.Message);
                }
                catch (Exception ex)
                {
                    update_StatusText(ex.Message);
                }
               
            }
        }

        /// <summary>
        /// Registration action 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void register_Click(object sender, RoutedEventArgs e)
        {
            if (check_Input() == false)
            {
                enable_Buttons(false);
                try
                {
                   
                }
                catch (HttpRequestException hre)
                {
                    update_StatusText("Error:" + hre.Message);
                }
                catch (Exception ex)
                {
                    update_StatusText(ex.Message);
                }

            }
        }

        public async Task<System.Net.Http.HttpResponseMessage> SendRequestAsync(string adaptiveUri, string json)
        {
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri("http://chronoschat.co/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpConent = new StringContent(json, Encoding.UTF8, "application/json");
                System.Net.Http.HttpResponseMessage responseMessage = null;
                try
                {
                    responseMessage = await client.PostAsync(adaptiveUri, httpConent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (responseMessage == null)
                    {
                        responseMessage = new System.Net.Http.HttpResponseMessage();
                    }
                    responseMessage.StatusCode = System.Net.HttpStatusCode.BadGateway;
                    responseMessage.ReasonPhrase = string.Format("HttpClient request failed.", ex);
                }
               
                return responseMessage;
            }
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>Boolean false means validation passes</returns>
        private Boolean check_Input()
        {
            if (string.IsNullOrWhiteSpace(userID_Box.Text) || string.IsNullOrWhiteSpace(password_Box.Text))
            {
                update_StatusBar("red");
                update_StatusText("Please enter User ID and Password to continue.");
                return true;
            }

            if (!Regex.IsMatch(userID_Box.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                update_StatusBar("blue");
                update_StatusText("Invalid User ID format.");
                return true;
            }

            if (password_Box.Text.Length < 6)
            {
                update_StatusBar("blue");
                update_StatusText("Invalid Password.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates status bar background color
        /// </summary>
        /// <param name="color"></param>
        private void update_StatusBar(string color)
        {
            if (color == "blue")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
            }
            
            if (color == "red")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 20, 0));
            }

            if (color == "green")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 177, 76));
            }

        }

        /// <summary>
        /// Updates status bar text content
        /// </summary>
        /// <param name="message"></param>
        private void update_StatusText(string message)
        {
            if (message == null)
            {
                update_StatusBar("red");
                update_StatusText("Null Response.");
                enable_Buttons(true);  
            }
            else
            {
                status_Text.Text = message;
                enable_Buttons(true);
            }
            
        }

        /// <summary>
        /// Enables and disables login and register buttons
        /// </summary>
        /// <param name="option"></param>
        private void enable_Buttons(Boolean option)
        {
            this.login_Button.IsEnabled = option;
            this.register_Button.IsEnabled = option;
        }

        /// <summary>
        /// Builds Json and HttpContent stringContent  for login and registration in the Login view
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pass"></param>
        private static void setJson(string email, string pass)
        {
            dynamic json = new ExpandoObject();
            json.email = email;
            json.password = pass;
            built_json = Newtonsoft.Json.JsonConvert.SerializeObject(json);
            stringContent = new StringContent(built_json, Encoding.UTF8, "application/json");
        }

        private void password_Box_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
