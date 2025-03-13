# ApplicationLoopBack
 An C++ lib to capture specifed app's auido by it's PID number. And also sample C# app to showing usage.

 I made a .dll that originally as .exe and I found it in Microsoft's github repository and can be used as an .exe application. This program capture specifed app's auido by it's PID number. So I converted it .dll and event typed; you can use it in your C# programs via P/Invoke as an event-based method. So when you call the `StartCaptureAsync` method from the library from C#, an event is triggered continuously with audio data until you stop it. `StopCaptureAsync`. I needed something like this and it was not in the NAudio library and I developed it. I also contributed to the NAudio library. Maybe the NAudio developer will add it.

 ***Note that this sample requires Windows 10 build 20348 or later. (Windows 11)***

````cs
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

        // 10560 was chrome's PID on my machine.
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
            // PCM data
            byte[] pcmData = pcmStream.ToArray();

            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                // WAV file header
                WriteWavHeader(fs, pcmData.Length, sampleRate, channels, bitDepth);

                // PCM writing
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

````

![Ekran görüntüsü 2025-03-13 161054](https://github.com/user-attachments/assets/5b73fa79-4cc1-486b-a6fd-60714ffac2a5)

![Ekran görüntüsü 2025-03-13 161258](https://github.com/user-attachments/assets/84b21017-52f9-459e-bd94-294ddbf097d8)

![Ekran görüntüsü 2025-03-13 161331](https://github.com/user-attachments/assets/88fde65c-bba6-4f02-bd67-99642da80063)




Original .exe program repo of Microsoft: https://learn.microsoft.com/en-us/samples/microsoft/windows-classic-samples/applicationloopbackaudio-sample/
