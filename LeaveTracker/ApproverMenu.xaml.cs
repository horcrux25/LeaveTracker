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
using System.Data.Common;
using static LeaveTracker.ApproverMenu;
using System.Globalization;
using System.Net.NetworkInformation;
using static LeaveTracker.User;

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
        bool SelectComboBox = false;
        User user = new User();
        User userTemp = new User();

        List<string> RolesSelection = new List<string>();

        public struct Leave
        {
            public string LeaveName { get; set; }
            public DateTime LeaveDate { get; set; }
        }

        List<Leave> leaves = new List<Leave>();

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

            User[] usersList = user.CheckPendingRegisters();
            if (usersList.Length != 0)
            {
                MainMenu.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                MainMenu.Background = default;
            }

            string[] UserListResult;

            query = "SELECT Name FROM Logins WHERE Access =  1";
            UserListResult = user.GetUsers(query, 1);
            DisplayUsers(UserListResult, 1);

            query = "SELECT Name FROM Logins WHERE Access =  2";
            UserListResult = user.GetUsers(query, 2);
            DisplayUsers(UserListResult, 2);

            DefaultSettings defautSettings = user.GetDefault();
            DisplayDefaultSettings(defautSettings);

            query = "SELECT Name, LeaveDate FROM Leave WHERE ApprovalFlag = 0";
            Leave[] Leaves = GetLeaves();
            leaves = Leaves.ToList();
            DisplayLeaves(Leaves, 0);
            if (leaves.Count != 0)
            {
                LeaveMenu.Background = new SolidColorBrush(Colors.Green);
                NewProfileButton.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                LeaveMenu.Background = default;
                NewProfileButton.Background = default;
            }

            Populate_TextBlocks();
        }

        private void Shown_Event(object sender, DependencyPropertyChangedEventArgs e)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connectionString);

            RolesSelection.Clear();
            RolesSelection.Add("Admin");
            RolesSelection.Add("User");

            NewUserRole.ItemsSource = RolesSelection;
            NewAdminRole.ItemsSource = RolesSelection;

            User[] usersList = user.CheckPendingRegisters();
            if (usersList.Length != 0)
            {
                MainMenu.Background = new SolidColorBrush(Colors.Green);
                NewProfileButton.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                MainMenu.Background = default;
                NewProfileButton.Background = default;
            }

            string[] UserListResult;

            query = "SELECT Name FROM Logins WHERE Access = 1 AND UpdateRequest IS NULL";
            UserListResult = user.GetUsers(query, 1);
            DisplayUsers(UserListResult, 1);

            query = "SELECT Name FROM Logins WHERE Access = 2 AND UpdateRequest IS NULL";
            UserListResult = user.GetUsers(query, 2);
            DisplayUsers(UserListResult, 2);

            DefaultSettings defautSettings = user.GetDefault();
            DisplayDefaultSettings(defautSettings);

            query = "SELECT Name, LeaveDate FROM Leave WHERE ApprovalFlag = 0";
            Leave[] Leaves = GetLeaves();
            leaves = Leaves.ToList();
            DisplayLeaves(Leaves, 0);
            if (leaves.Count != 0)
            {
                LeaveMenu.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                LeaveMenu.Background = default;
            }

            Populate_TextBlocks();
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

        private void DisplayDefaultSettings(DefaultSettings defaultSettings)
        {
            StartYearDate.SelectedDate = defaultSettings.CalendarStartDay;
            DefaultLeaves.Text = defaultSettings.DefaultYearlyLeave.ToString();
            DefaultPassword.Text = defaultSettings.DefaultPassword;
        }

        private Leave[] GetLeaves()
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;

                DataTable LeaveTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    adapter.Fill(LeaveTable);

                if (LeaveTable.Rows.Count != 0)
                {
                    Leave[] LeaveList = new Leave[LeaveTable.Rows.Count];

                    int LeaveCount = 0;
                    foreach (DataRow row in LeaveTable.Rows)
                    {
                        Leave ReadLeave = new Leave()
                        {
                            LeaveName = row.ItemArray[0].ToString(),                //Name
                            LeaveDate = DateTime.Parse(row.ItemArray[1].ToString()) //Date

                            //LeaveDate = row.ItemArray[1].ToString()  //Date
                        };
                        LeaveList.SetValue(ReadLeave, LeaveCount);
                        LeaveCount++;
                    }
                    return LeaveList;
                }
                else
                {
                    return new Leave[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return new Leave[0];
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void DisplayLeaves(Leave[] leaveArray, int index)
        {
            LeaveNames.ItemsSource = leaveArray.Select(x => x.LeaveName).Distinct().ToList();
            LeaveNames.SelectedIndex = index;

            if (LeaveNames.Text != "")
            {
                DateTime[] LeaveDates = leaveArray.Where(x => x.LeaveName == LeaveNames.Text).Select(t => t.LeaveDate).ToArray();


                string[] StrDate = new string[LeaveDates.Length];
                int LeaveCount = 0;
                foreach (DateTime date in LeaveDates)
                {
                    StrDate[LeaveCount] = date.ToString("d", CultureInfo.InvariantCulture);
                    LeaveCount++;
                }

                LeaveList.ItemsSource = StrDate;
            }
            else
            {
                LeaveList.ClearValue(UidProperty);
            }
        }

        private User[] GetProfileRequests()
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                SqlParameter para1 = new SqlParameter("True", SqlDbType.Bit) { Value = 1 };
                SqlParameter para2 = new SqlParameter("False", SqlDbType.Bit) { Value = 0 };
                sqlCommand.Parameters.Add(para1);
                sqlCommand.Parameters.Add(para2);

                DataTable ProfileTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    adapter.Fill(ProfileTable);

                if (ProfileTable.Rows.Count != 0)
                {
                    User[] ProfileList = new User[ProfileTable.Rows.Count];

                    int ProfileCount = 0;
                    foreach (DataRow row in ProfileTable.Rows)
                    {
                        User ReadProfile = new User()
                        {
                            Username = row.ItemArray[0].ToString(),     
                            Password = row.ItemArray[1].ToString(),
                            Name = row.ItemArray[2].ToString(),
                            Access = (int)row.ItemArray[3],
                            LeaveCount = (int)row.ItemArray[4],
                            TotalLeave = (int)row.ItemArray[5],
                            UpdateRequest = (bool)row.ItemArray[6],
                            Id = (int)row.ItemArray[7],
                            LastUpdate = row.ItemArray[8].ToString()
                        };
                        ProfileList.SetValue(ReadProfile, ProfileCount);
                        ProfileCount++;
                    }
                    return ProfileList;
                }
                else
                {
                    return new User[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return new User[0];
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void Populate_TextBlocks()
        {
            query = "SELECT UserName, Password, Name, Access, LeaveCount, TotalLeave, UpdateRequest, Id, LastUpdate FROM Logins WHERE UpdateRequest = @True OR UpdateRequest = @False";
            User[] ProfileChangeList = GetProfileRequests();

            if (ProfileChangeList.Length != 0)
            {
                ProfileMenu.Background = new SolidColorBrush(Colors.Green);
                DisplayProfileRequests(ProfileChangeList);
            }
            else
            {
                ProfileMenu.Background = default;
                DisplayProfileRequestsEmpty();
            }
        }

        private void DisplayProfileRequests(User[] ProfileLists)
        {
            string[] UniqueNameList = ProfileLists.Where(x => x.UpdateRequest == true).Select(x => x.Name).Distinct().ToArray();
            ProfileUpdateList.ItemsSource = UniqueNameList;
            if (ProfileLists.Length != 0)
            {
                if (SelectComboBox == false) 
                {
                    ProfileUpdateList.SelectedIndex = 0;
                }
                DisplayProfileDetails(ProfileLists);
                InitializeProfileTextColors();
                CheckForFieldWithChange();
            }
        }

        private void DisplayProfileRequestsEmpty()
        {
            string[] EmptyNameList = new string[1] { "No pending request" };
            ProfileUpdateList.ItemsSource = EmptyNameList;
            ProfileUpdateList.SelectedIndex = 0;
            InitializeProfileTextBoxes();
            InitializeProfileTextColors();
        }

        private void DisplayProfileDetails (User[] ProfileLists)
        {
            User[] OrigProfiles = ProfileLists.Where(x => x.UpdateRequest == true).Distinct().ToArray();
            User[] RequestProfiles = ProfileLists.Where(x => x.UpdateRequest == false).Distinct().ToArray();

            int OrigAccessProfile = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Access).FirstOrDefault();
            int ProfileId = int.Parse(OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Id).FirstOrDefault().ToString());

            OrigName.Text = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Name).FirstOrDefault().ToString();
            OrigUserName.Text = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Username).FirstOrDefault().ToString();
            OrigPassword.Password = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Password).FirstOrDefault().ToString();
            OrigAccess.Text = OrigAccessProfile == 2 ? "Admin" : OrigAccessProfile == 3 ? "Admin" : "Member";
            OrigTotalLeave.Text = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.TotalLeave).FirstOrDefault().ToString();
            OrigLeaveCount.Text = OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.LeaveCount).FirstOrDefault().ToString();

            int ChangeAccessProfile = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.Access).FirstOrDefault();

            ChangeName.Text = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.Name).FirstOrDefault().ToString();
            ChangeUserName.Text = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.Username).FirstOrDefault().ToString();
            ChangePassword.Password = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.Password).FirstOrDefault().ToString();
            ChangeAccess.Text = ChangeAccessProfile == 2 ? "Admin" : ChangeAccessProfile == 3 ? "Admin" : "Member";
            ChangeTotalLeave.Text = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.TotalLeave).FirstOrDefault().ToString();
            ChangeLeaveCount.Text = RequestProfiles.Where(x => int.Parse(x.LastUpdate) == ProfileId).Select(x => x.LeaveCount).FirstOrDefault().ToString();
        }

        private void InitializeProfileTextColors()
        {
            TextBlock[] ProfileTextBlock = new TextBlock[10]
            {
                OrigName, OrigUserName, OrigAccess, OrigLeaveCount, OrigTotalLeave, 
                ChangeName, ChangeUserName, ChangeAccess, ChangeLeaveCount, ChangeTotalLeave
            };

            for (int i = 0; i < 10; i++)
            {
                ProfileTextBlock[i].Foreground = Brushes.Black;
            }
            OrigPassword.Background = default;
            ChangePassword.Background = default;
        }

        private void InitializeProfileTextBoxes()
        {
            TextBlock[] ProfileTextBlock = new TextBlock[10]
            {
                OrigName, OrigUserName, OrigAccess, OrigLeaveCount, OrigTotalLeave,
                ChangeName, ChangeUserName, ChangeAccess, ChangeLeaveCount, ChangeTotalLeave
            };

            for (int i = 0; i < 10; i++)
            {
                ProfileTextBlock[i].Text = "";
            }
            OrigPassword.Password = default;
            ChangePassword.Password = default;
        }

        private void CheckForFieldWithChange()
        {
            TextBlock[] OrigTextBlock = new TextBlock[5]
            {
                OrigName, OrigUserName, OrigAccess, OrigLeaveCount, OrigTotalLeave
            };

            TextBlock[] ChangeTextBlock = new TextBlock[5]
            {
                ChangeName, ChangeUserName, ChangeAccess, ChangeLeaveCount, ChangeTotalLeave
            };

            for (int i = 0; i < 5; i++)
            {
                if (OrigTextBlock[i].Text != ChangeTextBlock[i].Text)
                {
                    OrigTextBlock[i].Foreground = Brushes.Orange;
                    ChangeTextBlock[i].Foreground = Brushes.Green;
                }
            }

            if (OrigPassword.Password != ChangePassword.Password)
            {
                OrigPassword.Background = Brushes.Orange;
                ChangePassword.Background = Brushes.Green;
            }
        }

        private void SelectProfileChange_Event(object sender, EventArgs e)
        {
            SelectComboBox = true;
            query = "SELECT UserName, Password, Name, Access, LeaveCount, TotalLeave, UpdateRequest, Id, LastUpdate FROM Logins WHERE UpdateRequest = @True OR UpdateRequest = @False";
            User[] ProfileChangeList = GetProfileRequests();

            if (ProfileChangeList.Length != 0)
            {
                DisplayProfileRequests(ProfileChangeList);
                DisplayProfileDetails(ProfileChangeList);
                InitializeProfileTextColors();
                CheckForFieldWithChange();
            }
            SelectComboBox = false;
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
                    User newAdminTemp = new User();
                    newAdminTemp.Name = NewAdmin.Text;
                    newAdminTemp.Password = DefaultPassword.Text;
                    newAdminTemp.Username = NewAdmin.Text;
                    newAdminTemp.Access = 2;
                    newAdminTemp.LeaveCount = int.Parse(DefaultLeaves.Text);
                    newAdminTemp.TotalLeave = int.Parse(DefaultLeaves.Text);

                    Profile profilePage = new Profile(newAdminTemp, 2);
                    this.Hide();
                    profilePage.Show();
                }
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
                    string query = "SELECT * FROM Logins WHERE Name = @Name";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Parameters.AddWithValue("Name", NewAdmin.Text);

                    userTemp = user.GetUserData(sqlCommand, query);

                    Profile profilePage = new Profile(userTemp, 1);
                    this.Hide();
                    profilePage.Show();
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
                    User newAdminTemp = new User();
                    newAdminTemp.Name = NewUser.Text;
                    newAdminTemp.Password = "P@ssword123";
                    newAdminTemp.Username = NewUser.Text;
                    newAdminTemp.Access = 1;
                    newAdminTemp.LeaveCount = 25;
                    newAdminTemp.TotalLeave = 25;

                    Profile profilePage = new Profile(newAdminTemp, 2);
                    this.Hide();
                    profilePage.Show();
                }
            }
        }

        private void UpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (NewUser.Text == "")
            {
                MessageBox.Show("No user name selected", "Update User");
            }
            else if (!UserList.Items.Contains(NewUser.Text))
            {
                MessageBox.Show("User not found", "Update User");
            }
            else
            {
                string msg1 = $"Do you want to update {NewUser.Text} profile?";
                MessageBoxResult result1 =
                  MessageBox.Show(
                    msg1,
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result1 == MessageBoxResult.Yes)
                {
                    string query = "SELECT * FROM Logins WHERE Name = @Name";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Parameters.AddWithValue("Name", NewUser.Text);

                    userTemp = user.GetUserData(sqlCommand, query);

                    Profile profilePage = new Profile(userTemp, 1);
                    this.Hide();
                    profilePage.Show();
                }
            }
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

        private void LeaveName_Change(object sender, EventArgs e)
        {
            if (LeaveNames.Text != "")
            {
                DateTime[] LeaveDates = leaves.Where(x => x.LeaveName == LeaveNames.Text).Select(t => t.LeaveDate).ToArray();


                string[] StrDate = new string[LeaveDates.Length];
                int LeaveCount = 0;
                foreach (DateTime date in LeaveDates)
                {
                    StrDate[LeaveCount] = date.ToString("d", CultureInfo.InvariantCulture);
                    LeaveCount++;
                }

                LeaveList.ItemsSource = StrDate;
            }
            else
            {
                LeaveList.ClearValue(UidProperty);
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (LeaveList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select date/s first to be rejected ", "Reject leave");
            }
            else
            {
                string[] FailedToRejectDate = new string[LeaveList.SelectedItems.Count];

                for (int i = 0; i < LeaveList.SelectedItems.Count; i++)
                {
                    string query = "UPDATE Leave SET ApprovalFlag = 1, Approver = @Approver, ApproveDate = @ApproveDate WHERE Name = @Name AND LeaveDate = @LeaveDate AND ApprovalFlag = 0";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlConnection.Open();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Parameters.AddWithValue("Approver", user.Name);
                    sqlCommand.Parameters.AddWithValue("ApproveDate", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("Name", LeaveNames.Text);
                    sqlCommand.Parameters.AddWithValue("LeaveDate", LeaveList.SelectedItems[i]);
                    sqlCommand.CommandText = query;

                    bool result = UpdateLeaveTable(sqlCommand);

                    if (result == false)
                    {
                        FailedToRejectDate[i] = LeaveList.SelectedItems[i].ToString();
                    }
                }
                FailedToRejectDate = FailedToRejectDate.OrderBy(x => x).ToArray();
                bool EmptyArray = FailedToRejectDate.All(x => x == null);

                if (EmptyArray == false)
                {
                    MessageBox.Show("Failed to reject the following date/s:\n" + string.Join(",\n", FailedToRejectDate),"Failed to Reject Leave Dates");
                }
                else
                {
                    MessageBox.Show("Successfully rejected the selected date/s","Reject Leave Dates");
                }

                string tempUser = LeaveNames.Text;
                query = "SELECT Name, LeaveDate FROM Leave WHERE ApprovalFlag = 0";
                Leave[] Leaves = GetLeaves();
                leaves = Leaves.ToList();
                
                if (LeaveNames.FindName(tempUser) != null)
                {
                    int index = LeaveNames.SelectedIndex = LeaveNames.Items.IndexOf(tempUser);
                    DisplayLeaves(Leaves, index);
                }
                else
                {
                    DisplayLeaves(Leaves,0);
                }
            }
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (LeaveList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select date/s to be approved", "Approve leave");
            }
            else
            {
                string[] FailedToApproveDate = new string[LeaveList.SelectedItems.Count];

                for (int i = 0; i < LeaveList.SelectedItems.Count; i++)
                {
                    string query = "UPDATE Leave SET ApprovalFlag = 2, Approver = @Approver, ApproveDate = @ApproveDate WHERE Name = @Name AND LeaveDate = @LeaveDate AND ApprovalFlag = 0";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlConnection.Open();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Parameters.AddWithValue("Approver", user.Name);
                    sqlCommand.Parameters.AddWithValue("ApproveDate", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("Name", LeaveNames.Text);
                    sqlCommand.Parameters.AddWithValue("LeaveDate", LeaveList.SelectedItems[i]);
                    sqlCommand.CommandText = query;

                    bool result = UpdateLeaveTable(sqlCommand);

                    if (result == false)
                    {
                        FailedToApproveDate[i] = LeaveList.SelectedItems[i].ToString();
                    }
                    else
                    {
                        UpdateLeaveCount();
                    }
                }
                FailedToApproveDate = FailedToApproveDate.OrderBy(x => x).ToArray();
                bool EmptyArray = FailedToApproveDate.All(x => x == null);

                if (EmptyArray == false)
                {
                    MessageBox.Show("Failed to approve the following date/s:\n" + string.Join(",\n", FailedToApproveDate),"Failed to Approve");
                }
                else
                {
                    MessageBox.Show("Successfully approved the selected date/s","Leave Dates Approved");
                }

                string tempUser = LeaveNames.Text;
                query = "SELECT Name, LeaveDate FROM Leave WHERE ApprovalFlag = 0";
                Leave[] Leaves = GetLeaves();
                leaves = Leaves.ToList();

                if (LeaveNames.FindName(tempUser) != null)
                {
                    int index = LeaveNames.SelectedIndex = LeaveNames.Items.IndexOf(tempUser);
                    DisplayLeaves(Leaves, index);
                }
                else
                {
                    DisplayLeaves(Leaves, 0);
                }
            }
        }

        private bool UpdateLeaveTable(SqlCommand sqlCommand)
        {
            try
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                sqlAdapter.UpdateCommand = sqlCommand;
                sqlAdapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                string datevalue = "";

                foreach(SqlParameter para in sqlCommand.Parameters)
                {
                    if (para.ParameterName == "LeaveDate")
                    {
                        datevalue = para.Value.ToString(); 
                        break;
                    }
                }

                MessageBox.Show(ex.Message + "\nDate: " + datevalue);
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void UpdateLeaveCount()
        {
            query = "UPDATE Logins SET LeaveCount = LeaveCount - 1 WHERE Name = @Name";
            SqlCommand sqlCommand = new SqlCommand();
            sqlConnection.Open();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Parameters.AddWithValue("Name", LeaveNames.Text);
            sqlCommand.CommandText = query;

            try
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                sqlAdapter.UpdateCommand = sqlCommand;
                sqlAdapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void ApproveProfile_Click(object sender, RoutedEventArgs e)
        {
            query = "DELETE FROM Logins WHERE @Name = Name AND UpdateRequest = @true";
            SqlCommand sqlCommand = new SqlCommand();
            DeleteProfile_Login(query, sqlCommand);
            query = "UPDATE Logins SET UpdateRequest = NULL WHERE @NewName = Name";
            UpdateProfile_Login(query);
            
            Populate_TextBlocks();

            MessageBox.Show("Change profile request was successfully approved", "Success");

        }

        private void RejectProfile_Click(object sender, RoutedEventArgs e)
        {
            query = "SELECT UserName, Password, Name, Access, LeaveCount, TotalLeave, UpdateRequest, Id, LastUpdate FROM Logins WHERE UpdateRequest = @True OR UpdateRequest = @False";
            User[] ProfileChangeList = GetProfileRequests();

            User[] OrigProfiles = ProfileChangeList.Where(x => x.UpdateRequest == true).Distinct().ToArray();
            User[] RequestProfiles = ProfileChangeList.Where(x => x.UpdateRequest == false).Distinct().ToArray();

            int ProfileId = int.Parse(OrigProfiles.Where(x => x.Name == ProfileUpdateList.Text).Select(x => x.Id).FirstOrDefault().ToString());

            query = "DELETE FROM Logins WHERE @ProfileId = LastUpdate AND UpdateRequest = @false";
            SqlParameter para = new SqlParameter("ProfileId", SqlDbType.NVarChar) { Value = ProfileId.ToString() };
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.Add(para);
            DeleteProfile_Login(query, sqlCommand);

            query = "UPDATE Logins SET UpdateRequest = NULL WHERE @OldName = Name";
            UpdateProfile_Login(query);

            Populate_TextBlocks();

            MessageBox.Show("Change profile request was rejected", "Reject");
        }

        private void DeleteProfile_Login(string query, SqlCommand sqlCommand)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("Name", SqlDbType.NVarChar){Value=ProfileUpdateList.Text},
                    new SqlParameter("true", SqlDbType.Bit){Value=true},
                    new SqlParameter("false", SqlDbType.Bit){Value=false}
                    };

                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Parameters.AddRange(parameters.ToArray());
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                adapter.DeleteCommand = sqlCommand;
                adapter.DeleteCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void UpdateProfile_Login(string query)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("NewName", ChangeName.Text);
                sqlCommand.Parameters.AddWithValue("OldName", OrigName.Text);
                adapter.UpdateCommand = sqlCommand;
                adapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
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

        private void NewProfile_Click(object sender, RoutedEventArgs e)
        {
            User[] usersList = user.CheckPendingRegisters();
            this.Hide();
            Register RegisterWindow = new Register(usersList, user, 1);
            RegisterWindow.ShowDialog();
        }

        private void SaveDefault_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DefaultLeaves.Text, out int temp) == true)
            {
                string msg = "Do you want to save default values?";
                MessageBoxResult result =
                  MessageBox.Show(
                    msg,
                    "Save Default",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    query = "UPDATE DefaultTable SET CalendarStart = @DefaultCalendarStart, DefaultPassword = @DefaultPassword, DefaultLeaveCount = @DefaultLeaveCount WHERE Id = 1";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlConnection.Open();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Parameters.AddWithValue("DefaultCalendarStart", StartYearDate.SelectedDate);
                    sqlCommand.Parameters.AddWithValue("DefaultPassword", DefaultPassword.Text);
                    sqlCommand.Parameters.AddWithValue("DefaultLeaveCount", DefaultLeaves.Text);
                    sqlCommand.CommandText = query;

                    try
                    {
                        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
                        sqlAdapter.UpdateCommand = sqlCommand;
                        sqlAdapter.UpdateCommand.ExecuteNonQuery();
                        sqlCommand.Dispose();

                        MessageBox.Show("Default values successfully updated", "Success update");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed update.\n" + ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Default yearly leave count is invalid", "Invalid value");
            }
        }
    }
}
