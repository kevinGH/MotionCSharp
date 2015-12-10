using System;
using System.Runtime.InteropServices;

namespace NUUO_SDK
{
    public enum Np_Result
    {
        Np_Result_OK =                 0,
        Np_Result_CLI_FAULT =          1,
        Np_Result_SVR_FAULT =          2,
        Np_Result_USER_ERROR =         3,
        Np_Result_FAILED =             4,
        Np_Result_NO_METHOD =          5,
        Np_Result_NO_DATA =            6,
        Np_Result_SOCKET_ERROR =       7,
        Np_Result_INVALID_ARGUMENT =   8,
        Np_Result_FATAL_ERROR =        9 // handle should be destroyed
    }

    public enum Np_Error
    {
        Np_ERROR_CODE_BASE = 1000,
        Np_ERROR_CONNECT_SUCCESS =     Np_ERROR_CODE_BASE+1,
        Np_ERROR_CONNECT_ERROR =       Np_ERROR_CODE_BASE+2,
        Np_ERROR_DISCONNECT_SUCCESS =  Np_ERROR_CODE_BASE+3,
        Np_ERROR_SESSION_NODATA =      Np_ERROR_CODE_BASE+4,
        Np_ERROR_SESSION_LOST =        Np_ERROR_CODE_BASE+5,
        Np_ERROR_EOF =                 Np_ERROR_CODE_BASE+6
    }

