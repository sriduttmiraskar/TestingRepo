<%@ Assembly Name="HRWebForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c8c0e2f713937cc8" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="NewHireRequest.aspx.cs" Inherits="HRWebForms.HRWeb.NewHireRequest" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<%@ Register TagPrefix="Custom" TagName="UserControl" Src="~/_ControlTemplates/HRWebForms/UploadJobUserControl.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">New Hire Request</asp:Content>
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
        <div id='hr-web' class="clearfix">
            <div class="row-fluid main-row-heading">
                <h2 class="span6" style="padding-left: 10px;">New Hire Request</h2>
                <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">Ref No:
                    <asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
            </div>
            <div style="float: right; margin-right: 20px; margin-bottom: 20px; font-style: italic">
                Fields marked <span style="color: red">*</span> are mandatory
            </div>
            <div>
                <span style="color: red">
                    <asp:Label ID="lblError" runat="server"></asp:Label></span>
            </div>
            <div class="container margin-bottom-20" id="divmain" runat="server">
                <div class="form-horizontal">
                    <div class="row-fluid">

                        <div class="span6">

                            <div class="control-group">
                                <label id="lblFirstName" class="control-label">First Name <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="border-radius-none span12"></asp:TextBox>
                                </div>
                            </div>
                            <div class="control-group">
                                <label id="Label2" class="control-label">Last Name <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="border-radius-none span12"></asp:TextBox>
                                </div>

                            </div>
                            <div class="control-group">
                                <label class="control-label">Address <span style="color: red">*</span></label>

                                <div class="controls">
                                    <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3" CssClass="border-radius-none span12"></asp:TextBox>


                                </div>
                            </div>
                            <div class="control-group">
                                <label id="Label3" class="control-label">City <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:TextBox ID="txtCity" runat="server" CssClass="border-radius-none span12"></asp:TextBox>
                                </div>

                            </div>
                            <div class="control-group">
                                <label class="control-label">State <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:TextBox ID="txtState" runat="server" CssClass="border-radius-none span12"></asp:TextBox>
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label">Post Code <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:TextBox ID="txtPostCode" runat="server" CssClass="border-radius-none span12"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                        <div class="span6">

                            <div class="control-group">
                                <label id="lblDate" class="control-label" style="padding-top: 0px">Date: </label>
                                <div class="controls" style="position: relative">
                                    <asp:Label ID="lblDateOfRequest" runat="server"></asp:Label>


                                </div>
                            </div>

                            <div class="control-group" runat="server" id="dvAppToHire">
                                <label class="control-label">App To Hire Ref No</label>
                                <div class="controls">

                                    <asp:DropDownList ID="ddlRef" runat="server" CssClass="border-radius-none span12" AutoPostBack="true" OnSelectedIndexChanged="ddlRef_SelectedIndexChanged">
                                        <asp:ListItem Value="New Hire">Create New Hire (Harvest Only)</asp:ListItem>
                                    </asp:DropDownList>


                                </div>

                            </div>

                            <!--<div class="control-group" runat="server" id="dvNewHire">
                                <label class="control-label">New Hire Ref No:</label>
                                <div class="controls" runat="server" style="padding-top:5px !important">

                                    <asp:Label ID="lblNewHire" runat="server"></asp:Label>


                                </div>

                            </div>-->

                            <div class="control-group" runat="server" id="dvlblPostionType">
                                <label id="Label5" class="control-label" style="padding-top: 0px">Position type:</label>
                                <div class="controls">
                                    <asp:Label ID="lblPositionType" runat="server" Width="30%"></asp:Label>
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image11" ToolTip="Contractors are not paid through payroll and provide an invoice.&#013;Salary employees  paid monthly.&#013;Wage Employee covered by Enterprise Agreement.&#013;Expatriate based overseas." runat="server" />
                                </div>
                            </div>

                            <div class="control-group" runat="server" id="dvdrpPostionType">
                                <label id="Label26" class="control-label" style="padding-top: 0px">Position type:</label>
                                <div class="controls">
                                    <asp:DropDownList ID="ddlPositionType" CssClass="border-radius-none span12" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPositionType_SelectedIndexChanged">
                                    </asp:DropDownList>
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image1" ToolTip="Contractors are not paid through payroll and provide an invoice.&#013;Salary employees  paid monthly.&#013;Wage Employee covered by Enterprise Agreement.&#013;Expatriate based overseas." runat="server" />
                                </div>
                            </div>

                            <div class="control-group">
                                <label class="control-label">Type Of Role <span style="color: red">*</span></label>


                                <div class="controls">
                                    <asp:DropDownList ID="ddlTypeOfRole" CssClass="border-radius-none span12" runat="server">
                                        <asp:ListItem Value="Permanent">Permanent</asp:ListItem>
                                        <asp:ListItem Value="Part Time">Part Time</asp:ListItem>
                                        <asp:ListItem Value="Fixed Term">Fixed Term</asp:ListItem>
                                        <asp:ListItem Value="Casual">Casual</asp:ListItem>
                                    </asp:DropDownList>
                                </div>


                            </div>
                        </div>

                    </div>
                </div>

                <div class="container portfolio-item">
                    <div class="row-fluid margin-bottom-20">
                        <ul class="nav nav-tabs tabs">
                            <li class="active"><a href="#Salary" class="">Salary</a></li>
                            <li class=""><a href="#Waged" class="">Waged</a></li>
                            <li class=""><a href="#Contractor" class="">Contractor</a></li>
                            <li class=""><a href="#Expat" class="">Expatriate</a></li>
                            <%--<li class=""><a href="#SuccessfulApplicant " class="">Successful Applicant</a></li>--%>
                        </ul>
                        <div class="tab-content">
                            <div class="form-horizontal">
                                <div class="margin-bottom-20 row-fluid">
                                    <!--- Tab Salary started-->
                                    <div id="Salary" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>


                                        </div>
                                        <div class="span4 positionAbs text-right">

                                            <asp:Button runat="server" ID="btnSalarySave" Text="Save" CssClass="button" OnClick="btnSalarySave_Click" />
                                            <asp:Button runat="server" ID="btnSalarySubmit" Text="Submit" CssClass="button" OnClick="btnSalarySubmit_Click" />

                                        </div>

                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblPositionTitle" class="control-label">Position Title <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <asp:TextBox ID="txtPositionTitle" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group" runat="server" id="dvlblBU">
                                                    <label class="control-label">
                                                        Business Unit  <span style="color: red">*</span>
                                                    </label>

                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:Label ID="lblBusinessUnit" runat="server"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" runat="server" id="dvdrpBU">
                                                    <label class="control-label">
                                                        Business Unit <span style="color: red">*</span>
                                                    </label>

                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:DropDownList ID="ddlBusinessUnit" AutoPostBack="true" CssClass="border-radius-none span12" runat="server" OnSelectedIndexChanged="ddlBusinessUnit_SelectedIndexChanged">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWorkArea" class="control-label">Work Area <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWorkArea" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblSiteLocation" class="control-label">Site Location <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlSiteLocation" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">

                                                    <label id="lblReportsTo" class="control-label">Reports to <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ReportsToPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblTypeofPosition" class="control-label">Type of Contract </label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContractType" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem>Salary</asp:ListItem>
                                                            <asp:ListItem>SLT</asp:ListItem>
                                                            <asp:ListItem>CMT</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblCommencementDate" class="control-label">Commencement Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="CommencementDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblFixedEndDate" class="control-label">Term End Date <span id="spFxTerm" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="TermEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label4" class="control-label">Next Salary Review</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtNewSalaryReview" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">

                                                    <label id="Label6" class="control-label">Who will sign the letter <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="SignLetterPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label7" class="control-label">Notes</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtNotes" TextMode="MultiLine" Rows="3" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                            </div>

                                            <div class="span6">

                                                <h4 class="">Remuneration Details</h4>

                                                <div class="control-group">
                                                    <label id="lblGrade" class="control-label">Grade <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlGrade" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblFAR" class="control-label">FAR <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtFAR" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblSTI" class="control-label">STI</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlSTI" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblVehicle" class="control-label">Vehicle</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlVehicle" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblIfOther" class="control-label">If other (specify)</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtIfOthers" Rows="3" TextMode="MultiLine" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image4" ToolTip="Include any specific details relating to this application. Eg: If using other in vehicle option, please specify." runat="server" />
                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label8" class="control-label">Relocation</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlRelocation" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label9" class="control-label">Relocation Details</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtRelocationDet" Rows="3" TextMode="MultiLine" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        
                                                    </div>
                                                </div>
                                            </div>

                                        </div>

                                        <!--<div class="row-fluid" style="width: 80%;">

                                            <div>
                                                <h4 class="">Offer Checklist (to be completed by HR Manager)</h4>
                                                <asp:CheckBoxList ID="chkbxLstSalOffer" runat="server">
                                                    <asp:ListItem>VEVO Check Completed (Right to work in Australia)</asp:ListItem>
                                                    <asp:ListItem>Reference Checks</asp:ListItem>
                                                    <asp:ListItem>Resume/Application Form</asp:ListItem>
                                                    <asp:ListItem>Interview Notes</asp:ListItem>
                                                    <asp:ListItem>Psychometric Testing</asp:ListItem>
                                                </asp:CheckBoxList>
                                            </div>
                                        </div>-->

                                    </div>
                                    <!--- Tab Waged started-->
                                    <div id="Waged" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>


                                        </div>
                                        <div class="span4 positionAbs text-right">

                                            <asp:Button runat="server" ID="btnWagedSave" Text="Save" CssClass="button" OnClick="btnWagedSave_Click" />
                                            <asp:Button runat="server" ID="btnWagedSubmit" Text="Submit" CssClass="button" OnClick="btnWagedSubmit_Click" />
                                        </div>

                                        <div class="row-fluid">

                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblWagedPositionTitle" class="control-label">Position Title <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedPositionTitle" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group" id="dvlblWagedBU" runat="server">
                                                    <label class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:Label ID="lblWagedBusinessUnit" runat="server"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" id="dvdrpWagedBU" runat="server">
                                                    <label class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:DropDownList ID="ddlWagedBusinessUnit" AutoPostBack="true" CssClass="border-radius-none span12" runat="server" OnSelectedIndexChanged="ddlWagedBusinessUnit_SelectedIndexChanged">
                                                        </asp:DropDownList>


                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedWorkArea" class="control-label">Work Area <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedWorkArea" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedSiteLocation" class="control-label">Site Location <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedSiteLocation" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedReportsTo" class="control-label">Reports to <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ReportsToWagedPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />


                                                    </div>
                                                </div>



                                                <div class="control-group">
                                                    <label id="lblWagedProposedStartDate" class="control-label">Commencement Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="WagedCommencementDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedFixedEndDate" class="control-label">Term End Date <span id="spFxTerm1" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="WagedTermEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label10" class="control-label">Who will sign the letter <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="WagedSignPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />


                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label11" class="control-label">Notes</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedNotes" TextMode="MultiLine" Rows="3" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                            </div>



                                            <div class="span6">




                                                <h4 class="">Remuneration Details</h4>

                                                <div class="control-group">
                                                    <label id="lblWagedLevel" class="control-label">Pay Level <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedLevel" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedShiftRotation" class="control-label">Roster Type <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedRosterType" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem>Day</asp:ListItem>
                                                            <asp:ListItem>Rotational</asp:ListItem>
                                                            <asp:ListItem>Continuous</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label12" class="control-label">Crew <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedCrew" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label13" class="control-label">Shift Team Leader <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedShiftTream" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label14" class="control-label">Allowances <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedAllowances" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedVehicle" class="control-label">Vehicle</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedVehicle" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedIfOther" class="control-label">If other (specify)</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedIfOther" Rows="3" TextMode="MultiLine" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image3" ToolTip="Include any specific details relating to this application. Eg: If using other in vehicle option, please specify." runat="server" />
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                        <!--<div class="row-fluid" style="width: 80%">
                                            <div>
                                                <h4 class="">Offer Checklist (to be completed by HR Manager)</h4>
                                                <asp:CheckBoxList ID="chkbxLstWaged" runat="server">
                                                    <asp:ListItem>VEVO Check Completed (Right to work in Australia)</asp:ListItem>
                                                    <asp:ListItem>Reference Checks</asp:ListItem>
                                                    <asp:ListItem>Resume/Application Form</asp:ListItem>
                                                    <asp:ListItem>Interview Notes</asp:ListItem>
                                                    <asp:ListItem>Psychometric Testing</asp:ListItem>
                                                </asp:CheckBoxList>
                                            </div>
                                        </div>-->
                                    </div>
                                    <!--- Tab Contractor started-->
                                    <div id="Contractor" class="tab-pane active">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>

                                            <h4 class="span6">Job Details&nbsp;<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image2" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" /></h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnContractorSave" Text="Save" CssClass="button" OnClick="btnContractorSave_Click" />
                                            <asp:Button runat="server" ID="btnContractorSubmit" Text="Submit" CssClass="button" OnClick="btnContractorSubmit_Click" />

                                        </div>
                                        <div class="row-fluid">

                                            <div class="span6">
                                                <div class="control-group">
                                                    <label id="Label15" class="control-label">Position Title </label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractPosition" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label16" class="control-label">Agency / Company / Trading Name <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractCompany" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label17" class="control-label">ABN <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractABN" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group" id="dvlblContraBU" runat="server">
                                                    <label class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:Label ID="lblContraBusinessUnit" runat="server"></asp:Label>


                                                    </div>
                                                </div>
                                                <div class="control-group" id="dvdrpContraBU" runat="server">
                                                    <label class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:DropDownList ID="ddlContraBusinessUnit" AutoPostBack="true" CssClass="border-radius-none span12" runat="server" OnSelectedIndexChanged="ddlContraBusinessUnit_SelectedIndexChanged">
                                                        </asp:DropDownList>


                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraWorkArea" class="control-label">Work Area <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContraWorkArea" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraSiteLocation" class="control-label">Site Location <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContraSiteLocation" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraReportsTo" class="control-label">Reports to <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ReportsToContractorPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContraCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label1" class="control-label">Contract Rate (ex GST) <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractRate" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblRateTypeField" class="control-label">Rate Type Field <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlRateTypeField" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem>Daily</asp:ListItem>
                                                            <asp:ListItem>Hourly</asp:ListItem>

                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraProStartDate" class="control-label">Contract Start Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ContraStartDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraFixedEndDate" class="control-label">Contract End Date <span id="spFxTerm3" style="color: red;">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ContraEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="Label18" class="control-label">Payment Terms</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContractPaymentTerms" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem>45</asp:ListItem>
                                                            <asp:ListItem>Other(Speicfy)</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label19" class="control-label">If other (specify)</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractOthers" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image5" ToolTip="Include any specific details relating to this application. Eg: If using other in vehicle option, please specify." runat="server" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label20" class="control-label">GST</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContractGST" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label21" class="control-label">Who will sign the<br />Contract <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ContractSignPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">

                                                <Custom:UserControl id="MyCustomControl" runat="server" />
                                                <div class="control-group">
                                                    <label id="lblContraDeliverables" class="">Services To Be Provided / Primary Objectives </label>
                                                    <div class="">
                                                        <asp:TextBox ID="txtContraRoleStatement" TextMode="MultiLine" Rows="10" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                            </div>
                                        </div>

                                    </div>
                                    <!--starts  Expat here-->
                                    <div id="Expat" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnExpatSave" Text="Save" CssClass="button" OnClick="btnExpatSave_Click" />
                                            <asp:Button runat="server" ID="btnExpatSubmit" Text="Submit" CssClass="button" OnClick="btnExpatSubmit_Click" />

                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblExpatPositionTitle" class="control-label">Position Title <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatPositionTitle" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatCostCentre" class="control-label">Cost Centre </label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtexpatCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group" id="dvlblExpatBU" runat="server">
                                                    <label id="lstExpatBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:Label ID="lblExpatBusinessUnit" runat="server"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" id="dvdrpExpatBU" runat="server">
                                                    <label class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls" style="padding-top: 5px">
                                                        <asp:DropDownList ID="ddlExpatBusinessUnit" AutoPostBack="true" CssClass="border-radius-none span12" runat="server" OnSelectedIndexChanged="ddlExpatBusinessUnit_SelectedIndexChanged">
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatWorkArea" class="control-label">Work Area <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatWorkArea" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatSiteLocatoin" class="control-label">Site Location <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatSiteLocation" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatReportsTo" class="control-label">Reports to <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ReportsToExpatPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />
                                                    </div>
                                                </div>



                                                <div class="control-group">
                                                    <label id="lblExpatProStartDate" class="control-label">Effective Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ExpatEffectiveTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label22" class="control-label">Contract Period (Years) <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractPeriods" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatTermEndDate" class="control-label">Contract End Date <span id="spFxTerm2" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ExpatContractDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label27" class="control-label">Next Salary Review <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatNextReview" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label28" class="control-label">Home Location <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatHomeLocation" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label23" class="control-label">Who will sign the letter</label>
                                                    <div class="controls">
                                                        <SharePoint:PeopleEditor ID="ExpatSignPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="true" PlaceButtonsUnderEntityEditor="false" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label24" class="control-label">Notes</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatNotes" TextMode="MultiLine" Rows="3" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">

                                                <h4 class="">Remuneration Details</h4>

                                                <div class="control-group">
                                                    <label id="lblExpatGrade" class="control-label">Grade <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatGrade" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatFAR" class="control-label">FAR <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatFAR" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatSTI" class="control-label">STI</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatSTI" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>


                                                <h4 class="">Personal Details</h4>

                                                <div class="control-group">
                                                    <label id="Label25" class="control-label">Marital Status <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlMaritalStatus" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem>Married</asp:ListItem>
                                                            <asp:ListItem>Single</asp:ListItem>
                                                            <asp:ListItem>De Facto</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">

                                                    <asp:Table ID="DependentsTabls" runat="server" CssClass="EU_DataTable" EnableViewState="true">
                                                        <asp:TableHeaderRow TableSection="TableHeader">
                                                            <asp:TableHeaderCell Width="25%">Dependent</asp:TableHeaderCell>
                                                            <asp:TableHeaderCell Width="30%">Name</asp:TableHeaderCell>
                                                            <asp:TableHeaderCell Width="30%">DOB</asp:TableHeaderCell>
                                                            <asp:TableHeaderCell Width="15%"></asp:TableHeaderCell>
                                                        </asp:TableHeaderRow>

                                                    </asp:Table>

                                                </div>
                                            </div>

                                        </div>
                                        <!-- <div class="row-fluid" style="width: 80%">
                                            <div>
                                                <h4 class="">Offer Checklist (to be completed by HR Manager)</h4>
                                                <asp:CheckBoxList ID="chkbxLstExpat" runat="server">
                                                    <asp:ListItem>VEVO Check Completed (Right to work in Australia)</asp:ListItem>
                                                    <asp:ListItem>Reference Checks</asp:ListItem>
                                                    <asp:ListItem>Resume/Application Form</asp:ListItem>
                                                    <asp:ListItem>Interview Notes</asp:ListItem>
                                                    <asp:ListItem>Psychometric Testing</asp:ListItem>
                                                </asp:CheckBoxList>
                                            </div>
                                        </div>-->
                                    </div>
                                    <!--starts  Successful Applicant here-->
                                    <%-- <div id="SuccessfulApplicant" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnSuccessfulApplicantSave" Text="Save" CssClass="btn btn-primary" OnClick="btnSuccessfulApplicantSave_Click" />

                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblSuccessfulApplicantName" class="control-label">Successful Applicant Name</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtSuccessfulApplicantName" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                 <div class="control-group">
                                                    <label id="lblPosition" class="control-label">Position</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtPosition" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                 <div class="control-group">
                                                    <label id="lblSAPNumber" class="control-label">SAP Number</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtSAPNumber" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>
                                                 <div class="control-group">
                                                    <label id="lblCommencementDate" class="control-label">Commencement Date</label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" ID="CommencementDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>

                                            </div>
                                        </div>
                                    </div>--%>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
            <asp:HiddenField ID="hdnRefNo" runat="server" />
            <div class="clearfix">&nbsp;</div>
            <br />
            <br />
            <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
            <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>
            <script>
                $(document).ready(function () {

                    var PositionType = $('#<%= lblPositionType.ClientID %>').text();
                    if (PositionType == "")
                        PositionType = $('option:selected', $('#<%= ddlPositionType.ClientID %>')).text();


                    if (PositionType == 'Contractor') {
                        MoveToContraTab();
                    }
                    else if (PositionType == 'Salary') {
                        MoveToSalTab();
                    }
                    else if (PositionType == 'Waged') {
                        MoveToWagedTab();
                    }
                    else if (PositionType == 'Expatriate') {
                        MoveToExpatTab();
                    }


                    if ($('option:selected', $('#<%= ddlTypeOfRole.ClientID %>')).text() == 'Fixed Term') {

                        var PositionType = $('#<%= lblPositionType.ClientID %>').text();
                        if (PositionType == "")
                            PositionType = $('option:selected', $('#<%= ddlPositionType.ClientID %>')).text();

                        if (PositionType == 'Salary') {
                            document.getElementById('spFxTerm').style.display = '';
                        }
                        else if (PositionType == 'Waged') {
                            document.getElementById('spFxTerm1').style.display = '';
                        }
                        /*else if (PositionType == 'Contractor') {
                            document.getElementById('spFxTerm3').style.display = '';
                        }*/
                        else if (PositionType == 'Expatriate') {
                            document.getElementById('spFxTerm2').style.display = '';
                        }
                    }

                    $('#<%= ddlTypeOfRole.ClientID %>').on('change', function () {

                        var PositionType = $('#<%= lblPositionType.ClientID %>').text();
                        if (PositionType == "")
                            PositionType = $('option:selected', $('#<%= ddlPositionType.ClientID %>')).text();

                        if (PositionType == 'Salary') {

                            if ($('option:selected', $(this)).text() == 'Fixed Term') {
                                document.getElementById('spFxTerm').style.display = '';
                            }
                            else {
                                document.getElementById('spFxTerm').style.display = 'none';
                            }
                        }
                        else if (PositionType == 'Waged') {

                            if ($('option:selected', $(this)).text() == 'Fixed Term') {
                                document.getElementById('spFxTerm1').style.display = '';
                            }
                            else {
                                document.getElementById('spFxTerm1').style.display = 'none';
                            }
                        }
                        /*else if (PositionType == 'Contractor') {

                            if ($('option:selected', $(this)).text() == 'Fixed Term') {
                                document.getElementById('spFxTerm3').style.display = '';
                            }
                            else {
                                document.getElementById('spFxTerm3').style.display = 'none';
                            }
                        }*/
                        else if (PositionType == 'Expatriate') {

                            if ($('option:selected', $(this)).text() == 'Fixed Term') {
                                document.getElementById('spFxTerm2').style.display = '';
                            }
                            else {
                                document.getElementById('spFxTerm2').style.display = 'none';
                            }
                        }

                    });

                    $('#<%= ddlPositionType.ClientID %>').on('change', function () {


                        if ($('option:selected', $(this)).text() == 'Salary') {
                            $("a[href='#Salary']").trigger("click");

                        }
                        else if ($('option:selected', $(this)).text() == 'Waged') {
                            $("a[href='#Waged']").trigger("click");

                        }
                        else if ($('option:selected', $(this)).text() == 'Contractor') {
                            $("a[href='#Contractor']").trigger("click");

                        }
                        else if ($('option:selected', $(this)).text() == 'Expatriate') {
                            $("a[href='#Expat']").trigger("click");

                        }
                    });

                    $("a[href='#Expat']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Expatriate') {
                                a.options[i].selected = true;
                                //$('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                                //document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                            }
                        }
                    });

                    $("a[href='#Contractor']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Contractor') {
                                a.options[i].selected = true;
                            }
                        }

                    });

                    $("a[href='#Salary']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Salary') {
                                a.options[i].selected = true;
                            }
                        }
                    });

                    $("a[href='#Waged']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Waged') {
                                a.options[i].selected = true;
                            }
                        }
                    });

                    if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Contractor') {

                        $("a[href='#Contractor']").trigger("click");
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Expatriate') {

                        $("a[href='#Expat']").trigger("click");
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Salary') {
                        $("a[href='#Salary']").trigger("click");
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Waged') {
                        $("a[href='#Waged']").trigger("click");
                    }

                });
        function MoveToExpatTab() {

            $("ul.tabs li").removeClass("active");
            $("a[href='#Expat']").parent('li').addClass("active");

            $(".tab-pane").hide();
            $("#Expat").show();
            return false;

        }
        function MoveToSalTab() {


            $("ul.tabs li").removeClass("active");
            $("a[href='#Salary']").parent('li').addClass("active");

            $(".tab-pane").hide();
            $("#Salary").show();
            return false;
        }
        function MoveToContraTab() {
            $("ul.tabs li").removeClass("active");
            $("a[href='#Contractor']").parent('li').addClass("active");

            $(".tab-pane").hide();
            $("#Contractor").show();

            return false;
        }
        function MoveToWagedTab() {

            $("ul.tabs li").removeClass("active");
            $("a[href='#Waged']").parent('li').addClass("active");

            $(".tab-pane").hide();
            $("#Waged").show();


            return false;
        }

        $(".tab-pane").hide();
        //$("ul.tabs li:first").addClass("active").show();
        $(".tab-pane:first").show();
        //On Click Event
        $("ul.tabs li").click(function () {
            if ($('#<%= dvdrpPostionType.ClientID %>').is(':visible')) {
                $("ul.tabs li").removeClass("active");
                $(this).addClass("active");
                $(".tab-pane").hide();
                var activeTab = $(this).find("a").attr("href");
                $(activeTab).fadeIn();


                if ($('option:selected', $('#<%= ddlTypeOfRole.ClientID %>')).text() == 'Fixed Term') {

                    var PositionType = $('#<%= lblPositionType.ClientID %>').text();
                    if (PositionType == "")
                        PositionType = $('option:selected', $('#<%= ddlPositionType.ClientID %>')).text();

                    if (PositionType == 'Salary') {
                        document.getElementById('spFxTerm').style.display = '';
                    }
                    else if (PositionType == 'Waged') {
                        document.getElementById('spFxTerm1').style.display = '';
                    }
                    /*else if (PositionType == 'Contractor') {
                        document.getElementById('spFxTerm3').style.display = '';
                    }*/
                    else if (PositionType == 'Expatriate') {
                        document.getElementById('spFxTerm2').style.display = '';
                    }

                }
                else {
                    document.getElementById('spFxTerm').style.display = 'none';
                    document.getElementById('spFxTerm1').style.display = 'none';
                    //document.getElementById('spFxTerm3').style.display = 'none';
                    document.getElementById('spFxTerm2').style.display = 'none';
                }

                return false;
            }
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

        });

        $(document).ready(function () {
            $('.controlrole-label').html("Insurance <span style='color: red'>*</span>");
        })

            </script>
    </body>
    </html>
</asp:Content>
