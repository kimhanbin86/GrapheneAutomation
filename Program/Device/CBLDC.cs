using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Library;
using Library.SerialPorts;

namespace Program
{
    internal class CBLDC : RS232
    {
        // e_Channel/_SlaveAddressTable 확인 및 수정 필요
        public enum e_Channel
        {
            CH01,
            CH02,
            CH03,
            CH04,
            CH05,
        }
        private readonly byte[] _SlaveAddressTable = new byte[] { 0x06, 0x07, 0x08, 0x09, 0x0A };

        #region enum

        public enum e_Control
        {
            드라이버 = 0x00,
            브레이크 = 0x01,
            알람리셋 = 0x02,
        }

        public enum e_Command
        {
            CLR = 0x00,
            SET = 0x01,
        }

        #endregion

        #region Thread

        private bool _isEntry = false;

        private System.Threading.Thread _ThreadInternal = null;
        private bool _isThreadInternal = false;
        private void Process_Internal()
        {
            int sleep = 100;
            int rpm = 0;
            t_MotorStatus motorStatus = new t_MotorStatus();
            bool comm = true;

            System.Threading.Thread.Sleep(1000);

            while (_isThreadInternal)
            {
                try
                {
                    if (_SerialPort != null)
                    {
                        if (IsOpen)
                        {
                            if (_isThreadInternal)
                            {
                                if (comm)
                                {
                                    for (int i = 0; i < MotorRPM.Length; i++)
                                    {
                                        if (comm &= GetMotorRPM((e_Channel)i, ref rpm))
                                        {
                                            MotorRPM[i] = rpm;
                                        }
                                    }
                                }
                            }

                            if (_isThreadInternal)
                            {
                                if (comm)
                                {
                                    for (int i = 0; i < MotorRPM.Length; i++)
                                    {
                                        if (comm &= GetMotorStatus((e_Channel)i, ref motorStatus))
                                        {
                                            MotorStatus[i] = motorStatus;
                                        }
                                    }
                                }
                            }
                        }

                        if (_isThreadInternal)
                        {
                            if (IsOpen == false || comm == false)
                            {
                                _isEntry = true;

                                LogWrite(MethodBase.GetCurrentMethod().Name, $"{(IsOpen == false ? "SerialPort" : "통신")} 이상, {Device} 재연결.");

                                base.Close();
                                System.Threading.Thread.Sleep(500);

                                if (_isThreadInternal)
                                {
                                    comm = base.Open();
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

        #region 필드

        private readonly object _LockObject = new object();

        private System.Diagnostics.Stopwatch _StopwatchTimeout = new System.Diagnostics.Stopwatch();

        private int _Timeout = 1000;

        #endregion

        #region 속성

        public int Timeout
        {
            set
            {
                _Timeout = value;
            }
        }

        public int[] MotorRPM = new int[Enum.GetNames(typeof(e_Channel)).Length];

        public struct t_MotorStatus
        {
            public bool BRK;
            public bool FRE;
            public bool ALM;
            public bool EMG;
            public bool DEC;
            public bool ACC;
            public bool DIR;
            public bool RUN;
        }
        public t_MotorStatus[] MotorStatus = new t_MotorStatus[Enum.GetNames(typeof(e_Channel)).Length];

        #endregion

        #region 메서드

        private bool GetMotorRPM(e_Channel channel, ref int rpm)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[8];
                    bytes[0] = _SlaveAddressTable[(int)channel];
                    bytes[1] = 0x04;
                    bytes[2] = 0x00;
                    bytes[3] = 0x03;
                    bytes[4] = 0x00;
                    bytes[5] = 0x01;
                    ushort CRC16 = GetCRC16(bytes, 6);
                    bytes[6] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                    bytes[7] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                    #endregion

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 7)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    CRC16 = GetCRC16(ack, ack.Length - 2);

                                    if (ack[0] == bytes[0] &&
                                        ack[1] == bytes[1] &&
                                        ack[2] == 0x02 &&
                                        ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                        ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                       )
                                    {
                                        rpm = ack[3] * 256 + ack[4];

                                        rpm /= 5; // TODO: 감속비 적용. 추후에 속성으로 추가할 것

                                        result = true;
                                    }
                                }

                                break;
                            }

                            System.Threading.Thread.Sleep(10);
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

        private bool GetMotorStatus(e_Channel channel, ref t_MotorStatus motorStatus)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[8];
                    bytes[0] = _SlaveAddressTable[(int)channel];
                    bytes[1] = 0x04;
                    bytes[2] = 0x00;
                    bytes[3] = 0x00;
                    bytes[4] = 0x00;
                    bytes[5] = 0x01;
                    ushort CRC16 = GetCRC16(bytes, 6);
                    bytes[6] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                    bytes[7] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                    #endregion

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 7)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    CRC16 = GetCRC16(ack, ack.Length - 2);

                                    if (ack[0] == bytes[0] &&
                                        ack[1] == bytes[1] &&
                                        ack[2] == 0x02 &&
                                        ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                        ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                       )
                                    {
                                        motorStatus.BRK = ((ack[4] >> 0) & 0x01) == 0x01;
                                        motorStatus.FRE = ((ack[4] >> 1) & 0x01) == 0x01;
                                        motorStatus.ALM = ((ack[4] >> 2) & 0x01) == 0x01;
                                        motorStatus.EMG = ((ack[4] >> 3) & 0x01) == 0x01;
                                        motorStatus.DEC = ((ack[4] >> 4) & 0x01) == 0x01;
                                        motorStatus.ACC = ((ack[4] >> 5) & 0x01) == 0x01;
                                        motorStatus.DIR = ((ack[4] >> 6) & 0x01) == 0x01;
                                        motorStatus.RUN = ((ack[4] >> 7) & 0x01) == 0x01;

                                        result = true;
                                    }
                                }

                                break;
                            }

                            System.Threading.Thread.Sleep(10);
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

        public bool 모터제어(e_Channel channel, e_Control control, e_Command command = e_Command.SET)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[8];
                    bytes[0] = _SlaveAddressTable[(int)channel];
                    bytes[1] = 0x06;
                    bytes[2] = 0x00;
                    bytes[3] = 0x78;
                    bytes[4] = (byte)control;
                    bytes[5] = (byte)command;
                    ushort CRC16 = GetCRC16(bytes, 6);
                    bytes[6] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                    bytes[7] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                    #endregion

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 8)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                                {
                                    CRC16 = GetCRC16(ack, ack.Length - 2);

                                    if (ack[0] == bytes[0] &&
                                        ack[1] == bytes[1] &&
                                        ack[2] == bytes[2] &&
                                        ack[3] == bytes[3] &&
                                        ack[4] == bytes[4] &&
                                        ack[5] == bytes[5] &&
                                        ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                        ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                       )
                                    {
                                        result = true;
                                    }
                                }

