
using System;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Database
{

    public enum EsitiLoginUtente : byte
    {
        Annullato = 0,
        Fallito = 1,
        Riuscito = 2
    }

    

    public class ValidaData
    {
        private static string valoreRet = "";
        public static bool Esegui(string data)
        {

            try
            {

                if (data == null || data == "" || data == "  /  /    ")
                    return true;

                if (data.Length < 10)
                    return false;

                DateTime dataNew;
                if (!DateTime.TryParseExact(data.Trim(), new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out dataNew))
                    return false;
                valoreRet = dataNew.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                return true;
            }
            catch (ArgumentException)
            {
                valoreRet = string.Empty;
                return false;
            }

        }

        public static string FEvalore()
        {
            try
            {
                DateTime parsed;
                if (!DateTime.TryParseExact(valoreRet, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    return string.Empty;
                valoreRet = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return valoreRet;
            }
            catch (FormatException)
            {
                throw;
            }
            return "";
        }
        public static string Valore()
        {
            try
            {

                return valoreRet;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            return "";
        }

    }


    public class ValidaMail
    {
        public static bool IsValidEmail(string email)
        {
            string[] mails;
            mails = email.Replace(",", ";").Replace(" ", "").Split(';');
            var mailcheck = new EmailAddressAttribute();

            try
            {
                //var addr = new System.Net.Mail.MailAddress(email);
                //return addr.Address == email;
                for (int i = 0; i < mails.Length; i++)
                {
                    if (!mailcheck.IsValid(mails[i].Trim()))                    
                        return false;                    
                }

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }

    public class IsNumeric
    {
        private static double valoreRet = 0;
        private static string valoreRetformat = "0.00";

        public static bool Esegui(string valore)
        {
            return Esegui(valore, 2);
          

        }
        public static bool Esegui(string valore, int decimali)
        {
            bool esito = false;
            string formattazione = "";
            for (int i = 0; i < decimali - 1; i++)
            {
                formattazione += "#";
            }

            double parsed;
            if (!double.TryParse(valore, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return false;

            valoreRet = parsed;
            valoreRetformat = parsed.ToString("N" + decimali, CultureInfo.InvariantCulture);
            return true;
        }

        public static double Valore()
        {
            try
            {

                return valoreRet;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
            return 0;
        }

        public static string Formattato()
        {
            try
            {
                return valoreRetformat;
            }
            catch (InvalidOperationException)
            {
                return "0.00";
            }
            return "0.00";
        }
        public static string FEFormattato()
        {
            try
            {

                return valoreRet.ToString("0.00", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return "0.00";
            }
            return "0.00";
        }
        public static string FEFormattato(int decimali)
        {
            string formattazione = "";
            for (int i = 0; i < decimali - 1; i++)
            {
                formattazione += "#";
            }

            try
            {

                return valoreRet.ToString("F" + decimali, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return "0.00";
            }
            return "0.00";
        }
    }


    public enum TipoCampo : byte
    {
        Carattere = 0,
        Numerico,
        Data
    }

    public enum OrdinaCampo : byte
    {
        Crescente = 0,
        Decrescente
    }

    public class FiltroCampi
    {
        string campo = null;
        string valore = null;
        string tipo = null;
        string nextCond = "";
        string segnoOp = null;
        byte attivo;


        public FiltroCampi()
        {
        }

        public string Campo
        {
            get { return this.campo; }
            set { this.campo = value; }
        }

        public string Valore
        {
            get { return this.valore; }
            set { this.valore = value; }
        }

        [Description("Tipo C=Carattere, N=Numerico, D=Data")]
        public string Tipo
        {
            get { return this.tipo; }
            set { this.tipo = value; }
        }


        [Description("Segno operazione >, >=, < , <=, =")]
        public string SegnoOp
        {
            get
            {
                return segnoOp;
            }
            set
            {
                segnoOp = value;
            }
        }


        [Description("Condizione Succ. And, Or")]
        public string NextCond
        {
            get
            {
                return nextCond;
            }
            set
            {
                nextCond = value;
            }
        }

        public byte Attivo
        {
            get
            {
                return attivo;
            }
            set
            {
                attivo = value;
            }
        }

        string parentesi = "";
        public string Parentesi
        {
            get
            {
                return parentesi;
            }
            set
            {
                parentesi = value;
            }
        }


        bool ordinato = false;
        public bool Ordinato
        {
            get
            {
                return ordinato;
            }
            set
            {
                ordinato = value;
            }
        }


        OrdinaCampo tipoord;
        public OrdinaCampo TipoOrdinamento
        {
            get
            {
                return tipoord;
            }
            set
            {
                tipoord = value;
            }
        }

        #region METODI DI SUPPORTO 

        private static readonly System.Collections.Generic.HashSet<string> CampiFiltroConsentiti =
            new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Istituto", "IdProcesso", "Ora_fine", "Data", "Codist", "Tiporecord", "Elaborato"
            };

        private static string CampoSql(FiltroCampi filtro)
        {
            var campo = filtro.Campo == "isnull(Elaborato,0)" ? "ISNULL(Elaborato,0)" : filtro.Campo;
            if (string.IsNullOrWhiteSpace(campo) ||
                (campo != "ISNULL(Elaborato,0)" && !CampiFiltroConsentiti.Contains(campo)))
                throw new ArgumentException("Campo filtro non consentito.", nameof(filtro));
            return campo;
        }

        private static string OperatoreSql(FiltroCampi filtro)
        {
            var op = string.IsNullOrWhiteSpace(filtro.SegnoOp) ? "=" : filtro.SegnoOp.Trim();
            if (op != "=" && op != "<" && op != ">" && op != "<=" && op != ">=")
                throw new ArgumentException("Operatore filtro non consentito.", nameof(filtro));
            return op;
        }

        private static string ConnettoreSql(FiltroCampi filtro)
        {
            var connector = string.IsNullOrWhiteSpace(filtro.NextCond) ? "AND" : filtro.NextCond.Trim();
            if (connector.Equals("AND", StringComparison.OrdinalIgnoreCase)) return " AND ";
            if (connector.Equals("OR", StringComparison.OrdinalIgnoreCase)) return " OR ";
            throw new ArgumentException("Connettore filtro non consentito.", nameof(filtro));
        }

        public static string BuildWhere(ArrayList filtroPar, Microsoft.Data.SqlClient.SqlCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            var where = new System.Text.StringBuilder();
            if (filtroPar == null) return string.Empty;

            var index = 0;
            foreach (FiltroCampi filtro in filtroPar)
            {
                if (filtro == null) throw new ArgumentException("Filtro nullo.", nameof(filtroPar));
                if (where.Length > 0) where.Append(ConnettoreSql(filtro));
                var campo = CampoSql(filtro);
                var op = OperatoreSql(filtro);
                var tipo = string.IsNullOrWhiteSpace(filtro.Tipo) ? "C" : filtro.Tipo.Trim().ToUpperInvariant();
                var parameterName = "@p" + index++;
                var expression = tipo == "C" ? "UPPER(" + campo + ")" : tipo == "D" ? "dbo.Datfin(" + campo + ")" : "(" + campo + ")";
                if (filtro.Parentesi == "(") where.Append('(');
                if (filtro.Parentesi != "" && filtro.Parentesi != "(" && filtro.Parentesi != ")")
                    throw new ArgumentException("Parentesi filtro non consentita.", nameof(filtroPar));
                where.Append(expression).Append(' ').Append(op).Append(' ').Append(parameterName);
                if (filtro.Parentesi == ")") where.Append(')');

                object value;
                if (tipo == "N")
                {
                    decimal numeric;
                    if (!decimal.TryParse(filtro.Valore, System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out numeric))
                        throw new ArgumentException("Valore numerico non valido.", nameof(filtroPar));
                    value = numeric;
                }
                else if (tipo == "D")
                {
                    DateTime date;
                    if (!DateTime.TryParseExact(filtro.Valore, new[] { "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss" },
                        System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                        throw new ArgumentException("Valore data non valido.", nameof(filtroPar));
                    value = date;
                }
                else if (tipo == "C") value = (filtro.Valore ?? string.Empty).Trim().ToUpperInvariant();
                else throw new ArgumentException("Tipo filtro non consentito.", nameof(filtroPar));
                command.Parameters.AddWithValue(parameterName, value);
            }
            return where.ToString();
        }


        #endregion


    }

    

}