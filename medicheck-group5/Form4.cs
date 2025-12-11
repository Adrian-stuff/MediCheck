using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Configuration;
using System.Data.SqlClient;

namespace medicheck_group5
{
    public partial class Form4 : Form
    {
        private int loggedInUserId;
        private int medicationId;
        private Form3 _parentForm;
        private bool sidebarExpand = true; // Sidebar state tracking

        private readonly string ConnectionString = DatabaseConfig.ConnectionString;

        public Form4(int userId, int medId, Form3 parent = null)
        {
            InitializeComponent();
            loggedInUserId = userId;
            medicationId = medId;
            _parentForm = parent;

            LoadMedicationDetails();
        }

        private void LoadMedicationDetails()
        {

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                string sql = @"
                SELECT Name, Dosage, Type, Notes, StartDate, EndDate, Frequency, TimeToTake, Stock, AlertLevel
                FROM Medications
                WHERE Id = @medId AND UserID = @uid
            ";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@medId", medicationId);
                cmd.Parameters.AddWithValue("@uid", loggedInUserId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtMedName.Text = reader["Name"].ToString();
                    txtDosage.Text = reader["Dosage"].ToString();
                    cboType.Text = reader["Type"].ToString();
                    txtNotes.Text = reader["Notes"].ToString();

                    dtStart.Value = reader["StartDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["StartDate"])
                                    : DateTime.Now;

                    dtEnd.Value = reader["EndDate"] != DBNull.Value
                                  ? Convert.ToDateTime(reader["EndDate"])
                                  : DateTime.Now;

                    cboFrequency.Text = reader["Frequency"].ToString();
                    dtTime.Value = reader["TimeToTake"] != DBNull.Value
                                   ? Convert.ToDateTime(reader["TimeToTake"])
                                   : DateTime.Now;

                    numStock.Value = reader["Stock"] != DBNull.Value
                                     ? Convert.ToDecimal(reader["Stock"])
                                     : 0;

                    numAlert.Value = reader["AlertLevel"] != DBNull.Value
                                     ? Convert.ToDecimal(reader["AlertLevel"])
                                     : 0;
                }

                con.Close();
            }
        }

        private void ClearForm()
        {
            txtMedName.Clear();
            txtDosage.Clear();
            txtNotes.Clear();

            cboType.SelectedIndex = -1;
            cboFrequency.SelectedIndex = -1;

            dtStart.Value = DateTime.Now;
            dtEnd.Value = DateTime.Now;

            dtTime.Value = DateTime.Now;

            numStock.Value = 0;
            numAlert.Value = 0;
        }
        private void Form4_Load(object sender, EventArgs e)
        {
             // Ensure sidebar matches Form3 state if passed? Or independent.
             // We'll trust its own state.
             
             // Manually wire sidebar events based on known Designer names (viewed in Form4.Designer.cs)
             this.sidebarTimer.Tick += sidebarTimer_Tick;
             this.menuButton.Click += menuButton_Click;
             
             // Navigation
             this.bttnHome.Click += (s, ev) => GoBackToHome();
             this.button2.Click += (s, ev) => GoBackToHome(); // "Home" Label/Button
             this.button4.Click += (s, ev) => GoBackToHome(); // "Home" (Duplicate?)

             this.bttnHistory.Click += (s, ev) => OpenHistory();
             this.button8.Click += (s, ev) => OpenHistory(); // "Home" (Label says Home but var likely History from copy?)
             // Note: In Form3 Designer, button8 was associated with History panel. 
             // In Form4 Designer, button8 text says "Home" in copy-paste? 
             // Designer view: button8 Text = "Home". bttnHistory Text = "History"
             // Use button names carefully.
             
             // To be safe, rely on the Icon/Name "bttnHistory"
             
             this.bttnCalendar.Click += (s, ev) => MessageBox.Show("Calendar View coming soon!");
        }

        private void GoBackToHome()
        {
             if (_parentForm != null)
             {
                 _parentForm.RefreshDashboard();
                 _parentForm.Show();
             }
             else
             {
                 new Form3(loggedInUserId, "").Show();
             }
             this.Close();
        }

        private void OpenHistory()
        {
             // Open History Form. 
             // If we want "Same as Form4", we open it and Close this one?
             // Or ShowDialog?
             // "button doesn't route".
             // If I navigate "History", I probably leave "Edit Med".
             new FormHistory(loggedInUserId).Show();
             this.Close(); // Close current form -> User is now in History. 
             // Note: FormHistory needs a way back to Home too.
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= 10;
                if (sidebar.Width <= sidebar.MinimumSize.Width)
                {
                    sidebar.Width = sidebar.MinimumSize.Width;
                    sidebarExpand = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= sidebar.MaximumSize.Width)
                {
                    sidebar.Width = sidebar.MaximumSize.Width;
                    sidebarExpand = true;
                    sidebarTimer.Stop();
                }
            }
        }

        private void menuButton_Click(object sender, EventArgs e)
        {
            sidebarTimer.Start();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void txtSIUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2DateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            dtTime.CustomFormat = "hh:mm tt";
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = DatabaseConfig.ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    // UPDATED: Add UserID column to insert
                    string query = @"
    INSERT INTO Medications
    (Name, Dosage, Type, Notes, StartDate, EndDate,
     Frequency, TimeToTake, Stock, AlertLevel, UserID)
    VALUES
    (@Name, @Dosage, @Type, @Notes, @Start, @End,
     @Freq, @Time, @Stock, @Alert, @UserID)
";



                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Name", txtMedName.Text);
                    cmd.Parameters.AddWithValue("@Dosage", txtDosage.Text);
                    cmd.Parameters.AddWithValue("@Type", cboType.Text);
                    cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);

                    cmd.Parameters.AddWithValue("@Start", dtStart.Value);
                    cmd.Parameters.AddWithValue("@End", dtEnd.Checked ? (object)dtEnd.Value : DBNull.Value);

                    cmd.Parameters.AddWithValue("@Freq", cboFrequency.Text);
                    cmd.Parameters.AddWithValue("@Time", dtTime.Value);
                    cmd.Parameters.AddWithValue("@Stock", numStock.Value);
                    cmd.Parameters.AddWithValue("@Alert", numAlert.Value);

                    cmd.Parameters.AddWithValue("@UserID", loggedInUserId);


                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("✅ Medication successfully saved!");

                    if (_parentForm != null)
                    {
                        _parentForm.RefreshDashboard();
                        _parentForm.Show();
                    }
                    else
                    {
                        // Fallback: create new if for some reason parent is missing (e.g. debugging)
                        Form3 home = new Form3(loggedInUserId, "");
                        home.Show();
                    }
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error saving medication:\n" + ex.Message);
            }
        }

        private void bttnMinimize_Click(object sender, EventArgs e)
        {

        }

        private void sidebar_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

