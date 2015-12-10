using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;
using System.Collections;
using System.IO;
using NUUO_SDK;
using AForge;
using AForge.Vision;
using AForge.Vision.Motion;


namespace Tester_CSharp
{
    struct stSession
    {
        public IntPtr session;
        public Np_ID id;
        public int width;
        public int height;
    }

    struct ExportCbInfo
    {
        public Np_ID id;
        public Np_ExportError error; 
        public uint percent;
        public int iFormatChangedIndex;

        public ExportCbInfo(Np_ID _id, Np_ExportError _error, uint _percent, int _iFormatChangedIndex)
        {
            id = _id;
            error = _error;
            percent = _percent;
            iFormatChangedIndex = _iFormatChangedIndex;
        }
    }

    struct ExportProfileItem
    {
        public int format;
        public int profile;
        public string desc;

        public override string ToString()
        {
            return desc;
        }
    }

    struct EventCbInfo
    {
        public Np_Event info;

        public EventCbInfo(Np_Event evt)
        {
            info = evt;
        }
    }

    public struct MetadataCbInfo
    {
        public Np_ID id;
        public byte[] textData;

        public int codePage;
        public bool isNew;
        public bool isComplete;
        public Np_Rectangle displayRectangle;
        public int displayTimeout;
        public bool isUseDefaultRect;
        public int len;
        public IntPtr ctx;

        public MetadataCbInfo(Np_ID _id, byte[] _textData, int _codePage, bool _isNew, bool _isComplete,
                              Np_Rectangle _displayRectangle, int _displayTimeout, bool _isUseDefaultRect , int _len ,IntPtr _ctx)
        {
            id = _id;
            textData = _textData;
            codePage = _codePage;
            isNew = _isNew;
            isComplete = _isComplete;
            displayRectangle = _displayRectangle;
            displayTimeout = _displayTimeout;
            isUseDefaultRect = _isUseDefaultRect;
            len = _len;
            ctx = _ctx;
        }
    }

    public struct BackupCbInfo
    {
        public Np_BackupStatus m_status;
        public string m_fileName;
        public int m_fileSize;

        public BackupCbInfo(Np_BackupStatus status, string fileName, int fileSize)
        {
            m_status = status;
            m_fileName = fileName;
            m_fileSize = fileSize;
        }

    }

    public partial class Tester_CSharp : Form
    {
        private IntPtr m_handle = IntPtr.Zero;
        private IntPtr m_evtSession = IntPtr.Zero;
        private IntPtr m_recorder = IntPtr.Zero;
        private IntPtr m_lvPlayer = IntPtr.Zero;
        private Dictionary<int, stSession> m_lvSessions = new Dictionary<int, stSession>();
        private IntPtr m_pbPlayer = IntPtr.Zero;
        private Dictionary<int, stSession> m_pbSessions = new Dictionary<int, stSession>();
        private List<Np_ID> m_backupDeviceList = new List<Np_ID>();
        private Np_DeviceList_CS m_deviceList;
        private PlaySound m_soundPlayer = new PlaySound();
        private fnVideoHandle m_vcb = null;
        private fnAudioHandle m_acb = null;
        private fnErrorHandle m_ecb = null;
        private fnExportHandle m_exptcb = null;
        private fnOSDHandle m_osdcb = null;
        private fnEventHandle m_evtcb = null;
        private fnMetadataHandle m_metadatacb = null;
        private fnAudioRecordHandle m_rcb = null;
        private fnBackupHandle m_bcb = null;
        private Graphics m_gViewPort = null;
        private Bitmap m_currentFrame = null;
        private Object m_frameLock = new Object();
        private System.Timers.Timer m_timer;
        private Np_ServerType m_serverType = Np_ServerType.kMainConsoleLiveview; 
        private bool m_bExporting = false;
        private bool m_bEnableTalk = false;
        private int session_series_number = 0;
        private List<ExportCbInfo> m_exptcbinfos = new List<ExportCbInfo>();
        private System.Object m_exptcbmutex = new System.Object();
        private stSession m_exptctx;
        private List<EventCbInfo> m_evtCbList = new List<EventCbInfo>();
        private System.Object m_evtCbListMutex = new System.Object();
        
        private List<MetadataCbInfo> m_metadataCbList = new List<MetadataCbInfo>();
        private System.Object m_metadataCbListMutex = new System.Object();

        private List<BackupCbInfo> m_backupCbList = new List<BackupCbInfo>();
        private System.Object m_backupCbListMutex = new System.Object();

        delegate string GetTextCallback(Control ctrl);
        delegate void SetTextCallback(Control ctrl, string text);
        delegate void SetEnabledCallback(Control ctrl, bool enabled);

        public delegate Np_Result DisableTalkInvoke();

        private static string m_clear_all_item_text = "Clear All";
        private static string m_add_preset_text = "Add Preset";
        private static string m_start_patrol_text = "Start Patrol";
        private static string m_stop_patrol_text = "Stop Patrol";
        private Font m_menu_item_font = new Font("Aerial", 8, FontStyle.Regular);

        private static ulong KB_TO_BYTE_UNIT = 1024;
        private static ulong MB_TO_BYTE_UNIT = 1024 * 1024;

        // Motion 參數
        private MotionDetector detector;
        private string motionMode = "0"; // 0:Pause 1:Slow play
        
        private enum MenuType
        {
            NONE = 0,
            PRESET_ON = 1,
            PRESET_OFF = 2,
            PATROL_SHOW = 3,
            PATROL_HIDE = 4
        }

        public Tester_CSharp(string IP, string Port, string Username, string Password, string CentralID, string LocalID, DateTime StartTime, DateTime EndTime)
        {
            InitializeComponent();

            // create motion detector
            detector = new MotionDetector(new SimpleBackgroundModelingDetector(), new MotionBorderHighlighting());

            tbIP.Text = IP;
            tbPort.Text = Port;
            tbUsername.Text = Username;
            tbPassword.Text = Password;
            tbCentralID_PB.Text = CentralID;
            tbLocalID_PB.Text = LocalID;            
            cmbServerType.SelectedIndex = 1;
            dtspFrom.Value = StartTime;
            dtspTo.Value = EndTime;
            dtspSeek.Value = dtspFrom.Value;

            m_vcb = new fnVideoHandle(VideoHandler);
            m_acb = new fnAudioHandle(AudioHandler);
            m_ecb = new fnErrorHandle(ErrorHandler);
            m_exptcb = new fnExportHandle(ExportHandler);
            m_osdcb = new fnOSDHandle(OSDHandler);
            m_evtcb = new fnEventHandle(EventHandler);
            m_metadatacb = new fnMetadataHandle(MetadataHandle);
            m_rcb = new fnAudioRecordHandle(AudioRecordHandle);
            m_bcb = new fnBackupHandle(BackupHandle);
            FormClosing += new FormClosingEventHandler(FormClosingHandler);
            m_gViewPort = pbViewPort.CreateGraphics();
            m_timer = new System.Timers.Timer();
            m_timer.Interval = 10;
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnUpdateUI);
            m_timer.SynchronizingObject = this;
            m_timer.Start();

            btnCreateHandle_Click(null, null);
            btnCreatePlayer_PB_Click(null, null);
            btnAttachSession_PB_Click(null, null);
            btnOpenRecord_Click(null, null);
            btnPlay_Click(null, null);
        }

        
        public void FormClosingHandler(object src, FormClosingEventArgs args)
        {
            m_timer.Stop();
            DisableTalk();

            NpClient.Info_ReleaseDeviceList_CS(m_handle, ref m_deviceList);
 
            if (m_handle != IntPtr.Zero)
            {
                if (m_lvPlayer != IntPtr.Zero)
                {
                    int[] keys = new int[m_lvSessions.Count];
                    m_lvSessions.Keys.CopyTo(keys, 0);
                    for (int i = 0; i < m_lvSessions.Count; ++i)
                    {
                        NpClient.LiveView_DetachSession(m_lvPlayer, m_lvSessions[keys[i]].session);
                    }
                    NpClient.LiveView_DestroyPlayer(m_lvPlayer);
                }

                if (m_pbPlayer != IntPtr.Zero)
                {
                    if (m_bExporting)
                    {
                        NpClient.PlayBack_StopExport(m_pbPlayer);
                    }

                    int[] keys = new int[m_pbSessions.Count];
                    m_pbSessions.Keys.CopyTo(keys, 0);
                    for (int i = 0; i < m_pbSessions.Count; i++)
                    {
                        NpClient.PlayBack_DetachSession(m_pbPlayer, m_pbSessions[keys[i]].session);
                    }
                    NpClient.PlayBack_DestroyPlayer(m_pbPlayer);
                }

                if (m_evtSession != IntPtr.Zero)
                {
                    NpClient.Event_Unsubscribe(m_evtSession);
                }
                NpClient.Destroy_Handle(m_handle);
            }
            m_soundPlayer.Dispose();
        }

        public void VideoHandler(   Np_DateTime time,
                                    IntPtr pBuffer,
                                    int len,
                                    int width,
                                    int height,
                                    IntPtr ctx)
        {
            try
            {
                Rectangle recViewPort = new Rectangle(0, 0, pbViewPort.Size.Width, pbViewPort.Size.Height);
                Rectangle recVideo = new Rectangle(0, 0, width, height);
                lock (m_frameLock)
                {
                    if (null == m_currentFrame || (m_currentFrame != null && (m_currentFrame.Width != width || m_currentFrame.Height != height)))
                    {
                        m_currentFrame = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    System.Drawing.Imaging.BitmapData bmp_data = m_currentFrame.LockBits(recVideo,
                                                                                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                                                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    byte[] buffer = new byte[len];
                    Marshal.Copy(pBuffer, buffer, 0, len);
                    Marshal.Copy(buffer, 0, bmp_data.Scan0, len);

                    
                    // process new video frame and check motion level
                    float x = detector.ProcessFrame(bmp_data);
                    Console.WriteLine(x);
                    if (x > 0.02)
                    {
                        // ring alarm or do somethng else

                        if (motionMode == "0")
                        {
                            // Pause                        
                            NpClient.PlayBack_Pause(m_pbPlayer);
                        }
                        else
                        {
                            // Slow play
                            NpClient.PlayBack_Pause(m_pbPlayer);
                            //NpClient.PlayBack_SetSpeed(m_pbPlayer, (float)1);

                            System.Threading.Thread.Sleep(2000);

                            //NpClient.PlayBack_SetSpeed(m_pbPlayer, (float)4);
                            NpClient.PlayBack_Play(m_pbPlayer);
                        }
                    }
                    else
                    {
                        NpClient.PlayBack_SetSpeed(m_pbPlayer, (float)4);
                    }
                    

                    m_currentFrame.UnlockBits(bmp_data);
                    m_gViewPort.DrawImage(m_currentFrame, recViewPort, recVideo, GraphicsUnit.Pixel);

                }
            }
            finally
            {
            }
        }

        public void AudioHandler(   Np_DateTime time,
                                    IntPtr pBuffer,
                                    int len,
                                    int bitsPerSample,
                                    int samplesPerSec,
                                    int channels,
                                    IntPtr ctx)
        {
            m_soundPlayer.WriteSoundData(pBuffer, len, channels, bitsPerSample, samplesPerSec);
        }

        public void ErrorHandler(Np_Error error, IntPtr ctx)
        {
            if (Np_Error.Np_ERROR_CONNECT_SUCCESS == error)
            {
                SetPlaybackButtonStatus(true);
            }
        }

        public void ExportHandler(  Np_ID id, 
                                   Np_ExportError error, 
                                   uint percent,
                                   int iFormateChangedIndex,
                                   IntPtr ctx)
        {
            PutExportCbInfo(id, error, percent, iFormateChangedIndex);
        }

        public void OSDHandler( Np_ID id,
                               IntPtr buffer,
                               int width,
                               int height,
                               Np_DateTime time, 
                               IntPtr ctx)
        {
        }

        public void EventHandler(Np_Event evt, IntPtr ctx)
        {
            lock (m_evtCbListMutex)
            {
                m_evtCbList.Add(new EventCbInfo(evt));
            }
        }

        public void MetadataHandle  (Np_ID id,
                                     IntPtr textData,
                                     int codePage,
                                     bool isNew,
                                     bool isComplete,
                                     Np_Rectangle displayRectangle,
                                     int displayTimeout,
                                     bool isUseDefaultRect,
                                     int len,
                                     IntPtr ctx)
        {
            lock (m_metadataCbListMutex)
            {
                byte[] metadataArray = null;
                if (len > 0)
                {
                    metadataArray = new byte[len + 1];
                    Marshal.Copy(textData, metadataArray, 0, len);
                }
                m_metadataCbList.Add(new MetadataCbInfo(id, metadataArray, codePage, isNew,
                                    isComplete, displayRectangle, displayTimeout, isUseDefaultRect, len, ctx));
            }
        }

        public void AudioRecordHandle(IntPtr buffer, int len, IntPtr ctx)
        {
            if(Np_Result.Np_Result_OK != NpClient.Talk_SendAudioPacket(m_handle, buffer, len))
            {
                DisableTalkInvoke disableInvoke = new DisableTalkInvoke(DisableTalk);
                this.BeginInvoke(disableInvoke, new Object[] {});
            }
        }

        public void BackupHandle(Np_BackupStatus status, string fileName, int updateFilePayloadSize, IntPtr ctx)
        {
            lock (m_backupCbListMutex)
            {
                m_backupCbList.Add(new BackupCbInfo(status, fileName, updateFilePayloadSize));
            }
        }

        public string FormatExportFilePath(string basePath, int iFormatChangedIndex, string formatDesc)
        {
            string path = basePath;
            if (iFormatChangedIndex > 0)
            {
                path += string.Format("_%d", iFormatChangedIndex);
            }

            if (formatDesc.Contains("ASF"))
            {
                path += ".ASF";
            }
            if (formatDesc.Contains("AVI"))
            {
                path += ".AVI";
            }
            if (formatDesc.Contains("MOV"))
            {
                path += ".MOV";
            }

            return path;
        }

        public void OnUpdateUI(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_pbSessions.Count != 0)
            {
                Np_DateTime dateTime;
                NpClient.PlayBack_GetTime(m_pbPlayer, out dateTime);
                float fSpeed = 1.0f;
                NpClient.PlayBack_GetSpeed(m_pbPlayer, out fSpeed);

                Np_PlayerState pb_state = Np_PlayerState.kStateStopped;
                string StateStr = "";
                NpClient.PlayBack_GetPlayerState(m_pbPlayer, out pb_state);
                switch(pb_state)
                {
                    case Np_PlayerState.kStateStopped:
                    {
                        StateStr = "Stopped";
                        break;
                    }
                case Np_PlayerState.kStatePaused:
                    {
                        StateStr = "Paused";
                        break;
                    }
                case Np_PlayerState.kStateRunning:
                    {
                        StateStr = "Running";
                        break;
                    }
                    default:
                    {
                        StateStr = "";
                        break;
                    }
                }

                string strTime =    "Time : "+
                                    string.Format("{0:D4}", dateTime.year)+"\\"+
                                    string.Format("{0:D2}", dateTime.month)+"\\"+
                                    string.Format("{0:D2}", dateTime.day)+" "+
                                    string.Format("{0:D2}", dateTime.hour)+":"+
                                    string.Format("{0:D2}", dateTime.minute)+":"+
                                    string.Format("{0:D2}", dateTime.second)+"  "+
                                    "Speed : "+fSpeed+" "+
                                    "State : "+StateStr;
                SetText(lbPBTime, strTime);
            }

            lock (m_exptcbmutex)
            {
                OnUpdateExportStatus();
            }

            OnUpdateCallbackEvt();
            OnUpdateCallbackMetadata();
            OnUpdateCallbackBackupInfo();
        }

        private void OnUpdateExportStatus()
        {
            if(m_exptcbinfos.Count > 0)
            {
                ExportCbInfo info = m_exptcbinfos[0];
                m_exptcbinfos.RemoveAt(0);
                if (info.error == Np_ExportError.kExportUpdateProgress)
                {
                    while (m_exptcbinfos.Count > 0)
                    {
                        ExportCbInfo next = m_exptcbinfos[0];
                        if (Np_ExportError.kExportUpdateProgress != next.error || info.percent != next.percent)
                        {
                            break;
                        }
                        m_exptcbinfos.RemoveAt(0);
                    }
                    pbExportProgress.Value = (int)info.percent;
                    lbExportPercent.Text = info.percent + "%";
                    return;
                }
                else if (info.error == Np_ExportError.kExportFormatChange)
                {
                    AddWaterMarkAfterExport(info.id, info.iFormatChangedIndex);
                    return;
                }

                switch(info.error)
                {
                case Np_ExportError.kExportSuccess:
                case Np_ExportError.kExportSizeOver4G:
                    AddWaterMarkAfterExport(info.id, info.iFormatChangedIndex);
                    ShowText("Export Success");
                    break;
                case Np_ExportError.kExportCallbackError:
                    ShowText("Export Callback Error");
                    break;
                case Np_ExportError.kExportChannelEmpty:
                    ShowText("Export Channel Empty");
                    break;
                case Np_ExportError.kExportCueTimeError:
                    ShowText("Export CueTime Error");
                    break;
                case Np_ExportError.kExportDimesionError:
                    ShowText("Export Dimension Error");
                    break;
                case Np_ExportError.kExportNoData:
                    ShowText("No Data to Export");
                    break;
                case Np_ExportError.kExportFail:
                case Np_ExportError.kExportContentFail:
                case Np_ExportError.kExportNetworkFail:
                    ShowText("Export Fail");
                    if(cmbExportProfile.Items.Count > 0)
                    {
                        FileInfo fi = new FileInfo(tbExportPath.Text);
                        FileInfo fi2 = new FileInfo(FormatExportFilePath(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length), 
                                             info.iFormatChangedIndex, cmbExportProfile.Text));
                        if(fi2.Exists)
                        {
                            fi2.Delete();
                        }
                    }
                    break;
                case Np_ExportError.kExportMJpegMac:
                    ShowText("Export MJPEG to MOV format is not supported");
                    break;
                }
                m_bExporting = false;
                pbExportProgress.Value = 0;
                lbExportPercent.Text = "0%";
                m_exptcbinfos.Clear();
                btnExport.Enabled = true;
                btnStopExport.Enabled = false;
            }
        }

