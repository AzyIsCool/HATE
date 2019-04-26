using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace HATE.Core
{
    //TODO: Make sure you don't shuffle the string obj_writer_slash_Draw_0_gml_147_0 ("||")
    public class StringPointer
    {
        public ResourcePointer Base;
        public string Ending;

        public StringPointer(ResourcePointer ptr, string str)
        {
            Base = ptr;
            char[] FormatChars = {'%', '/', 'C'};
            List<char> Ending = new List<char>();

            for (int i = 1; i < str.Length; i++)
            {
                char C = str[str.Length - i];

                if (FormatChars.Contains(C))
                    Ending.Add(C);
                else
                    break;
            }

            Ending.Reverse();
            this.Ending = new string(Ending.ToArray());
        }
    }

    public class ResourcePointer
    {
        public int Address;
        public int Location;

        public ResourcePointer(int ptr, int loc)
        {
            Address = ptr;
            Location = loc;
        }

        public ResourcePointer(byte[] ptr, int loc)
        {
            Address = BitConverter.ToInt32(ptr, 0);
            Location = loc;
        }
    }


    public class Main
    {
        public static string DataFile { get; set; }

        private static readonly string[] FriskSpriteHandles =
        {
            "spr_maincharal", "spr_maincharau", "spr_maincharar", "spr_maincharad", "spr_maincharau_stark",
            "spr_maincharar_stark", "spr_maincharal_stark", "spr_maincharad_pranked", "spr_maincharal_pranked",
            "spr_maincharad_umbrellafall", "spr_maincharau_umbrellafall", "spr_maincharar_umbrellafall",
            "spr_maincharal_umbrellafall", "spr_maincharad_umbrella", "spr_maincharau_umbrella",
            "spr_maincharar_umbrella", "spr_maincharal_umbrella", "spr_charad", "spr_charad_fall", "spr_charar",
            "spr_charar_fall", "spr_charal", "spr_charal_fall", "spr_charau", "spr_charau_fall",
            "spr_maincharar_shadow", "spr_maincharal_shadow", "spr_maincharau_shadow", "spr_maincharad_shadow",
            "spr_maincharal_tomato", "spr_maincharal_burnt", "spr_maincharal_water", "spr_maincharar_water",
            "spr_maincharau_water", "spr_maincharad_water", "spr_mainchara_pourwater", "spr_maincharad_b",
            "spr_maincharau_b", "spr_maincharar_b", "spr_maincharal_b", "spr_doorA", "spr_doorB", "spr_doorC",
            "spr_doorD", "spr_doorX"
        };

        public static bool FriskMode;

        public static bool ShuffleAudio_Func(Random random, float chance, StreamWriter logstream)
        {
            return Shuffle.LoadDataAndFind("SOND", random, chance, logstream, GetFileLocation(DataFile),
                       Shuffle.SimpleShuffle) &&
                   Shuffle.LoadDataAndFind("AUDO", random, chance, logstream, GetFileLocation(DataFile),
                       Shuffle.SimpleShuffle);
        }

        public static bool ShuffleBG_Func(Random random, float chance, StreamWriter logstream)
        {
            return Shuffle.LoadDataAndFind("BGND", random, chance, logstream, GetFileLocation(DataFile),
                Shuffle.SimpleShuffle);
        }

        public static bool ShuffleFont_Func(Random random, float chance, StreamWriter logstream)
        {
            return Shuffle.LoadDataAndFind("FONT", random, chance, logstream, GetFileLocation(DataFile),
                Shuffle.SimpleShuffle);
        }

        public static bool HitboxFix_Func(Random random_, float chance, StreamWriter logstream_)
        {
            return Shuffle.LoadDataAndFind("SPRT", random_, chance, logstream_, GetFileLocation(DataFile),
                Shuffle.ComplexShuffle(Shuffle.SimpleAccumulator, HitboxFix_Shuffler, Shuffle.SimpleWriter));
        }

        public static bool ShuffleGFX_Func(Random random_, float chance, StreamWriter logstream_)
        {
            return Shuffle.LoadDataAndFind("SPRT", random_, chance, logstream_, GetFileLocation(DataFile),
                Shuffle.ComplexShuffle(ShuffleGFX_Accumulator, Shuffle.SimpleShuffler, Shuffle.SimpleWriter));
        }

        public static bool ShuffleText_Func(Random random_, float chance, StreamWriter logstream_, string game_name)
        {
            if (game_name == "Deltarune")
            {
                return Shuffle.JSONStringShuffle(GetFileLocation(Path.Combine("lang", "lang_en.json")),
                           GetFileLocation(Path.Combine("lang", "lang_en.json")), random_, chance, logstream_) &&
                       Shuffle.JSONStringShuffle(GetFileLocation(Path.Combine("lang", "lang_ja.json")),
                           GetFileLocation(Path.Combine("lang", "lang_ja.json")), random_, chance, logstream_);
            }
            else
            {
                return Shuffle.LoadDataAndFind("STRG", random_, chance, logstream_, GetFileLocation(DataFile),
                    Shuffle.ComplexShuffle(Shuffle.SimpleAccumulator, ShuffleText_Shuffler, Shuffle.SimpleWriter));
            }
        }

        // TODO: clean this
        public static List<ResourcePointer> ShuffleGFX_Accumulator(FileStream stream, Random random,
            float shufflechance, StreamWriter logstream)
        {
            byte[] readBuffer = new byte[Shuffle.WordSize];
            int pointerNum = 0;
            long pointerArrayBegin = 0;
            List<ResourcePointer> pointerList = new List<ResourcePointer>();

            stream.Read(readBuffer, 0, 4);
            pointerNum = BitConverter.ToInt32(readBuffer, 0);
            pointerArrayBegin = stream.Position;

            for (int i = 0; i < pointerNum; i++)
            {
                if (random.NextDouble() < shufflechance)
                {
                    byte[] _tmp = new byte[4], _tmp2 = new byte[4];
                    stream.Read(_tmp, 0, 4);
                    ResourcePointer ptr = new ResourcePointer(_tmp, (int) stream.Position - 4);
                    long pos = stream.Position;
                    stream.Position = ptr.Address;
                    stream.Read(_tmp2, 0, 4);
                    stream.Position = BitConverter.ToInt32(_tmp2, 0);

                    List<byte> byteString = new List<byte>();
                    bool stringBegun = false;

                    for (int j = 0; j < 128; j++)
                    {
                        byteString.Add((byte) stream.ReadByte());

                        if (byteString[byteString.Count - 1] == 0 && stringBegun)
                            break;

                        if (byteString[byteString.Count - 1] != 0)
                            stringBegun = true;
                    }

                    string convertedString = new string(byteString
                        .Where(x => x == '_' || (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') ||
                                    (x >= '0' && x <= '9')).Select(x => (char) x).ToArray());

                    if (!FriskSpriteHandles.Contains(convertedString.Trim()) || FriskMode)
                        pointerList.Add(ptr);

                    stream.Position = pos;
                }
            }

            logstream.WriteLine($"Added {pointerList.Count} out of {pointerNum} sprite pointers to SPRT List.");

            return pointerList;
        }

        // TODO: implement this
        public static List<ResourcePointer> HitboxFix_Shuffler(FileStream stream, Random random, float shufflechance,
            StreamWriter logstream, List<ResourcePointer> pointerlist)
        {
            foreach (ResourcePointer spriteptr in pointerlist)
            {
                // Previous implementation of HitboxFix didn't work at all and was unsafe to boot, better implementation will be added later.
            }

            logstream.WriteLine($"Wrote {pointerlist.Count} hitboxes to {DataFile}.");

            return pointerlist;
        }

        // TODO: delete this
        // TODO: clean this
        public static List<ResourcePointer> ShuffleText_Shuffler(FileStream stream, Random random, float shufflechance,
            StreamWriter logstream, List<ResourcePointer> pointerlist)
        {
            string[] bannedStrings = {"_"};
            List<ResourcePointer> shuffledPointerList = new List<ResourcePointer>();
            List<StringPointer> strPointerList = new List<StringPointer>();


            for (int i = 0; i < pointerlist.Count; i++)
            {
                stream.Position = pointerlist[i].Address;
                byte StrlenByte = (byte) stream.ReadByte();
                stream.Position += 3;
                List<byte> ByteString = new List<byte>();

                for (int j = 0; j < StrlenByte; j++)
                {
                    byte TMP = (byte) stream.ReadByte();

                    if (TMP == 0)
                        break;
                    else
                        ByteString.Add(TMP);
                }

                string convertedString = new string(ByteString.Select(x => (char) x).ToArray());

                if (StrlenByte >= 3 && !bannedStrings.Any(convertedString.Contains))
                    strPointerList.Add(new StringPointer(pointerlist[i], convertedString));
            }

            logstream.WriteLine($"Added {strPointerList.Count} good string pointers to SprPointerList.");

            Dictionary<string, List<ResourcePointer>> stringDict = new Dictionary<string, List<ResourcePointer>>();
            int totalStrings = 0;

            foreach (StringPointer s in strPointerList)
            {
                if (!string.IsNullOrWhiteSpace(s.Ending))
                {
                    if (!stringDict.ContainsKey(s.Ending))
                        stringDict[s.Ending] = new List<ResourcePointer>();

                    stringDict[s.Ending].Add(s.Base);
                    totalStrings++;
                }
            }

            foreach (string ending in stringDict.Keys)
            {
                logstream.WriteLine(
                    $"Added {stringDict[ending].Count} string pointers of ending {ending} to dialogue string List.");

                stringDict[ending].Shuffle(Shuffle.PointerSwapLoc, random);

                shuffledPointerList = shuffledPointerList.Concat(stringDict[ending]).ToList();
            }

            return shuffledPointerList;
        }

        public static string GetGame()
        {
            if (File.Exists("DELTARUNE.exe"))
            {
                return $"DELTARUNE.exe";
            }
            else if (Directory.Exists("SURVEY_PROGRAM.app"))
            {
                return "SURVEY_PROGRAM.app";
            }
            else if (File.Exists("UNDERTALE.exe"))
            {
                return $"UNDERTALE.exe";
            }
            else if (Directory.Exists("UNDERTALE.app"))
            {
                return "UNDERTALE.app";
            }
            else if (File.Exists("runner"))
            {
                return "./runner";
            }
            else
            {
                var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory());
                foreach (string s in files)
                {
                    if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Windows &&
                        !s.Remove(0, s.LastIndexOf("\\") + 1).Contains("HATE.exe") && s.Contains(".exe") ||
                        s.Contains(".app"))
                    {
                        int count = s.Length - 1;
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i] == '\\')
                            {
                                count = i;
                            }
                        }

                        return s.Remove(0, count + 1);
                    }
                    else if (s != "HATE.exe" && s.Contains(".exe") || s.Contains(".app"))
                        return s.Remove(0, s.LastIndexOf("/") + 1);
                }
            }

            return "";
        }

        //Need this because macOS does stuff differently so it's best if we work with the whole File Location String
        public static string GetFileLocation(string file)
        {
            switch (OS.WhatOperatingSystemUserIsOn)
            {
                case OS.OperatingSystem.Linux
                    when Safe.IsValidFile(Path.Combine(Directory.GetCurrentDirectory(), "assets", "options.ini")):
                    return Path.Combine(Directory.GetCurrentDirectory(), "assets",
                        file); //This means that it's running a native version of the game
                case OS.OperatingSystem.macOS:
                    return Path.Combine("/Applications", GetGame(), "Contents", "Resources", file);
                case OS.OperatingSystem.Windows:
                case OS.OperatingSystem.Unknown:
                    return Path.Combine(Directory.GetCurrentDirectory(), file);
            }

            return file; //In case it none of them somehow
        }

        public static void DebugListChunks(string resource_file, StreamWriter logstream)
        {
            byte[] readBuffer = new byte[4];

            using (FileStream stream = new FileStream(resource_file, FileMode.OpenOrCreate))
            {
                logstream.WriteLine($"Opened {resource_file}.");
                stream.Position = 8;

                int dataSegmentCounter = 0;

                while (stream.Position != stream.Length)
                {
                    stream.Read(readBuffer, 0, 4);
                    string headerName = new string(readBuffer.Select(x => (char) x).ToArray());
                    stream.Read(readBuffer, 0, 4);
                    int chunk_size = BitConverter.ToInt32(readBuffer, 0);
                    logstream.WriteLine($"Chunk #{dataSegmentCounter}: {headerName} found, size {chunk_size}.");
                    stream.Position += chunk_size;
                    dataSegmentCounter++;
                }

                logstream.WriteLine($"Closed {resource_file}.");
            }
        }
    }
}