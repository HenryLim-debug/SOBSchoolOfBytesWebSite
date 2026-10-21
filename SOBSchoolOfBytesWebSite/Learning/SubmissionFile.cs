using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

namespace SOBSchoolOfBytesWebSite.Learning
{
    public sealed class SubmissionFile
    {
        public string Name { get; private set; }
        public byte[] Data { get; private set; }
        public static SubmissionFile Read(HttpPostedFile file)
        {
            if(file==null || string.IsNullOrEmpty(file.FileName)) throw new ClassRuleException("Choose your completed work before submitting.");
            if(file.FileName.Any(char.IsControl)) throw new ClassRuleException("Use a file name without control characters.");
            string name=Path.GetFileName(file.FileName.Replace('\\','/'));
            string ext=Path.GetExtension(name).ToLowerInvariant();
            if(name.Length>180) throw new ClassRuleException("Shorten the file name to 180 characters or fewer.");
            if(ext==".doc" || ext==".docx" || ext==".ppt" || ext==".pptx")
            {
                ChapterUpload office=ChapterUpload.Read(file,ext==".ppt" || ext==".pptx");
                return new SubmissionFile{Name=office.Name,Data=office.Data};
            }
            if(ext!=".pdf" && ext!=".xlsx" && ext!=".py") throw new ClassRuleException("Use Word, PowerPoint, Excel (.xlsx), PDF or Python (.py) files.");
            if(file.ContentLength<=0 || file.ContentLength>ChapterUpload.MaxBytes) throw new ClassRuleException("Your file must be non-empty and no larger than 10 MB.");
            byte[] data;
            using(var output=new MemoryStream())
            {
                byte[] buffer=new byte[8192]; int read;
                while((read=file.InputStream.Read(buffer,0,buffer.Length))>0)
                { if(output.Length+read>ChapterUpload.MaxBytes) throw new ClassRuleException("Your file must be no larger than 10 MB."); output.Write(buffer,0,read); }
                data=output.ToArray();
            }
            if(data.Length==0) throw new ClassRuleException("Your file is empty.");
            if(ext==".pdf" && (data.Length<5 || Encoding.ASCII.GetString(data,0,5)!="%PDF-")) throw new ClassRuleException("Choose a valid PDF file.");
            if(ext==".xlsx")
            {
                try
                {
                    using(var stream=new MemoryStream(data)) using(var zip=new ZipArchive(stream,ZipArchiveMode.Read))
                    {
                        if(zip.Entries.Count>5000 || zip.Entries.Sum(e=>e.Length)>200L*1024*1024 || zip.GetEntry("xl/workbook.xml")==null || zip.GetEntry("[Content_Types].xml")==null ||
                            zip.Entries.Any(e=>e.FullName.EndsWith("vbaProject.bin",StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException();
                    }
                }
                catch(InvalidDataException) { throw new ClassRuleException("Save your Excel workbook as a valid .xlsx file and try again."); }
            }
            if(ext==".py")
            {
                try { if(new UTF8Encoding(false,true).GetString(data).Contains("\0")) throw new DecoderFallbackException(); }
                catch(DecoderFallbackException) { throw new ClassRuleException("Save your Python file as UTF-8 text and try again."); }
            }
            return new SubmissionFile{Name=name,Data=data};
        }
    }
}
