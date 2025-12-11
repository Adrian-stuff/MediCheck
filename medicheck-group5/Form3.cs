using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using static Humanizer.In;

namespace medicheck_group5
{
    public partial class Form3 : Form
    {
        bool sidebarExpand;
        private int loggedInUserId;
        private string loggedInFullName;
        private const string ConnectionString = @"Data Source=STROBERI\SQLEXPRESS;Initial Catalog=MediCheck_Login;Integrated Security=True;TrustServerCertificate=True";
        private Timer countdownTimer;

        // Parameterized constructor
        public Form3(int userID, string fullName)
        {
            InitializeComponent();
            loggedInUserId = userID;
            loggedInFullName = string.IsNullOrWhiteSpace(fullName) ? "User" : fullName;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            lblGreeting.Text = $"Hello, {loggedInFullName}!";

            LoadComingUpMedication();
            LoadStats();
            LoadWeeklyProgress();  // ✅ load weekly progress

            // Start countdown timer for next medication
            countdownTimer = new Timer();
            countdownTimer.Interval = 60000; // 1 minute
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            LoadTodaysMedicationsSimple();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            DateTime nextMedTime;
            if (DateTime.TryParse(lblComingTime.Text, out nextMedTime))
            {
                UpdateCountdown(nextMedTime);
            }
        }

        // ------------------- COMING UP -------------------
        private void LoadComingUpMedication()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT Id, Name, Dosage, TimeToTake
            FROM Medications
            WHERE UserID = @uid";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@uid", loggedInUserId);

                SqlDataReader reader = cmd.ExecuteReader();

                var candidates = new List<(int Id, string Name, string Dosage, DateTime Occurrence)>();
                DateTime now = DateTime.Now;
                while (reader.Read())
                {
                    object raw = reader["TimeToTake"];
                    if (raw == DBNull.Value) continue;

                    DateTime occurrence;
                    // SQL TIME maps to TimeSpan in .NET; DATETIME maps to DateTime
                    if (raw is TimeSpan ts)
                    {
                        occurrence = DateTime.Today.Add(ts);
                    }
                    else
                    {
                        // Convert to DateTime then normalize to today's date but keep the time
                        DateTime dt = Convert.ToDateTime(raw);
                        occurrence = DateTime.Today.Add(dt.TimeOfDay);
                    }

                    candidates.Add((
                        Id: Convert.ToInt32(reader["Id"]),
                        Name: reader["Name"].ToString(),
                        Dosage: reader["Dosage"].ToString(),
                        Occurrence: occurrence
                    ));
                }

                reader.Close();

                // Find the next occurrence (today) that is >= now
                var next = candidates.Where(c => c.Occurrence >= now).OrderBy(c => c.Occurrence).FirstOrDefault();

                if (next != default)
                {
                    lblComingName.Text = next.Name;
                    lblComingDosage.Text = next.Dosage;
                    lblComingTime.Text = next.Occurrence.ToString("hh:mm tt");
                    UpdateCountdown(next.Occurrence);
                }
                else
                {
                    // Fallback: show most recent earlier today
                    var lastToday = candidates.Where(c => c.Occurrence.Date == DateTime.Today)
                                              .OrderByDescending(c => c.Occurrence)
                                              .FirstOrDefault();

                    if (lastToday != default)
                    {
                        lblComingName.Text = lastToday.Name;
                        lblComingDosage.Text = lastToday.Dosage;
                        lblComingTime.Text = lastToday.Occurrence.ToString("hh:mm tt");
                        UpdateCountdown(lastToday.Occurrence);
                    }
                    else
                    {
                        lblComingCountdown.Text = "No upcoming meds";
                        lblComingName.Text = "--";
                        lblComingDosage.Text = "--";
                        lblComingTime.Text = "--";
                    }
                }

