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

        private readonly string ConnectionString = @"Data Source=STROBERI\SQLEXPRESS;Initial Catalog=MediCheck_Login;Integrated Security=True;TrustServerCertificate=True";

        public Form4(int userId, int medId)
        {
            InitializeComponent();
            loggedInUserId = userId;
            medicationId = medId;

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
                string connectionString = @"Data Source=STROBERI\SQLEXPRESS;Initial Catalog=MediCheck_Login;Integrated Security=True;TrustServerCertificate=True";

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

