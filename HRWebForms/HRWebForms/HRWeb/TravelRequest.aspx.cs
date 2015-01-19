using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;
using System;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HRWebForms.HRWeb
{
    public partial class TravelRequest : WebPartPage
    {

        string strRefno = string.Empty;

        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                ApplicationDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                if (ViewState["vsCTLegsGetNew"] != null)
                {
                    int cnt = Convert.ToInt16(ddlNoOfLegs.SelectedValue);
                    PopulateLegTableFromDataTable(cnt);
                }

                if (ViewState["vsCTLegs"] != null)
                {
                    accordion.InnerHtml = "";
                    int cnt = Convert.ToInt16(ddlNoOfLegs.SelectedValue);
                    PopulateLegTableFromDataTable(cnt);

                }

                if (ViewState["vsAccommodation"] != null)
                {
                    DataTable dtDatable = (DataTable)ViewState["vsAccommodation"];
                    PopulateNewRowFromDataTable(dtDatable);

                }
                else
                {
                    PopulateHeader();
                    AddAccommodationReqTable_New();
                }

                if (ViewState["vsVehicle"] != null)
                {
                    DataTable dtDatable = (DataTable)ViewState["vsVehicle"];
                    PopulateNewRowFromVehicleDataTable(dtDatable);

                }

                if (!IsPostBack)
                {

                    bool bValid = false;
                    if (Page.Request.QueryString["refno"] != null)
                    {
                        lblReferenceNo.Text = "Ref No: " + Page.Request.QueryString["refno"];
                        bValid = ValidateApplication();
                    }
                    else
                    {
                        bValid = true;

                    }
                    if (bValid)
                    {
                        PopulateTaxonomy();
                        bool bProceed = SetTravelSummaryList(true, "");
                        if (bProceed)
                        {
                            GetAllListData();
                        }
                    }
                    else
                    {
                        lblError.Text = "The application number passed does not exist or has already been submitted.";

                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.Page_Load", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private bool ValidateApplication()
        {
            bool bValid = false;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HRWebTravelSummary", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    if (Convert.ToString(listitem["Status"]) == "Draft")
                    {
                        bValid = true;
                        break;
                    }
                }
            }
            return bValid;
        }

        private void GetAllListData()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            GetTravelSummaryListdata(strRefno);
            if (ddlBookingReq.SelectedValue == "Combined Travel")
            {
                GetCombinedTravelItineraryListdata(strRefno);
                GetTravelLegData(strRefno);
            }
            else if (ddlBookingReq.SelectedValue == "Accommodation Only")
            {
                GetTravelAccommodationListData(strRefno);
            }
            else if (ddlBookingReq.SelectedValue == "Vehicle Only")
            {
                GetTravelVehicleListData(strRefno);
            }

        }

        private void PopulateTaxonomy()
        {

            TaxonomySession txnSession = new TaxonomySession(new SPSite(SPContext.Current.Site.RootWeb.Url));
            TermStore trmStore = null;
            try
            {
                trmStore = txnSession.TermStores["SunRice Managed Metadata"];
            }
            catch
            {
                trmStore = txnSession.TermStores["SunRice_Metadata_Service"];
            }
            Microsoft.SharePoint.Taxonomy.GroupCollection groups = trmStore.Groups;
            foreach (Microsoft.SharePoint.Taxonomy.Group termGroup in trmStore.Groups)
            {

                switch (termGroup.Name)
                {
                    case "HR Group":
                        ddlTravelBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);
                        ddlTravelBusinessUnit.DataTextField = "Term";
                        ddlTravelBusinessUnit.DataValueField = "Term";
                        ddlTravelBusinessUnit.DataBind();
                        break;

                    case "Organsiation Group":
                        /*ddlTravelBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                        ddlTravelBusinessUnit.DataTextField = "Term";
                        ddlTravelBusinessUnit.DataValueField = "Term";
                        ddlTravelBusinessUnit.DataBind();*/
                        break;

                }

            }


        }

        private DataTable AddTerms(string strTermset, Microsoft.SharePoint.Taxonomy.Group termGroup)
        {
            DataTable dtTermTable = new DataTable();
            dtTermTable.Columns.Add("Term");
            dtTermTable.Columns.Add("TermID");
            TermSet trmSet = termGroup.TermSets[strTermset];
            foreach (Term t in trmSet.Terms)
            {
                dtTermTable.Rows.Add(new string[] { t.Name, t.Id.ToString() });
            }
            return dtTermTable;
        }

        private DataTable AddSubTerms(string strTermset, Microsoft.SharePoint.Taxonomy.Group termGroup, string strSubTermSet)
        {
            DataTable dtTermTable = new DataTable();
            dtTermTable.Columns.Add("Term");
            dtTermTable.Columns.Add("TermID");
            TermSet trmSet = termGroup.TermSets[strTermset];
            Term trmSubSets = trmSet.Terms[strSubTermSet];

            foreach (Term trm in trmSubSets.Terms)
            {
                if (trm.TermsCount > 0)
                {
                    foreach (Term t in trm.Terms)
                    {
                        dtTermTable.Rows.Add(new string[] { t.Name, t.Id.ToString() });
                    }
                }
                else
                    dtTermTable.Rows.Add(new string[] { trm.Name, trm.Id.ToString() });

            }
            dtTermTable.DefaultView.Sort = "Term ASC";
            return dtTermTable.DefaultView.ToTable();
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

        private void GetTravelSummaryListdata(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            string MetadataField1 = "BusinessUnit";
            //string MetadataField2 = "CostCentre";

            SPWeb mySite = SPContext.Current.Web;
            SPListItemCollection collectionItems = GetListData("HRWebTravelSummary", strRefno);
            foreach (SPListItem ListItems in collectionItems)
            {

                if (Convert.ToString(ListItems["ApplicationDate"]) != "")
                    ApplicationDate.Text = Convert.ToString(ListItems["ApplicationDate"]);
                ddlTypeofTravel.SelectedValue = Convert.ToString(ListItems["TypeofTravel"]);
                ddlBookingReq.SelectedValue = Convert.ToString(ListItems["BookingRequirements"]);
                ddlVisaReq.SelectedValue = Convert.ToString(ListItems["VisaRequired"]);
                txtTravellerName.Text = Convert.ToString(ListItems["TravellerName"]);
                txtTravellerEmailID.Text = Convert.ToString(ListItems["TravellerEmailID"]);

                /*if (ListItems["TravellerName"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["TravellerName"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    TravellerNamePeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }*/

                ddlPositionTitle.SelectedValue = Convert.ToString(ListItems["PositionTitle"]);
                txtIfOthers.Text = Convert.ToString(ListItems["IfOthersPositionTitle"]);
                ddlTravelBusinessUnit.SelectedValue = Convert.ToString(ListItems[MetadataField1]);
                txtTravelCostCentre.Text = Convert.ToString(ListItems["CostCentre"]);

                //ddlTravelCostCentre.SelectedValue = Convert.ToString(ListItems[MetadataField2]);
                if (ListItems["ManagerName"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ManagerName"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    ManagerPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }

                if (Convert.ToString(ListItems["DepartureDate"]) != "")
                    DepartureDate.SelectedDate = Convert.ToDateTime(ListItems["DepartureDate"]);
                if (Convert.ToString(ListItems["ReturnDate"]) != "")
                    ReturnDate.SelectedDate = Convert.ToDateTime(ListItems["ReturnDate"]);
                txtPurposeofTravel.Text = Convert.ToString(ListItems["PurposeOfTravel"]);
                txtNotestoTC.Text = Convert.ToString(ListItems["NotestoTC"]);
            }
        }

        private void GetCombinedTravelItineraryListdata(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            SPWeb mySite = SPContext.Current.Web;
            SPListItemCollection collectionItems = GetListData("HRWebCombinedTravelItinerary", strRefno);
            foreach (SPListItem ListItems in collectionItems)
            {
                ddlNoOfLegs.SelectedValue = Convert.ToString(ListItems["NoOfLegs"]);
                if (Convert.ToString(ListItems["FlightTravelType"]) == "Flight")
                    chkboxFlight.Checked = true;
                if (Convert.ToString(ListItems["VehicleTravelType"]) == "Hire Vehicle")
                {
                    VehicleReqRadioButton.Items[2].Selected = true;
                    chkboxVehicle.Checked = true;
                }
                if (Convert.ToString(ListItems["VehicleTravelType"]) == "Company Vehicle")
                {
                    VehicleReqRadioButton.Items[1].Selected = true;
                    chkboxVehicle.Checked = true;
                }
                if (Convert.ToString(ListItems["VehicleTravelType"]) == "Personal Vehicle")
                {
                    VehicleReqRadioButton.Items[0].Selected = true;
                    chkboxVehicle.Checked = true;
                }
                if (Convert.ToString(ListItems["AccommodationTravelType"]) == "Accommodation")
                    CheckBoxAccommodation.Checked = true;
                if (Convert.ToString(ListItems["AccommodationTravelType"]) == "No Accommodation")
                    ChkboxAccomNotReq.Checked = true;
            }

        }

        private void GetTravelLegData(string strRefno)
        {
            if (strRefno != null)
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

                int cntLeg = Convert.ToInt16(ddlNoOfLegs.SelectedValue);
                for (int inx = 1; inx <= cntLeg; inx++)
                {
                    DateTime flightDepartureDate = new DateTime();
                    string DepartureLocation = string.Empty;
                    string FlightCarrier = string.Empty;
                    string FlightNo = string.Empty;
                    DateTime flightDepartureTime = new DateTime();
                    string DestinationLocation = string.Empty;

                    SPListItemCollection collectionItems = GetListData("HRWebTravelFlight", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {

                        foreach (SPListItem ListItems in collectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                flightDepartureDate = Convert.ToDateTime(ListItems["DepartureDate"]);
                                DepartureLocation = Convert.ToString(ListItems["DepartureLocation"]);
                                FlightCarrier = Convert.ToString(ListItems["FlightCarrier"]);
                                FlightNo = Convert.ToString(ListItems["FlightNo"]);
                                flightDepartureTime = Convert.ToDateTime(ListItems["FlightDepartureTime"]);
                                DestinationLocation = Convert.ToString(ListItems["TravelTo"]);
                            }

                        }
                    }

                    DateTime CheckInDate = new DateTime();
                    DateTime CheckOutDate = new DateTime();
                    string HotelName = string.Empty;
                    string NoOfNights = string.Empty;


                    SPListItemCollection AccomcollectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                    if (AccomcollectionItems != null && AccomcollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in AccomcollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]);
                                CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]);
                                HotelName = Convert.ToString(ListItems["HotelName"]);
                                NoOfNights = Convert.ToString(ListItems["NoOfNights"]);
                            }

                        }
                    }

                    DateTime PickUpDate = new DateTime();
                    DateTime PickUpTime = new DateTime();
                    string PULocation = string.Empty;
                    DateTime DropOffDate = new DateTime();
                    DateTime DropOffTime = new DateTime();
                    string DropOffLocation = string.Empty;

                    SPListItemCollection VehiclecollectionItems = GetListData("HRWebTravelVehicle", strRefno);
                    if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in VehiclecollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                PickUpDate = Convert.ToDateTime(ListItems["PickUpDate"]);
                                PickUpTime = Convert.ToDateTime(ListItems["PickUpTime"]);
                                PULocation = Convert.ToString(ListItems["PULocation"]);
                                DropOffDate = Convert.ToDateTime(ListItems["DropOffDate"]);
                                DropOffTime = Convert.ToDateTime(ListItems["DropOffTime"]);
                                DropOffLocation = Convert.ToString(ListItems["DropOffLocation"]);
                            }
                        }
                    }

                    AddCombinedTravelTable_GET(inx, flightDepartureDate, DepartureLocation, FlightCarrier, FlightNo, flightDepartureTime, DestinationLocation,
                                               CheckInDate, CheckOutDate, HotelName, NoOfNights, PickUpDate, PickUpTime, PULocation, DropOffDate, DropOffTime, DropOffLocation);

                }
                DataSet dsTravelLegGet = new DataSet();
                ViewState["vsCTLegs"] = dsTravelLegGet;
            }


        }

        private void GetTravelAccommodationListData(string strRefno)
        {
            if (strRefno != null)
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
                AccomRequirementsTable.Rows.Clear();
                PopulateHeader();

                DataTable dtAccommodation = new DataTable();
                dtAccommodation.Columns.Add("Check In");
                dtAccommodation.Columns.Add("Check Out");
                dtAccommodation.Columns.Add("Hotel Name");
                dtAccommodation.Columns.Add("Nights");

                SPListItemCollection collectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in collectionItems)
                    {
                        AddAccommodationReqTable(Convert.ToDateTime(ListItems["CheckIn"]), Convert.ToDateTime(ListItems["CheckOut"]), Convert.ToString(ListItems["HotelName"]), Convert.ToString(ListItems["NoOfNights"]));

                        DataRow dr = dtAccommodation.NewRow();
                        dr["Check In"] = Convert.ToDateTime(ListItems["CheckIn"]);
                        dr["Check Out"] = Convert.ToDateTime(ListItems["CheckOut"]);
                        dr["Hotel Name"] = Convert.ToString(ListItems["HotelName"]);
                        dr["Nights"] = Convert.ToString(ListItems["NoOfNights"]);

                        dtAccommodation.Rows.Add(dr);
                    }
                }
                AddAccommodationReqTable_New();

                ViewState["vsAccommodation"] = dtAccommodation;
                ViewState["vsAccomSaveFirst"] = dtAccommodation;

            }
        }

        private void GetTravelVehicleListData(string strRefno)
        {
            if (strRefno != null)
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
                VehicleTabel.Rows.Clear();
                PopulateVehicleHeader();

                DataTable dtVehicle = new DataTable();

                dtVehicle.Columns.Add("VLegNo");
                dtVehicle.Columns.Add("VPickupDate");
                dtVehicle.Columns.Add("VPickupTime");
                dtVehicle.Columns.Add("VPULocation");
                dtVehicle.Columns.Add("VDropoffDate");
                dtVehicle.Columns.Add("VDropoffTime");
                dtVehicle.Columns.Add("VDropoffLocation");

                dtVehicle.Rows.Add(new string[] { "", "", "", "", "", "", "" });

                SPListItemCollection collectionItems = GetListData("HRWebTravelVehicle", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in collectionItems)
                    {
                        ddlMotorVehicle.SelectedValue = Convert.ToString(ListItems["MotorVehicle"]);
                        AddVehicleTable(Convert.ToString(ListItems["LegNo"]), Convert.ToDateTime(ListItems["PickUpDate"]), Convert.ToDateTime(ListItems["PickUpTime"]),
                                        Convert.ToString(ListItems["PULocation"]), Convert.ToDateTime(ListItems["DropOffDate"]),
                                        Convert.ToDateTime(ListItems["DropOffTime"]), Convert.ToString(ListItems["DropOffLocation"]));

                        DataRow dr = dtVehicle.NewRow();
                        dr["VLegNo"] = Convert.ToString(ListItems["LegNo"]);
                        dr["VPickupDate"] = Convert.ToDateTime(ListItems["PickUpDate"]);
                        dr["VPickupTime"] = Convert.ToDateTime(ListItems["PickUpTime"]);
                        dr["VPULocation"] = Convert.ToString(ListItems["PULocation"]);
                        dr["VDropoffDate"] = Convert.ToDateTime(ListItems["DropOffDate"]);
                        dr["VDropoffTime"] = Convert.ToDateTime(ListItems["DropOffTime"]);
                        dr["VDropoffLocation"] = Convert.ToString(ListItems["DropOffLocation"]);

                        dtVehicle.Rows.Add(dr);
                    }
                }
                AddVehicleTable_New_Second();

                ViewState["vsVehicle"] = dtVehicle;
                ViewState["vsVehicleSaveFirst"] = dtVehicle;

            }
        }

        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            string lstURL = HrWebUtility.GetListUrl(SetListByName);
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
            SPListItemCollection collectionItems = oList.GetItems(oQuery);

            return collectionItems;
        }

        private bool SetTravelSummaryList(bool UpdateTitleOnly, string strStatus)
        {
            bool bProceed = true;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HRWebTravelSummary", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    if (!UpdateTitleOnly)
                        bProceed = UpdateTravelSummaryList(listitem, strStatus);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (strRefno == "")
                    {
                        SPWeb web = SPContext.Current.Web;
                        string lstURL = HrWebUtility.GetListUrl("HRWebTravelSummary");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();
                        web.AllowUnsafeUpdates = true;
                        listitem.Update();
                        lblReferenceNo.Text = "Ref No: TA" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                        strRefno = "TA" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                        listitem["Title"] = strRefno;
                        listitem.Update();
                        web.AllowUnsafeUpdates = false;
                    }
                });
            }
            return bProceed;
        }

        private void SetCombinedTravelItineraryList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = "Ref No: " + strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            }
            SPListItem item = null;
            SPListItemCollection collectionItems = null;
            string lstURL = HrWebUtility.GetListUrl("HRWebCombinedTravelItinerary");
            SPList List = SPContext.Current.Site.RootWeb.GetList(lstURL);
            if (strRefno != "")
                collectionItems = SetListData("HRWebCombinedTravelItinerary", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {

                item = collectionItems[0];
                StringBuilder deletebuilder = BatchCommand(List.ID.ToString(), collectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());

            }
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                UpdateCombinedTravelItineraryList(List, strRefno);
            });

            /*SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HRWebCombinedTravelItinerary", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateCombinedTravelItineraryList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    string lstURL = HrWebUtility.GetListUrl("HRWebCombinedTravelItinerary");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateCombinedTravelItineraryList(listitem);
                });
            }*/
        }

        private void SetTravelLeg(string strRefno)
        {

            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = "Ref No: " + strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            }

            SPListItem Fitem = null;
            SPListItemCollection FcollectionItems = null;
            string FlstURL = HrWebUtility.GetListUrl("HRWebTravelFlight");
            SPList FList = SPContext.Current.Site.RootWeb.GetList(FlstURL);
            if (strRefno != "")
                FcollectionItems = SetListData("HRWebTravelFlight", strRefno);
            if (FcollectionItems != null && FcollectionItems.Count > 0)
            {

                Fitem = FcollectionItems[0];
                StringBuilder deletebuilder = BatchCommand(FList.ID.ToString(), FcollectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());

            }

            SPListItem Aitem = null;
            SPListItemCollection AcollectionItems = null;
            string AlstURL = HrWebUtility.GetListUrl("HRWebTravelAccommodation");
            SPList AList = SPContext.Current.Site.RootWeb.GetList(AlstURL);
            if (strRefno != "")
                AcollectionItems = SetListData("HRWebTravelAccommodation", strRefno);
            if (AcollectionItems != null && AcollectionItems.Count > 0)
            {

                Aitem = AcollectionItems[0];
                StringBuilder deletebuilder = BatchCommand(AList.ID.ToString(), AcollectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());

            }

            SPListItem Vitem = null;
            SPListItemCollection VcollectionItems = null;
            string VlstURL = HrWebUtility.GetListUrl("HRWebTravelVehicle");
            SPList VList = SPContext.Current.Site.RootWeb.GetList(VlstURL);
            if (strRefno != "")
                VcollectionItems = SetListData("HRWebTravelVehicle", strRefno);
            if (VcollectionItems != null && VcollectionItems.Count > 0)
            {

                Vitem = VcollectionItems[0];
                StringBuilder deletebuilder = BatchCommand(VList.ID.ToString(), VcollectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());

            }

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                UpdateTravelLeg(strRefno);
            });


        }

        private void SetTravelAccommodationList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = "Ref No: " + strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            }
            SPListItem item = null;
            SPListItemCollection collectionItems = null;
            string lstURL = HrWebUtility.GetListUrl("HRWebTravelAccommodation");
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
            if (strRefno != "")
                collectionItems = SetListData("HRWebTravelAccommodation", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {

                item = collectionItems[0];

                StringBuilder deletebuilder = BatchCommand(oList.ID.ToString(), collectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    UpdateTravelAccommodationList(oList, strRefno);
                });

            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    UpdateTravelAccommodationList(oList, strRefno);
                });
            }
        }

        private void SetTravelVehicleList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = "Ref No: " + strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            }
            SPListItem item = null;
            SPListItemCollection collectionItems = null;
            string lstURL = HrWebUtility.GetListUrl("HRWebTravelVehicle");
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
            if (strRefno != "")
                collectionItems = SetListData("HRWebTravelVehicle", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {

                item = collectionItems[0];

                StringBuilder deletebuilder = BatchCommand(oList.ID.ToString(), collectionItems);
                SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    UpdateTravelVehicleList(oList, strRefno);
                });

            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    UpdateTravelVehicleList(oList, strRefno);
                });
            }
        }

        private bool UpdateTravelSummaryList(SPListItem listitem, string strStatus)
        {
            bool bProceed = true;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["Title"] = lblReferenceNo.Text.Split(':')[1].Trim();
                listitem["ApplicationDate"] = ApplicationDate.Text;
                listitem["TypeofTravel"] = ddlTypeofTravel.SelectedValue;
                listitem["BookingRequirements"] = ddlBookingReq.SelectedValue;
                listitem["VisaRequired"] = ddlVisaReq.SelectedValue;
                /*SPFieldUserValueCollection TravellerUserCollection = new SPFieldUserValueCollection();
                string[] TravellerUsersSeperated = TravellerNamePeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in TravellerUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        TravellerUserCollection.Add(UserName);
                    }
                }*/

                listitem["TravellerName"] = txtTravellerName.Text;
                listitem["TravellerEmailID"] = txtTravellerEmailID.Text;
                listitem["PositionTitle"] = ddlPositionTitle.Text;
                listitem["IfOthersPositionTitle"] = txtIfOthers.Text;
                listitem["IsUserSLT"] = rdoSLTYes.Checked ? true : false;
                listitem["BusinessUnit"] = ddlTravelBusinessUnit.SelectedItem.Text;
                listitem["CostCentre"] = txtTravelCostCentre.Text;
                SPFieldUserValueCollection ManagerUserCollection = new SPFieldUserValueCollection();
                string[] managerUsersSeperated = ManagerPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in managerUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ManagerUserCollection.Add(UserName);
                    }
                }
                listitem["ManagerName"] = ManagerUserCollection;

                if (!DepartureDate.IsDateEmpty)
                    listitem["DepartureDate"] = Convert.ToDateTime(DepartureDate.SelectedDate.ToString("dd/MM/yyyy"));
                if (!ReturnDate.IsDateEmpty)
                    listitem["ReturnDate"] = Convert.ToDateTime(ReturnDate.SelectedDate.ToString("dd/MM/yyyy"));
                listitem["PurposeOfTravel"] = txtPurposeofTravel.Text;
                listitem["NotestoTC"] = txtNotestoTC.Text;
                listitem["Status"] = strStatus;
                if (strStatus == "Pending Approval")
                {
                    if (ddlPositionTitle.SelectedValue == "Other")
                    {
                        listitem["PendingWith"] = "Manager";
                    }
                    else if (ddlPositionTitle.SelectedValue == "CEO" || ddlPositionTitle.SelectedValue == "Director")
                    {
                        listitem["PendingWith"] = "Chairman";
                    }
                    else if (ddlPositionTitle.SelectedValue == "Chairman")
                    {
                        listitem["PendingWith"] = "CEO";
                    }
                }

                /*if (strStatus == "Pending Approval")
                {
                    listitem["ApprovalStatus"] = GetApproverString(ddlPositionType.SelectedItem.Text);
                    if (Convert.ToString(ViewState["ApproverEmail"]) != "")
                    {
                        listitem["Status"] = strStatus;
                        bProceed = true;
                    }
                    else
                    {
                        bProceed = false;
                        listitem["Status"] = "Draft";
                        lblError.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }*/


                listitem.Update();
            });
            return bProceed;
        }

        private void UpdateCombinedTravelItineraryList(SPList ItineraryList, string strRefno)
        {

            SPListItem listitem = ItineraryList.Items.Add();
            listitem["Title"] = strRefno;
            listitem["NoOfLegs"] = ddlNoOfLegs.SelectedValue;


            if (chkboxFlight.Checked == true)
                listitem["FlightTravelType"] = "Flight";

            if (chkboxVehicle.Checked == true)
            {
                if (VehicleReqRadioButton.Items[2].Selected == true)
                {
                    listitem["VehicleTravelType"] = "Hire Vehicle";
                }
                if (VehicleReqRadioButton.Items[1].Selected == true)
                {
                    listitem["VehicleTravelType"] = "Company Vehicle";
                }
                if (VehicleReqRadioButton.Items[0].Selected == true)
                {
                    listitem["VehicleTravelType"] = "Personal Vehicle";
                }
            }

            if (CheckBoxAccommodation.Checked == true)
                listitem["AccommodationTravelType"] = "Accommodation";
            if (ChkboxAccomNotReq.Checked == true)
                listitem["AccommodationTravelType"] = "No Accommodation";

            listitem.Update();
        }

        private void UpdateTravelLeg(string strRefno)
        {
            int cntLeg = Convert.ToInt16(ddlNoOfLegs.SelectedValue);

            for (int inx = 1; inx <= cntLeg; inx++)
            {
                Table tblLeg = (Table)accordion.FindControl("tblLeg" + inx);

                if (chkboxFlight.Checked == true)
                {
                    Table tblFlight = (Table)tblLeg.FindControl("tblFlight" + inx);

                    for (int flCnt = 1; flCnt <= tblFlight.Rows.Count - 1; flCnt++)
                    {
                        DateTimeControl FlightDeparturedate = (DateTimeControl)tblFlight.Rows[flCnt].FindControl("FlightDepartureDate" + inx);
                        TextBox FlightDeparturelocation = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightDeptLocation" + inx);
                        TextBox FlightCarrier = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightCarrier" + inx);
                        TextBox FlightNumber = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightNumber" + inx);
                        DateTimeControl FlightDeparturetime = (DateTimeControl)tblFlight.Rows[flCnt].FindControl("FlightDepartureTime" + inx);
                        TextBox FlightDestLocation = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightDestLocation" + inx);

                        string lstURL = HrWebUtility.GetListUrl("HRWebTravelFlight");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();


                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            listitem["Title"] = strRefno;
                            listitem["LegNo"] = inx;
                            if (!FlightDeparturedate.IsDateEmpty)
                                listitem["DepartureDate"] = Convert.ToDateTime(FlightDeparturedate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["DepartureLocation"] = FlightDeparturelocation.Text;
                            listitem["FlightCarrier"] = FlightCarrier.Text;
                            listitem["FlightNo"] = FlightNumber.Text;
                            listitem["FlightDepartureTime"] = FlightDeparturetime.SelectedDate;
                            listitem["TravelTo"] = FlightDestLocation.Text;
                            listitem.Update();
                        });


                    }
                }

                if (CheckBoxAccommodation.Checked == true)
                {
                    Table tblAccommodation = (Table)tblLeg.FindControl("tblAccommodation" + inx);

                    for (int AcomCnt = 1; AcomCnt <= tblAccommodation.Rows.Count - 1; AcomCnt++)
                    {
                        DateTimeControl AccomCheckinDate = (DateTimeControl)tblAccommodation.Rows[AcomCnt].FindControl("CheckinCTDate" + inx);
                        TextBox AccomHotelName = (TextBox)tblAccommodation.Rows[AcomCnt].FindControl("txtCTHotelName" + inx);
                        System.Web.UI.WebControls.Label AccomNoofNights = (System.Web.UI.WebControls.Label)tblAccommodation.Rows[AcomCnt].FindControl("txtCTNoofNights" + inx);
                        DateTimeControl AccomCheckoutDate = (DateTimeControl)tblAccommodation.Rows[AcomCnt].FindControl("CheckoutCTDate" + inx);

                        string lstURL = HrWebUtility.GetListUrl("HRWebTravelAccommodation");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();


                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            listitem["Title"] = strRefno;
                            listitem["LegNo"] = inx;
                            if (!AccomCheckinDate.IsDateEmpty)
                                listitem["CheckIn"] = Convert.ToDateTime(AccomCheckinDate.SelectedDate.ToString("dd/MM/yyyy"));
                            if (!AccomCheckoutDate.IsDateEmpty)
                                listitem["CheckOut"] = Convert.ToDateTime(AccomCheckoutDate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["HotelName"] = AccomHotelName.Text;
                            listitem["NoOfNights"] = AccomNoofNights.Text;
                            listitem.Update();
                        });


                    }
                }


                if (VehicleReqRadioButton.Items[2].Selected == true)
                {
                    Table tblVehicle = (Table)tblLeg.FindControl("tblVehicle" + inx);

                    for (int VCnt = 1; VCnt <= tblVehicle.Rows.Count - 1; VCnt++)
                    {
                        DateTimeControl HCPickUpDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCDate" + inx);
                        DateTimeControl HCPickUpTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCTime" + inx);
                        TextBox HCPickUpLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCPickUpLocation" + inx);
                        DateTimeControl HCDropoffDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCDate" + inx);
                        DateTimeControl HCDropoffTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCTime" + inx);
                        TextBox HCReturnLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCReturnLocation" + inx);

                        string lstURL = HrWebUtility.GetListUrl("HRWebTravelVehicle");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();


                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            listitem["Title"] = strRefno;
                            listitem["LegNo"] = inx;
                            if (!HCPickUpDate.IsDateEmpty)
                                listitem["PickUpDate"] = Convert.ToDateTime(HCPickUpDate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["PickUpTime"] = HCPickUpTime.SelectedDate;
                            listitem["PULocation"] = HCPickUpLocation.Text;
                            if (!HCDropoffDate.IsDateEmpty)
                                listitem["DropOffDate"] = Convert.ToDateTime(HCDropoffDate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["DropOffTime"] = HCDropoffTime.SelectedDate;
                            listitem["DropOffLocation"] = HCReturnLocation.Text;
                            listitem.Update();
                        });


                    }
                }

                if (VehicleReqRadioButton.Items[1].Selected == true)
                {
                    Table tblVehicle = (Table)tblLeg.FindControl("tblVehicle" + inx);

                    for (int VCnt = 1; VCnt <= tblVehicle.Rows.Count - 1; VCnt++)
                    {
                        DateTimeControl HCPickUpDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCDate" + inx);
                        DateTimeControl HCPickUpTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCTime" + inx);
                        TextBox HCPickUpLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCPickUpLocation" + inx);
                        DateTimeControl HCDropoffDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCDate" + inx);
                        DateTimeControl HCDropoffTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCTime" + inx);
                        TextBox HCReturnLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCReturnLocation" + inx);

                        string lstURL = HrWebUtility.GetListUrl("HRWebTravelVehicle");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();


                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            listitem["Title"] = strRefno;
                            listitem["LegNo"] = inx;
                            if (!HCPickUpDate.IsDateEmpty)
                                listitem["PickUpDate"] = Convert.ToDateTime(HCPickUpDate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["PickUpTime"] = HCPickUpTime.SelectedDate;
                            listitem["PULocation"] = HCPickUpLocation.Text;
                            if (!HCDropoffDate.IsDateEmpty)
                                listitem["DropOffDate"] = Convert.ToDateTime(HCDropoffDate.SelectedDate.ToString("dd/MM/yyyy"));
                            listitem["DropOffTime"] = HCDropoffTime.SelectedDate;
                            listitem["DropOffLocation"] = HCReturnLocation.Text;
                            listitem.Update();
                        });


                    }
                }


            }
        }

        private void UpdateTravelAccommodationList(SPList AccommodationList, string strRefno)
        {
            DataTable dt = (DataTable)ViewState["vsAccommodation"];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int count = 0; count < dt.Rows.Count; count++)
                {
                    SPListItem item = AccommodationList.Items.Add();
                    item["Title"] = strRefno;
                    item["CheckIn"] = Convert.ToDateTime(dt.Rows[count]["Check In"]);
                    item["CheckOut"] = Convert.ToDateTime(dt.Rows[count]["Check Out"]);
                    item["HotelName"] = Convert.ToString(dt.Rows[count]["Hotel Name"]);
                    item["NoOfNights"] = Convert.ToString(dt.Rows[count]["Nights"]);
                    item.Update();
                }
            }
        }

        private void UpdateTravelVehicleList(SPList VehicleList, string strRefno)
        {
            DataTable dt = (DataTable)ViewState["vsVehicle"];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int count = 1; count < dt.Rows.Count; count++)
                {


                    SPListItem item = VehicleList.Items.Add();
                    item["Title"] = strRefno;
                    item["MotorVehicle"] = ddlMotorVehicle.SelectedValue;
                    item["LegNo"] = Convert.ToString(dt.Rows[count]["VLegNo"]);
                    item["PickUpDate"] = Convert.ToDateTime(dt.Rows[count]["VPickupDate"]);
                    item["PickUpTime"] = Convert.ToDateTime(dt.Rows[count]["VPickupTime"]);
                    item["PULocation"] = Convert.ToString(dt.Rows[count]["VPULocation"]);
                    item["DropOffDate"] = Convert.ToDateTime(dt.Rows[count]["VDropoffDate"]);
                    item["DropOffTime"] = Convert.ToDateTime(dt.Rows[count]["VDropoffTime"]);
                    item["DropOffLocation"] = Convert.ToString(dt.Rows[count]["VDropoffLocation"]);
                    item.Update();
                }
            }
        }

        private StringBuilder BatchCommand(string listid, SPListItemCollection collectionItems)
        {
            StringBuilder deletebuilder = new StringBuilder();
            deletebuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");
            string command = "<Method><SetList Scope=\"Request\">" + listid +
                "</SetList><SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";

            foreach (SPListItem item in collectionItems)
            {
                deletebuilder.Append(string.Format(command, item.ID.ToString()));
            }
            deletebuilder.Append("</Batch>");
            return deletebuilder;
        }

        protected void btnCombinedSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                bool bProceed = SetTravelSummaryList(false, "Draft");
                SetCombinedTravelItineraryList();
                SetTravelLeg(refno);
                Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Draft");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnCombinedSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnAccomSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                bool bProceed = SetTravelSummaryList(false, "Draft");
                SetTravelAccommodationList();
                Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Draft");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnAccomSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnVehicleSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                bool bProceed = SetTravelSummaryList(false, "Draft");
                SetTravelVehicleList();
                Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Draft");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnVehicleSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        #region CombinedTravel

        protected void GenerateLeg_Click(object sender, EventArgs e)
        {
            int drpcnt = Convert.ToInt32(ddlNoOfLegs.SelectedValue);

            if (ViewState["vsCTLegsGet"] != null)
            {
                accordion.InnerHtml = "";
                GenerateNewLegDataTable(drpcnt);
            }
            else
            {
                if (drpcnt > 0)
                {
                    if (ViewState["vsCTLegs"] != null)
                    {
                        accordion.InnerHtml = "";

                        for (int i = 1; i <= drpcnt; i++)
                        {

                            AddCombinedTravelTable_New(i);
                        }

                    }
                    else
                    {
                        for (int i = 1; i <= drpcnt; i++)
                        {
                            AddCombinedTravelTable_New(i);
                        }
                    }
                }
            }
        }

        private void GenerateNewLegDataTable(int drpcnt)
        {
            if (strRefno != null)
            {
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
                DataSet dsTravelLeg = new DataSet();

                for (int inx = 1; inx <= drpcnt; inx++)
                {


                    DateTime flightDepartureDate = new DateTime();
                    string DepartureLocation = string.Empty;
                    string FlightCarrier = string.Empty;
                    string FlightNo = string.Empty;
                    DateTime flightDepartureTime = new DateTime();
                    string DestinationLocation = string.Empty;

                    SPListItemCollection collectionItems = GetListData("HRWebTravelFlight", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in collectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                flightDepartureDate = Convert.ToDateTime(ListItems["DepartureDate"]);
                                DepartureLocation = Convert.ToString(ListItems["DepartureLocation"]);
                                FlightCarrier = Convert.ToString(ListItems["FlightCarrier"]);
                                FlightNo = Convert.ToString(ListItems["FlightNo"]);
                                flightDepartureTime = Convert.ToDateTime(ListItems["FlightDepartureTime"]);
                                DestinationLocation = Convert.ToString(ListItems["TravelTo"]);
                            }

                        }
                    }
                    DataTable dtCTFlight = new DataTable();
                    dtCTFlight.Columns.Add("FDepartureDate");
                    dtCTFlight.Columns.Add("FDepartureLocation");
                    dtCTFlight.Columns.Add("FCarrier");
                    dtCTFlight.Columns.Add("FCarrierNo");
                    dtCTFlight.Columns.Add("FDepartureTime");
                    dtCTFlight.Columns.Add("FDestinationLocation");

                    dtCTFlight.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTFlight);

                    DateTime CheckInDate = new DateTime();
                    string HotelName = string.Empty;
                    string NoOfNights = string.Empty;
                    DateTime CheckOutDate = new DateTime();

                    SPListItemCollection AccomcollectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                    if (AccomcollectionItems != null && AccomcollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in AccomcollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]);
                                HotelName = Convert.ToString(ListItems["HotelName"]);
                                NoOfNights = Convert.ToString(ListItems["NoOfNights"]);
                                CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]);
                            }

                        }

                    }

                    DataTable dtCTAccommodation = new DataTable();
                    dtCTAccommodation.Columns.Add("ACheckinDate");
                    dtCTAccommodation.Columns.Add("AHotelName");
                    dtCTAccommodation.Columns.Add("ANoOfNights");
                    dtCTAccommodation.Columns.Add("ACheckoutDate");

                    dtCTAccommodation.Rows.Add(new string[] { "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTAccommodation);

                    DateTime PickUpDate = new DateTime();
                    DateTime PickUpTime = new DateTime();
                    string PULocation = string.Empty;
                    DateTime DropOffDate = new DateTime();
                    DateTime DropOffTime = new DateTime();
                    string DropOffLocation = string.Empty;

                    SPListItemCollection VehiclecollectionItems = GetListData("HRWebTravelVehicle", strRefno);
                    if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
                    {
                        foreach (SPListItem ListItems in VehiclecollectionItems)
                        {
                            string LegCount = Convert.ToString(ListItems["LegNo"]);

                            if (LegCount == Convert.ToString(inx))
                            {
                                PickUpDate = Convert.ToDateTime(ListItems["PickUpDate"]);
                                PickUpTime = Convert.ToDateTime(ListItems["PickUpTime"]);
                                PULocation = Convert.ToString(ListItems["PULocation"]);
                                DropOffDate = Convert.ToDateTime(ListItems["DropOffDate"]);
                                DropOffTime = Convert.ToDateTime(ListItems["DropOffTime"]);
                                DropOffLocation = Convert.ToString(ListItems["DropOffLocation"]);
                            }
                        }
                    }
                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);




                    AddCombinedTravelTable_GET(inx, flightDepartureDate, DepartureLocation, FlightCarrier, FlightNo, flightDepartureTime, DestinationLocation,
                                               CheckInDate, CheckOutDate, HotelName, NoOfNights, PickUpDate, PickUpTime, PULocation, DropOffDate, DropOffTime, DropOffLocation);

                }

                ViewState["vsCTLegsGetNew"] = dsTravelLeg;

            }

        }

        private void PopulateLegTableFromDataTable(int cnt)
        {

            strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            for (int inx = 1; inx <= cnt; inx++)
            {

                DateTime CheckInDate = new DateTime();
                string HotelName = string.Empty;
                string NoOfNights = string.Empty;
                DateTime CheckOutDate = new DateTime();

                SPListItemCollection AccomcollectionItems = GetListData("HRWebTravelAccommodation", strRefno);
                if (AccomcollectionItems != null && AccomcollectionItems.Count > 0)
                {
                    foreach (SPListItem ListItems in AccomcollectionItems)
                    {
                        string LegCount = Convert.ToString(ListItems["LegNo"]);

                        if (LegCount == Convert.ToString(inx))
                        {
                            CheckInDate = Convert.ToDateTime(ListItems["CheckIn"]);
                            HotelName = Convert.ToString(ListItems["HotelName"]);
                            NoOfNights = Convert.ToString(ListItems["NoOfNights"]);
                            CheckOutDate = Convert.ToDateTime(ListItems["CheckOut"]);
                        }

                    }

                }


                AddCombinedTravelTable(inx, NoOfNights);

            }


        }

        private void AddCombinedTravelTable_GET(int cnt, DateTime flightDepartureDate, string DepartureLocation, string FlightCarrier,
                                               string FlightNo, DateTime flightDepartureTime, string DestinationLocation, DateTime CheckInDate, DateTime CheckOutDate, string HotelName, string NoOfNights,
                                               DateTime PickUpDate, DateTime PickUpTime, string PULocation, DateTime DropOffDate,
                                               DateTime DropOffTime, string DropOffLocation)
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

            DataSet dsTravelLeg = new DataSet();

            if (chkboxFlight.Checked)
            {
                Table tb2 = new Table();
                tb2.ID = "tblFlight" + cnt;
                tb2.CssClass = "EU_DataTable2";

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

                DateTimeControl FlightDepartureDate = new DateTimeControl();
                FlightDepartureDate.ID = "FlightDepartureDate" + cnt;
                FlightDepartureDate.SelectedDate = flightDepartureDate;
                FlightDepartureDate.LocaleId = 2057;
                FlightDepartureDate.DateOnly = true;
                FlightDepartureDate.UseTimeZoneAdjustment = false;


                TextBox txtFlightDeptLocation = new TextBox();
                txtFlightDeptLocation.ID = "txtFlightDeptLocation" + cnt;
                txtFlightDeptLocation.Text = DepartureLocation;
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("Width", "176px");

                TextBox txtFlightCarrier = new TextBox();
                txtFlightCarrier.ID = "txtFlightCarrier" + cnt;
                txtFlightCarrier.Text = FlightCarrier;
                txtFlightCarrier.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("Width", "120px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-right", "3px");

                TextBox txtFlightNumber = new TextBox();
                txtFlightNumber.ID = "txtFlightNumber" + cnt;
                txtFlightNumber.Text = FlightNo;
                txtFlightNumber.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("Width", "109px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-right", "3px");

                DateTimeControl FlightDepartureTime = new DateTimeControl();
                FlightDepartureTime.ID = "FlightDepartureTime" + cnt;
                FlightDepartureTime.SelectedDate = flightDepartureTime;
                FlightDepartureTime.LocaleId = 2057;
                FlightDepartureTime.TimeOnly = true;
                FlightDepartureTime.UseTimeZoneAdjustment = false;

                TextBox txtFlightDestLocation = new TextBox();
                txtFlightDestLocation.ID = "txtFlightDestLocation" + cnt;
                txtFlightDestLocation.Text = DestinationLocation;
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("Width", "176px");


                FrowNew.Controls.Add(FcellNew);
                FrowNew.Controls.Add(FcellNew2);
                FrowNew.Controls.Add(FcellNew3);
                FrowNew.Controls.Add(FcellNew4);
                FrowNew.Controls.Add(FcellNew5);
                FrowNew.Controls.Add(FcellNew6);
                FrowNew.Controls.Add(FcellNew7);

                FcellNew.Controls.Add(FlblNew);
                FcellNew2.Controls.Add(txtFlightDeptLocation);
                System.Web.UI.WebControls.Literal litMandatory = new Literal();
                litMandatory.Text = "<span style='color:red'>*</span>";
               
                FcellNew2.Controls.Add(litMandatory);              

                FcellNew3.Controls.Add(FlightDepartureDate);


               
                FcellNew4.Controls.Add(txtFlightDestLocation);
                System.Web.UI.WebControls.Literal litMandatory4 = new Literal();
                litMandatory4.Text = "<span style='color:red'>*</span>";
                FcellNew4.Controls.Add(litMandatory4);

                FcellNew5.Controls.Add(FlightDepartureTime);
                               

                FcellNew6.Controls.Add(txtFlightCarrier);
                FcellNew7.Controls.Add(txtFlightNumber);
                cellNew.Controls.Add(tb2);

                DataTable dtCTFlight = new DataTable();
                dtCTFlight.Columns.Add("FDepartureDate");
                dtCTFlight.Columns.Add("FDepartureLocation");
                dtCTFlight.Columns.Add("FCarrier");
                dtCTFlight.Columns.Add("FCarrierNo");
                dtCTFlight.Columns.Add("FDepartureTime");
                dtCTFlight.Columns.Add("FDestinationLocation");

                dtCTFlight.Rows.Add(new string[] { "", "", "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTFlight);

            }

            if (CheckBoxAccommodation.Checked)
            {
                Table tb3 = new Table();
                tb3.CssClass = "EU_DataTable2";
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

                DateTimeControl CTCheckinDate = new DateTimeControl();
                CTCheckinDate.ID = "CheckinCTDate" + cnt;
                CTCheckinDate.SelectedDate = CheckInDate;
                CTCheckinDate.LocaleId = 2057;
                CTCheckinDate.DateOnly = true;
                CTCheckinDate.UseTimeZoneAdjustment = false;

                DateTimeControl CTCheckoutDate = new DateTimeControl();
                CTCheckoutDate.ID = "CheckoutCTDate" + cnt;
                CTCheckoutDate.SelectedDate = CheckOutDate;
                CTCheckoutDate.LocaleId = 2057;
                CTCheckoutDate.DateOnly = true;
                CTCheckoutDate.UseTimeZoneAdjustment = false;
                CTCheckoutDate.AutoPostBack = true;
                CTCheckoutDate.DateChanged += CheckoutCTDate_DateChanged;


                TextBox txtCTHotelName = new TextBox();
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
                txtCTNoofNights.Attributes.CssStyle.Add("text-align", "center");


                ArowNew.Controls.Add(AcellNew);
                ArowNew.Controls.Add(AcellNew2);
                ArowNew.Controls.Add(AcellNew3);
                ArowNew.Controls.Add(AcellNew4);
                ArowNew.Controls.Add(AcellNew5);

                AcellNew.Controls.Add(AlblNew);
                AcellNew2.Controls.Add(CTCheckinDate);
                AcellNew3.Controls.Add(CTCheckoutDate);
                AcellNew4.Controls.Add(txtCTHotelName);
                AcellNew5.Controls.Add(txtCTNoofNights);

                cellNew.Controls.Add(tb3);

                DataTable dtCTAccommodation = new DataTable();
                dtCTAccommodation.Columns.Add("ACheckinDate");
                dtCTAccommodation.Columns.Add("AHotelName");
                dtCTAccommodation.Columns.Add("ANoOfNights");
                dtCTAccommodation.Columns.Add("ACheckoutDate");

                dtCTAccommodation.Rows.Add(new string[] { "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTAccommodation);

            }

            if (chkboxVehicle.Checked)
            {
                if (VehicleReqRadioButton.Items[2].Selected)
                {
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable2";

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

                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.SelectedDate = PickUpDate;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.SelectedDate = PickUpTime;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Text = PULocation;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.SelectedDate = DropOffDate;
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;

                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.SelectedDate = DropOffTime;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);

                    cellNew.Controls.Add(tb4);

                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);
                }
                if (VehicleReqRadioButton.Items[1].Selected)
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

                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.SelectedDate = PickUpDate;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.SelectedDate = PickUpTime;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Text = PULocation;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.SelectedDate = DropOffDate;
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.SelectedDate = DropOffTime;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);

                    cellNew.Controls.Add(tb4);

                    //DataTable dtCTVehicle = new DataTable();
                    //dtCTVehicle.Columns.Add("VHCPickUpDate");
                    //dtCTVehicle.Columns.Add("VHCPickUpTime");
                    //dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    //dtCTVehicle.Columns.Add("VHCDropoffDate");
                    //dtCTVehicle.Columns.Add("VHCDropoffTime");
                    //dtCTVehicle.Columns.Add("VHCReturnLocation");

                    //dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    //dsTravelLeg.Tables.Add(dtCTVehicle);
                }
            }
            //ViewState["vsCTLegsGet"] = dsTravelLeg;
            cellNew.Controls.Add(lblNew);
            rowNew.Controls.Add(cellNew);

            if (chkboxFlight.Checked || CheckBoxAccommodation.Checked || CheckBoxAccommodation.Checked)
                accordion.Controls.Add(tbl);
        }

        private void AddCombinedTravelTable(int cnt, string NoofNights)
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

            DataSet dsTravelLeg = new DataSet();

            if (chkboxFlight.Checked)
            {
                Table tb2 = new Table();
                tb2.ID = "tblFlight" + cnt;
                tb2.CssClass = "EU_DataTable2";

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

                DateTimeControl FlightDepartureDate = new DateTimeControl();
                FlightDepartureDate.ID = "FlightDepartureDate" + cnt;
                FlightDepartureDate.LocaleId = 2057;
                FlightDepartureDate.DateOnly = true;
                FlightDepartureDate.UseTimeZoneAdjustment = false;

                TextBox txtFlightDeptLocation = new TextBox();
                txtFlightDeptLocation.ID = "txtFlightDeptLocation" + cnt;
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("Width", "176px");

                TextBox txtFlightCarrier = new TextBox();
                txtFlightCarrier.ID = "txtFlightCarrier" + cnt;
                txtFlightCarrier.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("Width", "120px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-right", "3px");

                TextBox txtFlightNumber = new TextBox();
                txtFlightNumber.ID = "txtFlightNumber" + cnt;
                txtFlightNumber.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("Width", "109px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-right", "3px");

                DateTimeControl FlightDepartureTime = new DateTimeControl();
                FlightDepartureTime.ID = "FlightDepartureTime" + cnt;
                FlightDepartureTime.LocaleId = 2057;
                FlightDepartureTime.TimeOnly = true;
                FlightDepartureTime.UseTimeZoneAdjustment = false;

                TextBox txtFlightDestLocation = new TextBox();
                txtFlightDestLocation.ID = "txtFlightDestLocation" + cnt;
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("Width", "176px");


                FrowNew.Controls.Add(FcellNew);
                FrowNew.Controls.Add(FcellNew2);
                FrowNew.Controls.Add(FcellNew3);
                FrowNew.Controls.Add(FcellNew4);
                FrowNew.Controls.Add(FcellNew5);
                FrowNew.Controls.Add(FcellNew6);
                FrowNew.Controls.Add(FcellNew7);

                FcellNew.Controls.Add(FlblNew);
                FcellNew2.Controls.Add(txtFlightDeptLocation);
                System.Web.UI.WebControls.Literal litMandatory = new Literal();
                litMandatory.Text = "<span style='color:red'>*</span>";
               
                FcellNew2.Controls.Add(litMandatory);               

                FcellNew3.Controls.Add(FlightDepartureDate);
               
                FcellNew4.Controls.Add(txtFlightDestLocation);
                System.Web.UI.WebControls.Literal litMandatory4 = new Literal();
                litMandatory4.Text = "<span style='color:red'>*</span>";
                FcellNew4.Controls.Add(litMandatory4);

                FcellNew5.Controls.Add(FlightDepartureTime);
              

                FcellNew6.Controls.Add(txtFlightCarrier);
                FcellNew7.Controls.Add(txtFlightNumber);
                cellNew.Controls.Add(tb2);

                DataTable dtCTFlight = new DataTable();
                dtCTFlight.Columns.Add("FDepartureDate");
                dtCTFlight.Columns.Add("FDepartureLocation");
                dtCTFlight.Columns.Add("FCarrier");
                dtCTFlight.Columns.Add("FCarrierNo");
                dtCTFlight.Columns.Add("FDepartureTime");
                dtCTFlight.Columns.Add("FDestinationLocation");

                dtCTFlight.Rows.Add(new string[] { "", "", "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTFlight);

            }

            if (CheckBoxAccommodation.Checked)
            {
                Table tb3 = new Table();
                tb3.CssClass = "EU_DataTable2";
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
                AlblNew.Text = "Accomodation";
                AlblNew.Style.Add("font-weight", "bold");

                DateTimeControl CTCheckinDate = new DateTimeControl();
                CTCheckinDate.ID = "CheckinCTDate" + cnt;
                CTCheckinDate.LocaleId = 2057;
                CTCheckinDate.DateOnly = true;
                CTCheckinDate.UseTimeZoneAdjustment = false;

                DateTimeControl CTCheckoutDate = new DateTimeControl();
                CTCheckoutDate.ID = "CheckoutCTDate" + cnt;
                CTCheckoutDate.LocaleId = 2057;
                CTCheckoutDate.DateOnly = true;
                CTCheckoutDate.UseTimeZoneAdjustment = false;
                CTCheckoutDate.AutoPostBack = true;
                CTCheckoutDate.DateChanged += CheckoutCTDate_DateChanged;

                TextBox txtCTHotelName = new TextBox();
                txtCTHotelName.ID = "txtCTHotelName" + cnt;
                txtCTHotelName.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-bottom", "1px");

                System.Web.UI.WebControls.Label txtCTNoofNights = new System.Web.UI.WebControls.Label();
                txtCTNoofNights.ID = "txtCTNoofNights" + cnt;
                txtCTNoofNights.Text = NoofNights;
                txtCTNoofNights.CssClass = "span12";
                txtCTNoofNights.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtCTNoofNights.Attributes.CssStyle.Add("text-align", "center");

                ArowNew.Controls.Add(AcellNew);
                ArowNew.Controls.Add(AcellNew2);
                ArowNew.Controls.Add(AcellNew3);
                ArowNew.Controls.Add(AcellNew4);
                ArowNew.Controls.Add(AcellNew5);

                AcellNew.Controls.Add(AlblNew);
                AcellNew2.Controls.Add(CTCheckinDate);
                AcellNew3.Controls.Add(CTCheckoutDate);
                AcellNew4.Controls.Add(txtCTHotelName);
                AcellNew5.Controls.Add(txtCTNoofNights);

                cellNew.Controls.Add(tb3);

                DataTable dtCTAccommodation = new DataTable();
                dtCTAccommodation.Columns.Add("ACheckinDate");
                dtCTAccommodation.Columns.Add("AHotelName");
                dtCTAccommodation.Columns.Add("ANoOfNights");
                dtCTAccommodation.Columns.Add("ACheckoutDate");

                dtCTAccommodation.Rows.Add(new string[] { "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTAccommodation);

            }

            if (chkboxVehicle.Checked)
            {
                if (VehicleReqRadioButton.Items[2].Selected)
                {
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable2";

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

                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);


                    cellNew.Controls.Add(tb4);

                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);

                }
                if (VehicleReqRadioButton.Items[1].Selected)
                {
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable2";

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

                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);


                    cellNew.Controls.Add(tb4);

                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);
                }
            }
            //ViewState["vsCTLegs"] = dsTravelLeg;
            cellNew.Controls.Add(lblNew);
            rowNew.Controls.Add(cellNew);
            accordion.Controls.Add(tbl);
        }

        private void AddCombinedTravelTable_New(int cnt)
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
            DataSet dsTravelLeg = new DataSet();


            if (chkboxFlight.Checked)
            {
                //ViewState["vsCTLegs"] = "";
                Table tb2 = new Table();
                tb2.ID = "tblFlight" + cnt;
                tb2.CssClass = "EU_DataTable2";

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
                DateTimeControl FlightDepartureDate = new DateTimeControl();
                FlightDepartureDate.ID = "FlightDepartureDate" + cnt;
                FlightDepartureDate.LocaleId = 2057;
                FlightDepartureDate.DateOnly = true;
                FlightDepartureDate.UseTimeZoneAdjustment = false;


                TextBox txtFlightDeptLocation = new TextBox();
                txtFlightDeptLocation.ID = "txtFlightDeptLocation" + cnt;
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDeptLocation.Attributes.CssStyle.Add("Width", "176px");

                TextBox txtFlightCarrier = new TextBox();
                txtFlightCarrier.ID = "txtFlightCarrier" + cnt;
                txtFlightCarrier.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightCarrier.Attributes.CssStyle.Add("Width", "120px");
                txtFlightCarrier.Attributes.CssStyle.Add("margin-right", "3px");

                TextBox txtFlightNumber = new TextBox();
                txtFlightNumber.ID = "txtFlightNumber" + cnt;
                txtFlightNumber.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightNumber.Attributes.CssStyle.Add("Width", "109px");
                txtFlightNumber.Attributes.CssStyle.Add("margin-right", "3px");

                DateTimeControl FlightDepartureTime = new DateTimeControl();
                FlightDepartureTime.ID = "FlightDepartureTime" + cnt;
                FlightDepartureTime.LocaleId = 2057;
                FlightDepartureTime.TimeOnly = true;
                FlightDepartureTime.UseTimeZoneAdjustment = false;

                TextBox txtFlightDestLocation = new TextBox();
                txtFlightDestLocation.ID = "txtFlightDestLocation" + cnt;
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-left", "3px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-top", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtFlightDestLocation.Attributes.CssStyle.Add("Width", "176px");


                FrowNew.Controls.Add(FcellNew);
                FrowNew.Controls.Add(FcellNew2);
                FrowNew.Controls.Add(FcellNew3);
                FrowNew.Controls.Add(FcellNew4);
                FrowNew.Controls.Add(FcellNew5);
                FrowNew.Controls.Add(FcellNew6);
                FrowNew.Controls.Add(FcellNew7);

                FcellNew.Controls.Add(FlblNew);
                FcellNew2.Controls.Add(txtFlightDeptLocation);
                System.Web.UI.WebControls.Literal litMandatory = new Literal();
                litMandatory.Text = "<span style='color:red'>*</span>";
               
                FcellNew2.Controls.Add(litMandatory);              

                FcellNew3.Controls.Add(FlightDepartureDate);


                FcellNew4.Controls.Add(txtFlightDestLocation);
                System.Web.UI.WebControls.Literal litMandatory4 = new Literal();
                litMandatory4.Text = "<span style='color:red'>*</span>";
                FcellNew4.Controls.Add(litMandatory4);

                FcellNew5.Controls.Add(FlightDepartureTime);
               
                FcellNew6.Controls.Add(txtFlightCarrier);
                FcellNew7.Controls.Add(txtFlightNumber);
                cellNew.Controls.Add(tb2);


                DataTable dtCTFlight = new DataTable();
                dtCTFlight.Columns.Add("FDepartureDate");
                dtCTFlight.Columns.Add("FDepartureLocation");
                dtCTFlight.Columns.Add("FCarrier");
                dtCTFlight.Columns.Add("FCarrierNo");
                dtCTFlight.Columns.Add("FDepartureTime");
                dtCTFlight.Columns.Add("FDestinationLocation");

                dtCTFlight.Rows.Add(new string[] { "", "", "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTFlight);

            }

            if (CheckBoxAccommodation.Checked)
            {
                
                Table tb3 = new Table();
                tb3.CssClass = "EU_DataTable2";
                tb3.ID = "tblAccommodation" + cnt;

                PopulateCTAccommodationHeader(tb3);


                TableRow ArowNew = new TableRow();
                tb3.Controls.Add(ArowNew);


                TableCell AcellNew = new TableCell();
                TableCell AcellNew2 = new TableCell();
                TableCell AcellNew3 = new TableCell();
                TableCell AcellNew4 = new TableCell();
                TableCell AcellNew5 = new TableCell();
                TableCell AcellNew6 = new TableCell();

                System.Web.UI.WebControls.Label AlblNew = new System.Web.UI.WebControls.Label();
                AlblNew.Text = "Accommodation";
                AlblNew.Style.Add("font-weight", "bold");

                DateTimeControl CTCheckinDate = new DateTimeControl();
                CTCheckinDate.ID = "CheckinCTDate" + cnt;
                CTCheckinDate.SelectedDate.ToString("dd/MM/YYYY");
                CTCheckinDate.LocaleId = 2057;
                CTCheckinDate.DateOnly = true;
                CTCheckinDate.UseTimeZoneAdjustment = false;

                DateTimeControl CTCheckoutDate = new DateTimeControl();
                CTCheckoutDate.ID = "CheckoutCTDate" + cnt;
                CTCheckoutDate.SelectedDate.ToString("dd/MM/YYYY");
                CTCheckoutDate.LocaleId = 2057;
                CTCheckoutDate.DateOnly = true;
                CTCheckoutDate.UseTimeZoneAdjustment = false;
                CTCheckoutDate.AutoPostBack = true;
                CTCheckoutDate.DateChanged += CheckoutCTDate_DateChanged;

                TextBox txtCTHotelName = new TextBox();
                txtCTHotelName.ID = "txtCTHotelName" + cnt;
                txtCTHotelName.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTHotelName.Attributes.CssStyle.Add("margin-bottom", "1px");

                System.Web.UI.WebControls.Label txtCTNoofNights = new System.Web.UI.WebControls.Label();
                txtCTNoofNights.ID = "txtCTNoofNights" + cnt;
                txtCTNoofNights.Attributes.CssStyle.Add("margin-left", "6px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-top", "1px");
                txtCTNoofNights.Attributes.CssStyle.Add("margin-bottom", "1px");
                txtCTNoofNights.Attributes.CssStyle.Add("text-align", "center");

                ArowNew.Controls.Add(AcellNew);
                ArowNew.Controls.Add(AcellNew2);
                ArowNew.Controls.Add(AcellNew3);
                ArowNew.Controls.Add(AcellNew4);
                ArowNew.Controls.Add(AcellNew5);


                AcellNew.Controls.Add(AlblNew);
                AcellNew2.Controls.Add(CTCheckinDate);
                AcellNew3.Controls.Add(CTCheckoutDate);
                AcellNew4.Controls.Add(txtCTHotelName);
                AcellNew5.Controls.Add(txtCTNoofNights);



                cellNew.Controls.Add(tb3);

                DataTable dtCTAccommodation = new DataTable();
                dtCTAccommodation.Columns.Add("ACheckinDate");
                dtCTAccommodation.Columns.Add("AHotelName");
                dtCTAccommodation.Columns.Add("ANoOfNights");
                dtCTAccommodation.Columns.Add("ACheckoutDate");

                dtCTAccommodation.Rows.Add(new string[] { "", "", "", "" });
                dsTravelLeg.Tables.Add(dtCTAccommodation);
              

            }

            if (chkboxVehicle.Checked)
            {
                if (VehicleReqRadioButton.Items[2].Selected)
                {
                   
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable2";

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
                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.SelectedDate.ToString("dd/MM/YYYY");
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);


                    cellNew.Controls.Add(tb4);

                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);
                   
                }
                if (VehicleReqRadioButton.Items[1].Selected)
                {
                    
                    Table tb4 = new Table();
                    tb4.ID = "tblVehicle" + cnt;
                    tb4.CssClass = "EU_DataTable2";

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
                    DateTimeControl HCPickUpDate = new DateTimeControl();
                    HCPickUpDate.ID = "PickUpHCDate" + cnt;
                    HCPickUpDate.LocaleId = 2057;
                    HCPickUpDate.DateOnly = true;
                    HCPickUpDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCPickUpTime = new DateTimeControl();
                    HCPickUpTime.ID = "PickUpHCTime" + cnt;
                    HCPickUpTime.LocaleId = 2057;
                    HCPickUpTime.TimeOnly = true;
                    HCPickUpTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCPickUpLocation = new TextBox();
                    txtHCPickUpLocation.ID = "txtHCPickUpLocation" + cnt;
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-left", "3px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-top", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("margin-bottom", "1px");
                    txtHCPickUpLocation.Attributes.CssStyle.Add("padding-right", "3px");

                    DateTimeControl HCDropoffDate = new DateTimeControl();
                    HCDropoffDate.ID = "DropoffHCDate" + cnt;
                    HCDropoffDate.SelectedDate.ToString("dd/MM/YYYY");
                    HCDropoffDate.LocaleId = 2057;
                    HCDropoffDate.DateOnly = true;
                    HCDropoffDate.UseTimeZoneAdjustment = false;


                    DateTimeControl HCDropoffTime = new DateTimeControl();
                    HCDropoffTime.ID = "DropoffHCTime" + cnt;
                    HCDropoffTime.LocaleId = 2057;
                    HCDropoffTime.TimeOnly = true;
                    HCDropoffTime.UseTimeZoneAdjustment = false;


                    TextBox txtHCReturnLocation = new TextBox();
                    txtHCReturnLocation.ID = "txtHCReturnLocation" + cnt;
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
                    HcellNew2.Controls.Add(HCPickUpDate);
                    HcellNew3.Controls.Add(HCPickUpTime);
                    HcellNew4.Controls.Add(txtHCPickUpLocation);
                    HcellNew5.Controls.Add(HCDropoffDate);
                    HcellNew6.Controls.Add(HCDropoffTime);
                    HcellNew7.Controls.Add(txtHCReturnLocation);


                    cellNew.Controls.Add(tb4);

                    DataTable dtCTVehicle = new DataTable();
                    dtCTVehicle.Columns.Add("VHCPickUpDate");
                    dtCTVehicle.Columns.Add("VHCPickUpTime");
                    dtCTVehicle.Columns.Add("VHCPickUpLocation");
                    dtCTVehicle.Columns.Add("VHCDropoffDate");
                    dtCTVehicle.Columns.Add("VHCDropoffTime");
                    dtCTVehicle.Columns.Add("VHCReturnLocation");

                    dtCTVehicle.Rows.Add(new string[] { "", "", "", "", "", "" });
                    dsTravelLeg.Tables.Add(dtCTVehicle);
                }

            }

            ViewState["vsCTLegs"] = dsTravelLeg;

            cellNew.Controls.Add(lblNew);
            rowNew.Controls.Add(cellNew);

            if (chkboxFlight.Checked || CheckBoxAccommodation.Checked || CheckBoxAccommodation.Checked)
                accordion.Controls.Add(tbl);

            //DataTable dtLeg = new DataTable();
            //dtLeg.Columns.Add("CTLegNo");
            //dtLeg.Rows.Add(new string[]  {""});
            //ViewState["vsCTLegNo"] = dtLeg;

        }

        void CheckoutCTDate_DateChanged(object sender, EventArgs e)
        {

            DateTimeControl dtCheckoutCntrol = (DateTimeControl)sender;
            string buttonid = dtCheckoutCntrol.ID.ToString();
            buttonid = buttonid.ToLower().Replace("checkoutctdate", "");
            int rowid;
            int.TryParse(buttonid, out rowid);
            Table CTtb3 = (Table)accordion.FindControl("tblAccommodation" + rowid);
            DateTimeControl dtCheckinCntrol = (DateTimeControl)CTtb3.Rows[1].FindControl("CheckinCTDate" + rowid);

            DateTime Checkin = dtCheckinCntrol.SelectedDate.Date;
            DateTime Checkout = dtCheckoutCntrol.SelectedDate.Date;
            TimeSpan t = Checkout - Checkin;

            System.Web.UI.WebControls.Label txtCTNoofNights = (System.Web.UI.WebControls.Label)CTtb3.Rows[1].FindControl("txtCTNoofNights" + rowid);
            txtCTNoofNights.Text = Convert.ToString(t.TotalDays);

        }

        private void PopulateCTFlightHeader(Table tb2)
        {
            TableHeaderRow Flightheader = new TableHeaderRow();
            Flightheader.Style.Add("width", "100%");
            TableHeaderCell FlightheaderTableCell = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell5 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell6 = new TableHeaderCell();
            TableHeaderCell FlightheaderTableCell7 = new TableHeaderCell();

            FlightheaderTableCell.Style.Add("width", "8%");
            FlightheaderTableCell2.Style.Add("width", "18%");
            FlightheaderTableCell3.Style.Add("width", "16%");
            FlightheaderTableCell4.Style.Add("width", "18%");
            FlightheaderTableCell5.Style.Add("width", "11%");
            FlightheaderTableCell6.Style.Add("width", "12%");
            FlightheaderTableCell7.Style.Add("width", "11%");

            FlightheaderTableCell.Text = "";
            FlightheaderTableCell2.Text = "Travel From";
            FlightheaderTableCell3.Text = "Departure Date";
            //FlightheaderTableCell4.Text = "Departure Time";
            //FlightheaderTableCell5.Text = "Travel To";
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
            HireCarheaderTableCell3.Style.Add("width", "11%");
            HireCarheaderTableCell4.Style.Add("width", "16%");
            HireCarheaderTableCell5.Style.Add("width", "15%");
            HireCarheaderTableCell6.Style.Add("width", "11%");
            HireCarheaderTableCell7.Style.Add("width", "16%");


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



        #endregion

        #region Accommodation

        private void AddAccommodationReqTable(DateTime checkin, DateTime checkout, string hotelname, string noofnights)
        {
            string cnt = AccomRequirementsTable.Rows.Count.ToString();


            TableRow AccomrowNew = new TableRow();


            TableCell AcellNew = new TableCell();
            TableCell AcellNew2 = new TableCell();
            TableCell AcellNew3 = new TableCell();
            TableCell AcellNew4 = new TableCell();
            TableCell AcellNew5 = new TableCell();


            AcellNew.Style.Add("width", "12%");
            AcellNew2.Style.Add("width", "12%");
            AcellNew3.Style.Add("width", "18%");
            AcellNew4.Style.Add("width", "12%");
            AcellNew5.Style.Add("width", "12%");

            DateTimeControl CheckinDate = new DateTimeControl();
            CheckinDate.ID = "CheckinDate" + cnt;
            CheckinDate.SelectedDate = checkin;
            CheckinDate.LocaleId = 2057;
            CheckinDate.DateOnly = true;
            CheckinDate.UseTimeZoneAdjustment = false;

            DateTimeControl CheckoutDate = new DateTimeControl();
            CheckoutDate.ID = "CheckoutDate" + cnt;
            CheckoutDate.SelectedDate = checkout;
            CheckoutDate.LocaleId = 2057;
            CheckoutDate.DateOnly = true;
            CheckoutDate.UseTimeZoneAdjustment = false;
            CheckoutDate.AutoPostBack = true;
            CheckoutDate.DateChanged += CheckoutDate_DateChanged;

            TextBox txtHotelName = new TextBox();
            txtHotelName.ID = "txtHotelName" + cnt;
            txtHotelName.Attributes.Add("runat", "server");
            txtHotelName.Attributes.CssStyle.Add("margin-left", "3px");
            txtHotelName.Attributes.CssStyle.Add("margin-top", "1px");
            txtHotelName.Text = hotelname;


            System.Web.UI.WebControls.Label lblNoOfNights = new System.Web.UI.WebControls.Label();
            lblNoOfNights.ID = "lblNoOfNights" + cnt;
            lblNoOfNights.Attributes.Add("runat", "server");
            TimeSpan t = checkout - checkin;
            lblNoOfNights.Text = Convert.ToString(t.TotalDays);
            lblNoOfNights.CssClass = "span12";
            lblNoOfNights.Attributes.CssStyle.Add("text-align", "center");

            ImageButton imgbtnEditNewRowInsersion = new ImageButton();
            imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
            imgbtnEditNewRowInsersion.ID = "imgbtnEditNewRowInsersion" + cnt;
            imgbtnEditNewRowInsersion.Click += imgbtnEditNewRowInsersion_Click;
            imgbtnEditNewRowInsersion.ToolTip = "Add new row";
            imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "50px");



            ImageButton imgbtnDeleteRow = new ImageButton();
            imgbtnDeleteRow.Attributes.Add("runat", "server");
            imgbtnDeleteRow.ID = "imgbtnDeleteRow" + cnt;
            imgbtnDeleteRow.Click += imgbtnDeleteRow_Click;
            imgbtnDeleteRow.ToolTip = "Delete row";
            imgbtnDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
            imgbtnDeleteRow.Attributes.CssStyle.Add("padding-left", "15px");


            AcellNew.Controls.Add(CheckinDate);
            AcellNew2.Controls.Add(CheckoutDate);
            AcellNew3.Controls.Add(txtHotelName);

            System.Web.UI.WebControls.Literal litMandatory = new Literal();
            litMandatory.Text = "<span style='color:red'>*</span>";
            //litMandatory.ID = "litmand1" + cnt;
            AcellNew3.Controls.Add(litMandatory);
           // AcellNew3.Attributes.Add("style", "min-width:250px");

            AcellNew4.Controls.Add(lblNoOfNights);
            AcellNew5.Controls.Add(imgbtnEditNewRowInsersion);
            AcellNew5.Controls.Add(imgbtnDeleteRow);

            AccomrowNew.Cells.Add(AcellNew);
            AccomrowNew.Cells.Add(AcellNew2);
            AccomrowNew.Cells.Add(AcellNew3);
            AccomrowNew.Cells.Add(AcellNew4);
            AccomrowNew.Cells.Add(AcellNew5);

            AccomRequirementsTable.Rows.Add(AccomrowNew);




        }

        private void imgbtnEditNewRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton SaveButton = (ImageButton)sender;
                string buttonid = SaveButton.ID.ToString();
                buttonid = buttonid.ToLower().Replace("imgbtneditnewrowinsersion", "");
                //buttonid = buttonid.Remove(0, 24);
                int rowid;
                int.TryParse(buttonid, out rowid);

                bool Accomvalid = ValidateAccomDetails(rowid);
                if (Accomvalid)
                {
                    DataTable dtAccommodation = new DataTable();
                    if (ViewState["vsAccommodation"] != null)
                    {
                        dtAccommodation = (DataTable)ViewState["vsAccommodation"];
                        TableRow tr = AccomRequirementsTable.Rows[rowid];
                        DateTimeControl CheckinDate = (DateTimeControl)tr.FindControl("CheckinDate" + rowid);
                        DateTimeControl CheckoutDate = (DateTimeControl)tr.FindControl("CheckoutDate" + rowid);

                        TextBox txtHotelName = (TextBox)tr.FindControl("txtHotelName" + rowid);

                        System.Web.UI.WebControls.Label lblNoOfNights = (System.Web.UI.WebControls.Label)tr.FindControl("lblNoOfNights" + rowid);

                        DateTime Checkin = new DateTime();
                        DateTime Checkout = new DateTime();
                        rowid = rowid - 1;
                        if ((CheckinDate) != null)
                        {
                            Checkin = CheckinDate.SelectedDate.Date;
                            dtAccommodation.Rows[rowid]["Check In"] = CheckinDate.SelectedDate.Date;
                        }
                        if ((CheckoutDate) != null)
                        {
                            Checkout = CheckoutDate.SelectedDate.Date;

                            TimeSpan t = Checkout - Checkin;
                            double NoOfDays = t.TotalDays;
                            lblNoOfNights.Text = Convert.ToString(NoOfDays);
                            dtAccommodation.Rows[rowid]["Check Out"] = CheckoutDate.SelectedDate.Date;
                            dtAccommodation.Rows[rowid]["Nights"] = lblNoOfNights.Text;
                        }




                        if (txtHotelName != null)
                            dtAccommodation.Rows[rowid]["Hotel Name"] = txtHotelName.Text;

                    }
                    PopulateNewRowFromDataTable(dtAccommodation);
                }
                else
                {
                    lblError.Text = "Please fill all mandatory fields in Accommodation Details Section.";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.imgbtnEditNewRowInsersion_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void imgbtnDeleteRow_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton RemoveButton = (ImageButton)sender;
            string buttonid = RemoveButton.ID.ToString();
            buttonid = buttonid.ToLower().Replace("imgbtndeleterow", "");
            //buttonid = buttonid.Remove(0, 15);
            int rowid;
            int.TryParse(buttonid, out rowid);
            rowid = rowid - 1;
            DataTable currentDataTable = (DataTable)ViewState["vsAccommodation"];
            int itemscount = currentDataTable.Rows.Count;
            currentDataTable.Rows[rowid].Delete();
            PopulateNewRowFromDataTable(currentDataTable);


        }

        private void imgbtnNewRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton AddButton = (ImageButton)sender;
                string buttonid = AddButton.ID.ToString();
                buttonid = buttonid.ToLower().Replace("imgbtnnewrowinsersion", "");
                //buttonid = buttonid.Remove(0, 24);
                int rowid;
                int.TryParse(buttonid, out rowid);

                bool Accomvalid = ValidateAccomDetails(rowid);
                if (Accomvalid)
                {
                    DataTable dtAccommodation = new DataTable();
                    if (ViewState["vsAccommodation"] == null)
                    {
                        dtAccommodation.Columns.Add("Check In");
                        dtAccommodation.Columns.Add("Check Out");
                        dtAccommodation.Columns.Add("Hotel Name");
                        dtAccommodation.Columns.Add("Nights");



                        TableRow tr = AccomRequirementsTable.Rows[rowid];
                        DateTimeControl CheckinDate = (DateTimeControl)tr.FindControl("CheckinDate" + rowid);
                        DateTimeControl CheckoutDate = (DateTimeControl)tr.FindControl("CheckoutDate" + rowid);

                        TextBox txtHotelName = (TextBox)tr.FindControl("txtHotelName" + rowid);

                        System.Web.UI.WebControls.Label lblNoOfNights = (System.Web.UI.WebControls.Label)tr.FindControl("lblNoOfNights" + rowid);
                        DateTime Checkin = CheckinDate.SelectedDate.Date;
                        DateTime Checkout = CheckoutDate.SelectedDate.Date;
                        TimeSpan t = Checkout - Checkin;
                        double NoOfDays = t.TotalDays;
                        lblNoOfNights.Text = Convert.ToString(NoOfDays);


                        DataRow dr = dtAccommodation.NewRow();
                        dr["Check In"] = CheckinDate.SelectedDate.Date;
                        dr["Check Out"] = CheckoutDate.SelectedDate.Date;
                        dr["Hotel Name"] = txtHotelName.Text;
                        dr["Nights"] = lblNoOfNights.Text;

                        dtAccommodation.Rows.Add(dr);

                        ViewState["vsAccomSaveFirst"] = dtAccommodation;

                    }
                    else
                    {
                        dtAccommodation = (DataTable)ViewState["vsAccommodation"];
                        TableRow tr = AccomRequirementsTable.Rows[rowid];
                        DateTimeControl CheckinDate = (DateTimeControl)tr.FindControl("CheckinDate" + rowid);
                        DateTimeControl CheckoutDate = (DateTimeControl)tr.FindControl("CheckoutDate" + rowid);

                        TextBox txtHotelName = (TextBox)tr.FindControl("txtHotelName" + rowid);

                        System.Web.UI.WebControls.Label lblNoOfNights = (System.Web.UI.WebControls.Label)tr.FindControl("lblNoOfNights" + rowid);

                        DateTime Checkin = new DateTime();
                        DateTime Checkout = new DateTime();
                        if ((CheckinDate.SelectedDate) != null)
                        {
                            Checkin = CheckinDate.SelectedDate.Date;
                        }
                        if ((CheckoutDate.SelectedDate) != null)
                        {
                            Checkout = CheckoutDate.SelectedDate.Date;
                        }
                        TimeSpan t = Checkout - Checkin;
                        double NoOfDays = t.TotalDays;
                        lblNoOfNights.Text = Convert.ToString(NoOfDays);


                        DataRow dr = dtAccommodation.NewRow();
                        dr["Check In"] = CheckinDate.SelectedDate.Date;
                        dr["Check Out"] = CheckoutDate.SelectedDate.Date;
                        dr["Hotel Name"] = txtHotelName.Text;
                        dr["Nights"] = lblNoOfNights.Text;

                        dtAccommodation.Rows.Add(dr);

                    }

                    PopulateNewRowFromDataTable(dtAccommodation);
                }

                else
                {
                    lblError.Text = "Please fill all mandatory fields in Accommodation Details Section.";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.imgbtnNewRowInsersion_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }



        }

        private bool ValidateAccomDetails(int rowid)
        {
            bool Accomvalid = true;

            TableRow tr = AccomRequirementsTable.Rows[rowid];
            DateTimeControl CheckinDate = (DateTimeControl)tr.FindControl("CheckinDate" + rowid);
            DateTimeControl CheckoutDate = (DateTimeControl)tr.FindControl("CheckoutDate" + rowid);
            TextBox txtHotelName = (TextBox)tr.FindControl("txtHotelName" + rowid);

            if (CheckinDate.IsDateEmpty)
                Accomvalid = false;
            else if (CheckoutDate.IsDateEmpty)
                Accomvalid = false;
            else if (txtHotelName.Text.Trim() == "")
                Accomvalid = false;

            return Accomvalid;
        }

        private void PopulateNewRowFromDataTable(DataTable dtAccommodation)
        {

            AccomRequirementsTable.Rows.Clear();
            PopulateHeader();
            if (dtAccommodation != null)
            {
                for (int i = 0; i < dtAccommodation.Rows.Count; i++)
                {

                    string strCheckin = Convert.ToString(dtAccommodation.Rows[i]["Check In"]);
                    string strCheckout = Convert.ToString(dtAccommodation.Rows[i]["Check Out"]);

                    DateTime checkin = new DateTime();
                    DateTime checkout = new DateTime();

                    if (!string.IsNullOrEmpty(strCheckin))
                        checkin = Convert.ToDateTime(dtAccommodation.Rows[i]["Check In"]);

                    if (!string.IsNullOrEmpty(strCheckout))
                        checkout = Convert.ToDateTime(dtAccommodation.Rows[i]["Check Out"]);

                    string hotelname = Convert.ToString(dtAccommodation.Rows[i]["Hotel Name"]);
                    string noofnights = Convert.ToString(dtAccommodation.Rows[i]["Nights"]);


                    AddAccommodationReqTable(checkin, checkout, hotelname, noofnights);
                }
            }

            AddAccommodationReqTable_New();

            ViewState["vsAccommodation"] = dtAccommodation;

        }

        private void AddAccommodationReqTable_New()
        {
            string cnt = Convert.ToString(AccomRequirementsTable.Rows.Count);

            TableRow AccomrowNew = new TableRow();

            TableCell AcellNew = new TableCell();
            TableCell AcellNew2 = new TableCell();
            TableCell AcellNew3 = new TableCell();
            TableCell AcellNew4 = new TableCell();
            TableCell AcellNew5 = new TableCell();


            AcellNew.Style.Add("width", "12%");
            AcellNew2.Style.Add("width", "12%");
            AcellNew3.Style.Add("width", "18%");
            AcellNew4.Style.Add("width", "12%");
            AcellNew5.Style.Add("width", "12%");

            DateTimeControl CheckinDate = new DateTimeControl();

            CheckinDate.ID = "CheckinDate" + cnt;
            CheckinDate.LocaleId = 2057;
            CheckinDate.DateOnly = true;
            CheckinDate.UseTimeZoneAdjustment = false;

            DateTimeControl CheckoutDate = new DateTimeControl();
            CheckoutDate.ID = "CheckoutDate" + cnt;
            CheckoutDate.LocaleId = 2057;
            CheckoutDate.DateOnly = true;
            CheckoutDate.UseTimeZoneAdjustment = false;
            CheckoutDate.AutoPostBack = true;
            CheckoutDate.DateChanged += CheckoutDate_DateChanged;

            TextBox txtHotelName = new TextBox();
            txtHotelName.ID = "txtHotelName" + cnt;
            txtHotelName.Attributes.Add("runat", "server");
            txtHotelName.Attributes.CssStyle.Add("margin-left", "3px");
            txtHotelName.Attributes.CssStyle.Add("margin-top", "1px");

            System.Web.UI.WebControls.Label lblNoOfNights = new System.Web.UI.WebControls.Label();
            lblNoOfNights.ID = "lblNoOfNights" + cnt;
            lblNoOfNights.Attributes.Add("runat", "server");
            lblNoOfNights.CssClass = "span12";
            lblNoOfNights.Text = "<br/>";
            lblNoOfNights.Attributes.CssStyle.Add("text-align", "center");

            ImageButton imgbtnNewRowInsersion = new ImageButton();
            imgbtnNewRowInsersion.Attributes.Add("runat", "server");
            imgbtnNewRowInsersion.ID = "imgbtnNewRowInsersion" + cnt;
            imgbtnNewRowInsersion.Click += imgbtnNewRowInsersion_Click;
            imgbtnNewRowInsersion.ToolTip = "Add new row";
            imgbtnNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnNewRowInsersion.Attributes.CssStyle.Add("padding-left", "50px");

            AcellNew.Controls.Add(CheckinDate);

            AcellNew2.Controls.Add(CheckoutDate);

            AcellNew3.Controls.Add(txtHotelName);

            System.Web.UI.WebControls.Literal litMandatory = new Literal();
            litMandatory.Text = "<span style='color:red'>*</span>";
            //litMandatory.ID = "litmand1" + cnt;
            AcellNew3.Controls.Add(litMandatory);
            //AcellNew3.Attributes.Add("style", "min-width:250px");

            AcellNew4.Controls.Add(lblNoOfNights);
            AcellNew5.Controls.Add(imgbtnNewRowInsersion);

            AccomrowNew.Cells.Add(AcellNew);
            AccomrowNew.Cells.Add(AcellNew2);
            AccomrowNew.Cells.Add(AcellNew3);
            AccomrowNew.Cells.Add(AcellNew4);
            AccomrowNew.Cells.Add(AcellNew5);

            AccomRequirementsTable.Rows.Add(AccomrowNew);

            /*DataTable dtAccommodation = new DataTable();
            dtAccommodation.Columns.Add("Check In");
            dtAccommodation.Columns.Add("Check Out");
            dtAccommodation.Columns.Add("Hotel Name");
            dtAccommodation.Columns.Add("Nights");
            dtAccommodation.Rows.Add(new string[] { "", "", "", "" });

            ViewState["vsAccommodation"] = dtAccommodation;*/

        }

        protected void CheckoutDate_DateChanged(object sender, EventArgs e)
        {

            DateTimeControl dtCheckoutCntrol = (DateTimeControl)sender;
            string buttonid = dtCheckoutCntrol.ID.ToString();
            buttonid = buttonid.ToLower().Replace("checkoutdate", "");
            int rowid;
            int.TryParse(buttonid, out rowid);
            DateTimeControl dtCheckinCntrol = (DateTimeControl)AccomRequirementsTable.Rows[rowid].FindControl("CheckinDate" + rowid);

            DateTime Checkin = dtCheckinCntrol.SelectedDate.Date;
            DateTime Checkout = dtCheckoutCntrol.SelectedDate.Date;
            TimeSpan t = Checkout - Checkin;

            System.Web.UI.WebControls.Label lblNoOfNights = (System.Web.UI.WebControls.Label)AccomRequirementsTable.Rows[rowid].FindControl("lblNoOfNights" + rowid);
            lblNoOfNights.Text = Convert.ToString(t.TotalDays);


        }

        private void PopulateHeader()
        {
            TableHeaderRow Accomheader = new TableHeaderRow();
            Accomheader.Style.Add("width", "72%");
            TableHeaderCell AccomheaderTableCell = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell2 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell3 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell4 = new TableHeaderCell();
            TableHeaderCell AccomheaderTableCell5 = new TableHeaderCell();

            AccomheaderTableCell.Style.Add("width", "12%");
            AccomheaderTableCell2.Style.Add("width", "12%");
            AccomheaderTableCell3.Style.Add("width", "18%");
            AccomheaderTableCell4.Style.Add("width", "12%");
            AccomheaderTableCell5.Style.Add("width", "12%");

            AccomheaderTableCell.Text = "Check In";
            AccomheaderTableCell2.Text = "Check Out";
            AccomheaderTableCell3.Text = "Hotel Name";
            AccomheaderTableCell4.Text = "Nights";
            AccomheaderTableCell5.Text = "";

            Accomheader.Cells.Add(AccomheaderTableCell);
            Accomheader.Cells.Add(AccomheaderTableCell2);
            Accomheader.Cells.Add(AccomheaderTableCell3);
            Accomheader.Cells.Add(AccomheaderTableCell4);
            Accomheader.Cells.Add(AccomheaderTableCell5);

            AccomRequirementsTable.Rows.Add(Accomheader);
        }

        #endregion

        #region Vehicle

        protected void ddlMotorVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ddlMotorVehicle.SelectedItem.Text == "Hire Vehicle")
                {
                    if (ViewState["vsVehicle"] != null)
                    {
                        DataTable dtDatable = (DataTable)ViewState["vsVehicle"];
                        PopulateNewRowFromVehicleDataTable(dtDatable);

                    }
                    else
                    {
                        PopulateVehicleHeader();
                        AddVehicleTable_New();
                    }

                }

                else if (ddlMotorVehicle.SelectedItem.Text == "Company Vehicle")
                {
                    if (ViewState["vsVehicle"] != null)
                    {
                        DataTable dtDatable = (DataTable)ViewState["vsVehicle"];
                        PopulateNewRowFromVehicleDataTable(dtDatable);

                    }
                    else
                    {
                        PopulateVehicleHeader();
                        AddVehicleTable_New();
                    }

                }
                else
                {
                    VehicleTabel.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.ddlMotorVehicle_SelectedIndexChanged", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }


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
            TableHeaderCell VehicleheaderTableCell8 = new TableHeaderCell();

            VehicleheaderTableCell.Style.Add("width", "8%");
            VehicleheaderTableCell2.Style.Add("width", "18%");
            VehicleheaderTableCell3.Style.Add("width", "10%");
            VehicleheaderTableCell4.Style.Add("width", "12%");
            VehicleheaderTableCell5.Style.Add("width", "18%");
            VehicleheaderTableCell6.Style.Add("width", "10%");
            VehicleheaderTableCell7.Style.Add("width", "12%");
            VehicleheaderTableCell8.Style.Add("width", "12%");

            VehicleheaderTableCell.Text = "Leg";
            VehicleheaderTableCell2.Text = "Pick Up Date";
            VehicleheaderTableCell3.Text = "Pick Up Time";
            VehicleheaderTableCell4.Text = "P/U Location";
            VehicleheaderTableCell5.Text = "Drop Off Date";
            VehicleheaderTableCell6.Text = "Drop Off Time";
            VehicleheaderTableCell7.Text = "Drop Off Location";
            VehicleheaderTableCell8.Text = "";


            Vehicleheader.Cells.Add(VehicleheaderTableCell);
            Vehicleheader.Cells.Add(VehicleheaderTableCell2);
            Vehicleheader.Cells.Add(VehicleheaderTableCell3);
            Vehicleheader.Cells.Add(VehicleheaderTableCell4);
            Vehicleheader.Cells.Add(VehicleheaderTableCell5);
            Vehicleheader.Cells.Add(VehicleheaderTableCell6);
            Vehicleheader.Cells.Add(VehicleheaderTableCell7);
            Vehicleheader.Cells.Add(VehicleheaderTableCell8);

            VehicleTabel.Rows.Add(Vehicleheader);
        }

        private void PopulateNewRowFromVehicleDataTable(DataTable dtVehicle)
        {
            VehicleTabel.Rows.Clear();
            PopulateVehicleHeader();
            if (dtVehicle != null)
            {
                for (int i = 1; i < dtVehicle.Rows.Count; i++)
                {
                    dtVehicle.Rows[i]["VLegNo"] = Convert.ToString(i);

                    string vlegno = Convert.ToString(dtVehicle.Rows[i]["VLegNo"]);

                    string strvpickupdate = Convert.ToString(dtVehicle.Rows[i]["VPickupDate"]);
                    DateTime vpickupdate = new DateTime();
                    if (!string.IsNullOrEmpty(strvpickupdate))
                        vpickupdate = Convert.ToDateTime(dtVehicle.Rows[i]["VPickupDate"]);

                    string strvpickuptime = Convert.ToString(dtVehicle.Rows[i]["VPickupTime"]);
                    DateTime vpickuptime = new DateTime();
                    if (!string.IsNullOrEmpty(strvpickuptime))
                        vpickuptime = Convert.ToDateTime(dtVehicle.Rows[i]["VPickupTime"]);

                    string vpulocation = Convert.ToString(dtVehicle.Rows[i]["VPULocation"]);

                    string strvdropoffdate = Convert.ToString(dtVehicle.Rows[i]["VDropoffDate"]);
                    DateTime vdropoffdate = new DateTime();
                    if (!string.IsNullOrEmpty(strvdropoffdate))
                        vdropoffdate = Convert.ToDateTime(dtVehicle.Rows[i]["VDropoffDate"]);

                    string strvdropofftime = Convert.ToString(dtVehicle.Rows[i]["VDropoffTime"]);
                    DateTime vdropofftime = new DateTime();
                    if (!string.IsNullOrEmpty(strvdropofftime))
                        vdropofftime = Convert.ToDateTime(dtVehicle.Rows[i]["VDropoffTime"]);

                    string vdropofflocation = Convert.ToString(dtVehicle.Rows[i]["VDropoffLocation"]);



                    AddVehicleTable(vlegno, vpickupdate, vpickuptime, vpulocation, vdropoffdate, vdropofftime, vdropofflocation);
                }
            }

            AddVehicleTable_New_Second();

            ViewState["vsVehicle"] = dtVehicle;
        }

        private void AddVehicleTable_New()
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
            TableCell VcellNew8 = new TableCell();

            VcellNew.Style.Add("width", "8%");
            VcellNew2.Style.Add("width", "12%");
            VcellNew3.Style.Add("width", "10%");
            VcellNew4.Style.Add("width", "18%");
            VcellNew5.Style.Add("width", "12%");
            VcellNew6.Style.Add("width", "10%");
            VcellNew7.Style.Add("width", "18%");
            VcellNew8.Style.Add("width", "12%");

            System.Web.UI.WebControls.Label legno = new System.Web.UI.WebControls.Label();
            legno.ID = "legno" + cnt;
            legno.Text = cnt;
            legno.CssClass = "span12";
            legno.Attributes.CssStyle.Add("text-align", "center");
            legno.Attributes.CssStyle.Add("margin-top", "5px");

            DateTimeControl PickUpDate = new DateTimeControl();
            PickUpDate.ID = "PickUpDate" + cnt;
            PickUpDate.LocaleId = 2057;
            PickUpDate.DateOnly = true;
            PickUpDate.UseTimeZoneAdjustment = false;

            DateTimeControl PickUpTime = new DateTimeControl();
            PickUpTime.ID = "PickUpTime" + cnt;
            PickUpTime.LocaleId = 2057;
            PickUpTime.TimeOnly = true;
            PickUpTime.UseTimeZoneAdjustment = false;


            TextBox txtPULocation = new TextBox();
            txtPULocation.ID = "txtPULocation" + cnt;
            txtPULocation.CssClass = "control-border";
            txtPULocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtPULocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtPULocation.Attributes.CssStyle.Add("width", "155px");

            DateTimeControl DropOffDate = new DateTimeControl();
            DropOffDate.ID = "DropOffDate" + cnt;
            DropOffDate.LocaleId = 2057;
            DropOffDate.DateOnly = true;
            DropOffDate.UseTimeZoneAdjustment = false;

            DateTimeControl DropOffTime = new DateTimeControl();
            DropOffTime.ID = "DropOffTime" + cnt;
            DropOffTime.LocaleId = 2057;
            DropOffTime.TimeOnly = true;
            DropOffTime.UseTimeZoneAdjustment = false;

            TextBox txtDropOffLocation = new TextBox();
            txtDropOffLocation.ID = "txtDropOffLocation" + cnt;
            txtDropOffLocation.CssClass = "control-border";
            txtDropOffLocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtDropOffLocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtDropOffLocation.Attributes.CssStyle.Add("width", "155px");

            ImageButton imgbtnVehicleNewRowInsersion = new ImageButton();
            imgbtnVehicleNewRowInsersion.Attributes.Add("runat", "server");
            imgbtnVehicleNewRowInsersion.ID = "imgbtnVehicleNewRowInsersion" + cnt;
            imgbtnVehicleNewRowInsersion.Click += imgbtnVehicleNewRowInsersion_Click;
            imgbtnVehicleNewRowInsersion.ToolTip = "Add new row";
            imgbtnVehicleNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnVehicleNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");



            VcellNew.Controls.Add(legno);
            VcellNew2.Controls.Add(PickUpDate);

            VcellNew3.Controls.Add(PickUpTime);

            VcellNew4.Controls.Add(txtPULocation);

            System.Web.UI.WebControls.Literal litMandatory = new Literal();
            litMandatory.Text = "<span style='color:red'>*</span>";
            //litMandatory.ID = "litmand15" + cnt;
            VcellNew4.Controls.Add(litMandatory);
            VcellNew4.Attributes.Add("style", "min-width:180px");

            VcellNew5.Controls.Add(DropOffDate);

            VcellNew6.Controls.Add(DropOffTime);

            VcellNew7.Controls.Add(txtDropOffLocation);

            System.Web.UI.WebControls.Literal litMandatory2 = new Literal();
            litMandatory2.Text = "<span style='color:red'>*</span>";
            //litMandatory2.ID = "litmand26" + cnt;
            VcellNew7.Controls.Add(litMandatory2);
            VcellNew7.Attributes.Add("style", "min-width:180px");

            VcellNew8.Controls.Add(imgbtnVehicleNewRowInsersion);


            VehiclerowNew.Cells.Add(VcellNew);
            VehiclerowNew.Cells.Add(VcellNew2);
            VehiclerowNew.Cells.Add(VcellNew3);
            VehiclerowNew.Cells.Add(VcellNew4);
            VehiclerowNew.Cells.Add(VcellNew5);
            VehiclerowNew.Cells.Add(VcellNew6);
            VehiclerowNew.Cells.Add(VcellNew7);
            VehiclerowNew.Cells.Add(VcellNew8);

            VehicleTabel.Rows.Add(VehiclerowNew);

            DataTable dtVehicle = new DataTable();
            dtVehicle.Columns.Add("VLegNo");
            dtVehicle.Columns.Add("VPickupDate");
            dtVehicle.Columns.Add("VPickupTime");
            dtVehicle.Columns.Add("VPULocation");
            dtVehicle.Columns.Add("VDropoffDate");
            dtVehicle.Columns.Add("VDropoffTime");
            dtVehicle.Columns.Add("VDropoffLocation");

            dtVehicle.Rows.Add(new string[] { "", "", "", "", "", "", "" });

            ViewState["vsVehicle"] = dtVehicle;


        }

        private void AddVehicleTable_New_Second()
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
            TableCell VcellNew8 = new TableCell();



            VcellNew.Style.Add("width", "8%");
            VcellNew2.Style.Add("width", "12%");
            VcellNew3.Style.Add("width", "10%");
            VcellNew4.Style.Add("width", "18%");
            VcellNew5.Style.Add("width", "12%");
            VcellNew6.Style.Add("width", "10%");
            VcellNew7.Style.Add("width", "18%");
            VcellNew8.Style.Add("width", "12%");

            System.Web.UI.WebControls.Label legno = new System.Web.UI.WebControls.Label();
            legno.ID = "legno" + cnt;
            legno.Text = cnt;
            legno.CssClass = "span12";
            legno.Attributes.CssStyle.Add("text-align", "center");
            legno.Attributes.CssStyle.Add("margin-top", "5px");

            DateTimeControl PickUpDate = new DateTimeControl();
            PickUpDate.ID = "PickUpDate" + cnt;
            PickUpDate.LocaleId = 2057;
            PickUpDate.DateOnly = true;
            PickUpDate.UseTimeZoneAdjustment = false;

            DateTimeControl PickUpTime = new DateTimeControl();
            PickUpTime.ID = "PickUpTime" + cnt;
            PickUpTime.LocaleId = 2057;
            PickUpTime.TimeOnly = true;
            PickUpTime.UseTimeZoneAdjustment = false;

            TextBox txtPULocation = new TextBox();
            txtPULocation.ID = "txtPULocation" + cnt;
            txtPULocation.CssClass = "control-border";
            txtPULocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtPULocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtPULocation.Attributes.CssStyle.Add("width", "155px");

            DateTimeControl DropOffDate = new DateTimeControl();
            DropOffDate.ID = "DropOffDate" + cnt;
            DropOffDate.LocaleId = 2057;
            DropOffDate.DateOnly = true;
            DropOffDate.UseTimeZoneAdjustment = false;

            DateTimeControl DropOffTime = new DateTimeControl();
            DropOffTime.ID = "DropOffTime" + cnt;
            DropOffTime.LocaleId = 2057;
            DropOffTime.TimeOnly = true;
            DropOffTime.UseTimeZoneAdjustment = false;

            TextBox txtDropOffLocation = new TextBox();
            txtDropOffLocation.ID = "txtDropOffLocation" + cnt;
            txtDropOffLocation.CssClass = "control-border";
            txtDropOffLocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtDropOffLocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtDropOffLocation.Attributes.CssStyle.Add("width", "155px");

            ImageButton imgbtnVehicleNewRowInsersion = new ImageButton();
            imgbtnVehicleNewRowInsersion.Attributes.Add("runat", "server");
            imgbtnVehicleNewRowInsersion.ID = "imgbtnVehicleNewRowInsersion" + cnt;
            imgbtnVehicleNewRowInsersion.Click += imgbtnVehicleNewRowInsersion_Click;
            imgbtnVehicleNewRowInsersion.ToolTip = "Add new row";
            imgbtnVehicleNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnVehicleNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");



            VcellNew.Controls.Add(legno);
            VcellNew2.Controls.Add(PickUpDate);

            VcellNew3.Controls.Add(PickUpTime);

            VcellNew4.Controls.Add(txtPULocation);

            System.Web.UI.WebControls.Literal litMandatory = new Literal();
            litMandatory.Text = "<span style='color:red'>*</span>";
            //litMandatory.ID = "litmanda1" + cnt;
            VcellNew4.Controls.Add(litMandatory);
            VcellNew4.Attributes.Add("style", "min-width:180px");

            VcellNew5.Controls.Add(DropOffDate);

            VcellNew6.Controls.Add(DropOffTime);

            VcellNew7.Controls.Add(txtDropOffLocation);

            System.Web.UI.WebControls.Literal litMandatory2 = new Literal();
            litMandatory2.Text = "<span style='color:red'>*</span>";
            //litMandatory2.ID = "litmandt2" + cnt;
            VcellNew7.Controls.Add(litMandatory2);
            VcellNew7.Attributes.Add("style", "min-width:180px");


            VcellNew8.Controls.Add(imgbtnVehicleNewRowInsersion);


            VehiclerowNew.Cells.Add(VcellNew);
            VehiclerowNew.Cells.Add(VcellNew2);
            VehiclerowNew.Cells.Add(VcellNew3);
            VehiclerowNew.Cells.Add(VcellNew4);
            VehiclerowNew.Cells.Add(VcellNew5);
            VehiclerowNew.Cells.Add(VcellNew6);
            VehiclerowNew.Cells.Add(VcellNew7);
            VehiclerowNew.Cells.Add(VcellNew8);

            VehicleTabel.Rows.Add(VehiclerowNew);



        }


        private void AddVehicleTable(string vlegno, DateTime vpickupdate, DateTime vpickuptime, string vpulocation, DateTime vdropoffdate, DateTime vdropofftime, string vdropofflocation)
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
            TableCell VcellNew8 = new TableCell();

            VcellNew.Style.Add("width", "8%");
            VcellNew2.Style.Add("width", "12%");
            VcellNew3.Style.Add("width", "10%");
            VcellNew4.Style.Add("width", "18%");
            VcellNew5.Style.Add("width", "12%");
            VcellNew6.Style.Add("width", "10%");
            VcellNew7.Style.Add("width", "18%");
            VcellNew8.Style.Add("width", "12%");

            System.Web.UI.WebControls.Label legno = new System.Web.UI.WebControls.Label();
            legno.ID = "legno" + cnt;
            legno.Text = vlegno;
            legno.CssClass = "span12";
            legno.Attributes.CssStyle.Add("text-align", "center");
            legno.Attributes.CssStyle.Add("margin-top", "5px");

            DateTimeControl PickUpDate = new DateTimeControl();
            PickUpDate.ID = "PickUpDate" + cnt;
            PickUpDate.SelectedDate = vpickupdate;
            PickUpDate.LocaleId = 2057;
            PickUpDate.DateOnly = true;
            PickUpDate.UseTimeZoneAdjustment = false;

            DateTimeControl PickUpTime = new DateTimeControl();
            PickUpTime.ID = "PickUpTime" + cnt;
            PickUpTime.SelectedDate = vpickuptime;
            PickUpTime.LocaleId = 2057;
            PickUpTime.TimeOnly = true;
            PickUpTime.UseTimeZoneAdjustment = false;

            TextBox txtPULocation = new TextBox();
            txtPULocation.ID = "txtPULocation" + cnt;
            txtPULocation.Text = vpulocation;
            txtPULocation.CssClass = "control-border";
            txtPULocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtPULocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtPULocation.Attributes.CssStyle.Add("width", "155px");

            DateTimeControl DropOffDate = new DateTimeControl();
            DropOffDate.ID = "DropOffDate" + cnt;
            DropOffDate.SelectedDate = vdropoffdate;
            DropOffDate.LocaleId = 2057;
            DropOffDate.DateOnly = true;
            DropOffDate.UseTimeZoneAdjustment = false;

            DateTimeControl DropOffTime = new DateTimeControl();
            DropOffTime.ID = "DropOffTime" + cnt;
            DropOffTime.SelectedDate = vdropofftime;
            DropOffTime.LocaleId = 2057;
            DropOffTime.TimeOnly = true;
            DropOffTime.UseTimeZoneAdjustment = false;

            TextBox txtDropOffLocation = new TextBox();
            txtDropOffLocation.ID = "txtDropOffLocation" + cnt;
            txtDropOffLocation.Text = vdropofflocation;
            txtDropOffLocation.CssClass = "control-border";
            txtDropOffLocation.Attributes.CssStyle.Add("margin-left", "3px");
            txtDropOffLocation.Attributes.CssStyle.Add("margin-top", "1px");
            txtDropOffLocation.Attributes.CssStyle.Add("width", "155px");

            ImageButton imgbtnVehicleEditRowInsersion = new ImageButton();
            imgbtnVehicleEditRowInsersion.Attributes.Add("runat", "server");
            imgbtnVehicleEditRowInsersion.ID = "imgbtnVehicleEditRowInsersion" + cnt;
            imgbtnVehicleEditRowInsersion.Click += imgbtnVehicleEditRowInsersion_Click;
            imgbtnVehicleEditRowInsersion.ToolTip = "Save row";
            imgbtnVehicleEditRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnVehicleEditRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");

            ImageButton imgbtnVehicleDeleteRow = new ImageButton();
            imgbtnVehicleDeleteRow.Attributes.Add("runat", "server");
            imgbtnVehicleDeleteRow.ID = "imgbtnVehicleDeleteRow" + cnt;
            imgbtnVehicleDeleteRow.Click += imgbtnVehicleDeleteRow_Click;
            imgbtnVehicleDeleteRow.ToolTip = "Delete row";
            imgbtnVehicleDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
            imgbtnVehicleDeleteRow.Attributes.CssStyle.Add("padding-left", "5px");

            VcellNew.Controls.Add(legno);
            VcellNew2.Controls.Add(PickUpDate);
            VcellNew3.Controls.Add(PickUpTime);
            VcellNew4.Controls.Add(txtPULocation);

            System.Web.UI.WebControls.Literal litMandatory = new Literal();
            litMandatory.Text = "<span style='color:red'>*</span>";
            //litMandatory.ID = "litmand17" + cnt;
            VcellNew4.Controls.Add(litMandatory);
            VcellNew4.Attributes.Add("style", "min-width:180px");

            VcellNew5.Controls.Add(DropOffDate);
            VcellNew6.Controls.Add(DropOffTime);
            VcellNew7.Controls.Add(txtDropOffLocation);

            System.Web.UI.WebControls.Literal litMandatory2 = new Literal();
            litMandatory2.Text = "<span style='color:red'>*</span>";
            //litMandatory2.ID = "litmand28" + cnt;
            VcellNew7.Controls.Add(litMandatory2);
            VcellNew7.Attributes.Add("style", "min-width:180px");

            VcellNew8.Controls.Add(imgbtnVehicleEditRowInsersion);
            VcellNew8.Controls.Add(imgbtnVehicleDeleteRow);

            VehiclerowNew.Cells.Add(VcellNew);
            VehiclerowNew.Cells.Add(VcellNew2);
            VehiclerowNew.Cells.Add(VcellNew3);
            VehiclerowNew.Cells.Add(VcellNew4);
            VehiclerowNew.Cells.Add(VcellNew5);
            VehiclerowNew.Cells.Add(VcellNew6);
            VehiclerowNew.Cells.Add(VcellNew7);
            VehiclerowNew.Cells.Add(VcellNew8);

            VehicleTabel.Rows.Add(VehiclerowNew);
        }

        private void imgbtnVehicleNewRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            try
            {

                {
                    ImageButton VehicleAddButton = (ImageButton)sender;
                    string buttonid = VehicleAddButton.ID.ToString();
                    buttonid = buttonid.ToLower().Replace("imgbtnvehiclenewrowinsersion", "");

                    int rowid;
                    int.TryParse(buttonid, out rowid);

                    bool VehicleValid = ValidateVehicledtls(rowid);
                    if (VehicleValid)
                    {

                        DataTable dtVehicle = new DataTable();
                        if (ViewState["vsVehicle"] == null)
                        {
                            dtVehicle.Columns.Add("VLegNo");
                            dtVehicle.Columns.Add("VPickupDate");
                            dtVehicle.Columns.Add("VPickupTime");
                            dtVehicle.Columns.Add("VPULocation");
                            dtVehicle.Columns.Add("VDropoffDate");
                            dtVehicle.Columns.Add("VDropoffTime");
                            dtVehicle.Columns.Add("VDropoffLocation");


                            TableRow tr = VehicleTabel.Rows[rowid];
                            System.Web.UI.WebControls.Label VLegNo = (System.Web.UI.WebControls.Label)tr.FindControl("legno" + rowid);
                            DateTimeControl VPickUpDate = (DateTimeControl)tr.FindControl("PickUpDate" + rowid);
                            DateTimeControl VPickUpTime = (DateTimeControl)tr.FindControl("PickUpTime" + rowid);
                            TextBox VPULocation = (TextBox)tr.FindControl("txtPULocation" + rowid);
                            DateTimeControl VDropoffDate = (DateTimeControl)tr.FindControl("DropOffDate" + rowid);
                            DateTimeControl VDropoffTime = (DateTimeControl)tr.FindControl("DropOffTime" + rowid);
                            TextBox VDropoffLocation = (TextBox)tr.FindControl("txtDropOffLocation" + rowid);

                            DataRow dr = dtVehicle.NewRow();
                            dr["VLegNo"] = VLegNo.Text;
                            dr["VPickupDate"] = VPickUpDate.SelectedDate.Date;
                            dr["VPickupTime"] = VPickUpTime.SelectedDate;
                            dr["VPULocation"] = VPULocation.Text;
                            dr["VDropoffDate"] = VDropoffDate.SelectedDate.Date;
                            dr["VDropoffTime"] = VDropoffTime.SelectedDate;
                            dr["VDropoffLocation"] = VDropoffLocation.Text;

                            dtVehicle.Rows.Add(dr);

                            ViewState["vsVehicleSaveFirst"] = dtVehicle;

                        }
                        else
                        {
                            dtVehicle = (DataTable)ViewState["vsVehicle"];

                            TableRow tr = VehicleTabel.Rows[rowid];
                            System.Web.UI.WebControls.Label VLegNo = (System.Web.UI.WebControls.Label)tr.FindControl("legno" + rowid);
                            DateTimeControl VPickUpDate = (DateTimeControl)tr.FindControl("PickUpDate" + rowid);
                            DateTimeControl VPickUpTime = (DateTimeControl)tr.FindControl("PickUpTime" + rowid);
                            TextBox VPULocation = (TextBox)tr.FindControl("txtPULocation" + rowid);
                            DateTimeControl VDropoffDate = (DateTimeControl)tr.FindControl("DropOffDate" + rowid);
                            DateTimeControl VDropoffTime = (DateTimeControl)tr.FindControl("DropOffTime" + rowid);
                            TextBox VDropoffLocation = (TextBox)tr.FindControl("txtDropOffLocation" + rowid);

                            DataRow dr = dtVehicle.NewRow();
                            dr["VLegNo"] = VLegNo.Text;
                            dr["VPickupDate"] = VPickUpDate.SelectedDate.Date;
                            dr["VPickupTime"] = VPickUpTime.SelectedDate;
                            dr["VPULocation"] = VPULocation.Text;
                            dr["VDropoffDate"] = VDropoffDate.SelectedDate.Date;
                            dr["VDropoffTime"] = VDropoffTime.SelectedDate;
                            dr["VDropoffLocation"] = VDropoffLocation.Text;

                            dtVehicle.Rows.Add(dr);
                            ViewState["vsVehicleSaveFirst"] = dtVehicle;

                        }
                        PopulateNewRowFromVehicleDataTable(dtVehicle);
                    }
                    else
                    {
                        lblError.Text = "Please fill all the fields in Vehicle Details Section";
                    }

                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.imgbtnVehicleNewRowInsersion_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }

        }

        private bool ValidateVehicledtls(int rowid)
        {
            bool VehicleValid = true;

            TableRow tr = VehicleTabel.Rows[rowid];
            DateTimeControl VPickUpDate = (DateTimeControl)tr.FindControl("PickUpDate" + rowid);
            DateTimeControl VPickUpTime = (DateTimeControl)tr.FindControl("PickUpTime" + rowid);
            TextBox VPULocation = (TextBox)tr.FindControl("txtPULocation" + rowid);
            DateTimeControl VDropoffDate = (DateTimeControl)tr.FindControl("DropOffDate" + rowid);
            DateTimeControl VDropoffTime = (DateTimeControl)tr.FindControl("DropOffTime" + rowid);
            TextBox VDropoffLocation = (TextBox)tr.FindControl("txtDropOffLocation" + rowid);

            if (VPickUpDate.IsDateEmpty)
                VehicleValid = false;
            else if (VPULocation.Text.Trim() == "")
                VehicleValid = false;
            else if (VDropoffDate.IsDateEmpty)
                VehicleValid = false;
            else if (VDropoffLocation.Text.Trim() == "")
                VehicleValid = false;

            return VehicleValid;

        }

        private void imgbtnVehicleEditRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton SaveButton = (ImageButton)sender;
                string buttonid = SaveButton.ID.ToString();
                buttonid = buttonid.ToLower().Replace("imgbtnvehicleeditrowinsersion", "");
                int rowid;
                int.TryParse(buttonid, out rowid);

                bool VehicleValid = ValidateVehicledtls(rowid);
                if (VehicleValid)
                {

                    DataTable dtVehicle = new DataTable();
                    if (ViewState["vsVehicle"] != null)
                    {
                        dtVehicle = (DataTable)ViewState["vsVehicle"];
                        TableRow tr = VehicleTabel.Rows[rowid];
                        System.Web.UI.WebControls.Label VLegNo = (System.Web.UI.WebControls.Label)tr.FindControl("legno" + rowid);
                        DateTimeControl VPickUpDate = (DateTimeControl)tr.FindControl("PickUpDate" + rowid);
                        DateTimeControl VPickUpTime = (DateTimeControl)tr.FindControl("PickUpTime" + rowid);
                        TextBox VPULocation = (TextBox)tr.FindControl("txtPULocation" + rowid);
                        DateTimeControl VDropoffDate = (DateTimeControl)tr.FindControl("DropOffDate" + rowid);
                        DateTimeControl VDropoffTime = (DateTimeControl)tr.FindControl("DropOffTime" + rowid);
                        TextBox VDropoffLocation = (TextBox)tr.FindControl("txtDropOffLocation" + rowid);
                        //rowid = rowid - 1;

                        if (VLegNo != null)
                            dtVehicle.Rows[rowid]["VLegNo"] = VLegNo.Text;
                        if ((VPickUpDate) != null)
                            dtVehicle.Rows[rowid]["VPickupDate"] = VPickUpDate.SelectedDate.Date;
                        if ((VPickUpTime) != null)
                            dtVehicle.Rows[rowid]["VPickupTime"] = VPickUpTime.SelectedDate;
                        if (VPULocation != null)
                            dtVehicle.Rows[rowid]["VPULocation"] = VPULocation.Text;
                        if ((VDropoffDate) != null)
                            dtVehicle.Rows[rowid]["VDropoffDate"] = VDropoffDate.SelectedDate.Date;
                        if ((VDropoffTime) != null)
                            dtVehicle.Rows[rowid]["VDropoffTime"] = VDropoffTime.SelectedDate;
                        if (VDropoffLocation != null)
                            dtVehicle.Rows[rowid]["VDropoffLocation"] = VDropoffLocation.Text;

                    }
                    PopulateNewRowFromVehicleDataTable(dtVehicle);
                }
                else
                {
                    lblError.Text = "Please fill all the fields in Vehicle Details Section";
                }


            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.imgbtnVehicleEditRowInsersion_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void imgbtnVehicleDeleteRow_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                ImageButton RemoveButton = (ImageButton)sender;
                string buttonid = RemoveButton.ID.ToString();
                buttonid = buttonid.ToLower().Replace("imgbtnvehicledeleterow", "");
                int rowid;
                int.TryParse(buttonid, out rowid);
                //rowid = rowid - 1;
                DataTable currentDataTable = (DataTable)ViewState["vsVehicle"];
                int itemscount = currentDataTable.Rows.Count;
                currentDataTable.Rows[rowid].Delete();
                PopulateNewRowFromVehicleDataTable(currentDataTable);
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TravelRequest.imgbtnVehicleDeleteRow_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        #endregion

        protected void btnCombinedSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                bool CombinedTravelLeg = ValidateCombinedTravel();
                if (CombinedTravelLeg)
                {
                    bool legvalid = ValidateTravelLeg();
                    if (legvalid)
                    {
                        bool valid = ValidateDetails();
                        if (valid)
                        {
                            bool bProceed = SetTravelSummaryList(false, "Pending Approval");
                            SetCombinedTravelItineraryList();
                            SetTravelLeg(refno);
                            SendEmail();
                            Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Submit");
                        }
                        else
                        {
                            lblError.Text = "Please fill all the mandatory fields, verify email format.";
                        }
                    }
                    else
                    {
                        lblError.Text = "Please fill all the mandatory fields. ";
                    }
                }
                else
                {
                    lblError.Text = "Please select atleast one leg.";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnCombinedSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void SendEmail()
        {
            string strRefNo = lblReferenceNo.Text.Split(':')[1].Trim();
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPSite site = SPContext.Current.Site;

                SPWeb web = site.OpenWeb();
                string lstURL = HrWebUtility.GetListUrl("EmailConfig");
                SPList lst = SPContext.Current.Site.RootWeb.GetList(lstURL);
                //SPList lst = web.Lists["EmailConfig"];
                SPQuery oQuery = new SPQuery();
                oQuery.Query = "<Query><Where><Eq><FieldRef Name='FormType' /><Value Type='Text'>TravelRequest</Value></Eq></Where></Query>";
                oQuery.ViewFields = string.Concat(
                    "<FieldRef Name='FormType' />",
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='EmailIP' />",
                                "<FieldRef Name='ApprovalSubject' />",
                                "<FieldRef Name='ApprovalMessage' />");
                SPListItemCollection collListItems = lst.GetItems(oQuery);

                foreach (SPListItem itm in collListItems)
                {
                    if (Convert.ToString(itm["FormType"]) == "TravelRequest")
                    {
                        //send email
                        string strFrom = "";
                        string strTo = "";
                        string strSubject = "";
                        string strMessage = "";

                        //get the Traveller Name
                        SPWeb mySite = SPContext.Current.Web;
                        string TravellerName = "";

                        /*SPFieldUserValueCollection ReqdUserCollection = new SPFieldUserValueCollection();
                        string[] reqdUsersSeperated = TravellerNamePeopleEditor.CommaSeparatedAccounts.Split(',');
                        foreach (string UserSeperated in reqdUsersSeperated)
                        {
                            if (!string.IsNullOrEmpty(UserSeperated))
                            {
                                SPUser User = mySite.SiteUsers[UserSeperated];
                                TravellerName = User.LoginName;

                            }
                        }

                        if (TravellerName != "")
                        {
                            string[] tmpTravellerName = TravellerName.Split('|');
                            TravellerName = HrWebUtility.GetUser(tmpTravellerName[tmpTravellerName.Length - 1]);
                        }*/
                        TravellerName = txtTravellerName.Text;

                        SmtpClient smtpClient = new SmtpClient();
                        smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                        smtpClient.Port = 25;
                        //smtpClient.Host = "smtp.gmail.com";
                        string url = site.Url + "/pages/hrweb/TravelReview.aspx?refno=" + strRefno;
                        strFrom = Convert.ToString(itm["Title"]);

                        strTo = GetApprover();

                        string[] tmparr = strTo.Split('|');
                        strTo = tmparr[tmparr.Length - 1];
                        if (strTo.Contains("#"))
                            strTo = strTo.Split('#')[1];


                        strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                        strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;TRAVELLER&gt;", TravellerName.Trim()).
                            Replace("&lt;TRAVELDATE&gt;", DepartureDate.SelectedDate.ToString("dd/MM/yyyy"));

                        MailMessage mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");
                        string[] mailto = strTo.Split(';');
                        var distinctIDs = mailto.Distinct();
                        foreach (string s in distinctIDs)
                        {
                            if (s.Trim() != "")
                                mailMessage.To.Add(s);
                        }
                        mailMessage.Subject = strSubject;
                        mailMessage.Body = strMessage;
                        mailMessage.IsBodyHtml = true;
                        smtpClient.Send(mailMessage);


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

        private string GetApprover()
        {
            string approver = string.Empty;
            SPSite site = SPContext.Current.Site;
            if (ddlPositionTitle.SelectedValue == "Other")
            {
                approver = ManagerPeopleEditor.CommaSeparatedAccounts.Split(',')[0];
            }
            else
            {
                SPWeb web = site.OpenWeb();
                SPList lst = web.Lists["TravelApprovalinfo"];
                SPQuery oQuery = new SPQuery();
                oQuery.ViewFields = string.Concat(
                "<FieldRef Name='CEOApprover' />",
                            "<FieldRef Name='ChairmanApprover' />");
                SPListItemCollection collListItems = lst.GetItems(oQuery);
                foreach (SPListItem itm in collListItems)
                {
                    if (ddlPositionTitle.SelectedValue == "CEO" || ddlPositionTitle.SelectedValue == "Director")
                    {
                        approver = Convert.ToString(itm["ChairmanApprover"]);
                    }
                    else if (ddlPositionTitle.SelectedValue == "Chairman")
                    {
                        approver = Convert.ToString(itm["CEOApprover"]);
                    }
                }
            }
            return approver;
        }

        private bool ValidateDetails()
        {
            bool valid = true;

            /*SPWeb mySite = SPContext.Current.Web;
            string TravellerName = "";
            
            SPFieldUserValueCollection ReqdUserCollection = new SPFieldUserValueCollection();
            string[] reqdUsersSeperated = TravellerNamePeopleEditor.CommaSeparatedAccounts.Split(',');
            foreach (string UserSeperated in reqdUsersSeperated)
            {
                if (!string.IsNullOrEmpty(UserSeperated))
                {
                    SPUser User = mySite.SiteUsers[UserSeperated];
                    TravellerName = User.LoginName;

                }
            }*/

            if (txtTravellerName.Text.Trim() == "")
                valid = false;

            bool isEmail = Regex.IsMatch(txtTravellerEmailID.Text.Trim(), @"\A(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?)\Z");
            if (!isEmail)
            {
                valid = false;
            }
            else if (txtTravellerEmailID.Text.Trim() == "")
            {
                valid = false;
            }
            else if (ddlPositionTitle.SelectedValue == "Other" && txtIfOthers.Text.Trim() == "")
                valid = false;
            else if (ManagerPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                valid = false;
            /*   Request was made to remove departure and return dates from form.
            else if (DepartureDate.IsDateEmpty)
                valid = false;
            else if (ReturnDate.IsDateEmpty)
                valid = false;*/
            else if (txtPurposeofTravel.Text.Trim() == "")
                valid = false;
            return valid;
        }

        private bool ValidateCombinedTravel()
        {
            bool CombinedTravelLeg = true;

            if (ddlNoOfLegs.SelectedValue == "0")
                CombinedTravelLeg = false;

            return CombinedTravelLeg;
        }

        private bool ValidateTravelLeg()
        {
            bool legvalid = true;

            int cntLeg = Convert.ToInt16(ddlNoOfLegs.SelectedValue);

            for (int inx = 1; inx <= cntLeg; inx++)
            {
                Table tblLeg = (Table)accordion.FindControl("tblLeg" + inx);

                if (chkboxFlight.Checked == true)
                {
                    Table tblFlight = (Table)tblLeg.FindControl("tblFlight" + inx);

                    for (int flCnt = 1; flCnt <= tblFlight.Rows.Count - 1; flCnt++)
                    {
                        DateTimeControl FlightDeparturedate = (DateTimeControl)tblFlight.Rows[flCnt].FindControl("FlightDepartureDate" + inx);
                        TextBox FlightDeparturelocation = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightDeptLocation" + inx);
                        TextBox FlightCarrier = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightCarrier" + inx);
                        TextBox FlightNumber = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightNumber" + inx);
                        DateTimeControl FlightDeparturetime = (DateTimeControl)tblFlight.Rows[flCnt].FindControl("FlightDepartureTime" + inx);
                        TextBox FlightDestinationlocation = (TextBox)tblFlight.Rows[flCnt].FindControl("txtFlightDestLocation" + inx);

                        if (FlightDeparturedate.IsDateEmpty)
                            legvalid = false;
                        else if (FlightDeparturelocation.Text.Trim() == "")
                            legvalid = false;
                        else if (FlightDestinationlocation.Text.Trim() == "")
                            legvalid = false;


                    }
                }

                /*if (CheckBoxAccommodation.Checked == true)
                {
                    Table tblAccommodation = (Table)tblLeg.FindControl("tblAccommodation" + inx);

                    for (int AcomCnt = 1; AcomCnt <= tblAccommodation.Rows.Count - 1; AcomCnt++)
                    {
                        DateTimeControl AccomCheckinDate = (DateTimeControl)tblAccommodation.Rows[AcomCnt].FindControl("CheckinCTDate" + inx);
                        TextBox AccomHotelName = (TextBox)tblAccommodation.Rows[AcomCnt].FindControl("txtCTHotelName" + inx);
                        System.Web.UI.WebControls.Label AccomNoofNights = (System.Web.UI.WebControls.Label)tblAccommodation.Rows[AcomCnt].FindControl("txtCTNoofNights" + inx);
                        DateTimeControl AccomCheckoutDate = (DateTimeControl)tblAccommodation.Rows[AcomCnt].FindControl("CheckoutCTDate" + inx);

                        if (AccomCheckinDate.IsDateEmpty)
                            legvalid = false;
                        else if (AccomHotelName.Text.Trim() == "")
                            legvalid = false;
                        else if (AccomCheckoutDate.IsDateEmpty)
                            legvalid = false;

                    }
                }


                if (VehicleReqRadioButton.Items[2].Selected == true)
                {
                    Table tblVehicle = (Table)tblLeg.FindControl("tblVehicle" + inx);

                    for (int VCnt = 1; VCnt <= tblVehicle.Rows.Count - 1; VCnt++)
                    {
                        DateTimeControl HCPickUpDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCDate" + inx);
                        DateTimeControl HCPickUpTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCTime" + inx);
                        TextBox HCPickUpLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCPickUpLocation" + inx);
                        DateTimeControl HCDropoffDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCDate" + inx);
                        DateTimeControl HCDropoffTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCTime" + inx);
                        TextBox HCReturnLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCReturnLocation" + inx);

                        if (HCPickUpDate.IsDateEmpty)
                            legvalid = false;
                        else if (HCPickUpLocation.Text.Trim() == "")
                            legvalid = false;
                        else if (HCDropoffDate.IsDateEmpty)
                            legvalid = false;
                        else if (HCReturnLocation.Text.Trim() == "")
                            legvalid = false;
                          
                    }
                }

                if (VehicleReqRadioButton.Items[1].Selected == true)
                {
                    Table tblVehicle = (Table)tblLeg.FindControl("tblVehicle" + inx);

                    for (int VCnt = 1; VCnt <= tblVehicle.Rows.Count - 1; VCnt++)
                    {
                        DateTimeControl HCPickUpDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCDate" + inx);
                        DateTimeControl HCPickUpTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("PickUpHCTime" + inx);
                        TextBox HCPickUpLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCPickUpLocation" + inx);
                        DateTimeControl HCDropoffDate = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCDate" + inx);
                        DateTimeControl HCDropoffTime = (DateTimeControl)tblVehicle.Rows[VCnt].FindControl("DropoffHCTime" + inx);
                        TextBox HCReturnLocation = (TextBox)tblVehicle.Rows[VCnt].FindControl("txtHCReturnLocation" + inx);

                        if (HCPickUpDate.IsDateEmpty)
                            legvalid = false;
                        else if (HCPickUpLocation.Text.Trim() == "")
                            legvalid = false;
                        else if (HCDropoffDate.IsDateEmpty)
                            legvalid = false;
                        else if (HCReturnLocation.Text.Trim() == "")
                            legvalid = false;

                    }
                }*/


            }

            return legvalid;

        }

        protected void btnAccomSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtAccommodation = (DataTable)ViewState["vsAccomSaveFirst"];
                if (dtAccommodation != null && dtAccommodation.Rows.Count > 0)
                {
                    bool valid = ValidateDetails();
                    if (valid)
                    {
                        string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                        bool bProceed = SetTravelSummaryList(false, "Pending Approval");
                        SetTravelAccommodationList();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "Please fill all the mandatory fields and Check the Email ID in correct format";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the fields in Accommodation Details Section";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnAccomSubmit_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnVehicleSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtVehicle = (DataTable)ViewState["vsVehicleSaveFirst"];
                if (dtVehicle != null && dtVehicle.Rows.Count > 0)
                {
                    bool valid = ValidateDetails();
                    if (valid)
                    {
                        string refno = lblReferenceNo.Text.Split(':')[1].Trim();
                        bool bProceed = SetTravelSummaryList(false, "Pending Approval");
                        SetTravelVehicleList();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/TravelStatus.aspx?refno=" + refno + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "Please fill all the mandatory fields and Check the Email ID in correct format";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the fields in Vehicle Details Section";
                }


            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelRequest.btnVehicleSubmit_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }



    }

}