                con.Close();
            }
        }
        private void UpdateCountdown(DateTime medTime)
        {
            // Ensure medTime is normalized to today if only time-of-day was provided
            if (medTime.Date != DateTime.Today)
                medTime = DateTime.Today.Add(medTime.TimeOfDay);

            // I-check muna kung nakalipas na ang oras
            if (medTime < DateTime.Now)
            {
                // Kung nakalipas na ang schedule, ipakita ang "Missed Dose" message
                lblComingCountdown.Text = "You've missed your dose";
                return;
            }

            // Kung hindi pa nakalipas, i-calculate ang natitirang oras
            TimeSpan left = medTime - DateTime.Now;

            // Ang block na ito ay para lang kung ang oras ay eksakto o napakalapit na (wala na tayong TotalSeconds <= 0 check dito)
            if (left.TotalSeconds < 60) // Halimbawa, kung less than 1 minute na lang
            {
                lblComingCountdown.Text = "It's time!";
                return;
            }

            int totalMinutes = (int)Math.Ceiling(left.TotalMinutes);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            lblComingCountdown.Text = $"Coming up in {hours}h {minutes}m";
        }
        private int GetComingUpMedicationId()
        {
            int medId = -1;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT Id, TimeToTake
            FROM Medications
            WHERE UserID = @uid";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@uid", loggedInUserId);

                SqlDataReader reader = cmd.ExecuteReader();

                var candidates = new List<(int Id, DateTime Occurrence)>();
                DateTime now = DateTime.Now;
                while (reader.Read())
                {
                    object raw = reader["TimeToTake"];
                    if (raw == DBNull.Value) continue;

                    DateTime occurrence;
                    if (raw is TimeSpan ts)
                    {
                        occurrence = DateTime.Today.Add(ts);
                    }
                    else
                    {
                        DateTime dt = Convert.ToDateTime(raw);
                        occurrence = DateTime.Today.Add(dt.TimeOfDay);
                    }

                    candidates.Add((Id: Convert.ToInt32(reader["Id"]), Occurrence: occurrence));
                }

                reader.Close();

                var next = candidates.Where(c => c.Occurrence >= now).OrderBy(c => c.Occurrence).FirstOrDefault();
                if (next != default)
                    medId = next.Id;
                else
                {
                    var lastToday = candidates.Where(c => c.Occurrence.Date == DateTime.Today)
                                              .OrderByDescending(c => c.Occurrence)
                                              .FirstOrDefault();
                    if (lastToday != default) medId = lastToday.Id;
                }

                con.Close();
            }

            return medId;
        }        // ------------------- STATS -------------------
        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string totalQuery = "SELECT COUNT(*) FROM Medications WHERE UserID = @uid";
                SqlCommand totalCmd = new SqlCommand(totalQuery, con);
                totalCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                lblTotal.Text = (totalCmd.ExecuteScalar() ?? 0).ToString();

                string takenQuery = @"
                    SELECT COUNT(*) FROM MedicationsTaken
                    WHERE UserID = @uid AND CAST(DateTaken AS DATE) = CAST(GETDATE() AS DATE)";
                SqlCommand takenCmd = new SqlCommand(takenQuery, con);
                takenCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                lblTaken.Text = (takenCmd.ExecuteScalar() ?? 0).ToString();

                string scheduledQuery = @"
                    SELECT COUNT(*) FROM Medications
                    WHERE UserID = @uid AND CAST(TimeToTake AS DATE) = CAST(GETDATE() AS DATE)";
                SqlCommand scheduledCmd = new SqlCommand(scheduledQuery, con);
                scheduledCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                int scheduledToday = (int)(scheduledCmd.ExecuteScalar() ?? 0);
                int takenToday = int.Parse(lblTaken.Text);
                int missedToday = scheduledToday - takenToday;
                lblMissed.Text = missedToday >= 0 ? missedToday.ToString() : "0";

                con.Close();
            }
        }

        // ------------------- MARK AS TAKEN -------------------
        private void btnMarkTaken_Click(object sender, EventArgs e)
        {
            if (lblComingName.Text == "--")
            {
                MessageBox.Show("No upcoming medication to mark as taken!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int medId = GetComingUpMedicationId();
            if (medId == -1)
            {
                MessageBox.Show("No upcoming medication found!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                string sql = "INSERT INTO MedicationsTaken (UserID, MedicationID) VALUES (@uid, @medId)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                cmd.Parameters.AddWithValue("@medId", medId);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("Medication marked as taken!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadComingUpMedication();
                LoadStats();
            }
        }

        // ------------------- SIDEBAR -------------------
        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= 10;
                if (sidebar.Width == sidebar.MinimumSize.Width)
                {
                    sidebarExpand = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width == sidebar.MaximumSize.Width)
                {
                    sidebarExpand = true;
                    sidebarTimer.Stop();
                }
            }
        }

        private void menuButton_Click(object sender, EventArgs e)
        {
            sidebarTimer.Start();
        }

        // ------------------- OPEN MEDICATION FORM -------------------
        private void bttnMedication_Click(object sender, EventArgs e)
        {
            // If you want to open Form4 for general medication management (no specific medId)
            Form4 medication = new Form4(loggedInUserId, -1); // Pass -1 or a default value for medId
            medication.Show();
            this.Hide();
        }

        private void LoadWeeklyProgress()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                // Determine start and end of current week (Monday to Sunday)
                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime weekStart = today.AddDays(-1 * diff); // Monday
                DateTime weekEnd = weekStart.AddDays(6);       // Sunday

                // Total meds scheduled for this week
                string totalQuery = @"
            SELECT COUNT(*) 
            FROM Medications
            WHERE UserID = @uid
            AND CAST(TimeToTake AS DATE) BETWEEN @start AND @end
        ";
                SqlCommand totalCmd = new SqlCommand(totalQuery, con);
                totalCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                totalCmd.Parameters.AddWithValue("@start", weekStart);
                totalCmd.Parameters.AddWithValue("@end", weekEnd);
                int totalWeek = (int)(totalCmd.ExecuteScalar() ?? 0);

                // Taken this week
                string takenQuery = @"
            SELECT COUNT(*) 
            FROM MedicationsTaken mt
            JOIN Medications m ON mt.MedicationID = m.Id
            WHERE mt.UserID = @uid
            AND CAST(mt.DateTaken AS DATE) BETWEEN @start AND @end
        ";
                SqlCommand takenCmd = new SqlCommand(takenQuery, con);
                takenCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                takenCmd.Parameters.AddWithValue("@start", weekStart);
                takenCmd.Parameters.AddWithValue("@end", weekEnd);
                int takenWeek = (int)(takenCmd.ExecuteScalar() ?? 0);

                int remainingWeek = totalWeek - takenWeek;
                if (remainingWeek < 0) remainingWeek = 0;

                // Calculate percentage
                int percentComplete = totalWeek > 0 ? (takenWeek * 100) / totalWeek : 0;

                // Update labels
                lblWeeklyCompletion.Text = $"You completed {percentComplete}% of your\n         doses this week";
                lblWeeklyTotal.Text = totalWeek.ToString();
                lblWeeklyTaken.Text = takenWeek.ToString();
                lblWeeklyRemaining.Text = remainingWeek.ToString();

                // Update progress bar
                progressBarWeekly.Value = percentComplete > 100 ? 100 : percentComplete;

                con.Close();
            }
        }

        private void LoadTodaysMedicationsSimple()
        {
            panelTodayMeds.Controls.Clear();
            panelTodayMeds.AutoScroll = true;
            panelTodayMeds.SuspendLayout();

            Label lblTitle = new Label
            {
                Text = "Today's Medication",
                Font = new Font("Viga", 12, FontStyle.Bold), // Adjust font/size as needed
                Location = new Point(5, 5), // Ilagay sa pinaka-itaas
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(0, 128, 128) // Teal color
            };
            panelTodayMeds.Controls.Add(lblTitle);

            var todaysTaken = new HashSet<int>();
            Font rowFont = new Font("Jaldi", 9, FontStyle.Regular);

            // Define spacing variables
            int horizontalMargin = 8;
            int verticalSpacing = 48;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                using (SqlCommand cmdTaken = new SqlCommand(
                    @"SELECT MedicationID FROM MedicationsTaken
              WHERE UserID = @uid
              AND CAST(DateTaken AS DATE) = CAST(GETDATE() AS DATE)", con))
                {
                    cmdTaken.Parameters.AddWithValue("@uid", loggedInUserId);

                    using (var r = cmdTaken.ExecuteReader())
                    {
                        while (r.Read())
                            todaysTaken.Add(Convert.ToInt32(r["MedicationID"]));
                    }
                }

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Id, Name, Dosage, TimeToTake
              FROM Medications
              WHERE UserID = @uid", con))
                {
                    cmd.Parameters.AddWithValue("@uid", loggedInUserId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int y = 35;
                        DateTime today = DateTime.Today;

                        while (reader.Read())
                        {
                            object raw = reader["TimeToTake"];
                            if (raw == DBNull.Value)
                                continue;

                            DateTime occurrence =
                                raw is TimeSpan ts
                                    ? today.Add(ts)
                                    : today.Add(Convert.ToDateTime(raw).TimeOfDay);

                            if (occurrence.Date != today)
                                continue;

                            int id = Convert.ToInt32(reader["Id"]);
                            string name = reader["Name"].ToString();
                            string dosage = reader["Dosage"].ToString();
                            string status = todaysTaken.Contains(id) ? "Taken" : "Upcoming";

                            Panel row = new Panel
                            {
                                // Set width and location with margin
                                Width = panelTodayMeds.ClientSize.Width - (horizontalMargin * 2),
                                Height = 40,
                                Location = new Point(horizontalMargin, y),
                                BackColor = Color.FromArgb(179, 230, 230) // Light Teal
                            };

                            Label lblName = new Label
                            {
                                Text = name,
                                Font = rowFont,
                                Location = new Point(5, 10),
                                Width = 90,
                                AutoSize = false
                            };

                            Label lblDosage = new Label
                            {
                                Text = dosage,
                                Font = rowFont,
                                // Adjusted X location: 100 -> 90
                                Location = new Point(90, 10),
                                Width = 55,
                                AutoSize = false
                            };

                            Label lblTime = new Label
                            {
                                Text = occurrence.ToString("hh:mm tt"),
                                Font = rowFont,
                                // Adjusted X location: 160 -> 145
                                Location = new Point(145, 10),
                                Width = 65,
                                AutoSize = false
                            };

                            Label lblStatus = new Label
                            {
                                Text = status,
                                Font = rowFont,
                                // Adjusted X location: 230 -> 210
                                Location = new Point(210, 10),
                                Width = 55,
                                AutoSize = false,
                                ForeColor = status == "Taken" ? Color.Green : Color.DarkOrange
                            };

                            row.Controls.Add(lblName);
                            row.Controls.Add(lblDosage);
                            row.Controls.Add(lblTime);
                            row.Controls.Add(lblStatus);

                            panelTodayMeds.Controls.Add(row);
                            y += verticalSpacing; // Use new vertical spacing for gap
                        }
                    }
                }
            }

            panelTodayMeds.ResumeLayout();
            panelTodayMeds.Refresh();
        }
        private void label4_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label4_Click_1(object sender, EventArgs e) { }
        private void label6_Click_1(object sender, EventArgs e) { }
        private void label7_Click_1(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) 
        {

        }
        private void label3_Click(object sender, EventArgs e) { }
        private void label12_Click(object sender, EventArgs e) { }
        private void progressBarMedication_ValueChanged(object sender, EventArgs e) { }
        private void label13_Click(object sender, EventArgs e) { }
        private void label19_Click(object sender, EventArgs e) { }
        private void calendarContainer_Paint(object sender, PaintEventArgs e) { }
        private void sidebar_Paint(object sender, PaintEventArgs e) { }

        private void panelQuickActions_Paint(object sender, PaintEventArgs e)
        {

        }

        private void sidebar_Paint_1(object sender, PaintEventArgs e)
        {

        }
    }
}
