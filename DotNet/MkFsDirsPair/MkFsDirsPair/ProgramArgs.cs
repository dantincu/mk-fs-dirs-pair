using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MkFsDirsPair
{
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
}
