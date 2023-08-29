using EncryptionDecryptionUsingSymmetricKey;
using Oracle.ManagedDataAccess.Client;
using pdfCertificateChecker;

class Program
{
    static Logger logger = new Logger(".\\" + DateTime.Now.ToString("yyyy-MM") + ".log");
    static Database db = new();

    static void Main(string[] args) { Menu(args); }

    static void Menu(string[] args)
    {
        try
        {
            switch (args.Length)
            {
                case 0:
                    LocalPdfCetificateChecker();
                    break;
                case 1:
                    string[] ar = AesEncryption.DecryptString(args[0]).Split('"');
                    string dbCredentails = string.Empty, select = string.Empty;
                    for (int i = 0; i < ar.Length; i++)
                    {
                        if (ar[i].Trim() == "-db_credentials" && i + 1 < ar.Length) dbCredentails = ar[i + 1];
                        if (ar[i].Trim() == "-select" && i + 1 < ar.Length) select = ar[i + 1];
                    }
                    //Console.WriteLine(dbCredentails, select);
                    if (dbCredentails == string.Empty || select == string.Empty) throw new Exception("Argumenti nisu valjani!");
                    else
                    {
                        try
                        {
                            MakeDatabaseConnection(dbCredentails, db);
                            CheckPdfsBySelect(db, select);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                    }
                    break;
                default:
                    break;
            }
            PrintError(args, string.Empty, string.Empty);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            PrintError(args, e.Message, e.StackTrace + "\n");
        }
        
    }

    static void PrintProgress(string message)
    {
        Console.Write(message);
        Console.SetCursorPosition(0, Console.CursorTop);
    }

    static void PrintError(string[] args, string exceptionMessage, string exceptionStackTrace)
    {
        logger.Log((exceptionMessage == string.Empty ? " \t  OK\t" : " \tError\t") 
            + "[" + StringArrayToSingleString(args) + "]"
            + (exceptionMessage != string.Empty ? "\n  Exception: " + exceptionMessage + "\n" + exceptionStackTrace : ""));
    }

    static string StringArrayToSingleString(string[] array)
    {
        string str = string.Empty;
        foreach(string item in array) str = str + ", " + item;

        return str == string.Empty ? "" : str.Substring(2);
    }

    static void LocalPdfCetificateChecker()
    {
        Console.WriteLine("Provjera potpisa PDF dokumenata na računalu");
        Console.Write("Unesite putanju PDF dokumenta: ");

        string? filePath = Console.ReadLine();
        if (filePath == "") throw new Exception("Putanja nesmije biti prazna!");
        try
        {
            PdfChecker pdf = new(filePath);
            pdf.PrintVariables();
        }
        catch (Exception)
        {
            throw;
        }
    }

    static void MakeDatabaseConnection(string dbCredentails, Database db)
    {
        try
        {
            if (dbCredentails.Count(c => c == '/') == 1 && dbCredentails.Count(c => c == '@') == 1)
            {
                string[] credentialParts = dbCredentails.Split('/');
                string username = credentialParts[0];
                string[] passwordLocation = credentialParts[1].Split('@');
                string password = passwordLocation[0];
                string database = passwordLocation[1];


                db.InitConnection(username, password, database);

            }
            else throw new Exception("Neispravan format podataka za prijavu u bazu!");
        }
        catch (Exception)
        {
            throw;
        }
    }

    static void CheckPdfsBySelect(Database db, string selectQuery)
    {
        string[] queryData = FindColumnsAndTable(selectQuery);

        if (queryData == Array.Empty<string>() || queryData.Length != 11) throw new Exception("SELECT nije valjan!");
        //foreach (string key in queryData) Console.WriteLine("select: " + key);
        try
        {
            OracleDataReader reader = db.ExecuteQuery(selectQuery.TrimEnd(';'), null);
            int progress = 0;
            while (reader.Read())
            {
                try
                {
                    string rowId = (string)reader.GetValue(0);
                    byte[] blobData = (byte[])reader.GetValue(1);
                    string filename = (string)reader.GetValue(2);
                    progress++;

                    OracleParameter[] parameters = new OracleParameter[]
                    {
                    new OracleParameter("p_cn_signer", null),
                    new OracleParameter("p_cn_issuer", null),
                    new OracleParameter("p_o_issuer", null),
                    new OracleParameter("p_nr_serial", null),
                    new OracleParameter("p_date", null),
                    new OracleParameter("p_status", null),
                    new OracleParameter("p_rowid", rowId)
                    };

                    PdfChecker pdf = new(new MemoryStream(blobData), filename, rowId);
                    parameters[0].Value = pdf.CN_Signer;
                    parameters[1].Value = pdf.CN_Issuer;
                    parameters[2].Value = pdf.O_Issuer;
                    parameters[3].Value = pdf.Nr_Serial;
                    parameters[4].Value = pdf.Date;
                    parameters[5].Value = pdf.Signed ? "potpisan" : "nepotpisan";

                    string updateQuery = "UPDATE " + queryData[10] + " SET " + queryData[3]
                        + " = :p_cn_signer, " + queryData[4] + " = :p_cn_issuer, " + queryData[5]
                        + " = :p_o_issuer, " + queryData[6] + " = :p_nr_serial, " + queryData[7]
                        + " = to_date(:p_date, 'dd.mm.yyyy. HH24:MI:SS'), " + queryData[8]
                        + " = :p_status WHERE " + queryData[0] + " = :p_rowid";

                    db.ExecuteQuery(updateQuery, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred for a specific row: " + progress + " [Description: " + ex.Message + "]");
                    string rowId = (string)reader.GetValue(0);
                    string updateQuery = "UPDATE " + queryData[10] + " SET " + queryData[8]
                        + " = 'error', " + queryData[9] + " = '[" + DateTime.Now.ToString("dd/MM/yyyy/ HH:mm:ss") + "] "+ ex.Message + "' WHERE " + queryData[0] + " = '" + rowId + "'";
                    try
                    {
                        db.ExecuteNonQuery(updateQuery);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    
                }
                finally
                {
                    PrintProgress("Broj provjerenih datoteka: " + progress);
                }
            }
            reader.Close();

        }
        catch (Exception ex)
        {
            // Handle the outer exception (e.g., logging, error message)
            Console.WriteLine("An error occurred: " + ex.Message);
            throw;
        }
    }


    static string[] FindColumnsAndTable(string select)
    {
        string[] arr = select.ToUpper().Split(' ');
        List<string> ret = new();

        if (arr[0] != "SELECT") return Array.Empty<string>();

        bool foundFrom = false;
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] == "FROM")
            {
                foundFrom = true;
                continue;
            }
            if (foundFrom)
            {
                ret.Add(arr[i]);
                break;
            }
            ret.Add(arr[i].TrimEnd(','));
        }

        if (foundFrom) return ret.ToArray();
        else return Array.Empty<string>();
    }
}