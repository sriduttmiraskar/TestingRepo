<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="TravelReview.aspx.cs" Inherits="HRWebForms.HRWeb.TravelReview" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Travel Review</asp:Content>
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
        <style type="text/css">
            .control-labelRight {
                margin-top: 5px;
            }

            .label-Review {
                float: left;
                width: 160px;
                padding-top: 0px;
                text-align: right;
                margin-bottom: 5px;
                margin-right: 10px;
                padding-left: 5px;
                font-weight: bold;
            }
        </style>
        <script type="text/javascript">
            function resetSharePointSubmitField() {
                window.WebForm_OnSubmit = function () { return true; };
            }
        </script>
    </head>
    <body class="bodyBg">
        <div id='travelreview' class="clearfix">
            <div class="row-fluid main-row-heading">
                <div class="container">
                    <h2 class="span6" style="padding-left:10px; padding-top:10px">Travel Request</h2>
                    <h2 class="span6" style="text-align: right; padding-RIGHT: 100px; padding-top:10px">
                        <asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
                </div>
            </div>
            
            <div class="container margin-bottom-20">
                 
                <div style="padding-bottom: 20px;">
                    <span style="color: red">
                        <asp:Label ID="lblError" runat="server"></asp:Label></span>
                    <span style="float: right;">
                        <asp:Button ID="btnPDF" CssClass="button" runat="server" Text="Generate PDF" OnClick="btnPDF_Click" OnClientClick="resetSharePointSubmitField();" />
                                    &nbsp; <asp:Button ID="btnApprove" runat="server" Text="Approve" CssClass="button" OnClick="btnApprove_Click" />&nbsp;
                                    &nbsp;<asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="button" OnClick="btnReject_Click" />&nbsp; 
                    </span>
                </div>                                                                
                
                <div class="form-horizontal" style="padding-left: 45px">                                                               
                    <div class="row-fluid">                         
                        <div class="span6">
                            <div class="margin-bottom-20 row-fluid">
                               <h4 class="span6">Travel Summary</h4>
                             </div>
                            <br />
                            <div class="control-group1">
                                <label id="ApplicationDate" class="control-label1">Application Date:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblApplicationDate" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="TypeofTravel" class="control-label1">Type of Travel:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblTypeofTravel" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="BookingRequirements" class="control-label1">Booking Requirements:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblBookingRequirements" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1" id="DivVisa" runat="server">
                                <label id="lblVisa" class="control-label1">Visa Required:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblVisaReq" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="TravellerName" class="control-label1">Traveller Name:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblTravellerName" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="Label1" class="control-label1">Traveller Email Address:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblTravellerEmailID" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="PositionTitle" class="control-label1">Designation:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblPositionTitle" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="IfOther" class="control-label1">Position Title:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblIfOther" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1" style="display: none;">
                                <label class="control-label1" width="250px">Does traveller belong to Senior Leadership Team:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblSLT" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="BusinessUnit" class="control-label1">Business Unit:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblBusinessUnit" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="CostCentre" class="control-label1">Cost Centre:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblCostCentre" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="ManagerName" class="control-label1">Manager Name:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblManagerName" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>

                        </div>
                        <div class="span6">

                            <div class="control-group1">
                                <div class="controls2" align="right">
                                                            
                                </div>
                            </div>
                            <br />
                            <div class="control-group1">
                                <label id="lblComments" class="control-label1">Add Comments:</label>
                                <div class="controls">
                                    <asp:TextBox ID="txtComments" TextMode="multiline" Rows="7" CssClass="span12 border-radius-none" runat="server" />
                                </div>
                            </div>
                            <br />                            
                            <div class="control-group1" style="display: none;">
                                <label id="DepartureDate" class="control-label1">Departure Date:</label>
                                <div class="controls">
                                    <asp:Label ID="lblDepartureDate" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1" style="display: none;">
                                <label id="ReturnDate" class="control-label1">Return Date:</label>
                                <div class="controls">
                                    <asp:Label ID="lblReturnDate" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>                            
                            <div class="control-group1">
                                <label id="Purposeoftravel" class="control-label1">Purpose of travel:</label>
                                <div class="controls">
                                    <asp:Label ID="lblPurposeoftravel" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label id="lblNotes" class="control-label1">Notes to Travel<br /> Coordinator:</label>
                                <div class="controls">
                                    <asp:Label ID="lblNotestoTC" class="border-radius-none span12" runat="server"></asp:Label>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
                <div class="container portfolio-item">
                    <div class="row-fluid margin-bottom-20">
                        <div class="tab-content1">
                            <div class="form-horizontal">
                                <div class="margin-bottom-20 row-fluid">
                                    <!-- Combined Travel Tab Start-->
                                    <div id="DivCombinedTravel" class="tab-pane" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Combined Travel Itinerary</h4>
                                        </div>
                                        <br />
                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group1">
                                                    <label id="NoOfLegs" class="control-label1">No of Legs:</label>
                                                    <div style="margin-left:150px">
                                                        <asp:Label ID="lblNoOfLegs" class="border-radius-none span12" runat="server"></asp:Label>
                                                    </div>
                                                </div>
                                               
                                            </div>
                                            <div class="span6">                                                                                             
                                            </div>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span12">
                                                <div class="control-group1" id="DivTravelType" runat="server">
                                                    <label class="control-label1" id="lblTravelType">Travel Type:</label>
                                                    <div class="controls2">
                                                        <ul class="unstyled">
                                                            <li>
                                                                <asp:Label ID="lblFlight" runat="server" Text="Flight" class="border-radius-none span12" />
                                                            </li>
                                                            <li><asp:Label ID="VehicleRequirement" runat="server" Text="Vehicle Requirement" class="border-radius-none span12" /></li>
                                                            <ul class="unstyled-subitem">
                                                                <div><asp:Label ID="lblPersonalVehicle" runat="server" Text="Personal Vehicle" class="border-radius-none span12" /></div>
                                                                <div><asp:Label ID="lblCompanyVehicle" runat="server" Text="Company Vehicle" class="border-radius-none span12" /></div>
                                                                <div><asp:Label ID="lblHireVehicle" runat="server" Text="Hire Vehicle" class="border-radius-none span12" /></div>
                                                            </ul>

                                                            <li>
                                                                <asp:Label ID="lblAccommodation" runat="server" Text="Accommodation Required" class="border-radius-none span12" />&nbsp;&nbsp;
                                                                <asp:Label ID="lblAccomNotReq" runat="server" Text="Accommodation not Required" class="border-radius-none span12" />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                                                      
                                        <!-- Leg Creation-->
                                        <div id="accordion" runat="server" class="clearfix" enableviewstate="true">
                                        </div>
                                        <!-- Combined Travel Tab End--> 
                                    </div>
                                     
                                    
                                </div>
                                
                                <!--- Tab Accommodation Only started-->
                                    <div id="DivAccommodationOnly" runat="server" >
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Accommodation Requirements</h4>
                                        </div>
                                       
                                       
                                       
                                        <div id="AccomRequirements" class="clearfix" >
                                       <asp:Table ID="AccomRequirementsTable" runat="server" CssClass="EU_DataTable" enableviewstate="true">

                                       </asp:Table>
                                             </div>

                                        </div>
                                <!--- Tab Accommodation Only End-->
                                <!--- Tab Vehicle Only started-->
                                 <div id="DivVehicleOnly" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Vehicle Details</h4>
                                        </div>
                                       
                                     <div class="control-group1">
                                    <label id="MotorVehicle" class="control-label1">Motor Vehicle:</label>
                                    <div class="controls2">                                        
                                        <asp:Label ID="lblMotorVehicle" class="border-radius-none span12" runat="server"></asp:Label>
                                    </div>
                                </div>
                                       
                                        
                                        <div id="VehicleDetails" class="clearfix" >
                                       <asp:Table ID="VehicleTabel" runat="server" CssClass="EU_DataTable" enableviewstate="true">

                                       </asp:Table>
                                             </div>

                                        </div>
                                <!--- Tab Vehicle Only End-->

                                <div class="row-fluid" style="width: 80%">
                                            <div>
                                                <h4 class="span6">Approval History:</h4>
                                                <asp:GridView ID="gdCommentHistory" CssClass="EU_DataTable" runat="server" AutoGenerateColumns="false" Width="100%">
                                                    <Columns>
                                                        <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True">
                                                            <HeaderStyle Width="20%" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="20%" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:BoundField>
                                                        <asp:BoundField DataField="UserName" HeaderText="UserName">
                                                            <HeaderStyle Width="20%" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="20%" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:BoundField>

                                                        <asp:TemplateField HeaderText="Comments">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblComments" runat="server" Text='<%# Bind("Comments") %>'></asp:Label>
                                                            </ItemTemplate>
                                                            <HeaderStyle Width="60%" Wrap="true" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="60%" Wrap="true" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <EmptyDataTemplate>
                                                        No Records are found.
                                                    </EmptyDataTemplate>
                                                </asp:GridView>
                                            </div>

                                        </div>
                            </div>
                        </div>
                    </div>
                </div>



            </div>
        </div>
        <br />
        <br />
        <br />
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>
    </body>

    </html>
</asp:Content>
