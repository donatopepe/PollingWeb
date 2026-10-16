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
            var settings = SmtpSettings.Load(Configuration);
            SmtpSettings.ValidateRecipientList(tomail);
            if (string.IsNullOrWhiteSpace(frommail)) throw new ArgumentException("SMTP sender address is required.", nameof(frommail));
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(frommail);
                    var recipients = tomail.Split(';');
                    message.To.Add(new MailAddress(recipients[0].Trim()));
                    for (var i = 1; i < recipients.Length; i++)
                        message.CC.Add(new MailAddress(recipients[i].Trim()));
                    message.Subject = oggetto;
                    message.Body = messaggio;
                    message.IsBodyHtml = true;
                    using (var client = new SmtpClient(settings.Host, settings.Port))
                    {
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.EnableSsl = settings.EnableSsl;
                        client.UseDefaultCredentials = settings.UseDefaultCredentials;
                        if (!settings.UseDefaultCredentials)
                            client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                        client.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
        }

        #endregion
    }
}