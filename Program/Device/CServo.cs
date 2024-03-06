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
    internal class CServo : SocketClient
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
            MoveOrigin = 0x33,
            MoveAbsPos = 0x34,
            GetStatus = 0x40,
            GetActualPos = 0x53,
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
            int actualPos = 0;
            bool comm = true;

            while (_isThreadInternal && GlobalVariables.Form.Main.StartupServoComp[m_ChannelIndex] == false)
            {
                System.Threading.Thread.Sleep(sleep);
            }

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

                                        if (m_isGetStatus == false)
                                        {
                                            m_isGetStatus = true;
                                        }
                                    }
                                }
                            }

                            if (m_isGetActualPos)
                            {
                                if (_isThreadInternal)
                                {
                                    if (comm)
                                    {
                                        if (comm = GetActualPos(ref actualPos))
                                        {
                                            _ActualPos = actualPos;
                                        }
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
            public bool ERRORALL;        // 여러 에러 중 하나 이상의 에러가 발생한 경우                                       (0X00000001)
            public bool HWPOSILMT;       // +방향 Limit 센서가 ON 이 된 경우                                                  (0X00000002)
            public bool HWNEGALMT;       // -방향 Limit 센서가 ON 이 된 경우                                                  (0X00000004)
            public bool SWPOGILMT;       // +방향 프로그램 Limit 를 초과한 경우                                               (0X00000008)
            public bool SWNEGALMT;       // -방향 프로그램 Limit 를 초과한 경우                                               (0X00000010)
            public bool ERRPOSOVERFLOW;  // 위치 명령 완료후 위치 오차가 ‘Pos Error Overflow Limit’ 값보다 크게 발생한 경우 (0X00000080)
            public bool ERRPOSTRACKING;  // 위치 명령 중 위치 오차가 ‘Pos Tracking Limit’값보다 크게 발생한 경우            (0X00000400)
            public bool ERRINPOSITION;   // Inposition 이상일 경우 Alarm 발생                                                 (0X00008000)
            public bool ORIGINRETURNING; // 원점 복귀 운전 중 일 경우                                                         (0X00040000)
            public bool INPOSITION;      // Inposition 이 완료된 상태일 경우                                                  (0X00080000)
            public bool SERVOON;         // 모터가 Servo ON 상태일 경우                                                       (0X00100000)
            public bool ORIGINSENSOR;    // 원점 센서가 ON 되어 있는 상태일 경우                                              (0X00800000)
            public bool ORIGINRETOK;     // 원점 복귀 운전이 완료 된 상황일 경우                                              (0X02000000)
            public bool MOTIONING;       // 모터가 현재 운전중일 경우                                                         (0X08000000)
            public bool MOTIONACCEL;     // 가속 구간의 운전중일 경우                                                         (0X20000000)
            public bool MOTIONDECEL;     // 감속 구간의 운전중일 경우                                                         (0X40000000)
            public bool MOTIONCONST;     // 가/감속 구간이 아닌 정속도 운전중인 상태일 경우                                   (0X80000000)
        }
        private t_Status _Status = new t_Status();

        private int _ActualPos = 0;

        private int m_ChannelIndex = 0;
        public int ChannelIndex
        {
            set
            {
                m_ChannelIndex = value;
            }
        }

        private bool m_isGetActualPos = false;

        private bool m_isGetStatus = false;
        public bool IsGetStatus
        {
            get
            {
                return m_isGetStatus;
            }
        }

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

        public int ActualPos
        {
            get
            {
                return _ActualPos;
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

                if (m_isGetActualPos == false)
                {
                    switch (frameType)
                    {
                        case e_FrameType.MoveOrigin:
                        case e_FrameType.MoveAbsPos:
                            m_isGetActualPos = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private byte[] posStringToBytes(string pos)
        {
            byte[] result = new byte[4];
            try
            {
                if (int.TryParse(pos, out int integer))
                {
                    integer = integer * (_PPR / 2); // 반바퀴가 1mm

                    result = BitConverter.GetBytes(integer);
                }
                else
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, $"정수 변환 실패 (pos=[{pos}])");
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
                                    case 03: alarm = "PosTracking"; break;
                                    case 04: alarm = "OverLoad"; break;
                                    case 05: alarm = "OverTemperature"; break;
                                    case 06: alarm = "BackEMF"; break;
                                    case 07: alarm = "MotorConnect"; break;
                                    case 08: alarm = "EncoderConnect"; break;
                                    case 10: alarm = "Inposition"; break;
                                    case 12: alarm = "ROMdevice"; break;
                                    case 15: alarm = "Position Overflow"; break;
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

        public bool MoveOrigin()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.MoveOrigin);
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

        public bool MoveAbsPos(string pos, string rpm)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.MoveAbsPos);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame = frame.Concat(posStringToBytes(pos)).ToArray();
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
                                status.HWPOSILMT = ((data >> 01) & 0x01) == 0x01;
                                status.HWNEGALMT = ((data >> 02) & 0x01) == 0x01;
                                status.SWPOGILMT = ((data >> 03) & 0x01) == 0x01;
                                status.SWNEGALMT = ((data >> 04) & 0x01) == 0x01;
                                status.ERRPOSOVERFLOW = ((data >> 07) & 0x01) == 0x01;
                                status.ERRPOSTRACKING = ((data >> 10) & 0x01) == 0x01;
                                status.ERRINPOSITION = ((data >> 15) & 0x01) == 0x01;
                                status.ORIGINRETURNING = ((data >> 18) & 0x01) == 0x01;
                                status.INPOSITION = ((data >> 19) & 0x01) == 0x01;
                                status.SERVOON = ((data >> 20) & 0x01) == 0x01;
                                status.ORIGINSENSOR = ((data >> 23) & 0x01) == 0x01;
                                status.ORIGINRETOK = ((data >> 25) & 0x01) == 0x01;
                                status.MOTIONING = ((data >> 27) & 0x01) == 0x01;
                                status.MOTIONACCEL = ((data >> 29) & 0x01) == 0x01;
                                status.MOTIONDECEL = ((data >> 30) & 0x01) == 0x01;
                                status.MOTIONCONST = ((data >> 31) & 0x01) == 0x01;

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

        private bool GetActualPos(ref int actualPos)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.GetActualPos);
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
                                actualPos = BitConverter.ToInt32(bytes, 0) / (_PPR / 2); // 반바퀴가 1mm

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

        public CServo()
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
