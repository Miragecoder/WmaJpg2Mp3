﻿using NAudio.Wave;
using NAudio.Lame;
using System.IO;

namespace WmaJpg2Mp3.Operations
{
    class Converter
    {
        static void ConvertToMP3(string sourceFilename, string targetFilename)
        {
            using (var reader = new NAudio.Wave.AudioFileReader(sourceFilename))
            using (var writer = new NAudio.Lame.LameMP3FileWriter(targetFilename, reader.WaveFormat, NAudio.Lame.LAMEPreset.STANDARD))
            {
                reader.CopyTo(writer);
            }
        }
    }
}
