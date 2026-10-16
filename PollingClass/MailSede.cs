using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database.Configuration;
using System.Net;
using System.Net.Mail;
using Database;

/// <summary>
/// Descrizione di riepilogo per MailSede
/// </summary>
/// 
namespace PollingClass
{
    public class MailSede
    {
        public static IAppConfig Configuration { get; set; } = new AppConfig();
        private string corpo_mail =
            "<html>< style type='text/css'>" +
            ".latosx {background-color:#dce5ea;font-size:11px;font-family:verdana;}" +
            ".latodx {background-color:#F4F4F4;font-size:10px;font-family:verdana;}" +
            "</style>" +
            "<link href = '{css}' rel='stylesheet' type='text/css'>" +
            "<table border= 0 cellspacing=1 cellpadding=0 align=center width= '700'>" +
            "<tr><td><tbody>" +
            "<table border=1 cellspacing=0 cellpadding=10 width='100%' bgcolor=white>" +
            "<tr>" +
            "<td class='desc_cell_c'>" +
            "<TABLE BORDER = 0 WIDTH='600' CELLSPACING=5 CELLPADDING=0 align=center> " +
            // Nessun logo di terze parti nel corpo della mail: l'immagine puntava al
            // marchio dell'istituto (rimosso in T0.4) e non e' ridistribuibile.
            // Chi installa l'applicazione puo' aggiungere qui il proprio logo, servito
            // dalla stessa applicazione e non da un dominio esterno.

            "<tr><td class='latosx'>Elaborazione Dati</td><td class='latodx'></td></tr>" +
            "<tr><td class='latosx'>ISTITUTO: </td><td class='latodx'><b>{istituto}</b></td></tr>" +
            "<tr><td class='latosx'>Processo</td><td class'latodx'><b>{idprocesso}</b></td></tr>" +
            "<tr><td class='latosx'> Note: </td><td class='latodx'><b>{note}</b></td></tr>" +
            "</table></td></tr></table></tbody></td></tr></table></html>";
        public MailSede()
        {

        }
        #region MAIL SEDE

               
        public void Invia(string tomail, string frommail, string oggetto, string messaggio)
        {
            try
            {
                string user = "username";
                string password = "password";
                    
                MailMessage oMsg = new MailMessage();
                //Imposta il mittente
                oMsg.From = new MailAddress(frommail, frommail);
                //La proprietà .To è una collezione di destinatari, 
                //quindi possiamo addizionare quanti destinatari vogliamo.
                string[] cc_mail = tomail.Split(';');

                oMsg.To.Add(new MailAddress(cc_mail[0], cc_mail[0]));

                

                if (cc_mail.Length > 1)
                {
                    for (int cc = 1; cc < cc_mail.Length; cc++)
                    {
                        oMsg.CC.Add(new MailAddress(cc_mail[cc], cc_mail[cc]));
                    }
                }

                
                //Imposto oggetto
                oMsg.Subject = oggetto;
                //Imposto contenuto

                String TestoBody = messaggio;

                oMsg.Body = TestoBody;
                oMsg.IsBodyHtml = true;
                //Imposto il Server Smtp
                SmtpClient oSmtp = new SmtpClient(
                Configuration.GetValue("smtpServer"));
                

                //Possiamo impostare differenti metodi di spedizione. 
                //Imposta consegna diretta.
                oSmtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //Alcuni Server SMTP richiedono l'accesso autenticato

                NetworkCredential oCredential = new NetworkCredential(user, password);
                //NetworkCredential oCredential = new NetworkCredential("username", "password");
                oSmtp.UseDefaultCredentials = true;
                oSmtp.Credentials = oCredential;
                //Spediamo la mail
                oSmtp.Send(oMsg);


            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }

        }


        #endregion
    }
}