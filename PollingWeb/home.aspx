<%@ Page Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true"  CodeFile="home.aspx.cs" Inherits="home" %>


<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">

</asp:Content>
       

<asp:Content ID="Content3" ContentPlaceHolderID="main" Runat="Server">
        
<div class="grid_3">
    <div class="col-md-12 label label-default">Tabelle di Gestione</div>
    <div class="col-md-2">TaskService</div>
    <div class="col-md-2">Day Quartz</div>
    <div class="col-md-2">Visualizza Log</div>
    <div class="col-md-6"></div>
    <div class="clearfix"></div>

    <div class="col-md-2"><asp:Button runat="server" ID="btnTaskservice" Text="seleziona" CssClass="button_seleziona" OnClick="btnTaskservice_Click" /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnDayQuartz" Text="seleziona" CssClass="button_seleziona" OnClick="btnDayQuartz_Click"  /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnVislog" Text="seleziona" CssClass="button_seleziona"  OnClick="btnVislog_Click"/></div>
       
    <div class="clearfix"></div>
    <hr />

    <div class="col-md-12 label label-default">Servizi</div>

	<div class="col-md-2">Lettura Fornitori</div>
    <div class="col-md-2">Importa Fatture</div>
    <div class="col-md-2">RDA/ODA</div>
    <div class="col-md-2">Importa Budget</div>
    <div class="col-md-2">SQL01</div>
    <div class="col-md-2"></div>
    <div class="col-md-2"></div>    

    <div class="clearfix"></div>

    <div class="col-md-2"><asp:Button runat="server" ID="btnLetturafor" Text="seleziona" CssClass="button_seleziona" OnClick="btnLetturafor_Click" /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnFatture" Text="seleziona" CssClass="button_seleziona" OnClick="btnFatture_Click" /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnOrdinisap" Text="seleziona" CssClass="button_seleziona" OnClick="btnOrdinisap_Click" /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnBudget" Text="seleziona" CssClass="button_seleziona" OnClick="btnBudget_Click" /></div>
    <div class="col-md-2"><asp:Button runat="server" ID="btnSql01" Text="seleziona" CssClass="button_seleziona" OnClick="btnSql01_Click" /></div>
    <div class="col-md-2"></div>
    <div class="col-md-2"></div>

    <div class="clearfix"></div>
    <hr />
    <div class="col-md-2"><asp:Button runat="server" ID="btnTEST" Text="Lista Trigger" CssClass="button_seleziona" OnClick="btnTEST_Click" /></div>
</div>
</asp:Content>

    
     



