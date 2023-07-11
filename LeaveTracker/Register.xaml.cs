using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        //Holds connection to DB
        SqlConnection sqlConnection;

        bool ClosingBypass = false;
        bool AdminView = false;
        User owner = new User();
        List<User> UserList = new List<User>();

        public Register()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);

            NewUsername.Focus();
        }

        public Register(User[] userList, User user)
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);

            //Setup Admin buttons and view
            owner = user;
            AdminView = true;
            NewPassword1.Background = Brushes.Gray;
            NewPassword2.Background = Brushes.Gray;
            NewPassword1.IsEnabled = false;
            NewPassword2.IsEnabled = false;
            NewRegister.ToolTip = "Approve as user";
            Title.Text = "New Pending Users";
            NewRegister.Content = "Approve";
            NewCancel.Content = "Reject";
            Next.Visibility = Visibility.Visible;
            Previous.Visibility = Visibility.Visible;
            Previous.IsEnabled = false;
            Complete.Visibility = Visibility.Visible;
            Next.IsEnabled = userList.Length == 0 ? false : true;

            foreach (User users in userList)
            {
                UserList.Add(users);
            }

            ShowData(userList,0);
        }

        private void ShowData(User[] userList,int userIndex)
        {
            if (userList.Length == 0)
            {
                NewUsername.Text = "";
                NewPassword1.Password = "";
                NewPassword2.Password = "";
                NewFullName.Text = "";
                Previous.IsEnabled = false;
                Next.IsEnabled = false;
                MessageBox.Show("No more pending users","New Users");
            }
            else
            {
                NewUsername.Text = userList[userIndex].Username;
                NewPassword1.Password = userList[userIndex].Password;
                NewPassword2.Password = userList[userIndex].Password;
                NewFullName.Text = userList[userIndex].Name;
            }

            int userCurrent = UserList.FindIndex(x => x.Name == NewFullName.Text);
            if (userCurrent == UserList.Count - 1)
            {
                Next.IsEnabled = false;
            }
            else if (userCurrent < UserList.Count - 1)
            {
                Next.IsEnabled = true;
            }

            if (userCurrent <= 0)
            {
                Previous.IsEnabled = false;
            }
            else
            {
                Previous.IsEnabled = true;
            }
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            Calendar CalendarWindow = new Calendar(owner);
            this.Visibility = Visibility.Hidden;
            CalendarWindow.ShowDialog();
        }

        private void NewCancel_Click(object sender, RoutedEventArgs e)
        {
            if (AdminView)
            {
                bool RejectResult = RejectNewUser();

                if (RejectResult == true)
                {
                    int userCurrent = UserList.FindIndex(x => x.Name == NewFullName.Text);
                    UserList.RemoveAt(userCurrent);

                    if (UserList.Count - 1 <= 0)
                    {
                        ShowData(UserList.ToArray(), 0);
                        Previous.IsEnabled = false;
                    }
                    else
                    {
                        if (userCurrent == UserList.Count)
                        {
                            ShowData(UserList.ToArray(), userCurrent - 1);
                            Next.IsEnabled = false;
                        }
                        else
                        {
                            ShowData(UserList.ToArray(), userCurrent);
                        }
                    }
                }
            }
            else
            {
                ClosingBypass = true;
                this.Close();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void NewRegister_Click(object sender, RoutedEventArgs e)
        {
            if (AdminView)
            {
                bool ApproveResult = ApprovePendingUser();

                if (ApproveResult == true)
                {
                    int userCurrent = UserList.FindIndex(x => x.Name == NewFullName.Text);
                    UserList.RemoveAt(userCurrent);

                    if (UserList.Count - 1 <= 0)
                    {
                        ShowData(UserList.ToArray(), 0);
                        Previous.IsEnabled = false;
                    }
                    else
                    {
                        if (userCurrent == UserList.Count)
                        {
                            ShowData(UserList.ToArray(), userCurrent - 1);
                            Next.IsEnabled = false;
                        }
                        else
                        {
                            ShowData(UserList.ToArray(), userCurrent);
                        }
                    }
                }
            }
            else
            {
                RegisterNewProcess(); 
            }
        }

        private bool RejectNewUser()
        {
            try
            {
                string query = "UPDATE Logins SET Access = 9, LastUpdate = @Admin  WHERE @UserName = UserName AND @Password = Password AND @Name = Name";
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("@Admin", owner.Name);
                sqlCommand.Parameters.AddWithValue("UserName", NewUsername.Text);
                sqlCommand.Parameters.AddWithValue("Password", NewPassword1.Password.ToString());
                sqlCommand.Parameters.AddWithValue("Name", NewFullName.Text);
                adapter.UpdateCommand = sqlCommand;
                adapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                MessageBox.Show("User rejected","New Users");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private bool ApprovePendingUser()
        {
            try
            {
                string query = "UPDATE Logins SET Access = 1, LastUpdate = @Admin WHERE @UserName = UserName AND @Password = Password AND @Name = Name";
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("@Admin", owner.Name);
                sqlCommand.Parameters.AddWithValue("UserName", NewUsername.Text);
                sqlCommand.Parameters.AddWithValue("Password", NewPassword1.Password.ToString());
                sqlCommand.Parameters.AddWithValue("Name", NewFullName.Text);
                adapter.UpdateCommand = sqlCommand;
                adapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                MessageBox.Show("Approved successfully","New Users");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void RegisterNewProcess()
        {
            bool ErrorFlag = false;

            TextBox[] RegisterTextBox = new TextBox[] { NewUsername, NewFullName };
            PasswordBox[] RegisterPasswordBox = new PasswordBox[] { NewPassword1, NewPassword2 };
            TextBlock[] RegisterTextBlock = new TextBlock[] { NewUsernameError, NewFullNameError };
            TextBlock[] RegisterPaswordBlock = new TextBlock[] { NewPassword1Error, NewPassword2Error };

            NewUsernameError.Text = "";
            NewPassword1Error.Text = "";
            NewPassword2Error.Text = "";
            NewFullNameError.Text = "";

            int TBIndex = 0;
            foreach (TextBox box in RegisterTextBox)
            {
                if (box.Text == "")
                {
                    RegisterTextBlock[TBIndex].Foreground = Brushes.Red;
                    RegisterTextBlock[TBIndex].TextAlignment = TextAlignment.Right;
                    ErrorFlag = true;

                    if (TBIndex == 0)
                    {
                        RegisterTextBlock[TBIndex].Text = string.Format("Invalid username");
                    }
                    else
                    {
                        RegisterTextBlock[TBIndex].Text = string.Format("Invalid Full Name");
                    }
                }
                TBIndex++;
            }

            int PWIndex = 0;
            foreach (PasswordBox box in RegisterPasswordBox)
            {
                if (box.Password.ToString() == "")
                {
                    RegisterPaswordBlock[PWIndex].Foreground = Brushes.Red;
                    RegisterPaswordBlock[PWIndex].TextAlignment = TextAlignment.Right;
                    RegisterPaswordBlock[PWIndex].Text = string.Format("Invalid password");
                    ErrorFlag = true;
                }
                PWIndex++;
            }

            if (ErrorFlag == false)
            {
                if (CheckPasswordValid() == true)
                {
                    if (CheckPasswordSame() == true)
                    {
                        bool successRegister = AddNewToDB();
                        if (successRegister == true)
                        {
                            MessageBox.Show("Registered successfully","Register");
                            ClosingBypass = true;
                            this.Close();
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                        }
                    }
                    else
                    {
                        NewPassword2Error.Text = string.Format("Passwords do not match");
                        NewPassword2Error.Foreground = Brushes.Red;
                        NewPassword2Error.TextAlignment = TextAlignment.Right;
                    }
                }
                else
                {
                    NewPassword1Error.Text = string.Format("Password does not meet criteria");
                    NewPassword1Error.Foreground = Brushes.Red;
                    NewPassword1Error.TextAlignment = TextAlignment.Right;
                }
            }
        }

        private bool CheckPasswordValid()
        {
            string SpecialCharacters = "`~!@#$%^&*()_+-=|,./;':<>[]";

            bool NumberCheck = NewPassword1.Password.ToString().Any(n => char.IsDigit(n));
            bool SpecialCharCheck = SpecialCharacters.Where(x => NewPassword1.Password.ToString().Contains(x)).Any();
            bool CapitalLetterCheck = NewPassword1.Password.ToString().Any(char.IsUpper);
            bool SmallLetterCheck = NewPassword1.Password.ToString().Any(char.IsLower);

            if (NumberCheck && SpecialCharCheck && CapitalLetterCheck && SmallLetterCheck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckPasswordSame()
        {
            if (NewPassword1.Password.ToString() != NewPassword2.Password.ToString())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void RegisterWindow_Closing(object sender, CancelEventArgs e)
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

        private bool AddNewToDB()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>() {
                new SqlParameter("@UserName", SqlDbType.NVarChar){Value=NewUsername.Text},
                new SqlParameter("@Password", SqlDbType.NVarChar){Value=NewPassword1.Password.ToString()},
                new SqlParameter("@Name", SqlDbType.NVarChar){Value=NewFullName.Text},
                new SqlParameter("@Access", SqlDbType.NVarChar){Value=0},
                new SqlParameter("@LeaveCount", SqlDbType.NVarChar){Value=25},
                new SqlParameter("@TotalLeave", SqlDbType.NVarChar){Value=25},
                new SqlParameter("@LastUpdate", SqlDbType.NVarChar){Value=DBNull.Value}};

                string query = "INSERT INTO Logins VALUES (@UserName, @Password, @Name, @Access, @LeaveCount, @TotalLeave, @LastUpdate)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddRange(parameters.ToArray());
                DataTable RegisterNewTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand)) 
                    adapter.Fill(RegisterNewTable);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            int userCurrent = UserList.FindIndex(x => x.Name == NewFullName.Text);
            if (userCurrent < UserList.Count - 1)
            {
                ShowData(UserList.ToArray(), userCurrent + 1);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            int userCurrent = UserList.FindIndex(x => x.Name == NewFullName.Text);
            if (userCurrent > 0)
            {
                ShowData(UserList.ToArray(), userCurrent - 1);
            }
        }

        private void Register_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NewRegister_Click(sender, e);
            }
        }
    }
}
