using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MkFsDirsPair
{
    internal class ProgramComponent
    {
        public const string DIRS_PAIR_JOIN_STR = "-";
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
                            dirName)
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
                    ShortDirName = pga.ShortDirName,
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
                    pga.WorkDir,
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
                    "# ") ?? false))
            {
                pair.KeepFilePath ??= Path.Combine(
                    pair.PrDirPath,
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

            pair.NewMdFilePath = Path.Combine(
                pga.WorkDir,
                pair.NewMdFilePath);
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
}
