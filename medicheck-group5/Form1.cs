using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;
using Guna.UI2.WinForms;

namespace medicheck_group5
{
    public partial class Form1 : Form
    {
        private Timer slideTimer;
        private Point finalPictureBoxPosition;
        
        private string ConnectionString = DatabaseConfig.ConnectionString;

        public Form1()
        {
            InitializeComponent();

            mainBasedGreeting.Text = TimeBasedGreeting();
            subtextGreeting.Text = TimeBasedSubtextGreeting();
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            int newY = guna2PictureBox1.Location.Y - 10;

            if (newY <= finalPictureBoxPosition.Y)
            {

                guna2PictureBox1.Location = finalPictureBoxPosition;
                slideTimer.Stop();
            }
            else
            {

                guna2PictureBox1.Location = new Point(finalPictureBoxPosition.X, newY);
            }
        }

        private string TimeBasedGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
            {

                return "Good Morning, CheckMate!";
            }
            else if (hour >= 12 && hour < 18)
            {

                return "Good Afternoon, CheckMate!";
            }
            else
            {

                return "Good Evening, CheckMate!";
            }
        }

        private string TimeBasedSubtextGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 17)
            {

                return "Ready to start your day right?";
            }
            else
            {

                return "You've worked hard. Let's finish up!";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //sliding image animation
            finalPictureBoxPosition = guna2PictureBox1.Location;
            guna2PictureBox1.Location = new Point(finalPictureBoxPosition.X, this.Height + 50);

            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;
            slideTimer.Start();

            //load saved if remember me was checked
            if (Properties.Settings.Default.Username != string.Empty)
            {

                txtSIUsername.Text = Properties.Settings.Default.Username;
                bttnRememberMe.Checked = true;
            }
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            string userInput = txtSIUsername.Text.Trim(); // username or email
            string plainPassword = txtSIPassword.Text;

            if (string.IsNullOrWhiteSpace(userInput) || string.IsNullOrWhiteSpace(plainPassword))
            {
                CustomMessageBox.Show("Please enter both username/email and password.", "INPUT REQUIRED");
                return;
            }

            string selectQuery = @"SELECT UserId, FirstName, LastName, Username, PasswordHash
                                   FROM UsersTable
                                   WHERE Username = @Input OR Email = @Input";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Input", userInput);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString();
                                int userID = Convert.ToInt32(reader["UserId"]);
                                string firstName = reader["FirstName"].ToString();
                                string lastName = reader["LastName"].ToString();
                                string fullName = (firstName + " " + lastName).Trim();

                                // Verify password
                                if (BCrypt.Net.BCrypt.Verify(plainPassword, storedHash))
                                {
                                    // Remember me
                                    if (bttnRememberMe.Checked)
                                    {
                                        Properties.Settings.Default.Username = userInput;
                                        Properties.Settings.Default.Save();
                                    }
                                    else
                                    {
                                        Properties.Settings.Default.Username = string.Empty;
                                        Properties.Settings.Default.Save();
                                    }

                                    // Open Dashboard (Form3)
                                    Form3 dashboard = new Form3(userID, fullName);
                                    dashboard.FormClosed += (s, args) => this.Close(); // close login when dashboard closes
                                    dashboard.Show();

                                    this.Hide(); // hide login form
                                }
                                else
                                {
                                    MessageBox.Show("Invalid Username/Email or Password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid Username/Email or Password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    
        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (!((Guna2ToggleSwitch)sender).Checked)
            {

                Properties.Settings.Default.Username = string.Empty;
                Properties.Settings.Default.Save();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private async void label1_Click(object sender, EventArgs e)
        {
            //fade outanimation from form1 to form2
            Form2 form2 = new Form2();

            for (double i = 1.0; i >= 0; i -= 0.05)
            {

                this.Opacity = i;
                await Task.Delay(8);
            }

            form2.Opacity = 0;
            form2.Show();

            for (double i = 0; i <= 1.0; i += 0.05)
            {

                form2.Opacity = i;
                await Task.Delay(8);
            }

            this.Hide();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

            txtSIPassword.UseSystemPasswordChar = false;
        }

        private void eyeOpen_Click(object sender, EventArgs e)
        {

            if (txtSIPassword.PasswordChar == '•')
            {
                txtSIPassword.PasswordChar = '\0';
                eyeOpen.Image = Properties.Resources.eyeopen;
            }
            else
            {

                txtSIPassword.PasswordChar = '•';
                eyeOpen.Image = Properties.Resources.eyeclose;
            }
        }

        private void txtSIUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void gradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}