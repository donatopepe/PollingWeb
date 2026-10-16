
using System;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.DataAnnotations;

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

                DateTime dataNew = Convert.ToDateTime(data);
                valoreRet = dataNew.ToShortDateString();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static string FEvalore()
        {
            try
            {
                valoreRet = Convert.ToDateTime(valoreRet).Year.ToString() + "-" +
                                Convert.ToDateTime(valoreRet).Month.ToString("00") + "-" +
                                Convert.ToDateTime(valoreRet).Day.ToString("00");

                return valoreRet;
            }
            catch
            {
            }
            return "";
        }
        public static string Valore()
        {
            try
            {

                return valoreRet;
            }
            catch
            {
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
            catch
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

            try
            {
                valore = valore.Replace('.', ',');
                Convert.ToDouble(valore);

                valoreRet = Convert.ToDouble(valore);
                valoreRetformat = String.Format("{0:###,###,###,##0." + formattazione + "0}", valore);
                esito = true;
            }
            catch
            {
                valore = "0";

            }
            valoreRet = Convert.ToDouble(valore);
            valoreRetformat = String.Format("{0:###,###,###,##0." + formattazione + "0}", valore);
            return esito;
        }

        public static double Valore()
        {
            try
            {

                return valoreRet;
            }
            catch
            {
            }
            return 0;
        }

        public static string Formattato()
        {
            try
            {
                return valoreRetformat;
            }
            catch
            {
            }
            return "0.00";
        }
        public static string FEFormattato()
        {
            try
            {

                return string.Format("{0:########0.#0}", Convert.ToDecimal(valoreRetformat)).Replace(",", ".");
            }
            catch
            {
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

                return string.Format("{0:########0." + formattazione + "0}", Convert.ToDecimal(valoreRetformat)).Replace(",", ".");
            }
            catch
            {
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

        public static string BuildWhere(ArrayList filtroPar)
        {
            string where = "";
            string segnoOp = "";
            if (filtroPar == null)
                return where;

            foreach (FiltroCampi fc in filtroPar)
            {
                if (where != "")
                    where += fc.NextCond != null && fc.NextCond != "" ? fc.NextCond : " and ";

                segnoOp = fc.SegnoOp != null ? fc.SegnoOp : "=";
                fc.Tipo = fc.Tipo == null ? "C" : fc.Tipo;

                if (fc.Parentesi == "(")
                    where += fc.Parentesi;

                if (fc.Tipo == "N")
                    where += "(" + fc.Campo + ")" + " " + segnoOp + " " + fc.Valore.ToUpper();
                if (fc.Tipo == "C")
                    where += "upper(" + fc.Campo + ")" + " " + segnoOp + " '" + fc.Valore.Trim() + "'";
                //				if (fc.Tipo == "D")
                //					where += "dbo.Datfin(" + fc.Campo + ") " + segnoOp + " Convert(smalldatetime,'" + fc.Valore + "')";				
                if (fc.Tipo == "D")
                    where += "dbo.Datfin(" + fc.Campo + ") " + segnoOp + " Convert(datetime,'" + fc.Valore + "',103)";

                if (fc.Parentesi == ")")
                    where += fc.Parentesi;
            }

            return where;
        }

        #endregion


    }

    

}