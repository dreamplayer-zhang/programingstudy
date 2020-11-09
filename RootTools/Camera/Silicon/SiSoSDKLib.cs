using System;
using System.Runtime.InteropServices;


/**
 * \brief	namespace of native SiSo SDK
 *
 *          purpose: 
 *          - interface to the SiSo SDJ Library (fglib)
 *          - low level interface definitions
 *          - linkage of SDK C-functions inside the DLL to C#
 *          - take aware of target operating System Win32/Win64 due to the
 *            used Versions of Siso SDK: 
 *            define _WIN64 for 64 bit version of Siso SDK in order to get correct 
 *            data types 
 *          
 *          caution:
 *          - all functions use the calling convention cdecl
 *          
 *          for detailed description see also the SiSi SDK function list
 *          
 *         CAUTION: plattform dependiciese due to different DLL interface of 32 and 64 Bit FGLIB.DLL
 *         #define _WIN64 to use the 64 Bit FGLIB.DLL
 *         
 *          
 *  Mapping of data types
 *  
 *  SiSo-C        -->  C#
 *  frameindex_t     int     (32 bit) on Win32 
 *                   long    (64 Bit) on Win64
 *  size_t           uint    (32 Bit) on Win32
 *                   ulong   (64 Bit) on Win64 
 *  
 */
namespace SisoSDKLib
{

        /**
        * \brief	flags for SDK-Function Fg_Acquire
        */
        public enum FgAcquisitionFlags
        {
            ACQ_STANDARD	= 0x1,
            ACQ_BLOCK		= 0x2,
            ACQ_PULSE       = 0x4
        }

        /**
        * \brief	defintions / datatypes for fg_get/setParameter interface
        */
        public enum FgParamTypes
        {
            FG_PARAM_TYPE_INVALID = 0x0,/**< will always cause an error, for internal use only */
            FG_PARAM_TYPE_INT32_T = 0x1,/**< signed 32 bit integer (int, int32_t) */
            FG_PARAM_TYPE_UINT32_T = 0x2,/**< unsigned 32 bit integer (unsigned int, uint32_t) */
            FG_PARAM_TYPE_INT64_T = 0x3,/**< signed 64 bit integer (long long, int64_t) */
            FG_PARAM_TYPE_UINT64_T = 0x4,/**< unsigned 64 bit integer (unsigned long long, uint64_t) */
            FG_PARAM_TYPE_DOUBLE = 0x5,/**< double / floating point data */
            FG_PARAM_TYPE_CHAR_PTR = 0x6,/**< char* */
            FG_PARAM_TYPE_SIZE_T = 0x7,	/**< size_t C-data type*/
            FG_PARAM_TYPE_STRUCT_FIELDPARAMACCESS = 0x1000,	/**< struct FieldParameterAccess */
            FG_PARAM_TYPE_STRUCT_FIELDPARAMINT = 0x1002,	/**< struct FieldParameterInt_s */
            FG_PARAM_TYPE_STRUCT_FIELDPARAMDOUBLE = 0x1005,	/**< struct FieldParameterDouble_s */
            FG_PARAM_TYPE_AUTO = -1	/**< will always cause an error, for SiSO internal use only */
        }

        /**
        * \brief constants to indicat, which field to be queried
        */
        public enum Fg_Info_Selector
        {
            INFO_APPLET_CAPABILITY_TAGS = 1,			/**< list of key value pairs describing the features supported by the applet */
            INFO_NR_OF_BOARDS = 1000,					/**< get the number of framegrabbers in the system */
            INFO_BOARDNAME = 1010,						/**< get the typename of the according board identified by param1*/
            INFO_BOARDTYPE = 1011,						/**< get the type of the according board identified by param1*/
            INFO_BOARDSERIALNO = 1012,					/**< get the serialNo. of the according board identified by param1*/
            INFO_DRIVERVERSION = 1100					/**< get the driver version used for the according board identified by param1*/
        };

        /**
         * \brief defintions for querying information about a certain field
         */
        public enum FgProperty
        {
	        PROP_ID_VALUE = 0,			/**< request the value of a parameter, information,..*/
	        PROP_ID_DATATYPE = 1,		/**< request the datatype of a parameter, information, */
	        PROP_ID_NAME = 2,			/**< request the symbolic name of a parameter, information, */
	        PROP_ID_PARAMETERNAME = 3,	/**< request the Name for Software access */
	        PROP_ID_VALUELLEN = 4		/**< request the current length of data */
        }

