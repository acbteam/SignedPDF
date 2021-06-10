using System;
using System.Linq;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using System.IO;
using Org.BouncyCastle.Pkcs;

namespace SignedPDF
{
    public class Certificates
    {
        string _Path = "";
        string _Password = "";

        public string AppPath { 
            get { return _Path; } 
            set { _Path = value; }
        }

        public string CertPassword
        {
            get { return _Password; }
            set { _Password = value; }
        }

        public MemoryStream GenerateCertificate(Stream template)
        {

            using (MemoryStream output = new MemoryStream())
            {
                PdfReader reader = new PdfReader(template);
                PdfWriter writer = new PdfWriter(output);
                PdfDocument pdf = new PdfDocument(reader, writer);
                Document document = new Document(pdf);

                #region Cert. Header

                Paragraph p = new Paragraph("Mr. JOHN DOE");
                p.SetFixedPosition(88, 690, 500);
                p.AddStyle(DefaultFont(18, true));
                document.Add(p);

                p = new Paragraph("Founder and CEO " + " of Acbteam Corporation");
                p.SetFixedPosition(88, 675, 500);
                p.AddStyle(DefaultFont(14));
                document.Add(p);

                #endregion Cert. Header

                p = new Paragraph();
                p.SetFixedPosition(88, 570, 450);
                p.SetTextAlignment(TextAlignment.JUSTIFIED);
                p.AddStyle(DefaultFont(12, false, false));

                string text = "";
                p.Add(new Text("Do hereby certify: ").AddStyle(DefaultFont(12, true, false)));

                text = @" That according to the records filed as a member Mr. Robert Smith is registered with us since day " + DateTime.Today.ToShortDateString();
                text += ". He is currently a full member of this association who is also qualified to pursue his profession.";

                p.Add(text);
                document.Add(p);

                p = new Paragraph("In witness whereof, at the request of the interested party, I issue this certificate,  with the approval of CEO of the company");
                p.SetFixedPosition(88, 500, 450);
                p.SetTextAlignment(TextAlignment.JUSTIFIED);
                p.AddStyle(DefaultFont(12, false, false));
                document.Add(p);

                #region Footer 

                p = new Paragraph("This PDF is digitally signed by Acbteam Corporation");
                p.AddStyle(DefaultFont(9));
                p.SetTextAlignment(TextAlignment.JUSTIFIED);
                p.SetMarginBottom(11); ;

                p.SetFixedPosition(88, 50, 300);
                p.SetTextAlignment(TextAlignment.JUSTIFIED);
                p.AddStyle(DefaultFont(9, false, false));
                document.Add(p);

                #endregion

                document.Add(p);
                pdf.Close();

                MemoryStream signme = new MemoryStream(output.ToArray());
                return this.SignPdf(signme);
            }

        }

        private MemoryStream SignPdf(Stream input)
        {
            try
            {
                MemoryStream msPFX = new MemoryStream(ResourcePDF.certificate);

                Pkcs12Store pk12 = new Pkcs12Store(msPFX, _Password.ToArray());
                string alias = null;
                foreach (object a in pk12.Aliases)
                {
                    alias = ((string)a);
                    if (pk12.IsKeyEntry(alias))
                        break;
                }
                ICipherParameters pk = pk12.GetKey(alias).Key;

                Org.BouncyCastle.Pkcs.X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                    chain[k] = ce[k].Certificate;

                using (PdfReader reader = new PdfReader(input))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        PdfSigner signer = new PdfSigner(reader, output, new StampingProperties());

                        PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
                        appearance
                            .SetReason("Certificate is issued at the upon request")
                            .SetLocation("alcorcon")
                            .SetContact("acbteam.com");
                        signer.SetFieldName("Signature");

                        IExternalSignature pks = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);
                        signer.SignDetached(pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

                        return output;
                    }
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Style DefaultFont(int size = 14, bool isBold = false, bool isItalic = false)
        {
            var _font = PdfFontFactory.CreateRegisteredFont(StandardFonts.COURIER);

            Style normal = new Style();
            normal
                .SetFont(_font)
                .SetFontSize(size)
                .SetFontColor(ColorConstants.BLACK);

            if (isBold)
                normal.SetBold();

            if (isItalic)
                normal.SetItalic();

            return normal;
        }

    }
}
