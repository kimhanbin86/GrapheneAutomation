using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    #region enum

    public enum e_TabPage
    {
        Status,
        Process,
        Manual,
    }

    public enum e_Channel
    {
        CH_01,
        CH_02,
        CH_03,
        CH_04,
        CH_05,
    }

    public enum e_Dispenser
    {
        흑연,
        과망간산칼륨,
    }

    #endregion

    public static class GlobalVariables
    {
        public const string PASSWORD = "itrencotech";

        public static string KeypadValue;
        public static bool IsLogin;

        public static class Form
        {
            public static frm_Keypad Keypad;
            public static frm_Login Login;
            public static frm_Main Main;
            public static frm_Parameter Parameter;
        }

        public static class Message
        {
            public const string I_공정완료 = "공정을 완료하였습니다";
            public const string I_셋업불가 = "입력값을 확인해 주세요";
            public const string I_셋업실패 = "Filling 셋업에 실패하였습니다";
            public const string I_중지불가 = "더 이상 공정을 중지할 수 없습니다";

            public const string Q_공정시작 = "공정을 시작하시겠습니까?";
            public const string Q_공정재개 = "공정을 재개하시겠습니까?";
            public const string Q_공정중지 = "공정을 중지하시겠습니까?";
            public const string Q_명령전송 = "명령을 전송하시겠습니까?";
            public const string Q_통신종료 = "통신을 해제하시겠습니까?";
        }

        public static class Parameter
        {
            public static class Sequence
            {
                public static bool test     = false;
                public static int  test_min = 1;

                public static string 인산_vol      = "125";
                public static string 인산_sec      = "3600";
                public static int    인산_min      = 60;
                public static double 인산_mlPerSec = 0.03;

                public static int Recycle_sec = 10;

                public static string 흑연_rev = "500";
                public static string 흑연_rpm = "100";
                public static int    흑연_min = 5;

                public static string 과망간산칼륨_rev = "280";
                public static string 과망간산칼륨_rpm = "7";
                public static int    과망간산칼륨_min = 40;

                public static int 반응로_1차_승온_min = 400;
                public static int 반응로_2차_승온_min = 90;

                public static int 반응로_가열_min = 950;

                public static int 반응로_온도_하강_min = 90;

                public static string 초순수_1차_vol      = "100";
                public static string 초순수_1차_sec      = "2100";
                public static int    초순수_1차_min      = 35;
                public static double 초순수_1차_mlPerSec = 0.05;

                public static double 반응조확인 = 50;

                public static string 초순수_3차_vol      = "1000";
                public static string 초순수_3차_sec      = "1200";
                public static int    초순수_3차_min      = 20;
                public static double 초순수_3차_mlPerSec = 0.83;

                public static string 초순수_4차_vol      = "1800";
                public static string 초순수_4차_sec      = "2160";
                public static int    초순수_4차_min      = 36;
                public static double 초순수_4차_mlPerSec = 0.83;

                public static int 반응물_교반_min = 100;

                public static string 과산화수소_1차_vol      = "100";
                public static string 과산화수소_1차_sec      = "6000";
                public static int    과산화수소_1차_min      = 100;
                public static double 과산화수소_1차_mlPerSec = 0.02;

                public static string 과산화수소_2차_vol      = "170";
                public static string 과산화수소_2차_sec      = "6000";
                public static int    과산화수소_2차_min      = 100;
                public static double 과산화수소_2차_mlPerSec = 0.03;

                public static bool skip_인산         = false;
                public static bool skip_흑연         = false;
                public static bool skip_과망간산칼륨 = false;

                public static int 교반기_rpm_인산         = 210;
                public static int 교반기_rpm_흑연         = 210;
                public static int 교반기_rpm_과망간산칼륨 = 210;
                public static int 교반기_rpm_반응로       = 210;
                public static int 교반기_rpm_초순수       = 210;
                public static int 교반기_rpm_과산화수소   = 210;
            }

            public static class CheckSequence
            {
                public static bool 채널선택확인 = true;

                public static bool 헤드하강확인 = true;

                public static bool 반응조센서확인 = true;

                public static bool 반응조온도확인 = true;
                public static double 반응조온도 = 10;

                public static bool 반응로온도확인 = true;
                public static double 반응로온도 = 5;
            }

            public static class CheckInterlock
            {
                public static double 반응로승온또는가열 = 20;

                public static double 부반응물제거 = 80;
            }
        }

        public static DateTime ProgramExecutionTime = DateTime.Now;
    }
}