        /**
        * \brief	Possible selection fpr Get/SetStatus methods
        */
        public enum Fg_Status_Selector
        {
            NUMBER_OF_GRABBED_IMAGES = 10,      /**< Sum of all acquired images. The parameter data will be ignored.*/
            NUMBER_OF_LOST_IMAGES = 20,         /**<  Sum of lost images. The parameter data requires the DMA no.*/
            NUMBER_OF_BLOCK_LOST_IMAGES = 30,   /**< Sum of all images, which are lost by blocking of the frame buffer. The parameter data requires the DMA no.*/
            NUMBER_OF_BLOCKED_IMAGES = 40,      /**< Sum of blocked images. The parameter data will be ignored.*/
            NUMBER_OF_ACT_IMAGE = 50,           /**< Number of last acquired image. The parameter data requires the timeout value.*/
            NUMBER_OF_LAST_IMAGE = 60,          /**< Number of last get image. The parameter data will be ignored.*/
            NUMBER_OF_NEXT_IMAGE = 70,          /**< Number of next image after last get image. The parameter data will be ignored.*/
            NUMBER_OF_IMAGES_IN_PROGRESS = 80,  /**<Sum of all images which aren't gotten, yet. The parameter data will be ignored.*/
            BUFFER_STATUS = 90,                 /**< 1 if the buffer is locked, otherwise 0. The parameter data will be ignored.*/
            GRAB_ACTIVE_X = 100                 /**<*/
        }
        /////////////////////////////////////////////////////////////////////////////////
        // datatype
        /////////////////////////////////////////////////////////////////////////////////

