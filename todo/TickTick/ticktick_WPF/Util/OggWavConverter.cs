// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.OggWavConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using csogg;
using csvorbis;
using System;
using System.IO;
using System.Text;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public class OggWavConverter
  {
    private static byte[] RIFF_HEADER = new byte[4]
    {
      (byte) 82,
      (byte) 73,
      (byte) 70,
      (byte) 70
    };
    private static byte[] FORMAT_WAVE = new byte[4]
    {
      (byte) 87,
      (byte) 65,
      (byte) 86,
      (byte) 69
    };
    private static byte[] FORMAT_TAG = new byte[4]
    {
      (byte) 102,
      (byte) 109,
      (byte) 116,
      (byte) 32
    };
    private static byte[] AUDIO_FORMAT = new byte[2]
    {
      (byte) 1,
      (byte) 0
    };
    private static byte[] SUBCHUNK_ID = new byte[4]
    {
      (byte) 100,
      (byte) 97,
      (byte) 116,
      (byte) 97
    };
    private const int BYTES_PER_SAMPLE = 2;

    public static void Convert(Stream input, string fileName)
    {
      byte[] buffer = new byte[(int) input.Length];
      TextWriter textWriter = (TextWriter) new DebugWriter();
      Stream stream = (Stream) new FileStream(AppPaths.PomoSoundDir + fileName + ".wav", FileMode.Create);
      stream.Seek(100L, SeekOrigin.Begin);
      SyncState syncState = new SyncState();
      StreamState streamState = new StreamState();
      Page og = new Page();
      Packet op = new Packet();
      Info vi = new Info();
      Comment vc = new Comment();
      DspState vd = new DspState();
      Block vb = new Block(vd);
      int bytes1 = 0;
      syncState.init();
      while (true)
      {
        int num1 = 0;
        int offset1 = syncState.buffer(4096);
        byte[] data1 = syncState.data;
        try
        {
          bytes1 = input.Read(data1, offset1, 4096);
        }
        catch (Exception ex)
        {
          textWriter.WriteLine((object) ex);
        }
        syncState.wrote(bytes1);
        if (syncState.pageout(og) != 1)
        {
          if (bytes1 >= 4096)
            textWriter.WriteLine("Input does not appear to be an Ogg bitstream.");
          else
            break;
        }
        streamState.init(og.serialno());
        vi.init();
        vc.init();
        if (streamState.pagein(og) < 0)
          textWriter.WriteLine("Error reading first page of Ogg bitstream data.");
        if (streamState.packetout(op) != 1)
          textWriter.WriteLine("Error reading initial header packet.");
        if (vi.synthesis_headerin(vc, op) < 0)
          textWriter.WriteLine("This Ogg bitstream does not contain Vorbis audio data.");
        int num2 = 0;
        while (num2 < 2)
        {
label_20:
          while (num2 < 2)
          {
            switch (syncState.pageout(og))
            {
              case 0:
                goto label_21;
              case 1:
                streamState.pagein(og);
                for (; num2 < 2; ++num2)
                {
                  switch (streamState.packetout(op))
                  {
                    case -1:
                      textWriter.WriteLine("Corrupt secondary header.  Exiting.");
                      break;
                    case 0:
                      goto label_20;
                  }
                  vi.synthesis_headerin(vc, op);
                }
                continue;
              default:
                continue;
            }
          }
label_21:
          int offset2 = syncState.buffer(4096);
          byte[] data2 = syncState.data;
          try
          {
            bytes1 = input.Read(data2, offset2, 4096);
          }
          catch (Exception ex)
          {
            textWriter.WriteLine((object) ex);
          }
          if (bytes1 == 0 && num2 < 2)
            textWriter.WriteLine("End of file before finding all Vorbis headers!");
          syncState.wrote(bytes1);
        }
        byte[][] userComments = vc.user_comments;
        for (int i = 0; i < vc.user_comments.Length && userComments[i] != null; ++i)
          textWriter.WriteLine(vc.getComment(i));
        textWriter.WriteLine("\nBitstream is " + vi.channels.ToString() + " channel, " + vi.rate.ToString() + "Hz");
        textWriter.WriteLine("Encoded by: " + vc.getVendor() + "\n");
        int num3 = 4096 / vi.channels;
        vd.synthesis_init(vi);
        vb.init(vd);
        float[][][] _pcm = new float[1][][];
        int[] index1 = new int[vi.channels];
        while (num1 == 0)
        {
          while (num1 == 0)
          {
            switch (syncState.pageout(og))
            {
              case -1:
                textWriter.WriteLine("Corrupt or missing data in bitstream; continuing...");
                continue;
              case 0:
                goto label_56;
              default:
                streamState.pagein(og);
label_35:
                int num4;
                do
                {
                  num4 = streamState.packetout(op);
                  if (num4 == 0)
                    goto label_53;
                }
                while (num4 == -1);
                if (vb.synthesis(op) == 0)
                  vd.synthesis_blockin(vb);
                int num5;
                while ((num5 = vd.synthesis_pcmout(_pcm, index1)) > 0)
                {
                  float[][] numArray = _pcm[0];
                  bool flag = false;
                  int bytes2 = num5 < num3 ? num5 : num3;
                  for (int index2 = 0; index2 < vi.channels; ++index2)
                  {
                    int index3 = index2 * 2;
                    int num6 = index1[index2];
                    for (int index4 = 0; index4 < bytes2; ++index4)
                    {
                      int num7 = (int) ((double) numArray[index2][num6 + index4] * (double) short.MaxValue);
                      if (num7 > (int) short.MaxValue)
                      {
                        num7 = (int) short.MaxValue;
                        flag = true;
                      }
                      if (num7 < (int) short.MinValue)
                      {
                        num7 = (int) short.MinValue;
                        flag = true;
                      }
                      if (num7 < 0)
                        num7 |= 32768;
                      buffer[index3] = (byte) num7;
                      buffer[index3 + 1] = (byte) (num7 >>> 8);
                      index3 += 2 * vi.channels;
                    }
                  }
                  int num8 = flag ? 1 : 0;
                  stream.Write(buffer, 0, 2 * vi.channels * bytes2);
                  vd.synthesis_read(bytes2);
                }
                goto label_35;
label_53:
                if (og.eos() != 0)
                {
                  num1 = 1;
                  continue;
                }
                continue;
            }
          }
label_56:
          if (num1 == 0)
          {
            int offset3 = syncState.buffer(4096);
            byte[] data3 = syncState.data;
            try
            {
              bytes1 = input.Read(data3, offset3, 4096);
            }
            catch (Exception ex)
            {
              textWriter.WriteLine((object) ex);
            }
            syncState.wrote(bytes1);
            if (bytes1 == 0)
              num1 = 1;
          }
        }
        streamState.clear();
        vb.clear();
        vd.clear();
        vi.clear();
      }
      syncState.clear();
      textWriter.WriteLine("Done.");
      stream.Seek(0L, SeekOrigin.Begin);
      OggWavConverter.WriteHeader(stream, (int) (stream.Length - 100L), vi.rate, (ushort) 16, (ushort) vi.channels);
      stream.Seek(0L, SeekOrigin.Begin);
      stream.Dispose();
      input.Dispose();
    }

    private static void WriteHeader(
      Stream stream,
      int length,
      int audioSampleRate,
      ushort audioBitsPerSample,
      ushort audioChannels)
    {
      BinaryWriter binaryWriter = new BinaryWriter(stream);
      binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
      int num = 36 + length;
      binaryWriter.Write(num);
      binaryWriter.Write(new char[8]
      {
        'W',
        'A',
        'V',
        'E',
        'f',
        'm',
        't',
        ' '
      });
      binaryWriter.Write(16);
      binaryWriter.Write((short) 1);
      binaryWriter.Write((short) audioChannels);
      binaryWriter.Write(audioSampleRate);
      binaryWriter.Write(audioSampleRate * ((int) audioBitsPerSample * (int) audioChannels / 8));
      binaryWriter.Write((short) ((int) audioBitsPerSample * (int) audioChannels / 8));
      binaryWriter.Write((short) audioBitsPerSample);
      binaryWriter.Write(new char[4]{ 'd', 'a', 't', 'a' });
      binaryWriter.Write(length);
    }

    public static void WriteHeader(
      Stream targetStream,
      int byteStreamSize,
      int channelCount,
      int sampleRate)
    {
      int source1 = sampleRate * channelCount * 2;
      int source2 = channelCount * 2;
      targetStream.Write(OggWavConverter.RIFF_HEADER, 0, OggWavConverter.RIFF_HEADER.Length);
      targetStream.Write(OggWavConverter.PackageInt(byteStreamSize + 42, 4), 0, 4);
      targetStream.Write(OggWavConverter.FORMAT_WAVE, 0, OggWavConverter.FORMAT_WAVE.Length);
      targetStream.Write(OggWavConverter.FORMAT_TAG, 0, OggWavConverter.FORMAT_TAG.Length);
      targetStream.Write(OggWavConverter.PackageInt(16, 4), 0, 4);
      targetStream.Write(OggWavConverter.AUDIO_FORMAT, 0, OggWavConverter.AUDIO_FORMAT.Length);
      targetStream.Write(OggWavConverter.PackageInt(channelCount), 0, 2);
      targetStream.Write(OggWavConverter.PackageInt(sampleRate, 4), 0, 4);
      targetStream.Write(OggWavConverter.PackageInt(source1, 4), 0, 4);
      targetStream.Write(OggWavConverter.PackageInt(source2), 0, 2);
      targetStream.Write(OggWavConverter.PackageInt(16), 0, 2);
      targetStream.Write(OggWavConverter.SUBCHUNK_ID, 0, OggWavConverter.SUBCHUNK_ID.Length);
      targetStream.Write(OggWavConverter.PackageInt(byteStreamSize, 4), 0, 4);
    }

    private static byte[] PackageInt(int source, int length = 2)
    {
      byte[] numArray = length == 2 || length == 4 ? new byte[length] : throw new ArgumentException("length must be either 2 or 4", nameof (length));
      numArray[0] = (byte) (source & (int) byte.MaxValue);
      numArray[1] = (byte) (source >> 8 & (int) byte.MaxValue);
      if (length == 4)
      {
        numArray[2] = (byte) (source >> 16 & (int) byte.MaxValue);
        numArray[3] = (byte) (source >> 24 & (int) byte.MaxValue);
      }
      return numArray;
    }
  }
}
