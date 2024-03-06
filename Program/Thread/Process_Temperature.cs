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
        private System.Threading.Thread _ThreadTemperature = null;
        private bool _isThreadTemperature = false;
        private void Process_Temperature()
        {
            while (_isThreadTemperature)
            {
                try
                {
                    if (_CTemperature != null)
                    {
                        if (_CTemperature.IsOpen)
                        {
                            for (int i = 0; i < _Temperature.Length; i++)
                            {
                                if (CheckTempUpdate(_Temperature[i], _CTemperature.Temperature[i]))
                                {
                                    _Temperature[i] = _CTemperature.Temperature[i];
                                }
                                else
                                {
                                    Log.Write(MethodBase.GetCurrentMethod().Name, $"※ 반응조[{(e_Channel)i}] - Temp. Update NG [prev: {_Temperature[i]}] / [{_Temperature[i] - 5} ≤ n ≤ {_Temperature[i] + 10}] / [comm: {_CTemperature.Temperature[i]}]");
                                }
                            }
                        }
                    }

                    if (_CWaterTank != null)
                    {
                        if (_CWaterTank.IsOpen)
                        {
                            _WaterTank_Targ = _CWaterTank.TargetTemperature;

                            if (CheckTempUpdate(_WaterTank_Curr, _CWaterTank.CurrentTemperature))
                            {
                                _WaterTank_Curr = _CWaterTank.CurrentTemperature;
                            }
                            else
                            {
                                Log.Write(MethodBase.GetCurrentMethod().Name, $"※ 수조 - Temp. Update NG [prev: {_WaterTank_Curr}] / [{_WaterTank_Curr - 5} ≤ n ≤ {_WaterTank_Curr + 10}] / [comm: {_CWaterTank.CurrentTemperature}]");
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

        private bool CheckTempUpdate(double prev, double curr)
        {
            return prev == 0 || (curr >= prev - 5 && curr <= prev + 10);
        }
    }
}
