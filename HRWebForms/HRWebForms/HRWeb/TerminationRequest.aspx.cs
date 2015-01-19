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
using System.Net.Mail;
using System.Web.UI.WebControls;

namespace HRWebForms.HRWeb
{
    public partial class TerminationRequest : WebPartPage
    {
        string strRefno = "";
        string UserName = "";

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

                if (!IsPostBack)
                {
                    bool bValid = false;

                    notificationTab.Visible = false;
                    TypeOfLeaveTab.Visible = false;
                    BusinessChecklistTab.Visible = false;
                    ISChecklistTab.Visible = false;
                    TerminationMeetingTab.Visible = false;
                    HRServicesTab.Visible = false;

                    lblDateOFRequest.Text = DateTime.Now.ToString("dd/MM/yyyy");

                    if (Page.Request.QueryString["refno"] != null)
                    {
                        lblReferenceNo.Text = Page.Request.QueryString["refno"];
                        bValid = ValidateApplication();

                        if (bValid)
                        {
                            PopulateTerminationTaxonomy();
                            GetTerminationGeneralInfo();
                            GetTerminationRequest();
                            GetTerminationLeave();
                            AddBusinessUnitWAandLoc();

                            if (IsInitiator() || ValidateHRManager()) 
                            {
                                notificationTab.Visible = true;
                                TypeOfLeaveTab.Visible = true;
                                BusinessChecklistTab.Visible = true;
                                ISChecklistTab.Visible = true;
                                TerminationMeetingTab.Visible = true;
                                btnInitiatorSubmit.Visible = false;
                                btnLeaveSave.Text = "Save & Next";

                                GetBusinessChecklist();
                                GetISChecklist();
                                GetMeeting();
                                AddBusinessUnitWAandLoc();


                            }
                            else
                            {
                                notificationTab.Visible = true;
                                TypeOfLeaveTab.Visible = true;
                                btnInitiatorSubmit.Visible = true;

                                BusinessChecklistTab.Visible = false;
                                ISChecklistTab.Visible = false;
                                TerminationMeetingTab.Visible = false;
                                btnLeaveSave.Text = "Save";
                            }
                        }

                    }
                    else
                    {
                        bValid = true;

                        PopulateTerminationTaxonomy();
                        SetTerminationGeneralInfoList(true, "", "");

                        /*if (IsInitiator())
                        {
                            notificationTab.Visible = true;
                            TypeOfLeaveTab.Visible = true;
                            BusinessChecklistTab.Visible = true;
                            ISChecklistTab.Visible = true;
                            TerminationMeetingTab.Visible = true;
                            btnInitiatorSubmit.Visible = false;
                            btnLeaveSave.Text = "Save & Next";
                        }
                        else
                        {*/
                            notificationTab.Visible = true;
                            TypeOfLeaveTab.Visible = true;
                            btnInitiatorSubmit.Visible = true;
                            BusinessChecklistTab.Visible =false;
                            ISChecklistTab.Visible = false;
                            TerminationMeetingTab.Visible = false;
                            btnLeaveSave.Text = "Save";
                        //}


                    }
                    if (!bValid)
                    {
                        lblTerminationRequest.Text = "The application number passed does not exist or has already been submitted.";

                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.Page_Load", ex.Message);
                lblTerminationRequest.Text = ex.Message;
            }
        }

        private void PopulateTerminationTaxonomy()
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
            //TermStore trmStore = txnSession.TermStores["SunRice_MetaData_Service"];
            GroupCollection groups = trmStore.Groups;

