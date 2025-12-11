using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace medicheck_group5
{
    public partial class FormHistory : Form
    {
        private int loggedInUserId;
        private string ConnectionString = DatabaseConfig.ConnectionString;

        // Optional: Support drag
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private Form3 _parentForm;

        public FormHistory(int userId, Form3 parent = null)
        {
            InitializeComponent();
            this.loggedInUserId = userId;
            this._parentForm = parent;
            
            // Drag support
            this.pnlHeader.MouseDown += (s, e) => { 
                if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } 
            };
            
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string sql = @"
                        SELECT m.Name, m.Dosage, mt.DateTaken
                        FROM MedicationsTaken mt
                        JOIN Medications m ON mt.MedicationID = m.Id
                        WHERE mt.UserID = @uid
                        ORDER BY mt.DateTaken DESC";
                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    da.SelectCommand.Parameters.AddWithValue("@uid", loggedInUserId);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);
                    dgvHistory.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading history: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_parentForm != null)
            {
                _parentForm.RefreshDashboard(); // Optional refresh
                _parentForm.Show();
            }
            else
            {
                 // Fallback if parent not passed (shouldn't happen with correct flow)
                 new Form3(loggedInUserId, "").Show();
            }
            this.Close();
        }
    }
}
