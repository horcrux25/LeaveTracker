using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        bool ClosingBypass = false;
        User user = new User();

        public Profile(User user)
        {
            InitializeComponent();
            this.user = user;
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
            Calendar calendarWindow = new Calendar(user);
            this.Close();
            calendarWindow.Show();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save");
        }
    }
}