        /**
        * \brief	transfer-structure for parameters in form of an integer-array/field
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FieldParameterInt
        {
            public uint value;     /**< Value of the field as integer*/
            public uint index;     /**< field index to be set / read */
        }

        /**
        * \brief	transfer-structure for parameters in form of an double-array/field
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FieldParameterDouble
        {
        	public double value;/**< Value of the field as double*/
            public uint index;/**< field index to be set / read */
        }



        /**
         * \brief	nested transfer-structure for parameters in form of an pointers
         */
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct FieldParameterAccessUnion
        {
            [FieldOffset(0)]
            public IntPtr p_int32_t;	/**< int* a range of signed 32 bit integer values */
            [FieldOffset(0)]
            public void* p_uint32_t;	/**< uint* a range of signed 32 bit integer values */
            [FieldOffset(0)]
            public IntPtr p_int64_t;    /**< long* a range of signed 64 bit integer values */
            [FieldOffset(0)]
            public IntPtr p_uint64_t;   /**< ulong* a range of unsigned 64 bit integer v*/
        }

        /**
         * \brief	transfer-structure for parameters in for certain fields
         */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FieldParameterAccess
        {
            public FgParamTypes vtype;  /**< gives the type of the included data */
            public uint index;          /**< gives the first index in the range */
            public uint count;          /**< gives the first index in the range */
            public IntPtr pData;        /**< replaces the SDK-union type of diffent Ptr. to ease the handling */
        }


        /////////////////////////////////////////////////////////////////////////////////
        // definitions for APC
        /////////////////////////////////////////////////////////////////////////////////
        /**
        * \brief	delegate / callback type for usage of APC
        */
        public enum FgApcControlCtrlFlags
        {
            FG_APC_DEFAULTS = 0,			/**< use APC default mode (handler is called for every transfer, grabbing is stopped on timeout) */
            FG_APC_BATCH_FRAMES = 0x1,		/**< when multiple images arrived only call APC handler once with the highest image number */
            FG_APC_IGNORE_TIMEOUTS = 0x2,	/**< ignore image timeouts */
            FG_APC_IGNORE_APCFUNC_RETURN = 0x4,	/**< do not stop acquisition if apcfunc returns != 0 */
            FG_APC_IGNORE_STOP = 0x8,		/**< do not stop the APC handler if acquisition stops */
            FG_APC_HIGH_PRIORITY = 0x10		/**< increase the priority of the APC thread */
        }

        /**
        * \brief	delegate / callback type for usage of APC
        */
        // APC Callback Function
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Fg_ApcFunc_t(   
#if _WIN64
                                            long imgNr, //frameindex_t
#else
                                            int imgNr, //frameindex_t
#endif

                                            IntPtr/*fg_apc_data*/ data);


        /**
        * \brief	defintions for controlling APC behaviour
        */
        public enum FgApcControlFlags
        {
            FG_APC_CONTROL_BASIC = 0		/**< default behaviour */
        }

        /**
        * \brief	placeholder, not necessarily to use in C#
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct fg_apc_data
        {
        }

        /**
        * \brief	setup-structure for using APC
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FgApcControl
        {
            public uint version;
            //public unsafe Fg_ApcFunc_t func;
            public IntPtr func; /**<Callback function, type: Fg_ApcFunc_t */
            public IntPtr data; /**< Placeholder for user defined Data : fg_apc_data */
            public uint timeout;/**< timeout for acquisition */
            public uint flags;  /**< FgApcControlCtrl: pls. refer to SDK function list*/
        }


        /////////////////////////////////////////////////////////////////////////////////
        // definitions for event system (starting at RT 5.2)
        /////////////////////////////////////////////////////////////////////////////////


        /**
         * \brief controls the behaviour of Fg_waitEvent() 
         */
        public enum FgEventControlFlags
        {
            FG_EVENT_DEFAULT_FLAGS = 0,	/**< default behaviour (i.e. none of the other flags) */
            FG_EVENT_BATCHED = 0x1		/**< if more than one event is recorded only return once */
        };

        /**
         * \brief the status flags passed in notify member of struct fg_event_info
         */
        public enum FgEventNotifiers
        {
            FG_EVENT_NOTIFY_JOINED = 0x1,		/**< events were joined in mode FG_EVENT_BATCHED */
            FG_EVENT_NOTIFY_TIMESTAMP = 0x2,	/**< timestamp contains valid data */
            FG_EVENT_NOTIFY_PAYLOAD = 0x4,		/**< the event has payload, use the payload member of the union */
            FG_EVENT_NOTIFY_LOST = 0x8		    /**< there was data lost before this event */
        };

        /**
        * \brief	delegate / callback for asynchronous event handling
        */
        // Event-Callback function
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int Fg_EventFunc_t(ulong events, IntPtr pData, IntPtr/*fg_event_info**/ pInfo);
        //
        /**
        * \brief	nested type for event data
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public unsafe struct payload
        {
            int length;
            fixed short data[254];
        }

        /**
        * \brief	type definition of a placeholder for conversion to C#
        */
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public unsafe struct SiSoEventData
        {
            public fixed ulong timestamp[64];
            public payload PayloadData;
        }

        //----------------------------------------------------------------
        /**
        * \brief	data structure for transport of event information
        */
        //----------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public unsafe struct fg_event_info
        {
            public uint version;		/**< version of this struct. Do not set directly, use FG_EVENT_INFO_INIT() */
            public uint _pad;		    /**< currently unused, inserted to allow 64 bit aligning of following fields */
//            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public fixed uint notify[64];	/**< see enum FgEventNotifiers for the meaning of the bits */
            public fixed ulong timestamp[64];
            public uint length;
        	public fixed short  data[254];		/**< payload data of the event */

        }


    // ------------------------------------------------------------------
    // Mapping of C - data types
    //  unsigned int                    uint    (32 Bit)
    //  char* [in]                      string
    //  uint64_t                        ulong   (64 Bit Integer)
    //  uint32_t                        uint    (32 bit Integer)
    //  void*                           IntPtr  (unmanaged)
    //  int*                            int*    (unsafed - fixed)
    //  double*                         double* (unsafed - fixed)
    //  char*  [out]                    IntPtr
    // ------------------------------------------------------------------
    // Mapping of SiSo defined data types
    //  Fg_Struct                       IntPtr  (unmanaged)         // Handle for GrabberControl
    //  dmamem*                         IntPtr  (unmanaged)         // Handle to memory
    //  enum Fg_Info_String_Selector    enum ...                    //
    //  ShadingMaster*                  IntPtr                      // Handle for Shading Control
    // ------------------------------------------------------------------

    /**
    * \brief	class for the SiSo DLL interface (FGLIB.DLL)
    * 
    */
    public class SDKLib
    {
        /**
        * \brief	Name of the DLL to be used
        * can bei done by either using the system enverionment variable SISODIR5 or giving an explicit path
        */
        public const String FGLIBNAME_C = "fglib5.dll";
        public SDKLib()
        {
        }
        //--------------------------------------------------------------------
        // Funktionen zur Initialisierung und Memory Management
        // functions for initialization and memory management
        //--------------------------------------------------------------------
        /**
        * \brief	Create Board-Handler and Initialilze Board
        *  Initialization of the board identified by BoardIndex
        *  Loading of the according applet identified by AppletFileName
        */
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_Init(String AppletFilename, uint BoardIndex);

        // Create Board-Handler and Initialilze Board in Master/Slave Mode
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_InitEx(String AppletFileName, uint BoardIndex, int isSlave);

        // Release Board-handler
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_FreeGrabber(IntPtr Fg);

        // Create Board-Handler and Initialilze Board according to MCF File settings
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_InitConfig(String MCFConfigFileName, uint BoardIndex);


        //--------------------------------------------------------------------
        // Funktionen zur Speicherverwaltung der Bilddaten
        // functions for memory management of frames
        //--------------------------------------------------------------------
        //Allocate Memory for Frames
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMemEx")]
        public unsafe static extern IntPtr Fg_AllocMemEx(   IntPtr Fg, 
#if _WIN64
                                                            ulong Size,  // size_t
                                                            long BufCnt //frameindex_t
#else
                                                            uint Size, // size_t
                                                            int BufCnt //frameindex_t
#endif
                                                            );

        //Allocate Memory for Frames (only at 64 Bit SiSoRuntime)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMemEx")]
        public unsafe static extern IntPtr Fg_AllocMemEx64(IntPtr Fg,
                                                            ulong Size,  // size_t
                                                            long BufCnt //frameindex_t
                                                            );
        //Allocate Memory for Frames (only at 32 Bit SiSoRuntime)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMemEx")]
        public unsafe static extern IntPtr Fg_AllocMemEx32(IntPtr Fg,
                                                            uint Size, // size_t
                                                            int BufCnt //frameindex_t
                                                            );


        //Allocate Memory for Frames
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_AllocMem(    IntPtr Fg, 
#if _WIN64
                                                    ulong Size,  // size_t
                                                    long BufCnt, //frameindex_t
#else
                                                    uint Size, // size_t
                                                    int BufCnt, //frameindex_t
#endif
                                                    uint DmaIndex);

        //Allocate Memory for Frames
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMem")]
        public static extern IntPtr Fg_AllocMem64(IntPtr Fg,
                                                    ulong Size,  // size_t
                                                    long BufCnt, //frameindex_t
                                                    uint DmaIndex);

        //Allocate Memory for Frames
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint="Fg_AllocMem")]
        public static extern IntPtr Fg_AllocMem32(    IntPtr Fg,
                                                    uint Size, // size_t
                                                    int BufCnt, //frameindex_t
                                                    uint DmaIndex);


        // Create Control structures for usage of client/user side allocated memory
        // Memory has to be allcocated in unsafe mode
        // return: dma_mem *
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_AllocMemHead(IntPtr Fg, 
#if _WIN64
                                                    ulong Size, 
                                                    long BufCnt //frameindex_t
#else
                                                    uint Size, 
                                                    int BufCnt //frameindex_t
#endif
                                                    );

        // Create Control structures for usage of client/user side allocated memory
        // Memory has to be allcocated in unsafe mode
        // return: dma_mem *
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMemHead")]
        public static extern IntPtr Fg_AllocMemHead64(  IntPtr Fg,
                                                        ulong Size, 
                                                        long BufCnt //frameindex_t
                                                    );

        // Create Control structures for usage of client/user side allocated memory
        // Memory has to be allcocated in unsafe mode
        // return: dma_mem *
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AllocMemHead")]
        public static extern IntPtr Fg_AllocMemHead32(  IntPtr Fg,
                                                        uint Size,
                                                        int BufCnt //frameindex_t
                                                    );

        // release memory allocated by using fg_allocmem
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_FreeMem(IntPtr Fg, uint DmaIndex);

        // release memory allocated by using fg_allocmemex
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_FreeMemEx(IntPtr Fg, IntPtr mem);

        // release control structures for administatron of user allocated memory
        // dma_mem *
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_FreeMemHead(IntPtr Fg, IntPtr memHandle);

        // Add user side allocated memory to administrativ control structures
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AddMem")]
        public static extern int Fg_AddMem( IntPtr Fg, 
                                            IntPtr pBuffer, 
#if _WIN64
                                            ulong Size, // size_t
                                            long BufferIndex, //frameindex_t
#else
                                            uint Size,  // size_t
                                            int BufferIndex, //frameindex_t
#endif
                                            IntPtr memHandle);
        // Add user side allocated memory to administrativ control structures
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AddMem")]
        public static extern int Fg_AddMem64(IntPtr Fg,
                                            IntPtr pBuffer,
                                            ulong Size, // size_t
                                            long BufferIndex, //frameindex_t
                                            IntPtr memHandle);
        // Add user side allocated memory to administrativ control structures
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AddMem")]
        public static extern int Fg_AddMem32(IntPtr Fg,
                                             IntPtr pBuffer,
                                             uint Size,  // size_t
                                             int BufferIndex, //frameindex_t
                                             IntPtr memHandle);


        // removes the memory from the system
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_DelMem")]
        public static extern int Fg_DelMem( IntPtr Fg, 
                                            IntPtr memHandle, 
#if _WIN64
                                            long BufferIndex //frameindex_t
#else
                                            int BufferIndex //frameindex_t
#endif
                                            );
        // removes the memory from the system
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_DelMem")]
        public static extern int Fg_DelMem64(IntPtr Fg,
                                            IntPtr memHandle,
                                            long BufferIndex //frameindex_t
                                            );
        // removes the memory from the system
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_DelMem")]
        public static extern int Fg_DelMem32(   IntPtr Fg,
                                                IntPtr memHandle,
                                                int BufferIndex //frameindex_t
                                            );

        //--------------------------------------------------------------------
        // Funktionen zur Paramtrisierung
        // functions for parametrization
        //--------------------------------------------------------------------
        // retrieve the no. of Applet Parameters
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getNrOfParameter(IntPtr Fg);

        // get the name of an parameter according to the parameter-Index
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterName")]
        public static extern IntPtr Fg_getParameterName(IntPtr Fg, int index);
        

        // retrive the ID for the parameter acc. to parameter's name
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getParameterIdByName(IntPtr Fg, string ParameterName);

        // set the value of an parameter
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int Fg_setParameter(IntPtr Fg, int Parameter, /*IntPtr*/ int* pValue, uint DmaIndex);
        // under construction
        // set the value of an parameter and checks, if operators parameter type matches top the expected type (FgParamTypes) )
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setParameterWithType")]
        public unsafe static extern int Fg_setParameterWithType(IntPtr Fg, int Parameter, ref  FieldParameterAccess Value, uint DmaIndex, FgParamTypes type);
        // set the value of an parameter and checks, if operators parameter type matches top the expected type (FgParamTypes) )
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setParameterWithType")]
        public unsafe static extern int Fg_setParameterWithType(IntPtr Fg, int Parameter, void* Value, uint DmaIndex, FgParamTypes type);
        // set the value of an parameter and checks, if operators parameter type matches top the expected type (FgParamTypes) )
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setParameterWithType")]
        public unsafe static extern int Fg_setParameterWithType(IntPtr Fg, int Parameter, IntPtr Value, uint DmaIndex, FgParamTypes type);
        // under construction - end

        // retrieve the value of the parameter
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int Fg_getParameter(IntPtr Fg, int ParameterID, void* pValue, uint DmaIndex);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int Fg_getParameter(IntPtr Fg, int ParameterID, IntPtr pValue, uint DmaIndex);

        // getting the value by declaring the data type, using a check for matching parameters
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterWithType")]
        public unsafe static extern int Fg_getParameterWithType(IntPtr Fg, int Parameter, void* Value, uint DmaIndex, FgParamTypes type);
        // getting the value by declaring the data type, using a check for matching parameters
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterWithType")]
        public unsafe static extern int Fg_getParameterWithType(IntPtr Fg, int Parameter, IntPtr Value, uint DmaIndex, FgParamTypes type);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        // dma_mem* - IntPtr
        // frameindex_t - long (int64)
        public unsafe static extern int Fg_getParameterEx(  IntPtr Fg, 
                                                            int Parameter,
                                                            void*/*IntPtr */Value, 
                                                            uint DmaIndex, 
                                                            IntPtr pDMAMem, 
#if _WIN64
                                                            long PicNr //frameindex_t
#else
                                                            int PicNr //frameindex_t
#endif
                                                            );

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint="Fg_getParameterEx")]
        // dma_mem* - IntPtr
        // frameindex_t - long (int64)
        public unsafe static extern int Fg_getParameterEx64(IntPtr Fg,
                                                            int Parameter,
                                                            void*/*IntPtr */Value,
                                                            uint DmaIndex,
                                                            IntPtr pDMAMem,
                                                            long PicNr //frameindex_t
                                                            );
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterEx")]
        // dma_mem* - IntPtr
        // frameindex_t - long (int64)
        public unsafe static extern int Fg_getParameterEx32(IntPtr Fg,
                                                            int Parameter,
                                                            void*/*IntPtr */Value,
                                                            uint DmaIndex,
                                                            IntPtr pDMAMem,
                                                            int PicNr //frameindex_t
                                                            );

        // Get the whole parameter information as genicam-xml 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterInfoXML")]
        public unsafe static extern int Fg_getParameterInfoXML( IntPtr Fg, 
                                                                int    port, 
                                                                IntPtr  infoBuffer, 
#if _WIN64
                                                                IntPtr/*ulong* */ infoBufferSize
#else
                                                                IntPtr /*uint* */ infoBufferSize
#endif
                                                                );
        // Get the whole parameter information as genicam-xml 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterInfoXML")]
        public unsafe static extern int Fg_getParameterInfoXML_void(IntPtr Fg, 
                                                                int    port, 
                                                                void*  infoBuffer, 
#if _WIN64
                                                                IntPtr/*ulong* */ infoBufferSize
#else
                                                                IntPtr /*uint* */ infoBufferSize
#endif
                                                                );

        // Get the whole parameter information as genicam-xml 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterInfoXML")]
        public unsafe static extern int Fg_getParameterInfoXML64(IntPtr Fg,
                                                                int port,
                                                                IntPtr infoBuffer,
                                                                IntPtr/*ulong* */ infoBufferSize
                                                                );

        // Get the whole parameter information as genicam-xml 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getParameterInfoXML")]
        public unsafe static extern int Fg_getParameterInfoXML32(IntPtr Fg,
                                                                int port,
                                                                IntPtr infoBuffer,
                                                                IntPtr /*uint* */ infoBufferSize
                                                                );

        // Saves all Applet parameters to the acc. file
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int Fg_saveConfig(IntPtr Fg, String MCFConfigFileName);

        // Loads all Applet parameters from the acc. file
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public unsafe  static extern int Fg_loadConfig(IntPtr Fg, String Filename);


        //--------------------------------------------------------------------
        // Funktionen zur Aquisition / 
        //--------------------------------------------------------------------

        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getLastPicNumberBlockingEx(IntPtr Fg,
#if _WIN64
                                                                long PicNr, //frameindex_t
#else
                                                                int PicNr, //frameindex_t
#endif
                                                                uint DmaIndex,
                                                                int Timeout,
                                                                IntPtr pMem);
        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberBlockingEx")]
        public static extern int Fg_getLastPicNumberBlockingEx64(IntPtr Fg,
                                                                long PicNr, //frameindex_t
                                                                uint DmaIndex,
                                                                int Timeout,
                                                                IntPtr pMem);
        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberBlockingEx")]
        public static extern int Fg_getLastPicNumberBlockingEx32(IntPtr Fg,
                                                                 int PicNr, //frameindex_t
                                                                 uint DmaIndex,
                                                                 int Timeout,
                                                                 IntPtr pMem);

        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberBlocking")]
