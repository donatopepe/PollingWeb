using PollingClass;
using System;
using Database;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Ordini_sap : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnRDA_Click(object sender, EventArgs e)
    {
        if (this.txtIstRDA.Text == "")
        {
            this.lblErrorRDA.Visible = true;
            this.lblErrorRDA.Text = "istituto mancante";
            return;
        }

        Ordini_Sap scrivi = new Ordini_Sap();
        scrivi.ScriviRDA(this.txtIstRDA.Text);
        //scrivi.ScriviRDA1(this.txtguidRDA.Text);
    }

    protected void btnOrdini_Click(object sender, EventArgs e)
    {
        if (this.txtIstOrdine.Text == "")
        {
            this.lblErrorOrdini.Visible = true;
            this.lblErrorOrdini.Text = "istituto mancante";
            return;
        }

        Ordini_Sap ordini = new Ordini_Sap();
        ordini.ScriviOrdini(this.txtIstOrdine.Text);
    }

    protected void btnOrdStart_Click(object sender, EventArgs e)
    {
        if (this.txtIstOrdStart.Text == "")
        {
            this.lblErrorOrdStart.Visible = true;
            this.lblErrorOrdStart.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskOrdStart.SelectedValue == "")
        {
            this.lblErrorOrdStart.Visible = true;
            this.lblErrorOrdStart.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorOrdStart.Visible = false;

        
        try
        {            
            Ordini_Sap ordini = new Ordini_Sap();
            this.lblErrorOrdStart.Text = ordini.Invia_file_Sap_FirstStart(this.txtIstOrdStart.Text, this.ddlTaskOrdStart.SelectedValue,this.ddlTipoexportORDstart.SelectedValue);            
            this.lblErrorOrdStart.Visible = this.lblErrorOrdStart.Text == "" ? false : true;
            this.lblOrdStartEseguito.Visible = !this.lblErrorOrdStart.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }

    protected void btnOrdSingolo_Click(object sender, EventArgs e)
    {
        if (this.txtIstOrdSingolo.Text == "")
        {
            this.lblErrorOrdSingolo.Visible = true;
            this.lblErrorOrdSingolo.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskOrdSingolo.SelectedValue == "")
        {
            this.lblErrorOrdSingolo.Visible = true;
            this.lblErrorOrdSingolo.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorOrdSingolo.Visible = false;

        try
        {
            Ordini_Sap ordini = new Ordini_Sap();
            this.lblErrorOrdSingolo.Text = ordini.Invia_file_Sap_Onetime(this.txtIstOrdSingolo.Text, this.ddlTaskOrdSingolo.SelectedValue, this.ddlTipoexportOrdsingolo.SelectedValue);
            this.lblErrorOrdSingolo.Visible = this.lblErrorOrdSingolo.Text == "" ? false : true;
            this.lblOrdSingoloEseguito.Visible = !this.lblErrorOrdSingolo.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }

    }

    protected void btnEsitoStart_Click(object sender, EventArgs e)
    {
        if (this.txtIstEsitoStart.Text == "")
        {
            this.lblErrorEsitoStart.Visible = true;
            this.lblErrorEsitoStart.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskEsitoStart.SelectedValue == "")
        {
            this.lblErrorEsitoStart.Visible = true;
            this.lblErrorEsitoStart.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorEsitoStart.Visible = false;

       
        
        try
        {
            Ordini_Sap ordini = new Ordini_Sap();
            this.lblErrorEsitoStart.Text = ordini.Leggi_Esito_Sap_FirstStart(this.txtIstEsitoStart.Text, this.ddlTaskEsitoStart.SelectedValue);
            this.lblErrorEsitoStart.Visible = this.lblErrorEsitoStart.Text == "" ? false : true;
            this.lblEsitoStartEseguito.Visible = !this.lblErrorEsitoStart.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }

    }
     
    protected void btnEsitoSingolo_Click(object sender, EventArgs e)
    {
        if (this.txtIstEsitoSingolo.Text == "")
        {
            this.lblErrorEsitoSingolo.Visible = true;
            this.lblErrorEsitoSingolo.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskEsitoSingolo.SelectedValue == "")
        {
            this.lblErrorEsitoSingolo.Visible = true;
            this.lblErrorEsitoSingolo.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorEsitoSingolo.Visible = false;               
       
        try
        {
            Ordini_Sap ordini = new Ordini_Sap();
            this.lblErrorEsitoSingolo.Text = ordini.Leggi_Esito_Sap_Onetime(this.txtIstEsitoSingolo.Text, this.ddlTaskEsitoSingolo.SelectedValue);
            this.lblErrorEsitoSingolo.Visible = this.lblErrorEsitoSingolo.Text == "" ? false : true;
            this.lblEsitoSingoloEseguito.Visible = !this.lblErrorEsitoSingolo.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }

   
}