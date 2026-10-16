using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using System.Data;

public partial class VisualizzaLogTaskService : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            this.txtData.Text = DateTime.Now.ToShortDateString();
        }
    }

    protected void btnFiltra_Click(object sender, EventArgs e)
    {
        //this.sqlLogTaskServices.SelectParameters["Data"].DefaultValue = this.txtData.Text;
        this.rigaTabella.DataBind();    
        //this.sqlLogTaskServices.Select(new DataSourceSelectArguments());
    }

    protected void btnSeleziona_Click(object sender, EventArgs e)
    {
        try
        {
            Parameter p;

            int indice = ((RepeaterItem)((Control)sender).Parent).ItemIndex;
            
            string istituto = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Istituto")).InnerText;
            string data = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("Data")).InnerText;
            string idprocesso = ((HtmlContainerControl)this.rigaTabella.Items[indice].FindControl("IdProcesso")).InnerText;


            p = new Parameter("Istituto", TypeCode.Int16, istituto);
            this.sqlLogTaskServices.DeleteParameters.Add(p);

            p = new Parameter("Data", TypeCode.DateTime, data);
            this.sqlLogTaskServices.DeleteParameters.Add(p);

            p = new Parameter("IdProcesso", TypeCode.String, idprocesso);
            this.sqlLogTaskServices.DeleteParameters.Add(p);


            this.sqlLogTaskServices.DeleteCommand = "Delete LogTaskService where Istituto=@Istituto and Data=@Data and IdProcesso=@IdProcesso";
            this.sqlLogTaskServices.Delete();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
}