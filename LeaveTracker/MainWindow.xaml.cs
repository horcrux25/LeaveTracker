﻿using System;
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

                GetUserData(user);

                if (user.Name != "")
                {
                    this.Visibility = Visibility.Hidden;
                    Calendar CalendarWindow = new Calendar(user);
                    CalendarWindow.ShowDialog();
                }
            }
        }

        private User GetUserData(User UserData)
        {
            try
            {
                string query = "SELECT UserName, Password, Name, Access FROM Logins WHERE UserName = @UserName AND Password = @Password";
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("UserName", UserData.Username);
                sqlCommand.Parameters.AddWithValue("Password", UserData.Password);

                using (DbDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        UsernameError.Text = "User not found";
                        UsernameError.TextAlignment = TextAlignment.Right;
                        UsernameError.Foreground = Brushes.Red;
                        UserData.Name = "";
                    }
                    else
                    {
                        reader.Read();
                        int nameIndex = reader.GetOrdinal("Name");
                        UserData.Name = reader.GetString(nameIndex);
                    }
                }
                return UserData;
            }
            catch (SqlException e)
            {
                if (e.Number == 100)
                {
                    UsernameError.Text = "User not found";
                    UsernameError.TextAlignment = TextAlignment.Right;
                    UsernameError.Foreground = Brushes.Red;
                }
                else MessageBox.Show(e.ToString());
                UserData.Name = "";
                return UserData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                UserData.Name = "";
                return UserData;
            }
            finally
            {
                sqlConnection.Close();
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
    }
}
