﻿
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCompress.Archive;
using SharpCompress.Archive.Zip;
using SharpCompress.Common;
using SharpCompress.Writer;

namespace SharpCompress.Test
{
    [TestClass]
    public class ZipArchiveTests : ArchiveTests
    {
        public ZipArchiveTests()
        {
            UseExtensionInsteadOfNameToVerify = true;
        }

        [TestMethod]
        public void Zip_ZipX_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.zipx");
        }


        [TestMethod]
        public void Zip_BZip2_Streamed_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.bzip2.dd.zip");
        }
        [TestMethod]
        public void Zip_BZip2_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.bzip2.zip");
        }
        [TestMethod]
        public void Zip_Deflate_Streamed2_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.deflate.dd-.zip");
        }
        [TestMethod]
        public void Zip_Deflate_Streamed_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.deflate.dd.zip");
        }
        [TestMethod]
        public void Zip_Deflate_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.deflate.zip");
        }

        [TestMethod]
        public void Zip_LZMA_Streamed_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.lzma.dd.zip");
        }
        [TestMethod]
        public void Zip_LZMA_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.lzma.zip");
        }
        [TestMethod]
        public void Zip_PPMd_Streamed_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.ppmd.dd.zip");
        }
        [TestMethod]
        public void Zip_PPMd_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.ppmd.zip");
        }
        [TestMethod]
        public void Zip_None_ArchiveStreamRead()
        {
            ArchiveStreamRead("Zip.none.zip");
        }

        [TestMethod]
        public void Zip_BZip2_Streamed_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.bzip2.dd.zip");
        }
        [TestMethod]
        public void Zip_BZip2_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.bzip2.zip");
        }
        [TestMethod]
        public void Zip_Deflate_Streamed2_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.deflate.dd-.zip");
        }
        [TestMethod]
        public void Zip_Deflate_Streamed_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.deflate.dd.zip");
        }
        [TestMethod]
        public void Zip_Deflate_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.deflate.zip");
        }

        [TestMethod]
        public void Zip_LZMA_Streamed_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.lzma.dd.zip");
        }
        [TestMethod]
        public void Zip_LZMA_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.lzma.zip");
        }
        [TestMethod]
        public void Zip_PPMd_Streamed_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.ppmd.dd.zip");
        }
        [TestMethod]
        public void Zip_PPMd_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.ppmd.zip");
        }
        [TestMethod]
        public void Zip_None_ArchiveFileRead()
        {
            ArchiveFileRead("Zip.none.zip");
        }

        [TestMethod]
        public void Zip_Random_Write_Remove()
        {
            string scratchPath = Path.Combine(SCRATCH_FILES_PATH, "Zip.deflate.mod.zip");
            string unmodified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.noEmptyDirs.zip");
            string modified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.mod.zip");

            base.ResetScratch();
            using (var archive = ZipArchive.Open(unmodified))
            {
                var entry = archive.Entries.Where(x => x.FilePath.EndsWith("jpg")).Single();
                archive.RemoveEntry(entry);
                archive.SaveTo(scratchPath, CompressionType.Deflate);
            }
            CompareArchivesByPath(modified, scratchPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArchiveException))]
        public void Zip_Random_Write_Remove_Fail()
        {
            string scratchPath = Path.Combine(SCRATCH_FILES_PATH, "Zip.deflate.mod.zip");
            string unmodified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.noEmptyDirs.zip");
            string modified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.mod.zip");

            base.ResetScratch();
            using (var stream = File.OpenRead(unmodified))
            using (var archive = ZipArchive.Open(stream))
            {
                var entry = archive.Entries.Where(x => x.FilePath.EndsWith("jpg")).Single();
                archive.RemoveEntry(entry);
                archive.SaveTo(scratchPath, CompressionType.Deflate);
            }
            CompareArchivesByPath(modified, scratchPath);
        }

        [TestMethod]
        public void Zip_Random_Write_Add()
        {
            string jpg = Path.Combine(ORIGINAL_FILES_PATH, "jpg\\test.jpg");
            string scratchPath = Path.Combine(SCRATCH_FILES_PATH, "Zip.deflate.mod.zip");
            string unmodified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.mod.zip");
            string modified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.mod2.zip");

            base.ResetScratch();
            using (var archive = ZipArchive.Open(unmodified))
            {
                archive.AddEntry("jpg\\test.jpg", jpg);
                archive.SaveTo(scratchPath, CompressionType.Deflate);
            }
            CompareArchivesByPath(modified, scratchPath);
        }

        [TestMethod]
        public void Zip_Create_New()
        {
            string scratchPath = Path.Combine(SCRATCH_FILES_PATH, "Zip.deflate.noEmptyDirs.zip");
            string unmodified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.noEmptyDirs.zip");

            base.ResetScratch();
            using (var archive = ZipArchive.Create())
            {
                archive.AddAllFromDirectory(ORIGINAL_FILES_PATH);
                archive.SaveTo(scratchPath, CompressionType.Deflate);
            }
            CompareArchivesByPath(unmodified, scratchPath);
        }

        [TestMethod]
        public void Zip_Deflate_WinzipAES_Read()
        {
            ResetScratch();
            using (var reader = ZipArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.WinzipAES.zip"), "test"))
            {
                foreach (var entry in reader.Entries.Where(x => !x.IsDirectory))
                {
                    entry.WriteToDirectory(SCRATCH_FILES_PATH, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }
            VerifyFiles();
        }


        [TestMethod]
        public void Zip_BZip2_Pkware_Read()
        {
            ResetScratch();
            using (var reader = ZipArchive.Open(Path.Combine(TEST_ARCHIVES_PATH, "Zip.bzip2.pkware.zip"), "test"))
            {
                foreach (var entry in reader.Entries.Where(x => !x.IsDirectory))
                {
                    entry.WriteToDirectory(SCRATCH_FILES_PATH, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }
            VerifyFiles();
        }

        [TestMethod]
        public void Zip_Random_Entry_Access()
        {
            string unmodified = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.noEmptyDirs.zip");

            base.ResetScratch();
            ZipArchive a = ZipArchive.Open(unmodified);
            int count = 0;
            foreach (var e in a.Entries)
                count++;

            //Prints 3
            Assert.AreEqual(count, 3);
            a.Dispose();

            a = ZipArchive.Open(unmodified);
            int count2 = 0;

            foreach (var e in a.Entries)
            {
                count2++;

                //Stop at last file
                if (count2 == count)
                {
                    var s = e.OpenEntryStream();
                    s.ReadByte(); //Actually access stream
                    s.Dispose();
                    break;
                }
            }

            int count3 = 0;
            foreach (var e in a.Entries)
                count3++;

            Assert.AreEqual(count3, 3);
        }


        class NonSeekableMemoryStream : MemoryStream
        {
            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }
        }

        [TestMethod]
        public void TestSharpCompressWithEmptyStream()
        {
            MemoryStream stream = new NonSeekableMemoryStream();

            using (IWriter zipWriter = WriterFactory.Open(stream, ArchiveType.Zip, CompressionType.Deflate))
            {
                zipWriter.Write("foo.txt", new MemoryStream(new byte[0]));
                zipWriter.Write("foo2.txt", new MemoryStream(new byte[10]));
            }

            stream = new MemoryStream(stream.ToArray());
            File.WriteAllBytes("foo.zip", stream.ToArray());

            using (var zipArchive = ZipArchive.Open(stream))
            {
                foreach(var entry in zipArchive.Entries)
                {
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        MemoryStream tempStream = new MemoryStream();
                        const int bufSize = 0x1000;
                        byte[] buf = new byte[bufSize];
                        int bytesRead = 0;
                        while ((bytesRead = entryStream.Read(buf, 0, bufSize)) > 0)
                            tempStream.Write(buf, 0, bytesRead);
                    }
                }
            }
        }
    }
}
