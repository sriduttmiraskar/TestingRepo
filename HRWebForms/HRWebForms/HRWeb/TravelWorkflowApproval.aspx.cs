using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.WebPartPages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web.Hosting;


namespace HRWebForms.HRWeb
{
    public partial class TravelWorkflowApproval : WebPartPage
    {
        string UserName = string.Empty;
        Hashtable ohash = new Hashtable();
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
                       VerifyUser(UserName);
                
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelWorkflowApproval.Page_Load", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void VerifyUser(string username)
        {
            DataTable dtDraftTable = new DataTable();
            dtDraftTable.Columns.Add(new DataColumn("TypeOfTravel"));
            dtDraftTable.Columns.Add(new DataColumn("TravellerName"));
            dtDraftTable.Columns.Add(new DataColumn("FormNo"));
            dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
            dtDraftTable.Columns.Add(new DataColumn("Approver"));
            dtDraftTable.Columns.Add(new DataColumn("ID"));
            DraftGrid.DataSource = dtDraftTable;
            DraftGrid.DataBind();

            DataTable dtPending = new DataTable();
            dtPending.Columns.Add(new DataColumn("TypeOfTravel"));
            dtPending.Columns.Add(new DataColumn("TravellerName"));
            dtPending.Columns.Add(new DataColumn("FormNo"));
            dtPending.Columns.Add(new DataColumn("BusinessUnit"));
            dtPending.Columns.Add(new DataColumn("Approver"));
            dtPending.Columns.Add(new DataColumn("ID"));
            PendingApprovalGrid.DataSource = dtPending;
            PendingApprovalGrid.DataBind();

            DataTable dtApproved = new DataTable();
            dtApproved.Columns.Add(new DataColumn("TypeOfTravel"));
            dtApproved.Columns.Add(new DataColumn("TravellerName"));
            dtApproved.Columns.Add(new DataColumn("FormNo"));
            dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
            dtApproved.Columns.Add(new DataColumn("Approver"));
            dtApproved.Columns.Add(new DataColumn("ID"));
            ApprovedGrid.DataSource = dtApproved;
            ApprovedGrid.DataBind();

            DataTable dtRejected = new DataTable();
            dtRejected.Columns.Add(new DataColumn("TypeOfTravel"));
            dtRejected.Columns.Add(new DataColumn("TravellerName"));
            dtRejected.Columns.Add(new DataColumn("FormNo"));
            dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
            dtRejected.Columns.Add(new DataColumn("Approver"));
            dtRejected.Columns.Add(new DataColumn("ID"));
            RejectedGrid.DataSource = dtRejected;
            RejectedGrid.DataBind();

            PopulateCEOInformation(dtPending,dtApproved,dtRejected,username,"All");
            PopulateMgrInformation(dtPending, dtApproved, dtRejected, username, "All");
            PopulateChairmanInformation(dtPending,dtApproved,dtRejected,username, "All");
            PopulateTCInformation(dtPending, dtApproved, dtRejected, username,"All");
            PopulateInitiatorInformation(username, dtDraftTable, dtPending, dtApproved, dtRejected,"All");

        }

