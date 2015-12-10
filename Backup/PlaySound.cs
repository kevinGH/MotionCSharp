using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using MMSystem;

namespace Tester_CSharp
{
    enum PSCommand_Type
    {
        StartPlaying,
        StopPlaying,
        Reset,
        WriteSoundData,
        MM_WOM_DONE
    }

    struct PSCommand
    {
        public PSCommand_Type type;
        public IntPtr arg;
    }

    class PlaySound : IDisposable
    {
        private WAVEFORMATEX m_WaveFormatEx;
        private bool m_bPlay = false;
        private EventWaitHandle m_ewhComplete = null;
        private bool m_bRunning = true;
        private IntPtr m_hPlay = IntPtr.Zero;
        
        private object writeLock = new object();
        private WINMM.waveOutProc m_wcb = null;
        private Thread m_thread = null;
        private ThreadStart m_tcb = null;
        private List<PSCommand> m_cmdQueue = new List<PSCommand>();

        public PlaySound()
        {
            m_WaveFormatEx.wFormatTag = WINMM.WAVE_FORMAT_PCM;
            m_WaveFormatEx.nChannels = 1;
            m_WaveFormatEx.wBitsPerSample = 16;
            m_WaveFormatEx.cbSize = 0;
            m_WaveFormatEx.nSamplesPerSec = 16000;
            m_WaveFormatEx.nBlockAlign = (ushort)((m_WaveFormatEx.wBitsPerSample / 8) * m_WaveFormatEx.nChannels);
            m_WaveFormatEx.nAvgBytesPerSec = m_WaveFormatEx.nBlockAlign * m_WaveFormatEx.nSamplesPerSec;

            m_wcb = new WINMM.waveOutProc(MMCallback);
            m_tcb = new ThreadStart(WorkProc);
            m_thread = new Thread(m_tcb);
            m_thread.Start();
            m_ewhComplete = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public void Dispose()
        {
            m_bRunning = false;
            m_thread.Abort();
        }

        public void WorkProc()
        {
            try
            {
                while (m_bRunning)
                {
                    if (m_cmdQueue.Count == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    PSCommand cmd = m_cmdQueue[0];
                    m_cmdQueue.RemoveAt(0);

                    switch (cmd.type)
                    {
                        case PSCommand_Type.StartPlaying:
                            OnStartPlaying();
                            break;
                        case PSCommand_Type.StopPlaying:
                            OnStopPlaying();
                            break;
                        case PSCommand_Type.Reset:
                            OnReset();
                            break;
                        case PSCommand_Type.WriteSoundData:
                            OnWriteSoundData(cmd.arg);
                            break;
                        case PSCommand_Type.MM_WOM_DONE:
                            OnEndPlaySoundData(cmd.arg);
                            break;
                    }
                }
            }
            finally
            {
                OnStopPlaying();
            }
        }

        public void MMCallback(IntPtr hwo, uint uMsg, int dwInstance, int dwParam1, int dwParam2)
        {
            if (WINMM.MM_WOM_DONE == uMsg)
            {
                PSCommand cmd = new PSCommand();
                cmd.type = PSCommand_Type.MM_WOM_DONE;
                cmd.arg = (IntPtr)dwParam1;
                m_cmdQueue.Add(cmd);
            }
        }

        public void StartPlay()
        {
            PSCommand cmd = new PSCommand();
            cmd.type = PSCommand_Type.StartPlaying;
            cmd.arg = IntPtr.Zero;
            m_cmdQueue.Add(cmd);
        }

        public void StopPlay()
        {
            PSCommand cmd = new PSCommand();
            cmd.type = PSCommand_Type.StopPlaying;
            cmd.arg = IntPtr.Zero;
            m_cmdQueue.Add(cmd);
        }

        public void Reset()
        {
            PSCommand cmd = new PSCommand();
            cmd.type = PSCommand_Type.Reset;
            cmd.arg = IntPtr.Zero;
            m_cmdQueue.Add(cmd);
        }

        void SetWaveFormat(int nChannels, int nBitsPerSample, int nSamplesPerSec)
        {
            m_WaveFormatEx.nChannels = (ushort)nChannels;
            m_WaveFormatEx.wBitsPerSample = (ushort)nBitsPerSample;
            m_WaveFormatEx.nSamplesPerSec = (uint)nSamplesPerSec;
            m_WaveFormatEx.nBlockAlign = (ushort)((m_WaveFormatEx.wBitsPerSample / 8) * m_WaveFormatEx.nChannels);
            m_WaveFormatEx.nAvgBytesPerSec = m_WaveFormatEx.nBlockAlign * m_WaveFormatEx.nSamplesPerSec;

            StopPlay();
            StartPlay();
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public void WriteSoundData(IntPtr lpData, int dwBufferLength, int nChannels, int nBitsPerSample, int nSamplesPerSec)
        {
            if (m_bPlay)
            {
                if (m_WaveFormatEx.nChannels != (short)nChannels ||
                    m_WaveFormatEx.wBitsPerSample != (short)nBitsPerSample ||
                    m_WaveFormatEx.nSamplesPerSec != (int)nSamplesPerSec)
                {
                    SetWaveFormat(nChannels, nBitsPerSample, nSamplesPerSec);
                }

                WAVEHDR hdr = new WAVEHDR();
                hdr.dwBufferLength = (uint)dwBufferLength;
                hdr.lpData = Marshal.AllocHGlobal(dwBufferLength);
                CopyMemory(hdr.lpData, lpData, dwBufferLength);

                PSCommand cmd = new PSCommand();
                cmd.type = PSCommand_Type.WriteSoundData;
                cmd.arg = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WAVEHDR)));
                Marshal.StructureToPtr(hdr, cmd.arg, true);
                m_cmdQueue.Add(cmd);

                EventWaitHandle[] aewh = { m_ewhComplete };
                WaitHandle.WaitAll(aewh);
            }
        }

