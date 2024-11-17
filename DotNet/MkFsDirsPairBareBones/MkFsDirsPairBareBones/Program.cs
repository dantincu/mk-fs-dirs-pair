﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

using MkFsDirsPairBareBones;

UtilsH.ExecuteProgram(
    () => new ProgramComponent().Run(args));

namespace MkFsDirsPairBareBones
{
    public class ProgramComponent
    {
        public const string KEEP_FILE_NAME = ".keep";
        public const string MD_FILE_NAME_PFX_STR = "0-";
        public const string MD_FILE_NAME_SFFX_STR = "[note].md";
        public const string FILES_DIR_NAME = "[note-files]";
        public const string FILES_DIR_TITLE = "Note Files";
        public const int MAX_DIR_NAME_LENGTH = 100;

        public void Run(string[] args)
        {
            var pga = new ProgramArgsParser(
                ).Parse(args);

            if (pga.IsRename)
            {
                RunRename(pga);
            }
            else
            {
                RunCreate(pga);
            }
        }

        private void RunRename(
            ProgramArgs pga)
        {
            if (pga.RecursiveDirNameRegex != null)
            {
                var dirNamesArr = Directory.GetDirectories(
                    pga.WorkDir).Select(
                        dir => Path.GetFileName(dir)).Where(
                    dir => pga.RecursiveDirNameRegex.IsMatch(dir)).ToArray();

                foreach (var dirName in dirNamesArr)
                {
                    var childPga = new ProgramArgs(pga)
                    {
                        WorkDir = Path.Combine(
                            pga.WorkDir,
                            dirName),
                        Title = null,
                        RenameFromKeepFile = false,
                    };

                    RunRename(childPga);
                    RunRenameCore(childPga);
                }
            }
            else
            {
                RunRenameCore(pga);
            }
        }

        private void RunRenameCore(
            ProgramArgs pga)
        {
            var pair = GetDirsPairEntries(pga);
            pga.Title ??= ExtractTitle(pga, pair);

            NormalizeEntryNames(pga, pair);
            RunRenameCore(pga, pair);
        }

        private void RunRenameCore(
            ProgramArgs pga,
            DirsPairEntries pair)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            if (pair.FullDirName != pair.NewFullDirName)
            {
                Directory.Move(
                    pair.FullDirPath,
                    pair.NewFullDirPath);

                Console.WriteLine("Renamed the full name dir");
            }

            if (pair.MdFileName != pair.NewMdFileName)
            {
                File.Move(
                    pair.MdFilePath,
                    pair.NewMdFilePath);

                Console.WriteLine("Renamed the md file");
            }

            if (pga.ShouldUpdateMdFileContents)
            {
                File.WriteAllLines(
                    pair.NewMdFilePath,
                    pair.MdFileLines);

                Console.WriteLine("Updated the md file contents");
            }

            if (pga.ShouldUpdateKeepFileContents)
            {
                File.WriteAllText(
                    pair.NewKeepFilePath,
                    pga.Title);

                Console.WriteLine("Updated the keep file contents");
            }

            Console.ResetColor();
        }

        private void RunCreate(
            ProgramArgs pga)
        {
            if (pga.SuppressCreationOfNoteDirsPair)
            {
                if (pga.CreateFileDirsPair)
                {
                    CreateFileDirsPair(
                        pga, pga.WorkDir);
                }
                else
                {
                    throw new Exception(
                        $"Suppressing the creation of note dirs pair requires the create files dir pair flag to be provided");
                }
            }
            else
            {
                string fullDirNamePart = NormalizeFullDirNamePart(
                    pga.Title);

                string shortDirPath = RunCreateCore(
                    new()
                    {
                        Pga = pga,
                        PrDirPath = pga.WorkDir,
                        DirsPairTitle = pga.Title,
                        ShortDirName = pga.ShortDirName,
                        FullDirNamePart = fullDirNamePart
                    });

                string mdFileName = string.Concat(
                    MD_FILE_NAME_PFX_STR,
                    fullDirNamePart,
                    MD_FILE_NAME_SFFX_STR);

                string mdFilePath = Path.Combine(
                    shortDirPath,
                    mdFileName);

                string mdTitle = UtilsH.EncodeForMd(
                    pga.Title);

                var mdFileLinesArr = new string[]
                {
                    $"# {mdTitle}",
                    ""
                };

                File.WriteAllLines(
                    mdFilePath,
                    mdFileLinesArr);

                if (pga.CreateFileDirsPair)
                {
                    CreateFileDirsPair(
                        pga, shortDirPath);
                }
            }
        }

