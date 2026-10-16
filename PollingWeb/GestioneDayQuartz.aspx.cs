using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class GestioneDayQuartz : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            this.AbilitaDisabilita(false, false);
    }
    protected void btnSeleziona_Click(object sender, EventArgs e)
    {
        int indice = ((RepeaterItem)((Control)sender).Parent).ItemIndex;
        this.txtDayQuartz.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("DayQuartz")).InnerText;
        this.txtDescri.Text = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Descrizione")).InnerText; ;
       
    }
    protected void btnModifica_Click(object sender, EventArgs e)
    {
        if (this.txtDayQuartz.Text == "")
            return;

        Session["Flag_GestioneDayQuartz"] = 1;
        this.AbilitaDisabilita(true, false);

    }

    protected void btnUscita_Click(object sender, EventArgs e)
    {

        this.AbilitaDisabilita(false, false);
    }

    protected void btnSalva_Click(object sender, EventArgs e)
    {
        if (!this.ValidaDati())
            return;

        this.AbilitaDisabilita(false, false);

        this.InsertUpdate();
    }

    protected void btnNuovo_Click(object sender, EventArgs e)
    {
        this.AbilitaDisabilita(true, true);
        this.PulisciCampi();
        Session["Flag_GestioneDayQuartz"] = 0;

    }

    protected void btnElimina_Click(object sender, EventArgs e)
    {
        if (this.txtDayQuartz.Text == "")
            return;

        this.AbilitaDisabilita(false, false);
        this.Delete();
        this.PulisciCampi();
    }
    private bool ValidaDati()
    {
        bool esito = true;
        this.lblErrore.Text = "";
        this.lblErrore.Visible = !esito;

        if (this.txtDayQuartz.Text == "")
        {
            this.lblErrore.Text = "codice obbligatorio";
            this.txtDayQuartz.Focus();
            esito = false;
            goto fine;
        }

        if (this.txtDescri.Text == "")
        {
            this.lblErrore.Text = "descrizione obbligatoria";
            this.txtDescri.Focus();
            esito = false;
            goto fine;
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

            if (Session["Flag_GestioneDayQuartz"].ToString() == "0")
            {
                p = new Parameter("DayQuartz", TypeCode.String, this.txtDayQuartz.Text.ToUpper());
                this.sqlDimDayquartz.InsertParameters.Add(p);

                p = new Parameter("Descrizione", TypeCode.String, this.txtDescri.Text);
                this.sqlDimDayquartz.InsertParameters.Add(p);

                this.sqlDimDayquartz.InsertCommand = "Insert into DimDayquartz( " +
                    "DayQuartz," +
                    "Descrizione) " +
                    "Values(" +
                    "@DayQuartz," +
                    "@Descrizione)";


                this.sqlDimDayquartz.Insert();
            }
            else
            {
                p = new Parameter("DayQuartz", TypeCode.String, this.txtDayQuartz.Text.ToUpper());
                this.sqlDimDayquartz.UpdateParameters.Add(p);

                p = new Parameter("Descrizione", TypeCode.String, this.txtDescri.Text);
                this.sqlDimDayquartz.UpdateParameters.Add(p);

                this.sqlDimDayquartz.UpdateCommand = "Update DimDayquartz set " +
                    "DayQuartz=@DayQuartz," +
                    "Descrizione=@Descrizione" +
                    " where DayQuartz=@DayQuartz ";

                this.sqlDimDayquartz.Update();
            }
            
            this.sqlDimDayquartz.Select(new DataSourceSelectArguments());
            this.AbilitaDisabilita(false, false);
            this.PulisciCampi();


        }
        catch (Exception ex)
        {
            //Log_Errori.ScriviLOGPROCEDURE(this.txtDayQuartz.Text, this.ddlIdProcessi.SelectedValue, "TaskService.InsertUpdate " + ex.Message);
            PollingExceptionHandler.Esegui(ex);
        }
    }

    private void Delete()
    {
        try
        {
            Parameter p;
            p = new Parameter("DayQuartz", TypeCode.String, this.txtDayQuartz.Text);
            this.sqlDimDayquartz.DeleteParameters.Add(p);
            this.sqlDimDayquartz.DeleteCommand = "Delete DimDayquartz where DayQuartz=@DayQuartz";
            this.sqlDimDayquartz.Delete();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        this.sqlDimDayquartz.Select(new DataSourceSelectArguments());

    }
    private void AbilitaDisabilita(bool abilita, bool inserimento)
    {
        this.pnl.Enabled = abilita;

        this.btnModifica.Visible = !abilita;
        this.btnElimina.Visible = !abilita;
        this.btnNuovo.Visible = !abilita;

        this.btnSalva.Visible = abilita;
        this.btnUscita.Visible = abilita;

        this.txtDayQuartz.ReadOnly = !inserimento;
        
        this.lblErrore.Visible = false;
    }

    private void PulisciCampi()
    {
        this.txtDayQuartz.Text = "";
        this.txtDescri.Text = "";        
    }
}