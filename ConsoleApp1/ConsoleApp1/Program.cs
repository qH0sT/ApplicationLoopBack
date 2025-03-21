using System.Runtime.InteropServices;
using System.Text;

class Program
{
    delegate void AudioCallback(IntPtr data, int length);

    [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.StdCall)]
    static extern void SetAudioCallback(AudioCallback callback);

    [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.StdCall)]
    static extern IntPtr StartCaptureAsync(uint processId, bool includeProcessTree, ushort channel, 
        uint sampleRate, ushort bitsPerSample);

    [DllImport("ApplicationLoopback.dll", CallingConvention = CallingConvention.StdCall)]
    static extern int StopCaptureAsync();


    static void OnAudioReceived(IntPtr data, int length)
    {
        byte[] buffer = new byte[length];
        Marshal.Copy(data, buffer, 0, length);

        ms.Write(buffer, 0, buffer.Length); // Writing PCM to temp stream to converting it to WAV later.

        Console.WriteLine($"Audio bytes are receiving from specifed process: {length} byte");
    }


    static MemoryStream ms;
    static void Main()
    {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

        ms = new MemoryStream();

        SetAudioCallback(OnAudioReceived); // we are declaring our audio output event in PCM format.

        StartCaptureAsync(10560, true, 1, 44100, 16); // Process PID number and includes process tree or not.

    }

    static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Audio capture from specifed process is started, press CTRL + C to stop.");

        StopCaptureAsync();


        WavConverter.WriteWavFile(ms, "Audio.wav", 44100, 1, 16); // We are converting PCM format to WAV.

        ms.Close();
        ms.Flush();
        ms.Dispose();
    }
    public class WavConverter
    {
        public static void WriteWavFile(MemoryStream pcmStream, string outputPath, int sampleRate, short channels, short bitDepth)
        {
            // PCM verisini al
            byte[] pcmData = pcmStream.ToArray();

            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                // WAV dosyasının başlık kısmı
                WriteWavHeader(fs, pcmData.Length, sampleRate, channels, bitDepth);

                // PCM verisini yaz
                fs.Write(pcmData, 0, pcmData.Length);
            }
        }

        private static void WriteWavHeader(FileStream fs, int pcmDataLength, int sampleRate, short channels, short bitDepth)
        {
            int blockAlign = channels * (bitDepth / 8);
            int byteRate = sampleRate * blockAlign;
            int dataChunkSize = pcmDataLength;
            int chunkSize = 36 + dataChunkSize;

            // "RIFF" header
            fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            fs.Write(BitConverter.GetBytes(chunkSize), 0, 4);
            fs.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);

            // "fmt " subchunk
            fs.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            fs.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size (16 for PCM)
            fs.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat (1 for PCM)
            fs.Write(BitConverter.GetBytes(channels), 0, 2); // NumChannels
            fs.Write(BitConverter.GetBytes(sampleRate), 0, 4); // SampleRate
            fs.Write(BitConverter.GetBytes(byteRate), 0, 4); // ByteRate
            fs.Write(BitConverter.GetBytes(blockAlign), 0, 2); // BlockAlign
            fs.Write(BitConverter.GetBytes(bitDepth), 0, 2); // BitsPerSample

            // "data" subchunk
            fs.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            fs.Write(BitConverter.GetBytes(dataChunkSize), 0, 4); // DataSize
        }
    }
}
