using System;
using System.Runtime.InteropServices;

namespace Tester_CSharp
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct SoundRecorderAudioFormat
    {
        public int channels;
        public int bitsPerSample;
        public int sampleRate;
        public int sampleRequest;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void fnAudioRecordHandle(IntPtr buffer,
                                            int len,
                                            IntPtr ctx);

    public class SoundRecorder
    {
        private const string module_name = "SoundRecorder.dll";

        public SoundRecorder()
        {
        }

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern bool Create_SoundRecorder(
            ref IntPtr recorder
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern bool Destroy_SoundRecorder(
            IntPtr recorder
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern bool Start_SoundRecord(
            IntPtr recorder,
            SoundRecorderAudioFormat fmt,
            fnAudioRecordHandle rcb, IntPtr rctx
         );

        [DllImport(module_name, CharSet = CharSet.Unicode)]
        public static extern bool Stop_SoundRecord(
            IntPtr recorder
         );
    }
}
