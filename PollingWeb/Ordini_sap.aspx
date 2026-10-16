<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="Ordini_sap.aspx.cs" Inherits="Ordini_sap" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
    <div class="grid_3 grid_5">
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblScriviRDA" Text="Scrivi RDA"></asp:Label>           
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstRDA" CssClass="form-control" placeholder="istituto"></asp:TextBox>  
        </div> 
        <div class="col-md-2">
        </div>
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnRDA" CssClass="button_seleziona" Text="Seleziona"  OnClick="btnRDA_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorRDA" CssClass="label label-danger" Text="codice  mancante" Visible="false"></asp:Label>
            
            <asp:Label runat="server" ID="lblRDAeseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>
        
        <div class="clearfix"></div>
        <br />
        
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblScriviOrdine" Text="Scrivi Ordine"></asp:Label>           
        </div>
        <div class="col-md-1">
            
            <asp:TextBox runat="server" ID ="txtIstOrdine" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>      
         <div class="col-md-2">
        </div>
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnOrdini" CssClass="button_seleziona" Text="Seleziona" OnClick="btnOrdini_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorOrdini" CssClass="label label-danger" Text="codice  mancante" Visible="false"></asp:Label>
            
            <asp:Label runat="server" ID="lblOrdeseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div>
        <br />

        <%--invio ordine/rda sap start--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblInviaStart" Text="Invia File Sap Start"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstOrdStart" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">            
            <asp:DropDownList ID="ddlTaskOrdStart" runat="server"  CssClass="form-control" Width="180px" DataSourceID="sqlTaskService_ord" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
        <div class="col-md-1">            
            <asp:DropDownList ID="ddlTipoexportORDstart" runat="server"  CssClass="form-control" Width="80px" >
                <asp:ListItem Selected="True" Text="RDA" Value="RDA"></asp:ListItem>
                <asp:ListItem Selected="false" Text="ODA" Value="ODA"></asp:ListItem>
            </asp:DropDownList>              
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnOrdStart" CssClass="button_seleziona" Text="Seleziona" OnClick="btnOrdStart_Click" ></asp:Button>       
        </div>
        <div class="col-md-4">
            <asp:Label runat="server" ID="lblErrorOrdStart" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoOrdStart" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblOrdStartEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 

        <%--invio ordine sap singola--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblInviaSingolo" Text="Invia File Sap Singola"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstOrdSingolo" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskOrdSingolo" runat="server"  CssClass="form-control" Width="180px" DataSourceID="sqlTaskService_ord" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
        <div class="col-md-1">            
            <asp:DropDownList ID="ddlTipoexportOrdsingolo" runat="server"  CssClass="form-control" Width="80px" >
                <asp:ListItem Selected="True" Text="RDA" Value="RDA"></asp:ListItem>
                <asp:ListItem Selected="false" Text="ODA" Value="ODA"></asp:ListItem>
            </asp:DropDownList>              
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnOrdSingolo" CssClass="button_seleziona" Text="Seleziona" OnClick="btnOrdSingolo_Click" ></asp:Button>       
        </div>
        <div class="col-md-4">
            <asp:Label runat="server" ID="lblErrorOrdSingolo" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoOrdSingolo" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblOrdSingoloEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 
        <br />

        <%--esito sap start--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblEsitoStart" Text="Esito Ordini Start"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstEsitoStart" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskEsitoStart" runat="server"  CssClass="form-control" Width="180px" DataSourceID="sqlTaskService_esiti" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
          
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnEsitoStart" CssClass="button_seleziona" Text="Seleziona" OnClick="btnEsitoStart_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorEsitoStart" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoEsitoStart" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblEsitoStartEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 
         

        <%--esito sap singola--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblEsitoSingolo" Text="Esito Ordini Singola"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstEsitoSingolo" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskEsitoSingolo" runat="server"  CssClass="form-control" Width="180px" DataSourceID="sqlTaskService_esiti" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnEsitoSingolo" CssClass="button_seleziona" Text="Seleziona" OnClick="btnEsitoSingolo_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorEsitoSingolo" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoEsitoSingolo" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblEsitoSingoloEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div>  

        </div>

    <asp:SqlDataSource ID="sqlTaskService_ord" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="select IdProcesso from DimProcessi where Tipo in (2,6) group by IdProcesso" >
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlTaskService_esiti" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="select IdProcesso from DimProcessi where Tipo=3 group by IdProcesso" >
    </asp:SqlDataSource>
</asp:Content>

