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
    public partial class frm_Keypad : Form
    {
        private readonly double _min;
        private readonly double _max;

        private readonly bool _isCondition = true;

        #region frm_Keypad

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

        public frm_Keypad()
        {
            InitializeComponent();

            _isCondition = false;
        }

        public frm_Keypad(double min, double max)
        {
            InitializeComponent();

            _min = min;
            _max = max;
        }

        private void frm_Keypad_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalVariables.Form.Keypad = null;
        }
        private void frm_Keypad_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        private void frm_Keypad_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        #endregion

        private const string c_Zero = "0";

        private void InitializeForm()
        {
            label1.Text = _isCondition ? $"{_min} ~ {_max}" : string.Empty;

            label2.Text = c_Zero;

            foreach (Control control in GlobalFunctions.GetControls(this))
            {
                if (control.Name.Contains("Close") == false)
                {
                    control.BackColor = SystemColors.Control;
                }

                GlobalFunctions.DoubleBuffered(control, true);

                control.TabStop = false;
            }

            label1.ForeColor = SystemColors.Highlight;
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btn_Back_Click(object sender, EventArgs e)
        {
            if (label2.Text.Length > 0)
            {
                label2.Text = label2.Text.Substring(0, label2.Text.Length - 1);
            }

            if (string.IsNullOrEmpty(label2.Text))
            {
                label2.Text = c_Zero;
            }
        }

        private const string c_Point = ".";

        private void btn_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;

            if (int.TryParse(name.Substring(4), out int result))
            {
                if (label2.Text == c_Zero)
                {
                    label2.Text = string.Empty;
                }

                label2.Text += result.ToString();

                if (_isCondition)
                {
                    CheckCondition();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(label2.Text) == false)
                {
                    if (label2.Text.Contains(c_Point) == false)
                    {
                        label2.Text += c_Point;
                    }
                }
            }
        }

        private void CheckCondition()
        {
            double value = Convert.ToDouble(label2.Text);

            if (value < _min)
            {
                label2.Text = _min.ToString();
            }
            else if (value > _max)
            {
                label2.Text = _max.ToString();
            }
        }

        private void btn_정정_Click(object sender, EventArgs e)
        {
            label2.Text = c_Zero;
        }

        private void btn_확인_Click(object sender, EventArgs e)
        {
            GlobalVariables.KeypadValue = label2.Text;

            DialogResult = DialogResult.OK;
        }
    }
}
