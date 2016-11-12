using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            check_Input();
        }

        private void register_Click(object sender, RoutedEventArgs e)
        {
            check_Input();
        }

        private void check_Input()
        {
            if (!Regex.IsMatch(userID_Box.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                update_StatusBar();
                update_StatusText("Invalid User ID format.");
            }

            if (password_Box.Text.Length < 6)
            {
                update_StatusBar();
                update_StatusText("Invalid Password.");
            }

            if (string.IsNullOrWhiteSpace(userID_Box.Text) || string.IsNullOrWhiteSpace(password_Box.Text))
            {
                update_StatusBar();
                update_StatusText("Please enter User ID and Password to continue.");
            }
        }

        private void update_StatusBar()
        {
            status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
        }

        private void update_StatusText(string message)
        {
            status_Text.Text = message;
        }

    }
}
