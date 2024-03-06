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
        private Timer _TimerMain = null;
        private void Tick_Main(object sender, EventArgs e)
        {
            _TimerMain?.Stop();
            try
            {
                ToolStripStatusLabel_DateTime.Text = GlobalFunctions.GetDateTimeString(DateTime.Now);

                switch ((e_TabPage)tabControl1.SelectedIndex)
                {
                    case e_TabPage.Status:
                        StatusBLDC();

                        StatusDI();

                        StatusDO();

                        StatusTemperature();

                        StatusWaterTank();

                        StatusServo();

                        StatusStep();

                        StatusNextPump();
                        break;


                    case e_TabPage.Process:
                        Process();
                        break;


                    case e_TabPage.Manual:
                        Manual();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                _TimerMain?.Start();
            }
        }

        #region Status

        private void StatusBLDC()
        {
            try
            {
                if (_CBLDC != null)
                {
                    btn_Status_BLDC_Open.BackColor = _CBLDC.IsOpen ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_BLDC_Open.BackColor = SystemColors.Control;
                }

                if (_CBLDC != null)
                {
                    lbl_Status_BLDC_CH1_BRK.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].BRK ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH1_FRE.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].FRE ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH1_ALM.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].ALM ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH1_EMG.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].EMG ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH1_DEC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].DEC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH1_ACC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].ACC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH1_DIR.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].DIR ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH1_RUN.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH01].RUN ? Color.Yellow : SystemColors.Control;

                    lbl_Status_BLDC_CH2_BRK.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].BRK ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH2_FRE.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].FRE ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH2_ALM.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].ALM ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH2_EMG.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].EMG ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH2_DEC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].DEC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH2_ACC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].ACC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH2_DIR.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].DIR ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH2_RUN.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH02].RUN ? Color.Yellow : SystemColors.Control;

                    lbl_Status_BLDC_CH3_BRK.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].BRK ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH3_FRE.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].FRE ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH3_ALM.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].ALM ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH3_EMG.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].EMG ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH3_DEC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].DEC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH3_ACC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].ACC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH3_DIR.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].DIR ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH3_RUN.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH03].RUN ? Color.Yellow : SystemColors.Control;

                    lbl_Status_BLDC_CH4_BRK.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].BRK ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH4_FRE.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].FRE ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH4_ALM.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].ALM ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH4_EMG.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].EMG ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH4_DEC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].DEC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH4_ACC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].ACC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH4_DIR.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].DIR ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH4_RUN.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH04].RUN ? Color.Yellow : SystemColors.Control;

                    lbl_Status_BLDC_CH5_BRK.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].BRK ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH5_FRE.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].FRE ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH5_ALM.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].ALM ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH5_EMG.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].EMG ? Color.Red    : SystemColors.Control;
                    lbl_Status_BLDC_CH5_DEC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].DEC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH5_ACC.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].ACC ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH5_DIR.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].DIR ? Color.Lime   : SystemColors.Control;
                    lbl_Status_BLDC_CH5_RUN.BackColor = _CBLDC.MotorStatus[(int)CBLDC.e_Channel.CH05].RUN ? Color.Yellow : SystemColors.Control;
                }
                else
                {
                    lbl_Status_BLDC_CH1_BRK.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_FRE.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_ALM.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_EMG.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_DEC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_ACC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_DIR.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH1_RUN.BackColor = SystemColors.Control;

                    lbl_Status_BLDC_CH2_BRK.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_FRE.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_ALM.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_EMG.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_DEC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_ACC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_DIR.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH2_RUN.BackColor = SystemColors.Control;

                    lbl_Status_BLDC_CH3_BRK.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_FRE.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_ALM.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_EMG.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_DEC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_ACC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_DIR.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH3_RUN.BackColor = SystemColors.Control;

                    lbl_Status_BLDC_CH4_BRK.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_FRE.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_ALM.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_EMG.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_DEC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_ACC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_DIR.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH4_RUN.BackColor = SystemColors.Control;

                    lbl_Status_BLDC_CH5_BRK.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_FRE.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_ALM.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_EMG.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_DEC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_ACC.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_DIR.BackColor = SystemColors.Control;
                    lbl_Status_BLDC_CH5_RUN.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusDI()
        {
            try
            {
                if (_CDI != null)
                {
                    btn_Status_DI_Connect.BackColor = _CDI.IsConnected ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_DI_Connect.BackColor = SystemColors.Control;
                }

                if (_CDI != null)
                {
                    lbl_Status_DI_00.BackColor = _DI[(int)e_DI.Input00_CH1_인산       ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_01.BackColor = _DI[(int)e_DI.Input01_CH1_과산화수소 ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_02.BackColor = _DI[(int)e_DI.Input02_CH1_초순수     ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_03.BackColor = _DI[(int)e_DI.Input03_CH1_실린더_Down] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_04.BackColor = _DI[(int)e_DI.Input04_CH1_실린더_Up  ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_05.BackColor = _DI[(int)e_DI.Input05_CH2_인산       ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_06.BackColor = _DI[(int)e_DI.Input06_CH2_과산화수소 ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_07.BackColor = _DI[(int)e_DI.Input07_CH2_초순수     ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_08.BackColor = _DI[(int)e_DI.Input08_CH2_실린더_Down] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_09.BackColor = _DI[(int)e_DI.Input09_CH2_실린더_Up  ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_10.BackColor = _DI[(int)e_DI.Input10_CH3_인산       ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_11.BackColor = _DI[(int)e_DI.Input11_CH3_과산화수소 ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_12.BackColor = _DI[(int)e_DI.Input12_CH3_초순수     ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_13.BackColor = _DI[(int)e_DI.Input13_CH3_실린더_Down] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_14.BackColor = _DI[(int)e_DI.Input14_CH3_실린더_Up  ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_15.BackColor = _DI[(int)e_DI.Input15                ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_16.BackColor = _DI[(int)e_DI.Input16_CH4_인산       ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_17.BackColor = _DI[(int)e_DI.Input17_CH4_과산화수소 ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_18.BackColor = _DI[(int)e_DI.Input18_CH4_초순수     ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_19.BackColor = _DI[(int)e_DI.Input19_CH4_실린더_Down] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_20.BackColor = _DI[(int)e_DI.Input20_CH4_실린더_Up  ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_21.BackColor = _DI[(int)e_DI.Input21_CH5_인산       ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_22.BackColor = _DI[(int)e_DI.Input22_CH5_과산화수소 ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_23.BackColor = _DI[(int)e_DI.Input23_CH5_초순수     ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_24.BackColor = _DI[(int)e_DI.Input24_CH5_실린더_Down] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_25.BackColor = _DI[(int)e_DI.Input25_CH5_실린더_Up  ] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_26.BackColor = _DI[(int)e_DI.Input26_CH1_반응조_센서] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_27.BackColor = _DI[(int)e_DI.Input27_CH2_반응조_센서] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_28.BackColor = _DI[(int)e_DI.Input28_CH3_반응조_센서] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_29.BackColor = _DI[(int)e_DI.Input29_CH4_반응조_센서] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_30.BackColor = _DI[(int)e_DI.Input30_CH5_반응조_센서] ? Color.Lime : SystemColors.Control;
                    lbl_Status_DI_31.BackColor = _DI[(int)e_DI.Input31_에어           ] ? Color.Lime : SystemColors.Control;
                }
                else
                {
                    lbl_Status_DI_00.BackColor = SystemColors.Control;
                    lbl_Status_DI_01.BackColor = SystemColors.Control;
                    lbl_Status_DI_02.BackColor = SystemColors.Control;
                    lbl_Status_DI_03.BackColor = SystemColors.Control;
                    lbl_Status_DI_04.BackColor = SystemColors.Control;
                    lbl_Status_DI_05.BackColor = SystemColors.Control;
                    lbl_Status_DI_06.BackColor = SystemColors.Control;
                    lbl_Status_DI_07.BackColor = SystemColors.Control;
                    lbl_Status_DI_08.BackColor = SystemColors.Control;
                    lbl_Status_DI_09.BackColor = SystemColors.Control;
                    lbl_Status_DI_10.BackColor = SystemColors.Control;
                    lbl_Status_DI_11.BackColor = SystemColors.Control;
                    lbl_Status_DI_12.BackColor = SystemColors.Control;
                    lbl_Status_DI_13.BackColor = SystemColors.Control;
                    lbl_Status_DI_14.BackColor = SystemColors.Control;
                    lbl_Status_DI_15.BackColor = SystemColors.Control;
                    lbl_Status_DI_16.BackColor = SystemColors.Control;
                    lbl_Status_DI_17.BackColor = SystemColors.Control;
                    lbl_Status_DI_18.BackColor = SystemColors.Control;
                    lbl_Status_DI_19.BackColor = SystemColors.Control;
                    lbl_Status_DI_20.BackColor = SystemColors.Control;
                    lbl_Status_DI_21.BackColor = SystemColors.Control;
                    lbl_Status_DI_22.BackColor = SystemColors.Control;
                    lbl_Status_DI_23.BackColor = SystemColors.Control;
                    lbl_Status_DI_24.BackColor = SystemColors.Control;
                    lbl_Status_DI_25.BackColor = SystemColors.Control;
                    lbl_Status_DI_26.BackColor = SystemColors.Control;
                    lbl_Status_DI_27.BackColor = SystemColors.Control;
                    lbl_Status_DI_28.BackColor = SystemColors.Control;
                    lbl_Status_DI_29.BackColor = SystemColors.Control;
                    lbl_Status_DI_30.BackColor = SystemColors.Control;
                    lbl_Status_DI_31.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusDO()
        {
            try
            {
                if (_CDO != null)
                {
                    btn_Status_DO_Connect.BackColor = _CDO.IsConnected ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_DO_Connect.BackColor = SystemColors.Control;
                }

                if (_CDO != null)
                {
                    btn_Status_DO_00.BackColor = _DO[(int)e_DO.Output00_에어_ON               ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_01.BackColor = _DO[(int)e_DO.Output01_에어_OFF              ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_02.BackColor = _DO[(int)e_DO.Output02_CH1_실린더_Up         ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_03.BackColor = _DO[(int)e_DO.Output03_CH1_실린더_Down       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_04.BackColor = _DO[(int)e_DO.Output04                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_05.BackColor = _DO[(int)e_DO.Output05_CH2_실린더_Up         ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_06.BackColor = _DO[(int)e_DO.Output06_CH2_실린더_Down       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_07.BackColor = _DO[(int)e_DO.Output07                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_08.BackColor = _DO[(int)e_DO.Output08_CH3_실린더_Up         ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_09.BackColor = _DO[(int)e_DO.Output09_CH3_실린더_Down       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_10.BackColor = _DO[(int)e_DO.Output10                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_11.BackColor = _DO[(int)e_DO.Output11_CH4_실린더_Up         ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_12.BackColor = _DO[(int)e_DO.Output12_CH4_실린더_Down       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_13.BackColor = _DO[(int)e_DO.Output13                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_14.BackColor = _DO[(int)e_DO.Output14_CH5_실린더_Up         ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_15.BackColor = _DO[(int)e_DO.Output15_CH5_실린더_Down       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_16.BackColor = _DO[(int)e_DO.Output16_CH1_디스펜서_에어_정지] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_17.BackColor = _DO[(int)e_DO.Output17_CH1_디스펜서_에어_배출] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_18.BackColor = _DO[(int)e_DO.Output18_CH2_디스펜서_에어_정지] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_19.BackColor = _DO[(int)e_DO.Output19_CH2_디스펜서_에어_배출] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_20.BackColor = _DO[(int)e_DO.Output20_CH3_디스펜서_에어_정지] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_21.BackColor = _DO[(int)e_DO.Output21_CH3_디스펜서_에어_배출] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_22.BackColor = _DO[(int)e_DO.Output22_CH4_디스펜서_에어_정지] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_23.BackColor = _DO[(int)e_DO.Output23_CH4_디스펜서_에어_배출] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_24.BackColor = _DO[(int)e_DO.Output24_CH5_디스펜서_에어_정지] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_25.BackColor = _DO[(int)e_DO.Output25_CH5_디스펜서_에어_배출] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_26.BackColor = _DO[(int)e_DO.Output26                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_27.BackColor = _DO[(int)e_DO.Output27                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_28.BackColor = _DO[(int)e_DO.Output28                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_29.BackColor = _DO[(int)e_DO.Output29                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_30.BackColor = _DO[(int)e_DO.Output30                       ] ? Color.Lime : SystemColors.Control;
                    btn_Status_DO_31.BackColor = _DO[(int)e_DO.Output31                       ] ? Color.Lime : SystemColors.Control;
                }
                else
                {
                    btn_Status_DO_00.BackColor = SystemColors.Control;
                    btn_Status_DO_01.BackColor = SystemColors.Control;
                    btn_Status_DO_02.BackColor = SystemColors.Control;
                    btn_Status_DO_03.BackColor = SystemColors.Control;
                    btn_Status_DO_04.BackColor = SystemColors.Control;
                    btn_Status_DO_05.BackColor = SystemColors.Control;
                    btn_Status_DO_06.BackColor = SystemColors.Control;
                    btn_Status_DO_07.BackColor = SystemColors.Control;
                    btn_Status_DO_08.BackColor = SystemColors.Control;
                    btn_Status_DO_09.BackColor = SystemColors.Control;
                    btn_Status_DO_10.BackColor = SystemColors.Control;
                    btn_Status_DO_11.BackColor = SystemColors.Control;
                    btn_Status_DO_12.BackColor = SystemColors.Control;
                    btn_Status_DO_13.BackColor = SystemColors.Control;
                    btn_Status_DO_14.BackColor = SystemColors.Control;
                    btn_Status_DO_15.BackColor = SystemColors.Control;
                    btn_Status_DO_16.BackColor = SystemColors.Control;
                    btn_Status_DO_17.BackColor = SystemColors.Control;
                    btn_Status_DO_18.BackColor = SystemColors.Control;
                    btn_Status_DO_19.BackColor = SystemColors.Control;
                    btn_Status_DO_20.BackColor = SystemColors.Control;
                    btn_Status_DO_21.BackColor = SystemColors.Control;
                    btn_Status_DO_22.BackColor = SystemColors.Control;
                    btn_Status_DO_23.BackColor = SystemColors.Control;
                    btn_Status_DO_24.BackColor = SystemColors.Control;
                    btn_Status_DO_25.BackColor = SystemColors.Control;
                    btn_Status_DO_26.BackColor = SystemColors.Control;
                    btn_Status_DO_27.BackColor = SystemColors.Control;
                    btn_Status_DO_28.BackColor = SystemColors.Control;
                    btn_Status_DO_29.BackColor = SystemColors.Control;
                    btn_Status_DO_30.BackColor = SystemColors.Control;
                    btn_Status_DO_31.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusTemperature()
        {
            try
            {
                if (_CTemperature != null)
                {
                    btn_Status_Temperature_Open.BackColor = _CTemperature.IsOpen ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_Temperature_Open.BackColor = SystemColors.Control;
                }

                if (_CTemperature != null)
                {
                    lbl_Status_Temperature_CH1.Text = $"{_Temperature[(int)e_Channel.CH_01]:0.0} ℃";
                    lbl_Status_Temperature_CH2.Text = $"{_Temperature[(int)e_Channel.CH_02]:0.0} ℃";
                    lbl_Status_Temperature_CH3.Text = $"{_Temperature[(int)e_Channel.CH_03]:0.0} ℃";
                    lbl_Status_Temperature_CH4.Text = $"{_Temperature[(int)e_Channel.CH_04]:0.0} ℃";
                    lbl_Status_Temperature_CH5.Text = $"{_Temperature[(int)e_Channel.CH_05]:0.0} ℃";
                }
                else
                {
                    lbl_Status_Temperature_CH1.Text = "-";
                    lbl_Status_Temperature_CH2.Text = "-";
                    lbl_Status_Temperature_CH3.Text = "-";
                    lbl_Status_Temperature_CH4.Text = "-";
                    lbl_Status_Temperature_CH5.Text = "-";
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusWaterTank()
        {
            try
            {
                if (_CWaterTank != null)
                {
                    btn_Status_WaterTank_Open.BackColor = _CWaterTank.IsOpen ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_WaterTank_Open.BackColor = SystemColors.Control;
                }

                if (_CWaterTank != null)
                {
                    lbl_Status_WaterTank_현재온도.Text = $"{_WaterTank_Curr:0.0} ℃";
                    lbl_Status_WaterTank_희망온도.Text = $"{_WaterTank_Targ:0.0} ℃";
                }
                else
                {
                    lbl_Status_WaterTank_현재온도.Text = "-";
                    lbl_Status_WaterTank_희망온도.Text = "-";
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusServo()
        {
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    switch ((e_Channel)i)
                    {
                        case e_Channel.CH_01:
                            if (_CServo[i] != null)
                            {
                                btn_Status_Servo_CH1_Connect.BackColor = _CServo[i].IsConnected ? Color.Lime : Color.Red;
                            }
                            else
                            {
                                btn_Status_Servo_CH1_Connect.BackColor = SystemColors.Control;
                            }

                            if (_CServo[i] != null)
                            {
                                lbl_Status_Servo_CH1_ERRORALL.BackColor        = _CServo[i].Status.ERRORALL        ? Color.Red    : SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINRETURNING.BackColor = _CServo[i].Status.ORIGINRETURNING ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINSENSOR.BackColor    = _CServo[i].Status.ORIGINSENSOR    ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINRETOK.BackColor     = _CServo[i].Status.ORIGINRETOK     ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH1_INPOSITION.BackColor      = _CServo[i].Status.INPOSITION      ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH1_SERVOON.BackColor         = _CServo[i].Status.SERVOON         ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH1_MOTIONING.BackColor       = _CServo[i].Status.MOTIONING       ? Color.Yellow : SystemColors.Control;
                            }
                            else
                            {
                                lbl_Status_Servo_CH1_ERRORALL.BackColor        = SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINRETURNING.BackColor = SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINSENSOR.BackColor    = SystemColors.Control;
                                lbl_Status_Servo_CH1_ORIGINRETOK.BackColor     = SystemColors.Control;
                                lbl_Status_Servo_CH1_INPOSITION.BackColor      = SystemColors.Control;
                                lbl_Status_Servo_CH1_SERVOON.BackColor         = SystemColors.Control;
                                lbl_Status_Servo_CH1_MOTIONING.BackColor       = SystemColors.Control;
                            }
                            break;


                        case e_Channel.CH_02:
                            if (_CServo[i] != null)
                            {
                                btn_Status_Servo_CH2_Connect.BackColor = _CServo[i].IsConnected ? Color.Lime : Color.Red;
                            }
                            else
                            {
                                btn_Status_Servo_CH2_Connect.BackColor = SystemColors.Control;
                            }

                            if (_CServo[i] != null)
                            {
                                lbl_Status_Servo_CH2_ERRORALL.BackColor        = _CServo[i].Status.ERRORALL        ? Color.Red    : SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINRETURNING.BackColor = _CServo[i].Status.ORIGINRETURNING ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINSENSOR.BackColor    = _CServo[i].Status.ORIGINSENSOR    ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINRETOK.BackColor     = _CServo[i].Status.ORIGINRETOK     ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH2_INPOSITION.BackColor      = _CServo[i].Status.INPOSITION      ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH2_SERVOON.BackColor         = _CServo[i].Status.SERVOON         ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH2_MOTIONING.BackColor       = _CServo[i].Status.MOTIONING       ? Color.Yellow : SystemColors.Control;
                            }
                            else
                            {
                                lbl_Status_Servo_CH2_ERRORALL.BackColor        = SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINRETURNING.BackColor = SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINSENSOR.BackColor    = SystemColors.Control;
                                lbl_Status_Servo_CH2_ORIGINRETOK.BackColor     = SystemColors.Control;
                                lbl_Status_Servo_CH2_INPOSITION.BackColor      = SystemColors.Control;
                                lbl_Status_Servo_CH2_SERVOON.BackColor         = SystemColors.Control;
                                lbl_Status_Servo_CH2_MOTIONING.BackColor       = SystemColors.Control;
                            }
                            break;


                        case e_Channel.CH_03:
                            if (_CServo[i] != null)
                            {
                                btn_Status_Servo_CH3_Connect.BackColor = _CServo[i].IsConnected ? Color.Lime : Color.Red;
                            }
                            else
                            {
                                btn_Status_Servo_CH3_Connect.BackColor = SystemColors.Control;
                            }

                            if (_CServo[i] != null)
                            {
                                lbl_Status_Servo_CH3_ERRORALL.BackColor        = _CServo[i].Status.ERRORALL        ? Color.Red    : SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINRETURNING.BackColor = _CServo[i].Status.ORIGINRETURNING ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINSENSOR.BackColor    = _CServo[i].Status.ORIGINSENSOR    ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINRETOK.BackColor     = _CServo[i].Status.ORIGINRETOK     ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH3_INPOSITION.BackColor      = _CServo[i].Status.INPOSITION      ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH3_SERVOON.BackColor         = _CServo[i].Status.SERVOON         ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH3_MOTIONING.BackColor       = _CServo[i].Status.MOTIONING       ? Color.Yellow : SystemColors.Control;
                            }
                            else
                            {
                                lbl_Status_Servo_CH3_ERRORALL.BackColor        = SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINRETURNING.BackColor = SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINSENSOR.BackColor    = SystemColors.Control;
                                lbl_Status_Servo_CH3_ORIGINRETOK.BackColor     = SystemColors.Control;
                                lbl_Status_Servo_CH3_INPOSITION.BackColor      = SystemColors.Control;
                                lbl_Status_Servo_CH3_SERVOON.BackColor         = SystemColors.Control;
                                lbl_Status_Servo_CH3_MOTIONING.BackColor       = SystemColors.Control;
                            }
                            break;


                        case e_Channel.CH_04:
                            if (_CServo[i] != null)
                            {
                                btn_Status_Servo_CH4_Connect.BackColor = _CServo[i].IsConnected ? Color.Lime : Color.Red;
                            }
                            else
                            {
                                btn_Status_Servo_CH4_Connect.BackColor = SystemColors.Control;
                            }

                            if (_CServo[i] != null)
                            {
                                lbl_Status_Servo_CH4_ERRORALL.BackColor        = _CServo[i].Status.ERRORALL        ? Color.Red    : SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINRETURNING.BackColor = _CServo[i].Status.ORIGINRETURNING ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINSENSOR.BackColor    = _CServo[i].Status.ORIGINSENSOR    ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINRETOK.BackColor     = _CServo[i].Status.ORIGINRETOK     ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH4_INPOSITION.BackColor      = _CServo[i].Status.INPOSITION      ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH4_SERVOON.BackColor         = _CServo[i].Status.SERVOON         ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH4_MOTIONING.BackColor       = _CServo[i].Status.MOTIONING       ? Color.Yellow : SystemColors.Control;
                            }
                            else
                            {
                                lbl_Status_Servo_CH4_ERRORALL.BackColor        = SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINRETURNING.BackColor = SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINSENSOR.BackColor    = SystemColors.Control;
                                lbl_Status_Servo_CH4_ORIGINRETOK.BackColor     = SystemColors.Control;
                                lbl_Status_Servo_CH4_INPOSITION.BackColor      = SystemColors.Control;
                                lbl_Status_Servo_CH4_SERVOON.BackColor         = SystemColors.Control;
                                lbl_Status_Servo_CH4_MOTIONING.BackColor       = SystemColors.Control;
                            }
                            break;


                        case e_Channel.CH_05:
                            if (_CServo[i] != null)
                            {
                                btn_Status_Servo_CH5_Connect.BackColor = _CServo[i].IsConnected ? Color.Lime : Color.Red;
                            }
                            else
                            {
                                btn_Status_Servo_CH5_Connect.BackColor = SystemColors.Control;
                            }

                            if (_CServo[i] != null)
                            {
                                lbl_Status_Servo_CH5_ERRORALL.BackColor        = _CServo[i].Status.ERRORALL        ? Color.Red    : SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINRETURNING.BackColor = _CServo[i].Status.ORIGINRETURNING ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINSENSOR.BackColor    = _CServo[i].Status.ORIGINSENSOR    ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINRETOK.BackColor     = _CServo[i].Status.ORIGINRETOK     ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH5_INPOSITION.BackColor      = _CServo[i].Status.INPOSITION      ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH5_SERVOON.BackColor         = _CServo[i].Status.SERVOON         ? Color.Lime   : SystemColors.Control;
                                lbl_Status_Servo_CH5_MOTIONING.BackColor       = _CServo[i].Status.MOTIONING       ? Color.Yellow : SystemColors.Control;
                            }
                            else
                            {
                                lbl_Status_Servo_CH5_ERRORALL.BackColor        = SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINRETURNING.BackColor = SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINSENSOR.BackColor    = SystemColors.Control;
                                lbl_Status_Servo_CH5_ORIGINRETOK.BackColor     = SystemColors.Control;
                                lbl_Status_Servo_CH5_INPOSITION.BackColor      = SystemColors.Control;
                                lbl_Status_Servo_CH5_SERVOON.BackColor         = SystemColors.Control;
                                lbl_Status_Servo_CH5_MOTIONING.BackColor       = SystemColors.Control;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusStep()
        {
            try
            {
                for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                {
                    for (int j = 0; j < Enum.GetNames(typeof(e_Dispenser)).Length; j++)
                    {
                        switch ((e_Channel)i)
                        {
                            case e_Channel.CH_01:
                                switch ((e_Dispenser)j)
                                {
                                    case e_Dispenser.흑연:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH1_흑연_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH1_흑연_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH1_흑연_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH1_흑연_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH1_흑연_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;


                                    case e_Dispenser.과망간산칼륨:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH1_과망간산칼륨_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH1_과망간산칼륨_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH1_과망간산칼륨_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH1_과망간산칼륨_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH1_과망간산칼륨_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;
                                }
                                break;


                            case e_Channel.CH_02:
                                switch ((e_Dispenser)j)
                                {
                                    case e_Dispenser.흑연:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH2_흑연_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH2_흑연_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH2_흑연_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH2_흑연_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH2_흑연_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;


                                    case e_Dispenser.과망간산칼륨:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH2_과망간산칼륨_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH2_과망간산칼륨_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH2_과망간산칼륨_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH2_과망간산칼륨_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH2_과망간산칼륨_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;
                                }
                                break;


                            case e_Channel.CH_03:
                                switch ((e_Dispenser)j)
                                {
                                    case e_Dispenser.흑연:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH3_흑연_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH3_흑연_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH3_흑연_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH3_흑연_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH3_흑연_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;


                                    case e_Dispenser.과망간산칼륨:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH3_과망간산칼륨_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH3_과망간산칼륨_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH3_과망간산칼륨_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH3_과망간산칼륨_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH3_과망간산칼륨_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;
                                }
                                break;


                            case e_Channel.CH_04:
                                switch ((e_Dispenser)j)
                                {
                                    case e_Dispenser.흑연:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH4_흑연_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH4_흑연_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH4_흑연_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH4_흑연_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH4_흑연_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;


                                    case e_Dispenser.과망간산칼륨:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH4_과망간산칼륨_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH4_과망간산칼륨_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH4_과망간산칼륨_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH4_과망간산칼륨_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH4_과망간산칼륨_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;
                                }
                                break;


                            case e_Channel.CH_05:
                                switch ((e_Dispenser)j)
                                {
                                    case e_Dispenser.흑연:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH5_흑연_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH5_흑연_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH5_흑연_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH5_흑연_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH5_흑연_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;


                                    case e_Dispenser.과망간산칼륨:
                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            btn_Status_Step_CH5_과망간산칼륨_Connect.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].IsConnected ? Color.Lime : Color.Red;
                                        }
                                        else
                                        {
                                            btn_Status_Step_CH5_과망간산칼륨_Connect.BackColor = SystemColors.Control;
                                        }

                                        if (_CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)] != null)
                                        {
                                            lbl_Status_Step_CH5_과망간산칼륨_ERRORALL.BackColor  = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.ERRORALL  ? Color.Red    : SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_RUNSTOP.BackColor   = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.RUNSTOP   ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_STEPON.BackColor    = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.STEPON    ? Color.Lime   : SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_MOTIONING.BackColor = _CStep[GetStepIdx((e_Channel)i, (e_Dispenser)j)].Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                                        }
                                        else
                                        {
                                            lbl_Status_Step_CH5_과망간산칼륨_ERRORALL.BackColor  = SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_RUNSTOP.BackColor   = SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_STEPON.BackColor    = SystemColors.Control;
                                            lbl_Status_Step_CH5_과망간산칼륨_MOTIONING.BackColor = SystemColors.Control;
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        private void StatusNextPump()
        {
            try
            {
                if (_CNextPump != null)
                {
                    btn_Status_NextPump_Open.BackColor = _CNextPump.IsOpen ? Color.Lime : Color.Red;
                }
                else
                {
                    btn_Status_NextPump_Open.BackColor = SystemColors.Control;
                }

                if (_CNextPump != null)
                {
                    lbl_Status_NextPump_Filling.BackColor = _CNextPump.Status[(int)CNextPump.e_Status.Filling] ? Color.Yellow : SystemColors.Control;
                    lbl_Status_NextPump_Recycle.BackColor = _CNextPump.Status[(int)CNextPump.e_Status.Recycle] ? Color.Yellow : SystemColors.Control;
                }
                else
                {
                    lbl_Status_NextPump_Filling.BackColor = SystemColors.Control;
                    lbl_Status_NextPump_Recycle.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        #endregion

        #region Manual

        private bool[] m_isSetEnabledServo = new bool[Enum.GetNames(typeof(e_Channel)).Length];
        private bool m_isSetEnabledServoComp = false;

        private void Manual()
        {
            try
            {
                lbl_Manual_WaterTank_Targ.Text = $"{_WaterTank_Targ:0.0} ℃";
                lbl_Manual_WaterTank_Curr.Text = $"{_WaterTank_Curr:0.0} ℃";

                lbl_Manual_CH1_반응조.BackColor = _DI[(int)e_DI.Input26_CH1_반응조_센서] ? Color.Yellow : SystemColors.Control;
                lbl_Manual_CH2_반응조.BackColor = _DI[(int)e_DI.Input27_CH2_반응조_센서] ? Color.Yellow : SystemColors.Control;
                lbl_Manual_CH3_반응조.BackColor = _DI[(int)e_DI.Input28_CH3_반응조_센서] ? Color.Yellow : SystemColors.Control;
                lbl_Manual_CH4_반응조.BackColor = _DI[(int)e_DI.Input29_CH4_반응조_센서] ? Color.Yellow : SystemColors.Control;
                lbl_Manual_CH5_반응조.BackColor = _DI[(int)e_DI.Input30_CH5_반응조_센서] ? Color.Yellow : SystemColors.Control;

                lbl_Manual_Temperature_CH1.Text = $"{_Temperature[(int)e_Channel.CH_01]:0.0} ℃";
                lbl_Manual_Temperature_CH2.Text = $"{_Temperature[(int)e_Channel.CH_02]:0.0} ℃";
                lbl_Manual_Temperature_CH3.Text = $"{_Temperature[(int)e_Channel.CH_03]:0.0} ℃";
                lbl_Manual_Temperature_CH4.Text = $"{_Temperature[(int)e_Channel.CH_04]:0.0} ℃";
                lbl_Manual_Temperature_CH5.Text = $"{_Temperature[(int)e_Channel.CH_05]:0.0} ℃";

                lbl_Manual_BLDC_CH1.BackColor = (bool)_CBLDC?.MotorStatus[(int)CBLDC.e_Channel.CH01].RUN ? Color.Yellow : SystemColors.Control;
                lbl_Manual_BLDC_CH2.BackColor = (bool)_CBLDC?.MotorStatus[(int)CBLDC.e_Channel.CH02].RUN ? Color.Yellow : SystemColors.Control;
                lbl_Manual_BLDC_CH3.BackColor = (bool)_CBLDC?.MotorStatus[(int)CBLDC.e_Channel.CH03].RUN ? Color.Yellow : SystemColors.Control;
                lbl_Manual_BLDC_CH4.BackColor = (bool)_CBLDC?.MotorStatus[(int)CBLDC.e_Channel.CH04].RUN ? Color.Yellow : SystemColors.Control;
                lbl_Manual_BLDC_CH5.BackColor = (bool)_CBLDC?.MotorStatus[(int)CBLDC.e_Channel.CH05].RUN ? Color.Yellow : SystemColors.Control;

                lbl_Manual_BLDC_CH1.Text = _CBLDC?.MotorRPM[(int)CBLDC.e_Channel.CH01].ToString();
                lbl_Manual_BLDC_CH2.Text = _CBLDC?.MotorRPM[(int)CBLDC.e_Channel.CH02].ToString();
                lbl_Manual_BLDC_CH3.Text = _CBLDC?.MotorRPM[(int)CBLDC.e_Channel.CH03].ToString();
                lbl_Manual_BLDC_CH4.Text = _CBLDC?.MotorRPM[(int)CBLDC.e_Channel.CH04].ToString();
                lbl_Manual_BLDC_CH5.Text = _CBLDC?.MotorRPM[(int)CBLDC.e_Channel.CH05].ToString();

                lbl_Manual_Servo_CH1.BackColor = (bool)_CServo[(int)e_Channel.CH_01]?.Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                lbl_Manual_Servo_CH2.BackColor = (bool)_CServo[(int)e_Channel.CH_02]?.Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                lbl_Manual_Servo_CH3.BackColor = (bool)_CServo[(int)e_Channel.CH_03]?.Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                lbl_Manual_Servo_CH4.BackColor = (bool)_CServo[(int)e_Channel.CH_04]?.Status.MOTIONING ? Color.Yellow : SystemColors.Control;
                lbl_Manual_Servo_CH5.BackColor = (bool)_CServo[(int)e_Channel.CH_05]?.Status.MOTIONING ? Color.Yellow : SystemColors.Control;

                lbl_Manual_Servo_CH1.Text = _CServo[(int)e_Channel.CH_01]?.ActualPos.ToString();
                lbl_Manual_Servo_CH2.Text = _CServo[(int)e_Channel.CH_02]?.ActualPos.ToString();
                lbl_Manual_Servo_CH3.Text = _CServo[(int)e_Channel.CH_03]?.ActualPos.ToString();
                lbl_Manual_Servo_CH4.Text = _CServo[(int)e_Channel.CH_04]?.ActualPos.ToString();
                lbl_Manual_Servo_CH5.Text = _CServo[(int)e_Channel.CH_05]?.ActualPos.ToString();

                if (Array.FindAll(m_isSetEnabledServo, value => value == false).Length > 0)
                {
                    for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                    {
                        if (_CServo[i].IsGetStatus && m_isSetEnabledServo[i] == false)
                        {
                            //SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{i + 1}", true);
                            m_isSetEnabledServo[i] = true;
                        }
                    }
                }
                else if (Array.FindAll(m_isSetEnabledServo, value => value == true).Length == m_isSetEnabledServo.Length)
                {
                    if (m_isSetEnabledServoComp == false)
                    {
                        for (int i = 0; i < Enum.GetNames(typeof(e_Channel)).Length; i++)
                        {
                            SetEnabled(GlobalFunctions.GetControls(TabPage_Manual), $"btn_Manual_Servo_CH{i + 1}", true);
                        }

                        m_isSetEnabledServoComp = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (c_Debug == false)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }
            }
        }

        #endregion

        #region Process

        private void Process()
        {
            try
            {
                #region 반응로, 반응조 온도

                lbl_Process_WaterTank_Targ.Text = $"{_WaterTank_Targ:0.0} ℃";
                lbl_Process_WaterTank_Curr.Text = $"{_WaterTank_Curr:0.0} ℃";

                lbl_Process_CH1_Temperature.Text = $"{_Temperature[(int)e_Channel.CH_01]:0.0} ℃";
                lbl_Process_CH2_Temperature.Text = $"{_Temperature[(int)e_Channel.CH_02]:0.0} ℃";
                lbl_Process_CH3_Temperature.Text = $"{_Temperature[(int)e_Channel.CH_03]:0.0} ℃";
                lbl_Process_CH4_Temperature.Text = $"{_Temperature[(int)e_Channel.CH_04]:0.0} ℃";
                lbl_Process_CH5_Temperature.Text = $"{_Temperature[(int)e_Channel.CH_05]:0.0} ℃";

                #endregion

                #region CH Button BackColor

                // CH_01
                if (_SelectedChannel[(int)e_Channel.CH_01])
                {
                    btn_Process_CH1_Select.BackColor = Color.Lime;
                }
                else
                {
                    btn_Process_CH1_Select.BackColor = _DI[(int)e_DI.Input26_CH1_반응조_센서] ? Color.Yellow : SystemColors.Control;
                }

                // CH_02
                if (_SelectedChannel[(int)e_Channel.CH_02])
                {
                    btn_Process_CH2_Select.BackColor = Color.Lime;
                }
                else
                {
                    btn_Process_CH2_Select.BackColor = _DI[(int)e_DI.Input27_CH2_반응조_센서] ? Color.Yellow : SystemColors.Control;
                }

                // CH_03
                if (_SelectedChannel[(int)e_Channel.CH_03])
                {
                    btn_Process_CH3_Select.BackColor = Color.Lime;
                }
                else
                {
                    btn_Process_CH3_Select.BackColor = _DI[(int)e_DI.Input28_CH3_반응조_센서] ? Color.Yellow : SystemColors.Control;
                }

                // CH_04
                if (_SelectedChannel[(int)e_Channel.CH_04])
                {
                    btn_Process_CH4_Select.BackColor = Color.Lime;
                }
                else
                {
                    btn_Process_CH4_Select.BackColor = _DI[(int)e_DI.Input29_CH4_반응조_센서] ? Color.Yellow : SystemColors.Control;
                }

                // CH_05
                if (_SelectedChannel[(int)e_Channel.CH_05])
                {
                    btn_Process_CH5_Select.BackColor = Color.Lime;
                }
                else
                {
                    btn_Process_CH5_Select.BackColor = _DI[(int)e_DI.Input30_CH5_반응조_센서] ? Color.Yellow : SystemColors.Control;
                }

                #endregion

                #region Button Enabled

                //btn_TabPage_0.Enabled = btn_TabPage_2.Enabled = _Curr_SequenceStep == e_SequenceStep.대기;

                //btn_Process_Parameter.Enabled = _Curr_SequenceStep == e_SequenceStep.대기;

                btn_Process_CH1_Select.Enabled = btn_Process_CH2_Select.Enabled = btn_Process_CH3_Select.Enabled = btn_Process_CH4_Select.Enabled = btn_Process_CH5_Select.Enabled = _Curr_SequenceStep == e_SequenceStep.대기;

                btn_Process_Start.Enabled = _Curr_SequenceStep == e_SequenceStep.대기;

                btn_Process_Interlock.Enabled = _Process_Interlock;

                if (_Curr_SequenceStep == e_SequenceStep.대기)
                {
                    btn_Process_CH1_Stop.Enabled = btn_Process_CH2_Stop.Enabled = btn_Process_CH3_Stop.Enabled = btn_Process_CH4_Stop.Enabled = btn_Process_CH5_Stop.Enabled = false;
                }
                else
                {
                    btn_Process_CH1_Stop.Enabled = _SelectedChannel[(int)e_Channel.CH_01];
                    btn_Process_CH2_Stop.Enabled = _SelectedChannel[(int)e_Channel.CH_02];
                    btn_Process_CH3_Stop.Enabled = _SelectedChannel[(int)e_Channel.CH_03];
                    btn_Process_CH4_Stop.Enabled = _SelectedChannel[(int)e_Channel.CH_04];
                    btn_Process_CH5_Stop.Enabled = _SelectedChannel[(int)e_Channel.CH_05];
                }

                #endregion

                #region

                // TODO: e_SequenceStep

                switch (_Curr_SequenceStep)
                {
                    case e_SequenceStep.대기:
                        #region Color.LightGray

                        // main
                        lbl_Process_Sequence_인산공급        .BackColor = Color.LightGray;
                        lbl_Process_Sequence_흑연공급        .BackColor = Color.LightGray;
                        lbl_Process_Sequence_과망간산칼륨공급.BackColor = Color.LightGray;
                        lbl_Process_Sequence_파우더헤드상승  .BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온      .BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로가열      .BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로온도하강  .BackColor = Color.LightGray;
                        lbl_Process_Sequence_초순수공급      .BackColor = Color.LightGray;
                        lbl_Process_Sequence_과산화수소공급  .BackColor = Color.LightGray;

                        // sub
                        lbl_Process_Sequence_반응로승온_05.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_15.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_25.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_30.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_35.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_40.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_45.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로승온_50.BackColor = Color.LightGray;

                        lbl_Process_Sequence_반응로온도하강_45.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로온도하강_40.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로온도하강_35.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로온도하강_30.BackColor = Color.LightGray;
                        lbl_Process_Sequence_반응로온도하강_25.BackColor = Color.LightGray;

                        lbl_Process_Sequence_초순수공급_1.BackColor = Color.LightGray;
                        lbl_Process_Sequence_초순수공급_2.BackColor = Color.LightGray;
                        lbl_Process_Sequence_초순수공급_3.BackColor = Color.LightGray;
                        lbl_Process_Sequence_초순수공급_4.BackColor = Color.LightGray;

                        lbl_Process_Sequence_과산화수소공급_1.BackColor = Color.LightGray;
                        lbl_Process_Sequence_과산화수소공급_2.BackColor = Color.LightGray;

                        #endregion

                        #region Stopwatch

                        lbl_Process_Sequence_인산공급_Stopwatch.Text = "-";
                        lbl_Process_Sequence_인산공급_Minute   .Text = "-";

                        lbl_Process_Sequence_흑연공급_Stopwatch.Text = "-";
                        lbl_Process_Sequence_흑연공급_Minute   .Text = "-";

                        lbl_Process_Sequence_과망간산칼륨공급_Stopwatch.Text = "-";
                        lbl_Process_Sequence_과망간산칼륨공급_Minute   .Text = "-";

                        lbl_Process_Sequence_반응로승온_Stopwatch.Text = "-";
                        lbl_Process_Sequence_반응로승온_Minute   .Text = "-";

                        lbl_Process_Sequence_반응로가열_Stopwatch.Text = "-";
                        lbl_Process_Sequence_반응로가열_Minute   .Text = "-";

                        lbl_Process_Sequence_반응로온도하강_Stopwatch.Text = "-";
                        lbl_Process_Sequence_반응로온도하강_Minute   .Text = "-";

                        lbl_Process_Sequence_초순수공급_Stopwatch.Text = "-";
                        lbl_Process_Sequence_초순수공급_Minute   .Text = "-";

                        lbl_Process_Sequence_과산화수소공급_Stopwatch.Text = "-";
                        lbl_Process_Sequence_과산화수소공급_Minute   .Text = "-";

                        #endregion

                        #region DateTime

                        lbl_Process_Sequence_인산공급_DateTime1.Text = "-";
                        lbl_Process_Sequence_인산공급_DateTime2.Text = "-";

                        lbl_Process_Sequence_흑연공급_DateTime1.Text = "-";
                        lbl_Process_Sequence_흑연공급_DateTime2.Text = "-";

                        lbl_Process_Sequence_과망간산칼륨공급_DateTime1.Text = "-";
                        lbl_Process_Sequence_과망간산칼륨공급_DateTime2.Text = "-";

                        lbl_Process_Sequence_반응로승온_DateTime1.Text = "-";
                        lbl_Process_Sequence_반응로승온_DateTime2.Text = "-";

                        lbl_Process_Sequence_반응로가열_DateTime1.Text = "-";
                        lbl_Process_Sequence_반응로가열_DateTime2.Text = "-";

                        lbl_Process_Sequence_반응로온도하강_DateTime1.Text = "-";
                        lbl_Process_Sequence_반응로온도하강_DateTime2.Text = "-";

                        lbl_Process_Sequence_초순수공급_DateTime1.Text = "-";
                        lbl_Process_Sequence_초순수공급_DateTime2.Text = "-";

                        lbl_Process_Sequence_과산화수소공급_DateTime1.Text = "-";
                        lbl_Process_Sequence_과산화수소공급_DateTime2.Text = "-";

                        #endregion

                        #region ml

                        lbl_Process_Sequence_초순수공급_ml.Text = "-";

                        lbl_Process_Sequence_과산화수소공급_ml.Text = "-";

                        #endregion
                        break;
                    case e_SequenceStep.인산_Filling_Setup:
                    case e_SequenceStep.인산_Filling_Start:
                    case e_SequenceStep.인산_Filling_Wait:
                    case e_SequenceStep.인산_Filling_Stop:
                        // main
                        lbl_Process_Sequence_인산공급.BackColor = Color.Yellow;


                        // Stopwatch
                        lbl_Process_Sequence_인산공급_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_인산공급_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        // DateTime
                        lbl_Process_Sequence_인산공급_DateTime1.Text = _DateTime[(int)e_SequenceStep.인산_Filling_Setup];
                        break;
                    case e_SequenceStep.흑연_Start:
                    case e_SequenceStep.흑연_Wait:
                    case e_SequenceStep.흑연_Stop:
                    case e_SequenceStep.흑연_Action:
                        // main
                        lbl_Process_Sequence_흑연공급.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_흑연공급_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_흑연공급_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        // DateTime
                        lbl_Process_Sequence_흑연공급_DateTime1.Text = _DateTime[(int)e_SequenceStep.흑연_Start];

                        if (GlobalVariables.Parameter.Sequence.skip_인산 == false &&
                            (lbl_Process_Sequence_인산공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_인산공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_인산공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.흑연_Start];
                        }
                        break;
                    case e_SequenceStep.과망간산칼륨_Start:
                    case e_SequenceStep.과망간산칼륨_Wait:
                    case e_SequenceStep.과망간산칼륨_Stop:
                    case e_SequenceStep.과망간산칼륨_Action:
                        // main
                        lbl_Process_Sequence_과망간산칼륨공급.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_과망간산칼륨공급_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_과망간산칼륨공급_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        // DateTime
                        lbl_Process_Sequence_과망간산칼륨공급_DateTime1.Text = _DateTime[(int)e_SequenceStep.과망간산칼륨_Start];

                        if (GlobalVariables.Parameter.Sequence.skip_인산 == false &&
                            (lbl_Process_Sequence_인산공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_인산공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_인산공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.과망간산칼륨_Start];
                        }
                        if (GlobalVariables.Parameter.Sequence.skip_흑연 == false &&
                            (lbl_Process_Sequence_흑연공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_흑연공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_흑연공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.과망간산칼륨_Start];
                        }
                        break;
                    case e_SequenceStep.실린더_Up:
                        // main
                        lbl_Process_Sequence_파우더헤드상승.BackColor = Color.Yellow;

                        // DateTime
                        if (GlobalVariables.Parameter.Sequence.skip_인산 == false &&
                            (lbl_Process_Sequence_인산공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_인산공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_인산공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.실린더_Up];
                        }
                        if (GlobalVariables.Parameter.Sequence.skip_흑연 == false &&
                            (lbl_Process_Sequence_흑연공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_흑연공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_흑연공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.실린더_Up];
                        }
                        if (GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 == false &&
                            (lbl_Process_Sequence_과망간산칼륨공급_DateTime2.Text == "-" || string.IsNullOrEmpty(lbl_Process_Sequence_과망간산칼륨공급_DateTime2.Text))
                           )
                        {
                            lbl_Process_Sequence_과망간산칼륨공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.실린더_Up];
                        }
                        break;
                    case e_SequenceStep.반응로_1차_승온:
                    case e_SequenceStep.반응로_2차_승온:
                        // main
                        lbl_Process_Sequence_반응로승온.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_반응로승온_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_반응로승온_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        #region sub

                        switch (_Curr_SequenceStep)
                        {
                            case e_SequenceStep.반응로_1차_승온:
                                switch (subSequenceStep)
                                {
                                    case 0:
                                    case 1:
                                        lbl_Process_Sequence_반응로승온_05.BackColor = Color.Yellow;
                                        break;
                                    case 2:
                                    case 3:
                                        lbl_Process_Sequence_반응로승온_15.BackColor = Color.Yellow;
                                        break;
                                    case 4:
                                    case 5:
                                        lbl_Process_Sequence_반응로승온_25.BackColor = Color.Yellow;
                                        break;
                                }

                                if (subSequenceStep > 1) { lbl_Process_Sequence_반응로승온_05.BackColor = Color.Lime; }
                                if (subSequenceStep > 3) { lbl_Process_Sequence_반응로승온_15.BackColor = Color.Lime; }
                                break;
                            case e_SequenceStep.반응로_2차_승온:
                                if (lbl_Process_Sequence_반응로승온_05.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_05.BackColor = Color.Lime; }
                                if (lbl_Process_Sequence_반응로승온_15.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_15.BackColor = Color.Lime; }
                                if (lbl_Process_Sequence_반응로승온_25.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_25.BackColor = Color.Lime; }

                                switch (subSequenceStep)
                                {
                                    case 0:
                                    case 1:
                                        lbl_Process_Sequence_반응로승온_30.BackColor = Color.Yellow;
                                        break;
                                    case 2:
                                    case 3:
                                        lbl_Process_Sequence_반응로승온_35.BackColor = Color.Yellow;
                                        break;
                                    case 4:
                                    case 5:
                                        lbl_Process_Sequence_반응로승온_40.BackColor = Color.Yellow;
                                        break;
                                    case 6:
                                    case 7:
                                        lbl_Process_Sequence_반응로승온_45.BackColor = Color.Yellow;
                                        break;
                                    case 8:
                                    case 9:
                                        lbl_Process_Sequence_반응로승온_50.BackColor = Color.Yellow;
                                        break;
                                }

                                if (subSequenceStep > 1) { lbl_Process_Sequence_반응로승온_30.BackColor = Color.Lime; }
                                if (subSequenceStep > 3) { lbl_Process_Sequence_반응로승온_35.BackColor = Color.Lime; }
                                if (subSequenceStep > 5) { lbl_Process_Sequence_반응로승온_40.BackColor = Color.Lime; }
                                if (subSequenceStep > 7) { lbl_Process_Sequence_반응로승온_45.BackColor = Color.Lime; }
                                break;
                        }

                        #endregion

                        // DateTime
                        lbl_Process_Sequence_반응로승온_DateTime1.Text = _DateTime[(int)e_SequenceStep.반응로_1차_승온];
                        break;
                    case e_SequenceStep.반응로_가열:
                        // main
                        lbl_Process_Sequence_반응로가열.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_반응로가열_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_반응로가열_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        #region sub

                        if (lbl_Process_Sequence_반응로승온_30.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_30.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로승온_35.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_35.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로승온_40.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_40.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로승온_45.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_45.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로승온_50.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로승온_50.BackColor = Color.Lime; }

                        #endregion

                        // DateTime
                        lbl_Process_Sequence_반응로승온_DateTime2.Text = _DateTime[(int)e_SequenceStep.반응로_가열];
                        lbl_Process_Sequence_반응로가열_DateTime1.Text = _DateTime[(int)e_SequenceStep.반응로_가열];
                        break;
                    case e_SequenceStep.반응로_온도_하강:
                        // main
                        lbl_Process_Sequence_반응로온도하강.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_반응로온도하강_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_반응로온도하강_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        #region sub

                        switch (subSequenceStep)
                        {
                            case 0:
                            case 1:
                                lbl_Process_Sequence_반응로온도하강_45.BackColor = Color.Yellow;
                                break;
                            case 2:
                            case 3:
                                lbl_Process_Sequence_반응로온도하강_40.BackColor = Color.Yellow;
                                break;
                            case 4:
                            case 5:
                                lbl_Process_Sequence_반응로온도하강_35.BackColor = Color.Yellow;
                                break;
                            case 6:
                            case 7:
                                lbl_Process_Sequence_반응로온도하강_30.BackColor = Color.Yellow;
                                break;
                            case 8:
                            case 9:
                                lbl_Process_Sequence_반응로온도하강_25.BackColor = Color.Yellow;
                                break;
                        }

                        if (subSequenceStep > 1) { lbl_Process_Sequence_반응로온도하강_45.BackColor = Color.Lime; }
                        if (subSequenceStep > 3) { lbl_Process_Sequence_반응로온도하강_40.BackColor = Color.Lime; }
                        if (subSequenceStep > 5) { lbl_Process_Sequence_반응로온도하강_35.BackColor = Color.Lime; }
                        if (subSequenceStep > 7) { lbl_Process_Sequence_반응로온도하강_30.BackColor = Color.Lime; }

                        #endregion

                        // DateTime
                        lbl_Process_Sequence_반응로가열_DateTime2.Text = _DateTime[(int)e_SequenceStep.반응로_온도_하강];
                        lbl_Process_Sequence_반응로온도하강_DateTime1.Text = _DateTime[(int)e_SequenceStep.반응로_온도_하강];
                        break;
                    case e_SequenceStep.초순수_1차_Filling_Setup:
                    case e_SequenceStep.초순수_1차_Filling_Start:
                    case e_SequenceStep.초순수_1차_Filling_Wait:
                    case e_SequenceStep.초순수_1차_Filling_Stop:
                    case e_SequenceStep.초순수_1차_반응조_확인:
                    case e_SequenceStep.초순수_2차_Filling_Start:
                    case e_SequenceStep.초순수_2차_Filling_Wait:
                    case e_SequenceStep.초순수_2차_Filling_Stop:
                    case e_SequenceStep.초순수_2차_반응조_확인:
                    case e_SequenceStep.초순수_3차_Filling_Setup:
                    case e_SequenceStep.초순수_3차_Filling_Start:
                    case e_SequenceStep.초순수_3차_Filling_Wait:
                    case e_SequenceStep.초순수_3차_Filling_Stop:
                    case e_SequenceStep.초순수_3차_반응조_확인:
                    case e_SequenceStep.초순수_4차_Filling_Setup:
                    case e_SequenceStep.초순수_4차_Filling_Start:
                    case e_SequenceStep.초순수_4차_Filling_Wait:
                    case e_SequenceStep.초순수_4차_Filling_Stop:
                    case e_SequenceStep.반응물_교반:
                        // main
                        lbl_Process_Sequence_초순수공급.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_초순수공급_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_초순수공급_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        #region sub

                        if (lbl_Process_Sequence_반응로온도하강_45.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로온도하강_45.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로온도하강_40.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로온도하강_40.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로온도하강_35.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로온도하강_35.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로온도하강_30.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로온도하강_30.BackColor = Color.Lime; }
                        if (lbl_Process_Sequence_반응로온도하강_25.BackColor == Color.Yellow) { lbl_Process_Sequence_반응로온도하강_25.BackColor = Color.Lime; }

                        switch (_Curr_SequenceStep)
                        {
                            case e_SequenceStep.초순수_1차_Filling_Setup:
                            case e_SequenceStep.초순수_1차_Filling_Start:
                            case e_SequenceStep.초순수_1차_Filling_Wait:
                            case e_SequenceStep.초순수_1차_Filling_Stop:
                                lbl_Process_Sequence_초순수공급_1.BackColor = Color.Yellow;
                                break;
                            case e_SequenceStep.초순수_2차_Filling_Start:
                            case e_SequenceStep.초순수_2차_Filling_Wait:
                            case e_SequenceStep.초순수_2차_Filling_Stop:
                                lbl_Process_Sequence_초순수공급_2.BackColor = Color.Yellow;
                                break;
                            case e_SequenceStep.초순수_3차_Filling_Setup:
                            case e_SequenceStep.초순수_3차_Filling_Start:
                            case e_SequenceStep.초순수_3차_Filling_Wait:
                            case e_SequenceStep.초순수_3차_Filling_Stop:
                                lbl_Process_Sequence_초순수공급_3.BackColor = Color.Yellow;
                                break;
                            case e_SequenceStep.초순수_4차_Filling_Setup:
                            case e_SequenceStep.초순수_4차_Filling_Start:
                            case e_SequenceStep.초순수_4차_Filling_Wait:
                            case e_SequenceStep.초순수_4차_Filling_Stop:
                                lbl_Process_Sequence_초순수공급_4.BackColor = Color.Yellow;
                                break;
                        }

                        #endregion

                        // DateTime
                        lbl_Process_Sequence_반응로온도하강_DateTime2.Text = _DateTime[(int)e_SequenceStep.초순수_1차_Filling_Setup];
                        lbl_Process_Sequence_초순수공급_DateTime1.Text = _DateTime[(int)e_SequenceStep.초순수_1차_Filling_Setup];
                        break;
                    case e_SequenceStep.과산화수소_1차_Filling_Setup:
                    case e_SequenceStep.과산화수소_1차_Filling_Start:
                    case e_SequenceStep.과산화수소_1차_Filling_Wait:
                    case e_SequenceStep.과산화수소_1차_Filling_Stop:
                    case e_SequenceStep.과산화수소_2차_Filling_Setup:
                    case e_SequenceStep.과산화수소_2차_Filling_Start:
                    case e_SequenceStep.과산화수소_2차_Filling_Wait:
                    case e_SequenceStep.과산화수소_2차_Filling_Stop:
                        // main
                        lbl_Process_Sequence_과산화수소공급.BackColor = Color.Yellow;

                        // Stopwatch
                        lbl_Process_Sequence_과산화수소공급_Stopwatch.Text = $"{(double)_SequenceStopwatch.ElapsedMilliseconds / 1000 / 60:0.0}";
                        lbl_Process_Sequence_과산화수소공급_Minute.Text = $"/ {_SequenceMinute / 1000 / 60} 분";

                        #region sub

                        switch (_Curr_SequenceStep)
                        {
                            case e_SequenceStep.과산화수소_1차_Filling_Setup:
                            case e_SequenceStep.과산화수소_1차_Filling_Start:
                            case e_SequenceStep.과산화수소_1차_Filling_Wait:
                            case e_SequenceStep.과산화수소_1차_Filling_Stop:
                                lbl_Process_Sequence_과산화수소공급_1.BackColor = Color.Yellow;
                                break;
                            case e_SequenceStep.과산화수소_2차_Filling_Setup:
                            case e_SequenceStep.과산화수소_2차_Filling_Start:
                            case e_SequenceStep.과산화수소_2차_Filling_Wait:
                            case e_SequenceStep.과산화수소_2차_Filling_Stop:
                                lbl_Process_Sequence_과산화수소공급_2.BackColor = Color.Yellow;
                                break;
                        }

                        #endregion

                        // DateTime
                        lbl_Process_Sequence_초순수공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.과산화수소_1차_Filling_Setup];
                        lbl_Process_Sequence_과산화수소공급_DateTime1.Text = _DateTime[(int)e_SequenceStep.과산화수소_1차_Filling_Setup];
                        break;
                    case e_SequenceStep.Servo_Down:
                    case e_SequenceStep.수조_Stop:
                    case e_SequenceStep.완료:
                        // DateTime
                        lbl_Process_Sequence_과산화수소공급_DateTime2.Text = _DateTime[(int)e_SequenceStep.Servo_Down];
                        break;
                }

                #endregion

                #region Color.Lime

                // TODO: e_SequenceStep

                // main
                //if (_Curr_SequenceStep > e_SequenceStep.인산_Recycle_Stop          ) { lbl_Process_Sequence_인산공급        .BackColor = Color.Lime; }
                //if (_Curr_SequenceStep > e_SequenceStep.흑연_Action                ) { lbl_Process_Sequence_흑연공급        .BackColor = Color.Lime; }
                //if (_Curr_SequenceStep > e_SequenceStep.과망간산칼륨_Action        ) { lbl_Process_Sequence_과망간산칼륨공급.BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.실린더_Up                  ) { lbl_Process_Sequence_파우더헤드상승  .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.반응로_2차_승온            ) { lbl_Process_Sequence_반응로승온      .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.반응로_가열                ) { lbl_Process_Sequence_반응로가열      .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.반응로_온도_하강           ) { lbl_Process_Sequence_반응로온도하강  .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.반응물_교반                ) { lbl_Process_Sequence_초순수공급      .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.과산화수소_2차_Filling_Stop) { lbl_Process_Sequence_과산화수소공급  .BackColor = Color.Lime; }

                // sub
                if (_Curr_SequenceStep > e_SequenceStep.초순수_1차_Filling_Stop    ) { lbl_Process_Sequence_초순수공급_1    .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.초순수_2차_Filling_Stop    ) { lbl_Process_Sequence_초순수공급_2    .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.초순수_3차_Filling_Stop    ) { lbl_Process_Sequence_초순수공급_3    .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.초순수_4차_Filling_Stop    ) { lbl_Process_Sequence_초순수공급_4    .BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.과산화수소_1차_Filling_Stop) { lbl_Process_Sequence_과산화수소공급_1.BackColor = Color.Lime; }
                if (_Curr_SequenceStep > e_SequenceStep.과산화수소_2차_Filling_Stop) { lbl_Process_Sequence_과산화수소공급_2.BackColor = Color.Lime; }

                #region 2023-12-16: 공정 skip parameter 추가에 따른 Color.Lime

                if (GlobalVariables.Parameter.Sequence.skip_인산 == false)
                {
                    if (_Curr_SequenceStep > e_SequenceStep.인산_Recycle_Stop) { lbl_Process_Sequence_인산공급.BackColor = Color.Lime; }
                }

                if (GlobalVariables.Parameter.Sequence.skip_흑연 == false)
                {
                    if (_Curr_SequenceStep > e_SequenceStep.흑연_Action) { lbl_Process_Sequence_흑연공급.BackColor = Color.Lime; }
                }

                if (GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 == false)
                {
                    if (_Curr_SequenceStep > e_SequenceStep.과망간산칼륨_Action) { lbl_Process_Sequence_과망간산칼륨공급.BackColor = Color.Lime; }
                }

                #endregion

                #endregion

                #region Parameter

                lbl_Process_Sequence_인산공급_ml.Text = GlobalVariables.Parameter.Sequence.인산_vol;
                lbl_Process_Sequence_인산공급_min.Text = GlobalVariables.Parameter.Sequence.인산_min.ToString();

                lbl_Process_Sequence_흑연공급_rev.Text = GlobalVariables.Parameter.Sequence.흑연_rev;
                lbl_Process_Sequence_흑연공급_rpm.Text = GlobalVariables.Parameter.Sequence.흑연_rpm;

                lbl_Process_Sequence_과망간산칼륨공급_rev.Text = GlobalVariables.Parameter.Sequence.과망간산칼륨_rev;
                lbl_Process_Sequence_과망간산칼륨공급_rpm.Text = GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm;

                lbl_Process_Sequence_초순수_1차_공급_ml .Text = GlobalVariables.Parameter.Sequence.초순수_1차_vol;
                lbl_Process_Sequence_초순수_1차_공급_min.Text = GlobalVariables.Parameter.Sequence.초순수_1차_min.ToString();

                lbl_Process_Sequence_초순수_3차_공급_ml .Text = GlobalVariables.Parameter.Sequence.초순수_3차_vol;
                lbl_Process_Sequence_초순수_3차_공급_min.Text = GlobalVariables.Parameter.Sequence.초순수_3차_min.ToString();

                lbl_Process_Sequence_초순수_4차_공급_ml .Text = GlobalVariables.Parameter.Sequence.초순수_4차_vol;
                lbl_Process_Sequence_초순수_4차_공급_min.Text = GlobalVariables.Parameter.Sequence.초순수_4차_min.ToString();

                lbl_Process_Sequence_과산화수소_1차_공급_ml .Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_vol;
                lbl_Process_Sequence_과산화수소_1차_공급_min.Text = GlobalVariables.Parameter.Sequence.과산화수소_1차_min.ToString();

                lbl_Process_Sequence_과산화수소_2차_공급_ml .Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_vol;
                lbl_Process_Sequence_과산화수소_2차_공급_min.Text = GlobalVariables.Parameter.Sequence.과산화수소_2차_min.ToString();

                #endregion

                #region ml

                switch (_Curr_SequenceStep)
                {
                    case e_SequenceStep.초순수_1차_Filling_Wait    : lbl_Process_Sequence_초순수공급_ml    .Text = $"{GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec     * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                    case e_SequenceStep.초순수_2차_Filling_Wait    : lbl_Process_Sequence_초순수공급_ml    .Text = $"{GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec     * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                    case e_SequenceStep.초순수_3차_Filling_Wait    : lbl_Process_Sequence_초순수공급_ml    .Text = $"{GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec     * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                    case e_SequenceStep.초순수_4차_Filling_Wait    : lbl_Process_Sequence_초순수공급_ml    .Text = $"{GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec     * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                    case e_SequenceStep.과산화수소_1차_Filling_Wait: lbl_Process_Sequence_과산화수소공급_ml.Text = $"{GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                    case e_SequenceStep.과산화수소_2차_Filling_Wait: lbl_Process_Sequence_과산화수소공급_ml.Text = $"{GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec * ((double)_SequenceStopwatch.ElapsedMilliseconds / 1000):0.00} ml"; break;
                }

                #endregion

                #region 2023-12-16: 공정 skip parameter 추가에 따른 skip label Visible

                lbl_Process_skip_인산공급.BackColor = SystemColors.Highlight;
                lbl_Process_skip_인산공급.ForeColor = SystemColors.HighlightText;
                lbl_Process_skip_인산공급.Visible = GlobalVariables.Parameter.Sequence.skip_인산;

                lbl_Process_skip_흑연공급.BackColor = SystemColors.Highlight;
                lbl_Process_skip_흑연공급.ForeColor = SystemColors.HighlightText;
                lbl_Process_skip_흑연공급.Visible = GlobalVariables.Parameter.Sequence.skip_흑연;

                lbl_Process_skip_과망간산칼륨공급.BackColor = SystemColors.Highlight;
                lbl_Process_skip_과망간산칼륨공급.ForeColor = SystemColors.HighlightText;
                lbl_Process_skip_과망간산칼륨공급.Visible = GlobalVariables.Parameter.Sequence.skip_과망간산칼륨;

                #endregion
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
        }

        #endregion
    }
}
