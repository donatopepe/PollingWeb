using Database;
using PollingClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SQL01 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnSQLStart_Click(object sender, EventArgs e)
    {
        if (this.txtIstSQLStart.Text == "")
        {
            this.lblErrorSQLStart.Visible = true;
            this.lblErrorSQLStart.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskSQLStart.SelectedValue == "")
        {
            this.lblErrorSQLStart.Visible = true;
            this.lblErrorSQLStart.Text = "idprocesso mancante";
            return;
        }
        this.lblErrorSQLStart.Visible = false;



        try
        {
            ExportSQL01 task = new ExportSQL01();
            this.lblErrorSQLStart.Text = task.SQL01_FirstStart(this.txtIstSQLStart.Text, this.ddlTaskSQLStart.SelectedValue);
            this.lblErrorSQLStart.Visible = this.lblErrorSQLStart.Text == "" ? false : true;
            this.lblSQLStartEseguito.Visible = !this.lblErrorSQLStart.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }

    protected void btnSQLSingolo_Click(object sender, EventArgs e)
    {
        if (this.txtIstSQLSingolo.Text == "")
        {
            this.lblErrorSQLSingolo.Visible = true;
            this.lblErrorSQLSingolo.Text = "istituto mancante";
            return;
        }
        if (this.ddlTaskSQLSingolo.SelectedValue == "")
        {
            this.lblErrorSQLSingolo.Visible = true;
            this.lblErrorSQLSingolo.Text = "idprocesso mancante";
            return;
        }


        try
        {
            ExportSQL01 task = new ExportSQL01();
            this.lblErrorSQLSingolo.Text = task.SQL01_Onetime(this.txtIstSQLSingolo.Text, this.ddlTaskSQLSingolo.SelectedValue);
            this.lblErrorSQLSingolo.Visible = this.lblErrorSQLSingolo.Text == "" ? false : true;
            this.lblEsitoSQLEseguito.Visible = !this.lblErrorSQLSingolo.Visible;
        }
        catch (Exception ex)
        {
            PollingExceptionHandler.Esegui(ex);
        }
    }
}