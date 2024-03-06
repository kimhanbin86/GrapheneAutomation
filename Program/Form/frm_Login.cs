using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Program
{
    public partial class frm_Login : Form
    {
        #region frm_Login

        // https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public frm_Login()
        {
            InitializeComponent();
        }

        private void frm_Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalVariables.Form.Login = null;
        }
        private void frm_Login_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        private void frm_Login_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        #endregion

        private void InitializeForm()
        {
            foreach (Control control in GlobalFunctions.GetControls(this))
            {
                control.BackColor = SystemColors.Control;

                GlobalFunctions.DoubleBuffered(control, true);

                control.TabStop = false;
            }
        }

        private void label5_DoubleClick(object sender, EventArgs e)
        {
            txt_PW.Text = GlobalVariables.PASSWORD;
        }

        private void btn_로그인_Click(object sender, EventArgs e)
        {
            GlobalVariables.IsLogin = txt_PW.Text == GlobalVariables.PASSWORD;

            DialogResult = DialogResult.OK;
        }

        private void btn_취소_Click(object sender, EventArgs e)
        {
            GlobalVariables.IsLogin = false;

            DialogResult = DialogResult.Cancel;
        }
    }
}
