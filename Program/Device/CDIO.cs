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
    internal class CDIO : SocketClient
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
            GetInput = 0xC0,
            SetOutput = 0xC6,
        }

        #endregion

        #region 상수

        private const byte c_Header = 0xAA;

        #endregion

        #region 필드

        private readonly object _LockObject = new object();

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
            }
            catch (Exception ex)
            {
                LogWrite(MethodBase.GetCurrentMethod().Name, Utility.GetString(ex));
            }
            return result;
        }

        public bool GetInput(ref byte[] bytes)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.GetInput);
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
                                bytes = new byte[4];
                                for (int i = 0; i < bytes.Length; i++)
                                {
                                    bytes[i] = ack[(int)e_Frame.FrameType + 2 + i];
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

        public bool SetOutput(byte[] bytes)
        {
            lock (_LockObject)
            {
                bool result = false;
                try
                {
                    byte[] frame = MakeFrame(e_FrameType.SetOutput);
                    frame[(int)e_Frame.SyncNo] = GetSyncNo();

                    frame = frame.Concat(bytes).ToArray();

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
    }
}
