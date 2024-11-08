using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class FsEntry
    {
        public FsEntry()
        {
        }

        public FsEntry(FsEntry src)
        {
            Path = src.Path;
            Name = src.Name;
            IsFolder = src.IsFolder;
            SubFolders = src.SubFolders;
            FolderFiles = src.FolderFiles;
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public List<FsEntry>? SubFolders { get; set; }
        public List<FsEntry>? FolderFiles { get; set; }
        public string FileTextContent { get; set; }
        public List<string> FileTextContentLines { get; set; }
    }
}
