﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Library;
using Library.SerialPorts;

namespace Program
{
    internal class CTemperature : RS232
    {
        // e_Channel 확인 및 수정 필요
        public enum e_Channel
        {
            CH01,
            CH02,
            CH03,
            CH04,
            CH05,
        }
        public double[] Temperature = new double[Enum.GetNames(typeof(e_Channel)).Length];

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
                                    comm = GetTemperature();
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

        private int _SlaveAddress = 1;

        #endregion

        #region 속성

        public int Timeout
        {
            set
            {
                _Timeout = value;
            }
        }

        public int SlaveAddress
        {
            set
            {
                _SlaveAddress = value;
            }
        }

        #endregion

        #region 메서드

        private bool GetTemperature(int slaveAddress, ushort startingAddress, ref double temperature)
        {
            bool result = false;
            try
            {
                #region Modbus RTU 프로토콜

                byte[] bytes = new byte[8];
                bytes[0] = (byte)slaveAddress;
                bytes[1] = 0x04;                          // Function
                bytes[2] = (byte)(startingAddress / 256); // Starting Address Hi
                bytes[3] = (byte)(startingAddress % 256); // Starting Address Lo
                bytes[4] = 0x00;                          // No. of Points Hi
                bytes[5] = 0x02;                          // No. of Points Lo
                ushort CRC16 = GetCRC16(bytes, 6);
                bytes[6] = (byte)(CRC16 % 256);           // Error Check(CRC16) Lo
                bytes[7] = (byte)(CRC16 / 256);           // Error Check(CRC16) Hi

                #endregion

                System.Threading.Thread.Sleep(50);

                bool log = false;

                if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                {
                    byte[] ack = null;

                    int count = bytes[4] * 256 + bytes[5];

                    _StopwatchTimeout.Restart();

                    while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                    {
                        if (BytesToRead == 5 + count * 2)
                        {
                            if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                            {
                                CRC16 = GetCRC16(ack, ack.Length - 2);

                                if (ack[0] == bytes[0] &&                         // Slave Address
                                    ack[1] == bytes[1] &&                         // Function
                                    ack[2] == count * 2 &&                        // Byte Count
                                    ack[ack.Length - 2] == (byte)(CRC16 % 256) && // Error Check(CRC16) Lo
                                    ack[ack.Length - 1] == (byte)(CRC16 / 256)    // Error Check(CRC16) Hi
                                   )
                                {
                                    int presentValue = ack[3] * 256 + ack[4];
                                    int dot = ack[5] * 256 + ack[6];

                                    temperature = dot == 0 ? presentValue : (double)presentValue / 10;

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

        // Slave 당 4채널 존재
        private readonly ushort[] TemperatureStartingAddressTable = new ushort[] { 0x03E8, 0x03EE, 0x03F4, 0x03FA };
        private bool GetTemperature()
        {
            lock (_LockObject)
            {
                bool result = true;
                try
                {
                    int slaveAddress = _SlaveAddress;
                    int idxStartingAddress = 0;
                    double temperature = 0;

                    for (int i = 0; i < Temperature.Length; i++)
                    {
                        if (i > 0)
                        {
                            if (i % 4 == 0)
                            {
                                slaveAddress++;
                                idxStartingAddress = 0;
                            }
                        }

                        if (result &= GetTemperature(slaveAddress, TemperatureStartingAddressTable[idxStartingAddress++], ref temperature))
                        {
                            Temperature[i] = temperature;
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

        public CTemperature()
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
