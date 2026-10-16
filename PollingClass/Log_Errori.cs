using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Database.TableInterface;
using Database.Base;


/// <summary>
/// Descrizione di riepilogo per Log_Errori
/// </summary>
/// 
namespace PollingClass
{
    public class Log_Errori
    {
        public static void ScriviLOGPROCEDURE(string istituto, string idprocesso, string log_procedure)
        {
            log_procedure = log_procedure.Replace((char)39, ' ');
            DatabaseInfo dbInfo = new DatabaseInfo();
            dbInfo.IniziaSessione();

            Aggset.Eseguicomando("insert into LogProcedure(Data,Istituto,IdProcesso,log_linea) values (getdate(),'" + istituto + "','" + idprocesso + "','" + log_procedure.Trim() + "')",
                 dbInfo);

            dbInfo.TerminaSessione();

        }
    }
}