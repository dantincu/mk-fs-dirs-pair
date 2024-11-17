using MkFsDirsPairBareBones;
using System.Reflection.PortableExecutable;

namespace TestProject
{
    public abstract class UnitTestBase
    {
        protected const string TEMP_DIR_NAME = "__TEMP__";

        protected void PerformTest(
            Action<string> assertAction)
        {
            var tempDirPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                TEMP_DIR_NAME);

            if (Directory.Exists(tempDirPath))
            {
                Directory.Delete(tempDirPath, true);
            }

            Directory.CreateDirectory(tempDirPath);
            assertAction(tempDirPath);
        }

        protected void PerformTest(
            string[] args,
            Action<string> assertAction,
            bool insertWorkDirArg = true) =>
                PerformTest(new TestArgs(
                    args, assertAction,
                    insertWorkDirArg));

        protected void PerformTest(
            string[] args,
            List<FsEntry> expectedFsEntriesList,
            bool insertWorkDirArg = true) => 
                PerformTest(new FsEntriesTestArgs(
                    args, expectedFsEntriesList,
                    insertWorkDirArg));

        protected void PerformTest(
            TestArgs args) =>
                PerformTest(tempDirPath =>
                {
                    PerformTestCore(
                        tempDirPath,
                        args);
                });

        protected void PerformTest(
            FsEntriesTestArgs args) =>
                PerformTest(tempDirPath =>
                {
                    args.OnBeforeAssert?.Invoke(
                        tempDirPath);

                    PerformTestCore(
                        tempDirPath,
                        args);
                });

        protected void PerformTest(
            TestArgs[] argsArr) => PerformTest(tempDirPath =>
            {
                foreach (var args in argsArr)
                {
                    PerformTestCore(
                        tempDirPath,
                        args);
                }
            });

        protected void PerformTest(
            FsEntriesTestArgs[] argsArr) => PerformTest(tempDirPath =>
            {
                foreach (var args in argsArr)
                {
                    args.OnBeforeAssert?.Invoke(
                        tempDirPath);

                    PerformTestCore(
                        tempDirPath,
                        args);
                }
            });

        protected void PerformTestCore(
            string tempDirPath,
            TestArgs args)
        {
            if (args.InsertWorkDirArg)
            {
                var argsList = args.Args.ToList();
                argsList.Insert(0, $":wk:{tempDirPath}");
                args.Args = argsList.ToArray();
            }

            new ProgramComponent().Run(args.Args);
            args.AssertAction(tempDirPath);
        }

        protected void PerformTestCore(
            string tempDirPath,
            FsEntriesTestArgs args) => PerformTestCore(
                tempDirPath, new TestArgs(
                    args.Args, dirPath => AssertFsEntries(
                        args.ExpectedFsEntriesList,
                        dirPath), args.InsertWorkDirArg));

        protected void AssertFsEntries(
            List<FsEntry> expectedFsEntriesList,
            string dirPath)
        {
            expectedFsEntriesList = expectedFsEntriesList.ToList();

            var actualFsEntriesArr = new DirectoryInfo(
                dirPath).EnumerateFileSystemInfos().ToArray();

            var expectedFsEntriesCount = expectedFsEntriesList.Count;
            var actualFsEntriesCount = actualFsEntriesArr.Length;

            Assert.Equal(
                expectedFsEntriesCount,
                actualFsEntriesCount);

            for (int i = 0; i < actualFsEntriesCount; i++)
            {
                var actualFsEntry = actualFsEntriesArr[i];

                var kvp = expectedFsEntriesList.FirstKvp(
                    (entry, idx) => entry.Name == actualFsEntry.Name);

                Assert.True(kvp.Key >= 0);

                Assert.Equal(
                    kvp.Value.IsFolder,
                    actualFsEntry is DirectoryInfo);

                if (kvp.Value.IsFolder)
                {
                    var childrenList = new List<FsEntry>();

                    childrenList.AddRange(
                        kvp.Value.SubFolders ?? []);

                    childrenList.AddRange(
                        kvp.Value.FolderFiles ?? []);

                    var childDirPath = Path.Combine(
                        dirPath,
                        kvp.Value.Name);

                    AssertFsEntries(
                        childrenList,
                        childDirPath);
                }
                else
                {
                    kvp.Value.FileTextContent ??= kvp.Value.FileTextContentLines?.ToArray(
                        ).JoinStr(Environment.NewLine)!;

                    if (kvp.Value.FileTextContent != null)
                    {
                        var actualFileTextContent = File.ReadAllText(
                            Path.Combine(dirPath,
                                kvp.Value.Name));

                        Assert.Equal(
                            kvp.Value.FileTextContent,
                            actualFileTextContent);
                    }
                }
            }
        }

        protected class TestArgs
        {
            public TestArgs(
                string[] args,
                Action<string> assertAction,
                bool insertWorkDirArg = true)
            {
                Args = args;
                AssertAction = assertAction;
                InsertWorkDirArg = insertWorkDirArg;
                AssertAction = assertAction;
            }

            public string[] Args { get; set; }
            public Action<string> AssertAction { get; set; }
            public bool InsertWorkDirArg { get; set; }
        }

        protected class FsEntriesTestArgs
        {
            public FsEntriesTestArgs(
                string[] args,
                List<FsEntry> expectedFsEntriesList,
                bool insertWorkDirArg = true,
                Action<string>? onBeforeAssert = null)
            {
                Args = args;
                ExpectedFsEntriesList = expectedFsEntriesList;
                InsertWorkDirArg = insertWorkDirArg;
                OnBeforeAssert = onBeforeAssert;
            }

            public string[] Args { get; set; }
            public List<FsEntry> ExpectedFsEntriesList { get; set; }
            public bool InsertWorkDirArg { get; set; }
            public Action<string>? OnBeforeAssert { get; set; }
        }
    }
}