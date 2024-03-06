using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Library.INI;
using Library.Log;

namespace Program
{
    public static class GlobalFunctions
    {
        public static bool CheckProcess(string processName)
        {
            bool result = false;
            try
            {
                result = Process.GetProcessesByName(processName).Length < 2;
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        public static void DoubleBuffered(object obj, object value)
        {
            try
            {
                Type type = obj.GetType();
                PropertyInfo property = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(obj, value);
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
        }

        public static Control[] GetControls(Control container)
        {
            List<Control> controls = new List<Control>();
            try
            {
                foreach (Control control in container.Controls)
                {
                    controls.Add(control);

                    if (control.Controls.Count > 0)
                    {
                        controls.AddRange(GetControls(control));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return controls.ToArray();
        }

        public static string GetDateTimeString(DateTime value)
        {
            string result = string.Empty;
            try
            {
                result = $"{value:yyyy-MM-dd HH:mm:ss.fff}";
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        public static string GetExceptionString(Exception ex)
        {
            return string.Format("try catch error (message=[{0}])", ex);
        }

        public static DialogResult MessageBox(string call, string text, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            DialogResult result = DialogResult.None;
            try
            {
                Log.Write(call, $"MessageBox=[{text}]");

                result = System.Windows.Forms.MessageBox.Show(text, "MessageBox", buttons, icon);

                Log.Write(call, $"[{buttons}]=[{result}]");
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        public static bool LoadParameter()
        {
            bool result = false;
            try
            {
                string file = $@"{Application.StartupPath}\INI\Parameter.ini";

                #region Sequence

                string section = "Sequence";

                GlobalVariables.Parameter.Sequence.test = INI.GetString(section, "test", file).Trim() == "1";
                GlobalVariables.Parameter.Sequence.test_min = Convert.ToInt32(INI.GetString(section, "test_min", file).Trim());

                GlobalVariables.Parameter.Sequence.인산_vol = INI.GetString(section, "인산_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.인산_sec = INI.GetString(section, "인산_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.인산_min = Convert.ToInt32(INI.GetString(section, "인산_min", file).Trim());
                GlobalVariables.Parameter.Sequence.인산_mlPerSec = Convert.ToDouble(INI.GetString(section, "인산_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.Recycle_sec = Convert.ToInt32(INI.GetString(section, "Recycle_sec", file).Trim());

                GlobalVariables.Parameter.Sequence.흑연_rev = INI.GetString(section, "흑연_rev", file).Trim();
                GlobalVariables.Parameter.Sequence.흑연_rpm = INI.GetString(section, "흑연_rpm", file).Trim();
                GlobalVariables.Parameter.Sequence.흑연_min = Convert.ToInt32(INI.GetString(section, "흑연_min", file).Trim());

                GlobalVariables.Parameter.Sequence.과망간산칼륨_rev = INI.GetString(section, "과망간산칼륨_rev", file).Trim();
                GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm = INI.GetString(section, "과망간산칼륨_rpm", file).Trim();
                GlobalVariables.Parameter.Sequence.과망간산칼륨_min = Convert.ToInt32(INI.GetString(section, "과망간산칼륨_min", file).Trim());

                GlobalVariables.Parameter.Sequence.반응로_1차_승온_min = Convert.ToInt32(INI.GetString(section, "반응로_1차_승온_min", file).Trim());
                GlobalVariables.Parameter.Sequence.반응로_2차_승온_min = Convert.ToInt32(INI.GetString(section, "반응로_2차_승온_min", file).Trim());

                GlobalVariables.Parameter.Sequence.반응로_가열_min = Convert.ToInt32(INI.GetString(section, "반응로_가열_min", file).Trim());

                GlobalVariables.Parameter.Sequence.반응로_온도_하강_min = Convert.ToInt32(INI.GetString(section, "반응로_온도_하강_min", file).Trim());

                GlobalVariables.Parameter.Sequence.초순수_1차_vol = INI.GetString(section, "초순수_1차_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_1차_sec = INI.GetString(section, "초순수_1차_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_1차_min = Convert.ToInt32(INI.GetString(section, "초순수_1차_min", file).Trim());
                GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec = Convert.ToDouble(INI.GetString(section, "초순수_1차_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.반응조확인 = Convert.ToDouble(INI.GetString(section, "반응조확인", file).Trim());

                GlobalVariables.Parameter.Sequence.초순수_3차_vol = INI.GetString(section, "초순수_3차_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_3차_sec = INI.GetString(section, "초순수_3차_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_3차_min = Convert.ToInt32(INI.GetString(section, "초순수_3차_min", file).Trim());
                GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec = Convert.ToDouble(INI.GetString(section, "초순수_3차_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.초순수_4차_vol = INI.GetString(section, "초순수_4차_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_4차_sec = INI.GetString(section, "초순수_4차_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.초순수_4차_min = Convert.ToInt32(INI.GetString(section, "초순수_4차_min", file).Trim());
                GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec = Convert.ToDouble(INI.GetString(section, "초순수_4차_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.반응물_교반_min = Convert.ToInt32(INI.GetString(section, "반응물_교반_min", file).Trim());

                GlobalVariables.Parameter.Sequence.과산화수소_1차_vol = INI.GetString(section, "과산화수소_1차_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.과산화수소_1차_sec = INI.GetString(section, "과산화수소_1차_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.과산화수소_1차_min = Convert.ToInt32(INI.GetString(section, "과산화수소_1차_min", file).Trim());
                GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec = Convert.ToDouble(INI.GetString(section, "과산화수소_1차_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.과산화수소_2차_vol = INI.GetString(section, "과산화수소_2차_vol", file).Trim();
                GlobalVariables.Parameter.Sequence.과산화수소_2차_sec = INI.GetString(section, "과산화수소_2차_sec", file).Trim();
                GlobalVariables.Parameter.Sequence.과산화수소_2차_min = Convert.ToInt32(INI.GetString(section, "과산화수소_2차_min", file).Trim());
                GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec = Convert.ToDouble(INI.GetString(section, "과산화수소_2차_mlPerSec", file).Trim());

                GlobalVariables.Parameter.Sequence.skip_인산 = INI.GetString(section, "skip_인산", file).Trim() == "1";
                GlobalVariables.Parameter.Sequence.skip_흑연 = INI.GetString(section, "skip_흑연", file).Trim() == "1";
                GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 = INI.GetString(section, "skip_과망간산칼륨", file).Trim() == "1";

                GlobalVariables.Parameter.Sequence.교반기_rpm_인산 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_인산", file).Trim());
                GlobalVariables.Parameter.Sequence.교반기_rpm_흑연 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_흑연", file).Trim());
                GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_과망간산칼륨", file).Trim());
                GlobalVariables.Parameter.Sequence.교반기_rpm_반응로 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_반응로", file).Trim());
                GlobalVariables.Parameter.Sequence.교반기_rpm_초순수 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_초순수", file).Trim());
                GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소 = Convert.ToInt32(INI.GetString(section, "교반기_rpm_과산화수소", file).Trim());

                #endregion

                #region Check

                section = "Check";

                GlobalVariables.Parameter.CheckSequence.채널선택확인 = INI.GetString(section, "채널선택확인", file).Trim() == "1";

                GlobalVariables.Parameter.CheckSequence.헤드하강확인 = INI.GetString(section, "헤드하강확인", file).Trim() == "1";

                GlobalVariables.Parameter.CheckSequence.반응조센서확인 = INI.GetString(section, "반응조센서확인", file).Trim() == "1";

                GlobalVariables.Parameter.CheckSequence.반응조온도확인 = INI.GetString(section, "반응조온도확인", file).Trim() == "1";
                GlobalVariables.Parameter.CheckSequence.반응조온도 = Convert.ToDouble(INI.GetString(section, "반응조온도", file).Trim());

                GlobalVariables.Parameter.CheckSequence.반응로온도확인 = INI.GetString(section, "반응로온도확인", file).Trim() == "1";
                GlobalVariables.Parameter.CheckSequence.반응로온도 = Convert.ToDouble(INI.GetString(section, "반응로온도", file).Trim());

                #endregion

                #region Interlock

                section = "Interlock";

                GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열 = Convert.ToDouble(INI.GetString(section, "반응로승온또는가열", file).Trim());

                GlobalVariables.Parameter.CheckInterlock.부반응물제거 = Convert.ToDouble(INI.GetString(section, "부반응물제거", file).Trim());

                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        public static void ViewParameter()
        {
            #region Sequence

            Log.Write(MethodBase.GetCurrentMethod().Name, $"test=[{GlobalVariables.Parameter.Sequence.test}] / min=[{GlobalVariables.Parameter.Sequence.test_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"인산_vol=[{GlobalVariables.Parameter.Sequence.인산_vol}] / sec=[{GlobalVariables.Parameter.Sequence.인산_sec}] / min=[{GlobalVariables.Parameter.Sequence.인산_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.인산_mlPerSec}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"Recycle_sec=[{GlobalVariables.Parameter.Sequence.Recycle_sec}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"흑연_rev=[{GlobalVariables.Parameter.Sequence.흑연_rev}] / rpm=[{GlobalVariables.Parameter.Sequence.흑연_rpm}] / min=[{GlobalVariables.Parameter.Sequence.흑연_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"과망간산칼륨_rev=[{GlobalVariables.Parameter.Sequence.과망간산칼륨_rev}] / rpm=[{GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm}] / min=[{GlobalVariables.Parameter.Sequence.과망간산칼륨_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로_1차_승온_min=[{GlobalVariables.Parameter.Sequence.반응로_1차_승온_min}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로_2차_승온_min=[{GlobalVariables.Parameter.Sequence.반응로_2차_승온_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로_가열_min=[{GlobalVariables.Parameter.Sequence.반응로_가열_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로_온도_하강_min=[{GlobalVariables.Parameter.Sequence.반응로_온도_하강_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"초순수_1차_vol=[{GlobalVariables.Parameter.Sequence.초순수_1차_vol}] / sec=[{GlobalVariables.Parameter.Sequence.초순수_1차_sec}] / min=[{GlobalVariables.Parameter.Sequence.초순수_1차_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응조확인=[{GlobalVariables.Parameter.Sequence.반응조확인}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"초순수_3차_vol=[{GlobalVariables.Parameter.Sequence.초순수_3차_vol}] / sec=[{GlobalVariables.Parameter.Sequence.초순수_3차_sec}] / min=[{GlobalVariables.Parameter.Sequence.초순수_3차_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"초순수_4차_vol=[{GlobalVariables.Parameter.Sequence.초순수_4차_vol}] / sec=[{GlobalVariables.Parameter.Sequence.초순수_4차_sec}] / min=[{GlobalVariables.Parameter.Sequence.초순수_4차_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응물_교반_min=[{GlobalVariables.Parameter.Sequence.반응물_교반_min}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"과산화수소_1차_vol=[{GlobalVariables.Parameter.Sequence.과산화수소_1차_vol}] / sec=[{GlobalVariables.Parameter.Sequence.과산화수소_1차_sec}] / min=[{GlobalVariables.Parameter.Sequence.과산화수소_1차_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"과산화수소_2차_vol=[{GlobalVariables.Parameter.Sequence.과산화수소_2차_vol}] / sec=[{GlobalVariables.Parameter.Sequence.과산화수소_2차_sec}] / min=[{GlobalVariables.Parameter.Sequence.과산화수소_2차_min}] / ml/sec=[{GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"skip_인산=[{GlobalVariables.Parameter.Sequence.skip_인산}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"skip_흑연=[{GlobalVariables.Parameter.Sequence.skip_흑연}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"skip_과망간산칼륨=[{GlobalVariables.Parameter.Sequence.skip_과망간산칼륨}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_인산=[{GlobalVariables.Parameter.Sequence.교반기_rpm_인산}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_흑연=[{GlobalVariables.Parameter.Sequence.교반기_rpm_흑연}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_과망간산칼륨=[{GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_반응로=[{GlobalVariables.Parameter.Sequence.교반기_rpm_반응로}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_초순수=[{GlobalVariables.Parameter.Sequence.교반기_rpm_초순수}]");
            Log.Write(MethodBase.GetCurrentMethod().Name, $"교반기_rpm_과산화수소=[{GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소}]");

            #endregion

            #region Check

            Log.Write(MethodBase.GetCurrentMethod().Name, $"채널선택확인=[{GlobalVariables.Parameter.CheckSequence.채널선택확인}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"헤드하강확인=[{GlobalVariables.Parameter.CheckSequence.헤드하강확인}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응조센서확인=[{GlobalVariables.Parameter.CheckSequence.반응조센서확인}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응조온도확인=[{GlobalVariables.Parameter.CheckSequence.반응조온도확인}] / 반응조온도=[{GlobalVariables.Parameter.CheckSequence.반응조온도}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로온도확인=[{GlobalVariables.Parameter.CheckSequence.반응로온도확인}] / 반응로온도=[{GlobalVariables.Parameter.CheckSequence.반응로온도}]");

            #endregion

            #region Interlock

            Log.Write(MethodBase.GetCurrentMethod().Name, $"반응로승온또는가열=[{GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열}]");

            Log.Write(MethodBase.GetCurrentMethod().Name, $"부반응물제거=[{GlobalVariables.Parameter.CheckInterlock.부반응물제거}]");

            #endregion
        }

        public static bool SaveParameter()
        {
            bool result = true;
            try
            {
                BackupParameter();

                string path = $@"{Application.StartupPath}\INI";
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                string file = $@"{path}\Parameter.ini";

                #region Sequence

                string section = "Sequence";

                result &= INI.WriteString(section, "test", GlobalVariables.Parameter.Sequence.test ? "1" : "0", file);
                result &= INI.WriteString(section, "test_min", GlobalVariables.Parameter.Sequence.test_min.ToString(), file);

                result &= INI.WriteString(section, "인산_vol", GlobalVariables.Parameter.Sequence.인산_vol, file);
                result &= INI.WriteString(section, "인산_sec", GlobalVariables.Parameter.Sequence.인산_sec, file);
                result &= INI.WriteString(section, "인산_min", GlobalVariables.Parameter.Sequence.인산_min.ToString(), file);
                result &= INI.WriteString(section, "인산_mlPerSec", GlobalVariables.Parameter.Sequence.인산_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "Recycle_sec", GlobalVariables.Parameter.Sequence.Recycle_sec.ToString(), file);

                result &= INI.WriteString(section, "흑연_rev", GlobalVariables.Parameter.Sequence.흑연_rev, file);
                result &= INI.WriteString(section, "흑연_rpm", GlobalVariables.Parameter.Sequence.흑연_rpm, file);
                result &= INI.WriteString(section, "흑연_min", GlobalVariables.Parameter.Sequence.흑연_min.ToString(), file);

                result &= INI.WriteString(section, "과망간산칼륨_rev", GlobalVariables.Parameter.Sequence.과망간산칼륨_rev, file);
                result &= INI.WriteString(section, "과망간산칼륨_rpm", GlobalVariables.Parameter.Sequence.과망간산칼륨_rpm, file);
                result &= INI.WriteString(section, "과망간산칼륨_min", GlobalVariables.Parameter.Sequence.과망간산칼륨_min.ToString(), file);

                result &= INI.WriteString(section, "반응로_1차_승온_min", GlobalVariables.Parameter.Sequence.반응로_1차_승온_min.ToString(), file);
                result &= INI.WriteString(section, "반응로_2차_승온_min", GlobalVariables.Parameter.Sequence.반응로_2차_승온_min.ToString(), file);

                result &= INI.WriteString(section, "반응로_가열_min", GlobalVariables.Parameter.Sequence.반응로_가열_min.ToString(), file);

                result &= INI.WriteString(section, "반응로_온도_하강_min", GlobalVariables.Parameter.Sequence.반응로_온도_하강_min.ToString(), file);

                result &= INI.WriteString(section, "초순수_1차_vol", GlobalVariables.Parameter.Sequence.초순수_1차_vol, file);
                result &= INI.WriteString(section, "초순수_1차_sec", GlobalVariables.Parameter.Sequence.초순수_1차_sec, file);
                result &= INI.WriteString(section, "초순수_1차_min", GlobalVariables.Parameter.Sequence.초순수_1차_min.ToString(), file);
                result &= INI.WriteString(section, "초순수_1차_mlPerSec", GlobalVariables.Parameter.Sequence.초순수_1차_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "반응조확인", GlobalVariables.Parameter.Sequence.반응조확인.ToString(), file);

                result &= INI.WriteString(section, "초순수_3차_vol", GlobalVariables.Parameter.Sequence.초순수_3차_vol, file);
                result &= INI.WriteString(section, "초순수_3차_sec", GlobalVariables.Parameter.Sequence.초순수_3차_sec, file);
                result &= INI.WriteString(section, "초순수_3차_min", GlobalVariables.Parameter.Sequence.초순수_3차_min.ToString(), file);
                result &= INI.WriteString(section, "초순수_3차_mlPerSec", GlobalVariables.Parameter.Sequence.초순수_3차_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "초순수_4차_vol", GlobalVariables.Parameter.Sequence.초순수_4차_vol, file);
                result &= INI.WriteString(section, "초순수_4차_sec", GlobalVariables.Parameter.Sequence.초순수_4차_sec, file);
                result &= INI.WriteString(section, "초순수_4차_min", GlobalVariables.Parameter.Sequence.초순수_4차_min.ToString(), file);
                result &= INI.WriteString(section, "초순수_4차_mlPerSec", GlobalVariables.Parameter.Sequence.초순수_4차_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "반응물_교반_min", GlobalVariables.Parameter.Sequence.반응물_교반_min.ToString(), file);

                result &= INI.WriteString(section, "과산화수소_1차_vol", GlobalVariables.Parameter.Sequence.과산화수소_1차_vol, file);
                result &= INI.WriteString(section, "과산화수소_1차_sec", GlobalVariables.Parameter.Sequence.과산화수소_1차_sec, file);
                result &= INI.WriteString(section, "과산화수소_1차_min", GlobalVariables.Parameter.Sequence.과산화수소_1차_min.ToString(), file);
                result &= INI.WriteString(section, "과산화수소_1차_mlPerSec", GlobalVariables.Parameter.Sequence.과산화수소_1차_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "과산화수소_2차_vol", GlobalVariables.Parameter.Sequence.과산화수소_2차_vol, file);
                result &= INI.WriteString(section, "과산화수소_2차_sec", GlobalVariables.Parameter.Sequence.과산화수소_2차_sec, file);
                result &= INI.WriteString(section, "과산화수소_2차_min", GlobalVariables.Parameter.Sequence.과산화수소_2차_min.ToString(), file);
                result &= INI.WriteString(section, "과산화수소_2차_mlPerSec", GlobalVariables.Parameter.Sequence.과산화수소_2차_mlPerSec.ToString(), file);

                result &= INI.WriteString(section, "skip_인산", GlobalVariables.Parameter.Sequence.skip_인산 ? "1" : "0", file);
                result &= INI.WriteString(section, "skip_흑연", GlobalVariables.Parameter.Sequence.skip_흑연 ? "1" : "0", file);
                result &= INI.WriteString(section, "skip_과망간산칼륨", GlobalVariables.Parameter.Sequence.skip_과망간산칼륨 ? "1" : "0", file);

                result &= INI.WriteString(section, "교반기_rpm_인산", GlobalVariables.Parameter.Sequence.교반기_rpm_인산.ToString(), file);
                result &= INI.WriteString(section, "교반기_rpm_흑연", GlobalVariables.Parameter.Sequence.교반기_rpm_흑연.ToString(), file);
                result &= INI.WriteString(section, "교반기_rpm_과망간산칼륨", GlobalVariables.Parameter.Sequence.교반기_rpm_과망간산칼륨.ToString(), file);
                result &= INI.WriteString(section, "교반기_rpm_반응로", GlobalVariables.Parameter.Sequence.교반기_rpm_반응로.ToString(), file);
                result &= INI.WriteString(section, "교반기_rpm_초순수", GlobalVariables.Parameter.Sequence.교반기_rpm_초순수.ToString(), file);
                result &= INI.WriteString(section, "교반기_rpm_과산화수소", GlobalVariables.Parameter.Sequence.교반기_rpm_과산화수소.ToString(), file);

                #endregion

                #region Check

                section = "Check";

                result &= INI.WriteString(section, "채널선택확인", GlobalVariables.Parameter.CheckSequence.채널선택확인 ? "1" : "0", file);

                result &= INI.WriteString(section, "헤드하강확인", GlobalVariables.Parameter.CheckSequence.헤드하강확인 ? "1" : "0", file);

                result &= INI.WriteString(section, "반응조센서확인", GlobalVariables.Parameter.CheckSequence.반응조센서확인 ? "1" : "0", file);

                result &= INI.WriteString(section, "반응조온도확인", GlobalVariables.Parameter.CheckSequence.반응조온도확인 ? "1" : "0", file);
                result &= INI.WriteString(section, "반응조온도", GlobalVariables.Parameter.CheckSequence.반응조온도.ToString(), file);

                result &= INI.WriteString(section, "반응로온도확인", GlobalVariables.Parameter.CheckSequence.반응로온도확인 ? "1" : "0", file);
                result &= INI.WriteString(section, "반응로온도", GlobalVariables.Parameter.CheckSequence.반응로온도.ToString(), file);

                #endregion

                #region Interlock

                section = "Interlock";

                result &= INI.WriteString(section, "반응로승온또는가열", GlobalVariables.Parameter.CheckInterlock.반응로승온또는가열.ToString(), file);

                result &= INI.WriteString(section, "부반응물제거", GlobalVariables.Parameter.CheckInterlock.부반응물제거.ToString(), file);

                #endregion
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        public static string GetDateTimeStringWithoutSecond(DateTime value)
        {
            string result = string.Empty;
            try
            {
                result = $"{value:yyyy-MM-dd HH:mm}";
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
            return result;
        }

        private static void BackupParameter()
        {
            try
            {
                string srcFileName = $@"{Application.StartupPath}\INI\Parameter.ini";
                if (File.Exists(srcFileName))
                {
                    string path = $@"{Application.StartupPath}\BAK";
                    if (Directory.Exists(path) == false)
                    {
                        Directory.CreateDirectory(path);
                    }

                    string dstFileName = $@"{path}\Parameter_{DateTime.Now:yyyyMMdd_HHmmss}.ini";
                    File.Copy(srcFileName, dstFileName, true);

                    Log.Write(MethodBase.GetCurrentMethod().Name, $"Backup OK ({dstFileName})");
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
        }

        public static void DeleteLogFile()
        {
            try
            {
                string path = $@"{Application.StartupPath}\Log";

                if (Directory.Exists(path) == false)
                {
                    return;
                }

                #region yyyy

                string[] directories = Directory.GetDirectories(path);

                string yyyy = GlobalVariables.ProgramExecutionTime.ToString("yyyy");

                for (int i = 0; i < directories.Length; i++)
                {
                    if (directories[i].Substring(directories[i].LastIndexOf("\\") + 1) != yyyy)
                    {
                        if (Directory.Exists(directories[i]))
                        {
                            Directory.Delete(directories[i], true);
                        }
                    }
                }

                #endregion

                // 당월 포함 3개월 로그 유지.
                // e.g. 12월이면 09, 10, 11, 12
                int offset = 6;

                #region MM

                directories = Directory.GetDirectories($"{path}\\{yyyy}");

                int MM = Convert.ToInt32(GlobalVariables.ProgramExecutionTime.ToString("MM"));

                for (int i = 0; i < directories.Length; i++)
                {
                    int month = Convert.ToInt32(directories[i].Substring(directories[i].LastIndexOf("\\") + 1));

                    if (month < MM - offset ||
                        month > MM
                       )
                    {
                        if (Directory.Exists(directories[i]))
                        {
                            Directory.Delete(directories[i], true);
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Log.Write(MethodBase.GetCurrentMethod().Name, GetExceptionString(ex));
            }
        }
    }
}