        private string RunCreateCore(
            DirsPairArgs args)
        {
            string shortDirNamePath = Path.Combine(
                args.PrDirPath,
                args.ShortDirName);

            string fullDirName = string.Join(
                args.Pga.DirsPairJoinStr,
                args.ShortDirName,
                args.FullDirNamePart);

            string fullDirNamePath = Path.Combine(
                args.PrDirPath,
                fullDirName);

            string keepFilePath = Path.Combine(
                fullDirNamePath,
                KEEP_FILE_NAME);

            CreateNewDirectory(shortDirNamePath);
            CreateNewDirectory(fullDirNamePath);

            File.WriteAllText(
                keepFilePath,
                args.DirsPairTitle);

            return shortDirNamePath;
        }

        private string CreateFileDirsPair(
            ProgramArgs pga,
            string prDirPath) => RunCreateCore(
                new()
                {
                    Pga = pga,
                    PrDirPath = prDirPath,
                    DirsPairTitle = FILES_DIR_TITLE,
                    ShortDirName = pga.FilePairShortDirName,
                    FullDirNamePart = FILES_DIR_NAME,
                });

        private void CreateNewDirectory(
            string newDirPath)
        {
            if (Directory.Exists(
                newDirPath) || File.Exists(
                    newDirPath))
            {
                throw new InvalidOperationException(
                    $"The following path points to an already existing file system entry: {newDirPath}");
            }

            Directory.CreateDirectory(
                newDirPath);
        }

        private string NormalizeFullDirNamePart(
            string fullDirNamePart)
        {
            fullDirNamePart = fullDirNamePart.Replace('/', '%').Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries).JoinStr(" ").Trim();

            if (fullDirNamePart.Length > MAX_DIR_NAME_LENGTH)
            {
                fullDirNamePart = fullDirNamePart.Substring(
                    0, MAX_DIR_NAME_LENGTH);

                fullDirNamePart = fullDirNamePart.TrimEnd();
            }

            if (fullDirNamePart.Last() == '.')
            {
                fullDirNamePart += "%";
            }