            foreach (Group termGroup in trmStore.Groups)
            {
                switch (termGroup.Name)
                {
                    case "HR Group":
                        drpdwnPositionType.DataSource = AddTerms("Position Type", termGroup);
                        drpdwnPositionType.DataTextField = "Term";
                        drpdwnPositionType.DataValueField = "Term";
                        drpdwnPositionType.DataBind();

                        drpdwnBusinessUnit.DataSource = AddTerms("Business Unit", termGroup);
                        drpdwnBusinessUnit.DataTextField = "Term";
                        drpdwnBusinessUnit.DataValueField = "Term";
                        drpdwnBusinessUnit.DataBind();

                        /*drpdwnWorkArea.DataSource = AddTerms("Work Area", termGroup);
                        drpdwnWorkArea.DataTextField = "Term";
                        drpdwnWorkArea.DataValueField = "TermID";
                        drpdwnWorkArea.DataBind();*/

                        break;

                    case "Location Group":
                        /* drpdwnSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                         drpdwnSiteLocation.DataTextField = "Term";
                         drpdwnSiteLocation.DataValueField = "TermID";
                         drpdwnSiteLocation.DataBind();*/

                        /* drpdwnLeaveSiteLocation.DataSource = AddSubTerms("Office Locations", termGroup, "SunRice");
                         drpdwnLeaveSiteLocation.DataTextField = "Term";
                         drpdwnLeaveSiteLocation.DataValueField = "TermID";
                         drpdwnLeaveSiteLocation.DataBind();*/


                        break;

                    case "Organsiation Group":
                        /* drpdwnBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
                         drpdwnBusinessUnit.DataTextField = "Term";
                         drpdwnBusinessUnit.DataValueField = "Term";
                         drpdwnBusinessUnit.DataBind();*/

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
            //DataRow 
            foreach (Term t in trmSet.Terms)
            {

                //dtTermTable.Rows.Add(t.Name);
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
            //DataRow 
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

        private bool ValidateApplication()
        {
            bool bValid = false;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text.Trim();
            SPListItemCollection collectionItems = null;

            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationGeneralInfo", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    string strStatus = Convert.ToString(listitem["Status"]);
                    string strStatus1 = Convert.ToString(listitem["ApprovalStatus"]);
                    ViewState["ApprovalStatus"] = strStatus1;
                    //if (Convert.ToString(listitem["Status"]) == "Draft" && Convert.ToString(listitem["ApprovalStatus"]) == string.Empty)
                    if (Convert.ToString(listitem["Status"]) == "Draft")
                    {
                        bValid = true;
                        break;
                    }
                    //else if (string.Equals(Convert.ToString(listitem["Status"]), "Pending Approval", StringComparison.OrdinalIgnoreCase) && string.Equals(Convert.ToString(listitem["ApprovalStatus"]), "HRManager", StringComparison.OrdinalIgnoreCase) && ValidateHRManager())
                    else if (string.Equals(Convert.ToString(listitem["Status"]), "Pending Approval", StringComparison.OrdinalIgnoreCase) && string.Equals(Convert.ToString(listitem["ApprovalStatus"]), "HRManager", StringComparison.OrdinalIgnoreCase))
                    {
                        bValid = true;
                        break;
                    }
                }
            }
            return bValid;
        }

        private bool IsInitiator() 
        {
            bool result = false; 

            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);

                    SPQuery oquery = new SPQuery();
                    /* oquery.Query = "<Where><And><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"User\">" + UserName + "</Value></Eq>" +
                                                 "<Contains><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + drpdwnBusinessUnit.SelectedItem.Text + "</Value></Contains>" +
                                             "</And</Where>";*/

                    oquery.Query = "<Where><And><And><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" +
                        lblReferenceNo.Text.Trim() +"</Value></Eq><Eq><FieldRef Name=\'Author\'/><Value Type=\"Text\">" + UserName +
                        "</Value></Eq></And><Eq><FieldRef Name=\'Status\'/><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>";
                    SPListItemCollection collitems = olist1.GetItems(oquery);
                    if (collitems != null && collitems.Count > 0)
                        result = true;
                });
            return result;
        }

        private bool IsPendingApprovalWithHRManager()
        {
            bool bValid = false;
            return bValid;
        }

        private bool ValidateHRManager()
        {
            bool result = false;

            string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);

                SPQuery oquery = new SPQuery();
                /* oquery.Query = "<Where><And><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"User\">" + UserName + "</Value></Eq>" +
                                             "<Contains><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + drpdwnBusinessUnit.SelectedItem.Text + "</Value></Contains>" +
                                         "</And</Where>";*/

                oquery.Query = "<Where><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"Text\">" + UserName + "</Value></Eq></Where>";
                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                {
                    foreach (SPListItem listitem in collitems)
                    {
                        TaxonomyFieldValue txfBusinessUnit = listitem["BusinessUnit"] as TaxonomyFieldValue;

                        lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olistNoti = SPContext.Current.Site.RootWeb.GetList(lstURL1);

                        SPQuery oqueryNoti = new SPQuery();
                        /* oquery.Query = "<Where><And><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"User\">" + UserName + "</Value></Eq>" +
                                                     "<Contains><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + drpdwnBusinessUnit.SelectedItem.Text + "</Value></Contains>" +
                                                 "</And</Where>";*/

                        oqueryNoti.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno.ToUpper() + "</Value></Eq></Where>";
                        SPListItemCollection collitemsNoti = olistNoti.GetItems(oqueryNoti);
                        if (collitemsNoti != null && collitemsNoti.Count > 0)
                        {
                            foreach (SPListItem listitemNoti in collitemsNoti)
                            {
                                //TaxonomyFieldValue txfBusinessUnitNoti = listitemNoti["BusinessUnit"] as TaxonomyFieldValue;
                                string txfBusinessUnitNoti = Convert.ToString(listitemNoti["BusinessUnit"]);
                                if (string.Equals(txfBusinessUnit.Label, txfBusinessUnitNoti, StringComparison.OrdinalIgnoreCase))
                                {
                                    result = true;
                                }

                            }
                        }
                    }
                }
            });
            return result;
        }

        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Trim();
            //SPList oList = SPContext.Current.Web.Lists[SetListByName];
            string lstURL = HrWebUtility.GetListUrl(SetListByName);
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPQuery oQuery = new SPQuery();
                    oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                    collectionItems = oList.GetItems(oQuery);
                });
            return collectionItems;
        }

        private void GetTerminationGeneralInfo()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {
                        lblDateOFRequest.Text = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");

                        /*TaxonomyFieldValue txfBusinessUnit = listitem["PositionType"] as TaxonomyFieldValue;
                        if (txfBusinessUnit.Label != null)
                            drpdwnPositionType.SelectedValue = txfBusinessUnit.TermGuid;*/

                        drpdwnPositionType.SelectedValue = Convert.ToString(listitem["PositionType"]);

                    }
                });
        }

        private void GetTerminationLeave()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationLeave");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {
                        if (string.Equals(Convert.ToString(listitem["IsParentalLeave"]), "True"))
                            drpdwnParentalLeave.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsParentalLeave"]), "False"))
                            drpdwnParentalLeave.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsLeaveWithoutPay"]), "True"))
                            drpdwnLeaveWithoutPay.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsLeaveWithoutPay"]), "False"))
                            drpdwnLeaveWithoutPay.SelectedValue = "No";

                        if (Convert.ToString(listitem["PeriodOfLeaveFrom"]) != "")
                            dtPeriodOfLeaveFrom.SelectedDate = Convert.ToDateTime(listitem["PeriodOfLeaveFrom"]);
                        if (Convert.ToString(listitem["PeriodOfLeaveTo"]) != "")
                            dtPeriodOfLeaveTo.SelectedDate = Convert.ToDateTime(listitem["PeriodOfLeaveTo"]);

                        txtLeaveComments.Text = Convert.ToString(listitem["Comments"]);

                        drpdwnLeaveWithoutPay.SelectedValue = Convert.ToString(listitem["IsLeaveWithoutPay"]);

                    }
                });
        }

        private bool SetTerminationGeneralInfoList(bool UpdateTitleOnly, string strStatus, string strApprovalStatus)
        {
            //strRefno = "AH" + String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now);
            bool bProceed = true;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text.Trim();

            SPListItemCollection collectionItems = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (strRefno != "")
                        collectionItems = SetListData("HrWebTerminationGeneralInfo", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {
                        foreach (SPListItem listitem in collectionItems)
                        {
                            if (!UpdateTitleOnly)
                            {


                                listitem["Title"] = lblReferenceNo.Text.Trim();
                                if (!string.IsNullOrEmpty(lblDateOFRequest.Text.Trim()))
                                    //listitem["DateOfRequest"] = Convert.ToDateTime(lblDateOFRequest.Text.Trim()));
                                    listitem["DateOfRequest"] = lblDateOFRequest.Text.Trim();

                                /* TaxonomyFieldValue PositionfieldValue = new TaxonomyFieldValue(string.Empty);
                                 PositionfieldValue.PopulateFromLabelGuidPair(drpdwnPositionType.SelectedItem.Value);
                                 PositionfieldValue.WssId = -1;
                                 listitem["PositionType"] = PositionfieldValue;*/

                                listitem["PositionType"] = drpdwnPositionType.SelectedItem.Value;

                                listitem["Status"] = strStatus;

                                if (drpdwnMobilePhone.SelectedValue == "Yes")
                                {
                                    listitem["ISAckStatus"] = "Pending";
                                }
                                else
                                {
                                    listitem["ISAckStatus"] = "";
                                }

                                if (strStatus == "Pending Approval" && strApprovalStatus == "HRManager")
                                {
                                    listitem["ApprovalStatus"] = "HRServices";
                                    listitem["Status"] = strStatus;
                                    bProceed = true;
                                    ViewState["CurrentApprover"] = "HRServices";
                                }
                                else if (strStatus == "Pending Approval")
                                {
                                    ViewState["CurrentApprover"] = "HRManager";
                                    listitem["ApprovalStatus"] = GetApproverString();
                                    if (Convert.ToString(ViewState["ApproverEmail"]) != "")
                                    {
                                        listitem["Status"] = strStatus;
                                        bProceed = true;
                                    }
                                    else
                                    {

                                        listitem["Status"] = "Draft";
                                        listitem["ApprovalStatus"] = "";
                                        bProceed = false;
                                        lblTerminationRequest.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                                    }
                                }


                                listitem.Update();



                            }
                        }
                    }
                    else
                    {

                        if (strRefno == "")
                        {
                            SPWeb web = SPContext.Current.Site.RootWeb;

                            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
                            SPSecurity.RunWithElevatedPrivileges(delegate()
                            {
                                SPList oList = web.GetList(lstURL);
                                SPListItem listitem = oList.AddItem();
                                web.AllowUnsafeUpdates = true;
                                listitem.Update();

                                lblReferenceNo.Text = "TN" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                strRefno = "TN" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                listitem["Title"] = strRefno;
                                listitem.Update();
                                web.AllowUnsafeUpdates = false;
                            });
                        }
                    }

                });
            return bProceed;
        }

        private void UpdateOtherApproverStatusGeneralInfo()
        {

            SPListItemCollection collectionItems = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (strRefno != "")
                        collectionItems = SetListData("HrWebTerminationGeneralInfo", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {
                        foreach (SPListItem listitem in collectionItems)
                        {
                            listitem["CreditCardAckStatus"] = "";
                            listitem["ProcurementAckStatus"] = "";
                            listitem["FinanceAckStatus"] = "";
                            listitem["MarketingAckStatus"] = "";
                            listitem["ISAckStatus"] = "";

                            if (string.Equals(drpdwnCancelCreditCard.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["CreditCardAckStatus"] = "Pending";
                            }

                            if (string.Equals(drpdwnClaimForm.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["CreditCardAckStatus"] = "Pending";
                            }

                            if (string.Equals(drpdwnCompanyVehicleReturned.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ProcurementAckStatus"] = "Pending";
                            }

                            if (string.Equals(drpdwnVehicleSet.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ProcurementAckStatus"] = "Pending";
                            }


                            if (string.Equals(drpdwnFuelCard.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ProcurementAckStatus"] = "Pending";
                            }


                            if (string.Equals(drpdwnVehicleReport.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ProcurementAckStatus"] = "Pending";
                            }


                            if (string.Equals(drpdwnChequeSignature.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["FinanceAckStatus"] = "Pending";
                            }


                            if (string.Equals(drpdwnRemoveEmployee.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["MarketingAckStatus"] = "Pending";
                            }


                            if (string.Equals(drpdwnRemovePhotos.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["MarketingAckStatus"] = "Pending";
                            }

                            if (string.Equals(drpdwnLeetor.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnRemoveAccess.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnMobileReturned.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnMobilePhonePurchased.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnElectronicEquip.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnLaptopCollected.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnChangeVoicemail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnRemoveEmployeeISChecklist.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            if (string.Equals(drpdwnSetAutomaticEmail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                listitem["ISAckStatus"] = "Pending";
                            }
                            listitem.Update();

                        }
                    }
                });
        }

        private string GetApproverString()
        {
            string Approver = "";
            string businessunit = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);


                    SPQuery oquery = new SPQuery();

                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    oquery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + drpdwnBusinessUnit.SelectedItem.Value +
                        "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    if (collitems.Count > 0)
                    {
                        if (Convert.ToString(collitems[0]["HrManager"]) != "")
                        {
                            Approver = "HRManager";
                            ViewState["ApproverEmail"] = collitems[0]["HrManager"];
                        }

                    }
                });
            return Approver;
        }

        private void GetTerminationRequest()
        {
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {

                        txtEmpName.Text = Convert.ToString(listitem["EmployeeName"]);
                        txtEmpNumber.Text = Convert.ToString(listitem["EmployeeNumber"]);
                        string test = Convert.ToString(listitem["IsMobilePurchaseRequired"]);

                        if (string.Equals(Convert.ToString(listitem["IsMobilePurchaseRequired"]), "True"))
                            drpdwnMobilePhone.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsMobilePurchaseRequired"]), "False"))
                            drpdwnMobilePhone.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsImmigrationVisa"]), "True"))
                            drpdwnImmigrationVisa.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsImmigrationVisa"]), "False"))
                            drpdwnImmigrationVisa.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsNovatedLease"]), "True"))
                            drpdwnInnovated.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsNovatedLease"]), "False"))
                            drpdwnInnovated.SelectedValue = "No";
                        if (Convert.ToString(listitem["LastDayAtWork"]) != "")
                            dtLastDayOfWork.SelectedDate = Convert.ToDateTime(listitem["LastDayAtWork"]);


                        if (Convert.ToString(listitem["PeriodOfServiceFrom"]) != "")
                            dtPeriodOfServiceFrom.SelectedDate = Convert.ToDateTime(listitem["PeriodOfServiceFrom"]);
                        if (Convert.ToString(listitem["PeriodOfServiceTo"]) != "")
                            dtPeriodOfServiceTo.SelectedDate = Convert.ToDateTime(listitem["PeriodOfServiceTo"]);
                        

                        txtNotificationComments.Text = Convert.ToString(listitem["Notes"]);

                        /*TaxonomyFieldValue txfWorkArea = listitem["WorkArea"] as TaxonomyFieldValue;
                        if (!string.IsNullOrEmpty(txfWorkArea.Label))
                            drpdwnWorkArea.SelectedValue = txfWorkArea.TermGuid;*/

                        drpdwnWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);

                        /*TaxonomyFieldValue txfBusinessUnit = listitem["BusinessUnit"] as TaxonomyFieldValue;
                        if (!string.IsNullOrEmpty(txfBusinessUnit.Label))
                            drpdwnBusinessUnit.SelectedValue = txfBusinessUnit.TermGuid;*/
                        string txfBusinessUnit = Convert.ToString(listitem["BusinessUnit"]);
                        drpdwnBusinessUnit.SelectedValue = txfBusinessUnit;
                        /*TaxonomyFieldValue txfSiteLocation = listitem["SiteLocation"] as TaxonomyFieldValue;
                        if (!string.IsNullOrEmpty(txfSiteLocation.Label))
                            drpdwnSiteLocation.SelectedValue = txfSiteLocation.TermGuid;*/
                        drpdwnSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);

                        //object obj = listitem["BusinessUnit"];


                    }

                });
        }

        private void GetManagedMetadataValue(SPFieldCollection fields)
        {
            TaxonomyField commodityGrpFld = (TaxonomyField)fields["FieldName"];
            // get the Term Store ID from the field
            Guid commodityGrptermStoreId = commodityGrpFld.SspId;
            // Open a taxonomysession and get the correct termstore
            TaxonomySession session = new TaxonomySession(new SPSite(SPContext.Current.Site.RootWeb.Url));
            TermStore termStore = session.TermStores[commodityGrptermStoreId];

            TermSet termSetCommodity = termStore.GetTermSet(commodityGrpFld.TermSetId);
            if (termSetCommodity != null)
            {
                TermCollection CommodityTermColl = termSetCommodity.Terms;
                ArrayList commodityList = new ArrayList();
                foreach (Term commTerm in CommodityTermColl)
                {
                    commodityList.Add(commTerm.Name);
                }
            }

        }

        private void SetTerminationNotification()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationNotification", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationNotification(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationNotification(listitem);
                });
            }
        }
        
        private void UpdateTerminationNotification(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
               {
                   listitem["EmployeeName"] = txtEmpName.Text;
                   listitem["EmployeeNumber"] = txtEmpNumber.Text;
                   listitem["Notes"] = txtNotificationComments.Text;

                   if (string.Equals(drpdwnMobilePhone.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                   {
                       listitem["IsMobilePurchaseRequired"] = true;

                   }
                   else if (string.Equals(drpdwnMobilePhone.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                   {
                       listitem["IsMobilePurchaseRequired"] = false;

                   }

                   if (string.Equals(drpdwnInnovated.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                       listitem["IsNovatedLease"] = true;
                   else if (string.Equals(drpdwnInnovated.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                       listitem["IsNovatedLease"] = false;

                   if (string.Equals(drpdwnImmigrationVisa.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                       listitem["IsImmigrationVisa"] = true;
                   else if (string.Equals(drpdwnImmigrationVisa.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                       listitem["IsImmigrationVisa"] = false;

                   if (!dtLastDayOfWork.IsDateEmpty)
                       listitem["LastDayAtWork"] = dtLastDayOfWork.SelectedDate.ToString("dd/MM/yyyy");

                   if (!dtPeriodOfServiceFrom.IsDateEmpty)
                       listitem["PeriodOfServiceFrom"] = dtPeriodOfServiceFrom.SelectedDate.ToString("dd/MM/yyyy");

                   if (!dtPeriodOfServiceTo.IsDateEmpty)
                       listitem["PeriodOfServiceTo"] = dtPeriodOfServiceTo.SelectedDate.ToString("dd/MM/yyyy");

                   

                   /*TaxonomyField oField = (TaxonomyField)listitem.Fields["WorkArea"];
                   TaxonomyFieldValue tagValue = new TaxonomyFieldValue(string.Empty);
                   tagValue.PopulateFromLabelGuidPair(drpdwnWorkArea.SelectedValue);
                   oField.SetFieldValue(listitem, tagValue);*/

                   /* TaxonomyFieldValue mmdWorkAreaValue = new TaxonomyFieldValue(string.Empty);
                    mmdWorkAreaValue.PopulateFromLabelGuidPair(drpdwnWorkArea.SelectedItem.Value);
                    mmdWorkAreaValue.WssId = -1;
                    listitem["WorkArea"] = mmdWorkAreaValue;*/

                   listitem["WorkArea"] = drpdwnWorkArea.SelectedValue;

                   /*TaxonomyFieldValue mmdBusinessUnit = new TaxonomyFieldValue(string.Empty);
                   mmdBusinessUnit.PopulateFromLabelGuidPair(drpdwnBusinessUnit.SelectedItem.Value);
                   mmdBusinessUnit.WssId = -1;
                   listitem["BusinessUnit"] = mmdBusinessUnit;*/
                   listitem["BusinessUnit"] = drpdwnBusinessUnit.SelectedItem.Value;

                   /* TaxonomyFieldValue mmdSiteLocation = new TaxonomyFieldValue(string.Empty);
                    mmdSiteLocation.PopulateFromLabelGuidPair(drpdwnSiteLocation.SelectedItem.Value);
                    mmdSiteLocation.WssId = -1;
                    listitem["SiteLocation"] = mmdSiteLocation;*/
                   listitem["SiteLocation"] = drpdwnSiteLocation.SelectedValue;


                   listitem.Update();
               });

        }

        private void SetTerminationBusinessChecklist()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationBusinessChecklist", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationBusinessChecklist(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationBusinessChecklist");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationBusinessChecklist(listitem);
                });
            }
        }

        private void SendEmailsToBusinessChecklistApprovers()
        {
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationBusinessChecklist");
            SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

            SPListItemCollection collitems = oList.GetItems(oquery);
            foreach (SPListItem listitem in collitems)
            {
                string strCancelCardAdvised = Convert.ToString(listitem["IsCancelCardAdvised"]);
                string strIsFinalClaimFormRecieved = Convert.ToString(listitem["IsFinalClaimFormRecieved"]);

                if (string.Equals(strCancelCardAdvised, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsFinalClaimFormRecieved, "True", StringComparison.OrdinalIgnoreCase))
                {
                    // SendEmail("TerminationCreditCardApproval");
                    SendEmailForBusinessUnitChecklists("TerminationCreditCardApproval", "CreditCard");
                }

                string strIsCompanyVehicleReturned = Convert.ToString(listitem["IsCompanyVehicleReturned"]);
                string strIsVehicleKeysSet = Convert.ToString(listitem["IsVehicleKeysSet"]);
                string strIsFuelCard = Convert.ToString(listitem["IsFuelCard"]);
                string strIsVehicleConditionCompleted = Convert.ToString(listitem["IsVehicleConditionCompleted"]);

                if (string.Equals(strIsCompanyVehicleReturned, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsVehicleKeysSet, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsFuelCard, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsVehicleConditionCompleted, "True", StringComparison.OrdinalIgnoreCase))
                {
                    //SendEmail("TerminationProcurementApproval");
                    SendEmailForBusinessUnitChecklists("TerminationProcurementApproval", "Procurement");
                }

                string strIsChequeSignatory = Convert.ToString(listitem["IsChequeSignatory"]);

                if (string.Equals(strIsChequeSignatory, "True", StringComparison.OrdinalIgnoreCase))
                {
                    SendEmailForBusinessUnitChecklists("TerminationFinanceApproval", "Finance");
                }

                string strIsEmployeeRemoved = Convert.ToString(listitem["IsEmployeeRemoved"]);
                string strIsPhotosRemoved = Convert.ToString(listitem["IsPhotosRemoved"]);

                if (string.Equals(strIsEmployeeRemoved, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsPhotosRemoved, "True", StringComparison.OrdinalIgnoreCase))
                {
                    SendEmailForBusinessUnitChecklists("TerminationMarketingApproval", "Marketing");
                }

                if (string.Equals(drpdwnLeetor.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(drpdwnRemoveAccess.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(drpdwnMobileReturned.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(drpdwnMobilePhonePurchased.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(drpdwnElectronicEquip.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(drpdwnLaptopCollected.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(drpdwnChangeVoicemail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(drpdwnRemoveEmployeeISChecklist.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(drpdwnSetAutomaticEmail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    SendEmailForBusinessUnitChecklists("TerminationISApproval", "IS");
                }



                /* string strIsSecurityCard = Convert.ToString(listitem["IsSecurityCard"]);
                 string strIsOfficeKeys = Convert.ToString(listitem["IsOfficeKeys"]);
                 string strIsLockerKeys = Convert.ToString(listitem["IsLockerKeys"]);
                 string strIsFOBPasses = Convert.ToString(listitem["IsFOBPasses"]);
                 string strIsUniformReturned = Convert.ToString(listitem["IsUniformReturned"]);

                 if (string.Equals(strIsSecurityCard, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsOfficeKeys, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsLockerKeys, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsFOBPasses, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(strIsUniformReturned, "True", StringComparison.OrdinalIgnoreCase))
                 {
                     SendEmailForBusinessUnitChecklists("TerminationSiteAdminApproval", "SiteAdmin");
                 }*/

            }

        }
        
        private void UpdateTerminationBusinessChecklist(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                if (string.Equals(drpdwnCancelCreditCard.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsCancelCardAdvised"] = true;

                }
                else if (string.Equals(drpdwnCancelCreditCard.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsCancelCardAdvised"] = false;
                }

                if (string.Equals(drpdwnClaimForm.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsFinalClaimFormRecieved"] = true;

                }
                else if (string.Equals(drpdwnClaimForm.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsFinalClaimFormRecieved"] = false;

                if (string.Equals(drpdwnCompanyVehicleReturned.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsCompanyVehicleReturned"] = true;

                }
                else if (string.Equals(drpdwnCompanyVehicleReturned.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsCompanyVehicleReturned"] = false;

                if (string.Equals(drpdwnVehicleSet.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsVehicleKeysSet"] = true;

                }
                else if (string.Equals(drpdwnVehicleSet.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsVehicleKeysSet"] = false;

                if (string.Equals(drpdwnFuelCard.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsFuelCard"] = true;

                }
                else if (string.Equals(drpdwnFuelCard.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsFuelCard"] = false;
                }

                if (string.Equals(drpdwnVehicleReport.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsVehicleConditionCompleted"] = true;

                }
                else if (string.Equals(drpdwnVehicleReport.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsVehicleConditionCompleted"] = false;
                }

                if (string.Equals(drpdwnChequeSignature.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsChequeSignatory"] = true;

                }
                else if (string.Equals(drpdwnChequeSignature.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsChequeSignatory"] = false;

                }

                if (string.Equals(drpdwnRemoveEmployee.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsEmployeeRemoved"] = true;

                }
                else if (string.Equals(drpdwnRemoveEmployee.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsEmployeeRemoved"] = false;

                }

                if (string.Equals(drpdwnRemovePhotos.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsPhotosRemoved"] = true;


                }
                else if (string.Equals(drpdwnRemovePhotos.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsPhotosRemoved"] = false;

                }

                if (string.Equals(drpdwnSecurityCard.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsSecurityCard"] = true;

                }
                else if (string.Equals(drpdwnSecurityCard.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsSecurityCard"] = false;

                }

                if (string.Equals(drpdwnOfficeKeys.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsOfficeKeys"] = true;

                }
                else if (string.Equals(drpdwnOfficeKeys.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsOfficeKeys"] = false;

                }

                if (string.Equals(drpdwnLockerKey.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsLockerKeys"] = true;

                }
                else if (string.Equals(drpdwnLockerKey.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsLockerKeys"] = false;

                }

                if (string.Equals(drpdwnFOBPassess.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsFOBPasses"] = true;

                }
                else if (string.Equals(drpdwnFOBPassess.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsFOBPasses"] = false;

                }

                if (string.Equals(drpdwnUniformReturn.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsUniformReturned"] = true;

                }
                else if (string.Equals(drpdwnUniformReturn.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                {
                    listitem["IsUniformReturned"] = false;

                }

                listitem.Update();
            });

        }

        private void SetTerminationISChecklist()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationISChecklist", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationISChecklist(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationISChecklist");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationISChecklist(listitem);
                });
            }
        }
        
        private void UpdateTerminationISChecklist(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if (string.Equals(drpdwnLeetor.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEquipmentsInLeeton"] = true;
                else if (string.Equals(drpdwnLeetor.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEquipmentsInLeeton"] = false;

                if (string.Equals(drpdwnRemoveAccess.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsComputerAccessRemoved"] = true;
                else if (string.Equals(drpdwnRemoveAccess.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsComputerAccessRemoved"] = false;

                if (string.Equals(drpdwnMobileReturned.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsMobileRecharged"] = true;
                else if (string.Equals(drpdwnMobileReturned.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsMobileRecharged"] = false;


                if (string.Equals(drpdwnMobilePhonePurchased.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsMobilePurchased"] = true;
                else if (string.Equals(drpdwnMobilePhonePurchased.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsMobilePurchased"] = false;

                if (string.Equals(drpdwnElectronicEquip.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsElectronicEquipment"] = true;
                else if (string.Equals(drpdwnElectronicEquip.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsElectronicEquipment"] = false;


                if (string.Equals(drpdwnLaptopCollected.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsLaptopCollected"] = true;
                else if (string.Equals(drpdwnLaptopCollected.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsLaptopCollected"] = false;

                if (string.Equals(drpdwnChangeVoicemail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsVoicemailChanged"] = true;
                else if (string.Equals(drpdwnChangeVoicemail.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsVoicemailChanged"] = false;


                if (string.Equals(drpdwnRemoveEmployeeISChecklist.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeRemoved"] = true;
                else if (string.Equals(drpdwnRemoveEmployeeISChecklist.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeRemoved"] = false;

                if (string.Equals(drpdwnSetAutomaticEmail.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsAutomaticEmailSet"] = true;
                else if (string.Equals(drpdwnSetAutomaticEmail.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsAutomaticEmailSet"] = false;


                listitem.Update();
            });

        }

        private void SetTerminationMeeting()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationMeeting", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationMeeting(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationMeeting");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationMeeting(listitem);
                });
            }
        }
        
        private void UpdateTerminationMeeting(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if (string.Equals(drpdwnExitInterview.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsExitInterview"] = true;
                else if (string.Equals(drpdwnExitInterview.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsExitInterview"] = false;


                if (string.Equals(drpdwnPropertyCollected.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsCompanyPropertyCollected"] = true;
                else if (string.Equals(drpdwnPropertyCollected.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsCompanyPropertyCollected"] = false;

                if (string.Equals(drpdwnReiterateAgreement.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsAgreementReiterate"] = true;
                else if (string.Equals(drpdwnReiterateAgreement.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsAgreementReiterate"] = false;

                if (string.Equals(drpdwnNotifyEmployeesContacts.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeContactsNotified"] = true;
                else if (string.Equals(drpdwnNotifyEmployeesContacts.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeContactsNotified"] = false;


                if (string.Equals(drpdwnConfirmEmployeesAddress.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeAddressConfirmed"] = true;
                else if (string.Equals(drpdwnConfirmEmployeesAddress.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsEmployeeAddressConfirmed"] = false;


                if (string.Equals(drpdwnCertificateService.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsServiceRequestCertificate"] = true;
                else if (string.Equals(drpdwnCertificateService.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsServiceRequestCertificate"] = false;


                listitem["Comments"] = txtMeetingComments.Text;


                listitem.Update();
            });

        }

        private void SetTerminationHrServices()
        {
            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationHrServices", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationHrServices(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationHrServices");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationHrServices(listitem);
                });
            }
        }
        
        private void UpdateTerminationHrServices(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if (string.Equals(drpdwnFinalPayment.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsProcessFinalPayment"] = true;
                else if (string.Equals(drpdwnFinalPayment.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsProcessFinalPayment"] = false;

                if (string.Equals(drpdwnTerminateSAP.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsTerminateSAPSystem"] = true;
                else if (string.Equals(drpdwnTerminateSAP.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsTerminateSAPSystem"] = false;


                if (string.Equals(drpdwnKronosRemoved.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsKronosRemoved"] = true;
                else if (string.Equals(drpdwnKronosRemoved.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsKronosRemoved"] = false;

                if (string.Equals(drpdwnTerminationPay.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsTerminationPayed"] = true;
                else if (string.Equals(drpdwnTerminationPay.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsTerminationPayed"] = false;

                if (string.Equals(drpdwnDelimitDate.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsDelimitMonitored"] = true;
                else if (string.Equals(drpdwnDelimitDate.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsDelimitMonitored"] = false;

                if (string.Equals(drpdwnRemoveFile.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsPersonnelFileRemoved"] = true;
                else if (string.Equals(drpdwnRemoveFile.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsPersonnelFileRemoved"] = false;

                if (string.Equals(drpdwnHousing.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsHousingVehicleDeclared"] = true;
                else if (string.Equals(drpdwnHousing.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsHousingVehicleDeclared"] = false;

                if (string.Equals(drpdwnVisaNotification.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    listitem["IsVisaNotified"] = true;
                else if (string.Equals(drpdwnVisaNotification.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                    listitem["IsVisaNotified"] = false;



                listitem.Update();
            });

        }

        private void SetTerminationLeave()
        {

            if (Page.Request.QueryString["refno"] != null)
            {
                strRefno = Page.Request.QueryString["refno"];
                lblReferenceNo.Text = strRefno;
            }
            else
            {
                strRefno = lblReferenceNo.Text.Trim();
            }
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
                collectionItems = SetListData("HrWebTerminationLeave", strRefno);
            if (collectionItems != null && collectionItems.Count > 0)
            {
                foreach (SPListItem listitem in collectionItems)
                {
                    UpdateTerminationLeave(listitem);
                }
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPList oList = SPContext.Current.Web.Lists["PositionDetails"];
                    string lstURL = HrWebUtility.GetListUrl("HrWebTerminationLeave");
                    SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    SPListItem listitem = oList.AddItem();
                    listitem["Title"] = strRefno;
                    UpdateTerminationLeave(listitem);
                });
            }
        }

        private void UpdateTerminationLeave(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
              {
                  if (string.Equals(drpdwnParentalLeave.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                      listitem["IsParentalLeave"] = true;
                  else if (string.Equals(drpdwnParentalLeave.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                      listitem["IsParentalLeave"] = false;



                  listitem["Comments"] = txtLeaveComments.Text;

                  /*TaxonomyFieldValue mmdSiteLocation = new TaxonomyFieldValue(string.Empty);
                  mmdSiteLocation.PopulateFromLabelGuidPair(drpdwnSiteLocation.SelectedItem.Value);
                  mmdSiteLocation.WssId = -1;
                  listitem["SiteLocation"] = mmdSiteLocation;*/
                  if (string.Equals(drpdwnLeaveWithoutPay.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                      listitem["IsLeaveWithoutPay"] = true;
                  else if (string.Equals(drpdwnLeaveWithoutPay.SelectedValue, "No", StringComparison.OrdinalIgnoreCase))
                      listitem["IsLeaveWithoutPay"] = false;

                  if (!dtPeriodOfLeaveFrom.IsDateEmpty)
                      listitem["PeriodOfLeaveFrom"] = dtPeriodOfLeaveFrom.SelectedDate.ToString("dd/MM/yyyy");

                  if (!dtPeriodOfLeaveTo.IsDateEmpty)
                      listitem["PeriodOfLeaveTo"] = dtPeriodOfLeaveTo.SelectedDate.ToString("dd/MM/yyyy");

                  listitem.Update();


              });

            //object obj = listitem["BusinessUnit"];


        }

        private void GetBusinessChecklist()
        {
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationBusinessChecklist");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);


                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {

                        if (string.Equals(Convert.ToString(listitem["IsCancelCardAdvised"]), "True"))
                            drpdwnCancelCreditCard.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsCancelCardAdvised"]), "False"))
                            drpdwnCancelCreditCard.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsFinalClaimFormRecieved"]), "True"))
                            drpdwnClaimForm.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsFinalClaimFormRecieved"]), "False"))
                            drpdwnClaimForm.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsCompanyVehicleReturned"]), "True"))
                            drpdwnCompanyVehicleReturned.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsCompanyVehicleReturned"]), "False"))
                            drpdwnCompanyVehicleReturned.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsVehicleKeysSet"]), "True"))
                            drpdwnVehicleSet.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsVehicleKeysSet"]), "False"))
                            drpdwnVehicleSet.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsFuelCard"]), "True"))
                            drpdwnFuelCard.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsFuelCard"]), "False"))
                            drpdwnFuelCard.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsVehicleConditionCompleted"]), "True"))
                            drpdwnVehicleReport.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsVehicleConditionCompleted"]), "False"))
                            drpdwnVehicleReport.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsChequeSignatory"]), "True"))
                            drpdwnChequeSignature.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsChequeSignatory"]), "False"))
                            drpdwnChequeSignature.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "True"))
                            drpdwnRemoveEmployee.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "False"))
                            drpdwnRemoveEmployee.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsPhotosRemoved"]), "True"))
                            drpdwnRemovePhotos.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsPhotosRemoved"]), "False"))
                            drpdwnRemovePhotos.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsSecurityCard"]), "True"))
                            drpdwnSecurityCard.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsSecurityCard"]), "False"))
                            drpdwnSecurityCard.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsOfficeKeys"]), "True"))
                            drpdwnOfficeKeys.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsOfficeKeys"]), "False"))
                            drpdwnOfficeKeys.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsLockerKeys"]), "True"))
                            drpdwnLockerKey.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsLockerKeys"]), "False"))
                            drpdwnLockerKey.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsFOBPasses"]), "True"))
                            drpdwnFOBPassess.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsFOBPasses"]), "False"))
                            drpdwnFOBPassess.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsUniformReturned"]), "True"))
                            drpdwnUniformReturn.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsUniformReturned"]), "False"))
                            drpdwnUniformReturn.SelectedValue = "No";

                    }
                });
        }

        private void GetISChecklist()
        {

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationISChecklist");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);



                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {

                        if (string.Equals(Convert.ToString(listitem["IsEquipmentsInLeeton"]), "True"))
                            drpdwnLeetor.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsEquipmentsInLeeton"]), "False"))
                            drpdwnLeetor.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsComputerAccessRemoved"]), "True"))
                            drpdwnRemoveAccess.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsComputerAccessRemoved"]), "False"))
                            drpdwnRemoveAccess.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsMobileRecharged"]), "True"))
                            drpdwnMobileReturned.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsMobileRecharged"]), "False"))
                            drpdwnMobileReturned.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsMobilePurchased"]), "True"))
                            drpdwnMobilePhonePurchased.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsMobilePurchased"]), "False"))
                            drpdwnMobilePhonePurchased.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsElectronicEquipment"]), "True"))
                            drpdwnElectronicEquip.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsElectronicEquipment"]), "False"))
                            drpdwnElectronicEquip.SelectedValue = "No";


                        if (string.Equals(Convert.ToString(listitem["IsLaptopCollected"]), "True"))
                            drpdwnLaptopCollected.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsLaptopCollected"]), "False"))
                            drpdwnLaptopCollected.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsVoicemailChanged"]), "True"))
                            drpdwnChangeVoicemail.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsVoicemailChanged"]), "False"))
                            drpdwnChangeVoicemail.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "True"))
                            drpdwnRemoveEmployeeISChecklist.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "False"))
                            drpdwnRemoveEmployeeISChecklist.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsAutomaticEmailSet"]), "True"))
                            drpdwnSetAutomaticEmail.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsAutomaticEmailSet"]), "False"))
                            drpdwnSetAutomaticEmail.SelectedValue = "No";

                    }
                });
        }

        private void GetMeeting()
        {
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationMeeting");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);


                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {

                        if (string.Equals(Convert.ToString(listitem["IsExitInterview"]), "True"))
                            drpdwnExitInterview.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsExitInterview"]), "False"))
                            drpdwnExitInterview.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsCompanyPropertyCollected"]), "True"))
                            drpdwnPropertyCollected.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsCompanyPropertyCollected"]), "False"))
                            drpdwnPropertyCollected.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsAgreementReiterate"]), "True"))
                            drpdwnReiterateAgreement.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsAgreementReiterate"]), "False"))
                            drpdwnReiterateAgreement.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsEmployeeContactsNotified"]), "True"))
                            drpdwnNotifyEmployeesContacts.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsEmployeeContactsNotified"]), "False"))
                            drpdwnNotifyEmployeesContacts.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsEmployeeAddressConfirmed"]), "True"))
                            drpdwnConfirmEmployeesAddress.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsEmployeeAddressConfirmed"]), "False"))
                            drpdwnConfirmEmployeesAddress.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsServiceRequestCertificate"]), "True"))
                            drpdwnCertificateService.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsServiceRequestCertificate"]), "False"))
                            drpdwnCertificateService.SelectedValue = "No";

                        txtMeetingComments.Text = Convert.ToString(listitem["Comments"]);

                    }
                });
        }

        private void GetHRServices()
        {
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationHrServices");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

                    SPQuery oquery = new SPQuery();
                    oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    foreach (SPListItem listitem in collitems)
                    {

                        if (string.Equals(Convert.ToString(listitem["IsProcessFinalPayment"]), "True"))
                            drpdwnFinalPayment.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsProcessFinalPayment"]), "False"))
                            drpdwnFinalPayment.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsTerminateSAPSystem"]), "True"))
                            drpdwnTerminateSAP.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsTerminateSAPSystem"]), "False"))
                            drpdwnTerminateSAP.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsKronosRemoved"]), "True"))
                            drpdwnKronosRemoved.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsKronosRemoved"]), "False"))
                            drpdwnKronosRemoved.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsTerminationPayed"]), "True"))
                            drpdwnTerminationPay.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsTerminationPayed"]), "False"))
                            drpdwnTerminationPay.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsDelimitMonitored"]), "True"))
                            drpdwnDelimitDate.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsDelimitMonitored"]), "False"))
                            drpdwnDelimitDate.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsPersonnelFileRemoved"]), "True"))
                            drpdwnRemoveFile.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsPersonnelFileRemoved"]), "False"))
                            drpdwnRemoveFile.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsHousingVehicleDeclared"]), "True"))
                            drpdwnHousing.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsHousingVehicleDeclared"]), "False"))
                            drpdwnHousing.SelectedValue = "No";

                        if (string.Equals(Convert.ToString(listitem["IsVisaNotified"]), "True"))
                            drpdwnVisaNotification.SelectedValue = "Yes";
                        else if (string.Equals(Convert.ToString(listitem["IsVisaNotified"]), "False"))
                            drpdwnVisaNotification.SelectedValue = "No";

                    }
                });
        }

        protected void btnTerminationSave_Click(object sender, EventArgs e)
        {
            try
            {
                //string refno = lblReferenceNo.Text.Trim();
                SetTerminationGeneralInfoList(false, "Draft", "");
                SetTerminationNotification();

                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToLeaveTab();", true);

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnTerminationSave_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }

        }

        protected void btnLeaveSave_Click(object sender, EventArgs e)
        {
            try
            {

                SetTerminationGeneralInfoList(false, "Draft", "");
                SetTerminationLeave();

                if (!BusinessChecklistTab.Visible)
                {
                    Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Draft");

                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToBCTab();", true);
                }
                /* if (string.Equals(GetUserType(), "Initiator", StringComparison.OrdinalIgnoreCase))
                 {
                     Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Draft");

                 }
                 else
                 {
                     Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToBCTab();", true);
                 }*/
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnLeaveSave_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }




        }

        protected void btnBusinessChecklist_Click(object sender, EventArgs e)
        {
            try
            {
                SetTerminationBusinessChecklist();
                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToISTab();", true);
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnBusinessChecklist_Click", ex.Message);
                lblTerminationRequest.Text ="Unexpected error has occured. Please contact IT team.";
                //lblTerminationRequest.Text = ex.Message;
            }
        }

        protected void btnISChecklist_Click(object sender, EventArgs e)
        {
            try
            {

                // SetTerminationGeneralInfoList(false, "Pending Approval", "");
                SetTerminationISChecklist();
                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToMeetingTab();", true);

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnISChecklist_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }


        }

        protected void btnMeeting_Click(object sender, EventArgs e)
        {
            try
            {

                SetTerminationGeneralInfoList(false, "Pending Approval", "");
                SetTerminationMeeting();
                Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Draft");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnMeeting_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }


        }

        protected void btnHrServiceces_Click(object sender, EventArgs e)
        {

            try
            {

                SetTerminationGeneralInfoList(false, "Draft", "");
                SetTerminationHrServices();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnHrServiceces_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }



        }

        protected void btnNotificationSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                SetTerminationGeneralInfoList(false, "Pending Approval", "");
                SetTerminationNotification();
                SetTerminationLeave();

                Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Submit");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnNotificationSubmit_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }
        }

        private bool ValidateInitiatorSubmit()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(lblDateOFRequest.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnPositionType.SelectedValue))
                bresult = false;

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            if (string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnBusinessUnit.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnSiteLocation.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnMobilePhone.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnImmigrationVisa.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnInnovated.SelectedValue))
                bresult = false;

            if (dtLastDayOfWork.IsDateEmpty)
                bresult = false;

            if (dtPeriodOfServiceFrom.IsDateEmpty)
                bresult = false;

            if (dtPeriodOfServiceTo.IsDateEmpty)
                bresult = false;

            if (dtPeriodOfLeaveFrom.IsDateEmpty && (drpdwnParentalLeave.SelectedValue=="Yes" || drpdwnLeaveWithoutPay.SelectedValue=="Yes"))
                bresult = false;

            if (dtPeriodOfLeaveTo.IsDateEmpty && (drpdwnParentalLeave.SelectedValue == "Yes" || drpdwnLeaveWithoutPay.SelectedValue == "Yes"))
                bresult = false;

            /*if (string.IsNullOrEmpty(txtNotificationComments.Text.Trim()))
                bresult = false;*/

            if (string.IsNullOrEmpty(drpdwnParentalLeave.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnLeaveWithoutPay.SelectedValue))
                bresult = false;

            /*if (string.IsNullOrEmpty(txtLeaveComments.Text.Trim()))
                bresult = false;*/

            return bresult;
        }

        protected void btnInitiatorSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateInitiatorSubmit())
                {

                    string Approver = GetApprover(drpdwnBusinessUnit.SelectedItem.Value);
                    if (Approver != "")
                    {
                        bool bProceed = SetTerminationGeneralInfoList(false, "Pending Approval", "");
                        if (bProceed)
                        {
                            SetTerminationNotification();
                            SetTerminationLeave();

                            SendEmail("Termination");



                            if (string.Equals(drpdwnMobilePhone.SelectedValue, "Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                SendEmailForBusinessUnitChecklists("TerminationMobilePhone", "IS");
                            }


                            Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Submit");
                        }
                    }
                    else
                    {
                        lblTerminationRequest.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }
                else
                {
                    lblTerminationRequest.Text = "Please fill all the mandatory fields";
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnInitiatorSubmit_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }
        }

        private ArrayList GetApproverEmail()
        {

            ArrayList arrApprover = new ArrayList();
            string businessunit = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);


                    SPQuery oquery = new SPQuery();

                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    oquery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + drpdwnBusinessUnit.SelectedItem.Value +
                        "</Value></Eq></Where>";

                    SPListItemCollection collitems = olist.GetItems(oquery);
                    if (collitems.Count > 0)
                    {
                        foreach (SPListItem item in collitems)
                        {
                            string strTo = Convert.ToString(item["HrManager"]);
                            string[] tmparr = strTo.Split('|');
                            strTo = tmparr[tmparr.Length - 1];
                            if (strTo.Contains("#"))
                                strTo = strTo.Split('#')[1];
                            arrApprover.Add(strTo);
                        }

                    }
                });
            return arrApprover;
        }

        private void SendEmailForBusinessUnitChecklists(string strFormType, string strBusinessType)
        {
            ArrayList arrTo = new ArrayList();
            string strRefNo = lblReferenceNo.Text.Trim();


            SPSecurity.RunWithElevatedPrivileges(delegate()
               {
                   SPSite site = SPContext.Current.Site;

                   SPWeb web = site.OpenWeb();
                   string lstURL1 = HrWebUtility.GetListUrl("EmailConfig");
                   SPList lst = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                   //SPList lst = web.Lists["EmailConfig"];


                   SPQuery oQuery = new SPQuery();
                   oQuery.Query = "<Where><Eq><FieldRef Name=\'FormType\' /><Value Type=\"Text\">" + "Termination" +
               "</Value></Eq></Where>";
                   oQuery.ViewFields = string.Concat(
                                  "<FieldRef Name='Title' />",
                                  "<FieldRef Name='EmailIP' />",
                                  "<FieldRef Name='ApprovedSubject' />",
                                  "<FieldRef Name='ApprovalSubject' />",
                                  "<FieldRef Name='ApprovedMessage' />",
                                  "<FieldRef Name='ApprovalMessage' />");
                   SPListItemCollection collListItems = lst.GetItems(oQuery);

                   foreach (SPListItem itm in collListItems)
                   {
                       //send email
                       string strFrom = "";
                       string strTo = "";
                       string strSubject = "";
                       string strMessage = "";



                       string lstURL = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

                       SPQuery oquery = new SPQuery();

                       oquery.Query = "<Where><Eq><FieldRef Name=\'BusinessType\' /><Value Type=\"Text\">" + strBusinessType +
                           "</Value></Eq></Where>";

                       SPListItemCollection collitems = olist.GetItems(oquery);
                       if (collitems.Count > 0)
                       {
                           foreach (SPListItem item in collitems)
                           {
                               string strApproverType = Convert.ToString(item["ApproverType"]);
                               if (string.Equals(strApproverType, "Individual", StringComparison.OrdinalIgnoreCase))
                               {
                                   string strMailTo = Convert.ToString(item["Approver"]);
                                   string[] tmparr = strMailTo.Split('|');
                                   strMailTo = tmparr[tmparr.Length - 1];
                                   if (strMailTo.Contains("#"))
                                       strMailTo = strMailTo.Split('#')[1];

                                   if (!string.IsNullOrEmpty(strMailTo.Trim()))
                                       arrTo.Add(strMailTo);

                               }
                               else if (string.Equals(strApproverType, "SPGroup", StringComparison.OrdinalIgnoreCase))
                               {
                                   string to = string.Empty;

                                   string strMailTo = Convert.ToString(item["Approver"]);
                                   string[] tmparr = strMailTo.Split('|');
                                   strMailTo = tmparr[tmparr.Length - 1];
                                   if (strMailTo.Contains("#"))
                                       strMailTo = strMailTo.Split('#')[1];

                                   if (!string.IsNullOrEmpty(strMailTo.Trim()))
                                   {
                                       /*using (SPSite newSite = new SPSite(site.ID))
                                       {
                                           using (SPWeb newWeb = newSite.OpenWeb(web.ID))
                                           {
                                               SPGroup group = newWeb.Groups[strMailTo];
                                               foreach (SPUser user in group.Users)
                                               {
                                                   arrTo.Add(user.Email);
                                               }
                                           }
                                       }*/
                                       arrTo.Add(HrWebUtility.GetDistributionEmail(Convert.ToString(item["BusinessType"])));

                                   }
                               }
                               else { }

                           }
                       }


                       if (arrTo.Count > 0) 
                       {
                           SmtpClient smtpClient = new SmtpClient();
                           smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                           smtpClient.Port = 25;
                           string url = site.Url + "/pages/hrweb/terminationreview.aspx?refno=" + strRefNo;
                           strFrom = Convert.ToString(itm["Title"]);



                           strTo = strTo.TrimStart(';');
                           strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                           if(strFormType == "TerminationMobilePhone")
                               strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                               Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;NAME&gt;", txtEmpName.Text.Trim()).
                               Replace("&lt;BU&gt;", drpdwnBusinessUnit.SelectedValue).
                               Replace("&lt;LOCATION&gt;", drpdwnSiteLocation.SelectedValue).
                               Replace("&lt;TERMINATIONDATE&gt;", lblDateOFRequest.Text);
                           else
                            strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                               Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;NAME&gt;", txtEmpName.Text.Trim()).
                               Replace("&lt;BU&gt;", drpdwnBusinessUnit.SelectedValue).
                               Replace("&lt;LOCATION&gt;", drpdwnSiteLocation.SelectedValue).
                               Replace("&lt;TERMINATIONDATE&gt;", lblDateOFRequest.Text);
                           MailMessage mailMessage = new MailMessage();
                           var distinctIDs = arrTo.ToArray().Distinct();
                           foreach (string strMailTo in distinctIDs)
                           {
                               if (!string.IsNullOrEmpty(strMailTo))
                                   mailMessage.To.Add(strMailTo);
                           }
                           mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");
                           mailMessage.Subject = strSubject;
                           mailMessage.Body = strMessage;

                           mailMessage.IsBodyHtml = true;
                           
                           smtpClient.Send(mailMessage);

                           SaveEmailDetails(strFrom, strTo, strSubject, strMessage);
                       }

                   }
               });

        }

        private void SendEmail(string strFormType)
        {
            string strRefNo = lblReferenceNo.Text.Trim();
            ArrayList strApprover = GetApproverEmail();
            if (strApprover.Count > 0)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPSite site = SPContext.Current.Site;

                    SPWeb web = site.OpenWeb();
                    string lstURL = HrWebUtility.GetListUrl("EmailConfig");
                    SPList lst = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    //SPList lst = web.Lists["EmailConfig"];


                    SPQuery oQuery = new SPQuery();
                    oQuery.Query = "<Where><Eq><FieldRef Name=\'FormType\' /><Value Type=\"Text\">" + "Termination" +
                "</Value></Eq></Where>";
                    oQuery.ViewFields = string.Concat(
                                   "<FieldRef Name='Title' />",
                                   "<FieldRef Name='EmailIP' />",
                                   "<FieldRef Name='ApprovalSubject' />",
                                   "<FieldRef Name='HRManagerApprovalMessage' />");
                    SPListItemCollection collListItems = lst.GetItems(oQuery);

                    foreach (SPListItem itm in collListItems) 
                    {
                        //send email
                        string strFrom = "";
                        string strTo = "";
                        string strSubject = "";
                        string strMessage = "";


                        SmtpClient smtpClient = new SmtpClient();
                        smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                        smtpClient.Port = 25;
                        strFrom = Convert.ToString(itm["Title"]);

                        string url = site.Url + "/pages/hrweb/terminationreview.aspx?refno=" + strRefNo;
                        
                        /*using (SPSite newSite = new SPSite(site.ID))
                        {
                            using (SPWeb newWeb = newSite.OpenWeb(web.ID))
                            {
                                SPGroup group = newWeb.Groups["HR Services"];
                                foreach (SPUser user in group.Users)
                                {
                                    strApprover.Add(user.Email);
                                }
                            }
                        }*/
                           strApprover.Add(HrWebUtility.GetDistributionEmail("HR Services"));
                        
                        strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                        strMessage = Convert.ToString(itm["HRManagerApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                               Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;NAME&gt;", txtEmpName.Text.Trim()).
                               Replace("&lt;BU&gt;", drpdwnBusinessUnit.SelectedValue).
                               Replace("&lt;LOCATION&gt;", drpdwnSiteLocation.SelectedValue).
                               Replace("&lt;TERMINATIONDATE&gt;", dtLastDayOfWork.SelectedDate.ToString("dd/MM/yyyy"));
                        // MailMessage mailMessage = new MailMessage(strFrom, strTo, strSubject, strMessage);

                        if (strApprover.Count > 0)
                        {
                            MailMessage mailMessage = new MailMessage();
                            var distinctIDs = strApprover.ToArray().Distinct();
                            foreach (string strMailTo in distinctIDs) 
                            {
                                if (strMailTo.Trim() != "") 
                                mailMessage.To.Add(strMailTo);
                            }
                            mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");
                            mailMessage.Subject = strSubject;
                            mailMessage.Body = strMessage;

                            mailMessage.IsBodyHtml = true;
                            smtpClient.Send(mailMessage);

                            SaveEmailDetails(strFrom, strTo, strSubject, strMessage);


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
                    oItem["FormType"] = "Termination";
                    oItem.Update();
                //}
            });
        }

        private SPListItemCollection GetBusinessUnitWAandLoc(string strLstName, string strValue)
        {
            string lstURL = HrWebUtility.GetListUrl(strLstName);
            SPList oList = null;
            SPQuery oQuery = new SPQuery();
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    oQuery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + strValue + "</Value></Eq></Where>";
                });
            return oList.GetItems(oQuery);
        }
        
        private void AddBusinessUnitWAandLoc()
        {
            SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", drpdwnBusinessUnit.SelectedItem.Value);
            drpdwnWorkArea.Items.Clear();
            if (oItems != null && oItems.Count > 0)
            {
                DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                drpdwnWorkArea.DataSource = dtWorkArea;
                drpdwnWorkArea.DataValueField = "WorkArea";


                drpdwnWorkArea.DataTextField = "WorkArea";
                drpdwnWorkArea.DataBind();
            }

            SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", drpdwnBusinessUnit.SelectedItem.Value);
            drpdwnSiteLocation.Items.Clear();
            if (oItems1 != null && oItems1.Count > 0)
            {
                foreach (SPListItem itm in oItems1)
                {
                    string strLoc = Convert.ToString(itm["Location"]);
                    drpdwnSiteLocation.Items.Add(strLoc);
                    /*DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    drpdwnSiteLocation.DataSource = dtLocation;
                    drpdwnSiteLocation.DataValueField = "Location";
                    drpdwnSiteLocation.DataTextField = "Location";
                    drpdwnSiteLocation.DataBind();*/
                }


            }
        }
        
        protected void drpdwnBusinessUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*notificationTab.Visible = true;
            TypeOfLeaveTab.Visible = true;

            BusinessChecklistTab.Visible = false;
            ISChecklistTab.Visible = false;
            TerminationMeetingTab.Visible = false;*/

            try
            {
                DropDownList ddlBusinessUnit = (DropDownList)sender;
                SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", ddlBusinessUnit.SelectedItem.Text);
                drpdwnWorkArea.Items.Clear();
                if (oItems != null && oItems.Count > 0)
                {
                    DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                    dtWorkArea.DefaultView.Sort = "WorkArea ASC";
                    drpdwnWorkArea.DataSource = dtWorkArea;
                    drpdwnWorkArea.DataValueField = "WorkArea";
                    drpdwnWorkArea.DataTextField = "WorkArea";
                    drpdwnWorkArea.DataBind();
                }

                SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", ddlBusinessUnit.SelectedItem.Text);
                drpdwnSiteLocation.Items.Clear();
                if (oItems1 != null && oItems1.Count > 0)
                {
                    DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                    dtLocation.DefaultView.Sort = "Location ASC";
                    drpdwnSiteLocation.DataSource = dtLocation;
                    drpdwnSiteLocation.DataValueField = "Location";
                    drpdwnSiteLocation.DataTextField = "Location";
                    drpdwnSiteLocation.DataBind();


                }

                /*if (IsInitiator())
                {
                    notificationTab.Visible = true;
                    TypeOfLeaveTab.Visible = true;
                    
                    BusinessChecklistTab.Visible = true;
                    ISChecklistTab.Visible = true;
                    TerminationMeetingTab.Visible = true;
                    btnInitiatorSubmit.Visible = false;
                    btnLeaveSave.Text = "Save & Next";


                }
                else
                {
                    notificationTab.Visible = true;
                    TypeOfLeaveTab.Visible = true;

                    BusinessChecklistTab.Visible = false;
                    ISChecklistTab.Visible = false;
                    TerminationMeetingTab.Visible = false;

                    btnInitiatorSubmit.Visible = true;
                    btnLeaveSave.Text = "Save";
                }*/

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.drpdwnBusinessUnit_SelectedIndexChanged", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }
        }

        private string GetApprover(string businessunit)
        {
            string Approver = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery3 = new SPQuery();
                // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                oquery3.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                    "</Value></Eq></Where>";
                SPListItemCollection collitems2 = olist1.GetItems(oquery3);
                if (collitems2.Count > 0)
                {
                    Approver = Convert.ToString(collitems2[0]["HrManager"]);
                }
            });
            return Approver;
        }
        
        private bool ValidateHrManagerSubmit()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(lblDateOFRequest.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnPositionType.SelectedValue))
                bresult = false;

            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            if (string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnBusinessUnit.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnSiteLocation.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnMobilePhone.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnImmigrationVisa.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnInnovated.SelectedValue))
                bresult = false;

            if (dtLastDayOfWork.IsDateEmpty)
                bresult = false;

            if (dtPeriodOfServiceFrom.IsDateEmpty)
                bresult = false;
            

            //if (string.IsNullOrEmpty(txtNotificationComments.Text.Trim()))
            //    bresult = false;

            if (string.IsNullOrEmpty(drpdwnParentalLeave.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnLeaveWithoutPay.SelectedValue))
                bresult = false;

            if (dtPeriodOfLeaveFrom.IsDateEmpty && (drpdwnParentalLeave.SelectedValue == "Yes" || drpdwnLeaveWithoutPay.SelectedValue == "Yes"))
                bresult = false;

            if (dtPeriodOfLeaveTo.IsDateEmpty && (drpdwnParentalLeave.SelectedValue == "Yes" || drpdwnLeaveWithoutPay.SelectedValue == "Yes"))
                bresult = false;

            //if (string.IsNullOrEmpty(txtLeaveComments.Text.Trim()))
            //    bresult = false;


            if (string.IsNullOrEmpty(drpdwnCancelCreditCard.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnClaimForm.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnCompanyVehicleReturned.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnVehicleSet.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnFuelCard.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnVehicleReport.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnChequeSignature.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnRemoveEmployee.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnRemovePhotos.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnSecurityCard.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnOfficeKeys.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnLockerKey.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnFOBPassess.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnUniformReturn.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnLeetor.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnRemoveAccess.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnMobileReturned.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnMobilePhonePurchased.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnElectronicEquip.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnLaptopCollected.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnChangeVoicemail.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnRemoveEmployeeISChecklist.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnSetAutomaticEmail.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnExitInterview.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnPropertyCollected.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnReiterateAgreement.SelectedValue))
                bresult = false;


            if (string.IsNullOrEmpty(drpdwnNotifyEmployeesContacts.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnConfirmEmployeesAddress.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(drpdwnCertificateService.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtMeetingComments.Text.Trim()))
                bresult = false;

            return bresult;

        }

        protected void btnMeetingSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateHrManagerSubmit())
                {
                    string Approver = GetApprover(drpdwnBusinessUnit.SelectedItem.Value);
                    if (Approver != "")
                    {
                        SetTerminationGeneralInfoList(false, "Pending Approval", "HRManager");
                        SetTerminationBusinessChecklist();
                        SetTerminationISChecklist();
                        SetTerminationMeeting();
                        UpdateOtherApproverStatusGeneralInfo();
                        //SendEmailForBusinessUnitChecklists("TerminationHrServices", "HR Services");
                        SendEmailsToBusinessChecklistApprovers();
                        Server.Transfer("/people/Pages/HRWeb/TerminationStatus.aspx?refno=" + strRefno + "&flow=Submit");
                    }
                    else
                    {
                        lblTerminationRequest.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }
                else
                {
                    lblTerminationRequest.Text = "Please fill all the mandatory fields";
                }

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.btnMeetingSubmit_Click", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblTerminationRequest.Text = ex.Message;
            }
        }


    }
}
