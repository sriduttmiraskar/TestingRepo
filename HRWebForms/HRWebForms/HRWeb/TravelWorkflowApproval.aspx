<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="TravelWorkflowApproval.aspx.cs" Inherits="HRWebForms.HRWeb.TravelWorkflowApproval" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Travel Workflow Approval</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta http-equiv="X-UA-Compatible" content="IE=edge" />
        <meta http-equiv="content-type" content="text/html" charset="UTF-8" />
        <meta name="HandheldFriendly" content="True" />
        <meta name="MobileOptimized" content="320" />
        <meta name="viewport" content="width=device-width,initial-scale = 1,maximum-scale=1.0" />
        <meta http-equiv="cleartype" content="on" />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <title></title>

    </head>
    <body class="bodyBg">
        <div class="container portfolio-item">
            <div id='hr-web' class="clearfix">
                <div>
                    <span style="color: red">
                        <asp:Label ID="WorkFlowlblError" runat="server"></asp:Label>
                    </span>
                </div>
                <div>
                    <h2 class="span6">Workflow Approval Summary</h2>
                </div>
                <div style="margin-top: -25px;">
                    <h5 class="span6">To access your forms, simply click on Form Number.</h5>
                </div>
                <div>
                    <h3 class="span6">Drafts</h3>
                </div>
                <asp:GridView ID="DraftGrid" runat="server" AutoGenerateColumns="false" Width="100%" AllowSorting="true"
                    CssClass="EU_DataTable" OnSorting="DraftGrid_Sorting">
                    <Columns>
                        <asp:BoundField DataField="TypeOfTravel" HeaderText="Type Of Travel" ReadOnly="true" SortExpression="TypeOfTravel">
                            <HeaderStyle Width="15%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TravellerName" HeaderText="Traveller Name" ReadOnly="True" SortExpression="TravellerName">
                            <HeaderStyle Width="20%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" VerticalAlign="Top" />
                        </asp:BoundField>

                        <asp:TemplateField HeaderText="FormNo" SortExpression="FormNo">
                            <ItemTemplate>
                                <asp:Label ID="linkFormNo" runat="server" Text='<%# Bind("FormNo")  %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <HeaderStyle Width="15%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" Wrap="true" VerticalAlign="Top" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="BusinessUnit" HeaderText="Business Unit" ReadOnly="True" SortExpression="BusinessUnit">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>

                        <asp:BoundField DataField="Approver" HeaderText="Approver" ReadOnly="True" SortExpression="Approver">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" Visible="false">
                            <HeaderStyle Width="1%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="1%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                    </Columns>
                    <EmptyDataTemplate>
                        No Records are found.
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="row-fluid">
                    <h3 class="span6">Pending Approval</h3>
                </div>
                <asp:GridView ID="PendingApprovalGrid" runat="server" AutoGenerateColumns="false" Width="100%" AllowSorting="true"
                    CssClass="EU_DataTable" EmptyDataText="No applications available in Pending status" OnSorting="PendingApprovalGrid_Sorting">
                    <Columns>
                        <asp:BoundField DataField="TypeOfTravel" HeaderText="Type Of Travel" ReadOnly="true" SortExpression="TypeOfTravel">
                            <HeaderStyle Width="15%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TravellerName" HeaderText="Traveller Name" ReadOnly="True" SortExpression="TravellerName">
                            <HeaderStyle Width="20%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="FormNo" SortExpression="FormNo">
                            <ItemTemplate>
                                <asp:Label ID="linkFormNo" runat="server" Text='<%# Bind("FormNo")  %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <HeaderStyle Width="15%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" Wrap="true" VerticalAlign="Top" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="BusinessUnit" HeaderText="Business Unit" ReadOnly="True" SortExpression="BusinessUnit">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Approver" HeaderText="Approver" ReadOnly="True" SortExpression="Approver">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" Visible="false">
                            <HeaderStyle Width="1%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="1%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                    </Columns>
                    <EmptyDataTemplate>
                        No Records are found.
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="row-fluid">
                    <h3 class="span6">Approved</h3>
                </div>
                <asp:GridView ID="ApprovedGrid" runat="server" AutoGenerateColumns="false" Width="100%" AllowSorting="true"
                    CssClass="EU_DataTable" EmptyDataText="No applications available in Pending status" OnSorting="ApprovedGrid_Sorting">
                    <Columns>
                        <asp:BoundField DataField="TypeOfTravel" HeaderText="Type Of Travel" ReadOnly="true" SortExpression="TypeOfTravel">
                            <HeaderStyle Width="15%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TravellerName" HeaderText="Traveller Name" ReadOnly="True" SortExpression="TravellerName">
                            <HeaderStyle Width="20%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="FormNo" SortExpression="FormNo">
                            <ItemTemplate>
                                <asp:Label ID="linkFormNo" runat="server" Text='<%# Bind("FormNo")  %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <HeaderStyle Width="15%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" Wrap="true" VerticalAlign="Top" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="BusinessUnit" HeaderText="Business Unit" ReadOnly="True" SortExpression="BusinessUnit">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Approver" HeaderText="Approved by" ReadOnly="True" SortExpression="Approver">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" Visible="false">
                            <HeaderStyle Width="1%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="1%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                    </Columns>
                    <EmptyDataTemplate>
                        No Records are found.
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="row-fluid">
                    <h3 class="span6">Rejected/Declined</h3>
                </div>
                <asp:GridView ID="RejectedGrid" runat="server" AutoGenerateColumns="false" Width="100%" AllowSorting="true"
                    CssClass="EU_DataTable" EmptyDataText="No applications available in Rejected status" OnSorting="RejectedGrid_Sorting">
                    <Columns>
                        <asp:BoundField DataField="TypeOfTravel" HeaderText="Type Of Travel" ReadOnly="true" SortExpression="TypeOfTravel">
                            <HeaderStyle Width="15%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TravellerName" HeaderText="Traveller Name" ReadOnly="True" SortExpression="TravellerName">
                            <HeaderStyle Width="20%" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="FormNo" SortExpression="FormNo">
                            <ItemTemplate>
                                <asp:Label ID="linkFormNo" runat="server" Text='<%# Bind("FormNo")  %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <HeaderStyle Width="15%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="15%" Wrap="true" VerticalAlign="Top" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="BusinessUnit" HeaderText="Business Unit" ReadOnly="True" SortExpression="BusinessUnit">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Approver" HeaderText="Rejected By" ReadOnly="True" SortExpression="Approver">
                            <HeaderStyle Width="20%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="20%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" Visible="false">
                            <HeaderStyle Width="1%" Wrap="true" HorizontalAlign="Left" ForeColor="White" />
                            <ItemStyle Width="1%" Wrap="true" VerticalAlign="Top" />
                        </asp:BoundField>
                    </Columns>
                    <EmptyDataTemplate>
                        No Records are found.
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
        <br />
        <br />
        <br />
        <br />
        <br />
    </body>
    </html>
</asp:Content>
