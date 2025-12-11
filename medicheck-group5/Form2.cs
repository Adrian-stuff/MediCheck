using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient; //database provider
using BCrypt.Net; //import the hashing library
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace medicheck_group5
{
    public partial class Form2 : Form
    {
        private Timer slideTimer;
        private Point finalPictureBoxPosition;

        public Form2()
        {
            InitializeComponent();
        }
        private bool UsernameExists(string username, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM UsersTable WHERE Username = @Username", con))
                {
                    cmd.Parameters.AddWithValue("@Username", username.Trim());
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        private bool EmailExists(string email, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM UsersTable WHERE Email = @Email", con))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim());
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
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

        private void gradientTextLabel1_Click(object sender, EventArgs e)
        {

        }
        private void Form2_Load(object sender, EventArgs e)
        {

            finalPictureBoxPosition = guna2PictureBox1.Location;
            guna2PictureBox1.Location = new Point(finalPictureBoxPosition.X, this.Height + 50);

            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;
            slideTimer.Start();
        }

        private void eyeOpen_Click(object sender, EventArgs e)
        {
            if (txtSUPassword.PasswordChar == '•')
            {
                txtSUPassword.PasswordChar = '\0';
                eyeOpen.Image = Properties.Resources.eyeopen;
            }
            else
            {
                txtSUPassword.PasswordChar = '•';
                eyeOpen.Image = Properties.Resources.eyeclose;
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            txtSUPassword.UseSystemPasswordChar = false;

        }

        private void label1_Click(object sender, EventArgs e)
        {
            Form1 main = new Form1();
            main.Show();
            this.Hide();
        }

        private void bttnSignin_Click(object sender, EventArgs e)
        {

            string firstName = txtFirstName.Text;
            string lastName = txtLastName.Text;
            string username = txtNewUsername.Text;
            string email = txtSUEmail.Text;
            string plainPassword = txtSUPassword.Text;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(plainPassword))
            {
                MessageBox.Show("All fields are REQUIRED for registration.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop execution if validation fails
            }
            string connectionString = DatabaseConfig.ConnectionString;

            if (UsernameExists(username, connectionString))
            {
                MessageBox.Show("This username is already registered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (EmailExists(email, connectionString))
            {
                MessageBox.Show("This email is already registered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            string insertQuery = @"INSERT INTO UsersTable (FirstNAme, LastName, Username, Email, PasswordHash, DateRegistered) VALUES (@FirstName, @LastName, @Username, @Email, @PasswordHash, GETDATE())";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // 4. Bind the C# variables to the SQL parameters
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash); // **The HASHED value is stored**

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            Form1 main = new Form1();
                            main.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Handle errors like duplicate username (Unique constraint violation)
                    MessageBox.Show($"An error occurred during registration. Details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void gradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
