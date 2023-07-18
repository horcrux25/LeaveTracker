using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        SqlConnection sqlConnection;
        bool ClosingBypass = false;
        User user = new User();
        int Access = 0;

        public Profile(User user, int Access)
        {
            InitializeComponent();
            
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);

            this.user = user;
            this.Access = Access;

            LoadProfileData();
        }

        private void LoadProfileData()
        {
            if (user == null)
            {
                MessageBox.Show("Profile not found","Warning",MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string AccessToShow = "";
                switch (user.Access)
                {
                    case 1:
                        AccessToShow = "Member";
                        break;

                    case 2:
                    case 3:
                        AccessToShow = "Admin";
                        break;
                }

                ProfileUserName.Text = user.Username;
                ProfilePassword.Text = user.Password;
                ProfileName.Text = user.Name;
                ProfileAccess.Text = AccessToShow;
                ProfileYearlyLeave.Text = user.TotalLeave.ToString();
                ProfileLeaveRemain.Text = user.LeaveCount.ToString();
            }
        }

        private void Profile_WindowClosing(object sender, CancelEventArgs e)
        {
            if (ClosingBypass == false)
            {
                string message = "Do you want to close the application?";
                MessageBoxResult result = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = false;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Close();
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;

            if (Access == 1)
            {
                this.Close();

                foreach (Window window in App.Current.Windows)
                {
                    if (!window.IsActive && window.Title != "MainWindow") window.Show();
                }
            }
            else
            {
                Calendar calendarWindow = new Calendar(user);
                this.Close();
                calendarWindow.Show();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool Result1 = CheckExistingRequest();

            if (Result1 == true)
            {
                MessageBox.Show("You have an existing request. Please settle it with admin first", "Existing request");
            }
            else
            {
                TextBox[] ChangeTextBox = new TextBox[6]
                {
                ProfileUserName,
                ProfilePassword,
                ProfileName,
                ProfileAccess,
                ProfileYearlyLeave,
                ProfileLeaveRemain
                };

                PropertyInfo[] info = user.GetType().GetProperties();

                bool HaveChanges = false;
                bool ValidatePass = false;
                int LeaveRemainConvert = 0;
                int YearlyLeaveConvert = 0;

                for (int i = 0; i < ChangeTextBox.Length; i++)
                {
                    if (ChangeTextBox[i].Text != info[i].GetValue(user))
                    {
                        if (info[i].Name.Contains("Leave"))
                        {
                            if (ChangeTextBox[i].Text != info[i].GetValue(user).ToString())
                            {
                                HaveChanges = true;
                            }
                        }
                        else if (info[i].Name == "Password")
                        {
                            ValidatePass = true;
                            HaveChanges = true;
                        }
                        else if (info[i].Name != "Access")
                        {
                            HaveChanges = true;
                        }
                    }
                }

                try
                {
                    LeaveRemainConvert = int.Parse(ProfileLeaveRemain.Text);
                }
                catch
                {
                    MessageBox.Show("Remaining leave count is not numeric");
                    HaveChanges = false;
                }

                try
                {
                    YearlyLeaveConvert = int.Parse(ProfileYearlyLeave.Text);
                }
                catch
                {
                    MessageBox.Show("Yearly leave count is not numeric");
                    HaveChanges = false;
                }

                if (HaveChanges == true)
                {
                    string message = "Do you want to save your changes?";
                    MessageBoxResult result = MessageBox.Show(message, "Save", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    while (true)
                    {
                        if (result == MessageBoxResult.Yes)
                        {
                            if (ValidatePass == true)
                            {
                                bool ValidPass = CheckPasswordValid();
                                if (ValidPass == false) break;
                            }

                            bool Result2 = InsertUpdate();
                            if (Result2 == false) break;

                            bool Result3 = UpdateRequest();
                            if (Result3 == true)
                            {
                                MessageBox.Show("Profile update has been requested", "Update Profile");
                            }
                            else
                            {
                                DeleteRequest();
                            }
                            break;
                        }
                        else break;
                    }
                }
            }

        }

        private bool CheckPasswordValid()
        {
            string SpecialCharacters = "`~!@#$%^&*()_+-=|,./;':<>[]";

            bool NumberCheck = ProfilePassword.Text.Any(n => char.IsDigit(n));
            bool SpecialCharCheck = SpecialCharacters.Where(x => ProfilePassword.Text.Contains(x)).Any();
            bool CapitalLetterCheck = ProfilePassword.Text.Any(char.IsUpper);
            bool SmallLetterCheck = ProfilePassword.Text.Any(char.IsLower);

            if (NumberCheck && SpecialCharCheck && CapitalLetterCheck && SmallLetterCheck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckExistingRequest()
        {
            try
            {
                string query = "SELECT * FROM Logins WHERE UserName = @UserName AND UpdateRequest = @true";
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("UserName", user.Username);
                sqlCommand.Parameters.AddWithValue("true", true);

                using (DbDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
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

        private bool InsertUpdate()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>() {
                new SqlParameter("@UserName", SqlDbType.NVarChar){Value=ProfileUserName.Text},
                new SqlParameter("@Password", SqlDbType.NVarChar){Value=ProfilePassword.Text},
                new SqlParameter("@Name", SqlDbType.NVarChar){Value=ProfileName.Text},
                new SqlParameter("@Access", SqlDbType.Int){Value=user.Access},
                new SqlParameter("@LeaveCount", SqlDbType.Int){Value=int.Parse(ProfileLeaveRemain.Text)},
                new SqlParameter("@TotalLeave", SqlDbType.Int){Value=int.Parse(ProfileYearlyLeave.Text)},
                new SqlParameter("@LastUpdate", SqlDbType.NVarChar){Value=user.Name},
                new SqlParameter("@UpdateRequest", SqlDbType.Bit){Value=false}};

                string query = "INSERT INTO Logins VALUES (@UserName, @Password, @Name, @Access, @LeaveCount, @TotalLeave, @LastUpdate, @UpdateRequest)";
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

        private bool UpdateRequest()
        {
            try
            {
                string query = "UPDATE Logins SET UpdateRequest = @true WHERE @UserName = UserName AND @Password = Password AND @Name = Name";
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter adapter = new SqlDataAdapter();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("true", true);
                sqlCommand.Parameters.AddWithValue("UserName", user.Username);
                sqlCommand.Parameters.AddWithValue("Password", user.Password);
                sqlCommand.Parameters.AddWithValue("Name", user.Name);
                adapter.UpdateCommand = sqlCommand;
                adapter.UpdateCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
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

        private void DeleteRequest()
        {
            try
            {
                string query = "DELETE FROM Logins WHERE @Name = Name AND UpdateRequest = @true";

                List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("Name", SqlDbType.NVarChar){Value=user.Name},
                    new SqlParameter("true", SqlDbType.Bit){Value=true}
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
    }
}
