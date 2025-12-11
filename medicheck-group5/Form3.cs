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
using System.Drawing.Drawing2D; // For standard brushes if needed

namespace medicheck_group5
{
    public partial class Form3 : Form
    {
        bool sidebarExpand;
        private int loggedInUserId;
        private string loggedInFullName;
        private string ConnectionString = DatabaseConfig.ConnectionString;
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

            // Initialize Calendar
            SetupWeeklyCalendar();
            // Load Today's Meds by default
            // Load Today's Meds by default
            LoadMedicationsForDate(DateTime.Today);
            
            // --- Sidebar Wiring ---
            // History: Show separate Form (consistent design request)
            // History: Show separate Form (consistent design request)
            this.bttnHistory.Click += (s, ev) => { 
                new FormHistory(loggedInUserId, this).Show(); 
                this.Hide(); 
            };
            this.button8.Click += (s, ev) => { 
                new FormHistory(loggedInUserId, this).Show(); 
                this.Hide(); 
            };
            
            // Calendar (Placeholder or feature)
            this.bttnCalendar.Click += (s, ev) => { MessageBox.Show("Full Calendar View coming soon!"); };
            this.button6.Click += (s, ev) => { MessageBox.Show("Full Calendar View coming soon!"); };
            
            // Settings
            this.button1.Click += (s, ev) => { MessageBox.Show("Settings coming soon!"); };
            this.button3.Click += (s, ev) => { MessageBox.Show("Settings coming soon!"); };

            // About
            this.button5.Click += (s, ev) => { MessageBox.Show("MediCheck v1.0\nCreated by Group 5"); };
            this.button7.Click += (s, ev) => { MessageBox.Show("MediCheck v1.0\nCreated by Group 5"); };

            // Home: Hide other panels, show dashboard
            this.bttnHome.Click += (s, ev) => { RefreshDashboard(); };
            this.button2.Click += (s, ev) => { RefreshDashboard(); };
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
            var takenIds = GetTakenMedicationIdsForDate(DateTime.Today);

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT Id, Name, Dosage, TimeToTake, StartDate, EndDate
            FROM Medications
            WHERE UserID = @uid
            AND (StartDate IS NULL OR StartDate <= @today)
            AND (EndDate IS NULL OR EndDate >= @today)";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                cmd.Parameters.AddWithValue("@today", DateTime.Today);

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

                // Filter out taken medications
                candidates = candidates.Where(c => !takenIds.Contains(c.Id)).ToList();

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

                try
                {
                    // Fixed Query: Check if today falls between StartDate and EndDate (handle NULL EndDate as 'ongoing')
                    string scheduledQuery = @"
                        SELECT COUNT(*) FROM Medications
                        WHERE UserID = @uid 
                        AND (StartDate IS NULL OR StartDate <= CAST(GETDATE() AS DATE))
                        AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS DATE))";
                    
                    SqlCommand scheduledCmd = new SqlCommand(scheduledQuery, con);
                    scheduledCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                    int scheduledToday = (int)(scheduledCmd.ExecuteScalar() ?? 0);
                    
                    int takenToday = 0;
                    int.TryParse(lblTaken.Text, out takenToday);
                    