        private void OnUpdateCallbackEvt()
        {
            lock (m_evtCbListMutex)
            {
                while (m_evtCbList.Count > 0)
                {
                    EventCbInfo next = m_evtCbList[0];
                    ShowCallbackEvent(next.info);
                    m_evtCbList.RemoveAt(0);
                }
            }
        }

        private void OnUpdateCallbackMetadata()
        {
            lock (m_metadataCbListMutex)
            {
                while (m_metadataCbList.Count > 0)
                {
                    MetadataCbInfo next = m_metadataCbList[0];
                    ShowCallbackMetadata(next);
                    m_metadataCbList.RemoveAt(0);
                }
            }
        }

        private void OnUpdateCallbackBackupInfo()
        {
            lock (m_backupCbListMutex)
            {
                while (m_backupCbList.Count > 0)
                {
                    BackupCbInfo next = m_backupCbList[0];
                    ShowCallbackBackupInfo(next);
                    m_backupCbList.RemoveAt(0);
                }
            }
        }

        private string GetText(Control ctrl)
        {
            if (ctrl.InvokeRequired)
            {
                GetTextCallback cb = new GetTextCallback(GetText);
                return (string)(Invoke(cb, new object[] { ctrl }));
            }
            else
            {
                return ctrl.Text;
            }
        }

        private void SetText(Control ctrl, string text)
        {
            if (ctrl.InvokeRequired)
            {
                SetTextCallback cb = new SetTextCallback(SetText);
                Invoke(cb, new object[] { ctrl, text });
            }
            else
            {
                ctrl.Text = text;
            }
        }

        private void SetEnabled(Control ctrl, bool enabled)
        {
            if (ctrl.InvokeRequired)
            {
                SetEnabledCallback cb = new SetEnabledCallback(SetEnabled);
                Invoke(cb, new object[] { ctrl, enabled });
            }
            else
            {
                ctrl.Enabled = enabled;
            }
        }

        private void ConvertTime(DateTime src, out Np_DateTime dst)
        {
            dst.year = (ushort)src.Year;
            dst.month = (ushort)src.Month;
            dst.day = (ushort)src.Day;
            dst.hour = (ushort)src.Hour;
            dst.minute = (ushort)src.Minute;
            dst.second = (ushort)src.Second;
            dst.millisecond = (ushort)src.Millisecond;
        }

        private void ShowText(string str)
        {
            tbMessages.AppendText(str + "\r\n");
        }

        private void ShowText(string str, ref string toString)  //Pre-processing version, due to calling TextBox.AppendText too frequently will cause process busy updating and then hang
        {
            toString += str + "\r\n";
        }

        private void ShowResult(Np_Result ret)
        {
            switch (ret)
            {
                case Np_Result.Np_Result_OK:
                    ShowText("OK");
                    break;
                case Np_Result.Np_Result_CLI_FAULT:
                    ShowText("CLI_FAULT");
                    break;
                case Np_Result.Np_Result_SVR_FAULT:
                    ShowText("SVR_FAULT");
                    break;
                case Np_Result.Np_Result_USER_ERROR:
                    ShowText("USER_ERROR");
                    break;
                case Np_Result.Np_Result_FAILED:
                    ShowText("FAILED");
                    break;
                case Np_Result.Np_Result_NO_METHOD:
                    ShowText("NO_METHOD");
                    break;
                case Np_Result.Np_Result_NO_DATA:
                    ShowText("NO_DATA");
                    break;
                case Np_Result.Np_Result_SOCKET_ERROR:
                    ShowText("SOCKET_ERROR");
                    break;
                case Np_Result.Np_Result_INVALID_ARGUMENT:
                    ShowText("INVALID_ARGUMENT");
                    break;
                case Np_Result.Np_Result_FATAL_ERROR:
                    ShowText("FATAL_ERROR");
                    break;
                default:
                    ShowText("Np_Result is Unrecognizable !!!");
                    break;
            }
            ShowText("\r\n");
        }

        private void ShowTime(string title, Np_DateTime time)
        {
            ShowText(title);
            ShowText(
                "year = "+time.year+"\r\n"+
                "month = "+time.month+"\r\n"+
                "day = "+time.day+"\r\n"+
                "hour = "+time.hour+"\r\n"+
                "minute = "+time.minute+"\r\n"+
                "second = "+time.second+"\r\n"+
                "millisecond = "+time.millisecond);
            ShowText("\r\n");
        }

        private void ShowTime(string title, Np_DateTime time, ref string toString)
        {
            ShowText(title, ref toString);
            ShowText(
                "year = " + time.year + "\r\n" +
                "month = " + time.month + "\r\n" +
                "day = " + time.day + "\r\n" +
                "hour = " + time.hour + "\r\n" +
                "minute = " + time.minute + "\r\n" +
                "second = " + time.second + "\r\n" +
                "millisecond = " + time.millisecond, ref toString);
            ShowText("\r\n", ref toString);
        }

