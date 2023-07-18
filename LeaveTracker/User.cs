using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Windows;
using System.Data.Common;

namespace LeaveTracker
{
    public class User
    {
        [Display(Order = 0)]
        public string Username { get; set; }

        [Display(Order = 1)]
        public string Password { get; set; }

        [Display(Order = 2)]
        public string Name { get; set; }

        [Display(Order = 3)]
        public int Access { get; set; }

        [Display(Order = 4)]
        public int LeaveCount { get; set; }

        [Display(Order = 5)]
        public int TotalLeave { get; set; }

        public User GetUserData(SqlCommand sqlCommand, string query)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            User user = new User();

            try
            {
                //string query = "SELECT * FROM Logins WHERE UserName = @UserName AND Password = @Password";
                //SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                //sqlCommand.Parameters.AddWithValue("UserName", UserData.Username);
                //sqlCommand.Parameters.AddWithValue("Password", UserData.Password);

                using (DbDataReader reader = sqlCommand.ExecuteReader())
                {
                    //if (!reader.HasRows)
                    //{
                    //    UsernameError.Text = "User not found";
                    //    UsernameError.TextAlignment = TextAlignment.Right;
                    //    UsernameError.Foreground = Brushes.Red;
                    //    UserData.Name = "";
                    //}
                    //else
                    if (reader.HasRows) 
                    {
                        reader.Read();
                        int UsernameIndex = reader.GetOrdinal("UserName");
                        user.Username = reader.GetString(UsernameIndex);
                        int PasswordIndex = reader.GetOrdinal("Password");
                        user.Password = reader.GetString(PasswordIndex);
                        int NameIndex = reader.GetOrdinal("Name");
                        user.Name = reader.GetString(NameIndex);
                        int AccessIndex = reader.GetOrdinal("Access");
                        user.Access = Convert.ToInt16(reader.GetValue(AccessIndex));
                        int TotLeaveIndex = reader.GetOrdinal("TotalLeave");
                        user.LeaveCount = Convert.ToInt16(reader.GetValue(TotLeaveIndex));
                        int RemainLeaveIndex = reader.GetOrdinal("LeaveCount");
                        user.TotalLeave = Convert.ToInt16(reader.GetValue(RemainLeaveIndex));
                        int LastUpdateIndex = reader.GetOrdinal("LeaveCount");
                    }
                }
                return user;
            }
            catch (SqlException e)
            {
                //if (e.Number == 100)
                //{
                //    UsernameError.Text = "User not found";
                //    UsernameError.TextAlignment = TextAlignment.Right;
                //    UsernameError.Foreground = Brushes.Red;
                //}
                //else MessageBox.Show(e.ToString());
                //UserData.Name = "";
                return user;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //UserData.Name = "";
                return user;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        public User[] CheckPendingRegisters()
        {

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            try
            {
                string query = "SELECT * FROM Logins WHERE Access = 0";
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;

                DataTable NewUsersTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                    adapter.Fill(NewUsersTable);

                if (NewUsersTable.Rows.Count == 0)
                {
                    return new User[0];
                }
                else
                {
                    User[] userList = new User[NewUsersTable.Rows.Count];

                    int UCount = 0;
                    foreach (DataRow row in NewUsersTable.Rows)
                    {
                        User ReadUser = new User()
                        {
                            Username = row.ItemArray[1].ToString(), //UserName
                            Password = row.ItemArray[2].ToString(), //Password
                            Name = row.ItemArray[3].ToString(),     //FullName
                            Access = (int)row.ItemArray[4],         //Access
                            LeaveCount = (int)row.ItemArray[5],    //LeaveCount
                            TotalLeave = (int)row.ItemArray[6]     //TotalLeave
                        };
                        userList.SetValue(ReadUser, UCount);
                        UCount++;
                    }
                    return userList;
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
    }
}
