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
    public partial class NewHireWorkflowApproval : WebPartPage
    {
        string UserName = string.Empty;
        Hashtable ohash = new Hashtable();
        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                WorkFlowlblError.Text = string.Empty;
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
                LogUtility.LogError("NewHireWorkflowApproval.Page_Load", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void VerifyUser(string username)
        {
            DataTable dtDraftTable = new DataTable();
            dtDraftTable.Columns.Add(new DataColumn("DateSubmitted"));
            dtDraftTable.Columns.Add(new DataColumn("Initiator"));
            dtDraftTable.Columns.Add(new DataColumn("FormNo"));
            dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
            dtDraftTable.Columns.Add(new DataColumn("Role"));
            dtDraftTable.Columns.Add(new DataColumn("Approver"));
            dtDraftTable.Columns.Add(new DataColumn("ID"));
            DraftGrid.DataSource = dtDraftTable;
            DraftGrid.DataBind();

            DataTable dtPending = new DataTable();
            dtPending.Columns.Add(new DataColumn("DateSubmitted"));
            dtPending.Columns.Add(new DataColumn("Initiator"));
            dtPending.Columns.Add(new DataColumn("FormNo"));
            dtPending.Columns.Add(new DataColumn("BusinessUnit"));
            dtPending.Columns.Add(new DataColumn("Role"));
            dtPending.Columns.Add(new DataColumn("Approver"));
            dtPending.Columns.Add(new DataColumn("ID"));
            PendingApprovalGrid.DataSource = dtDraftTable;
            PendingApprovalGrid.DataBind();

            DataTable dtApproved = new DataTable();
            dtApproved.Columns.Add(new DataColumn("DateApproved"));
            dtApproved.Columns.Add(new DataColumn("Initiator"));
            dtApproved.Columns.Add(new DataColumn("FormNo"));
            dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
            dtApproved.Columns.Add(new DataColumn("Role"));
            dtApproved.Columns.Add(new DataColumn("Approver"));
            dtApproved.Columns.Add(new DataColumn("ID"));
            ApprovedGrid.DataSource = dtApproved;
            ApprovedGrid.DataBind();

            DataTable dtRejected = new DataTable();
            dtRejected.Columns.Add(new DataColumn("DateApproved"));
            dtRejected.Columns.Add(new DataColumn("Initiator"));
            dtRejected.Columns.Add(new DataColumn("FormNo"));
            dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
            dtRejected.Columns.Add(new DataColumn("Role"));
            dtRejected.Columns.Add(new DataColumn("RejectedBy"));
            dtRejected.Columns.Add(new DataColumn("ID"));
            RejectedGrid.DataSource = dtRejected;
            RejectedGrid.DataBind();
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {

                       bool IsHRServiceUser = IsUserMemberOfGroup();
                       if (IsHRServiceUser)
                       {
                           GetPendingStatusDetailsForHR(dtPending,true);
                           GetApprovedStatusDetailsForHR(dtApproved,true);
                           GetRejectedStatusDetailsForHR(dtRejected,true);
                       }
                        bool bVehicleApprover = IsVehicleApprover(username);
                        if (bVehicleApprover)
                        {
                            GetPendingStatusDetailsForVC(dtPending,true);
                            GetApprovedStatusDetailsForVC(dtApproved,true);
                            GetRejectedStatusDetailsForVC(dtRejected,true);
                        }
                           
                        string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");

                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                        SPQuery oquery = new SPQuery();
                        string query = string.Concat("<Where><Eq><FieldRef Name='Approver'/><Value Type='User'>" +
                            username + "</Value></Eq></Where>");
                        oquery.Query = query;
                        SPListItemCollection collitems = olist1.GetItems(oquery);
                        if (collitems != null && collitems.Count > 0)
                        {
                            foreach (SPListItem itm in collitems)
                            {
                                string value = Convert.ToString(itm["BusinessUnit"]);
                                string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                                SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                                SPQuery queryPostion = new SPQuery();
                                value = value.Split('|')[0];
                                // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                                queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value + "</Value></Eq></Where>";
                                SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                                if (collitemsPosition != null && collitemsPosition.Count > 0)
                                {
                                    foreach (SPListItem itmPostion in collitemsPosition)
                                    {
                                        string strRefNo = Convert.ToString(itmPostion["Title"]);
                                        
                                            //GetDraftStatusDetailsByRefNo(strRefNo, username, dtDraftTable);
                                            GetPendingStatusDetailsByRefno(strRefNo, username, dtPending, value,true);
                                            GetApprovedStatusDetails(strRefNo, username, dtApproved,true);
                                            GetRejectedStatusDetails(strRefNo, username, dtRejected,true);
                                               
                                    }
                                }

                            }
                            //GetDraftStatusDetailsByAuthor(username, dtDraftTable);
                        }
                               
                       GetDraftStatusDetailsByAuthor(username, dtDraftTable,true);
                       GetPendingStatusDetailsByAuthor(username, dtPending,true);
                       GetApprovedStatusDetailsByAuthor(username, dtApproved,true);
                       GetRejectedStatusDetailsByAuthor(username, dtRejected,true);
                   });
        }

        private bool IsVehicleApprover(string username)
        {
            bool bVApprover = false;
            string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                       SPQuery oquery = new SPQuery();
                       string query = string.Concat("<Where><Eq><FieldRef Name='VehicleApprover'/><Value Type='User'>" +
                           username + "</Value></Eq></Where>");
                       oquery.Query = query;
                       oquery.RowLimit = 100;
                       SPListItemCollection collitems = olist1.GetItems(oquery);
                       if (collitems != null && collitems.Count > 0)
                       {
                           bVApprover = true;
                       }
                   });
            return bVApprover;
        }

        private void GetPendingStatusDetailsForVC(DataTable dtPending, bool Sort)
        {
            string strReferenceNo = "";

            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'ApprovalStatus\' /><Value Type=\"Text\">Vehicle</Value></Eq>" +
                       "<Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                       "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);

                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtPending.NewRow();
                               dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";
                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem["BusinessUnit"] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem["BusinessUnit"]);
                                   dtGridRow["BusinessUnit"] = value;

                                   dtGridRow["Approver"] = GetNextApprover(value, currapprover);
                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                               }
                               ohash.Add(strReferenceNo, strReferenceNo);
                               dtPending.Rows.Add(dtGridRow);
                           }
                       }
                       dtPending.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                       PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                       PendingApprovalGrid.DataBind();
                   });
        }

        private void GetApprovedStatusDetailsForVC(DataTable dtApproved,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);
                           if (!ohash.Contains(strReferenceNo))
                           {
                               string lstCommentsHistory = HrWebUtility.GetListUrl("NewHireApprovalHistory");
                               SPList splistCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstCommentsHistory);
                               SPQuery oqueryCH = new SPQuery();

                               string queryCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">Vehicle</Value></Eq>" +
                                   "<Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></And></Where>");
                               oqueryCH.Query = queryCH;

                               SPListItemCollection collitemsTCH = splistCommentsHistory.GetItems(oqueryCH);
                               if (collitemsTCH != null && collitemsTCH.Count > 0)
                               {
                                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                                   DataRow dtGridRow = dtApproved.NewRow();
                                   dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                                   dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                                   dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                                   dtGridRow["Approver"] = "HR Services";


                                   string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                                   dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                                   string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                                   SPQuery oquery1 = new SPQuery();
                                   oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                                   SPListItemCollection collectionitems = olist1.GetItems(oquery1);
                                   foreach (SPListItem ListItem in collectionitems)
                                   {
                                       /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                       string value = Convert.ToString(ListItem[MetadataField]);
                                       dtGridRow["BusinessUnit"] = value;

                                       if (ListItem["Role"] != null)
                                           dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                       else
                                           dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                                   }
                                   ohash.Add(strReferenceNo, strReferenceNo);
                                   dtApproved.Rows.Add(dtGridRow);
                               }
                           }
                       }
                       dtApproved.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                       ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                       ApprovedGrid.DataBind();
                   });
        }

        private void GetRejectedStatusDetailsForVC(DataTable dtRejected,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);
                           if (!ohash.Contains(strReferenceNo))
                           {
                               string lstCommentsHistory = HrWebUtility.GetListUrl("NewHireApprovalHistory");
                               SPList splistCommentsHistory = SPContext.Current.Site.RootWeb.GetList(lstCommentsHistory);
                               SPQuery oqueryCH = new SPQuery();

                               string queryCH = string.Concat("<Where><And><Eq><FieldRef Name=\'ApproverStep\' /><Value Type=\"Text\">Vehicle</Value></Eq>" +
                                   "<Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></And></Where>");
                               oqueryCH.Query = queryCH;

                               SPListItemCollection collitemsTCH = splistCommentsHistory.GetItems(oqueryCH);
                               if (collitemsTCH != null && collitemsTCH.Count > 0)
                               {
                                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                                   DataRow dtGridRow = dtRejected.NewRow();
                                   dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                                   dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                                   dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                                   if(Convert.ToString(listitem["RejectedBy"])!="")
                                       dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";


                                   string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                                   dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                                   string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                                   SPQuery oquery1 = new SPQuery();
                                   oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                                   SPListItemCollection collectionitems = olist1.GetItems(oquery1);
                                   foreach (SPListItem ListItem in collectionitems)
                                   {
                                       /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                       string value = Convert.ToString(ListItem[MetadataField]);
                                       dtGridRow["BusinessUnit"] = value;

                                       if (ListItem["Role"] != null)
                                           dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                       else
                                           dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                                   }
                                   ohash.Add(strReferenceNo, strReferenceNo);
                                   dtRejected.Rows.Add(dtGridRow);
                               }
                           }
                       }
                       dtRejected.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
                       RejectedGrid.DataSource = dtRejected.DefaultView.ToTable();
                       RejectedGrid.DataBind();
                   });
        }

        private string GetNextApprover(string businessunit, string currapprover)
        {
            string Approver = string.Empty;
            string ApproverlstURL = HrWebUtility.GetListUrl("NewHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList applist = SPContext.Current.Site.RootWeb.GetList(ApproverlstURL);
                       SPQuery appquery = new SPQuery();
                       // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                       appquery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit + "</Value></Eq></Where>";

                       SPListItemCollection appcollectionitems = applist.GetItems(appquery);

                       foreach (SPListItem appListItem in appcollectionitems)
                       {
                           if (currapprover == "HRManager")
                           {
                               Approver = GetApprover(Convert.ToString(appListItem["Approver"])) + " (HR Manager)";
                           }
                           else if (currapprover == "Vehicle")
                           {
                               Approver = GetApprover(Convert.ToString(appListItem["VehicleApprover"])) + " (Vehicle)";
                           }
                           else if (currapprover == "HRServices")
                           {
                               Approver = "HR Services";
                           }
                       }
                   });
            return Approver;
        }

        private string GetApproverLevel(string businessunit, string currapprover)
        {
            string Approver = string.Empty;
            string ApproverlstURL = HrWebUtility.GetListUrl("NewHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList applist = SPContext.Current.Site.RootWeb.GetList(ApproverlstURL);
                SPQuery appquery = new SPQuery();
                // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                appquery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit + "</Value></Eq></Where>";

                SPListItemCollection appcollectionitems = applist.GetItems(appquery);

                foreach (SPListItem appListItem in appcollectionitems)
                {
                    if (currapprover == "HRManager")
                    {
                        Approver = "HR Manage)";
                    }
                    else if (currapprover == "Vehicle")
                    {
                        Approver = "Vehicle";
                    }
                    else if (currapprover == "HRServices")
                    {
                        Approver = "HR Services";
                    }
                }
            });
            return Approver;
        }

        private void GetPendingStatusDetailsForHR(DataTable dtPending,bool Sort)
        {
            string strReferenceNo = "";

            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPQuery oquery = new SPQuery();
                       /*oquery.Query = "<Where><And><Eq><FieldRef Name=\'ApprovalStatus\' /><Value Type=\"Text\">HRServices</Value></Eq>" +
                       "<Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                       "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";*/

                       oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></Where>" +
                       "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);
                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtPending.NewRow();
                               dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);


                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";
                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem["BusinessUnit"] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem["BusinessUnit"]);
                                   dtGridRow["BusinessUnit"] = value;

                                   dtGridRow["Approver"] = GetNextApprover(value, currapprover);
                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                               }
                               ohash.Add(strReferenceNo, strReferenceNo);
                               dtPending.Rows.Add(dtGridRow);
                           }
                       }
                       dtPending.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                       PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                       PendingApprovalGrid.DataBind();
                   });
        }

        private void GetApprovedStatusDetailsForHR(DataTable dtApproved,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);
                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtApproved.NewRow();
                               dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                               dtGridRow["Approver"] = "HR Services";

                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;

                                   dtGridRow["Approver"] = "HR Services";

                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                               }
                               ohash.Add(strReferenceNo, strReferenceNo);
                               dtApproved.Rows.Add(dtGridRow);
                           }
                       }
                       dtApproved.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                       ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                       ApprovedGrid.DataBind();
                   });
        }

        private void GetRejectedStatusDetailsForHR(DataTable dtRejected,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq>" +
                       "<Eq><FieldRef Name=\'RejectedBy\' /><Value Type=\"Text\">HRServices</Value></Eq></And></Where>" +
                       "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);

                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtRejected.NewRow();
                               dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                               if (Convert.ToString(listitem["RejectedBy"]) != "")
                                   dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";

                               

                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;

                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                               }
                               ohash.Add(strReferenceNo, strReferenceNo);
                               dtRejected.Rows.Add(dtGridRow);
                           }
                       }
                       dtRejected.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
                       RejectedGrid.DataSource = dtRejected.DefaultView.ToTable();
                       RejectedGrid.DataBind();
                   });
        }

        private SPListItemCollection GetGeneralInfoItems(string strRefno, string strStatus)
        {
            SPListItemCollection collitemsGeneralInfo = null;
            SPList splstGeneralInfo = SPContext.Current.Site.RootWeb.GetList("NewHireGeneralInfo");
            SPQuery queryGeneralInfo = new SPQuery();
            queryGeneralInfo.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">" + strStatus + "</Value></Eq></And></Where>";
            return collitemsGeneralInfo = splstGeneralInfo.GetItems(queryGeneralInfo);

        }
        private void GetDraftStatusDetailsByAuthor(string strUserName, DataTable dtGridTable,bool Sort)
        {

            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Draft</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {

                           strReferenceNo = Convert.ToString(listitem["RefNo"]);

                           if (!ohash.Contains(strReferenceNo))
                           {

                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                               DataRow dtGridRow = dtGridTable.NewRow();


                               dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireRequest.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                   "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/

                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;


                                   dtGridRow["Approver"] = GetNextApprover(value, currapprover);

                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);

                               }

                               dtGridTable.Rows.Add(dtGridRow);
                               ohash.Add(strReferenceNo, strReferenceNo);
                           }
                           dtGridTable.DefaultView.Sort = "ID DESC";
                           if (Sort)
                               Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                           DraftGrid.DataSource = dtGridTable.DefaultView.ToTable();
                           DraftGrid.DataBind();
                       }
                   });
        }


        private void GetPendingStatusDetailsByAuthor(string strUserName, DataTable dtGridTable,bool Sort)
        {

            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);

                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                               DataRow dtGridRow = dtGridTable.NewRow();


                               dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                   "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;

                                   dtGridRow["Approver"] = GetNextApprover(value, currapprover);

                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);

                               }

                               dtGridTable.Rows.Add(dtGridRow);
                               ohash.Add(strReferenceNo, strReferenceNo);
                           }
                       }
                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                       PendingApprovalGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       PendingApprovalGrid.DataBind();
                   });
        }

        private void GetApprovedStatusDetailsByAuthor(string strUserName, DataTable dtGridTable,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {

                           strReferenceNo = Convert.ToString(listitem["RefNo"]);

                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtGridTable.NewRow();
                               dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                   "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;

                                   dtGridRow["Approver"] = "HR Services";

                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);

                               }
                               dtGridTable.Rows.Add(dtGridRow);
                               ohash.Add(strReferenceNo, strReferenceNo);
                           }
                       }

                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                       ApprovedGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       ApprovedGrid.DataBind();
                   });
        }

        private void GetRejectedStatusDetailsByAuthor(string strUserName, DataTable dtGridTable,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       foreach (SPListItem listitem in collitems)
                       {
                           strReferenceNo = Convert.ToString(listitem["RefNo"]);
                           if (!ohash.Contains(strReferenceNo))
                           {
                               string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                               DataRow dtGridRow = dtGridTable.NewRow();
                               dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                               dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                               dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                               if (Convert.ToString(listitem["RejectedBy"]) != "")
                                   dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";

                               

                               string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                               dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                               string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                               SPQuery oquery1 = new SPQuery();
                               oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                   "</Value></Eq></Where>";

                               SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                               foreach (SPListItem ListItem in collectionitems)
                               {
                                   // TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;
                                   string value = Convert.ToString(ListItem[MetadataField]);
                                   dtGridRow["BusinessUnit"] = value;


                                   if (ListItem["Role"] != null)
                                       dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                   else
                                       dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                               }
                               dtGridTable.Rows.Add(dtGridRow);
                               ohash.Add(strReferenceNo, strReferenceNo);
                           }
                       }
                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
                       RejectedGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       RejectedGrid.DataBind();
                   });
        }

        private void GetPendingStatusDetailsByRefno(string strRefno, string strUserName, DataTable dtGridTable, string BusinessUnit,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'RefNo\' /><Value Type=\"Text\">" + strRefno +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       if (collitems != null && collitems.Count > 0)
                       {

                           foreach (SPListItem listitem in collitems)
                           {
                               if (!ohash.Contains(strRefno))
                               {
                                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                                   string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");
                                   SPList olist5 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                                   SPQuery oquery5 = new SPQuery();
                                   //TaxonomyFieldValue value = listitem["PositionType"] as TaxonomyFieldValue;
                                   string value = Convert.ToString(listitem["PositionType"]);

                                   DataRow dtGridRow = dtGridTable.NewRow();
                                   dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["Date"]).ToString("dd/MM/yyyy");
                                   dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                                   dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                                   strReferenceNo = Convert.ToString(listitem["RefNo"]);
                                   string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                                   dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                                   string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                                   SPQuery oquery1 = new SPQuery();
                                   oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                       "</Value></Eq></Where>";

                                   SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                                   foreach (SPListItem ListItem in collectionitems)
                                   {
                                       // TaxonomyFieldValue BuValue = ListItem[MetadataField] as TaxonomyFieldValue;
                                       string BuValue = Convert.ToString(ListItem[MetadataField]);
                                       dtGridRow["BusinessUnit"] = BuValue;
                                       dtGridRow["Approver"] = GetNextApprover(BuValue, currapprover);

                                       if (ListItem["Role"] != null)
                                           dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                       else
                                           dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                                   }
                                   dtGridTable.Rows.Add(dtGridRow);
                                   ohash.Add(strRefno, strRefno);
                                   //}
                               }
                           }
                       }
                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                       PendingApprovalGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       PendingApprovalGrid.DataBind();
                   });
        }

        private void GetApprovedStatusDetails(string strRefno, string strUserName, DataTable dtGridTable, bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'RefNo\' /><Value Type=\"Text\">" + strRefno +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);



                       if (collitems != null && collitems.Count > 0)
                       {
                           foreach (SPListItem listitem in collitems)
                           {
                               if (!ohash.Contains(strRefno))
                               {
                                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                                   DataRow dtGridRow = dtGridTable.NewRow();
                                   dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                                   dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                                   dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                                   strReferenceNo = Convert.ToString(listitem["RefNo"]);
                                   string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                                   dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                                   string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                                   SPQuery oquery1 = new SPQuery();
                                   oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                       "</Value></Eq></Where>";

                                   SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                                   foreach (SPListItem ListItem in collectionitems)
                                   {
                                       // TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;
                                       string value = Convert.ToString(ListItem[MetadataField]);
                                       dtGridRow["BusinessUnit"] = value;

                                       dtGridRow["Approver"] = "HR Services";
                                       if (ListItem["Role"] != null)
                                           dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                       else
                                           dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                                   }
                                   dtGridTable.Rows.Add(dtGridRow);
                                   ohash.Add(strRefno, strRefno);
                               }
                           }

                       }
                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                       ApprovedGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       ApprovedGrid.DataBind();
                   });
        }

        private void GetRejectedStatusDetails(string strRefno, string strUserName, DataTable dtGridTable,bool Sort)
        {
            string strReferenceNo = "";

            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'RefNo\' /><Value Type=\"Text\">" + strRefno +
                           "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></And></Where>" +
                           "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                       SPListItemCollection collitems = olist.GetItems(oquery);

                       if (collitems != null && collitems.Count > 0)
                       {
                           foreach (SPListItem listitem in collitems)
                           {
                               if (!ohash.Contains(strRefno))
                               {
                                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                                   DataRow dtGridRow = dtGridTable.NewRow();
                                   if (Convert.ToString(listitem["RejectedBy"]) != "")
                                       dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";

                                   dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                                   dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                                   dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                                   strReferenceNo = Convert.ToString(listitem["RefNo"]);
                                   string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/NewHireReview.aspx?refno=" + strReferenceNo;
                                   dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                                   string PstDtlslstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                                   SPQuery oquery1 = new SPQuery();
                                   oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                                       "</Value></Eq></Where>";

                                   SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                                   foreach (SPListItem ListItem in collectionitems)
                                   {
                                       // TaxonomyFieldValue value = ListItem["BusinessUnit"] as TaxonomyFieldValue;
                                       string value = Convert.ToString(ListItem["BusinessUnit"]);
                                       dtGridRow["BusinessUnit"] = value;


                                       if (ListItem["Role"] != null)
                                           dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                                       else
                                           dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                                   }
                                   dtGridTable.Rows.Add(dtGridRow);
                                   ohash.Add(strRefno, strRefno);

                               }
                           }

                       }
                       dtGridTable.DefaultView.Sort = "ID DESC";
                       if (Sort)
                           Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
                       RejectedGrid.DataSource = dtGridTable.DefaultView.ToTable();
                       RejectedGrid.DataBind();
                   });
        }


        private string GetUser(string strAuthor)
        {
            string strName = "";
            string[] tmparr = strAuthor.Split('|');
            strAuthor = tmparr[tmparr.Length - 1].Trim();
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

        private string GetApprover(string strAuthor)
        {
            string strName = "";
            string[] tmparr = strAuthor.Split('|');
            strAuthor = tmparr[tmparr.Length - 1];
            if (strAuthor != "")
            {
                if (strAuthor.Contains("#"))
                    strAuthor = strAuthor.Split('#')[1].Trim();
                using (HostingEnvironment.Impersonate())
                {
                    using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
                    {

                        PrincipalContext context1 = new PrincipalContext(ContextType.Domain);

                        //string strUserEmailID = strAuthor.Substring(strAuthor.IndexOf('#') + 1);

                        string strUser = strAuthor.Substring(0, strAuthor.IndexOf('@'));
                        //string userName = userWithoutDomain.Substring(userWithoutDomain.IndexOf('#') + 1);

                        // Below code was written to display current logged in user, if approver's name doesn't have proper entry in AD to get FN and LN
                        //string strUserName = SPContext.Current.Web.CurrentUser.LoginName;
                        //strName = strUserName;

                        // Replaced above code with below, just to display email id instead.
                        strName = strAuthor;

                        UserPrincipal foundUser =
                            UserPrincipal.FindByIdentity(context1, strUser);
                        if (foundUser != null)
                        {
                            DirectoryEntry directoryEntry = foundUser.GetUnderlyingObject() as DirectoryEntry;

                            DirectorySearcher searcher = new DirectorySearcher(directoryEntry);


                            searcher.Filter = string.Format("(mail={0})", strAuthor);

                            SearchResult result = searcher.FindOne();

                            strName = result.Properties["name"][0].ToString();
                        }
                    }
                }
            }
            return strName;
        }

        /*private bool IsUserInHRService(string username)
        {
            bool bValid = false;
            using (HostingEnvironment.Impersonate())
            {
                using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
                {

                    PrincipalContext context1 = new PrincipalContext(ContextType.Domain);

                    if (username.Contains("@"))
                        username = username.Split('@')[0].Trim();

                    UserPrincipal foundUser =
                        UserPrincipal.FindByIdentity(context1, username);

                    foreach (Principal p in foundUser.GetGroups())
                    {
                        if (p.DisplayName == "HR Services")
                        {
                            bValid = true;
                            break;
                        }
                    }
                    
                }
            }
            return true;
        }*/

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

        protected void RejectedGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                string SortDir = string.Empty;
                if (Rejecteddir == System.Web.UI.WebControls.SortDirection.Ascending)
                {
                    Rejecteddir = System.Web.UI.WebControls.SortDirection.Descending;
                    SortDir = "Desc";
                }
                else
                {
                    Rejecteddir = System.Web.UI.WebControls.SortDirection.Ascending;
                    SortDir = "Asc";
                }

                DataTable dtRejected = new DataTable();
                dtRejected.Columns.Add(new DataColumn("DateApproved"));
                dtRejected.Columns.Add(new DataColumn("Initiator"));
                dtRejected.Columns.Add(new DataColumn("FormNo"));
                dtRejected.Columns.Add(new DataColumn("BusinessUnit"));
                dtRejected.Columns.Add(new DataColumn("Role"));
                dtRejected.Columns.Add(new DataColumn("RejectedBy"));
                dtRejected.Columns.Add(new DataColumn("ID"));
                RejectedGrid.DataSource = dtRejected;
                RejectedGrid.DataBind();

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    bool IsHRServiceUser = IsUserMemberOfGroup();
                    if (IsHRServiceUser)
                    {
                        GetRejectedStatusDetailsForHR(dtRejected, false);
                    }
                    bool bVehicleApprover = IsVehicleApprover(UserName);
                    if (bVehicleApprover)
                    {
                        GetRejectedStatusDetailsForVC(dtRejected, false);
                    }

                    string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");

                    SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                    SPQuery oquery = new SPQuery();
                    string query = string.Concat("<Where><Eq><FieldRef Name='Approver'/><Value Type='User'>" +
                        UserName + "</Value></Eq></Where>");
                    oquery.Query = query;
                    SPListItemCollection collitems = olist1.GetItems(oquery);
                    if (collitems != null && collitems.Count > 0)
                    {
                        foreach (SPListItem itm in collitems)
                        {
                            string value = Convert.ToString(itm["BusinessUnit"]);
                            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                            SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                            SPQuery queryPostion = new SPQuery();
                            value = value.Split('|')[0];
                            // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                            queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" +
                                value + "</Value></Eq></Where>";
                            SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                            if (collitemsPosition != null && collitemsPosition.Count > 0)
                            {
                                foreach (SPListItem itmPostion in collitemsPosition)
                                {
                                    string strRefNo = Convert.ToString(itmPostion["Title"]);

                                    GetRejectedStatusDetails(strRefNo, UserName, dtRejected, false);
                                }
                            }
                        }
                    }
                    GetRejectedStatusDetailsByAuthor(UserName, dtRejected, false);
                });
                
                DataView sortedView = new DataView(dtRejected);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                RejectedGrid.DataSource = sortedView;
                RejectedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("NewHireWorkflowApproval.RejectedGrid_Sorting", ex.Message);
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

                DataTable dtApproved = new DataTable();
                dtApproved.Columns.Add(new DataColumn("DateApproved"));
                dtApproved.Columns.Add(new DataColumn("Initiator"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("Role"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    bool IsHRServiceUser = IsUserMemberOfGroup();
                    if (IsHRServiceUser)
                    {
                        GetApprovedStatusDetailsForHR(dtApproved,false);
                    }
                    bool bVehicleApprover = IsVehicleApprover(UserName);
                    if (bVehicleApprover)
                    {
                        GetApprovedStatusDetailsForVC(dtApproved,false);
                    }

                    string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");

                    SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                    SPQuery oquery = new SPQuery();
                    string query = string.Concat("<Where><Eq><FieldRef Name='Approver'/><Value Type='User'>" +
                        UserName + "</Value></Eq></Where>");
                    oquery.Query = query;
                    SPListItemCollection collitems = olist1.GetItems(oquery);
                    if (collitems != null && collitems.Count > 0)
                    {
                        foreach (SPListItem itm in collitems)
                        {
                            string value = Convert.ToString(itm["BusinessUnit"]);
                            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                            SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                            SPQuery queryPostion = new SPQuery();
                            value = value.Split('|')[0];
                            // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                            queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" +
                                value + "</Value></Eq></Where>";
                            SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                            if (collitemsPosition != null && collitemsPosition.Count > 0)
                            {
                                foreach (SPListItem itmPostion in collitemsPosition)
                                {
                                    string strRefNo = Convert.ToString(itmPostion["Title"]);
                                    GetApprovedStatusDetails(strRefNo, UserName, dtApproved,false);
                                }
                            }

                        }
                    }
                    GetApprovedStatusDetailsByAuthor(UserName, dtApproved,false);
                });

               DataView sortedView = new DataView(dtApproved);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                ApprovedGrid.DataSource = sortedView;
                ApprovedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("NewHireWorkflowApproval.ApprovedGrid_Sorting", ex.Message);
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

                DataTable dtPending = new DataTable();
                dtPending.Columns.Add(new DataColumn("DateSubmitted"));
                dtPending.Columns.Add(new DataColumn("Initiator"));
                dtPending.Columns.Add(new DataColumn("FormNo"));
                dtPending.Columns.Add(new DataColumn("BusinessUnit"));
                dtPending.Columns.Add(new DataColumn("Role"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    bool IsHRServiceUser = IsUserMemberOfGroup();
                    if (IsHRServiceUser)
                    {
                        GetPendingStatusDetailsForHR(dtPending,false);
                        
                    }
                    bool bVehicleApprover = IsVehicleApprover(UserName);
                    if (bVehicleApprover)
                    {
                        GetPendingStatusDetailsForVC(dtPending,false);
                       
                    }

                    string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");

                    SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                    SPQuery oquery = new SPQuery();
                    string query = string.Concat("<Where><Eq><FieldRef Name='Approver'/><Value Type='User'>" +
                        UserName + "</Value></Eq></Where>");
                    oquery.Query = query;
                    SPListItemCollection collitems = olist1.GetItems(oquery);
                    if (collitems != null && collitems.Count > 0)
                    {
                        foreach (SPListItem itm in collitems)
                        {
                            string value = Convert.ToString(itm["BusinessUnit"]);
                            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                            SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                            SPQuery queryPostion = new SPQuery();
                            value = value.Split('|')[0];
                            // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                            queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" +
                                value + "</Value></Eq></Where>";
                            SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                            if (collitemsPosition != null && collitemsPosition.Count > 0)
                            {
                                foreach (SPListItem itmPostion in collitemsPosition)
                                {
                                    string strRefNo = Convert.ToString(itmPostion["Title"]);

                                    GetPendingStatusDetailsByRefno(strRefNo, UserName, dtPending, value,false);
                                }
                            }

                        }
                    }
                    
                    GetPendingStatusDetailsByAuthor(UserName, dtPending,false);
                    
                });
                
                DataView sortedView = new DataView(dtPending);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                PendingApprovalGrid.DataSource = sortedView;
                PendingApprovalGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("NewHireWorkflowApproval.PendingApprovalGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
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
                dtDraftTable.Columns.Add(new DataColumn("DateSubmitted"));
                dtDraftTable.Columns.Add(new DataColumn("Initiator"));
                dtDraftTable.Columns.Add(new DataColumn("FormNo"));
                dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
                dtDraftTable.Columns.Add(new DataColumn("Role"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                GetDraftStatusDetailsByAuthor(UserName, dtDraftTable,false);
               
                DataView sortedView = new DataView(dtDraftTable);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                DraftGrid.DataSource = sortedView;
                DraftGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("NewHireWorkflowApproval.DraftGrid_Sorting", ex.Message);
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
