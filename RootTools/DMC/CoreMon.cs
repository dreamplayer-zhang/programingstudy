using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace RootTools.DMC
{

    public class CoreMon
    {

        /*--------------------- connect & disconnect to coreCon ----------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_create_robot", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int createRobot(string address);

        [DllImport("coremon.dll", EntryPoint = "cmon_delete_robot", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int deleteRobot(int id);

        [DllImport("coremon.dll", EntryPoint = "cmon_start_remote_control", CallingConvention = CallingConvention.Cdecl)]
        public static extern int startService(int idRobot);

        [DllImport("coremon.dll", EntryPoint = "cmon_stop_remote_control", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stopService(int idRobot);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_robot_address", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_robot_address(int idRobot, StringBuilder address);

        public static int getServerAddress(int idRobot, string address)
        {
            int retcode;
            StringBuilder sb = new StringBuilder(512);
            
            retcode = cmon_get_robot_address(idRobot, sb);

            address = sb.ToString();

            return retcode;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_robot_connection_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_robot_connection_status(int idRobot, ref bool res);

        public static bool isConnected(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_robot_connection_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }



        /*---------------------   robot status --------------------------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_lock_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_lock_status(int idRobot, ref bool res);
        public static bool getLockStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_lock_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_error_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_error_status(int idRobot, ref bool res);
        public static bool getErrorStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_error_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_tcr_mode", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_tcr_mode(int idRobot, ref int res);
        public static int getTCRMode(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_tcr_mode(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_hold_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_hold_status(int idRobot, ref bool res);

        public static bool getHoldStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_hold_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_hold_done_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_hold_done_status(int idRobot, ref bool res);
        public static bool getHoldDoneStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_hold_done_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }


        [DllImport("coremon.dll", EntryPoint = "cmon_get_moving_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_moving_status(int idRobot, ref bool res);
        public static bool getMovingStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_moving_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_tp_enable_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_tp_enable_status(int idRobot, ref bool res);
        public static bool getTPEnableStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_tp_enable_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_motor_on_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_motor_on_status(int idRobot, ref bool res);
        public static bool getMotorOnStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_motor_on_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_inching_status", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_inching_status(int idRobot, ref bool res);
        public static bool getInchingStatus(int idRobot)
        {
            bool res = false;

            int retcode = cmon_get_inching_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return false;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_teach_coordinate", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_teach_coordinate(int idRobot, ref int res);
        public static int getTeachCoordinate(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_teach_coordinate(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_teach_speed", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_teach_speed(int idRobot, ref int res);
        public static int getTeachSpeed(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_teach_speed(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        /*---------------  get position & DIO --------------------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_joint_position", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCurJointPosition(int idRobot, float[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_trans_position", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCurTransPosition(int idRobot, float[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_din", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDIN(int idRobot, UInt32[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_dout", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDOUT(int idRobot, UInt32[] res);


        /*---------------  command execute --------------------------------------*/
        [DllImport("coremon.dll", EntryPoint = "cmon_execute_command", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int executeCommand(int idRobot, string cmd);

        [DllImport("coremon.dll", EntryPoint = "cmon_execute_instruction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int executeInstruction(int idRobot, string cmd);


        /*--------------------------  program status ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_name", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_maintask_name(int idRobot, StringBuilder name);

        static public String getMainTaskName(int idRobot)
        {
            int retcode;
            StringBuilder sb = new StringBuilder(512);

            retcode = cmon_get_maintask_name(idRobot, sb);

            if (retcode == 0)
                return sb.ToString();

            return "";
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_cur_step", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_maintask_cur_step(int idRobot, ref int step);
        static public int getMainTaskCurStep(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_maintask_cur_step(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_maintask_status(int idRobot, ref int status);
        static public int getMainTaskStatus(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_maintask_status(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }
        

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_cycle", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_maintask_cycle(int idRobot, ref int cycle);
        static public int getMainTaskCycle(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_maintask_cycle(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_cur_cycle", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_maintask_cur_cycle(int idRobot, ref int cycle);
        static public int getMainTaskCurCycle(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_maintask_cur_cycle(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_maintask_move_step", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getMainTaskMoveStep(int idRobot, ref int step);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_subtask_name", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int cmon_get_subtask_name(int idRobot, StringBuilder name);
        static public String getSubTaskName(int idRobot)
        {
            int retcode;
            StringBuilder sb = new StringBuilder(512);

            retcode = cmon_get_subtask_name(idRobot, sb);

            if (retcode == 0)
                return sb.ToString();

            return "";
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_subtask_cur_step", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_subtask_cur_step(int idRobot, ref int step);
        public static int getSubTaskCurStep(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_subtask_cur_step(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_subtask_cycle", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_subtask_cycle(int idRobot, ref int cycle);
        static public int getSubTaskCycle(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_subtask_cycle(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_subtask_cur_cycle", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_subtask_cur_cycle(int idRobot, ref int cycle);
        static public int getSubTaskCurCycle(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_subtask_cur_cycle(idRobot, ref res);

            if (retcode == 0)
                return res;

            return 0;
        }

        /*--------------------------  program data ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_program_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_program_list(int idRobot, StringBuilder proglist);

        static public String getPrograms(int idRobot)
        {
            int retcode;
            StringBuilder sb = new StringBuilder(2084);

            retcode = cmon_get_program_list(idRobot, sb);

            if (retcode == 0)
                return sb.ToString();

            return "";
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_program_steps", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_program_steps(int idRobot, string pgname, StringBuilder steps);

        static public String getProgramSteps(int idRobot, string pgname)
        {
            int retcode;
            StringBuilder sb = new StringBuilder(2048);

            retcode = cmon_get_program_steps(idRobot, pgname, sb);

            if (retcode == 0)
                return sb.ToString();

            return "";
        }


        /*--------------------------  current axis data ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_cur_velocity",  CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCurVelocity(int idRobot, int[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_cur_torque", CharSet = CharSet.Ansi,CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCurTorque(int idRobot, short[] res);
        
        [DllImport("coremon.dll", EntryPoint = "cmon_get_cur_position",  CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCurPosition(int idRobot, int[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_log_start", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setLogStart(int idRobot, bool on);


        /*--------------------------  logging data ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_get_log_cur_velocity", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getLoggetCurVelocityocity(int idRobot, int axis, int[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_log_cur_torque", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getLoggetCurTorqueque(int idRobot, int axis, short[] res);

        [DllImport("coremon.dll", EntryPoint = "cmon_get_log_cur_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getLoggetCurPositionition(int idRobot, int axis, int[] res);


        /*--------------------------  set function ---------------------------*/

        /*--------------------------  set key ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_set_motor_on", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setMotorOn(int idRobot, bool on);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_teach_mode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setTeachMode(int idRobot, bool on);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_tp_enable", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setTPEnable(int idRobot, bool on);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_teach_coordinate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setTeachCoordinate(int idRobot, int mode);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_teach_speed", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setTeachSpeed(int idRobot, int mode);

        [DllImport("coremon.dll", EntryPoint = "cmon_reset_error", CallingConvention = CallingConvention.Cdecl)]
        public static extern int resetError(int idRobot);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_inching_on", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setInchingOn(int idRobot, bool on);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_jog_on", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setJogOn(int idRobot, uint minusKey, uint plusKey);

        [DllImport("coremon.dll", EntryPoint = "cmon_set_jog_off", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setJogOff(int idRobot, uint minusKey, uint plusKey);

        /*--------------------------  set program steps ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_set_program_steps", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static public extern int setProgramSteps(int idRobot, string pgname, string steps);


        /*--------------------------  set lock state ---------------------------*/

        [DllImport("coremon.dll", EntryPoint = "cmon_set_lock", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setLock(int idRobot);
        [DllImport("coremon.dll", EntryPoint = "cmon_set_unlock", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setUnLock(int idRobot);

        /*--------------------------- system log -------------------------------------*/
        [DllImport("coremon.dll", EntryPoint = "cmon_get_slog", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_slog(int idRobot, StringBuilder log);
        static public String getSystemlog(int idRobot)
        {
            StringBuilder sb = new StringBuilder(2048);

            int retcode = cmon_get_slog(idRobot, sb);

            if (retcode == 0)
                return sb.ToString();

            return "";
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_slog_size", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_slog_size(int idRobot, ref int logsize);
        public static int getSLogSize(int idRobot)
        {
            int res = 0;

            int retcode = cmon_get_slog_size(idRobot, ref res);

            return res;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_number_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_number_data(int idRobot, string vbname, ref float value);
        public static float getNumberData(int idRobot, string vbname)
        {
            float res = 0;

            int retcode = cmon_get_number_data(idRobot, vbname, ref res);

            return res;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_set_number_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_set_number_data(int idRobot, string vbname, ref float value);
        public static int setNumberData(int idRobot, string vbname, ref float value)
        {
            int retcode = cmon_set_number_data(idRobot, vbname, ref value);

            return retcode;
        }

        [DllImport("coremon.dll", EntryPoint = "cmon_get_error_code", CallingConvention = CallingConvention.Cdecl)]
        static extern int cmon_get_error_code(int idRobot, ref int errcode);
        public static int getErrorCode(int idRobot, ref int errcode)
        {
            
            int retcode = cmon_get_error_code(idRobot, ref errcode);

            return retcode;
        }
      
      

    }
}
