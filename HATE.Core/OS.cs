using System;
using System.IO;

namespace HATE.Core
{
    public class OS
    {
        //Code from https://stackoverflow.com/questions/38790802/determine-operating-system-in-net-core
        //Was needed because what we get isn't the best when using .NET Standard (only shows if we are on windows or Unix (which could be macOS or Linix))
        public static OperatingSystem WhatOperatingSystemUserIsOn
        {
            get
            {
                string windir = Environment.GetEnvironmentVariable("windir");
                if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                {
                    return OperatingSystem.Windows;
                }
                else if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                    {
                        // Note: Android gets here too
                        return OperatingSystem.Linux;
                    }
                    else
                    {
                        return OperatingSystem.Unknown;
                    }
                }
                else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                {
                    // Note: iOS gets here too
                    return OperatingSystem.macOS;
                }
                else
                {
                    return OperatingSystem.Unknown;
                }
            }
        }

        public enum OperatingSystem
        {
            Windows,
            Linux,
            macOS,
            Unknown
        }
    }
}
