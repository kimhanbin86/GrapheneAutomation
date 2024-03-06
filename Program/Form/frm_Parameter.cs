using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;

using Library.Log;

namespace Program
{
    public partial class frm_Parameter : Form
    {
        #region Timer

        private Timer _Timer = null;
        private void Tick(object sender, EventArgs e)
        {
            _Timer?.Stop();
            try
            {
                #region Sequence

                chk_test.BackColor = chk_test.Checked != GlobalVariables.Parameter.Sequence.test ? Color.Yellow : SystemColors.Control;

                txt_test_min.BackColor = Convert.ToInt32(txt_test_min.Text) != GlobalVariables.Parameter.Sequence.test_min ? Color.Yellow : SystemColors.Highlight;
                txt_test_min.ForeColor = txt_test_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_인산_vol.BackColor = txt_Sequence_인산_vol.Text != GlobalVariables.Parameter.Sequence.인산_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_인산_vol.ForeColor = txt_Sequence_인산_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_인산_min.BackColor = Convert.ToInt32(txt_Sequence_인산_min.Text) != GlobalVariables.Parameter.Sequence.인산_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_인산_min.ForeColor = txt_Sequence_인산_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_Recycle_sec.BackColor = Convert.ToInt32(txt_Sequence_Recycle_sec.Text) != GlobalVariables.Parameter.Sequence.Recycle_sec ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_Recycle_sec.ForeColor = txt_Sequence_Recycle_sec.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_흑연_rpm.BackColor = txt_Sequence_흑연_rpm.Text != GlobalVariables.Parameter.Sequence.흑연_rpm ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_흑연_rpm.ForeColor = txt_Sequence_흑연_rpm.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_과망간산칼륨_rpm.BackColor = txt_Sequence_과망간산칼륨_rpm.Text != GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_과망간산칼륨_rpm.ForeColor = txt_Sequence_과망간산칼륨_rpm.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응로_1차_승온_min.BackColor = Convert.ToInt32(txt_Sequence_반응로_1차_승온_min.Text) != GlobalVariables.Parameter.Sequence.반응로_1차_승온_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응로_1차_승온_min.ForeColor = txt_Sequence_반응로_1차_승온_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응로_2차_승온_min.BackColor = Convert.ToInt32(txt_Sequence_반응로_2차_승온_min.Text) != GlobalVariables.Parameter.Sequence.반응로_2차_승온_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응로_2차_승온_min.ForeColor = txt_Sequence_반응로_2차_승온_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응로_가열_min.BackColor = Convert.ToInt32(txt_Sequence_반응로_가열_min.Text) != GlobalVariables.Parameter.Sequence.반응로_가열_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응로_가열_min.ForeColor = txt_Sequence_반응로_가열_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응로_온도_하강_min.BackColor = Convert.ToInt32(txt_Sequence_반응로_온도_하강_min.Text) != GlobalVariables.Parameter.Sequence.반응로_온도_하강_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응로_온도_하강_min.ForeColor = txt_Sequence_반응로_온도_하강_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_1차_vol.BackColor = txt_Sequence_초순수_1차_vol.Text != GlobalVariables.Parameter.Sequence.초순수_1차_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_1차_vol.ForeColor = txt_Sequence_초순수_1차_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_1차_min.BackColor = Convert.ToInt32(txt_Sequence_초순수_1차_min.Text) != GlobalVariables.Parameter.Sequence.초순수_1차_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_1차_min.ForeColor = txt_Sequence_초순수_1차_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응조확인.BackColor = Convert.ToDouble(txt_Sequence_반응조확인.Text) != GlobalVariables.Parameter.Sequence.반응조확인 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응조확인.ForeColor = txt_Sequence_반응조확인.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_3차_vol.BackColor = txt_Sequence_초순수_3차_vol.Text != GlobalVariables.Parameter.Sequence.초순수_3차_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_3차_vol.ForeColor = txt_Sequence_초순수_3차_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_3차_min.BackColor = Convert.ToInt32(txt_Sequence_초순수_3차_min.Text) != GlobalVariables.Parameter.Sequence.초순수_3차_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_3차_min.ForeColor = txt_Sequence_초순수_3차_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_4차_vol.BackColor = txt_Sequence_초순수_4차_vol.Text != GlobalVariables.Parameter.Sequence.초순수_4차_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_4차_vol.ForeColor = txt_Sequence_초순수_4차_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_초순수_4차_min.BackColor = Convert.ToInt32(txt_Sequence_초순수_4차_min.Text) != GlobalVariables.Parameter.Sequence.초순수_4차_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_초순수_4차_min.ForeColor = txt_Sequence_초순수_4차_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_반응물_교반_min.BackColor = Convert.ToInt32(txt_Sequence_반응물_교반_min.Text) != GlobalVariables.Parameter.Sequence.반응물_교반_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_반응물_교반_min.ForeColor = txt_Sequence_반응물_교반_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_과산화수소_1차_vol.BackColor = txt_Sequence_과산화수소_1차_vol.Text != GlobalVariables.Parameter.Sequence.과산화수소_1차_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_과산화수소_1차_vol.ForeColor = txt_Sequence_과산화수소_1차_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_과산화수소_1차_min.BackColor = Convert.ToInt32(txt_Sequence_과산화수소_1차_min.Text) != GlobalVariables.Parameter.Sequence.과산화수소_1차_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_과산화수소_1차_min.ForeColor = txt_Sequence_과산화수소_1차_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_과산화수소_2차_vol.BackColor = txt_Sequence_과산화수소_2차_vol.Text != GlobalVariables.Parameter.Sequence.과산화수소_2차_vol ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_과산화수소_2차_vol.ForeColor = txt_Sequence_과산화수소_2차_vol.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_과산화수소_2차_min.BackColor = Convert.ToInt32(txt_Sequence_과산화수소_2차_min.Text) != GlobalVariables.Parameter.Sequence.과산화수소_2차_min ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_과산화수소_2차_min.ForeColor = txt_Sequence_과산화수소_2차_min.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                chk_Sequence_skip_인산.BackColor = chk_Sequence_skip_인산.Checked != GlobalVariables.Parameter.Sequence.skip_인산 ? Color.Yellow : SystemColors.Control;
                chk_Sequence_skip_흑연.BackColor = chk_Sequence_skip_흑연.Checked != GlobalVariables.Parameter.Sequence.skip_흑연 ? Color.Yellow : SystemColors.Control;
                chk_Sequence_skip_과망간산칼륨.BackColor = chk_Sequence_skip_과망간산칼륨.Checked != GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 ? Color.Yellow : SystemColors.Control;

                txt_Sequence_교반기_rpm_인산.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_인산.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_인산 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_인산.ForeColor = txt_Sequence_교반기_rpm_인산.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_교반기_rpm_흑연.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_흑연.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_흑연 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_흑연.ForeColor = txt_Sequence_교반기_rpm_흑연.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_교반기_rpm_과망간산칼륨.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_과망간산칼륨.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_과망간산칼륨.ForeColor = txt_Sequence_교반기_rpm_과망간산칼륨.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_교반기_rpm_반응로.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_반응로.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_반응로 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_반응로.ForeColor = txt_Sequence_교반기_rpm_반응로.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_교반기_rpm_초순수.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_초순수.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_초순수 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_초순수.ForeColor = txt_Sequence_교반기_rpm_초순수.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Sequence_교반기_rpm_과산화수소.BackColor = Convert.ToInt32(txt_Sequence_교반기_rpm_과산화수소.Text) != GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소 ? Color.Yellow : SystemColors.Highlight;
                txt_Sequence_교반기_rpm_과산화수소.ForeColor = txt_Sequence_교반기_rpm_과산화수소.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                #endregion

                #region Check

                chk_Check_채널선택확인.BackColor = chk_Check_채널선택확인.Checked != GlobalVariables.Parameter.CheckSequence.채널선택확인 ? Color.Yellow : SystemColors.Control;

                chk_Check_헤드하강확인.BackColor = chk_Check_헤드하강확인.Checked != GlobalVariables.Parameter.CheckSequence.헤드하강확인 ? Color.Yellow : SystemColors.Control;

                chk_Check_반응조센서확인.BackColor = chk_Check_반응조센서확인.Checked != GlobalVariables.Parameter.CheckSequence.반응조센서확인 ? Color.Yellow : SystemColors.Control;

                chk_Check_반응조온도확인.BackColor = chk_Check_반응조온도확인.Checked != GlobalVariables.Parameter.CheckSequence.반응조온도확인 ? Color.Yellow : SystemColors.Control;

                txt_Check_반응조온도확인.BackColor = Convert.ToDouble(txt_Check_반응조온도확인.Text) != GlobalVariables.Parameter.CheckSequence.반응조온도 ? Color.Yellow : SystemColors.Highlight;
                txt_Check_반응조온도확인.ForeColor = txt_Check_반응조온도확인.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                chk_Check_반응로온도확인.BackColor = chk_Check_반응로온도확인.Checked != GlobalVariables.Parameter.CheckSequence.반응로온도확인 ? Color.Yellow : SystemColors.Control;

                txt_Check_반응로온도확인.BackColor = Convert.ToDouble(txt_Check_반응로온도확인.Text) != GlobalVariables.Parameter.CheckSequence.반응로온도 ? Color.Yellow : SystemColors.Highlight;
                txt_Check_반응로온도확인.ForeColor = txt_Check_반응로온도확인.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                #endregion

                #region Interlock

                txt_Interlock_반응로승온또는가열.BackColor = Convert.ToDouble(txt_Interlock_반응로승온또는가열.Text) != GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열 ? Color.Yellow : SystemColors.Highlight;
                txt_Interlock_반응로승온또는가열.ForeColor = txt_Interlock_반응로승온또는가열.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                txt_Interlock_부반응물제거.BackColor = Convert.ToDouble(txt_Interlock_부반응물제거.Text) != GlobalVariables.Parameter.CheckInterlock.부반응물제거 ? Color.Yellow : SystemColors.Highlight;
                txt_Interlock_부반응물제거.ForeColor = txt_Interlock_부반응물제거.BackColor == Color.Yellow ? SystemColors.ControlText : SystemColors.HighlightText;

                #endregion
            }
            catch (Exception ex)
            {
                Log.Write($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                _Timer?.Start();
            }
        }

        #endregion

        #region frm_Parameter

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

        public frm_Parameter()
        {
            InitializeComponent();
        }

        private void frm_Parameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalVariables.Form.Parameter = null;
        }
        private void frm_Parameter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Timer != null)
            {
                if (_Timer.Enabled)
                {
                    _Timer.Stop();
                }

                _Timer.Dispose();
                _Timer = null;
            }
        }
        private void frm_Parameter_Load(object sender, EventArgs e)
        {
            InitializeForm();

            UpdateForm();

            _Timer = new Timer();
            _Timer.Tick += new EventHandler(Tick);
            _Timer.Interval = 100;
            _Timer.Start();
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

            #region BackColor

            txt_Sequence_인산_sec.BackColor = Color.LightGray;
            txt_Sequence_인산_mlPerSec.BackColor = Color.LightGray;

            txt_Sequence_흑연_rev.BackColor = Color.LightGray;
            txt_Sequence_흑연_min.BackColor = Color.LightGray;

            txt_Sequence_과망간산칼륨_rev.BackColor = Color.LightGray;
            txt_Sequence_과망간산칼륨_min.BackColor = Color.LightGray;

            txt_Sequence_초순수_1차_sec.BackColor = Color.LightGray;
            txt_Sequence_초순수_1차_mlPerSec.BackColor = Color.LightGray;

            txt_Sequence_초순수_3차_sec.BackColor = Color.LightGray;
            txt_Sequence_초순수_3차_mlPerSec.BackColor = Color.LightGray;

            txt_Sequence_초순수_4차_sec.BackColor = Color.LightGray;
            txt_Sequence_초순수_4차_mlPerSec.BackColor = Color.LightGray;

            txt_Sequence_과산화수소_1차_sec.BackColor = Color.LightGray;
            txt_Sequence_과산화수소_1차_mlPerSec.BackColor = Color.LightGray;

            txt_Sequence_과산화수소_2차_sec.BackColor = Color.LightGray;
            txt_Sequence_과산화수소_2차_mlPerSec.BackColor = Color.LightGray;

            #endregion
        }

        private void UpdateForm()
        {
            #region Sequence

            chk_test.Checked = GlobalVariables.Parameter.Sequence.test;
            txt_test_min.Text = GlobalVariables.Parameter.Sequence.test_min.ToString();

            txt_Sequence_인산_vol.Text = GlobalVariables.Parameter.Sequence.인산_vol;
            txt_Sequence_인산_sec.Text = GlobalVariables.Parameter.Sequence.인산_sec;
            txt_Sequence_인산_min.Text = GlobalVariables.Parameter.Sequence.인산_min.ToString();
            txt_Sequence_인산_mlPerSec.Text = GlobalVariables.Parameter.Sequence.인산_mlPerSec.ToString();

            txt_Sequence_Recycle_sec.Text = GlobalVariables.Parameter.Sequence.Recycle_sec.ToString();

            txt_Sequence_흑연_rev.Text = GlobalVariables.Parameter.Sequence.흑연_rev;
            txt_Sequence_흑연_rpm.Text = GlobalVariables.Parameter.Sequence.흑연_rpm;
            txt_Sequence_흑연_min.Text = GlobalVariables.Parameter.Sequence.흑연_min.ToString();

            txt_Sequence_과망간산칼륨_rev.Text = GlobalVariables.Parameter.Sequence.과망간산칼륨_rev;
            txt_Sequence_과망간산칼륨_rpm.Text = GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm;
            txt_Sequence_과망간산칼륨_min.Text = GlobalVariables.Parameter.Sequence.과망간산칼륨_min.ToString();

            txt_Sequence_반응로_1차_승온_min.Text = GlobalVariables.Parameter.Sequence.반응로_1차_승온_min.ToString();
            txt_Sequence_반응로_2차_승온_min.Text = GlobalVariables.Parameter.Sequence.반응로_2차_승온_min.ToString();

            txt_Sequence_반응로_가열_min.Text = GlobalVariables.Parameter.Sequence.반응로_가열_min.ToString();

            txt_Sequence_반응로_온도_하강_min.Text = GlobalVariables.Parameter.Sequence.반응로_온도_하강_min.ToString();

            txt_Sequence_초순수_1차_vol.Text = GlobalVariables.Parameter.Sequence.초순수_1차_vol;
            txt_Sequence_초순수_1차_sec.Text = GlobalVariables.Parameter.Sequence.초순수_1차_sec;
            txt_Sequence_초순수_1차_min.Text = GlobalVariables.Parameter.Sequence.초순수_1차_min.ToString();
            txt_Sequence_초순수_1차_mlPerSec.Text = GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec.ToString();

            txt_Sequence_반응조확인.Text = GlobalVariables.Parameter.Sequence.반응조확인.ToString();

            txt_Sequence_초순수_3차_vol.Text = GlobalVariables.Parameter.Sequence.초순수_3차_vol;
            txt_Sequence_초순수_3차_sec.Text = GlobalVariables.Parameter.Sequence.초순수_3차_sec;
            txt_Sequence_초순수_3차_min.Text = GlobalVariables.Parameter.Sequence.초순수_3차_min.ToString();
            txt_Sequence_초순수_3차_mlPerSec.Text = GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec.ToString();

            txt_Sequence_초순수_4차_vol.Text = GlobalVariables.Parameter.Sequence.초순수_4차_vol;
            txt_Sequence_초순수_4차_sec.Text = GlobalVariables.Parameter.Sequence.초순수_4차_sec;
            txt_Sequence_초순수_4차_min.Text = GlobalVariables.Parameter.Sequence.초순수_4차_min.ToString();
            txt_Sequence_초순수_4차_mlPerSec.Text = GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec.ToString();

            txt_Sequence_반응물_교반_min.Text = GlobalVariables.Parameter.Sequence.반응물_교반_min.ToString();

            txt_Sequence_과산화수소_1차_vol.Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_vol;
            txt_Sequence_과산화수소_1차_sec.Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_sec;
            txt_Sequence_과산화수소_1차_min.Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_min.ToString();
            txt_Sequence_과산화수소_1차_mlPerSec.Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec.ToString();

            txt_Sequence_과산화수소_2차_vol.Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_vol;
            txt_Sequence_과산화수소_2차_sec.Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_sec;
            txt_Sequence_과산화수소_2차_min.Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_min.ToString();
            txt_Sequence_과산화수소_2차_mlPerSec.Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec.ToString();

            chk_Sequence_skip_인산.Checked = GlobalVariables.Parameter.Sequence.skip_인산;
            chk_Sequence_skip_흑연.Checked = GlobalVariables.Parameter.Sequence.skip_흑연;
            chk_Sequence_skip_과망간산칼륨.Checked = GlobalVariables.Parameter.Sequence.skip_과망간산칼륨;

            txt_Sequence_교반기_rpm_인산.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_인산.ToString();
            txt_Sequence_교반기_rpm_흑연.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_흑연.ToString();
            txt_Sequence_교반기_rpm_과망간산칼륨.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨.ToString();
            txt_Sequence_교반기_rpm_반응로.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_반응로.ToString();
            txt_Sequence_교반기_rpm_초순수.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_초순수.ToString();
            txt_Sequence_교반기_rpm_과산화수소.Text = GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소.ToString();

            #endregion

            #region Check

            chk_Check_채널선택확인.Checked = GlobalVariables.Parameter.CheckSequence.채널선택확인;

            chk_Check_헤드하강확인.Checked = GlobalVariables.Parameter.CheckSequence.헤드하강확인;

            chk_Check_반응조센서확인.Checked = GlobalVariables.Parameter.CheckSequence.반응조센서확인;

            chk_Check_반응조온도확인.Checked = GlobalVariables.Parameter.CheckSequence.반응조온도확인;
            txt_Check_반응조온도확인.Text = GlobalVariables.Parameter.CheckSequence.반응조온도.ToString();

            chk_Check_반응로온도확인.Checked = GlobalVariables.Parameter.CheckSequence.반응로온도확인;
            txt_Check_반응로온도확인.Text = GlobalVariables.Parameter.CheckSequence.반응로온도.ToString();

            #endregion

            #region Interlock

            txt_Interlock_반응로승온또는가열.Text = GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열.ToString();

            txt_Interlock_부반응물제거.Text = GlobalVariables.Parameter.CheckInterlock.부반응물제거.ToString();

            #endregion
        }

        private void txt_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;
            string name = label.Name;

            if (GlobalVariables.Form.Keypad == null)
            {
                using (GlobalVariables.Form.Keypad = new frm_Keypad())
                {
                    if (GlobalVariables.Form.Keypad.ShowDialog() == DialogResult.OK)
                    {
                        #region Remove "."

                        switch (name)
                        {
                            case "txt_test_min":
                            case "txt_Sequence_인산_vol":
                            case "txt_Sequence_인산_min":
                            case "txt_Sequence_Recycle_sec":
                            case "txt_Sequence_흑연_rpm":
                            case "txt_Sequence_과망간산칼륨_rpm":
                            case "txt_Sequence_반응로_1차_승온_min":
                            case "txt_Sequence_반응로_2차_승온_min":
                            case "txt_Sequence_반응로_가열_min":
                            case "txt_Sequence_반응로_온도_하강_min":
                            case "txt_Sequence_초순수_1차_vol":
                            case "txt_Sequence_초순수_1차_min":
                            case "txt_Sequence_초순수_3차_vol":
                            case "txt_Sequence_초순수_3차_min":
                            case "txt_Sequence_초순수_4차_vol":
                            case "txt_Sequence_초순수_4차_min":
                            case "txt_Sequence_반응물_교반_min":
                            case "txt_Sequence_과산화수소_1차_vol":
                            case "txt_Sequence_과산화수소_1차_min":
                            case "txt_Sequence_과산화수소_2차_vol":
                            case "txt_Sequence_과산화수소_2차_min":
                            case "txt_Sequence_교반기_rpm_인산":
                            case "txt_Sequence_교반기_rpm_흑연":
                            case "txt_Sequence_교반기_rpm_과망간산칼륨":
                            case "txt_Sequence_교반기_rpm_반응로":
                            case "txt_Sequence_교반기_rpm_초순수":
                            case "txt_Sequence_교반기_rpm_과산화수소":
                                int idx = GlobalVariables.KeypadValue.IndexOf(".");

                                if (idx >= 0)
                                {
                                    GlobalVariables.KeypadValue = GlobalVariables.KeypadValue.Remove(idx);
                                }
                                break;
                        }

                        #endregion

                        label.Text = GlobalVariables.KeypadValue;

                        #region calc

                        if (name == "txt_Sequence_인산_min" || name == "txt_Sequence_인산_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_인산_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_인산_min.Text) * 60;

                            txt_Sequence_인산_sec.Text = sec.ToString();
                            txt_Sequence_인산_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_흑연_rpm")
                        {
                            int rev = Convert.ToInt32(txt_Sequence_흑연_rev.Text);
                            int rpm = Convert.ToInt32(txt_Sequence_흑연_rpm.Text);
                            int min = rev / rpm;

                            txt_Sequence_흑연_min.Text = min.ToString();

                            if (rev % rpm != 0)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "정수로 입력해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_과망간산칼륨_rpm")
                        {
                            int rev = Convert.ToInt32(txt_Sequence_과망간산칼륨_rev.Text);
                            int rpm = Convert.ToInt32(txt_Sequence_과망간산칼륨_rpm.Text);
                            int min = rev / rpm;

                            txt_Sequence_과망간산칼륨_min.Text = min.ToString();

                            if (rev % rpm != 0)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "정수로 입력해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_초순수_1차_min" || name == "txt_Sequence_초순수_1차_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_초순수_1차_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_초순수_1차_min.Text) * 60;

                            txt_Sequence_초순수_1차_sec.Text = sec.ToString();
                            txt_Sequence_초순수_1차_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_초순수_3차_min" || name == "txt_Sequence_초순수_3차_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_초순수_3차_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_초순수_3차_min.Text) * 60;

                            txt_Sequence_초순수_3차_sec.Text = sec.ToString();
                            txt_Sequence_초순수_3차_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_초순수_4차_min" || name == "txt_Sequence_초순수_4차_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_초순수_4차_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_초순수_4차_min.Text) * 60;

                            txt_Sequence_초순수_4차_sec.Text = sec.ToString();
                            txt_Sequence_초순수_4차_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_과산화수소_1차_min" || name == "txt_Sequence_과산화수소_1차_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_과산화수소_1차_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_과산화수소_1차_min.Text) * 60;

                            txt_Sequence_과산화수소_1차_sec.Text = sec.ToString();
                            txt_Sequence_과산화수소_1차_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }
                        else if (name == "txt_Sequence_과산화수소_2차_min" || name == "txt_Sequence_과산화수소_2차_vol")
                        {
                            int vol = Convert.ToInt32(txt_Sequence_과산화수소_2차_vol.Text);
                            int sec = Convert.ToInt32(txt_Sequence_과산화수소_2차_min.Text) * 60;

                            txt_Sequence_과산화수소_2차_sec.Text = sec.ToString();
                            txt_Sequence_과산화수소_2차_mlPerSec.Text = $"{GlobalVariables.Form.Main.GetmlPerSecond(vol.ToString(), sec.ToString()):0.00}";

                            if (GlobalVariables.Form.Main.IsFillingSetup(vol.ToString(), sec.ToString()) == false)
                            {
                                GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}::{name}", "입력값을 확인해 주세요");
                            }
                        }

                        #endregion
                    }
                }
            }
        }

        private void btn_저장_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", "파라미터를 저장하시겠습니까?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (UpdateParameter())
                {
                    if (GlobalFunctions.SaveParameter())
                    {
                        GlobalFunctions.ViewParameter();

                        GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", "저장되었습니다");
                    }
                    else
                    {
                        GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", "저장에 실패하였습니다");
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", "파라미터 업데이트에 실패하였습니다");
                }
            }
        }

        private void btn_닫기_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;

            if (CheckUpdate())
            {
                result = GlobalFunctions.MessageBox($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", "변경사항이 발견되었습니다. 저장하지 않고 닫으시겠습니까?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes)
            {
                Close();
            }
        }

        private bool UpdateParameter()
        {
            bool result = false;
            try
            {
                #region Sequence

                GlobalVariables.Parameter.Sequence.test = chk_test.Checked;
                GlobalVariables.Parameter.Sequence.test_min = Convert.ToInt32(txt_test_min.Text);

                GlobalVariables.Parameter.Sequence.인산_vol = txt_Sequence_인산_vol.Text;
                GlobalVariables.Parameter.Sequence.인산_sec = txt_Sequence_인산_sec.Text;
                GlobalVariables.Parameter.Sequence.인산_min = Convert.ToInt32(txt_Sequence_인산_min.Text);
                GlobalVariables.Parameter.Sequence.인산_mlPerSec = Convert.ToDouble(txt_Sequence_인산_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.Recycle_sec = Convert.ToInt32(txt_Sequence_Recycle_sec.Text);

                GlobalVariables.Parameter.Sequence.흑연_rev = txt_Sequence_흑연_rev.Text;
                GlobalVariables.Parameter.Sequence.흑연_rpm = txt_Sequence_흑연_rpm.Text;
                GlobalVariables.Parameter.Sequence.흑연_min = Convert.ToInt32(txt_Sequence_흑연_min.Text);

                GlobalVariables.Parameter.Sequence.과망간산칼륨_rev = txt_Sequence_과망간산칼륨_rev.Text;
                GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm = txt_Sequence_과망간산칼륨_rpm.Text;
                GlobalVariables.Parameter.Sequence.과망간산칼륨_min = Convert.ToInt32(txt_Sequence_과망간산칼륨_min.Text);

                GlobalVariables.Parameter.Sequence.반응로_1차_승온_min = Convert.ToInt32(txt_Sequence_반응로_1차_승온_min.Text);
                GlobalVariables.Parameter.Sequence.반응로_2차_승온_min = Convert.ToInt32(txt_Sequence_반응로_2차_승온_min.Text);

                GlobalVariables.Parameter.Sequence.반응로_가열_min = Convert.ToInt32(txt_Sequence_반응로_가열_min.Text);

                GlobalVariables.Parameter.Sequence.반응로_온도_하강_min = Convert.ToInt32(txt_Sequence_반응로_온도_하강_min.Text);

                GlobalVariables.Parameter.Sequence.초순수_1차_vol = txt_Sequence_초순수_1차_vol.Text;
                GlobalVariables.Parameter.Sequence.초순수_1차_sec = txt_Sequence_초순수_1차_sec.Text;
                GlobalVariables.Parameter.Sequence.초순수_1차_min = Convert.ToInt32(txt_Sequence_초순수_1차_min.Text);
                GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec = Convert.ToDouble(txt_Sequence_초순수_1차_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.반응조확인 = Convert.ToDouble(txt_Sequence_반응조확인.Text);

                GlobalVariables.Parameter.Sequence.초순수_3차_vol = txt_Sequence_초순수_3차_vol.Text;
                GlobalVariables.Parameter.Sequence.초순수_3차_sec = txt_Sequence_초순수_3차_sec.Text;
                GlobalVariables.Parameter.Sequence.초순수_3차_min = Convert.ToInt32(txt_Sequence_초순수_3차_min.Text);
                GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec = Convert.ToDouble(txt_Sequence_초순수_3차_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.초순수_4차_vol = txt_Sequence_초순수_4차_vol.Text;
                GlobalVariables.Parameter.Sequence.초순수_4차_sec = txt_Sequence_초순수_4차_sec.Text;
                GlobalVariables.Parameter.Sequence.초순수_4차_min = Convert.ToInt32(txt_Sequence_초순수_4차_min.Text);
                GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec = Convert.ToDouble(txt_Sequence_초순수_4차_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.반응물_교반_min = Convert.ToInt32(txt_Sequence_반응물_교반_min.Text);

                GlobalVariables.Parameter.Sequence.과산화수소_1차_vol = txt_Sequence_과산화수소_1차_vol.Text;
                GlobalVariables.Parameter.Sequence.과산화수소_1차_sec = txt_Sequence_과산화수소_1차_sec.Text;
                GlobalVariables.Parameter.Sequence.과산화수소_1차_min = Convert.ToInt32(txt_Sequence_과산화수소_1차_min.Text);
                GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec = Convert.ToDouble(txt_Sequence_과산화수소_1차_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.과산화수소_2차_vol = txt_Sequence_과산화수소_2차_vol.Text;
                GlobalVariables.Parameter.Sequence.과산화수소_2차_sec = txt_Sequence_과산화수소_2차_sec.Text;
                GlobalVariables.Parameter.Sequence.과산화수소_2차_min = Convert.ToInt32(txt_Sequence_과산화수소_2차_min.Text);
                GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec = Convert.ToDouble(txt_Sequence_과산화수소_2차_mlPerSec.Text);

                GlobalVariables.Parameter.Sequence.skip_인산 = chk_Sequence_skip_인산.Checked;
                GlobalVariables.Parameter.Sequence.skip_흑연 = chk_Sequence_skip_흑연.Checked;
                GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 = chk_Sequence_skip_과망간산칼륨.Checked;

                GlobalVariables.Parameter.Sequence.교반기_rpm_인산 = Convert.ToInt32(txt_Sequence_교반기_rpm_인산.Text);
                GlobalVariables.Parameter.Sequence.교반기_rpm_흑연 = Convert.ToInt32(txt_Sequence_교반기_rpm_흑연.Text);
                GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨 = Convert.ToInt32(txt_Sequence_교반기_rpm_과망간산칼륨.Text);
                GlobalVariables.Parameter.Sequence.교반기_rpm_반응로 = Convert.ToInt32(txt_Sequence_교반기_rpm_반응로.Text);
                GlobalVariables.Parameter.Sequence.교반기_rpm_초순수 = Convert.ToInt32(txt_Sequence_교반기_rpm_초순수.Text);
                GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소 = Convert.ToInt32(txt_Sequence_교반기_rpm_과산화수소.Text);

                #endregion

                #region Check

                GlobalVariables.Parameter.CheckSequence.채널선택확인 = chk_Check_채널선택확인.Checked;

                GlobalVariables.Parameter.CheckSequence.헤드하강확인 = chk_Check_헤드하강확인.Checked;

                GlobalVariables.Parameter.CheckSequence.반응조센서확인 = chk_Check_반응조센서확인.Checked;

                GlobalVariables.Parameter.CheckSequence.반응조온도확인 = chk_Check_반응조온도확인.Checked;
                GlobalVariables.Parameter.CheckSequence.반응조온도 = Convert.ToDouble(txt_Check_반응조온도확인.Text);

                GlobalVariables.Parameter.CheckSequence.반응로온도확인 = chk_Check_반응로온도확인.Checked;
                GlobalVariables.Parameter.CheckSequence.반응로온도 = Convert.ToDouble(txt_Check_반응로온도확인.Text);

                #endregion

                #region Interlock

                GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열 = Convert.ToDouble(txt_Interlock_반응로승온또는가열.Text);

                GlobalVariables.Parameter.CheckInterlock.부반응물제거 = Convert.ToDouble(txt_Interlock_부반응물제거.Text);

                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                Log.Write($"frm_Parameter::{MethodBase.GetCurrentMethod().Name}", GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private bool CheckUpdate()
        {
            bool result = false;
            foreach (Control control in GlobalFunctions.GetControls(this))
            {
                result = control.BackColor == Color.Yellow;

                if (result)
                {
                    break;
                }
            }
            return result;
        }
    }
}
