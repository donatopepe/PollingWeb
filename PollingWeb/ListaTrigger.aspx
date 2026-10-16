<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true" CodeFile="ListaTrigger.aspx.cs" Inherits="ListaTrigger" %>

<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
         <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel runat="server" ID="updTaskService">        
        <ContentTemplate>
        <div class="grid_3 grid_5">
            
           <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Nome</th>                        
                        <th>Prossimo Start</th>
                        <th>Ultimo Start</th>                       
                        
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rigaTabella" DataSourceID="sqlListaTrigger">
                        <ItemTemplate>
                            <tr style="font-size:14px">
                                <td scope="row" runat="server" id="Nome"><%# Eval("Trigger_nome") %></td>
                                <td scope="row" runat="server" id="Next"><%# Eval("Trigger_next") %></td>
                                <td scope="row" runat="server" id="Prev"><%# Eval("Trigger_prev") %></td></tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>

            </table>
        </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:SqlDataSource ID="sqlListaTrigger" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="Select * from ListaTriggerTemp" ></asp:SqlDataSource>
        
    
</asp:Content>

