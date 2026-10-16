<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="Import_Fatture.aspx.cs" Inherits="Import_Fatture" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="main" Runat="Server">
        <div class="grid_3 grid_5">
             
                            <div class="col-md-3">
                                <asp:Label runat="server" ID ="lblStartForgg" Text="Start Import Fatture RDA giornaliera"></asp:Label>           
                            </div>
                            <div class="col-md-1">
                                <asp:TextBox runat="server" ID ="txbStartForgg" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
                            </div>
                            <div class="col-md-2">
                                <asp:DropDownList ID="ddlIdProcessiStart" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>
                            </div>
                            <div class="col-md-1">
                                <asp:Button runat="server" ID ="btnStartForgg" CssClass="button_seleziona" Text="Seleziona" OnClick="btnStartForgg_Click"></asp:Button>       
                            </div>
                            <div class="col-md-5">
                                <asp:Label runat="server" ID="lblErrorStartForgg" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
                                 <asp:Label runat="server" ID="lblStartgg" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
                            </div>

                            <div class="clearfix"></div>
                                <br />

                             <div class="col-md-3">
                                <asp:Label runat="server" ID ="lblStartForsingola" Text="Start Import Fatture RDA Singola"></asp:Label>  
                                                               
                            </div>
                            <div class="col-md-1">
                                <asp:TextBox runat="server" ID ="txbStartForsingola" CssClass="form-control" placeholder="istituto"></asp:TextBox>       
                            </div>
                            <div class="col-md-2">
                                <asp:DropDownList ID="ddlIdProcessiSingola" runat="server"  CssClass="form-control" Width="150px" DataSourceID="sqlTaskService" DataTextField="IdProcesso" DataValueField="IdProcesso"></asp:DropDownList>
                            </div>
                  
                            <div class="col-md-1">
                                <asp:Button runat="server" ID ="btnStartForsingola" CssClass="button_seleziona" Text="Seleziona" OnClick="btnStartForsingola_Click"></asp:Button>       
                            </div>
                            <div class="col-md-5">
                                <asp:Label runat="server" ID="lblErrorStartForsingola" CssClass="label label-danger" Text="codice istituto mancante" Visible="false"></asp:Label>
                                 <asp:Label runat="server" ID="lblStartsingola" CssClass="label label-info" Text="operazione eseguita" Visible="false"></asp:Label>
                            </div>

                            <div class="clearfix"></div> 
        </div>
    <asp:SqlDataSource ID="sqlTaskService" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="select IdProcesso from ViewTaskService where Tipo=7 group by IdProcesso" >
    </asp:SqlDataSource>

    </asp:Content>

