<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="AppToHireRequest.aspx.cs" Inherits="HRWebForms.HRWeb.AppToHireRequest" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<%@ Register TagPrefix="Custom" TagName="UserControl" Src="~/_ControlTemplates/HRWebForms/UploadJobUserControl.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Application To Hire</asp:Content>
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
                <h2 class="span6" style="padding-left:10px;">Application to Hire</h2>
                <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">
                    Ref No: <asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
            </div>
            <div style="float:right;margin-right:20px;margin-bottom:20px;font-style:italic">Fields marked <span style="color:red">*</span> are mandatory</div>
            <div>
                <span style="color: red">
                    <asp:Label ID="lblError" runat="server"></asp:Label></span>
            </div>
            <div class="container margin-bottom-20">
                <div class="form-horizontal">
                    <div class="row-fluid">

                        <div class="span6">
                            <div class="control-group">
                                <asp:Label ID="lblAppToHireRequest" runat="server"></asp:Label>
                                <label id="lblDate" class="control-label">Date: </label>
                                <div class="controls" style="margin-top:5px;">
                                    <!--<SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="DateofRequest" DateOnly="true" CssClassTextBox="border-radius-none span12" />-->
                                    <asp:Label ID="lblDateNow" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group">
                                <label id="lblPositionType" class="control-label">Position Type <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:DropDownList ID="ddlPositionType" CssClass="border-radius-none span12 ddlPositionType" runat="server">
                                    </asp:DropDownList>&nbsp;
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image11" ToolTip="Contractors are not paid through payroll and provide an invoice.&#013;Salary employees  paid monthly.&#013;Wage Employee covered by Enterprise Agreement.&#013;Expatriate based overseas." runat="server" />
                                </div>
                            </div>
                            <div class="control-group">
                                <label id="lblReasonPositionRqd" class="control-label">
                                    Reason Position<br />
                                    Required <span style="color: red">*</span></label>
                                <div class="controls">
                                    <asp:DropDownList ID="ddlReasonPositionRqd" CssClass="border-radius-none span12" runat="server">
                                    </asp:DropDownList>

                                </div>
                            </div>
                            <div class="control-group">
                                <label id="lblReplacePosition" class="control-label">Replacement for Position Held by<span style="color: red">*</span></label>

                                <div class="controls">
                                    <asp:TextBox ID="txtPositionHeldBy" runat="server" CssClass="border-radius-none span12"></asp:TextBox>


                                </div>
                            </div>
                            <div class="control-group">
                                <label id="lblBudgetPosition" class="control-label">Budgeted Position <span style="color: red">*</span></label>

                                <div class="controls">
                                    <asp:DropDownList ID="ddlBudgetPosition" CssClass="border-radius-none span12" runat="server">
                                        <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                        <asp:ListItem Value="No">No</asp:ListItem>
                                    </asp:DropDownList>

                                </div>
                            </div>

                            <div class="control-group">
                                <label id="lblStaffingLevel" class="control-label">Is this an increase in staffing levels <span style="color: red">*</span></label>

                                <div class="controls">
                                    <asp:DropDownList ID="ddlStaffingLevel" CssClass="border-radius-none span12" Width="90%" runat="server">
                                        <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                        <asp:ListItem Value="No">No</asp:ListItem>
                                    </asp:DropDownList>

                                </div>
                            </div>

                        </div>
                        <div class="span6">
                            <div class="control-group">
                                <label id="lblRequiredBy" class="control-label">Required by <span style="color: red">*</span></label>

                                <div class="controls">
                                    <SharePoint:PeopleEditor Width="370px" ID="RequiredByPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="false" PlaceButtonsUnderEntityEditor="false" ValidateResolvedEntity ="false" />


                                </div>
                            </div>

                            <div class="control-group">
                                <label id="lblComments" class="control-label">Comments</label>

                                <div class="controls">
                                    <asp:TextBox Width="360px" ID="txtComments" TextMode="multiline" Rows="3"  runat="server" />&nbsp;
                                    <!--<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" CssClass="imgicon" ID="Image3" ToolTip="Include any specific details relating to this application. Eg: If using other in vehicle option, please specify." runat="server" />-->
                                </div>
                            </div>
                            <div class="control-group">
                                <label id="lblRecruitmentProc" class="control-label">Recruitment Process <span style="color: red">*</span></label>

                                <div class="controls">
                                    <asp:DropDownList ID="ddlRecruitmentProc" Width="360px" runat="server">
                                    </asp:DropDownList>

                                </div>
                            </div>
                            <div class="control-group">

                                <label id="lblDetails" class="control-label">Details</label>

                                <div class="controls">

                                    <asp:TextBox Width="360px" ID="txtDetails" TextMode="multiline" Rows="3" runat="server" />&nbsp;
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image1" CssClass="imgicon" ToolTip="Include details of recruitment process recruiters being considered, internal/external advertising, confidential role." runat="server" />

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
                            <!--<li class=""><a href="#SuccessfulApplicant " class="">Successful Applicant</a></li>-->
                        </ul>
                        <div class="tab-content">
                            <div class="form-horizontal">
                                <div class="margin-bottom-20 row-fluid">
                                    <!--- Tab Salary started-->
                                    <div id="Salary" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>

                                            <h4 class="span6">Job Details&nbsp;<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image2" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" /></h4>
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

                                                    <label id="lblSAPPositionNo" class="control-label">
                                                        SAP Position No</label>

                                                    <div class="controls">
                                                        <asp:TextBox ID="txtSAPPositionNo" CssClass="border-radius-none span12" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="imgURL" ToolTip="SAP Position No can be found SAP Employee Records" runat="server" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>

                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlBusinessUnit" CssClass="border-radius-none span12" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBusinessUnit_SelectedIndexChanged">
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
                                                        <SharePoint:PeopleEditor Width="320px" ID="ReportsToPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="false" ValidateResolvedEntity ="false" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblTypeofPosition" class="control-label">Type of Position <span id="spPos" style="color: red;">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlTypeOfPosition" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblProStartDate" class="control-label">Proposed Start Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="StartDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblFixedEndDate" class="control-label">Fixed Term End Date <span id="spFxTerm" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="EndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">


                                                <Custom:UserControl id="MyCustomControl" runat="server" />
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
                                                        <asp:TextBox ID="txtIfOthers" Rows="3" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image4" ToolTip="Include any specific details relating to this application. Eg: If using other in vehicle option, please specify." runat="server" />
                                                    </div>
                                                </div>
                                            </div>

                                        </div>

                                    </div>
                                    <!--- Tab Waged started-->
                                    <div id="Waged" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>

                                            <h4 class="span6">Job Details&nbsp;<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image8" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" /></h4>
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
                                                    <label id="lblWagedSAPPositionNo" class="control-label">SAP Position No</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedSAPPositionNo" CssClass="border-radius-none span12" runat="server"></asp:TextBox>&nbsp;
                                                        <span style="color: red">
                                                            <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image5" ToolTip="SAP Position No can be found SAP Employee Records" runat="server" /></span>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedBusinessUnit" CssClass="border-radius-none span12" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlWagedBusinessUnit_SelectedIndexChanged">
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
                                                        <SharePoint:PeopleEditor ID="ReportsToWagedPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="false" ValidateResolvedEntity ="false" PlaceButtonsUnderEntityEditor="false" />


                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtWagedCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedTypeOfPosition" class="control-label">Type of Position <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedTypOfPosition" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblWagedProposedStartDate" class="control-label">Proposed Start Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="WagedStartDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedFixedEndDate" class="control-label">Fixed Term End Date <span id="spFxTerm1" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="WagedEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>

                                            </div>



                                            <div class="span6">

                                                <Custom:UserControl id="UserControl1" runat="server" />


                                                <h4 class="">Remuneration Details</h4>

                                                <div class="control-group">
                                                    <label id="lblWagedLevel" class="control-label">Level <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedLevel" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblWagedShiftRotation" class="control-label">Shift Rotation <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlWagedShiftRotation" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

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
                                                        <asp:TextBox ID="txtWagedIfOther" Rows="3" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image10" ToolTip="Other documents relevant to the 'Application to Hire' can be included here." runat="server" />
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <!--- Tab Contractor started-->
                                    <div id="Contractor" class="tab-pane active">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>

                                            <h4 class="span6">Job Details&nbsp;<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image6" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" /></h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnContractorSave" Text="Save" CssClass="button" OnClick="btnContractorSave_Click" />
                                            <asp:Button runat="server" ID="btnContractorSubmit" Text="Submit" CssClass="button" OnClick="btnContractorSubmit_Click" />

                                        </div>
                                        <div class="row-fluid">

                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblContraRole" class="control-label">Role <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContraRole" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblContraBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContraBusinessUnit" CssClass="border-radius-none span12" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlContraBusinessUnit_SelectedIndexChanged">
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
                                                        <SharePoint:PeopleEditor ID="ReportsToContractorPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="false" ValidateResolvedEntity ="false" PlaceButtonsUnderEntityEditor="false" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContraCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraTypeofPosition" class="control-label">Type of Contract Agreement <span style="color: red;">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlContraTypeofPosition" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="Label1" class="control-label">Contract Rate <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtContractRate" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblContraProStartDate" class="control-label">Effective Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ContraStartDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblContraFixedEndDate" class="control-label">Contract End Date <span id="spFxTerm3" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ContraEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">

                                                <Custom:UserControl id="UserControl2" runat="server" />
                                                <div class="control-group">
                                                    <label id="lblContraDeliverables" class="">Contract Deliverables / Role Statement Comments: <span style="color: red">*</span></label>
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

                                            <h4 class="span6">Job Details&nbsp;<asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image9" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" /></h4>
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
                                                    <label id="lstExpatBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatBusinessUnit" CssClass="border-radius-none span12 " Rows="3" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlExpatBusinessUnit_SelectedIndexChanged">
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
                                                        <SharePoint:PeopleEditor ID="ReportsToExpatPeopleEditor" runat="server" AllowEmpty="true" CssClass="border-radius-none span12" SelectionSet="User" MultiSelect="false" ValidateResolvedEntity ="false" ValidateRequestMode="Disabled" PlaceButtonsUnderEntityEditor="false" />
                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtexpatCostCentre" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatTypeofPosition" class="control-label">Type of Position <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatTypeofPosition" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                                <div class="control-group">
                                                    <label id="lblExpatProStartDate" class="control-label">Proposed Start Date <span style="color: red">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ExpatStartDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatTermEndDate" class="control-label">Fixed Term End Date <span id="spFxTerm2" style="color: red; display: none">*</span></label>
                                                    <div class="controls" style="position: relative">
                                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ExpatEndDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">


                                                <Custom:UserControl id="UserControl3" runat="server" />


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
                                                <!--<div class="control-group">
                                                    <label id="lblExpatUtilities" class="control-label">Utilities <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatUtilities" CssClass="border-radius-none span12" runat="server">
                                                            <asp:ListItem Value="yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="no">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatRelocation" class="control-label">Relocation <span style="color: red">*</span></label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatRelocation" CssClass="border-radius-none span12" runat="server"></asp:TextBox>

                                                    </div>
                                                </div>-->
                                                <div class="control-group">
                                                    <label id="lblExpatVehicle" class="control-label">Vehicle</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlExpatVehicle" CssClass="border-radius-none span12" runat="server">
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group">
                                                    <label id="lblExpatIfOther" class="control-label">If other (specify)</label>
                                                    <div class="controls">
                                                        <asp:TextBox ID="txtExpatIfother" TextMode="MultiLine" Rows="3" CssClass="span12 border-radius-none" runat="server"></asp:TextBox>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image7" CssClass="imgicon" ToolTip="Other documents relevant to the Application to Hire can be included here." runat="server" />

                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <!--starts  Successful Applicant here-->
                                    <!-- <div id="SuccessfulApplicant" class="tab-pane">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnSuccessfulApplicantSave" Text="Save" CssClass="btn btn-primary"  />

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
                                    </div>-->
                                </div>

                                <!-- Comments History section started-->
                                <div class="row-fluid" style="width: 80%" id="divHistory" runat="server">
                                            <div>
                                                <h4 class="">Approval History:</h4>
                                                <asp:GridView ID="gdCommentHistory" CssClass="EU_DataTable" runat="server" AutoGenerateColumns="false" Width="100%">
                                                    <Columns>
                                                        <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True">
                                                            <HeaderStyle Width="20%" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="20%" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:BoundField>
                                                        <asp:BoundField DataField="UserName" HeaderText="UserName">
                                                            <HeaderStyle Width="40%" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="40%" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:BoundField>

                                                        <asp:TemplateField HeaderText="Comments">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblComments" runat="server" Text='<%# Bind("Comments") %>'></asp:Label>
                                                            </ItemTemplate>
                                                            <HeaderStyle Width="40%" Wrap="true" HorizontalAlign="Left" CssClass="Griditem" />
                                                            <ItemStyle Width="40%" Wrap="true" VerticalAlign="Top" CssClass="Griditem" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <EmptyDataTemplate>
                                                        No Records are found.
                                                    </EmptyDataTemplate>
                                                </asp:GridView>
                                            </div>

                                        </div>
                                 <!-- Comments History section end-->

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

                    if ($('option:selected', $('#<%= ddlTypeOfPosition.ClientID %>')).text() == 'Fixed Term') {
                        document.getElementById('spFxTerm').style.display = '';
                    }
                    else {
                        document.getElementById('spFxTerm').style.display = 'none';
                    }

                    if ($('option:selected', $('#<%= ddlWagedTypOfPosition.ClientID %>')).text() == 'Fixed Term') {
                        document.getElementById('spFxTerm1').style.display = '';
                    }
                    else {
                        document.getElementById('spFxTerm1').style.display = 'none';
                    }

                    if ($('option:selected', $('#<%= ddlContraTypeofPosition.ClientID %>')).text() == 'Fixed Term') {
                        document.getElementById('spFxTerm3').style.display = '';
                    }
                    else {
                        document.getElementById('spFxTerm3').style.display = 'none';
                    }

                    if ($('option:selected', $('#<%= ddlExpatTypeofPosition.ClientID %>')).text() == 'Fixed Term') {
                        document.getElementById('spFxTerm2').style.display = '';
                    }
                    else {
                        document.getElementById('spFxTerm2').style.display = 'none';
                    }


                    $('#<%= ddlTypeOfPosition.ClientID %>').on('change', function () {

                        if ($('option:selected', $(this)).text() == 'Fixed Term') {
                            document.getElementById('spFxTerm').style.display = '';
                        }
                        else {
                            document.getElementById('spFxTerm').style.display = 'none';
                        }

                    });

                    $('#<%= ddlWagedTypOfPosition.ClientID %>').on('change', function () {

                        if ($('option:selected', $(this)).text() == 'Fixed Term') {
                            document.getElementById('spFxTerm1').style.display = '';
                        }
                        else {
                            document.getElementById('spFxTerm1').style.display = 'none';
                        }

                    });


                    $('#<%= ddlContraTypeofPosition.ClientID %>').on('change', function () {

                        if ($('option:selected', $(this)).text() == 'Fixed Term') {
                            document.getElementById('spFxTerm3').style.display = '';
                        }
                        else {
                            document.getElementById('spFxTerm3').style.display = 'none';
                        }

                    });

                    $('#<%= ddlExpatTypeofPosition.ClientID %>').on('change', function () {

                        if ($('option:selected', $(this)).text() == 'Fixed Term') {
                            document.getElementById('spFxTerm2').style.display = '';
                        }
                        else {
                            document.getElementById('spFxTerm2').style.display = 'none';
                        }

                    });
                    $("a[href='#Contractor']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Contractor') {
                                a.options[i].selected = true;
                                $('.controlrole-label').html("Contract Deliverables/Role Statement <span style='color: red'>*</span>");
                                document.getElementById('lblReplacePosition').innerHTML = "Reason Contract<br>Required <span style='color:red'>*</span>";
                            }
                        }

                    });


                    $("a[href='#Expat']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Expatriate') {
                                a.options[i].selected = true;
                                $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                                document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                            }
                        }
                    });

                    $("a[href='#Salary']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Salary') {
                                a.options[i].selected = true;
                                $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                                document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                            }
                        }
                    });

                    $("a[href='#Waged']").click(function () {
                        var a = document.getElementById('<%= ddlPositionType.ClientID %>');
                        for (i = 0; i < a.length; i++) {

                            if (a.options[i].text == 'Waged') {
                                a.options[i].selected = true;
                                $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                                document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                            }
                        }
                    });

                    if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Contractor') {

                        $("a[href='#Contractor']").trigger("click");
                        $('.controlrole-label').html("Contract Deliverables/Role Statement <span style='color: red'>*</span>");
                        document.getElementById('lblReplacePosition').innerHTML = "Reason Contract<br>Required <span style='color:red'>*</span>";
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Expatriate') {

                        $("a[href='#Expat']").trigger("click");
                        $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                        document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Salary') {
                        $("a[href='#Salary']").trigger("click");
                        $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                        document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                    }
                    else if ($('option:selected', $('#<%= ddlPositionType.ClientID %>')).text() == 'Waged') {
                        $("a[href='#Waged']").trigger("click");
                        $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                        document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                    }
                });
        $(".tab-pane").hide();
        //$("ul.tabs li:first").addClass("active").show();
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
            $('#<%= ddlPositionType.ClientID %>').on('change', function () {


                if ($('option:selected', $(this)).text() == 'Salary') {
                    $("a[href='#Salary']").trigger("click");
                    $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                    document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                }
                else if ($('option:selected', $(this)).text() == 'Waged') {
                    $("a[href='#Waged']").trigger("click");
                    $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                    document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                }
                else if ($('option:selected', $(this)).text() == 'Contractor') {
                    $("a[href='#Contractor']").trigger("click");
                    $('.controlrole-label').html("Contract Deliverables/Role Statement <span style='color: red'>*</span>");
                    document.getElementById('lblReplacePosition').innerHTML = "Reason Contract<br>Required <span style='color:red'>*</span>";
                }
                else if ($('option:selected', $(this)).text() == 'Expatriate') {
                    $("a[href='#Expat']").trigger("click");
                    $('.controlrole-label').html("Attached updated Role Statement <span style='color: red'>*</span>");
                    document.getElementById('lblReplacePosition').innerHTML = "Replacement for Position Held by <span style='color:red'>*</span>";
                }
            });


        });

            </script>
    </body>
    </html>
</asp:Content>
