using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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

            query = "SELECT LeaveDate FROM Leave WHERE @Name = Name AND ApprovalFlag = 0";
            DateTime[] RequestedLeaves = GetLeaves();
            DisplayRequestedLeaves(RequestedLeaves);
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
    }
}
