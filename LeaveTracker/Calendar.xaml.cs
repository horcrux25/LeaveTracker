using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
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
using static LeaveTracker.User;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : Window
    {
        SqlConnection sqlConnection;
        User user = new User();
        bool ClosingBypass = false;
        bool CalendarUpdateDone = false;
        string query;

        public Calendar(User user1)
        {
            InitializeComponent();

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);

            CalName.Text = user1.Name;
            CalTotalLeave.Text = user1.TotalLeave.ToString();
            CalLeaveRemain.Text = user1.LeaveCount.ToString();
            user = user1;

            query = "SELECT LeaveDate FROM Leave WHERE @Name = Name AND ApprovalFlag = 1";
            DateTime[] ApprovedLeaves = GetLeaves();
            DisplayApprovedLeaves(ApprovedLeaves);

            CheckUserCalendarUpdate();
            GetRequestLeaveProcess();
        }

        private void CheckUserCalendarUpdate()
        { 
            try
            {
                query = "SELECT * From Logins WHERE Name = @Name";
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.Parameters.AddWithValue("Name", CalName.Text);
                sqlCommand.CommandText = query;

                using (DbDataReader reader = sqlCommand.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        reader.Read();
                        int CalendarUpdateIndex = reader.GetOrdinal("CalendarUpdate");
                        CalendarUpdateDone = reader.GetBoolean(CalendarUpdateIndex);
                    }
                }
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

        private void FiscalStartDateProcess()
        {
            DefaultSettings defautSettings = user.GetDefault();

            if (CalendarUpdateDone == true)
            {
                if (DateTime.Now == defautSettings.CalendarStartDay)
                {
                    query = "UPDATE Logins SET LeaveCount = @LeaveCount, TotalLeave = @TotalLeave, CalendarUpdate = True WHERE Name = @Name";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlConnection.Open();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Parameters.AddWithValue("LeaveCount", defautSettings.DefaultYearlyLeave);
                    sqlCommand.Parameters.AddWithValue("TotalLeave", defautSettings.DefaultYearlyLeave);
                    sqlCommand.Parameters.AddWithValue("Name", CalName.Text);
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
                else
                {
                    query = "UPDATE Logins SET CalendarUpdate = NULL WHERE Name = @Name";
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlConnection.Open();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Parameters.AddWithValue("Name", CalName.Text);
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
            }
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

        private void CalendarWindow_Closing(object sender, CancelEventArgs e)
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

        private void GetRequestLeaveProcess()
        {

            //query = "SELECT FORMAT (LeaveDate, 'MM/dd/yyyy') as dateLeaveDate FROM Leave WHERE @Name = Name AND ApprovalFlag = 0";
            query = "SELECT LeaveDate FROM Leave WHERE @Name = Name AND ApprovalFlag = 0";
            DateTime[] RequestedLeaves = GetLeaves();
            DisplayRequestedLeaves(RequestedLeaves);
        }

        private DateTime[] GetLeaves()
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = query;
                sqlCommand.Parameters.AddWithValue("Name", user.Name);

                DataTable LeaveTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                  adapter.Fill(LeaveTable);

                if (LeaveTable.Rows.Count == 0)
                {
                    return new DateTime[0];
                }
                else
                {
                    DateTime[] Leaves = new DateTime[LeaveTable.Rows.Count];

                    int LeaveCount = 0;

                    foreach (DataRow row in LeaveTable.Rows)
                    {
                        string strDate = row.ItemArray[0].ToString();
                        DateTime ReadDate = DateTime.Parse(strDate);
                        Leaves.SetValue(ReadDate, LeaveCount);
                        LeaveCount++;
                    }
                    return Leaves;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return new DateTime[0];
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void DisplayApprovedLeaves(DateTime[] ApprovedLeaves)
        {
            //ApprovedList.DisplayMemberPath = ApprovedLeaves[0].ToString();
            if (ApprovedLeaves.Length == 0)
            {
                List<string> EmptyList = new List<string>();
                EmptyList.Add("Empty");
                ApprovedList.ItemsSource = EmptyList;
            }
            else
            {
                string[] StrDate = new string[ApprovedLeaves.Length];
                int LeaveCount = 0;
                foreach (DateTime date in ApprovedLeaves)
                {
                    StrDate[LeaveCount] = date.ToString("d", CultureInfo.InvariantCulture);
                    LeaveCount++;
                }

                ApprovedList.ItemsSource = StrDate;
            }
        }

        private void DisplayRequestedLeaves(DateTime[] RequestedLeaves)
        {
            //ApprovedList.DisplayMemberPath = ApprovedLeaves[0].ToString();
            if (RequestedLeaves.Length == 0)
            {
                List<string> EmptyList = new List<string>();
                EmptyList.Add("Empty");
                PendingList.ItemsSource = EmptyList;
            }
            else
            {
                string[] StrDate = new string[RequestedLeaves.Length];
                int LeaveCount = 0;
                foreach (DateTime date in RequestedLeaves)
                {
                    StrDate[LeaveCount] = date.ToString("d", CultureInfo.InvariantCulture);
                    LeaveCount++;
                }

                PendingList.ItemsSource = StrDate;
            }
        }

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            if (DateSelect.Text == "")
            {
                MessageBox.Show("No date selected","Leave Request");
            }
            else
            {
                if (DateTime.Today >= DateTime.Parse(DateSelect.Text))
                {
                    MessageBox.Show("Selected date should be in the future","Leave Request");
                }
                else
                {
                    if (VerifyDateExist() == false)
                    {
                        int ReqType = 1;
                        DateTime ConvertedDate = Convert.ToDateTime(DateSelect.Text);

                        bool requestResult = SaveDate(ConvertedDate, ReqType);

                        if (requestResult == true)
                        {
                            MessageBox.Show(DateTime.Parse(DateSelect.Text).ToString("d", CultureInfo.InvariantCulture) + " is successfully submitted approval");

                            GetRequestLeaveProcess();

                        }
                        else
                        {
                            MessageBox.Show("Request failed");
                        }
                    }
                }
            }
        }

        private bool VerifyDateExist()
        {
            bool DateResult = false;
            string ConvertedDate = DateTime.Parse(DateSelect.Text).ToString("d", CultureInfo.InvariantCulture);


            for (int i = 0; i < PendingList.Items.Count; i++)
            {
                string listDate = PendingList.Items[i].ToString();
                if (listDate.Contains(ConvertedDate))
                {
                    MessageBox.Show("Request date is already existing", "Leave Request");
                    DateResult = true;
                    i = PendingList.Items.Count;
                }
            }

            if (DateResult)
            {
                for (int i = 0; i < ApprovedList.Items.Count; i++)
                {
                    string listDate = ApprovedList.Items[i].ToString();
                    if (listDate.Contains(ConvertedDate))
                    {
                        MessageBox.Show("Request date was already approved", "Leave Request");
                        DateResult = true;
                        i = ApprovedList.Items.Count;
                    }
                }
            }

            return DateResult;
        }

        private bool SaveDate(DateTime RequestDate, int RequestType)
        {
            try
            {
                if (RequestType == 1)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("@Name", SqlDbType.NVarChar){Value=CalName.Text},
                    new SqlParameter("@LeaveDate", SqlDbType.DateTime){Value=RequestDate},
                    new SqlParameter("@ApprovalFlag", SqlDbType.NVarChar){Value=0},
                    new SqlParameter("@Approver", SqlDbType.NVarChar){Value=DBNull.Value},
                    new SqlParameter("@ApproveDate", SqlDbType.Date){Value=DBNull.Value}
                    };

                    string query = "INSERT INTO Leave VALUES (@Name, @LeaveDate, @ApprovalFlag, @Approver, @ApproveDate)";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();
                    sqlCommand.Parameters.AddRange(parameters.ToArray());
                    DataTable RequestLeaveTable = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand))
                        adapter.Fill(RequestLeaveTable);
                }
                else
                {
                    string query = "DELETE FROM Leave WHERE @Name = Name AND CAST(@LeaveDate AS DATE) = LeaveDate AND @ApprovalFlag = ApprovalFlag";

                    List<SqlParameter> parameters = new List<SqlParameter>() {
                    new SqlParameter("@Name", SqlDbType.NVarChar){Value=CalName.Text},
                    new SqlParameter("@LeaveDate", SqlDbType.DateTime){Value=RequestDate},
                    new SqlParameter("@ApprovalFlag", SqlDbType.NVarChar){Value=0}
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

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (PendingList.SelectedValue == null)
            {
                MessageBox.Show("No selected date from request list to be cancelled", "Leave Request");
            }
            else
            {
                int selectedItemCount = PendingList.SelectedItems.Count;

                for (int i = 0; i < selectedItemCount; i++)
                {
                    int ReqType = 2;
                    string strDate = PendingList.SelectedItems[i].ToString();

                    DateTime ConvertedDate = DateTime.Parse(strDate, CultureInfo.InvariantCulture);

                    bool requestResult = SaveDate(ConvertedDate, ReqType);

                    if (requestResult == true)
                    {
                        MessageBox.Show(strDate + " is removed from request list", "Leave Request");
                    }
                    else
                    {
                        MessageBox.Show("Request failed", "Leave Request");
                    }
                }
                GetRequestLeaveProcess();
            }
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            ApproverMenu approverMenu = new ApproverMenu(user);
            this.Close();
            approverMenu.Show();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            Profile profilePage = new Profile(user, 0);
            this.Close();
            profilePage.Show();
        }
    }
}