            return fullDirNamePart;
        }

        private DirsPairEntries GetDirsPairEntries(
            ProgramArgs pga)
        {
            string prDirPath = Path.GetDirectoryName(
                pga.WorkDir)!;

            var shortDirName = Path.GetFileName(
                pga.WorkDir);

            string fullNameDirBasePart = string.Concat(
                shortDirName,
                pga.DirsPairJoinStr);

            string fullDirName = Directory.GetDirectories(
                prDirPath).Select(
                dir => Path.GetFileName(dir)).Single(
                dir => dir.StartsWith(
                    fullNameDirBasePart));

            string mdFileName = Directory.GetFiles(
                pga.WorkDir).Select(
                file => Path.GetFileName(file)).Single(
                    file => file.StartsWith(
                        MD_FILE_NAME_PFX_STR) && file.EndsWith(
                            MD_FILE_NAME_SFFX_STR));

            var pair = new DirsPairEntries
            {
                PrDirPath = prDirPath,
                ShortDirName = shortDirName,
                FullDirName = fullDirName,
                MdFileName = mdFileName,
                FullDirPath = Path.Combine(
                    prDirPath,
                    fullDirName),
                MdFilePath = Path.Combine(
                    pga.WorkDir,
                    mdFileName)
            };

            pair.MdFileLines = File.ReadAllLines(
                pair.MdFilePath);

            var firstLineKvp = pair.MdFileLines.FirstKvp(
                (line, idx) => !string.IsNullOrWhiteSpace(line));

            if (firstLineKvp.Key >= 0)
            {
                pair.FirstMdLine = firstLineKvp.Value;
                pair.FirstMdLineIdx = firstLineKvp.Key;
            }

            return pair;
        }

        private string ExtractTitle(
            ProgramArgs pga,
            DirsPairEntries pair)
        {
            string title;

            if (pga.RenameFromKeepFile || (
                pair.FirstMdLine?.StartsWith(
                    "# ") ?? false) == false)
            {
                pair.KeepFilePath ??= Path.Combine(
                    pair.FullDirPath,
                    KEEP_FILE_NAME);

                title = File.ReadAllText(
                    pair.KeepFilePath).Trim();
            }
            else
            {
                pair.MdTitle = pair.FirstMdLine.Substring(2).Trim();
                title = UtilsH.DecodeForMd(pair.MdTitle);
            }

            return title;
        }

        private void NormalizeEntryNames(
            ProgramArgs pga,
            DirsPairEntries pair)
        {
            pair.MdTitle ??= UtilsH.EncodeForMd(pga.Title);
            pair.FirstMdLine = $"# {pair.MdTitle}";

            if (pair.FirstMdLineIdx >= 0)
            {
                pair.MdFileLines[pair.FirstMdLineIdx] = pair.FirstMdLine;
            }

            pair.NewFullDirNamePart = NormalizeFullDirNamePart(
                pga.Title);

            pair.NewFullDirName = string.Join(
                pga.DirsPairJoinStr,
                pair.ShortDirName,
                pair.NewFullDirNamePart);

            pair.NewFullDirPath = Path.Combine(
                pair.PrDirPath,
                pair.NewFullDirName);

            pair.NewKeepFilePath = Path.Combine(
                pair.NewFullDirPath,
                KEEP_FILE_NAME);

            pair.NewMdFileName = string.Concat(
                MD_FILE_NAME_PFX_STR,
                pair.NewFullDirNamePart,
                MD_FILE_NAME_SFFX_STR);

            pair.NewMdFilePath = Path.Combine(
                pga.WorkDir,
                pair.NewMdFileName);
        }

        private class DirsPairEntries
        {
            public string PrDirPath { get; set; }
            public string ShortDirName { get; set; }
            public string FullDirName { get; set; }
            public string FullDirPath { get; set; }
            public string MdFileName { get; set; }
            public string MdFilePath { get; set; }
            public string KeepFilePath { get; set; }
            public string[] MdFileLines { get; set; }
            public string FirstMdLine { get; set; }
            public int FirstMdLineIdx { get; set; }
            public string MdTitle { get; set; }
            public string NewFullDirName { get; set; }
            public string NewFullDirNamePart { get; set; }
            public string NewFullDirPath { get; set; }
            public string NewKeepFilePath { get; set; }
            public string NewMdFileName { get; set; }
            public string NewMdFilePath { get; set; }
        }

        private class DirsPairArgs
        {
            public ProgramArgs Pga { get; set; }
            public string DirsPairTitle { get; set; }
            public string PrDirPath { get; set; }
            public string ShortDirName { get; set; }
            public string FullDirNamePart { get; set; }
        }
    }

    internal class ProgramArgsParser
    {
        public const string DIRS_PAIR_JOIN_STR = "-";
        public const string FLAG_START_DELIM_STR = ":";

        public static readonly string FlagStartDblDelimStr = string.Concat(
            FLAG_START_DELIM_STR,
            FLAG_START_DELIM_STR);

        public static readonly Dictionary<ProgramArgs.Flag, string> PgaFlagsMap = new()
        {
            { ProgramArgs.Flag.WorkDir, "wk" },
            { ProgramArgs.Flag.RenameFromMdFile, "rn" },
            { ProgramArgs.Flag.RenameFromKeepFile, "rk" },
            { ProgramArgs.Flag.CreateFilesDirPair, "nf" },
            { ProgramArgs.Flag.Recursive, "rc" }
        };

        public ProgramArgs Parse(string[] args)
        {
            var pga = new ProgramArgs
            {
                Items = args.Select(ParseItem).ToList()
            };

            int flagsCount = 0, argsCount = 0;

            foreach (var item in pga.Items)
            {
                if (item.IsFlag)
                {
                    switch (item.Flag!.Value)
                    {
                        case ProgramArgs.Flag.WorkDir:
                            pga.WorkDir = item.Value!;
                            break;
                        case ProgramArgs.Flag.RenameFromMdFile:
                            pga.IsRename = true;
                            break;
                        case ProgramArgs.Flag.RenameFromKeepFile:
                            pga.IsRename = true;
                            pga.RenameFromKeepFile = true;
                            break;
                        case ProgramArgs.Flag.CreateFilesDirPair:
                            pga.CreateFileDirsPair = true;
                            pga.FilePairShortDirName = item.Value ?? throw new InvalidOperationException(
                                $"The {nameof(ProgramArgs.Flag.CreateFilesDirPair)} flag must be provided along with the short dir name for the files pair");
                            break;
                        case ProgramArgs.Flag.Recursive:
                            pga.RecursiveDirNameRegexStr = item.Value ?? throw new InvalidOperationException(
                                $"The {nameof(ProgramArgs.Flag.Recursive)} flag must be provided along with a regular expression string");

                            pga.RecursiveDirNameRegex = new Regex(
                                pga.RecursiveDirNameRegexStr);

                            break;
                        default:
                            throw new NotSupportedException(
                                $"Unsupported flag: {item.Flag.Value}");
                    }

                    flagsCount++;
                }
                else
                {
                    switch (argsCount)
                    {
                        case 0:
                            pga.Title = item.Value!;
                            break;
                        case 1:
                            pga.ShortDirName = item.Value!;
                            break;
                        case 2:
                            pga.DirsPairJoinStr = item.Value!;
                            break;
                        default:
                            throw new NotSupportedException(
                                $"No more than 3 non-flag arguments should be supplied to this tool");
                    }

                    argsCount++;
                }
            }

            pga.DirsPairJoinStr ??= DIRS_PAIR_JOIN_STR;

            if (string.IsNullOrWhiteSpace(pga.WorkDir))
            {
                pga.WorkDir = Directory.GetCurrentDirectory();
            }
            else
            {
                if (!Path.IsPathRooted(pga.WorkDir))
                {
                    pga.WorkDir = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        pga.WorkDir);
                }
            }

            if (string.IsNullOrWhiteSpace(
                pga.Title))
            {
                pga.Title = null;

                if (pga.IsRename)
                {
                    if (pga.RenameFromKeepFile)
                    {
                        pga.ShouldUpdateMdFileContents = true;
                    }
                    else
                    {
                        pga.ShouldUpdateKeepFileContents = true;
                    }
                }
                else
                {
                    pga.SuppressCreationOfNoteDirsPair = true;
                }
            }
            else if (pga.IsRename)
            {
                pga.ShouldUpdateMdFileContents = true;
                pga.ShouldUpdateKeepFileContents = true;
            }

            return pga;
        }

        private ProgramArgs.Item ParseItem(
            string arg, int idx)
        {
            bool startsWithDelim = arg.StartsWith(
                FLAG_START_DELIM_STR);

            bool startsWithDblDelim = startsWithDelim && arg.StartsWith(
                FlagStartDblDelimStr);

            if (startsWithDelim)
            {
                arg = arg.Substring(1);
            }

            bool isFlag = startsWithDelim && !startsWithDblDelim;

            string[]? flagParts = isFlag ? arg.Split(
                FLAG_START_DELIM_STR) : null;

            string? flagName = flagParts?.First();

            var item = new ProgramArgs.Item
            {
                Idx = idx,
                FlagName = flagName,
                Flag = isFlag ? PgaFlagsMap.Single(kvp => kvp.Value == flagName).Key : null,
                IsFlag = isFlag,
                Value = isFlag ? string.Join(":", flagParts!.Skip(1)) : arg
            };

            return item;
        }
    }

    internal class ProgramArgs
    {
        public ProgramArgs()
        {
        }

        public ProgramArgs(ProgramArgs src)
        {
            Items = src.Items;
            WorkDir = src.WorkDir;
            SuppressCreationOfNoteDirsPair = src.SuppressCreationOfNoteDirsPair;
            IsRename = src.IsRename;
            RenameFromKeepFile = src.RenameFromKeepFile;
            CreateFileDirsPair = src.CreateFileDirsPair;
            RecursiveDirNameRegexStr = src.RecursiveDirNameRegexStr;
            RecursiveDirNameRegex = src.RecursiveDirNameRegex;
            Title = src.Title;
            ShortDirName = src.ShortDirName;
            DirsPairJoinStr = src.DirsPairJoinStr;
            ShouldUpdateKeepFileContents = src.ShouldUpdateKeepFileContents;
            ShouldUpdateMdFileContents = src.ShouldUpdateMdFileContents;
        }

        public List<Item> Items { get; set; }

        public string WorkDir { get; set; }
        public bool SuppressCreationOfNoteDirsPair { get; set; }
        public bool IsRename { get; set; }
        public bool RenameFromKeepFile { get; set; }
        public bool CreateFileDirsPair { get; set; }
        public string? FilePairShortDirName { get; set; }
        public string? RecursiveDirNameRegexStr { get; set; }
        public Regex? RecursiveDirNameRegex { get; set; }
        public string? Title { get; set; }
        public string? ShortDirName { get; set; }
        public string DirsPairJoinStr { get; set; }
        public bool ShouldUpdateKeepFileContents { get; set; }
        public bool ShouldUpdateMdFileContents { get; set; }

        public enum Flag
        {
            WorkDir = 0,
            RenameFromMdFile,
            RenameFromKeepFile,
            CreateFilesDirPair,
            Recursive
        }

        public class Item
        {
            public int Idx { get; set; }
            public bool IsFlag { get; set; }
            public string? FlagName { get; set; }
            public Flag? Flag { get; set; }
            public string? Value { get; set; }
        }
    }

    public static class UtilsH
    {
        public static readonly string NwLn = Environment.NewLine;

        public static void OpenWithDefaultProgramIfNotNull(string path)
        {
            if (path != null)
            {
                using Process fileopener = new Process();

                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + path + "\"";
                fileopener.Start();
            }
        }

        public static void ExecuteProgram(
            Action program)
        {
            try
            {
                program();
                Console.ResetColor();
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;

                Console.WriteLine("AN UNHANDLED EXCEPTION WAS THROWN: ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine();
                Console.WriteLine(exc);
                Console.ResetColor();
            }
        }

        public static string JoinStr(
            this string[] strArr,
            string joinStr = null) => string.Join(
                joinStr ?? string.Empty, strArr);

        public static string EncodeForMd(string str)
        {
            str = HttpUtility.HtmlEncode(str);

            str = str.Replace("\\", "\\\\");
            str = str.Replace("_", "\\_");

            return str;
        }

        public static string DecodeForMd(string str)
        {
            str = HttpUtility.HtmlDecode(str);

            str = str.Split("\\\\").Select(
                part => new string(part.Where(
                    c => c != '\\').ToArray(
                        ))).ToArray().JoinStr("\\");

            return str;
        }

        public static KeyValuePair<int, T> FirstKvp<T>(
            this IEnumerable<T> nmrbl,
            Func<T, int, bool> predicate)
        {
            KeyValuePair<int, T> retKvp = new KeyValuePair<int, T>(-1, default);
            int idx = 0;

            foreach (T item in nmrbl)
            {
                if (predicate(item, idx))
                {
                    retKvp = new KeyValuePair<int, T>(idx, item);
                    break;
                }
                else
                {
                    idx++;
                }
            }

            return retKvp;
        }
    }
}
