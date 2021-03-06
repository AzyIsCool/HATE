﻿using System;
using Optional;
using System.IO;
using System.Linq;
using System.Security;
using HATE.Core.Logging;
using System.Collections.Generic;

namespace HATE.Core
{
    public static class Safe
    {
        private static bool IsValidPath(string path)
        {
            try
            {
                Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool HasWriteAccess(string directory)
        {
            try
            {
                File.WriteAllText(Path.Combine(directory, "Wew"),
                    "Wew this is just HATE checking if we can write and delete to the drive (if you see this delete the file if you want to, it will no ill effect on HATE :p)");
                File.Delete(Path.Combine(directory, "Wew"));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsValidFile(string path)
        {
            return File.Exists(path);
        }

        public static Option<List<string>> GetDirectories(string dirname, string searchstring)
        {
            if (!IsValidPath(dirname) || string.IsNullOrWhiteSpace(searchstring) || !Directory.Exists(dirname))
            {
                return Option.None<List<string>>();
            }

            List<string> output = new List<string>();
            try
            {
                output = Directory.GetDirectories(dirname, searchstring, SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to get the list of directories in {dirname}. The directory requires permissions which this application does not have to access. Please remove permission requirement from the directory and try again.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to get the list of directories in {dirname}. Please ensure that the directory is not in use and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error,
                            $"{ex.ToString()} has occured while attempting to get the list of directories in {dirname}.");
                        break;
                }

                return Option.None<List<string>>();
            }

            return Option.Some(output);
        }

        public static bool CreateDirectory(string dirname)
        {
            if (!IsValidPath(dirname))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(dirname);
            }
            catch (Exception ex)
            {
                Logger.Log(MessageType.Error,
                    ex is UnauthorizedAccessException
                        ? $"UnauthorizedAccessException has occured while attempting to create {dirname}. Creation of the specified directory requires permissions which this application does not have. Please remove permission requirement from the parent directory and try again."
                        : $"{ex} has occured while attempting to create {dirname}.");
                return false;
            }

            return true;
        }

        public static bool CopyFile(string from, string to)
        {
            if (!IsValidPath(from) || !IsValidPath(to) || !File.Exists(from))
            {
                return false;
            }

            try
            {
                File.Copy(from, to, true);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to copy {@from} to {to}. Please ensure that the source file doesn't require permissions to access and that destination file is not read-only.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to copy {@from} to {to}. Please ensure that the files are not in use and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error,
                            $"Exception {ex} has occured while attempting to copy {@from} to {to}.");
                        break;
                }

                return false;
            }

            return true;
        }

        public static Option<List<string>> GetFiles(string dirname, bool alldirs = true, string format = "*.*")
        {
            if (!IsValidPath(dirname) || !Directory.Exists(dirname))
            {
                return Option.None<List<string>>();
            }

            List<string> output = new List<string>();
            try
            {
                output = Directory.GetFiles(dirname, format,
                    alldirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to get the list of files in {dirname}. The directory requires permissions which this application does not have to access. Please remove permission requirement from the directory and try again.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to get the list of files in {dirname}. Please ensure that the directory is not in use and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error,
                            $"{ex} has occured while attempting to get the list of files in {dirname}.");
                        break;
                }

                return Option.None<List<string>>();
            }

            return Option.Some(output);
        }

        public static bool DeleteDirectory(string dirname)
        {
            if (!IsValidPath(dirname) || !Directory.Exists(dirname))
            {
                return false;
            }

            try
            {
                Directory.Delete(dirname, true);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to delete {dirname}. The directory requires permissions which this application does not have to access. Please remove permission requirement from the directory and try again.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to delete {dirname}. Please ensure that the directory is not in use and doesn't contain a read-only file and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error, $"{ex} has occured while attempting to delete {dirname}.");
                        break;
                }

                return false;
            }

            return true;
        }

        public static bool DeleteFile(string filename)
        {
            if (!IsValidPath(filename) || !File.Exists(filename))
            {
                return false;
            }

            try
            {
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to delete {filename}. Please ensure that the file is neither read-only, requiring permissions to access, actually a directory all along, or a executable currently in use.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to delete {filename}. Please ensure that the file is not in use and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error, $"{ex} has occured while attempting to delete {filename}.");
                        break;
                }

                return false;
            }

            return true;
        }

        public static bool MoveFile(string from, string to)
        {
            if (!IsValidPath(from) || !IsValidPath(to) || !File.Exists(from))
            {
                return false;
            }

            try
            {
                File.Move(from, to);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while attempting to move {@from} to {to}. Please ensure that the source file doesn't require permissions to access.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while attempting to move {@from} to {to}. Please ensure that the destination file doesn't exist and that the source file does.");
                        break;
                    default:
                        Logger.Log(MessageType.Error, $"{ex} has occured while attempting to move {@from} to {to}.");
                        break;
                }

                return false;
            }

            return true;
        }

        public static Option<StreamWriter> OpenStreamWriter(string filename)
        {
            if (!IsValidPath(filename) || !File.Exists(filename))
            {
                return Option.None<StreamWriter>();
            }

            StreamWriter TXW;
            try
            {
                TXW = new StreamWriter(filename);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case SecurityException _:
                        Logger.Log(MessageType.Error,
                            $"SecurityException has occured while opening {filename} with a StreamWriter. File requires permissions to access which this program does not have.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while opening {filename} with a StreamWriter. Please ensure that the path is correct and the file is not in use and try again.");
                        break;
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while opening {filename} with a StreamWriter. The file requires permissions which this program doesn't have to open. Please ensure that file at the specified address requires no permissions to open and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error,
                            $"{ex} has occured when while opening {filename} with a StreamWriter.");
                        break;
                }

                return Option.None<StreamWriter>();
            }

            return Option.Some(TXW);
        }

        public static Option<FileStream> OpenFileStream(string filename)
        {
            if (!IsValidPath(filename) || !File.Exists(filename))
            {
                return Option.None<FileStream>();
            }

            FileStream TXW;
            try
            {
                TXW = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case SecurityException _:
                        Logger.Log(MessageType.Error,
                            $"SecurityException has occured while opening {filename} with a FileStream. File requires permissions to access which this program does not have.");
                        break;
                    case IOException _:
                        Logger.Log(MessageType.Error,
                            $"IOException has occured while opening {filename} with a FileStream. Please ensure that the path is correct and the file isn't in use and try again.");
                        break;
                    case UnauthorizedAccessException _:
                        Logger.Log(MessageType.Error,
                            $"UnauthorizedAccessException has occured while opening {filename} with a FileStream. The file requires permissions which this program doesn't have to open. Please ensure that file at the specified address requires no permissions to open and try again.");
                        break;
                    default:
                        Logger.Log(MessageType.Error,
                            $"{ex} has occured when while opening {filename} with a FileStream.");
                        break;
                }

                return Option.None<FileStream>();
            }

            return Option.Some(TXW);
        }
    }
}