    public enum Np_Event_Type
    {
        Np_EVENT_CODE_BASE =                                        3000,
        Np_EVENT_USER_DEFINED_CODE_BASE =                           8000,
        //------for source entity type: general server
        Np_EVENT_RESOURCE_DEPLETED =                                Np_EVENT_CODE_BASE+0,
        Np_EVENT_NETWORK_CONGESTION =                               Np_EVENT_CODE_BASE+1,
        Np_EVENT_SYSTEM_HEALTH_UNUSUAL =                            Np_EVENT_CODE_BASE+2,
        Np_EVENT_RESOURCE_DEPLETED_STOP =                           Np_EVENT_CODE_BASE+50,
        Np_EVENT_NETWORK_CONGESTION_STOP =                          Np_EVENT_CODE_BASE+51,
        Np_EVENT_SYSTEM_HEALTH_UNUSUAL_STOP =                       Np_EVENT_CODE_BASE+52,
        //------for source entity type: archiver server
        Np_EVENT_DISK_SPACE_EXHAUSTED =                             Np_EVENT_CODE_BASE+100,
        Np_EVENT_DISK_ABNORMAL =                                    Np_EVENT_CODE_BASE+101,
        Np_EVENT_SERVER_AUTO_BACKUP_START =                         Np_EVENT_CODE_BASE+106,
        Np_EVENT_SERVER_AUTO_BACKUP_STOP =                          Np_EVENT_CODE_BASE+107,
        Np_EVENT_SERVER_AUTO_BACKUP_FAIL =                          Np_EVENT_CODE_BASE+108,
        Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE =                    Np_EVENT_CODE_BASE+109,
        Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE =                Np_EVENT_CODE_BASE+110,
        Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS =                  Np_EVENT_CODE_BASE+111,
        Np_EVENT_SERVER_CONNECTION_LOST	=					        Np_EVENT_CODE_BASE+112,
        Np_EVENT_DISK_SPACE_EXHAUSTED_STOP =                        Np_EVENT_CODE_BASE+150,
        //------for source entity type: sensor (MainConsole only)
        Np_EVENT_GENERAL_MOTION_1 =                                 Np_EVENT_CODE_BASE+180,
        Np_EVENT_GENERAL_MOTION_2 =                                 Np_EVENT_CODE_BASE+181,
        Np_EVENT_GENERAL_MOTION_3 =                                 Np_EVENT_CODE_BASE+182,
        Np_EVENT_GENERAL_MOTION_4 =                                 Np_EVENT_CODE_BASE+183,
        Np_EVENT_GENERAL_MOTION_5 =                                 Np_EVENT_CODE_BASE+184,
        Np_EVENT_FOREIGN_OBJECT =                                   Np_EVENT_CODE_BASE+185,
        Np_EVENT_MISSING_OBJECT =                                   Np_EVENT_CODE_BASE+186,
        Np_EVENT_FOCUS_LOST =                                       Np_EVENT_CODE_BASE+187,
        Np_EVENT_CAMERA_OCCLUSION =                                 Np_EVENT_CODE_BASE+188,
        Np_EVENT_GENERAL_MOTION_DEVICE =                            Np_EVENT_CODE_BASE+189,
        Np_EVENT_COUNTING =                                         Np_EVENT_CODE_BASE+190,
        Np_EVENT_COUNTING_STOP =                                    Np_EVENT_CODE_BASE+191,
        Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_START =              Np_EVENT_CODE_BASE+192, //description: "EventId:xxxxxxxx"
        Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_STOP =               Np_EVENT_CODE_BASE+193, //description: "EventId:xxxxxxxx"
        Np_EVENT_SMART_GUARD_SOUND_ALERT_START =                    Np_EVENT_CODE_BASE+194, //description: "EventId:xxxxxxxx"
        Np_EVENT_SMART_GUARD_SOUND_ALERT_STOP =                     Np_EVENT_CODE_BASE+195, //description: "EventId:xxxxxxxx"
        Np_EVENT_RECORD_STATUS_UPDATE =                             Np_EVENT_CODE_BASE+196, //description: "Mode:None/Always/Event/Motion/Boost"
        Np_EVENT_TALK_BE_RESERVED =                                 Np_EVENT_CODE_BASE+197, //description: "User:%s""
        Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_START =                Np_EVENT_CODE_BASE + 198, //description: "EventId:xxxxxxxx"
        Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_STOP =                 Np_EVENT_CODE_BASE + 199, //description: "EventId:xxxxxxxx"
        //------for source entity type: sensor
        Np_EVENT_SIGNAL_LOST =                                      Np_EVENT_CODE_BASE+200,
        Np_EVENT_SIGNAL_RESTORE =                                   Np_EVENT_CODE_BASE+201,
        Np_EVENT_MOTION_START =                                     Np_EVENT_CODE_BASE+202,
        Np_EVENT_MOTION_STOP =                                      Np_EVENT_CODE_BASE+203,
        Np_EVENT_RECORD_ON_MOTION_START =                           Np_EVENT_CODE_BASE+204,
        Np_EVENT_RECORD_ON_MOTION_STOP =                            Np_EVENT_CODE_BASE+205,
        Np_EVENT_RECORD_ON_EVENT_START =                            Np_EVENT_CODE_BASE+206,
        Np_EVENT_RECORD_ON_EVENT_STOP =                             Np_EVENT_CODE_BASE+207,
        Np_EVENT_RECORD_ON_MANUAL_START =                           Np_EVENT_CODE_BASE+208,
        Np_EVENT_RECORD_ON_MANUAL_STOP =                            Np_EVENT_CODE_BASE+209,
        Np_EVENT_RECORD_ON_SCHEDULE_START =                         Np_EVENT_CODE_BASE+210,
        Np_EVENT_RECORD_ON_SCHEDULE_STOP =                          Np_EVENT_CODE_BASE+211,
        Np_EVENT_RECORD_START =                                     Np_EVENT_CODE_BASE+212,
        Np_EVENT_RECORD_STOP =                                      Np_EVENT_CODE_BASE+213,
        Np_EVENT_BACKUP_AUTO_START =                                Np_EVENT_CODE_BASE+214,
        Np_EVENT_BACKUP_AUTO_STOP =                                 Np_EVENT_CODE_BASE+215,
        Np_EVENT_BACKUP_AUTO_FAIL =                                 Np_EVENT_CODE_BASE+216,
        Np_EVENT_BACKUP_MANUAL_START =                              Np_EVENT_CODE_BASE+217,
        Np_EVENT_BACKUP_MANUAL_STOP =                               Np_EVENT_CODE_BASE+218,
        Np_EVENT_BACKUP_MANUAL_FAIL =                               Np_EVENT_CODE_BASE+219,
        Np_EVENT_EXPORT_START =                                     Np_EVENT_CODE_BASE+220,
        Np_EVENT_EXPORT_STOP =                                      Np_EVENT_CODE_BASE+221,
        Np_EVENT_EXPORT_FAIL =                                      Np_EVENT_CODE_BASE+222,
        Np_EVENT_MANUAL_RECORD_MODE_START =                         Np_EVENT_CODE_BASE+223,
        Np_EVENT_MANUAL_RECORD_MODE_STOP =                          Np_EVENT_CODE_BASE+224,	
        //------for source entity type: POS (MainConsole only)
        Np_EVENT_TRANSACTION_START =                                Np_EVENT_CODE_BASE+300,
        Np_EVENT_TRANSACTION_STOP =                                 Np_EVENT_CODE_BASE+301,
        Np_EVENT_CASH_DRAWER_OPENED =                               Np_EVENT_CODE_BASE+302,
        /*POSTPONE*/ //NUUO_EVENT_CASH_DRAWER_CLOSED =              Np_EVENT_CODE_BASE+303,
        Np_EVENT_USER_DEFINE_RULE_1 =                               Np_EVENT_CODE_BASE+304,
        Np_EVENT_USER_DEFINE_RULE_2 =                               Np_EVENT_CODE_BASE+305,
        Np_EVENT_USER_DEFINE_RULE_3 =                               Np_EVENT_CODE_BASE+306,
        Np_EVENT_USER_DEFINE_RULE_4 =                               Np_EVENT_CODE_BASE+307,
        Np_EVENT_USER_DEFINE_RULE_5 =                               Np_EVENT_CODE_BASE+308,
        Np_EVENT_USER_DEFINE_RULE_6 =                               Np_EVENT_CODE_BASE+309,
        Np_EVENT_USER_DEFINE_RULE_7 =                               Np_EVENT_CODE_BASE+310,
        Np_EVENT_USER_DEFINE_RULE_8 =                               Np_EVENT_CODE_BASE+311,
        Np_EVENT_USER_DEFINE_RULE_9 =                               Np_EVENT_CODE_BASE+312,
        Np_EVENT_USER_DEFINE_RULE_10 =                              Np_EVENT_CODE_BASE+313,
        //------for source entity type: digital input
        Np_EVENT_INPUT_CLOSED =                                     Np_EVENT_CODE_BASE+400,
        Np_EVENT_INPUT_OPENED =                                     Np_EVENT_CODE_BASE+401,
        //------for source entity type: output relay,
        Np_EVENT_OUTPUT_ON =                                        Np_EVENT_CODE_BASE+500,
        Np_EVENT_OUTPUT_OFF =                                       Np_EVENT_CODE_BASE+501,
        //------for source entity type: unit,
        Np_EVENT_CONNECTION_LOST =                                  Np_EVENT_CODE_BASE+600,
        Np_EVENT_CONNECTION_RESTORE =                               Np_EVENT_CODE_BASE+601,
        //------for source entity type: archiver server (Mini&Mini2 only)
        Np_EVENT_DAILY_REPORT =                                     Np_EVENT_CODE_BASE+2000,
        Np_UNABLE_ACCESS_FTP =                                      Np_EVENT_CODE_BASE+2001,
        Np_UNFINISHED_BACKUP =                                      Np_EVENT_CODE_BASE+2002,
        //------for source entity type: different entity type
        Np_EVENT_PHYSICAL_DEVICE_TREELIST_UPDATED =                 Np_EVENT_CODE_BASE+3000,
        Np_EVENT_LOGICAL_DEVICE_TREELIST_UPDATED =                  Np_EVENT_CODE_BASE+3001,
        Np_EVENT_DEVICE_TREELIST_UPDATED =                          Np_EVENT_CODE_BASE+3002,
        Np_EVENT_CAROUSEL_LIST_UPDATED =                            Np_EVENT_CODE_BASE+3003,
        Np_EVENT_PANORAMA_LIST_UPDATED =                            Np_EVENT_CODE_BASE+3004,
        Np_EVENT_AOI_LIST_UPDATED =                                 Np_EVENT_CODE_BASE+3005,
        Np_EVENT_EMAP_TREELIST_UPDATED =                            Np_EVENT_CODE_BASE+3006,
        Np_EVENT_EMAP_IMAGE_ID_LIST_UPDATED =                       Np_EVENT_CODE_BASE+3007,
        Np_EVENT_EMAP_IMAGE_ICON_ID_LIST_UPDATED =                  Np_EVENT_CODE_BASE+3008,
        Np_EVENT_LAYOUT_LIST_UPDATED =                              Np_EVENT_CODE_BASE+3009,
        Np_EVENT_SHARE_VIEW_TREE_UPDATED =                          Np_EVENT_CODE_BASE+3010,
        Np_EVENT_PRIVATE_VIEW_TREE_UPDATED =                        Np_EVENT_CODE_BASE+3011,
        Np_EVENT_PRIVILEGE_ACCESS_RIGHT_UPDATED =                   Np_EVENT_CODE_BASE+3012,
        Np_EVENT_PASSWORD_CHANGED =				                    Np_EVENT_CODE_BASE+3013,
        Np_EVENT_ACCOUNT_DELETE =                  				    Np_EVENT_CODE_BASE+3014,
        Np_EVENT_SERVICE_NETWORK_SETTING_CHANGED =    			    Np_EVENT_CODE_BASE+3015
    }

