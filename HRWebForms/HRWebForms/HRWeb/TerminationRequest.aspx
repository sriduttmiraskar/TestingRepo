<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="TerminationRequest.aspx.cs" Inherits="HRWebForms.HRWeb.TerminationRequest" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<%@ Register TagPrefix="Custom" TagName="UserControl" Src="~/_ControlTemplates/HRWebForms/UploadJobUserControl.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Termination Request</asp:Content>
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
            .term-select {
                width: 55% !important;
            }
        </style>

    </head>
    <body class="bodyBg">
        <div id='termination-web' class="clearfix">
            <div class="row-fluid main-row-heading">
                <div class="container">
                    <h2 class="span6" style="padding-left:10px;">Termination Request</h2>
                    <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">
                        Ref No:<asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
                </div>
            </div>
            <div style="float: right; margin-right: 20px; margin-bottom: 20px; font-style: italic">Fields marked <span style="color: red">*</span> are mandatory</div>
            <div>
                <span style="color: red">
                    <asp:Label ID="lblTerminationRequest" runat="server"></asp:Label></span>
            </div>
            <div class="container margin-bottom-20">

                <div class="form-horizontal">
                    <div class="row-fluid">

                        <div class="span6">
                            <div class="control-group">
                                <label for="" class="control-label" style="padding-top: 0px">Date <span style="color: red">*</span></label>
                                <div class="controls" style="position: relative">
                                    <asp:Label ID="lblDateOFRequest" runat="server"></asp:Label>
                                </div>
                            </div>

                            <div class="control-group">
                                <label for="" class="control-label">
                                    Position Type <span style="color: red">*</span>
                                </label>
                                <div class="controls">
                                    <asp:DropDownList ID="drpdwnPositionType" runat="server" Width="71%">
                                    </asp:DropDownList>
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image11" ToolTip="Contractors are not paid through payroll and provide an invoice.&#013;Salary employees  paid monthly.&#013;Wage Employee covered by Enterprise Agreement.&#013;Expatriate based overseas." runat="server" />
                                </div>
                            </div>


                        </div>
                        <div class="span6">
                            <Custom:UserControl id="MyCustomControl" runat="server" />

                        </div>



                    </div>
                </div>
                <div class="container portfolio-item">
                    <div class="row-fluid margin-bottom-20">
                        <ul class="nav nav-tabs tabs">
                            <li id="notificationTab" class="active" runat="server"><a href="#Notification" class="">Notification</a></li>
                            <li id="TypeOfLeaveTab" class="" runat="server"><a href="#Typeofleave" class="">Type of leave</a></li>
                            <li id="BusinessChecklistTab" runat="server" class=""><a href="#BusinessChecklist" class="">Business Checklist</a></li>
                            <li id="ISChecklistTab" runat="server" class=""><a href="#ISChecklist" class="">IS Checklist</a></li>
                            <li id="TerminationMeetingTab" runat="server" class=""><a href="#TerminationMeeting" class="">Termination Meeting</a></li>
                            <li id="HRServicesTab" runat="server" class=""><a href="#HRServices" class="">HR Services</a></li>
                        </ul>
                        <div class="tab-content">
                            <div class="form-horizontal">
                                <div class="margin-bottom-20 row-fluid">
                                    <div id="Notification" class="tab-pane active">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Notification of Termination</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->
                                            <asp:Button CssClass="button" Text="Save & Next" ID="btnTerminationSav" runat="server" OnClick="btnTerminationSave_Click" />&nbsp;&nbsp;
                                            
                                            
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group">
                                                    <label for="" class="control-label">Employee Name <span style="color: red">*</span></label>
                                                    <div class="controls">

                                                        <asp:TextBox class="border-radius-none span12" ID="txtEmpName" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Employee Number<span style="color: red">*</span></label>
                                                    <div class="controls">

                                                        <asp:TextBox class="border-radius-none span12" ID="txtEmpNumber" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Business Unit<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnBusinessUnit" class="term-select1" runat="server"
                                                            AutoPostBack="true" OnSelectedIndexChanged="drpdwnBusinessUnit_SelectedIndexChanged">
                                                        </asp:DropDownList>
                                                        
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Work Area<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnWorkArea" class="term-select1" runat="server">
                                                            <asp:ListItem>Administration</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Site Location<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnSiteLocation" class="term-select1" runat="server">
                                                            <asp:ListItem>Sydney</asp:ListItem>
                                                            <asp:ListItem>Leeton</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Is mobile phone/equipment purchase required<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnMobilePhone" class="term-select1" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label for="" class="control-label">Does this employee hold an Immigration Visa<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnImmigrationVisa" class="term-select1" runat="server">
                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Does this employee have a novated lease<span style="color: red">*</span></label>
                                                    <div class="controls">


                                                        <asp:DropDownList ID="drpdwnInnovated" class="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                            </div>


                                            <div class="span6">
                                                <div class="control-group">
                                                    <label for="" class="control-label">Last day of work<span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" ID="dtLastDayOfWork" UseTimeZoneAdjustment="false" LocaleId="2057" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Period of Service Start Date<span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" ID="dtPeriodOfServiceFrom" UseTimeZoneAdjustment="false" LocaleId="2057" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>


                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Period of Service End Date<span style="color: red">*</span></label>

                                                    <div class="controls" style="position: relative; padding-top: 1%">
                                                        <SharePoint:DateTimeControl runat="server" ID="dtPeriodOfServiceTo" UseTimeZoneAdjustment="false" LocaleId="2057" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>
                                                
                                                <h6 class="">Comments</h6>
                                                <div class="control-group">

                                                    <asp:TextBox ID="txtNotificationComments" CssClass="span12 border-radius-none" runat="server" TextMode="MultiLine" Rows="6"></asp:TextBox>
                                                </div>
                                            </div>

                                        </div>

                                    </div>
                                    <!--- Tab Typeof Leave started-->
                                    <div id="Typeofleave" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Type of Leave</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->
                                            <asp:Button ID="btnLeaveSave" runat="server" CssClass="button" Text="Save" OnClick="btnLeaveSave_Click" />
                                            <asp:Button CssClass="button" Text="Submit" ID="btnInitiatorSubmit" runat="server" OnClick="btnInitiatorSubmit_Click" />&nbsp;&nbsp;
                                        </div>
                                        <div class="row-fluid">

                                            <div class="span5">

                                                <div class="control-group">
                                                    <label for="" class="control-label">Is this Parental Leave<span  style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnParentalLeave" runat="server" CssClass="term-select">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>

                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Leave Without Pay<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnLeaveWithoutPay" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>

                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Period of leave Start Date<span id="spPeriodofLeaveStart" style="color: red; display:none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" ID="dtPeriodOfLeaveFrom" UseTimeZoneAdjustment="false" LocaleId="2057" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Period of leave End Date<span id="spPeriodofLeaveEnd" style="color: red; display:none">*</span></label>
                                                    <div class="controls" style="position: relative; padding-top: 1%">
                                                        <SharePoint:DateTimeControl runat="server" ID="dtPeriodOfLeaveTo" UseTimeZoneAdjustment="false" LocaleId="2057" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row-fluid">
                                            <div class="span12">


                                                <div class="control-group">
                                                    <label for="" class="control-label">Comments</label>
                                                    <div class="controls">

                                                        <asp:TextBox ID="txtLeaveComments" CssClass="span12 border-radius-none" runat="server" TextMode="MultiLine" Rows="6"></asp:TextBox>
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    
                                    <div id="BusinessChecklist" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="">Credit Card</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->
                                            <asp:Button ID="btnBusinessChecklist" runat="server" CssClass="button" Text="Save & Next" OnClick="btnBusinessChecklist_Click" />

                                        </div>
                                        <div class="row-fluid">

                                            <div class="span5">

                                                <div class="control-group">
                                                    <label for="" class="control-label3">Cancel Credit Card – advise Amex Administrator to  cancel card<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnCancelCreditCard" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>


                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Have all receipts been received to submit  final Amex claim form<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnClaimForm" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                            </div>

                                        </div>
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="">Procurement</h4>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span5">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Company Vehicle Returned<span style="color: red">*</span></label><div class="controls3">

                                                        <asp:DropDownList ID="drpdwnCompanyVehicleReturned" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Vehicle keys x 2 sets<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnVehicleSet" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Fuel Card<span style="color: red">*</span></label><div class="controls3">

                                                        <asp:DropDownList ID="drpdwnFuelCard" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Vehicle condition report completed<span style="color: red">*</span></label>
                                                    <div class="controls3">

                                                        <asp:DropDownList ID="drpdwnVehicleReport" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                            </div>


                                        </div>
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="">Finance</h4>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span5">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Is the employee a Cheque Signatory<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnChequeSignature" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                            </div>


                                        </div>
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="">Marketing</h4>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span5">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Remove employee from websites SunRice/Careers/SunConnect<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnRemoveEmployee" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Remove Photos from Corporate Affairs images directory<span style="color: red">*</span></label>

                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnRemovePhotos" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="">Site Administration</h4>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span5">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Security Card<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnSecurityCard" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Office/Site Keys<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnOfficeKeys" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Locker Key<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnLockerKey" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">FOB Passes<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnFOBPassess" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Uniform Return<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnUniformReturn" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                            </div>


                                        </div>

                                    </div>


                                    <!--starts  Expat here-->
                                    <div id="ISChecklist" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Information Techonology Checklist</h4>

                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button ID="btnISChecklist" runat="server" CssClass="button" Text="Save & Next" OnClick="btnISChecklist_Click" />

                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Remove employee from email contact listing/folders/SunConnect Contacts listing<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnRemoveEmployeeISChecklist" CssClass="term-select" runat="server" Enabled="false">
                                                            <asp:ListItem Value="Yes" Selected="True">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">All equipment to be returned to IS in Leeton<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnLeetor" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Remove/Disable computer access<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnRemoveAccess" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Mobile Phone & Charger returned<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnMobileReturned" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Mobile Phone purchased and transferred into employee's name<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnMobilePhonePurchased" CssClass="term-select" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Any electronic equipment (ipad etc)<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnElectronicEquip" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Laptop Collected<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnLaptopCollected" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Change employees voicemail<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnChangeVoicemail" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Set automatic email notification to alert sender that the employee is no longer employed<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnSetAutomaticEmail" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <div id="TerminationMeeting" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span5">Termination Meeting</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button ID="btnMeeting" runat="server" CssClass="button" Text="Save" OnClick="btnMeeting_Click" />
                                            <asp:Button ID="btnMeetingSubmit" runat="server" CssClass="button" Text="Submit" OnClick="btnMeetingSubmit_Click" />

                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Exit Interview<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnExitInterview" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">All company property collected & actioned<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnPropertyCollected" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Re-iterate confidentiality agreement<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnReiterateAgreement" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Prepare to notify employees contacts(Customers/Suppliers)<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnNotifyEmployeesContacts" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Confirm employee's address for future mailing of information<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnConfirmEmployeesAddress" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label3">Certificate of Service request<span style="color: red">*</span></label>
                                                    <div class="controls3">
                                                        <asp:DropDownList ID="drpdwnCertificateService" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>

                                                <h6 class="">Address / Comments<span style="color: red">*</span></h6>
                                                <div class="control-group">

                                                    <asp:TextBox ID="txtMeetingComments" CssClass="span12 border-radius-none" runat="server" TextMode="MultiLine" Rows="6"></asp:TextBox>
                                                </div>
                                            </div>


                                        </div>
                                    </div>
                                    <div id="HRServices" class="tab-pane" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">HR Services</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button ID="btnHrServiceces" runat="server" CssClass="button" Text="Save" OnClick="btnHrServiceces_Click" />

                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">
                                                <div class="control-group">
                                                    <label for="" class="control-label">Process Final Payment<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnFinalPayment" CssClass="term-select" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                &nbsp;
                                                <div class="control-group">
                                                    <label for="" class="control-label">Terminat from SAP Payroll System<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnTerminateSAP" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>


                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Kronos access removed<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnKronosRemoved" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>


                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Termination pay provided<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnTerminationPay" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Delimit date monitoring<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnDelimitDate" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Remove personal file<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnRemoveFile" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">Housing subsidy/Motor vehicle Declaration<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnHousing" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>


                                                </div>
                                                <div class="control-group">
                                                    <label for="" class="control-label">457 Visa Notification to Immigration Department<span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnVisaNotification" CssClass="term-select1" runat="server">

                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>


                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                </div>




                            </div>
                        </div>
                        <div id="openModaltrash" class="form-horizontal" style="display: none">
                            <i>Click on checkbox to select files you want to delete</i>

                            <div class="">
                                <label class="checkbox">
                                    <input type="checkbox">
                                    Delete file name .......
                                </label>
                            </div>
                            <div class="">
                                <label class="checkbox">
                                    <input type="checkbox">
                                    Delete file name .......
                                </label>
                            </div>
                            <div class="">
                                <label class="checkbox">
                                    <input type="checkbox">
                                    Delete file name .......
                                </label>
                            </div>
                            <div class="">
                                <label class="checkbox">
                                    <input type="checkbox">
                                    Delete file name .......
                                </label>
                            </div>


                            <div class="form-group text-right">
                                <button class="btn btn-primary">Delete Selected</button>
                            </div>
                        </div>
                        <!--Upload File modal starts-->
                        <div id="openModalupload" class="form-horizontal" style="display: none">
                            <i>Click CLRT to upload multiple files</i>

                            <div class="control-group">
                                <label for="" class="control-label">Select File location</label>
                                <div class="controls">
                                    <input name="filesToUpload[]" id="filesToUpload" class="btn" type="file" multiple="" />
                                </div>
                            </div>
                            <div class="control-group">
                                <label for="" class="control-label">File name....</label>
                                <div class="controls">
                                    <div class="progress progress-info span3">
                                        <div style="width: 20%" class="bar">
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="control-group">
                                <label for="" class="control-label">File name....</label>
                                <div class="controls">
                                    <div class="progress progress-info span3">
                                        <div style="width: 20%" class="bar">
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <p>Do not close until all files are uploaded    </p>
                            <div class="form-group text-right">
                                <button class="btn btn-primary">Save to file</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="clearfix">&nbsp;</div>
        <br />
        <br />
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>
        <script type="text/javascript">
            $(document).ready(function () {

                if ($('option:selected', $('#<%= drpdwnParentalLeave.ClientID %>')).text() == 'Yes' || $('option:selected', $('#<%= drpdwnLeaveWithoutPay.ClientID %>')).text() == 'Yes') {
                    document.getElementById('spPeriodofLeaveStart').style.display = '';
                    document.getElementById('spPeriodofLeaveEnd').style.display = '';
                }
                else {
                    document.getElementById('spPeriodofLeaveStart').style.display = 'none';
                    document.getElementById('spPeriodofLeaveEnd').style.display = 'none';
                }

                $('#<%= drpdwnParentalLeave.ClientID %>').on('change', function () {

                    if ($('option:selected', $(this)).text() == 'Yes') {
                        document.getElementById('spPeriodofLeaveStart').style.display = '';
                        document.getElementById('spPeriodofLeaveEnd').style.display = '';
                    }
                    else if ($('option:selected', $('#<%= drpdwnLeaveWithoutPay.ClientID %>')).text() == 'No')  {
                        document.getElementById('spPeriodofLeaveStart').style.display = 'none';
                        document.getElementById('spPeriodofLeaveEnd').style.display = 'none';
                    }

                });
                $('#<%= drpdwnLeaveWithoutPay.ClientID %>').on('change', function () {

                    if ($('option:selected', $(this)).text() == 'Yes') {
                        document.getElementById('spPeriodofLeaveStart').style.display = '';
                        document.getElementById('spPeriodofLeaveEnd').style.display = '';
                    }
                    else if ($('option:selected', $('#<%= drpdwnParentalLeave.ClientID %>')).text() == 'No') {
                        document.getElementById('spPeriodofLeaveStart').style.display = 'none';
                        document.getElementById('spPeriodofLeaveEnd').style.display = 'none';
                    }

                });
            });




            function MoveToLeaveTab() {
                $("a[href='#Typeofleave']").trigger("click");
                return false;
            }
            function MoveToBCTab() {

                $("a[href='#BusinessChecklist']").trigger("click");
                return false;
            }
            function MoveToISTab() {

                $("a[href='#ISChecklist']").trigger("click");
                return false;
            }
            function MoveToMeetingTab() {

                $("a[href='#TerminationMeeting']").trigger("click");
                return false;
            }
            $(document).ready(function () {

                $('.controlrole-label').html("Attached Documents <span style='color: red'>*</span>");


            });
            $(".tab-pane").hide();
            $("ul.tabs li:first").addClass("active").show();
            $(".tab-pane:first").show();
            //On Click Event
            $("ul.tabs li").click(function () {
                $("ul.tabs li").removeClass("active");
                $(this).addClass("active");
                $(".tab-pane").hide();
                var activeTab = $(this).find("a").attr("href");
                $(activeTab).fadeIn();
                return false;
            });

            $(function () {
                $('a.add-new,a.add-new-row').click(function (event) {
                    event.preventDefault();
                    var newRow = $('<tr><td></td><td><input type="text" class=""></input></td><td><input type="text" class=""></input> </td><td><input type="text" class=""></input></td><td></td></tr>');
                    var addnewRow = $('<tr><td><select id=""><option value="Select">Select</option><option selected="" value="Langi">Langi</option></select></td> <td></td><td><select id=""><option selected="" value="Select">Select</option></select></td><td><select id=""><option selected="" value="Select">Select</option></select></td><td><input type="text" class=""></input></td><td></td><td><input type="text" class=""></input></td> <td></td></tr> ');
                    $('table.opt-payment').append(addnewRow);
                    $('table.new-arrangment').append(newRow);
                    return false;
                });
                $(".datepicker").datepicker();
                $(".datepicker").datepicker("setDate", new Date);

                //$( "#openModalupload ,#openModaltrash" ).hide();
                //$( "#openModalupload ,#openModaltrash" ).dialog({ autoOpen: false });
                $('.trash').click(function (e) {
                    $("#openModaltrash").dialog({
                        draggable: false,
                        height: "auto",
                        width: 550,
                        title: false,
                        modal: true,
                        resizable: false

                    });
                    $(".ui-dialog-title").html("<h2 class='modal-header'> Delete File</h2");
                    return false;
                });
                $('.upload').click(function (e) {
                    $("#openModalupload").dialog({
                        draggable: false,
                        height: "auto",
                        width: 550,
                        title: false,
                        modal: true,
                        resizable: false
                    });
                    $(".ui-dialog-title").html("<h2 class='modal-header'> Upload File</h2");
                    return false;
                });
                $(".ddlPositionType").on('change', function () {
                    if ($(this).val() == 'Salary') { $("a[href='#Salary']").trigger("click"); }
                    else if ($(this).val() == 'Waged') { $("a[href='#Waged']").trigger("click"); }
                    else if ($(this).val() == 'Contractor') { $("a[href='#Contractor']").trigger("click"); }
                    else if ($(this).val() == 'Expat') { $("a[href='#Expat']").trigger("click"); }
                });


            });

        </script>
    </body>

    </html>
</asp:Content>
