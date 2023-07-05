using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
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

namespace LeaveTracker
{
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : Window
    {
        SqlConnection sqlConnection;
        bool ClosingBypass = false;

        public Calendar(User user)
        {
            InitializeComponent();

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);

            CalName.Text = user.Name;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            this.Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
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
    }
}