    //------Source Device Type ID
    //------for Info_EventQuery
    public enum Np_Source_Device_Type
    {
        Np_SOURCE_DEVICE_EMPTY =                                    0,
        Np_SOURCE_DEVICE_SENSOR =                                   1,
        Np_SOURCE_DEVICE_DIGITAL_INPUT =                            2,
        Np_SOURCE_DEVICE_SERVER =                                   3
    }

    public enum Np_ServerType
    {
        kMainConsoleLiveview = 1,
        kMainConsolePlayback,
        kCorporate,
        kTitan
    }

    public enum Np_VideoCodec
    {
        kVideoCodecNone = 0,
        kVideoCodecMJPEG = 1,
        kVideoCodecMPEG4 = 2,
        kVideoCodecH264 = 3,
        kVideoCodecMXPEG = 4,
    }

    public enum Np_AudioCodec
    {
        kAudioCodecNone = 0,
        kAudioCodecAAC = 1,
        kAudioCodecACM = 2,
        kAudioCodecMSADPCM = 3,
        kAudioCodecG711Alaw = 4,
        kAudioCodecG711Mulaw = 5,
        kAudioCodecG726 = 6,
        kAudioCodecGSM_AMR = 7,
        kAudioCodecPCM = 8,
        kAudioCodecG7221 = 9,
        kAudioCodecGSM610 = 10,
    }

