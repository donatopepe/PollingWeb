<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="SQL01.aspx.cs" Inherits="SQL01" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
       <div class="grid_3 grid_5">
        
        <%--esito sap start--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblSQLtStart" Text="SQL01 Start"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstSQLStart" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskSQLStart" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService_SQL" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnSQLStart" CssClass="button_seleziona" Text="Seleziona"  OnClick="btnSQLStart_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorSQLStart" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoSQLStart" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblSQLStartEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 
          <br />

        <%--esito sap singola--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblSQLSingolo" Text="SQL Singola"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstSQLSingolo" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskSQLSingolo" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService_SQL" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnSQLSingolo" CssClass="button_seleziona" Text="Seleziona" OnClick="btnSQLSingolo_Click"  ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorSQLSingolo" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoSQLSingolo" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblEsitoSQLEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 

        </div>

  
    <asp:SqlDataSource ID="sqlTaskService_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="select IdProcesso from ViewTaskService where Tipo=5 group by IdProcesso" >
    </asp:SqlDataSource>
</asp:Content>

