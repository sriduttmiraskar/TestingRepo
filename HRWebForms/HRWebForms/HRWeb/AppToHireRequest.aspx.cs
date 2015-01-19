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
using System.Configuration;
using System.Data;
using Microsoft.SharePoint.Utilities;
using System.Net.Mail;
using System.Web.UI.WebControls;

namespace HRWebForms.HRWeb
{
    public partial class AppToHireRequest : WebPartPage
    {

        string strRefno = string.Empty;
        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                lblError.Text = string.Empty;                
                lblDateNow.Text = DateTime.Now.ToString("dd/MM/yyyy");
                if (!IsPostBack)
                {

                    bool bValid = false;
                    if (Page.Request.QueryString["refno"] != null)
                    {
                        lblReferenceNo.Text = Page.Request.QueryString["refno"];
                        strRefno = lblReferenceNo.Text;
                        bValid = ValidateApplication();
                        if (bValid)
                        {
                            PopulateTaxonomy();
                            AddBusinessUnitWAandLoc();
                            PopulateChoiceFields();
                            GetAllListData();
                            GetCommentHistory(strRefno);
                          
                        }
                    }
                    else
                    {
                        lblError.Text = "";
                        PopulateTaxonomy();
                        AddBusinessUnitWAandLoc();
                        PopulateChoiceFields();
                        bValid = SetAppToHireGeneralInfoList(true, "");
                        GetCommentHistory(strRefno);
   
                    }

                    if (!bValid)                  
                    {
                        lblError.Text = "The application number passed does not exist or has already been submitted.";

                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.Page_Load", ex.Message);
                lblError.Text = ex.Message;
                //lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private bool ValidateApplication()
        {
            bool bValid = false;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("AppToHireGeneralInfo", strRefno);
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

            GroupCollection groups = trmStore.Groups;
            foreach (Group termGroup in trmStore.Groups)
            {

                switch (termGroup.Name)
                {
                    case "HR Group":

                        ddlBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);                        
                        ddlBusinessUnit.DataTextField = "Term";
                        ddlBusinessUnit.DataValueField = "Term";
                        ddlBusinessUnit.DataBind();

                        ddlWagedBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);
                        ddlWagedBusinessUnit.DataTextField = "Term";
                        ddlWagedBusinessUnit.DataValueField = "Term";
                        ddlWagedBusinessUnit.DataBind();

                        ddlContraBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);
                        ddlContraBusinessUnit.DataTextField = "Term";
                        ddlContraBusinessUnit.DataValueField = "Term";
                        ddlContraBusinessUnit.DataBind();

                        ddlExpatBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);
                        ddlExpatBusinessUnit.DataTextField = "Term";
                        ddlExpatBusinessUnit.DataValueField = "Term";
                        ddlExpatBusinessUnit.DataBind();

                        ddlPositionType.DataSource = AddTerms("Position Type", termGroup);
                        ddlPositionType.DataTextField = "Term";
                        ddlPositionType.DataValueField = "Term";
                        ddlPositionType.DataBind();
                        for (int i = 0; i < ddlPositionType.Items.Count; i++)
                        {
                            if (ddlPositionType.Items[i].Text == "Salary")
                                ddlPositionType.Items[i].Selected = true;
                        }
                        ddlReasonPositionRqd.DataSource = AddTerms("Reason For Position", termGroup);
                        ddlReasonPositionRqd.DataTextField = "Term";
                        ddlReasonPositionRqd.DataValueField = "Term";
                        ddlReasonPositionRqd.DataBind();
                        ddlRecruitmentProc.DataSource = AddTerms("Recruitment Process", termGroup);
                        ddlRecruitmentProc.DataTextField = "Term";
                        ddlRecruitmentProc.DataValueField = "Term";
                        ddlRecruitmentProc.DataBind();


                        /*ddlWorkArea.DataSource = AddTerms("Work Area", termGroup);
                        ddlWorkArea.DataTextField = "Term";
                        ddlWorkArea.DataValueField = "TermID";
                        ddlWorkArea.DataBind();
                        ddlWagedWorkArea.DataSource = AddTerms("Work Area", termGroup);
                        ddlWagedWorkArea.DataTextField = "Term";
                        ddlWagedWorkArea.DataValueField = "TermID";
                        ddlWagedWorkArea.DataBind();
                        ddlContraWorkArea.DataSource = AddTerms("Work Area", termGroup);
                        ddlContraWorkArea.DataTextField = "Term";
                        ddlContraWorkArea.DataValueField = "TermID";
                        ddlContraWorkArea.DataBind();
                        ddlExpatWorkArea.DataSource = AddTerms("Work Area", termGroup);
                        ddlExpatWorkArea.DataTextField = "Term";
                        ddlExpatWorkArea.DataValueField = "TermID";
                        ddlExpatWorkArea.DataBind();*/
                        ddlTypeOfPosition.DataSource = AddTerms("Contract Type", termGroup);
                        ddlTypeOfPosition.DataTextField = "Term";
                        ddlTypeOfPosition.DataValueField = "Term";
                        ddlTypeOfPosition.DataBind();
                        ddlWagedTypOfPosition.DataSource = AddTerms("Contract Type", termGroup);
                        ddlWagedTypOfPosition.DataTextField = "Term";
                        ddlWagedTypOfPosition.DataValueField = "Term";
                        ddlWagedTypOfPosition.DataBind();
                        ddlContraTypeofPosition.DataSource = AddTerms("Contract Type", termGroup);
                        ddlContraTypeofPosition.DataTextField = "Term";
                        ddlContraTypeofPosition.DataValueField = "Term";
                        ddlContraTypeofPosition.DataBind();
                        ddlExpatTypeofPosition.DataSource = AddTerms("Contract Type", termGroup);
                        ddlExpatTypeofPosition.DataTextField = "Term";
                        ddlExpatTypeofPosition.DataValueField = "Term";
                        ddlExpatTypeofPosition.DataBind();
                        ddlWagedShiftRotation.DataSource = AddTerms("Shift", termGroup);
                        ddlWagedShiftRotation.DataTextField = "Term";
                        ddlWagedShiftRotation.DataValueField = "Term";
                        ddlWagedShiftRotation.DataBind();
                        break;
                    case "Location Group":
                        /*ddlSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                        ddlSiteLocation.DataTextField = "Term";
                        ddlSiteLocation.DataValueField = "TermID";
                        ddlSiteLocation.DataBind();
                        ddlWagedSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                        ddlWagedSiteLocation.DataTextField = "Term";
                        ddlWagedSiteLocation.DataValueField = "TermID";
                        ddlWagedSiteLocation.DataBind();
                        ddlContraSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                        ddlContraSiteLocation.DataTextField = "Term";
                        ddlContraSiteLocation.DataValueField = "TermID";
                        ddlContraSiteLocation.DataBind();
                        ddlExpatSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                        ddlExpatSiteLocation.DataTextField = "Term";
                        ddlExpatSiteLocation.DataValueField = "TermID";
                        ddlExpatSiteLocation.DataBind();*/
                        break;
                    case "Organsiation Group":
                        /*ddlBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                        ddlBusinessUnit.DataTextField = "Term";
                        ddlBusinessUnit.DataValueField = "Term";
                        ddlBusinessUnit.DataBind();
                        ddlWagedBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                        ddlWagedBusinessUnit.DataTextField = "Term";
                        ddlWagedBusinessUnit.DataValueField = "Term";
                        ddlWagedBusinessUnit.DataBind();
                        ddlContraBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                        ddlContraBusinessUnit.DataTextField = "Term";
                        ddlContraBusinessUnit.DataValueField = "Term";
                        ddlContraBusinessUnit.DataBind();
                        ddlExpatBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                        ddlExpatBusinessUnit.DataTextField = "Term";
                        ddlExpatBusinessUnit.DataValueField = "Term";
                        ddlExpatBusinessUnit.DataBind();*/
                        break;
                }
            }
        }

        private DataTable AddTerms(string strTermset, Group termGroup)
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

        private DataTable AddSubTerms(string strTermset, Group termGroup, string strSubTermSet)
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

        private void PopulateChoiceFields()
        {

            //SPList oList = SPContext.Current.Web.Lists["RemunerationDetails"];
            string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPFieldChoice GradeChoice = (SPFieldChoice)oList.Fields["Grade"];
                ddlGrade.DataSource = GradeChoice.Choices;
                ddlGrade.DataBind();
                SPFieldChoice VehicleChoice = (SPFieldChoice)oList.Fields["Vehicle"];
                ddlVehicle.DataSource = VehicleChoice.Choices;
                ddlVehicle.DataBind();
                ddlVehicle.SelectedValue = "N/A";
                //Waged Tab items
                SPFieldChoice WagedLevelChoice = (SPFieldChoice)oList.Fields["Level"];
                ddlWagedLevel.DataSource = WagedLevelChoice.Choices;
                ddlWagedLevel.DataBind();
                SPFieldChoice WagedVehicleChoice = (SPFieldChoice)oList.Fields["Vehicle"];
                ddlWagedVehicle.DataSource = WagedVehicleChoice.Choices;
                ddlWagedVehicle.DataBind();
                ddlWagedVehicle.SelectedValue = "N/A";
                //Expat Tab Items
                SPFieldChoice ExpatLevelChoice = (SPFieldChoice)oList.Fields["Grade"];
                ddlExpatGrade.DataSource = ExpatLevelChoice.Choices;
                ddlExpatGrade.DataBind();
                SPFieldChoice ExpatVehicleChoice = (SPFieldChoice)oList.Fields["Vehicle"];
                ddlExpatVehicle.DataSource = ExpatVehicleChoice.Choices;
                ddlExpatVehicle.DataBind();
                ddlExpatVehicle.SelectedValue = "N/A";
            });
        }

        private void GetAllListData()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;

            GetApptoHireListdata(strRefno);

            if (ddlPositionType.SelectedItem.Text == "Salary")
            {
                GetSalaryPositionDetails(strRefno);
                GetSalaryRemunerattionDetails(strRefno);

            }
            else if (ddlPositionType.SelectedItem.Text == "Waged")
            {
                GetWagedPositionDetails(strRefno);
                GetWagedRemunerattionDetails(strRefno);
            }
            else if (ddlPositionType.SelectedItem.Text == "Contractor")
            {
                GetContractorPositionDetails(strRefno);
                GetContractorRoleStatementDetails(strRefno);
            }
            else if (ddlPositionType.SelectedItem.Text == "Expatriate")
            {
                GetExpatPositionDetails(strRefno);
                GetExpatRemunerattionDetails(strRefno);
            }
            //GetSuccessfulApplicantDetails(strRefno);

        }

        private SPListItemCollection GetListData(string GetListByName, string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection collectionItems = null;
            //SPList oList = SPContext.Current.Web.Lists[GetListByName];
            string lstURL = HrWebUtility.GetListUrl(GetListByName);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oQuery = new SPQuery();
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                oQuery.RowLimit = 100;
                collectionItems = oList.GetItems(oQuery);
            });
            return collectionItems;
        }

        private void GetApptoHireListdata(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string MetadataField1 = "PositionType";
            string MetadataField2 = "PositionReason";
            string MetadataField3 = "RecruitmentProcess";
            SPWeb mySite = SPContext.Current.Web;
            SPListItemCollection collectionItems = GetListData("AppToHireGeneralInfo", strRefno);
            foreach (SPListItem ListItems in collectionItems)
            {
                //DateofRequest.SelectedDate = Convert.ToDateTime(ListItems["DateOfRequest"]);
                lblDateNow.Text = Convert.ToDateTime(ListItems["DateOfRequest"]).ToString("dd/MM/yyyy");

                /* TaxonomyFieldValue value = ListItems[MetadataField1] as TaxonomyFieldValue;
                 if (value != null)*/
                ddlPositionType.SelectedValue = Convert.ToString(ListItems[MetadataField1]);
                //ddlPositionType.SelectedItem.Text = value.Label;

                /* TaxonomyFieldValue value1 = ListItems[MetadataField2] as TaxonomyFieldValue;
                 if (value1 != null)*/
                ddlReasonPositionRqd.SelectedValue = Convert.ToString(ListItems[MetadataField2]);
                //ddlReasonPositionRqd.SelectedItem.Text = value1.Label;

                /*if (ListItems["ReplacementFor"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ReplacementFor"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    PositionHeldByPeoplePicker.CommaSeparatedAccounts = strpplpicker;
                }*/
                txtPositionHeldBy.Text = Convert.ToString(ListItems["ReplacementFor"]);
                ddlReasonPositionRqd.SelectedValue = Convert.ToString(ListItems["PositionReason"]);
                /*TaxonomyFieldValue value2 = ListItems[MetadataField3] as TaxonomyFieldValue;
                if (value2 != null)*/
                ddlRecruitmentProc.SelectedValue = Convert.ToString(ListItems[MetadataField3]);
                //ddlRecruitmentProc.SelectedItem.Text = value2.Label;

                if (string.Equals(Convert.ToString(ListItems["IsBudgetedPosition"]), "True"))
                    ddlBudgetPosition.SelectedValue = "Yes";
                //ddlBudgetPosition.SelectedItem.Text = "Yes";
                else if (string.Equals(Convert.ToString(ListItems["IsBudgetedPosition"]), "False"))
                    ddlBudgetPosition.SelectedValue = "No";

                if (string.Equals(Convert.ToString(ListItems["IsIncreaseInStaffing"]), "True"))
                    ddlStaffingLevel.SelectedValue = "Yes";
                else if (string.Equals(Convert.ToString(ListItems["IsIncreaseInStaffing"]), "False"))
                    ddlStaffingLevel.SelectedValue = "No";

                if (ListItems["RequiredBy"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["RequiredBy"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    RequiredByPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }
                txtComments.Text = Convert.ToString(ListItems["Comments"]);
                txtDetails.Text = Convert.ToString(ListItems["Details"]);
            }
        }

        private void GetSalaryPositionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string PstDtlMetaField1 = "BusinessUnit";
            //string PstDtlMetaField2 = "WorkArea";
            //string PstDtlMetaField3 = "SiteLocation";
            string PstDtlMetaField4 = "PositionType";
            SPListItemCollection PositionDetailscollecItems = GetListData("PositionDetails", strRefno);
            foreach (SPListItem ListItems in PositionDetailscollecItems)
            {
                txtPositionTitle.Text = Convert.ToString(ListItems["PositionTitle"]);
                txtSAPPositionNo.Text = Convert.ToString(ListItems["SAPPositionNo"]);

                /*TaxonomyFieldValue value = ListItems[PstDtlMetaField1] as TaxonomyFieldValue;
                if (value != null)
                {
                    ddlBusinessUnit.SelectedValue = value.TermGuid;
                }*/
                ddlBusinessUnit.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField1]);

                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlBusinessUnit.SelectedItem.Text);

                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    ddlWorkArea.DataSource = dtWorkArea;
                    ddlWorkArea.DataValueField = "WorkArea";
                    ddlWorkArea.DataTextField = "WorkArea";
                    ddlWorkArea.DataBind();
                    ddlWorkArea.SelectedValue = Convert.ToString(ListItems["WorkArea"]);
                }


                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlBusinessUnit.SelectedItem.Text);

                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    ddlSiteLocation.DataSource = dtLocation;
                    ddlSiteLocation.DataValueField = "Location";
                    ddlSiteLocation.DataTextField = "Location";
                    ddlSiteLocation.DataBind();
                    ddlSiteLocation.SelectedValue = Convert.ToString(ListItems["SiteLocation"]);
                }




                if (ListItems["ReportsTo"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ReportsTo"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    ReportsToPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }
                txtCostCentre.Text = Convert.ToString(ListItems["CostCenter"]);
                
                /*TaxonomyFieldValue value3 = ListItems[PstDtlMetaField4] as TaxonomyFieldValue;
                if (value3 != null)
                    ddlTypeOfPosition.SelectedValue = value3.TermGuid;*/
                ddlTypeOfPosition.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField4]);


                StartDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedStartDate"]).ToString("dd/MM/yyyy"));
                EndDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedEndDate"]).ToString("dd/MM/yyyy"));

            }
        }

        private void GetSalaryRemunerattionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("RemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlGrade.SelectedValue = Convert.ToString(ListItems["Grade"]);
                ddlVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtFAR.Text = Convert.ToString(ListItems["FAR"]);
                //txtSTI.Text = Convert.ToString(ListItems["STI"]);
                ddlSTI.SelectedValue = Convert.ToString(ListItems["STI"]);
                txtIfOthers.Text = Convert.ToString(ListItems["OtherVehicleText"]);
            }
        }

        private void GetWagedPositionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string PstDtlMetaField1 = "BusinessUnit";
            //string PstDtlMetaField2 = "WorkArea";
            //string PstDtlMetaField3 = "SiteLocation";
            string PstDtlMetaField4 = "PositionType";
            SPListItemCollection PositionDetailscollecItems = GetListData("PositionDetails", strRefno);
            foreach (SPListItem ListItems in PositionDetailscollecItems)
            {
                txtWagedPositionTitle.Text = Convert.ToString(ListItems["PositionTitle"]);
                txtWagedSAPPositionNo.Text = Convert.ToString(ListItems["SAPPositionNo"]);

                /*TaxonomyFieldValue value = ListItems[PstDtlMetaField1] as TaxonomyFieldValue;
                if (value != null)
                    ddlWagedBusinessUnit.SelectedValue = value.TermGuid;*/

                ddlWagedBusinessUnit.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField1]);

                SPListItemCollection oItemsWaged = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlWagedBusinessUnit.SelectedItem.Text);
                if (oItemsWaged != null && oItemsWaged.Count > 0)
                {
                    DataTable dtWagedWorkArea = oItemsWaged.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    ddlWagedWorkArea.DataSource = dtWagedWorkArea;
                    ddlWagedWorkArea.DataValueField = "WorkArea";
                    ddlWagedWorkArea.DataTextField = "WorkArea";
                    ddlWagedWorkArea.DataBind();

                    ddlWagedWorkArea.SelectedValue = Convert.ToString(ListItems["WorkArea"]);
                }

                /*TaxonomyFieldValue value1 = ListItems[PstDtlMetaField2] as TaxonomyFieldValue;
                if (value1 != null)
                    ddlWagedWorkArea.SelectedValue = value1.TermGuid;
                TaxonomyFieldValue value2 = ListItems[PstDtlMetaField3] as TaxonomyFieldValue;
                if (value2 != null)
                    ddlWagedSiteLocation.SelectedValue = value2.TermGuid;*/


                SPListItemCollection oItemsSalary = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlWagedBusinessUnit.SelectedItem.Text);

                if (oItemsSalary != null && oItemsSalary.Count > 0)
                {
                    DataTable dtSalLocation = oItemsSalary.GetDataTable().DefaultView.ToTable(true, "Location");
                    ddlWagedSiteLocation.DataSource = dtSalLocation;
                    ddlWagedSiteLocation.DataValueField = "Location";
                    ddlWagedSiteLocation.DataTextField = "Location";
                    ddlWagedSiteLocation.DataBind();
                    ddlWagedSiteLocation.SelectedValue = Convert.ToString(ListItems["SiteLocation"]);

                }


                if (ListItems["ReportsTo"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ReportsTo"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    ReportsToWagedPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }
                txtWagedCostCentre.Text = Convert.ToString(ListItems["CostCenter"]);
               
                /*TaxonomyFieldValue value3 = ListItems[PstDtlMetaField4] as TaxonomyFieldValue;
                if (value3 != null)
                    ddlWagedTypOfPosition.SelectedValue = value3.TermGuid;*/

                ddlWagedTypOfPosition.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField4]);


                WagedStartDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedStartDate"]).ToString("dd/MM/yyyy"));
                WagedEndDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedEndDate"]).ToString("dd/MM/yyyy"));
            }
        }

        private void GetWagedRemunerattionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string RemuDtlMetafield = "ShiftRotation";
            SPListItemCollection RemunarationDetailscollecItems = GetListData("RemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlWagedLevel.SelectedValue = Convert.ToString(ListItems["Level"]);
                
                /*TaxonomyFieldValue value = ListItems[RemuDtlMetafield] as TaxonomyFieldValue;
                if (value != null)
                    ddlWagedShiftRotation.SelectedValue = value.TermGuid;*/

                ddlWagedShiftRotation.SelectedValue = Convert.ToString(ListItems[RemuDtlMetafield]);

                ddlWagedVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtWagedIfOther.Text = Convert.ToString(ListItems["OtherVehicleText"]);
            }
        }

        private void GetContractorPositionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string PstDtlMetaField1 = "BusinessUnit";
            //string PstDtlMetaField2 = "WorkArea";
            //string PstDtlMetaField3 = "SiteLocation";
            string PstDtlMetaField4 = "PositionType";
            SPListItemCollection PositionDetailscollecItems = GetListData("PositionDetails", strRefno);
            foreach (SPListItem ListItems in PositionDetailscollecItems)
            {
                txtContraRole.Text = Convert.ToString(ListItems["Role"]);

               /* TaxonomyFieldValue value = ListItems[PstDtlMetaField1] as TaxonomyFieldValue;
                if (value != null)
                    ddlContraBusinessUnit.SelectedValue = value.TermGuid;*/
                ddlContraBusinessUnit.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField1]);


                SPListItemCollection oItemsContra = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlContraBusinessUnit.SelectedItem.Text);
                if (oItemsContra != null && oItemsContra.Count > 0)
                {
                    DataTable dtContradWorkArea = oItemsContra.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    ddlContraWorkArea.DataSource = dtContradWorkArea;
                    ddlContraWorkArea.DataValueField = "WorkArea";
                    ddlContraWorkArea.DataTextField = "WorkArea";
                    ddlContraWorkArea.DataBind();
                    ddlContraWorkArea.SelectedValue = Convert.ToString(ListItems["WorkArea"]);
                }

                SPListItemCollection oItemscontrasal = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlContraBusinessUnit.SelectedItem.Text);

                if (oItemscontrasal != null && oItemscontrasal.Count > 0)
                {
                    DataTable dtContraLocation = oItemscontrasal.GetDataTable().DefaultView.ToTable(true, "Location");
                    ddlContraSiteLocation.DataSource = dtContraLocation;
                    ddlContraSiteLocation.DataValueField = "Location";
                    ddlContraSiteLocation.DataTextField = "Location";
                    ddlContraSiteLocation.DataBind();

                    ddlContraSiteLocation.SelectedValue = Convert.ToString(ListItems["SiteLocation"]);
                }



                if (ListItems["ReportsTo"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ReportsTo"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    ReportsToContractorPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }
                txtContraCostCentre.Text = Convert.ToString(ListItems["CostCenter"]);
                txtContractRate.Text = Convert.ToString(ListItems["ContractRate"]);
               
                /*TaxonomyFieldValue value3 = ListItems[PstDtlMetaField4] as TaxonomyFieldValue;
                if (value3 != null)
                    ddlContraTypeofPosition.SelectedValue = value3.TermGuid;*/

                ddlContraTypeofPosition.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField4]);

                ContraStartDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedStartDate"]).ToString("dd/MM/yyyy"));
                ContraEndDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedEndDate"]).ToString("dd/MM/yyyy"));
            }
        }

        private void GetContractorRoleStatementDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("ContractRoleStatement", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                txtContraRoleStatement.Text = Convert.ToString(ListItems["RoleStatement"]);
            }
        }

        private void GetExpatPositionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            string PstDtlMetaField1 = "BusinessUnit";
            //string PstDtlMetaField2 = "WorkArea";
            //string PstDtlMetaField3 = "SiteLocation";
            string PstDtlMetaField4 = "PositionType";
            SPListItemCollection PositionDetailscollecItems = GetListData("PositionDetails", strRefno);
            foreach (SPListItem ListItems in PositionDetailscollecItems)
            {
                txtExpatPositionTitle.Text = Convert.ToString(ListItems["PositionTitle"]);

               /* TaxonomyFieldValue value = ListItems[PstDtlMetaField1] as TaxonomyFieldValue;
                if (value != null)
                    ddlExpatBusinessUnit.SelectedValue = value.TermGuid;*/

                ddlExpatBusinessUnit.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField1]);

                SPListItemCollection oItemsExpat = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlExpatBusinessUnit.SelectedItem.Text);
                if (oItemsExpat != null && oItemsExpat.Count > 0)
                {
                    DataTable dtExpatWorkArea = oItemsExpat.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    ddlExpatWorkArea.DataSource = dtExpatWorkArea;
                    ddlExpatWorkArea.DataValueField = "WorkArea";
                    ddlExpatWorkArea.DataTextField = "WorkArea";
                    ddlExpatWorkArea.DataBind();

                    ddlExpatWorkArea.SelectedValue = Convert.ToString(ListItems["WorkArea"]);
                }


                SPListItemCollection oItemsexpatloc = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlExpatBusinessUnit.SelectedItem.Text);

                if (oItemsexpatloc != null && oItemsexpatloc.Count > 0)
                {
                    DataTable dtExpatLocation = oItemsexpatloc.GetDataTable().DefaultView.ToTable(true, "Location");

                    ddlExpatSiteLocation.DataSource = dtExpatLocation;
                    ddlExpatSiteLocation.DataValueField = "Location";
                    ddlExpatSiteLocation.DataTextField = "Location";
                    ddlExpatSiteLocation.DataBind();
                    ddlExpatSiteLocation.SelectedValue = Convert.ToString(ListItems["SiteLocation"]);
                }



                if (ListItems["ReportsTo"] != null)
                {
                    string strpplpicker = string.Empty;
                    SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(ListItems["ReportsTo"].ToString());
                    for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                    {
                        strpplpicker = strpplpicker + workers[coworker] + ",";
                    }
                    ReportsToExpatPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                }
                txtexpatCostCentre.Text = Convert.ToString(ListItems["CostCenter"]);
               
                /*TaxonomyFieldValue value3 = ListItems[PstDtlMetaField4] as TaxonomyFieldValue;
                if (value3 != null)
                    ddlExpatTypeofPosition.SelectedValue = value3.TermGuid;*/

                ddlExpatTypeofPosition.SelectedValue = Convert.ToString(ListItems[PstDtlMetaField4]);
                ExpatStartDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedStartDate"]).ToString("dd/MM/yyyy"));
                ExpatEndDateTimeControl.SelectedDate = Convert.ToDateTime(Convert.ToDateTime(ListItems["ProposedEndDate"]).ToString("dd/MM/yyyy"));
            }
        }

        private void GetExpatRemunerattionDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("RemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlExpatGrade.SelectedValue = Convert.ToString(ListItems["Grade"]);
                txtExpatFAR.Text = Convert.ToString(ListItems["FAR"]);
                //txtExpatSTI.Text = Convert.ToString(ListItems["STI"]);
                ddlExpatSTI.SelectedValue = Convert.ToString(ListItems["STI"]);

                /*if (string.Equals(Convert.ToString(ListItems["Utilities"]), "Yes"))
                    ddlExpatUtilities.SelectedValue = "Yes";

                else if (string.Equals(Convert.ToString(ListItems["Utilities"]), "No"))
                    ddlExpatUtilities.SelectedValue = "No";

                txtExpatRelocation.Text = Convert.ToString(ListItems["Relocation"]);*/
                ddlExpatVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtExpatIfother.Text = Convert.ToString(ListItems["OtherVehicleText"]);
            }
        }

        //private void GetSuccessfulApplicantDetails(string strRefno)
        //{
        //    if (strRefno == "")
        //        strRefno = lblReferenceNo.Text;
        //    SPListItemCollection SuccessfulApplicationcollecItems = GetListData("SuccessfulApplication", strRefno);
        //    foreach (SPListItem ListItems in SuccessfulApplicationcollecItems)
        //    {
        //        txtSuccessfulApplicantName.Text = Convert.ToString(ListItems["SuccessfulApplicantName"]);
        //        txtPosition.Text = Convert.ToString(ListItems["Position"]);
        //        txtSAPNumber.Text = Convert.ToString(ListItems["SAPNumber"]);
        //        CommencementDateTimeControl.SelectedDate = Convert.ToDateTime(ListItems["CommencementDate"]);
        //    }
        //}

        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection collectionItems  = null;
            //SPList oList = SPContext.Current.Web.Lists[SetListByName];
            string lstURL = HrWebUtility.GetListUrl(SetListByName);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oQuery = new SPQuery();
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                oQuery.RowLimit = 100;
                collectionItems = oList.GetItems(oQuery);
            });
            return collectionItems;
        }

        private bool SetAppToHireGeneralInfoList(bool UpdateTitleOnly, string strStatus)
        {
            //strRefno = "AH" + String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now);
            bool bProceed = true;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text;

            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("AppToHireGeneralInfo", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    if (!UpdateTitleOnly)
                        bProceed = UpdateAppToHireGeneralInfo(listitem, strStatus);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (strRefno == "")
                    {
                        SPWeb web = SPContext.Current.Web;
                        // SPList oList = web.Lists["AppToHireGeneralInfo"];
                        string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPListItem listitem = oList.AddItem();
                        web.AllowUnsafeUpdates = true;
                        listitem.Update();
                        //listitem["Title"] = "Ref No: AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                        lblReferenceNo.Text = "AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                        strRefno = "AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                        listitem["Title"] = strRefno;
                        listitem.Update();
                        web.AllowUnsafeUpdates = false;
                    }
                });
            }
            return bProceed;
        }

        private void SetPositionDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("PositionDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdatePositionDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdatePositionDetailsList(listitem);
                });
            }

        }

        private void SetRemunerationDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("RemunerationDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateRemunerationDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["RemunerationDetails"];
                    string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateRemunerationDetailsList(listitem);
                });
            }
        }

        private void SetWagedPositionDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("PositionDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateWagedPositionDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateWagedPositionDetailsList(listitem);
                });
            }
        }

        private void SetWagedRemunerationDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("RemunerationDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateWagedRemunerationDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    // SPList oList = SPContext.Current.Web.Lists["RemunerationDetails"];
                    string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateWagedRemunerationDetailsList(listitem);
                });
            }
        }

        private void SetContractorPositionDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("PositionDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateContractorPositionDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateContractorPositionDetailsList(listitem);
                });
            }
        }

        private void SetContractorRoleStatementList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("ContractRoleStatement", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateContractorRoleStatementList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    // SPList oList = SPContext.Current.Web.Lists["ContractRoleStatement"];
                    string lstURL = HrWebUtility.GetListUrl("ContractRoleStatement");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateContractorRoleStatementList(listitem);
                });
            }
        }

        private void SetExpatPositionDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("PositionDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateExpatPositionDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateExpatPositionDetailsList(listitem);
                });
            }
        }

        private void SetExpatRemunerationDetailsList()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text;
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("RemunerationDetails", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateExpatRemunerationDetailsList(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["RemunerationDetails"];
                    string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateExpatRemunerationDetailsList(listitem);
                });
            }
        }

        //private void SetSuccessfulApplicationList()
        //{
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //        {
        //            if (strRefno == "")
        //                strRefno = lblReferenceNo.Text;
        //            SPListItemCollection collectionItems = SetListData("SuccessfulApplication", strRefno);
        //            if (collectionItems != null && collectionItems.Count > 0)
        //            {
        //                foreach (SPListItem listitem in collectionItems)
        //                {
        //                    UpdateSuccessfulApplicationList(listitem);
        //                }
        //            }
        //            else
        //            {
        //                SPList oList = SPContext.Current.Web.Lists["SuccessfulApplication"];
        //                SPListItem listitem = oList.AddItem();
        //                listitem["Title"] = strRefno;
        //                UpdateSuccessfulApplicationList(listitem);

        //            }
        //        });
        //}

        private bool UpdateAppToHireGeneralInfo(SPListItem listitem, string strStatus)
        {
            bool bProceed = true;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["Title"] = lblReferenceNo.Text;
                //listitem["DateOfRequest"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateofRequest.SelectedDate);
                listitem["DateOfRequest"] = lblDateNow.Text;

                /*TaxonomyField posTypeField = listitem.Fields["PositionType"] as TaxonomyField;
                TaxonomySession oSession = new TaxonomySession(mySite.Site);
                Term oTerm = oSession.GetTerm(new Guid(ddlPositionType.SelectedItem.Value));
                posTypeField.SetFieldValue(listitem, oTerm);

                TaxonomyField posReasonField = listitem.Fields["PositionReason"] as TaxonomyField;
                Term oTerm1 = oSession.GetTerm(new Guid(ddlReasonPositionRqd.SelectedItem.Value));
                posReasonField.SetFieldValue(listitem, oTerm1);*/
                listitem["PositionType"] = ddlPositionType.SelectedValue;
                listitem["PositionReason"] = ddlReasonPositionRqd.SelectedValue;

                listitem["ReplacementFor"] = txtPositionHeldBy.Text;
                if (string.Equals(ddlBudgetPosition.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsBudgetedPosition"] = true;
                else if (string.Equals(ddlBudgetPosition.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsBudgetedPosition"] = false;
                if (string.Equals(ddlStaffingLevel.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsIncreaseInStaffing"] = true;
                else if (string.Equals(ddlStaffingLevel.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsIncreaseInStaffing"] = false;

                SPFieldUserValueCollection ReqdUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = RequiredByPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];                        
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ReqdUserCollection.Add(UserName);
                    }
                }
                listitem["RequiredBy"] = ReqdUserCollection;

                listitem["Comments"] = txtComments.Text;

                /*TaxonomyFieldValue recruitmentProcess = new TaxonomyFieldValue(string.Empty);
                recruitmentProcess.PopulateFromLabelGuidPair(ddlRecruitmentProc.SelectedItem.Value);
                recruitmentProcess.WssId = -1;
                listitem["RecruitmentProcess"] = recruitmentProcess;*/

                /* TaxonomyField recField = listitem.Fields["RecruitmentProcess"] as TaxonomyField;
                 Term oTerm2 = oSession.GetTerm(new Guid(ddlRecruitmentProc.SelectedItem.Value));
                 recField.SetFieldValue(listitem, oTerm2);*/
                listitem["RecruitmentProcess"] = ddlRecruitmentProc.SelectedValue;


                listitem["Details"] = txtDetails.Text;
                listitem["Status"] = strStatus;
                listitem["ApprovalStatus"] = "Approver1";
                if (strStatus == "Pending Approval")
                {
                    listitem["ApprovalStatus"] = GetApproverString(ddlPositionType.SelectedValue);
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
                }
                listitem.Update();
            });
            return bProceed;
        }

        
        private DataTable GetBusinessUnit()
        {
            DataTable dtTable = new DataTable();

            SPList olist = SPContext.Current.Web.Lists["HRWebBusinessUnitWorkarea"];
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<OrderBy><FieldRef Name='Title' Descending='FALSE' /></OrderBy>";
            oQuery.ViewFields = string.Concat(
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='WorkArea' />");
            oQuery.RowLimit = 500;
            SPListItemCollection oItems = olist.GetItems(oQuery);
            if (oItems != null && oItems.Count > 0)
            {
                dtTable = oItems.GetDataTable().DefaultView.ToTable(true, "Title");
            }
            return dtTable;
        }

        private string GetApproverString(string PositionfieldValue)
        {
            string Approver = "HRServices";
            string businessunit = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL);


               SPQuery oquery3 = new SPQuery();
               if (PositionfieldValue == "Salary")
                   businessunit = ddlBusinessUnit.SelectedValue;
               else if (PositionfieldValue == "Waged")
                   businessunit = ddlWagedBusinessUnit.SelectedValue;
               else if (PositionfieldValue == "Contractor")
                   businessunit = ddlContraBusinessUnit.SelectedValue;
               else if (PositionfieldValue == "Expatriate")
                   businessunit = ddlExpatBusinessUnit.SelectedValue;
               if (PositionfieldValue == "Waged")
               {
                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                   oquery3.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                   "</Value></Eq><Eq><FieldRef Name=\'PositionType\' /><Value Type=\"Text\">Waged</Value></Eq></And></Where>";
               }
               else
               {
                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                   oquery3.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                   "</Value></Eq><Eq><FieldRef Name=\'PositionType\' /><Value Type=\"Text\">Salary</Value></Eq></And></Where>";
               }
               oquery3.ViewFields = string.Concat(
                   "<FieldRef Name='Approver1' />",
                   "<FieldRef Name='Approver2' />",
                   "<FieldRef Name='Approver3' />",
                   "<FieldRef Name='Approver4' />",
                   "<FieldRef Name='Approver5' />",
                   "<FieldRef Name='Approver6' />",
                   "<FieldRef Name='Approver7' />",
                   "<FieldRef Name='HRServices' />");
               oquery3.RowLimit = 100;
               SPListItemCollection collitems2 = olist1.GetItems(oquery3);
               if (collitems2.Count > 0)
               {
                   if (Convert.ToString(collitems2[0]["Approver1"]) != "")
                   {
                       Approver = "Approver1";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver1"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver2"]) != "")
                   {
                       Approver = "Approver2";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver2"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver3"]) != "")
                   {
                       Approver = "Approver3";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver3"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver4"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                   {
                       Approver = "Approver4";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver4"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver5"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                   {
                       Approver = "Approver5";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver5"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver6"]) != "")
                   {
                       Approver = "Approver6";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                   }
                   else if (Convert.ToString(collitems2[0]["Approver7"]) != "" && Convert.ToString(collitems2[0]["Approver7"]) != Convert.ToString(collitems2[0]["Approver5"]))
                   {
                       Approver = "Approver7";
                       ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                   }
                   else if (Convert.ToString(collitems2[0]["HRServices"]) != "")
                   {
                       Approver = "HRServices";
                       ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                   }
               }
           });
            return Approver;
        }

        private void UpdatePositionDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["PositionTitle"] = txtPositionTitle.Text;
                listitem["SAPPositionNo"] = txtSAPPositionNo.Text;
                /*TaxonomyFieldValue businessunit = new TaxonomyFieldValue(string.Empty);
                businessunit.PopulateFromLabelGuidPair(ddlBusinessUnit.SelectedItem.Value);
                businessunit.WssId = -1;*/
                listitem["BusinessUnitTermID"] = ddlBusinessUnit.SelectedValue;
                listitem["BusinessUnit"] = ddlBusinessUnit.SelectedValue;

                /*TaxonomyFieldValue workarea = new TaxonomyFieldValue(string.Empty);
                workarea.PopulateFromLabelGuidPair(ddlWorkArea.SelectedItem.Value);
                workarea.WssId = -1;
                listitem["WorkArea"] = workarea;*/
                listitem["WorkArea"] = ddlWorkArea.SelectedValue;


                /*TaxonomyFieldValue sitelocation = new TaxonomyFieldValue(string.Empty);
                sitelocation.PopulateFromLabelGuidPair(ddlSiteLocation.SelectedItem.Value);
                sitelocation.WssId = -1;
                listitem["SiteLocation"] = sitelocation;*/

                listitem["SiteLocation"] = ddlSiteLocation.SelectedValue;


                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtCostCentre.Text;
                /*TaxonomyFieldValue typeofPosition = new TaxonomyFieldValue(string.Empty);
                typeofPosition.PopulateFromLabelGuidPair(ddlTypeOfPosition.SelectedItem.Value);
                typeofPosition.WssId = -1;*/
                listitem["PositionType"] = ddlTypeOfPosition.SelectedValue;


                /*if (!StartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(StartDateTimeControl.SelectedDate);
                if (!EndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(EndDateTimeControl.SelectedDate);*/

                if (!StartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = StartDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                if (!EndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = EndDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                listitem.Update();
            });
        }

        private void UpdateRemunerationDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["Grade"] = ddlGrade.SelectedValue;
                listitem["FAR"] = txtFAR.Text;
                listitem["STI"] = ddlSTI.SelectedValue;
                listitem["Vehicle"] = ddlVehicle.SelectedValue;
                listitem["OtherVehicleText"] = txtIfOthers.Text;
                listitem.Update();
            });
        }

        private void UpdateWagedPositionDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["PositionTitle"] = txtWagedPositionTitle.Text;
                listitem["SAPPositionNo"] = txtWagedSAPPositionNo.Text;

                /*TaxonomyFieldValue businessunit = new TaxonomyFieldValue(string.Empty);
                businessunit.PopulateFromLabelGuidPair(ddlWagedBusinessUnit.SelectedItem.Value);
                businessunit.WssId = -1;*/
                listitem["BusinessUnit"] = ddlWagedBusinessUnit.SelectedValue;
                listitem["BusinessUnitTermID"] = ddlWagedBusinessUnit.SelectedValue;



                /*TaxonomyFieldValue workarea = new TaxonomyFieldValue(string.Empty);
                workarea.PopulateFromLabelGuidPair(ddlWagedWorkArea.SelectedItem.Value);
                workarea.WssId = -1;
                listitem["WorkArea"] = workarea;
                TaxonomyFieldValue sitelocation = new TaxonomyFieldValue(string.Empty);
                sitelocation.PopulateFromLabelGuidPair(ddlWagedSiteLocation.SelectedItem.Value);
                sitelocation.WssId = -1;
                listitem["SiteLocation"] = sitelocation;*/
                listitem["WorkArea"] = ddlWagedWorkArea.SelectedValue;
                listitem["SiteLocation"] = ddlWagedSiteLocation.SelectedValue;

                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToWagedPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtWagedCostCentre.Text;

                /*TaxonomyFieldValue typeofPosition = new TaxonomyFieldValue(string.Empty);
                typeofPosition.PopulateFromLabelGuidPair(ddlWagedTypOfPosition.SelectedItem.Value);
                typeofPosition.WssId = -1;*/
                listitem["PositionType"] = ddlWagedTypOfPosition.SelectedValue;

                /*if (!WagedStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(WagedStartDateTimeControl.SelectedDate);
                if (!WagedEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(WagedEndDateTimeControl.SelectedDate);*/

                if (!WagedStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = WagedStartDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                if (!WagedEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = WagedEndDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                listitem.Update();
            });
        }

        private void UpdateWagedRemunerationDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["Level"] = ddlWagedLevel.SelectedValue;
                /*TaxonomyFieldValue ShiftRotation = new TaxonomyFieldValue(string.Empty);
                ShiftRotation.PopulateFromLabelGuidPair(ddlWagedShiftRotation.SelectedItem.Value);
                ShiftRotation.WssId = -1;*/
                listitem["ShiftRotation"] = ddlWagedShiftRotation.SelectedValue;
                listitem["Vehicle"] = ddlWagedVehicle.SelectedValue;
                listitem["OtherVehicleText"] = txtWagedIfOther.Text;
                listitem.Update();
            });
        }

        private void UpdateContractorPositionDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["Role"] = txtContraRole.Text;

                /*TaxonomyFieldValue businessunit = new TaxonomyFieldValue(string.Empty);
                businessunit.PopulateFromLabelGuidPair(ddlContraBusinessUnit.SelectedItem.Value);
                businessunit.WssId = -1;*/
                listitem["BusinessUnit"] = ddlContraBusinessUnit.SelectedValue;
                listitem["BusinessUnitTermID"] = ddlContraBusinessUnit.SelectedValue;


                /*TaxonomyFieldValue workarea = new TaxonomyFieldValue(string.Empty);
                workarea.PopulateFromLabelGuidPair(ddlContraWorkArea.SelectedItem.Value);
                workarea.WssId = -1;
                listitem["WorkArea"] = workarea;
                TaxonomyFieldValue sitelocation = new TaxonomyFieldValue(string.Empty);
                sitelocation.PopulateFromLabelGuidPair(ddlContraSiteLocation.SelectedItem.Value);
                sitelocation.WssId = -1;
                listitem["SiteLocation"] = sitelocation;*/

                listitem["WorkArea"] = ddlContraWorkArea.SelectedValue;
                listitem["SiteLocation"] = ddlContraSiteLocation.SelectedValue;
                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToContractorPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtContraCostCentre.Text;
                listitem["ContractRate"] = txtContractRate.Text;

                /* TaxonomyFieldValue typeofPosition = new TaxonomyFieldValue(string.Empty);
                 typeofPosition.PopulateFromLabelGuidPair(ddlContraTypeofPosition.SelectedItem.Value);
                 typeofPosition.WssId = -1;*/

                listitem["PositionType"] = ddlContraTypeofPosition.SelectedValue;
                /*if (!ContraStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ContraStartDateTimeControl.SelectedDate);
                if (!ContraEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ContraEndDateTimeControl.SelectedDate);*/

                if (!ContraStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = ContraStartDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                if (!ContraEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = ContraEndDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                listitem.Update();
            });
        }

        private void UpdateContractorRoleStatementList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["RoleStatement"] = txtContraRoleStatement.Text;
                listitem.Update();
            });
        }

        private void UpdateExpatPositionDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWeb mySite = SPContext.Current.Web;
                listitem["PositionTitle"] = txtExpatPositionTitle.Text;
                /*TaxonomyFieldValue businessunit = new TaxonomyFieldValue(string.Empty);
                businessunit.PopulateFromLabelGuidPair(ddlExpatBusinessUnit.SelectedItem.Value);
                businessunit.WssId = -1;*/
                listitem["BusinessUnit"] = ddlExpatBusinessUnit.SelectedValue;

                listitem["BusinessUnitTermID"] = ddlExpatBusinessUnit.SelectedValue;

                /*TaxonomyFieldValue workarea = new TaxonomyFieldValue(string.Empty);
                workarea.PopulateFromLabelGuidPair(ddlExpatWorkArea.SelectedItem.Value);
                workarea.WssId = -1;
                listitem["WorkArea"] = workarea;
                TaxonomyFieldValue sitelocation = new TaxonomyFieldValue(string.Empty);
                sitelocation.PopulateFromLabelGuidPair(ddlExpatSiteLocation.SelectedItem.Value);
                sitelocation.WssId = -1;
                listitem["SiteLocation"] = sitelocation;*/

                listitem["WorkArea"] = ddlExpatWorkArea.SelectedValue;
                listitem["SiteLocation"] = ddlExpatSiteLocation.SelectedValue;

                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToExpatPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = mySite.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtexpatCostCentre.Text;
                /* TaxonomyFieldValue typeofPosition = new TaxonomyFieldValue(string.Empty);
                 typeofPosition.PopulateFromLabelGuidPair(ddlExpatTypeofPosition.SelectedItem.Value);
                 typeofPosition.WssId = -1;*/
                listitem["PositionType"] = ddlExpatTypeofPosition.SelectedValue;
                /*if (!ExpatStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ExpatStartDateTimeControl.SelectedDate);
                if (!ExpatEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ExpatEndDateTimeControl.SelectedDate);*/

                if (!ExpatStartDateTimeControl.IsDateEmpty)
                    listitem["ProposedStartDate"] = ExpatStartDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                if (!ExpatEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = ExpatEndDateTimeControl.SelectedDate.ToString("dd/MM/yyyy");
                listitem.Update();
            });
        }

        private void UpdateExpatRemunerationDetailsList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["Grade"] = ddlExpatGrade.SelectedValue;
                listitem["FAR"] = txtExpatFAR.Text;
                //listitem["STI"] = txtExpatSTI.Text;
                listitem["STI"] = ddlExpatSTI.SelectedValue;
                /*if (string.Equals(ddlExpatUtilities.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["Utilities"] = "Yes";
                else if (string.Equals(ddlExpatUtilities.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["Utilities"] = "No";
                listitem["Relocation"] = txtExpatRelocation.Text;*/
                listitem["Vehicle"] = ddlExpatVehicle.SelectedValue;
                listitem["OtherVehicleText"] = txtExpatIfother.Text;
                listitem.Update();
            });
        }

        //private void UpdateSuccessfulApplicationList(SPListItem listitem)
        //{
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //        {
        //            listitem["SuccessfulApplicantName"] = txtSuccessfulApplicantName.Text;
        //            listitem["Position"] = txtPosition.Text;
        //            listitem["SAPNumber"] = txtSAPNumber.Text;
        //            listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(CommencementDateTimeControl.SelectedDate);
        //            listitem.Update();
        //        });
        //}

        private bool ValidateSummary()
        {
            bool bresult = true;
            /*if (DateofRequest.IsDateEmpty)
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlPositionType.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlReasonPositionRqd.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtPositionHeldBy.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlBudgetPosition.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlStaffingLevel.SelectedValue))
                bresult = false;

            if (RequiredByPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            /* if (string.IsNullOrEmpty(txtComments.Text.Trim()))
                 bresult = false;*/

            if (string.IsNullOrEmpty(ddlRecruitmentProc.SelectedValue))
                bresult = false;

            /*if (string.IsNullOrEmpty(txtDetails.Text.Trim()))
                bresult = false;*/

            if (string.IsNullOrEmpty(txtPositionTitle.Text.Trim()))
                bresult = false;

            /*  if (string.IsNullOrEmpty(txtSAPPositionNo.Text.Trim()))
                  bresult = false;*/

            if (string.IsNullOrEmpty(ddlBusinessUnit.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(txtCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlTypeOfPosition.SelectedValue))
                bresult = false;

            if (StartDateTimeControl.IsDateEmpty)
                bresult = false;


            if (string.Equals(ddlTypeOfPosition.SelectedItem.Text, "Fixed Term", StringComparison.OrdinalIgnoreCase) && EndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.IsNullOrEmpty(ddlGrade.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtFAR.Text.Trim()))
                bresult = false;

            /* if (string.IsNullOrEmpty(ddlSTI.SelectedValue))
                 bresult = false;

             if (string.IsNullOrEmpty(ddlVehicle.SelectedValue))
                 bresult = false;


             if (string.IsNullOrEmpty(txtIfOthers.Text.Trim()))
                 bresult = false;*/

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            return bresult;

        }
        
        protected void btnSalarySave_Click(object sender, EventArgs e)
        {
            try
            {

                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Draft");
                //if (bProceed)
                //{
                SetPositionDetailsList();
                SetRemunerationDetailsList();
                //UploadAttachments("Salary");
                //lblError.Text = "Your application has been temporarily saved. You have to submit the application for further processing.";
                Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Draft");

                //}
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnSalarySave_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }

        }

        /*private void UploadAttachments(string PositionType)
        {
            //HtmlTable tblAttachment = (HtmlTable)MyCustomControl.FindControl("tblAttachment");
            System.Web.UI.WebControls.Table tblAttachment = (System.Web.UI.WebControls.Table)MyCustomControl.FindControl("tblAttachment");

            if (tblAttachment != null)
            {
                for (int cnt = 1; cnt < tblAttachment.Rows.Count; cnt++)
                {
                    //SPSecurity.RunWithElevatedPrivileges(delegate()
                    //{
                        SPWeb web = SPContext.Current.Web;
                        SPDocumentLibrary oLibrary = web.Lists["JobDetails"] as SPDocumentLibrary;

                        FileStream fileStream = File.OpenRead(tblAttachment.Rows[cnt].Cells[4].Text);


                        string fileUrl = oLibrary.RootFolder.Url + "/" + Convert.ToString(tblAttachment.Rows[cnt].Cells[2].Text);
                    
                        bool IsOverwriteFile = true;
                        SPFile file = oLibrary.RootFolder.Files.Add(fileUrl, fileStream, IsOverwriteFile);

                        SPListItem item = file.Item;
                        item["Title"] = strRefno;
                        item.Update();
                        file.Update();
                    //});
                }

            }


        }*/

        protected void btnWagedSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Draft");
                //if (bProceed)
                //{
                SetWagedPositionDetailsList();
                SetWagedRemunerationDetailsList();
                Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Draft");
                //}
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnWagedSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnContractorSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Draft");
                //if (bProceed)
                //{
                SetContractorPositionDetailsList();
                SetContractorRoleStatementList();
                Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Draft");
                //}
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnContractorSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void btnExpatSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Draft");
                //if (bProceed)
                //{
                SetExpatPositionDetailsList();
                SetExpatRemunerationDetailsList();
                Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Draft");
                //}
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnExpatSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void ClearControls()
        {
            //DateofRequest.ClearSelection();

        }

        protected void btnSalarySubmit_Click(object sender, EventArgs e)
        {
            try
            {

                // System.Web.UI.Control asptable1 = this.Parent.FindControl("tblAttachment");
                if (ValidateSummary())
                {

                    string refno = lblReferenceNo.Text;
                    bool bProceed = SetAppToHireGeneralInfoList(false, "Pending Approval");
                    if (bProceed)
                    {
                        SetPositionDetailsList();
                        SetRemunerationDetailsList();
                        SendEmail();
                        //lblError.Text = "Your application has been submitted and sent for further processing.";
                        Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Submit");
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the mandatory fields";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnSalarySubmitted_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private bool ValidateWages()
        {
            bool bresult = true;
            /*if (DateofRequest.IsDateEmpty)
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlPositionType.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlReasonPositionRqd.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtPositionHeldBy.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlBudgetPosition.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlStaffingLevel.SelectedValue))
                bresult = false;

            if (RequiredByPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            /* if (string.IsNullOrEmpty(txtComments.Text.Trim()))
                 bresult = false;*/

            if (string.IsNullOrEmpty(ddlRecruitmentProc.SelectedValue))
                bresult = false;

            /*if (string.IsNullOrEmpty(txtDetails.Text.Trim()))
                bresult = false;*/





            if (string.IsNullOrEmpty(txtWagedPositionTitle.Text.Trim()))
                bresult = false;

            /*if (string.IsNullOrEmpty(txtWagedSAPPositionNo.Text.Trim()))
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlWagedBusinessUnit.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToWagedPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedTypOfPosition.SelectedValue))
                bresult = false;

            if (WagedStartDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.Equals(ddlWagedTypOfPosition.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && WagedEndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedShiftRotation.SelectedValue))
                bresult = false;

            /* if (string.IsNullOrEmpty(ddlWagedVehicle.SelectedValue))
                 bresult = false;

             if (string.IsNullOrEmpty(txtWagedIfOther.Text.Trim()))
                 bresult = false;*/

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            return bresult;
        }

        protected void btnWagedSubmit_Click(object sender, EventArgs e)
        {
            if (ValidateWages())
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Pending Approval");
                if (bProceed)
                {
                    SetWagedPositionDetailsList();
                    SetWagedRemunerationDetailsList();
                    SendEmail();
                    Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Submit");
                }
            }
            else
            {
                lblError.Text = "Please fill the following mandatory fields";
            }
        }
        
        private bool ValidateContract()
        {
            bool bresult = true;
            /*if (DateofRequest.IsDateEmpty)
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlPositionType.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlReasonPositionRqd.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtPositionHeldBy.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlBudgetPosition.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlStaffingLevel.SelectedValue))
                bresult = false;

            if (RequiredByPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            /* if (string.IsNullOrEmpty(txtComments.Text.Trim()))
                 bresult = false;*/

            if (string.IsNullOrEmpty(ddlRecruitmentProc.SelectedValue))
                bresult = false;

            /* if (string.IsNullOrEmpty(txtDetails.Text.Trim()))
                 bresult = false;*/





            if (string.IsNullOrEmpty(txtContraRole.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraBusinessUnit.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraSiteLocation.SelectedValue))
                bresult = false;


            if (ReportsToContractorPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(txtContraCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraTypeofPosition.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(txtContractRate.Text.Trim()))
                bresult = false;


            if (ContraStartDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.Equals(ddlContraTypeofPosition.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && ContraEndDateTimeControl.IsDateEmpty)
                bresult = false;


            if (string.IsNullOrEmpty(txtContraRoleStatement.Text.Trim()))
                bresult = false;

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            return bresult;
        }
        
        protected void btnContractorSubmit_Click(object sender, EventArgs e)
        {
            if (ValidateContract())
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Pending Approval");
                if (bProceed)
                {
                    SetContractorPositionDetailsList();
                    SetContractorRoleStatementList();
                    SendEmail();
                    Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Submit");
                }
            }
            else
            {
                lblError.Text = "Please fill the following mandatory fields";
            }
        }

        private bool ValidateExpat()
        {
            bool bresult = true;
            /*if (DateofRequest.IsDateEmpty)
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlPositionType.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlReasonPositionRqd.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtPositionHeldBy.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlBudgetPosition.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlStaffingLevel.SelectedValue))
                bresult = false;

            if (RequiredByPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            /*if (string.IsNullOrEmpty(txtComments.Text.Trim()))
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlRecruitmentProc.SelectedValue))
                bresult = false;

            /* if (string.IsNullOrEmpty(txtDetails.Text.Trim()))
                 bresult = false;*/





            if (string.IsNullOrEmpty(txtExpatPositionTitle.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatBusinessUnit.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatSiteLocation.SelectedValue))
                bresult = false;


            if (ReportsToExpatPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(txtexpatCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatTypeofPosition.SelectedValue))
                bresult = false;

            if (ExpatStartDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.Equals(ddlExpatTypeofPosition.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && ExpatEndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatGrade.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtExpatFAR.Text.Trim()))
                bresult = false;

            /* if (string.IsNullOrEmpty(ddlExpatSTI.SelectedValue))
                 bresult = false;*/

           /* if (string.IsNullOrEmpty(ddlExpatUtilities.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(txtExpatRelocation.Text.Trim()))
                bresult = false;*/

            /* if (string.IsNullOrEmpty(ddlExpatVehicle.Text.Trim()))
                 bresult = false;

             if (string.IsNullOrEmpty(txtExpatIfother.Text.Trim()))
                 bresult = false;*/

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            return bresult;
        }

        protected void btnExpatSubmit_Click(object sender, EventArgs e)
        {
            if (ValidateExpat())
            {
                string refno = lblReferenceNo.Text;
                bool bProceed = SetAppToHireGeneralInfoList(false, "Pending Approval");
                if (bProceed)
                {
                    SetExpatPositionDetailsList();
                    SetExpatRemunerationDetailsList();
                    SendEmail();
                    Server.Transfer("/people/Pages/HRWeb/AppToHireStatus.aspx?refno=" + refno + "&flow=Submit");
                }
            }
            else
            {
                lblError.Text = "Please fill the following mandatory fields";
            }
        }

        private void SendEmail()
        {
            string strRefNo = lblReferenceNo.Text;
            if (Convert.ToString(ViewState["ApproverEmail"]) != "")
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
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
                                   "<FieldRef Name='ApprovalMessage' />");
                    oQuery.RowLimit = 50;
                    SPListItemCollection collListItems = lst.GetItems(oQuery);

                    foreach (SPListItem itm in collListItems)
                    {
                        if (Convert.ToString(itm["FormType"]) == "AppToHire")
                        {
                            //send email
                            string strFrom = "";
                            string strTo = "";
                            string strSubject = "";
                            string strMessage = "";


                            SmtpClient smtpClient = new SmtpClient();
                            smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                            smtpClient.Port = 25;
                            //smtpClient.Host = "smtp.gmail.com";
                            string url = site.Url + "/pages/hrweb/apptohirereview.aspx?refno=" + strRefno;
                            strFrom = Convert.ToString(itm["Title"]);

                            strTo = Convert.ToString(ViewState["ApproverEmail"]);

                            string[] tmparr = strTo.Split('|');
                            strTo = tmparr[tmparr.Length - 1];
                            if (strTo.Contains("#"))
                                strTo = strTo.Split('#')[1];

                            string positiontitle = string.Empty;

                            if (ddlPositionType.SelectedItem.Text == "Salary")
                            {
                                positiontitle = txtPositionTitle.Text.Trim();
                            }
                            else if (ddlPositionType.SelectedItem.Text == "Waged")
                            {
                                positiontitle = txtWagedPositionTitle.Text.Trim();
                            }
                            else if (ddlPositionType.SelectedItem.Text == "Contractor")
                            {
                                positiontitle = txtContraRole.Text.Trim();
                            }
                            else if (ddlPositionType.SelectedItem.Text == "Expatriate")
                            {
                                positiontitle = txtExpatPositionTitle.Text.Trim();
                            }

                            if (strTo.ToLower() == "hrservices")
                            {
                                string to = string.Empty;

                                using (SPSite newSite = new SPSite(site.ID))
                                {
                                    /*using (SPWeb newWeb = newSite.OpenWeb(web.ID))
                                    {
                                        SPGroup group = newWeb.Groups["HR Services"];
                                        foreach (SPUser user in group.Users)
                                        {
                                            to += ";" + user.Email;
                                        }
                                        to = to.TrimStart(';');
                                        strTo = to;
                                    }*/
                                    strTo = HrWebUtility.GetDistributionEmail("HR Services");
                                }

                                strMessage = Convert.ToString(itm["HRManagerApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", positiontitle);

                            }
                            else
                            {
                                strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", positiontitle);
                            }
                            strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", positiontitle);


                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(strFrom,"HR Forms - SunConnect");
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
                    oItem["FormType"] = "AppToHire";
                    oItem.Update();
                //}
            });
        }

        private SPListItemCollection GetBusinessUnitWAandLoc(string strLstName, string strValue)
        {
            string lstURL = HrWebUtility.GetListUrl(strLstName);
            SPList oList = null;
            SPQuery oQuery = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
               oQuery = new SPQuery();
               // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
               oQuery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + strValue + "</Value></Eq></Where>";
               oQuery.RowLimit = 500;
           });
            return oList.GetItems(oQuery);
        }
        
        private void AddBusinessUnitWAandLoc()
        {
            SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlBusinessUnit.SelectedValue);
            ddlWorkArea.Items.Clear();
            if (oItems != null && oItems.Count > 0)
            {
                DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                ddlWorkArea.DataSource = dtWorkArea;
                ddlWorkArea.DataValueField = "WorkArea";
                ddlWorkArea.DataTextField = "WorkArea";
                ddlWorkArea.DataBind();
            }

            SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlBusinessUnit.SelectedValue);
            ddlSiteLocation.Items.Clear();
            if (oItems1 != null && oItems1.Count > 0)
            {
                DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                dtLocation.DefaultView.Sort = "Location ASC";
                ddlSiteLocation.DataSource = dtLocation;
                ddlSiteLocation.DataValueField = "Location";
                ddlSiteLocation.DataTextField = "Location";
                ddlSiteLocation.DataBind();


            }

            SPListItemCollection oItemsddlWagedBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlWagedBusinessUnit.SelectedValue);
            ddlWagedWorkArea.Items.Clear();
            if (oItemsddlWagedBusinessUnit != null && oItemsddlWagedBusinessUnit.Count > 0)
            {
                DataTable dtWorkArea = oItemsddlWagedBusinessUnit.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                ddlWagedWorkArea.DataSource = dtWorkArea;
                ddlWagedWorkArea.DataValueField = "WorkArea";
                ddlWagedWorkArea.DataTextField = "WorkArea";
                ddlWagedWorkArea.DataBind();

            }

            SPListItemCollection oItems1ddlWagedBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlWagedBusinessUnit.SelectedValue);
            ddlWagedSiteLocation.Items.Clear();
            if (oItems1ddlWagedBusinessUnit != null && oItems1ddlWagedBusinessUnit.Count > 0)
            {
                DataTable dtLocation = oItems1ddlWagedBusinessUnit.GetDataTable().DefaultView.ToTable(true, "Location");
                dtLocation.DefaultView.Sort = "Location ASC";
                ddlWagedSiteLocation.DataSource = dtLocation;
                ddlWagedSiteLocation.DataValueField = "Location";
                ddlWagedSiteLocation.DataTextField = "Location";
                ddlWagedSiteLocation.DataBind();
            }

            SPListItemCollection oItemsddlContraBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlContraBusinessUnit.SelectedValue);
            ddlContraWorkArea.Items.Clear();
            if (oItemsddlContraBusinessUnit != null && oItemsddlContraBusinessUnit.Count > 0)
            {
                DataTable dtWorkArea = oItemsddlContraBusinessUnit.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                ddlContraWorkArea.DataSource = dtWorkArea;
                ddlContraWorkArea.DataValueField = "WorkArea";
                ddlContraWorkArea.DataTextField = "WorkArea";
                ddlContraWorkArea.DataBind();
            }

            SPListItemCollection oItems1ddlContraBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlContraBusinessUnit.SelectedValue);
            ddlContraSiteLocation.Items.Clear();
            if (oItems1ddlContraBusinessUnit != null && oItems1ddlContraBusinessUnit.Count > 0)
            {
                DataTable dtLocation = oItems1ddlContraBusinessUnit.GetDataTable().DefaultView.ToTable(true, "Location");
                dtLocation.DefaultView.Sort = "Location ASC";
                ddlContraSiteLocation.DataSource = dtLocation;
                ddlContraSiteLocation.DataValueField = "Location";
                ddlContraSiteLocation.DataTextField = "Location";
                ddlContraSiteLocation.DataBind();
            }

            SPListItemCollection oItemsddlExpatBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlExpatBusinessUnit.SelectedValue);
            ddlExpatWorkArea.Items.Clear();
            if (oItemsddlExpatBusinessUnit != null && oItemsddlExpatBusinessUnit.Count > 0)
            {
                DataTable dtWorkArea = oItemsddlExpatBusinessUnit.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                ddlExpatWorkArea.DataSource = dtWorkArea;
                ddlExpatWorkArea.DataValueField = "WorkArea";
                ddlExpatWorkArea.DataTextField = "WorkArea";
                ddlExpatWorkArea.DataBind();
            }

            SPListItemCollection oItems1ddlExpatBusinessUnit = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlExpatBusinessUnit.SelectedValue);
            ddlExpatSiteLocation.Items.Clear();
            if (oItems1ddlExpatBusinessUnit != null && oItems1ddlExpatBusinessUnit.Count > 0)
            {
                DataTable dtLocation = oItems1ddlExpatBusinessUnit.GetDataTable().DefaultView.ToTable(true, "Location");
                dtLocation.DefaultView.Sort = "Location ASC";
                ddlExpatSiteLocation.DataSource = dtLocation;
                ddlExpatSiteLocation.DataValueField = "Location";
                ddlExpatSiteLocation.DataTextField = "Location";
                ddlExpatSiteLocation.DataBind();
            }

        }
        
        protected void ddlBusinessUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList ddlBusinessUnit = (DropDownList)sender;
                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlBusinessUnit.SelectedValue);
                ddlWorkArea.Items.Clear();
                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                    ddlWorkArea.DataSource = dtWorkArea;
                    ddlWorkArea.DataValueField = "WorkArea";
                    ddlWorkArea.DataTextField = "WorkArea";
                    ddlWorkArea.DataBind();
                }

                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlBusinessUnit.SelectedValue);
                ddlSiteLocation.Items.Clear();
                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    ddlSiteLocation.DataSource = dtLocation;
                    ddlSiteLocation.DataValueField = "Location";
                    ddlSiteLocation.DataTextField = "Location";
                    ddlSiteLocation.DataBind();


                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.ddlBusinessUnit_SelectedIndexChanged", ex.Message);
                lblError.Text ="Unexpected error has occured. Please contact IT team.";
                //lblError.Text = ex.Message;
            }


        }

        protected void ddlWagedBusinessUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList ddlBusinessUnit = (DropDownList)sender;
                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlWagedBusinessUnit.SelectedValue);
                ddlWagedWorkArea.Items.Clear();
                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                    ddlWagedWorkArea.DataSource = dtWorkArea;
                    ddlWagedWorkArea.DataValueField = "WorkArea";
                    ddlWagedWorkArea.DataTextField = "WorkArea";
                    ddlWagedWorkArea.DataBind();

                }

                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlWagedBusinessUnit.SelectedValue);
                ddlWagedSiteLocation.Items.Clear();
                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    ddlWagedSiteLocation.DataSource = dtLocation;
                    ddlWagedSiteLocation.DataValueField = "Location";
                    ddlWagedSiteLocation.DataTextField = "Location";
                    ddlWagedSiteLocation.DataBind();
                }


            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.ddlWagedBusinessUnit_SelectedIndexChanged", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }

        }

        protected void ddlContraBusinessUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList ddlBusinessUnit = (DropDownList)sender;

                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlContraBusinessUnit.SelectedValue);
                ddlContraWorkArea.Items.Clear();
                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                    ddlContraWorkArea.DataSource = dtWorkArea;
                    ddlContraWorkArea.DataValueField = "WorkArea";
                    ddlContraWorkArea.DataTextField = "WorkArea";
                    ddlContraWorkArea.DataBind();
                }

                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlContraBusinessUnit.SelectedValue);
                ddlContraSiteLocation.Items.Clear();
                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    ddlContraSiteLocation.DataSource = dtLocation;
                    ddlContraSiteLocation.DataValueField = "Location";
                    ddlContraSiteLocation.DataTextField = "Location";
                    ddlContraSiteLocation.DataBind();
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.ddlContraBusinessUnit_SelectedIndexChanged", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void ddlExpatBusinessUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList ddlBusinessUnit = (DropDownList)sender;

                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlExpatBusinessUnit.SelectedValue);
                ddlExpatWorkArea.Items.Clear();
                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                    ddlExpatWorkArea.DataSource = dtWorkArea;
                    ddlExpatWorkArea.DataValueField = "WorkArea";
                    ddlExpatWorkArea.DataTextField = "WorkArea";
                    ddlExpatWorkArea.DataBind();
                }

                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlExpatBusinessUnit.SelectedValue);
                ddlExpatSiteLocation.Items.Clear();
                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    ddlExpatSiteLocation.DataSource = dtLocation;
                    ddlExpatSiteLocation.DataValueField = "Location";
                    ddlExpatSiteLocation.DataTextField = "Location";
                    ddlExpatSiteLocation.DataBind();
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.ddlExpatBusinessUnit_SelectedIndexChanged", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void GetCommentHistory(string strRefno)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("UserName", typeof(string)));
            dt.Columns.Add(new DataColumn("Comments", typeof(string)));


            string lstURL = HrWebUtility.GetListUrl("AppToHireCommentsHistory");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                oquery.RowLimit = 1000;
                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strModified = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy H:mm:ss");
                    string level = Convert.ToString(listitem["ApproverStep"]);
                    if (level == "Approver1")
                        level = " (Level 1)";
                    else if (level == "Approver2")
                        level = " (Level 2)";
                    else if (level == "Approver3")
                        level = " (Level 3)";
                    else if (level == "Approver4")
                        level = " (Level 4)";
                    else if (level == "Approver5")
                        level = " (Level 5)";
                    else if (level == "Approver6")
                        level = " (Level 6)";
                    else if (level == "Approver7")
                        level = " (Level 7)";
                    else if (level == "HRServices")
                        level = " (HR Services)";
                    string strAuthor = Convert.ToString(listitem["ApproverName"]) + level;
                    string strComments = Convert.ToString(listitem["Comment"]);

                    dt.Rows.Add(new string[] { strModified, strAuthor, strComments });
                }

                gdCommentHistory.DataSource = dt;
                gdCommentHistory.DataBind();

                if (gdCommentHistory.Rows.Count == 0)
                    divHistory.Visible = false;
                else
                    divHistory.Visible = true;
            });

        }

        //protected void btnSuccessfulApplicantSave_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SetSuccessfulApplicationList();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.btnSuccessfulApplicantSave_Click", ex.Message);
        //        //lblError.Text ="Unexpected error has occured. Please contact IT team.";
        //        lblError.Text = "Unexpected error has occured. Please contact IT team.";
        //    }
        //}


    }

}