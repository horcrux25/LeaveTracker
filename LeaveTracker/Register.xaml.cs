using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        public Register()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveTracker.Properties.Settings.LeaveTrackerConnectionString"].ConnectionString;

            //Initialize Connection
            sqlConnection = new SqlConnection(connectionString);
        }

        private void NewCancel_Click(object sender, RoutedEventArgs e)
        {
            ClosingBypass = true;
            this.Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void NewRegister_Click(object sender, RoutedEventArgs e)
        {
            bool ErrorFlag = false;

            TextBox[] RegisterTextBox = new TextBox[] {NewUsername, NewFullName};
            PasswordBox[] RegisterPasswordBox = new PasswordBox[] {NewPassword1, NewPassword2};
            TextBlock[] RegisterTextBlock = new TextBlock[] {NewUsernameError, NewFullNameError};
            TextBlock[] RegisterPaswordBlock = new TextBlock[] {NewPassword1Error, NewPassword2Error};

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
                            MessageBox.Show("Successful register");
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
                new SqlParameter("@Access", SqlDbType.NVarChar){Value=0}};

                string query = "INSERT INTO Logins VALUES (@UserName, @Password, @Name, @Access)";
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
    }
}
