using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Esempio di lettura diretta di un'estrazione budget:
        //   var xml = new PollingClass.ImportaXML();
        //   var bdgs = xml.ReadExcelFileDOM(camminoFile);
        // Il percorso va fornito da configurazione, mai scritto nel codice (vincolo C8).
    }

   

    protected void btnLeggiRDA_Click(object sender, EventArgs e)
    {
        //Ordini_Sap ord = new Ordini_Sap();
        //string guid = "12345";
        //string istituto = "47";
        //ord.Esegui(guid, istituto, null);

    }

    protected void btnSelProcesso_Click(object sender, EventArgs e)
    {
        //string valore = this.DropDownListBreed.SelectedItem.Value.ToString();
        //string descri = this.DropDownListBreed.SelectedItem.Text;
    }
}