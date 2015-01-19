<%@ Assembly Name="HRWebForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c8c0e2f713937cc8" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="AppToHireReview.aspx.cs" Inherits="HRWebForms.HRWeb.AppToHireReview" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Application To Hire Review</asp:Content>
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
                    <h2 class="span6" style="padding-left:10px;">Application to Hire</h2>
                    <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">
                        Ref No: <asp:Label ID="lblRefNo" runat="server"></asp:Label>
                    </h2>
                </div>
            </div>

            <div class="container margin-bottom-20">
                <div style="padding-bottom: 20px;">
                    <span style="color: red">
                        <asp:Label ID="lblError" runat="server"></asp:Label></span>
                    <span style="float:right;">
                        <asp:Button ID="btnPDF" CssClass="button" runat="server" Text="Generate PDF" OnClick="btnPDF_Click" OnClientClick="resetSharePointSubmitField();" />
                        &nbsp;<asp:Button ID="btnApprove" CssClass="button"  runat="server" Text="Approve" OnClick="btnApprove_Click" />&nbsp;
                        &nbsp;<asp:Button ID="btnReject" CssClass="button" runat="server" Text="Reject" OnClick="btnReject_Click" />&nbsp;
                        <asp:Button ID="btnBack" CssClass="button" runat="server" Text="Back To Initiator" OnClick="btnBack_Click" />
                    </span>
                </div>
                <div class="form-horizontal">
                    <div class="row-fluid">

                        <div class="span6">
                            <div class="control-group1">

                                <label id="Date" class="control-label1">Date:</label>
                                <div class="controls2" style="position: relative">
                                    <asp:Label ID="lblDate" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Position Type:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblPositionType" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Reason Position Required:</label>
                                <div class="controls2">
                                    <asp:Label ID="lblReasonPositionRqd" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Replacement for Position Held by:</label>

                                <div class="controls2">
                                    <asp:Label ID="lblReplacePosition" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                </div>
                            </div>
                            <div class="control-group1">
                                <label class="control-label1">Budgeted Position:</label>

                                <div class="controls2">
                                    <asp:Label ID="lblBudgetPosition" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>

                            <div class="control-group1">
                                <label class="control-label1">Is this an increase in staffing levels:</label>

                                <div class="controls2">
                                    <asp:Label ID="lblStaffingLevel" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>

                            <div class="control-group1">
                                <label class="control-label1">Recruitment Process:</label>

                                <div class="controls2">
                                    <asp:Label ID="lblRecruitmentProcess" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>

                            <div class="control-group1">
                                <label class="control-label1">Details:</label>

                                <div class="controls2">
                                    <asp:Label ID="lblDetails" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                </div>
                            </div>

                        </div>
                        <div class="span6">

                            <div class="control-group1" style="margin-top:10px;">
                                <label class="control-label1">Required by:</label>

                                <div class="controls">
                                    <asp:Label ID="lblRequiredBy" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                </div>
                            </div>

                             <div class="control-group1">
                                <label class="control-label1">Comments:</label>

                                <div class="controls">
                                    <asp:Label ID="lblcomments" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                </div>
                            </div>

                            <div style="padding-top:10px;" class="control-group1" id="divComments" runat="server">
                                <label id="lblComments" class="control-label1">Add Comments</label>
                                <div class="controls">
                                    <asp:TextBox ID="txtComments" TextMode="multiline" Rows="7" CssClass="span12 border-radius-none" runat="server" />
                                </div>
                            </div>
                            <!--starts  Successful Applicant here-->
                            <div id="SuccessfulApplicantEdit" class="tab-pane" runat="server">
                            <div class="margin-bottom-20 row-fluid">
                                <h4 class="span6">Successful Applicant Details</h4>
                            </div>
                            
                            <div class="row-fluid">
                                
                                    <div class="control-group">
                                        <label id="lblSuccessfulApplicantName" class="control-label1" style="padding-top:0px;width:150px">Successful Applicant Name</label>
                                        <div class="controls">
                                            <asp:TextBox ID="txtSuccessfulApplicantName" runat="server" CssClass="border-radius-none span12" ></asp:TextBox>
                                        </div>
                                    </div>
                                        <div class="control-group">
                                        <label id="lblPosition" class="control-label1" style="padding-top:0px">Position</label>
                                        <div class="controls">
                                            <asp:TextBox ID="txtPosition" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                        </div>
                                    </div>
                                        <div class="control-group">
                                        <label id="lblSAPNumber" class="control-label1" style="padding-top:0px">SAP Number</label>
                                        <div class="controls">
                                            <asp:TextBox ID="txtSAPNumber" CssClass="border-radius-none span12" runat="server"></asp:TextBox>
                                        </div>
                                    </div>
                                        <div class="control-group">
                                        <label id="lblCommencementDate" class="control-label1" style="padding-top:0px">Commencement Date</label>
                                        <div class="controls" style="position: relative">
                                            <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="CommencementDateTimeControl" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                        </div>
                                    </div>
                                
                            </div>
                        </div>
                            <div id="SuccessfulApplicantRead" class="tab-pane" runat="server">
                            <div class="margin-bottom-20 row-fluid">
                                <h4 class="span6">Successful Applicant Details</h4>
                            </div>
                            
                            <div class="row-fluid">
                                
                                    <div class="control-group1">
                                        <label id="Label1" class="control-label1" style="padding-top:0px;width:150px">Successful Applicant Name:</label>
                                        <div class="controls">
                                            <asp:Label ID="lblSAName" runat="server" Width="100px"  ></asp:Label>
                                        </div>
                                    </div>
                                        <div class="control-group1">
                                        <label id="Label2" class="control-label1" style="padding-top:0px">Position:</label>
                                        <div class="controls">
                                            <asp:Label ID="lblSAPos"  runat="server"></asp:Label>
                                        </div>
                                    </div>
                                        <div class="control-group1" >
                                        <label id="Label3" class="control-label1" style="padding-top:0px">SAP Number:</label>
                                        <div class="controls">
                                            <asp:Label ID="lblSASAP"  runat="server"></asp:Label>
                                        </div>
                                    </div>
                                        <div class="control-group1">
                                        <label id="Label4" class="control-label1" style="padding-top:0px">Commencement Date:</label>
                                        <div class="controls" style="position: relative">
                                            <asp:Label ID="lblSACommDate" runat="server"></asp:Label>
                                        </div>
                                    </div>
                                
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
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Position Details</h4>

                                            <h4 class="span6">Job Details</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                        </div>

                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group1">

                                                    <asp:Label ID="lblPostionHeader" CssClass="control-label1" runat="server">Position Title:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblPositionTitle" runat="server" CssClass="border-radius-none span12"></asp:Label>
                                                    </div>

                                                </div>
                                                <div class="control-group1">

                                                    <asp:Label ID="lblSAPPositionHeader" CssClass="control-label1" runat="server">SAP Position No:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblSAPPositionNo" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">
                                                  <asp:Label class="control-label1" runat="server">Business Unit:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblBusinessUnit" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">
                                                    <asp:Label class="control-label1" runat="server">Work Area:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblWorkArea" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">
                                                   <asp:Label class="control-label1" runat="server">Site Location:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblSiteLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">

                                                   <asp:Label class="control-label1" runat="server">Reports to:</asp:Label>

                                                    <div class="controls">
                                                        <asp:Label ID="lblReportsTo" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group1">

                                                    <asp:Label ID="lblCostCentreHeader" runat="server" CssClass="control-label1">Cost Centre:</asp:Label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblCostCentre" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                                    </div>
                                                </div>
                                                <div class="control-group1">
                                                    <asp:Label ID="lblTypePositionHeader" runat="server" CssClass="control-label1">Type of Position:</asp:Label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblTypeofPosition" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">

                                                    <asp:Label ID="lblContractRateHeader" runat="server" CssClass="control-label1">Contract Rate:</asp:Label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblContractRate" runat="server" CssClass="border-radius-none span12"></asp:Label>


                                                    </div>
                                                </div>
                                                <div class="control-group1">

                                                    <asp:Label ID="lblProStartDateHeader" runat="server" CssClass="control-label1">Proposed Start Date</asp:Label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblProStartDate" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group1">

                                                    <asp:Label ID="lblFixedTermHeader" runat="server" CssClass="control-label1">Fixed Term End Date:</asp:Label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblFixedEndDate" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">


                                                <div class="control-group1">
                                                    <label for="inputPassword" class="control-labelleft">Attached updated Role Statement:</label>

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

                                                <h4 class="">Remuneration Details</h4>
                                                <div id="dvSalaryRenum" runat="server">
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
                                                        <label class="control-label1">If other (specify)</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblIfOther" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                </div>
                                                <div id="dvWageed" runat="server">
                                                    <div class="control-group1">
                                                        <label class="control-label1">Level:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblWagedLevel" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Shift Rotation:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblShiftLocation" runat="server" CssClass="border-radius-none span12"></asp:Label>

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
                                                            <asp:Label ID="lblWagedIfAny" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>

                                                </div>

                                                <div id="dvContractor" runat="server">
                                                    <div class="control-group1">
                                                        <label class="control-label1">Contract Deliverables / Role Statement:</label>
                                                        <div class="controls-role">
                                                            <asp:Label ID="lblContractDelivery" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>


                                                </div>

                                                <div id="dvExpat" runat="server">
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
                                                   <!-- <div class="control-group1">
                                                        <label class="control-label1">Utilities:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatUtilities" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">Relocation:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatRelocation" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>-->
                                                    <div class="control-group1">
                                                        <label class="control-label1">Vehicle:</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatVehicle" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                    <div class="control-group1">
                                                        <label class="control-label1">If other (specify):</label>
                                                        <div class="controls">
                                                            <asp:Label ID="lblExpatIfAny" runat="server" CssClass="border-radius-none span12"></asp:Label>

                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                        </div>

                                        <div class="row-fluid" style="width: 100%">
                                            <div>
                                                <h4 class="">Approval History:</h4>
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
            </div>
        </div>
        <br />
        <br />


        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>

    </body>

    </html>
</asp:Content>