        private void PopulateMgrInformation(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username,string Mode)
        {
            string lstURL1 = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
            SPQuery oquery = new SPQuery();
            string query = string.Empty;
            if(Mode=="All")
                query = string.Concat("<Where><Eq><FieldRef Name=\'ManagerName\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            else if(Mode=="Pending")
                query = string.Concat("<Where><And><Eq><FieldRef Name=\'ManagerName\' /><Value Type=\"User\">" + username +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>");
            else if (Mode == "Approved")
                query = string.Concat("<Where><And><Eq><FieldRef Name=\'ManagerName\' /><Value Type=\"User\">" + username +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>");
            else if (Mode == "Rejected")
                query = string.Concat("<Where><And><Eq><FieldRef Name=\'ManagerName\' /><Value Type=\"User\">" + username +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></And></Where>");
            oquery.Query = query;

            SPListItemCollection collitems = olist1.GetItems(oquery);
            if (collitems != null && collitems.Count > 0)
            {
                foreach (SPListItem itm in collitems)
                {
                    string strStatus = Convert.ToString(itm["Status"]);
                    if (string.Equals(strStatus, "Pending Approval", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                        string strTravellerName = Convert.ToString(itm["TravellerName"]);                        
                        string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                        string strID = Convert.ToString(itm["ID"]);
                        username = GetApprover(itm);
                        if (!ohash.Contains(strRefno))
                        {
                            GetPendingStatusDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtPending, username,Mode);
                            ohash.Add(strRefno, strRefno);
                        }
                    }
                    else if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                        string strTravellerName = Convert.ToString(itm["TravellerName"]);
                        string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                        string strID = Convert.ToString(itm["ID"]);
                        username = GetApprover(itm);
                        if (!ohash.Contains(strRefno))
                        {
                            GetPendingApprovedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtApproved, username,Mode);
                            ohash.Add(strRefno, strRefno);
                        }
                    }
                    else if (string.Equals(strStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                        string strTravellerName = Convert.ToString(itm["TravellerName"]);
                        string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                        string strID = Convert.ToString(itm["ID"]);
                        username = GetApprover(itm);
                        if (!ohash.Contains(strRefno))
                        {
                            GetPendingRejectedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtRejected, username,Mode);
                            ohash.Add(strRefno, strRefno);
                        }
                    }

                }
            }
        }

        private void PopulateCEOInformation(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username,string Mode)
        {
            string lstTravelAppInfo = HrWebUtility.GetListUrl("TravelApprovalInfo");
            SPList splistTravelAppInfo = SPContext.Current.Site.RootWeb.GetList(lstTravelAppInfo);
            SPQuery oqueryTravelAppInfo = new SPQuery();
            string queryTravelAppInfo = string.Concat("<Where><Eq><FieldRef Name=\'CEOApprover\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            oqueryTravelAppInfo.Query = queryTravelAppInfo;

            SPListItemCollection collitemsTravelAppInfo = splistTravelAppInfo.GetItems(oqueryTravelAppInfo);
            if (collitemsTravelAppInfo != null && collitemsTravelAppInfo.Count > 0)
            {
                GetCEODetails(dtPending, dtApproved, dtRejected, username,Mode);
            }
        }

        private void PopulateChairmanInformation(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username,String Mode)
        {
            string lstTravelAppInfo = HrWebUtility.GetListUrl("TravelApprovalInfo");
            SPList splistTravelAppInfo = SPContext.Current.Site.RootWeb.GetList(lstTravelAppInfo); 
            string queryTravelAppInfoCh = string.Concat("<Where><Eq><FieldRef Name=\'ChairmanApprover\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            SPQuery oqueryTravelAppInfoCh = new SPQuery();
            oqueryTravelAppInfoCh.Query = queryTravelAppInfoCh;

            SPListItemCollection collitemsTravelAppInfoCh = splistTravelAppInfo.GetItems(oqueryTravelAppInfoCh);

            if (collitemsTravelAppInfoCh != null && collitemsTravelAppInfoCh.Count > 0)
            {
                GetChairmanDetails(dtPending, dtApproved, dtRejected, username, Mode);
            }
        }

        private void PopulateTCInformation(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username,string Mode)
        {
            string lstURL2 = HrWebUtility.GetListUrl("TravelCoordinatorApprovalInfo");
            SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL2);
            SPQuery oquery1 = new SPQuery();
            oquery1.Query = string.Concat("<Where><Eq><FieldRef Name=\'TravelCoordinator\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            SPListItemCollection collitems1 = olist2.GetItems(oquery1);
            if (collitems1.Count > 0)
            {
                GetCoordinatorDetails(dtPending, dtApproved, dtRejected, username,Mode);
            }
        }

        private void PopulateInitiatorInformation(string username, DataTable dtDraftTable, DataTable dtPending, DataTable dtApproved, DataTable dtRejected,string Mode)
        {
            string lstURL1 = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
            string queryInit = string.Empty;
            if(Mode=="All")
                queryInit = string.Concat("<Where><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" +
                    username + "</Value></Eq></Where>");
            else if(Mode=="Draft")
                queryInit = string.Concat("<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" +
                    username + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Draft</Value></Eq></And></Where>");
            else if (Mode == "Pending")
                queryInit = string.Concat("<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" +
                    username + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>");
            else if (Mode == "Approved")
                queryInit = string.Concat("<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" +
                    username + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>");
            else if (Mode == "Rejected")
                queryInit = string.Concat("<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" +
                    username + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></And></Where>");
            SPQuery oqueryInit = new SPQuery();
            oqueryInit.Query = queryInit;

            SPListItemCollection collitemsInit = olist1.GetItems(oqueryInit);
            if (collitemsInit != null && collitemsInit.Count > 0)
            {
                foreach (SPListItem itm in collitemsInit)
                {
                    string strStatus = Convert.ToString(itm["Status"]);

                    if (string.Equals(strStatus, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        if (!ohash.Contains(strRefno))
                        {
                            ohash.Add(strRefno, strRefno);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strManagerName = Convert.ToString(itm["ManagerName"]);
                            string strID = Convert.ToString(itm["ID"]);

                            GetDraftDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtDraftTable, strManagerName,Mode);
                        }
                    }
                    else if (string.Equals(strStatus, "Pending Approval", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        if (!ohash.Contains(strRefno))
                        {
                            ohash.Add(strRefno, strRefno);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strManagerName = GetApprover(itm);
                            GetPendingStatusDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtPending, strManagerName,Mode);
                        }
                    }
                    else if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        if (!ohash.Contains(strRefno))
                        {
                            ohash.Add(strRefno, strRefno);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strManagerName = GetApprover(itm);
                            GetPendingApprovedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtApproved, strManagerName,Mode);
                        }
                    }
                    else if (string.Equals(strStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        if (!ohash.Contains(strRefno))
                        {
                            ohash.Add(strRefno, strRefno);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strManagerName = GetApprover(itm);
                            GetPendingRejectedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit,strID, dtRejected, strManagerName,Mode);
                        }
                    }
                }
            }
        }
        
        private void GetCEODetails(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username, string Mode)
        {
            //Get approver for CEO
            string lstTravelSumm = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList splistTravelSumm = SPContext.Current.Site.RootWeb.GetList(lstTravelSumm);
            SPQuery oqueryTravelSumm = new SPQuery();

            //CEO Pending Approval
            if (Mode == "All" || Mode == "Pending")
            {
                string queryTravelSumm = string.Concat("<Where><And><Eq><FieldRef Name=\'PendingWith\' /><Value Type=\"Text\">CEO</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>");
                oqueryTravelSumm.Query = queryTravelSumm;

                SPListItemCollection collitemsTravelSumm = splistTravelSumm.GetItems(oqueryTravelSumm);
                if (collitemsTravelSumm != null && collitemsTravelSumm.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSumm)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                        string strTravellerName = Convert.ToString(itm["TravellerName"]);
                        string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                        string strID = Convert.ToString(itm["ID"]);
                        string strApprover = GetApprover(itm);
                        if (!ohash.Contains(strRefno))
                        {
                            GetPendingStatusDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtPending, strApprover,Mode);
                            ohash.Add(strRefno, strRefno);
                        }
                    }
                }
            }


            //CEO Approved
            if (Mode == "All" || Mode == "Approved")
            {
                string queryTravelSummApp = string.Concat("<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>");
                SPQuery oqueryTravelSummApp = new SPQuery();
                oqueryTravelSummApp.Query = queryTravelSummApp;

                SPListItemCollection collitemsTravelSummApp = splistTravelSumm.GetItems(oqueryTravelSummApp);
                if (collitemsTravelSummApp != null && collitemsTravelSummApp.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSummApp)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);

                        string lstTravelCommentsHistory = HrWebUtility.GetListUrl("TravelCommentsHistory");
                        SPList splistTravelCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstTravelCommentsHistory);
                        SPQuery oqueryTCH = new SPQuery();

                        //CEO Pending Approval
                        string queryTCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">CEO</Value></Eq><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq></And></Where>");
                        oqueryTCH.Query = queryTCH;

                        SPListItemCollection collitemsTCH = splistTravelCommentsHistory.GetItems(oqueryTCH);
                        if (collitemsTCH != null && collitemsTCH.Count > 0)
                        {
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingApprovedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtApproved, strApprover,Mode);
                            }
                        }
                    }
                }
            }

            //CEO Rejected
            if (Mode == "All" || Mode == "Rejected")
            {
                string queryTravelSummRej = string.Concat("<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></Where>");
                SPQuery oqueryTravelSummRej = new SPQuery();
                oqueryTravelSummRej.Query = queryTravelSummRej;

                SPListItemCollection collitemsTravelSummRej = splistTravelSumm.GetItems(oqueryTravelSummRej);
                if (collitemsTravelSummRej != null && collitemsTravelSummRej.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSummRej)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string lstTravelCommentsHistory = HrWebUtility.GetListUrl("TravelCommentsHistory");
                        SPList splistTravelCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstTravelCommentsHistory);
                        SPQuery oqueryTCH = new SPQuery();

                        //CEO Pending Approval
                        string queryTCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">CEO</Value></Eq><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq></And></Where>");
                        oqueryTCH.Query = queryTCH;

                        SPListItemCollection collitemsTCH = splistTravelCommentsHistory.GetItems(oqueryTCH);
                        if (collitemsTCH != null && collitemsTCH.Count > 0)
                        {
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingRejectedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtRejected, strApprover,Mode);
                            }
                        }
                    }
                }
            }
        }
        
        private void GetChairmanDetails(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username,String Mode)
        {
            
            string lstTravelSumm = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList splistTravelSumm = SPContext.Current.Site.RootWeb.GetList(lstTravelSumm);
            SPQuery oqueryTravelSumm = new SPQuery();

            //Chairman Pending Approval
            string queryTravelSumm = String.Empty;
            if (Mode == "All" || Mode == "Pending")
            {
                queryTravelSumm = string.Concat("<Where><And><Eq><FieldRef Name=\'PendingWith\' /><Value Type=\"Text\">Chairman</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>");

                oqueryTravelSumm.Query = queryTravelSumm;

                SPListItemCollection collitemsTravelSumm = splistTravelSumm.GetItems(oqueryTravelSumm);
                if (collitemsTravelSumm != null && collitemsTravelSumm.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSumm)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                        string strTravellerName = Convert.ToString(itm["TravellerName"]);
                        string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                        string strID = Convert.ToString(itm["ID"]);
                        string strApprover = GetApprover(itm);
                        if (!ohash.Contains(strRefno))
                        {
                            ohash.Add(strRefno, strRefno);
                            GetPendingStatusDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtPending, strApprover,Mode);
                        }
                    }
                }
            }

            //Chairman Approved
            string queryTravelSummApp = string.Empty;
            if (Mode == "All" || Mode == "Approved")
            {
                queryTravelSummApp = string.Concat("<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>");
                SPQuery oqueryTravelSummApp = new SPQuery();
                oqueryTravelSummApp.Query = queryTravelSummApp;

                SPListItemCollection collitemsTravelSummApp = splistTravelSumm.GetItems(oqueryTravelSummApp);
                if (collitemsTravelSummApp != null && collitemsTravelSummApp.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSummApp)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string lstTravelCommentsHistory = HrWebUtility.GetListUrl("TravelCommentsHistory");
                        SPList splistTravelCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstTravelCommentsHistory);
                        SPQuery oqueryTCH = new SPQuery();


                        string queryTCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">Chairman</Value></Eq><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq></And></Where>");
                        oqueryTCH.Query = queryTCH;

                        SPListItemCollection collitemsTCH = splistTravelCommentsHistory.GetItems(oqueryTCH);
                        if (collitemsTCH != null && collitemsTCH.Count > 0)
                        {
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingApprovedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtApproved, strApprover,Mode);
                            }
                        }
                    }
                }
            }

            //Chairman Rejected
            string queryTravelSummRej = string.Empty;
            if (Mode == "All" || Mode == "Rejected")
            {
                queryTravelSummRej = string.Concat("<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></Where>");
                SPQuery oqueryTravelSummRej = new SPQuery();
                oqueryTravelSummRej.Query = queryTravelSummRej;

                SPListItemCollection collitemsTravelSummRej = splistTravelSumm.GetItems(oqueryTravelSummRej);
                if (collitemsTravelSummRej != null && collitemsTravelSummRej.Count > 0)
                {
                    foreach (SPListItem itm in collitemsTravelSummRej)
                    {
                        string strRefno = Convert.ToString(itm["Title"]);
                        string lstTravelCommentsHistory = HrWebUtility.GetListUrl("TravelCommentsHistory");
                        SPList splistTravelCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstTravelCommentsHistory);
                        SPQuery oqueryTCH = new SPQuery();

                        string queryTCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">CEO</Value></Eq><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq></And></Where>");
                        oqueryTCH.Query = queryTCH;

                        SPListItemCollection collitemsTCH = splistTravelCommentsHistory.GetItems(oqueryTCH);
                        if (collitemsTCH != null && collitemsTCH.Count > 0)
                        {
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingRejectedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtRejected, strApprover,Mode);
                            }
                        }
                    }
                }
            }
        }

        private void GetCoordinatorDetails(DataTable dtPending, DataTable dtApproved, DataTable dtRejected, string username, String Mode)
        {
            string BusinessUnit = string.Empty;

            string lstTravelSumm = HrWebUtility.GetListUrl("HRWebTravelSummary");
            SPList splistTravelSumm = SPContext.Current.Site.RootWeb.GetList(lstTravelSumm);

            string lstURL2 = HrWebUtility.GetListUrl("TravelCoordinatorApprovalInfo");
            SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL2);
            SPQuery oquery1 = new SPQuery();
            oquery1.Query = string.Concat("<Where><Eq><FieldRef Name=\'TravelCoordinator\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>");
            SPListItemCollection collitems1 = olist2.GetItems(oquery1);
            foreach (SPListItem item in collitems1)
            {
                TaxonomyFieldValue value = item["BusinessUnit"] as TaxonomyFieldValue;
                if (value != null)
                {
                    BusinessUnit = value.Label;
                }

                if (Mode == "All" || Mode == "Pending")
                {
                    SPQuery oqueryTravelSumm = new SPQuery();
                    //TC Pending Approval
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    string queryTravelSumm = "<Where><And><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + BusinessUnit +
                                                           "</Value></Eq><Eq><FieldRef Name='PendingWith'/><Value Type='Text'>TC" +
                                                           "</Value></Eq></And><Eq><FieldRef Name='Status'/><Value Type='Text'>Pending Approval</Value></Eq></And></Where>";
                    oqueryTravelSumm.Query = queryTravelSumm;
                    SPListItemCollection collitemsTravelSumm = splistTravelSumm.GetItems(oqueryTravelSumm);
                    if (collitemsTravelSumm != null && collitemsTravelSumm.Count > 0)
                    {
                        foreach (SPListItem itm in collitemsTravelSumm)
                        {
                            string strRefno = Convert.ToString(itm["Title"]);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingStatusDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtPending, strApprover,Mode);
                            }
                        }
                    }
                }

                if (Mode == "All" || Mode == "Approved")
                {
                    SPQuery oqueryTravelSumm1 = new SPQuery();
                    //TC Approved
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    string queryTravelSumm1 = "<Where><And><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + BusinessUnit +
                                                           "</Value></Eq><Eq><FieldRef Name='PendingWith'/><Value Type='Text'>TC" +
                                                           "</Value></Eq></And><Eq><FieldRef Name='Status'/><Value Type='Text'>Approved</Value></Eq></And></Where>";
                    oqueryTravelSumm1.Query = queryTravelSumm1;
                    SPListItemCollection collitemsTravelSumm1 = splistTravelSumm.GetItems(oqueryTravelSumm1);
                    if (collitemsTravelSumm1 != null && collitemsTravelSumm1.Count > 0)
                    {
                        foreach (SPListItem itm in collitemsTravelSumm1)
                        {
                            string strRefno = Convert.ToString(itm["Title"]);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                            {
                                ohash.Add(strRefno, strRefno);
                                GetPendingApprovedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtApproved, strApprover,Mode);
                            }
                        }
                    }
                }

                if (Mode == "All" || Mode == "Rejected")
                {
                    SPQuery oqueryTravelSumm2 = new SPQuery();
                    //TC Rejected
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    string queryTravelSumm2 = "<Where><And><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + BusinessUnit +
                                                           "</Value></Eq><Eq><FieldRef Name='PendingWith'/><Value Type='Text'>TC" +
                                                           "</Value></Eq></And><Eq><FieldRef Name='Status'/><Value Type='Text'>Rejected</Value></Eq></And></Where>";
                    oqueryTravelSumm2.Query = queryTravelSumm2;
                    SPListItemCollection collitemsTravelSumm2 = splistTravelSumm.GetItems(oqueryTravelSumm2);
                    if (collitemsTravelSumm2 != null && collitemsTravelSumm2.Count > 0)
                    {
                        foreach (SPListItem itm in collitemsTravelSumm2)
                        {
                            string strRefno = Convert.ToString(itm["Title"]);
                            string strTypeofTravel = Convert.ToString(itm["TypeofTravel"]);
                            string strTravellerName = Convert.ToString(itm["TravellerName"]);
                            string strBUnit = Convert.ToString(itm["BusinessUnit"]);
                            string strID = Convert.ToString(itm["ID"]);
                            string strApprover = GetApprover(itm);
                            if (!ohash.Contains(strRefno))
                                GetPendingRejectedDetails(strRefno, strTypeofTravel, strTravellerName, strBUnit, strID, dtRejected, strApprover,Mode);
                        }
                    }
                }
            }
        }
        
        private string GetApprover(SPListItem item)
        {
            string strApprover = string.Empty;
            string PendingWith = Convert.ToString(item["PendingWith"]);
            string Status = Convert.ToString(item["Status"]);
            string strManager = string.Empty;
            ViewState["ApprovalLevel"] = "";
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
                ViewState["ApprovalLevel"] = "Manager";
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
                        strApprover = Convert.ToString(collitemsTravelAppInfo[0]["CEOApprover"]);
                        ViewState["ApprovalLevel"] = "CEO";
                    }
                    else if (PendingWith == "Chairman")
                    {
                        strApprover = Convert.ToString(collitemsTravelAppInfo[0]["ChairmanApprover"]);
                        ViewState["ApprovalLevel"] = "Chairman";
                    }
                    else if (PendingWith == "Initiator")
                    {
                        // SLT requests... In this case, show manager name who approved the request.
                        strApprover = strManager;
                        ViewState["ApprovalLevel"] = "Manager";
                    }
                    else if (PendingWith == "TC" || (PendingWith == "" && Status == "Approved"))
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
                            ViewState["ApprovalLevel"] = "Travel Coordinator";
                        }
                    }

                }
            }
            if (strApprover.Contains('#'))
                strApprover = strApprover.Split('#')[1];
            return strApprover;
        }
        
        private void GetPendingStatusDetails(string strRefno, string strTypeofTravel, string strTravellerName, string BU, string ID, DataTable dtGridTable, string username, string Mode)
        {
            DataRow dtGridRow = dtGridTable.NewRow();
            dtGridRow["TypeOfTravel"] = strTypeofTravel;
            dtGridRow["TravellerName"] = strTravellerName;

            string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TravelReview.aspx?refno=" + strRefno;
            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefno + "</a>";
            dtGridRow["BusinessUnit"] = BU;
            dtGridRow["ID"] = ID;
            string Approver = HrWebUtility.GetUserByEmailID(username);
            if (Convert.ToString(ViewState["ApprovalLevel"]) != "")
                Approver += " ("+Convert.ToString(ViewState["ApprovalLevel"])+")";
            dtGridRow["Approver"] = Approver;
            dtGridTable.Rows.Add(dtGridRow);

            dtGridTable.DefaultView.Sort = "ID DESC";
            if (Mode == "All")
            {
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
            }
            PendingApprovalGrid.DataSource = dtGridTable.DefaultView.ToTable();            
            PendingApprovalGrid.DataBind();
        }
        
        private void GetPendingApprovedDetails(string strRefno, string strTypeofTravel, string strTravellerName, string BU,string ID, DataTable dtGridTable, string username,string Mode)
        {
            DataRow dtGridRow = dtGridTable.NewRow();
            dtGridRow["TypeOfTravel"] = strTypeofTravel;
            dtGridRow["TravellerName"] = strTravellerName;

            string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TravelReview.aspx?refno=" + strRefno;
            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefno + "</a>";
            dtGridRow["BusinessUnit"] = BU;
            dtGridRow["ID"] = ID;
            string Approver = HrWebUtility.GetUserByEmailID(username);
            if (Convert.ToString(ViewState["ApprovalLevel"]) != "")
                Approver += " (" + Convert.ToString(ViewState["ApprovalLevel"]) + ")";
            dtGridRow["Approver"] = Approver;
            
            dtGridTable.Rows.Add(dtGridRow);

            dtGridTable.DefaultView.Sort = "ID DESC";
            /*if (Mode == "Draft" || Mode == "All")
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
            else if (Mode == "Approved" || Mode == "All")
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
            else if (Mode == "Pending" || Mode == "All")
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
            else if (Mode == "Rejected" || Mode == "All")
                Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;*/
            if (Mode == "All")
            {
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
            }

            ApprovedGrid.DataSource = dtGridTable.DefaultView.ToTable();            
            ApprovedGrid.DataBind();
        }
        
        private void GetPendingRejectedDetails(string strRefno, string strTypeofTravel, string strTravellerName, string BU,string ID, DataTable dtGridTable, string username,string Mode)
        {

            DataRow dtGridRow = dtGridTable.NewRow();
            dtGridRow["TypeOfTravel"] = strTypeofTravel;
            dtGridRow["TravellerName"] = strTravellerName;
            string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TravelReview.aspx?refno=" + strRefno;
            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefno + "</a>";
            dtGridRow["BusinessUnit"] = BU;
            dtGridRow["ID"] = ID;
            string Approver = HrWebUtility.GetUserByEmailID(username);
            if (Convert.ToString(ViewState["ApprovalLevel"]) != "")
                Approver += " (" + Convert.ToString(ViewState["ApprovalLevel"]) + ")";
            dtGridRow["Approver"] = Approver;
            dtGridTable.Rows.Add(dtGridRow);
            dtGridTable.DefaultView.Sort = "ID DESC";
            if (Mode == "All")
            {
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
            }
            RejectedGrid.DataSource = dtGridTable.DefaultView.ToTable();             
            RejectedGrid.DataBind();
        }
        
        private void GetDraftDetails(string strRefno, string strTypeofTravel, string strTravellerName, string BU,string ID, DataTable dtGridTable, string username,string Mode)
        {
            DataRow dtGridRow = dtGridTable.NewRow();
            dtGridRow["TypeOfTravel"] = strTypeofTravel;
            dtGridRow["TravellerName"] = strTravellerName;

            string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TravelRequest.aspx?refno=" + strRefno;
            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefno + "</a>";
            dtGridRow["BusinessUnit"] = BU;
            dtGridRow["ID"] = ID;
            string Approver = HrWebUtility.GetUserByEmailID(username);
            if (Convert.ToString(ViewState["ApprovalLevel"]) != "")
                Approver += " (" + Convert.ToString(ViewState["ApprovalLevel"]) + ")";
            dtGridRow["Approver"] = Approver;
            dtGridTable.Rows.Add(dtGridRow);
            dtGridTable.DefaultView.Sort = "ID DESC";
            if (Mode == "All")
            {
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
            }
            DraftGrid.DataSource = dtGridTable.DefaultView.ToTable();              
            DraftGrid.DataBind();
        }
        
        public static bool IsUserMemberOfGroup()
        {
            bool result = false;
            SPUser user = SPContext.Current.Web.CurrentUser;
            if (!String.IsNullOrEmpty("HR Services") && user != null)
            {
                foreach (SPGroup group in user.Groups)
                {
                    if (group.Name == "HR Services")
                    {
                        // found it
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        protected void DraftGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                string SortDir = string.Empty;
                if (Draftdir == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                    SortDir = "Desc";
                }
                else
                {
                    Draftdir = System.Web.UI.WebControls.SortDirection.Ascending;
                    SortDir = "Asc";
                }
                
                DataTable dtDraftTable = new DataTable();
                dtDraftTable.Columns.Add(new DataColumn("TypeOfTravel"));
                dtDraftTable.Columns.Add(new DataColumn("TravellerName"));
                dtDraftTable.Columns.Add(new DataColumn("FormNo"));
                dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                DataTable dtPending = new DataTable();
                dtPending.Columns.Add(new DataColumn("TypeOfTravel"));
                dtPending.Columns.Add(new DataColumn("TravellerName"));
                dtPending.Columns.Add(new DataColumn("FormNo"));
                dtPending.Columns.Add(new DataColumn("BusinessUnit"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();

                DataTable dtApproved = new DataTable();
                dtApproved.Columns.Add(new DataColumn("TypeOfTravel"));
                dtApproved.Columns.Add(new DataColumn("TravellerName"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                DataTable dtRejected = new DataTable();
                dtRejected.Columns.Add(new DataColumn("TypeOfTravel"));
                dtRejected.Columns.Add(new DataColumn("TravellerName"));
                dtRejected.Columns.Add(new DataColumn("FormNo"));
                dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
                dtRejected.Columns.Add(new DataColumn("Approver"));
                dtRejected.Columns.Add(new DataColumn("ID"));
                RejectedGrid.DataSource = dtRejected;
                RejectedGrid.DataBind();

                
                PopulateInitiatorInformation(UserName, dtDraftTable, dtPending, dtApproved, dtRejected, "Draft");

                DataView sortedView = new DataView(dtDraftTable);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                DraftGrid.DataSource = sortedView;
                DraftGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelWorkflowApproval.DraftGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void PendingApprovalGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {

                string SortDir = string.Empty;
                if (Pendingdir == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                    SortDir = "Desc";
                }
                else
                {
                    Pendingdir = System.Web.UI.WebControls.SortDirection.Ascending;
                    SortDir = "Asc";
                } 
                DataTable dtDraftTable = new DataTable();
                dtDraftTable.Columns.Add(new DataColumn("TypeOfTravel"));
                dtDraftTable.Columns.Add(new DataColumn("TravellerName"));
                dtDraftTable.Columns.Add(new DataColumn("FormNo"));
                dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                DataTable dtPending = new DataTable();
                dtPending.Columns.Add(new DataColumn("TypeOfTravel"));
                dtPending.Columns.Add(new DataColumn("TravellerName"));
                dtPending.Columns.Add(new DataColumn("FormNo"));
                dtPending.Columns.Add(new DataColumn("BusinessUnit"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();

                DataTable dtApproved = new DataTable();
                dtApproved.Columns.Add(new DataColumn("TypeOfTravel"));
                dtApproved.Columns.Add(new DataColumn("TravellerName"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                DataTable dtRejected = new DataTable();
                dtRejected.Columns.Add(new DataColumn("TypeOfTravel"));
                dtRejected.Columns.Add(new DataColumn("TravellerName"));
                dtRejected.Columns.Add(new DataColumn("FormNo"));
                dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
                dtRejected.Columns.Add(new DataColumn("Approver"));
                dtRejected.Columns.Add(new DataColumn("ID"));
                RejectedGrid.DataSource = dtRejected;
                RejectedGrid.DataBind();

                PopulateCEOInformation(dtPending, dtApproved, dtRejected, UserName, "Pending");
                PopulateMgrInformation(dtPending, dtApproved, dtRejected, UserName, "Pending");
                PopulateChairmanInformation(dtPending, dtApproved, dtRejected, UserName, "Pending");
                PopulateTCInformation(dtPending, dtApproved, dtRejected, UserName, "Pending");
                PopulateInitiatorInformation(UserName, dtDraftTable, dtPending, dtApproved, dtRejected, "Pending");

                DataView sortedView = new DataView(dtPending);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                PendingApprovalGrid.DataSource = sortedView;
                PendingApprovalGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelWorkflowApproval.PendingApprovalGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void ApprovedGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                string SortDir = string.Empty;
                if (Approveddir == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                    SortDir = "Desc";
                }
                else
                {
                    Approveddir = System.Web.UI.WebControls.SortDirection.Ascending;
                    SortDir = "Asc";
                } 
                DataTable dtDraftTable = new DataTable();
                dtDraftTable.Columns.Add(new DataColumn("TypeOfTravel"));
                dtDraftTable.Columns.Add(new DataColumn("TravellerName"));
                dtDraftTable.Columns.Add(new DataColumn("FormNo"));
                dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                DataTable dtPending = new DataTable();
                dtPending.Columns.Add(new DataColumn("TypeOfTravel"));
                dtPending.Columns.Add(new DataColumn("TravellerName"));
                dtPending.Columns.Add(new DataColumn("FormNo"));
                dtPending.Columns.Add(new DataColumn("BusinessUnit"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();

                DataTable dtApproved = new DataTable();
                dtApproved.Columns.Add(new DataColumn("TypeOfTravel"));
                dtApproved.Columns.Add(new DataColumn("TravellerName"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                DataTable dtRejected = new DataTable();
                dtRejected.Columns.Add(new DataColumn("TypeOfTravel"));
                dtRejected.Columns.Add(new DataColumn("TravellerName"));
                dtRejected.Columns.Add(new DataColumn("FormNo"));
                dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
                dtRejected.Columns.Add(new DataColumn("Approver"));
                dtRejected.Columns.Add(new DataColumn("ID"));
                RejectedGrid.DataSource = dtRejected;
                RejectedGrid.DataBind();

                PopulateCEOInformation(dtPending, dtApproved, dtRejected, UserName, "Approved");
                PopulateMgrInformation(dtPending, dtApproved, dtRejected, UserName, "Approved");
                PopulateChairmanInformation(dtPending, dtApproved, dtRejected, UserName, "Approved");
                PopulateTCInformation(dtPending, dtApproved, dtRejected, UserName, "Approved");
                PopulateInitiatorInformation(UserName, dtDraftTable, dtPending, dtApproved, dtRejected, "Approved");

                DataView sortedView = new DataView(dtApproved);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                ApprovedGrid.DataSource = sortedView;
                ApprovedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelWorkflowApproval.ApprovedGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void RejectedGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                string SortDir = string.Empty;
                if (Approveddir == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                    SortDir = "Desc";
                }
                else
                {
                    Approveddir = System.Web.UI.WebControls.SortDirection.Ascending;
                    SortDir = "Asc";
                } 
                DataTable dtDraftTable = new DataTable();
                dtDraftTable.Columns.Add(new DataColumn("TypeOfTravel"));
                dtDraftTable.Columns.Add(new DataColumn("TravellerName"));
                dtDraftTable.Columns.Add(new DataColumn("FormNo"));
                dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                DataTable dtPending = new DataTable();
                dtPending.Columns.Add(new DataColumn("TypeOfTravel"));
                dtPending.Columns.Add(new DataColumn("TravellerName"));
                dtPending.Columns.Add(new DataColumn("FormNo"));
                dtPending.Columns.Add(new DataColumn("BusinessUnit"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();

                DataTable dtApproved = new DataTable();
                dtApproved.Columns.Add(new DataColumn("TypeOfTravel"));
                dtApproved.Columns.Add(new DataColumn("TravellerName"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                DataTable dtRejected = new DataTable();
                dtRejected.Columns.Add(new DataColumn("TypeOfTravel"));
                dtRejected.Columns.Add(new DataColumn("TravellerName"));
                dtRejected.Columns.Add(new DataColumn("FormNo"));
                dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
                dtRejected.Columns.Add(new DataColumn("Approver"));
                dtRejected.Columns.Add(new DataColumn("ID"));
                RejectedGrid.DataSource = dtRejected;
                RejectedGrid.DataBind();

                PopulateCEOInformation(dtPending, dtApproved, dtRejected, UserName, "Rejected");
                PopulateMgrInformation(dtPending, dtApproved, dtRejected, UserName, "Rejected");
                PopulateChairmanInformation(dtPending, dtApproved, dtRejected, UserName, "Rejected");
                PopulateTCInformation(dtPending, dtApproved, dtRejected, UserName, "Rejected");
                PopulateInitiatorInformation(UserName, dtDraftTable, dtPending, dtApproved, dtRejected, "All");

                DataView sortedView = new DataView(dtRejected);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                RejectedGrid.DataSource = sortedView;
                RejectedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TravelWorkflowApproval.RejectedGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        public System.Web.UI.WebControls.SortDirection Approveddir
        {
            get
            {
                if (ViewState["ApproveddirState"] == null)
                {
                    ViewState["ApproveddirState"] = System.Web.UI.WebControls.SortDirection.Ascending;
                }
                return (System.Web.UI.WebControls.SortDirection)ViewState["ApproveddirState"];
            }
            set
            {
                ViewState["ApproveddirState"] = value;
            }
        }

        public System.Web.UI.WebControls.SortDirection Pendingdir
        {
            get
            {
                if (ViewState["PendingdirState"] == null)
                {
                    ViewState["PendingdirState"] = System.Web.UI.WebControls.SortDirection.Ascending;
                }
                return (System.Web.UI.WebControls.SortDirection)ViewState["PendingdirState"];
            }
            set
            {
                ViewState["PendingdirState"] = value;
            }
        }
        public System.Web.UI.WebControls.SortDirection Draftdir
        {
            get
            {
                if (ViewState["DraftdirState"] == null)
                {
                    ViewState["DraftdirState"] = System.Web.UI.WebControls.SortDirection.Ascending;
                }
                return (System.Web.UI.WebControls.SortDirection)ViewState["DraftdirState"];
            }
            set
            {
                ViewState["DraftdirState"] = value;
            }
        }
        public System.Web.UI.WebControls.SortDirection Rejecteddir
        {
            get
            {
                if (ViewState["RejecteddirState"] == null)
                {
                    ViewState["RejecteddirState"] = System.Web.UI.WebControls.SortDirection.Ascending;
                }
                return (System.Web.UI.WebControls.SortDirection)ViewState["RejecteddirState"];
            }
            set
            {
                ViewState["RejecteddirState"] = value;
            }
        }
    }
}