        void OnStartPlaying()
        {
            if (m_bPlay != true)
            {
	            // open wavein device
                MMRESULT mmReturn = WINMM.waveOutOpen(out m_hPlay, WINMM.WAVE_MAPPER, ref m_WaveFormatEx, m_wcb, 0, WINMM.CALLBACK_FUNCTION);

                m_bPlay = (MMRESULT.MMSYSERR_NOERROR == mmReturn);
            }
        }

        void OnStopPlaying()
        {
             MMRESULT mmReturn = 0;

	        if (m_bPlay)
	        {
                mmReturn = WINMM.waveOutReset(m_hPlay);

                if (MMRESULT.MMSYSERR_NOERROR == mmReturn)
                {
                    mmReturn = WINMM.waveOutClose(m_hPlay);
                    m_hPlay = IntPtr.Zero;
                }

    	        m_bPlay = !(MMRESULT.MMSYSERR_NOERROR == mmReturn);
	        }
        }

        void OnEndPlaySoundData(IntPtr ptr)
        {
            try{
                if (ptr != IntPtr.Zero)
                {
                    WINMM.waveOutUnprepareHeader(m_hPlay, ptr, Marshal.SizeOf(typeof(WAVEHDR)));
                    Marshal.FreeHGlobal(((WAVEHDR)Marshal.PtrToStructure(ptr, typeof(WAVEHDR))).lpData);
                    Marshal.FreeHGlobal(ptr);
                }
	        }
            finally
            {
            }
        }

        void OnWriteSoundData(IntPtr hdr)
        {
            try{
                MMRESULT mmResult = 0;

                if (m_bPlay)
                {
                    mmResult = WINMM.waveOutPrepareHeader(m_hPlay, hdr, Marshal.SizeOf(typeof(WAVEHDR)));
                    mmResult = WINMM.waveOutWrite(m_hPlay, hdr, Marshal.SizeOf(typeof(WAVEHDR)));
                }
            }
            finally
            {
                m_ewhComplete.Set();
            }
        }

        void OnReset()
        {
            if (m_bPlay)
            {
                WINMM.waveOutReset(m_hPlay);
            }
        }
    }
}