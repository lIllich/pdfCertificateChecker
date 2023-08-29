using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace pdfCertificateChecker
{
    internal class PdfChecker
    {
        PdfReader reader;
        private string filename = string.Empty;
        private string rowId;

        private bool signed = false;
        private string CN_signer;
        private string C_signer;
        private string CN_issuer;
        private string OU_issuer;
        private string O_issuer;
        private string C_issuer;
        private string nr_serial;
        private string date;

        public PdfChecker(MemoryStream stream, string filename, string id)
        {
            IsSigned(stream);
            this.filename = filename;
            rowId = id;
        }

        public PdfChecker(string path)
        {
            if (IsPdf(path))
            {
                this.filename = Path.GetFileName(path);
                reader = new PdfReader(path);
                IsSigned(path);
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.filename);
        }

        static bool IsPdf(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) return true;
            else throw new Exception("Putanja nije valjana!");
        }

        void IsSigned(object o)
        {
            try
            {
                if (o is MemoryStream) this.reader = new PdfReader((MemoryStream)o);
                AcroFields af = this.reader.AcroFields;
                PdfPKCS7 pk;

                // Check if there are any signature fields in the PDF
                if (af != null && af.GetSignatureNames().Count > 0)
                {
                    var names = af.GetSignatureNames();
                    this.signed = true;
                    foreach (string name in names)
                    {
                        pk = af.VerifySignature(name);

                        this.CN_signer = CertificateInfo.GetSubjectFields(pk.SigningCertificate).GetField("CN");
                        this.C_signer = CertificateInfo.GetSubjectFields(pk.SigningCertificate).GetField("C");
                        this.CN_issuer = CertificateInfo.GetIssuerFields(pk.SigningCertificate).GetField("CN");
                        this.OU_issuer = CertificateInfo.GetIssuerFields(pk.SigningCertificate).GetField("OU");
                        this.O_issuer = CertificateInfo.GetIssuerFields(pk.SigningCertificate).GetField("O");
                        this.C_issuer = CertificateInfo.GetIssuerFields(pk.SigningCertificate).GetField("C");
                        this.nr_serial = pk.SigningCertificate.SerialNumber.ToString();
                        this.date = pk.SignDate.ToString();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void PrintVariables()
        {
            Console.WriteLine("\nDatoteka: " + this.filename);
            if (this.signed)
            {
                Console.WriteLine("Signer: " + this.CN_signer);
                Console.WriteLine("Country: " + this.C_signer);
                Console.WriteLine("Issuer: " + this.CN_issuer);
                Console.WriteLine("Issuer OU: " + this.OU_issuer);
                Console.WriteLine("Issuer O: " + this.O_issuer);
                Console.WriteLine("Issuer Country: " + this.C_issuer);
                Console.WriteLine("Serial Number: " + this.nr_serial);
                Console.WriteLine("Signing Date: " + this.date);
            }
            else
            {
                Console.WriteLine("Nije potpisana!");
            }
        }

        public string RowId
        {
            get { return rowId; }
        }
        public string Filename
        {
            get { return filename; }
        }

        public bool Signed
        {
            get { return signed; }
        }

        public string CN_Signer
        {
            get { return CN_signer; }
        }

        public string C_Signer
        {
            get { return C_signer; }
        }

        public string CN_Issuer
        {
            get { return CN_issuer; }
        }

        public string OU_Issuer
        {
            get { return OU_issuer; }
        }

        public string O_Issuer
        {
            get { return O_issuer; }
        }

        public string C_Issuer
        {
            get { return C_issuer; }
        }

        public string Nr_Serial
        {
            get { return nr_serial; }
        }

        public string Date
        {
            get { return date; }
        }
    }
}
