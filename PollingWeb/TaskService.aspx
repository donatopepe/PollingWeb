<%@ Page Title="" Language="C#" MasterPageFile="~/Polling_web.master" AutoEventWireup="true"  CodeFile="TaskService.aspx.cs" Inherits="TaskService" %>




<asp:Content ID="Content1" ContentPlaceHolderID="menuUtente" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" Runat="Server">
     <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel runat="server" ID="updTaskService">        
        <ContentTemplate>
            <div class="grid_3 grid_5">
            <asp:Panel runat="server" ID="pnl" Enabled="false">
                <div class="col-md-1">Istituto</div>
                <div class="col-md-2">ID Processo</div>
                <div class="col-md-3">Day Check</div> 
                <div class="col-md-1">Ora Check</div>              
                
                <div class="col-md-1">Intervallo</div>                           
                <div class="col-md-4">Cutoff</div>     
                <div class="clearfix"></div> 


                <div class="col-md-1"><asp:TextBox runat="server" ID="txtIstituto" Width="50" ReadOnly="true" TextMode="Number" ToolTip="Valore numerico" ></asp:TextBox></div>
                <div class="col-md-2">
                    <%--<asp:TextBox runat="server" ID="txtIdProcesso" MaxLength="20" placeholder="idprocesso"></asp:TextBox>--%>
                    <asp:DropDownList ID="ddlIdProcessi" runat="server" CssClass="text-success" Width="100%" DataSourceID="sqlDimProcessi" DataTextField="Descrizione" DataValueField="IdProcesso" ></asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <%--<asp:TextBox runat="server" ID="txtDay_Check_File" Width="80" MaxLength="10" placeholder="MON-FRI"></asp:TextBox>--%>
                    <asp:DropDownList ID="ddlDayQuartz" runat="server" CssClass="text-success" Width="100%" DataSourceID="sqlDimDayquartz" DataTextField="Descrizione" DataValueField="DayQuartz" ></asp:DropDownList>

                </div>
                
                <div class="col-md-1"><asp:TextBox runat="server" ID="txtOrario_Check_File" Width="70" MaxLength="5" placeholder="hh:mm"></asp:TextBox></div>
                <div class="col-md-1"><asp:TextBox runat="server" ID="txtPolling_Intervall" Width="50" TextMode="Number" MaxLength="2" ></asp:TextBox></div>
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtCutoff" Width="50" TextMode="Number" MaxLength="3" ></asp:TextBox></div>
 
                <div class="clearfix"></div> 


                <div class="col-md-4">Path Source</div>
                <div class="col-md-3">File Source</div>
                <div class="col-md-1">No Move</div>
                <div class="col-md-4">Path Archiviazione</div> 
                <div class="clearfix"></div>    
               
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtPath_Source" MaxLength="100" Width="380" placeholder="Path source"></asp:TextBox></div>
                <div class="col-md-3"><asp:TextBox runat="server" ID="txtFile_Source" MaxLength="50" Width="250" placeholder="file source"></asp:TextBox></div>              
                <div class="col-md-1"><asp:CheckBox runat="server" ID="chkLasciacopia" MaxLength="50" Width="250"></asp:CheckBox></div>              
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtPath_Archiviazione" MaxLength="50" Width="380"></asp:TextBox></div>
                <div class="clearfix"></div> 


                <div class="col-md-4">Mail Mittente</div>     
                <div class="col-md-4">Mail Failure</div>     
                <div class="col-md-4">Mail Success</div>  
                <div class="clearfix"></div> 
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtMail_Mittente" MaxLength="50" TextMode="Email" ValidateRequestMode="Enabled" ></asp:TextBox></div>
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtMail_Failure" MaxLength="250" ></asp:TextBox></div>
                <div class="col-md-4"><asp:TextBox runat="server" ID="txtMail_success" MaxLength="250" ></asp:TextBox></div>
                <div class="clearfix"></div> 

                <div class="col-md-12"><asp:TextBox runat="server" ID="txtTipoTask" Visible="false"></asp:TextBox></div>
 
                <br />
            </asp:Panel>
                               
                <div class="col-md-2"><asp:Button runat="server" CssClass="button_seleziona" ID="btnModifica" Text="Modifica"  OnClick="btnModifica_Click" /></div>
                <div class="col-md-1"><asp:Button runat="server" CssClass="button_seleziona" ID="btnNuovo" Text="Nuovo"  OnClick="btnNuovo_Click" /></div>
                <div class="col-md-1"><asp:Button runat="server" CssClass="button_seleziona" ID="btnSalva" Text="Salva"  OnClick="btnSalva_Click" /></div>
                <div class="col-md-3"><asp:Label runat="server" CssClass="label label-danger" ID="lblErrore" Text="errore" Visible="false"    /></div>
                <div class="col-md-2"><asp:Button runat="server" CssClass="button_cancella" ID="btnUscita" Text="Uscita" OnClick="btnUscita_Click" /></div>
                <div class="col-md-3"><asp:Button runat="server" CssClass="button_cancella" ID="btnElimina" Text="Elimina" OnClick="btnElimina_Click" /></div>
                 
            
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Istituto</th>                        
                        <th>ID Processo</th>
                        <th>Descrizione</th>
                        
                        <th>File</th>
                        <th>Ora CHK</th>
                        <th>Day CHK</th>
                        <th>Polling</th>
                        <th>Cutoff</th>
                   

                        <th>Seleziona</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rigaTabella" DataSourceID="sqlTaskService">
                        <ItemTemplate>
                            <tr style="font-size:14px">
                                <td scope="row" runat="server" visible="false" id="TipoTask"><%# Eval("Tipo") %></td>
                                <td scope="row" runat="server" id="Istituto"><%# Eval("Istituto") %></td>
                                <td scope="row" runat="server" id="IdProcesso"><%# Eval("IdProcesso") %></td>
                                <td scope="row" runat="server" id="Descrizione"><%# Eval("Desprocesso") %></td>
                                <td scope="row" runat="server" visible="false" id="Path_Source"><%# Eval("Path_Source") %></td>
                                <td scope="row" runat="server" id="File_Source"><%# Eval("File_Source") %></td>

                                <td scope="row" runat="server" id="Orario_Check_File"><%# Eval("Orario_Check_File", "{0:HH:mm}") %></td>

                                <td scope="row" runat="server" id="Day_Check_File"><%# Eval("Day_Check_File") %></td>
                                <td scope="row" runat="server" id="Polling_Intervall"><%# Eval("Polling_Intervall") %></td>
                                <td scope="row" runat="server" id="Cutoff"><%# Eval("Cutoff") %></td>
                                <td scope="row" runat="server" visible="false" id="Mail_Mittente"><%# Eval("Mail_Mittente") %></td>
                                <td scope="row" runat="server" visible="false" id="Mail_Failure"><%# Eval("Mail_Failure") %></td>
                                <td scope="row" runat="server" visible="false" id="Path_Archiviazione"><%# Eval("Path_Archiviazione") %></td>
                                <td scope="row" runat="server" visible="false" id="Mail_success"><%# Eval("Mail_success") %></td>
                                <td scope="row" runat="server" visible="false" id="LasciaInSource"><%# Eval("LasciaInSource") %></td>

                                <td><asp:Button runat="server" ID="btnSeleziona" Text="Seleziona" CssClass="button_seleziona" OnClick="btnSeleziona_Click" /> </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>

            </table>
</div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:SqlDataSource ID="sqlTaskService" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" 
        SelectCommand="Select * from ViewTaskService" >
        
    </asp:SqlDataSource>
     <asp:SqlDataSource ID="sqlDimProcessi" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" SelectCommand="SELECT * from DimProcessi">
    </asp:SqlDataSource>
     <asp:SqlDataSource ID="sqlDimDayquartz" runat="server" ConnectionString="<%$ ConnectionStrings:PollingConnectionString %>" SelectCommand="SELECT * from DimDayquartz">
    </asp:SqlDataSource>
</asp:Content>