#if _WIN64
        public static extern long Fg_getLastPicNumberBlocking(  IntPtr Fg,
                                                                long PicNr, //frameindex_t
#else
        public static extern int Fg_getLastPicNumberBlocking(IntPtr Fg,
                                                                int PicNr, //frameindex_t
#endif
                                                                uint DmaIndex, int Timeout);

        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberBlocking")]
        public static extern long Fg_getLastPicNumberBlocking64(IntPtr Fg,
                                                                long PicNr, //frameindex_t
                                                                uint DmaIndex, 
                                                                int Timeout);
        // fetch image / synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberBlocking")]
        public static extern int Fg_getLastPicNumberBlocking32(IntPtr Fg,
                                                                int PicNr, //frameindex_t
                                                                uint DmaIndex, 
                                                                int Timeout);

        // fetch image / not synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumber")]
#if _WIN64
        public static extern long Fg_getLastPicNumber(IntPtr Fg, uint DmaIndex);
#else
        public static extern int Fg_getLastPicNumber(IntPtr Fg, uint DmaIndex);
#endif
        // fetch image / not synchronized to internal image data transfer (64 Bit runtime)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumber")]
        public static extern long Fg_getLastPicNumber64(IntPtr Fg, uint DmaIndex);
        // fetch image / not synchronized to internal image data transfer (32 Bit Runtime)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumber")]
        public static extern int Fg_getLastPicNumber32(IntPtr Fg, uint DmaIndex);


        // fetch image / not synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberEx")]
#if _WIN64
        public static extern long Fg_getLastPicNumberEx( IntPtr Fg, uint DmaIndex, IntPtr pMem);
#else
        public static extern int Fg_getLastPicNumberEx(IntPtr Fg, uint DmaIndex, IntPtr pMem);
#endif
        // fetch image / not synchronized to internal image data transfer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberEx")]
        public static extern long Fg_getLastPicNumberEx64( IntPtr Fg, uint DmaIndex, IntPtr pMem);
        // fetch image / not synchronized to internal image data transfer (32 Bit Runtime)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastPicNumberEx")]
        public static extern int Fg_getLastPicNumberEx32(IntPtr Fg, uint DmaIndex, IntPtr pMem);


        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtrEx")]
        // Retrive imagedata from SDK
        public static extern IntPtr Fg_getImagePtrEx(IntPtr Fg,
#if _WIN64
                                                     long PicNr, //frameindex_t
#else
                                                     int PicNr, //frameindex_t
#endif
                                                     uint DmaIndex,
                                                     IntPtr pMem);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtrEx")]
        // Retrive imagedata from SDK
        public static extern IntPtr Fg_getImagePtrEx64(IntPtr Fg,
                                                       long PicNr, //frameindex_t
                                                       uint DmaIndex,
                                                       IntPtr pMem);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtrEx")]
        // Retrive imagedata from SDK
        public static extern IntPtr Fg_getImagePtrEx32( IntPtr Fg,
                                                        int PicNr, //frameindex_t
                                                        uint DmaIndex,
                                                        IntPtr pMem);


        // Retrive the Pointer to the image data accoring to the picicnr.        
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtr")]
        public static extern IntPtr Fg_getImagePtr( IntPtr Fg,
#if _WIN64
                                                    long PicNr, //frameindex_t
#else
                                                    int PicNr, //frameindex_t
#endif
                                                    uint DmaIndex);

        // Retrive the Pointer to the image data accoring to the picicnr.        
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtr")]
        public static extern IntPtr Fg_getImagePtr64(IntPtr Fg,
                                                    long PicNr, //frameindex_t
                                                    uint DmaIndex);
        // Retrive the Pointer to the image data accoring to the picicnr.        
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImagePtr")]
        public static extern IntPtr Fg_getImagePtr32(IntPtr Fg,
                                                     int PicNr, //frameindex_t
                                                     uint DmaIndex);

        // Start acquisition of an image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AcquireEx")]
        public static extern int Fg_AcquireEx(IntPtr Fg,
                                              uint DmaIndex,
#if _WIN64
                                              long NrOfImagesToGrab,
#else
                                              int NrOfImagesToGrab,
#endif
                                              int nFlag,
                                              IntPtr pMem);
        // Start acquisition of an image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AcquireEx")]
        public static extern int Fg_AcquireEx64(IntPtr Fg,
                                              uint DmaIndex,
                                              long NrOfImagesToGrab,
                                              int nFlag,
                                              IntPtr pMem);
        // Start acquisition of an image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_AcquireEx")]
        public static extern int Fg_AcquireEx32(IntPtr Fg,
                                                uint DmaIndex,
                                                int NrOfImagesToGrab,
                                                int nFlag,
                                                IntPtr pMem);

        // Start acquisition of images
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_Acquire")]
        public static extern int Fg_Acquire(IntPtr Fg, 
                                            uint DmaIndex, 
#if _WIN64
                                            long PicCount
#else
                                            int PicCount
#endif
                                            );
        // Start acquisition of images
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_Acquire")]
        public static extern int Fg_Acquire64(IntPtr Fg,
                                            uint DmaIndex,
                                            long PicCount
                                            );
        // Start acquisition of images
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_Acquire")]
        public static extern int Fg_Acquire32(IntPtr Fg,
                                              uint DmaIndex,
                                              int PicCount
                                              );
        // Stops data acquisition
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_stopAcquireEx(IntPtr Fg,
                                                    uint DmaIndex,
                                                    IntPtr pMem,
                                                    int nFlag);

        // Stops the image acquisition at the camera 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_stopAcquire(IntPtr Fg, uint DmaIndex);
        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImage")]
        public static extern int Fg_sendImage(IntPtr Fg, 
#if _WIN64
                                              long startImage, 
                                              long PicCount,
#else
                                              int startImage, 
                                              int PicCount,
#endif
                                              int nFlag, 
                                              uint DmaIndex);
        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImage")]
        public static extern int Fg_sendImage64(IntPtr Fg,
                                               long startImage, 
                                               long PicCount,
                                               int nFlag,
                                               uint DmaIndex);
        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImage")]
        public static extern int Fg_sendImage32(IntPtr Fg,
                                                int startImage,
                                                int PicCount,
                                                int nFlag,
                                                uint DmaIndex);

        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImageEx")]
        public static extern int Fg_sendImageEx( IntPtr Fg, 
#if _WIN64
                                                 long startImage, 
                                                 long PicCount,
#else
                                                 int startImage, 
                                                 int PicCount,
#endif
                                                 int nFlag, 
                                                 uint DmaIndex, 
                                                 IntPtr memHandle);
        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImageEx")]
        public static extern int Fg_sendImageEx64(IntPtr Fg,
                                                 long startImage, 
                                                 long PicCount,
                                                 int nFlag,
                                                 uint DmaIndex,
                                                 IntPtr memHandle);
        // Transferring an image from memory via DMA to the grabber
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_sendImageEx")]
        public static extern int Fg_sendImageEx32(IntPtr Fg,
                                                  int startImage,
                                                  int PicCount,
                                                  int nFlag,
                                                  uint DmaIndex,
                                                  IntPtr memHandle);

        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImage")]
        public static extern long Fg_getImage(  IntPtr Fg, 
                                                int Param,
#if _WIN64
                                                long PicNr, //frameindex_t
#else
                                                int PicNr, //frameindex_t
#endif
                                                uint DmaIndex, 
                                                uint Timeout);
        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImage")]
        public static extern long Fg_getImage64(IntPtr Fg,
                                                int Param,
                                                long PicNr, //frameindex_t
                                                uint DmaIndex,
                                                uint Timeout);
        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImage")]
        public static extern long Fg_getImage32(IntPtr Fg,
                                                int Param,
                                                int PicNr, //frameindex_t
                                                uint DmaIndex,
                                                uint Timeout);

        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImageEx")]
        public static extern long Fg_getImageEx(IntPtr Fg, 
                                                int Param,
#if _WIN64
                                                long PicNr, //frameindex_t
#else
                                                int PicNr, //frameindex_t
#endif
                                                uint DmaIndex, 
                                                uint Timeout, 
                                                IntPtr pMem);
        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImageEx")]
        public static extern long Fg_getImageEx64(IntPtr Fg,
                                                int Param,
                                                long PicNr, //frameindex_t
                                                uint DmaIndex,
                                                uint Timeout,
                                                IntPtr pMem);
        // returns the buffer no of the requested image
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getImageEx")]
        public static extern int Fg_getImageEx32(IntPtr Fg,
                                                  int Param,
                                                  int PicNr, //frameindex_t
                                                  uint DmaIndex,
                                                  uint Timeout,
                                                  IntPtr pMem);

        //Reading the Details of the acquisition status of frame buffer.
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern long Fg_getStatus(IntPtr Fg, 
                                               int Param, 
                                               uint Data, 
                                               uint DmaIndex);

        //Reading the Details of the acquisition status of frame buffer.
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getStatusEx")]
        public static extern long Fg_getStatusEx(IntPtr Fg, 
                                                 int Param, 
#if _WIN64
                                                 long Data, //frameindex_t
#else
                                                 int Data, //frameindex_t
#endif
                                                 uint DmaIndex, 
                                                 IntPtr pMem);

        //Reading the Details of the acquisition status of frame buffer.
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getStatusEx")]
        public static extern long Fg_getStatusEx64(IntPtr Fg,
                                                 int Param,
                                                 long Data, //frameindex_t
                                                 uint DmaIndex,
                                                 IntPtr pMem);

        //Reading the Details of the acquisition status of frame buffer.
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getStatusEx")]
        public static extern long Fg_getStatusEx32(IntPtr Fg,
                                                 int Param,
                                                 int Data, //frameindex_t
                                                 uint DmaIndex,
                                                 IntPtr pMem);


        // release the lock if the according image buffer in various modes
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getStatusEx")]
        public static extern int Fg_setStatus32(IntPtr Fg, 
                                              int Param, 
                                              int Data, //frameindex_t
                                              uint DmaIndex);

        // release the lock if the according image buffer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setStatusEx")]
        public static extern int Fg_setStatusEx(IntPtr Fg, 
                                                int Param, 
#if _WIN64
                                                long Data, //frameindex_t
#else
                                                int Data, //frameindex_t
#endif
                                                uint DmaIndex, 
                                                IntPtr pMem);
        // release the lock if the according image buffer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setStatusEx")]
        public static extern int Fg_setStatusEx64(IntPtr Fg,
                                                int Param,
                                                long Data, //frameindex_t
                                                uint DmaIndex,
                                                IntPtr pMem);
        // release the lock if the according image buffer
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_setStatusEx")]
        public static extern int Fg_setStatusEx32(IntPtr Fg,
                                                  int Param,
                                                  int Data, //frameindex_t
                                                  uint DmaIndex,
                                                  IntPtr pMem);

        //----------------------------------------------------------------------------------
        // Software Trigger
        //----------------------------------------------------------------------------------
        // request an image from camera by software call
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_sendSoftwareTrigger(IntPtr Fg, uint CamPort);

        // request an image from camera by software call
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_sendSoftwareTriggerEx(IntPtr Fg, uint CamPort, uint Triggers);

        //----------------------------------------------------------------------------------
        // Asynchr. handling
        //----------------------------------------------------------------------------------
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_registerApcHandler")]
        public unsafe static extern int Fg_registerApcHandlerIntPtr(IntPtr Fg, uint DmaIndex, IntPtr control /*ref FgApcControl control*/, FgApcControlFlags flags);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_registerApcHandler")]
        public unsafe static extern int Fg_registerApcHandler(IntPtr Fg, uint DmaIndex, ref FgApcControl control, FgApcControlFlags flags);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_registerEventCallback")]
        public unsafe static extern int Fg_registerEventCallback(IntPtr Fg, ulong mask, IntPtr/*Fg_EventFunc_t*/ handler, IntPtr data, uint flags, ref  fg_event_info info);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_registerEventCallback")]
        public unsafe static extern int Fg_registerEventCallbackIntPtr(IntPtr Fg, ulong mask, IntPtr/*Fg_EventFunc_t*/ handler, IntPtr data, uint flags, IntPtr/*fg_event_info*/ info);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_eventWait")]
        public unsafe static extern ulong Fg_eventWait(IntPtr Fg, ulong mask, uint timeout, uint flags, ref fg_event_info info);
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_eventWait")]
        public unsafe static extern ulong Fg_eventWaitIntPtr(IntPtr Fg, ulong mask, uint timeout, uint flags, IntPtr /*fg_event_info*/ info);
        

        // retrieve the Bitmaske for a named event
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong Fg_getEventMask(IntPtr Fg, string name);
        
        // retrieve the name for an event indicated by a certain bit
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getEventName")]
        public static extern IntPtr Fg_getEventName(IntPtr Fg, ulong mask);
        
        // enabling / diabling of t certain event (mask)
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_activateEvents(IntPtr Fg, ulong mask, uint enable);

        // removes all pending events
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_clearEvents(IntPtr Fg, ulong mask);

        //----------------------------------------------------------------------------------
        // Fehlerbehandlung  / Error Handling
        //----------------------------------------------------------------------------------

        // returns the errorcode of the last occured error
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getLastErrorNumber(IntPtr Fg);

        // returns the Textual Description of the last occured error
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getLastErrorDescription")]
        public static extern IntPtr Fg_getLastErrorDescription(IntPtr Fg);

        //
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getErrorDescription")]
        public static extern IntPtr Fg_getErrorDescription(IntPtr Fg, int ErrorNumber);

        // returns the text for the acc. errorcode
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getErrorDescription")]
        public static extern IntPtr getErrorDescription(int ErrorNumber);

        //----------------------------------------------------------------------------------
        // Boardinfo
        //----------------------------------------------------------------------------------

        // returns the type of the board identified by its index
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getBoardType(int BoardIndex);

        // retieve the board's serial no
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Fg_getSerialNumber(IntPtr Fg);

        // retrieve the SiSo runtime's Version
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Fg_getSWVersion();

        // returns the type of applet 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_getAppletId(IntPtr Fg, IntPtr ignored);

        // query global system informations
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getSystemInformation")]
        public unsafe static extern int Fg_getSystemInformation(IntPtr Fg, Fg_Info_Selector selector, FgProperty propertyId, int param1, IntPtr buffer, IntPtr buflen);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Fg_getSystemInformation")]
        public unsafe static extern int Fg_getSystemInformation(IntPtr Fg, Fg_Info_Selector selector, FgProperty propertyId, int param1, void* buffer, IntPtr buflen);

        //----------------------------------------------------------------------------------
        // convinience function for application dev.
        // Spezielle Funktionen für bestimmte Anwednungsfälle
        //----------------------------------------------------------------------------------
        
        // set/reset the exsync signal
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_setExsync(IntPtr Fg, int Flag, uint CamPort);

        // set/reset the flash signal
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_setFlash(IntPtr Fg, int Flag, uint CamPort);

        // allocate a shading handle
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        //ShadingMaster *
        public static extern IntPtr Fg_AllocShading(IntPtr Fg, int set, uint CamPort);
        
        // release the shading handle
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fg_FreeShading(IntPtr Fg, IntPtr shm);
        
        // request an access to the shading object
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_GetAccess(IntPtr Fg, IntPtr sh);

        // release access to the shading object
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_FreeAccess(IntPtr Fg, IntPtr sh);

        // 
        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_GetMaxLine(IntPtr Fg, IntPtr sh);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_SetSubValueLine(IntPtr Fg, IntPtr sh, int x, int channel, float sub);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_SetMultValueLine(IntPtr Fg, IntPtr sh, int x, int channel, float mult);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_SetFixedPatternNoiseLine(IntPtr Fg, IntPtr sh, int x, int channel, int on);

        [DllImport(FGLIBNAME_C, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Shad_WriteActLine(IntPtr Fg, IntPtr sh, int Line);
    }
}
