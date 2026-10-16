<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
    <div class="grid_3">
	<div class="col-md-2">LEGGI RDA</div>
    <div class="col-md-2"></div>
    <div class="col-md-2"></div>
    <div class="col-md-6"></div>
            
    <div class="clearfix"></div>

    <div class="col-md-2"><asp:Button runat="server" ID="btnLeggiRDA" Text="seleziona" CssClass="button_seleziona" OnClick="btnLeggiRDA_Click"  /></div>

    <div class="col-md-2"><asp:Button runat="server" ID="btnTaskservice" Text="seleziona" CssClass="button_seleziona"  /></div>
    <div class="col-md-6">
    <asp:DropDownList ID="ddlIdProcessi" runat="server" Height="20px" Width="110px" DataSourceID="sqlDimProcessi" DataTextField="Descrizione" DataValueField="IdProcesso">
    </asp:DropDownList>
            <div class="col-md-2"><asp:Button runat="server" ID="btnSelProcesso" Text="seleziona" CssClass="button_seleziona" OnClick="btnSelProcesso_Click" /></div>
    </div>
</div>
    <asp:TextBox
id="Email"
runat="server">
</asp:TextBox>

<asp:RequiredFieldValidator id="reqEmail"
runat="server"
controltovalidate="Email"
display="None"
errormessage="Inserisci un indirizzo email">
</asp:RequiredFieldValidator>

<asp:RegularExpressionValidator
id="regExEmail"
runat="server"
display="None"
errormessage="inserisci un indirizzo email valido" controltovalidate="Email"
validationexpression="w+([-+.]w+)*@w+([-.]w+)*.w+([-.]w+)*"></asp:RegularExpressionValidator>

    <asp:SqlDataSource ID="sqlDimProcessi" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" SelectCommand="SELECT * from DimProcessi">
           
</asp:SqlDataSource>
</asp:Content>

