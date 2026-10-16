using System;
using Database;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PollingClass;

public partial class Import_Budget : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnBudgetStart_Click(object sender, EventArgs e)
    {
        if (this.txtIstBudgetStart.Text == "")
        {
            this.lblErrorBudgetStart.Visible = true;
            this.lblErrorBudgetStart.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskBudgetStart.SelectedValue == "")
        {
            this.lblErrorBudgetStart.Visible = true;
            this.lblErrorBudgetStart.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorBudgetStart.Visible = false;



        try
        {
            ImportBudget task = new ImportBudget();
            this.lblErrorBudgetStart.Text = task.ImportaBudget_FirstStart(this.txtIstBudgetStart.Text, this.ddlTaskBudgetStart.SelectedValue);
            this.lblErrorBudgetStart.Visible = this.lblErrorBudgetStart.Text == "" ? false : true;
            this.lblBudgetStartEseguito.Visible = !this.lblErrorBudgetStart.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }

    protected void btnBudgetSingolo_Click(object sender, EventArgs e)
    {
        if (this.txtIstBudgetSingolo.Text == "")
        {
            this.lblErrorBudgetSingolo.Visible = true;
            this.lblErrorBudgetSingolo.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskBudgetSingolo.SelectedValue == "")
        {
            this.lblErrorBudgetSingolo.Visible = true;
            this.lblErrorBudgetSingolo.Text = "idprocesso mancante";
            return;
        }


        try
        {
            ImportBudget task = new ImportBudget();
            this.lblErrorBudgetSingolo.Text = task.ImportaBudget_Onetime(this.txtIstBudgetSingolo.Text, this.ddlTaskBudgetSingolo.SelectedValue);
            this.lblErrorBudgetSingolo.Visible = this.lblErrorBudgetSingolo.Text == "" ? false : true;
            this.lblEsitoBudgetEseguito.Visible = !this.lblErrorBudgetSingolo.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }
}