<%@ Assembly Name="HRWebForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c8c0e2f713937cc8" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadJobUserControl.ascx.cs" Inherits="HRWebForms.ControlTemplates.HRWebForms.UploadJobUserControl" %>
<div class="control-group">
    <asp:Label ID="lblRoleStatement" runat="server" class="controlrole-label">Contract Deliverables/Role Statement <span style="color:red">*</span></asp:Label>

    <div>
        <p><b>Note: </b>Please do not use these characters in file name ~, #, %, &, *, {, }, \, :, <, >, ?, /, +, |, ". Do not use '_' at the beginning of file name.</p>
        <asp:FileUpload ID="HrWebFileUpload" runat="server" EnableViewState="true"  CssClass="fileupload" /><asp:Button ID="btnUpload" Text="Add" CssClass="button_small" OnClick="btnAdd_Click" runat="server" />
        <asp:Button ID="btnDelete" Text="Delete" OnClick="btnDelete_Click" runat="server" CssClass="button_small" />
    </div>
</div>
<div class="control-group">
    <div name="" class="mainframe span12 scroll" style="width:97% !important">
        <div id='Div1' class="clearfix">
            <div id="Div2" class="row-fluid" runat="server">
                <asp:Table ID="tblAttachment" runat="server" CssClass="span4" Width="97%" EnableViewState="true">
                    <asp:TableHeaderRow ID="TableHeaderRow1" runat="server" Width="100%">
                        <asp:TableHeaderCell ID="TableHeaderCell1" runat="server" Width="5%" HorizontalAlign="Left"></asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="TableHeaderCell2" runat="server" Width="15%" HorizontalAlign="Left">File Type</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="TableHeaderCell3" runat="server" Width="80%" HorizontalAlign="Left">Name</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="TableHeaderCell4" runat="server" Width="5%" HorizontalAlign="Left">Date</asp:TableHeaderCell>
                        <asp:TableHeaderCell ID="TableHeaderCell5" runat="server" Visible="false"  HorizontalAlign="Left">RefNo</asp:TableHeaderCell>
                    </asp:TableHeaderRow>

                </asp:Table>
            </div>

        </div>

    </div>
</div>