                    int missedToday = scheduledToday - takenToday;
                    lblMissed.Text = missedToday >= 0 ? missedToday.ToString() : "0";
                }
                catch (Exception ex)
                {
                   // Fail silently slightly better or show error for debugging
                   MessageBox.Show("Error loading stats: " + ex.Message);
                }

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
                if (sidebar.Width <= sidebar.MinimumSize.Width)
                {
                    sidebar.Width = sidebar.MinimumSize.Width; // Snap to min
                    sidebarExpand = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= sidebar.MaximumSize.Width)
                {
                    sidebar.Width = sidebar.MaximumSize.Width; // Snap to max
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
        // ------------------- OPEN MEDICATION FORM -------------------
        private void bttnMedication_Click(object sender, EventArgs e)
        {
            // If you want to open Form4 for general medication management (no specific medId)
            Form4 medication = new Form4(loggedInUserId, -1, this); // Pass this instance
            medication.Show();
            this.Hide();
        }

        public void RefreshDashboard()
        {
             LoadStats();
             LoadComingUpMedication();
             LoadWeeklyProgress();
             LoadMedicationsForDate(DateTime.Today);
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
                // Total meds scheduled for this week (Calculated in C# to handle date ranges)
                int totalWeek = 0;
                string schedQuery = "SELECT StartDate, EndDate, Frequency FROM Medications WHERE UserID = @uid";
                
                using (SqlCommand schedCmd = new SqlCommand(schedQuery, con))
                {
                    schedCmd.Parameters.AddWithValue("@uid", loggedInUserId);
                    using (SqlDataReader r = schedCmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            // Parse dates, handle nulls if necessary (though Meds usually have StartDate)
                            DateTime start = r["StartDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["StartDate"]);
                            DateTime end = r["EndDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(r["EndDate"]);
                            string freq = r["Frequency"].ToString(); 

                            // Calculate overlap with this week
                            // Iterate through each day of the current week
                            for (int i = 0; i < 7; i++)
                            {
                                DateTime dayToCheck = weekStart.AddDays(i);
                                
                                // Check if this day is within the medication's active period
                                if (dayToCheck >= start && dayToCheck <= end)
                                {
                                    // For now, assume 'Daily' means 1 per day.
                                    // If you have '2x a day' etc, you'd parse 'freq' string here.
                                    // Assuming 1 dose per active day for simplicity as per current 'Frequency' inputs
                                    totalWeek++;
                                }
                            }
                        }
                    }
                }

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

        // Mock Data Class
        public class MedicationItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Dosage { get; set; }
            public DateTime TimeToTake { get; set; }
        }

        // Helper to get medications for a specific date from DB
        private List<MedicationItem> GetMedicationsForDate(DateTime date)
        {
            var list = new List<MedicationItem>();
            
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                // Check simple date range overlap
                    string sql = @"
                    SELECT Id, Name, Dosage, TimeToTake, StartDate, EndDate
                    FROM Medications
                    WHERE UserID = @uid
                    AND (StartDate IS NULL OR StartDate <= @date)
                    AND (EndDate IS NULL OR EndDate >= @date)";
                
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                    cmd.Parameters.AddWithValue("@date", date.Date);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object rawTime = reader["TimeToTake"];
                            if (rawTime == DBNull.Value) continue;

                            DateTime timeToTake;
                            if (rawTime is TimeSpan ts)
                            {
                                timeToTake = date.Date.Add(ts);
                            }
                            else
                            {
                                DateTime dt = Convert.ToDateTime(rawTime);
                                timeToTake = date.Date.Add(dt.TimeOfDay);
                            }

                            list.Add(new MedicationItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Dosage = reader["Dosage"].ToString(),
                                TimeToTake = timeToTake
                            });
                        }
                    }
                }
            }
            // Sort by time
            return list.OrderBy(m => m.TimeToTake).ToList();
        }

        // Helper to get IDs of medications taken on a specific date
        private HashSet<int> GetTakenMedicationIdsForDate(DateTime date)
        {
            var taken = new HashSet<int>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                string sql = @"
                    SELECT MedicationID 
                    FROM MedicationsTaken
                    WHERE UserID = @uid 
                    AND CAST(DateTaken AS DATE) = CAST(@date AS DATE)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                    cmd.Parameters.AddWithValue("@date", date);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            taken.Add(Convert.ToInt32(reader["MedicationID"]));
                        }
                    }
                }
            }
            return taken;
        }

        // Action to mark medication as taken in DB
        private void MarkMedicationAsTaken(int medId, DateTime dateOfMed)
        {
            // Only allow marking if it's today (or maybe past/future? let's stick to simple logic for now)
            // But we must record the DateTaken as the date of the med schedule, or Now?
            // Usually you mark it as taken NOW. 
            // If viewing past/future, marking as taken might be "backfilling". 
            // Let's use `DateTime.Now` for timestamp, but ensure we check logic correctly.
            
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    // We insert the actual time they took it (NOW), but effectively it counts for that day's schedule.
                    // The query uses CAST(DateTaken AS DATE) = ScheduleDate.
                    // If I take yesterday's med today, it will show as taken TODAY, not YESTERDAY.
                    // To support "marking for a specific date", we might need to manipulate DateTaken.
                    // For now, let's assume we mark it for the passed date.
                    
                    DateTime timestamp = dateOfMed.Date == DateTime.Today ? DateTime.Now : dateOfMed.Date.AddHours(12);

                    string sql = "INSERT INTO MedicationsTaken (UserID, MedicationID, DateTaken) VALUES (@uid, @mid, @dt)";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                        cmd.Parameters.AddWithValue("@mid", medId);
                        cmd.Parameters.AddWithValue("@dt", timestamp);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Medication marked as taken.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh stats and list
                LoadStats();
                LoadWeeklyProgress();
                LoadMedicationsForDate(dateOfMed); // Refresh the specific date view
                
                LoadComingUpMedication(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error marking medication: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Variable to track currently selected date
        private DateTime _currentSelectedDate = DateTime.Today;

        private void LoadMedicationsForDate(DateTime date)
        {
            _currentSelectedDate = date;

            panelTodayMeds.Controls.Clear();
            panelTodayMeds.AutoScroll = true;
            panelTodayMeds.SuspendLayout();

            string title = date.Date == DateTime.Today ? "Today's Medication" : $"Meds for {date:MMM dd}";

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Viga", 12, FontStyle.Bold),
                Location = new Point(5, 5),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(0, 128, 128) // Teal color
            };
            panelTodayMeds.Controls.Add(lblTitle);

            // Fetch Data
            var meds = GetMedicationsForDate(date);
            var takenIds = GetTakenMedicationIdsForDate(date);

            Font rowFont = new Font("Jaldi", 9, FontStyle.Regular);
            int horizontalMargin = 8;
            int verticalSpacing = 48;
            int y = 35;

            // Optional: Message if empty
            if (meds.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "No medications scheduled.",
                    Font = new Font("Jaldi", 10, FontStyle.Italic),
                    Location = new Point(horizontalMargin, 40),
                    AutoSize = true,
                    ForeColor = Color.Gray
                };
                panelTodayMeds.Controls.Add(lblEmpty);
            }

            foreach (var med in meds)
            {
                string status = takenIds.Contains(med.Id) ? "Taken" : "Upcoming";
                bool isTaken = status == "Taken";

                Panel row = new Panel
                {
                    Width = panelTodayMeds.ClientSize.Width - (horizontalMargin * 2),
                    Height = 40,
                    Location = new Point(horizontalMargin, y),
                    BackColor = Color.FromArgb(179, 230, 230) // Light Teal
                };
                
                // ... (Logic to build row similar to before) ...
                Label lblName = new Label
                {
                    Text = med.Name,
                    Font = rowFont,
                    Location = new Point(5, 10),
                    Width = 85, // Slightly reduced
                    AutoSize = false
                };

                Label lblDosage = new Label
                {
                    Text = med.Dosage,
                    Font = rowFont,
                    Location = new Point(95, 10), // Shifted
                    Width = 50,
                    AutoSize = false
                };

                Label lblTime = new Label
                {
                    Text = med.TimeToTake.ToString("hh:mm tt"),
                    Font = rowFont,
                    Location = new Point(150, 10), // Shifted
                    Width = 60,
                    AutoSize = false
                };

                if (isTaken)
                {
                    Label lblStatus = new Label
                    {
                        Text = "Taken",
                        Font = rowFont,
                        Location = new Point(220, 10), // Shifted
                        Width = 50,
                        AutoSize = false,
                        ForeColor = Color.Green
                    };
                    row.Controls.Add(lblStatus);
                }
                else
                {
                     // Only show "Take" button if date is today or present (or past? Maybe allow retroactive taking)
                     // For now, always allow.
                    Guna2Button btnTake = new Guna2Button
                    {
                        Text = "Take",
                        Font = new Font("Jaldi", 8, FontStyle.Bold),
                        Location = new Point(215, 5), // Shifted slightly right to be safe
                        Width = 80, // Widened per user request
                        Height = 30,
                        BorderRadius = 10,
                        FillColor = Color.Teal,
                        ForeColor = Color.White
                    };
                    int currentId = med.Id;
                    DateTime targetDate = date; // Capture closure
                    btnTake.Click += (s, args) => MarkMedicationAsTaken(currentId, targetDate);
                    
                    row.Controls.Add(btnTake);
                }

                row.Controls.Add(lblName);
                row.Controls.Add(lblDosage);
                row.Controls.Add(lblTime);

                panelTodayMeds.Controls.Add(row);
                y += verticalSpacing;
            }

            panelTodayMeds.ResumeLayout();
            panelTodayMeds.Refresh();
        }

        private void SetupWeeklyCalendar()
        {
            // Determine current week (Monday start)
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime weekStart = today.AddDays(-1 * diff).Date;

            // Clear dynamic controls from calendarContainer, keep the Header Label/Icon
            // Assuming pictureBox2 and label21 are the header items.
            // We can identify them by checking existing controls in Designer or just clearing by type/tag.
            // But simplified: Remove anything that looks like a day panel? 
            // Or just append panels at specific Y.
            
            // Removing old dynamic panels if re-called
            var toRemove = new List<Control>();
            foreach(Control c in calendarContainer.Controls)
            {
                if (c is Panel p && p.Tag != null && p.Tag.ToString() == "DayPanel")
                {
                    toRemove.Add(c);
                }
            }
            foreach(Control c in toRemove) calendarContainer.Controls.Remove(c);


            int startX = 20;
            int startY = 70;
            int gap = 10;
            int size = 50;

            for (int i = 0; i < 7; i++)
            {
                DateTime dayDate = weekStart.AddDays(i);
                bool hasMeds = CheckIfMedsOnDate(dayDate);
                bool isSelected = dayDate == _currentSelectedDate;

                Panel dayPanel = new Panel
                {
                    Size = new Size(size, 70), // Taller to fit indicator
                    Location = new Point(startX + (i * (size + gap)), startY),
                    BackColor = isSelected ? Color.FromArgb(200, 240, 240) : Color.Transparent,
                    Tag = "DayPanel",
                    Cursor = Cursors.Hand
                };
                
                // Click Logic Wrapper
                EventHandler handleDayClick = (s, e) => {
                    LoadMedicationsForDate(dayDate);
                    SetupWeeklyCalendar(); // Re-render to update selection highlight
                };

                // Assign logic to Panel
                dayPanel.Click += handleDayClick;

                // Day Name (Mon)
                Label lblDay = new Label
                {
                    Text = dayDate.ToString("ddd"),
                    Font = new Font("Jaldi", 9, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    Location = new Point(0, 5),
                    Width = size,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblDay.Click += handleDayClick;

                // Date Num (12)
                Label lblNum = new Label
                {
                    Text = dayDate.Day.ToString(),
                    Font = new Font("Viga", 10, FontStyle.Bold),
                    ForeColor = dayDate == DateTime.Today ? Color.Teal : Color.Black,
                    Location = new Point(0, 25),
                    Width = size,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblNum.Click += handleDayClick;

                dayPanel.Controls.Add(lblDay);
                dayPanel.Controls.Add(lblNum);

                // Green Indicator
                if (hasMeds)
                {
                     Panel indicator = new Panel
                     {
                         Size = new Size(6, 6),
                         BackColor = Color.LimeGreen,
                         Location = new Point((size - 6) / 2, 55) // Centered bottom
                     };
                     // Make it round
                     System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                     gp.AddEllipse(0, 0, 6, 6);
                     indicator.Region = new Region(gp);
                     
                     indicator.Click += handleDayClick;
                     dayPanel.Controls.Add(indicator);
                }

                calendarContainer.Controls.Add(dayPanel);
            }
        }

        private bool CheckIfMedsOnDate(DateTime date)
        {
             // Optimize: In a real app, query the whole week at once. 
             // For now, simple query per day is acceptable for 7 queries.
             using (SqlConnection con = new SqlConnection(ConnectionString))
             {
                 con.Open();
                 string sql = @"
                     SELECT COUNT(*) FROM Medications 
                     WHERE UserID = @uid 
                     AND (StartDate IS NULL OR StartDate <= @date)
                     AND (EndDate IS NULL OR EndDate >= @date)";
                 
                 using (SqlCommand cmd = new SqlCommand(sql, con))
                 {
                     cmd.Parameters.AddWithValue("@uid", loggedInUserId);
                     cmd.Parameters.AddWithValue("@date", date.Date); // Use .Date just to be safe
                     int count = (int)cmd.ExecuteScalar();
                     return count > 0;
                 }
             }
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
