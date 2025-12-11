using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace medicheck_group5
{
    public partial class CustomMessageBox : Form
    {
        public CustomMessageBox(string message, string title)
        {
            InitializeComponent();

            this.labelCaption.Text = title;
            this.labelMessage.Text = message;
        }
        public static DialogResult Show(string message, string title)
        {
            using (var msgBox = new CustomMessageBox(message, title))
            {
                // ShowDialog() makes the form modal (blocks the calling form)
                return msgBox.ShowDialog();
            }
        }
        private void gradientTextLabel1_Click(object sender, EventArgs e)
        {

        }

        private void labelMessage_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CustomMessageBox_Load(object sender, EventArgs e)
        {

        }
    }
}
