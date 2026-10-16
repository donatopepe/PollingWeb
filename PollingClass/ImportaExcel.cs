using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollingClass
{
    public class ConnexionExcel
    {
        public string _pathExcelFile;
        public ExcelQueryFactory _urlConnexion;
        public ConnexionExcel(string path)
        {
            this._pathExcelFile = path;
            this._urlConnexion = new ExcelQueryFactory(_pathExcelFile);
        }
        public string PathExcelFile
        {
            get
            {
                return _pathExcelFile;
            }
        }
        public ExcelQueryFactory UrlConnexion
        {
            get
            {
                return _urlConnexion;
            }
        }
    }

    public class Budget
    {
        public string IST
        {
            get;
            set;
        }
        public string CDR
        {
            get;
            set;
        }
        public string VOCE
        {
            get;
            set;
        }
        public string BDGRILASCI
        {
            get;
            set;
        }
        public string BDGRESIDUO
        {
            get;
            set;
        }

        public string CODFONDO
        {
            get;
            set;
        }
        public string DATARIF
        {
            get;
            set;
        }
    }
}
