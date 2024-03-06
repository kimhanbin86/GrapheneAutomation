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
        private Timer _TimerInterlock = null;
        private void Tick_Interlock(object sender, EventArgs e)
        {
            _TimerInterlock?.Stop();
            try
            {
                if (_Process_Interlock)
                {
                    TabPage_Process.BackColor = TabPage_Process.BackColor != Color.Red ? Color.Red : SystemColors.Control;
                }
                else
                {
                    TabPage_Process.BackColor = SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GlobalFunctions.GetExceptionString(ex));
            }
            finally
            {
                _TimerInterlock?.Start();
            }
        }
    }
}
