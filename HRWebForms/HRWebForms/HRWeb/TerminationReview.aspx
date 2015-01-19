<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>


<%@ Page Language="C#" CodeBehind="TerminationReview.aspx.cs" Inherits="HRWebForms.HRWeb.TerminationReview" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

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
                width: 20% !important;
                margin-left: 75px !important;
            }

            .control-labelRight {
                margin-top: 5px;
            }

            .label-Review {
                float: left;
                width: 400px;
                padding-top: 0px;
                margin-bottom: 5px;
                margin-right: 10px;
                padding-left: 5px;
                font-weight: bold;
            }

            .label-Review1 {
                word-break: break-all;
                float: left;
                width: 150px;
                padding-top: 0px;
                margin-bottom: 5px;
                margin-right: 10px;
                padding-left: 5px;
                font-weight: bold;
            }

            .label-Review2 {
                float: left;
                width: 325px;
                padding-top: 0px;
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
        <div id='termination-web' class="clearfix">
            <div class="row-fluid main-row-heading">
                <div class="container">
                    <h2 class="span6" style="padding-left: 10px;">Termination Request</h2>
                    <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">
                        <asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
                </div>
            </div>
            <div>
                <span style="color: red">
                    <asp:Label ID="lblTerminationRequest" runat="server"></asp:Label></span>
            </div>
            <div class="container margin-bottom-20">
                <div class="form-horizontal">

                    <div class="row-fluid">
                        <span style="float: right;">
                            <asp:Button ID="btnPDF" CssClass="button" runat="server" Text="Generate PDF" OnClick="btnPDF_Click" OnClientClick="resetSharePointSubmitField();" />
                            &nbsp;<asp:Button CssClass="button" ID="btnAck" runat="server" Text="Acknowledge" OnClick="btnAck_Click" />&nbsp;                                                     
                        </span>
                    </div>
                </div>
                <div class="form-horizontal" style="padding-left: 45px;padding-top:15px">
                    <div class="row-fluid">

                        <div class="span6">
                            <div class="control-group" style="margin-bottom: 5px !important">
                                <label for="" class="label-Review1">Date:</label>
                                <div class="controls" style="position: relative">
                                    <asp:Label ID="lblDateOfRequest" runat="server" CssClass="control-labelRight"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group" style="margin-bottom: 5px !important">
                                <label for="" class="label-Review1">Initiator:</label>
                                <div class="controls" style="position: relative">
                                    <asp:Label ID="lblInitiator" runat="server" CssClass="control-labelRight"></asp:Label>
                                </div>
                            </div>
                            <div class="control-group" style="margin-bottom: 5px !important">
                                <label for="" class="label-Review1">
                                    Position Type:
                                </label>
                                <div class="controls">
                                    <asp:Label ID="lblPositionType" runat="server" CssClass="control-labelRight"></asp:Label>

                                </div>
                            </div>


                        </div>

                        <div class="span6">
                            <div class="control-group" style="margin-bottom: 5px !important">
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




                        </div>

                    </div>
                </div>
                <div class="container portfolio-item">
                    <div class="row-fluid margin-bottom-20">

                        <div style="padding-bottom: 2em; padding-left: 3em; padding-right: 3em; margin-bottom: 30px; padding-top: 2em">
                            <div class="form-horizontal">
                                <div class="margin-bottom-20 row-fluid">
                                    <div id="dvNotification" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Notification of Termination</h4>
                                        </div>

                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Employee Name:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblEmployeeName" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Employee Number:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblEmployeeNum" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Business Unit:</label>
                                                    <div class="controls">

                                                        <asp:Label ID="lblBusinessUnit" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Work Area:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblWorkArea" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Site Location:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblSiteLocation" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Is mobile phone/equipment purchase required:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblMobilePhone" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Does this employee hold an Immigration Visa:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblImmigrationVisa" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Does this employee have a novated lease:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblInnovatedLease" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                            </div>


                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Last Day of Work:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblLastDayWork" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Period of Service:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblPeriodOfService" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>


                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1"></label>

                                                    <div class="controls" style="position: relative;">
                                                        <asp:Label ID="lblPeriodOfServiceEndDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Comments</label>
                                                    <div class="controls" style="position: relative;">
                                                        <asp:Label ID="lblNotifyComments" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>

                                        </div>

                                    </div>

                                    <div id="dvTypeOFLeave" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Type Of Leave</h4>
                                        </div>

                                        <div style="padding-top: 15px">
                                            <div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Is this Parental Leave:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblParentalLeave" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Leave without Pay:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblLeaveWithoutPay" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Period of leave:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblPeriodOfLeave" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>

                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review"></label>
                                                    <div class="controls" style="position: relative;">
                                                        <asp:Label ID="lblPeriodOfLeaveEndDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Comments:</label>
                                                    <div class="controls" >
                                                        <asp:Label ID="lblTypeOfLeaveComments" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                            </div>
                                        </div>

                                    </div>

                                    

                                    <div id="dvCreditCard" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Credit Card</h4>
                                        </div>
                                        <div>
                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Cancel Credit Card – advise Amex Administrator to  cancel card:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblCancelCredit" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Have all receipts been received to submit  final Amex claim form:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblReceiptsReceived" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                            </div>
                                            <div class="span6" id="CreditAckDiv" runat="server">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblCreditCardAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblCreditCardAckName" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>



                                        </div>

                                    </div>

                                    <div id="dvMarketing" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Marketing</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->



                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Remove employee from websites SunRice/Careers/SunConnect
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblRemoveEmployee" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Remove Photos from Corporate Affairs images directory:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblRemovePhotos" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                            </div>

                                            <div class="span6" id="MarketingAckDiv" runat="server">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblMarketingAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblMarketingAckName" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>


                                        </div>

                                    </div>

                                    <div id="dvProcurement" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Procurement</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->



                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Company Vehicle Returned
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblCompanyVehicle" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Vehicle keys x 2 sets:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblVehicleKeys" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Fuel Card:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblFuelCard" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">Vehicle condition report completed:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblVehicleCondition" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                            </div>
                                            <div class="span6" id="ProcurementAckDiv" runat="server">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblProcurementAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblProcurementAckName" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>



                                        </div>

                                    </div>

                                    <div id="dvFinance" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Finance</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->


                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Is the employee a Cheque Signatory
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblChequeSignatory" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>



                                            </div>

                                            <div class="span6" id="FinanceAckDiv" runat="server">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblFinanceAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblFinanceAckName" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>


                                        </div>

                                    </div>

                                    <div id="dvSiteAdmin" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Site Administration</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->


                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Security Card
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblSecurityCard" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Office/Site Keys
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblOfficeKeys" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Locker Key
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblLockerKey" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        FOB Passes
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblFobPasses" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Uniform Return
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblUniformReturn" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                            </div>

                                        </div>

                                    </div>

                                    <div id="dvInformationTechonology" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Information Technology Checklist</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->
                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Remove employee from email contact listing/folders/SunConnect Contacts listing
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblRemoveContacts" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        All equipment to be returned to IS in Leeton
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblISLeeton" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Remove/Disable computer access
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblRemoveAccess" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Mobile Phone & Charger returned
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblMobileCharger" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Mobile Phone purchased and transferred into employee's name
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblMobilePurchased" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Any electronic equipment (ipad etc)
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblElectronic" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Laptop Collected
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblLaptopCollected" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Disable employees voicemail
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblDisableVoicemail" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Set automatic email notification to alert sender that the employee is no longer employed
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblAutomaticEmail" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>



                                            </div>
                                            <div class="span6">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblInfoAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblInfoAckName" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>

                                            </div>



                                        </div>

                                    </div>

                                    <div id="dvTerminationMeeting" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">Termination Meeting</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->




                                        </div>
                                        <div style="padding-top: 15px" >
                                            <div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Exit Interview
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblExitInterview" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>


                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        All company property collected & actioned
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblPropertyCollected" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Re-iterate confidentiality agreement
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblReiterateAgree" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Prepare to notify employees contacts(Customers/Suppliers)
                                                    :</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblNotifyContacts" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Confirm employee's address for future mailing of information:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblConfirmEmployee" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Certificate of Service request:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblCertificateService" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review">
                                                        Address / Comments:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblMeetingComments" runat="server" CssClass="control-labelRight"></asp:Label>

                                                    </div>
                                                </div>

                                            </div>




                                        </div>

                                    </div>

                                    <div id="dvHRServices" runat="server">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="hrweb_h4">HR Services</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <!--<a class="btn btn-primary" href="">Save</a>-->



                                        </div>
                                        <div style="padding-top: 15px" class="row-fluid">
                                            <div class="span6" id="dvdrpHRServices" runat="server">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Process Final Payment:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnFinalPayment" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>

                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important;">
                                                    <label for="" class="label-Review2">Terminat from SAP Payroll System:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnTerminateSAP" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important;">
                                                    <label for="" class="label-Review2">Kronos access removed:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnKronosRemoved" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Termination pay provided:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnTerminationPay" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Delimit date monitoring:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnDelimitDate" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>


                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Remove personal file:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnRemoveFile" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Housing subsidy/Motor vehicle Declaration:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnHousing" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">457 Visa Notification to Immigration Department:</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="drpdwnVisaNotification" CssClass="term-select" runat="server">

                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:DropDownList>

                                                    </div>
                                                </div>

                                            </div>

                                            <div class="span6" id="divlblHRServices" runat="server">

                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Process Final Payment:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblFinalPayment" runat="server" CssClass="term-select"></asp:Label>

                                                    </div>

                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important;">
                                                    <label for="" class="label-Review2">Terminat from SAP Payroll System:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblPayrollSystem" runat="server" CssClass="term-select"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important;">
                                                    <label for="" class="label-Review2">Kronos access removed:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblKronosAccess" runat="server" CssClass="term-select"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Termination pay provided:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblTerminationPay" runat="server" CssClass="term-select"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Delimit date monitoring:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblDelimitDate" runat="server" CssClass="term-select"></asp:Label>


                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Remove personal file:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblRemovePersonal" runat="server" CssClass="term-select"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">Housing subsidy/Motor vehicle Declaration:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblHousingSubsidy" runat="server" CssClass="term-select"></asp:Label>

                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review2">457 Visa Notification to Immigration Department:</label>
                                                    <div class="controls">
                                                        <asp:Label ID="lblVisaNotify" runat="server" CssClass="term-select"></asp:Label>

                                                    </div>
                                                </div>

                                            </div>
                                            <div class="span6">
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Date Acknowledged:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblHRServiceAckDate" runat="server" CssClass="control-labelRight"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="control-group" style="margin-bottom: 5px !important">
                                                    <label for="" class="label-Review1">Name:</label>
                                                    <div class="controls" style="position: relative">
                                                        <asp:Label ID="lblHRServiceAckName" runat="server" CssClass="control-labelRight"></asp:Label>
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
            </div>
        </div>

        <div class="clearfix">&nbsp;</div>
        <br />
        <br />
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>
        <script type="text/javascript">
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
                $('.controlrole-label').text("Attached Documents");

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
