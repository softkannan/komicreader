﻿using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCompress.Common;
using SharpCompress.Reader.Tar;

namespace SharpCompress.Test
{
    [TestClass]
    public class TarReaderTests : ReaderTests
    {
        public TarReaderTests()
        {
            UseExtensionInsteadOfNameToVerify = true;
        }

        [TestMethod]
        public void Tar_Reader()
        {
            Read("Tar.tar", CompressionType.None);
        }

        [TestMethod]
        public void Tar_BZip2_Reader()
        {
            Read("Tar.tar.bz2", CompressionType.BZip2);
        }

        [TestMethod]
        public void Tar_GZip_Reader()
        {
            Read("Tar.tar.gz", CompressionType.GZip);
        }

        [TestMethod]
        public void Tar_BZip2_Entry_Stream()
        {
            ResetScratch();
            using (Stream stream = File.OpenRead(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar.bz2")))
            using (var reader = TarReader.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Assert.AreEqual(reader.Entry.CompressionType, CompressionType.BZip2);
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            string file = Path.GetFileName(reader.Entry.FilePath);
                            string folder = Path.GetDirectoryName(reader.Entry.FilePath);
                            string destdir = Path.Combine(SCRATCH_FILES_PATH, folder);
                            if (!Directory.Exists(destdir))
                            {
                                Directory.CreateDirectory(destdir);
                            }
                            string destinationFileName = Path.Combine(destdir, file);

                            using (FileStream fs = File.OpenWrite(destinationFileName))
                            {
                                entryStream.TransferTo(fs);
                            }
                        }
                    }
                }
            }
            VerifyFiles();
        }

        [TestMethod]
        public void Tar_BZip2_Skip_Entry_Stream()
        {
            using (Stream stream = File.OpenRead(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar.bz2")))
            using (var reader = TarReader.Open(stream))
            {
                List<string> names = new List<string>();
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Assert.AreEqual(reader.Entry.CompressionType, CompressionType.BZip2);
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            entryStream.SkipEntry();
                            names.Add(reader.Entry.FilePath);
                        }
                    }
                }
                Assert.AreEqual(names.Count, 3);
            }
        }
    }
}
