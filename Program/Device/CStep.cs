using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Library;
using Library.Sockets;

namespace Program
{
    internal class CStep : SocketClient
    {
        #region enum

        private enum e_Frame
        {
            Header,
            Length,
            SyncNo,
            Reserved,
            FrameType,
        }

        private enum e_FrameType
        {
            ServoEnable = 0x2A,
            ServoAlarmReset = 0x2B,
            GetAlarmType = 0x2E,
            MoveStop = 0x31,
            MoveIncPos = 0x35,
            GetStatus = 0x40,
            ClearPosition = 0x56,
        }

        #endregion

        #region Thread

        private bool _isEntry = false;

        private System.Threading.Thread _ThreadInternal = null;
        private bool _isThreadInternal = false;
        private void Process_Internal()
        {
            int sleep = 100;
            t_Status status = new t_Status();
            bool comm = true;

            System.Threading.Thread.Sleep(1000);

            while (_isThreadInternal)
            {
                try
                {
                    if (_Client != null)
                    {
                        if (IsConnected)
                        {
                            if (_isThreadInternal)
                            {
                                if (comm)
                                {
                                    if (comm = GetStatus(ref status))
                                    {
                                        _Status = status;
                                    }
                                }
                            }
                        }

                        if (_isThreadInternal)
                        {
                            if (IsConnected == false || comm == false)
                            {
                                _isEntry = true;

                                LogWrite(MethodBase.GetCurrentMethod().Name, $"{(IsConnected == false ? "Socket" : "통신")} 이상, {Device} 재연결.");

                                base.Disconnect();
                                System.Threading.Thread.Sleep(500);

                                if (_isThreadInternal)
                                {
                                    comm = base.Connect();
                                }

                                _isEntry = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }

                System.Threading.Thread.Sleep(sleep);
            }
        }

        #endregion

        #region 상수

        private const byte c_Header = 0xAA;

        #endregion

        #region 필드

        private readonly object _LockObject = new object();

        private int _PPR = 10000;

        public struct t_Status
        {
            public bool ERRORALL;  // 여러 에러 중 하나 이상의 에러가 발생한 경우 (0X00000001)
            public bool RUNSTOP;   // 모터가 구동중인 상태일 경우                 (0X00080000)
            public bool STEPON;    // 모터가 Step ON 상태일 경우                  (0X00100000)
            public bool MOTIONING; // 모터가 현재 운전중일 경우                   (0X08000000)
        }
        private t_Status _Status = new t_Status();

        #endregion

        #region 속성

        public int PPR
        {
            set
            {
                _PPR = value;
            }
        }

        public t_Status Status
        {
            get
            {
                return _Status;
            }
        }

        #endregion

        #region 메서드

        private byte GetSyncNo()
        {
            Random random = new Random();
            byte[] bytes = new byte[1];
            random.NextBytes(bytes);
            return bytes[0];
        }

        private byte[] MakeFrame(e_FrameType frameType)
        {
            byte[] result = new byte[Enum.GetNames(typeof(e_Frame)).Length];
            try
            {
                result[(int)e_Frame.Header] = c_Header;
                result[(int)e_Frame.FrameType] = (byte)frameType;

                #region // TODO: 2023-12-15

                // 파스텍 인원에게 듣기로 동시 명령 처리가 안되기 때문에 통신 사이에 적당한 딜레이가 필요함.
                // n 개의 명령을 처리하는 Servo, Step에만 적용하며,
                // 추후 SocketClient 클래스에 속성 추가 및 Send 적용할 것
                // e.g. System.Threading.Thread.Sleep(DelayBeforeSend);

                System.Threading.Thread.Sleep(50);

                #endregion
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private byte[] revStringToBytes(string rev)
        {
            byte[] result = new byte[4];
            try
            {
                if (int.TryParse(rev, out int integer))
                {
                    integer = integer * _PPR;

                    result = BitConverter.GetBytes(integer);
                }
                else
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, $"정수 변환 실패 (rev=[{rev}])");
                }
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private byte[] rpmStringToBytes(string rpm)
        {
            byte[] result = new byte[4];
            try
            {
                if (int.TryParse(rpm, out int integer))
                {
                    integer = integer * _PPR / 60;

                    result = BitConverter.GetBytes(integer);
                }
                else
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, $"정수 변환 실패 (rpm=[{rpm}])");
                }
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        public bool ServoEnable(bool data = true)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.ServoEnable);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    byte[] _ = new byte[1];
                    _[0] = (byte)(data ? 1 : 0);
                    frame = frame.Concat(_).ToArray();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool ServoAlarmReset()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.ServoAlarmReset);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool GetAlarmType(ref string alarm)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.GetAlarmType);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                alarm = string.Empty;

                                switch (ack[(int)e_Frame.FrameType + 2])
                                {
                                    case 00: break;
                                    case 01: alarm = "OverCurrent"; break;
                                    case 02: alarm = "OverSpeed"; break;
                                    case 03: alarm = "StepOut"; break;
                                    case 05: alarm = "OverTemperature"; break;
                                    case 06: alarm = "BackEMF"; break;
                                    case 07: alarm = "MotorConnect"; break;
                                    case 11: alarm = "SystemHalt"; break;
                                    case 12: alarm = "ROMdevice"; break;
                                    default: alarm = "Not define"; break;
                                }

                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool MoveStop()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.MoveStop);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool MoveIncPos(string rev, string rpm)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.MoveIncPos);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame = frame.Concat(revStringToBytes(rev)).ToArray();
                    frame = frame.Concat(rpmStringToBytes(rpm)).ToArray();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool GetStatus(ref t_Status status)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.GetStatus);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    bool log = false;

                    if (Send(frame, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name, log))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                byte[] bytes = new byte[] { ack[6], ack[7], ack[8], ack[9] };
                                uint data = BitConverter.ToUInt32(bytes, 0);

                                status.ERRORALL = ((data >> 00) & 0x01) == 0x01;
                                status.RUNSTOP = ((data >> 19) & 0x01) == 0x01;
                                status.STEPON = ((data >> 20) & 0x01) == 0x01;
                                status.MOTIONING = ((data >> 27) & 0x01) == 0x01;

                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool ClearPosition()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.ClearPosition);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame[(int)e_Frame.Length] = (byte)(frame.Length - 2);

                    if (Send(frame, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        if (Receive(ref ack, MethodBase.GetCurrentMethod().Name))
                        {
                            if (ack[(int)e_Frame.Header] == c_Header &&
                                ack[(int)e_Frame.SyncNo] == frame[(int)e_Frame.SyncNo] &&
                                ack[(int)e_Frame.FrameType] == frame[(int)e_Frame.FrameType] &&
                                ack[(int)e_Frame.FrameType + 1] == 0x00
                               )
                            {
                                result = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        #endregion

        #region 생성자, 종료자

        public CStep()
        {
            _ThreadInternal = new System.Threading.Thread(Process_Internal);
            _ThreadInternal.IsBackground = true;
            _isThreadInternal = true;
            _ThreadInternal.Start();
        }

        public override void Dispose()
        {
            _isThreadInternal = false;

            if (_isEntry)
            {
                System.Threading.Thread.Sleep(1000);
            }

            base.Dispose();
        }

        #endregion
    }
}