        private void ShowRecordDateList(Np_RecordDateList dateList)
        {
            if (0 == dateList.size)
            {
                ShowText("No Dates have Record Logs");
                return;
            }

            IntPtr cursor = dateList.dateList;
            for (int i=0; i < dateList.size; i++)
            {
                ShowTime("Record date", (Np_DateTime)(Marshal.PtrToStructure(cursor, typeof(Np_DateTime))));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DateTime)));
            }
        }

        public void ShowRecordLogItem(Np_RecordLogItem logItem, ref string toString)
        {
            ShowText("---Record Log---\n", ref toString);
            ShowID("Device ID", logItem.ID, ref toString);
            ShowTime("Reocord StartTime", logItem.startTime, ref toString);
            ShowTime("Reocord EndTime", logItem.endTime, ref toString);
        }

        public void ShowRecordLogs(Np_RecordLogList logList)
        {
            string finalLog = "";
            if (0 == logList.size)
            {
                ShowText("No Record Logs");
                return;
            }

            ShowText(logList.size + " Record Logs");
            IntPtr cursor = logList.logList;
            for (int i=0; i < logList.size; i++)
            {
                ShowRecordLogItem((Np_RecordLogItem)(Marshal.PtrToStructure(cursor, typeof(Np_RecordLogItem))), ref finalLog);
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_RecordLogItem)));
            }

            ShowText(finalLog);
        }

        public void ShowSchduleType(Np_ScheduleType schType)
        {
            ShowText("Schedule Type:");
            switch(schType)
            {
                case Np_ScheduleType.kScheduleNone:
                {
                    ShowText("kScheduleNone");
                    break;
                }
                case Np_ScheduleType.kScheduleRecordOnly:
                {
                    ShowText("kScheduleRecordOnly");
                    break;
                }
                case Np_ScheduleType.kScheduleMotionDetectOnly:
                {
                    ShowText("kScheduleMotionDetectOnly");
                    break;
                }
                case Np_ScheduleType.kScheduleRecordAndMotionDetect:
                {
                    ShowText("kScheduleRecordAndMotionDetect");
                    break;
                }
                case Np_ScheduleType.kScheduleRecordMovingOnly:
                {
                    ShowText("kScheduleRecordMovingOnly");
                    break;
                }
                case Np_ScheduleType.kScheduleRecordOnEvent:
                {
                    ShowText("kScheduleRecordOnEvent");
                    break;
                }
                case Np_ScheduleType.kScheduleUndefined:
                {
                    ShowText("kScheduleUndefined");
                    break;
                }
                case Np_ScheduleType.kScheduleRecordBoost:
                {
                    ShowText("kScheduleRecordBoost");
                    break;
                }
                default:
                {
                    ShowText("Schedule Type is Unrecognizable !!!");
                    break;
                }
            }
            ShowText("\r\n");
        }

        public void ShowScheduleLogItem(Np_ScheduleLogItem logItem)
        {
            ShowText("---Scheudle Log---\n");
            ShowID("Device ID", logItem.ID);
            ShowSchduleType(logItem.type);
            ShowTime("Scheudle StartTime", logItem.startTime);
            ShowTime("Scheudle EndTime", logItem.endTime);
        }

        public void ShowScheduleLogs(Np_ScheduleLogList logList)
        {
            if (0 == logList.size)
            {
                ShowText("No Schedule Logs");
                return;
            }

            IntPtr cursor = logList.logList;
            for (int i=0; i < logList.size; i++)
            {
                ShowScheduleLogItem((Np_ScheduleLogItem)(Marshal.PtrToStructure(cursor, typeof(Np_ScheduleLogItem))));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_ScheduleLogItem)));
            }
        }
        
        public void ShowEvent(Np_Event _event)
        {
            string description = "Description: "+_event.description+"\r\n";
            string eventID = getEventIDText(_event.eventID);
            if (eventID.Length == 0)
            {
                return; //Ignore unrecognizable events
            }
            ShowID("Event Source ID", _event.sourceID);
            ShowTime("event occurTime", _event.occurTime);
            ShowText(eventID);
            ShowText("\r\n");
            ShowText(description);
        }

        public void ShowEventList(Np_EventList eventList)
        {
            if (0 == eventList.size)
            {
                ShowText("No Events");
                return;
            }

            IntPtr cursor = eventList.list;
            for (int i = 0; i < eventList.size; i++)
            {
                ShowEvent((Np_Event)(Marshal.PtrToStructure(cursor, typeof(Np_Event))));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_Event)));
            }
        }

        public void ShowMetadataSourceType(Np_MetadataSourceType type, ref string toString)
        {
            ShowText("Source Type:" , ref toString);
            switch (type)
            {
                case Np_MetadataSourceType.kMetadataPOS:
                    {
                        ShowText("kMetadataPOS", ref toString);
                        break;
                    }
                case Np_MetadataSourceType.kMetadataAccessControl:
                    {
                        ShowText("kMetadataAccessControl");
                        break;
                    }
                case Np_MetadataSourceType.kMetadataLPR:
                    {
                        ShowText("kMetadataLPR", ref toString);
                        break;
                    }
                default:
                    {
                        ShowText("kMetadataNone", ref toString);
                        break;
                    }
            }
        }

        public void ShowMetadataChannel(Np_MetadataChannel channel, ref string toString)
        {
            ShowText("Channels:", ref toString);
            ShowID("Channel ID: ", channel.id, ref toString);
            ShowText("Channel Name:\r\n"+ channel.name, ref toString);
            ShowText("\r\n", ref toString);
        }

        public void ShowMetadataSource(Np_MetadataSource src, ref string toString)
        {
            ShowID("==========Source ID: ", src.id, ref toString);
            ShowText("Source Name:" + src.name, ref toString);
            ShowMetadataSourceType(src.type, ref toString);
            ShowText("Source ip:\r\n" + src.ip, ref toString);
            ShowText("Source port:" + src.port.ToString() + "\r\n", ref toString);

            IntPtr channel = src.channels.items;
            for (int i = 0; i < src.channels.size; ++i)
            {
                ShowMetadataChannel((Np_MetadataChannel)(Marshal.PtrToStructure(channel, typeof(Np_MetadataChannel))), ref toString);
                channel = (IntPtr)(channel.ToInt32() + Marshal.SizeOf(typeof(Np_MetadataChannel)));
            }
        }

        public void ShowMetadataSourceList(Np_MetadataSourceList list)
        {
            string finalLog = "";
            if (0 < list.size)
            {
                IntPtr source = list.items;
                for (int i = 0; i < list.size; ++i)
                {
                    ShowMetadataSource((Np_MetadataSource)(Marshal.PtrToStructure(source, typeof(Np_MetadataSource))), ref finalLog);
                    source = (IntPtr)(source.ToInt32() + Marshal.SizeOf(typeof(Np_MetadataSource)));
                }
            }
            else
            {
                ShowText("No Metadata Source", ref finalLog);
            }
            ShowText(finalLog);
        }

        public void ShowMetadataLogItem(Np_MetadataLogItem metadataLogItem)
        {
            IntPtr iDListPtr = metadataLogItem.npIDList.IDList;
            for (int i = 0; i < metadataLogItem.npIDList.size; i++)
            {
                ShowText("----------Metdata start-----------");
                ShowID("Camera ID:", (Np_ID)(Marshal.PtrToStructure(iDListPtr, typeof(Np_ID))));
                iDListPtr = (IntPtr)(iDListPtr.ToInt32() + Marshal.SizeOf(typeof(Np_ID)));
                ShowID("Metadata Device ID:", metadataLogItem.metadata_id);
                ShowTime("Metdata logTime:", metadataLogItem.metadataTime);
                ShowText("codepage:");
                ShowText(metadataLogItem.codepage.ToString());
                ShowText("textDataLen:");
                ShowText(metadataLogItem.textDataLen.ToString());

                if (metadataLogItem.textDataLen > 0)
                {
                    byte[] metadataArray = null;
                    metadataArray = new byte[metadataLogItem.textDataLen + 1];
                    Marshal.Copy(metadataLogItem.textData, metadataArray, 0, metadataLogItem.textDataLen);
                    ShowText(Encoding.GetEncoding(metadataLogItem.codepage).GetString(metadataArray));
                }

                ShowText("\r\n----------Metdata end-----------\r\n");
            }
        }

        public void ShowMetadataLog(Np_MetadataLogList metadataList)
        {
            if (0 == metadataList.size)
            {
                ShowText("No Metadata Logs");
                return;
            }

            ShowText(" Metadata Logs Size:" + metadataList.size);
            IntPtr cursor = metadataList.metadataList;
            for (int i = 0; i < metadataList.size; i++)
            {
                ShowMetadataLogItem((Np_MetadataLogItem)(Marshal.PtrToStructure(cursor, typeof(Np_MetadataLogItem))));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_MetadataLogItem)));
            }
        }

        public void ShowCallbackEvent(Np_Event evt)
        {
            ShowText("===EventHandle===");
            string description = "";
            string eventID = getEventIDText(evt.eventID);
            string aux = string.Format("Aux: {0}\r\n", evt.auxiliaryCode);
            if (eventID.Length == 0)
            {
                return; //Ignore unrecognizable events
            }

            if (evt.eventID == Np_Event_Type.Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_START || 
                evt.eventID == Np_Event_Type.Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_STOP ||
                evt.eventID == Np_Event_Type.Np_EVENT_SMART_GUARD_SOUND_ALERT_START ||
                evt.eventID == Np_Event_Type.Np_EVENT_SMART_GUARD_SOUND_ALERT_STOP ||
                evt.eventID == Np_Event_Type.Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_START ||
                evt.eventID == Np_Event_Type.Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_STOP)
            {
                string oriDesc = evt.description;
                string strHeader = "EventId:";
                Np_Event_Type TriggerEventID = (Np_Event_Type)Int32.Parse(oriDesc.Substring(strHeader.Length));
                description = string.Format("Description: Trigger by {0}\r\n", getEventIDText(TriggerEventID));
            }
            else
            {
                description = "Description: " + evt.description + "\r\n";
            }
            ShowID("Event Source ID", evt.sourceID);
            if (m_serverType == Np_ServerType.kCorporate && 
                evt.eventID != Np_Event_Type.Np_EVENT_SERVER_CONNECTION_LOST)
                ShowTime("event occurTime", evt.occurTime);
            ShowText(eventID);
            ShowText("\r\n");
            ShowText(description);
            ShowText(aux);
        }

        public void ShowCallbackMetadata(MetadataCbInfo info)
        {
            ShowText("===Metadata start===");
            ShowText("centralID: " + info.id.centralID + "\r\nlocalID: " + info.id.localID + "\r\n");
            ShowText("isNew: " + info.isNew + "\r\nisComplete: " + info.isComplete + "\r\n");
            ShowText("topLeftX: " + info.displayRectangle.topLeftX + "\r\n" + "topLeftY: " + info.displayRectangle.topLeftY);
            ShowText("bottomRightX: " + info.displayRectangle.bottomRightX + "\r\n" + "bottomRightY: " + info.displayRectangle.bottomRightY + "\r\n");
            ShowText("displayTimeout: " + info.displayTimeout + "\r\n");
            ShowText("isUseDefaultRect: " + info.isUseDefaultRect + "\r\n");
            ShowText("length: " + info.len + "\r\n");
            ShowText("Codepage: " + info.codePage + "\r\n");
            if (info.len > 0)
                ShowText(Encoding.GetEncoding(info.codePage).GetString(info.textData));
            ShowText("\r\n");
            ShowText("===Metadata end===");
        }

        private void ShowCallbackBackupInfo(BackupCbInfo info)
        {
            string msg = "---------BackupInfo start----------\r\n";
            switch (info.m_status)
            {
                case Np_BackupStatus.kBackupStart:
                    msg += "Start Backup";
                    break;
                case Np_BackupStatus.kBackupSuccess:
                    btnStartBackup.Enabled = true;
                    btnPauseBackup.Enabled = false;
                    btnResumeBackup.Enabled = false;
                    btnAbortBackup.Enabled = false;
                    msg += "Start Success";
                    break;
                case Np_BackupStatus.kBackupUpdateFilePayload:
                    msg += "Update File " + info.m_fileName + " Payload:" + info.m_fileSize + "bytes";
                    break;
                case Np_BackupStatus.kBackupCreateNewFile:
                    msg += "Create File " + info.m_fileName;
                    break;
                case Np_BackupStatus.kBackupNetworkError:
                case Np_BackupStatus.kBackupFail:
                    msg += "Backup failed";
                    break;
                default:
                    break;
            }
            msg += "\r\n";
            ShowText(msg);
        }

        private void ShowID(string str, Np_ID id)
        {
            ShowText(str);
            ShowText("centralID: " + id.centralID + "\r\nlocalID: " + id.localID + "\r\n");
        }

        private void ShowID(string str, Np_ID id, ref string toString)
        {
            ShowText(str, ref toString);
            ShowText("centralID: " + id.centralID + "\r\nlocalID: " + id.localID + "\r\n", ref toString);
        }

        private void ShowSubDevice(Np_SubDevice_CS sub_device)
        {
            ShowID("SubDeviceID", sub_device.ID);
            ShowText("Device Name:");
            ShowText(sub_device.name);
            ShowText("Device Description:");
            ShowText(sub_device.description);
        }

        private void ShowPTZCapability(Np_SubDevice_CS sub_device)
        {
            Np_Result ret = Np_Result.Np_Result_OK;
            long ptzCap = 0;
            ret = NpClient.Info_GetPTZCapability(m_handle, sub_device.ID, out ptzCap);
            if (ret == Np_Result.Np_Result_OK)
            {
                string szCapability = "PTZ capability supported:\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZAreaZoom) != 0)
                    szCapability += "    Area Zoom\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZAutoPan) != 0)
                    szCapability += "    Auto Pan\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZFocus) != 0)
                    szCapability += "    Focus\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZLilin) != 0)
                    szCapability += "    Lilin\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZPane) != 0)
                    szCapability += "    Pane\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZPreset) != 0)
                    szCapability += "    Preset\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZSpeedDomeOSDMenu) != 0)
                    szCapability += "    Speed Dome OSD Menu\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZTilt) != 0)
                    szCapability += "    Tilt\r\n";
                if ((ptzCap & (long)Np_PTZCap.kPTZZoom) != 0)
                    szCapability += "    Zoom\r\n";
                ShowText(szCapability);
            }
        }

        private void ShowPTZPreset(Np_PTZPreset_CS ptzPreset)
        {
            ShowText("=====PresetInfo\r\n");
            ShowText("PresetName: " + ptzPreset.presetName); 
            ShowText("PresetNo: " + ptzPreset.presetNo);
            ShowText("\r\n");
        }

        private void ShowPTZPresetList(Np_PTZPreset_CS_List ptzPresetList)
        {
            IntPtr cursor = ptzPresetList.items;
            for (int i = 0; i < ptzPresetList.size; ++i)
            {
                ShowPTZPreset((Np_PTZPreset_CS)Marshal.PtrToStructure(cursor, typeof(Np_PTZPreset_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_PTZPreset_CS)));
            } 
        }

        private void ShowDIAssociatedDevice(Np_SubDevice_CS device)
        {
            Np_ID id = new Np_ID();
            Np_Result ret = NpClient.Info_GetDIAssociatedDevice(m_handle, device.ID, ref id);
            if (ret == Np_Result.Np_Result_OK)
            {
                ShowID("Associated Device",id);
            }
            else if (ret == Np_Result.Np_Result_NO_DATA)
            {
                ShowText("Associated Device\r\nNo Setting\r\n");
            }
        }

        private void ShowDevice(Np_Device_CS device)
        {
            ShowID("==========DeviceID", device.ID);
            ShowText("Device Name:");
            ShowText(device.name);
            ShowText("Device Description:");
            ShowText(device.description);
            showDeviceCapability(device);

            ShowText("Sensor Device:\r\n");
            Np_SubDevice_CS_List list = device.SensorDevices;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowSubDevice((Np_SubDevice_CS)Marshal.PtrToStructure(cursor, typeof(Np_SubDevice_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
            }
            ShowText("PTZ Device:\r\n");
            list = device.PTZDevices;
            cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_SubDevice_CS sub_device = (Np_SubDevice_CS)Marshal.PtrToStructure(cursor, typeof(Np_SubDevice_CS));
                ShowSubDevice(sub_device);
                ShowPTZCapability(sub_device);
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
            }
            ShowText("DI Device:\r\n");
            list = device.DIDevices;
            cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_SubDevice_CS sub_device = (Np_SubDevice_CS)Marshal.PtrToStructure(cursor, typeof(Np_SubDevice_CS));
                ShowSubDevice(sub_device);
                ShowDIAssociatedDevice(sub_device);
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
            }
            ShowText("DO Device:\r\n");
            list = device.DODevices;
            cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowSubDevice((Np_SubDevice_CS)Marshal.PtrToStructure(cursor, typeof(Np_SubDevice_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
            }
        }

        private void ShowDeviceGroup(Np_DeviceGroup_CS group)
        {
            ShowText("Group(Server) Name:");
            ShowText(group.name);
            ShowText("Group(Server) Description:");
            ShowText(group.description);
            ShowText("=====Cameras:\r\n");
            Np_Device_CS_List list = group.Camera;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowDevice((Np_Device_CS)Marshal.PtrToStructure(cursor, typeof(Np_Device_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
            }
            ShowText("=====IOBox:\r\n");
            list = group.IOBox;
            cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowDevice((Np_Device_CS)Marshal.PtrToStructure(cursor, typeof(Np_Device_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
            }
        }

        private void ShowDeviceList(Np_DeviceList_CS deviceList)
        {
            ShowText("Logical Group:\r\n");
            Np_DeviceGroup_CS_List list = deviceList.LogicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowDeviceGroup((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }

            ShowText("Physical Group:\r\n");
            list = deviceList.PhysicalGroup;
            cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                ShowDeviceGroup((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }
        }

        void showDeviceCapability(Np_Device_CS device)
        {
            Np_Result ret = Np_Result.Np_Result_OK;
            long capability = 0;
            ret = NpClient.Info_GetDeviceCapability(m_handle, device.ID, out capability);
            if (Np_Result.Np_Result_OK == ret)
            {
                string szCapability = "Device support:\r\n";

                if (0 == capability)
                {
                    szCapability += "    None\r\n";
                }
                else
                {
                    if ((capability & (long)Np_DeviceCapability.kDeviceAudio) != 0)
                        szCapability += "    Audio\r\n";
                    if ((capability & (long)Np_DeviceCapability.kDeviceTalk) != 0)
                        szCapability += "    Talk\r\n";
                    if ((capability & (long)Np_DeviceCapability.kDevicePTZ) != 0)
                            szCapability += "    PTZ\r\n";
                    if ((capability & (long)Np_DeviceCapability.kDeviceDIO) != 0)
                            szCapability += "    DIO\r\n";
                }
                ShowText(szCapability);
            }
        }

        void ShowStreamProfile(Np_StreamProfile streamProfile)
        {
            switch(streamProfile)
            {
                case Np_StreamProfile.kProfileNormal:
                {
                    ShowText("kProfileNormal");
                    break;
                }
                case Np_StreamProfile.kProfileOriginal:
                {
                    ShowText("kProfileOriginal");
                    break;
                }
                case Np_StreamProfile.kProfileLow:
                {
                    ShowText("kProfileLow");
                    break;
                }
                case Np_StreamProfile.kProfileMinimum:
                {
                    ShowText("kProfileMinimum");
                    break;
                }
                default:
                {
                    ShowText("Stream Profile is Unrecognizable !!!");
                    break;
                }
            }
        }

        void ShowSensorProfile(Np_SensorProfile_CS sensorProfile)
        {
            ShowStreamProfile(sensorProfile.profile);
            ShowText("Bitrate: "+sensorProfile.bitrate);
            ShowText("Codec: "+sensorProfile.codec);
            ShowText("FrameRate: "+sensorProfile.frameRate);
            ShowText("Quality: "+sensorProfile.quality);
            ShowText("Resolution: "+sensorProfile.resolution);
            ShowText("----------\n");
        }

        void ShowSensorProfileList(Np_SensorProfile_CS_List sensorProfileList)
        {
            IntPtr cursor = sensorProfileList.items;
            for (int i = 0; i < sensorProfileList.size; ++i)
            {
                ShowSensorProfile((Np_SensorProfile_CS)Marshal.PtrToStructure(cursor, typeof(Np_SensorProfile_CS)));
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_SensorProfile_CS)));
            }
        }

        private static Np_ServerType checkServerType(string type)
        {
            if (type == "MainConsole_LiveView")
                return Np_ServerType.kMainConsoleLiveview;
            if (type == "MainConsole_Playback")
                return Np_ServerType.kMainConsolePlayback;
            if (type == "Corporate")
                return Np_ServerType.kCorporate;
            if (type == "Titan")
                return Np_ServerType.kTitan;
            return Np_ServerType.kCorporate;
        }

        private static Np_Event_Type checkEventID(string id)
        {
            if (id == "ALL")
            {
                return Np_Event_Type.Np_EVENT_USER_DEFINED_CODE_BASE;
            }
            if (id == "Np_EVENT_MOTION_1")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_1;
            }
            if (id == "Np_EVENT_MOTION_2")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_2;
            }
            if (id == "Np_EVENT_MOTION_3")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_3;
            }
            if (id == "Np_EVENT_MOTION_4")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_4;
            }
            if (id == "Np_EVENT_MOTION_5")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_5;
            }
            if (id == "Np_EVENT_FOREIGN_OBJECT")
            {
                return Np_Event_Type.Np_EVENT_FOREIGN_OBJECT;
            }
            if (id == "Np_EVENT_MISSING_OBJECT")
            {
                return Np_Event_Type.Np_EVENT_MISSING_OBJECT;
            }
            if (id == "Np_EVENT_FOCUS_LOST")
            {
                return Np_Event_Type.Np_EVENT_FOCUS_LOST;
            }
            if (id == "Np_EVENT_CAMERA_OCCLUSION")
            {
                return Np_Event_Type.Np_EVENT_CAMERA_OCCLUSION;
            }
            if (id == "Np_EVENT_GENERAL_MOTION_DEVICE")
            {
                return Np_Event_Type.Np_EVENT_GENERAL_MOTION_DEVICE;
            }
            if (id == "Np_EVENT_SIGNAL_LOST")
            {
                return Np_Event_Type.Np_EVENT_SIGNAL_LOST;
            }
            if (id == "Np_EVENT_MOTION_START")
            {
                return Np_Event_Type.Np_EVENT_MOTION_START;
            }
            if (id == "Np_EVENT_MOTION_STOP")
            {
                return Np_Event_Type.Np_EVENT_MOTION_STOP;
            }
            if (id == "Np_EVENT_MANUAL_RECORD_MODE_START")
            {
                return Np_Event_Type.Np_EVENT_MANUAL_RECORD_MODE_START;
            }
            if (id == "Np_EVENT_MANUAL_RECORD_MODE_STOP")
            {
                return Np_Event_Type.Np_EVENT_MANUAL_RECORD_MODE_STOP;
            }
            if (id == "Np_EVENT_CONNECTION_LOST")
            {
                return Np_Event_Type.Np_EVENT_CONNECTION_LOST;
            }
            if (id == "Np_EVENT_SERVER_AUTO_BACKUP_START")
            {
                return Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_START;
            }
            if (id == "Np_EVENT_SERVER_AUTO_BACKUP_FAIL")
            {
                return Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_FAIL;
            }
            if (id == "Np_EVENT_SERVER_AUTO_BACKUP_STOP")
            {
                return Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_STOP;
            }
            if (id == "Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE")
            {
                return Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE;
            }
            if (id == "Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE")
            {
                return Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE;
            }
            if (id == "Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS")
            {
                return Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS;
            }
            if (id == "Np_EVENT_SERVER_CONNECTION_LOST")
            {
                return Np_Event_Type.Np_EVENT_SERVER_CONNECTION_LOST;
            }
            if (id == "Np_EVENT_INPUT_OPENED")
            {
                return Np_Event_Type.Np_EVENT_INPUT_OPENED;
            }
            if (id == "Np_EVENT_INPUT_CLOSED")
            {
                return Np_Event_Type.Np_EVENT_INPUT_CLOSED;
            }
            if (id == "Np_EVENT_RESOURCE_DEPLETED")
            {
                return Np_Event_Type.Np_EVENT_RESOURCE_DEPLETED;
            }
            if (id == "Np_EVENT_NETWORK_CONGESTION")
            {
                return Np_Event_Type.Np_EVENT_NETWORK_CONGESTION;
            }
            if (id == "Np_EVENT_SYSTEM_HEALTH_UNUSUAL")
            {
                return Np_Event_Type.Np_EVENT_SYSTEM_HEALTH_UNUSUAL;
            }
            if (id == "Np_EVENT_DISK_SPACE_EXHAUSTED")
            {
                return Np_Event_Type.Np_EVENT_DISK_SPACE_EXHAUSTED;
            }
            if (id == "Np_EVENT_DISK_ABNORMAL")
            {
                return Np_Event_Type.Np_EVENT_DISK_ABNORMAL;
            }
            if (id == "Np_EVENT_DAILY_REPORT")
            {
                return Np_Event_Type.Np_EVENT_DAILY_REPORT;
            }
            if (id == "Np_UNABLE_ACCESS_FTP")
            {
                return Np_Event_Type.Np_UNABLE_ACCESS_FTP;
            }
            if (id == "Np_UNFINISHED_BACKUP")
            {
                return Np_Event_Type.Np_UNFINISHED_BACKUP;
            }
            return Np_Event_Type.Np_EVENT_USER_DEFINED_CODE_BASE;
        }

        public static Np_Source_Device_Type checkDeviceTypeID(string id)
        {
            if (id == "ALL")
            {
                return Np_Source_Device_Type.Np_SOURCE_DEVICE_EMPTY;
            }
            if (id == "Np_SOURCE_DEVICE_SENSOR")
            {
                return Np_Source_Device_Type.Np_SOURCE_DEVICE_SENSOR;
            }
            if (id == "Np_SOURCE_DEVICE_DIGITAL_INPUT")
            {
                return Np_Source_Device_Type.Np_SOURCE_DEVICE_DIGITAL_INPUT;
            }
            if (id == "Np_SOURCE_DEVICE_SERVER")
            {
                return Np_Source_Device_Type.Np_SOURCE_DEVICE_SERVER;
            }
            return Np_Source_Device_Type.Np_SOURCE_DEVICE_EMPTY;
        }

        public static string getEventIDText(Np_Event_Type ID)
        {
            switch (ID)
            {
                //------sensor
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_1:
                    return "Np_EVENT_MOTION_1";
                    break;
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_2:
                    return "Np_EVENT_MOTION_2";
                    break;
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_3:
                    return "Np_EVENT_MOTION_3";
                    break;
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_4:
                    return "Np_EVENT_MOTION_4";
                    break;
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_5:
                    return "Np_EVENT_MOTION_5";
                    break;
                case Np_Event_Type.Np_EVENT_FOREIGN_OBJECT:
                    return "Np_EVENT_FOREIGN_OBJECT";
                    break;
                case Np_Event_Type.Np_EVENT_MISSING_OBJECT:
                    return "Np_EVENT_MISSING_OBJECT";
                    break;
                case Np_Event_Type.Np_EVENT_FOCUS_LOST:
                    return "Np_EVENT_FOCUS_LOST";
                    break;
                case Np_Event_Type.Np_EVENT_CAMERA_OCCLUSION:
                    return "Np_EVENT_CAMERA_OCCLUSION";
                    break;
                case Np_Event_Type.Np_EVENT_GENERAL_MOTION_DEVICE:
                    return "Np_EVENT_GENERAL_MOTION_DEVICE";
                    break;
                case Np_Event_Type.Np_EVENT_COUNTING:
                    return "Np_EVENT_COUNTING";
                    break;
                case Np_Event_Type.Np_EVENT_COUNTING_STOP:
                    return "Np_EVENT_COUNTING_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_START:
                    return "Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_START";
                    break;
                case Np_Event_Type.Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_STOP:
                    return "Np_EVENT_SMART_GUARD_ON_SCREEN_DISPLAY_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_SMART_GUARD_SOUND_ALERT_START:
                    return "Np_EVENT_SMART_GUARD_SOUND_ALERT_START";
                    break;
                case Np_Event_Type.Np_EVENT_SMART_GUARD_SOUND_ALERT_STOP:
                    return "Np_EVENT_SMART_GUARD_SOUND_ALERT_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_START:
                    return "Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_START";
                    break;
                case Np_Event_Type.Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_STOP:
                    return "Np_EVENT_EMAP_POPUP_AND_UPDATE_EVENT_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_RECORD_STATUS_UPDATE:
                    return "Np_EVENT_RECORD_STATUS_UPDATE";
                    break;
                case Np_Event_Type.Np_EVENT_SIGNAL_LOST:
                    return "Np_EVENT_SIGNAL_LOST";
                    break;
                case Np_Event_Type.Np_EVENT_SIGNAL_RESTORE:
                    return "Np_EVENT_SIGNAL_RESTORE";
                    break;
                case Np_Event_Type.Np_EVENT_MOTION_START:
                    return "Np_EVENT_MOTION_START";
                    break;
                case Np_Event_Type.Np_EVENT_MOTION_STOP:
                    return "Np_EVENT_MOTION_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_SERVER_CONNECTION_LOST:
                    return "Np_EVENT_SERVER_CONNECTION_LOST";
                    break;
                case Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_START:
                    return "Np_EVENT_SERVER_AUTO_BACKUP_START";
                    break;
                case Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_STOP:
                    return "Np_EVENT_SERVER_AUTO_BACKUP_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_SERVER_AUTO_BACKUP_FAIL:
                    return "Np_EVENT_SERVER_AUTO_BACKUP_FAIL";
                    break;
                case Np_Event_Type.Np_EVENT_MANUAL_RECORD_MODE_START:
                    return "Np_EVENT_MANUAL_RECORD_MODE_START";
                    break;
                case Np_Event_Type.Np_EVENT_MANUAL_RECORD_MODE_STOP:
                    return "Np_EVENT_MANUAL_RECORD_MODE_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_INPUT_OPENED:
                    return "Np_EVENT_INPUT_OPENED";
                    break;
                case Np_Event_Type.Np_EVENT_INPUT_CLOSED:
                    return "Np_EVENT_INPUT_CLOSED";
                    break;
                case Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE:
                    return "Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE";
                    break;
                case Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE:
                    return "Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE";
                    break;
                case Np_Event_Type.Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS:
                    return "Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS";
                    break;
                case Np_Event_Type.Np_EVENT_CONNECTION_LOST:
                    return "Np_EVENT_CONNECTION_LOST";
                    break;
                case Np_Event_Type.Np_EVENT_DEVICE_TREELIST_UPDATED:
                    return "Np_EVENT_DEVICE_TREELIST_UPDATED";
                    break;
                case Np_Event_Type.Np_EVENT_RESOURCE_DEPLETED:
                    return "Np_EVENT_RESOURCE_DEPLETED";
                    break;
                case Np_Event_Type.Np_EVENT_RESOURCE_DEPLETED_STOP:
                    return "Np_EVENT_RESOURCE_DEPLETED_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_NETWORK_CONGESTION:
                    return "Np_EVENT_NETWORK_CONGESTION";
                    break;
                case Np_Event_Type.Np_EVENT_NETWORK_CONGESTION_STOP:
                    return "Np_EVENT_NETWORK_CONGESTION_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_SYSTEM_HEALTH_UNUSUAL:
                    return "Np_EVENT_SYSTEM_HEALTH_UNUSUAL";
                    break;
                case Np_Event_Type.Np_EVENT_SYSTEM_HEALTH_UNUSUAL_STOP:
                    return "Np_EVENT_SYSTEM_HEALTH_UNUSUAL_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_DISK_SPACE_EXHAUSTED:
                    return "Np_EVENT_DISK_SPACE_EXHAUSTED";
                    break;
                case Np_Event_Type.Np_EVENT_DISK_SPACE_EXHAUSTED_STOP:
                    return "Np_EVENT_DISK_SPACE_EXHAUSTED_STOP";
                    break;
                case Np_Event_Type.Np_EVENT_DISK_ABNORMAL:
                    return "Np_EVENT_DISK_ABNORMAL";
                    break;
                case Np_Event_Type.Np_EVENT_DAILY_REPORT:
                    return "Np_EVENT_DAILY_REPORT";
                    break;
                case Np_Event_Type.Np_UNABLE_ACCESS_FTP:
                    return "Np_UNABLE_ACCESS_FTP";
                    break;
                case Np_Event_Type.Np_UNFINISHED_BACKUP:
                    return "Np_UNFINISHED_BACKUP";
                    break;
                case Np_Event_Type.Np_EVENT_TALK_BE_RESERVED:
                    return "Np_EVENT_TALK_BE_RESERVED";
                    break;
                default:
                    return "";
                    break;
            }
        }

        static Np_StreamProfile checkStreamProfile(string profile)
        {
            if (profile == "Normal")
            {
                return Np_StreamProfile.kProfileNormal;
            }
            if (profile == "Original")
            {
                return Np_StreamProfile.kProfileOriginal;
            }
            if (profile == "Low")
            {
                return Np_StreamProfile.kProfileLow;
            }
            if (profile == "Minimum")
            {
                return Np_StreamProfile.kProfileMinimum;
            }

            return Np_StreamProfile.kProfileNormal;
        }

        static void ConvertTalkAudioFormat(Np_TalkAudioFormat source, ref SoundRecorderAudioFormat target)
        {
            target.bitsPerSample = source.bitsPerSample;
            target.channels = source.channels;
            target.sampleRate = source.sampleRate;
            target.sampleRequest = source.sampleRequest;
        }

        private void SetLiveviewPTZUIStatus(Np_ID camID, bool disableAll)
        {
            long ptzCap = 0;
            if (!disableAll)
            {
                Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
                IntPtr cursor = list.items;
                for (int i = 0; i < list.size; ++i)
                {
                    Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                    Np_Device_CS_List device_list = group.Camera;
                    IntPtr cursor2 = device_list.items;
                    for (int j = 0; j < device_list.size; ++j)
                    {
                        Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));
                        if (device.ID.centralID == camID.centralID &&
                            device.ID.localID == camID.localID)
                        {
                            if (device.PTZDevices.size > 0)
                            {
                                Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(device.PTZDevices.items, typeof(Np_SubDevice_CS)));
                                NpClient.Info_GetPTZCapability(m_handle, sub_device.ID, out ptzCap);
                            }
                        }
                        cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                    }
                    cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
                }
            }

            btnZoomIn.Enabled = (ptzCap & (long)Np_PTZCap.kPTZZoom) != 0;
            btnZoomOut.Enabled = (ptzCap & (long)Np_PTZCap.kPTZZoom) != 0;

            btnFocusNear.Enabled = (ptzCap & (long)Np_PTZCap.kPTZFocus) != 0;
            btnFocusFar.Enabled = (ptzCap & (long)Np_PTZCap.kPTZFocus) != 0;

            btnUp.Enabled = (ptzCap & (long)Np_PTZCap.kPTZTilt) != 0;
            btnDown.Enabled = (ptzCap & (long)Np_PTZCap.kPTZTilt) != 0;

            btnLeft.Enabled = (ptzCap & (long)Np_PTZCap.kPTZPane) != 0;
            btnRight.Enabled = (ptzCap & (long)Np_PTZCap.kPTZPane) != 0;

            btnHome.Enabled = (ptzCap != 0);

            btnGetPTZPreset.Enabled = (ptzCap != 0);
            btnPresetGo.Enabled = (ptzCap != 0);
            btnPresetClear.Enabled = (ptzCap != 0);
            btnPresetSet.Enabled = (ptzCap != 0);
            lbPresetNumber.Enabled = (ptzCap != 0);
            tbPresetNumber.Enabled = (ptzCap != 0);
            lbPresetSetName.Enabled = (ptzCap != 0);
            tbPresetSetName.Enabled = (ptzCap != 0);
        }

        private void SetPlaybackButtonStatus(bool bFileOpen)
        {
            SetEnabled(btnPlay, bFileOpen);
            SetEnabled(btnPause, bFileOpen);
            SetEnabled(btnPrevious, bFileOpen);
            SetEnabled(btnNext, bFileOpen);
            SetEnabled(btnSeek, bFileOpen);
            SetEnabled(btnForward, bFileOpen);
            SetEnabled(btnBackward, bFileOpen);
            SetEnabled(btnBackward, bFileOpen);
            if (cmbExportProfile.Items.Count > 0)
            {
                SetEnabled(btnExport, bFileOpen);
                SetEnabled(btnStopExport, !bFileOpen);
            }
        }

        private void PutExportCbInfo(Np_ID id, Np_ExportError error, uint percent, int iFormateChangedIndex)
        {
            lock (m_exptcbmutex)
            {
                m_exptcbinfos.Add(new ExportCbInfo(id, error, percent, iFormateChangedIndex));
            }
        }

        private void SetExportProfileList()
        {
            Np_ExportFormatList fmtlist = new Np_ExportFormatList();
            Np_Result ret = NpClient.PlayBack_GetExportFormatList(m_pbPlayer, ref fmtlist);
            if(Np_Result.Np_Result_OK == ret)
            {
                cmbExportProfile.Items.Clear();
                IntPtr cursor = fmtlist.items;
                for(int i = 0; i < fmtlist.size; ++i)
                {
                    Np_ExportFormat fmt = (Np_ExportFormat)Marshal.PtrToStructure(cursor, typeof(Np_ExportFormat));
                    cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_ExportFormat)));
                    ExportProfileItem epi = new ExportProfileItem();
                    string desc_base = fmt.description;
                    epi.desc = desc_base;
                    epi.format = fmt.format;
                    epi.profile = 0;
                    if(0 == fmt.supportedProfile.size)
                    {
                        cmbExportProfile.Items.Add(epi);
                        continue;
                    }

                    IntPtr cursor2 = fmt.supportedProfile.items;
                    for(int j = 0; j < fmt.supportedProfile.size; ++j)
                    {
                        Np_ExportProfile profile = (Np_ExportProfile)Marshal.PtrToStructure(cursor2, typeof(Np_ExportProfile));
                        cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_ExportProfile)));
                        epi.desc = desc_base + " - " + profile.description;
                        epi.profile = profile.profile;
                        cmbExportProfile.Items.Add(epi);
                    }
                }
                ret = NpClient.PlayBack_ReleaseExportFormatList(m_pbPlayer, ref fmtlist);
                cmbExportProfile.SelectedIndex = 0;
            }
        }

        private void AddWaterMarkAfterExport(Np_ID id, int iFormatChangedIndex)
        {
            FileInfo fi = new FileInfo(tbExportPath.Text);
            string filpath = FormatExportFilePath(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length), 
                                                    iFormatChangedIndex, cmbExportProfile.Text);
            Np_DateTime startTime, endTime;
            ConvertTime(dtspFrom.Value, out startTime);
            ConvertTime(dtspTo.Value, out endTime);

            string camera_name = GetCameraName(id);

            NpClient.Utility_AddVideoWaterMark(filpath, ref startTime, ref endTime, camera_name);
        }

        private string GetCameraName(Np_ID id)
        {
            Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                Np_Device_CS_List device_list = group.Camera;
                IntPtr cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));
                    if (device.ID.centralID == id.centralID &&
                        device.ID.localID == id.localID)
                    {
                        return device.name;
                    }

                    Np_SubDevice_CS_List sensor_list = device.SensorDevices;
                    IntPtr cursor3 = sensor_list.items;
                    for (int k = 0; k < sensor_list.size; ++k)
                    {
                        Np_SubDevice_CS sensor = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sensor.ID.centralID == id.centralID &&
                            sensor.ID.localID == id.localID)
                        {
                            return device.name;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }
                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }

            return "";
        }

        
        //-----------------------------------------                       ----------------------------------------------------//
        //-----------------------------------------        General        ----------------------------------------------------//
        //-----------------------------------------                       ----------------------------------------------------//
        private void btnCreateHandle_Click(object sender, EventArgs e)
        {
            ShowText("*** Create_HandleWChar ***\r\n");
            m_serverType = checkServerType((string)(cmbServerType.SelectedItem));
            ushort port = 0;
            ushort.TryParse(tbPort.Text, out port);
            Np_Result ret = NpClient.Create_HandleWChar(ref m_handle,
                                                        m_serverType, 
                                                        tbUsername.Text, 
                                                        tbPassword.Text, 
                                                        tbIP.Text,
                                                        port);

            ShowResult(ret);

            if (Np_Result.Np_Result_OK == ret)
            {
                NpClient.Info_GetDeviceList_CS(m_handle, ref m_deviceList);

                cmbEventID.Items.Clear();
                if (Np_ServerType.kMainConsolePlayback == m_serverType)
                {
                    cmbEventID.Items.Clear();
                    cmbEventID.Items.Add("ALL");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_1");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_2");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_3");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_4");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_5");
                    cmbEventID.Items.Add("Np_EVENT_FOREIGN_OBJECT");
                    cmbEventID.Items.Add("Np_EVENT_MISSING_OBJECT");
                    cmbEventID.Items.Add("Np_EVENT_FOCUS_LOST");
                    cmbEventID.Items.Add("Np_EVENT_CAMERA_OCCLUSION");
                    cmbEventID.Items.Add("Np_EVENT_GENERAL_MOTION_DEVICE");
                    cmbEventID.Items.Add("Np_EVENT_SIGNAL_LOST");
                    cmbEventID.Items.Add("Np_EVENT_INPUT_OPENED");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_START");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_STOP");
                    cmbEventID.Items.Add("Np_EVENT_RESOURCE_DEPLETED");
                    cmbEventID.Items.Add("Np_EVENT_NETWORK_CONGESTION");
                    cmbEventID.Items.Add("Np_EVENT_SYSTEM_HEALTH_UNUSUAL");
                    cmbEventID.Items.Add("Np_EVENT_DISK_SPACE_EXHAUSTED");
                    cmbEventID.Enabled = true;
                }
                else if (Np_ServerType.kCorporate == m_serverType)
                {
                    cmbEventID.Items.Clear();
                    cmbEventID.Items.Add("ALL");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_START");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_STOP");
                    cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_START");
                    cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_STOP");
                    cmbEventID.Items.Add("Np_EVENT_CONNECTION_LOST");
                    cmbEventID.Items.Add("Np_EVENT_DISK_ABNORMAL");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_START");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_STOP");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_FAIL");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS");
                    cmbEventID.Items.Add("Np_EVENT_INPUT_OPENED");
                    cmbEventID.Items.Add("Np_EVENT_INPUT_CLOSED");
                    cmbEventID.Enabled = true;

                    cmbSourceTypeID.Items.Clear();
                    cmbSourceTypeID.Items.Add("ALL");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_SENSOR");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_DIGITAL_INPUT");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_SERVER");
                    cmbSourceTypeID.Enabled = true;

                    btnGetScheduleLogs.Enabled = false;
                }
            }
        }

        private void btnGetDeviceList_Click(object sender, EventArgs e)
        {
            Np_Result ret = NpClient.Info_ReleaseDeviceList_CS(m_handle, ref m_deviceList);
            ret = NpClient.Info_GetDeviceList_CS(m_handle, ref m_deviceList);
            ShowText("*** GetDeviceList_CS ***\r\n");
            ShowResult(ret);
            ShowDeviceList(m_deviceList);
        }

        private void btnSubscribeEvent_Click(object sender, EventArgs e)
        {
            Np_Result ret = NpClient.Event_Subscribe(m_handle, ref m_evtSession, m_evtcb, IntPtr.Zero);
            ShowText("*** Event_Subscribe ***\r\n");
            ShowResult(ret);
            if (ret == Np_Result.Np_Result_OK)
                btnSubscribeEvent.Enabled = false;
        }

        private void btnUnsubscribeEvent_Click(object sender, EventArgs e)
        {
            Np_Result ret = NpClient.Event_Unsubscribe(m_evtSession);
            ShowText("*** Event_Unsubscribe ***\r\n");
            ShowResult(ret);
            if (ret == Np_Result.Np_Result_OK)
                btnSubscribeEvent.Enabled = true;
        }

        private void btnDestroyHandle_Click(object sender, EventArgs e)
        {
            ShowText("*** Destroy_Handle ***\r\n");
            Np_Result ret = Np_Result.Np_Result_OK;
            if (IntPtr.Zero == m_handle)
                ShowText("handle is null...");

            cmbEventID.Enabled = true;

            NpClient.Info_ReleaseDeviceList_CS(m_handle, ref m_deviceList);

            ret = NpClient.Destroy_Handle(m_handle);
            ShowResult(ret);
            m_handle = IntPtr.Zero;
            m_lvSessions.Clear();
            m_pbSessions.Clear();
            m_lvPlayer = IntPtr.Zero;
            m_pbPlayer = IntPtr.Zero;

            btnSubscribeEvent.Enabled = true;
            btnUnsubscribeEvent.Enabled = true;
            cmbSourceTypeID.Items.Clear();
            cmbSourceTypeID.Enabled = false;
            cmbEventID.Enabled = false;
            btnGetScheduleLogs.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbMessages.Clear();
        }

        //-----------------------------------------                       ----------------------------------------------------//
        //-----------------------------------------        LiveView       ----------------------------------------------------//
        //-----------------------------------------                       ----------------------------------------------------//

        private void btnCreatePlayer_LV_Click(object sender, EventArgs e)
        {
            btnDetachSession_LV_Click(sender, e);
            btnDestroyPlayer_Click(sender, e);

            Np_Result ret = NpClient.LiveView_CreatePlayer(m_handle, ref m_lvPlayer);
            ShowText("*** LiveView_CreatePlayer ***\r\n");
            ShowResult(ret);
        }

        private void btnGetSensorProfile_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Np_Result ret = Np_Result.Np_Result_OK;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                Np_Device_CS_List device_list = group.Camera;
                IntPtr cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.SensorDevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if(sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            Np_SensorProfile_CS_List profileList = new Np_SensorProfile_CS_List();
                            ShowText("***...Get Sensor Profile List...***"); 
                            ret = NpClient.Info_GetSensorProfileList_CS(
                                m_handle, 
                                id, 
                                ref profileList
                                );
                            ShowSensorProfileList(profileList);
                            ShowResult(ret);
                            NpClient.Info_ReleaseSensorProfileList_CS(m_handle, ref profileList);
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }
                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }
                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }
            ShowText("***...No this Sensor device...***\n"); 
        }

        private void btnAttachSession_LV_Click(object sender, EventArgs e)
        {
            ShowText("***...AttachSession...***");    
            Np_Result ret = Np_Result.Np_Result_OK;    
            stSession st = new stSession();
            Int32.TryParse(tbCentralID.Text, out st.id.centralID);
            Int32.TryParse(tbLocalID.Text, out st.id.localID);
            IntPtr ctx = new IntPtr(session_series_number);

            for (int i = 0; i < m_lvSessions.Count; ++i)
            {
                int[] keys = new int[m_lvSessions.Count];
                m_lvSessions.Keys.CopyTo(keys, 0);
                if (m_lvSessions[keys[i]].id.centralID == st.id.centralID &&
                    m_lvSessions[keys[i]].id.localID == st.id.localID)
                {
                    ShowText("***...Already attach this session...***");
                    return;
                }
            }

            Np_StreamProfile profileID = checkStreamProfile(cmbProfileSelect.Text);
           
            ShowID("Attach ID:", st.id);
            ret = NpClient.LiveView_AttachSessionExt(   m_lvPlayer, ref st.session, st.id,
                                                        profileID,
                                                                Np_PixelFormat.kPixelFormatBGR24,
                                                                m_vcb, ctx,
                                                                m_acb, ctx, 
                                                                m_ecb, ctx);

            if (Np_Result.Np_Result_OK == ret)
            {
                m_lvSessions.Add(session_series_number, st);
                ++session_series_number;
                m_soundPlayer.StartPlay();
                SetLiveviewPTZUIStatus(st.id, false);
                btnShowPatrol.Enabled = true;
            }

            ShowResult(ret);
        }

        private void btnDetachSession_LV_Click(object sender, EventArgs e)
        {
            m_soundPlayer.StopPlay();

            ShowText("***...DetachSession...***");
            Np_Result ret = Np_Result.Np_Result_OK; 

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);
            for(int i = 0; i < m_lvSessions.Count; ++i)
            {
                int[] keys = new int[m_lvSessions.Count];
                m_lvSessions.Keys.CopyTo(keys, 0);
                if (m_lvSessions[keys[i]].id.centralID == id.centralID &&
                    m_lvSessions[keys[i]].id.localID == id.localID)
                {
                    ret = NpClient.LiveView_DetachSession(
                        m_lvPlayer,
                        m_lvSessions[keys[i]].session       
                        );

                    if (ret == Np_Result.Np_Result_OK)
                    {
                        EnablePatrolUI(false);
                        btnShowPatrol.Enabled = false;
                    }

                    ShowResult(ret);
                    m_lvSessions.Remove(keys[i]);
                    if (m_lvSessions.Count > 0)
                    {
                        keys = new int[m_lvSessions.Count];
                        m_lvSessions.Keys.CopyTo(keys, 0);
                        SetLiveviewPTZUIStatus(m_lvSessions[keys[m_lvSessions.Count - 1]].id, false);
                    }
                    else
                    {
                        SetLiveviewPTZUIStatus(id, true);
                    }
                    return;
                }
            }
            ShowText("***...No Such session...***"); 
        }

        private void btnSetAudioOn_Click(object sender, EventArgs e)
        {
            ShowText("*** LiveView_SetAudioOn ***\r\n");
            Np_Result ret = Np_Result.Np_Result_INVALID_ARGUMENT;
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);
            for (int i = 0; i < m_lvSessions.Count; ++i)
            {
                int[] keys = new int[m_lvSessions.Count];
                m_lvSessions.Keys.CopyTo(keys, 0);
                if (m_lvSessions[keys[i]].id.centralID == id.centralID &&
                    m_lvSessions[keys[i]].id.localID == id.localID)
                {
                    ret = NpClient.LiveView_SetAudioOn(m_lvPlayer, m_lvSessions[keys[i]].session);
                    ShowResult(ret);
                    return;
                }
            }
            
            ShowResult(ret);
        }

        private void btnSetAudioOff_Click(object sender, EventArgs e)
        {
            ShowText("*** LiveView_SetAudioOff ***\r\n");
            Np_Result ret = NpClient.LiveView_SetAudioOff(m_lvPlayer);
            ShowResult(ret);
        }

        private void btnGetAudioStatus_Click(object sender, EventArgs e)
        {
            ShowText("***...GetAudioStatus...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            for (int i = 0; i < m_lvSessions.Count; ++i)
            {
                int[] keys = new int[m_lvSessions.Count];
                m_lvSessions.Keys.CopyTo(keys, 0);
                if (m_lvSessions[keys[i]].id.centralID == id.centralID &&
                    m_lvSessions[keys[i]].id.localID == id.localID)
                {
                    Np_AudioStatus status = Np_AudioStatus.kAUDIO_OFF;
                    ret = NpClient.LiveView_GetSessionAudioStatus(m_lvPlayer, m_lvSessions[keys[i]].session, ref status);
                    if (Np_Result.Np_Result_OK == ret)
                    {
                        ShowText("Audio Status = " + (int)status + "\r\n");
                    }
                    ShowResult(ret);  
                    return;
                }
            }

            ShowResult(Np_Result.Np_Result_INVALID_ARGUMENT);
        }

        private void btnDestroyPlayer_Click(object sender, EventArgs e)
        {
            ShowText("*** LiveView_DestroyPlayer ***\r\n");
            Np_Result ret = NpClient.LiveView_DestroyPlayer(m_lvPlayer);
            m_lvPlayer = IntPtr.Zero;
            m_lvSessions.Clear();
            ShowResult(ret);
        }

        private void btnGetDIStatus_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Np_DIOStatus status = Np_DIOStatus.kDIO_OFF;
            Np_Result ret = Np_Result.Np_Result_OK;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                Np_Device_CS_List device_list = group.Camera;
                IntPtr cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DIDevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if(sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Show DI Status...***"); 

                            ret = NpClient.Info_GetDIOStatus(m_handle, id, out status);

                            ShowResult(ret);
                            ShowText("DI Status = " + (int)status + "\r\n");
                            ShowText("==========\r\n"); 
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                device_list = group.IOBox;
                cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DIDevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Show DI Status...***");

                            ret = NpClient.Info_GetDIOStatus(m_handle, id, out status);

                            ShowResult(ret);
                            ShowText("DI Status = " + (int)status + "\r\n");
                            ShowText("==========\r\n");
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }
           
            ShowText("***...No this DI device...***\n");
        }

        private void btnGetDOStatus_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Np_DIOStatus status = Np_DIOStatus.kDIO_OFF;
            Np_Result ret = Np_Result.Np_Result_OK;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                Np_Device_CS_List device_list = group.Camera;
                IntPtr cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DODevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Show DO Status...***");

                            ret = NpClient.Info_GetDIOStatus(m_handle, id, out status);

                            ShowResult(ret);
                            ShowText("DO Status = " + (int)status + "\r\n");
                            ShowText("==========\r\n");
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                device_list = group.IOBox;
                cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DODevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Show DO Status...***");

                            ret = NpClient.Info_GetDIOStatus(m_handle, id, out status);

                            ShowResult(ret);
                            ShowText("DO Status = " + (int)status + "\r\n");
                            ShowText("==========\r\n");
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }

            ShowText("***...No this DO device...***\n");
        }

        private void btnControlDO_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Np_Result ret = Np_Result.Np_Result_OK;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_DeviceGroup_CS_List list = m_deviceList.PhysicalGroup;
            IntPtr cursor = list.items;
            for (int i = 0; i < list.size; ++i)
            {
                Np_DeviceGroup_CS group = ((Np_DeviceGroup_CS)Marshal.PtrToStructure(cursor, typeof(Np_DeviceGroup_CS)));

                Np_Device_CS_List device_list = group.Camera;
                IntPtr cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DODevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Control Digital Output...***");
                            ret = NpClient.Control_DigitalOutput(
                                m_handle,
                                id,
                                (cmbControlDO.Text == "TurnOn")
                                );

                            ShowText(cmbControlDO.Text + " DO\r\n");
                            ShowResult(ret);
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                device_list = group.IOBox;
                cursor2 = device_list.items;
                for (int j = 0; j < device_list.size; ++j)
                {
                    Np_Device_CS device = ((Np_Device_CS)Marshal.PtrToStructure(cursor2, typeof(Np_Device_CS)));

                    Np_SubDevice_CS_List sub_device_list = device.DODevices;
                    IntPtr cursor3 = sub_device_list.items;
                    for (int k = 0; k < sub_device_list.size; ++k)
                    {
                        Np_SubDevice_CS sub_device = ((Np_SubDevice_CS)Marshal.PtrToStructure(cursor3, typeof(Np_SubDevice_CS)));
                        if (sub_device.ID.centralID == id.centralID &&
                            sub_device.ID.localID == id.localID)
                        {
                            ShowText("***...Control Digital Output...***");
                            ret = NpClient.Control_DigitalOutput(
                                m_handle,
                                id,
                                (cmbControlDO.Text == "TurnOn")
                                );

                            ShowText(cmbControlDO.Text + " DO\r\n");
                            ShowResult(ret);
                            return;
                        }
                        cursor3 = (IntPtr)(cursor3.ToInt32() + Marshal.SizeOf(typeof(Np_SubDevice_CS)));
                    }

                    cursor2 = (IntPtr)(cursor2.ToInt32() + Marshal.SizeOf(typeof(Np_Device_CS)));
                }

                cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_DeviceGroup_CS)));
            }

            ShowText("***...No this DO device...***\n");
        }

        private void PTZPan(Np_PanDirection direction)
        {
            Np_PTZContinuousMove ptzMoveDirection = new Np_PTZContinuousMove();
            ptzMoveDirection.pan = direction;
            PTZMove(ref ptzMoveDirection);
        }

        private void PTZTilt(Np_TiltDirection direction)
        {
            Np_PTZContinuousMove ptzMoveDirection = new Np_PTZContinuousMove();
            ptzMoveDirection.tilt = direction;
            PTZMove(ref ptzMoveDirection);
        }

        private void PTZZoom(Np_ZoomDirection direction)
        {
            Np_PTZContinuousMove ptzMoveDirection = new Np_PTZContinuousMove();
            ptzMoveDirection.zoom = direction;
            PTZMove(ref ptzMoveDirection);
        }

        private void PTZFocus(Np_FocusDirection direction)
        {
            Np_PTZContinuousMove ptzMoveDirection = new Np_PTZContinuousMove();
            ptzMoveDirection.focus = direction;
            PTZMove(ref ptzMoveDirection);
        }

        private void PTZMove(ref Np_PTZContinuousMove ptzMoveDirection)
        {
            Np_PTZControlParam_CS ptzParam = new Np_PTZControlParam_CS();

            ptzParam.command = Np_PTZCommand.kPTZContinuousMove;
            ptzParam.param.move = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_PTZContinuousMove)));
            Marshal.StructureToPtr(ptzMoveDirection, ptzParam.param.move, false);

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzParam);
            System.Threading.Thread.Sleep(300);
            PTZStop(ptzParam, ref ptzMoveDirection);
            Marshal.FreeHGlobal(ptzParam.param.move);
        }

        private void PTZStop(Np_PTZControlParam_CS ptzControlParam, ref Np_PTZContinuousMove ptzMoveCommand)
        {
            ptzControlParam.command = Np_PTZCommand.kPTZStop;
            //Stop previous PTZ command
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);
            NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzControlParam);
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            PTZZoom(Np_ZoomDirection.kZoomIn);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            PTZZoom(Np_ZoomDirection.kZoomOut);
        }

        private void btnFocusNear_Click(object sender, EventArgs e)
        {
            PTZFocus(Np_FocusDirection.kFocusNear);
        }

        private void btnFocusFar_Click(object sender, EventArgs e)
        {
            PTZFocus(Np_FocusDirection.kFocusFar);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            PTZTilt(Np_TiltDirection.kTiltUp);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            PTZTilt(Np_TiltDirection.kTiltDown);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            PTZPan(Np_PanDirection.kPanLeft);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            PTZPan(Np_PanDirection.kPanRight);
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            Np_PTZControlParam_CS ptzParam = new Np_PTZControlParam_CS();
            Np_PTZContinuousMove ptzMoveDirection = new Np_PTZContinuousMove();

            ptzParam.command = Np_PTZCommand.kPTZHome;
            ptzParam.param.move = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_PTZContinuousMove)));
            Marshal.StructureToPtr(ptzMoveDirection, ptzParam.param.move, false);

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzParam);
            System.Threading.Thread.Sleep(300);
            PTZStop(ptzParam, ref ptzMoveDirection);
            Marshal.FreeHGlobal(ptzParam.param.move);
        }

        private void btnGetPTZPreset_Click(object sender, EventArgs e)
        {
            ShowText("***...GetPTZPreset...***");

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_Result ret = Np_Result.Np_Result_OK;
            Np_PTZPreset_CS_List ptzPresetList = new Np_PTZPreset_CS_List();
            ret = NpClient.Info_GetPTZPreset_CS(
                    m_handle,
                    id,
                    ref ptzPresetList
                    );
            ShowPTZPresetList(ptzPresetList);
            ShowResult(ret);
            NpClient.Info_ReleasePTZPreset_CS(m_handle, ref ptzPresetList);
        }

        private void btnPresetGo_Click(object sender, EventArgs e)
        {
            ShowText("***...PresetGo...***");

            Np_PTZControlParam_CS ptzParam = new Np_PTZControlParam_CS();
            Np_PTZPreset_CS ptzPreset = new Np_PTZPreset_CS();

            ptzPreset.presetName = "";
            ptzPreset.presetNo = 0;
            Int32.TryParse(tbPresetNumber.Text, out ptzPreset.presetNo);

            ptzParam.command = Np_PTZCommand.kPTZPresetGo;
            ptzParam.param.preset = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_PTZPreset_CS)));
            Marshal.StructureToPtr(ptzPreset, ptzParam.param.preset, false);

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzParam);
            ShowResult(ret);
            Marshal.FreeHGlobal(ptzParam.param.preset);
        }

        private void btnPresetClear_Click(object sender, EventArgs e)
        {
            ShowText("***...PresetClear...***\r\n");
            if (0 == tbPresetNumber.Text.Length)
            {
                ShowResult(Np_Result.Np_Result_INVALID_ARGUMENT);
                ShowText("Please input a preset number.\r\n");
                return;
            }

            Np_PTZControlParam_CS ptzParam = new Np_PTZControlParam_CS();
            Np_PTZPreset_CS ptzPreset = new Np_PTZPreset_CS();

            ptzPreset.presetName = "";
            ptzPreset.presetNo = 0;
            Int32.TryParse(tbPresetNumber.Text, out ptzPreset.presetNo);

            ptzParam.command = Np_PTZCommand.kPTZPresetClear;
            ptzParam.param.preset = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_PTZPreset_CS)));
            Marshal.StructureToPtr(ptzPreset, ptzParam.param.preset, false);

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzParam);
            ShowResult(ret);
            Marshal.FreeHGlobal(ptzParam.param.preset);
        }

        private void btnPresetSet_Click(object sender, EventArgs e)
        {
            Np_PTZControlParam_CS ptzParam = new Np_PTZControlParam_CS();
            Np_PTZPreset_CS ptzPreset = new Np_PTZPreset_CS();

            ptzPreset.presetName = tbPresetSetName.Text;

            string preset_number = tbPresetNumber.Text.Trim();
            if (preset_number == "")
                ptzPreset.presetNo = 0;
            else
                ptzPreset.presetNo = Convert.ToInt32(preset_number);

            ptzParam.command = Np_PTZCommand.kPTZPresetSet;
            ptzParam.param.preset = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_PTZPreset_CS)));
            Marshal.StructureToPtr(ptzPreset, ptzParam.param.preset, false);

            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref ptzParam);
            ShowResult(ret);
            Marshal.FreeHGlobal(ptzParam.param.preset);
        }

        //-----------------------------------------                       ----------------------------------------------------//
        //-----------------------------------------        Playback       ----------------------------------------------------//
        //-----------------------------------------                       ----------------------------------------------------//

        private void btnCreatePlayer_PB_Click(object sender, EventArgs e)
        {
            ShowText("***...CreatePBPlayer...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_CreatePlayer(
                m_handle,
                ref m_pbPlayer
                );

            if (Np_Result.Np_Result_OK == ret)
            {
                SetExportProfileList();
            }

            ShowResult(ret);
        }

        private void btnAttachSession_PB_Click(object sender, EventArgs e)
        {
            ShowText("***...AttachSession...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            stSession st = new stSession();
            Int32.TryParse(tbCentralID_PB.Text, out st.id.centralID);
            Int32.TryParse(tbLocalID_PB.Text, out st.id.localID);
            ShowID("Attach ID:", st.id);
            IntPtr ctx = new IntPtr(session_series_number);

            ret = NpClient.PlayBack_AttachSessionExt(
                m_pbPlayer,
                ref st.session,
                st.id,
                Np_PixelFormat.kPixelFormatBGR24,
                m_vcb, ctx,
                m_acb, ctx,
                m_ecb, ctx
                );

            if (Np_Result.Np_Result_OK == ret)
            {
                m_pbSessions.Add(session_series_number, st);
                ++session_series_number;
            }
            ShowResult(ret);  
        }

        private void btnDetachSession_PB_Click(object sender, EventArgs e)
        {
            ShowText("***...DetachSession...***");    
            Np_Result ret = Np_Result.Np_Result_INVALID_ARGUMENT;

            Np_ID id;
            Int32.TryParse(tbCentralID_PB.Text, out id.centralID);
            Int32.TryParse(tbLocalID_PB.Text, out id.localID);
            for(int i = 0; i < m_pbSessions.Count; ++i)
            {
                int[] keys = new int[m_pbSessions.Count];
                m_pbSessions.Keys.CopyTo(keys, 0);
                if (m_pbSessions[keys[i]].id.centralID == id.centralID && 
                    m_pbSessions[keys[i]].id.localID == id.localID)
                {
                    ret = NpClient.PlayBack_DetachSession(
                        m_pbPlayer,
                        m_pbSessions[keys[i]].session        
                        );
                    ShowResult(ret);
                    m_pbSessions.Remove(keys[i]);
                    return;
                }
            }

            ShowResult(ret);
        }

        private void btnDestroyPlayer_PB_Click(object sender, EventArgs e)
        {
            ShowText("***...DestroyPlayer...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_DestroyPlayer(
                m_pbPlayer
                );
            m_pbPlayer = IntPtr.Zero;
            m_pbSessions.Clear();
            ShowResult(ret); 
        }

        int getMonthDays(int year, int month)
        {
            int[] month_days = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

            if(2 == month && 0 == year % 4)
            {
                return 29;
            }

            return month_days[month - 1];
        }

        private void btnGetRecordDateList_Click(object sender, EventArgs e)
        {
            ShowText("***...GetRecordDateList...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_RecordDateList dateList = new Np_RecordDateList();
            if (Np_ServerType.kCorporate == checkServerType(cmbServerType.Text))
            {
                ShowText("Range applied : ");
                Np_DateTime start, end;
                DateTime dt = dtspQueryDate.Value;
                DateTime dtStart = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                ConvertTime(dtStart, out start);
                ShowTime("Start date", start);
                DateTime dtEnd = new DateTime(dt.Year, dt.Month, getMonthDays(dt.Year, dt.Month), 0, 0, 0);
                ConvertTime(dtEnd, out end);
                ShowTime("End date", end);
                IntPtr ptrStart = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
                Marshal.StructureToPtr(start, ptrStart, false);
                IntPtr ptrEnd = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
                Marshal.StructureToPtr(end, ptrEnd, false);
                ret = NpClient.Info_GetRangedRecordDateList(
                    m_handle,
                    ptrStart,
                    ptrEnd,
                    ref dateList
                    );
                ShowText("==========\n");
                Marshal.FreeHGlobal(ptrStart);
                Marshal.FreeHGlobal(ptrEnd);
            }
            else
            {
                ret = NpClient.Info_GetRangedRecordDateList(
                    m_handle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ref dateList
                    );
            }
            if (Np_Result.Np_Result_OK == ret)
            {
                ShowRecordDateList(dateList);
                NpClient.Info_ReleaseRecordDateList(m_handle, ref dateList);
            }
            ShowResult(ret);
        }

        private void btnGetRecordLogs_Click(object sender, EventArgs e)
        {
            ShowText("***...GetRecordLogs...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_RecordLogList logList = new Np_RecordLogList();
            Np_DateTime queryTime;
            ConvertTime(dtspQueryDate.Value, out queryTime);
            ret = NpClient.Info_GetRecordLogs(
                m_handle,
                queryTime,
                ref logList
                );
            if (Np_Result.Np_Result_OK == ret)
            {
                ShowRecordLogs(logList);
                NpClient.Info_ReleaseRecordLogs(m_handle, ref logList);
            }
            ShowResult(ret);
        }

        private void btnGetScheduleLogs_Click(object sender, EventArgs e)
        {
            ShowText("***...GetScheduleLogs...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_ScheduleLogList logList = new Np_ScheduleLogList();
            Np_DateTime queryTime;
            ConvertTime(dtspQueryDate.Value, out queryTime);
            ret = NpClient.Info_GetScheduleLogs(
                m_handle,
                queryTime,
                ref logList
                );
            if (Np_Result.Np_Result_OK == ret)
            {
                ShowScheduleLogs(logList);
                NpClient.Info_ReleaseScheduleLogs(m_handle, ref logList);
            }
            ShowResult(ret);
        }

        private void btnQueryEvents_Click(object sender, EventArgs e)
        {
            ShowText("***...GetEventLogs...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_EventList eventList = new Np_EventList();
            IntPtr fromTime = IntPtr.Zero;
            IntPtr toTime = IntPtr.Zero;
            IntPtr deviceTypeID = IntPtr.Zero;
            IntPtr eventID = IntPtr.Zero;
            IntPtr id = IntPtr.Zero;

            if (dtspFrom.Value > dtspTo.Value)
            {
                ShowText("\r\n***...StartTime can't large than EndTime...***\r\n");
                return;
            }
            Np_DateTime tempTime = new Np_DateTime();
            fromTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
            toTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
            ConvertTime(dtspFrom.Value, out tempTime);
            Marshal.StructureToPtr(tempTime, fromTime, false);
            ConvertTime(dtspTo.Value, out tempTime);
            Marshal.StructureToPtr(tempTime, toTime, false);

            if (tbCentralID_PB.Text.Length > 0 && tbLocalID_PB.Text.Length > 0)
            {
                Np_ID id_actually;
                Int32.TryParse(tbCentralID_PB.Text, out id_actually.centralID);
                Int32.TryParse(tbLocalID_PB.Text, out id_actually.localID);
                id = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_ID)));
                Marshal.StructureToPtr(id_actually, id, false);
            }

            if (cmbSourceTypeID.Enabled)
            {
                Np_Source_Device_Type tmpDevType = checkDeviceTypeID(cmbSourceTypeID.Text);
                if (Np_Source_Device_Type.Np_SOURCE_DEVICE_EMPTY != tmpDevType)
                {
                    deviceTypeID = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                    Marshal.StructureToPtr((int)tmpDevType, deviceTypeID, false);
                }
            }

            Np_Event_Type tmp = checkEventID(cmbEventID.Text);
            if (Np_Event_Type.Np_EVENT_USER_DEFINED_CODE_BASE != tmp)
            {
                eventID = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.StructureToPtr((int)tmp, eventID, false);
            }

            ret = NpClient.Info_QueryEvents(m_handle,
                                            fromTime,
                                            toTime,
                                            deviceTypeID,
                                            id,
                                            eventID,
                                            ref eventList
                                            );

            if (Np_Result.Np_Result_OK == ret)
            {
                ShowEventList(eventList);
                NpClient.Info_ReleaseEvents(m_handle, ref eventList);
            }
            ShowResult(ret);

            Marshal.FreeHGlobal(fromTime);
            Marshal.FreeHGlobal(toTime);
            if (id != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(id);
            }
            if (eventID != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(eventID);
            }
            if (deviceTypeID != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(deviceTypeID);
            }
        }

        private void btnOpenRecord_Click(object sender, EventArgs e)
        {
            ShowText("***...OpenRecord...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_DateTime starttime;
            Np_DateTime endtime;
            if (dtspFrom.Value > dtspTo.Value)
            {
                ShowText("\r\n***...StartTime can't large than EndTime...***\r\n");
                return;
            }
            ConvertTime(dtspFrom.Value, out starttime);
            ConvertTime(dtspTo.Value, out endtime);
            ShowTime("start time", starttime);
            ShowTime("end time", endtime);
            ret = NpClient.PlayBack_OpenRecord(
                m_pbPlayer,
                starttime,
                endtime
                );
            ShowResult(ret);
            if (Np_Result.Np_Result_OK == ret)
            {
                m_soundPlayer.StopPlay();
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            m_soundPlayer.StartPlay();
            ShowText("***...Play...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_Play(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            ShowText("***...Pause...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_Pause(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnReverse_Click(object sender, EventArgs e)
        {
            ShowText("***...ReversePlay...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_ReversePlay(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            ShowText("***...StepForward...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_StepForward(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnBackward_Click(object sender, EventArgs e)
        {
            ShowText("***...StepBackward...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_StepBackward(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            ShowText("***...Next...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_Next(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            ShowText("***...Previous...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ret = NpClient.PlayBack_Previous(
                m_pbPlayer
                );
            ShowResult(ret); 
        }

        private void btnSeek_Click(object sender, EventArgs e)
        {
            ShowText("***...Seek...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_DateTime time;
            ConvertTime(dtspFrom.Value, out time);
            ShowTime("Seek Time", time);
            ret = NpClient.PlayBack_Seek(
                m_pbPlayer,
                time
                );
            ShowResult(ret); 
        }

        private void btnSetSpeed_Click(object sender, EventArgs e)
        {
            ShowText("***...SetSpeed...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            ShowText("Speed:");
            ShowText(nspSpeed.Value.ToString());
            ret = NpClient.PlayBack_SetSpeed(
                m_pbPlayer,
                (float)nspSpeed.Value
                );
            ShowResult(ret); 
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ShowText("***...ExportVideo...***");
            Np_Result ret = Np_Result.Np_Result_OK;

            if (dtspFrom.Value > dtspTo.Value)
            {
                ShowText("\n***...StartTime shouldn't be larger than EndTime...***\n");
                return;
            }

            if(cmbExportProfile.Items.Count <= 0)
            {
                ShowText("\n***...No profile supported, unable to export video...***\n");
                return;
            }

            Np_ExportContent content = new Np_ExportContent();
            Int32.TryParse(tbCentralID_PB.Text, out content.id.centralID);
            Int32.TryParse(tbLocalID_PB.Text, out content.id.localID);
            m_exptctx.id.centralID = content.id.centralID;
            m_exptctx.id.localID = content.id.localID;

            ConvertTime(dtspFrom.Value, out content.startTime);
            ConvertTime(dtspTo.Value, out content.endTime);

            for(int i = 0; i < m_pbSessions.Count; ++i)
            {
                int[] keys = new int[m_pbSessions.Count];
                m_pbSessions.Keys.CopyTo(keys, 0);
                if(m_exptctx.id.centralID == m_pbSessions[keys[i]].id.centralID && m_exptctx.id.localID == m_pbSessions[keys[i]].id.localID)
                {
                    m_exptctx.width = content.width = m_pbSessions[keys[i]].width;
                    m_exptctx.height = content.height = m_pbSessions[keys[i]].height;
                    break;
                }
            }

            content.excludeAudio = chbExcludeAudio.Checked;

            ExportProfileItem epi = (ExportProfileItem)cmbExportProfile.SelectedItem;
            try
            {
                FileInfo fi = new FileInfo(tbExportPath.Text);
                if (!fi.Directory.Exists || tbExportPath.Text.EndsWith("\\") || tbExportPath.Text.EndsWith("/"))
                {
                    ShowText("\n***...Should specify a valid file name...***\n");
                    return;
                }

                string fimename_base = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length);
                tbExportPath.Text = FormatExportFilePath(fimename_base, -1, epi.desc);

                ret = NpClient.PlayBack_ExportVideo(m_pbPlayer, content, fimename_base,
                                            epi.format, epi.profile, m_exptcb, IntPtr.Zero, m_osdcb, IntPtr.Zero);
            }
            catch(ArgumentException)
            {
                ShowText("\n***...Should specify a valid file name...***\n");
                return;
            }
            finally
            {
            }
            ShowResult(ret);
            m_bExporting = (Np_Result.Np_Result_OK == ret);
            btnExport.Enabled = !m_bExporting;
            btnStopExport.Enabled = m_bExporting;
        }

        private void btnExportPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export Video";
            sfd.Filter = "Videos (*.asf *.avi)|*.asf;*.avi;";
            sfd.ValidateNames = false;
            sfd.FileName = tbExportPath.Text;

            DialogResult result = sfd.ShowDialog(this);
            if (DialogResult.OK == result || DialogResult.Yes == result)
            {
                tbExportPath.Text = sfd.FileName;
            }
        }

        private void btnStopExport_Click(object sender, EventArgs e)
        {
            ShowText("***...StopExport...***");
            Np_Result ret = Np_Result.Np_Result_OK;

            ret = NpClient.PlayBack_StopExport(m_pbPlayer);
            ShowResult(ret);
            pbExportProgress.Value = 0;
            lbExportPercent.Text = "0%";

            FileInfo fi = new FileInfo(tbExportPath.Text);
            FileInfo fi2 = new FileInfo(FormatExportFilePath(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length),
                                        0, cmbExportProfile.Text));
        }

        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            ShowText("***...SnapShot...***");  
            try{
                FileInfo fi = new FileInfo(tbSnapShotPath.Text);
                if (!fi.Directory.Exists || tbSnapShotPath.Text.EndsWith("\\") || tbSnapShotPath.Text.EndsWith("/"))
                {
                    ShowText("\n***...Should specify a valid file name...***\n");
                    return;
                }

                if( !tbSnapShotPath.Text.EndsWith(".bmp", true, System.Globalization.CultureInfo.CurrentUICulture) &&
                    !tbSnapShotPath.Text.EndsWith(".jpg", true, System.Globalization.CultureInfo.CurrentUICulture))
                {
                    tbSnapShotPath.Text = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".jpg";
                }
            }
            catch(ArgumentException)
            {
                ShowText("\n***...Should specify a valid file name...***\n");
                return;
            }
            finally
            {
            }

            Np_Result ret = Np_Result.Np_Result_NO_DATA;
            Np_Frame frame = new Np_Frame();
            Np_ID id = new Np_ID();

            if (m_lvPlayer != IntPtr.Zero)
            {
                Int32.TryParse(tbCentralID.Text, out id.centralID);
                Int32.TryParse(tbLocalID.Text, out id.localID);
                for (int i = 0; i < m_lvSessions.Count; ++i)
                {
                    int[] keys = new int[m_lvSessions.Count];
                    m_lvSessions.Keys.CopyTo(keys, 0);
                    if (m_lvSessions[keys[i]].id.centralID == id.centralID &&
                        m_lvSessions[keys[i]].id.localID == id.localID)
                    {
                        ret = NpClient.LiveView_GetSessionCurrentImage(
                            m_lvPlayer,
                            m_lvSessions[keys[i]].session,
                            ref frame);
                        break;
                    }
                }
            }
            else if (m_pbPlayer != IntPtr.Zero)
            {
                Int32.TryParse(tbCentralID_PB.Text, out id.centralID);
                Int32.TryParse(tbLocalID_PB.Text, out id.localID);
                for (int i = 0; i < m_pbSessions.Count; ++i)
                {
                    int[] keys = new int[m_pbSessions.Count];
                    m_pbSessions.Keys.CopyTo(keys, 0);
                    if (m_pbSessions[keys[i]].id.centralID == id.centralID &&
                        m_pbSessions[keys[i]].id.localID == id.localID)
                    {
                        ret = NpClient.PlayBack_GetSessionCurrentImage(
                            m_pbPlayer,
                            m_pbSessions[keys[i]].session,
                            ref frame);
                        break;
                    }
                }
            }

            if (Np_Result.Np_Result_OK == ret)
            {
                ret = NpClient.Utility_SaveSnapShotImage(
                    tbSnapShotPath.Text, 
                    frame.buffer,
                    frame.len,
                    frame.width,
                    frame.height);

                if (m_lvPlayer != IntPtr.Zero)
                {
                    NpClient.LiveView_ReleaseSessionCurrentImage(m_lvPlayer, ref frame);
                }
                else if (m_pbPlayer != IntPtr.Zero)
                {
                    NpClient.PlayBack_ReleaseSessionCurrentImage(m_pbPlayer, ref frame);
                }
            }

            ShowResult(ret);

            if(Np_Result.Np_Result_OK == ret)
            {
                if(m_lvSessions.Count > 0)
                {
                    Np_DateTime now;
                    ConvertTime(DateTime.Now, out now);
                    ret = NpClient.Utility_AddImageWaterMark(tbSnapShotPath.Text, ref now, GetCameraName(id));
                }
                else if(m_pbSessions.Count > 0)
                {
                    Np_DateTime pb_time;
                    NpClient.PlayBack_GetTime(m_pbPlayer, out pb_time);
                    ret = NpClient.Utility_AddImageWaterMark(tbSnapShotPath.Text, ref pb_time, GetCameraName(id));
                }
            }
        }

        private void btnSnapShotPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "SnapShot";
            sfd.Filter = "Images (*.bmp *.jpg)|*.bmp;*.jpg;";
            sfd.ValidateNames = false;

            FileInfo fi = new FileInfo(tbSnapShotPath.Text);
            if (!fi.Directory.Exists || tbSnapShotPath.Text.EndsWith("\\") || tbSnapShotPath.Text.EndsWith("/"))
                sfd.FileName = "test.bmp";
            else
                sfd.FileName = fi.Name;

            DialogResult result = sfd.ShowDialog(this);
            if (DialogResult.OK == result || DialogResult.Yes == result)
            {
                tbSnapShotPath.Text = sfd.FileName;
            }
        }

        private void cmbSourceTypeID_SelectedIndexChanged(object sender, EventArgs e)
        {
            Np_Source_Device_Type tmp = checkDeviceTypeID(cmbSourceTypeID.Text);

            if (Np_Source_Device_Type.Np_SOURCE_DEVICE_EMPTY == tmp)
            {
                cmbEventID.Items.Clear();
                cmbEventID.Items.Add("ALL");
                cmbEventID.Items.Add("Np_EVENT_MOTION_START");
                cmbEventID.Items.Add("Np_EVENT_MOTION_STOP");
                cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_START");
                cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_STOP");
                cmbEventID.Items.Add("Np_EVENT_CONNECTION_LOST");
                cmbEventID.Items.Add("Np_EVENT_DISK_ABNORMAL");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_START");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_STOP");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_FAIL");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS");
                cmbEventID.Items.Add("Np_EVENT_INPUT_OPENED");
                cmbEventID.Items.Add("Np_EVENT_INPUT_CLOSED");
            }
            else if (Np_Source_Device_Type.Np_SOURCE_DEVICE_SENSOR == tmp)
            {
                cmbEventID.Items.Clear();
                cmbEventID.Items.Add("ALL");
                cmbEventID.Items.Add("Np_EVENT_MOTION_START");
                cmbEventID.Items.Add("Np_EVENT_MOTION_STOP");
                cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_START");
                cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_STOP");
                cmbEventID.Items.Add("Np_EVENT_CONNECTION_LOST");
            }
            else if (Np_Source_Device_Type.Np_SOURCE_DEVICE_DIGITAL_INPUT == tmp)
            {
                cmbEventID.Items.Clear();
                cmbEventID.Items.Add("ALL");
                cmbEventID.Items.Add("Np_EVENT_INPUT_OPENED");
                cmbEventID.Items.Add("Np_EVENT_INPUT_CLOSED");
                cmbEventID.Items.Add("Np_EVENT_CONNECTION_LOST");
            }
            else if (Np_Source_Device_Type.Np_SOURCE_DEVICE_SERVER == tmp)
            {
                cmbEventID.Items.Clear();
                cmbEventID.Items.Add("ALL");
                cmbEventID.Items.Add("Np_EVENT_DISK_ABNORMAL");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_START");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_STOP");
                cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_FAIL");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE");
                cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS");
            }
            cmbEventID.SelectedIndex = 0;
        }

        private void btnEnableTalk_Click(object sender, EventArgs e)
        {
            Np_Result ret = Np_Result.Np_Result_FAILED;
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            ShowText("*** Talk_Enable ***\r\n");
            if (!m_bEnableTalk)
            {
                Np_TalkAudioFormat fmt = new Np_TalkAudioFormat();
                SoundRecorderAudioFormat fmtRecorder = new SoundRecorderAudioFormat();
                ret = NpClient.Talk_Enable(m_handle, id, ref fmt); ;
                if (Np_Result.Np_Result_OK == ret)
                {
                    ConvertTalkAudioFormat(fmt, ref fmtRecorder);
                    SoundRecorder.Create_SoundRecorder(ref m_recorder);
                    SoundRecorder.Start_SoundRecord(m_recorder, fmtRecorder, m_rcb, IntPtr.Zero);
                    m_bEnableTalk = true;
                }
            }

            ShowResult(ret);
        }

        private Np_Result DisableTalk()
        {
            Np_Result ret = Np_Result.Np_Result_INVALID_ARGUMENT;
            if (m_bEnableTalk)
            {
                SoundRecorder.Stop_SoundRecord(m_recorder);
                SoundRecorder.Destroy_SoundRecorder(m_recorder);
                ret = NpClient.Talk_Disable(m_handle);
                m_bEnableTalk = false;
            }

            return ret;
        }

        private void btnDisableTalk_Click(object sender, EventArgs e)
        {
            ShowText("*** Talk_Disable ***\r\n");
            Np_Result ret = DisableTalk();
            ShowResult(ret);
        }

        private void btnGetTalkID_Click(object sender, EventArgs e)
        {
            ShowText("*** Talk_GetEnabledID ***\r\n");
            Np_ID id = new Np_ID();
            Np_Result ret = NpClient.Talk_GetEnabledID(m_handle, ref id);
            if (Np_Result.Np_Result_OK == ret)
            {
                ShowID("*** Talk Enabled Device ***\r\n", id);
            }
            ShowResult(ret);
        }

        private void btnCreateHandleAndSubscribeEvent_Click(object sender, EventArgs e)
        {
            ShowText("***...CreateHandle and SubscribeEvent...***");
            m_serverType = checkServerType((string)(cmbServerType.SelectedItem));
            ushort port = 0;
            ushort.TryParse(tbPort.Text, out port);
            Np_Result ret = NpClient.Create_Handle_And_Event_Subscribe(ref m_handle,    
                                                                       m_serverType,
                                                                       tbUsername.Text,
                                                                       tbPassword.Text,
                                                                       tbIP.Text,
                                                                       port,
                                                                       m_evtcb, IntPtr.Zero);

            ShowResult(ret);

            if (Np_Result.Np_Result_OK == ret)
            {
                btnSubscribeEvent.Enabled = false;
                btnUnsubscribeEvent.Enabled = false;

                NpClient.Info_GetDeviceList_CS(m_handle, ref m_deviceList);

                cmbEventID.Items.Clear();
                if (Np_ServerType.kCorporate == m_serverType)
                {
                    cmbEventID.Items.Clear();
                    cmbEventID.Items.Add("ALL");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_START");
                    cmbEventID.Items.Add("Np_EVENT_MOTION_STOP");
                    cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_START");
                    cmbEventID.Items.Add("Np_EVENT_MANUAL_RECORD_MODE_STOP");
                    cmbEventID.Items.Add("Np_EVENT_CONNECTION_LOST");
                    cmbEventID.Items.Add("Np_EVENT_DISK_ABNORMAL");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_START");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_STOP");
                    cmbEventID.Items.Add("Np_EVENT_SERVER_AUTO_BACKUP_FAIL");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_NONE");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_SCHEDULE");
                    cmbEventID.Items.Add("Np_EVENT_RECORD_SETTING_CHANGE_TO_ALWAYS");
                    cmbEventID.Items.Add("Np_EVENT_INPUT_OPENED");
                    cmbEventID.Items.Add("Np_EVENT_INPUT_CLOSED");
                    cmbEventID.Enabled = true;


                    cmbSourceTypeID.Items.Clear();
                    cmbSourceTypeID.Items.Add("ALL");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_SENSOR");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_DIGITAL_INPUT");
                    cmbSourceTypeID.Items.Add("Np_SOURCE_DEVICE_SERVER");
                    cmbSourceTypeID.Enabled = true;

                    btnGetScheduleLogs.Enabled = false;
                }
            }
        }
        private void btnSubscribeMetadata_Click(object sender, EventArgs e)
        {
            Np_Result ret = NpClient.LiveView_SubscribeMetadata(m_handle, m_metadatacb, IntPtr.Zero);
            ShowText("*** Metadata_Subscribe ***\r\n");
            ShowResult(ret);
        }

        private void btnUnsubscribeMetadata_Click(object sender, EventArgs e)
        {
            ShowText("***...UnSubscribeMetadata...***");
            Np_Result ret = NpClient.LiveView_UnsubscribeMetadata(
                m_handle
                );
            ShowResult(ret); 
        }

        private void ShowPTZPatrol(Np_PTZPatrol_CS info)
        {
            string str;
            str = "PatrolStartEnabled:" + info.isPatrolStartEnabled + "    "
                  + "PatrolStopEnabled:" + info.isPatrolStopEnabled + "\r\n"
                  + "Active Group:" + info.activeGroupIndex + "    "
                  + "Group Amount:" + info.maxPatrolGroupNumber + "\r\n";

            ShowText(str);

            for (int i = 0; i < info.maxPatrolGroupNumber; i++)
            {
                str = "==== Group " + i + " ====\r\n"
                      + "    -Name: " + info.group[i].name + "\r\n"
                      + "    -Time: " + info.group[i].period + "\r\n"
                      + "    -Preset Amount: " + info.group[i].presetCount;
                ShowText(str);

                for (int j = 0; j < info.group[i].presetCount; j++)
                {
                    str = "        -Preset " + j + ": " + info.group[i].presetPoint[j];
                    ShowText(str);
                }

                ShowText("");
            }

        }

        private void UpdatePatrolGroupUI(Np_PatrolGroup_CS group)
        {
            textGroupName.Text = group.name.ToString();
            textStayTime.Text = group.period.ToString();

            comboGroupPreset.Text = "";
            comboGroupPreset.Items.Clear();
            for (int i = 0; i < group.presetCount; i++)
                comboGroupPreset.Items.Add(group.presetPoint[i].ToString());

            if (group.presetCount > 0)
                comboGroupPreset.SelectedIndex = 0;
        }

        private void btnShowPatrol_Click(object sender, EventArgs e)
        {
            ShowText("***...Info_GetPTZInfo_CS...***\r\n");
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_PTZInfo_CS info = new Np_PTZInfo_CS();
            Np_Result ret = NpClient.Info_GetPTZInfo_CS(m_handle, id, ref info);

            if (ret == Np_Result.Np_Result_OK)
            {
                ShowPTZPatrol(info.ptzPatrol);

                comboGroupNo.Items.Clear();
                for (int i = 0; i < info.ptzPatrol.maxPatrolGroupNumber; i++)
                    comboGroupNo.Items.Add(i.ToString());

                if (info.ptzPatrol.maxPatrolGroupNumber > 0)
                    comboGroupNo.SelectedIndex = 0;
            }

            EnablePatrolUI(ret == Np_Result.Np_Result_OK);
            ShowResult(ret);
        }

        private void onComboGroupNo_Changed(object sender, EventArgs e)
        {
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_PTZInfo_CS info = new Np_PTZInfo_CS();
            Np_Result ret = NpClient.Info_GetPTZInfo_CS(m_handle, id, ref info);

            int group_index = comboGroupNo.SelectedIndex;
            if (ret == Np_Result.Np_Result_OK && group_index >= 0
                && group_index < info.ptzPatrol.maxPatrolGroupNumber)
            {
                UpdatePatrolGroupUI(info.ptzPatrol.group[group_index]);

                if (info.ptzPatrol.activeGroupIndex == group_index)
                    checkActiveGroup.CheckState = CheckState.Checked;
                else
                    checkActiveGroup.CheckState = CheckState.Unchecked;
            }
        }

        private void EnablePatrolUI(bool is_enable)
        {
            labelGroupName.Enabled = is_enable;
            labelGroupNo.Enabled = is_enable;
            labelGroupPreset.Enabled = is_enable;
            labelStay.Enabled = is_enable;
            labelSec.Enabled = is_enable;

            textGroupName.Enabled = is_enable;
            textStayTime.Enabled = is_enable;

            comboGroupNo.Enabled = is_enable;
            comboGroupPreset.Enabled = is_enable;

            checkAddPatrolPreset.Enabled = is_enable;
            checkDeletePatrolPreset.Enabled = is_enable;
            checkActiveGroup.Enabled = is_enable;

            btnSetPatrol.Enabled = is_enable;
            btnStartStopPatrol.Enabled = is_enable;
        }

        private bool GetPatrolGroupSetting(ref Np_PatrolGroup_CS group,
                                           Np_PTZPreset_CS_List preset_list)
        {
            bool is_succeed = true;

            string group_name = textGroupName.Text.ToString();
            string preset_name = tbPresetSetName.Text.ToString();
            bool is_add = checkAddPatrolPreset.Checked;
            bool is_delete = checkDeletePatrolPreset.Checked;

            int stay_time = 0;
            Int32.TryParse(textStayTime.Text, out stay_time);

            group.name = group_name;
            group.period = stay_time;

            if (is_add && !is_delete)
            {
                string preset_number = tbPresetNumber.Text.Trim();

                if (preset_number != "")
                {
                    int add_preset = 0;
                    Int32.TryParse(preset_number, out add_preset);

                    bool is_exist = false;

                    IntPtr cursor = preset_list.items;
                    for (int i = 0; i < preset_list.size; ++i)
                    {
                        Np_PTZPreset_CS preset = (Np_PTZPreset_CS)Marshal.PtrToStructure(cursor, typeof(Np_PTZPreset_CS));

                        if (preset.presetNo == add_preset)
                        {
                            is_exist = true;
                            break;
                        }

                        cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_PTZPreset_CS)));
                    }

                    if (is_exist)
                    {
                        group.presetPoint[group.presetCount] = add_preset;
                        ++group.presetCount;
                    }
                    else
                    {
                        is_succeed = false;
                        ShowText("Preset number does not exist");
                    }
                }
                else
                {
                    is_succeed = false;
                    ShowText("Preset number is empty");
                }
            }

            if (is_delete && !is_add)
            {
                int delete_index = comboGroupPreset.SelectedIndex;
                if (delete_index >= 0 && delete_index < group.presetCount)
                {
                    for (int i = delete_index; i < group.presetPoint.Length - 1; i++)
                    {
                        group.presetPoint[i] = group.presetPoint[i + 1];
                    }

                    group.presetPoint[group.presetPoint.Length - 1] = 0;
                }

                --group.presetCount;
            }

            return is_succeed;
        }

        private void onSetPatrol_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_PTZInfo_CS info = new Np_PTZInfo_CS();
            Np_Result get_ret = NpClient.Info_GetPTZInfo_CS(m_handle, id, ref info);

            if (get_ret != Np_Result.Np_Result_OK)
                return;

            int group_number = comboGroupNo.SelectedIndex;
            bool is_get_setting_succeed = GetPatrolGroupSetting(ref info.ptzPatrol.group[group_number], info.ptzPresetList);

            if (is_get_setting_succeed)
            {
                if (checkActiveGroup.Checked)
                    info.ptzPatrol.activeGroupIndex = group_number;
                else
                    info.ptzPatrol.activeGroupIndex = -1;

                ShowText("***...Control_SetPatrol...***\r\n");
                Np_Result ret = NpClient.Control_SetPatrol(m_handle, id, ref info.ptzPatrol);
                ShowResult(ret);

                onComboGroupNo_Changed(null, null);
            }
        }

        private void onStartStopPatrol_Click(object sender, EventArgs e)
        {
            Np_ID id;
            Int32.TryParse(tbCentralID.Text, out id.centralID);
            Int32.TryParse(tbLocalID.Text, out id.localID);

            Np_PTZInfo_CS info = new Np_PTZInfo_CS();
            NpClient.Info_GetPTZInfo_CS(m_handle, id, ref info);

            Np_PTZControlParam_CS control = new Np_PTZControlParam_CS();

            if (info.ptzPatrol.isPatrolStartEnabled != 0)
            {
                control.command = Np_PTZCommand.kPTZPatrolStart;
                ShowText("***...Start Patrol...***\r\n");
            }
            else
            {
                control.command = Np_PTZCommand.kPTZPatrolStop;
                ShowText("***...Stop Patrol...***\r\n");
            }

            Np_Result ret = NpClient.Control_PTZ_PTZDeviceID_CS(m_handle, id, ref control);
            ShowResult(ret);

            Marshal.FreeHGlobal(control.param.preset);
        }

        private void onAddPatrolPreset_Click(object sender, EventArgs e)
        {
            checkDeletePatrolPreset.CheckState = CheckState.Unchecked;
        }

        private void onDeletePatrolPreset_Click(object sender, EventArgs e)
        {
            checkAddPatrolPreset.CheckState = CheckState.Unchecked;
        }

        private void onGetPlaybackMetadata_Click(object sender, EventArgs e)
        {
            ShowText("***...GetPlaybackMetadata...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_MetadataLogList metadataList = new Np_MetadataLogList();
            Np_MetadataSearchCriterion criterion = new Np_MetadataSearchCriterion();
            bool isLogExceedMaxLimit = false;

            if (dtspFrom.Value > dtspTo.Value)
            {
                ShowText("\r\n***...StartTime can't large than EndTime...***\r\n");
                return;
            }

            int chLocalID = 0, chCentralID = 0;
            Np_ID newNpID;
            Int32.TryParse(tbLocalID_PB.Text, out chLocalID);
            Int32.TryParse(tbCentralID_PB.Text, out chCentralID);

            if (chLocalID != 0)
            {
                newNpID.centralID = chCentralID;
                newNpID.localID = chLocalID;
                criterion.metadataDeviceID.IDList = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_ID)));
                Marshal.StructureToPtr(newNpID, criterion.metadataDeviceID.IDList, false);
                criterion.metadataDeviceID.size = 1;
            }
            else
            {
                criterion.metadataDeviceID.IDList = IntPtr.Zero;
                criterion.metadataDeviceID.size = 0;
            }

            criterion.startTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
            criterion.endTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_DateTime)));
            Np_DateTime tempTime = new Np_DateTime();
            ConvertTime(dtspFrom.Value, out tempTime);
            Marshal.StructureToPtr(tempTime, criterion.startTime, false);
            ConvertTime(dtspTo.Value, out tempTime);
            Marshal.StructureToPtr(tempTime, criterion.endTime, false);

            ret = NpClient.Info_GetMetadataLog(
                m_handle,
                criterion,
                ref metadataList,
                ref isLogExceedMaxLimit
                );

            if (Np_Result.Np_Result_OK == ret)
            {
                if (isLogExceedMaxLimit == true)
                    ShowText("The query data exceeded the maximum limitation. Please shorten the time interval.");
                ShowMetadataLog(metadataList);
                NpClient.Info_ReleaseMetadataLog(m_handle, ref metadataList);
            }
            ShowResult(ret);
        }

        private void btnGetMetadataSourceList_Click(object sender, EventArgs e)
        {
            ShowText("***...GetMetadataSourceList...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_MetadataSourceList sourceList = new Np_MetadataSourceList();

            ret = NpClient.Info_GetMetadataSourceList(m_handle, ref sourceList);
            if (Np_Result.Np_Result_OK == ret)
            {
                ShowMetadataSourceList(sourceList);
                NpClient.Info_ReleaseMetadataSourceList(m_handle, ref sourceList);
            }
            ShowResult(ret);
        }

        private void btnBackupPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fdDlg = new FolderBrowserDialog();
            fdDlg.ShowNewFolderButton = true;

            DialogResult result = fdDlg.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                tbBackupPath.Text = fdDlg.SelectedPath;
            }
        }

        private void btnStartBackup_Click(object sender, EventArgs e)
        {
            ShowText("***...Backup Start...***");
            Np_Result ret = NpClient.Backup_SetBackupDestinationDir(m_handle, tbBackupPath.Text);
            if (ret == Np_Result.Np_Result_OK)
            {
                ret = NpClient.Backup_Start(m_handle);
                if (ret == Np_Result.Np_Result_OK)
                {
                    btnStartBackup.Enabled = false;
                    btnPauseBackup.Enabled = true;
                    btnResumeBackup.Enabled = false;
                    btnAbortBackup.Enabled = true;
                }
            }
            ShowResult(ret);  
        }

        private void btnPauseBackup_Click(object sender, EventArgs e)
        {
            ShowText("***...Backup Pause...***");
            btnPauseBackup.Enabled = false;
            btnResumeBackup.Enabled = true;
            Np_Result ret = NpClient.Backup_Pause(m_handle);
            ShowResult(ret);  
        }

        private void btnResumeBackup_Click(object sender, EventArgs e)
        {
            ShowText("***...Backup Resume...***");
            btnPauseBackup.Enabled = true;
            btnResumeBackup.Enabled = false;
            Np_Result ret = NpClient.Backup_Resume(m_handle);
            ShowResult(ret);  
        }

        private void btnAbortBackup_Click(object sender, EventArgs e)
        {
            ShowText("***...Backup Abort...***");
            btnStartBackup.Enabled = true;
            btnPauseBackup.Enabled = false;
            btnResumeBackup.Enabled = false;
            btnAbortBackup.Enabled = false;
            Np_Result ret = NpClient.Backup_Abort(m_handle);
            ShowResult(ret);  
        }

        private void btnGetBackupFileSize_Click(object sender, EventArgs e)
        {
            ShowText("*** GetBackupFileSize ***\r\n");

            Np_DateTime starttime;
            Np_DateTime endtime;
            if (dtspBKFrom.Value > dtspBKTo.Value)
            {
                ShowText("\n***...StartTime shouldn't be larger than EndTime...***\n");
                return;
            }
            ConvertTime(dtspBKFrom.Value, out starttime);
            ConvertTime(dtspBKTo.Value, out endtime);
            ShowTime("start time", starttime);
            ShowTime("end time", endtime);

            if (m_backupDeviceList.Count == 0)
            {
                ShowText("\n***...Please add backup channel...***\n");
                return;
            }

            IntPtr ptrIDList = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_ID)) * m_backupDeviceList.Count);
            long longIDList = ptrIDList.ToInt64();
            for (int i = 0; i < m_backupDeviceList.Count; ++i)
            {
                IntPtr ptr = new IntPtr(longIDList);
                Marshal.StructureToPtr(m_backupDeviceList[i], ptr, false);
                longIDList += Marshal.SizeOf(typeof(Np_ID));
            }
            Np_IDList idList = new Np_IDList();
            idList.size = m_backupDeviceList.Count;
            idList.IDList = ptrIDList;

            Np_SequencedRecordList seqRecordList = new Np_SequencedRecordList();
            seqRecordList.size = 0;
            seqRecordList.items = IntPtr.Zero;

            ulong fileSize = 0;
            Np_Result ret = NpClient.Info_GetBackupFileSize(m_handle,
                                                            starttime,
                                                            endtime,
                                                            idList,
                                                            ref seqRecordList,
                                                            false,
                                                            out fileSize);

            if (Np_Result.Np_Result_OK == ret)
            {
                if (cbIncludeEventLogs.Checked)
                    fileSize += MB_TO_BYTE_UNIT;
                if (cbIncludeCounterLogs.Checked)
                    fileSize += MB_TO_BYTE_UNIT;
                if (cbIncludeSystemLogs.Checked)
                    fileSize += 512 * KB_TO_BYTE_UNIT;
                if (cbIncludeMetadataLogs.Checked)
                    fileSize += MB_TO_BYTE_UNIT;

                ShowText("Total size: "+fileSize+"  Bytes");
            }
            ShowResult(ret);
        }

        private void btnAddBackupDevice_Click(object sender, EventArgs e)
        {
            ShowText("***...Add Backup Device...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_ID id = new Np_ID();
            Int32.TryParse(tbCentralID_BK.Text, out id.centralID);
            Int32.TryParse(tbLocalID_BK.Text, out id.localID);

            m_backupDeviceList.Add(id);
            ShowID("ID", id);
        }

        private void btnDeleteBackupDevice_Click(object sender, EventArgs e)
        {
            ShowText("***...Delete Backup Device...***");
            Np_Result ret = Np_Result.Np_Result_OK;
            Np_ID id = new Np_ID();
            Int32.TryParse(tbCentralID_BK.Text, out id.centralID);
            Int32.TryParse(tbLocalID_BK.Text, out id.localID);

            for (int i = 0; i < m_backupDeviceList.Count; ++i)
            {
                if (m_backupDeviceList[i].centralID == id.centralID && m_backupDeviceList[i].localID == id.localID)
                {
                    m_backupDeviceList.RemoveAt(i);
                    ShowID("ID:", id);
                    return;
                }
            }
            ShowText("Can't find the device");
        }

        private void btnInitializeBackup_Click(object sender, EventArgs e)
        {
            ShowText("*** Initialize Backup ***\r\n");

            Np_DateTime starttime;
            Np_DateTime endtime;
            if (dtspBKFrom.Value > dtspBKTo.Value)
            {
                ShowText("\n***...StartTime shouldn't be larger than EndTime...***\n");
                return;
            }
            ConvertTime(dtspBKFrom.Value, out starttime);
            ConvertTime(dtspBKTo.Value, out endtime);

            if (m_backupDeviceList.Count == 0)
            {
                ShowText("\n***...Please add backup channel...***\n");
            }

            IntPtr ptrIDList = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Np_ID)) * m_backupDeviceList.Count);
            long longIDList = ptrIDList.ToInt64();
            for (int i = 0; i < m_backupDeviceList.Count; ++i)
            {
                IntPtr ptr = new IntPtr(longIDList);
                Marshal.StructureToPtr(m_backupDeviceList[i], ptr, false);
                longIDList += Marshal.SizeOf(typeof(Np_ID));
            }
            Np_IDList idList = new Np_IDList();
            idList.size = m_backupDeviceList.Count;
            idList.IDList = ptrIDList;


            Np_SequencedRecordList seqRecordList = new Np_SequencedRecordList();
            seqRecordList.size = 0;
            seqRecordList.items = IntPtr.Zero;

            Np_Result ret = NpClient.Backup_Initial(m_handle, 
                                                    starttime, 
                                                    endtime,
                                                    idList,
                                                    ref seqRecordList,
                                                    cbIncludeEventLogs.Checked,
                                                    cbIncludeCounterLogs.Checked,
                                                    cbIncludeSystemLogs.Checked,
                                                    cbIncludeMetadataLogs.Checked, 
                                                    false,
                                                    m_bcb, 
                                                    IntPtr.Zero);
            ShowResult(ret);
        }

        private void btnGetBackupFileItemList_Click(object sender, EventArgs e)
        {
            ShowText("***...GetBackupFileItemList...***");    
            Np_Result ret = NpClient.Backup_SetBackupDestinationDir(m_handle, tbBackupPath.Text);
            if (ret == Np_Result.Np_Result_OK)
            {
                Np_BackupItemList list = new Np_BackupItemList();
                ret = NpClient.Backup_GetBackupFileItemList(m_handle, ref list);
                if (ret == Np_Result.Np_Result_OK)
                {
                    IntPtr cursor = list.items;
                    for (int i = 0; i < list.size; ++i)
                    {
                        Np_BackupItem item = (Np_BackupItem)(Marshal.PtrToStructure(cursor, typeof(Np_BackupItem)));
                        ShowText("File " + i + ": " + item.name);
                        cursor = (IntPtr)(cursor.ToInt32() + Marshal.SizeOf(typeof(Np_BackupItem)));
                    }
                    NpClient.Backup_ReleaseBackupFileItemList(m_handle, ref list);
                }
            }
            ShowResult(ret);  
        }

        private void btnUnintializeBackup_Click(object sender, EventArgs e)
        {
            ShowText("***...Unitialize Backup...***");
            Np_Result ret = NpClient.Backup_Uninit(m_handle);
            ShowResult(ret);  
        }
    }
}