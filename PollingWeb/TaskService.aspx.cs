using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using System.Data;

using Database;
using System.ComponentModel.DataAnnotations;
using PollingClass;
using Quartz;
using Quartz.Impl;

public partial class TaskService : System.Web.UI.Page
{


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            this.AbilitaDisabilita(false,false);
    }

    protected void btnSeleziona_Click(object sender, EventArgs e)
    {
        int indice = ((RepeaterItem)((Control)sender).Parent).ItemIndex;
        this.txtIstituto.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Istituto")).InnerText;
        this.ddlIdProcessi.SelectedValue = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("IdProcesso")).InnerText; ;
        //this.txtIdProcesso.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("IdProcesso")).InnerText;
        //this.txtDescrizione.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Descrizione")).InnerText;
        this.txtPath_Source.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Path_Source")).InnerText;
        this.txtFile_Source.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("File_Source")).InnerText;
        

            this.chkLasciacopia.Checked = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("LasciaInSource")).InnerText == "" ? false :
                    Convert.ToBoolean(((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("LasciaInSource")).InnerText);

        this.txtOrario_Check_File.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Orario_Check_File")).InnerText;
        //this.txtDay_Check_File.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Day_Check_File")).InnerText;
        this.ddlDayQuartz.SelectedValue = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Day_Check_File")).InnerText;
        this.txtPolling_Intervall.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Polling_Intervall")).InnerText;
        this.txtCutoff.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Cutoff")).InnerText;
        this.txtMail_Mittente.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Mail_Mittente")).InnerText;
        this.txtMail_Failure.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Mail_Failure")).InnerText;
        this.txtPath_Archiviazione.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Path_Archiviazione")).InnerText;
        this.txtMail_success.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Mail_success")).InnerText;
        this.txtTipoTask .Text= ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("TipoTask")).InnerText;
    }

    protected void btnModifica_Click(object sender, EventArgs e)
    {
        if (this.txtIstituto.Text == "")
            return;

        Session["Flag_TaskService"] = 1;
        this.AbilitaDisabilita(true,false);
      
    }

    protected void btnUscita_Click(object sender, EventArgs e)
    {
        
        this.AbilitaDisabilita(false,false);
    }
    

    protected void btnElimina_Click(object sender, EventArgs e)
    {
        if (this.txtIstituto.Text == "")
            return;

        this.AbilitaDisabilita(false,false);
        this.Delete();
        this.PulisciCampi();
    }

    protected void btnSalva_Click(object sender, EventArgs e)
    {
        if (!this.ValidaDati())
            return;

        this.AbilitaDisabilita(false,false);

        this.InsertUpdate();        
    }

    protected void btnNuovo_Click(object sender, EventArgs e)
    {
        this.AbilitaDisabilita(true, true);
        this.PulisciCampi();
        Session["Flag_TaskService"] = 0;

    }

    private void AbilitaDisabilita(bool abilita,bool inserimento)
    {
        this.pnl.Enabled = abilita;

        this.btnModifica.Visible = !abilita;
        this.btnElimina.Visible = !abilita;
        this.btnNuovo.Visible = !abilita;

        this.btnSalva.Visible = abilita;
        this.btnUscita.Visible = abilita;

        this.txtIstituto.ReadOnly = !inserimento;
        this.ddlIdProcessi.Enabled = inserimento;
        this.lblErrore.Visible = false;
    }

    private bool ValidaDati()
    {
        bool esito = true;
        this.lblErrore.Text = "";
        this.lblErrore.Visible = !esito;

        if (this.txtIstituto.Text=="")
        {
            this.lblErrore.Text = "istituto obbligatorio";
            this.txtIstituto.Focus();
            esito = false;
            goto fine;
        }

        if (this.txtOrario_Check_File.Text == "")
        {
            this.lblErrore.Text = "orario obbligatorio";
            this.txtOrario_Check_File.Focus();
            esito = false;
            goto fine;
        }

        if (!ValidaData.Esegui(DateTime.Now.ToShortDateString() + " " + this.txtOrario_Check_File.Text))
        {
            this.lblErrore.Text = "formato ora non valido";
            this.txtOrario_Check_File.Focus();
            esito = false;
            goto fine;
        }

               

        //if (this.txtDay_Check_File.Text == "")
        //{
        //    this.lblErrore.Text = "Day check obbligatorio";
        //    this.txtDay_Check_File.Focus();
        //    esito = false;
        //    goto fine;
        //}

        if (!IsNumeric.Esegui(this.txtPolling_Intervall.Text) || IsNumeric.Valore()<0)
        {
            this.lblErrore.Text = "Intervallo non corretto";
            this.txtPolling_Intervall.Focus();
            esito = false;
            goto fine;
        }


        if (!IsNumeric.Esegui(this.txtCutoff.Text) || IsNumeric.Valore() <= 0)
        {
            this.lblErrore.Text = "Cutoff non corretto";
            this.txtCutoff.Focus();
            esito = false;
            goto fine;
        }

        if (this.txtPath_Source.Text == "")
        {
            this.lblErrore.Text = "Path souce obbligatorio";
            this.txtPath_Source.Focus();
            esito = false;
            goto fine;
        }

        if (this.txtFile_Source.Text == "")
        {
            this.lblErrore.Text = "file obbligatorio";
            this.txtFile_Source.Focus();
            esito = false;
            goto fine;
        }

        if (this.txtPath_Archiviazione.Text == "")
        {
            this.lblErrore.Text = "Path archiviazione obbligatorio";
            this.txtPath_Archiviazione.Focus();
            esito = false;
            goto fine;
        }


        //string[] mails;        
        //var mailcheck = new EmailAddressAttribute();

        if (this.txtMail_Mittente.Text == "")
        {
            this.lblErrore.Text = "mail mittente obbligatoria";
            this.txtMail_Mittente.Focus();
            esito = false;
            goto fine;
        }
        else
        {
            //controlliamo che sia valida o multipla           
            if (!ValidaMail.IsValidEmail(this.txtMail_Mittente.Text))
            {
                this.lblErrore.Text = "mail mittente errata";
                this.txtMail_Mittente.Focus();
                esito = false;
                goto fine;
            }

            //mails = this.txtMail_Mittente.Text.Split(';');
            //for (int i = 0; i < mails.Length; i++)
            //{
            //    if (!mailcheck.IsValid(mails[i].Trim()))
            //    {
            //        this.lblErrore.Text = "mail mittente errata";
            //        this.txtMail_Mittente.Focus();
            //        esito = false;
            //        goto fine;
            //    }
            //}
        }




        if (this.txtMail_Failure.Text == "")
        {
            this.lblErrore.Text = "mail failure obbligatoria";
            this.txtMail_Failure.Focus();
            esito = false;
            goto fine;
        }
        else
        {
            //controlliamo che sia valida o multipla
            if (!ValidaMail.IsValidEmail(this.txtMail_Failure.Text))
            {
                this.lblErrore.Text = "mail failure errata";
                this.txtMail_Failure.Focus();
                esito = false;
                goto fine;
            }
            //mails = this.txtMail_Failure.Text.Split(';');
            //for (int i =0; i< mails.Length; i++)
            //{
            //    if (!mailcheck.IsValid(mails[i].Trim()))
            //    {
            //        this.lblErrore.Text = "mail failure errata";
            //        this.txtMail_Failure.Focus();
            //        esito = false;
            //        goto fine;
            //    }
            //}            
        }

        if (this.txtMail_success.Text == "")
        {
            this.lblErrore.Text = "mail success obbligatoria";
            this.txtMail_success.Focus();
            esito = false;
            goto fine;
        }
        else
        {
            //controlliamo che sia valida o multipla
            if (!ValidaMail.IsValidEmail(this.txtMail_success.Text))
            {
                this.lblErrore.Text = "mail success errata";
                this.txtMail_success.Focus();
                esito = false;
                goto fine;
            }
            //mails = this.txtMail_success.Text.Split(';');
            //for (int i = 0; i < mails.Length; i++)
            //{
            //    if (!mailcheck.IsValid(mails[i].Trim()))
            //    {
            //        this.lblErrore.Text = "mail success errata";
            //        this.txtMail_success.Focus();
            //        esito = false;
            //        goto fine;
            //    }
            //}
        }


        fine:
        this.lblErrore.Visible = !esito;
        return esito;
    }

    

    private void InsertUpdate()
    {
        
        try
        {


            Parameter p;
            
            if (Session["Flag_TaskService"].ToString()=="0")
            {
                this.sqlDimProcessi.SelectCommand = "SELECT * FROM DimProcessi WHERE IdProcesso = @IdProcesso";
                this.sqlDimProcessi.SelectParameters.Clear();
                this.sqlDimProcessi.SelectParameters.Add("IdProcesso", TypeCode.String, this.ddlIdProcessi.SelectedValue);
                DataView dw = (DataView) this.sqlDimProcessi.Select(new DataSourceSelectArguments());
                this.txtTipoTask.Text = dw[0]["Tipo"].ToString();

                p = new Parameter("Istituto", TypeCode.Int16, this.txtIstituto.Text);
                this.sqlTaskService.InsertParameters.Add(p);

                p = new Parameter("IdProcesso", TypeCode.String, this.ddlIdProcessi.SelectedValue);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Descrizione", TypeCode.String, this.ddlIdProcessi.SelectedItem.Text.Length > 20 ?
                    this.ddlIdProcessi.SelectedItem.Text.Substring(0, 20): this.ddlIdProcessi.SelectedItem.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Path_Source", TypeCode.String, this.txtPath_Source.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("File_Source", TypeCode.String, this.txtFile_Source.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("LasciaInSource", TypeCode.Boolean, this.chkLasciacopia.Checked.ToString());
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Orario_Check_File", TypeCode.DateTime, this.txtOrario_Check_File.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Day_Check_File", TypeCode.String, this.ddlDayQuartz.SelectedValue);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Polling_Intervall", TypeCode.Int16, this.txtPolling_Intervall.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Cutoff", TypeCode.Int16, this.txtCutoff.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Mail_Mittente", TypeCode.String, this.txtMail_Mittente.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Mail_Failure", TypeCode.String, this.txtMail_Failure.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Mail_success", TypeCode.String, this.txtMail_success.Text);
                this.sqlTaskService.InsertParameters.Add(p);
                p = new Parameter("Path_Archiviazione", TypeCode.String, this.txtPath_Archiviazione.Text);
                this.sqlTaskService.InsertParameters.Add(p);

                this.sqlTaskService.InsertCommand = "Insert into TaskService( " +
                    "Istituto," +
                    "IdProcesso," +
                    "Descrizione," +
                    "Path_Source," +
                    "File_Source," +
                    "LasciaInSource," +
                    "Orario_Check_File," +
                    "Day_Check_File," +
                    "Polling_Intervall," +
                    "Cutoff," +
                    "Mail_Mittente," +
                    "Mail_Failure," +
                    "Mail_success," +
                    "Path_Archiviazione) " +
                    "Values(" +
                    "@Istituto," +
                    "@IdProcesso," +
                    "@Descrizione," +
                    "@Path_Source," +
                    "@File_Source," +
                    "@LasciaInSource," +
                    "@Orario_Check_File," +
                    "@Day_Check_File," +

                    "@Polling_Intervall," +
                    "@Cutoff," +
                    "@Mail_Mittente," +
                    "@Mail_Failure," +
                    "@Mail_success," +
                    "@Path_Archiviazione)";



                this.sqlTaskService.Insert();
                //this.sqlTaskService.InsertParameters.Add(new Parameter("prova", TypeCode.Int16, this.txtIstituto.Text));
            }
            else
            {
                p = new Parameter("Istituto", TypeCode.Int16, this.txtIstituto.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("IdProcesso", TypeCode.String, this.ddlIdProcessi.SelectedValue);
                this.sqlTaskService.UpdateParameters.Add(p);

                //p = new Parameter("Descrizione", TypeCode.String, this.txtDescrizione.Text);
                //this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Descrizione", TypeCode.String, this.ddlIdProcessi.SelectedItem.Text.Length > 20 ?
                    this.ddlIdProcessi.SelectedItem.Text.Substring(0, 20) : this.ddlIdProcessi.SelectedItem.Text);
                this.sqlTaskService.UpdateParameters.Add(p);

                p = new Parameter("Path_Source", TypeCode.String, this.txtPath_Source.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("File_Source", TypeCode.String, this.txtFile_Source.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("LasciaInSource", TypeCode.Boolean, this.chkLasciacopia.Checked.ToString());
                this.sqlTaskService.UpdateParameters.Add(p);

                p = new Parameter("Orario_Check_File", TypeCode.DateTime, this.txtOrario_Check_File.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Day_Check_File", TypeCode.String, this.ddlDayQuartz.SelectedValue);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Polling_Intervall", TypeCode.Int16, this.txtPolling_Intervall.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Cutoff", TypeCode.Int16, this.txtCutoff.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Mail_Mittente", TypeCode.String, this.txtMail_Mittente.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Mail_Failure", TypeCode.String, this.txtMail_Failure.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Mail_success", TypeCode.String, this.txtMail_success.Text);
                this.sqlTaskService.UpdateParameters.Add(p);
                p = new Parameter("Path_Archiviazione", TypeCode.String, this.txtPath_Archiviazione.Text);
                this.sqlTaskService.UpdateParameters.Add(p);

                this.sqlTaskService.UpdateCommand = "Update TaskService set " +
                    "IdProcesso=@IdProcesso," +
                    "Descrizione=@Descrizione," +
                    "Path_Source=@Path_Source," +
                    "File_Source=@File_Source," +
                    "LasciaInSource=@LasciaInSource," +
                    "Orario_Check_File=@Orario_Check_File," +
                    "Day_Check_File=@Day_Check_File," +
                    "Polling_Intervall=@Polling_Intervall," +
                    "Cutoff=@Cutoff," +
                    "Mail_Mittente=@Mail_Mittente," +
                    "Mail_Failure=@Mail_Failure," +
                    "Mail_success=@Mail_success," +
                    "Path_Archiviazione=@Path_Archiviazione" +
                    " where Istituto=@Istituto and IdProcesso=@IdProcesso";

                this.sqlTaskService.Update();
            }

            this.RischedulaTask();

            this.sqlTaskService.Select(new DataSourceSelectArguments());
            this.AbilitaDisabilita(false, false);
            this.PulisciCampi();

           
        }
        catch (Exception ex)
        {
            Log_Errori.ScriviLOGPROCEDURE(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, "TaskService.InsertUpdate " + ex.Message);
            PollingExceptionHandler.Esegui(ex);
        }
    }

    private void Delete()
    {
        try
        {
            this.EliminaTask();

            Parameter p;
            p = new Parameter("Istituto", TypeCode.Int16, this.txtIstituto.Text);
            this.sqlTaskService.DeleteParameters.Add(p);
            p = new Parameter("IdProcesso", TypeCode.String, this.ddlIdProcessi.SelectedValue);
            this.sqlTaskService.DeleteParameters.Add(p);

            this.sqlTaskService.DeleteCommand = "Delete TaskService where Istituto=@Istituto and IdProcesso=@IdProcesso";
            this.sqlTaskService.Delete();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        this.sqlTaskService.Select(new DataSourceSelectArguments());

    }

    private void PulisciCampi()
    {
        this.txtIstituto.Text = "";
        this.ddlIdProcessi.SelectedIndex = 0;
        //this.txtDescrizione.Text = "";
        this.txtPath_Source.Text = "";
        this.txtFile_Source.Text = "";
        this.chkLasciacopia.Checked = false;
        this.txtOrario_Check_File.Text = "";
        this.ddlIdProcessi.SelectedIndex = 0;
        this.txtPolling_Intervall.Text = "";
        this.txtCutoff.Text = "";
        this.txtMail_Mittente.Text = "";
        this.txtMail_Failure.Text = "";
        this.txtPath_Archiviazione.Text = "";
        this.txtMail_success.Text = "";
    }


    private void RischedulaTask()
    {
        switch (this.txtTipoTask.Text)
        {
            case "1": //lettura fornitori
                LeggiFornitore fornitori = new LeggiFornitore();
                fornitori.FirstStartFornitori_nuovo(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, true);
                break;

            case "2": //scrivi ordine
                Ordini_Sap ordini = new Ordini_Sap();
                ordini.Invia_file_Sap_FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue,true,"ODA");
                break;

            case "3": // leggi esiti
                Ordini_Sap esiti = new Ordini_Sap();
                esiti.Leggi_Esito_Sap_FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue,true);
                break;

            case "4": // leggi esiti
                ImportBudget budget = new ImportBudget();
                budget.ImportaBudget_FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, true);
                break;

            case "5": // eport sql
                ExportSQL01 sql = new ExportSQL01();
                sql.SQL01_FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, true);
                break;

            case "6": //invia rda
            Ordini_Sap ordini_rda = new Ordini_Sap();
            ordini_rda.Invia_file_Sap_FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, true,"RDA");
            break;

            case "7": // leggi esiti
                ImportFatture fatture = new ImportFatture();
                fatture.FirstStart(this.txtIstituto.Text, this.ddlIdProcessi.SelectedValue, true);
                break;
            default:               
                break;
        }
    }


    private void EliminaTask()
    {
        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
        scheduler.Start();

        ITrigger trigger = TriggerBuilder.Create()
            //.WithIdentity(this.ddlIdProcessi.SelectedValue + "_" + this.txtIstituto.Text, "IDG")
            .WithIdentity("Trigger_" + this.ddlIdProcessi.SelectedValue + "_" + this.txtIstituto.Text, "IDG")

            .Build();

        scheduler.UnscheduleJob(trigger.Key);

        //ISchedulerFactory schedFact = new StdSchedulerFactory();
        //IScheduler quartzScheduler = schedFact.GetScheduler();
        //quartzScheduler.Start();

        //ITrigger trigger = TriggerBuilder.Create()
        //          .WithIdentity(this.ddlIdProcessi.SelectedValue + "_" + this.txtIstituto.Text, "IDG") 
        //          .Build();
        //quartzScheduler.UnscheduleJob(trigger.Key);
    }







}