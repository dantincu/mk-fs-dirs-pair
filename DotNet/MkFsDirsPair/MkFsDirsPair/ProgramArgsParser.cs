﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MkFsDirsPair
{
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
}