                                break;
                            }

                            System.Threading.Thread.Sleep(10);
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

        public bool 정방향속도(e_Channel channel, int rpm = 0)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    rpm *= 5; // TODO: 감속비 적용. 추후에 속성으로 추가할 것

                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[8];
                    bytes[0] = _SlaveAddressTable[(int)channel];
                    bytes[1] = 0x06;
                    bytes[2] = 0x00;
                    bytes[3] = 0x79;
                    bytes[4] = (byte)(rpm / 256);
                    bytes[5] = (byte)(rpm % 256);
                    ushort CRC16 = GetCRC16(bytes, 6);
                    bytes[6] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                    bytes[7] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                    #endregion

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 8)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                                {
                                    CRC16 = GetCRC16(ack, ack.Length - 2);

                                    if (ack[0] == bytes[0] &&
                                        ack[1] == bytes[1] &&
                                        ack[2] == bytes[2] &&
                                        ack[3] == bytes[3] &&
                                        ack[4] == bytes[4] &&
                                        ack[5] == bytes[5] &&
                                        ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                        ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                       )
                                    {
                                        result = true;
                                    }
                                }

                                break;
                            }

                            System.Threading.Thread.Sleep(10);
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

        public CBLDC()
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

        #region CRC16

        private readonly ushort[] CRC16Table = new ushort[]
        {
            0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
            0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
            0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
            0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
            0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
            0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
            0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
            0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
            0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
            0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
            0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
            0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
            0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
            0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
            0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
            0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
            0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
            0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
            0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
            0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
            0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
            0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
            0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
            0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
            0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
            0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
            0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
            0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
        };

        private ushort GetCRC16(byte[] bytes, int length)
        {
            ushort CRC = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                ushort temp = (ushort)(CRC ^ bytes[i]);
                CRC = (ushort)((CRC >> 8) ^ CRC16Table[temp & 0xFF]);
            }
            return CRC;
        }

        #endregion
    }
}
