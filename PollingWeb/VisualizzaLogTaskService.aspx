<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="VisualizzaLogTaskService.aspx.cs" Inherits="VisualizzaLogTaskService" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="main" Runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel runat="server" ID="updLog">        
        <ContentTemplate>
            <div class="grid_3 grid_5">
            
            <div class="col-md-1">Data</div>
            <div class="col-md-2">
                <asp:TextBox runat="server" ID="txtData" placeholder="gg/mm/yyyy" Text=""></asp:TextBox>
            </div>
            <div class="col-md-9"><asp:Button runat="server" CssClass="button_seleziona" ID="btnFiltra" Text="Filtra" OnClick="btnFiltra_Click"  /></div>

            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Istituto</th>
                        <th>Data</th>
                        <th>ID Processo</th>
                        <th>Stato</th>
                        <th>Inzio</th>
                        <th>Fine</th>
                        <th>Messaggio aggiuntivo</th>
                        
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rigaTabella" DataSourceID="sqlLogTaskServices" >
                        <ItemTemplate>
                            <tr style="font-size:14px">
                                <td scope="row" runat="server" id="Istituto"><%# Eval("Istituto") %></td>
                                <td scope="row" runat="server" id="Data"><%# Eval("Data","{0:dd/M/yyyy}") %></td>
                                <td scope="row" runat="server" id="IdProcesso"><%# Eval("IdProcesso") %></td>
                                <td scope="row" runat="server" id="Codice_stato"><%# Eval("Codice_stato") %></td>
                                <td scope="row" runat="server" id="Ora_Inizio"><%# Eval("Ora_Inizio", "{0:HH:mm}") %></td>
                                <td scope="row" runat="server" id="Ora_fine"><%# Eval("Ora_fine","{0:HH:mm}") %></td>
                                <td scope="row" runat="server" id="Messaggio_Return"><%# Eval("Messaggio_Return") %></td>
                                <td><asp:Button runat="server" ID="btnSeleziona" Text="Elimina" Visible="false" CssClass="button_cancella"  OnClick="btnSeleziona_Click"/> </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>

            </table>
            </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:SqlDataSource ID="sqlLogTaskServices" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="Select * from LogTaskService where Data=@Data" >
        <SelectParameters>
            <asp:ControlParameter ControlID="txtData" DbType="Date" Name="Data" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>

