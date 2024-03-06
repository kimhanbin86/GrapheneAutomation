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
        private bool _isEntryDI = false;
        private bool _isEntryDO = false;

        private System.Threading.Thread _ThreadDIO = null;
        private bool _isThreadDIO = false;
        private void Process_DIO()
        {
            bool comm = false;

            while (_isThreadDIO)
            {
                try
                {
                    if (_CDI != null)
                    {
                        if (_CDI.IsConnected)
                        {
                            if (_isThreadDIO)
                            {
                                byte[] bytes = null;

                                if (comm = _CDI.GetInput(ref bytes))
                                {
                                    byte mask = 0x01;

                                    for (int i1 = 0; i1 < bytes.Length; i1++)
                                    {
                                        for (int i2 = 0; i2 < 8; i2++)
                                        {
                                            _DI[i1 * 8 + i2] = ((bytes[i1] >> i2) & mask) == mask;
                                        }
                                    }

                                    #region 신호 반전

                                    _DI[(int)e_DI.Input00_CH1_인산      ] = !_DI[(int)e_DI.Input00_CH1_인산      ];
                                    _DI[(int)e_DI.Input01_CH1_과산화수소] = !_DI[(int)e_DI.Input01_CH1_과산화수소];
                                    _DI[(int)e_DI.Input02_CH1_초순수    ] = !_DI[(int)e_DI.Input02_CH1_초순수    ];

                                    _DI[(int)e_DI.Input05_CH2_인산      ] = !_DI[(int)e_DI.Input05_CH2_인산      ];
                                    _DI[(int)e_DI.Input06_CH2_과산화수소] = !_DI[(int)e_DI.Input06_CH2_과산화수소];
                                    _DI[(int)e_DI.Input07_CH2_초순수    ] = !_DI[(int)e_DI.Input07_CH2_초순수    ];

                                    _DI[(int)e_DI.Input10_CH3_인산      ] = !_DI[(int)e_DI.Input10_CH3_인산      ];
                                    _DI[(int)e_DI.Input11_CH3_과산화수소] = !_DI[(int)e_DI.Input11_CH3_과산화수소];
                                    _DI[(int)e_DI.Input12_CH3_초순수    ] = !_DI[(int)e_DI.Input12_CH3_초순수    ];

                                    _DI[(int)e_DI.Input16_CH4_인산      ] = !_DI[(int)e_DI.Input16_CH4_인산      ];
                                    _DI[(int)e_DI.Input17_CH4_과산화수소] = !_DI[(int)e_DI.Input17_CH4_과산화수소];
                                    _DI[(int)e_DI.Input18_CH4_초순수    ] = !_DI[(int)e_DI.Input18_CH4_초순수    ];

                                    _DI[(int)e_DI.Input21_CH5_인산      ] = !_DI[(int)e_DI.Input21_CH5_인산      ];
                                    _DI[(int)e_DI.Input22_CH5_과산화수소] = !_DI[(int)e_DI.Input22_CH5_과산화수소];
                                    _DI[(int)e_DI.Input23_CH5_초순수    ] = !_DI[(int)e_DI.Input23_CH5_초순수    ];

                                    _DI[(int)e_DI.Input26_CH1_반응조_센서] = !_DI[(int)e_DI.Input26_CH1_반응조_센서];
                                    _DI[(int)e_DI.Input27_CH2_반응조_센서] = !_DI[(int)e_DI.Input27_CH2_반응조_센서];
                                    _DI[(int)e_DI.Input28_CH3_반응조_센서] = !_DI[(int)e_DI.Input28_CH3_반응조_센서];
                                    _DI[(int)e_DI.Input29_CH4_반응조_센서] = !_DI[(int)e_DI.Input29_CH4_반응조_센서];
                                    _DI[(int)e_DI.Input30_CH5_반응조_센서] = !_DI[(int)e_DI.Input30_CH5_반응조_센서];

                                    #endregion
                                }
                            }
                        }

                        if (_isThreadDIO)
                        {
                            if (_CDI?.IsConnected == false || comm == false)
                            {
                                _isEntryDI = true;

                                Log.Write(MethodBase.GetCurrentMethod().Name, $"{(_CDI?.IsConnected == false ? "Socket" : "통신")} 이상, DI 재연결.");

                                DIStop();
                                System.Threading.Thread.Sleep(500);

                                if (_isThreadDIO && _CDI != null)
                                {
                                    DIStart();
                                }

                                _isEntryDI = false;
                            }
                        }
                    }

                    if (_CDO != null)
                    {
                        if (_CDO.IsConnected)
                        {
                            if (_isThreadDIO)
                            {
                                byte[] bytes = new byte[8];

                                for (int i1 = 0; i1 < 4; i1++)
                                {
                                    byte sum1 = 0;
                                    byte sum2 = 0;

                                    for (int i2 = 0; i2 < 8; i2++)
                                    {
                                        sum1 += (byte)Math.Pow(2, i2);

                                        if (_DO[i1 * 8 + i2] == false)
                                        {
                                            sum2 += (byte)Math.Pow(2, i2);
                                        }
                                    }

                                    bytes[i1 + 0] = sum1;
                                    bytes[i1 + 4] = sum2;
                                }

                                comm = _CDO.SetOutput(bytes);
                            }
                        }

                        if (_isThreadDIO)
                        {
                            if (_CDO?.IsConnected == false || comm == false)
                            {
                                _isEntryDO = true;

                                Log.Write(MethodBase.GetCurrentMethod().Name, $"{(_CDO?.IsConnected == false ? "Socket" : "통신")} 이상, DO 재연결.");

                                DOStop();
                                System.Threading.Thread.Sleep(500);

                                if (_isThreadDIO && _CDO != null)
                                {
                                    DOStart();
                                }

                                _isEntryDO = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
                }

                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
