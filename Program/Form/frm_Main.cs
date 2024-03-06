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
    public partial class frm_Main : Form
    {
        #region Thread

        private void ThreadStart()
        {
            _ThreadDIO = new System.Threading.Thread(Process_DIO);
            _ThreadDIO.IsBackground = true;
            _isThreadDIO = true;
            _ThreadDIO.Start();

            _ThreadSequence = new System.Threading.Thread(Process_Sequence);
            _ThreadSequence.IsBackground = true;
            _isThreadSequence = true;
            _ThreadSequence.Start();

            _ThreadTemperature = new System.Threading.Thread(Process_Temperature);
            _ThreadTemperature.IsBackground = true;
            _isThreadTemperature = true;
            _ThreadTemperature.Start();

            _ThreadSequenceSub = new System.Threading.Thread(Process_SequenceSub);
            _ThreadSequenceSub.IsBackground = true;
            _isThreadSequenceSub = true;
            _ThreadSequenceSub.Start();
        }

        private void ThreadStop()
        {
            _isThreadDIO = false;

            if (_isEntryDI || _isEntryDO)
            {
                System.Threading.Thread.Sleep(1000);
            }

            _isThreadSequence = false;

            _isThreadTemperature = false;

            _isThreadSequenceSub = false;
        }

        #endregion

        #region Timer

        private void TimerStart()
        {
            _TimerInterlock = new Timer();
            _TimerInterlock.Tick += new EventHandler(Tick_Interlock);
            _TimerInterlock.Interval = 1000;
            _TimerInterlock.Start();

            _TimerMain = new Timer();
            _TimerMain.Tick += new EventHandler(Tick_Main);
            _TimerMain.Interval = 100;
            _TimerMain.Start();
        }

        private void TimerStop()
        {
            if (_TimerInterlock != null)
            {
                if (_TimerInterlock.Enabled)
                {
                    _TimerInterlock.Stop();
                }

                _TimerInterlock.Dispose();
                _TimerInterlock = null;
            }

            if (_TimerMain != null)
            {
                if (_TimerMain.Enabled)
                {
                    _TimerMain.Stop();
                }

                _TimerMain.Dispose();
                _TimerMain = null;
            }
        }

        #endregion

        #region frm_Main

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

        public frm_Main()
        {
            InitializeComponent();
        }

        private void frm_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.Dispose();

            GlobalVariables.Form.Main = null;
        }
        private void frm_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GlobalVariables.Form.Main != null)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, $"CloseReason=[{e.CloseReason}]");

                e.Cancel = GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, "프로그램을 종료하시겠습니까?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;

                if (e.Cancel == false)
                {
                    TimerStop();

                    ThreadStop();

                    DeviceStop();

                    Log.Write(MethodBase.GetCurrentMethod().Name, "Program Close");
                }
            }
        }
        private void frm_Main_Load(object sender, EventArgs e)
        {
            if (GlobalFunctions.CheckProcess(System.Diagnostics.Process.GetCurrentProcess().ProcessName))
            {
                GlobalVariables.Form.Main = this;

                Log.MsgEvent += new MsgEventHandler(Log_MsgEvent);

                InitializeForm();

                if (c_Debug == false)
                {
                    DeviceStart();
                }

                ThreadStart();

                TimerStart();

                Log.Write(MethodBase.GetCurrentMethod().Name, $"Program Start - Version {Assembly.GetExecutingAssembly().GetName().Version}");

                if (GlobalFunctions.LoadParameter())
                {
                    GlobalFunctions.ViewParameter();
                }
                else
                {
                    GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, "파라미터 로드에 실패하였습니다. 파라미터 설정이 필요합니다.");
                }

                GlobalFunctions.DeleteLogFile();
            }
            else
            {
                GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, "프로그램이 이미 실행 중입니다");

                exitToolStripMenuItem_Click(null, null);
            }
        }

        #endregion

        private void Log_MsgEvent(Msg msg)
        {
            logListView1.AddListViewItem(msg);
        }

        private void splitter1_MouseUp(object sender, MouseEventArgs e)
        {
            panel1.Height -= e.Location.Y;
        }

        private void splitter1_DoubleClick(object sender, EventArgs e)
        {
            panel1.Height = 400;
        }

        #region MenuStrip

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logToolStripMenuItem.Checked = !logToolStripMenuItem.Checked;

            panel1.Visible = logToolStripMenuItem.Checked;

            if (panel1.Visible)
            {
                splitter1_DoubleClick(null, null);
            }
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (GlobalVariables.Form.Login = new frm_Login())
            {
                if (GlobalVariables.Form.Login.ShowDialog() == DialogResult.OK)
                {
                    if (GlobalVariables.IsLogin)
                    {
                        using (GlobalVariables.Form.Parameter = new frm_Parameter())
                        {
                            GlobalVariables.Form.Parameter.ShowDialog();
                        }
                    }
                }
            }
        }

        #endregion

        private void InitializeForm()
        {
            Size = new Size(1200, 1600);

            Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version}";

            tabControl1.ItemSize = new Size(0, 1);

            #region Control

            foreach (Control control in GlobalFunctions.GetControls(this))
            {
                if (control is ComboBox == false &&
                    control is LogListView == false &&
                    control is Splitter == false &&
                    control is TextBox == false
                   )
                {
                    control.BackColor = SystemColors.Control;
                }

                GlobalFunctions.DoubleBuffered(control, true);

                control.TabStop = false;
            }

            #endregion

            #region Status

            #region DI

            string[] strings = Enum.GetNames(typeof(e_DI));

            foreach (Control control in GlobalFunctions.GetControls(grp_Status_DI))
            {
                if (control is Label)
                {
                    int idx = Convert.ToInt32(control.Name.Substring(control.Name.LastIndexOf("_") + 1));

                    try
                    {
                        control.Text = strings[idx].Substring(9 - 1).Replace("_", " ");
                    }
                    catch
                    {
                        control.Text = "Spare";
                    }
                }
            }

            #endregion

            #region DO

            strings = Enum.GetNames(typeof(e_DO));

            foreach (Control control in GlobalFunctions.GetControls(grp_Status_DO))
            {
                if (control is Button)
                {
                    if (control.Text != c_Connect &&
                        control.Text != c_Disconnect
                       )
                    {
                        int idx = Convert.ToInt32(control.Name.Substring(control.Name.LastIndexOf("_") + 1));

                        try
                        {
                            control.Text = strings[idx].Substring(10 - 1).Replace("_", " ");
                        }
                        catch
                        {
                            control.Text = "Spare";
                        }
                    }
                }
            }

            #endregion

            #endregion

            #region Manual

            #region BackColor

            txt_Manual_WaterTank.BackColor = SystemColors.Highlight;
            txt_Manual_BLDC_CH1.BackColor = SystemColors.Highlight;
            txt_Manual_BLDC_CH2.BackColor = SystemColors.Highlight;
            txt_Manual_BLDC_CH3.BackColor = SystemColors.Highlight;
            txt_Manual_BLDC_CH4.BackColor = SystemColors.Highlight;
            txt_Manual_BLDC_CH5.BackColor = SystemColors.Highlight;
            txt_Manual_Servo_CH1.BackColor = SystemColors.Highlight;
            txt_Manual_Servo_CH2.BackColor = SystemColors.Highlight;
            txt_Manual_Servo_CH3.BackColor = SystemColors.Highlight;
            txt_Manual_Servo_CH4.BackColor = SystemColors.Highlight;
            txt_Manual_Servo_CH5.BackColor = SystemColors.Highlight;
            txt_Manual_NextPump_공급_Volume.BackColor = SystemColors.Highlight;
            txt_Manual_NextPump_공급_Time.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH1_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH1_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH2_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH2_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH3_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH3_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH4_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH4_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH5_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_흑연투입_CH5_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH1_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH1_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH2_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH2_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH3_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH3_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH4_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH4_RPM.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH5_rev.BackColor = SystemColors.Highlight;
            txt_Manual_Step_과망간산칼륨투입_CH5_RPM.BackColor = SystemColors.Highlight;

            #endregion

            lbl_Manual_NextPump_공급_SetupPer.Text = $"{GetmlPerSecond(txt_Manual_NextPump_공급_Volume.Text, txt_Manual_NextPump_공급_Time.Text):0.00} ml/sec";

            #endregion

            btn_TabPage_Click(btn_TabPage_2, null);
        }

        private void btn_TabPage_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(name.LastIndexOf("_") + 1));

            if ((e_TabPage)idx == e_TabPage.Status)
            {
                using (GlobalVariables.Form.Login = new frm_Login())
                {
                    if (GlobalVariables.Form.Login.ShowDialog() == DialogResult.OK)
                    {
                        if (GlobalVariables.IsLogin)
                        {
                            tabControl1.SelectedIndex = idx;

                            #region BackColor

                            btn_TabPage_0.BackColor = btn_TabPage_1.BackColor = btn_TabPage_2.BackColor = SystemColors.Control;

                            button.BackColor = Color.Lime;

                            #endregion
                        }
                    }
                }
            }
            else
            {
                tabControl1.SelectedIndex = idx;

                #region BackColor

                btn_TabPage_0.BackColor = btn_TabPage_1.BackColor = btn_TabPage_2.BackColor = SystemColors.Control;

                button.BackColor = Color.Lime;

                #endregion
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private const int c_ConnectTimeout = 1000;
        private const bool c_Debug = false; // TODO: Debug

        #region Device (Status)

        private const string c_Open = "Open";
        private const string c_Close = "Close";
        private const string c_Connect = "Connect";
        private const string c_Disconnect = "Disconnect";

        private async Task DeviceStart()
        {
            //if (ServoStart())
            //{
            //    for (int i = 0; i < Enum.GetNames((typeof(e_Channel))).Length; i++)
            //    {
            //        StartupServo((e_Channel)i);
            //    }
            //}

            if (BLDCStart())
            {
                StartupBLDC();
            }

            if (DIStart() && DOStart())
            {
                //에어제어(false);

                for (int i = 0; i < Enum.GetNames((typeof(e_Channel))).Length; i++)
                {
                    디스펜서에어배출((e_Channel)i, false);
                }
            }

            NextPumpStart();

            if (StepStart())
            {
                for (int i = 0; i < Enum.GetNames((typeof(e_Channel))).Length; i++)
                {
                    for (int j = 0; j < Enum.GetNames((typeof(e_Dispenser))).Length; j++)
                    {
                        StartupStep((e_Channel)i, (e_Dispenser)j);
                    }
                }
            }

            TemperatureStart();

            if (WaterTankStart())
            {
                StartupWaterTank();
            }

            if (ServoStart())
            {
                for (int i = 0; i < Enum.GetNames((typeof(e_Channel))).Length; i++)
                {
                    await StartupServo((e_Channel)i);
                }
            }
        }

        private void DeviceStop()
        {
            BLDCStop();

            DIStop();

            DOStop();

            TemperatureStop();

            WaterTankStop();

            ServoStop();

            StepStop();

            NextPumpStop();
        }

        #region BLDC

        private CBLDC _CBLDC = null;

        private bool BLDCStart()
        {
            bool result = false;
            try
            {
                if (_CBLDC == null)
                {
                    _CBLDC = new CBLDC();

                    _CBLDC.LogMsgEvent += new Library.LogMsgEventHandler(_CBLDC_LogMsgEvent);

                    _CBLDC.LogEnabled = true;

                    _CBLDC.Device = "BLDC";

                    _CBLDC.PortName = "COM4";
                    _CBLDC.BaudRate = 19200;
                    _CBLDC.Parity = System.IO.Ports.Parity.None;
                    _CBLDC.DataBits = 8;
                    _CBLDC.StopBits = System.IO.Ports.StopBits.One;

                    _CBLDC.Timeout = 1000;

                    result = _CBLDC.Open();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CBLDC_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void BLDCStop()
        {
            try
            {
                if (_CBLDC != null)
                {
                    _CBLDC.Dispose();
                    _CBLDC = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_BLDC_Open_Click(object sender, EventArgs e)
        {
            BLDCStart();
        }

        private void btn_Status_BLDC_Close_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            BLDCStop();
        }

        private async void btn_Status_BLDC_Driver_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_BLDC;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Status - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.모터제어(channel, CBLDC.e_Control.드라이버, CBLDC.e_Command.SET) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_BLDC_Driver_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_BLDC;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Status - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.모터제어(channel, CBLDC.e_Control.드라이버, CBLDC.e_Command.CLR) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_BLDC_Brake_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_BLDC;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Status - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.모터제어(channel, CBLDC.e_Control.브레이크, CBLDC.e_Command.SET) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_BLDC_Brake_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_BLDC;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Status - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.모터제어(channel, CBLDC.e_Control.브레이크, CBLDC.e_Command.CLR) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_BLDC_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_BLDC;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Status - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.모터제어(channel, CBLDC.e_Control.알람리셋) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region DI

        private enum e_DI
        {
            Input00_CH1_인산,
            Input01_CH1_과산화수소,
            Input02_CH1_초순수,
            Input03_CH1_실린더_Down,
            Input04_CH1_실린더_Up,
            Input05_CH2_인산,
            Input06_CH2_과산화수소,
            Input07_CH2_초순수,
            Input08_CH2_실린더_Down,
            Input09_CH2_실린더_Up,
            Input10_CH3_인산,
            Input11_CH3_과산화수소,
            Input12_CH3_초순수,
            Input13_CH3_실린더_Down,
            Input14_CH3_실린더_Up,
            Input15,
            Input16_CH4_인산,
            Input17_CH4_과산화수소,
            Input18_CH4_초순수,
            Input19_CH4_실린더_Down,
            Input20_CH4_실린더_Up,
            Input21_CH5_인산,
            Input22_CH5_과산화수소,
            Input23_CH5_초순수,
            Input24_CH5_실린더_Down,
            Input25_CH5_실린더_Up,
            Input26_CH1_반응조_센서,
            Input27_CH2_반응조_센서,
            Input28_CH3_반응조_센서,
            Input29_CH4_반응조_센서,
            Input30_CH5_반응조_센서,
            Input31_에어,
        }
        private bool[] _DI = new bool[Enum.GetNames(typeof(e_DI)).Length];

        private CDIO _CDI = null;

        private bool DIStart()
        {
            bool result = false;
            try
            {
                if (_CDI == null)
                {
                    _CDI = new CDIO();

                    _CDI.LogMsgEvent += new Library.LogMsgEventHandler(_CDI_LogMsgEvent);

                    _CDI.LogEnabled = true;

                    _CDI.Device = "DI";

                    _CDI.IP = "192.168.0.51";
                    _CDI.Port = 2001;
                    _CDI.ConnectTimeout = c_ConnectTimeout;

                    result = _CDI.Connect();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CDI_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void DIStop()
        {
            try
            {
                if (_CDI != null)
                {
                    _CDI.Dispose();
                    _CDI = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_DI_Connect_Click(object sender, EventArgs e)
        {
            DIStart();
        }

        private void btn_Status_DI_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            DIStop();
        }

        #endregion

        #region DO

        public enum e_DO
        {
            Output00_에어_ON,
            Output01_에어_OFF,
            Output02_CH1_실린더_Up,
            Output03_CH1_실린더_Down,
            Output04,
            Output05_CH2_실린더_Up,
            Output06_CH2_실린더_Down,
            Output07,
            Output08_CH3_실린더_Up,
            Output09_CH3_실린더_Down,
            Output10,
            Output11_CH4_실린더_Up,
            Output12_CH4_실린더_Down,
            Output13,
            Output14_CH5_실린더_Up,
            Output15_CH5_실린더_Down,
            Output16_CH1_디스펜서_에어_정지,
            Output17_CH1_디스펜서_에어_배출,
            Output18_CH2_디스펜서_에어_정지,
            Output19_CH2_디스펜서_에어_배출,
            Output20_CH3_디스펜서_에어_정지,
            Output21_CH3_디스펜서_에어_배출,
            Output22_CH4_디스펜서_에어_정지,
            Output23_CH4_디스펜서_에어_배출,
            Output24_CH5_디스펜서_에어_정지,
            Output25_CH5_디스펜서_에어_배출,
            Output26,
            Output27,
            Output28,
            Output29,
            Output30,
            Output31,
        }
        private bool[] _DO = new bool[Enum.GetNames(typeof(e_DO)).Length];

        private CDIO _CDO = null;

        private bool DOStart()
        {
            bool result = false;
            try
            {
                if (_CDO == null)
                {
                    _CDO = new CDIO();

                    _CDO.LogMsgEvent += new Library.LogMsgEventHandler(_CDO_LogMsgEvent);

                    _CDO.LogEnabled = true;

                    _CDO.Device = "DO";

                    _CDO.IP = "192.168.0.50";
                    _CDO.Port = 2001;
                    _CDO.ConnectTimeout = c_ConnectTimeout;

                    result = _CDO.Connect();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CDO_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void DOStop()
        {
            try
            {
                if (_CDO != null)
                {
                    _CDO.Dispose();
                    _CDO = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_DO_Connect_Click(object sender, EventArgs e)
        {
            DOStart();
        }

        private void btn_Status_DO_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            DOStop();
        }

        private void btn_Status_DO_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(name.LastIndexOf("_") + 1));

            _DO[idx] = !_DO[idx];
        }

        #endregion

        #region Temperature

        private double[] _Temperature = new double[Enum.GetNames(typeof(e_Channel)).Length];

        private CTemperature _CTemperature = null;

        private bool TemperatureStart()
        {
            bool result = false;
            try
            {
                if (_CTemperature == null)
                {
                    _CTemperature = new CTemperature();

                    _CTemperature.LogMsgEvent += new Library.LogMsgEventHandler(_CTemperature_LogMsgEvent);

                    _CTemperature.LogEnabled = true;

                    _CTemperature.Device = "Temperature";

                    _CTemperature.PortName = "COM7";
                    _CTemperature.BaudRate = 9600;
                    _CTemperature.Parity = System.IO.Ports.Parity.None;
                    _CTemperature.DataBits = 8;
                    _CTemperature.StopBits = System.IO.Ports.StopBits.One;

                    _CTemperature.Timeout = 1000;

                    _CTemperature.SlaveAddress = 1;

                    result = _CTemperature.Open();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CTemperature_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void TemperatureStop()
        {
            try
            {
                if (_CTemperature != null)
                {
                    _CTemperature.Dispose();
                    _CTemperature = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_Temperature_Open_Click(object sender, EventArgs e)
        {
            TemperatureStart();
        }

        private void btn_Status_Temperature_Close_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            TemperatureStop();
        }

        #endregion

        #region WaterTank

        private double _WaterTank_Targ = 0;
        private double _WaterTank_Curr = 0;

        private CWaterTank _CWaterTank = null;

        private bool WaterTankStart()
        {
            bool result = false;
            try
            {
                if (_CWaterTank == null)
                {
                    _CWaterTank = new CWaterTank();

                    _CWaterTank.LogMsgEvent += new Library.LogMsgEventHandler(_CWaterTank_LogMsgEvent);

                    _CWaterTank.LogEnabled = true;

                    _CWaterTank.Device = "WaterTank";

                    _CWaterTank.PortName = "COM1";
                    _CWaterTank.BaudRate = 9600;
                    _CWaterTank.Parity = System.IO.Ports.Parity.None;
                    _CWaterTank.DataBits = 8;
                    _CWaterTank.StopBits = System.IO.Ports.StopBits.One;

                    _CWaterTank.Timeout = 1000;

                    result = _CWaterTank.Open();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CWaterTank_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void WaterTankStop()
        {
            try
            {
                if (_CWaterTank != null)
                {
                    _CWaterTank.Dispose();
                    _CWaterTank = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_WaterTank_Open_Click(object sender, EventArgs e)
        {
            WaterTankStart();
        }

        private void btn_Status_WaterTank_Close_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            WaterTankStop();
        }

        private async void btn_Status_WaterTank_Start_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_WaterTank;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;

            string call = $"Status - WaterTank {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CWaterTank.Start() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_WaterTank_Stop_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_WaterTank;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Open) == false &&
                        control.Name.Contains(c_Close) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;

            string call = $"Status - WaterTank {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CWaterTank.Stop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Servo

        private readonly int[] _ServoIPTable = new int[] { 33, 34, 35, 36, 37 };

        private CServo[] _CServo = new CServo[Enum.GetNames(typeof(e_Channel)).Length];

        private bool ServoStart()
        {
            bool result = false;
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    result = ServoStart((e_Channel)i);

                    //if (result == false)
                    //{
                    //    break;
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }
        private bool ServoStart(e_Channel channel)
        {
            bool result = false;
            try
            {
                if (_CServo[(int)channel] == null)
                {
                    _CServo[(int)channel] = new CServo();

                    _CServo[(int)channel].LogMsgEvent += new Library.LogMsgEventHandler(_CServo_LogMsgEvent);

                    _CServo[(int)channel].LogEnabled = true;

                    _CServo[(int)channel].Device = $"Servo[{channel}]";

                    _CServo[(int)channel].IP = $"192.168.0.{_ServoIPTable[(int)channel]}";
                    _CServo[(int)channel].Port = 2001;
                    _CServo[(int)channel].ConnectTimeout = c_ConnectTimeout;
                    _CServo[(int)channel].ChannelIndex = (int)channel;

                    result = _CServo[(int)channel].Connect();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CServo_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void ServoStop()
        {
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    ServoStop((e_Channel)i);
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }
        private void ServoStop(e_Channel channel)
        {
            try
            {
                if (_CServo[(int)channel] != null)
                {
                    _CServo[(int)channel].Dispose();
                    _CServo[(int)channel] = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        #region CH1.

        private void btn_Status_Servo_CH1_Connect_Click(object sender, EventArgs e)
        {
            ServoStart(e_Channel.CH_01);
        }

        private void btn_Status_Servo_CH1_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            ServoStop(e_Channel.CH_01);
        }

        private async void btn_Status_Servo_CH1_Servo_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH1;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH1_Servo_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH1;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH1_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH1;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CServo[idx].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH1_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH1;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region CH2.

        private void btn_Status_Servo_CH2_Connect_Click(object sender, EventArgs e)
        {
            ServoStart(e_Channel.CH_02);
        }

        private void btn_Status_Servo_CH2_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            ServoStop(e_Channel.CH_02);
        }

        private async void btn_Status_Servo_CH2_Servo_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH2;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH2_Servo_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH2;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH2_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH2;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CServo[idx].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH2_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH2;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region CH3.

        private void btn_Status_Servo_CH3_Connect_Click(object sender, EventArgs e)
        {
            ServoStart(e_Channel.CH_03);
        }

        private void btn_Status_Servo_CH3_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            ServoStop(e_Channel.CH_03);
        }

        private async void btn_Status_Servo_CH3_Servo_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH3;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH3_Servo_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH3;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH3_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH3;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CServo[idx].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH3_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH3;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region CH4.

        private void btn_Status_Servo_CH4_Connect_Click(object sender, EventArgs e)
        {
            ServoStart(e_Channel.CH_04);
        }

        private void btn_Status_Servo_CH4_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            ServoStop(e_Channel.CH_04);
        }

        private async void btn_Status_Servo_CH4_Servo_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH4;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH4_Servo_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH4;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH4_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH4;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CServo[idx].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH4_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH4;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region CH5.

        private void btn_Status_Servo_CH5_Connect_Click(object sender, EventArgs e)
        {
            ServoStart(e_Channel.CH_05);
        }

        private void btn_Status_Servo_CH5_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            ServoStop(e_Channel.CH_05);
        }

        private async void btn_Status_Servo_CH5_Servo_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH5;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH5_Servo_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH5;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH5_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH5;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CServo[idx].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Servo_CH5_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Servo_CH5;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Status - Servo[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[idx].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region Step

        private readonly int[] _StepIPTable = new int[] { 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };

        private CStep[] _CStep = new CStep[Enum.GetNames(typeof(e_Channel)).Length * Enum.GetNames(typeof(e_Dispenser)).Length];

        private int GetStepIdx(e_Channel channel, e_Dispenser dispenser)
        {
            return Enum.GetNames(typeof(e_Dispenser)).Length * (int)channel + (int)dispenser;
        }

        private bool StepStart()
        {
            bool result = false;
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    for (int j = 0; j < Enum.GetNames(typeof(e_Dispenser)).Length; j++)
                    {
                        result = StepStart((e_Channel)i, (e_Dispenser)j);

                        //if (result == false)
                        //{
                        //    break;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }
        private bool StepStart(e_Channel channel, e_Dispenser dispenser)
        {
            bool result = false;
            try
            {
                if (_CStep[GetStepIdx(channel, dispenser)] == null)
                {
                    _CStep[GetStepIdx(channel, dispenser)] = new CStep();

                    _CStep[GetStepIdx(channel, dispenser)].LogMsgEvent += new Library.LogMsgEventHandler(_CStep_LogMsgEvent);

                    _CStep[GetStepIdx(channel, dispenser)].LogEnabled = true;

                    _CStep[GetStepIdx(channel, dispenser)].Device = $"Step[{channel}_{dispenser}]";

                    _CStep[GetStepIdx(channel, dispenser)].IP = $"192.168.0.{_StepIPTable[GetStepIdx(channel, dispenser)]}";
                    _CStep[GetStepIdx(channel, dispenser)].Port = 2001;
                    _CStep[GetStepIdx(channel, dispenser)].ConnectTimeout = c_ConnectTimeout;

                    result = _CStep[GetStepIdx(channel, dispenser)].Connect();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CStep_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void StepStop()
        {
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    for (int j = 0; j < Enum.GetNames(typeof(e_Dispenser)).Length; j++)
                    {
                        StepStop((e_Channel)i, (e_Dispenser)j);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }
        private void StepStop(e_Channel channel, e_Dispenser dispenser)
        {
            try
            {
                if (_CStep[GetStepIdx(channel, dispenser)] != null)
                {
                    _CStep[GetStepIdx(channel, dispenser)].Dispose();
                    _CStep[GetStepIdx(channel, dispenser)] = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        #region CH1.

        #region 흑연

        private void btn_Status_Step_CH1_흑연_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_01, e_Dispenser.흑연);
        }

        private void btn_Status_Step_CH1_흑연_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_01, e_Dispenser.흑연);
        }

        private async void btn_Status_Step_CH1_흑연_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_흑연_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_흑연_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_흑연_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region 과망간산칼륨

        private void btn_Status_Step_CH1_과망간산칼륨_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_01, e_Dispenser.과망간산칼륨);
        }

        private void btn_Status_Step_CH1_과망간산칼륨_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_01, e_Dispenser.과망간산칼륨);
        }

        private async void btn_Status_Step_CH1_과망간산칼륨_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_과망간산칼륨_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_과망간산칼륨_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH1_과망간산칼륨_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH1_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region CH2.

        #region 흑연

        private void btn_Status_Step_CH2_흑연_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_02, e_Dispenser.흑연);
        }

        private void btn_Status_Step_CH2_흑연_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_02, e_Dispenser.흑연);
        }

        private async void btn_Status_Step_CH2_흑연_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_흑연_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_흑연_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_흑연_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region 과망간산칼륨

        private void btn_Status_Step_CH2_과망간산칼륨_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_02, e_Dispenser.과망간산칼륨);
        }

        private void btn_Status_Step_CH2_과망간산칼륨_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_02, e_Dispenser.과망간산칼륨);
        }

        private async void btn_Status_Step_CH2_과망간산칼륨_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_과망간산칼륨_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_과망간산칼륨_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH2_과망간산칼륨_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH2_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region CH3.

        #region 흑연

        private void btn_Status_Step_CH3_흑연_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_03, e_Dispenser.흑연);
        }

        private void btn_Status_Step_CH3_흑연_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_03, e_Dispenser.흑연);
        }

        private async void btn_Status_Step_CH3_흑연_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_흑연_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_흑연_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_흑연_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region 과망간산칼륨

        private void btn_Status_Step_CH3_과망간산칼륨_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_03, e_Dispenser.과망간산칼륨);
        }

        private void btn_Status_Step_CH3_과망간산칼륨_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_03, e_Dispenser.과망간산칼륨);
        }

        private async void btn_Status_Step_CH3_과망간산칼륨_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_과망간산칼륨_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_과망간산칼륨_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH3_과망간산칼륨_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH3_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region CH4.

        #region 흑연

        private void btn_Status_Step_CH4_흑연_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_04, e_Dispenser.흑연);
        }

        private void btn_Status_Step_CH4_흑연_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_04, e_Dispenser.흑연);
        }

        private async void btn_Status_Step_CH4_흑연_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_흑연_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_흑연_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_흑연_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region 과망간산칼륨

        private void btn_Status_Step_CH4_과망간산칼륨_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_04, e_Dispenser.과망간산칼륨);
        }

        private void btn_Status_Step_CH4_과망간산칼륨_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_04, e_Dispenser.과망간산칼륨);
        }

        private async void btn_Status_Step_CH4_과망간산칼륨_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_과망간산칼륨_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_과망간산칼륨_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH4_과망간산칼륨_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH4_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region CH5.

        #region 흑연

        private void btn_Status_Step_CH5_흑연_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_05, e_Dispenser.흑연);
        }

        private void btn_Status_Step_CH5_흑연_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_05, e_Dispenser.흑연);
        }

        private async void btn_Status_Step_CH5_흑연_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_흑연_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_흑연_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_흑연_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_흑연;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #region 과망간산칼륨

        private void btn_Status_Step_CH5_과망간산칼륨_Connect_Click(object sender, EventArgs e)
        {
            StepStart(e_Channel.CH_05, e_Dispenser.과망간산칼륨);
        }

        private void btn_Status_Step_CH5_과망간산칼륨_Disconnect_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            StepStop(e_Channel.CH_05, e_Dispenser.과망간산칼륨);
        }

        private async void btn_Status_Step_CH5_과망간산칼륨_Step_ON_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(true) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_과망간산칼륨_Step_OFF_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoEnable(false) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_과망간산칼륨_GetAlarm_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string alarm = string.Empty;

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].GetAlarmType(ref alarm) ? Color.Lime : SystemColors.Control; });

                if (string.IsNullOrEmpty(alarm) == false)
                {
                    GlobalFunctions.MessageBox(call, alarm);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        private async void btn_Status_Step_CH5_과망간산칼륨_AlarmReset_Click(object sender, EventArgs e)
        {
            #region Control

            Control container = grp_Status_Step_CH5_과망간산칼륨;

            foreach (Control control in GlobalFunctions.GetControls(container))
            {
                if (control is Button)
                {
                    if (control.Name.Contains(c_Connect) == false &&
                        control.Name.Contains(c_Disconnect) == false
                       )
                    {
                        control.BackColor = SystemColors.Control;
                    }

                    control.Enabled = false;
                }
            }

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;
            bool check = name.Contains(e_Dispenser.흑연.ToString());

            e_Channel channel = (e_Channel)idx;
            e_Dispenser dispenser = check ? e_Dispenser.흑연 : e_Dispenser.과망간산칼륨;

            string call = $"Status - Step[{channel}_{dispenser}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                foreach (Control control in GlobalFunctions.GetControls(container))
                {
                    if (control is Button)
                    {
                        control.Enabled = true;
                    }
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #endregion

        #region NextPump

        private CNextPump _CNextPump = null;

        private bool NextPumpStart()
        {
            bool result = false;
            try
            {
                if (_CNextPump == null)
                {
                    _CNextPump = new CNextPump();

                    _CNextPump.LogMsgEvent += new Library.LogMsgEventHandler(_CNextPump_LogMsgEvent);

                    _CNextPump.LogEnabled = true;

                    _CNextPump.Device = "NextPump";

                    _CNextPump.PortName = "COM2";
                    _CNextPump.BaudRate = 9600;
                    _CNextPump.Parity = System.IO.Ports.Parity.None;
                    _CNextPump.DataBits = 8;
                    _CNextPump.StopBits = System.IO.Ports.StopBits.One;

                    _CNextPump.Timeout = 1000;

                    _CNextPump.FillingSetupInterval = 500;

                    result = _CNextPump.Open();
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void _CNextPump_LogMsgEvent(string call, string text)
        {
            Log.Write(call, text);
        }

        private void NextPumpStop()
        {
            try
            {
                if (_CNextPump != null)
                {
                    _CNextPump.Dispose();
                    _CNextPump = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void btn_Status_NextPump_Open_Click(object sender, EventArgs e)
        {
            NextPumpStart();
        }

        private void btn_Status_NextPump_Close_Click(object sender, EventArgs e)
        {
            if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_통신종료, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            NextPumpStop();
        }

        #endregion

        #endregion

        #region Manual

        private void txt_Manual_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;
            string name = label.Name;

            #region min, max

            double min = 0;
            double max = 0;

            if (name.Contains("WaterTank"))
            {
                min = 0;
                max = 50;
            }
            else if (name.Contains("BLDC"))
            {
                min = 0;
                max = 600;
            }
            else if (name.Contains("Servo"))
            {
                min = 0;
                max = 595;
            }

            #endregion

            if (GlobalVariables.Form.Keypad == null)
            {
                if (name.Contains("WaterTank") ||
                    name.Contains("BLDC") ||
                    name.Contains("Servo")
                   )
                {
                    GlobalVariables.Form.Keypad = new frm_Keypad(min, max);
                }
                else
                {
                    GlobalVariables.Form.Keypad = new frm_Keypad();
                }

                if (GlobalVariables.Form.Keypad.ShowDialog() == DialogResult.OK)
                {
                    if (name.Contains("BLDC") ||
                        name.Contains("Servo") ||
                        name.Contains("Step")
                       )
                    {
                        int idx = GlobalVariables.KeypadValue.IndexOf(".");

                        if (idx >= 0)
                        {
                            GlobalVariables.KeypadValue = GlobalVariables.KeypadValue.Remove(idx);
                        }
                    }

                    label.Text = GlobalVariables.KeypadValue;

                    if (name == "txt_Manual_NextPump_공급_Volume" ||
                        name == "txt_Manual_NextPump_공급_Time"
                       )
                    {
                        string ml = txt_Manual_NextPump_공급_Volume.Text;
                        string sec = txt_Manual_NextPump_공급_Time.Text;

                        lbl_Manual_NextPump_공급_Setup.Visible = !IsFillingSetup(ml, sec);
                        lbl_Manual_NextPump_공급_SetupPer.Text = $"{GetmlPerSecond(ml, sec):0.00} ml/sec";
                    }
                }
            }
        }

        public double GetmlPerSecond(string ml, string sec)
        {
            double volume = double.Parse(ml) * 100;
            double time = double.Parse(sec) * 10;
            return volume / time / 10;
        }

        public bool IsFillingSetup(string ml, string sec)
        {
            return GetmlPerSecond(ml, sec) < 17;
        }

        private async void btn_Manual_WaterTank_설정_Click(object sender, EventArgs e)
        {
            #region Control

            btn_Manual_WaterTank_설정.Enabled = false;

            #endregion

            Button button = sender as Button;

            string call = $"Manual - WaterTank {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CWaterTank.SetTemperature(txt_Manual_WaterTank.Text) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                btn_Manual_WaterTank_설정.Enabled = true;

                #endregion
            }
        }

        private async void btn_Manual_에어제어_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_에어제어", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_에어제어", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;

            string call = $"Manual - 에어제어 {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                bool command = name.Substring(name.LastIndexOf("_") + 1) == "ON";

                button.BackColor = await 에어제어(command) ? Color.Lime : Color.Red;
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_에어제어", true);

                #endregion
            }
        }

        #region BLDC

        private async void btn_Manual_BLDC_회전_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Manual - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                int rpm = 0;

                switch (channel)
                {
                    case CBLDC.e_Channel.CH01: rpm = Convert.ToInt32(txt_Manual_BLDC_CH1.Text); break;
                    case CBLDC.e_Channel.CH02: rpm = Convert.ToInt32(txt_Manual_BLDC_CH2.Text); break;
                    case CBLDC.e_Channel.CH03: rpm = Convert.ToInt32(txt_Manual_BLDC_CH3.Text); break;
                    case CBLDC.e_Channel.CH04: rpm = Convert.ToInt32(txt_Manual_BLDC_CH4.Text); break;
                    case CBLDC.e_Channel.CH05: rpm = Convert.ToInt32(txt_Manual_BLDC_CH5.Text); break;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.정방향속도(channel, rpm) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", true);

                #endregion
            }
        }

        private async void btn_Manual_BLDC_정지_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(19 - 1, 1)) - 1;

            CBLDC.e_Channel channel = (CBLDC.e_Channel)idx;

            string call = $"Manual - BLDC[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CBLDC.정방향속도(channel, 0) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_BLDC", true);

                #endregion
            }
        }

        #endregion

        private async void btn_Manual_디스펜서에어배출_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(23 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_디스펜서에어배출_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_디스펜서에어배출_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - 디스펜서에어배출[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                bool command = name.Substring(name.LastIndexOf("_") + 1) == "ON";

                button.BackColor = await 디스펜서에어배출(channel, command) ? Color.Lime : Color.Red;
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_디스펜서에어배출_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_실린더제어_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_실린더제어_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_실린더제어_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - 실린더제어[{channel}] {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                bool command = name.Substring(name.LastIndexOf("_") + 1) == "Up";

                button.BackColor = await 실린더제어(channel, command) ? Color.Lime : Color.Red;
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_실린더제어_CH{idx + 1}", true);

                #endregion
            }
        }

        #region Servo

        private async void btn_Manual_Servo_이동_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Servo[{channel}] {button.Text} Click";

            try
            {
                if (_CServo[(int)channel].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.SERVOON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.ORIGINRETOK == false)
                {
                    GlobalFunctions.MessageBox(call, "원점 이동 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                #region get pos

                string pos = string.Empty;

                switch (channel)
                {
                    case e_Channel.CH_01: pos = txt_Manual_Servo_CH1.Text; break;
                    case e_Channel.CH_02: pos = txt_Manual_Servo_CH2.Text; break;
                    case e_Channel.CH_03: pos = txt_Manual_Servo_CH3.Text; break;
                    case e_Channel.CH_04: pos = txt_Manual_Servo_CH4.Text; break;
                    case e_Channel.CH_05: pos = txt_Manual_Servo_CH5.Text; break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CServo[(int)channel].MoveAbsPos(pos, "120") ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Servo_정지_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Servo[{channel}] {button.Text} Click";

            try
            {
                if (_CServo[(int)channel].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.SERVOON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[(int)channel].MoveStop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Servo_원점이동_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(20 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Servo[{channel}] {button.Text} Click";

            try
            {
                if (_CServo[(int)channel].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.SERVOON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CServo[(int)channel].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CServo[(int)channel].MoveOrigin() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{idx + 1}", true);

                #endregion
            }
        }

        #endregion

        private void SetBackColor(Control[] controls, string value, Color color)
        {
            try
            {
                foreach (Control control in controls)
                {
                    if (control.Name.Contains(value))
                    {
                        control.BackColor = color;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void SetEnabled(Control[] controls, string value, bool enable)
        {
            try
            {
                foreach (Control control in controls)
                {
                    if (control.Name.Contains(value))
                    {
                        control.Enabled = enable;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        #region NextPump

        private async void btn_Manual_NextPump_비우기정지_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_채우기정지_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CNextPump.FillingStop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_인산공급_채우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(28 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string ml = txt_Manual_NextPump_공급_Volume.Text;
                string sec = txt_Manual_NextPump_공급_Time.Text;

                if (IsFillingSetup(ml, sec))
                {
                    await Task.Run(() => _CNextPump.FillingStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    switch (channel)
                    {
                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true; break;
                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true; break;
                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true; break;
                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true; break;
                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true; break;
                    }

                    #endregion

                    if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                    {
                        await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                    }
                    else
                    {
                        button.BackColor = Color.Red;

                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_인산공급_비우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(28 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => _CNextPump.RecycleStop());

                #region Channel 선택

                Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                switch (channel)
                {
                    case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true; break;
                    case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true; break;
                    case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true; break;
                    case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true; break;
                    case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true; break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_초순수공급_채우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(29 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string ml = txt_Manual_NextPump_공급_Volume.Text;
                string sec = txt_Manual_NextPump_공급_Time.Text;

                if (IsFillingSetup(ml, sec))
                {
                    await Task.Run(() => _CNextPump.FillingStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    switch (channel)
                    {
                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true; break;
                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true; break;
                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true; break;
                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true; break;
                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true; break;
                    }

                    #endregion

                    if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                    {
                        await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                    }
                    else
                    {
                        button.BackColor = Color.Red;

                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_초순수공급_비우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(29 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => _CNextPump.RecycleStop());

                #region Channel 선택

                Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                switch (channel)
                {
                    case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true; break;
                    case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true; break;
                    case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true; break;
                    case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true; break;
                    case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true; break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_과산화수소공급_채우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(31 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string ml = txt_Manual_NextPump_공급_Volume.Text;
                string sec = txt_Manual_NextPump_공급_Time.Text;

                if (IsFillingSetup(ml, sec))
                {
                    await Task.Run(() => _CNextPump.FillingStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    switch (channel)
                    {
                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true; break;
                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true; break;
                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true; break;
                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true; break;
                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true; break;
                    }

                    #endregion

                    if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                    {
                        await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                    }
                    else
                    {
                        button.BackColor = Color.Red;

                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_과산화수소공급_비우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(31 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => _CNextPump.RecycleStop());

                #region Channel 선택

                Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                switch (channel)
                {
                    case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true; break;
                    case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true; break;
                    case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true; break;
                    case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true; break;
                    case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true; break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_공급_채우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(26 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string ml = txt_Manual_NextPump_공급_Volume.Text;
                string sec = txt_Manual_NextPump_공급_Time.Text;

                if (IsFillingSetup(ml, sec))
                {
                    await Task.Run(() => _CNextPump.FillingStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    switch (channel)
                    {
                        case e_Channel.CH_01:
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true;
                            break;
                        case e_Channel.CH_02:
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true;
                            break;
                        case e_Channel.CH_03:
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true;
                            break;
                        case e_Channel.CH_04:
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true;
                            break;
                        case e_Channel.CH_05:
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;
                            _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true;
                            break;
                    }

                    #endregion

                    if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                    {
                        await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                    }
                    else
                    {
                        button.BackColor = Color.Red;

                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_공급_비우기_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(26 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => _CNextPump.RecycleStop());

                #region Channel 선택

                Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                switch (channel)
                {
                    case e_Channel.CH_01:
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true;
                        break;
                    case e_Channel.CH_02:
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true;
                        break;
                    case e_Channel.CH_03:
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true;
                        break;
                    case e_Channel.CH_04:
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true;
                        break;
                    case e_Channel.CH_05:
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true;
                        break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        private async void btn_Manual_NextPump_전부_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;
            string name = button.Name;

            string call = $"Manual - NextPump {button.Text} Click";

            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                if (name.Contains("채우기"))
                {
                    string ml = txt_Manual_NextPump_공급_Volume.Text;
                    string sec = txt_Manual_NextPump_공급_Time.Text;

                    if (IsFillingSetup(ml, sec))
                    {
                        await Task.Run(() => _CNextPump.FillingStop());

                        #region Channel 선택

                        Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true;
                        _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true;

                        #endregion

                        if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                        {
                            await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                        }
                        else
                        {
                            button.BackColor = Color.Red;

                            GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                        }
                    }
                    else
                    {
                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                    }
                }
                else
                {
                    await Task.Run(() => _CNextPump.RecycleStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true;

                    #endregion

                    await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        #endregion

        #region Step

        private async void btn_Manual_Step_흑연투입_투입_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(24 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.흑연}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                #region rev, rpm

                string rev = string.Empty;
                string rpm = string.Empty;

                switch (channel)
                {
                    case e_Channel.CH_01:
                        rev = txt_Manual_Step_흑연투입_CH1_rev.Text;
                        rpm = txt_Manual_Step_흑연투입_CH1_RPM.Text;
                        break;
                    case e_Channel.CH_02:
                        rev = txt_Manual_Step_흑연투입_CH2_rev.Text;
                        rpm = txt_Manual_Step_흑연투입_CH2_RPM.Text;
                        break;
                    case e_Channel.CH_03:
                        rev = txt_Manual_Step_흑연투입_CH3_rev.Text;
                        rpm = txt_Manual_Step_흑연투입_CH3_RPM.Text;
                        break;
                    case e_Channel.CH_04:
                        rev = txt_Manual_Step_흑연투입_CH4_rev.Text;
                        rpm = txt_Manual_Step_흑연투입_CH4_RPM.Text;
                        break;
                    case e_Channel.CH_05:
                        rev = txt_Manual_Step_흑연투입_CH5_rev.Text;
                        rpm = txt_Manual_Step_흑연투입_CH5_RPM.Text;
                        break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, e_Dispenser.흑연)].MoveIncPos(rev, rpm) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Step_흑연투입_정지_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(24 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.흑연}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, e_Dispenser.흑연)].MoveStop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Step_흑연투입_Action_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(24 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.흑연}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.흑연)].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                button.BackColor = await StepAction(channel, e_Dispenser.흑연) ? Color.Lime : Color.Red;
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_흑연투입_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Step_과망간산칼륨투입_투입_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(28 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.과망간산칼륨}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                #region rev, rpm

                string rev = string.Empty;
                string rpm = string.Empty;

                switch (channel)
                {
                    case e_Channel.CH_01:
                        rev = txt_Manual_Step_과망간산칼륨투입_CH1_rev.Text;
                        rpm = txt_Manual_Step_과망간산칼륨투입_CH1_RPM.Text;
                        break;
                    case e_Channel.CH_02:
                        rev = txt_Manual_Step_과망간산칼륨투입_CH2_rev.Text;
                        rpm = txt_Manual_Step_과망간산칼륨투입_CH2_RPM.Text;
                        break;
                    case e_Channel.CH_03:
                        rev = txt_Manual_Step_과망간산칼륨투입_CH3_rev.Text;
                        rpm = txt_Manual_Step_과망간산칼륨투입_CH3_RPM.Text;
                        break;
                    case e_Channel.CH_04:
                        rev = txt_Manual_Step_과망간산칼륨투입_CH4_rev.Text;
                        rpm = txt_Manual_Step_과망간산칼륨투입_CH4_RPM.Text;
                        break;
                    case e_Channel.CH_05:
                        rev = txt_Manual_Step_과망간산칼륨투입_CH5_rev.Text;
                        rpm = txt_Manual_Step_과망간산칼륨투입_CH5_RPM.Text;
                        break;
                }

                #endregion

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].MoveIncPos(rev, rpm) ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Step_과망간산칼륨투입_정지_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(28 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.과망간산칼륨}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => { button.BackColor = _CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].MoveStop() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", true);

                #endregion
            }
        }

        private async void btn_Manual_Step_과망간산칼륨투입_Action_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(28 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", false);

            #endregion

            string call = $"Manual - Step[{channel}_{e_Dispenser.과망간산칼륨}] {button.Text} Click";

            try
            {
                if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.ERRORALL)
                {
                    GlobalFunctions.MessageBox(call, "서보 에러 상태입니다. 에러 해제 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.STEPON == false)
                {
                    GlobalFunctions.MessageBox(call, "서보 ON 후 재시도 해주세요");

                    return;
                }
                else if (_CStep[GetStepIdx(channel, e_Dispenser.과망간산칼륨)].Status.MOTIONING)
                {
                    GlobalFunctions.MessageBox(call, "서보 이동 완료 후 재시도 해주세요");

                    return;
                }

                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                button.BackColor = await StepAction(channel, e_Dispenser.과망간산칼륨) ? Color.Lime : Color.Red;
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Step_과망간산칼륨투입_CH{idx + 1}", true);

                #endregion
            }
        }

        #endregion

        #endregion

        #region Task

        private enum e_TaskStep
        {
            Step_01,
            Step_02,
            Step_03,
            Step_04,
            Step_05,
            Step_06,
            Step_07,
            Step_08,
            Step_09,
            Step_10,
            Step_11,
            Step_12,
            Step_13,
            Step_14,
            Step_15,
            Step_16,
            Step_17,
            Step_18,
            Step_19,
            Step_20,
        }

        /// <param name="timeout">second</param>
        private async Task<bool> method(int timeout = 10)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            break;
                        case e_TaskStep.Step_02:
                            break;
                        case e_TaskStep.Step_03:
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        /// <param name="command">ON/OFF</param>
        /// <param name="timeout">second</param>
        private async Task<bool> 에어제어(bool command, int timeout = 10)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            _DO[(int)e_DO.Output00_에어_ON ] = false;
                            _DO[(int)e_DO.Output01_에어_OFF] = false;

                            step = command ? e_TaskStep.Step_02 : e_TaskStep.Step_03;
                            break;
                        case e_TaskStep.Step_02: // ON
                            _DO[(int)e_DO.Output00_에어_ON] = true;

                            step = e_TaskStep.Step_20;
                            break;
                        case e_TaskStep.Step_03: // OFF
                            _DO[(int)e_DO.Output01_에어_OFF] = true;

                            step = e_TaskStep.Step_20;
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        /// <param name="channel">channel</param>
        /// <param name="command">ON/OFF</param>
        /// <param name="timeout">second</param>
        private async Task<bool> 디스펜서에어배출(e_Channel channel, bool command, int timeout = 10)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            switch (channel)
                            {
                                case e_Channel.CH_01:
                                    _DO[(int)e_DO.Output16_CH1_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output17_CH1_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_02:
                                    _DO[(int)e_DO.Output18_CH2_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output19_CH2_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_03:
                                    _DO[(int)e_DO.Output20_CH3_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output21_CH3_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_04:
                                    _DO[(int)e_DO.Output22_CH4_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output23_CH4_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_05:
                                    _DO[(int)e_DO.Output24_CH5_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output25_CH5_디스펜서_에어_배출] = false;
                                    break;
                            }

                            step = command ? e_TaskStep.Step_02 : e_TaskStep.Step_03;
                            break;
                        case e_TaskStep.Step_02: // ON
                            switch (channel)
                            {
                                case e_Channel.CH_01: _DO[(int)e_DO.Output17_CH1_디스펜서_에어_배출] = true; break;
                                case e_Channel.CH_02: _DO[(int)e_DO.Output19_CH2_디스펜서_에어_배출] = true; break;
                                case e_Channel.CH_03: _DO[(int)e_DO.Output21_CH3_디스펜서_에어_배출] = true; break;
                                case e_Channel.CH_04: _DO[(int)e_DO.Output23_CH4_디스펜서_에어_배출] = true; break;
                                case e_Channel.CH_05: _DO[(int)e_DO.Output25_CH5_디스펜서_에어_배출] = true; break;
                            }

                            step = e_TaskStep.Step_04;
                            break;
                        case e_TaskStep.Step_03: // OFF
                            switch (channel)
                            {
                                case e_Channel.CH_01: _DO[(int)e_DO.Output16_CH1_디스펜서_에어_정지] = true; break;
                                case e_Channel.CH_02: _DO[(int)e_DO.Output18_CH2_디스펜서_에어_정지] = true; break;
                                case e_Channel.CH_03: _DO[(int)e_DO.Output20_CH3_디스펜서_에어_정지] = true; break;
                                case e_Channel.CH_04: _DO[(int)e_DO.Output22_CH4_디스펜서_에어_정지] = true; break;
                                case e_Channel.CH_05: _DO[(int)e_DO.Output24_CH5_디스펜서_에어_정지] = true; break;
                            }

                            step = e_TaskStep.Step_04;
                            break;
                        case e_TaskStep.Step_04: // Delay
                            await Task.Delay(1000);

                            step = e_TaskStep.Step_05;
                            break;
                        case e_TaskStep.Step_05:
                            switch (channel)
                            {
                                case e_Channel.CH_01:
                                    _DO[(int)e_DO.Output16_CH1_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output17_CH1_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_02:
                                    _DO[(int)e_DO.Output18_CH2_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output19_CH2_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_03:
                                    _DO[(int)e_DO.Output20_CH3_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output21_CH3_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_04:
                                    _DO[(int)e_DO.Output22_CH4_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output23_CH4_디스펜서_에어_배출] = false;
                                    break;
                                case e_Channel.CH_05:
                                    _DO[(int)e_DO.Output24_CH5_디스펜서_에어_정지] = false;
                                    _DO[(int)e_DO.Output25_CH5_디스펜서_에어_배출] = false;
                                    break;
                            }

                            step = e_TaskStep.Step_20;
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        /// <param name="channel">channel</param>
        /// <param name="command">Up/Down</param>
        /// <param name="timeout">second</param>
        private async Task<bool> 실린더제어(e_Channel channel, bool command, int timeout = 10)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            switch (channel)
                            {
                                case e_Channel.CH_01:
                                    _DO[(int)e_DO.Output02_CH1_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output03_CH1_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_02:
                                    _DO[(int)e_DO.Output05_CH2_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output06_CH2_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_03:
                                    _DO[(int)e_DO.Output08_CH3_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output09_CH3_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_04:
                                    _DO[(int)e_DO.Output11_CH4_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output12_CH4_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_05:
                                    _DO[(int)e_DO.Output14_CH5_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output15_CH5_실린더_Down] = false;
                                    break;
                            }

                            step = command ? e_TaskStep.Step_02 : e_TaskStep.Step_03;
                            break;
                        case e_TaskStep.Step_02: // Up
                            switch (channel)
                            {
                                case e_Channel.CH_01: _DO[(int)e_DO.Output02_CH1_실린더_Up] = true; break;
                                case e_Channel.CH_02: _DO[(int)e_DO.Output05_CH2_실린더_Up] = true; break;
                                case e_Channel.CH_03: _DO[(int)e_DO.Output08_CH3_실린더_Up] = true; break;
                                case e_Channel.CH_04: _DO[(int)e_DO.Output11_CH4_실린더_Up] = true; break;
                                case e_Channel.CH_05: _DO[(int)e_DO.Output14_CH5_실린더_Up] = true; break;
                            }

                            step = e_TaskStep.Step_04;
                            break;
                        case e_TaskStep.Step_03: // Down
                            switch (channel)
                            {
                                case e_Channel.CH_01: _DO[(int)e_DO.Output03_CH1_실린더_Down] = true; break;
                                case e_Channel.CH_02: _DO[(int)e_DO.Output06_CH2_실린더_Down] = true; break;
                                case e_Channel.CH_03: _DO[(int)e_DO.Output09_CH3_실린더_Down] = true; break;
                                case e_Channel.CH_04: _DO[(int)e_DO.Output12_CH4_실린더_Down] = true; break;
                                case e_Channel.CH_05: _DO[(int)e_DO.Output15_CH5_실린더_Down] = true; break;
                            }

                            step = e_TaskStep.Step_05;
                            break;
                        case e_TaskStep.Step_04: // check (Up)
                            bool check = false;

                            switch (channel)
                            {
                                case e_Channel.CH_01: check = _DI[(int)e_DI.Input04_CH1_실린더_Up]; break;
                                case e_Channel.CH_02: check = _DI[(int)e_DI.Input09_CH2_실린더_Up]; break;
                                case e_Channel.CH_03: check = _DI[(int)e_DI.Input14_CH3_실린더_Up]; break;
                                case e_Channel.CH_04: check = _DI[(int)e_DI.Input20_CH4_실린더_Up]; break;
                                case e_Channel.CH_05: check = _DI[(int)e_DI.Input25_CH5_실린더_Up]; break;
                            }

                            if (check)
                            {
                                step = e_TaskStep.Step_06;

                                await Task.Delay(1000);
                            }
                            break;
                        case e_TaskStep.Step_05: // check (Down)
                            check = false;

                            switch (channel)
                            {
                                case e_Channel.CH_01: check = _DI[(int)e_DI.Input03_CH1_실린더_Down]; break;
                                case e_Channel.CH_02: check = _DI[(int)e_DI.Input08_CH2_실린더_Down]; break;
                                case e_Channel.CH_03: check = _DI[(int)e_DI.Input13_CH3_실린더_Down]; break;
                                case e_Channel.CH_04: check = _DI[(int)e_DI.Input19_CH4_실린더_Down]; break;
                                case e_Channel.CH_05: check = _DI[(int)e_DI.Input24_CH5_실린더_Down]; break;
                            }

                            if (check)
                            {
                                step = e_TaskStep.Step_06;
                            }
                            break;
                        case e_TaskStep.Step_06:
                            switch (channel)
                            {
                                case e_Channel.CH_01:
                                    _DO[(int)e_DO.Output02_CH1_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output03_CH1_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_02:
                                    _DO[(int)e_DO.Output05_CH2_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output06_CH2_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_03:
                                    _DO[(int)e_DO.Output08_CH3_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output09_CH3_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_04:
                                    _DO[(int)e_DO.Output11_CH4_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output12_CH4_실린더_Down] = false;
                                    break;
                                case e_Channel.CH_05:
                                    _DO[(int)e_DO.Output14_CH5_실린더_Up  ] = false;
                                    _DO[(int)e_DO.Output15_CH5_실린더_Down] = false;
                                    break;
                            }

                            step = e_TaskStep.Step_20;
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
                switch (channel)
                {
                    case e_Channel.CH_01:
                        _DO[(int)e_DO.Output02_CH1_실린더_Up  ] = false;
                        _DO[(int)e_DO.Output03_CH1_실린더_Down] = false;
                        break;
                    case e_Channel.CH_02:
                        _DO[(int)e_DO.Output05_CH2_실린더_Up  ] = false;
                        _DO[(int)e_DO.Output06_CH2_실린더_Down] = false;
                        break;
                    case e_Channel.CH_03:
                        _DO[(int)e_DO.Output08_CH3_실린더_Up  ] = false;
                        _DO[(int)e_DO.Output09_CH3_실린더_Down] = false;
                        break;
                    case e_Channel.CH_04:
                        _DO[(int)e_DO.Output11_CH4_실린더_Up  ] = false;
                        _DO[(int)e_DO.Output12_CH4_실린더_Down] = false;
                        break;
                    case e_Channel.CH_05:
                        _DO[(int)e_DO.Output14_CH5_실린더_Up  ] = false;
                        _DO[(int)e_DO.Output15_CH5_실린더_Down] = false;
                        break;
                }
            }

            return result;
        }

        private async Task<bool> StepAction(e_Channel channel, e_Dispenser dispenser, int count = 10, int timeout = 60)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            int cnt = 0;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            string rpm = "500";
                            switch (dispenser)
                            {
                                case e_Dispenser.과망간산칼륨: rpm = "250"; break;
                            }

                            if (_CStep[GetStepIdx(channel, dispenser)].MoveIncPos("10", rpm))
                            {
                                cnt++;

                                step = e_TaskStep.Step_02;
                            }
                            break;
                        case e_TaskStep.Step_02:
                            int millisecondsDelay = 1200;
                            switch (dispenser)
                            {
                                case e_Dispenser.과망간산칼륨: millisecondsDelay = 2400; break;
                            }

                            await Task.Delay(millisecondsDelay);

                            step = e_TaskStep.Step_03;
                            break;
                        case e_TaskStep.Step_03:
                            step = cnt < count ? e_TaskStep.Step_01 : e_TaskStep.Step_20;
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        public struct t_ServoUpResult
        {
            public bool result;
            public e_Channel channel;
        }
        private async Task<t_ServoUpResult> ServoUpDown(e_Channel channel, bool upDown, int timeout = 60)
        {
            t_ServoUpResult result = new t_ServoUpResult();
            result.result = false;
            result.channel = channel;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            int pos = 0;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    if (step > e_TaskStep.Step_01)
                    {
                        if (_CServo[(int)channel].Status.ERRORALL)
                        {
                            step = e_TaskStep.Step_10;
                        }
                    }

                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            if (upDown)
                            {
                                pos = _CServo[(int)channel].ActualPos - 20;
                            }
                            else
                            {
                                pos = _CServo[(int)channel].ActualPos + 15;
                            }

                            step = e_TaskStep.Step_02;
                            break;
                        case e_TaskStep.Step_02:
                            if (_CServo[(int)channel].MoveAbsPos(pos.ToString(), "120"))
                            {
                                step = e_TaskStep.Step_03;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            if (pos == _CServo[(int)channel].ActualPos && _CServo[(int)channel].Status.MOTIONING == false)
                            {
                                step = e_TaskStep.Step_20;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            if (_CServo[(int)channel].ServoAlarmReset())
                            {
                                step = e_TaskStep.Step_11;
                            }
                            break;
                        case e_TaskStep.Step_11:
                            if (_CServo[(int)channel].ServoEnable(true))
                            {
                                step = e_TaskStep.Step_02;
                            }
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result.result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        #region Startup

        /// <param name="timeout">second</param>
        private async Task Startup(int timeout = 10)
        {
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            break;
                        case e_TaskStep.Step_02:
                            break;
                        case e_TaskStep.Step_03:
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }
        }

        /// <param name="timeout">second</param>
        private async Task StartupBLDC(int timeout = 10)
        {
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            bool[] result = new bool[Enum.GetNames(typeof(e_Channel)).Length];

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == false)
                                {
                                    result[i] = _CBLDC.모터제어((CBLDC.e_Channel)i, CBLDC.e_Control.드라이버, CBLDC.e_Command.CLR);
                                }
                            }

                            if (result.All(_ => _ != false))
                            {
                                Array.Clear(result, 0, result.Length);

                                step = e_TaskStep.Step_02;
                            }
                            break;
                        case e_TaskStep.Step_02:
                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == false)
                                {
                                    result[i] = _CBLDC.모터제어((CBLDC.e_Channel)i, CBLDC.e_Control.브레이크, CBLDC.e_Command.CLR);
                                }
                            }

                            if (result.All(_ => _ != false))
                            {
                                Array.Clear(result, 0, result.Length);

                                step = e_TaskStep.Step_03;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == false)
                                {
                                    result[i] = _CBLDC.모터제어((CBLDC.e_Channel)i, CBLDC.e_Control.알람리셋);
                                }
                            }

                            if (result.All(_ => _ != false))
                            {
                                Array.Clear(result, 0, result.Length);

                                step = e_TaskStep.Step_04;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == false)
                                {
                                    result[i] = _CBLDC.모터제어((CBLDC.e_Channel)i, CBLDC.e_Control.드라이버, CBLDC.e_Command.SET);
                                }
                            }

                            if (result.All(_ => _ != false))
                            {
                                Array.Clear(result, 0, result.Length);

                                step = e_TaskStep.Step_20;
                            }
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }
        }

        /// <param name="timeout">second</param>
        private async Task StartupWaterTank(int timeout = 10)
        {
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            if (_CWaterTank.Stop())
                            {
                                step = e_TaskStep.Step_02;
                            }
                            break;
                        case e_TaskStep.Step_02:
                            if (_CWaterTank.Start())
                            {
                                step = e_TaskStep.Step_03;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            if (_CWaterTank.SetTemperature("5"))
                            {
                                step = e_TaskStep.Step_20;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }
        }

        /// <param name="channel">channel</param>
        /// <param name="timeout">second</param>
        private async Task StartupServo(e_Channel channel, int timeout = 10)
        {
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;
            CServo.t_Status status = new CServo.t_Status();

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            //CServo.t_Status status = new CServo.t_Status();

                            if (_CServo[(int)channel].GetStatus(ref status))
                            {
                                if (status.ERRORALL)
                                {
                                    step = e_TaskStep.Step_02;
                                }
                                else if (status.SERVOON)
                                {
                                    step = e_TaskStep.Step_20;
                                }
                                else if (status.SERVOON == false)
                                {
                                    if (status.ORIGINRETOK)
                                    {
                                        step = e_TaskStep.Step_05;
                                    }
                                    else
                                    {
                                        step = e_TaskStep.Step_03;
                                    }

                                    //step = e_TaskStep.Step_04;
                                }
                            }
                            break;
                        case e_TaskStep.Step_02:
                            if (_CServo[(int)channel].ServoAlarmReset())
                            {
                                step = e_TaskStep.Step_01;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            if (_CServo[(int)channel].ClearPosition())
                            {
                                step = e_TaskStep.Step_04;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            if (_CServo[(int)channel].ServoEnable(false))
                            {
                                step = e_TaskStep.Step_05;
                            }
                            break;
                        case e_TaskStep.Step_05:
                            if (_CServo[(int)channel].ServoEnable(true))
                            {
                                step = e_TaskStep.Step_06;
                            }
                            break;
                        case e_TaskStep.Step_06:
                            if (_CServo[(int)channel].GetStatus(ref status))
                            {
                                if (status.SERVOON)
                                {
                                    step = e_TaskStep.Step_20;
                                }
                            }
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(500);
            }

            if (isTimeout == false)
            {
            }

            StartupServoComp[(int)channel] = true;
        }
        public bool[] StartupServoComp = new bool[Enum.GetNames(typeof(e_Channel)).Length];

        /// <param name="channel">channel</param>
        /// <param name="dispenser">dispenser</param>
        /// <param name="timeout">second</param>
        /// <returns></returns>
        private async Task StartupStep(e_Channel channel, e_Dispenser dispenser, int timeout = 10)
        {
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            CStep.t_Status status = new CStep.t_Status();

                            if (_CStep[GetStepIdx(channel, dispenser)].GetStatus(ref status))
                            {
                                if (status.ERRORALL)
                                {
                                    step = e_TaskStep.Step_02;
                                }
                                else if (status.STEPON)
                                {
                                    step = e_TaskStep.Step_20;
                                }
                                else
                                {
                                    step = e_TaskStep.Step_03;
                                }
                            }
                            break;
                        case e_TaskStep.Step_02:
                            if (_CStep[GetStepIdx(channel, dispenser)].ServoAlarmReset())
                            {
                                step = e_TaskStep.Step_03;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            if (_CStep[GetStepIdx(channel, dispenser)].ClearPosition())
                            {
                                step = e_TaskStep.Step_04;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            if (_CStep[GetStepIdx(channel, dispenser)].ServoEnable(true))
                            {
                                step = e_TaskStep.Step_20;
                            }
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }
        }

        /// <param name="timeout">second</param>
        private async Task<bool> StartupSequence(int timeout = 10)
        {
            bool result = false;
            bool isBreak = true;
            bool isTimeout = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            e_TaskStep step = e_TaskStep.Step_01;

            stopwatch.Start();
            while (isBreak && isTimeout)
            {
                try
                {
                    switch (step)
                    {
                        case e_TaskStep.Step_01:
                            if (_CWaterTank.Start())
                            {
                                step = e_TaskStep.Step_02;
                            }
                            break;
                        case e_TaskStep.Step_02:
                            if (_CNextPump.FillingStop())
                            {
                                step = e_TaskStep.Step_03;
                            }
                            break;
                        case e_TaskStep.Step_03:
                            if (_CNextPump.RecycleStop())
                            {
                                step = e_TaskStep.Step_20;
                            }
                            break;
                        case e_TaskStep.Step_04:
                            break;
                        case e_TaskStep.Step_05:
                            break;
                        case e_TaskStep.Step_06:
                            break;
                        case e_TaskStep.Step_07:
                            break;
                        case e_TaskStep.Step_08:
                            break;
                        case e_TaskStep.Step_09:
                            break;
                        case e_TaskStep.Step_10:
                            break;
                        case e_TaskStep.Step_11:
                            break;
                        case e_TaskStep.Step_12:
                            break;
                        case e_TaskStep.Step_13:
                            break;
                        case e_TaskStep.Step_14:
                            break;
                        case e_TaskStep.Step_15:
                            break;
                        case e_TaskStep.Step_16:
                            break;
                        case e_TaskStep.Step_17:
                            break;
                        case e_TaskStep.Step_18:
                            break;
                        case e_TaskStep.Step_19:
                            break;
                        case e_TaskStep.Step_20:
                            result = true;
                            isBreak = false;
                            break;
                    }

                    isTimeout = stopwatch.ElapsedMilliseconds <= 1000 * timeout;
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                await Task.Delay(100);
            }

            if (isTimeout == false)
            {
            }

            return result;
        }

        #endregion

        #endregion

        #region Process

        bool[] _SelectedChannel = new bool[Enum.GetNames(typeof(e_Channel)).Length];

        private void btn_Process_Select_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(15 - 1, 1)) - 1;

            _SelectedChannel[idx] = !_SelectedChannel[idx];
        }

        private void btn_Process_Start_Click(object sender, EventArgs e)
        {
            if (_Curr_SequenceStep == e_SequenceStep.대기)
            {
                if (_Process_Start == false)
                {
                    if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_공정시작, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _Process_Start = true;
                    }
                }
            }
        }

        private void btn_Process_Stop_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Name;
            int idx = Convert.ToInt32(name.Substring(15 - 1, 1)) - 1;

            e_Channel channel = (e_Channel)idx;

            string call = $"Process - {channel} {button.Text} Click";

            if (Array.FindAll(_SelectedChannel, _ => _ == true).Length == 1)
            {
                GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_중지불가);
            }
            else
            {
                if (GlobalFunctions.MessageBox(call, $"{channel} {GlobalVariables.Message.Q_공정중지}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _SelectedChannel[idx] = false;
                }
            }
        }

        private void btn_Process_Interlock_Click(object sender, EventArgs e)
        {
            if (_Curr_SequenceStep > e_SequenceStep.대기)
            {
                if (_Process_Interlock)
                {
                    if (GlobalFunctions.MessageBox(MethodBase.GetCurrentMethod().Name, GlobalVariables.Message.Q_공정재개, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _Process_Interlock_Click = true;
                    }
                }
            }
        }

        private void btn_Process_Parameter_Click(object sender, EventArgs e)
        {
            testToolStripMenuItem_Click(null, null);
        }

        #endregion

        // 파우더 헤드 전부 상승
        private async void button1_Click(object sender, EventArgs e)
        {
            string call = "파우더 헤드 전부 상승";
            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                List<Task<bool>> tasks = new List<Task<bool>>(Enum.GetNames(typeof(e_Channel)).Length);

                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    tasks.Add(실린더제어((e_Channel)i, true));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
        }

        // 파우더 헤드 전부 하강
        private async void button2_Click(object sender, EventArgs e)
        {
            string call = "파우더 헤드 전부 하강";
            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                List<Task<bool>> tasks = new List<Task<bool>>(Enum.GetNames(typeof(e_Channel)).Length);

                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    tasks.Add(실린더제어((e_Channel)i, false));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
        }

        // 초순수 공급 전부 채우기
        private async void button3_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;

            string call = "초순수 공급 전부 채우기";
            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                string ml = txt_Manual_NextPump_공급_Volume.Text;
                string sec = txt_Manual_NextPump_공급_Time.Text;

                if (IsFillingSetup(ml, sec))
                {
                    await Task.Run(() => _CNextPump.FillingStop());

                    #region Channel 선택

                    Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                    _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;

                    #endregion

                    if (await Task.Run(() => _CNextPump.FillingSetup(ml, sec)))
                    {
                        await Task.Run(() => { button.BackColor = _CNextPump.FillingStart() ? Color.Lime : Color.Red; });
                    }
                    else
                    {
                        button.BackColor = Color.Red;

                        GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업실패);
                    }
                }
                else
                {
                    GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_셋업불가);
                }
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }

        // 초순수 공급 전부 비우기
        private async void button4_Click(object sender, EventArgs e)
        {
            #region Control

            SetBackColor(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", SystemColors.Control);

            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", false);

            #endregion

            Button button = sender as Button;

            string call = "초순수 공급 전부 비우기";
            try
            {
                if (GlobalFunctions.MessageBox(call, GlobalVariables.Message.Q_명령전송, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                await Task.Run(() => _CNextPump.RecycleStop());

                #region Channel 선택

                Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true;
                _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true;
                _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true;
                _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true;
                _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true;

                #endregion

                await Task.Run(() => { button.BackColor = _CNextPump.RecycleStart() ? Color.Lime : Color.Red; });
            }
            catch (Exception ex)
            {
                Log.Write(call, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                #region Control

                SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), "btn_Manual_NextPump", true);

                #endregion
            }
        }
    }
}
