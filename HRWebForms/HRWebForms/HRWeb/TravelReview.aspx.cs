using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using Microsoft.SharePoint.WebControls;
using System.Web;
using Microsoft.SharePoint.Taxonomy;
using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Data;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web.Hosting;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace HRWebForms.HRWeb
{
    public partial class TravelReview : WebPartPage
    {
        string UserName = string.Empty;
        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                using (SPWeb web = SPControl.GetContextWeb(this.Context))
                {
                    if (web.CurrentUser.LoginName.Contains("\\"))
                        UserName = web.CurrentUser.LoginName.Split('\\')[1];
                    else if (web.CurrentUser.LoginName.Contains("|"))
                    {
                        string[] tmp = web.CurrentUser.LoginName.Split('|');
                        UserName = tmp[tmp.Length - 1];
                    }
                    else
                        UserName = web.CurrentUser.LoginName;
                }

                string strRefno = "";
                if (Page.Request.QueryString["refno"] != null)
                {
                    strRefno = Page.Request.QueryString["refno"];
                    lblReferenceNo.Text = Page.Request.QueryString["refno"];
                }

                if (!IsPostBack)
                {
                    DivCombinedTravel.Visible = false;
                    DivAccommodationOnly.Visible = false;
                    DivVehicleOnly.Visible = false;
                    DivTravelType.Visible = false;
                    btnPDF.Visible = false;
                    if (strRefno != "")
                    {
                        string sError = VerifyUser(UserName, strRefno);
                        if (sError == "")
                        {
                            lblReferenceNo.Text = strRefno;
                            string strBookingRequirements = "";
                            GetTravelSummaryListInfo(strRefno, ref strBookingRequirements);
                            if (lblTypeofTravel.Text == "Domestic")
                            {
                                DivVisa.Visible = false;
                            }
                            else
                            {
                                DivVisa.Visible = true; ;
                            }
                            GetCommentHistory(strRefno);

                            if (string.Equals(strBookingRequirements, "Combined Travel"))
                            {
                                DivCombinedTravel.Visible = true;
                                GetCombinedTravelItineraryListInfo(strRefno);
                                GetCombinedTravelLeg(strRefno);
                            }
                            else if (string.Equals(strBookingRequirements, "Accommodation Only"))
                            {
                                DivAccommodationOnly.Visible = true;
                                GetAccommodationList(strRefno);
                            }
                            else if (string.Equals(strBookingRequirements, "Vehicle Only"))
                            {
                                DivVehicleOnly.Visible = true;
                                GetVehicleList(strRefno);
                            }

                        }
                        else
                        {
                            SPUtility.HandleAccessDenied(new Exception("You don’t have access rights to see this content"));
                        }
                    }
                    else
                    {
                        lblError.Text = "Please pass the reference number.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelReview.Page_Load", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void GetVehicleList(string strRefno)
        {
            if (strRefno != null)
            {
                VehicleTabel.Rows.Clear();
                PopulateVehicleHeader();

                SPListItemCollection collectionItems = GetListData("HRWebTravelVehicle", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in collectionItems)
                    {
                        lblMotorVehicle.Text = Convert.ToString(ListItems["MotorVehicle"]);
                        AddVehicleTable(Convert.ToString(ListItems["LegNo"]), Convert.ToDateTime(ListItems["PickUpDate"]).ToString("dd/MM/yyyy"),
                                        Convert.ToDateTime(ListItems["PickUpTime"]).ToString("hh:mm tt"), Convert.ToString(ListItems["PULocation"]),
                                        Convert.ToDateTime(ListItems["DropOffDate"]).ToString("dd/MM/yyyy"), Convert.ToDateTime(ListItems["DropOffTime"]).ToString("hh:mm tt"),
                                        Convert.ToString(ListItems["DropOffLocation"]));
                    }
                }
            }
        }

        private void AddVehicleTable(string vlegno, string vpickupdate, string vpickuptime, string vpulocation,
                                             string vdropoffdate, string vdropofftime, string vdropofflocation)
        {
            string cnt = VehicleTabel.Rows.Count.ToString();

            TableRow VehiclerowNew = new TableRow();

            TableCell VcellNew = new TableCell();
            TableCell VcellNew2 = new TableCell();
            TableCell VcellNew3 = new TableCell();
            TableCell VcellNew4 = new TableCell();
            TableCell VcellNew5 = new TableCell();
            TableCell VcellNew6 = new TableCell();
            TableCell VcellNew7 = new TableCell();

            VcellNew.Style.Add("width", "8%");
            VcellNew2.Style.Add("width", "12%");
            VcellNew3.Style.Add("width", "10%");
            VcellNew4.Style.Add("width", "18%");
            VcellNew5.Style.Add("width", "12%");
            VcellNew6.Style.Add("width", "10%");
            VcellNew7.Style.Add("width", "18%");

            System.Web.UI.WebControls.Label legno = new System.Web.UI.WebControls.Label();
            legno.ID = "legno" + cnt;
            legno.Text = vlegno;
            legno.CssClass = "span12";
            legno.Attributes.CssStyle.Add("text-align", "center");
            //legno.Attributes.CssStyle.Add("margin-top", "5px");

            if (vpickupdate != null)
            {
                System.Web.UI.WebControls.Label PickUpDate = new System.Web.UI.WebControls.Label();
                PickUpDate.ID = "PickUpDate" + cnt;
                PickUpDate.Text = vpickupdate;
                VcellNew2.Controls.Add(PickUpDate);

                System.Web.UI.WebControls.Label PickUpTime = new System.Web.UI.WebControls.Label();
                PickUpTime.ID = "PickUpTime" + cnt;
                PickUpTime.Text = vpickuptime;
                VcellNew3.Controls.Add(PickUpTime);
            }

            System.Web.UI.WebControls.Label txtPULocation = new System.Web.UI.WebControls.Label();
            txtPULocation.ID = "txtPULocation" + cnt;
            txtPULocation.Text = vpulocation;
            txtPULocation.CssClass = "control-border";
            txtPULocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtPULocation.Attributes.CssStyle.Add("margin-top", "1px");

            if (vdropoffdate != null)
            {
                System.Web.UI.WebControls.Label DropOffDate = new System.Web.UI.WebControls.Label();
                DropOffDate.ID = "DropOffDate" + cnt;
                DropOffDate.Text = vdropoffdate;
                VcellNew5.Controls.Add(DropOffDate);

                System.Web.UI.WebControls.Label DropOffTime = new System.Web.UI.WebControls.Label();
                DropOffTime.ID = "DropOffTime" + cnt;
                DropOffTime.Text = vdropofftime;
                VcellNew6.Controls.Add(DropOffTime);
            }

            System.Web.UI.WebControls.Label txtDropOffLocation = new System.Web.UI.WebControls.Label();
            txtDropOffLocation.ID = "txtDropOffLocation" + cnt;
            txtDropOffLocation.Text = vdropofflocation;
            txtDropOffLocation.CssClass = "control-border";
            txtDropOffLocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtDropOffLocation.Attributes.CssStyle.Add("margin-top", "1px");

            VcellNew.Controls.Add(legno);                        
            VcellNew4.Controls.Add(txtPULocation);                       
            VcellNew7.Controls.Add(txtDropOffLocation);

            VehiclerowNew.Cells.Add(VcellNew);
            VehiclerowNew.Cells.Add(VcellNew2);
            VehiclerowNew.Cells.Add(VcellNew3);
            VehiclerowNew.Cells.Add(VcellNew4);
            VehiclerowNew.Cells.Add(VcellNew5);
            VehiclerowNew.Cells.Add(VcellNew6);
            VehiclerowNew.Cells.Add(VcellNew7);

            VehicleTabel.Rows.Add(VehiclerowNew);
        }

        private void PopulateVehicleHeader()
        {
            TableHeaderRow Vehicleheader = new TableHeaderRow();

            TableHeaderCell VehicleheaderTableCell = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell5 = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell6 = new TableHeaderCell();
            TableHeaderCell VehicleheaderTableCell7 = new TableHeaderCell();

            VehicleheaderTableCell.Style.Add("width", "8%");
            VehicleheaderTableCell2.Style.Add("width", "12%");
            VehicleheaderTableCell3.Style.Add("width", "10%");
            VehicleheaderTableCell4.Style.Add("width", "18%");
            VehicleheaderTableCell5.Style.Add("width", "12%");
            VehicleheaderTableCell6.Style.Add("width", "10%");
            VehicleheaderTableCell7.Style.Add("width", "18%");

            VehicleheaderTableCell.Text = "Leg";
            VehicleheaderTableCell2.Text = "Pick Up Date";
            VehicleheaderTableCell3.Text = "Pick Up Time";
            VehicleheaderTableCell4.Text = "P/U Location";
            VehicleheaderTableCell5.Text = "Drop Off Date";
            VehicleheaderTableCell6.Text = "Drop Off Time";
            VehicleheaderTableCell7.Text = "Drop Off Location";

            Vehicleheader.Cells.Add(VehicleheaderTableCell);
            Vehicleheader.Cells.Add(VehicleheaderTableCell2);
            Vehicleheader.Cells.Add(VehicleheaderTableCell3);
            Vehicleheader.Cells.Add(VehicleheaderTableCell4);
            Vehicleheader.Cells.Add(VehicleheaderTableCell5);
            Vehicleheader.Cells.Add(VehicleheaderTableCell6);
            Vehicleheader.Cells.Add(VehicleheaderTableCell7);

            VehicleTabel.Rows.Add(Vehicleheader);
        }

        private void GetAccommodationList(string strRefno)
        {
            if (strRefno != null)
            {

                AccomRequirementsTable.Rows.Clear();
                PopulateAccommodationHeader();

                SPListItemCollection collectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in collectionItems)
                    {
                        AddAccommodationReqTable(Convert.ToDateTime(ListItems["CheckIn"]).ToString("dd/MM/yyyy"), Convert.ToDateTime(ListItems["CheckOut"]).ToString("dd/MM/yyyy"),
                                                 Convert.ToString(ListItems["HotelName"]), Convert.ToString(ListItems["NoOfNights"]));
                    }
                }
            }
        }

        private void AddAccommodationReqTable(string checkin, string checkout, string hotelname, string noofnights)
        {
            string cnt = AccomRequirementsTable.Rows.Count.ToString();

            TableRow AccomrowNew = new TableRow();

            TableCell AcellNew = new TableCell();
            TableCell AcellNew2 = new TableCell();
            TableCell AcellNew3 = new TableCell();
            TableCell AcellNew4 = new TableCell();

            AcellNew.Style.Add("width", "12%");
            AcellNew2.Style.Add("width", "12%");
            AcellNew3.Style.Add("width", "18%");
            AcellNew4.Style.Add("width", "12%");

            if (checkin != null)
            {
                System.Web.UI.WebControls.Label CheckinDate = new System.Web.UI.WebControls.Label();
                CheckinDate.ID = "CheckinDate" + cnt;
                CheckinDate.Text = checkin;
                AcellNew.Controls.Add(CheckinDate);
            }

            if (checkout != null)
            {
                System.Web.UI.WebControls.Label CheckoutDate = new System.Web.UI.WebControls.Label();
                CheckoutDate.ID = "CheckoutDate" + cnt;
                CheckoutDate.Text = checkout;
                AcellNew2.Controls.Add(CheckoutDate);
            }

            System.Web.UI.WebControls.Label txtHotelName = new System.Web.UI.WebControls.Label();
            txtHotelName.ID = "txtHotelName" + cnt;
            txtHotelName.Attributes.CssStyle.Add("margin-left", "8px");
            txtHotelName.Text = hotelname;


            System.Web.UI.WebControls.Label lblNoOfNights = new System.Web.UI.WebControls.Label();
            lblNoOfNights.ID = "lblNoOfNights" + cnt;
            lblNoOfNights.Text = noofnights;
            lblNoOfNights.CssClass = "span12";
            lblNoOfNights.Attributes.CssStyle.Add("text-align", "center");
                        
            AcellNew3.Controls.Add(txtHotelName);
            AcellNew4.Controls.Add(lblNoOfNights);

            AccomrowNew.Cells.Add(AcellNew);
            AccomrowNew.Cells.Add(AcellNew2);
            AccomrowNew.Cells.Add(AcellNew3);
            AccomrowNew.Cells.Add(AcellNew4);

            AccomRequirementsTable.Rows.Add(AccomrowNew);
        }

        private void PopulateAccommodationHeader()
        {
            TableHeaderRow Accomheader = new TableHeaderRow();
            Accomheader.Style.Add("width", "72%");
            TableHeaderCell AccomheaderTableCell = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell4 = new TableHeaderCell();

            AccomheaderTableCell.Style.Add("width", "12%");
            AccomheaderTableCell2.Style.Add("width", "12%");
            AccomheaderTableCell3.Style.Add("width", "18%");
            AccomheaderTableCell4.Style.Add("width", "12%");

            AccomheaderTableCell.Text = "Check In";
            AccomheaderTableCell2.Text = "Check Out";
            AccomheaderTableCell3.Text = "Hotel Name";
            AccomheaderTableCell4.Text = "Nights";

            Accomheader.Cells.Add(AccomheaderTableCell);
            Accomheader.Cells.Add(AccomheaderTableCell2);
            Accomheader.Cells.Add(AccomheaderTableCell3);
            Accomheader.Cells.Add(AccomheaderTableCell4);

            AccomRequirementsTable.Rows.Add(Accomheader);
        }

        private void GetCombinedTravelLeg(string strRefno)
        {
            if (strRefno != null)
            {
                int cntLeg = 0;
                string lstURL = HrWebUtility.GetListUrl("HRWebCombinedTravelItinerary");

                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                    cntLeg = Convert.ToInt16(listitem["NoOfLegs"]);

                for (int inx = 1; inx <= cntLeg; inx++)
                {
                    string flightDepartureDate = string.Empty;
                    string DepartureLocation = string.Empty;
                    string FlightCarrier = string.Empty;
                    string FlightNo = string.Empty;
                    string flightDepartureTime = string.Empty;
                    string DestinationLocation = string.Empty;

                    SPListItemCollection collectionItems = GetListData("HRWebTravelFlight", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {

                        foreach (SPListItem ListItems in collectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {                                
                                if (Convert.ToString(ListItems["DepartureDate"]) != "")
                                    flightDepartureDate = Convert.ToDateTime(ListItems["DepartureDate"]).ToString("dd/MM/yyyy");
                                DepartureLocation = Convert.ToString(ListItems["DepartureLocation"]);
                                FlightCarrier = Convert.ToString(ListItems["FlightCarrier"]);
                                FlightNo = Convert.ToString(ListItems["FlightNo"]);
                                if (Convert.ToString(ListItems["DepartureDate"]) != "")
                                    flightDepartureTime = Convert.ToDateTime(ListItems["FlightDepartureTime"]).ToString("hh:mm tt");
                                DestinationLocation = Convert.ToString(ListItems["TravelTo"]);
                            }

                        }
                    }

                    string CheckInDate = string.Empty;
                    string HotelName = string.Empty;
                    string NoOfNights = string.Empty;
                    string CheckOutDate = string.Empty;

                    SPListItemCollection AccomcollectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                    if (AccomcollectionItems != null && AccomcollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in AccomcollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                if (Convert.ToString(ListItems["CheckIn"]) != "")
                                    CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]).ToString("dd/MM/yyyy");
                                HotelName = Convert.ToString(ListItems["HotelName"]);
                                NoOfNights = Convert.ToString(ListItems["NoOfNights"]);
                                if (Convert.ToString(ListItems["CheckOut"]) != "")
                                    CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]).ToString("dd/MM/yyyy");
                            }

                        }
                    }

                    string PickUpDate = string.Empty;
                    string PickUpTime = string.Empty;
                    string PULocation = string.Empty;
                    string DropOffDate = string.Empty;
                    string DropOffTime = string.Empty;
                    string DropOffLocation = string.Empty;

                    SPListItemCollection VehiclecollectionItems = GetListData("HRWebTravelVehicle", strRefno);
                    if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in VehiclecollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                if (Convert.ToString(ListItems["PickUpDate"]) != "")
                                {
                                    PickUpDate = Convert.ToDateTime(ListItems["PickUpDate"]).ToString("dd/MM/yyyy");
                                    PickUpTime = Convert.ToDateTime(ListItems["PickUpTime"]).ToString("hh:mm tt");
                                }
                                PULocation = Convert.ToString(ListItems["PULocation"]);
                                if (Convert.ToString(ListItems["DropOffDate"]) != "")
                                {
                                    DropOffDate = Convert.ToDateTime(ListItems["DropOffDate"]).ToString("dd/MM/yyyy");
                                    DropOffTime = Convert.ToDateTime(ListItems["DropOffTime"]).ToString("hh:mm tt");
                                }
                                DropOffLocation = Convert.ToString(ListItems["DropOffLocation"]);
                            }
                        }
                    }

                    AddCombinedTravelTable_GET(inx, flightDepartureDate, DepartureLocation, FlightCarrier, FlightNo, flightDepartureTime,DestinationLocation,
                                               CheckInDate, HotelName, NoOfNights, CheckOutDate, PickUpDate, PickUpTime, PULocation, DropOffDate, DropOffTime, DropOffLocation);

                }
            }
        }

        private void AddCombinedTravelTable_GET(int cnt, string flightDepartureDate, string DepartureLocation, string FlightCarrier, string FlightNo,
                                                string flightDepartureTime, string DestinationLocation, string CheckInDate, string HotelName, string NoOfNights, string CheckOutDate,
                                                string PickUpDate, string PickUpTime, string PULocation, string DropOffDate, string DropOffTime, string DropOffLocation)
        {                       
            Table tbl = new Table();
            tbl.ID = "tblLeg" + cnt;
            tbl.CssClass = "legtable  table-bordered";

            TableHeaderRow header = new TableHeaderRow();
            TableHeaderCell headerTableCell = new TableHeaderCell();
            headerTableCell.Text = "TRAVEL LEG " + cnt + ":";
            headerTableCell.Style.Add("text-align", "left");
            header.Cells.Add(headerTableCell);

            tbl.Rows.Add(header);
            TableRow rowNew = new TableRow();
            tbl.Controls.Add(rowNew);

            TableCell cellNew = new TableCell();
            System.Web.UI.WebControls.Label lblNew = new System.Web.UI.WebControls.Label();
            lblNew.Text = "<br />";



            if (Convert.ToString(ViewState["Flight"])!= "")
            {
                Table tb2 = new Table();
                tb2.ID = "tblFlight" + cnt;
                tb2.CssClass = "EU_DataTable";

                PopulateCTFlightHeader(tb2);

                TableRow FrowNew = new TableRow();
                tb2.Controls.Add(FrowNew);

                TableCell FcellNew = new TableCell();
                TableCell FcellNew2 = new TableCell();
                TableCell FcellNew3 = new TableCell();
                TableCell FcellNew4 = new TableCell();
                TableCell FcellNew5 = new TableCell();
                TableCell FcellNew6 = new TableCell();
                TableCell FcellNew7 = new TableCell();


                System.Web.UI.WebControls.Label FlblNew = new System.Web.UI.WebControls.Label();
                FlblNew.Text = "Flight";
                FlblNew.Style.Add("font-weight", "bold");

                System.Web.UI.WebControls.Label txtFlightDeptLocation = new System.Web.UI.WebControls.Label();
                txtFlightDeptLocation.ID = "txtFlightDeptLocation" + cnt;
                txtFlightDeptLocation.Text = DepartureLocation;
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                if (flightDepartureDate != null)
                {
                    System.Web.UI.WebControls.Label FlightDepartureDate = new System.Web.UI.WebControls.Label();
                    FlightDepartureDate.ID = "FlightDepartureDate" + cnt;
                    FlightDepartureDate.Text = flightDepartureDate;
                    FcellNew3.Controls.Add(FlightDepartureDate);
                }

                if (flightDepartureDate != null)
                {
                    System.Web.UI.WebControls.Label FlightDepartureTime = new System.Web.UI.WebControls.Label();
                    FlightDepartureTime.ID = "FlightDepartureTime" + cnt;
                    FlightDepartureTime.Text = flightDepartureTime;
                    FcellNew5.Controls.Add(FlightDepartureTime);
                }

                System.Web.UI.WebControls.Label txtFlightDestLocation = new System.Web.UI.WebControls.Label();
                txtFlightDestLocation.ID = "txtFlightDestLocation" + cnt;
                txtFlightDestLocation.Text = DestinationLocation;
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                System.Web.UI.WebControls.Label txtFlightCarrier = new System.Web.UI.WebControls.Label();
                txtFlightCarrier.ID = "txtFlightCarrier" + cnt;
                txtFlightCarrier.Text = FlightCarrier;
                txtFlightCarrier.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-bottom", "1px");

                System.Web.UI.WebControls.Label txtFlightNumber = new System.Web.UI.WebControls.Label();
                txtFlightNumber.ID = "txtFlightNumber" + cnt;
                txtFlightNumber.Text = FlightNo;
                txtFlightNumber.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-bottom", "1px");
                                
                FrowNew.Controls.Add(FcellNew);
                FrowNew.Controls.Add(FcellNew2);
                FrowNew.Controls.Add(FcellNew3);
                FrowNew.Controls.Add(FcellNew4);
                FrowNew.Controls.Add(FcellNew5);
                FrowNew.Controls.Add(FcellNew6);
                FrowNew.Controls.Add(FcellNew7);

                FcellNew.Controls.Add(FlblNew);                
                FcellNew2.Controls.Add(txtFlightDeptLocation);
                FcellNew4.Controls.Add(txtFlightDestLocation);
                FcellNew6.Controls.Add(txtFlightCarrier);
                FcellNew7.Controls.Add(txtFlightNumber);
                

                cellNew.Controls.Add(tb2);


            }

            if (Convert.ToString(ViewState["Accommodation"]) != "")
            {
                Table tb3 = new Table();
                tb3.CssClass = "EU_DataTable";
                tb3.ID = "tblAccommodation" + cnt;

                PopulateCTAccommodationHeader(tb3);

                TableRow ArowNew = new TableRow();
                tb3.Controls.Add(ArowNew);

                TableCell AcellNew = new TableCell();
                TableCell AcellNew2 = new TableCell();
                TableCell AcellNew3 = new TableCell();
                TableCell AcellNew4 = new TableCell();
                TableCell AcellNew5 = new TableCell();


                System.Web.UI.WebControls.Label AlblNew = new System.Web.UI.WebControls.Label();
                AlblNew.Text = "Accommodation";
                AlblNew.Style.Add("font-weight", "bold");

                if (CheckInDate != null)
                {
                    System.Web.UI.WebControls.Label CTCheckinDate = new System.Web.UI.WebControls.Label();
                    CTCheckinDate.ID = "CTCheckinDate" + cnt;
                    CTCheckinDate.Text = CheckInDate;
                    AcellNew2.Controls.Add(CTCheckinDate);
                }

                if (CheckOutDate != null)
                {
                    System.Web.UI.WebControls.Label CTCheckoutDate = new System.Web.UI.WebControls.Label();
                    CTCheckoutDate.ID = "CTCheckoutDate" + cnt;
                    CTCheckoutDate.Text = CheckOutDate;
                    AcellNew3.Controls.Add(CTCheckoutDate);
                }

                System.Web.UI.WebControls.Label txtCTHotelName = new System.Web.UI.WebControls.Label();
                txtCTHotelName.ID = "txtCTHotelName" + cnt;
                txtCTHotelName.Text = HotelName;
                txtCTHotelName.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-bottom", "1px");

                System.Web.UI.WebControls.Label txtCTNoofNights = new System.Web.UI.WebControls.Label();
                txtCTNoofNights.ID = "txtCTNoofNights" + cnt;
                txtCTNoofNights.Text = NoOfNights;
                txtCTNoofNights.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-bottom", "1px");

                ArowNew.Controls.Add(AcellNew);
                ArowNew.Controls.Add(AcellNew2);
                ArowNew.Controls.Add(AcellNew3);
                ArowNew.Controls.Add(AcellNew4);
                ArowNew.Controls.Add(AcellNew5);

                AcellNew.Controls.Add(AlblNew);                                
                AcellNew4.Controls.Add(txtCTHotelName);
                AcellNew5.Controls.Add(txtCTNoofNights);


                cellNew.Controls.Add(tb3);
            }

            if (Convert.ToString(ViewState["VehicleTravelType"]) != "")
            {
                if (Convert.ToString(ViewState["HireVehicle"]) != "")
                {
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable";

                    PopulateCTHCHearder(tb4);

                    TableRow HrowNew = new TableRow();
                    tb4.Controls.Add(HrowNew);

                    TableCell HcellNew = new TableCell();
                    TableCell HcellNew2 = new TableCell();
                    TableCell HcellNew3 = new TableCell();
                    TableCell HcellNew4 = new TableCell();
                    TableCell HcellNew5 = new TableCell();
                    TableCell HcellNew6 = new TableCell();
                    TableCell HcellNew7 = new TableCell();

                    System.Web.UI.WebControls.Label HlblNew = new System.Web.UI.WebControls.Label();
                    HlblNew.Text = "Hire Car";
                    HlblNew.Style.Add("font-weight", "bold");

                    if (PickUpDate != null)
                    {
                        System.Web.UI.WebControls.Label HCPickUpDate = new System.Web.UI.WebControls.Label();
                        HCPickUpDate.ID = "HCPickUpDate" + cnt;
                        HCPickUpDate.Text = PickUpDate;
                        HcellNew2.Controls.Add(HCPickUpDate);

                        System.Web.UI.WebControls.Label HCPickUpTime = new System.Web.UI.WebControls.Label();
                        HCPickUpTime.ID = "HCPickUpTime" + cnt;
                        HCPickUpTime.Text = PickUpTime;
                        HcellNew3.Controls.Add(HCPickUpTime);
                    }

                    System.Web.UI.WebControls.Label txtHCPickUpLocation = new System.Web.UI.WebControls.Label();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Text = PULocation;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                    if (DropOffDate != null)
                    {
                        System.Web.UI.WebControls.Label HCDropoffDate = new System.Web.UI.WebControls.Label();
                        HCDropoffDate.ID = "HCDropoffDate" + cnt;
                        HCDropoffDate.Text = DropOffDate;
                        HcellNew5.Controls.Add(HCDropoffDate);

                        System.Web.UI.WebControls.Label HCDropoffTime = new System.Web.UI.WebControls.Label();
                        HCDropoffTime.ID = "HCDropoffTime" + cnt;
                        HCDropoffTime.Text = DropOffTime;
                        HcellNew6.Controls.Add(HCDropoffTime);
                    }

                    System.Web.UI.WebControls.Label txtHCReturnLocation = new System.Web.UI.WebControls.Label();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
                    txtHCReturnLocation.Text = DropOffLocation;
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                    HrowNew.Controls.Add(HcellNew);
                    HrowNew.Controls.Add(HcellNew2);
                    HrowNew.Controls.Add(HcellNew3);
                    HrowNew.Controls.Add(HcellNew4);
                    HrowNew.Controls.Add(HcellNew5);
                    HrowNew.Controls.Add(HcellNew6);
                    HrowNew.Controls.Add(HcellNew7);

                    HcellNew.Controls.Add(HlblNew);                                       
                    HcellNew4.Controls.Add(txtHCPickUpLocation);                                        
                    HcellNew7.Controls.Add(txtHCReturnLocation);

                    cellNew.Controls.Add(tb4);
                }
                if (Convert.ToString(ViewState["CompanyVehicle"]) != "")
                {
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable";

                    PopulateCTHCHearder(tb4);

                    TableRow HrowNew = new TableRow();
                    tb4.Controls.Add(HrowNew);

                    TableCell HcellNew = new TableCell();
                    TableCell HcellNew2 = new TableCell();
                    TableCell HcellNew3 = new TableCell();
                    TableCell HcellNew4 = new TableCell();
                    TableCell HcellNew5 = new TableCell();
                    TableCell HcellNew6 = new TableCell();
                    TableCell HcellNew7 = new TableCell();

                    System.Web.UI.WebControls.Label HlblNew = new System.Web.UI.WebControls.Label();
                    HlblNew.Text = "Hire Car";
                    HlblNew.Style.Add("font-weight", "bold");

                    if (PickUpDate != null)
                    {
                        System.Web.UI.WebControls.Label HCPickUpDate = new System.Web.UI.WebControls.Label();
                        HCPickUpDate.ID = "HCPickUpDate" + cnt;
                        HCPickUpDate.Text = PickUpDate;
                        HcellNew2.Controls.Add(HCPickUpDate);

                        System.Web.UI.WebControls.Label HCPickUpTime = new System.Web.UI.WebControls.Label();
                        HCPickUpTime.ID = "HCPickUpTime" + cnt;
                        HCPickUpTime.Text = PickUpTime;
                        HcellNew3.Controls.Add(HCPickUpTime);
                    }

                    System.Web.UI.WebControls.Label txtHCPickUpLocation = new System.Web.UI.WebControls.Label();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Text = PULocation;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                    if (DropOffDate != null)
                    {
                        System.Web.UI.WebControls.Label HCDropoffDate = new System.Web.UI.WebControls.Label();
                        HCDropoffDate.ID = "HCDropoffDate" + cnt;
                        HCDropoffDate.Text = DropOffDate;
                        HcellNew5.Controls.Add(HCDropoffDate);

                        System.Web.UI.WebControls.Label HCDropoffTime = new System.Web.UI.WebControls.Label();
                        HCDropoffTime.ID = "HCDropoffTime" + cnt;
                        HCDropoffTime.Text = DropOffTime;
                        HcellNew6.Controls.Add(HCDropoffTime);
                    }

                    System.Web.UI.WebControls.Label txtHCReturnLocation = new System.Web.UI.WebControls.Label();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
                    txtHCReturnLocation.Text = DropOffLocation;
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCReturnLocation.Attributes.CssStyle.Add("margin-bottom", "1px");

                    HrowNew.Controls.Add(HcellNew);
                    HrowNew.Controls.Add(HcellNew2);
                    HrowNew.Controls.Add(HcellNew3);
                    HrowNew.Controls.Add(HcellNew4);
                    HrowNew.Controls.Add(HcellNew5);
                    HrowNew.Controls.Add(HcellNew6);
                    HrowNew.Controls.Add(HcellNew7);

                    HcellNew.Controls.Add(HlblNew);                                       
                    HcellNew4.Controls.Add(txtHCPickUpLocation);                                        
                    HcellNew7.Controls.Add(txtHCReturnLocation);

                    cellNew.Controls.Add(tb4);
                }
            }

            cellNew.Controls.Add(lblNew);
            rowNew.Controls.Add(cellNew);
            accordion.Controls.Add(tbl);
        }

        private void PopulateCTFlightHeader(Table tb2)
        {
            TableHeaderRow Flightheader = new TableHeaderRow();
            Flightheader.Style.Add("width", "98%");
            TableHeaderCell FlightheaderTableCell = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell5 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell6 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell7 = new TableHeaderCell();

            FlightheaderTableCell.Style.Add("width", "12%");
            FlightheaderTableCell2.Style.Add("width", "17%");
            FlightheaderTableCell3.Style.Add("width", "14%");
            FlightheaderTableCell4.Style.Add("width", "14%");
            FlightheaderTableCell5.Style.Add("width", "17%");
            FlightheaderTableCell6.Style.Add("width", "14%");
            FlightheaderTableCell7.Style.Add("width", "10%");

            FlightheaderTableCell.Text = "";
            FlightheaderTableCell2.Text = "Travel From";
            FlightheaderTableCell3.Text = "Departure Date";
            FlightheaderTableCell4.Text = "Travel To";
            FlightheaderTableCell5.Text = "Departure Time";
            FlightheaderTableCell6.Text = "Flight Carrier";
            FlightheaderTableCell7.Text = "Flight No";
            

            Flightheader.Cells.Add(FlightheaderTableCell);
            Flightheader.Cells.Add(FlightheaderTableCell2);
            Flightheader.Cells.Add(FlightheaderTableCell3);
            Flightheader.Cells.Add(FlightheaderTableCell4);
            Flightheader.Cells.Add(FlightheaderTableCell5);
            Flightheader.Cells.Add(FlightheaderTableCell6);
            Flightheader.Cells.Add(FlightheaderTableCell7);

            tb2.Rows.Add(Flightheader);
        }

        private void PopulateCTAccommodationHeader(Table tb3)
        {
            TableHeaderRow Accomodationheader = new TableHeaderRow();
            Accomodationheader.Style.Add("width", "94%");
            TableHeaderCell AccomheaderTableCell = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell5 = new TableHeaderCell();


            AccomheaderTableCell.Style.Add("width", "15%");
            AccomheaderTableCell2.Style.Add("width", "13%");
            AccomheaderTableCell3.Style.Add("width", "13%");
            AccomheaderTableCell4.Style.Add("width", "19%");
            AccomheaderTableCell5.Style.Add("width", "17%");



            AccomheaderTableCell.Text = "";
            AccomheaderTableCell2.Text = "CheckIn Date";
            AccomheaderTableCell3.Text = "Checkout Date";
            AccomheaderTableCell4.Text = "Hotel Name";
            AccomheaderTableCell5.Text = "No.of Nights";


            Accomodationheader.Cells.Add(AccomheaderTableCell);
            Accomodationheader.Cells.Add(AccomheaderTableCell2);
            Accomodationheader.Cells.Add(AccomheaderTableCell3);
            Accomodationheader.Cells.Add(AccomheaderTableCell4);
            Accomodationheader.Cells.Add(AccomheaderTableCell5);


            tb3.Rows.Add(Accomodationheader);
        }

        private void PopulateCTHCHearder(Table tb4)
        {
            TableHeaderRow HireCarheader = new TableHeaderRow();
            HireCarheader.Style.Add("width", "96%");
            TableHeaderCell HireCarheaderTableCell = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell5 = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell6 = new TableHeaderCell();
            TableHeaderCell HireCarheaderTableCell7 = new TableHeaderCell();


            HireCarheaderTableCell.Style.Add("width", "12%");
            HireCarheaderTableCell2.Style.Add("width", "15%");
            HireCarheaderTableCell3.Style.Add("width", "10%");
            HireCarheaderTableCell4.Style.Add("width", "17%");
            HireCarheaderTableCell5.Style.Add("width", "15%");
            HireCarheaderTableCell6.Style.Add("width", "10%");
            HireCarheaderTableCell7.Style.Add("width", "17%");


            HireCarheaderTableCell.Text = "";
            HireCarheaderTableCell2.Text = "Pick Up Date";
            HireCarheaderTableCell3.Text = "Pick Up Time";
            HireCarheaderTableCell4.Text = "Pick Up Location";
            HireCarheaderTableCell5.Text = "Drop Off Date";
            HireCarheaderTableCell6.Text = "Drop Off Time";
            HireCarheaderTableCell7.Text = "Drop Off Location";


            HireCarheader.Cells.Add(HireCarheaderTableCell);
            HireCarheader.Cells.Add(HireCarheaderTableCell2);
            HireCarheader.Cells.Add(HireCarheaderTableCell3);
            HireCarheader.Cells.Add(HireCarheaderTableCell4);
            HireCarheader.Cells.Add(HireCarheaderTableCell5);
            HireCarheader.Cells.Add(HireCarheaderTableCell6);
            HireCarheader.Cells.Add(HireCarheaderTableCell7);
            tb4.Rows.Add(HireCarheader);
        }

        private void GetCombinedTravelItineraryListInfo(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("HRWebCombinedTravelItinerary");

            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist.GetItems(oquery);
            foreach (SPListItem listitem in collitems)
            {
                

                lblNoOfLegs.Text = Convert.ToString(listitem["NoOfLegs"]);

                if (Convert.ToString(listitem["FlightTravelType"]) == "Flight")
                {
                    ViewState["Flight"] = "Flight";
                    lblFlight.Visible = true;
                }
                else
                {
                    lblFlight.Visible = false;
                }
                if ((Convert.ToString(listitem["VehicleTravelType"])) != null)
                {
                    ViewState["VehicleTravelType"] = "VehicleType";
                    VehicleRequirement.Visible = true;
                    lblHireVehicle.Visible = false;
                    lblCompanyVehicle.Visible = false;
                    lblPersonalVehicle.Visible = false;

                    if (Convert.ToString(listitem["VehicleTravelType"]) == "Hire Vehicle")
                    {
                        lblHireVehicle.Visible = true;
                        ViewState["HireVehicle"] = "HireVehicle";
                    }
                    else if (Convert.ToString(listitem["VehicleTravelType"]) == "Company Vehicle")
                    {
                        lblCompanyVehicle.Visible = true;
                        ViewState["CompanyVehicle"] = "CompanyVehicle";
                    }
                    else if (Convert.ToString(listitem["VehicleTravelType"]) == "Personal Vehicle")
                    {
                        lblPersonalVehicle.Visible = true;
                        ViewState["PersonalVehicle"] = "PersonalVehicle";
                    }
                }
                else
                {
                    VehicleRequirement.Visible = false;
                    lblHireVehicle.Visible = false;
                    lblCompanyVehicle.Visible = false;
                    lblPersonalVehicle.Visible = false;
                }
                if (Convert.ToString(listitem["AccommodationTravelType"]) == "Accommodation")
                {
                    lblAccommodation.Visible = true;
                    ViewState["Accommodation"] = "Accommodation";
                }
                else
                {
                    lblAccommodation.Visible = false;
                }
                if (Convert.ToString(listitem["AccommodationTravelType"]) == "No Accommodation")
                {
                    lblAccomNotReq.Visible = true;
                    ViewState["NoAccommodation"] = "No Accommodation";
                }
                else
                {
                    lblAccomNotReq.Visible = false;
                }
            }
        }

        private void GetTravelSummaryListInfo(string strRefno, ref string strBookingRequirements)
        {
            string lstURL = HrWebUtility.GetListUrl("HRWebTravelSummary");

            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist.GetItems(oquery);
            foreach (SPListItem listitem in collitems)
            {
                lblApplicationDate.Text = Convert.ToString(listitem["ApplicationDate"]);
                lblTypeofTravel.Text = Convert.ToString(listitem["TypeofTravel"]);
                lblBookingRequirements.Text = Convert.ToString(listitem["BookingRequirements"]);
                lblVisaReq.Text = Convert.ToString(listitem["VisaRequired"]);
                strBookingRequirements = lblBookingRequirements.Text;
                lblTravellerName.Text = Convert.ToString(listitem["TravellerName"]);
                lblTravellerEmailID.Text = Convert.ToString(listitem["TravellerEmailID"]);
                lblPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                lblIfOther.Text = Convert.ToString(listitem["IfOthersPositionTitle"]);
                lblSLT.Text = (Convert.ToString(listitem["IsUserSLT"]).ToLower() == "true") ? "Yes" : "No";
                lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                lblCostCentre.Text = Convert.ToString(listitem["CostCentre"]);
                lblManagerName.Text = GetUser(Convert.ToString(listitem["ManagerName"]));
                lblDepartureDate.Text = Convert.ToDateTime(listitem["DepartureDate"]).ToString("dd/MM/yyyy");
                lblReturnDate.Text = Convert.ToDateTime(listitem["ReturnDate"]).ToString("dd/MM/yyyy");                
                lblPurposeoftravel.Text = Convert.ToString(listitem["PurposeOfTravel"]);
                lblNotestoTC.Text = Convert.ToString(listitem["NotestoTC"]);
            }
        }

        private void GetCommentHistory(string strRefno)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("UserName", typeof(string)));
            dt.Columns.Add(new DataColumn("Comments", typeof(string)));


            string lstURL = HrWebUtility.GetListUrl("TravelCommentsHistory");
            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist.GetItems(oquery);
            foreach (SPListItem listitem in collitems)
            {
                string strModified = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy H:mm:ss");
                string strAuthor = Convert.ToString(listitem["ApproverName"]);
                string strComments = Convert.ToString(listitem["Comment"]);

                dt.Rows.Add(new string[] { strModified, strAuthor, strComments });
            }



            gdCommentHistory.DataSource = dt;
            gdCommentHistory.DataBind();


        }

        private SPListItemCollection GetListData(string GetListByName, string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl(GetListByName);
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
            SPListItemCollection collectionItems = oList.GetItems(oQuery);

            return collectionItems;
        }

        private string GetUser(string strAuthor)
        {
            string strName = "";
            string[] tmparr = strAuthor.Split('|');
            strAuthor = tmparr[tmparr.Length - 1];
            if (strAuthor != "")
            {
                using (HostingEnvironment.Impersonate())
                {
                    using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
                    {

                        PrincipalContext context1 = new PrincipalContext(ContextType.Domain);

                        string strUserEmailID = strAuthor.Substring(strAuthor.IndexOf('#') + 1);

                        string userWithoutDomain = strAuthor.Substring(0, strAuthor.IndexOf('@'));
                        string userName = userWithoutDomain.Substring(userWithoutDomain.IndexOf('#') + 1);

                        string strUserName = SPContext.Current.Web.CurrentUser.LoginName;
                        strName = strUserName;
                        UserPrincipal foundUser =
                            UserPrincipal.FindByIdentity(context1, userName);
                        if (foundUser != null)
                        {
                            DirectoryEntry directoryEntry = foundUser.GetUnderlyingObject() as DirectoryEntry;

                            DirectorySearcher searcher = new DirectorySearcher(directoryEntry);


                            searcher.Filter = string.Format("(mail={0})", strUserEmailID);

                            SearchResult result = searcher.FindOne();

                            strName = result.Properties["name"][0].ToString();
                        }

                    }
                }
            }
            return strName;
        }

        private string VerifyUser(string username, string refno)
        {
            string Error = "ACCESSDENIED";
            string lstURL = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
            SPListItemCollection collectionItems = olist.GetItems(oQuery);
            if (collectionItems.Count > 0)
            {
                SPListItem item = collectionItems[0];
                string currapprover = Convert.ToString(item["PendingWith"]);
                ViewState["ApprovalStatus"] = currapprover;
                string initiator = Convert.ToString(item["Author"]);
                string typeoftravel = Convert.ToString(item["TypeofTravel"]);
                string position = Convert.ToString(item["PositionTitle"]);
                string Status = Convert.ToString(item["Status"]);
                string strManager = string.Empty;
                if (item["ManagerName"] != null)
                {
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(item["ManagerName"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strManager = workers[coworker];
                    }
                }
                string NextApprover = GetApprover(item);
               
                if (Status == "Approved" || Status == "Rejected")
                {
                    bool bTC = ISTravelCoordinator(username);
                    if (bTC)
                    {                        
                        btnPDF.Visible = true;
                        btnApprove.Visible = false;
                        btnReject.Visible = false;
                    }
                }
                if (NextApprover == username && currapprover == "TC")
                {
                    Error = "";
                    btnPDF.Visible = true;
                    btnReject.Visible = false;
                    btnApprove.Visible = true;
                    btnApprove.Text = "Acknowledge";
                    txtComments.Enabled = true;
                }
                else if (NextApprover == username)
                {
                    Error = "";
                    if (Status == "Rejected")
                    {
                        btnReject.Visible = false;
                        btnApprove.Visible = false;
                        txtComments.Enabled = false;
                    }
                    else
                    {
                        btnReject.Visible = true;
                        btnApprove.Visible = true;
                        btnApprove.Text = "Approve";
                        txtComments.Enabled = true;
                    }
                }
                else
                {
                    btnApprove.Visible = false;
                    btnReject.Visible = false;
                    txtComments.Enabled = false;
                    string lstTravelAppInfo = HrWebUtility.GetListUrl("TravelApprovalInfo");
                    SPList splistTravelAppInfo = SPContext.Current.Site.RootWeb.GetList(lstTravelAppInfo);
                    bool bfound = false;
                    SPListItemCollection collitemsTravelAppInfo = splistTravelAppInfo.Items;
                    foreach (SPListItem item1 in collitemsTravelAppInfo)
                    {

                        bool bTC = ISTravelCoordinator(username);

                        if (Convert.ToString(item1["CEOApprover"]).Contains(username) ||
                            Convert.ToString(item1["ChairmanApprover"]).Contains(username) ||
                            bTC || strManager.Contains(username))
                        {
                            bfound = true;
                            Error = "";
                            break;
                        }
                    }
                    if (!bfound)
                    {
                        Error = "";
                        string[] tmparr = initiator.Split('|');
                        initiator = tmparr[tmparr.Length - 1];
                        if (initiator.Contains(username))
                        {
                            Error = "";
                        }
                        // Check if logged in user is the one who travels, if not then return message as access denied.
                        else if (username != Convert.ToString(item["TravellerEmailID"]))
                        {
                            Error = "ACCESSDENIED";
                        }
                    }
                }
            }
            return Error;
        }

        private bool ISTravelCoordinator(string username)
        {
            bool bTC = false;
            string lstURL2 = HrWebUtility.GetListUrl("TravelCoordinatorApprovalInfo");
            SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL2);
            SPQuery oquery1 = new SPQuery();
            oquery1.Query = string.Concat("<Where><Eq><FieldRef Name=\'TravelCoordinator\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            SPListItemCollection collitems1 = olist2.GetItems(oquery1);
            if (collitems1.Count > 0)
            {
                bTC = true;
            }
            return bTC;
        }

        private string GetApprover(SPListItem item)
        {
            string strApprover = string.Empty;
            string PendingWith = Convert.ToString(item["PendingWith"]);
            string strManager = string.Empty;
            if (item["ManagerName"] != null)
            {
                SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(item["ManagerName"].ToString());
                for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                {
                    strManager = workers[coworker];
                }
            }
            if (PendingWith == "Manager")
            {
                strApprover = strManager;
            }
            else
            {
                string lstTravelAppInfo = HrWebUtility.GetListUrl("TravelApprovalInfo");
                SPList splistTravelAppInfo = SPContext.Current.Site.RootWeb.GetList(lstTravelAppInfo);

                SPListItemCollection collitemsTravelAppInfo = splistTravelAppInfo.Items;
                if (collitemsTravelAppInfo != null && collitemsTravelAppInfo.Count > 0)
                {
                    if (PendingWith == "CEO")
                    {
                        strApprover = Convert.ToString(collitemsTravelAppInfo[0]["CEOApprover"]); ;
                    }
                    else if (PendingWith == "Chairman")
                    {
                        strApprover = Convert.ToString(collitemsTravelAppInfo[0]["ChairmanApprover"]);
                    }
                    else if (PendingWith == "TC")
                    {
                        //strApprover = Convert.ToString(collitemsTravelAppInfo[0]["TravelCoordinator"]);
                        string lstURL1 = HrWebUtility.GetListUrl("TravelCoordinatorApprovalInfo");
                        string businessunit = Convert.ToString(item["BusinessUnit"]);
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                        SPQuery oquery1 = new SPQuery();
                        // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                                                   "</Value></Eq></Where>";
                        SPListItemCollection collitems1 = olist1.GetItems(oquery1);
                        if (collitems1.Count > 0)
                        {
                            strApprover = Convert.ToString(collitems1[0]["TravelCoordinator"]);
                        }
                    }
                }
            }
            if (strApprover.Contains('#'))
                strApprover = strApprover.Split('#')[1];
            return strApprover;
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateGeneralInfo("Approved");
                //Response.Redirect("/people/Pages/HRWeb/Travelworkflowapproval.aspx");
                if (btnApprove.Text == "Acknowledge")
                    Response.Redirect("/people/Pages/HRWeb/TravelReview.aspx?refno=" + lblReferenceNo.Text);
                else
                    Response.Redirect("/people/Pages/HRWeb/Travelworkflowapproval.aspx");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelReview.btnApprove_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateGeneralInfo("Rejected");
                Response.Redirect("/people/Pages/HRWeb/Travelworkflowapproval.aspx");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelReview.btnReject_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void UpdateGeneralInfo(string status)
        {
            string strRefno = lblReferenceNo.Text.Trim();
            string lstURL = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
            SPListItemCollection collectionItems = olist.GetItems(oQuery);
            if (collectionItems.Count > 0)
            {
                SPListItem item = collectionItems[0];
                string currapprover = Convert.ToString(item["PendingWith"]);
                string initiator = Convert.ToString(item["Author"]);
                string typeoftravel = Convert.ToString(item["TypeofTravel"]);
                string position = Convert.ToString(item["PositionTitle"]);
                string TravellerName = string.Empty;
                
                /*if (item["TravellerName"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(item["TravellerName"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    TravellerName = strpplpicker;
                    TravellerName = GetUserNameFromAD(strpplpicker.TrimEnd(','));
                }*/
                TravellerName = Convert.ToString(item["TravellerName"]);
                ViewState["TravellerName"] = TravellerName;

                string DepartureDate = Convert.ToString(item["DepartureDate"]);                
                ViewState["DepartureDate"] = DepartureDate;
                string[] tmparr = initiator.Split('|');
                initiator = tmparr[tmparr.Length - 1];
                ViewState["Initiator"] = initiator;
                string nextapprover = "";

                if (typeoftravel == "Domestic" && status == "Approved")
                {
                    // This condition was included on CR coming in on 25/11/2014 for SLT user.
                    // In such case, send mail back to initiator to book his own travel. Else normal procedure which will go to TC.
                    if (lblSLT.Text == "Yes")
                    {
                        nextapprover = "Initiator";
                        status = "Approved";
                    }
                    else
                    {
                        if (currapprover == "CEO" || currapprover == "Chairman" || currapprover == "Manager")
                        {
                            nextapprover = "TC";
                            status = "Pending Approval";
                        }
                        else if (currapprover == "TC")
                        {
                            status = "Approved";
                        }
                    }
                }
                else if ((typeoftravel == "International" || typeoftravel == "Domestic & International") && status == "Approved")
                {
                    if (currapprover == "Manager")
                    {
                        nextapprover = "CEO";
                        status = "Pending Approval";
                    }
                    else if (currapprover == "CEO" || currapprover == "Chairman")
                    {
                        nextapprover = "TC";
                        status = "Pending Approval";
                    }
                    else if (currapprover == "TC")
                    {
                        status = "Approved";
                        //nextapprover = "TC";
                    }
                }
                else if (status == "Rejected")
                {
                    status = "Rejected";
                    nextapprover = currapprover;                   
                }

                item["PendingWith"] = nextapprover;
                item["Status"] = status;
                item.Update();
                if (btnApprove.Text != "Acknowledge")
                {
                    SendEmail(item);
                }
            }
        }

        private void UpdateComment()
        {
            string appno = lblReferenceNo.Text.Trim();
            string approveremail = UserName;
            string username = GetUserNameFromAD(approveremail);
            string approverid = UserName.Split('@')[0].Trim();
            string comment = txtComments.Text;
            string approverstep = Convert.ToString(ViewState["ApprovalStatus"]);

            string lstURL = HrWebUtility.GetListUrl("TravelCommentsHistory");
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

            SPListItem listitem = oList.AddItem();
            listitem["Title"] = appno;
            listitem["ApproverID"] = approverid;
            listitem["ApproverName"] = username;
            listitem["ApproverEmail"] = approveremail;
            listitem["ApproverStep"] = approverstep;
            listitem["Comment"] = comment;
            listitem.Update();
        }

        private string GetUserNameFromAD(string approveremail)
        {
            string strName = "";
            using (HostingEnvironment.Impersonate())
            {
                using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
                {

                    PrincipalContext context1 = new PrincipalContext(ContextType.Domain);


                    string userName = approveremail.Split('@')[0].Trim();
                    strName = userName;
                    UserPrincipal foundUser =
                        UserPrincipal.FindByIdentity(context1, userName);
                    if (foundUser != null)
                    {
                        DirectoryEntry directoryEntry = foundUser.GetUnderlyingObject() as DirectoryEntry;

                        DirectorySearcher searcher = new DirectorySearcher(directoryEntry);


                        searcher.Filter = string.Format("(mail={0})", approveremail);

                        SearchResult result = searcher.FindOne();

                        strName = result.Properties["name"][0].ToString();
                    }

                }
            }
            return strName;
        }

        private void SendEmail(SPListItem item)
        {
            string strRefNo = lblReferenceNo.Text;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string status = Convert.ToString(item["Status"]);

                SPSite site = SPContext.Current.Site;

                SPWeb web = site.OpenWeb();
                string lstURL = HrWebUtility.GetListUrl("EmailConfig");
                SPList lst = SPContext.Current.Site.RootWeb.GetList(lstURL);
                //SPList lst = web.Lists["EmailConfig"];

                SPQuery oQuery = new SPQuery();
                oQuery.Query = "<Query><Where><Eq><FieldRef Name='FormType' /><Value Type='Text'>AppToHire</Value></Eq></Where></Query>";


                oQuery.ViewFields = string.Concat(
                    "<FieldRef Name='FormType' />",
                               "<FieldRef Name='Title' />",
                               "<FieldRef Name='EmailIP' />",
                               "<FieldRef Name='ApprovalSubject' />",
                               "<FieldRef Name='ApprovalMessage' />",
                               "<FieldRef Name='ApprovedSubject' />",
                               "<FieldRef Name='ApprovedMessage' />",
                               "<FieldRef Name='RejectedSubject' />",
                               "<FieldRef Name='RejectedMessage' />",
                               "<FieldRef Name='RevertedSubject' />",
                               "<FieldRef Name='RevertedMessage' />");
                SPListItemCollection collListItems = lst.GetItems(oQuery);

                foreach (SPListItem itm in collListItems)
                {
                    if (Convert.ToString(itm["FormType"]) == "TravelRequest")
                    {
                        //send email
                        string strFrom = "";
                        string strTo = "";
                        string strCC = string.Empty;
                        string strSubject = "";
                        string strMessage = "";

                        SmtpClient smtpClient = new SmtpClient();
                        smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                        smtpClient.Port = 25;
                        //smtpClient.Host = "smtp.gmail.com";
                        string url = site.Url + "/pages/hrweb/TravelReview.aspx?refno=" + strRefNo;
                        strFrom = Convert.ToString(itm["Title"]);

                        if (status == "Approved" && Convert.ToString(item["TypeofTravel"]) == "Domestic")
                        {
                            strCC = Convert.ToString(ViewState["Initiator"]);
                            strTo = Convert.ToString(item["TravellerEmailID"]);
                            strSubject = Convert.ToString(itm["RevertedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                            
                            strMessage = Convert.ToString(itm["RevertedMessage"]).
                                Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").
                                Replace("&lt;TRAVELLER&gt;", Convert.ToString(ViewState["TravellerName"])).
                                Replace("&lt;", "<").Replace("&gt;", ">");
                            //.Replace("&lt;TRAVELDATE&gt;", Convert.ToDateTime(ViewState["DepartureDate"]).ToString("dd/MM/yyyy"));
                        }
                        else if (status == "Approved")
                        {
                            // For international travel
                            strCC = Convert.ToString(ViewState["Initiator"]);
                            strTo = Convert.ToString(item["TravellerEmailID"]);
                            strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                            strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;TRAVELLER&gt;", Convert.ToString(ViewState["TravellerName"]));
                            //.Replace("&lt;TRAVELDATE&gt;", Convert.ToDateTime(ViewState["DepartureDate"]).ToString("dd/MM/yyyy"));
                        }
                        else if (status == "Rejected")
                        {
                            strTo = Convert.ToString(ViewState["Initiator"]);
                            strSubject = Convert.ToString(itm["RejectedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                            strMessage = Convert.ToString(itm["RejectedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;TRAVELLER&gt;", Convert.ToString(ViewState["TravellerName"]));
                            //.Replace("&lt;TRAVELDATE&gt;", Convert.ToDateTime(ViewState["DepartureDate"]).ToString("dd/MM/yyyy"));
                        }
                        else
                        {
                            strCC = GetApprover(item);
                            string[] tmparr = strCC.Split('|');
                            strCC = tmparr[tmparr.Length - 1];
                            strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                            strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;TRAVELLER&gt;", Convert.ToString(ViewState["TravellerName"]));
                            //.Replace("&lt;TRAVELDATE&gt;", Convert.ToDateTime(ViewState["DepartureDate"]).ToString("dd/MM/yyyy"));
                            if (strCC.Contains("#"))
                                strCC = strCC.Split('#')[1];

                            if (Convert.ToString(item["PendingWith"]) == "TC")
                            {
                                strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                                strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;TRAVELLER&gt;", Convert.ToString(ViewState["TravellerName"]));
                                //.Replace("&lt;TRAVELDATE&gt;", Convert.ToDateTime(ViewState["DepartureDate"]).ToString("dd/MM/yyyy"));
                                string TravellerName = string.Empty;

                                if (item["TravellerEmailID"] != null)
                                {
                                    TravellerName = Convert.ToString(item["TravellerEmailID"]);
                                }
                                /*if (TravellerName.Contains("#"))
                                    TravellerName = strCC.Split('#')[1];*/

                                strTo = TravellerName;
                            }
                        }

                        if (strCC.Contains("#"))
                            strCC = strCC.Split('#')[1];

                        MailMessage mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");

                        // if To is empty, make CC as To and CC as empty.
                        string newTo = string.Empty;
                        if (strTo.Trim().Length == 0)
                        {
                            newTo = strCC;
                            strCC = string.Empty;
                        }
                        else
                        {
                            newTo = strTo;
                        }
                        string[] mailto = newTo.Split(';');
                        var distinctToIDs = mailto.Distinct();
                        foreach (string s in distinctToIDs)
                        {
                            if (s.Trim() != "")
                                mailMessage.To.Add(s);
                        }

                        string[] mailCC = strCC.Split(';');
                        var distinctIDs = mailCC.Distinct();
                        foreach (string s in distinctIDs)
                        {
                            if (s.Trim() != "")
                            {
                                if (!distinctToIDs.Contains(s.Trim()))
                                {
                                    mailMessage.CC.Add(s);
                                }
                            }
                        }

                        mailMessage.Subject = strSubject;
                        mailMessage.Body = strMessage;
                        mailMessage.IsBodyHtml = true;

                        try
                        {
                            smtpClient.Send(mailMessage);
                        }
                        catch (SmtpFailedRecipientException)
                        {
                            lblError.Text = "Notification could not be sent to some of the recipients, due to invalid email ids.";
                        }
                        
                        SaveEmailDetails(strFrom, strTo, strSubject, strMessage);
                        break;
                    }
                }
            });
        }

        private void SaveEmailDetails(string strFrom, string strTo, string strSubject, string strMessage)
        {

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                //SPSite oSite = SPContext.Current.Site;
                //using (SPWeb oWeb = oSite.OpenWeb())
                //{
                    string lstURL = HrWebUtility.GetListUrl("EmailDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    //SPList oList = oWeb.Lists["EmailDetails"];
                    SPListItem oItem = oList.AddItem();
                    oItem["Title"] = strFrom;
                    oItem["To"] = strTo;
                    oItem["Subject"] = strSubject;
                    oItem["Comments"] = strMessage;
                    oItem["FormType"] = "Travel";
                    oItem.Update();
                //}
            });
        }

        protected void btnPDF_Click(object sender, EventArgs e)
        {
            try
            {
                string srtRefno = lblReferenceNo.Text;
                if (string.Equals(lblBookingRequirements.Text, "Combined Travel", StringComparison.OrdinalIgnoreCase))
                {
                    GenerateCombinedTravelPDF(srtRefno);
                }
                else if (string.Equals(lblBookingRequirements.Text, "Accommodation Only", StringComparison.OrdinalIgnoreCase))
                {
                    GenerateAccommodationOnlyPDF(srtRefno);
                }
                else if (string.Equals(lblBookingRequirements.Text, "Vehicle Only", StringComparison.OrdinalIgnoreCase))
                {
                    GenerateVehicleOnlyPDF(srtRefno);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelReview.btnPDF_Click", ex.Message);
                lblError.Text = ex.InnerException.Message;
                //lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void GenerateVehicleOnlyPDF(string strRefno)
        {
            string filename = "TravelRequest_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font legddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font legcellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(phEmpty);

            GenerateTraveSummarySection(headerTbl, pdfDoc);

            pdfDoc.Add(phEmpty);
            Paragraph CTpositionHead = new Paragraph("            Vehicle Details", headFont);
            pdfDoc.Add(CTpositionHead);
            pdfDoc.Add(phEmpty);

            PdfPTable MotorVehicleTable = new PdfPTable(2);
            float[] MotorVehicleTableWidth = new float[] { 50f, 50f };
            MotorVehicleTable.SetWidths(MotorVehicleTableWidth);

            PdfPTable MotorVehicleLeftTable = new PdfPTable(2);
            float[] MotorVehicleLeftTableWidth = new float[] { 60f, 40f };
            MotorVehicleLeftTable.SetWidths(MotorVehicleLeftTableWidth);

            Chunk MotorVehicleChnk = new Chunk(" Motor Vehicle: ", ddlLabelFonts);
            Phrase MotorVehiclePh1 = new Phrase(MotorVehicleChnk);
            PdfPCell MotorVehiclecell = new PdfPCell(MotorVehiclePh1);
            MotorVehiclecell.Border = 0;
            MotorVehicleLeftTable.AddCell(MotorVehiclecell);
           
            MotorVehicleChnk = new Chunk(lblMotorVehicle.Text, ddlFonts);
            MotorVehiclePh1 = new Phrase(MotorVehicleChnk);
            PdfPCell MotorVehiclecell1 = new PdfPCell(MotorVehiclePh1);
            MotorVehiclecell1.Border = 0;
            MotorVehicleLeftTable.AddCell(MotorVehiclecell1);

            PdfPCell leftCell = new PdfPCell(MotorVehicleLeftTable);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            MotorVehicleTable.AddCell(leftCell);

            PdfPTable MotorVehicleRightTable = new PdfPTable(2);
            float[] MotorVehicleRightTableWidth = new float[] { 60f, 40f };
            MotorVehicleRightTable.SetWidths(MotorVehicleRightTableWidth);

            PdfPCell rightCell = new PdfPCell(MotorVehicleRightTable);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            MotorVehicleTable.AddCell(rightCell);

            pdfDoc.Add(MotorVehicleTable);
            pdfDoc.Add(phEmpty);
            //Vehicle Tabel
            string LegNo = string.Empty;
            string PickUpDate = string.Empty;
            string PickUpTime = string.Empty;
            string PULocation = string.Empty;
            string DropOffDate = string.Empty;
            string DropOffTime = string.Empty;
            string DropOffLocation = string.Empty;

            PdfPTable VehicleHeaderTable = new PdfPTable(1);


            PdfPTable VehicleTable = new PdfPTable(7);
            float[] VehicleTableWidth = new float[] { 10f, 12f, 12f, 21f, 12f, 12f, 21f };
            VehicleTable.SetWidths(VehicleTableWidth);

            Chunk VehicleChnk = new Chunk(" Leg  ", legcellFnt);
            Phrase VehiclePh1 = new Phrase(VehicleChnk);
            PdfPCell Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" PickUp  Date ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);
            
            VehicleChnk = new Chunk(" PickUp Time ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" P/U Location ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" DropOff Date ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" DropOff Time ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" DropOff Location ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            SPListItemCollection VehiclecollectionItems = GetListData("HRWebTravelVehicle", strRefno);
            if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
            {
              foreach (SPListItem ListItems in VehiclecollectionItems)
               {
                    LegNo = Convert.ToString(ListItems["LegNo"]);
                    if (Convert.ToString(ListItems["PickUpDate"]) != "")
                    {
                        PickUpDate = Convert.ToDateTime(ListItems["PickUpDate"]).ToString("dd/MM/yyyy");
                        PickUpTime = Convert.ToDateTime(ListItems["PickUpTime"]).ToString("hh:mm tt");
                    }
                    PULocation = Convert.ToString(ListItems["PULocation"]);
                    if (Convert.ToString(ListItems["DropOffDate"]) != "")
                    {
                        DropOffDate = Convert.ToDateTime(ListItems["DropOffDate"]).ToString("dd/MM/yyyy");
                        DropOffTime = Convert.ToDateTime(ListItems["DropOffTime"]).ToString("hh:mm tt");
                    }
                    DropOffLocation = Convert.ToString(ListItems["DropOffLocation"]);

                    VehicleChnk = new Chunk(LegNo, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (PickUpDate != null)
                    {
                        VehicleChnk = new Chunk(PickUpDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(PickUpTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(PULocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (DropOffDate != null)
                    {
                        VehicleChnk = new Chunk(DropOffDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(DropOffTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(DropOffLocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                }
            }

            PdfPCell VehicleTabelcell = new PdfPCell(VehicleTable);
            VehicleTabelcell.Padding = 1f;
            VehicleHeaderTable.AddCell(VehicleTabelcell);

            pdfDoc.Add(VehicleHeaderTable);


            //Comment History
            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            Chunk PosTypeChnk = new Chunk(" Date ", cellFnt);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("            Approval History", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfAppHistory);


            pdfDoc.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();


        }

        private void GenerateAccommodationOnlyPDF(string strRefno)
        {
            string filename = "TravelRequest_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font legddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font legcellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(phEmpty);

            

            GenerateTraveSummarySection(headerTbl, pdfDoc);

            Paragraph CTpositionHead = new Paragraph("            Accommodation Requirements", headFont);
            pdfDoc.Add(CTpositionHead);
            pdfDoc.Add(phEmpty);

            //Accommodation Tabel
           
            string CheckInDate = string.Empty;
            string CheckOutDate = string.Empty;
            string HotelName = string.Empty;
            string NoOfNights = string.Empty;

            PdfPTable AccomHeaderTable = new PdfPTable(1);


            PdfPTable AccommodationTable = new PdfPTable(4);
            float[] AccomTableWidth = new float[] { 25f, 25f, 25f, 25f };
            AccommodationTable.SetWidths(AccomTableWidth);

            Chunk AccomChnk = new Chunk(" CheckIn  Date ", legcellFnt);
            Phrase AccomPh1 = new Phrase(AccomChnk);
            PdfPCell Accomcell = new PdfPCell(AccomPh1);
            Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            AccommodationTable.AddCell(Accomcell);            

            AccomChnk = new Chunk(" CheckOut Date ", legcellFnt);
            AccomPh1 = new Phrase(AccomChnk);
            Accomcell = new PdfPCell(AccomPh1);
            Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            AccommodationTable.AddCell(Accomcell);

            AccomChnk = new Chunk(" Hotel Name ", legcellFnt);
            AccomPh1 = new Phrase(AccomChnk);
            Accomcell = new PdfPCell(AccomPh1);
            Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            AccommodationTable.AddCell(Accomcell);

            AccomChnk = new Chunk(" No Of Nights ", legcellFnt);
            AccomPh1 = new Phrase(AccomChnk);
            Accomcell = new PdfPCell(AccomPh1);
            Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            AccommodationTable.AddCell(Accomcell);            

            SPListItemCollection collectionItems = GetListData("HRWebTravelAccommodation", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem ListItems in collectionItems)
                {
                    if (Convert.ToString(ListItems["CheckIn"]) != "")
                        CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]).ToString("dd/MM/yyyy");
                    if (Convert.ToString(ListItems["CheckOut"]) != "")
                        CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]).ToString("dd/MM/yyyy");
                    HotelName = Convert.ToString(ListItems["HotelName"]);
                    NoOfNights = Convert.ToString(ListItems["NoOfNights"]);

                    if (CheckInDate != null)
                    {
                        AccomChnk = new Chunk(CheckInDate, legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }
                    else
                    {
                        AccomChnk = new Chunk("", legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }

                    if (CheckOutDate != null)
                    {
                        AccomChnk = new Chunk(CheckOutDate, legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }
                    else
                    {
                        AccomChnk = new Chunk("", legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }

                    AccomChnk = new Chunk(HotelName, legddlFonts);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(NoOfNights, legddlFonts);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    AccommodationTable.AddCell(Accomcell);
        
                }
            }

            PdfPCell AccomTabelcell = new PdfPCell(AccommodationTable);
            AccomTabelcell.Padding = 1f;
            AccomHeaderTable.AddCell(AccomTabelcell);

            pdfDoc.Add(AccomHeaderTable);
            

            //Comment History
            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            Chunk PosTypeChnk = new Chunk(" Date ", cellFnt);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("            Approval History", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfAppHistory);


            pdfDoc.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

        }

        private void GenerateCombinedTravelPDF(string strRefno)
        {
            string filename = "TravelRequest_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font legddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font legcellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            GenerateTraveSummarySection(headerTbl, pdfDoc);


            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(phEmpty);

            Paragraph CTpositionHead = new Paragraph("            Combined Travel Itinerary", headFont);
            pdfDoc.Add(CTpositionHead);
            //pdfDoc.Add(phEmpty);

            /*PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblCombinedTravelItineraryLeft = new PdfPTable(2);
            float[] tblCombinedTravelWidth = new float[] { 60f, 40f };
            tblCombinedTravelItineraryLeft.SetWidths(tblCombinedTravelWidth);

            Chunk NoofLegs = new Chunk("No of Legs:", ddlLabelFonts);
            Phrase NoofLegsValPh1 = new Phrase(NoofLegs);
            PdfPCell NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
            NoofLegsvalcell.Border = 0;
            tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

            NoofLegs = new Chunk(lblNoOfLegs.Text, ddlFonts);
            NoofLegsValPh1 = new Phrase(NoofLegs);
            NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
            NoofLegsvalcell.Border = 0;
            tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

            NoofLegs = new Chunk("Travel Type:", ddlLabelFonts);
            NoofLegsValPh1 = new Phrase(NoofLegs);
            NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
            NoofLegsvalcell.Border = 0;
            tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

            if (Convert.ToString(ViewState["Flight"]) != "")
            {
                NoofLegs = new Chunk(lblFlight.Text, ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                NoofLegsvalcell.HorizontalAlignment = 0;
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
            }

            if ((Convert.ToString(ViewState["HireVehicle"]) != "") || (Convert.ToString(ViewState["CompanyVehicle"]) != "") || (Convert.ToString(ViewState["PersonalVehicle"]) != ""))
            {


                NoofLegs = new Chunk(VehicleRequirement.Text, ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                NoofLegsvalcell.Colspan = 2;
                NoofLegsvalcell.HorizontalAlignment = 2;
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

                if (Convert.ToString(ViewState["PersonalVehicle"]) != "")
                {
                    NoofLegs = new Chunk("", ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);                                       
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

                    NoofLegs = new Chunk(lblPersonalVehicle.Text, ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);                    
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
                }
                if (Convert.ToString(ViewState["CompanyVehicle"]) != "")
                {
                    NoofLegs = new Chunk("", ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

                    NoofLegs = new Chunk(lblCompanyVehicle.Text, ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);                   
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
                }
                if (Convert.ToString(ViewState["HireVehicle"]) != "")
                {
                    NoofLegs = new Chunk("", ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
                    
                    NoofLegs = new Chunk(lblHireVehicle.Text, ddlFonts);
                    NoofLegsValPh1 = new Phrase(NoofLegs);
                    NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);                    
                    NoofLegsvalcell.Border = 0;
                    tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
                }

            }
            if (Convert.ToString(ViewState["Accommodation"]) != "")
            {
                NoofLegs = new Chunk("", ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

                NoofLegs = new Chunk(lblAccommodation.Text, ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);               
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
            }

            if (Convert.ToString(ViewState["NoAccommodation"]) != "")
            {
                NoofLegs = new Chunk("", ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);

                NoofLegs = new Chunk(lblAccomNotReq.Text, ddlFonts);
                NoofLegsValPh1 = new Phrase(NoofLegs);
                NoofLegsvalcell = new PdfPCell(NoofLegsValPh1);
                NoofLegsvalcell.Border = 0;
                tblCombinedTravelItineraryLeft.AddCell(NoofLegsvalcell);
            }

            PdfPCell CTleftCell = new PdfPCell(tblCombinedTravelItineraryLeft);

            CTleftCell.Border = 0;
            CTleftCell.Padding = 0f;

            headerTbl1.AddCell(CTleftCell);

            PdfPCell CTRightCell = new PdfPCell();

            CTRightCell.Border = 0;
            CTRightCell.Padding = 0f;

            headerTbl1.AddCell(CTRightCell);
            pdfDoc.Add(headerTbl1);*/

            //Combined Travel Leg

            int cntLeg = 0;
            string lstURL = HrWebUtility.GetListUrl("HRWebCombinedTravelItinerary");

            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist.GetItems(oquery);

            foreach (SPListItem listitem in collitems)
                cntLeg = Convert.ToInt16(listitem["NoOfLegs"]);

            for (int inx = 1; inx <= cntLeg; inx++)
            {
                string flightDepartureDate = string.Empty;
                string DepartureLocation = string.Empty;
                string FlightCarrier = string.Empty;
                string FlightNo = string.Empty;
                string flightDepartureTime = string.Empty;
                string DestinationLocation = string.Empty;

                SPListItemCollection collectionItems = GetListData("HRWebTravelFlight", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {

                    foreach (SPListItem ListItems in collectionItems)
                    {
                        string LegCount = Convert.ToString(ListItems["LegNo"]);

                        if (LegCount == Convert.ToString(inx))
                        {
                            if(Convert.ToString(ListItems["DepartureDate"])!="")
                                flightDepartureDate = Convert.ToDateTime(ListItems["DepartureDate"]).ToString("dd/MM/yyyy");
                            DepartureLocation = Convert.ToString(ListItems["DepartureLocation"]);
                            FlightCarrier = Convert.ToString(ListItems["FlightCarrier"]);
                            FlightNo = Convert.ToString(ListItems["FlightNo"]);
                            if (Convert.ToString(ListItems["DepartureDate"]) != "")
                                flightDepartureTime = Convert.ToDateTime(ListItems["FlightDepartureTime"]).ToString("hh:mm tt");
                            DestinationLocation = Convert.ToString(ListItems["TravelTo"]);
                        }

                    }
                }

                string CheckInDate = string.Empty;
                string HotelName = string.Empty;
                string NoOfNights = string.Empty;
                string CheckOutDate = string.Empty;

                SPListItemCollection AccomcollectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                if (AccomcollectionItems != null && AccomcollectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in AccomcollectionItems)
                    {
                        string LegCount = Convert.ToString(ListItems["LegNo"]);

                        if (LegCount == Convert.ToString(inx))
                        {
                            if (Convert.ToString(ListItems["CheckIn"]) != "")
                                CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]).ToString("dd/MM/yyyy");
                            HotelName = Convert.ToString(ListItems["HotelName"]);
                            NoOfNights = Convert.ToString(ListItems["NoOfNights"]);
                            if (Convert.ToString(ListItems["CheckIn"]) != "")
                                CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]).ToString("dd/MM/yyyy");
                        }

                    }
                }

                string PickUpDate = string.Empty;
                string PickUpTime = string.Empty;
                string PULocation = string.Empty;
                string DropOffDate = string.Empty;
                string DropOffTime = string.Empty;
                string DropOffLocation = string.Empty;

                SPListItemCollection VehiclecollectionItems = GetListData("HRWebTravelVehicle", strRefno);
                if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in VehiclecollectionItems)
                    {
                        string LegCount = Convert.ToString(ListItems["LegNo"]);

                        if (LegCount == Convert.ToString(inx))
                        {
                            if (Convert.ToString(ListItems["PickUpDate"]) != "")
                            {
                                PickUpDate = Convert.ToDateTime(ListItems["PickUpDate"]).ToString("dd/MM/yyyy");
                                PickUpTime = Convert.ToDateTime(ListItems["PickUpTime"]).ToString("hh:mm tt");
                            }
                            PULocation = Convert.ToString(ListItems["PULocation"]);
                            if (Convert.ToString(ListItems["DropOffDate"]) != "")
                            {
                                DropOffDate = Convert.ToDateTime(ListItems["DropOffDate"]).ToString("dd/MM/yyyy");
                                DropOffTime = Convert.ToDateTime(ListItems["DropOffTime"]).ToString("hh:mm tt");
                            }
                            DropOffLocation = Convert.ToString(ListItems["DropOffLocation"]);
                        }
                    }
                }

                

                PdfPTable LegTable = new PdfPTable(1);

                Chunk TravelLegChnk = new Chunk(" TRAVEL LEG " + inx + ":", ddlFonts);
                Phrase TravelLegPh1 = new Phrase(TravelLegChnk);
                PdfPCell Legcell = new PdfPCell(TravelLegPh1);


                LegTable.AddCell(Legcell);


                pdfDoc.Add(new Phrase(" "));

                if (Convert.ToString(ViewState["Flight"]) != "")
                {
                    PdfPTable FlightTable = new PdfPTable(7);

                    float[] TableWidth = new float[] { 10f, 16f, 15f, 15f, 16f, 16f, 12f };
                    FlightTable.SetWidths(TableWidth);

                    Chunk FlightChnk = new Chunk(" ", legcellFnt);
                    Phrase FlightPh1 = new Phrase(FlightChnk);
                    PdfPCell Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(" Travel From ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);
                    
                    FlightChnk = new Chunk("Departure Date ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(" Travel To ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk("Daparture Time ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(" Flight Carrier ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(" Flight No ", legcellFnt);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    Flightcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    FlightTable.AddCell(Flightcell);
                   
                    FlightChnk = new Chunk(" Flight ", legddlFonts);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(DepartureLocation, legddlFonts);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    FlightTable.AddCell(Flightcell);
                    
                    if (flightDepartureDate != null)
                    {
                        FlightChnk = new Chunk(flightDepartureDate, legddlFonts);
                        FlightPh1 = new Phrase(FlightChnk);
                        Flightcell = new PdfPCell(FlightPh1);
                        FlightTable.AddCell(Flightcell);
                    }
                    else
                    {
                        FlightChnk = new Chunk("", legddlFonts);
                        FlightPh1 = new Phrase(FlightChnk);
                        Flightcell = new PdfPCell(FlightPh1);
                        FlightTable.AddCell(Flightcell);
                    }

                    FlightChnk = new Chunk(DestinationLocation, legddlFonts);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    FlightTable.AddCell(Flightcell);

                    if (flightDepartureDate != null)
                    {
                        FlightChnk = new Chunk(flightDepartureTime, legddlFonts);
                        FlightPh1 = new Phrase(FlightChnk);
                        Flightcell = new PdfPCell(FlightPh1);
                        FlightTable.AddCell(Flightcell);
                    }
                    else
                    {
                        FlightChnk = new Chunk("", legddlFonts);
                        FlightPh1 = new Phrase(FlightChnk);
                        Flightcell = new PdfPCell(FlightPh1);
                        FlightTable.AddCell(Flightcell);
                    }                    

                    FlightChnk = new Chunk(FlightCarrier, legddlFonts);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    FlightTable.AddCell(Flightcell);

                    FlightChnk = new Chunk(FlightNo, legddlFonts);
                    FlightPh1 = new Phrase(FlightChnk);
                    Flightcell = new PdfPCell(FlightPh1);
                    FlightTable.AddCell(Flightcell);

                    


                    PdfPCell TravelLegFlightcell = new PdfPCell(FlightTable);
                    TravelLegFlightcell.Padding = 4f;
                    LegTable.AddCell(TravelLegFlightcell);
                }

                pdfDoc.Add(new Phrase(" "));

                if (Convert.ToString(ViewState["Accommodation"]) != "")
                {
                    PdfPTable AccommodationTable = new PdfPTable(5);
                    float[] AccomTableWidth = new float[] { 20f, 20f, 20f, 25f, 15f };
                    AccommodationTable.SetWidths(AccomTableWidth);

                    Chunk AccomChnk = new Chunk(" ", legcellFnt);
                    Phrase AccomPh1 = new Phrase(AccomChnk);
                    PdfPCell Accomcell = new PdfPCell(AccomPh1);
                    Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(" CheckIn  Date ", legcellFnt);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(" CheckOut Date ", legcellFnt);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(" Hotel Name ", legcellFnt);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(" No Of Nights ", legcellFnt);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    Accomcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(" Accommodation ", legddlFonts);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    AccommodationTable.AddCell(Accomcell);

                    if (CheckInDate != null)
                    {
                        AccomChnk = new Chunk(CheckInDate, legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);

                        AccomChnk = new Chunk(CheckOutDate, legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }
                    else
                    {
                        AccomChnk = new Chunk("", legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);

                        AccomChnk = new Chunk("", legddlFonts);
                        AccomPh1 = new Phrase(AccomChnk);
                        Accomcell = new PdfPCell(AccomPh1);
                        AccommodationTable.AddCell(Accomcell);
                    }                    

                    AccomChnk = new Chunk(HotelName, legddlFonts);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    AccommodationTable.AddCell(Accomcell);

                    AccomChnk = new Chunk(NoOfNights, legddlFonts);
                    AccomPh1 = new Phrase(AccomChnk);
                    Accomcell = new PdfPCell(AccomPh1);
                    AccommodationTable.AddCell(Accomcell);


                    PdfPCell TravelLegAccomcell = new PdfPCell(AccommodationTable);
                    TravelLegAccomcell.Padding = 4f;
                    LegTable.AddCell(TravelLegAccomcell);
                }
                
                

                pdfDoc.Add(new Phrase(" "));

                if (Convert.ToString(ViewState["HireVehicle"]) != "")
                {

                    PdfPTable VehicleTable = new PdfPTable(7);
                    float[] VehicleTableWidth = new float[] { 12f, 12f, 12f, 20f, 12f, 12f, 20f };
                    VehicleTable.SetWidths(VehicleTableWidth);

                    Chunk VehicleChnk = new Chunk(" ", legcellFnt);
                    Phrase VehiclePh1 = new Phrase(VehicleChnk);
                    PdfPCell Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" PickUp  Date ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" PickUp Time ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" P/U Location ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Date ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Time ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Location ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);



                    VehicleChnk = new Chunk(" Hire Car ", legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (PickUpDate != null)
                    {
                        VehicleChnk = new Chunk(PickUpDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(PickUpTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(PULocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (DropOffDate != null)
                    {
                        VehicleChnk = new Chunk(DropOffDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(DropOffTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(DropOffLocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    PdfPCell TravelLegVehiclecell = new PdfPCell(VehicleTable);
                    TravelLegVehiclecell.Padding = 4f;
                    LegTable.AddCell(TravelLegVehiclecell);
                }

                if (Convert.ToString(ViewState["CompanyVehicle"]) != "")
                {

                    PdfPTable VehicleTable = new PdfPTable(7);
                    float[] VehicleTableWidth = new float[] { 12f, 12f, 12f, 20f, 12f, 12f, 20f };
                    VehicleTable.SetWidths(VehicleTableWidth);

                    Chunk VehicleChnk = new Chunk(" ", legcellFnt);
                    Phrase VehiclePh1 = new Phrase(VehicleChnk);
                    PdfPCell Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" PickUp  Date ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" PickUp Time ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" P/U Location ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Date ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Time ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(" DropOff Location ", legcellFnt);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
                    VehicleTable.AddCell(Vehiclecell);



                    VehicleChnk = new Chunk(" Hire Car ", legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (PickUpDate != null)
                    {
                        VehicleChnk = new Chunk(PickUpDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(PickUpTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(PULocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    if (DropOffDate != null)
                    {
                        VehicleChnk = new Chunk(DropOffDate, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk(DropOffTime, legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }
                    else
                    {
                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);

                        VehicleChnk = new Chunk("", legddlFonts);
                        VehiclePh1 = new Phrase(VehicleChnk);
                        Vehiclecell = new PdfPCell(VehiclePh1);
                        VehicleTable.AddCell(Vehiclecell);
                    }

                    VehicleChnk = new Chunk(DropOffLocation, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    VehicleTable.AddCell(Vehiclecell);

                    PdfPCell TravelLegVehiclecell = new PdfPCell(VehicleTable);
                    TravelLegVehiclecell.Padding = 4f;
                    LegTable.AddCell(TravelLegVehiclecell);
                }

                pdfDoc.Add(new Phrase(" "));

                pdfDoc.Add(LegTable);




            }









            //Comment History
            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            Chunk PosTypeChnk = new Chunk(" Date ", cellFnt);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("            Approval History", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfAppHistory);


            pdfDoc.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();
        }

        private void GenerateTraveSummarySection(PdfPTable headerTbl, Document pdfDoc)
        {
            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);
            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            Paragraph TravelSummaryHead = new Paragraph("            Travel Summary", headFont);
            pdfDoc.Add(TravelSummaryHead);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(phEmpty);

            PdfPTable tblTravelSummaryLeft = new PdfPTable(2);
            float[] tblTravelSummaryWidth = new float[] { 60f, 40f };
            tblTravelSummaryLeft.SetWidths(tblTravelSummaryWidth);

            Chunk AppDate = new Chunk("Application Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(AppDate);
            PdfPCell AppDatevalcell = new PdfPCell(ValPh1);
            AppDatevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(AppDatevalcell);

            Chunk AppDateVal = new Chunk(lblApplicationDate.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(AppDateVal);
            PdfPCell AppDateValcell2 = new PdfPCell(ValPh2);
            AppDateValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(AppDateValcell2);

            Chunk TypeofTravel = new Chunk("Type of Travel: ", ddlLabelFonts);
            Phrase TypeofTravelValPh1 = new Phrase(TypeofTravel);
            PdfPCell TypeofTravelvalcell = new PdfPCell(TypeofTravelValPh1);
            TypeofTravelvalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(TypeofTravelvalcell);

            Chunk TypeofTravelVal = new Chunk(lblTypeofTravel.Text, ddlFonts);
            Phrase TypeofTravelValPh2 = new Phrase(TypeofTravelVal);
            PdfPCell TypeofTravelValcell2 = new PdfPCell(TypeofTravelValPh2);
            TypeofTravelValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(TypeofTravelValcell2);

            Chunk BookingRequirements = new Chunk("Booking Requirements: ", ddlLabelFonts);
            Phrase BookingRequirementsValPh1 = new Phrase(BookingRequirements);
            PdfPCell BookingRequirementsvalcell = new PdfPCell(BookingRequirementsValPh1);
            BookingRequirementsvalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(BookingRequirementsvalcell);

            Chunk BookingRequirementsVal = new Chunk(lblBookingRequirements.Text, ddlFonts);
            Phrase BookingRequirementsValPh2 = new Phrase(BookingRequirementsVal);
            PdfPCell BookingRequirementsValcell2 = new PdfPCell(BookingRequirementsValPh2);
            BookingRequirementsValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(BookingRequirementsValcell2);

            if (lblTypeofTravel.Text != "Domestic")
            {
                BookingRequirements = new Chunk("Visa Required: ", ddlLabelFonts);
                BookingRequirementsValPh1 = new Phrase(BookingRequirements);
                BookingRequirementsvalcell = new PdfPCell(BookingRequirementsValPh1);
                BookingRequirementsvalcell.Border = 0;
                tblTravelSummaryLeft.AddCell(BookingRequirementsvalcell);

                BookingRequirementsVal = new Chunk(lblVisaReq.Text, ddlFonts);
                BookingRequirementsValPh2 = new Phrase(BookingRequirementsVal);
                BookingRequirementsValcell2 = new PdfPCell(BookingRequirementsValPh2);
                BookingRequirementsValcell2.Border = 0;
                tblTravelSummaryLeft.AddCell(BookingRequirementsValcell2);
            }

            Chunk TravellerName = new Chunk("Traveller Name: ", ddlLabelFonts);
            Phrase TravellerNameValPh1 = new Phrase(TravellerName);
            PdfPCell TravellerNamevalcell = new PdfPCell(TravellerNameValPh1);
            TravellerNamevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(TravellerNamevalcell);

            Chunk TravellerNameVal = new Chunk(lblTravellerName.Text, ddlFonts);
            Phrase TravellerNameValPh2 = new Phrase(TravellerNameVal);
            PdfPCell TravellerNameValcell2 = new PdfPCell(TravellerNameValPh2);
            TravellerNameValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(TravellerNameValcell2);

            TravellerName = new Chunk("Traveller Email Address: ", ddlLabelFonts);
            TravellerNameValPh1 = new Phrase(TravellerName);
            TravellerNamevalcell = new PdfPCell(TravellerNameValPh1);
            TravellerNamevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(TravellerNamevalcell);

            TravellerNameVal = new Chunk(lblTravellerEmailID.Text, ddlFonts);
            TravellerNameValPh2 = new Phrase(TravellerNameVal);
            TravellerNameValcell2 = new PdfPCell(TravellerNameValPh2);
            TravellerNameValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(TravellerNameValcell2);

            Chunk Designation = new Chunk("Designation: ", ddlLabelFonts);
            Phrase DesignationValPh1 = new Phrase(Designation);
            PdfPCell Designationvalcell = new PdfPCell(DesignationValPh1);
            Designationvalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(Designationvalcell);

            Chunk DesignationVal = new Chunk(lblPositionTitle.Text, ddlFonts);
            Phrase DesignationValPh2 = new Phrase(DesignationVal);
            PdfPCell DesignationValcell2 = new PdfPCell(DesignationValPh2);
            DesignationValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(DesignationValcell2);

            Chunk PositionTitle = new Chunk("Position Title: ", ddlLabelFonts);
            Phrase PositionTitleValPh1 = new Phrase(PositionTitle);
            PdfPCell PositionTitlevalcell = new PdfPCell(PositionTitleValPh1);
            PositionTitlevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(PositionTitlevalcell);

            Chunk PositionTitleVal = new Chunk(lblIfOther.Text, ddlFonts);
            Phrase PositionTitleValPh2 = new Phrase(PositionTitleVal);
            PdfPCell PositionTitleValcell2 = new PdfPCell(PositionTitleValPh2);
            PositionTitleValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(PositionTitleValcell2);

            Chunk BusinessUnit = new Chunk("Business Unit: ", ddlLabelFonts);
            Phrase BusinessUnitValPh1 = new Phrase(BusinessUnit);
            PdfPCell BusinessUnitvalcell = new PdfPCell(BusinessUnitValPh1);
            BusinessUnitvalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(BusinessUnitvalcell);

            Chunk BusinessUnitVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            Phrase BusinessUnitValPh2 = new Phrase(BusinessUnitVal);
            PdfPCell BusinessUnitValcell2 = new PdfPCell(BusinessUnitValPh2);
            BusinessUnitValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(BusinessUnitValcell2);

            Chunk CostCentre = new Chunk("Cost Centre: ", ddlLabelFonts);
            Phrase CostCentreValPh1 = new Phrase(CostCentre);
            PdfPCell CostCentrevalcell = new PdfPCell(CostCentreValPh1);
            CostCentrevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(CostCentrevalcell);

            Chunk CostCentreVal = new Chunk(lblCostCentre.Text, ddlFonts);
            Phrase CostCentreValPh2 = new Phrase(CostCentreVal);
            PdfPCell CostCentreValcell2 = new PdfPCell(CostCentreValPh2);
            CostCentreValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(CostCentreValcell2);

            Chunk ManagerName = new Chunk("Manager Name: ", ddlLabelFonts);
            Phrase ManagerNameValPh1 = new Phrase(ManagerName);
            PdfPCell ManagerNamevalcell = new PdfPCell(ManagerNameValPh1);
            ManagerNamevalcell.Border = 0;
            tblTravelSummaryLeft.AddCell(ManagerNamevalcell);

            Chunk ManagerNameVal = new Chunk(lblManagerName.Text, ddlFonts);
            Phrase ManagerNameValPh2 = new Phrase(ManagerNameVal);
            PdfPCell ManagerNameValcell2 = new PdfPCell(ManagerNameValPh2);
            ManagerNameValcell2.Border = 0;
            tblTravelSummaryLeft.AddCell(ManagerNameValcell2);

            PdfPCell leftCell = new PdfPCell(tblTravelSummaryLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            PdfPTable tblTravelSummaryRight = new PdfPTable(2);
            float[] tblTravelSummaryRightWidth = new float[] { 60f, 40f };
            tblTravelSummaryRight.SetWidths(tblTravelSummaryRightWidth);            

            // Departure and Return dates were removed as per request from business.
            /*Chunk DepartureDate = new Chunk("Departure Date: ", ddlLabelFonts);
            Phrase DepartureDateValPh1 = new Phrase(DepartureDate);
            PdfPCell DepartureDatevalcell = new PdfPCell(DepartureDateValPh1);
            DepartureDatevalcell.Border = 0;
            tblTravelSummaryRight.AddCell(DepartureDatevalcell);

            Chunk DepartureDateVal = new Chunk(lblDepartureDate.Text, ddlFonts);
            Phrase DepartureDateValPh2 = new Phrase(DepartureDateVal);
            PdfPCell DepartureDateValcell2 = new PdfPCell(DepartureDateValPh2);
            DepartureDateValcell2.Border = 0;
            tblTravelSummaryRight.AddCell(DepartureDateValcell2);

            Chunk ReturnDate = new Chunk("Return Date: ", ddlLabelFonts);
            Phrase ReturnDateValPh1 = new Phrase(ReturnDate);
            PdfPCell ReturnDatevalcell = new PdfPCell(ReturnDateValPh1);
            ReturnDatevalcell.Border = 0;
            tblTravelSummaryRight.AddCell(ReturnDatevalcell);

            Chunk ReturnDateVal = new Chunk(lblReturnDate.Text, ddlFonts);
            Phrase ReturnDateValPh2 = new Phrase(ReturnDateVal);
            PdfPCell ReturnDateValcell2 = new PdfPCell(ReturnDateValPh2);
            ReturnDateValcell2.Border = 0;
            tblTravelSummaryRight.AddCell(ReturnDateValcell2);*/

            Chunk Purposeoftravel = new Chunk("Purpose of travel: ", ddlLabelFonts);
            Phrase PurposeoftravelValPh1 = new Phrase(Purposeoftravel);
            PdfPCell Purposeoftravelvalcell = new PdfPCell(PurposeoftravelValPh1);
            Purposeoftravelvalcell.Border = 0;
            tblTravelSummaryRight.AddCell(Purposeoftravelvalcell);

            Chunk PurposeoftravelVal = new Chunk(lblPurposeoftravel.Text, ddlFonts);
            Phrase PurposeoftravelValPh2 = new Phrase(PurposeoftravelVal);
            PdfPCell PurposeoftravelValcell2 = new PdfPCell(PurposeoftravelValPh2);
            PurposeoftravelValcell2.Border = 0;
            tblTravelSummaryRight.AddCell(PurposeoftravelValcell2);

            Purposeoftravel = new Chunk("Notes to Travel Coordinator: ", ddlLabelFonts);
            PurposeoftravelValPh1 = new Phrase(Purposeoftravel);
            Purposeoftravelvalcell = new PdfPCell(PurposeoftravelValPh1);
            Purposeoftravelvalcell.Border = 0;
            tblTravelSummaryRight.AddCell(Purposeoftravelvalcell);

            PurposeoftravelVal = new Chunk(lblNotestoTC.Text, ddlFonts);
            PurposeoftravelValPh2 = new Phrase(PurposeoftravelVal);
            PurposeoftravelValcell2 = new PdfPCell(PurposeoftravelValPh2);
            PurposeoftravelValcell2.Border = 0;
            tblTravelSummaryRight.AddCell(PurposeoftravelValcell2);

            PdfPCell rightCell = new PdfPCell(tblTravelSummaryRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);
            pdfDoc.Add(headerTbl);

        }

        public class pdfPagePaymentHistory : iTextSharp.text.pdf.PdfPageEventHelper
        {


            public override void OnStartPage(PdfWriter writer, Document doc)
            {
                //I use a PdfPtable with 1 column to position my header where I want it
                PdfPTable headerTbl = new PdfPTable(3);
                //headerTbl.TotalWidth = 100f;
                //float[] widths = new float[] { 65f, 25f, 10f }; 
                //headerTbl.SetWidths(widths);
                //set the width of the table to be the same as the document
                headerTbl.TotalWidth = doc.PageSize.Width;
                string surl = SPContext.Current.Web.Url;

                /*SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(surl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPFile file = web.GetFile(web.Url + "/Style%20Library/HRWeb/Images/mainlogo.png");*/
                SPFile file = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR Web/Images/main-logo.png");
                byte[] imageBytes = file.OpenBinary();
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imageBytes);

                //logo.ScalePercent(75f, 50f);
                PdfPCell logocell = new PdfPCell(logo);
                logocell.HorizontalAlignment = Element.ALIGN_LEFT;
                logocell.PaddingLeft = 50;
                logocell.Border = 0;
                logocell.Colspan = 3;
                PdfPCell emptyCell = new PdfPCell();
                emptyCell.Border = 0;

                headerTbl.AddCell(logocell);

                iTextSharp.text.Font hFonts = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
                Chunk empchunk = new Chunk(" ", hFonts);
                Phrase empph = new Phrase(empchunk);
                PdfPCell empcell = new PdfPCell(empph);
                empcell.Border = 0;
                empcell.Colspan = 3;
                headerTbl.AddCell(empcell);


                Chunk chunk = new Chunk("Travel Request", hFonts);
                chunk.SetUnderline(0.5f, -1.5f);
                Phrase ph = new Phrase(chunk);
                PdfPCell cell1 = new PdfPCell(ph);
                cell1.Border = 0;
                cell1.Colspan = 3;
                cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                headerTbl.AddCell(cell1);
                //headerTbl.AddCell(cell);


                //headerTbl.AddCell(
                //write the rows out to the PDF output stream. I use the height of the document to position the table. Positioning seems quite strange in iTextSharp and caused me the biggest headache.. It almost seems like it starts from the bottom of the page and works up to the top, so you may ned to play around with this.
                headerTbl.WriteSelectedRows(0, -1, 0, (doc.PageSize.Height - 40), writer.DirectContent);
                /*}
            }
    
        });*/
            }

        }
    }
}
