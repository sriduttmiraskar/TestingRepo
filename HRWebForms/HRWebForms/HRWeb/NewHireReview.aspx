<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="NewHireReview.aspx.cs" Inherits="HRWebForms.HRWeb.NewHireReview" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">New Hire Review</asp:Content>
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
            .control-labelleft {
                float: left;
                width: 2000px;
                text-align: left;
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
        <div id='termination-web' class="clearfix">
            <div class="row-fluid main-row-heading">

                <div class="container">
                    <h2 class="span6" style="padding-left: 10px;">New Hire Request</h2>
                    <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">Ref No:
                        <asp:Label ID="lblRefNo" runat="server"></asp:Label>
                    </h2>
                </div>
            </div>

            <div class="container margin-bottom-20">
                <div style="padding-bottom: 20px;">
                    <span style="color: red">
                        <asp:Label ID="lblError" runat="server"></asp:Label></span>
                    <span style="float: right;">
                        <asp:Button ID="btnPDF" CssClass="button" runat="server" Text="Generate PDF" OnClick="btnPDF_Click" OnClientClick="resetSharePointSubmitField();" />
                        &nbsp;<asp:Button ID="btnApprove" CssClass="button" runat="server" Text="Approve" OnClick="btnApprove_Click" />&nbsp;
                        &nbsp;<asp:Button ID="btnReject" CssClass="button" runat="server" Text="Reject" OnClick="btnReject_Click" />&nbsp;
                    </span>
                </div>
                <div class="form-horizontal">
                    <div class="row-fluid">

                        <div class="span6" style="padding-left:40px">
                            <div class="control-group1">

                                <label id="FirstName" class="control-label1">First Name:</label>
                                <div class="controls" style="position: relative">
                                    <asp:Label ID="lblFirstName" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Last Name:</label>
                                <div class="controls">
                                    <asp:Label ID="lblLastName" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Address:</label>
                                <div class="controls">
                                    <asp:Label ID="lblAddress" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">City:</label>
                                <div class="controls">
                                    <asp:Label ID="lblCity" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">State:</label>
                                <div class="controls">
                                    <asp:Label ID="lblState" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Post Code:</label>
                                <div class="controls">
                                    <asp:Label ID="lblPostCode" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                        </div>
                        <div class="span6">
                            <div style="padding-top: 10px;" class="control-group1" id="divComments" runat="server">
                                <label id="Label1" class="control-label1">Add Comments</label>
                                <div class="controls">
                                    <asp:TextBox ID="txtComments" TextMode="multiline" Rows="7" CssClass="span12 border-radius-none" runat="server" />
                                </div>
                            </div>
                            <div class="control-group1" style="margin-top: 10px;">
                                <label class="control-label1">Date:</label>
                                <div class="controls">
                                    <asp:Label ID="lblDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <asp:label id="AppToHireRefNoText" class="control-label1" runat="server">App To Hire Ref No:</asp:label>
                                <div class="controls">
                                    <asp:Label ID="lblAppToHireRefNo" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Position type:</label>
                                <div class="controls">
                                    <asp:Label ID="lblPositiontype" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Type Of Role:</label>
                                <div class="controls">
                                    <asp:Label ID="lblTypeOfRole" runat="server" CssClass="border-radius-none span12"></asp:Label>
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
                                    <div id="Salary" class="tab-pane">
                                        <div class="span4 positionAbs text-right">
                                        </div>

                                        <div class="row-fluid">

                                            <div class="span6">

                                                <div id="DivSalaryPositionDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Position Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="lblPostionHeader" class="control-label1">Position Title:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblPositionTitle" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="CostCentre" class="control-label1">Cost Centre:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblCostCentre" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Business Unit:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblBusinessUnit" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Work Area:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWorkArea" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Site Location:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblSiteLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Reports to:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblReportsTo" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="TypeofContract" class="control-label1">Type of Contract:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblTypeofContract" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="CommencementDate" class="control-label1">Commencement Date:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblCommencementDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="TermEndDate" class="control-label1">Term End Date:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblTermEndDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="NewSalaryReview" class="control-label1">Next Salary Review:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblNextSalaryReview" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Whowillsign" class="control-label1">Who will sign the<br /> letter:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblWhowillsign" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Notes" class="control-label1">Notes:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblNotes" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>

                                                </div>

                                                <div id="DivWagedPositionDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Position Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label7" class="control-label1">Position Title:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedPositionTitle" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label15" class="control-label1">Cost Centre:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedCostCentre" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label19" class="control-label1">Business Unit:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedBusinessUnit" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label23" class="control-label1">Work Area:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedWorkArea" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label26" class="control-label1">Site Location:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedSiteLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label28" class="control-label1">Reports to:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedReportsto" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label32" class="control-label1">Commencement Date:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedCommencementDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label34" class="control-label1">Term End Date:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblWagedTermEndDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label38" class="control-label1">Who will sign the<br /> letter:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblWagedWhowillsign" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label40" class="control-label1">Notes:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblWagedNotes" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div id="DivContractorPositionDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Position Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label2" class="control-label1">Position Title:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorPositionTitle" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label4" class="control-label1">Agency/Company/<br />Trading Name:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorCompany" runat="server" CssClass="border-radius-none span6"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label3" class="control-label1">ABN:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorABN" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label6" class="control-label1">Business Unit:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorBusinessUnit" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label8" class="control-label1">Work Area:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorWorkArea" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label10" class="control-label1">Site Location:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorSiteLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label12" class="control-label1">Reports to:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorReportsto" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label14" class="control-label1">Cost Centre:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorCostCentre" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label16" class="control-label1">Contract Rate<br /> (ex GST):</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblContractorContractRate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label18" class="control-label1">Rate Type Field:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorRateTypeField" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label20" class="control-label1">Contract Start Date:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorContractStartDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label22" class="control-label1">Contract End Date:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorContractEndDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label24" class="control-label1">Payment Terms:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorPaymentTerms" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label5" class="control-label1">If other (specify):</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorIfother" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label9" class="control-label1">GST:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorGST" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label13" class="control-label1">Who will sign the<br /> Contract:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblContractorWhoWillSign" runat="server" CssClass="border-radius-none span6"></asp:Label>
                                                        </div>
                                                    </div>

                                                </div>

                                                <div id="DivExpatPositionDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Position Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label11" class="control-label1">Position Title:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatPositionTitle" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label21" class="control-label1">Cost Centre:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatCostCentre" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Business Unit:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatBusinessUnit" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Work Area:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatWorkArea" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Site Location:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatSiteLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Reports to:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatReportsto" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label33" class="control-label1">Effective Date:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatEffectiveDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label36" class="control-label1">Contract Period<br /> (Years):</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatContractPeriod" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label39" class="control-label1">Contract End Date:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblExpatContractEndDate" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label42" class="control-label1">New Salary Review:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblExpatNextSalaryReview" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label17" class="control-label1">Home Location:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblExpatHomeLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label44" class="control-label1">Who will sign the<br /> letter:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblExpatWhowillsign" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label46" class="control-label1">Notes:</label>
                                                        <div class="controls" style="position: relative">
                                                            <asp:Label ID="lblExpatNotes" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>

                                                </div>

                                            </div>

                                            <div class="span6">

                                                <div id="divSalaryRemunerationDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Remuneration Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Grade:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblGrade" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">FAR:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblFAR" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">STI:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblSTI" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Vehicle:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblVehicle" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">If other (specify):</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblIfOther" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Relocation:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblRelocation" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Relocation Details:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblRelocationDetails" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                </div>

                                                <div id="divWagedRemunerationDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Remuneration Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Pay Level:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedPayLevel" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Roster Type:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedRosterType" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Crew:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedCrew" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Shift Team Leader:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedShiftTeamLeader" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Allowances:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedAllowances" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Vehicle:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedVehicle" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">If other (specify):</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedIfOthers" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>

                                                </div>

                                                <div id="divContractorJobDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Job Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label for="lblInsurance" class="control-labelleft">Insurance:</label>
                                                    </div>
                                                    <div class="control-group1">
                                                        <div class="mainframe span12 scroll">
                                                            <div id='Div1' class="clearfix">
                                                                <div id="Div2" class="row-fluid" runat="server">
                                                                    <asp:Table ID="tblAttachment" runat="server" CssClass="span4" Width="100%" EnableViewState="true">
                                                                        <asp:TableHeaderRow ID="TableHeaderRow1" runat="server">
                                                                            <asp:TableHeaderCell ID="TableHeaderCell1" runat="server" Width="5%" HorizontalAlign="Left"></asp:TableHeaderCell>
                                                                            <asp:TableHeaderCell ID="TableHeaderCell2" runat="server" Width="15%" HorizontalAlign="Left">File Type</asp:TableHeaderCell>
                                                                            <asp:TableHeaderCell ID="TableHeaderCell3" runat="server" Width="60%" HorizontalAlign="Left">Name</asp:TableHeaderCell>
                                                                            <asp:TableHeaderCell ID="TableHeaderCell4" runat="server" Width="20%" HorizontalAlign="Left">Date</asp:TableHeaderCell>
                                                                        </asp:TableHeaderRow>
                                                                    </asp:Table>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <br />
                                                    <div class="control-group1">
                                                        <div>
                                                            <label class="control-label1">Services To Be Provided / Primary Objectives </label>
                                                        </div>
                                                        <div style="padding-left:5px" >
                                                            <asp:Label  ID="lblContractorServicesToBe" runat="server" CssClass="border-radius-none span12" ></asp:Label>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div id="divExpatRemunerationDetails" runat="server">
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Remuneration Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Grade:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatGrade" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">FAR:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatFAR" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">STI:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatSTI" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="margin-bottom-20 row-fluid">
                                                        <h4 class="span6">Personal Details</h4>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="MaritalStatus" class="control-label1">Marital Status:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblMaritalStatus" class="border-radius-none span12" runat="server"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label id="Label25" class="control-label1">Dependents:</label>
                                                    </div>
                                                    <div class="clearfix">
                                                        <asp:Table ID="DependentsTable" runat="server" CssClass="EU_DataTable1" EnableViewState="true">
                                                        </asp:Table>
                                                    </div>
                                                </div>

                                            </div>

                                        </div>

                                        <div id="DivHRManagerCheckList" class="row-fluid" style="width: 80%" runat="server">
                                            <div>
                                                <h4 class="">Offer Checklist</h4>
                                                <asp:CheckBoxList ID="chkbxLstExpat" runat="server">
                                                    <asp:ListItem Text = "Immigration Requirements completed" Value="Immigration"></asp:ListItem>
                                                    <asp:ListItem Text = "Reference Checks" Value="ReferenceCheck"></asp:ListItem>
                                                    <asp:ListItem Text = "Resume/Application Form" Value="Resume"></asp:ListItem>
                                                    <asp:ListItem Text = "Interview Notes" Value="InterviewNotes"></asp:ListItem>
                                                    <asp:ListItem Text = "Psychometric Testing" Value="PsychometricTesting"></asp:ListItem>
                                                </asp:CheckBoxList>
                                            </div>
                                        </div>

                                        <div class="row-fluid" style="width: 80%">
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


        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>

    </body>

    </html>
</asp:Content>
