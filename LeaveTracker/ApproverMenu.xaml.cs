using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Shapes;
using System.Configuration;

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for ApproverMenu.xaml
    /// </summary>
    public partial class ApproverMenu : Window
    {
        SqlConnection sqlConnection;
        string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;
        string query;

        bool ClosingBypass = false;
        User user = new User();

        List<string> RolesSelection = new List<string>();

        public ApproverMenu(User user)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connectionString);
            this.user = user;

            RolesSelection.Add("Admin");
            RolesSelection.Add("User");

            NewUserRole.ItemsSource = RolesSelection;
            NewAdminRole.ItemsSource = RolesSelection;

            query = "SELECT Name FROM Logins WHERE Access =  1";
            GetUsers(1);

            query = "SELECT Name FROM Logins WHERE Access =  2";
            GetUsers(2);
        }

        private void GetUsers(int AdminUserFlag)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;

                DataTable UserTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    adapter.Fill(UserTable);

                string[] UsersList = new string[UserTable.Rows.Count];

                if (UserTable.Rows.Count > 0)
                {
                    int UserCount = 0;

                    foreach (DataRow row in UserTable.Rows)
                    {
                        string strName = row.ItemArray[0].ToString();
                        UsersList.SetValue(strName, UserCount);
                        UserCount++;
                    }
                }

                DisplayUsers(UsersList, AdminUserFlag);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void DisplayUsers(string[] UsersList, int AdminUserFlag)
        {
            switch (AdminUserFlag)
            {
                case 1 when UsersList.Length > 0:
                    UserList.ItemsSource = UsersList;
                    NewUser.ItemsSource = UsersList;
                    break;

                case 2 when UsersList.Length > 0:
                    ApproverList.ItemsSource = UsersList;
                    NewAdmin.ItemsSource = UsersList;
                    break;

                case 1 when UsersList.Length == 0:
                    List<string> EmptyUserList = new List<string>();
                    EmptyUserList.Add("No Users found");
                    UserList.ItemsSource = EmptyUserList;
                    NewUser.ItemsSource = EmptyUserList;
                    break;

                case 2 when UsersList.Length == 0:
                    List<string> EmptyAdminList = new List<string>();
                    EmptyAdminList.Add("No Admins found");
                    ApproverList.ItemsSource = EmptyAdminList;
                    NewAdmin.ItemsSource = EmptyAdminList;
                    break;
            }
            
        }

        private void Admin_Change(object sender, RoutedEventArgs e)
        {
            NewAdmin.SelectedIndex = ApproverList.SelectedIndex;
            NewAdminRole.SelectedIndex = 0;
        }

        private void User_Change(object sender, RoutedEventArgs e)
        {
            NewUser.SelectedIndex = UserList.SelectedIndex;
            NewUserRole.SelectedIndex = 1;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            Calendar calendar = new Calendar(user);
            this.Close();
            calendar.Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            string msg = "Do you want to logout?";
            MessageBoxResult result =
              MessageBox.Show(
                msg,
                "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ClosingBypass = true;
                this.Close();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Admin");
        }

        private void UpdateAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update Admin");
        }

        private void DeleteAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete Admin");
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add User");
        }

        private void UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update User");
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete User");
        }


        private void AdminWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ClosingBypass == false)
            {
                string msg = "Do you want to close the application?";
                MessageBoxResult result =
                  MessageBox.Show(
                    msg,
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = false;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Close();
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }

    
}
