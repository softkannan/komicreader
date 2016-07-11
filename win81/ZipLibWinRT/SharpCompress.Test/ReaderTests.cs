﻿using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace SharpCompress.Test
{
    public class ReaderTests : TestBase
    {
        protected void Read(string testArchive, CompressionType expectedCompression)
        {
            testArchive = Path.Combine(TEST_ARCHIVES_PATH, testArchive);
            Read(testArchive.AsEnumerable(), expectedCompression);
        }

        protected void Read(IEnumerable<string> testArchives, CompressionType expectedCompression)
        {
            foreach (var path in testArchives)
            {
                using (Stream stream = File.OpenRead(path))
                using (IReader reader = ReaderFactory.Open(stream))
                {
                    UseReader(this, reader, expectedCompression);
                }
            }
        }

        public static void UseReader(TestBase test, IReader reader, CompressionType expectedCompression)
        {
            test.ResetScratch();
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    Assert.AreEqual(reader.Entry.CompressionType, expectedCompression);
                    reader.WriteEntryToDirectory(SCRATCH_FILES_PATH, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }
            test.VerifyFiles();
        }
    }
}
