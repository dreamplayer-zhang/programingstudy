using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using SisoSDKLib;



/**
 * \brief	namespace of SiSo wrapper class, 
 *
 *          purpose: 
 *          wrapping of native C-datatypes to C# data types
 *          managing Siso-device handle for grabber access
 * 
 */
namespace SiSoFramegrabber
{
    //-------------------------------------------------------------------------
    /**
     * \brief	Access to System calls
     *
     *          purpose: 
     *          enable late binding of SiSo- SDK Dll's 
     * 
     */
    public class NativeDll
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr IntPtr_Module);

        /**
        * \brief	checking for 64 Bit Operating system
        */
        public static bool Is64Bit()
        {
            if (IntPtr.Size == 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //-------------------------------------------------------------------------
    /**
     * \brief	Internal class for transport of APC-Event (Fg_RegisterAPC)
     */
    //-------------------------------------------------------------------------
    class FgAPCTransferData
    {
        public IntPtr mWrapperObject;
        public IntPtr mUserData;
        public IntPtr mReceiverObject;
        public uint mDMAIndex;
        public FgAPCTransferData( IntPtr wrapperObject, IntPtr userData, IntPtr receiverObject, uint dmaIndex)
        {
            mWrapperObject = wrapperObject;
            mUserData = userData;
            mReceiverObject = receiverObject;
            mDMAIndex = dmaIndex;
        }
    }
    //-------------------------------------------------------------------------
    /**
     * \brief	Internal class for transport of SDK Event (Fg_RegisterEvent)
     */
    //-------------------------------------------------------------------------
    class FgEventTransferdata
    {
        public IntPtr mWrapperObject;
        public IntPtr mUserData;
        public IntPtr mReceiverObject;
        public FgEventTransferdata(IntPtr wrapperObject, IntPtr userData, IntPtr receiverObject)
        {
            mWrapperObject = wrapperObject;
            mUserData = userData;
            mReceiverObject = receiverObject;
        }
    }


    //-------------------------------------------------------------------------
    /**
     * \brief	Base class for information, which is notified by the SiSo Event system
     */
    //-------------------------------------------------------------------------
    public class SiSoBaseEvent
    {
        /**
        * \brief	Additional information transported for the client and got passed from Register functions to
        *           event handlers
        */
        public IntPtr userData;

        /**
        * \brief	constructor
        */
        public SiSoBaseEvent(IntPtr aUserData)
        {
            userData = aUserData;
        }
    }

    //-------------------------------------------------------------------------
    /**
     * \brief	Events notified by APC Subsystem
     */
    //-------------------------------------------------------------------------
    public class APCEvent : SiSoBaseEvent
    {
        /**
        * \brief	DMA channel, which transferred the image
        */
        public uint DMAIndex;
        /**
        * \brief	image no, which is newly available in memory
        */
        public long imageNo;

        /**
        * \brief	constructor
        */
        public APCEvent(IntPtr aUserData, uint aDMAIndex, long aImageNo)
            : base (aUserData)
        {
            DMAIndex = aDMAIndex;
            imageNo = aImageNo;
        }
    }
    

    //-------------------------------------------------------------------------
    /**
     * \brief	Base class for information, which is notified by the SDK-Event Callback
     */
    //-------------------------------------------------------------------------
    public class FramegrabberEvent : SiSoBaseEvent
    {
        /**
        * \brief	timestamp, when the event got notified
        */
        public ulong timestamp;
        /**
         * \brief	event mask, representing the 
         */
        public ulong eventMask;
        /**
         * \brief	SiSo event information. Constains timestamps, event data and certain event flags 
         * See the SDK docuemntation for details
         */
        public fg_event_info eventInfo;

        private List<ulong> timestampsByBit;
        /**
         * \brief	Setter for timestamp array
         */
        private void SetTimeStamp(ulong aTimestamp, int bit)
        {
            timestampsByBit[bit] = aTimestamp;
        }
        /**
         * \brief	Reading the timestamp for the event, based on the given bit
         */
        public ulong GetTimeStamp(int bit)
        {
            ulong rc = 0;
            if ((bit >= 0) && (bit < timestampsByBit.Count)){
                rc = timestampsByBit[bit];
            }
            return rc;
        }

        private List<uint> notifiersByBit;
        /**
         * \brief	setter for additional event notification array
         */
        private void SetEventNotifier(uint notificationFlags, int bit)
        {
            notifiersByBit[bit] = notificationFlags;

        }
        /**
         * \brief	Reading the additional event notification
         */
        public uint GetEventNotifier(int bit)
        {
            uint rc = 0;

            if ((bit >= 0) && (bit < 64))
            {
                rc = notifiersByBit[bit];
            }
            return rc;
        }
        /**
         * \brief	constructor for event container
         */
        public FramegrabberEvent(ulong aTimestamp, IntPtr aUserData, ulong aEventMask, fg_event_info aEventInfo)
            : base (aUserData)
        {
            timestamp = aTimestamp;
            eventMask = aEventMask;
            eventInfo = aEventInfo;
             
            timestampsByBit = new List<ulong>();
            notifiersByBit = new List<uint>();

            for (int i = 0; i < 64; i++){
                timestampsByBit.Add(0);
                notifiersByBit.Add(0);
                unsafe
                {
                    SetTimeStamp(aEventInfo.timestamp[i], i);
                    SetEventNotifier(aEventInfo.notify[i], i);
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    /**
     * \brief	declaration of Callback function pointer acc. to fgrab_struct.h
     *
     * \param	imgNr	number of the currently notified images
     * \param	data    user data, which has been passed by RegisterAPCCallback
     */
    //-------------------------------------------------------------------------
    public delegate int ApcFuncCallback(long imgNr, IntPtr data);

    //-------------------------------------------------------------------------
    /**
     * \brief	declaration of enhanced Callback function pointer similar to fgrab_struct.h
     *
     * \param	sender    reference to wrapper object, that fired the event
     * \param	ev	      container for event information, see APCEvent  
     */
    //-------------------------------------------------------------------------
    public delegate int ApcFuncCallbackEx( Object sender,  APCEvent ev);


    //-------------------------------------------------------------------------
    /**
     * \brief	declaration of Callback function for Event system
     *
     * \param	sender    reference to wrapper object, that fired the event
     * \param	ev	      container for event information, see FramegrabberEvent  
     */
    //-------------------------------------------------------------------------
    public delegate int SiSoEventFuncCallback(Object sender, FramegrabberEvent ev);

    //-------------------------------------------------------------------------
    /* \brief	type of error / location, which generated the error. either then
    * SDK direct or wrapping class. In order to retrieve status information call
    * in case of 
    *  fgOK: call was successfull
    *  fgeErrorSiSoSDK: the corresconding getLastError
    *  fgeErrorWrapper: covers errors inside the wraper in case of ... 
    */
    //-------------------------------------------------------------------------
    public enum fgErrorType
    {
        fgeOK = 0x00,            /**< calling SISO SDK was successfull */
        fgeErrorSiSoSDK = 0x01,    /**< callng SISO-SDK produced an error, call FgGetLastError/FgGetLastErrorDesciption to get a error message from SISO SDK */
        fgeErrorWrapper = 0x02,     /**< an error was detected at the C# Assembly layer */
    }


    public enum WrapperError
    {
        weOK = 0,           /**< calling SISO SDK was successfull */
        weSDK = 100,        /**< error occured, should be retrieved from SDK */
        weException = 200,  /**< Exception when calling SDK*/
        weParamCheck = 300, /**< Wrapper sided parameter check */
        weNoPointer  = 301,  /**< no pointer returned from SDK */
        weCustomerError = 400  /**< custom error set at a derived class */
    }
    //-------------------------------------------------------------------------
    // Wrapper-Klasse
    //-------------------------------------------------------------------------
    /**
    * \brief	convinience wrapper class for access to SiSo frame grabber
    * 
    */
    //-------------------------------------------------------------------------
    public class Framegrabber : SDKLib 
    {

    /**
        * \brief	default value for max number of DMA channels in use
        */
        const int MAXDMACHANNELS_C = 10; // max. Nr of DMA - change acc. to application needs
        /**
        * \brief	default value for max size for string parameter values of the applets
        */
        const uint MAXSISOSTRINGLEN = 1024; // Default value for string parameter values
        /**
        * \brief	for iteration of eevents
        */
        private ulong EventMaskIterator;

        /**
        * \brief	GarbageCollection of event callbacks
        */
        protected List<GCHandle> EventCallbackPins;
        /**
        * \brief	GarbageCollection of event callbacks
        */
        protected List<GCHandle> APCCallbackPins;

        /**
        * \brief	GarbageCollection for marshalled data
        */
        protected List<IntPtr> MarshalledData;
        

        /**
        * \brief	enable tracefile creation
        */
        public bool logging; // activate trace file generation
        /**
        * \brief	a flag to indicate the SiSo runtime, which is in use. Gets initialized by the 
        *  current operation system, but might be switched from 64 Bit to 32 Bit runtime
        */
        public bool using64BitRuntime; 
        /**
        * \brief	currently used maximum sizef of applet's string parameter values
        * e.g. for filename parameters. 
        */
        public uint maxSISOStringSize;  // max. size for String Parameter values
        /**
        * \brief	handle for frame grabber access
        */
        public IntPtr pFGHandle;        // Handle of frame grabber control
        /**
        * \brief	control of grabber allocated memory for  images  (alternatively to pGrabberMem)
        */
        public IntPtr[] GrabberDMAMem;  // completely GrabberAllocated Image 
        /**
        * \brief	control structure for grabber side allocated Memory
        */
        public IntPtr pGrabberMem;
        /**
        * \brief	control of user allocated memory
        */
        public IntPtr[] MemHead;        // GrabberAllocated memory (Administration of user allocated Memory)

        /**
        * \brief	Libhanld for sisohal.dll
        */
        private IntPtr hSiSoHal;
        /**
        * \brief	Libhanld for framgegraber dll
        */
        private IntPtr hFgLib;
        /**
        * \brief	location of glib5.dll
        */
        public string SiSoDllDirectory;
        /**
          * \brief	perform late binding of siso dll
        */
        private fgErrorType ActivateACertainSisoDll()
        {
             if (SiSoDllDirectory != "")
            {

                string SDKDllNameSiSohw = SiSoDllDirectory + "\\bin\\" + "siso_hw.dll";
                IntPtr hSiSohw = NativeDll.LoadLibrary(SDKDllNameSiSohw);
                
                string SDKDllNameHal = SiSoDllDirectory + "\\bin\\" + "siso_hal.dll";
                hSiSoHal = NativeDll.LoadLibrary(SDKDllNameHal);


                string SDKDllNameHaprt = SiSoDllDirectory + "\\bin\\" + "haprt.dll";
                IntPtr hSiSoHapRt = NativeDll.LoadLibrary(SDKDllNameHaprt);

                if ((hSiSoHal != IntPtr.Zero) || (!File.Exists(SDKDllNameHal)))
                {
                    GCHandle gchSiSoHal = GCHandle.Alloc(hSiSoHal);

                    // !File.Exists(SDKDllNameHal) -> RT 5.1
                    string SDKDllName = SiSoDllDirectory + "\\bin\\" + SDKLib.FGLIBNAME_C;
                    hFgLib = NativeDll.LoadLibrary(SDKDllName);
                    if (hFgLib == IntPtr.Zero)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return fgErrorType.fgeErrorWrapper;
                    }
                    else
                    {
                        GCHandle gchFGLib = GCHandle.Alloc(hFgLib);
                    }
                }
                else
                {
                    SetErrorCode(WrapperError.weParamCheck);
                    return fgErrorType.fgeErrorWrapper;
                }
            }
            // the default way: no dir
            return fgErrorType.fgeOK;
        }

        /**
        * \brief	delegation of APC callbacks
        */
        private List < uint> APCEventHandlerIndizes;
        private List < ApcFuncCallback> APCEventHandlers;
        private List<ApcFuncCallbackEx> APCEventHandlersEx;

        /**
        * \brief	delegation of SDK- Event callbacks
        */
        private List<ulong> EventCallbackMasks;
        private List<SiSoEventFuncCallback> EventCallbackList;

        /**
        * \brief	signal an event that has been notified by the grabber
        */
        private bool RemoveAPCEventHandler(uint DMAIndex)
        {
            bool rc = true;
            int index = GetAPCEventHandlerIndex(DMAIndex);
            if (index >= 0)
            {
                ApcFuncCallback EventHandler = APCEventHandlers[index];
                ApcFuncCallbackEx ExEventHandler = APCEventHandlersEx[index];

                APCEventHandlerIndizes.Remove(DMAIndex);
                APCEventHandlers.Remove(EventHandler);
                APCEventHandlersEx.Remove(ExEventHandler);
                rc = true;
            }
            else
            {
                rc = false;
            }
            return rc;
        }

        /**
        * \brief	signal an event that has been notified by the grabber
        */
        private bool AddAPCEventHandler(uint DMAIndex, ApcFuncCallback CallbackHandler, ApcFuncCallbackEx ExCallbackHandler)
        {
            bool rc = true;
            int index = GetAPCEventHandlerIndex(DMAIndex);
            if (index >= 0)
            {
                // registered twice, unregister first
                rc = false;
            }
            else
            {
                APCEventHandlerIndizes.Add(DMAIndex);
                APCEventHandlers.Add(CallbackHandler);
                APCEventHandlersEx.Add(ExCallbackHandler);
                rc = true;
            }
            return rc;
        }

        private int GetAPCEventHandlerIndex(uint DMAIndex)
        {
            int rc = -1;
            for (int i = 0; i < APCEventHandlerIndizes.Count; i++)
            {
                uint index = APCEventHandlerIndizes[i];
                if (index == DMAIndex)
                {
                    rc = i;
                    break;
                }
            }
            return rc;
        }

        /**
        * \brief	Get according event Handler
        */
        private ApcFuncCallback GetAPCEventHandler(uint DMAIndex)
        {
            ApcFuncCallback EventHandler = null;
            int index = GetAPCEventHandlerIndex(DMAIndex);
            if (index >= 0)
            {
                EventHandler = APCEventHandlers[index];
            }
            return EventHandler;
        }

        /**
        * \brief	Get according event Handler (extended)
        */
        private ApcFuncCallbackEx GetAPCExEventHandler(uint DMAIndex)
        {
            ApcFuncCallbackEx EventHandler = null;
            int index = GetAPCEventHandlerIndex(DMAIndex);
            if (index >= 0)
            {
                EventHandler = APCEventHandlersEx[index];
            }
            return EventHandler;
        }
        

        ////////////////////////////////////////////////////////////////////////////////////
        // error state
        ////////////////////////////////////////////////////////////////////////////////////        
        /**
        * \brief	current error state based on the last call
        */
        protected WrapperError ErrorCode;

        protected int SiSoSDKError;

        /**
        * \brief	setter for error code
        */
        public void SetErrorCode(WrapperError NewError)
        {
            ErrorCode = NewError;
        }
        /**
        * \brief	reset error before calling SISO, automatically done inside the wrapper
        */
        protected void ResetErrorCode()
        {
            ErrorCode = WrapperError.weOK;
            SiSoSDKError = 0;
        }
        /**
        * \brief	Getter for the current error state
        */
        public WrapperError GetLastError()
        {
            return ErrorCode;
        }

        /**
        * \brief	Getter for the current error state
        */
        public int GetLastSDKError()
        {
            return SiSoSDKError;
        }


        /**
         * \brief	Converter form FgLib error codes to C# Wrapper ErrorTypes
         */
        private fgErrorType SiSoRc2FgeErrorType(int fg_rc)
        {
            SiSoSDKError = fg_rc; 
            if (fg_rc == 0)
            {
                SetErrorCode(WrapperError.weOK);
                return fgErrorType.fgeOK;
            }
            else
            {
                SetErrorCode(WrapperError.weSDK);
                return fgErrorType.fgeErrorSiSoSDK;
            }
        }

        /**
         * \brief	returns the previously occured error, independend of the source
         */
        public fgErrorType GetLastError(out WrapperError errorType, out int errorCode, out string errormsg)
        {
            errormsg = "";
            errorCode = 0;
            errorType = GetLastError();
            fgErrorType rc = fgErrorType.fgeOK;
            switch (errorType)
            {
                case WrapperError.weException:
                    {
                        errormsg = "Exception occured while calling SiSo SDK";
                        errorCode = -1;
                        break;
                    }
                case WrapperError.weNoPointer:
                    {
                        errormsg = "no pointer returned";
                        errorCode = -1;
                        break;
                    }
                case WrapperError.weParamCheck:
                    {
                        errormsg = "invalid parameters for SDK wrapper";
                        errorCode = -1;
                        break;
                    }
                case WrapperError.weSDK:
                    {
                        errormsg = FgGetLastErrorDescription();
                        errorCode = FgGetLastErrorNumber();
                        break;
                    }
                case WrapperError.weCustomerError:
                    {
                        errormsg = "custom error at derived class";
                        errorCode = -1;
                        break;
                    }
            }
            return rc;
        }


        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        /**
        * \brief	default constructor
        */
        public Framegrabber()
        {
            MemHead = new IntPtr[MAXDMACHANNELS_C];
            for (int i = 0; i < MAXDMACHANNELS_C; i++)
            {
                MemHead[i] = IntPtr.Zero;
            }

            pGrabberMem = IntPtr.Zero;

            GrabberDMAMem = new IntPtr[MAXDMACHANNELS_C];
            for (int i = 0; i < MAXDMACHANNELS_C; i++)
            {
                GrabberDMAMem[i] = IntPtr.Zero;
            }
            logging = false;
            pFGHandle = IntPtr.Zero;

            hFgLib = IntPtr.Zero;
            hSiSoHal = IntPtr.Zero;

           //SiSoDllDirectory = ""; // use the default DLL
            SiSoDllDirectory = @"C:\Program Files\SiliconSoftware\Runtime5.4.3";

            maxSISOStringSize = MAXSISOSTRINGLEN;

            // to distinguish the size of frameindex_t : change for using 32 Siso-Runtime on 64 Bit OS
            using64BitRuntime = NativeDll.Is64Bit();

            EventCallbackPins = new List<GCHandle>();
            APCCallbackPins = new List<GCHandle>();

            // Administation of registered SDK-Event Eventshandlers
            EventCallbackMasks = new List<ulong>();
            EventCallbackList = new List<SiSoEventFuncCallback>();

            // Administation of registered APC Eventhandlers
            APCEventHandlerIndizes = new List<uint>();
            APCEventHandlers = new List < ApcFuncCallback>();
            APCEventHandlersEx = new List < ApcFuncCallbackEx>();
            
            // AllocHGlobal
            MarshalledData = new List <IntPtr>(); 
        }

        //---------------------------------------------------------------------
        /**
        * \brief	destructor
        */
        ~Framegrabber()
        {

            
            if (pFGHandle!= IntPtr.Zero)
            {
                //FgFreeGrabber();
                pFGHandle = IntPtr.Zero;
            }

            if (hFgLib != IntPtr.Zero)
            {
                NativeDll.FreeLibrary(hFgLib);
                hFgLib = IntPtr.Zero;
            }
            if (hSiSoHal != IntPtr.Zero)
            {
                NativeDll.FreeLibrary(hSiSoHal);
                hSiSoHal = IntPtr.Zero;
            }
            DoGarbageCollection();

        }

        protected void DoGarbageCollection()
        {
            // Event Callbacks 
            foreach (GCHandle CallbackPin in EventCallbackPins)
            {
                CallbackPin.Free();
            }
            EventCallbackPins.Clear();

            // APC Callbacks 
            foreach (GCHandle CallbackPin in APCCallbackPins)
            {
                CallbackPin.Free();
            }
            APCCallbackPins.Clear();

            foreach (IntPtr pMarshalledData in MarshalledData)
            {
                Marshal.FreeHGlobal(pMarshalledData);
            }
            MarshalledData.Clear();
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	initializing the SiSo framegrabber, incl. loading a given applet
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgInit(string HapName, uint BoardNo, int IsSlave = 0)
        {
            return FgInitEx(HapName, BoardNo, IsSlave);
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	initializing the SiSo framegrabber, incl. loading a given applet
        *           including possibility for Master/Slave mode for using the library
        *          across process boundaries
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgInitEx(string HapName, uint BoardNo, int isSlave)
        {
            fgErrorType rc;
            ResetErrorCode();

            // Load a certain FGLIB and its friends
            rc = ActivateACertainSisoDll();
            if (rc != fgErrorType.fgeOK)
            {
                return rc;
            }

            // initialize the grabber
            pFGHandle = Fg_InitEx(HapName, BoardNo, isSlave);
            if (pFGHandle != IntPtr.Zero)
            {
                int fg_rc = 0;
                int value = 1;
                unsafe
                {
                    // trace the calls to a log file. 
                    if (logging)
                    {
                        fg_rc = Fg_setParameter(pFGHandle, 43010, &value, 0); //Fg_setParameter("FG_LOGGING", "1", 0); = Enable logging
                    }
                }
                if (fg_rc == 0)
                {
                    rc = fgErrorType.fgeOK;
                }
                else
                {
                    rc = fgErrorType.fgeErrorSiSoSDK;
                    SetErrorCode(WrapperError.weSDK);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	Initializing the framegrabber based on a configuration file including 
        *           applet parameters (*.mcf)
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgInitConfig(string MCFConfigFileName, uint BoardNo)
        {
            fgErrorType rc;
            ResetErrorCode();

            rc = ActivateACertainSisoDll();
            if (rc != fgErrorType.fgeOK)
            {
                return rc;
            }

            pFGHandle = Fg_InitConfig(MCFConfigFileName, BoardNo);
            if (pFGHandle != IntPtr.Zero)
            {
                int fg_rc = 0;
                int value = 1;
                unsafe
                {
                    // trace the calls to a log file. 
                    if (logging)
                    {
                        fg_rc = Fg_setParameter(pFGHandle, 43010, &value, 0); //Fg_setParameter("FG_LOGGING", "1", 0); = Enable logging
                    }

                }
                if (fg_rc == 0)
                {
                    rc = fgErrorType.fgeOK;
                }
                else
                {
                    rc = fgErrorType.fgeErrorSiSoSDK;
                    SetErrorCode(WrapperError.weSDK);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorSiSoSDK;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	releases the frame grabber control completely
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgFreeGrabber()
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_FreeGrabber(pFGHandle);
                rc = SiSoRc2FgeErrorType(fg_rc);
                pFGHandle = IntPtr.Zero;
                
                // Release the data for this session
                DoGarbageCollection();
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            
            return rc;
        }


        ////////////////////////////////////////////////////////////////////////////////////
        // general information
        ////////////////////////////////////////////////////////////////////////////////////
        //----------------------------------------------------------------------------------
        /**
        * \brief	returns the board type
        */
        //----------------------------------------------------------------------------------
        public int 	FgGetBoardType (uint BoardNo)
        {
            int rc;
            ResetErrorCode();
            rc = Fg_getBoardType((int)BoardNo);
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	returns the board type
        */
        //----------------------------------------------------------------------------------
        public uint 	FgGetSerialNumber()
        {
            uint rc;
            ResetErrorCode();
            rc = Fg_getSerialNumber(pFGHandle);
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	returns the runtime version at the given board
        */
        //----------------------------------------------------------------------------------
        public string 	FgGetSWVersion()
        {
            string rc;
            IntPtr pData;
            ResetErrorCode();

            pData = Fg_getSWVersion();

            rc = Marshal.PtrToStringAnsi(pData);
            char[] result = rc.ToCharArray();
            return new string(result);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	returns the applet version at the given board
        */
        //----------------------------------------------------------------------------------
        public int 	FgGetAppletID (uint BoardNo)
        {
            int rc;
            ResetErrorCode();
            rc = Fg_getAppletId(pFGHandle, IntPtr.Zero);
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	returns various information about the framegrabber system
        */
        //---------------------------------------------------------
        public fgErrorType FgGetSystemInformation(uint BoardNo, Fg_Info_Selector selector, FgProperty propertyId, int param, IntPtr buffer, ref int buflen)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            int fg_rc = 0;

            ResetErrorCode();
            try
            {
                unsafe
                {
                    int BufSize = buflen;
                    int* pBufSize = &BufSize;
                    IntPtr pDataBufSize = (IntPtr)pBufSize;
                    fg_rc = Fg_getSystemInformation(pFGHandle, selector, propertyId, param, buffer, pDataBufSize);
                    buflen = BufSize;
                    rc = SiSoRc2FgeErrorType(fg_rc);
                }
            }
            catch
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weException);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	Overload for string
        */
        //---------------------------------------------------------
        public fgErrorType FgGetSystemInformation(uint BoardNo, Fg_Info_Selector selector, FgProperty propertyId, int param, out string result)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            int fg_rc = 0;
            result = "";
            ResetErrorCode();
            try
            {
                unsafe
                {
                    int BufSize = 255;
                    int* pBufSize = &BufSize;
                    IntPtr pDataBufSize = (IntPtr)pBufSize;

                    byte[] stringbuffer = new byte[BufSize];
                    fixed (byte* pBuffor = stringbuffer)
                    {
                        fg_rc = Fg_getSystemInformation(pFGHandle, selector, propertyId, param, (void*)pBuffor, pDataBufSize);
                        if (fg_rc == 0)
                        {
                            result = System.Text.Encoding.ASCII.GetString(stringbuffer);
                        }
                    }
                    rc = SiSoRc2FgeErrorType(fg_rc);
                }
            }
            catch
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weException);
            }
            return rc;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        // Memory Managment
        ////////////////////////////////////////////////////////////////////////////////////

        //----------------------------------------------------------------------------------
        /**
        * \brief	allocating frame memory at grabber side
        * \retval   pointer to image data for the corresponding buffer
        */
        //----------------------------------------------------------------------------------
        public IntPtr FgAllocMem(ulong Size, int BufCnt, uint DMAIndex)
        {
            IntPtr pMem0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    pMem0 = Fg_AllocMem64(pFGHandle, Size, BufCnt, DMAIndex);
                }
                else
                {
                    uint uiSize;
                    if (Size > uint.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return IntPtr.Zero;
                    }
                    uiSize = (uint)Size;
                    pMem0 = Fg_AllocMem32(pFGHandle, uiSize, BufCnt, DMAIndex);
                }
                if (pMem0 != IntPtr.Zero)
                {
                    GrabberDMAMem[DMAIndex] = pMem0;// store the reference at this object for later use
                }
                else
                {
                    SetErrorCode(WrapperError.weNoPointer);
                }
                return pMem0;
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                return IntPtr.Zero;
            }
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	allocating memory for frame buffers at grabber side
        * \retval   pointer to Memory control structure for image buffers 
        */
        //----------------------------------------------------------------------------------
        public IntPtr FgAllocMemEx(ulong Size, int BufCnt)
        {

            IntPtr pMem0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    pMem0 = Fg_AllocMemEx64(pFGHandle, Size, BufCnt);
                }
                else
                {
                    uint uiSize;
                    if (Size > uint.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return IntPtr.Zero;
                    }
                    uiSize = (uint)Size;
                    pMem0 = Fg_AllocMemEx32(pFGHandle, uiSize, BufCnt);
                }
                if (pMem0 != IntPtr.Zero)
                {
                    pGrabberMem = pMem0;
                }
                else
                {
                    SetErrorCode(WrapperError.weNoPointer);
                }
                return pMem0;
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                return IntPtr.Zero;
            }
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	release the memory, which is allocated at grabber side
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgFreeMem( uint DMAIndex)
        {
            fgErrorType rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                int fg_rc;
                fg_rc = Fg_FreeMem(pFGHandle, DMAIndex);
                if (fg_rc == 0)
                {
                    if ((DMAIndex >= 0) && (DMAIndex < MAXDMACHANNELS_C))
                    {
                        GrabberDMAMem[DMAIndex] = IntPtr.Zero;// store the reference at this object
                    }
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	releases the memory and the image buffers allocated at grabber side
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgFreeMemEx(IntPtr pMem)
        {
            ResetErrorCode();
            if (pFGHandle!= IntPtr.Zero)
            {
                int fg_rc = 0;
                try
                {
                    fg_rc = Fg_FreeMemEx(pFGHandle, pMem);
                    pGrabberMem = IntPtr.Zero;
                    return SiSoRc2FgeErrorType(fg_rc);
                }
                catch
                {
                    SetErrorCode(WrapperError.weException);
                    return fgErrorType.fgeErrorWrapper;
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                return fgErrorType.fgeErrorWrapper;
            }
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	allocating control structure for image buffers allocated at user's side
        * \retval   pointer to Memory control structure for adding user memory
        */
        //----------------------------------------------------------------------------------
        public IntPtr FgAllocMemHead(ulong Size, long BufCnt, uint DMAIndex)
        {
            IntPtr pHead = IntPtr.Zero;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                if (MemHead[DMAIndex] == IntPtr.Zero)
                {
                    if (using64BitRuntime)
                    {
                        ulong Size2Use = Size;
                        long BufCnt2Use = BufCnt;
                        pHead = Fg_AllocMemHead64(pFGHandle, Size2Use, BufCnt2Use);
                    }
                    else
                    {
                        if ((Size > uint.MaxValue) || (BufCnt > uint.MaxValue))
                        {
                            SetErrorCode(WrapperError.weParamCheck);
                            return IntPtr.Zero;
                        }
                        uint Size2Use = (uint)Size;
                        int BufCnt2Use = (int)BufCnt;
                        pHead = Fg_AllocMemHead32(pFGHandle, Size2Use, BufCnt2Use);
                    }
                    if (pHead != IntPtr.Zero)
                    {
                        // Add to Admin per DMA
                        if ((DMAIndex >= 0) && (DMAIndex < MAXDMACHANNELS_C))
                        {
                            MemHead[DMAIndex] = pHead;
                        }
                    }
                    else
                    {
                        SetErrorCode(WrapperError.weNoPointer);
                    }
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
            }
            return pHead;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	releasing Buffer control structure identified by Pointer directly
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgFreeMemHead(IntPtr pMemHead)
        {
            fgErrorType rc;
            int fg_rc = 0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_FreeMemHead(pFGHandle, pMemHead);
                if (fg_rc == 0)
                {
                    for (int i = 0; i < MAXDMACHANNELS_C; i++)
                    {
                        if (MemHead[i] == pMemHead)
                        {
                            MemHead[i] = IntPtr.Zero;
                        }
                    }
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	releasing Buffer control structure identified by DMA index
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgFreeMemHead(uint DmaIndex)
        {
            fgErrorType rc;
            int fg_rc = 0;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                IntPtr pMemHead = IntPtr.Zero;
                if ((DmaIndex >= 0) && (DmaIndex < MAXDMACHANNELS_C))
                {
                    pMemHead = MemHead[DmaIndex];
                }
                fg_rc = Fg_FreeMemHead(pFGHandle,pMemHead);
             
                if (fg_rc == 0)
                {
                    pMemHead = MemHead[DmaIndex] = IntPtr.Zero;
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	Adding memory for images to Memory control structure
        */
        //----------------------------------------------------------------------------------
        public int FgAddMem(uint DmaIndex, IntPtr pUserBuffer, long Size, long BufferIndex, IntPtr pMemHead)
        {
            int rc = -1; // invalid Buffer
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                // Fetching the Head for User Memory
                IntPtr pMemHead2Use = pMemHead;
                if (pMemHead2Use == IntPtr.Zero)
                {
                    if ((DmaIndex >= 0) && (DmaIndex < MAXDMACHANNELS_C))
                    {
                        pMemHead2Use = MemHead[DmaIndex];
                    }
                }
                // Moving towards the SDK
                int fg_rc = 0;
                if (using64BitRuntime)
                {
                    ulong Size2Use = (ulong)Size;
                    long BufferIndex2Use = BufferIndex;
                    fg_rc = Fg_AddMem64(pFGHandle, pUserBuffer, Size2Use, BufferIndex2Use, pMemHead2Use);
                }
                else
                {
                    if ((Size > uint.MaxValue) || (BufferIndex > Int32.MaxValue))
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return -1;
                    }

                    uint Size2Use = (uint)Size;
                    int BufferIndex2Use = (int)BufferIndex;
                    fg_rc = Fg_AddMem32(pFGHandle, pUserBuffer, Size2Use, BufferIndex2Use, pMemHead2Use);
                }
                if (fg_rc < 0)
                {
                    SetErrorCode(WrapperError.weSDK);
                }
                else
                {
                    rc = fg_rc; // BufferIndex modulo 2exp(30)
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	removing memory for images from Memory control structure
        */
        //----------------------------------------------------------------------------------
        public int FgDelMem(uint DmaIndex, long BufferIndex, IntPtr pMemHead)
        {
            fgErrorType rc;
            int fg_rc = -1;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                // Fetching the Head for User Memory
                IntPtr pMemHead2Use = pMemHead;
                if (pMemHead2Use == IntPtr.Zero)
                {
                    if ((DmaIndex >= 0) && (DmaIndex < MAXDMACHANNELS_C))
                    {
                        pMemHead2Use = MemHead[DmaIndex];
                    }
                }
                // Moving towards the SDK
                if (using64BitRuntime)
                {
                    long BufferIndex2Use = BufferIndex;
                    fg_rc = Fg_DelMem64(pFGHandle, pMemHead2Use, BufferIndex2Use);
                }
                else
                {
                    if (BufferIndex > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        fg_rc = -1;
                    }
                    int BufferIndex2Use = (int)BufferIndex;
                    fg_rc = Fg_DelMem32(pFGHandle, pMemHead2Use, BufferIndex2Use);
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return fg_rc;
        }


        ////////////////////////////////////////////////////////////////////////////////////
        // Image Acquisition
        ////////////////////////////////////////////////////////////////////////////////////


        //----------------------------------------------------------------------------------
        /**
        * \brief	Reading the status of a certain buffer
        */
        //----------------------------------------------------------------------------------
        public long FgGetStatusEx(Fg_Status_Selector Param, long Data, uint DmaIndex, IntPtr pMem)
        {
            long fg_rc = 0;
            ResetErrorCode();
            if (using64BitRuntime)
            {
                fg_rc = Fg_getStatusEx64(pFGHandle,(int) Param, Data, DmaIndex, pMem);
            }
            else
            {
                if (Data > Int32.MaxValue)
                {
                    SetErrorCode(WrapperError.weParamCheck);
                    return 0;
                }
                int iData = (int)Data;
                fg_rc = Fg_getStatusEx32(pFGHandle, (int) Param, iData, DmaIndex, pMem);
            }
            if (fg_rc < 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }
            return fg_rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	Setting the status of a certain buffer
        */
        //----------------------------------------------------------------------------------
        public int FgSetStatusEx(Fg_Status_Selector Param, long Data, uint DmaIndex, IntPtr pMem)
        {
            int fg_rc = 0;
            ResetErrorCode();
            if (using64BitRuntime)
            {
                fg_rc = Fg_setStatusEx64(pFGHandle,(int)Param, Data, DmaIndex, pMem);
            }
            else
            {
                if (Data > Int32.MaxValue)
                {
                    SetErrorCode(WrapperError.weParamCheck);
                    return 0;
                }
                int iData = (int)Data;
                fg_rc = Fg_setStatusEx32(pFGHandle, (int)Param, iData, DmaIndex, pMem);
            }

            return fg_rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve last frame from grabber in blocking mode
        */
        //----------------------------------------------------------------------------------
        public long FgGetLastPicNumberBlockingEx(long PicNr,
                                                uint DmaIndex,
                                                int Timeout,
                                                IntPtr pMem)
        {
            int fg_rc;
            ResetErrorCode();
            if (using64BitRuntime)
            {
                long PicNr2Use = PicNr; 
                fg_rc = Fg_getLastPicNumberBlockingEx64(pFGHandle, PicNr2Use, DmaIndex, Timeout, pMem);
            }
            else
            {
                if (PicNr > Int32.MaxValue)
                {
                    SetErrorCode(WrapperError.weParamCheck);
                    return 0;

                }
                int PicNr2Use = (int)PicNr; fg_rc = Fg_getLastPicNumberBlockingEx32(pFGHandle, PicNr2Use, DmaIndex, Timeout, pMem);
            }

            if (fg_rc < 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }

            return fg_rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve last frame from grabber in blocking mode
        */
        //----------------------------------------------------------------------------------
        public long FgGetLastPicNumberBlocking(long PicNr,
                                               uint DmaIndex,
                                               int Timeout)
        {
            long fg_rc;
            ResetErrorCode();

            if (using64BitRuntime)
            {
                long PicNr2Use = PicNr; 
                fg_rc = Fg_getLastPicNumberBlocking64(pFGHandle, PicNr2Use, DmaIndex, Timeout);
            }
            else
            {
                if (PicNr > Int32.MaxValue)
                {
                    SetErrorCode(WrapperError.weParamCheck);
                    return 0;

                }
                int PicNr2Use = (int)PicNr; 
                fg_rc = Fg_getLastPicNumberBlocking32(pFGHandle, PicNr2Use, DmaIndex, Timeout);
            }
            if (fg_rc < 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }
            return fg_rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve last frame from grabber in non blocking mode
        */
        //----------------------------------------------------------------------------------
        public long FgGetLastPicNumberEx(uint DmaIndex, IntPtr pMem)
        {
            long fg_rc;
            ResetErrorCode();
            if (using64BitRuntime)
            {
                fg_rc = Fg_getLastPicNumberEx64(pFGHandle, DmaIndex, pMem);
            }
            else
            {
                int PicNr2Use;
                PicNr2Use = Fg_getLastPicNumberEx32(pFGHandle, DmaIndex, pMem);
                fg_rc = (long)PicNr2Use;
            }
            if (fg_rc < 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }
            return fg_rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve last frame from grabber in nopn blocking mode
        */
        //----------------------------------------------------------------------------------
        public long FgGetLastPicNumber(uint DmaIndex)
        {
            long fg_rc;
            ResetErrorCode();

            if (using64BitRuntime)
            {
                fg_rc = Fg_getLastPicNumber64(pFGHandle, DmaIndex);
            }
            else
            {
                int PicNr2Use;
                PicNr2Use = Fg_getLastPicNumber32(pFGHandle, DmaIndex);
                fg_rc = (long)PicNr2Use;
            }
            if (fg_rc < 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }
            return fg_rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve a pointer to the according image number based on the buffers
        * \brief	Bildzeiger der angegegebenen Bildnummer abholen auf basis des Zustands 
        *           der Bildpuffer
        */
        //----------------------------------------------------------------------------------
        public IntPtr FgGetImagePtr(long PicNr,
                                    uint DmaIndex)
        {
            IntPtr pImgage = IntPtr.Zero;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    long PicNr2Use = PicNr;
                    pImgage = Fg_getImagePtr64(pFGHandle, PicNr2Use, DmaIndex);
                }
                else
                {
                    if (PicNr > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return IntPtr.Zero;
                    }
                    int PicNr2Use = (int)PicNr;
                    pImgage = Fg_getImagePtr32(pFGHandle, PicNr2Use, DmaIndex);
                }
                if (pImgage == IntPtr.Zero)
                {
                    // Fehler
                    SetErrorCode(WrapperError.weNoPointer);
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
            }
            return pImgage;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve a pointer to the according image number based on the buffers
         */
        //----------------------------------------------------------------------------------
        public IntPtr FgGetImagePtrEx(long PicNr,
                                       uint DmaIndex,
                                       IntPtr pMem)
        {
            IntPtr pImgage = IntPtr.Zero;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    long PicNr2Use = PicNr;
                    pImgage = Fg_getImagePtrEx64(pFGHandle, PicNr2Use, DmaIndex, pMem);
                }
                else
                {
                    if (PicNr > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return IntPtr.Zero;
                    }
                    int PicNr2Use = (int)PicNr;
                    pImgage = Fg_getImagePtrEx32(pFGHandle, PicNr2Use, DmaIndex, pMem);
                }
                if (pImgage == IntPtr.Zero)
                {
                    // Fehler
                    SetErrorCode(WrapperError.weNoPointer);
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
            }
            return pImgage;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve a an image in non blocking (or blocking mode)
        * \params   Param 
         *          SEL_NEW_IMAGE Selection of current acquired image (running process),
         *          SEL_ACT_IMAGE Selection of last acquired image, 
         *          SEL_NEXT_IMAGE Selection of next image after last get image 
         *          SEL_NUMBER Selection of an image by image number, not available in blocking mode!
         *          pMemHead memory pointer accroding to the fg_allocMem, Fg_allocMemHead function
         *\retval   buffer index according to mode defined at Param, indizes starting with 0.
         */
        //----------------------------------------------------------------------------------
        public long FgGetImageEx(int Param, // modes
                                 long PicNr, //frameindex_t
                                 uint DmaIndex,
                                 uint Timeout,
                                 IntPtr pMemHead)
        {
            ResetErrorCode();
            int fg_rc_int;
            long fg_rc  = -1;
            if (pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    long PicNr2Use = PicNr;
                    fg_rc = Fg_getImageEx64(pFGHandle, Param, PicNr2Use, DmaIndex, Timeout, pMemHead);
                }
                else
                {
                    if (PicNr > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return -1;
                    }
                    int PicNr2Use = (int)PicNr;
                    fg_rc_int = Fg_getImageEx32(pFGHandle, Param, PicNr2Use, DmaIndex, Timeout, pMemHead);
                    fg_rc = fg_rc_int;
                }
                if (fg_rc <  0)
                {
                    // Fehler
                    SetErrorCode(WrapperError.weSDK);
                }
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
            }
            return fg_rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	starting acquisition at a certain channel identified by CameraPort
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgAcquire(uint CameraPort, long NrOfImagesToGrab, int nFlag)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    fg_rc = Fg_Acquire64(pFGHandle, CameraPort, NrOfImagesToGrab);
                }
                else
                {
                    if (NrOfImagesToGrab > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return fgErrorType.fgeErrorWrapper;
                    }
                    int iNrOfImagesToGrab = (int)NrOfImagesToGrab;
                    fg_rc = Fg_Acquire32(pFGHandle, CameraPort, iNrOfImagesToGrab);
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	starting acquisition at a certain channel identified by CameraPort
        * \params   pMem: pointer to imageMemory, either user side allocated or grabber side allocated      
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgAcquireEx( uint CameraPort,
                                        long NrOfImagesToGrab,
                                        int nFlag,
                                        IntPtr pMem)
        {
            fgErrorType rc;
            int fg_rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                if (using64BitRuntime)
                {
                    fg_rc = Fg_AcquireEx64(pFGHandle, CameraPort, NrOfImagesToGrab, nFlag, pMem);
                }
                else
                {
                    if (NrOfImagesToGrab > Int32.MaxValue)
                    {
                        SetErrorCode(WrapperError.weParamCheck);
                        return fgErrorType.fgeErrorWrapper;
                    }
                    int iNrOfImagesToGrab = (int)NrOfImagesToGrab;
                    fg_rc = Fg_AcquireEx32(pFGHandle, CameraPort, iNrOfImagesToGrab, nFlag, pMem);
                }
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	stopping acquisition at a certain channel identified by CameraPort
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgStopAcquireEx(uint DmaIndex,
                                    IntPtr pMem,
                                    int nFlag)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_stopAcquireEx(pFGHandle, DmaIndex, pMem, nFlag);
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	stopping acquisition at a certain channel identified by CameraPort
        */
        //----------------------------------------------------------------------------------
        public fgErrorType FgStopAcquire(uint DmaIndex)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_stopAcquire(pFGHandle, DmaIndex);
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	request an image from camera by software call
        */
        //----------------------------------------------------------------------------------
        public fgErrorType Fg_sendSoftwareTriggerEx(uint CamPort, uint Triggers)
        {
            fgErrorType rc;
            int fg_rc;
            if (pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_sendSoftwareTriggerEx(pFGHandle, CamPort, Triggers);
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve the latest error as error code
        */
        //----------------------------------------------------------------------------------        
        public int FgGetLastErrorNumber()
        {
            return Fg_getLastErrorNumber(pFGHandle);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve last error as text
        */
        //----------------------------------------------------------------------------------        
        public string FgGetLastErrorDescription()
        {
            string rc;
            IntPtr pData;
            if (pFGHandle != IntPtr.Zero)
            {
                pData = Fg_getLastErrorDescription(pFGHandle);
                rc = Marshal.PtrToStringAnsi(pData);
            }
            else
            {
                rc = "Framegrabber not initialized";
            }

            char[] result = rc.ToCharArray();
            return new string(result);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve error description for given ID
        */
        //----------------------------------------------------------------------------------        
        public string FgGetErrorDescription(int ErrorNumber)
        {
            string rc;
            IntPtr pData;
            if (pFGHandle != IntPtr.Zero)
            {
                pData = Fg_getErrorDescription(pFGHandle, ErrorNumber);
                rc = Marshal.PtrToStringAnsi(pData);
            }
            else
            {
                rc = "Framegrabber not initialized";
            }

            char[] result = rc.ToCharArray();
            return new string(result);
        }

        ////////////////////////////////////////////////////////////////////////////////////
        // Parameter-Behandlung
        ////////////////////////////////////////////////////////////////////////////////////
        //----------------------------------------------------------------------------------
        /**
        * \brief	save applet parameters from File (*.mcf)
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgLoadConfig(string MCFConfigFileName)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_loadConfig(pFGHandle, MCFConfigFileName);
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	save current applet parameters to File (*.mcf)
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSaveConfig(string MCFConfigFileName)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            fg_rc = Fg_saveConfig(pFGHandle, MCFConfigFileName);
            rc = SiSoRc2FgeErrorType(fg_rc);
            return rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve an XML-based snapshot of all applet parameters
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgGetParameterInfoXML(int Port, out string xmlData)
        {

            int fg_rc = 0;
            fgErrorType rc;
            int BufSize;
            ResetErrorCode();
            xmlData = "";
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    int* pBufSize = &BufSize;
                    IntPtr pDataBufSize = (IntPtr)pBufSize;
                    try
                    {
                        fg_rc = Fg_getParameterInfoXML(pFGHandle, Port, IntPtr.Zero, pDataBufSize);
                        if (fg_rc == 0)
                        {
                            byte[] buffor = new byte[BufSize];
                            fixed (byte* pBuffor = buffor)
                            {
                                fg_rc = Fg_getParameterInfoXML_void(pFGHandle, Port, (void*)pBuffor, pDataBufSize);
                                if (fg_rc == 0)
                                {
                                    xmlData = System.Text.Encoding.ASCII.GetString(buffor);
                                }
                            }
                        }
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve the number of available applet parameters
        */
        //----------------------------------------------------------------------------------        
        public int FgGetNrOfParameter()
        {
            int NrOfParams = 0;
            ResetErrorCode();
            NrOfParams = Fg_getNrOfParameter(pFGHandle);
            return NrOfParams;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve the name of a certain parameter adressed by its index
        * \param	index index of the parameter according within the Range 0..FgGetNrOfParameter
        */          
        //----------------------------------------------------------------------------------        
        public string FgGetParameterName(int index)
        {
            string ParamName = "";
            ResetErrorCode();
            IntPtr pData = Fg_getParameterName(pFGHandle, index);
            ParamName = Marshal.PtrToStringAnsi(pData);
            char[] result = ParamName.ToCharArray();
            return new string(result);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	retrieve the parameter ID for a parameter identified by its name
        */
        //----------------------------------------------------------------------------------        
        public int FgGetParameterIdByName(string ParameterName)
        {
            int ParamID;
            ResetErrorCode();
            ParamID = Fg_getParameterIdByName(pFGHandle, ParameterName);
            if (ParamID <= 0)
            {
                SetErrorCode(WrapperError.weSDK);
            }
            return ParamID;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an integer parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName, int NewValue, uint CameraPort)
        {
            int ParamId;
            ResetErrorCode();
            ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, NewValue, CameraPort);
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an integer parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, int NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc;

            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    int* pNewValue = &NewValue;
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, pNewValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT32_T);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an unsighned integer parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName, uint NewValue, uint CameraPort)
        {
            int ParamId = 0;
            ResetErrorCode(); 
            ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, NewValue, CameraPort);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an unsigned integer parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, uint NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    uint* pNewValue = &NewValue;
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, pNewValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT32_T);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }


        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a long integer parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName, long NewValue, uint CameraPort)
        {
            int ParamId = 0;
            ResetErrorCode();
            ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, NewValue, CameraPort);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a long integer parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, long NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    long* pNewValue = &NewValue;
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, pNewValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT32_T);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an unsigned long integer parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName, ulong NewValue, uint CameraPort)
        {
            int ParamId = 0;
            ResetErrorCode();
            ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, NewValue, CameraPort);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of an unsigned long integer parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, ulong NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    ulong* pNewValue = &NewValue;
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, pNewValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT32_T);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a double parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName, double NewValue, uint CameraPort)
        {
            int ParamId = 0;
            ResetErrorCode();
            ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, NewValue, CameraPort);
        }
        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a double parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, double NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle!= IntPtr.Zero)
            {
                unsafe
                {
                    double* pNewValue = &NewValue;
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, pNewValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_DOUBLE);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading an double parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out double NewValue)
        {
            fgErrorType rc;
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgGetParameter(ParamId, CameraPort, out NewValue);
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	reading an unsigned long integer parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out double NewValue)
        {
            int fg_rc;
            fgErrorType rc;
            NewValue = 0;
            ResetErrorCode();
            if(pFGHandle!= IntPtr.Zero)
            {
                unsafe
                {
                    double value;
                    double* pValue = &value;
                    try
                    {
                        fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_DOUBLE);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                        NewValue = value;
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading a string parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out string NewValue)
        {
            int fg_rc;
            fgErrorType rc;
            NewValue = "";
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    byte[] cArray = new byte[maxSISOStringSize];
                    fixed (byte* pValue = cArray)
                    {
                        try
                        {
                            fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_CHAR_PTR);
                            NewValue = System.Text.Encoding.ASCII.GetString(cArray);
                            rc = SiSoRc2FgeErrorType(fg_rc);
                        }
                        catch
                        {
                            rc = fgErrorType.fgeErrorWrapper;
                            SetErrorCode(WrapperError.weException);
                        }
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	reading a string parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out string NewValue)
        {
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            return FgGetParameter(ParamId, CameraPort, out NewValue);
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a string parameter identified by its name and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameterByName(string ParameterName,  string NewValue, uint CameraPort)
        {
            int ParamId;
            fgErrorType rc;
            ResetErrorCode();
            ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgSetParameter(ParamId, NewValue, CameraPort);
            return rc;
        }

        //----------------------------------------------------------------------------------
        /**
        * \brief	setting the value of a string parameter identified by its ID and port
        */
        //----------------------------------------------------------------------------------        
        public fgErrorType FgSetParameter(int Parameter, string NewValue, uint CameraPort)
        {
            int fg_rc;
            fgErrorType rc = fgErrorType.fgeOK;
            NewValue += "\0";
            ResetErrorCode();
            if (pFGHandle!= IntPtr.Zero)
            {
               unsafe
               {
                    IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(NewValue);
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, Parameter, stringPointer, CameraPort, FgParamTypes.FG_PARAM_TYPE_CHAR_PTR);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }

                    Marshal.FreeHGlobal(stringPointer);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading an integer parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out int NewValue)
        {
            fgErrorType rc;
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgGetParameter(ParamId, CameraPort, out NewValue);
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading an integer parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out int NewValue)
        {
            int fg_rc;
            fgErrorType rc = fgErrorType.fgeErrorWrapper; 
            NewValue = 0;
            ResetErrorCode();
            if(pFGHandle!= IntPtr.Zero)
            {
                unsafe
                {
                    int value;
                    int* pValue = &value;
                    try
                    {
                        fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT32_T);
                        NewValue = value;
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading an unsigned integer parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out uint NewValue)
        {
            fgErrorType rc;
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgGetParameter(ParamId, CameraPort, out NewValue);
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	reading an unsigned integer parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out uint NewValue)
        {
            int fg_rc;
            fgErrorType rc = fgErrorType.fgeErrorWrapper;
            NewValue = 0;
            ResetErrorCode();
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    try
                    {
                        uint value;
                        uint* pValue = &value;
                        fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_UINT32_T);
                        NewValue = value;
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading a long integer parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out long NewValue)
        {
            fgErrorType rc;
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgGetParameter(ParamId, CameraPort, out NewValue);
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	reading a long integer parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out long NewValue)
        {
            int fg_rc;
            ResetErrorCode();
            fgErrorType rc = fgErrorType.fgeOK;
            NewValue = 0;
            if(pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    try
                    {
                        long value;
                        long* pValue = &value;
                        fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT64_T);
                        NewValue = value;
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading an unsigned long integer parameter identified by its name and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out ulong NewValue)
        {
            fgErrorType rc;
            ResetErrorCode();
            int ParamId = FgGetParameterIdByName(ParameterName);
            rc = FgGetParameter(ParamId, CameraPort, out NewValue);
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	reading an unsigned long integer parameter identified by its ID and port
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int Parameter, uint CameraPort, out ulong NewValue)
        {
            int fg_rc;
            ResetErrorCode();
            fgErrorType rc = fgErrorType.fgeErrorWrapper;
            NewValue = 0;
            if (pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    try
                    {
                        ulong value;
                        ulong* pValue = &value;
                        fg_rc = Fg_getParameterWithType(pFGHandle, Parameter, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_UINT64_T);
                        NewValue = value;
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	reading a certain parameter independend of its type and return it as a string
         *
         * \param	ParameterName	string of the paramter
         * \param	CameraPort      dma channel to read the parameter from
         * \param	NewValue        return value
         *
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByNameAsString(string ParameterName, uint CameraPort, out string NewValue)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            NewValue = "";
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                int ParamId;
                int fg_rc;
                ParamId = FgGetParameterIdByName(ParameterName);
                unsafe
                {
                    try
                    {
                        long ValueLong;
                        double ValueDbl;
                        void* pValue = &ValueLong;
                        fg_rc = Fg_getParameterWithType(pFGHandle, ParamId, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_INT64_T);
                        if (fg_rc < 0)
                        {
                            pValue = &ValueDbl;
                            try
                            {
                                fg_rc = Fg_getParameterWithType(pFGHandle, ParamId, pValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_DOUBLE);
                                if (fg_rc != 0)
                                {
                                    char[] buffor = new char[maxSISOStringSize];
                                    long size = buffor.Length;
                                    long* pSize = &size;
                                    fixed (char* pBuffor = buffor)
                                    {
                                        try
                                        {
                                            fg_rc = Fg_getParameterWithType(pFGHandle, ParamId, (void*)pBuffor, CameraPort, FgParamTypes.FG_PARAM_TYPE_CHAR_PTR);
                                            if (fg_rc == 0)
                                            {
                                                string ValueString;
                                                ValueString = new string(pBuffor);
                                                NewValue = ValueString;
                                            }
                                        }
                                        catch
                                        {
                                            rc = fgErrorType.fgeErrorWrapper;
                                            SetErrorCode(WrapperError.weException);
                                        }
                                    }
                                }
                                else
                                {
                                    NewValue = ValueDbl.ToString();
                                }
                            }
                            catch
                            {
                                rc = fgErrorType.fgeErrorWrapper;
                                SetErrorCode(WrapperError.weException);
                            }
                        }
                        else
                        {
                            NewValue = ValueLong.ToString();
                        }
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded getter for integer - fields parameters by Parametername
        */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, uint FieldIndex, out uint NewValue)
        {
            int ParamId = FgGetParameterIdByName(ParameterName);
            return FgGetParameter(ParamId, CameraPort, FieldIndex, out NewValue);
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded getter for integer - fields parameters
        */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameter(int ParamId, uint CameraPort, uint FieldIndex, out uint NewValue)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            NewValue = 0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    int fg_rc;
                    FieldParameterInt Value;
                    Value.value = NewValue; // readwrite
                    Value.index = FieldIndex;

                    IntPtr pFieldParameterInt = IntPtr.Zero;
                    int sizeofFieldParameterInt = Marshal.SizeOf(Value);
                    pFieldParameterInt = Marshal.AllocHGlobal(sizeofFieldParameterInt);
                    bool deleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterInt, deleteOld);

                    try
                    {
                        fg_rc = Fg_getParameterWithType(pFGHandle, ParamId, pFieldParameterInt, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMINT);
                        if (fg_rc == 0)
                        {
                            FieldParameterInt retVal;
                            retVal = (FieldParameterInt)Marshal.PtrToStructure(pFieldParameterInt, typeof(FieldParameterInt));
                            NewValue = retVal.value;
                        }
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                    Marshal.FreeHGlobal(pFieldParameterInt);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded setter for int - fields parameters by Name
        */
        //---------------------------------------------------------------------
        public fgErrorType FgSetParameterByName(string ParameterName, uint CameraPort, uint FieldIndex, uint NewValue)
        {
            int ParamId = FgGetParameterIdByName(ParameterName);
            return FgSetParameter(ParamId, CameraPort, FieldIndex, NewValue);
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded setter for int - fields parameters 
        */
        //---------------------------------------------------------------------
        public fgErrorType FgSetParameter(int ParamId, uint CameraPort, uint FieldIndex, uint NewValue)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            int fg_rc = 0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    IntPtr pFieldParameterInt = IntPtr.Zero;
                    FieldParameterInt Value;
                    Value.value = NewValue;
                    Value.index = FieldIndex;

                    // 1. erzeuge äußere Übergabestruktur
                    int sizeofFieldParameterInt = Marshal.SizeOf(Value);
                    pFieldParameterInt = Marshal.AllocHGlobal(sizeofFieldParameterInt);

                    // Kopiere Übergabestruktur mit Marshaller 
                    bool DeleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterInt, DeleteOld);
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, ParamId, pFieldParameterInt, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMINT);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                    Marshal.FreeHGlobal(pFieldParameterInt);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded getter for integer - fields parameters
        */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, uint FieldIndex, out double NewValue)
        {
            fgErrorType rc = fgErrorType.fgeOK;
            NewValue = 0;

            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                unsafe
                {
                    int ParamId;
                    int fg_rc;

                    ParamId = FgGetParameterIdByName(ParameterName);

                    FieldParameterDouble Value;
                    Value.value = NewValue; // readwrite
                    Value.index = FieldIndex;

                    IntPtr pFieldParameterDouble = IntPtr.Zero;
                    pFieldParameterDouble = Marshal.AllocHGlobal(sizeof(FieldParameterDouble));
                    bool DeleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterDouble, DeleteOld);
                    // Feuer
                    try
                    {
                        fg_rc = Fg_getParameterWithType(pFGHandle, ParamId, pFieldParameterDouble, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMDOUBLE);
                        if (fg_rc == 0)
                        {
                            FieldParameterDouble retVal;
                            retVal = (FieldParameterDouble)Marshal.PtrToStructure(pFieldParameterDouble, typeof(FieldParameterDouble));
                            NewValue = retVal.value;
                        }
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                    Marshal.FreeHGlobal(pFieldParameterDouble);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded setter for double - fields parameters
        */
        //---------------------------------------------------------------------
        public fgErrorType FgSetParameterByName(string ParameterName, uint CameraPort, uint FieldIndex, double NewValue)
        {
            int ParamId;
            fgErrorType rc;
            int fg_rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                ParamId = FgGetParameterIdByName(ParameterName);
                unsafe
                {
                    IntPtr pFieldParameterDouble = IntPtr.Zero;

                    FieldParameterDouble Value;
                    Value.value = NewValue;
                    Value.index = FieldIndex;

                    // generate transfer data
                    int sizeofFieldParameterDouble = Marshal.SizeOf(Value);
                    pFieldParameterDouble = Marshal.AllocHGlobal(sizeofFieldParameterDouble);

                    // copy argument to transfer data
                    bool DeleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterDouble, DeleteOld);

                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, ParamId, pFieldParameterDouble, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMDOUBLE);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        SetErrorCode(WrapperError.weException);
                    }
                    Marshal.FreeHGlobal(pFieldParameterDouble);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }


        //---------------------------------------------------------------------
        /**
        * \brief	overloaded getter for transfer of a complete array
         * retrieving an array of data from the applet starting at offset : StartIndex
         * data is stored at the target array beginning with offset 0.
         *  Param: 
         *  uint StartIndex, offset of the array related to the applet array, 
         *  uint NrOfElements array elements to be copied  
         *  Array (of ... int, uint, long, ulong)
        */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterByName(string ParameterName, uint CameraPort, out int[] pIntArray, int StartIndex, int NrOfElements)
        {
            int ParamId;
            fgErrorType rc;
            int fg_rc;
            ResetErrorCode();

            if (pFGHandle != IntPtr.Zero)
            {
                ParamId = FgGetParameterIdByName(ParameterName);
                IntPtr pFieldParameterAccessValue; //  Transfered data
                IntPtr pArrayData; // nested array in Struct

                unsafe
                {
                    pArrayData = IntPtr.Zero;
                    pFieldParameterAccessValue = IntPtr.Zero;

                    FieldParameterAccess Value = new FieldParameterAccess();
                    Value.count = (uint)NrOfElements;
                    Value.index = (uint)StartIndex;
                    Value.vtype = FgParamTypes.FG_PARAM_TYPE_INT32_T; // uint, int -> FgParamTypes.FG_PARAM_TYPE_INT32_T

                    // generate transport data
                    int sizeofFieldParameterAccess = Marshal.SizeOf(Value);
                    pFieldParameterAccessValue = Marshal.AllocHGlobal(sizeofFieldParameterAccess);

                    // generate Trasnport array / 2. erzeuge innere Struktur (array) für die Datenübergabe
                    int dummy = 0;
                    int sizeofArray = NrOfElements * Marshal.SizeOf(dummy);
                    pArrayData = Marshal.AllocHGlobal(sizeofArray);

                    // reference array at tranport data / referenziere innere Struktur (Array) in äußerer Struktur
                    Value.pData = pArrayData;

                    // copy argument to transfer data / kopiere äußere Übergabestruktur mit Marshaller 
                    bool DeleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterAccessValue, DeleteOld);

                    // initialise return values / Rückgabewert init.
                    int[] pData = new int[NrOfElements];
                    for (int i = 0; i < NrOfElements; i++)
                    {
                        pData[i] = 0;
                    }

                    // Feuer
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, ParamId, pFieldParameterAccessValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMACCESS);
                        if (fg_rc == 0)
                        {
                            Marshal.Copy(pArrayData, pData, 0, sizeofArray);
                            pIntArray = pData;
                        }
                        pIntArray = pData;
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        rc = fgErrorType.fgeErrorWrapper;
                        pIntArray = new int[1];// dummy
                        SetErrorCode(WrapperError.weException);
                    }

                    Marshal.FreeHGlobal(pArrayData);
                    Marshal.FreeHGlobal(pArrayData);
                }
            }
            else
            {
                pIntArray = new int[1];// dummy
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
        * \brief	overloaded setter for transfer of a complete array
        * under construction 
         *  Param: 
         *  uint StartIndex, 
         *  uint Count
         *  Array (of ... int, uint, long, ulong)
        */
        //---------------------------------------------------------------------
        public fgErrorType FgSetParameterByName(string ParameterName, uint CameraPort, int[] pUintArray, int StartIndex, int NrOfElements)
        {
            int ParamId;
            fgErrorType rc;
            int fg_rc;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {

                ParamId = FgGetParameterIdByName(ParameterName);
                IntPtr pFieldParameterAccessValue; //  Transfered data
                IntPtr pArrayData; // nested array in Struct

                unsafe
                {
                    pArrayData = IntPtr.Zero;
                    pFieldParameterAccessValue = IntPtr.Zero;

                    FieldParameterAccess Value = new FieldParameterAccess();
                    Value.count = (uint)NrOfElements;
                    Value.index = (uint)StartIndex;
                    Value.vtype = FgParamTypes.FG_PARAM_TYPE_INT32_T; // uint, int -> FgParamTypes.FG_PARAM_TYPE_INT32_T

                    // generate outer transfer data / 1. erzeuge äußere Übergabestruktur
                    int sizeofFieldParameterAccess = Marshal.SizeOf(Value);
                    pFieldParameterAccessValue = Marshal.AllocHGlobal(sizeofFieldParameterAccess);

                    // generate array transport data / 2. erzeuge innere Struktur (array) und kopiere das Array mit Marshaller
                    int dummy = 0;
                    int sizeofArray = NrOfElements * Marshal.SizeOf(dummy);
                    pArrayData = Marshal.AllocHGlobal(sizeofArray);
                    Marshal.Copy(pUintArray, StartIndex, pArrayData, NrOfElements);

                    // reference array at outer transfer data / referenziere innere Struktur (Array) in äußerer Struktur
                    Value.pData = pArrayData;

                    // Kopiere äußere Übergabestruktur mit Marshaller 
                    bool DeleteOld = false;
                    Marshal.StructureToPtr(Value, pFieldParameterAccessValue, DeleteOld);

                    // Feuer
                    try
                    {
                        fg_rc = Fg_setParameterWithType(pFGHandle, ParamId, pFieldParameterAccessValue, CameraPort, FgParamTypes.FG_PARAM_TYPE_STRUCT_FIELDPARAMACCESS);
                        rc = SiSoRc2FgeErrorType(fg_rc);
                    }
                    catch
                    {
                        SetErrorCode(WrapperError.weException);
                        rc = fgErrorType.fgeErrorWrapper;
                    }


                    Marshal.FreeHGlobal(pArrayData);
                    Marshal.FreeHGlobal(pFieldParameterAccessValue);
                }
            }
            else
            {
                rc = fgErrorType.fgeErrorWrapper;
                SetErrorCode(WrapperError.weNoPointer);
            }
            return rc;
        }


         //---------------------------------------------------------------------
        /**
         * \brief	delegation to the original object
         * event handler for aynchronous frame notification
         */
        //---------------------------------------------------------------------
        public fgErrorType FgGetParameterEx(int ParamId, uint CameraPort, IntPtr pMem, long ImgNr, out long NewValue)
       {
           fgErrorType rc;
           int fg_rc;
           ResetErrorCode();
           NewValue = 0;
           if (pFGHandle != IntPtr.Zero)
           {
               unsafe
               {
                   long retVal;
                   void* pValue = &retVal;
                   try
                   {
                       if (using64BitRuntime)
                       {
                           fg_rc = Fg_getParameterEx64(pFGHandle, ParamId, pValue, CameraPort, pMem, ImgNr);
                       }
                       else
                       {
                           if (ImgNr > Int32.MaxValue)
                           {
                               SetErrorCode(WrapperError.weParamCheck);
                               NewValue = 0;
                               return fgErrorType.fgeErrorWrapper;

                           }
                           int localImgNr = (int)ImgNr; //frameindex_t
                           fg_rc = Fg_getParameterEx32(pFGHandle, ParamId, pValue, CameraPort, pMem, localImgNr);
                       }
                       if (fg_rc == 0)
                       {
                           NewValue = retVal;
                       }
                       rc = SiSoRc2FgeErrorType(fg_rc);
                   }
                   catch
                   {
                       SetErrorCode(WrapperError.weException);
                       rc = fgErrorType.fgeErrorWrapper;
                   }
               }
           }
           else
           {
               rc = fgErrorType.fgeErrorWrapper;
               SetErrorCode(WrapperError.weNoPointer);
           }
           return rc;
       }

       //---------------------------------------------------------------------
       /**
        * \brief	delegation to the original object
        * event handler for aynchronous frame notification
        */
       //---------------------------------------------------------------------
       public fgErrorType FgGetParameterEx(int ParamId, uint CameraPort, IntPtr pMem, long ImgNr, out int NewValue)
       {
           fgErrorType rc;
           int fg_rc;
           ResetErrorCode();
           NewValue = 0; 
           if (pFGHandle != IntPtr.Zero)
           {
 
               unsafe
               {
                   long retVal;
                   void* pValue = &retVal;
                   try
                   {
                       if (using64BitRuntime)
                       {
                           fg_rc = Fg_getParameterEx64(pFGHandle, ParamId, pValue, CameraPort, pMem, ImgNr);
                       }
                       else
                       {
                           if (ImgNr > Int32.MaxValue)
                           {
                               SetErrorCode(WrapperError.weParamCheck);
                               NewValue = 0;
                               return fgErrorType.fgeErrorWrapper;

                           }
                           int localImgNr = (int)ImgNr; //frameindex_t
                           fg_rc = Fg_getParameterEx32(pFGHandle, ParamId, pValue, CameraPort, pMem, localImgNr);
                       }
                       if (fg_rc == 0)
                       {
                           string msg = FgGetLastErrorDescription();
                           NewValue = (int)retVal;
                       }
                       rc = SiSoRc2FgeErrorType(fg_rc);
                   }
                   catch
                   {
                       rc = fgErrorType.fgeErrorWrapper;
                       SetErrorCode(WrapperError.weException);
                   }
               }
           }
           else
           {
               rc = fgErrorType.fgeErrorWrapper;
               SetErrorCode(WrapperError.weNoPointer);
           }
           return rc;
       }


        //---------------------------------------------------------------------
       /**
        * \brief	delegation to the original object
        * event handler for aynchronous frame notification
        * 
        * \param	dmaIndex	camera / dma source of the data
        * \param	imgNr	    image no. of the notified imaged
        * \param	data        user data acc. to registerAPCCallback
        * 
        */
       //---------------------------------------------------------------------
        // Callback - delegate
        protected int DoOnAPCCallbackCalled(uint DMAIndex, long imgNr, IntPtr UserData)
        {
            int rc = 0;

            // fire callback
            try
            {
                ApcFuncCallback EventHandler = GetAPCEventHandler(DMAIndex);
                if (EventHandler != null)
                {
                    rc = EventHandler(imgNr, UserData);
                }
                ApcFuncCallbackEx EventHandlerEx = GetAPCExEventHandler(DMAIndex);
                if (EventHandlerEx != null)
                {
                    APCEvent apcEvent = new APCEvent(UserData, DMAIndex, imgNr);
                    rc = rc | EventHandlerEx(this, apcEvent);
                }
            }
            catch
            {
                rc = 1; // Emergency stop of callback
            }
            return rc;// != 0 -> stopping the callback
        }


        //---------------------------------------------------------------------
        /**
         * \brief	delegate / callback from SiSo Library
         *          
         * \param	imgNr	image no. of the notified imaged
         * \param	data    user data acc. to registerAPCCallback
         *
         * the notification from Siso in case of available images
         * passing the call to the registered object (static-non static)
         * 
         *
         * \retval	0
         */
        //---------------------------------------------------------------------
        static int OnFgAPCCallbackHandler(
#if _WIN64
                                        long imgNr, //frameindex_t
#else
                                        int imgNr, //frameindex_t
#endif
                                        IntPtr/*fg_apc_data */data)
        {
            int rc = 0;

            // get C# objects back from transport object
            // unmarshal Transport object to get Receiver, DMAIndex and Wrapperobject
            GCHandle hTransportObject = GCHandle.FromIntPtr(data);
            FgAPCTransferData TransferObj = (FgAPCTransferData)hTransportObject.Target;
            // Wrapper Object
            GCHandle hWrapperObj = GCHandle.FromIntPtr(TransferObj.mWrapperObject);
            Framegrabber WrapperObject = (Framegrabber)hWrapperObj.Target;
            // Client "User data"
            IntPtr UserData = TransferObj.mUserData;
            // Additional Info
            uint DMAIndex = TransferObj.mDMAIndex; 

            // Fire callback to client
            rc = WrapperObject.DoOnAPCCallbackCalled(DMAIndex, imgNr, UserData);

            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	registering an callback / delegate for notification about new images.
         * \param	Receiver	Object ot be notified
         * \param	callback    delegate to be fired
         * \param	DMAIndex    number of DMA-channel to attach
         * \param	Flags       see SiSo documentation of FG-library
         * \param   UserData    Pinned UserData to be passed to the eventHandler
         * \param   ImageTimeout time for new images in ms. handler gets stopped, if this 
         *                      time is elapsed (depending on FgApcControlFlags)
         *
         * The c- function pointer (static delegate) of the SiSo SDK gets mapped to a
         * method pointer (non static delegate) of the calling object which can be defined 
         * by the parameter: receiver 
         *
         * \retval	see SiSo-SDK function list
         */
        //---------------------------------------------------------------------
        public fgErrorType FgRegisterAPCHandler(Object Receiver, ApcFuncCallbackEx Callback, uint DMAIndex, FgApcControlFlags Flags, FgApcControlCtrlFlags CtrlFlags, IntPtr UserData, uint ImageTimeout = 5)
        {
            int fg_rc;
            fgErrorType rc;
            ResetErrorCode();
            if(pFGHandle!= IntPtr.Zero)
            {
                // APC setup structure 
                FgApcControl ApcControl = new FgApcControl();

                // Callback / delegate 
                Fg_ApcFunc_t APCCallback = new Fg_ApcFunc_t(OnFgAPCCallbackHandler);
                GCHandle hAPCCallback = GCHandle.Alloc(APCCallback);
                APCCallbackPins.Add(hAPCCallback);// for garbage collection

                IntPtr pAPCCallback = Marshal.GetFunctionPointerForDelegate(APCCallback);
                ApcControl.func = pAPCCallback;

                // user defined data to be passed to the delegate
                GCHandle hWrapperObject = GCHandle.Alloc(this);
                IntPtr pWrapperObject = GCHandle.ToIntPtr(hWrapperObject);
                APCCallbackPins.Add(hWrapperObject);

                // pinning of Client object
                IntPtr pReceiver = IntPtr.Zero;

                // Create Transfer Object for user data, wrapper object and additional information
                FgAPCTransferData APCData = new FgAPCTransferData(pWrapperObject, UserData, pReceiver, DMAIndex);
                GCHandle hTransferData = GCHandle.Alloc(APCData);
                IntPtr pTransferObject = GCHandle.ToIntPtr(hTransferData);
                APCCallbackPins.Add(hTransferData);

                // use transfer object as SDK's user data
                ApcControl.data = pTransferObject; // 
            
                // rest of setup
                ApcControl.flags = (uint)CtrlFlags; // controls APC behaviour
                ApcControl.timeout = ImageTimeout;
                ApcControl.version = 0;

                // pinning of the control structure, 
                GCHandle PinnedAPCControl = GCHandle.Alloc(ApcControl);
                IntPtr pAPCControl = GCHandle.ToIntPtr(PinnedAPCControl);

                fg_rc = Fg_registerApcHandler(pFGHandle, DMAIndex, ref ApcControl, Flags);
                rc = SiSoRc2FgeErrorType(fg_rc);

                // Put it to the list of Callback Handlers
                AddAPCEventHandler(DMAIndex, null, Callback); // Use extended Callback
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }
        //---------------------------------------------------------------------
        /**
         * \brief	removing the registered APC handler
         * \param	DMAIndex    number of DMA-channel to attach
         *
         * the reference to the delegate / function pointer will be removed
         */
        //---------------------------------------------------------------------
        public fgErrorType FgUnRegisterAPCHandler(uint DMAIndex)
        {
            fgErrorType rc;
            int fg_rc;

            ResetErrorCode();
            RemoveAPCEventHandler(DMAIndex);
            if (pFGHandle != IntPtr.Zero)
            {
                fg_rc = Fg_registerApcHandlerIntPtr(pFGHandle, DMAIndex, IntPtr.Zero, 0);
                rc = SiSoRc2FgeErrorType(fg_rc);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	delegation to the original object
         * event handler for aynchronous event notification
         * \retval	return code of the call to the receiver
         */
        //---------------------------------------------------------------------
        // Callback - delegate
        protected int DoOnEventCallbackCalled( ulong eventMask, fg_event_info ev, IntPtr UserData)
        {
            int rc = 0;
            // fire callback
            try
            {
                unsafe
                {
                    ulong timestamp = ev.timestamp[0];
                    FramegrabberEvent GrabberEvent = new FramegrabberEvent(timestamp, UserData, eventMask, ev);

                    int index = FindEventMaskIndex(eventMask);
                    while (index >= 0)
                    {
                        SiSoEventFuncCallback CB = EventCallbackList[index];
                        if (CB != null)
                        {
                            rc = CB(this, GrabberEvent);
                        }
                        index = FindEventMaskIndex(eventMask, index+1);
                    }
                }
            }
            catch
            {
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	search the callbackhandler for the according event mask at the internal administration
         * \param	Startindex  first index of the list, from which the search begins, usage for stepping though the list
         * \retval	index       of the first matching mask
         */
        //---------------------------------------------------------------------
        private int FindEventMaskIndex(ulong eventMask, int Startindex = 0)
        {
            int index = -1;
            if (Startindex < 0) return index;
            for (int i = Startindex; i < EventCallbackMasks.Count; i++)
            {
                ulong mask = EventCallbackMasks[i];
                if ((mask & eventMask) != 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	removes all Callback Handlers for the according mask from internal administration
         */
        //---------------------------------------------------------------------
        private void RemoveEventCalback(ulong eventMask)
        {
            int index = FindEventMaskIndex(eventMask); 
            while (index >= 0){
                EventCallbackMasks.RemoveAt(index);
                EventCallbackList.RemoveAt(index);
                index = FindEventMaskIndex(eventMask, index+1); 
            }
        }

        //---------------------------------------------------------------------
        /**
         * \brief	Initializes the event structure
         */
        //---------------------------------------------------------------------
        private static void InitEventInfo(ref fg_event_info evi)
        {
            unsafe
            {
                evi.version = 2;
                evi._pad = 0;
                evi.length = 0;
                fixed (fg_event_info* pEvi = &evi)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        pEvi->timestamp[i] = 0;
                        pEvi->notify[i] = 0;
                    }

                    for (int i = 0; i < 254; i++)
                    {
                        pEvi->data[i] = 0;
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        /**
         * \brief	copy an event structure
         */
        //---------------------------------------------------------------------
        public void CopyEventInfo(ref fg_event_info EventInfoSource, ref fg_event_info EventInfoDest)
        {
            EventInfoDest.version = EventInfoSource.version;
            EventInfoDest._pad = EventInfoSource._pad;
            EventInfoDest.length = EventInfoSource.length;
            unsafe
            {
                fixed (fg_event_info* pSource = &EventInfoSource)
                {
                    fixed (fg_event_info* pDest = &EventInfoDest)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            pDest->timestamp[i] = pSource->timestamp[i];
                            pDest->notify[i] = pSource->notify[i];
                        }

                        for (int i = 0; i < 254; i++)
                        {
                            pDest->data[i] = pSource->data[i];
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        /**
         * \brief	Event Callback Handler
         * \param	eventMask   bitmask of the event occurred
         * \param	pData       context-data: pointer to the registered receiver object
         * \param	pEventInfo  SiSo data structure for additional event information
         */
        //---------------------------------------------------------------------
        static int OnFgEventCallbackHandler(ulong events, IntPtr data, IntPtr pEventInfo)
        {
            int rc = 0; // Rückgabewert für EventHandler. set != 0 zum abbruch des Callback
            // Zurückholen des C# Empfängers
            if (data != IntPtr.Zero)
            {
                // get C# objects back from transport object
                // unmarshal Transport object to get Receiver, DMAIndex and Wrapperobject
                GCHandle hTransportObject = GCHandle.FromIntPtr(data);
                FgEventTransferdata TransferObj = (FgEventTransferdata)hTransportObject.Target;
                // Wrapper Object
                GCHandle hWrapperObj = GCHandle.FromIntPtr(TransferObj.mWrapperObject);
                Framegrabber WrapperObject = (Framegrabber)hWrapperObj.Target;
                // Client "User data"
                IntPtr UserData = TransferObj.mUserData;
                // Additional Info not present so far

                // Fire callback to client
                if (pEventInfo != IntPtr.Zero)
                {
                    fg_event_info EventInfo = new fg_event_info();
                    InitEventInfo(ref EventInfo);
                    EventInfo = (fg_event_info)Marshal.PtrToStructure(pEventInfo, typeof(fg_event_info));
                    rc = WrapperObject.DoOnEventCallbackCalled(events, EventInfo, UserData);
                }
                else
                {
                    fg_event_info EventInfo = new fg_event_info();
                    InitEventInfo(ref EventInfo);
                    rc = WrapperObject.DoOnEventCallbackCalled(events, EventInfo, UserData);
                }
            }
            return rc;
        }


        //---------------------------------------------------------------------
        /**
         * \brief	register a callback handler for receiving eventnotfications from the grabber.
         * \param	object      receiver object, that should get notified
         * \param	eventMask   bitmask representing the according event, which shall be traced
         * \param   Callback    delegate for notification
         * \param   data        additional context data
         * \param   flags       various control flas, details see Siso-SDK documentation
         */
        //---------------------------------------------------------------------
        public fgErrorType FgRegisterEventCallback( Object Receiver, 
                                                    ulong eventMask, 
                                                    SiSoEventFuncCallback Callback, 
                                                    IntPtr UserData, 
                                                    FgEventControlFlags flags)
        {
            unsafe{
                fgErrorType rc;
                int fg_rc = 0;
                if (pFGHandle != IntPtr.Zero)
                {
                    // Event notification structure 
                    fg_event_info EventInfo = new fg_event_info();
                    EventInfo.version = 2;

                    // Callback / delegate
                    Fg_EventFunc_t EventCallback = new Fg_EventFunc_t(OnFgEventCallbackHandler);
                    GC.KeepAlive(EventCallback);
                    GCHandle hPinnedEventCallback = GCHandle.Alloc(EventCallback);
                    IntPtr pEventCallback = Marshal.GetFunctionPointerForDelegate(EventCallback);
                    EventCallbackPins.Add(hPinnedEventCallback);// for garbage collection

                    try
                    {
                        //----------------------------------------------------------
                        // pinning of the transport - struct
                        //----------------------------------------------------------
                        IntPtr pEventInfo = Marshal.AllocHGlobal(sizeof(fg_event_info));
                        Marshal.StructureToPtr(EventInfo, pEventInfo, false);

                        GCHandle hEventInfo = GCHandle.Alloc(pEventInfo);
                        IntPtr pEventInfoPinned = GCHandle.ToIntPtr(hEventInfo);
                        EventCallbackPins.Add(hEventInfo);// for garbage collection

                        //----------------------------------------------------------
                        // user defined data to be passed to the delegate. 
                        //----------------------------------------------------------
                        GCHandle hWrapperObject = GCHandle.Alloc(this);
                        IntPtr pWrapperObject = GCHandle.ToIntPtr(hWrapperObject);
                        EventCallbackPins.Add(hWrapperObject);

                        // User Object
                        GCHandle hReceiver = GCHandle.Alloc(Receiver);
                        IntPtr pReceiver = GCHandle.ToIntPtr(hReceiver);
                        EventCallbackPins.Add(hReceiver);

                        // Create Transfer Object for user data, wrapper object and additional information
                        FgEventTransferdata EventData = new FgEventTransferdata(pWrapperObject, UserData, pReceiver);
                        GCHandle hTransferData = GCHandle.Alloc(EventData);
                        IntPtr pTransferObject = GCHandle.ToIntPtr(hTransferData);
                        EventCallbackPins.Add(hTransferData);

                        fg_rc = Fg_registerEventCallbackIntPtr(pFGHandle, eventMask, pEventCallback,  pTransferObject, (uint)flags, pEventInfo);

                        // store eventhandlers for event-notification / für das befeuern der Schnittstelle alle registrierten callbacks vorhalten
                        EventCallbackMasks.Add(eventMask);
                        EventCallbackList.Add(Callback);

                        rc = SiSoRc2FgeErrorType(fg_rc);

                        // Do not free Transport data, since this is kept inside the FGLIB
                        //Marshal.FreeHGlobal();
                        MarshalledData.Add(pEventInfo); // for later garbage collection

                        GC.KeepAlive(pEventCallback);
                        GC.KeepAlive(hEventInfo);
                    }
                    catch
                    {
                        SetErrorCode(WrapperError.weException);
                        rc = fgErrorType.fgeErrorWrapper;
                    }
                }
                else
                {
                    SetErrorCode(WrapperError.weNoPointer);
                    rc = fgErrorType.fgeErrorWrapper;
                }
                return rc;
             }
        }

        //---------------------------------------------------------------------
        /**
         * \brief	unregister the event notification
         * \param	eventMask   bitmask representing the according event
         */
        //---------------------------------------------------------------------
        public fgErrorType FgUnRegisterEventCallback(ulong eventMask)
        {
            int fg_rc;
            fgErrorType rc;

            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                uint flags = 0;
                fg_rc = Fg_registerEventCallbackIntPtr(pFGHandle, eventMask, IntPtr.Zero, IntPtr.Zero, flags, IntPtr.Zero);
                rc = SiSoRc2FgeErrorType(fg_rc);

                // remove all callbacks for the mask from Admin-structure
                RemoveEventCalback(eventMask);
            }
            else
            {
                SetErrorCode(WrapperError.weNoPointer);
                rc = fgErrorType.fgeErrorWrapper;
            }
            return rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	Wait for occurance of a certain event represented by the bitmask
         * \param	eventMask   bitmask representing the according event
         * \retval	event mask of the detect event, 0 if no event was detected
         */
        //---------------------------------------------------------------------
        public ulong FgEventWait(ulong eventMask, uint timeout, FgEventControlFlags flags, out fg_event_info EventInfo)
        {
            ulong fg_rc;
            ResetErrorCode();

            fg_event_info EventInfoInt = new fg_event_info();
            unsafe
            {
                EventInfoInt.version = 2;
                EventInfoInt._pad = 0;
                EventInfoInt.length = 0;
                for (int i = 0; i < 64; i++)
                {
                    EventInfoInt.timestamp[i] = 0;
                    EventInfoInt.notify[i] = 0;
                }

                for (int i = 0; i < 254; i++)
                {
                    EventInfoInt.data[i] = 0;
                }
            }

            EventInfo = EventInfoInt;

            fg_rc = Fg_eventWait(pFGHandle, eventMask, timeout, (uint)flags, ref  EventInfoInt);
            if (fg_rc != 0)
            {
                InitEventInfo(ref EventInfo);
                CopyEventInfo(ref EventInfoInt, ref EventInfo);
            }
            return fg_rc;
        }

        //---------------------------------------------------------------------
        /**
         * \brief	retrieve the name for an event indicated by a certain bit at the bitmask
         * \param	eventMask   bitmask representing the according event
         *
         * \retval	string containing the name of the event. 
         *          Empty string in case of an invalid bitmask
         */
        //---------------------------------------------------------------------
        public string FgGetEventName(ulong eventMask)
        {
            string rc;
            IntPtr pData;

            ResetErrorCode();

            if (pFGHandle != IntPtr.Zero)
            {
                pData = Fg_getEventName(pFGHandle, eventMask);
                rc = Marshal.PtrToStringAnsi(pData);
            }
            else
            {
                rc = "";
            }
            char[] result = rc.ToCharArray();
            return new string(result);
        }


        //---------------------------------------------------------------------
        /**
         * \brief	retrieve the event mask for a specific event identified by its name
         * \param	eventName   name of the event specific to firmware and applet
         * \retval	bitmask for the according event
         * 
         */
        //---------------------------------------------------------------------
        public ulong FgGetEventMask(string eventName)
        {
            ulong rc = 0;
            ResetErrorCode();
            if (pFGHandle != IntPtr.Zero)
            {
                rc = Fg_getEventMask(pFGHandle, eventName);
            }
            return rc;
        }


        
        //---------------------------------------------------------------------
        /**
         * \brief	query for available events.  
         * \param	mask  [out]  return value representing the bitmask of the detected event
         *
         * \retval	string containing the name of the detected event. Empty string 
         *          if no event is available at all    
         */
        //---------------------------------------------------------------------
        public string FgGetFirstEvent(out ulong eventMask)
        {
            string rc;
            IntPtr pData;
            ulong QueryMask = 0x01;
            bool found = false;

            ResetErrorCode();

            eventMask = 0;
            if (pFGHandle != IntPtr.Zero)
            {
                rc = "";
                while ((rc == "") && (!found) && (QueryMask != 0))
                {
                    pData = Fg_getEventName(pFGHandle, QueryMask);
                    if (pData != IntPtr.Zero)
                    {
                        rc = Marshal.PtrToStringAnsi(pData);
                        if (QueryMask == 0)
                        {
                            break;
                        }
                        if (rc != "")
                        {
                            found = true;
                            eventMask = QueryMask;
                            EventMaskIterator = QueryMask;
                            break;
                        }
                    }
                    QueryMask = QueryMask << 0x1; // shl
                }
            }
            else
            {
                rc = "";

            }
            char[] result = rc.ToCharArray();
            return new string(result);
        }

        //---------------------------------------------------------------------
        /**
         * \brief	query for available events.  
         * \param	mask    return value representing the bitmask of the detected event
         *
         * \retval	string containing the name of the detected event. Empty string if 
         *          no further event is available 
         */
        //---------------------------------------------------------------------
        public string FgGetNextEvent(out ulong eventMask)
        {
            string rc;
            IntPtr pData;
            ulong QueryMask = EventMaskIterator;
            bool found = false;

            ResetErrorCode();

            eventMask = 0;
            rc = "";
            if (pFGHandle != IntPtr.Zero)
            {
                rc = "";
                QueryMask = QueryMask << 1;
                while ((rc == "") && (!found) && (QueryMask!= 0))
                {
                    pData = Fg_getEventName(pFGHandle, QueryMask);
                    if (pData != IntPtr.Zero)
                    {
                        rc = Marshal.PtrToStringAnsi(pData);
                        if (rc != "")
                        {
                            found = true;
                            eventMask = QueryMask;
                            EventMaskIterator = QueryMask;
                            break;
                        }
                    }
                    QueryMask = QueryMask << 0x1; // shl
                }
            }
            else
            {
                rc = "";

            }
            char[] result = rc.ToCharArray();
            return new string(result);
        }
        //---------------------------------------------------------------------
        /**
         * \brief	activate the event notification for one or multiple events 
         *          according to the bitmask
         * \param	mask    the bitmask of the event(s) to be activated
         * \param   enable  switch event notification on(1) or off (0)
         *
         * \retval	see the SDK function list to obtain additional description
         */
        //---------------------------------------------------------------------
        public fgErrorType FgActivateEvents(ulong mask, uint enable)
        {
            int fg_rc;
            ResetErrorCode();
            fg_rc = Fg_activateEvents(pFGHandle, mask, enable);
            return SiSoRc2FgeErrorType(fg_rc);
        }
        //---------------------------------------------------------------------
        /**
         * \brief	removes all pending events
         * \retval	see the SDK function list to obtain additional description
         */
        //---------------------------------------------------------------------
        public fgErrorType FgClearEvents(ulong mask)
        {
            int fg_rc;
            ResetErrorCode();
            fg_rc = Fg_clearEvents(pFGHandle, mask);
            return SiSoRc2FgeErrorType(fg_rc);
        }
    }
}

