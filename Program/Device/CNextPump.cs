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
    internal class CNextPump : RS232
    {
        #region enum

        public enum e_Channel
        {
            ID_01,
            ID_02,
            ID_03,
            ID_04,
            ID_05,
            ID_06,
            ID_07,
            ID_08,
            ID_09,
            ID_10,
            ID_11,
            ID_12,
            ID_13,
            ID_14,
            ID_15,
            ID_16,
            ID_17,
            ID_18,
            ID_19,
            ID_20,
            ID_21,
            ID_22,
            ID_23,
            ID_24,
            ID_25,
            ID_26,
            ID_27,
            ID_28,
            ID_29,
            ID_30,
            ID_31,
            ID_32,
        }

        private enum e_Filling
        {
            Volume,
            Time,
        }

        public enum e_Status
        {
            Filling,
            Recycle,
        }

        #endregion

        #region Thread

        private bool _isEntry = false;

        private System.Threading.Thread _ThreadInternal = null;
        private bool _isThreadInternal = false;
        private void Process_Internal()
        {
            int sleep = 100;
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
                                    comm = ReadStatus(e_Status.Filling);
                                }
                            }

                            if (_isThreadInternal)
                            {
                                if (comm)
                                {
                                    comm = ReadStatus(e_Status.Recycle);
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

        public bool[] Channels = new bool[Enum.GetNames(typeof(e_Channel)).Length];

        public bool[] Status = new bool[Enum.GetNames(typeof(e_Status)).Length];

        private int _FillingSetupInterval = 500;

        #endregion

        #region 속성

        public int Timeout
        {
            set
            {
                _Timeout = value;
            }
        }

        public int FillingSetupInterval
        {
            get
            {
                return _FillingSetupInterval;
            }
            set
            {
                _FillingSetupInterval = value;
            }
        }

        #endregion

        #region 메서드

        private bool SelectChannel(e_Channel channel)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[8];
                bytes[0] = 0x01;
                bytes[1] = 0x06;
                bytes[2] = 0x00;
                bytes[3] = 0x01;
                bytes[4] = 0x00;
                bytes[5] = (byte)((int)channel + 1);
                ushort CRC16 = GetCRC16(bytes, 6);
                bytes[6] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                bytes[7] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                if (bytes.SequenceEqual(ack))
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

        private double ComputeFillingValue(string value, e_Filling filling)
        {
            double result = 0;
            try
            {
                result = double.Parse(value);

                switch (filling)
                {
                    case e_Filling.Volume: result *= 100; break;
                    case e_Filling.Time: result *= 10; break;
                }
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private byte[] ConvertFillingValue(string value, e_Filling filling)
        {
            byte[] result = new byte[4];
            try
            {
                double compute = ComputeFillingValue(value, filling);
                uint convert = Convert.ToUInt32(compute);
                result = BitConverter.GetBytes(convert);
                Array.Reverse(result);
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private bool SetFillingVolume(string ml)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[13];
                bytes[00] = 0x01;
                bytes[01] = 0x10;
                bytes[02] = 0x00;
                bytes[03] = 0x04;
                bytes[04] = 0x00;
                bytes[05] = 0x02;
                bytes[06] = 0x04;
                byte[] data = ConvertFillingValue(ml, e_Filling.Volume);
                bytes[07] = data[0];
                bytes[08] = data[1];
                bytes[09] = data[2];
                bytes[10] = data[3];
                ushort CRC16 = GetCRC16(bytes, 11);
                bytes[11] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                bytes[12] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                if (ack[0] == 0x01 &&
                                    ack[1] == 0x10 &&
                                    ack[2] == 0x00 &&
                                    ack[3] == 0x04 &&
                                    ack[4] == 0x00 &&
                                    ack[5] == 0x02 &&
                                    ack[6] == 0x00 &&
                                    ack[7] == 0x09
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

        private bool SendFillingVolume(string ml)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[] { 0x01, 0x03, 0x00, 0x04, 0x00, 0x02, 0x85, 0xCA };

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

                if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                {
                    byte[] ack = null;

                    _StopwatchTimeout.Restart();

                    while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                    {
                        if (BytesToRead == 9)
                        {
                            if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                            {
                                byte[] data = ConvertFillingValue(ml, e_Filling.Volume);

                                ushort CRC16 = GetCRC16(ack, ack.Length - 2);

                                if (ack[0] == 0x01 &&
                                    ack[1] == 0x03 &&
                                    ack[2] == 0x04 &&
                                    ack[3] == data[0] &&
                                    ack[4] == data[1] &&
                                    ack[5] == data[2] &&
                                    ack[6] == data[3] &&
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

        private bool SetFillingTime(string sec)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[13];
                bytes[00] = 0x01;
                bytes[01] = 0x10;
                bytes[02] = 0x00;
                bytes[03] = 0x06;
                bytes[04] = 0x00;
                bytes[05] = 0x02;
                bytes[06] = 0x04;
                byte[] data = ConvertFillingValue(sec, e_Filling.Time);
                bytes[07] = data[0];
                bytes[08] = data[1];
                bytes[09] = data[2];
                bytes[10] = data[3];
                ushort CRC16 = GetCRC16(bytes, 11);
                bytes[11] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                bytes[12] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                if (ack[0] == 0x01 &&
                                    ack[1] == 0x10 &&
                                    ack[2] == 0x00 &&
                                    ack[3] == 0x06 &&
                                    ack[4] == 0x00 &&
                                    ack[5] == 0x02 &&
                                    ack[6] == 0xA1 &&
                                    ack[7] == 0xC9
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

        private bool SendFillingTime(string sec)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[] { 0x01, 0x03, 0x00, 0x06, 0x00, 0x02, 0x24, 0x0A };

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

                if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                {
                    byte[] ack = null;

                    _StopwatchTimeout.Restart();

                    while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                    {
                        if (BytesToRead == 9)
                        {
                            if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                            {
                                byte[] data = ConvertFillingValue(sec, e_Filling.Time);

                                ushort CRC16 = GetCRC16(ack, ack.Length - 2);

                                if (ack[0] == 0x01 &&
                                    ack[1] == 0x03 &&
                                    ack[2] == 0x04 &&
                                    ack[3] == data[0] &&
                                    ack[4] == data[1] &&
                                    ack[5] == data[2] &&
                                    ack[6] == data[3] &&
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

        public bool FillingSetup(string ml, string sec)
        {
            lock (_LockObject)
            {
                bool result = true;
                try
                {
                    int prevIdx = 0;
                    int currIdx = 0;
                    int failCnt = 0;

                    while (currIdx < 16)
                    {
                        prevIdx = currIdx;

                        if (Channels[currIdx])
                        {
                            if (SelectChannel((e_Channel)currIdx))
                            {
                                if (SetFillingVolume(ml))
                                {
                                    if (SendFillingVolume(ml))
                                    {
                                        if (SetFillingTime(sec))
                                        {
                                            if (SendFillingTime(sec))
                                            {
                                                currIdx++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            currIdx++;
                        }

                        if (prevIdx < currIdx)
                        {
                            failCnt = 0;
                        }
                        else
                        {
                            failCnt++;

                            if (failCnt >= 3)
                            {
                                result = false;

                                break;
                            }
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        private byte[] ConvertChannel()
        {
            byte[] result = new byte[4];
            try
            {
                int idx = 0;

                ushort compute = 0x0000;
                for (; idx < 16; idx++)
                {
                    if (Channels[idx])
                    {
                        compute += (ushort)Math.Pow(2, idx);
                    }
                }
                byte[] bytes1 = BitConverter.GetBytes(compute);
                Array.Reverse(bytes1);

                compute = 0x0000;
                for (; idx < 32; idx++)
                {
                    if (Channels[idx])
                    {
                        compute += (ushort)Math.Pow(2, idx - 16);
                    }
                }
                byte[] bytes2 = BitConverter.GetBytes(compute);
                Array.Reverse(bytes2);

                result[0] = bytes1[0];
                result[1] = bytes1[1];
                result[2] = bytes2[0];
                result[3] = bytes2[1];
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        private bool ChannelEnable()
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[13];
                bytes[00] = 0x01;
                bytes[01] = 0x10;
                bytes[02] = 0x00;
                bytes[03] = 0x0F;
                bytes[04] = 0x00;
                bytes[05] = 0x02;
                bytes[06] = 0x04;
                byte[] data = ConvertChannel();
                bytes[07] = data[0];
                bytes[08] = data[1];
                bytes[09] = data[2];
                bytes[10] = data[3];
                ushort CRC16 = GetCRC16(bytes, 11);
                bytes[11] = (byte)(CRC16 % 256); // Error Check(CRC16) Lo
                bytes[12] = (byte)(CRC16 / 256); // Error Check(CRC16) Hi

                #endregion

                System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                if (ack[0] == 0x01 &&
                                    ack[1] == 0x10 &&
                                    ack[2] == 0x00 &&
                                    ack[3] == 0x0F &&
                                    ack[4] == 0x00 &&
                                    ack[5] == 0x02 &&
                                    ack[6] == 0x71 &&
                                    ack[7] == 0xCB
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

        public bool FillingStart()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    if (ChannelEnable())
                    {
                        #region Modbus RTU 프로토콜

                        byte[] bytes = new byte[] { 0x01, 0x05, 0x00, 0x01, 0xFF, 0x00, 0xDD, 0xFA };

                        #endregion

                        System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                        if (bytes.SequenceEqual(ack))
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
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool FillingStop()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[] { 0x01, 0x05, 0x00, 0x01, 0x00, 0x00, 0x9C, 0x0A };

                    #endregion

                    System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                    if (bytes.SequenceEqual(ack))
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

        public bool RecycleStart()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    if (ChannelEnable())
                    {
                        #region Modbus RTU 프로토콜

                        byte[] bytes = new byte[] { 0x01, 0x05, 0x00, 0x03, 0xFF, 0x00, 0x7C, 0x3A };

                        #endregion

                        System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                        if (bytes.SequenceEqual(ack))
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
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool RecycleStop()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = new byte[] { 0x01, 0x05, 0x00, 0x03, 0x00, 0x00, 0x3D, 0xCA };

                    #endregion

                    System.Threading.Thread.Sleep(_FillingSetupInterval);

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
                                    if (bytes.SequenceEqual(ack))
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

        private bool ReadStatus(e_Status status)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    #region Modbus RTU 프로토콜

                    byte[] bytes = null;

                    switch (status)
                    {
                        case e_Status.Filling: bytes = new byte[] { 0x01, 0x01, 0x00, 0x01, 0x00, 0x01, 0xAC, 0x0A }; break;
                        case e_Status.Recycle: bytes = new byte[] { 0x01, 0x01, 0x00, 0x03, 0x00, 0x01, 0x0D, 0xCA }; break;
                    }

                    #endregion

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 6)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    ushort CRC16 = GetCRC16(ack, ack.Length - 2);

                                    if (ack[0] == 0x01 &&
                                        ack[1] == 0x01 &&
                                        ack[2] == 0x01 &&
                                        ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                        ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                       )
                                    {
                                        if (ack[3] == 0x01 ||
                                            ack[3] == 0x00
                                           )
                                        {
                                            result = true;

                                            Status[(int)status] = ack[3] == 0x01;
                                        }
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

        public CNextPump()
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
