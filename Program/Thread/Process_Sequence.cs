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
        #region // TODO: e_SequenceStep

        private enum e_SequenceStep
        {
            대기,
            인산_교반기_Start,
            인산_Filling_Setup,
            인산_Filling_Start,
            인산_Filling_Wait,
            인산_Filling_Stop,
            인산_Recycle_Start,
            인산_Recycle_Wait,
            인산_Recycle_Stop,
            흑연_교반기_Start,
            흑연_Start,
            흑연_Wait,
            흑연_Stop,
            흑연_Action,
            과망간산칼륨_교반기_Start,
            과망간산칼륨_Start,
            과망간산칼륨_Wait,
            과망간산칼륨_Stop,
            과망간산칼륨_Action,
            실린더_Up,
            반응로_교반기_Start,
            반응로_1차_승온,
            반응로_2차_승온,
            반응로_가열,
            반응로_온도_하강,
            초순수_교반기_Start,
            초순수_1차_Filling_Setup,
            초순수_1차_Filling_Start,
            초순수_1차_Filling_Wait,
            초순수_1차_Filling_Stop,
            초순수_1차_반응조_확인,
            초순수_2차_Filling_Start,
            초순수_2차_Filling_Wait,
            초순수_2차_Filling_Stop,
            초순수_2차_반응조_확인,
            초순수_3차_Filling_Setup,
            초순수_3차_Filling_Start,
            초순수_3차_Filling_Wait,
            초순수_3차_Filling_Stop,
            초순수_3차_반응조_확인,
            초순수_4차_Filling_Setup,
            초순수_4차_Filling_Start,
            초순수_4차_Filling_Wait,
            초순수_4차_Filling_Stop,
            초순수_Recycle_Start,
            초순수_Recycle_Wait,
            초순수_Recycle_Stop,
            반응물_교반,
            Servo_Up,
            과산화수소_교반기_Start,
            과산화수소_1차_Filling_Setup,
            과산화수소_1차_Filling_Start,
            과산화수소_1차_Filling_Wait,
            과산화수소_1차_Filling_Stop,
            과산화수소_2차_Filling_Setup,
            과산화수소_2차_Filling_Start,
            과산화수소_2차_Filling_Wait,
            과산화수소_2차_Filling_Stop,
            Servo_Down,
            수조_Stop,
            완료,
        }
        private e_SequenceStep _Prev_SequenceStep = e_SequenceStep.대기;
        private e_SequenceStep _Curr_SequenceStep = e_SequenceStep.대기;

        #endregion

        private bool _Process_Start = false;

        private int _SequenceMinute = 0;
        private System.Diagnostics.Stopwatch _SequenceStopwatch = new System.Diagnostics.Stopwatch();

        private bool _Process_Interlock = false;
        private bool _Process_Interlock_Click = false;

        private int subSequenceStep = 0;

        private string[] _DateTime = new string[Enum.GetNames(typeof(e_SequenceStep)).Length];

        private System.Threading.Thread _ThreadSequence = null;
        private bool _isThreadSequence = false;
        private async void Process_Sequence()
        {
            string call = "Process_Sequence";

            #region local

            bool isSequenceStopwatchRunning = false;

            string NG = string.Empty;
            bool[] commResult = new bool[Enum.GetNames(typeof(e_Channel)).Length];
            bool check;
            int second = 0;
            List<Task<bool>> tasks = new List<Task<bool>>(Enum.GetNames(typeof(e_Channel)).Length);
            double waterTankTemp = 0;

            List<Task<t_ServoUpResult>> servoUpDownTasks = new List<Task<t_ServoUpResult>>(Enum.GetNames(typeof(e_Channel)).Length);

            System.Diagnostics.Stopwatch stopwatchLog = new System.Diagnostics.Stopwatch();

            // TODO: 2024-01-09 온도 승온 및 하강 단계가 바뀔 때마다 실린더_Up 수행
            int subPrevSequenceStep = 0;

            #endregion

            while (_isThreadSequence)
            {
                try
                {
                    #region Prev != Curr

                    if (_Prev_SequenceStep != _Curr_SequenceStep)
                    {
                        Log.Write(call, $"---------------------------------------------------------------------- [{_Prev_SequenceStep}] 완료");
                        _Prev_SequenceStep = _Curr_SequenceStep;
                        Log.Write(call, $"---------------------------------------------------------------------- [{_Curr_SequenceStep}] 시작");

                        Array.Clear(commResult, 0, commResult.Length);

                        _DateTime[(int)_Curr_SequenceStep] = GlobalFunctions.GetDateTimeStringWithoutSecond(DateTime.Now);

                        #region stopwatchLog Stop or Restart

                        if (_Curr_SequenceStep == e_SequenceStep.대기 ||
                            _Curr_SequenceStep == e_SequenceStep.완료
                           )
                        {
                            if (stopwatchLog.IsRunning)
                            {
                                stopwatchLog.Stop();
                            }
                        }
                        else
                        {
                            if (stopwatchLog.IsRunning == false)
                            {
                                stopwatchLog.Restart();
                            }
                        }

                        #endregion

                        #region TODO: 2024-01-09 실린더_Up 공정 이후 공정이 바뀔 때마다 실린더_Up 수행

                        //if (_Curr_SequenceStep > e_SequenceStep.실린더_Up)
                        //{
                        //    tasks.Clear();

                        //    for (int i = 0; i < _SelectedChannel.Length; i++)
                        //    {
                        //        //if (_SelectedChannel[i])
                        //        {
                        //            tasks.Add(실린더제어((e_Channel)i, true));
                        //        }
                        //    }

                        //    await Task.WhenAll(tasks);
                        //}

                        #endregion
                    }

                    #endregion

                    #region 자동공정 중 1분마다 온도 로깅

                    if (_Curr_SequenceStep > e_SequenceStep.대기 ||
                        _Curr_SequenceStep < e_SequenceStep.완료
                       )
                    {
                        if (stopwatchLog.ElapsedMilliseconds >= 1000 * 60 * 1)
                        {
                            string text = $"{_WaterTank_Curr:0.0}";

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                text += $"\t{_Temperature[i]:0.0}";
                            }

                            SequenceLog(text, "Temperature");

                            stopwatchLog.Restart();
                        }
                    }

                    #endregion

                    #region interlock

                    if (_Process_Interlock && _Process_Interlock_Click)
                    {
                        _Process_Interlock_Click = false;

                        if (_Curr_SequenceStep >= e_SequenceStep.반응로_1차_승온 &&
                            _Curr_SequenceStep <= e_SequenceStep.반응로_가열
                           )
                        {
                            _Process_Interlock = !_CWaterTank.Start();
                        }
                        else if (_Curr_SequenceStep >= e_SequenceStep.초순수_1차_Filling_Setup &&
                                 _Curr_SequenceStep <= e_SequenceStep.과산화수소_2차_Filling_Stop
                                )
                        {
                            _Process_Interlock = false;
                        }

                        if (_Process_Interlock == false)
                        {
                            if (isSequenceStopwatchRunning)
                            {
                                isSequenceStopwatchRunning = false;

                                _SequenceStopwatch.Start();
                            }

                            Log.Write(call, $"인터록 해제");
                        }
                    }

                    if (_Process_Interlock == false)
                    {
                        if (_Curr_SequenceStep >= e_SequenceStep.반응로_1차_승온 &&
                            _Curr_SequenceStep <= e_SequenceStep.반응로_가열
                           )
                        {
                            if (CheckInterlock_반응로승온또는가열())
                            {
                                _Process_Interlock = _CWaterTank.Stop();
                            }
                        }
                        else if (_Curr_SequenceStep >= e_SequenceStep.초순수_1차_Filling_Setup &&
                                 _Curr_SequenceStep <= e_SequenceStep.과산화수소_2차_Filling_Stop
                                )
                        {
                            if (CheckInterlock_부반응물제거())
                            {
                                _Process_Interlock = _CNextPump.FillingStop();
                            }
                        }

                        if (_Process_Interlock)
                        {
                            if (_SequenceStopwatch.IsRunning)
                            {
                                isSequenceStopwatchRunning = true;

                                _SequenceStopwatch.Stop();
                            }

                            Log.Write(call, $"[{_Curr_SequenceStep}] 진행 중 인터록 발생");
                        }
                    }

                    if (_Process_Interlock)
                    {
                        continue;
                    }

                    #endregion

                    switch (_Curr_SequenceStep)
                    {
                        case e_SequenceStep.대기:
                            if (_Process_Start)
                            {
                                GlobalFunctions.ViewParameter();

                                if (CheckSequence(GlobalVariables.Parameter.Sequence.test, ref NG))
                                {
                                    if (await StartupSequence())
                                    {
                                        #region 분기

                                        if (GlobalVariables.Parameter.Sequence.skip_인산 == false)
                                        {
                                            _Curr_SequenceStep = e_SequenceStep.인산_교반기_Start;
                                        }
                                        else if (GlobalVariables.Parameter.Sequence.skip_흑연 == false)
                                        {
                                            _Curr_SequenceStep = e_SequenceStep.흑연_교반기_Start;
                                        }
                                        else if (GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 == false)
                                        {
                                            _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_교반기_Start;
                                        }
                                        else
                                        {
                                            _Curr_SequenceStep = e_SequenceStep.실린더_Up;
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        GlobalFunctions.MessageBox(call, $"StartupSequence NG");
                                    }
                                }
                                else
                                {
                                    GlobalFunctions.MessageBox(call, $"{NG}");
                                }

                                _Process_Start = false;
                            }
                            break;
                        case e_SequenceStep.인산_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_인산);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.인산_Filling_Setup;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.인산_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_11] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_12] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_13] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_14] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_15] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.인산_vol, GlobalVariables.Parameter.Sequence.인산_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.인산_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.인산_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.인산_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.인산_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.인산_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.인산_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.인산_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.인산_Recycle_Start;
                            }
                            break;
                        case e_SequenceStep.인산_Recycle_Start:
                            if (_CNextPump.RecycleStart())
                            {
                                second = SetSecond(GlobalVariables.Parameter.Sequence.Recycle_sec); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.인산_Recycle_Wait;
                            }
                            break;
                        case e_SequenceStep.인산_Recycle_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= second)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.인산_Recycle_Stop;
                            }
                            break;
                        case e_SequenceStep.인산_Recycle_Stop:
                            if (_CNextPump.RecycleStop())
                            {
                                #region 분기

                                if (GlobalVariables.Parameter.Sequence.skip_흑연 == false)
                                {
                                    _Curr_SequenceStep = e_SequenceStep.흑연_교반기_Start;
                                }
                                else if (GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 == false)
                                {
                                    _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_교반기_Start;
                                }
                                else
                                {
                                    _Curr_SequenceStep = e_SequenceStep.실린더_Up;
                                }

                                #endregion
                            }
                            break;
                        case e_SequenceStep.흑연_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_흑연);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.흑연_Start;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.흑연_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CStep[GetStepIdx((e_Channel)i, e_Dispenser.흑연)].MoveIncPos(GlobalVariables.Parameter.Sequence.흑연_rev, GlobalVariables.Parameter.Sequence.흑연_rpm);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.흑연_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.흑연_Wait;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.흑연_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.흑연_Stop;
                            }
                            break;
                        case e_SequenceStep.흑연_Stop:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CStep[GetStepIdx((e_Channel)i, e_Dispenser.흑연)].MoveStop();
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.흑연_Action;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.흑연_Action:
                            tasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    tasks.Add(StepAction((e_Channel)i, e_Dispenser.흑연));
                                }
                            }

                            await Task.WhenAll(tasks);

                            #region check

                            check = true;

                            for (int i = 0; i < tasks.Count; i++)
                            {
                                check &= tasks[i].Result;
                            }

                            if (check)
                            {
                                #region 분기

                                if (GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 == false)
                                {
                                    _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_교반기_Start;
                                }
                                else
                                {
                                    _Curr_SequenceStep = e_SequenceStep.실린더_Up;
                                }

                                #endregion
                            }

                            #endregion
                            break;
                        case e_SequenceStep.과망간산칼륨_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_Start;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.과망간산칼륨_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CStep[GetStepIdx((e_Channel)i, e_Dispenser.과망간산칼륨)].MoveIncPos(GlobalVariables.Parameter.Sequence.과망간산칼륨_rev, GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.과망간산칼륨_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_Wait;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.과망간산칼륨_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_Stop;
                            }
                            break;
                        case e_SequenceStep.과망간산칼륨_Stop:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CStep[GetStepIdx((e_Channel)i, e_Dispenser.과망간산칼륨)].MoveStop();
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.과망간산칼륨_Action;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.과망간산칼륨_Action:
                            tasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    tasks.Add(StepAction((e_Channel)i, e_Dispenser.과망간산칼륨));
                                }
                            }

                            await Task.WhenAll(tasks);

                            #region check

                            check = true;

                            for (int i = 0; i < tasks.Count; i++)
                            {
                                check &= tasks[i].Result;
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.실린더_Up;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.실린더_Up:
                            tasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    tasks.Add(실린더제어((e_Channel)i, true));
                                }
                            }

                            await Task.WhenAll(tasks);

                            #region check

                            check = true;

                            for (int i = 0; i < tasks.Count; i++)
                            {
                                check &= tasks[i].Result;
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.반응로_교반기_Start;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.반응로_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_반응로);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                subSequenceStep = 0;

                                _Curr_SequenceStep = e_SequenceStep.반응로_1차_승온;

                                subPrevSequenceStep = 0;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.반응로_1차_승온:
                            #region TODO: 2024-01-09 온도 승온 및 하강 단계가 바뀔 때마다 실린더_Up 수행

                            //if (subPrevSequenceStep != subSequenceStep)
                            //{
                            //    subPrevSequenceStep = subSequenceStep;

                            //    switch (subSequenceStep)
                            //    {
                            //        case 1:
                            //        case 3:
                            //        case 5:
                            //            tasks.Clear();

                            //            for (int i = 0; i < _SelectedChannel.Length; i++)
                            //            {
                            //                //if (_SelectedChannel[i])
                            //                {
                            //                    tasks.Add(실린더제어((e_Channel)i, true));
                            //                }
                            //            }

                            //            await Task.WhenAll(tasks);
                            //            break;
                            //    }
                            //}

                            #endregion

                            switch (subSequenceStep)
                            {
                                case 0:
                                    waterTankTemp = 5;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_1차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 1:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 2:
                                    waterTankTemp = 15;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_1차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 3:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 4:
                                    waterTankTemp = 25;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_1차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 5:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep = 0;

                                            _Curr_SequenceStep = e_SequenceStep.반응로_2차_승온;

                                            subPrevSequenceStep = 0;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case e_SequenceStep.반응로_2차_승온:
                            #region TODO: 2024-01-09 온도 승온 및 하강 단계가 바뀔 때마다 실린더_Up 수행

                            //if (subPrevSequenceStep != subSequenceStep)
                            //{
                            //    subPrevSequenceStep = subSequenceStep;

                            //    switch (subSequenceStep)
                            //    {
                            //        case 1:
                            //        case 3:
                            //        case 5:
                            //        case 7:
                            //        case 9:
                            //            tasks.Clear();

                            //            for (int i = 0; i < _SelectedChannel.Length; i++)
                            //            {
                            //                //if (_SelectedChannel[i])
                            //                {
                            //                    tasks.Add(실린더제어((e_Channel)i, true));
                            //                }
                            //            }

                            //            await Task.WhenAll(tasks);
                            //            break;
                            //    }
                            //}

                            #endregion

                            switch (subSequenceStep)
                            {
                                case 0:
                                    waterTankTemp = 30;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_2차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 1:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 2:
                                    waterTankTemp = 35;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_2차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 3:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 4:
                                    waterTankTemp = 40;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_2차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 5:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 6:
                                    waterTankTemp = 45;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_2차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 7:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 8:
                                    waterTankTemp = 50;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_2차_승온_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 9:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_가열_min); _SequenceStopwatch.Restart();

                                            _Curr_SequenceStep = e_SequenceStep.반응로_가열;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case e_SequenceStep.반응로_가열:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                subSequenceStep = 0;

                                _Curr_SequenceStep = e_SequenceStep.반응로_온도_하강;

                                subPrevSequenceStep = 0;
                            }
                            break;
                        case e_SequenceStep.반응로_온도_하강:
                            #region TODO: 2024-01-09 온도 승온 및 하강 단계가 바뀔 때마다 실린더_Up 수행

                            //if (subPrevSequenceStep != subSequenceStep)
                            //{
                            //    subPrevSequenceStep = subSequenceStep;

                            //    switch (subSequenceStep)
                            //    {
                            //        case 1:
                            //        case 3:
                            //        case 5:
                            //        case 7:
                            //        case 9:
                            //            tasks.Clear();

                            //            for (int i = 0; i < _SelectedChannel.Length; i++)
                            //            {
                            //                //if (_SelectedChannel[i])
                            //                {
                            //                    tasks.Add(실린더제어((e_Channel)i, true));
                            //                }
                            //            }

                            //            await Task.WhenAll(tasks);
                            //            break;
                            //    }
                            //}

                            #endregion

                            switch (subSequenceStep)
                            {
                                case 0:
                                    waterTankTemp = 45;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_온도_하강_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 1:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 2:
                                    waterTankTemp = 40;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_온도_하강_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 3:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 4:
                                    waterTankTemp = 35;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_온도_하강_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 5:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 6:
                                    waterTankTemp = 30;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_온도_하강_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 7:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            subSequenceStep++;
                                        }
                                    }
                                    break;
                                case 8:
                                    waterTankTemp = 25;

                                    if (_CWaterTank.SetTemperature(waterTankTemp.ToString()))
                                    {
                                        _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응로_온도_하강_min); _SequenceStopwatch.Restart();

                                        subSequenceStep++;
                                    }
                                    break;
                                case 9:
                                    if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                                    {
                                        _SequenceStopwatch.Stop();

                                        if (CheckWaterTankTemp(waterTankTemp) || GlobalVariables.Parameter.Sequence.test)
                                        {
                                            _Curr_SequenceStep = e_SequenceStep.초순수_교반기_Start;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case e_SequenceStep.초순수_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_초순수);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_1차_Filling_Setup;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.초순수_1차_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.초순수_1차_vol, GlobalVariables.Parameter.Sequence.초순수_1차_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_1차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.초순수_1차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.초순수_1차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.초순수_1차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.초순수_1차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.초순수_1차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.초순수_1차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_1차_반응조_확인;
                            }
                            break;
                        case e_SequenceStep.초순수_1차_반응조_확인:
                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= CheckTemperature((e_Channel)i, GlobalVariables.Parameter.Sequence.반응조확인);
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_2차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.초순수_2차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.초순수_1차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.초순수_2차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.초순수_2차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.초순수_2차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.초순수_2차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_2차_반응조_확인;
                            }
                            break;
                        case e_SequenceStep.초순수_2차_반응조_확인:
                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= CheckTemperature((e_Channel)i, GlobalVariables.Parameter.Sequence.반응조확인);
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_3차_Filling_Setup;
                            }
                            break;
                        case e_SequenceStep.초순수_3차_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.초순수_3차_vol, GlobalVariables.Parameter.Sequence.초순수_3차_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_3차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.초순수_3차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.초순수_3차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.초순수_3차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.초순수_3차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.초순수_3차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.초순수_3차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_3차_반응조_확인;
                            }
                            break;
                        case e_SequenceStep.초순수_3차_반응조_확인:
                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= CheckTemperature((e_Channel)i, GlobalVariables.Parameter.Sequence.반응조확인);
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_4차_Filling_Setup;
                            }
                            break;
                        case e_SequenceStep.초순수_4차_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_01] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_02] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_03] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_04] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_05] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.초순수_4차_vol, GlobalVariables.Parameter.Sequence.초순수_4차_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_4차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.초순수_4차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.초순수_4차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.초순수_4차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.초순수_4차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.초순수_4차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.초순수_4차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.초순수_Recycle_Start;
                            }
                            break;
                        case e_SequenceStep.초순수_Recycle_Start:
                            if (_CNextPump.RecycleStart())
                            {
                                second = SetSecond(GlobalVariables.Parameter.Sequence.Recycle_sec); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.초순수_Recycle_Wait;
                            }
                            break;
                        case e_SequenceStep.초순수_Recycle_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= second)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.초순수_Recycle_Stop;
                            }
                            break;
                        case e_SequenceStep.초순수_Recycle_Stop:
                            if (_CNextPump.RecycleStop())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.반응물_교반_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.반응물_교반;
                            }
                            break;
                        case e_SequenceStep.반응물_교반:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.과산화수소_1차_Filling_Setup;

                                #region 2023-12-12: 공정(과산화수소) 이전에 서보 상승 step 추가

                                _Curr_SequenceStep = e_SequenceStep.Servo_Up;

                                #endregion
                            }
                            break;
                        case e_SequenceStep.Servo_Up:
                            servoUpDownTasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    servoUpDownTasks.Add(ServoUpDown((e_Channel)i, true));
                                }
                            }

                            await Task.WhenAll(servoUpDownTasks);

                            for (int i = 0; i < servoUpDownTasks.Count; i++)
                            {
                                if (servoUpDownTasks[i].Result.result == false)
                                {
                                    _SelectedChannel[(int)servoUpDownTasks[i].Result.channel] = false;
                                }
                            }

                            _Curr_SequenceStep = e_SequenceStep.과산화수소_교반기_Start;
                            break;
                        case e_SequenceStep.과산화수소_교반기_Start:
                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    if (commResult[i] == false)
                                    {
                                        commResult[i] = _CBLDC.정방향속도((CBLDC.e_Channel)i, GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소);
                                    }
                                }
                            }

                            #region check

                            check = true;

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    check &= commResult[i];
                                }
                            }

                            if (check)
                            {
                                _Curr_SequenceStep = e_SequenceStep.과산화수소_1차_Filling_Setup;
                            }

                            #endregion
                            break;
                        case e_SequenceStep.과산화수소_1차_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.과산화수소_1차_vol, GlobalVariables.Parameter.Sequence.과산화수소_1차_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.과산화수소_1차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.과산화수소_1차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.과산화수소_1차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.과산화수소_1차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.과산화수소_1차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.과산화수소_1차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.과산화수소_1차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.과산화수소_2차_Filling_Setup;
                            }
                            break;
                        case e_SequenceStep.과산화수소_2차_Filling_Setup:
                            Array.Clear(_CNextPump.Channels, 0, _CNextPump.Channels.Length);

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    switch ((e_Channel)i)
                                    {
                                        case e_Channel.CH_01: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_06] = true; break;
                                        case e_Channel.CH_02: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_07] = true; break;
                                        case e_Channel.CH_03: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_08] = true; break;
                                        case e_Channel.CH_04: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_09] = true; break;
                                        case e_Channel.CH_05: _CNextPump.Channels[(int)CNextPump.e_Channel.ID_10] = true; break;
                                    }
                                }
                            }

                            if (_CNextPump.FillingSetup(GlobalVariables.Parameter.Sequence.과산화수소_2차_vol, GlobalVariables.Parameter.Sequence.과산화수소_2차_sec))
                            {
                                _Curr_SequenceStep = e_SequenceStep.과산화수소_2차_Filling_Start;
                            }
                            break;
                        case e_SequenceStep.과산화수소_2차_Filling_Start:
                            if (_CNextPump.FillingStart())
                            {
                                _SequenceMinute = GlobalVariables.Parameter.Sequence.test ? SetMinute(GlobalVariables.Parameter.Sequence.test_min) : SetMinute(GlobalVariables.Parameter.Sequence.과산화수소_2차_min); _SequenceStopwatch.Restart();

                                _Curr_SequenceStep = e_SequenceStep.과산화수소_2차_Filling_Wait;
                            }
                            break;
                        case e_SequenceStep.과산화수소_2차_Filling_Wait:
                            if (_SequenceStopwatch.ElapsedMilliseconds >= _SequenceMinute)
                            {
                                _SequenceStopwatch.Stop();

                                _Curr_SequenceStep = e_SequenceStep.과산화수소_2차_Filling_Stop;
                            }
                            break;
                        case e_SequenceStep.과산화수소_2차_Filling_Stop:
                            if (_CNextPump.FillingStop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.수조_Stop;

                                #region 2023-12-14: 공정(과산화수소) 완료 후 서보 하강 step 추가

                                _Curr_SequenceStep = e_SequenceStep.Servo_Down;

                                #endregion
                            }
                            break;
                        case e_SequenceStep.Servo_Down:
                            servoUpDownTasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                if (_SelectedChannel[i])
                                {
                                    servoUpDownTasks.Add(ServoUpDown((e_Channel)i, false));
                                }
                            }

                            await Task.WhenAll(servoUpDownTasks);

                            _Curr_SequenceStep = e_SequenceStep.수조_Stop;
                            break;
                        case e_SequenceStep.수조_Stop:
                            if (_CWaterTank.Stop())
                            {
                                _Curr_SequenceStep = e_SequenceStep.완료;
                            }
                            break;
                        case e_SequenceStep.완료:
                            GlobalFunctions.MessageBox(call, GlobalVariables.Message.I_공정완료);

                            Array.Clear(_SelectedChannel, 0, _SelectedChannel.Length);

                            _SequenceMinute = 0;
                            _SequenceStopwatch.Reset();

                            Array.Clear(_DateTime, 0, _DateTime.Length);

                            _Curr_SequenceStep = e_SequenceStep.대기;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(call, GlobalFunctions.GetExceptionString(ex));
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        private int SetSecond(int second)
        {
            return 1000 * second;
        }

        private int SetMinute(int minute)
        {
            return 1000 * 60 * minute;
        }

        /// <summary>
        /// 반응로 온도 확인
        /// </summary>
        private bool CheckWaterTankTemp(double temp, double range = 0.5)
        {
            return _WaterTank_Curr >= temp - range && _WaterTank_Curr < temp + range && _WaterTank_Curr > 0;
        }

        /// <summary>
        /// 반응조 온도 확인
        /// </summary>
        private bool CheckTemperature(e_Channel channel, double temp)
        {
            return _Temperature[(int)channel] < temp && _Temperature[(int)channel] > 0;
        }

        private bool CheckSequence(bool test, ref string NG)
        {
            bool result = true;
            try
            {
                if (test)
                {
                    return true;
                }

                #region 채널 선택 확인

                if (GlobalVariables.Parameter.CheckSequence.채널선택확인)
                {
                    if (result)
                    {
                        result &= Array.FindAll(_SelectedChannel, _ => _ == true).Length > 0;

                        if (result == false)
                        {
                            NG = $"채널 선택 확인";
                        }
                    }
                }

                #endregion

                #region 실린더 Down 확인

                if (GlobalVariables.Parameter.CheckSequence.헤드하강확인)
                {
                    if (result)
                    {
                        for (int i = 0; i < _SelectedChannel.Length; i++)
                        {
                            if (_SelectedChannel[i])
                            {
                                switch ((e_Channel)i)
                                {
                                    case e_Channel.CH_01: result &= _DI[(int)e_DI.Input03_CH1_실린더_Down]; break;
                                    case e_Channel.CH_02: result &= _DI[(int)e_DI.Input08_CH2_실린더_Down]; break;
                                    case e_Channel.CH_03: result &= _DI[(int)e_DI.Input13_CH3_실린더_Down]; break;
                                    case e_Channel.CH_04: result &= _DI[(int)e_DI.Input19_CH4_실린더_Down]; break;
                                    case e_Channel.CH_05: result &= _DI[(int)e_DI.Input24_CH5_실린더_Down]; break;
                                }
                            }
                        }

                        if (result == false)
                        {
                            NG = $"실린더 Down 확인";
                        }
                    }
                }

                #endregion

                #region 반응조 센서 확인

                if (GlobalVariables.Parameter.CheckSequence.반응조센서확인)
                {
                    if (result)
                    {
                        for (int i = 0; i < _SelectedChannel.Length; i++)
                        {
                            if (_SelectedChannel[i])
                            {
                                switch ((e_Channel)i)
                                {
                                    case e_Channel.CH_01: result &= _DI[(int)e_DI.Input26_CH1_반응조_센서]; break;
                                    case e_Channel.CH_02: result &= _DI[(int)e_DI.Input27_CH2_반응조_센서]; break;
                                    case e_Channel.CH_03: result &= _DI[(int)e_DI.Input28_CH3_반응조_센서]; break;
                                    case e_Channel.CH_04: result &= _DI[(int)e_DI.Input29_CH4_반응조_센서]; break;
                                    case e_Channel.CH_05: result &= _DI[(int)e_DI.Input30_CH5_반응조_센서]; break;
                                }
                            }
                        }

                        if (result == false)
                        {
                            NG = $"반응조 센서 확인";
                        }
                    }
                }

                #endregion

                #region 반응조 온도 확인

                if (GlobalVariables.Parameter.CheckSequence.반응조온도확인)
                {
                    if (result)
                    {
                        for (int i = 0; i < _SelectedChannel.Length; i++)
                        {
                            if (_SelectedChannel[i])
                            {
                                result &= CheckTemperature((e_Channel)i, GlobalVariables.Parameter.CheckSequence.반응조온도);
                            }
                        }

                        if (result == false)
                        {
                            NG = $"반응조 온도 확인";
                        }
                    }
                }

                #endregion

                #region 반응로 온도 확인

                if (GlobalVariables.Parameter.CheckSequence.반응로온도확인)
                {
                    if (result)
                    {
                        result &= CheckWaterTankTemp(GlobalVariables.Parameter.CheckSequence.반응로온도);

                        if (result == false)
                        {
                            NG = $"반응로 온도 확인";
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private bool CheckInterlock_반응로승온또는가열()
        {
            bool result = false;
            try
            {
                for (int i = 0; i < _SelectedChannel.Length; i++)
                {
                    if (_SelectedChannel[i])
                    {
                        int diff = Math.Abs(Convert.ToInt32(Math.Truncate(_WaterTank_Curr - _Temperature[i])));

                        result = diff >= GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열;
                    }

                    if (result)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private bool CheckInterlock_부반응물제거()
        {
            bool result = false;
            try
            {
                for (int i = 0; i < _SelectedChannel.Length; i++)
                {
                    if (_SelectedChannel[i])
                    {
                        result = _Temperature[i] >= GlobalVariables.Parameter.CheckInterlock.부반응물제거;
                    }

                    if (result)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            return result;
        }

        private void SequenceLog(string text, string file = "Sequence")
        {
            Log.Write(string.Empty, text, file);
        }

        private bool CheckCylinderSensor()
        {
            bool result = _DI[(int)e_DI.Input04_CH1_실린더_Up] & 
                          _DI[(int)e_DI.Input09_CH2_실린더_Up] & 
                          _DI[(int)e_DI.Input14_CH3_실린더_Up] & 
                          _DI[(int)e_DI.Input20_CH4_실린더_Up] & 
                          _DI[(int)e_DI.Input25_CH5_실린더_Up];
            return !result;
        }

        private System.Threading.Thread _ThreadSequenceSub = null;
        private bool _isThreadSequenceSub = false;
        private async void Process_SequenceSub()
        {
            string call = "Process_SequenceSub";

            #region local

            List<Task<bool>> tasks = new List<Task<bool>>(Enum.GetNames(typeof(e_Channel)).Length);

            #endregion

            while (_isThreadSequenceSub)
            {
                try
                {
                    if (_Curr_SequenceStep > e_SequenceStep.실린더_Up)
                    {
                        if (CheckCylinderSensor())
                        {
                            Log.Write(call, "※ SequenceSub - 실린더 하강으로 인한 실린더 제어");

                            tasks.Clear();

                            for (int i = 0; i < _SelectedChannel.Length; i++)
                            {
                                //if (_SelectedChannel[i])
                                {
                                    tasks.Add(실린더제어((e_Channel)i, true));
                                }
                            }

                            await Task.WhenAll(tasks);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(call, GlobalFunctions.GetExceptionString(ex));
                }

                System.Threading.Thread.Sleep(1000 * 60);
            }
        }
    }
}
