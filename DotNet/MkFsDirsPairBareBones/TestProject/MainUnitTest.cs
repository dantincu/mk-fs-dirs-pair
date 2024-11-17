using MkFsDirsPairBareBones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class MainUnitTest : UnitTestBase
    {
        [Fact]
        public void BasicNoteTest()
        {
            PerformTest(["asdf", "999"], [
                new()
                {
                    Name = "999",
                    IsFolder = true,
                    FolderFiles = [
                        new()
                        {
                            Name = "0-asdf[note].md",
                            FileTextContentLines = [
                                "# asdf",
                                "",
                                ""]
                        }]
                },
                new()
                {
                    Name = "999-asdf",
                    IsFolder = true,
                    FolderFiles = [
                        new()
                        {
                            Name = ".keep",
                            FileTextContent = "asdf"
                        }]
                }]);
        }

        [Fact]
        public void FilesPairTest()
        {
            PerformTest([":nf:01"], [
                new ()
                {
                    Name = "01",
                    IsFolder = true,
                },
                new ()
                {
                    Name = "01-[note-files]",
                    IsFolder = true,
                    FolderFiles = [
                        new()
                        {
                            Name = ".keep",
                            FileTextContent = "Note Files"
                        }]
                }]);
        }

        [Fact]
        public void NoteWithFilesPairTest()
        {
            PerformTest(["asdf", "999", ":nf:01"], [
                new()
                {
                    Name = "999",
                    IsFolder = true,
                    FolderFiles = [
                        new()
                        {
                            Name = "0-asdf[note].md",
                            FileTextContentLines = [
                                "# asdf",
                                "",
                                ""]
                        }],
                    SubFolders = [
                        new ()
                        {
                            Name = "01",
                            IsFolder = true,
                        },
                        new ()
                        {
                            Name = "01-[note-files]",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = ".keep",
                                    FileTextContent = "Note Files"
                                }]
                        }]
                },
                new()
                {
                    Name = "999-asdf",
                    IsFolder = true,
                    FolderFiles = [
                        new()
                        {
                            Name = ".keep",
                            FileTextContent = "asdf"
                        }]
                }]);
        }

        [Fact]
        public void RenameNoteTest()
        {
            PerformTest(new FsEntriesTestArgs[]
                {
                    new (["asdf", "999"], [
                        new()
                        {
                            Name = "999",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = "0-asdf[note].md",
                                    FileTextContentLines = [
                                        "# asdf",
                                        "",
                                        ""]
                                }]
                        },
                        new()
                        {
                            Name = "999-asdf",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = ".keep",
                                    FileTextContent = "asdf"
                                }]
                        }]),
                    new ([":rn", $":wk:{Path.Combine(TEMP_DIR_NAME, "999")}", "qwer"], [
                        new()
                        {
                            Name = "999",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = "0-qwer[note].md",
                                    FileTextContentLines = [
                                        "# qwer",
                                        "",
                                        ""]
                                }]
                        },
                        new()
                        {
                            Name = "999-qwer",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = ".keep",
                                    FileTextContent = "qwer"
                                }]
                        }], false),
                    new ([":rn", $":wk:{Path.Combine(TEMP_DIR_NAME, "999")}"], [
                        new()
                        {
                            Name = "999",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = "0-zxcv[note].md",
                                    FileTextContentLines = [
                                        "# zxcv",
                                        "",
                                        ""]
                                }]
                        },
                        new()
                        {
                            Name = "999-zxcv",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = ".keep",
                                    FileTextContent = "zxcv"
                                }]
                        }], false, tempDirPath =>
                        {
                            File.WriteAllLines(
                                Path.Combine(
                                    tempDirPath,
                                    "999",
                                    "0-qwer[note].md"),
                                    ["# zxcv", ""]
                                );
                        }),
                    new ([":rk", $":wk:{Path.Combine(TEMP_DIR_NAME, "999")}"], [
                        new()
                        {
                            Name = "999",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = "0-tyui[note].md",
                                    FileTextContentLines = [
                                        "# tyui",
                                        "",
                                        ""]
                                }]
                        },
                        new()
                        {
                            Name = "999-tyui",
                            IsFolder = true,
                            FolderFiles = [
                                new()
                                {
                                    Name = ".keep",
                                    FileTextContent = "tyui"
                                }]
                        }], false, tempDirPath =>
                        {
                            File.WriteAllText(
                                Path.Combine(
                                    tempDirPath,
                                    "999-zxcv",
                                    ".keep"),
                                    "tyui"
                                );
                        }),
                });
        }

        [Fact]
        public void RecursiveRenameNoteTest()
        {
            PerformTest(new FsEntriesTestArgs[]
            {
                new ([":rn", ":rc:^[1-9][0-9]{2}$"], [
                    new ()
                    {
                        Name = "197",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = "0-Competitions[note].md",
                                FileTextContentLines = [
                                    "# Competitions",
                                    "",
                                    ""]
                            }],
                        SubFolders = [
                            new ()
                            {
                                Name = "01",
                                IsFolder = true
                            },
                            new ()
                            {
                                Name = "01-[note-files]",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Note Files"
                                    }]
                            },
                            new ()
                            {
                                Name = "998",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Champions League[note].md",
                                        FileTextContentLines = [
                                            "# Champions League",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "998-Champions League",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Champions League"
                                    }]
                            },
                            new ()
                            {
                                Name = "999",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Europa League[note].md",
                                        FileTextContentLines = [
                                            "# Europa League",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "999-Europa League",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Europa League"
                                    }]
                            }]
                    },
                    new ()
                    {
                        Name = "197-Competitions",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = ".keep",
                                FileTextContent = "Competitions"
                            }]
                    },
                    new ()
                    {
                        Name = "198",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = "0-Players[note].md",
                                FileTextContentLines = [
                                    "# Players",
                                    "",
                                    ""]
                            }],
                        SubFolders = [
                            new ()
                            {
                                Name = "998",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Cristiano Ronaldo[note].md",
                                        FileTextContentLines = [
                                            "# Cristiano Ronaldo",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "998-Cristiano Ronaldo",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Cristiano Ronaldo"
                                    }]
                            },
                            new ()
                            {
                                Name = "999",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Lionel Messi[note].md",
                                        FileTextContentLines = [
                                            "# Lionel Messi",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "999-Lionel Messi",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Lionel Messi"
                                    }]
                            }]
                    },
                    new ()
                    {
                        Name = "198-Players",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = ".keep",
                                FileTextContent = "Players"
                            }]
                    },
                    new ()
                    {
                        Name = "199",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = "0-Former Players[note].md",
                                FileTextContentLines = [
                                    "# Former Players",
                                    "",
                                    ""]
                            }],
                        SubFolders = [
                            new ()
                            {
                                Name = "998",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Ronaldo Nazario[note].md",
                                        FileTextContentLines = [
                                            "# Ronaldo Nazario",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "998-Ronaldo Nazario",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Ronaldo Nazario"
                                    }]
                            },
                            new ()
                            {
                                Name = "999",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = "0-Zinedine Zidane[note].md",
                                        FileTextContentLines = [
                                            "# Zinedine Zidane",
                                            "",
                                            ""]
                                    }],
                            },
                            new ()
                            {
                                Name = "999-Zinedine Zidane",
                                IsFolder = true,
                                FolderFiles = [
                                    new ()
                                    {
                                        Name = ".keep",
                                        FileTextContent = "Zinedine Zidane"
                                    }]
                            }]
                    },
                    new ()
                    {
                        Name = "199-Former Players",
                        IsFolder = true,
                        FolderFiles = [
                            new ()
                            {
                                Name = ".keep",
                                FileTextContent = "Former Players"
                            }]
                    }], true, tempDirPath =>
                {
                    var component = new ProgramComponent();
                    component.Run([$":wk:{TEMP_DIR_NAME}", "Former Players", "199"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "199")}", "Ronaldo Nazario", "998"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "199")}", "Zinedine Zidane", "999"]);
                    component.Run([$":wk:{TEMP_DIR_NAME}", "Players", "198"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "198")}", "Cristiano Ronaldo", "998"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "198")}", "Lionel Messi", "999"]);
                    component.Run([$":wk:{TEMP_DIR_NAME}", "Competitions", "197", ":nf:01"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "197")}", "Champions League", "998"]);
                    component.Run([$":wk:{Path.Combine(TEMP_DIR_NAME, "197")}", "Europa League", "999"]);
                })
            });
        }
    }
}
