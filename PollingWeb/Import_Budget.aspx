<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="Import_Budget.aspx.cs" Inherits="Import_Budget" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
    <div class="grid_3 grid_5">
        
        <%--esito sap start--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblBudgetStart" Text="Budget Start"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstBudgetStart" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskBudgetStart" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService_budget" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnBudgetStart" CssClass="button_seleziona" Text="Seleziona"  OnClick="btnBudgetStart_Click" ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorBudgetStart" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoBudgetStart" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblBudgetStartEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 
          <br />

        <%--esito sap singola--%>
        <div class="col-md-3">
            <asp:Label runat="server" ID ="lblBudgetSingolo" Text="Budget Singola"></asp:Label>                                                                 
        </div>
        <div class="col-md-1">
            <asp:TextBox runat="server" ID ="txtIstBudgetSingolo" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
        </div>
        <div class="col-md-2">
            <asp:DropDownList ID="ddlTaskBudgetSingolo" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService_budget" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>  
        </div>
                  
        <div class="col-md-1">
            <asp:Button runat="server" ID ="btnBudgetSingolo" CssClass="button_seleziona" Text="Seleziona" OnClick="btnBudgetSingolo_Click"  ></asp:Button>       
        </div>
        <div class="col-md-5">
            <asp:Label runat="server" ID="lblErrorBudgetSingolo" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblAvviatoBudgetSingolo" CssClass="label label-success" Text="servizio avviato" Visible="false"></asp:Label>
            <asp:Label runat="server" ID="lblEsitoBudgetEseguito" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
        </div>

        <div class="clearfix"></div> 

        </div>

  
    <asp:SqlDataSource ID="sqlTaskService_budget" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="select IdProcesso from ViewTaskService where Tipo=4 group by IdProcesso" >
    </asp:SqlDataSource>
</asp:Content>

