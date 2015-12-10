using System;
using System.Runtime.InteropServices;

namespace MMSystem
{
    public enum MMRESULT
    {
        MMSYSERR_BASE = 0,
        MMSYSERR_NOERROR       = 0,                    /* no error */
        MMSYSERR_ERROR         = (MMSYSERR_BASE + 1),  /* unspecified error */
        MMSYSERR_BADDEVICEID   = (MMSYSERR_BASE + 2),  /* device ID out of range */
        MMSYSERR_NOTENABLED    = (MMSYSERR_BASE + 3),  /* driver failed enable */
        MMSYSERR_ALLOCATED     = (MMSYSERR_BASE + 4),  /* device already allocated */
        MMSYSERR_INVALHANDLE   = (MMSYSERR_BASE + 5),  /* device handle is invalid */
        MMSYSERR_NODRIVER      = (MMSYSERR_BASE + 6),  /* no device driver present */
        MMSYSERR_NOMEM         = (MMSYSERR_BASE + 7),  /* memory allocation error */
        MMSYSERR_NOTSUPPORTED  = (MMSYSERR_BASE + 8),  /* function isn't supported */
        MMSYSERR_BADERRNUM     = (MMSYSERR_BASE + 9),  /* error value out of range */
        MMSYSERR_INVALFLAG     = (MMSYSERR_BASE + 10), /* invalid flag passed */
        MMSYSERR_INVALPARAM    = (MMSYSERR_BASE + 11), /* invalid parameter passed */
        MMSYSERR_HANDLEBUSY    = (MMSYSERR_BASE + 12), /* handle being used */
                                                       /* simultaneously on another */
                                                       /* thread (eg callback) */
        MMSYSERR_INVALIDALIAS  = (MMSYSERR_BASE + 13), /* specified alias not found */
        MMSYSERR_BADDB         = (MMSYSERR_BASE + 14), /* bad registry database */
        MMSYSERR_KEYNOTFOUND   = (MMSYSERR_BASE + 15), /* registry key not found */
        MMSYSERR_READERROR     = (MMSYSERR_BASE + 16), /* registry read error */
        MMSYSERR_WRITEERROR    = (MMSYSERR_BASE + 17), /* registry write error */
        MMSYSERR_DELETEERROR   = (MMSYSERR_BASE + 18), /* registry delete error */
        MMSYSERR_VALNOTFOUND   = (MMSYSERR_BASE + 19), /* registry value not found */
        MMSYSERR_NODRIVERCB    = (MMSYSERR_BASE + 20), /* driver does not call DriverCallback */
        MMSYSERR_MOREDATA      = (MMSYSERR_BASE + 21), /* more data to be returned */
        MMSYSERR_LASTERROR     = (MMSYSERR_BASE + 21)  /* last error in range */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct WAVEFORMATEX
    {
        public ushort    wFormatTag;             /* format type */
        public ushort    nChannels;              /* number of channels (i.e. mono, stereo...) */
        public uint      nSamplesPerSec;         /* sample rate */
        public uint      nAvgBytesPerSec;        /* for buffer estimation */
        public ushort    nBlockAlign;            /* block size of data */
        public ushort    wBitsPerSample;         /* number of bits per sample of mono data */
        public ushort    cbSize;                 /* the count in bytes of the size of */
                                                 /* extra information (after cbSize) */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct WAVEHDR {
        public IntPtr   lpData;                 /* pointer to locked data buffer */
        public uint     dwBufferLength;         /* length of data buffer */
        public uint     dwBytesRecorded;        /* used for input only */
        public uint     dwUser;                 /* for client's use */
        public uint     dwFlags;                /* assorted flags (see defines) */
        public uint     dwLoops;                /* loop control counter */
        public IntPtr   lpNext;                 /* reserved for driver */
        public uint     reserved;               /* reserved for driver */
    }

    public class WINMM
    {
        public const int WAVE_MAPPER = - 1;
        public const ushort WAVE_FORMAT_PCM = 1;
        public const uint CALLBACK_FUNCTION = 0x00030000;    /* dwCallback is a FARPROC */
        public const uint MM_WOM_DONE = 0x3BD;

        public delegate void waveOutProc(IntPtr hwo, uint uMsg, int dwInstance, int dwParam1, int dwParam2);

        private const string module_name = "winmm.dll";

        [DllImport(module_name)]
        public static extern MMRESULT waveOutSetVolume(IntPtr hwo, int dwVolume);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutGetVolume(IntPtr hwo, out int dwVolume);

        [DllImport(module_name)]
        public static extern MMRESULT waveOutOpen(out IntPtr phwo, int uDeviceID, ref WAVEFORMATEX pwfx, waveOutProc dwCallback, uint dwInstance, uint fdwOpen);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutClose(IntPtr hwo);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutPrepareHeader(IntPtr hwo, IntPtr pwh/* LPWAVEHDR */, int cbwh);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutUnprepareHeader(IntPtr hwo, IntPtr pwh/* LPWAVEHDR */, int cbwh);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutWrite(IntPtr hwo, IntPtr pwh/* LPWAVEHDR */, int cbwh);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutPause(IntPtr hwo);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutRestart(IntPtr hwo);
        [DllImport(module_name)]
        public static extern MMRESULT waveOutReset(IntPtr hwo);
    }
}