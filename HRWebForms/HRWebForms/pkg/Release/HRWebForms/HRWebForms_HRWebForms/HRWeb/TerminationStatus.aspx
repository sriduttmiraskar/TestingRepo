<%@ Assembly Name="HRWebForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c8c0e2f713937cc8" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="TerminationStatus.aspx.cs" Inherits="HRWebForms.HRWeb.TerminationStatus" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Termination Status</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <!DOCTYPE html>
    <html lang="en">
   
    <body class="bodyBg">
        <div id='hr-web' class="clearfix">
            <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </div>
        <div class="clearfix">&nbsp;</div>
            <br />
            <br />
        <br />
        <br />
    </body>
    </html>
</asp:Content>
