<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="GestioneDayQuartz.aspx.cs" Inherits="GestioneDayQuartz" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
         <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel runat="server" ID="updDayQuartz">        
        <ContentTemplate>
            <div class="grid_3 grid_5">
            <asp:Panel runat="server" ID="pnl" Enabled="false">
                <div class="col-md-2">Codice</div>
                <div class="col-md-3">Descrizione</div>
                <div class="col-md-7"></div>
                <div class="clearfix"></div>   
                
                <div class="col-md-2"><asp:TextBox runat="server" ID="txtDayQuartz" Width="100%"  ToolTip="es. MON-FRI" ></asp:TextBox></div>
                <div class="col-md-3"><asp:TextBox runat="server" ID="txtDescri" Width="100%"  ToolTip="descrizione giorni" ></asp:TextBox></div>
                <div class="col-md-7"></div>
                <div class="clearfix"></div>
            </asp:Panel>
                <hr />
                               
                <div class="col-md-2"><asp:Button runat="server" CssClass="button_seleziona" ID="btnModifica" Text="Modifica"  OnClick="btnModifica_Click" /></div>
                <div class="col-md-1"><asp:Button runat="server" CssClass="button_seleziona" ID="btnNuovo" Text="Nuovo"  OnClick="btnNuovo_Click" /></div>
                <div class="col-md-1"><asp:Button runat="server" CssClass="button_seleziona" ID="btnSalva" Text="Salva"  OnClick="btnSalva_Click"/></div>
                <div class="col-md-3"><asp:Label runat="server" CssClass="label label-danger" ID="lblErrore" Text="errore" Visible="false"    /></div>
                <div class="col-md-2"><asp:Button runat="server" CssClass="button_cancella" ID="btnUscita" Text="Uscita"  OnClick="btnUscita_Click"/></div>
                <div class="col-md-3"><asp:Button runat="server" CssClass="button_cancella" ID="btnElimina" Text="Elimina" OnClick="btnElimina_Click"  /></div>
                 
            
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Codice</th>        
                        <th>Descrizione</th>

                        <th>Seleziona</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rigaTabella" DataSourceID="sqlDimDayquartz">
                        <ItemTemplate>
                            <tr>
                                <td scope="row" runat="server"  id="DayQuartz"><%# Eval("DayQuartz") %></td>                             
                                <td scope="row" runat="server" id="Descrizione"><%# Eval("Descrizione") %></td>                                

                                <td><asp:Button runat="server" ID="btnSeleziona" Text="Seleziona" CssClass="button_seleziona"  OnClick="btnSeleziona_Click"/> </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>

            </table>
</div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:SqlDataSource ID="sqlDimDayquartz" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="Select * from DimDayquartz" ></asp:SqlDataSource>
</asp:Content>

