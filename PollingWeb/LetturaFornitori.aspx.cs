using System;
using Database;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PollingClass;


public partial class LetturaFornitori : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnStartForgg_Click(object sender, EventArgs e)
    {
        
        if (this.txbStartForgg.Text == "")
        {
            this.lblErrorStartForgg.Visible = true;
            this.lblErrorStartForgg.Text = "istituto mancante";
            return;
        }
        //if (this.txbStartIDprocessgg.Text == "")
        if (this.ddlIdProcessiStart.SelectedValue == "")
        {
            this.lblErrorStartForgg.Visible = true;
            this.lblErrorStartForgg.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorStartForgg.Visible = false;

        try
        {
            //Log_Errori.ScriviLOGPROCEDURE("btnStartForgg_Click ");

            LeggiFornitore leggi = new LeggiFornitore();
            this.lblErrorStartForgg.Text = leggi.FirstStartFornitori_nuovo(this.txbStartForgg.Text, this.ddlIdProcessiStart.SelectedValue);
            this.lblErrorStartForgg.Visible = this.lblErrorStartForgg.Text == "" ? false : true;
            this.lblStartgg.Visible = !this.lblErrorStartForgg.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
        

       
    }

    protected void btnStartForsingola_Click(object sender, EventArgs e)
    {
        if (this.txbStartForsingola.Text == "")
        {
            this.lblErrorStartForsingola.Visible = true;
            this.lblErrorStartForsingola.Text = "istituto mancante";
            return;
        }
        if (this.ddlIdProcessiSingola.SelectedValue == "")
        {
            this.lblErrorStartForsingola.Visible = true;
            this.lblErrorStartForsingola.Text = "idprocesso mancante";
            return;
        }
         

        LeggiFornitore leggi = new LeggiFornitore();
        this.lblErrorStartForsingola.Text = leggi.LetturaForONETIME_nuovo(this.txbStartForsingola.Text, this.ddlIdProcessiSingola.SelectedValue);
        
        this.lblErrorStartForsingola.Visible = this.lblErrorStartForsingola.Text == "" ? false : true;
        this.lblStartsingola.Visible = !this.lblErrorStartForsingola.Visible;
    }

    protected void btnStartForsingola_Click_old(object sender, EventArgs e)
    {
        if (this.txbStartForsingola.Text == "")
        {
            this.lblErrorStartForsingola.Visible = true;
            this.lblErrorStartForsingola.Text = "istituto mancante";
            return;
        }
        if (this.ddlIdProcessiSingola.SelectedValue == "")
        {
            this.lblErrorStartForsingola.Visible = true;
            this.lblErrorStartForsingola.Text = "idprocesso mancante";
            return;
        }


        Fornitori_Scheduler sched = new Fornitori_Scheduler();
        sched.Polling_Interval(0, this.txbStartForsingola.Text, this.ddlIdProcessiSingola.SelectedValue);
        this.lblErrorStartForsingola.Text = sched.messaggio_errore;
        this.lblErrorStartForsingola.Visible = this.lblErrorStartForsingola.Text == "" ? false : true;
        this.lblStartsingola.Visible = !this.lblErrorStartForsingola.Visible;
    }
}