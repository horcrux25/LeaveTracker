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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

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

            userName.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            List<object> userInfoBoxes = new List<object>() { userName,  password };
            TextBlock[] userTextBlock = new TextBlock[] { UsernameError, PasswordError };
            UsernameError.Text = "";
            PasswordError.Text = "";
            NewUser.Text = "";

            bool EmptyFlag = false;
            int LoopCounter = 0;
            foreach(object box in userInfoBoxes)
            {
                if (box is TextBox)
                {
                    TextBox box1 = (TextBox)box;
                    if (box1.Text == "") SetError(LoopCounter);
                }
                else if (box is PasswordBox)
                {
                    PasswordBox box2 = (PasswordBox)box;
                    if (box2.Password.ToString() == "") SetError(LoopCounter);
                }
                LoopCounter++;
            }

            void SetError(int ErrorIndex)
            {
                EmptyFlag = true;
                if (ErrorIndex == 0) userTextBlock[ErrorIndex].Text = string.Format("Invalid username");
                else userTextBlock[ErrorIndex].Text = string.Format("Invalid password");

                userTextBlock[ErrorIndex].TextAlignment = TextAlignment.Right;
                userTextBlock[ErrorIndex].Foreground = Brushes.Red;

                NewUser.Text = string.Format("Select register for new user");
                NewUser.TextAlignment = TextAlignment.Right;
                NewUser.Foreground = Brushes.Blue;
            }

            if (EmptyFlag == false)
            {
                User user = new User() { 
                    Username = userName.Text, 
                    Password = password.Password.ToString() 
                };

                string query = "SELECT * FROM Logins WHERE UserName = @UserName AND Password = @Password";
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Parameters.AddWithValue("UserName", user.Username);
                sqlCommand.Parameters.AddWithValue("Password", user.Password);

                user = user.GetUserData(sqlCommand, query);

                if (user.Name != "" && user.Name != null)
                {
                    switch (user.Access)
                    {
                        case 0:
                            UsernameError.Text = "Awaiting admin approval";
                            UsernameError.TextAlignment = TextAlignment.Right;
                            UsernameError.Foreground = Brushes.Red;
                            break;

                        case 2:
                        case 3:
                            User[] usersList = user.CheckPendingRegisters();
                            if (usersList.Length == 0)
                            {
                                this.Visibility = Visibility.Hidden;
                                Calendar CalendarWindow1 = new Calendar(user);
                                CalendarWindow1.ShowDialog();
                            }
                            else
                            {
                                MessageBoxResult result = 
                                    MessageBox.Show("Do you want to check pending user request/" +
                                    "s?","Confirm",MessageBoxButton.YesNo,MessageBoxImage.Question);

                                if (result == MessageBoxResult.Yes)
                                {
                                    this.Visibility = Visibility.Hidden;
                                    Register RegisterWindow = new Register(usersList, user);
                                    RegisterWindow.ShowDialog();
                                }
                                else
                                {
                                    this.Visibility = Visibility.Hidden;
                                    Calendar CalendarWindow1 = new Calendar(user);
                                    CalendarWindow1.ShowDialog();
                                }
                                
                            }
                            break;

                        default:
                            this.Visibility = Visibility.Hidden;
                            Calendar CalendarWindow2 = new Calendar(user);
                            CalendarWindow2.ShowDialog();
                            break;
                    }
                }
                else
                {
                    UsernameError.Text = "User not found";
                    UsernameError.TextAlignment = TextAlignment.Right;
                    UsernameError.Foreground = Brushes.Red;
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Register RegisterWindow = new Register();
            RegisterWindow.ShowDialog();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Login_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
            }
        }
    }
}
