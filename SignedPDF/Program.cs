
using System;
using System.IO;

namespace SignedPDF
{
    class Program
    {
        static void Main(string[] args)
        {
            string YourDownloadPath = "C:\\dev\\";
            Console.WriteLine("Creating a Signed PDF...");

            var cls = new Certificates();
            cls.CertPassword = "@acbteam";
            
            MemoryStream mstemplate = new MemoryStream(ResourcePDF.template);

            using (FileStream file = new FileStream(YourDownloadPath + DateTime.Now.Ticks + ".pdf", FileMode.Create, System.IO.FileAccess.Write))
            {
                var ms = cls.GenerateCertificate(mstemplate);
                var result = new MemoryStream(ms.ToArray());
    
                result.CopyTo(file);
                file.Flush();
                
            }

            Console.WriteLine("PDF Generated in " + cls.AppPath);
        }

    }
}
