using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public sealed class ChapterUpload
    {
        public const int MaxBytes = 10 * 1024 * 1024;
        public string Name { get; private set; }
        public byte[] Data { get; private set; }

        public static ChapterUpload Read(HttpPostedFile file, bool slides)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName)) return null;
            string name = Path.GetFileName(file.FileName.Replace('\\', '/'));
            string extension = Path.GetExtension(name).ToLowerInvariant();
            string[] allowed = slides ? new[] { ".ppt", ".pptx" } : new[] { ".doc", ".docx" };
            if (!allowed.Contains(extension))
                throw new ClassRuleException(slides ? "Lecture slides must be a .ppt or .pptx file." : "Tutorial questions must be a .doc or .docx file.");
            if (name.Length > 180 || name.Any(char.IsControl))
                throw new ClassRuleException("Use a shorter file name (up to 180 characters) without control characters.");
            if (file.ContentLength <= 0 || file.ContentLength > MaxBytes)
                throw new ClassRuleException("Each uploaded file must be non-empty and no larger than 10 MB.");
            byte[] data;
            using (var output = new MemoryStream())
            {
                byte[] buffer = new byte[8192]; int read;
                while ((read = file.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (output.Length + read > MaxBytes) throw new ClassRuleException("Each uploaded file must be no larger than 10 MB.");
                    output.Write(buffer, 0, read);
                }
                data = output.ToArray();
            }
            if (data.Length == 0) throw new ClassRuleException("The uploaded document is empty.");
            if (extension.EndsWith("x", StringComparison.Ordinal))
            {
                try
                {
                    using (var stream = new MemoryStream(data))
                    using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        string main = slides ? "ppt/presentation.xml" : "word/document.xml";
                        if (zip.Entries.Count > 5000 || zip.Entries.Sum(e => e.Length) > 200L * 1024 * 1024 ||
                            zip.GetEntry("[Content_Types].xml") == null || zip.GetEntry(main) == null ||
                            zip.Entries.Any(e => e.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase)))
                            throw new InvalidDataException();
                    }
                }
                catch (InvalidDataException) { throw new ClassRuleException("This file is not a supported Office document. Open it in Word or PowerPoint and save it again as .docx or .pptx."); }
            }
            else
            {
                byte[] signature = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
                if (data.Length < 512 || !data.Take(8).SequenceEqual(signature))
                    throw new ClassRuleException("The file contents do not match an Office document. Save the file from Word or PowerPoint and try again.");
            }
            return new ChapterUpload { Name = name, Data = data };
        }
    }
}
