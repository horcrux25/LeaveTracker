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
using System.Web.UI.HtmlControls;
using Microsoft.VisualBasic;

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

            RolesSelection.Clear();
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
            if (NewAdmin.Text == "")
            {
                MessageBox.Show("No admin name entered","Add Admin");
            }
            else if (ApproverList.Items.Contains(NewAdmin.Text))
            {
                MessageBox.Show("Admin already exists", "Add Admin");
            } 
            else if (UserList.Items.Contains(NewAdmin.Text))
            {
                MessageBox.Show("User exists in user list", "Add Admin");
            }
            else
            {
                string msg1 = $"Do you want to add {NewAdmin.Text} as admin?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg1,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result1 == MessageBoxResult.Yes)
                {
                    string AdminPassword = "P@ssword123";
                    string msg2 = $"Default password is P@ssword123. Do you want to change the password for {NewAdmin.Text}'s account?";
                    MessageBoxResult result2 =
                      MessageBox.Show(
                        msg2,
                        "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result2 == MessageBoxResult.Yes)
                    {
                        AdminPassword = Interaction.InputBox("New Password", "New Password");
                    }

                    User newAdmin = new User();
                    newAdmin.Username = NewAdmin.Text;
                    newAdmin.Password = AdminPassword;
                    newAdmin.Name = NewAdmin.Text;
                    newAdmin.Access = 2;
                    newAdmin.LeaveCount = 25;
                    newAdmin.TotalLeave = 25;

                    bool result = AddNewToDB(newAdmin);

                    if (result == true)
                    {
                        MessageBox.Show($"{newAdmin.Username} was added successfully", "Add Admin");

                        query = "SELECT Name FROM Logins WHERE Access =  2";
                        GetUsers(2);
                    }
                    else
                    {
                        MessageBox.Show("Failed to add new user", "Add Admin");
                    }

                }
            }
        }

        private bool AddNewToDB(User NewUserAdmin)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("@UserName", SqlDbType.NVarChar){Value=NewUserAdmin.Username},
                    new SqlParameter("@Password", SqlDbType.NVarChar){Value=NewUserAdmin.Password},
                    new SqlParameter("@Name", SqlDbType.NVarChar){Value=NewUserAdmin.Name},
                    new SqlParameter("@Access", SqlDbType.Int){Value=NewUserAdmin.Access},
                    new SqlParameter("@LeaveCount", SqlDbType.Int){Value=NewUserAdmin.LeaveCount},
                    new SqlParameter("@TotalLeave", SqlDbType.Int){Value=NewUserAdmin.TotalLeave},
                    new SqlParameter("@LastUpdate", SqlDbType.NVarChar){Value=DBNull.Value},
                    new SqlParameter("@UpdateRequest", SqlDbType.Bit){Value=DBNull.Value}
                    };

                string query = "INSERT INTO Logins VALUES (@UserName, @Password, @Name, @Access, @LeaveCount, @TotalLeave, @LastUpdate, @UpdateRequest)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddRange(parameters.ToArray());
                DataTable AddNewTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    adapter.Fill(AddNewTable);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private bool DeleteFromDB(User DeleteUserAdmin)
        {
            try
            {
                string query = "DELETE FROM Logins WHERE @Name = Name AND @Access = Access";

                List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("@Name", SqlDbType.NVarChar){Value=DeleteUserAdmin.Name},
                    new SqlParameter("@Access", SqlDbType.Int){Value=DeleteUserAdmin.Access},
                    };

                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Parameters.AddRange(parameters.ToArray());
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                adapter.DeleteCommand = sqlCommand;
                adapter.DeleteCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void UpdateAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (NewAdmin.Text == "")
            {
                MessageBox.Show("No admin name selected", "Update Admin");
            }
            else if (!ApproverList.Items.Contains(NewAdmin.Text))
            {
                MessageBox.Show("Admin not found", "Updaate Admin");
            }
            else
            {
                string msg1 = $"Do you want to update {NewAdmin.Text} profile?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg1,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result1 == MessageBoxResult.Yes)
                {

                }
            }
        }

        private void DeleteAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (NewAdmin.Text == "")
            {
                MessageBox.Show("No admin name selected", "Delete Admin");
            }
            else
            {
                string msg = $"Do you want to delete {NewAdmin.Text} from admin list?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result1 == MessageBoxResult.Yes)
                {
                    User AdminToDelete = new User();
                    AdminToDelete.Name = NewAdmin.Text;
                    AdminToDelete.Access = 2;
                    bool result = DeleteFromDB(AdminToDelete);

                    if (result == true)
                    {
                        MessageBox.Show($"{AdminToDelete.Name} was removed from admin list.", "Delete Admin");

                        query = "SELECT Name FROM Logins WHERE Access =  2";
                        GetUsers(2);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to remove {AdminToDelete.Name} from admin list.", "Delete Admin");
                    }
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (NewUser.Text == "")
            {
                MessageBox.Show("No user name entered", "Add User");
            }
            else if (ApproverList.Items.Contains(NewUser.Text))
            {
                MessageBox.Show("User exists in Admin list", "Add User");
            }
            else if (UserList.Items.Contains(NewUser.Text))
            {
                MessageBox.Show("User already exists", "Add User");
            }
            else
            {
                string msg = $"Do you want to add {NewUser.Text} as a user?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result1 == MessageBoxResult.Yes)
                {
                    string UserPassword = "P@ssword123";
                    string msg2 = $"Default password is P@ssword123. Do you want to change the password for {NewUser.Text}'s account?";
                    MessageBoxResult result2 =
                      MessageBox.Show(
                        msg2,
                        "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result2 == MessageBoxResult.Yes)
                    {
                        UserPassword = Interaction.InputBox("New Password", "New Password");
                    }

                    User newAdmin = new User();
                    newAdmin.Username = NewUser.Text;
                    newAdmin.Password = UserPassword;
                    newAdmin.Name = NewUser.Text;
                    newAdmin.Access = 1;
                    newAdmin.LeaveCount = 25;
                    newAdmin.TotalLeave = 25;

                    bool result = AddNewToDB(newAdmin);

                    if (result == true)
                    {
                        MessageBox.Show($"{newAdmin.Username} was added successfully", "Add User");

                        query = "SELECT Name FROM Logins WHERE Access =  1";
                        GetUsers(1);

                    }
                    else
                    {
                        MessageBox.Show("Failed to add new user","Add User");
                    }
                }
            }
        }

        private void UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update User");
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (NewUser.Text == "")
            {
                MessageBox.Show("No user name selected", "Delete User");
            }
            else
            {
                string msg = $"Do you want to delete {NewUser.Text} from the users list?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result1 == MessageBoxResult.Yes)
                {
                    User AdminToDelete = new User();
                    AdminToDelete.Name = NewUser.Text;
                    AdminToDelete.Access = 1;
                    bool result = DeleteFromDB(AdminToDelete);

                    if (result == true)
                    {
                        MessageBox.Show($"{AdminToDelete.Name} was removed from user list.", "Delete User");

                        query = "SELECT Name FROM Logins WHERE Access =  1";
                        GetUsers(1);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to remove {AdminToDelete.Name} from admin list.", "Delete Admin");

                    }
                }
            }
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
