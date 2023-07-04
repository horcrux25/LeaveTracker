using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Holds connection to DB
        SqlConnection sqlConnection;

        public MainWindow()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            UsernameError.Text = "";
            PasswordError.Text = "";
            NewUser.Text = "";

            if ((userName.Text == "") || (password.Text == ""))
            {
                if ((userName.Text == "") && (password.Text == ""))
                {
                    UsernameError.Text = string.Format("Invalid username");
                    UsernameError.Foreground = Brushes.Red;
                    UsernameError.TextAlignment = TextAlignment.Right;
                    PasswordError.Text = string.Format("Invalid password");
                    PasswordError.Foreground = Brushes.Red;
                    PasswordError.TextAlignment = TextAlignment.Right;
                }
                else if (userName.Text == "")
                {
                    UsernameError.Text = string.Format("Invalid username");
                    UsernameError.Foreground = Brushes.Red;
                    UsernameError.TextAlignment = TextAlignment.Right;
                }
                else
                {
                    PasswordError.Text = string.Format("Invalid password");
                    PasswordError.Foreground = Brushes.Red;
                    PasswordError.TextAlignment = TextAlignment.Right;
                }

                NewUser.Text = string.Format("Select register for new user");
                NewUser.TextAlignment = TextAlignment.Right;
                NewUser.Foreground = Brushes.Blue;
            }
        }
    }
}
