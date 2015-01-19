<%@ Assembly Name="HRWebForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c8c0e2f713937cc8" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" CodeBehind="TravelRequest.aspx.cs" Inherits="HRWebForms.HRWeb.TravelRequest" MasterPageFile="~sitecollection/_catalogs/masterpage/SunRice.v4.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">Travel Request Form</asp:Content>
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
            .ms-dttimeinput {
                border: 0px !important;
            }
        </style>

    </head>
    <body class="bodyBg">
        <div id='travel-web' class="clearfix">
            <div class="row-fluid main-row-heading">
                <h2 class="span6" style="padding-left: 10px;">Travel Authority</h2>
                <h2 class="span6" style="text-align: right; padding-RIGHT: 100px;">
                    <asp:Label ID="lblReferenceNo" runat="server"></asp:Label></h2>
            </div>
            <div style="float: right; margin-right: 20px; margin-bottom: 20px; font-style: italic">Fields marked <span style="color: red">*</span> are mandatory</div>
            <div>
                <span style="color: red">
                    <asp:Label ID="lblError" runat="server"></asp:Label></span>
            </div>
            <div class="container margin-bottom-20">
                <div class="form-horizontal">
                    <div class="row-fluid">
                        <div class="margin-bottom-20 row-fluid">
                            <h3 class="span6">Travel Summary</h3>
                        </div>
                        <div class="row-fluid">
                            <div class="span6">
                                <label id="lblNote"><u>Please Note:</u> If travel being booked is to / from the Riverina, no form approval is required.</label>
                                <div class="control-group">
                                    <label class="control-label" id="lblApplicationDate">Application Date</label>
                                    <div class="controls" style="padding-top: 5px;">
                                        <asp:Label ID="ApplicationDate" class="border-radius-none span12" runat="server"></asp:Label>
                                        <!--<SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ApplicationDateTime" DateOnly="true" CssClassTextBox="border-radius-none span12"  />-->
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblTypeofTravel" class="control-label">Type of Travel <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlTypeofTravel" class="border-radius-none span12" runat="server">
                                            <asp:ListItem>Domestic</asp:ListItem>
                                            <asp:ListItem>International</asp:ListItem>
                                            <asp:ListItem>Domestic & International</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblBookingReq" class="control-label">Booking Requirements <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlBookingReq" class="border-radius-none span12 ddlBookingReq" runat="server">
                                            <asp:ListItem>Combined Travel</asp:ListItem>
                                            <asp:ListItem>Accommodation Only</asp:ListItem>
                                            <asp:ListItem>Vehicle Only</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="control-group" id="divVisa">
                                    <label id="lblVisa" class="control-label">Visa Required <span id="spVisa" style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlVisaReq" class="border-radius-none span12" runat="server">
                                            <asp:ListItem>Yes</asp:ListItem>
                                            <asp:ListItem Selected="True">No</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblTravellerName" class="control-label">Traveller Name <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtTravellerName" runat="server" CssClass="border-radius-none span12" />
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblTravellerEmailID" class="control-label">Traveller Email Address <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtTravellerEmailID" runat="server" CssClass="border-radius-none span12" />
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblDesignation" class="control-label">Designation <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlPositionTitle" class="border-radius-none span12" runat="server">
                                            <asp:ListItem Value="Other" Text="Other"></asp:ListItem>
                                            <asp:ListItem Value="CEO" Text="CEO"></asp:ListItem>
                                            <asp:ListItem Value="Chairman" Text="Chairman"></asp:ListItem>
                                            <asp:ListItem Value="Director" Text="Director"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblIfOthers" class="control-label">Position Title  <span id="spIfOthers" style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtIfOthers" type="text" class="border-radius-none span12" runat="server"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="control-group" style="display: none;">
                                    <label id="lblSLT" class="control-label">Do you belong to Senior Leadership Team?<span style="color: red">*</span></label>
                                    <div class="controls" style="margin-top: 15px;">
                                        <asp:RadioButton ID="rdoSLTYes" GroupName="SLT" runat="server" Text="Yes" Checked="true" />
                                        <asp:RadioButton ID="rdoSLTNo" GroupName="SLT" runat="server" Text="No" />
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblBusinessUnit" class="control-label">Business Unit <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:DropDownList ID="ddlTravelBusinessUnit" class="border-radius-none span12" runat="server">
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="control-group">
                                    <label id="lblCostCentre" class="control-label">Cost Centre <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtTravelCostCentre" class="border-radius-none span12" runat="server">
                                        </asp:TextBox>
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblManagerName" class="control-label">Manager Name <span style="color: red">*</span></label>
                                    <div class="controls" style="padding-left: 17px">
                                        <SharePoint:PeopleEditor ID="ManagerPeopleEditor" runat="server" AllowEmpty="true" MultiSelect="false" CssClass="border-radius-none" Width="350px" SelectionSet="User" PlaceButtonsUnderEntityEditor="false" />
                                        &nbsp;
                                    <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image1" ToolTip="Type in a minimum of four characters from the manager’s email address to locate the full email." runat="server" ImageAlign="top" />

                                    </div>
                                </div>
                            </div>
                            <div class="span6">
                                <div class="control-group" style="display: none;">
                                    <label class="control-label" id="lblDepartureDate">Departure Date <span style="color: red">*</span></label>
                                    <div style="position: relative" class="controls">
                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="DepartureDate" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                    </div>
                                </div>
                                <div class="control-group" style="display: none;">
                                    <label class="control-label" id="lblReturnDate">Return Date <span style="color: red">*</span></label>
                                    <div style="position: relative" class="controls">
                                        <SharePoint:DateTimeControl runat="server" UseTimeZoneAdjustment="false" LocaleId="2057" ID="ReturnDate" DateOnly="true" CssClassTextBox="border-radius-none span12" />
                                    </div>
                                </div>

                                <div class="control-group">
                                    <label id="lblPurposeofTravel" class="control-label">Purpose of travel <span style="color: red">*</span></label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtPurposeofTravel" TextMode="multiline" Rows="3" class="span12 border-radius-none" runat="server"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="control-group">
                                    <label id="lblNotes" class="control-label">Notes to Travel Coordinator<br />
                                        (International travel only)</label>
                                    <div class="controls">
                                        <asp:TextBox ID="txtNotestoTC" TextMode="multiline" Rows="3" class="span12 border-radius-none" runat="server"></asp:TextBox>
                                    </div>
                                </div>


                            </div>
                        </div>
                    </div>

                    <div class="container portfolio-item">
                        <div class="row-fluid margin-bottom-20">
                            <ul class="nav nav-tabs tabs">
                                <li class="active"><a href="#CombinedTravel" class="">Combined Travel</a></li>
                                <li class=""><a href="#AccommodationOnly" class="">Accommodation Only</a></li>
                                <li class=""><a href="#VehicleOnly" class="">Vehicle Only</a></li>
                            </ul>
                            <div class="tab-content">
                                <div class="margin-bottom-20 row-fluid">
                                    <div id="CombinedTravel" class="tab-pane active">
                                        <div class="margin-bottom-20 row-fluid">
                                            <h4 class="span6">Combined Travel Itinerary</h4>
                                        </div>
                                        <div class="span4 positionAbs text-right">
                                            <asp:Button runat="server" ID="btnCombinedSave" Text="Save" CssClass="button" OnClick="btnCombinedSave_Click" CausesValidation="false" />
                                            <asp:Button runat="server" ID="btnCombinedSubmit" Text="Submit" CssClass="button" OnClick="btnCombinedSubmit_Click" CausesValidation="false" />
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span6">

                                                <div class="control-group">
                                                    <label id="lblNoOfLegs" class="control-label">No of Legs</label>
                                                    <div class="controls">
                                                        <asp:DropDownList ID="ddlNoOfLegs" class="border-radius-none" Width="330px" runat="server">
                                                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                                                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                                                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                                                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                                                        </asp:DropDownList>&nbsp;
                                                        <asp:Image ImageUrl="../../Style Library/HR Web/Images/tooltip.png" ID="Image2" ToolTip="eg: If flying from Sydney to Melbourne return, select 2 legs." runat="server" />
                                                    </div>
                                                </div>

                                            </div>
                                            <div class="span6">
                                            </div>
                                        </div>
                                        <div class="row-fluid">
                                            <div class="span12">
                                                <div class="control-group">
                                                    <label class="control-label" id="lblTravelType">Travel Type</label>
                                                    <div class="controls">
                                                        <ul class="unstyled unstyled-subitem">
                                                            <li>
                                                                <asp:CheckBox ID="chkboxFlight" runat="server" />
                                                                Flight</li>
                                                            <li>
                                                                <asp:CheckBox ID="chkboxVehicle" runat="server" />
                                                                Vehicle Details</li>
                                                            <ul class="unstyled-subitem" id="VehicleSection">

                                                                <asp:RadioButtonList CssClass="radio_label" ID="VehicleReqRadioButton" runat="server" RepeatDirection="Vertical">
                                                                    <asp:ListItem Selected="True">Personal Vehicle</asp:ListItem>
                                                                    <asp:ListItem>Company Vehicle</asp:ListItem>
                                                                    <asp:ListItem>Hire Vehicle</asp:ListItem>
                                                                </asp:RadioButtonList>


                                                            </ul>

                                                            <li>
                                                                <asp:RadioButton ID="CheckBoxAccommodation" runat="server" />
                                                                Accommodation Required &nbsp;&nbsp;<asp:RadioButton ID="ChkboxAccomNotReq" runat="server" Checked="true" />
                                                                Accommodation Not Required</li>
                                                        </ul>
                                                    </div>
                                                </div>
                                                <div class="controls">
                                                    <asp:Button runat="server" ID="GenerateLeg" Text="Generate Leg" CssClass="button" OnClick="GenerateLeg_Click" CausesValidation="false" />
                                                </div>
                                            </div>

                                        </div>


                                        <br />
                                        <br />
                                        <!-- Leg Creation-->
                                        <div id="accordion" runat="server" class="clearfix" enableviewstate="true">
                                        </div>

                                    </div>
                                </div>

                                <!--- Tab Accommodation Only started-->
                                <div id="AccommodationOnly" class="tab-pane">
                                    <div class="margin-bottom-20 row-fluid">
                                        <h4 class="span6">Accommodation Requirements</h4>
                                    </div>
                                    <div class="span4 positionAbs text-right">
                                        <asp:Button runat="server" ID="btnAccomSave" Text="Save" CssClass="button" OnClick="btnAccomSave_Click" CausesValidation="false" />
                                        <asp:Button runat="server" ID="btnAccomSubmit" Text="Submit" CssClass="button" OnClick="btnAccomSubmit_Click" CausesValidation="false" />
                                    </div>
                                    <br />

                                    <div id="AccomRequirements" class="clearfix">
                                        <asp:Table ID="AccomRequirementsTable" runat="server" CssClass="EU_DataTable" EnableViewState="true">
                                        </asp:Table>
                                    </div>

                                </div>

                                <!--- Tab Vehicle Only started-->
                                <div id="VehicleOnly" class="tab-pane">
                                    <div class="margin-bottom-20 row-fluid">
                                        <h4 class="span6">Vehicle Details</h4>
                                    </div>
                                    <div class="span4 positionAbs text-right">
                                        <asp:Button runat="server" ID="btnVehicleSave" Text="Save" CssClass="button" OnClick="btnVehicleSave_Click" CausesValidation="false" />
                                        <asp:Button runat="server" ID="btnVehicleSubmit" Text="Submit" CssClass="button" OnClick="btnVehicleSubmit_Click" CausesValidation="false" />
                                    </div>
                                    <div class="control-group span6">
                                        <label id="lblMotorVehicle" class="control-label">Motor Vehicle</label>
                                        <div class="controls">
                                            <asp:DropDownList ID="ddlMotorVehicle" class="border-radius-none span12" runat="server" OnSelectedIndexChanged="ddlMotorVehicle_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem>Select</asp:ListItem>
                                                <asp:ListItem>Company Vehicle</asp:ListItem>
                                                <asp:ListItem>Hire Vehicle</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <br />

                                    <div id="PositionDetails" class="clearfix">
                                        <asp:Table ID="VehicleTabel" runat="server" CssClass="EU_DataTable" EnableViewState="true">
                                        </asp:Table>
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

        <br />
        <br />
        <br />

        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-1.10.2.js"></script>
        <script type="text/javascript" src="../../Style%20Library/HR%20Web/JS/jquery-ui.min.js"></script>
        <script>
            $(document).ready(function () {

                var fdd = $("input[id*='FlightDepartureDate']").parent().parent();
                fdd.append("<td style='border:0px !important'><span style='color:red'>*</span></td>");

                //var fdd1 = $("select[id*='FlightDepartureTime']").parent().parent();
                //fdd1.append("<td style='border:0px !important'><span style='padding-left:5px'>*</span></td>");

                var fdd2 = $("input[id*='CheckinDate']").parent().parent();
                fdd2.append("<td style='border:0px !important'><span style='color:red'>*</span></td>");

                var fdd3 = $("input[id*='CheckoutDate']").parent().parent();
                fdd3.append("<td style='border:0px !important'><span style='color:red'>*</span></td>");

                var fdd4 = $("input[id*='PickUpDate']").parent().parent();
                fdd4.append("<td style='border:0px !important'><span style='color:red'>*</span></td>");

                var fdd5 = $("select[id*='PickUpTime']").parent().parent();
                fdd5.append("<td style='border:0px !important'><span style='color:red;padding-left:5px'>*</span></td>");

                var fdd6 = $("input[id*='DropOffDate']").parent().parent();
                fdd6.append("<td style='border:0px !important'><span style='color:red'>*</span></td>");

                var fdd7 = $("select[id*='DropOffTime']").parent().parent();
                fdd7.append("<td style='border:0px !important'><span style='color:red;padding-left:5px'>*</span></td>");

                $("iframe[title*='Select a date from the calendar']").parent().attr('style', 'border:none');



                if ($('#<%=chkboxVehicle.ClientID %>').prop('checked')) {
                    $("#VehicleSection").show();
                }
                else {
                    $("#VehicleSection").hide();
                }

                if ($('#<%=CheckBoxAccommodation.ClientID %>').prop('checked')) {
                    $('#<%=ChkboxAccomNotReq.ClientID %>').prop('checked', false);
                    $('#<%=CheckBoxAccommodation.ClientID %>').prop('checked', true);
                }
                if ($('#<%=ChkboxAccomNotReq.ClientID %>').prop('checked')) {
                    $('#<%=ChkboxAccomNotReq.ClientID %>').prop('checked', true);
                    $('#<%=CheckBoxAccommodation.ClientID %>').prop('checked', false);
                }
                if ($('option:selected', $('#<%= ddlTypeofTravel.ClientID %>')).text() == 'Domestic') {
                    document.getElementById('divVisa').style.display = 'none';

                }
                if ($('option:selected', $('#<%= ddlTypeofTravel.ClientID %>')).text() != 'Domestic') {
                    document.getElementById('divVisa').style.display = '';

                }
                if ($('option:selected', $('#<%= ddlPositionTitle.ClientID %>')).text() == 'Other') {
                    document.getElementById('spIfOthers').style.display = '';
                }
                if ($('option:selected', $('#<%= ddlPositionTitle.ClientID %>')).text() == 'CEO') {
                    document.getElementById('spIfOthers').style.display = 'none';
                }
                if ($('option:selected', $('#<%= ddlPositionTitle.ClientID %>')).text() == 'Chairman') {
                    document.getElementById('spIfOthers').style.display = 'none';
                }
                if ($('option:selected', $('#<%= ddlPositionTitle.ClientID %>')).text() == 'Director') {
                    document.getElementById('spIfOthers').style.display = 'none';
                }

                $('#<%= ddlPositionTitle.ClientID %>').on('change', function () {

                    if ($('option:selected', $(this)).text() == 'Other') {
                        document.getElementById('spIfOthers').style.display = '';
                    }
                    else {
                        document.getElementById('spIfOthers').style.display = 'none';
                    }

                });

                $('#<%= ddlTypeofTravel.ClientID %>').on('change', function () {

                    if ($('option:selected', $(this)).text() == 'Domestic') {

                        document.getElementById('divVisa').style.display = 'none';

                    }
                    else {

                        document.getElementById('divVisa').style.display = '';
                    }

                });



                $("a[href='#CombinedTravel']").click(function () {
                    var a = document.getElementById('<%= ddlBookingReq.ClientID %>');
                    for (i = 0; i < a.length; i++) {

                        if (a.options[i].text == 'Combined Travel') {
                            a.options[i].selected = true;

                        }
                    }
                });

                $("a[href='#AccommodationOnly']").click(function () {
                    var a = document.getElementById('<%= ddlBookingReq.ClientID %>');
                    for (i = 0; i < a.length; i++) {

                        if (a.options[i].text == 'Accommodation Only') {
                            a.options[i].selected = true;

                        }
                    }
                });

                $("a[href='#VehicleOnly']").click(function () {
                    var a = document.getElementById('<%= ddlBookingReq.ClientID %>');
                    for (i = 0; i < a.length; i++) {

                        if (a.options[i].text == 'Vehicle Only') {
                            a.options[i].selected = true;

                        }
                    }
                });

                if ($('option:selected', $('#<%= ddlBookingReq.ClientID %>')).text() == 'Combined Travel') {

                    $("a[href='#CombinedTravel']").trigger("click");

                }
                else if ($('option:selected', $('#<%= ddlBookingReq.ClientID %>')).text() == 'Accommodation Only') {

                    $("a[href='#AccommodationOnly']").trigger("click");

                }
                else if ($('option:selected', $('#<%= ddlBookingReq.ClientID %>')).text() == 'Vehicle Only') {
                    $("a[href='#VehicleOnly']").trigger("click");

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
            $('a.add-new-row').click(function (event) {
                event.preventDefault();
                var addnewRow = $('<tr><td>1</td> <td><img ref="calendarIcon" src="themes/images/calendar_alt_fill_16x16.png" ></td><td><img ref="calendarIcon" src="themes/images/calendar_alt_fill_16x16.png" ></td><td><input type="text" class=""></td><td><img ref="calendarIcon" src="themes/images/calendar_alt_fill_16x16.png" ></td><td><img ref="calendarIcon" src="themes/images/calendar_alt_fill_16x16.png"></td><td><input type="text" class=""></td><td><a href="" class="btn deleteRow">-</a></td></tr>');
                $('table.vehical-only').append(addnewRow);
                return false;
            });
            $('table.vehical-only').on('click', 'a.deleteRow', function (event) {
                event.preventDefault();
                $(this).closest('tr').remove();
                return false;
            });
            $(".datepicker").datepicker();
            /*$(".datepicker-image").on("click",function(){
    $('.picker').datepicker({
                         changeMonth: true,
                        changeYear: true,
                    }).hide().click(function() {
                    $(this).hide();
                    });
                                });
            $(".datepicker-image").click(function() {
           $(".picker").show(); 
        });*/

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
            $('#accordion').find('.accordion-toggle').click(function () {
                $(this).find('h4.expand').toggleClass('collaps');
                //Expand or collapse this panel
                $(this).next().slideToggle('fast');
                //Hide the other panels
                $(".accordion-content").not($(this).next()).slideUp('fast');


            });


            $('#<%= chkboxVehicle.ClientID %>').on('change', function () {
                    if ($(this).is(":checked")) {
                        $("#VehicleSection").show();
                    }
                    else {
                        $("#VehicleSection").hide();
                    }
                });
                $('#<%= CheckBoxAccommodation.ClientID %>').on('change', function () {
                    if ($(this).is(":checked")) {
                        $('#<%=ChkboxAccomNotReq.ClientID %>').prop('checked', false)
                    }
                });
                $('#<%= ChkboxAccomNotReq.ClientID %>').on('change', function () {
                    if ($(this).is(":checked")) {
                        $('#<%=CheckBoxAccommodation.ClientID %>').prop('checked', false)
                    }
                });

                $('#<%= ddlBookingReq.ClientID %>').on('change', function () {


                    if ($('option:selected', $(this)).text() == 'Combined Travel') {
                        $("a[href='#CombinedTravel']").trigger("click");

                    }
                    else if ($('option:selected', $(this)).text() == 'Accommodation Only') {
                        $("a[href='#AccommodationOnly']").trigger("click");

                    }
                    else if ($('option:selected', $(this)).text() == 'Vehicle Only') {
                        $("a[href='#VehicleOnly']").trigger("click");

                    }
                });


            });

        </script>

    </body>
    </html>
</asp:Content>
