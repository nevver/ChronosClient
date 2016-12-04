using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Net.Http;
using System.Diagnostics;
using Flurl.Http;
using Newtonsoft.Json;
using Windows.Storage;

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
        /// 
        public class jsonParse
        {
            public string auth_token { get; set; }
            public string session_Token { get; set; }
            public string email { get; set; }
            public string error { get; set; }
            public string password { get; set; }
        }

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
            DataContainer.User = userID_Box.Text.ToString();

            if (check_Input() == false)
            {
                enable_Buttons(false);
                try
                {
                   
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/auth_user".PostUrlEncodedAsync(new
                    {

                        email = userID_Box.Text.ToString(),
                        password = password_Box.Password.ToString()

                    });

                    string login_Response = await responseMessage.Content.ReadAsStringAsync();
                    string sent = responseMessage.ToString();
                    jsonParse json = JsonConvert.DeserializeObject<jsonParse>(login_Response);
                    DataContainer.Token = json.auth_token.ToString();
                    update_StatusBar("blue");
                    update_StatusText("Authenticated!");
                    Debug.WriteLine(sent);
                    Debug.WriteLine(login_Response.ToString());
                    Debug.WriteLine(DataContainer.Token);
                    if (checkKeysDirectory() == false)
                    {
                        Frame.Navigate(typeof(AsymmetricKeys));
                    }
                    else if (checkKeysDirectory() == true)
                    {
                        Frame.Navigate(typeof(ListUsers));
                    }

                }
                catch (HttpRequestException hre)
                {
                    update_StatusBar("red"); 
                    update_StatusText("Login failed. Try again.");
                }
                catch (Exception ex)
                {
                    update_StatusBar("red");
                    update_StatusText("Login failed. Try again.");
                }
               
            }
        }



        /// <summary>
        /// Checks directory for keys and returns a boolean
        /// 
        /// true = keys exists
        /// false = keys do not exists
        /// 
        /// </summary>
        /// <returns></returns>
        private static Boolean checkKeysDirectory()
        {

            //Automatically skips direcory check when true
            // Changed for debugging purposes
            return true;
            
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
                    HttpResponseMessage responseMessage = await "https://chronoschat.co/reg_user".PostUrlEncodedAsync(new
                    {
                        email = userID_Box.Text.ToString(),
                        password = password_Box.Password.ToString()
                    });
                    string reg_Response = await responseMessage.Content.ReadAsStringAsync();
                    string sent = responseMessage.ToString();
                    update_StatusText(reg_Response);
                    Debug.WriteLine(sent);
                    Debug.WriteLine(reg_Response.ToString());

                }
                catch (HttpRequestException hre)
                {
                    update_StatusBar("red");
                    update_StatusText("Registration failed. Try again.");
                }
                catch (Exception ex)
                {
                    update_StatusBar("red");
                    update_StatusText("Registration failed. Try again.");
                }


            }
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>Boolean false means validation passes</returns>
        private Boolean check_Input()
        {
            if (string.IsNullOrWhiteSpace(userID_Box.Text) || string.IsNullOrWhiteSpace(password_Box.Password.ToString()))
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

            if (password_Box.ToString().Length < 6)
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


        private void password_Box_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