    public enum Np_PixelFormat
    {
        kPixelFormatYUV420P = 0,
        kPixelFormatRGB24 = 1,
        kPixelFormatBGR24 = 2,
    }

    public enum Np_ExportError
    {
        kExportSuccess = 0,
        kExportUpdateProgress = 1,
        kExportCallbackError = 2,
        kExportChannelEmpty = 3,
        kExportCueTimeError = 4,
        kExportDimesionError = 5,
        kExportFail = 6,
        kExportNoData = 7,
        kExportMJpegMac = 8,
        kExportSonyFormatMac = 9,
        kExportFormatChange = 10,
        kExportSizeOver4G = 11,
        kExportContentFail = 12,
        kExportNetworkFail = 13
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_Rectangle
    {
        public int topLeftX;
        public int topLeftY;
        public int bottomRightX;
        public int bottomRightY;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_DateTime
    {
        public ushort year;
        public ushort month;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
        public ushort millisecond;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_Frame
    {
        public Np_DateTime time;
        public IntPtr buffer;
        public int len;
        public int width;
        public int height;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ID
    {
        public int centralID;
        public int localID;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnVideoHandle( Np_DateTime time,
                                        IntPtr buffer,
                                        int len,
                                        int width,
                                        int height,
                                        IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnAudioHandle( Np_DateTime time,
                                        IntPtr buffer,
                                        int len,
                                        int bitsPerSample,
                                        int samplesPerSec,
                                        int channels,
                                        IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnRawVideoHandle(  Np_DateTime time,
                                            IntPtr buffer,
                                            int len,
                                            bool isKeyFrame,
                                            Np_VideoCodec videoCodec,
                                            IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnErrorHandle(Np_Error error, IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnExportHandle(Np_ID id, 
                                        Np_ExportError error, 
                                        uint percent,
                                        int iFormatChangedIndex,
                                        IntPtr usrCtx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnOSDHandle (  Np_ID id,
                                        IntPtr buffer,
                                        int width,
                                        int height,
                                        Np_DateTime time, 
                                        IntPtr ctx);

    public enum Np_StreamProfile
    {
        kProfileNormal = 0,
        kProfileOriginal,
        kProfileLow,
        kProfileMinimum
    }

    public enum Np_ScheduleType
    {
        kScheduleNone = 0,
        kScheduleRecordOnly = 1,
        kScheduleMotionDetectOnly = 2,
        kScheduleRecordAndMotionDetect = 3,
        kScheduleRecordMovingOnly = 4,
        kScheduleRecordOnEvent = 5,
        kScheduleUndefined = 6, //undefined
        kScheduleRecordBoost = 7
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_SensorProfile_CS
    {
        public Np_StreamProfile profile;
        public string frameRate;
        public string bitrate;
        public string resolution;
        public string codec;
        public string quality;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_SensorProfile_CS_List
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_SubDevice_CS
    {
        public Np_ID ID;
        public string name;
        public string description;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_SubDevice_CS_List
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_Device_CS
    {
        public Np_ID ID;
        public string name;
        public string description;
        public Np_SubDevice_CS_List SensorDevices;
        public Np_SubDevice_CS_List PTZDevices;
        public Np_SubDevice_CS_List AudioDevices;
        public Np_SubDevice_CS_List DIDevices;
        public Np_SubDevice_CS_List DODevices;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_Device_CS_List
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_DeviceGroup_CS
    {
        public string name; //server name
        public string description;
        public Np_Device_CS_List Camera;
        public Np_Device_CS_List IOBox;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_DeviceGroup_CS_List
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_DeviceList_CS
    {
        public Np_DeviceGroup_CS_List PhysicalGroup;
        public Np_DeviceGroup_CS_List LogicalGroup;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_OperationLog_CS
    {
        public Np_DateTime occurTime;
        public Np_ID sourceID;
        public int eventID;
        public string description;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_OperationLog_CS_List
    {
        public int size;
        public IntPtr items;
    } ;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_ConfigurationLog_CS
    {
        public Np_DateTime occurTime;
        public Np_ID sourceID;
        public int eventID;
        public string description;
        public string user;
        public string behavier;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ConfigurationLog_CS_List
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_Log_CS
    {
        public Np_OperationLog_CS_List opLog;
        public Np_ConfigurationLog_CS_List cnfgLog;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ScheduleLogItem{
        public Np_ID ID;
        public Np_DateTime startTime;
        public Np_DateTime endTime;
        public Np_ScheduleType type;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_RecordLogItem{
        public Np_ID ID;
        public Np_DateTime startTime;
        public Np_DateTime endTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ScheduleLogList{
        public int size;
        public IntPtr logList;    //point to Np_ScheduleLogItem[]
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_RecordLogList{
        public int size;
        public IntPtr logList;  //point to Np_RecordLogItem[]
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_Event{
        public Np_DateTime occurTime;
        public Np_ID sourceID;
        public Np_Event_Type eventID;
        public string auxiliaryCode;
        public string description;
        public string sourceName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_EventList{
        public int size;
        public IntPtr list;     //point to Np_Event[]
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_RecordDateList{
        public int size;
        public IntPtr dateList;    //point to Np_DateTime[]
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_IDList{
        public int size;
        public IntPtr IDList;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_Period{
        public Np_DateTime startTime;
        public Np_DateTime endTime;
    };
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_MetadataSearchCriterion{
        public IntPtr startTime;
        public IntPtr endTime;
        public Np_IDList metadataDeviceID;
        public string keyWord;
        public bool usingRE;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_MetadataLogItem{
        public Np_IDList npIDList;
        public Np_DateTime metadataTime;
        public Np_ID metadata_id;
        public int codepage;
        public int textDataLen;
        public IntPtr textData;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_MetadataLogList{
        public int size;
        public IntPtr metadataList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_SequencedRecord{
        public Np_ID size;
        public Np_Period seqPeriod;
        public int startSeq;
        public int endSeq;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_SequencedRecordList{
        public int size;
        public IntPtr items;
    }

    public enum Np_DIOStatus {
        kDIO_OFF = 0,
	    kDIO_ON,
    }

    public enum Np_PlayerState {
        kStateStopped,
        kStatePaused,
        kStateRunning
    }

    public enum Np_AudioStatus {
        kAUDIO_OFF = 0,
        kAUDIO_ON,
    }

    public enum Np_DeviceCapability
    {
        kDeviceNone = 0x0000,
        kDeviceAudio = 0x0001,
        kDeviceTalk = 0x0002,
        kDevicePTZ = 0x0004,
        kDeviceDIO = 0x0008,
    }

    //////////////////////////////////////////////////////////////////
    /*
     * Metadata related 
     */
    //////////////////////////////////////////////////////////////////
    public enum Np_MetadataSourceType
    {
        kMetadataNone = 0,
        kMetadataPOS = 1,
        kMetadataAccessControl = 2,
        kMetadataLPR = 3
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_MetadataChannel
    {
        public Np_ID id;
        public string name;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_MetadataChannelList
    {
        public int size;
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_MetadataSource
    {
        public Np_ID id;
        public string name;
        public string ip;
        public uint port;
        public Np_MetadataSourceType type;
        public Np_MetadataChannelList channels;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_MetadataSourceList
    {
        public int size;
        public IntPtr items;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnMetadataHandle(Np_ID id,
                                          IntPtr textData,
                                          int codePage,
                                          bool isNew,
                                          bool isComplete,
                                          Np_Rectangle displayRectangle,
                                          int displayTimeout,
                                          bool isUseDefaultRect,
                                          int len,
                                          IntPtr ctx);

    //////////////////////////////////////////////////////////////////
    /*
     * Backup related 
     */
    //////////////////////////////////////////////////////////////////
    public enum Np_BackupStatus
    {
        kBackupStart = 0,
        kBackupSuccess = 1,
        kBackupUpdateFilePayload = 2,
        kBackupCreateNewFile = 3,
        kBackupNetworkError = 4,
        kBackupFail = 5,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_BackupItem{
        public string name;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_BackupItemList{
        public int size;
        public IntPtr items;
    };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public delegate void fnBackupHandle(Np_BackupStatus status,
                                        string fileName,
                                        int updateFilePayloadSize,
                                        IntPtr ctx);
    
    //////////////////////////////////////////////////////////////////
    /*
     * Events related 
     */
    //////////////////////////////////////////////////////////////////
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnEventHandle(Np_Event evt, IntPtr ctx);

    //////////////////////////////////////////////////////////////////
    /*
     * Control related 
     */
    //////////////////////////////////////////////////////////////////

    public enum Np_PTZCommand
    {
        kPTZStop,
        kPTZContinuousMove,
        kPTZAutoFocus,
        kPTZHome,
        kPTZRectangle,
        kPTZPresetGo,
        kPTZPresetSet,
        kPTZPresetClear,
        kPTZPatrolStart,
        kPTZPatrolStop,
    }

    public enum Np_PanDirection { kNoPan, kPanLeft, kPanRight }
    public enum Np_TiltDirection { kNoTilt, kTiltUp, kTiltDown }
    public enum Np_ZoomDirection { kNoZoom, kZoomIn, kZoomOut }
    public enum Np_FocusDirection { kNoFocus, kFocusNear, kFocusFar }

    public enum Np_PTZCap
    {
        kPTZNone = 0x0000,
        kPTZEnable = 0x0001,
        kPTZBuiltin = 0x0002,
        kPTZPane = 0x0004,
        kPTZTilt = 0x0008,
        kPTZZoom = 0x0010,
        kPTZFocus = 0x0020,
        kPTZPreset = 0x0040,
        kPTZLilin = 0x0080,
        kPTZAutoPan = 0x0100,
        kPTZAreaZoom = 0x0200,
        kPTZSpeedDomeOSDMenu = 0x400
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct Np_PTZContinuousMove{		
	    public Np_PanDirection   pan;
        public Np_TiltDirection  tilt;
        public Np_ZoomDirection  zoom;
        public Np_FocusDirection focus;
        public int panSpeed;   // speed from 1 ~ 100, -1 as default
        public int tiltSpeed;  // speed from 1 ~ 100, -1 as default
        public int zoomSpeed;  // speed from 1 ~ 100, -1 as default
        public int focusSpeed; // speed from 1 ~ 100, -1 as default	
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct Np_PTZRectangle{
        public int currentWidth;
        public int currentHeight;
        public int leftupX;
        public int leftupY;
        public int targetWidth;
        public int targetHeight;
        public int speed;      // speed from 1 ~ 100, -1 as default    
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct Np_PTZPreset_CS
    {
        public int presetNo;
        public string presetName;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct Np_PTZPreset_CS_List
    {
        [FieldOffset(0)]
        public int size;
        [FieldOffset(4)]
        public IntPtr items;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Np_PTZParameter_CS
    {
        [FieldOffset(0)]
        public IntPtr move;         // point to Np_PTZContinuousMove

        [FieldOffset(0)]
        public IntPtr rectangle;    // point to Np_PTZRectangle

        [FieldOffset(0)]
        public IntPtr preset;       // point to Np_PTZPreset_CS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_PTZControlParam_CS
    {
        public Np_PTZCommand command;
        public Np_PTZParameter_CS param;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ExportContent
    {
        public Np_ID id;
        public Np_DateTime startTime;
        public Np_DateTime endTime;
        public bool excludeAudio;
        public int width;
        public int height;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_ExportProfile
    {
        public int profile;
        public string description;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ExportProfileList
    {
        public int size;
        public IntPtr items;    // point to Np_ExportProfile
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_ExportFormat
    {
        public int format;
        public string description;
        public Np_ExportProfileList supportedProfile;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Np_ExportFormatList
    {
        public int size;
        public IntPtr items;     // point to Np_ExportFormat
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 0)]
    public struct Np_TalkAudioFormat
    {
        public int channels;
        public int bitsPerSample;
        public int sampleRate;
        public int sampleRequest;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct Np_PTZPresetType_CS
    {
        [FieldOffset(0)]
        public byte allowSetPresetByIndex;
        [FieldOffset(1)]
        public byte allowClearAllPreset;
        [FieldOffset(4)]
        public uint maxPresetNumber;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 0, CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
    public struct Np_PatrolGroup_CS
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public int[] presetPoint;
        [FieldOffset(320)]
        public int presetCount;
        [FieldOffset(324)]
        public int period;
        [FieldOffset(328)]
        public int time;
        [FieldOffset(332)]
        public int nextPoint;
        [FieldOffset(336)]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct Np_PTZPatrol_CS
    {
        [FieldOffset(0)]
        public byte isPatrolStartEnabled;
        [FieldOffset(1)]
        public byte isPatrolStopEnabled;
        [FieldOffset(2)]
        public byte isPatrolEnable;
        [FieldOffset(4)]
        public int activeGroupIndex;
        [FieldOffset(8)]
        public int eventTriggerGroupIndex;
        [FieldOffset(12)]
        public ushort maxPatrolGroupNumber;
        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Np_PatrolGroup_CS[] group;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct Np_PTZInfo_CS
    {
        [FieldOffset(0)]
        public Np_PTZPresetType_CS ptzPresetType;
        [FieldOffset(8)]
        public Np_PTZPreset_CS_List ptzPresetList;
        [FieldOffset(16)]
        public Np_PTZPatrol_CS ptzPatrol;
    }

    public class NpClient
    {
        private const string module_name = "NpClient.dll";

        public NpClient()
        {
        }

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Create_HandleWChar(
            ref IntPtr handle, 
            Np_ServerType type, 									
            string username, 
            string passwd,
            string ipaddress,
            ushort port
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Create_Handle_And_Event_Subscribe(
            ref IntPtr handle,
            Np_ServerType type,
            string username,
            string passwd,
            string ipaddress,
            ushort port,
            fnEventHandle evtcb, IntPtr evtctx
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Destroy_Handle(
            IntPtr handle
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetDeviceList_CS(
            IntPtr handle, 
            ref Np_DeviceList_CS deviceList
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseDeviceList_CS(
            IntPtr handle, 
            ref Np_DeviceList_CS deviceList
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetDeviceCapability (IntPtr handle, Np_ID id, out long capability);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetDIOStatus(IntPtr handle, Np_ID id, out Np_DIOStatus status);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetDOPrivilege(IntPtr handle, Np_ID id, out uint dwDOPrivilege);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetDIAssociatedDevice(IntPtr handle, Np_ID id, ref Np_ID associatedDevice);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetPTZPreset_CS(IntPtr handle, Np_ID id, ref Np_PTZPreset_CS_List ptzPresetList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleasePTZPreset_CS(IntPtr handle, ref Np_PTZPreset_CS_List ptzPresetList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetPTZCapability (IntPtr handle, Np_ID id, out long ptzCapability);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetSensorProfileList_CS(
            IntPtr handle, 
            Np_ID id, 
            ref Np_SensorProfile_CS_List sensorProfileList
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseSensorProfileList_CS(
            IntPtr  handle, 
            ref Np_SensorProfile_CS_List sensorProfileList
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetRangedRecordDateList(IntPtr handle, 
                                                                    IntPtr startDate,   //Point to Np_DateTime
                                                                    IntPtr endDate,     //Point to Np_DateTime
                                                                    ref Np_RecordDateList recordDateList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseRecordDateList (IntPtr handle, ref Np_RecordDateList recordDateList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetScheduleLogs (IntPtr handle, Np_DateTime date, ref Np_ScheduleLogList scheduleLogList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseScheduleLogs (IntPtr handle, ref Np_ScheduleLogList scheduleLogList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetRecordLogs (IntPtr handle, Np_DateTime date, ref Np_RecordLogList recordLogList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseRecordLogs(IntPtr handle, ref Np_RecordLogList recordLogList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_QueryEvents(    IntPtr handle,
                                                            IntPtr startTime,       //Point to Np_DateTime
                                                            IntPtr endTime,         //Point to Np_DateTime
                                                            IntPtr deviceTypeID,    //Point to int
                                                            IntPtr deviceID,        //Point to Np_ID
                                                            IntPtr eventID,         //Point to int
                                                            ref Np_EventList eventList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseEvents (IntPtr handle, ref Np_EventList eventList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetPTZInfo_CS(IntPtr handle, Np_ID id, ref Np_PTZInfo_CS info);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetMetadataLog(IntPtr handle, Np_MetadataSearchCriterion criterion, ref Np_MetadataLogList metadataList, ref bool isLogExceedMaxLimit);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetMetadataSourceList(IntPtr handle, ref Np_MetadataSourceList list);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseMetadataSourceList(IntPtr handle, ref Np_MetadataSourceList list);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_ReleaseMetadataLog(IntPtr handle, ref Np_MetadataLogList metadataList);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Info_GetBackupFileSize(IntPtr handle, 
                                                              Np_DateTime startTime, 
                                                              Np_DateTime endTime, 
                                                              Np_IDList backupIDList, 
                                                              ref Np_SequencedRecordList seqRecordList, 
							                                  bool isIncluedExeFile,
                                                              out ulong size);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Control_PTZ_CS(IntPtr handle, Np_ID id, ref Np_PTZControlParam_CS param);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Control_PTZ_PTZDeviceID_CS(IntPtr handle, Np_ID id, ref Np_PTZControlParam_CS param);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Control_DigitalOutput(IntPtr handle, Np_ID id, bool turnOn);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Control_SetPatrol(IntPtr handle, Np_ID id, ref Np_PTZPatrol_CS info);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Talk_Enable(IntPtr handle, Np_ID id, ref Np_TalkAudioFormat fmt);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Talk_Disable(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Talk_SendAudioPacket(IntPtr handle, IntPtr buf, int size);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Talk_GetEnabledID(IntPtr handle, ref Np_ID id);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Event_Subscribe(IntPtr handle, ref IntPtr session, fnEventHandle evtcb, IntPtr evtctx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Event_Unsubscribe(IntPtr session);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_CreatePlayer(IntPtr handle, ref IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_DestroyPlayer(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_AttachSession(  IntPtr player, ref IntPtr session, 
								                                Np_ID id,
								                                Np_StreamProfile profile, 
								                                fnVideoHandle vcb, IntPtr vctx, 
								                                fnAudioHandle acb, IntPtr actx, 
								                                fnErrorHandle ecb, IntPtr ectx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_AttachSessionExt(   IntPtr player, ref IntPtr session,
                                                                    Np_ID id,
                                                                    Np_StreamProfile profile,
                                                                    Np_PixelFormat videoPixalFormat,
                                                                    fnVideoHandle vcb, IntPtr vctx,
                                                                    fnAudioHandle acb, IntPtr actx,
                                                                    fnErrorHandle ecb, IntPtr ectx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_DetachSession(IntPtr player, IntPtr session);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_SetAudioOn(IntPtr player, IntPtr session);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_SetAudioOff(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_GetSessionAudioStatus(IntPtr player, IntPtr session, ref Np_AudioStatus status);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_GetSessionCurrentImage(IntPtr player, IntPtr session, ref Np_Frame frame);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_ReleaseSessionCurrentImage(IntPtr player, ref Np_Frame frame);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_CreatePlayer(IntPtr handle, ref IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_DestroyPlayer(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_AttachSession(IntPtr player, ref IntPtr session, 
								            Np_ID id,
                                            fnVideoHandle vcb, IntPtr vctx,
                                            fnAudioHandle acb, IntPtr actx,
                                            fnErrorHandle ecb, IntPtr ectx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_AttachSessionExt(IntPtr player, ref IntPtr session, 
                                               Np_ID id,
                                               Np_PixelFormat videoPixelFormat,
                                               fnVideoHandle vcb, IntPtr vctx,
                                               fnAudioHandle acb, IntPtr actx,
                                               fnErrorHandle ecb, IntPtr ectx);
        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_AttachRawVideoSession(IntPtr player, ref IntPtr session, 
                                            Np_ID id,
                                            fnRawVideoHandle vcb, IntPtr vctx,
                                            fnAudioHandle acb, IntPtr actx,
                                            fnErrorHandle ecb, IntPtr ectx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_DetachSession(IntPtr player, IntPtr session);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_Seek(IntPtr player, Np_DateTime time);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_OpenRecord(IntPtr player, Np_DateTime startTime, Np_DateTime endTime);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_Play(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_ReversePlay(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_Pause(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_StepForward(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_StepBackward(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_Next(IntPtr  player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_Previous(IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_SetSpeed(IntPtr player, float speed); 

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_GetSpeed(IntPtr player, out float speed);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_GetTime(IntPtr player, out Np_DateTime time);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result PlayBack_GetPlayerState(IntPtr player, out Np_PlayerState state);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_ExportVideo  ( IntPtr  player,
                                                                Np_ExportContent content, 
                                                                string filename, 
                                                                int format,
                                                                int profile,
                                                                fnExportHandle ecb, IntPtr ectx,
                                                                fnOSDHandle ocb, IntPtr octx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_StopExport   (IntPtr player);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_GetExportFormatList(IntPtr player, ref Np_ExportFormatList fmtlist);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_GetSessionCurrentImage(IntPtr player, IntPtr session, ref Np_Frame frame);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_ReleaseExportFormatList(IntPtr player, ref Np_ExportFormatList fmtlist);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result  PlayBack_ReleaseSessionCurrentImage(IntPtr player, ref Np_Frame frame);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Utility_SaveSnapShotImage(string filename, IntPtr buffer, int len, int width, int height);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Utility_AddImageWaterMark(string  filename, ref Np_DateTime time, string cameraname);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Utility_AddVideoWaterMark(string  filename, ref Np_DateTime start_time, ref Np_DateTime end_time, string cameraname);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_SubscribeMetadata(IntPtr handle, fnMetadataHandle mcb, IntPtr mctx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result LiveView_UnsubscribeMetadata(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Initial(IntPtr handle, 
                                                      Np_DateTime startTime, 
                                                      Np_DateTime endTime, 
                                                      Np_IDList backupIdList,
                                                      ref Np_SequencedRecordList seqRecordList,
                                                      bool includeEventLogs,
                                                      bool includeCounterLogs,
                                                      bool includeSystemLogs,
                                                      bool includeMetadataLogs,
                                                      bool includeExeFiles,
                                                      fnBackupHandle bcb,
                                                      IntPtr bctx);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Uninit(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_GetBackupFileItemList(IntPtr handle, ref Np_BackupItemList list);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_ReleaseBackupFileItemList(IntPtr handle, ref Np_BackupItemList list);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_SetBackupDestinationDir(IntPtr handle, string dir);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Start(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Pause(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Resume(IntPtr handle);

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern Np_Result Backup_Abort(IntPtr handle);

    }
}