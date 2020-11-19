using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiSoDefines
{
    
    public class SiSoParameter
    {
            /*
             * Defines
             */
        /* +-- PARAMETER --+ */

        public const int FG_WIDTH = 100;
        public const int FG_HEIGHT = 200;

        public const int FG_MAXWIDTH = 6100;
        public const int FG_MAXHEIGHT = 6200;
        public const int FG_ACTIVEPORT = 6300;

        public const int FG_XOFFSET = 300;
        public const int FG_YOFFSET = 400;
        public const int FG_XSHIFT = 500;
        public const int FG_TIMEOUT = 600;
        public const int FG_FORMAT = 700;
        public const int FG_CAMSUBTYP = 80;
        public const int FG_FRAMESPERSEC = 90;
        public const int FG_MAXFRAMESPERSEC = 91;
        public const int FG_MINFRAMESPERSEC = 92;
        public const int FG_LINESPERSEC = 95;
        public const int FG_LINEPERIODE = 96;

        public const int FG_EXPOSURE = 10020;		/**< Exposure Time in us (Brigthness) (float) */
        public const int FG_LINEEXPOSURE = 10030;
        public const int FG_HDSYNC = 10050;

        public const int FG_PRESCALER = FG_HDSYNC;
        public const int FG_LINETRIGGER = FG_HDSYNC;

        public const int FG_RS232PARON = 10060;


        public const int FG_PIXELDEPTH = 4000;
        public const int FG_BITALIGNMENT = 4010;
        public const int FG_LINEALIGNMENT = 4020;

        public const int FG_CAMBITWIDTH = 5000;
        public const int FG_CAMBITSHIFT = 5010;
        public const int FG_SHIFTCAMDATARIGHT = 5020;
        public const int FG_ROTATECAMDATA = FG_SHIFTCAMDATARIGHT; /* compatibility mode, do not use */
        public const int FG_USEDVAL = 5025;
        public const int FG_SWAPENDIAN = 5028;
        public const int FG_MASKCAMDATA = 5030;
        public const int FG_ADDOFFSET = 5035;
        public const int FG_DROPPEDIMAGEES = 5040;
        public const int FG_SENSORREADOUT = 5050;
        public const int FG_SENSORREADOUT_TAPS = 5051;
        public const int FG_SENSORREADOUT_DIREC = 5052;

        public const int FG_TRIGGERMODE = 8100;
        public const int FG_LINETRIGGERMODE = 8102;
        public const int FG_IMGTRIGGERMODE = 8104;
        public const int FG_IMGTRIGGERON = 8106;
        public const int FG_TRIGGERINSRC = 8110;
        public const int FG_LINETRIGGERINSRC = 8112;
        public const int FG_IMGTRIGGERINSRC = 8113;
        public const int FG_LINETRIGGERINPOLARITY = 8115;
        public const int FG_IMGTRIGGERINPOLARITY = 8116;
        public const int FG_TRIGGERINPOLARITY = FG_IMGTRIGGERINPOLARITY;
        public const int FG_IMGTRIGGERGATEDELAY = 8118;
        public const int FG_USEROUT = 8120;
        public const int FG_EXSYNCINVERT = 8200;
        public const int FG_EXSYNCON = 8300;
        public const int FG_EXSYNCDELAY = 8400;
        public const int FG_EXSYNCPOLARITY = 8420;
        public const int FG_DEADTIME = 8450;
        public const int FG_DEADTIME_OFFSET = 8460;
        public const int FG_BGRRGBORDER = 8500;
        public const int FG_FLASHON = 8600;
        public const int FG_SENDSOFTWARETRIGGER = 8800;
        public const int FG_SETSOFTWARETRIGGER = 8801;
        public const int FG_LINETRIGGERDELAY = 8900;
        public const int FG_TRIGGERMASTERSYNC = 9000;

        public const int FG_SHAFTENCODERINSRC = 9100;
        public const int FG_SHAFTENCODERON = 9110;
        public const int FG_SHAFTENCODERLEADING = 9120;
        public const int FG_SHAFTENCODER_COMPCOUNTER = 9125;

        public const int FG_RGB_MAP_RED = 9200;
        public const int FG_RGB_MAP_GREEN = 9210;
        public const int FG_RGB_MAP_BLUE = 9220;

        public const int FG_CAMSTATUS = 2000;
        public const int FG_CAMSTATUS_EXTENDED = 2050;
        public const int FG_TWOCAMMODEL = 2100;
        public const int FG_PORT = 3000;
        public const int FG_NR_OF_DMAS = 3050;
        public const int FG_NR_OF_CAMS = 3060;
        public const int FG_NR_OF_PROCESSES = 3070;
        public const int FG_DMA_PORT = 3080;
        public const int FG_CAM_PORT = 3090;

        public const int FG_TRANSFER_LEN = 5210;

        public const int FG_STROBEPULSEDELAY = 8700;
        public const int FG_STROBEPULSEREDUCE = 8710;
        public const int FG_STROBEPULSESRCSEL = 8720;
        public const int FG_STROBEPULSEINVERT = 8730;

        public const int FG_CAMERA_LINK_CAMTYP = 11011;
        public const int FG_CL_CAMTYP = 11011;
        public const int FG_CAMTYP = 11011;
        public const int FG_GBE_CAMTYP = 11011;

        public const int FG_LOOKUPTABLE = 12000;
        public const int FG_LUT_FILE = 12010;
        public const int FG_LUT_SAVE_LOAD_FILE = 12020;

        public const int FG_KNEE_LUT = 12100;
        public const int FG_KNEE_LUT_FILE = 12110;
        public const int FG_KNEE_LUT_SAVE_LOAD_FILE = 12120;
        public const int FG_KNEE_LUT_MODE = 12130;
        public const int FG_KNEE_LUT_ACCESS = 12140;

        public const int FG_KNEE_LUT_SCALE = 12101;
        public const int FG_KNEE_LUT_OFFSET = 12102;
        public const int FG_KNEE_LUT_GAMMA = 12103;
        public const int FG_KNEE_LUT_INVERT = 12104;

        public const int FG_MEDIAN = 12200;

        public const int FG_2DSHADINGPARAMETER = 12500;

        public const int FG_SCALINGFACTOR_RED = 13000;
        public const int FG_SCALINGFACTOR_BLUE = 13010;
        public const int FG_BAYERINIT = 13020;
        public const int FG_SCALINGFACTOR_GREEN = 13030;

        public const int FG_CCSEL = 14000;
        public const int FG_CCSEL0 = 14001;
        public const int FG_CCSEL1 = 14002;
        public const int FG_CCSEL2 = 14003;
        public const int FG_CCSEL3 = 14004;

        public const int FG_CCSEL_INVERT = 14005;
        public const int FG_CCSEL_INVERT0 = 14006;
        public const int FG_CCSEL_INVERT1 = 14007;
        public const int FG_CCSEL_INVERT2 = 14008;
        public const int FG_CCSEL_INVERT3 = 14009;

        public const int FG_DIGIO_INPUT = 14010;
        public const int FG_DIGIO_OUTPUT = 14020;

        public const int FG_IMAGE_TAG = 22000;
        public const int FG_TIMESTAMP = 22020;
        public const int FG_TIMESTAMP_LONG = 22030;

        public const int FG_LICENSESTRING0 = 23000;
        public const int FG_LICENSESTRING1 = 23010;
        public const int FG_LICENSESTRING2 = 23020;

        public const int FG_ACCESS_POINTER = 23030;

        public const int FG_ROIX = 23100;
        public const int FG_ROIY = 23110;
        public const int FG_SHADING_SUBIMAGE = 23120;
        public const int FG_SHADING_MULTENABLE = 23130;
        public const int FG_SHADING_OFFSETENABLE = 23140;
        public const int FG_SHADING_SUBENABLE = FG_SHADING_OFFSETENABLE;

        public const int FG_SHADING_MAX_MULT = 23135;

        public const int FG_SHADING_RUNSUBIMAGE0 = 23121;
        public const int FG_SHADING_RUNSUBIMAGE1 = 23122;
        public const int FG_SHADING_RUNSUBIMAGE2 = 23123;
        public const int FG_SHADING_RUNSUBIMAGE3 = 23124;

        public const int FG_SHADING_ENABLEMULT0 = 23131;
        public const int FG_SHADING_ENABLEMULT1 = 23132;
        public const int FG_SHADING_ENABLEMULT2 = 23133;
        public const int FG_SHADING_ENABLEMULT3 = 23134;

        public const int FG_SHADING_ENABLESUB0 = 23141;
        public const int FG_SHADING_ENABLESUB1 = 23142;
        public const int FG_SHADING_ENABLESUB2 = 23143;
        public const int FG_SHADING_ENABLESUB3 = 23144;

        public const int FG_SHADING_FPNENABLE = 23150;
        public const int FG_SHADING_ENABLEFPN0 = 23151;
        public const int FG_SHADING_ENABLEFPN1 = 23152;
        public const int FG_SHADING_ENABLEFPN2 = 23153;
        public const int FG_SHADING_ENABLEFPN3 = 23154;


        public const int FG_SHADING_THRESHOLD0 = 23156;
        public const int FG_SHADING_THRESHOLD1 = 23157;
        public const int FG_SHADING_THRESHOLD2 = 23158;
        public const int FG_SHADING_THRESHOLD3 = 23159;

        public const int FG_SHADING_MULTFILE0 = 23160;
        public const int FG_SHADING_SUBFILE0 = 23170;
        public const int FG_SHADING_FPNFILE0 = 23180;
        public const int FG_SHADING_MULTFILE1 = 23210;
        public const int FG_SHADING_SUBFILE1 = 23225;
        public const int FG_SHADING_FPNFILE1 = 23230;
        public const int FG_SHADING_MULTFILE2 = 23240;
        public const int FG_SHADING_SUBFILE2 = 23250;
        public const int FG_SHADING_FPNFILE2 = 23260;
        public const int FG_SHADING_MULTFILE3 = 23270;
        public const int FG_SHADING_SUBFILE3 = 23280;
        public const int FG_SHADING_FPNFILE3 = 23290;

        public const int FG_CONTRAST = 23200;
        public const int FG_BRIGHTNESS = 23220;

        public const int FG_DOWNSCALE = 24040;
        public const int FG_LINE_DOWNSCALE = FG_DOWNSCALE;
        public const int FG_LINE_DOWNSCALEINIT = 24050;
        public const int FG_FLASH_POLARITY = 24060;
        public const int FG_FLASHDELAY = FG_STROBEPULSEDELAY;

        public const int FG_LOAD_SHADINGDATA = 24070;
        public const int FG_CLEAR_SHADINGDATA = 24080;


        public const int FG_LINESHADINGPARAMETER = 24081;
        public const int FG_1DSHADINGPARAMETER = FG_LINESHADINGPARAMETER;

        public const int FG_LINESHADING_SUB_ENABLE = 24082;
        public const int FG_LINESHADING_MULT_ENABLE = 24083;
        public const int FG_ENABLEDISABLE_SHADING = FG_LINESHADING_MULT_ENABLE;
        public const int FG_SHADING_WIDTH = 24089;
        public const int FG_AUTO_SHADING_WIDTH = 24090;
        public const int FG_WRITE_SHADING_12 = 24091;

        public const int FG_LINESHADING_MULT_FILENAME = 24084;
        public const int FG_LINESHADING_SUB_FILENAME = 24085;
        public const int FG_LINESHADING_LOAD_FROM_FILE = 24086;
        public const int FG_LINESHADING_MODE = 24087;

        public const int FG_DMASTATUS = 24092;
        public const int FG_LINEVALID_SIGNAL_COUNT = 24093;
        public const int FG_FRAMEVALID_SIGNAL_COUNT = 24094;

        public const int FG_1DSHADING_FILE = FG_LINESHADING_MULT_FILENAME;
        public const int FG_LOAD_1DSHADINGDATA = FG_LINESHADING_LOAD_FROM_FILE;

        public const int FG_BURSTLENGTH = 24097;
        public const int FG_SUPERFRAME = 24098;

        public const int FG_PLX_CLK = 24102;
        public const int FG_MEASURED_PCIE_CLK = 24103;
        public const int FG_FPGA_CLK = 24104;
        public const int FG_HAP_FILE = 24108;

        public const int FG_GLOBAL_ACCESS = 24110;
        public const int FG_DOC_URL = 24112;
        public const int FG_PARAM_DESCR = 24114;
        public const int FG_REG_VALUE_STRING = 24115;

        public const int FG_CAMPORT_CONFIG = 30000;
        public const int FG_CAMERA_TYPE = 30001;
        public const int FG_COLOR_FLAVOUR = 30002;

        /* defines from 200000 to 210000 are reserved for customer projects */


        public const int FG_APPLET_ID = 24010;
        public const int FG_APPLET_VERSION = 24020;
        public const int FG_APPLET_REVISION = 24030;

        public const int FG_DESIGNCLK = 24040;
        public const int FG_ALL = 24050;

        public const int FG_THRESHOLD_H_MIN = 25000;
        public const int FG_THRESHOLD_H_MAX = 25010;

        public const int FG_THRESHOLD_S_MIN = 25020;
        public const int FG_THRESHOLD_S_MAX = 25030;

        public const int FG_THRESHOLD_I_MIN = 25040;
        public const int FG_THRESHOLD_I_MAX = 25050;

        public const int FG_DO_THRESHOLD_S = 25060;
        public const int FG_DO_THRESHOLD_I = 25070;

        public const int FG_SHADING_H = 25080;
        public const int FG_SHADING_S = 25090;
        public const int FG_SHADING_I = 25100;

        public const int FG_FASTCONFIG_SEQUENCE = 30010;
        public const int FG_FASTCONFIG_PAGECMD = 30020;
        public const int FG_FASTCONFIG_PAGECMD_PTR = 30030;
        public const int FG_FASTCONFIG_PULSEDIGIO = 30040;

        public const int FG_IMG_SELECT_PERIOD = 25110;
        public const int FG_IMG_SELECT = 25111;

        public const int FG_NROFEXTERN_TRIGGER = 30110;
        public const int FG_ACTIVATE_EXTERN_TRIGGER = 30120;
        public const int FG_READ_EXTERN_TRIGGER = 30130;

        public const int FG_NB_QUAD_IMG = 30300;
        public const int FG_NB_STD_IMG = 30310;

        public const int FG_GEN_ENABLE = 30099;
        public const int FG_GEN_PASSIVE = 30100;
        public const int FG_GEN_ACTIVE = 30101;
        public const int FG_GEN_WIDTH = 30102;
        public const int FG_GEN_LINE_WIDTH = 30103;
        public const int FG_GEN_HEIGHT = 30104;
        public const int FG_GEN_START = 30113;

        public const int FG_GEN_LINE_GAP = 30105;
        public const int FG_GEN_FREQ = 30106;
        public const int FG_GEN_ACCURACY = 30107;

        public const int FG_GEN_TAP1 = 30108;
        public const int FG_GEN_TAP2 = 30109;
        public const int FG_GEN_TAP3 = 30110;
        public const int FG_GEN_TAP4 = 30111;
        public const int FG_GEN_ROLL = 30112;

        public const int FG_BOARD_INFORMATION = 42042;

        enum BOARD_INFORMATION_SELECTOR
        {
            BINFO_BOARDTYPE = 0,		/**< type of installed board (see #PN_MICROENABLE and corresponding defines in sisoboards.h) */
            BINFO_POCL = 1			/**< board supports PoCL (Power over CameraLink) */
        };

        public const int FG_CABLE_SELECT = 1001010;
        public const int FG_IMAGE_ENABLE = 1001020;
        public const int FG_STAT_ENABLE = 1001030;
        public const int FG_MIN_DX = 1001040;
        public const int FG_THR1 = 1001050;
        public const int FG_THR2 = 1001060;
        public const int FG_MEDIAN_ON = 1001070;
        public const int FG_DMA_WRITE = 1001080;
        public const int FG_FAST_CONFIG = 1001090;
        public const int FG_SYNC = 1001100;
        public const int FG_NODMA1IR = 1001110;
    }


    /*
    public class OtherDefines
    {

public const int FG_NO	=	0;
public const int FG_YES	=	1;

public const int FG_LOW	=	0;
public const int FG_HIGH		=1;

public const int	FG_FALLING=	1;
public const int	FG_RISING=	0;

public const int FG_ON	=	1;
public const int FG_OFF	=	0;
        
public const int FG_ZERO	=	0;
public const int FG_ONE	=	1;

public const int FG_APPLY=	1;

public const int FG_LEFT_ALIGNED		=	1;
public const int FG_RIGHT_ALIGNED	=	0;

public const int FG_SAVE_LUT_TO_FILE	=	1;
public const int FG_LOAD_LUT_FROM_FILE=	0;

public const int FG_0_BIT	=0;
public const int FG_1_BIT=	1;
public const int FG_2_BIT=	2;
public const int FG_3_BIT=	3;
public const int FG_4_BIT=	4;
public const int FG_5_BIT=	5;
public const int FG_6_BIT=	6;
public const int FG_7_BIT=	7;
public const int FG_8_BIT=	8;
public const int FG_9_BIT=	9;
public const int FG_10_BIT=	10;
public const int FG_11_BIT=	11;
public const int FG_12_BIT=	12;
public const int FG_13_BIT=	13;
public const int FG_14_BIT=	14;
public const int FG_15_BIT=	15;
public const int FG_16_BIT=	16;
public const int FG_17_BIT=	17;
public const int FG_18_BIT=	18;
public const int FG_19_BIT=	19;
public const int FG_20_BIT=	20;
public const int FG_21_BIT=	21;
public const int FG_22_BIT=	22;
public const int FG_23_BIT=	23;
public const int FG_24_BIT=	24;
public const int FG_25_BIT=	25;
public const int FG_26_BIT=	26;
public const int FG_27_BIT=	27;
public const int FG_28_BIT=	28;
public const int FG_29_BIT=	29;
public const int FG_30_BIT=	30;
public const int FG_31_BIT=	31;
public const int FG_32_BIT=	32;
public const int FG_36_BIT=	36;
public const int FG_48_BIT=	48;
    
public const int FG_MSB		=0;
public const int FG_LSB	=	1;
    
public const int MAX_BUF_NR =1048576;

public const int CONTMODE			=0x10;
public const int HANDSHAKEMODE	=	0x20;
public const int BLOCKINGMODE	=	HANDSHAKEMODE;
public const int PULSEMODE		=	0x30;

public const int FG_GRAY			=		3;
public const int FG_GRAY_PLUS_PICNR	=	30;
public const int FG_GRAY16			=	1;
public const int FG_GRAY16_PLUS_PICNR=	10;
public const int FG_COL24			=	2;
public const int FG_COL32			=	4;
public const int FG_COL30			=	5;
public const int FG_COL48			=	6;
public const int FG_BINARY			=	8;
public const int FG_GRAY32			=	20;

// acquire formats
public const int ACQ_STANDARD	=0x1;
public const int ACQ_BLOCK	=	0x2;
public const int ACQ_PULSE	=	0x4	;	//< deprecated, unsupported, do not use

public const int NUMBER_OF_GRABBED_IMAGES	=	10;	//**< how many frames were already transferred 
public const int NUMBER_OF_LOST_IMAGES		=	20	;//**< how many frames were lost 
public const int NUMBER_OF_BLOCK_LOST_IMAGES	=	30;	//**< how many frames were lost because all buffers were blocked 
public const int NUMBER_OF_BLOCKED_IMAGES	=	40;	//**< how many frames were blocked 
public const int NUMBER_OF_ACT_IMAGE		=	50	;//**< index of the frame transferred last 
public const int NUMBER_OF_LAST_IMAGE	=		60;	//**< index of the frame requested last 
public const int NUMBER_OF_NEXT_IMAGE	=		70	;//**< index of the frame following the last requested one 
public const int NUMBER_OF_IMAGES_IN_PROGRESS	=	80	;//**< how many frames were transferred but not yet requested 

public const int BUFFER_STATUS		=			90;
public const int GRAB_ACTIVE			=		100;	//**< state of transfer (running, stopped) 

public const int FG_REVNR			=			99;

public const int FG_BLOCK				=		0x100;
public const int FG_UNBLOCK				=		0x200;
public const int FG_UNBLOCK_ALL			=		0x220;
public const int FG_PULSE_NEXT_BUFFER	=		0x230;		//**< deprecated, unsupported, do not use 

public const int SEL_ACT_IMAGE	=	200;
public const int SEL_LAST_IMAGE	=	210;
public const int SEL_NEXT_IMAGE	=	220;
public const int SEL_NUMBER		=	230;
public const int SEL_NEW_IMAGE	=	240;
    
// LUT defines
public const int LUT_RED		=	0;
public const int LUT_GREEN 	=	1;
public const int LUT_BLUE	=	2;
public const int LUT_GRAY 	=	3;

public const int PORT_A		=0;
public const int PORT_B		=1;
public const int PORT_C		=2;
public const int PORT_D		=3;
public const int PORT_AB		=4;

public const int FG_RED		=0;
public const int FG_GREEN	=1;
public const int FG_BLUE		=2;

public const int TRGINSOFTWARE =-1;
public const int TRGINSRC_0=	0;
public const int TRGINSRC_1	=1;
public const int TRGINSRC_2=	2;
public const int TRGINSRC_3=	3;
public const int TRGINSRC_4	=4;
public const int TRGINSRC_5	=5;
public const int TRGINSRC_6=	6;
public const int TRGINSRC_7	=7;

#if _WIN64
    public const long GRAB_INFINITE= -1;
    public const long GRAB_ALL_BUFFERS= -2;
#else
    public const int GRAB_INFINITE = -1;
    public const int GRAB_ALL_BUFFERS = -2;
#endif

//public const int STOP_SYNC	= -2147483648;
//public const int STOP_ASYNC =	0;

// Error codes
public const int		FG_INIT_OK		=				1;
public const int		FG_OK 			=				0;
public const int		FG_ERROR 		=				-1;

public const int		FG_DUMMY_BUFFER		=			-1;
public const int		FG_NO_PICTURE_AVAILABLE 	=	-2;

public const int     FG_ALR_INIT 				=		-10;
public const int     FG_NOT_AVAILABLE 				=	-12;
public const int     FG_NO_BOARD_AVAILABLE 			=	-20;
public const int     FG_INVALID_BOARD_NUMBER 		=	-21;
public const int		FG_BOARD_INIT_FAILED			=	-22;
public const int		FG_INVALID_CLOCK				=	-23;
public const int		FG_INVALID_DESIGN_NAME			=	-26;
public const int		FG_SYSTEM_LOCKED				=	-27;
public const int		FG_RESSOURCES_STILL_IN_USE		=	-28;
public const int		FG_CLOCK_INIT_FAILED			=	-29;
public const int     FG_WRONG_ARCHITECTURE 			=	-50;
public const int     FG_SOFTWARE_TRIGGER_BUSY		=	-60;
public const int     FG_INVALID_PORT_NUMBER 			=	-61;
public const int     FG_HAP_FILE_NOT_LOAD			=	-100;

public const int     FG_MICROENABLE_NOT_INIT			=	-110;
public const int     FG_DLL_NOT_LOAD					=	-120;
public const int		FG_REG_KEY_NOT_FOUND			=	-121;
public const int     FG_VASDLL_NOT_LOAD				=	-122;
public const int     FG_SIZE_ERROR 					=	-200;
public const int     FG_PTR_INVALID 					=	-300;
public const int     FG_RANGE_ERR 					=	-400;
public const int     FG_NOT_ENOUGH_MEM				=	-500;
public const int     FG_DMATRANSFER_INVALID			=	-600;
public const int     FG_HAP_FILE_DONT_MATCH			=	-700;
public const int     FG_VERSION_MISMATCH				=	-701;

public const int     FG_NOT_INIT 					=	-2001;
public const int     FG_WRONG_SIZE 					=	-2002;
public const int     FG_WRONG_NUMBER_OF_BUFFER 		=	-2010;
public const int     FG_TOO_MANY_BUFFER 				=	-2011;
public const int     FG_NOT_ENOUGH_MEMORY 			=	-2020;
public const int     FG_MEMORY_ALREADY_ALLOCATED		=	-2024;
public const int		FG_CANNOT_WRITE_MEM_CONFIG_FAILED=	-2026;
public const int     FG_INTERNAL_STATUS_ERROR 			=-2030;
public const int     FG_INTERNAL_ERROR 				=	-2031;
public const int     FG_CANNOT_START					=	-2040;
public const int     FG_CANNOT_STOP 					=	-2042;
public const int     FG_SYNC_ACQUIRE_NOT_SUPPORTED	=	-2045;
public const int     FG_INVALID_DESIGN 				=	-2050;
public const int     FG_CONFIGURE_FAILED 			=	-2052;
public const int     FG_RECONFIGURE_FAILED 			=	-2053;
public const int		FG_NO_APPLET_ID 				=	-2055;

public const int     FG_INVALID_MEMORY 				=	-2060;
public const int     FG_INVALID_PARAMETER 			=	-2070;
public const int     FG_ILLEGAL_WHILE_APC			=	-2071;

public const int		FG_INVALID_VALUE				=	-2075;
public const int		FG_INVALID_FILENAME				=	-2076;
public const int		FG_INVALID_REGISTER				=	-7040;
public const int		FG_INVALID_MODULO				=	-7080;
public const int		FG_INVALID_CONFIGFILE			=	-5000;
public const int		FG_INVALID_CONFIGFILEEXT		=	FG_INVALID_CONFIGFILE;

public const int     FG_NOT_LOAD 					=	-2080;
public const int     FG_ALREADY_STARTED 				=	-2090;
public const int     FG_STILL_ACTIVE 				=	-2100;
public const int     FG_NO_VALID_DESIGN 				=	-2110;
public const int     FG_TIMEOUT_ERR 					=	-2120;
public const int     FG_NOT_IMPLEMENTED 				=	-2130;
public const int     FG_NOT_WRONG_TRIGGER_MODE		=	-2140;
public const int     FG_ALL_BUFFER_BLOCKED			=	-2150;
public const int     FG_CANNOT_INIT_MICROENABLE 		=	-3000;
public const int     FG_TRANSFER_NOT_ACTIVE 			=	-3010;
public const int     FG_CLOCK_NOT_LOCKED				=	-3120;
public const int     FG_STILL_NOT_STARTED 			=	-4000;
public const int		FG_VALUE_OUT_OF_RANGE			=	-6000;
public const int		FG_CANNOT_CHANGE_DISPLAY_WIDTH	=	-7000;
public const int		FG_CANNOT_CHANGE_DISPLAY_HEIGHT	=	-7005;
public const int		FG_CANNOT_CHANGE_DISPLAY_SIZE	=	-7010;
public const int     FG_NO_VALID_LICENSE 			=	-7020;
public const int		FG_CANNOT_CHANGE_CAMERA_FORMAT	=	-7030;
public const int		FG_REGISTER_INIT_FAILED			=	-7050;
public const int		FG_INVALID_SHADING_CORRECTION_FILE=	-7060;
public const int		FG_WRITE_LINE_SHADING_TIMEOUT	=	-7070;
public const int     FG_CANNOT_CHANGE_DURING_ACQU	=	-7090;
public const int		FG_TOKEN_NOT_FOUND_ERROR		=	-8000;
public const int     FG_WRITE_ACCESS_DENIED			=	-8010;
public const int     FG_REGISTER_UPDATE_FAILED		=	-8020;

public const int SINGLE_AREA_GRAY                     =   0x10;
public const int SINGLE_AREA_2DSHADING        =   0x11;
public const int DUAL_AREA_GRAY                =          0x20;
public const int SINGLE_AREA_BAYER              =         0x30;
public const int DUAL_AREA_BAYER                 =        0x31;
public const int SINGLE_AREA_GRAY_SHADING     =   0x40;
public const int SDRAM_ACCESS                  =          0x41;
public const int SINGLE_LINE_GRAY               =         0x50;
public const int SINGLE_LINE_RGB                 =        0x60;
public const int DUAL_LINE_RGB                    =       0x61;
public const int DUAL_LINE_RGB_SHADING         =  0x62;
public const int DUAL_LINE_GRAY                 =         0x70;
public const int VISIGLAS                        =                0x80;
public const int TRUMPFINESS                      =               0x81;
public const int SOUDRONIC                         =              0x82;
public const int SINGLEHIGHPRECISION                =     0x83;
public const int SINGLE_AREA_GRAY_OFFSET        = 0x84;
public const int SINGLE_AREA_HSI                 =        0x90;
public const int SINGLE_AREA_RGB                  =       0xa0;
public const int DUAL_AREA_RGB                     =      0xb0;
public const int SINGLE_AREA_RGB_SEPARATION      =0xb1;
public const int MEDIUM_LINE_RGB                  =       0xb2;
public const int MEDIUM_LINE_GRAY                  =      0xb3;
public const int SINGLE_FAST_CONFIG                 =     0xb5;
public const int FASTCONFIG_SINGLE_AREA_GRAY =SINGLE_FAST_CONFIG;

public const int SINGLE_AREA_GRAY_XXL         =   0x110;
public const int SINGLE_AREA_2DSHADING_XXL     =  0x111;
public const int DUAL_AREA_GRAY_XXL             =         0x120;
public const int SINGLE_AREA_BAYER_XXL           =0x130;
public const int DUAL_AREA_BAYER_XXL              =      0x131;
public const int SINGLE_AREA_GRAY_SHADING_XXL =0x140;
public const int SDRAM_ACCESS_XXL             =           0x141;
public const int SINGLE_LINE_GRAY_XXL        =    0x150;
public const int SINGLE_LINE_RGB_XXL        =             0x160;
public const int DUAL_LINE_RGB_XXL           =            0x161;
public const int DUAL_LINE_RGB_SHADING_XXL   =    0x162;
public const int DUAL_LINE_GRAY_XXL          =            0x170;
public const int SINGLE_AREA_HSI_XXL         =            0x190;
public const int SINGLE_AREA_RGB_XXL         =            0x1a0;
public const int DUAL_AREA_RGB_XXL           =                    0x1b0;
public const int SINGLE_AREA_RGB_SEPARATION_XXL = 0x1b1;
public const int MEDIUM_LINE_RGB_XXL        =             0x1b2;
public const int MEDIUM_LINE_GRAY_XXL       =     0x1b3;
public const int MEDIUM_AREA_GRAY_XXL       =     0x1b4;
public const int MEDIUM_AREA_RGB_XXL        =             0x1b5;
public const int SINGLE_AREA_BAYER12_XXL     =    0x1c0;
public const int DUAL_AREA_GRAY12_XXL       =     0x1d0;
public const int SINGLE_LINE_GRAY12_XXL     =     0x1d1;
public const int DUAL_AREA_RGB36_XXL        =             0x1d2;
public const int DUAL_LINE_GRAY12_XXL      =      0x1d3;
public const int MEDIUM_LINE_GRAY12_XXL    =      0x1d4;
public const int SINGLE_AREA_GRAY12_XXL      =    0x1d5;
public const int DUAL_LINE_RGB36_XXL         =            0x1d6;
public const int SINGLE_AREA_RGB36_XXL       =    0x1d7;
public const int SINGLE_LINE_RGB36_XXL       =    0x1d8;
public const int DUAL_AREA_BAYER12_XXL        =   0x1d9;
public const int SINGLE_AREA_2DSHADING12_XXL  =   0x1da;
public const int SINGLE_LINE_RGB24_XXL        =   0x1db;

public const int LSC1020XXL                      =                0x500;
public const int LSC1020JPGXXL                  =         0x501;
public const int CLSC2050                       =                 0x502;
public const int CLSC2050JPGXXL                 =         0x503;
public const int SEQUENCE_EXTRACTOR             =         0x510;
public const int SAG_COMPRESSION                =         0x520;
public const int MEDIUM_LINE_GRAY_FIR_XXL      =  0x530;
public const int DUAL_LINE_RGB_SORTING_XXL    =   0x540;
public const int SINGLE_LINE_GRAY_2X12_XXL   =    0x550;
public const int MEDIUM_LINE_GRAY12             =         0x560;
public const int SINGLE_LINE_RGB36PIPELINE2_XXL  =        0x570;
public const int DUAL_AREA_GRAY_16               =        0x580;


public const int DUAL_AREA_GRAY16_ME4BASEX1	=	0xa400010;
public const int DUAL_AREA_RGB48_ME4BASEX1	=	0xa400020;
public const int DUAL_LINE_GRAY16_ME4BASEX1	=	0xa400030;
public const int DUAL_LINE_RGB48_ME4BASEX1	=	0xa400040;
public const int MEDIUM_AREA_GRAY16_ME4BASEX1=	0xa400050;
public const int MEDIUM_AREA_RGB36_ME4BASEX1	=	0xa400060;
public const int MEDIUM_LINE_GRAY16_ME4BASEX1=	0xa400070;
public const int MEDIUM_LINE_RGB36_ME4BASEX1	=	0xa400080;


public const int DUAL_AREA_BAYER12_ME4FULLX1	=	0xa410010;
public const int DUAL_AREA_GRAY16_ME4FULLX1	=	0xa410020;
public const int DUAL_AREA_RGB48_ME4FULLX1	=	0xa410030;
public const int DUAL_LINE_GRAY16_ME4FULLX1	=	0xa410040;
public const int DUAL_LINE_RGB30_ME4FULLX1	=	0xa410050;
public const int FULL_AREA_GRAY8_ME4FULLX1	=	0xa410060;
public const int FULL_LINE_GRAY8_ME4FULLX1	=	0xa410070;
public const int MEDIUM_AREA_GRAY16_ME4FULLX1=	0xa410080;
public const int MEDIUM_AREA_RGB36_ME4FULLX1	=	0xa410090;
public const int MEDIUM_LINE_GRAY16_ME4FULLX1=	0xa4100a0;
public const int MEDIUM_LINE_RGB36_ME4FULLX1	=	0xa4100b0;
public const int SINGLE_AREA_BAYERHQ_ME4FULLX1=	0xa4100c0;
public const int SINGLE_AREA_GRAY2DSHADING_ME4FULLX1=	0xa4100d0;

public const int DUAL_AREA_BAYER12_ME4FULLX4		=0xa440010;
public const int DUAL_AREA_GRAY16_ME4FULLX4	=	0xa440020;
public const int DUAL_AREA_RGB48_ME4FULLX4	=	0xa440030;
public const int DUAL_LINE_GRAY16_ME4FULLX4	=	0xa440040;
public const int DUAL_LINE_RGB30_ME4FULLX4	=	0xa440050;
public const int FULL_AREA_GRAY8_ME4FULLX4	=	0xa440060;
public const int FULL_LINE_GRAY8_ME4FULLX4	=	0xa440070;
public const int MEDIUM_AREA_GRAY16_ME4FULLX4=	0xa440080;
public const int MEDIUM_AREA_RGB36_ME4FULLX4	=	0xa440090;
public const int MEDIUM_LINE_GRAY16_ME4FULLX4=	0xa4400a0;
public const int MEDIUM_LINE_RGB36_ME4FULLX4	=	0xa4400b0;
public const int SINGLE_AREA_BAYERHQ_ME4FULLX4 =	0xa4400c0;
public const int SINGLE_AREA_GRAY2DSHADING_ME4FULLX4=	0xa4400d0;

public const int QUAD_AREA_BAYER24_ME4GBEX4	=	0xe440010;
public const int QUAD_AREA_GRAY16_ME4GBEX4	=	0xe440020;
public const int	QUAD_AREA_RG24_ME4GBEX4		=	0xe440030;
public const int QUAD_AREA_RGB48_ME4GBEX4	=	0xe440040;
public const int QUAD_AREA_GRAY8_ME4GBEX4	=	0xe440050;
public const int QUAD_LINE_GRAY16_ME4GBEX4	=	0xe440060;
public const int QUAD_LINE_RGB24_ME4GBEX4	=	0xe440070;
public const int QUAD_LINE_GRAY8_ME4GBEX4	=	0xe440080;
    
//public const int FULL_AREA_GRAY8_HS_ME4VD4	=	((PN_MICROENABLE4VD4CL << 16) + 0x00e0);
//public const int FULL_AREA_GRAY8_HS_ME4AD4	=	((PN_MICROENABLE4AD4CL << 16) + 0x00e0);

    }
     */
}

