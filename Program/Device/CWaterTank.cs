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
    internal class CWaterTank : RS232
    {
        #region Thread

        private bool _isEntry = false;

        private System.Threading.Thread _ThreadInternal = null;
        private bool _isThreadInternal = false;
        private void Process_Internal()
        {
            int sleep = 100;
            double temperature = 0;
            bool comm = true;

            System.Threading.Thread.Sleep(1000);

            bool toggle = false;

            while (_isThreadInternal)
            {
                try
                {
                    if (_SerialPort != null)
                    {
                        if (IsOpen)
                        {
                            toggle = !toggle;

                            if (toggle)
                            {
                                if (_isThreadInternal)
                                {
                                    if (comm)
                                    {
                                        if (comm = GetTargetTemperature(ref temperature))
                                        {
                                            _TargetTemperature = temperature;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (_isThreadInternal)
                                {
                                    if (comm)
                                    {
                                        if (comm = GetCurrentTemperature(ref temperature))
                                        {
                                            _CurrentTemperature = temperature;
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

        private double _TargetTemperature = 0;

        private double _CurrentTemperature = 0;

        private bool _isBusy = false;

        #endregion

        #region 속성

        public int Timeout
        {
            set
            {
                _Timeout = value;
            }
        }

        /// <summary>
        /// sv
        /// </summary>
        public double TargetTemperature
        {
            get
            {
                return _TargetTemperature;
            }
        }

        /// <summary>
        /// pv
        /// </summary>
        public double CurrentTemperature
        {
            get
            {
                return _CurrentTemperature;
            }
        }

        public bool isBusy
        {
            get
            {
                return _isBusy;
            }
        }

        #endregion

        #region 메서드

        private double HexToDouble(int data)
        {
            return ((double)data - 12288) / 100;
        }

        private byte[] TemperatureToBytes(double data)
        {
            byte[] result = new byte[2];
            double calc = 12288 + (data * 100);
            result[0] = (byte)(calc / 256);
            result[1] = (byte)(calc % 256);
            return result;
        }

        private bool GetTargetTemperature(ref double sv)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] bytes = new byte[] { 0x1b, 0x04, 0x00, 0x00, 0x00, 0x0a };

                    System.Threading.Thread.Sleep(100);

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 2)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    string hex = $"{ack[0]:X2}{ack[1]:X2}";
                                    int data = Convert.ToInt32(hex, 16);
                                    sv = HexToDouble(data);

                                    result = true;
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

        private bool GetCurrentTemperature(ref double pv)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] bytes = new byte[] { 0x1b, 0x04, 0x50, 0x00, 0x00, 0x0a };

                    System.Threading.Thread.Sleep(100);

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 2)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    string hex = $"{ack[0]:X2}{ack[1]:X2}";
                                    int data = Convert.ToInt32(hex, 16);
                                    pv = HexToDouble(data);

                                    result = true;
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

        private bool GetStatus()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] bytes = new byte[] { 0x1b, 0x04, 0x57, 0x00, 0x00, 0x0a };

                    System.Threading.Thread.Sleep(100);

                    bool log = false;

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name, log))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 2)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name, log))
                                {
                                    _isBusy = ((ack[1] >> 7) & 0x01) == 0x01;

                                    result = true;
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

        public bool SetTemperature(string temperature)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    if (double.TryParse(temperature, out double data))
                    {
                        if (data >= 0 &&
                            data <= 100
                           )
                        {
                            byte[] ret = TemperatureToBytes(data);

                            byte[] bytes = new byte[] { 0x1b, 0x05, 0x00, ret[0], ret[1], 0x0a };

                            System.Threading.Thread.Sleep(100);

                            if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                            {
                                byte[] ack = null;

                                _StopwatchTimeout.Restart();

                                while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                                {
                                    if (BytesToRead == 2)
                                    {
                                        if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                                        {
                                            if (ack[0] == bytes[3] &&
                                                ack[1] == bytes[4]
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
                        else
                        {
                            LogWrite(MethodBase.GetCurrentMethod().Name, $"온도 범위는 0도부터 100도까지 ({data})");
                        }
                    }
                    else
                    {
                        LogWrite(MethodBase.GetCurrentMethod().Name, $"double 변환 실패 ({temperature})");
                    }
                }
                catch (Exception ex)
                {
                    LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
                }
                return result;
            }
        }

        public bool Start()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] bytes = new byte[] { 0x1b, 0x00, 0x01, 0x01, 0x01, 0x0a };

                    System.Threading.Thread.Sleep(100);

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 2)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                                {
                                    if (ack[0] == 0x00 &&
                                        ack[1] == 0x01
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

        public bool Stop()
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] bytes = new byte[] { 0x1b, 0x00, 0x02, 0x02, 0x01, 0x0a };

                    System.Threading.Thread.Sleep(100);

                    if (Write(bytes, MethodBase.GetCurrentMethod().Name))
                    {
                        byte[] ack = null;

                        _StopwatchTimeout.Restart();

                        while (_StopwatchTimeout.ElapsedMilliseconds <= _Timeout)
                        {
                            if (BytesToRead == 2)
                            {
                                if (Read(ref ack, MethodBase.GetCurrentMethod().Name))
                                {
                                    if (ack[0] == 0x00 &&
                                        ack[1] == 0x01
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

        public CWaterTank()
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
