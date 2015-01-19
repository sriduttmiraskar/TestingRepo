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
    public partial class AppToHireWorkflowApproval : WebPartPage
    {
        string UserName = string.Empty;
        Hashtable ohash = new Hashtable();
        protected void page_load(object sender, EventArgs e)
        {
           try
            {
                using (SPWeb web = SPControl.GetContextWeb(this.Context))
                {

                    WorkFlowlblError.Text = string.Empty; 
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
               LogUtility.LogError("AppToHireWorkflowApproval.Page_Load", ex.Message);
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
               GetHRManagerDetails(username, dtPending,dtApproved,dtRejected);
               
               GetDraftStatusDetailsByAuthor(username, dtDraftTable,true);
               GetPendingStatusDetailsByAuthor(username, dtPending,true);
               GetApprovedStatusDetailsByAuthor(username, dtApproved,true);
               GetRejectedStatusDetailsByAuthor(username, dtRejected,true);
           });
        }

        private void GetHRManagerDetails(string username, DataTable dtPending, DataTable dtApproved, DataTable dtRejected)
        {
            string lstURL1 = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
            SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
            SPQuery oquery = new SPQuery();
            string query = string.Concat("<Where>" +
                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver1'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver2'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver3'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                      "<Or>" +
                                                        "<Eq>" +
                                                          "<FieldRef Name='Approver4' />" +
                                                          "<Value Type='User'>" + username + "</Value>" +
                                                        "</Eq>" +

                                                        "<Or>" +
                                                          "<Eq>" +
                                                            "<FieldRef Name='Approver5' />" +
                                                            "<Value Type='User'>" + username + "</Value>" +
                                                          "</Eq>" +

                                                          "<Or>" +
                                                            "<Eq>" +
                                                              "<FieldRef Name='Approver6' />" +
                                                              "<Value Type='User'>" + username + "</Value>" +
                                                            "</Eq>" +
                                                            "<Eq>" +
                                                              "<FieldRef Name='Approver7' />" +
                                                              "<Value Type='User'>" + username + "</Value>" +
                                                            "</Eq>" +
                                                          "</Or>" +

                                                        "</Or>" +

                                                      "</Or>" +

                                                    "</Or>" +

                                                    "</Or>" +

                                                   "</Or>" +
                                              "</Where>");
            oquery.Query = query;
            oquery.RowLimit = 100;
            // oquery.Query = "<Where><Eq><FieldRef Name=\'Approver5\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist1.GetItems(oquery);
            if (collitems != null && collitems.Count > 0)
            {
                foreach (SPListItem itm in collitems)
                {

                    /*TaxonomyFieldValue value = itm["BusinessUnit"] as TaxonomyFieldValue;*/
                    string value = Convert.ToString(itm["BusinessUnit"]);
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");

                    SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                    SPQuery queryPostion = new SPQuery();
                    value = value.Split('|')[0];
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value + "</Value></Eq></Where>";
                    queryPostion.RowLimit = 2000;
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
                                //ohash.Add(strRefNo, strRefNo);
                           // }
                        }
                    }

                }
            }
        }

        private void GetHRManagerDetails(string username, DataTable dt, string mode)
        {
            string lstURL1 = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
            SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
            SPQuery oquery = new SPQuery();
            string query = string.Concat("<Where>" +
                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver1'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver2'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                   "<Or>" +
                                                      "<Eq>" +
                                                        "<FieldRef Name='Approver3'/>" +
                                                        "<Value Type='User'>" + username + "</Value>" +
                                                      "</Eq>" +

                                                      "<Or>" +
                                                        "<Eq>" +
                                                          "<FieldRef Name='Approver4' />" +
                                                          "<Value Type='User'>" + username + "</Value>" +
                                                        "</Eq>" +

                                                        "<Or>" +
                                                          "<Eq>" +
                                                            "<FieldRef Name='Approver5' />" +
                                                            "<Value Type='User'>" + username + "</Value>" +
                                                          "</Eq>" +

                                                          "<Or>" +
                                                            "<Eq>" +
                                                              "<FieldRef Name='Approver6' />" +
                                                              "<Value Type='User'>" + username + "</Value>" +
                                                            "</Eq>" +
                                                            "<Eq>" +
                                                              "<FieldRef Name='Approver7' />" +
                                                              "<Value Type='User'>" + username + "</Value>" +
                                                            "</Eq>" +
                                                          "</Or>" +

                                                        "</Or>" +

                                                      "</Or>" +

                                                    "</Or>" +

                                                    "</Or>" +

                                                   "</Or>" +
                                              "</Where>");
            oquery.Query = query;
            oquery.RowLimit = 100;
            // oquery.Query = "<Where><Eq><FieldRef Name=\'Approver5\' /><Value Type=\"User\">" + username + "</Value></Eq></Where>";

            SPListItemCollection collitems = olist1.GetItems(oquery);
            if (collitems != null && collitems.Count > 0)
            {
                foreach (SPListItem itm in collitems)
                {

                    /*TaxonomyFieldValue value = itm["BusinessUnit"] as TaxonomyFieldValue;*/
                    string value = Convert.ToString(itm["BusinessUnit"]);
                    string lstURL = HrWebUtility.GetListUrl("PositionDetails");

                    SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                    SPQuery queryPostion = new SPQuery();
                    value = value.Split('|')[0];
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value + "</Value></Eq></Where>";
                    queryPostion.RowLimit = 2000;
                    SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                    if (collitemsPosition != null && collitemsPosition.Count > 0)
                    {
                        foreach (SPListItem itmPostion in collitemsPosition)
                        {
                            string strRefNo = Convert.ToString(itmPostion["Title"]);

                            //GetDraftStatusDetailsByRefNo(strRefNo, username, dtDraftTable);
                            if(mode == "Pending")
                                GetPendingStatusDetailsByRefno(strRefNo, username, dt, value,false);
                            else if(mode=="Approved")
                                GetApprovedStatusDetails(strRefNo, username, dt,false);
                            else if(mode=="Rejected")
                                GetRejectedStatusDetails(strRefNo, username, dt,false);
                            //ohash.Add(strRefNo, strRefNo);
                            // }
                        }
                    }

                }
            }
        }

        private void GetPendingStatusDetailsForHR(DataTable dtPending,bool Sort)
        {
            string strReferenceNo = "";

            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               /*oquery.Query = "<Where><And><Eq><FieldRef Name=\'ApprovalStatus\' /><Value Type=\"Text\">HRServices</Value></Eq>" +
               "<Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
               "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";*/
               oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></Where>" +
               "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

               oquery.RowLimit = 2000;
               SPListItemCollection collitems = olist.GetItems(oquery);

               foreach (SPListItem listitem in collitems)
               {
                   strReferenceNo = Convert.ToString(listitem["Title"]);
                   if (!ohash.Contains(strReferenceNo))
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       DataRow dtGridRow = dtPending.NewRow();
                       dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                       dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                       string posvalue = Convert.ToString(listitem["PositionType"]);
                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";
                       oquery1.RowLimit = 100;
                       SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                       foreach (SPListItem ListItem in collectionitems)
                       {
                           /*TaxonomyFieldValue value = ListItem["BusinessUnit"] as TaxonomyFieldValue;*/
                           string value = Convert.ToString(ListItem["BusinessUnit"]);
                           dtGridRow["BusinessUnit"] = value;

                           dtGridRow["Approver"] = GetNextApprover(currapprover,posvalue,value);
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
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                   "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
               oquery.RowLimit = 2000;
               SPListItemCollection collitems = olist.GetItems(oquery);

               foreach (SPListItem listitem in collitems)
               {
                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!ohash.Contains(strReferenceNo))
                    {
                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["Approver"] = "HR Services";



                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";
                        oquery1.RowLimit = 100;
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
                        dtApproved.Rows.Add(dtGridRow);
                        ohash.Add(strReferenceNo, strReferenceNo);
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
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq>" +
               "<Eq><FieldRef Name=\'RejectedBy\' /><Value Type=\"Text\">HRServices</Value></Eq></And></Where>" +
               "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
               oquery.RowLimit = 2000;
               SPListItemCollection collitems = olist.GetItems(oquery);

               foreach (SPListItem listitem in collitems)
               {
                   strReferenceNo = Convert.ToString(listitem["Title"]);
                   if (!ohash.Contains(strReferenceNo))
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       DataRow dtGridRow = dtRejected.NewRow();
                       dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                       if (Convert.ToString(listitem["RejectedBy"]) != "")
                           dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";



                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";
                       oquery1.RowLimit = 100;
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
                       dtRejected.Rows.Add(dtGridRow);
                       ohash.Add(strReferenceNo, strReferenceNo);
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
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList splstGeneralInfo = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery queryGeneralInfo = new SPQuery();
               queryGeneralInfo.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno + "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">" + strStatus + "</Value></Eq></And></Where>";
               collitemsGeneralInfo = splstGeneralInfo.GetItems(queryGeneralInfo);
           });
            return collitemsGeneralInfo;
        }
        private void GetDraftStatusDetailsByAuthor(string strUserName, DataTable dtGridTable, bool Sort)
        {

            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
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
                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                   strReferenceNo = Convert.ToString(listitem["Title"]);

                   if (!ohash.Contains(strReferenceNo))
                   {
                       DataRow dtGridRow = dtGridTable.NewRow();


                       dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                       string posvalue = Convert.ToString(listitem["PositionType"]);
                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireRequest.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                       SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                       foreach (SPListItem ListItem in collectionitems)
                       {
                           /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/

                           string value = Convert.ToString(ListItem[MetadataField]);
                           dtGridRow["BusinessUnit"] = value;

                           
                           dtGridRow["Approver"] = GetNextApprover(currapprover,posvalue,value);
                           
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
               if(Sort)
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
               DraftGrid.DataSource = dtGridTable.DefaultView.ToTable();
               DraftGrid.DataBind();
           });
        }

        private string GetNextApprover(string currapprover,string posvalue, string value)
        {
            string nextapprover = string.Empty;
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               string ApproverlstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
               SPList applist = SPContext.Current.Site.RootWeb.GetList(ApproverlstURL);
               SPQuery appquery = new SPQuery();
               if (posvalue == "Waged")
               {
                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                   appquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                       "</Value></Eq><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Waged</Value></Eq></And></Where>";
               }
               else
               {
                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                   appquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                       "</Value></Eq><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Salary</Value></Eq></And></Where>";
               }
               appquery.ViewFields = string.Concat(
               "<FieldRef Name='Approver1' />",
               "<FieldRef Name='Approver2' />",
               "<FieldRef Name='Approver3' />",
               "<FieldRef Name='Approver4' />",
               "<FieldRef Name='Approver5' />",
               "<FieldRef Name='Approver6' />",
               "<FieldRef Name='Approver7' />",
               "<FieldRef Name='HRServices' />");
               SPListItemCollection appcollectionitems = applist.GetItems(appquery);
               foreach (SPListItem appListItem in appcollectionitems)
               {
                   if (currapprover == "Approver1")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver1"]));
                   }
                   else if (currapprover == "Approver2")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver2"]));
                   }
                   else if (currapprover == "Approver3")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver3"]));
                   }
                   else if (currapprover == "Approver4")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver4"]));
                   }
                   else if (currapprover == "Approver5")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver5"]));
                   }
                   else if (currapprover == "Approver6")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver6"]));
                   }
                   else if (currapprover == "Approver7")
                   {
                       nextapprover = GetApprover(Convert.ToString(appListItem["Approver7"]));
                   }
                   else if (currapprover == "HRServices")
                   {
                       nextapprover = "HR Services";
                   }
               }
           });
            return nextapprover;
        }
        private void GetPendingStatusDetailsByAuthor(string strUserName, DataTable dtGridTable,bool Sort)
        {

            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
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
                   strReferenceNo = Convert.ToString(listitem["Title"]);

                   if (!ohash.Contains(strReferenceNo))
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                       DataRow dtGridRow = dtGridTable.NewRow();


                       dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                       string posvalue = Convert.ToString(listitem["PositionType"]);


                       strReferenceNo = Convert.ToString(listitem["Title"]);
                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                       SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                       foreach (SPListItem ListItem in collectionitems)
                       {
                           /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                           string value = Convert.ToString(ListItem[MetadataField]);
                           dtGridRow["BusinessUnit"] = value;

                           dtGridRow["Approver"] = GetNextApprover(currapprover, posvalue,value);


                           if (ListItem["Role"] != null)
                               dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                           else
                               dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);

                       }
                       ohash.Add(strReferenceNo, strReferenceNo);
                       dtGridTable.Rows.Add(dtGridRow);
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
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
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
                   strReferenceNo = Convert.ToString(listitem["Title"]);

                   if (!ohash.Contains(strReferenceNo))
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       DataRow dtGridRow = dtGridTable.NewRow();
                       dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));


                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                       SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                       foreach (SPListItem ListItem in collectionitems)
                       {
                           /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                           string value = Convert.ToString(ListItem[MetadataField]);
                           dtGridRow["BusinessUnit"] = value;

                           dtGridRow["Approver"] = "HRServices";

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

        private void GetRejectedStatusDetailsByAuthor(string strUserName, DataTable dtGridTable, bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
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
                   string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                   strReferenceNo = Convert.ToString(listitem["Title"]);

                   if (!ohash.Contains(strReferenceNo))
                   {
                       DataRow dtGridRow = dtGridTable.NewRow();
                       dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                       if (Convert.ToString(listitem["RejectedBy"]) != "")
                           dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";


                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

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

        private void GetDraftStatusDetailsByRefNo(string strRefno, string strUserName, DataTable dtGridTable,bool Sort)
        {

            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno +
                   "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Draft</Value></Eq></And></Where>" +
                   "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

               SPListItemCollection collitems = olist.GetItems(oquery);

               if (collitems != null && collitems.Count > 0)
               {

                   foreach (SPListItem listitem in collitems)
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       DataRow dtGridRow = dtGridTable.NewRow();


                       dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                       dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                       dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                       string posvalue = Convert.ToString(listitem["PositionType"]);

                       strReferenceNo = Convert.ToString(listitem["Title"]);
                       string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireRequest.aspx?refno=" + strReferenceNo;
                       dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                       string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                       SPQuery oquery1 = new SPQuery();
                       oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo +
                           "</Value></Eq></Where>";

                       SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                       foreach (SPListItem ListItem in collectionitems)
                       {
                           //TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;
                           string value = Convert.ToString(ListItem[MetadataField]);
                           dtGridRow["BusinessUnit"] = value;

                           dtGridRow["Approver"] = GetNextApprover(currapprover, posvalue,value);
                           
                           if (ListItem["Role"] != null)
                               dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                           else
                               dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                       }
                       dtGridTable.Rows.Add(dtGridRow);
                   }
               }
               dtGridTable.DefaultView.Sort = "ID DESC";
               if (Sort)
               Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
               DraftGrid.DataSource = dtGridTable.DefaultView.ToTable();
               DraftGrid.DataBind();
           });
        }

        private void GetPendingStatusDetailsByRefno(string strRefno, string strUserName, DataTable dtGridTable, string BusinessUnit, bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno +
                   "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                   "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

               SPListItemCollection collitems = olist.GetItems(oquery);

               if (collitems != null && collitems.Count > 0)
               {

                   foreach (SPListItem listitem in collitems)
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       strReferenceNo = Convert.ToString(listitem["Title"]);

                       if (!ohash.Contains(strReferenceNo))
                       {


                           string lstURL1 = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
                           SPList olist5 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                           SPQuery oquery5 = new SPQuery();
                           //TaxonomyFieldValue value = listitem["PositionType"] as TaxonomyFieldValue;
                           string value = Convert.ToString(listitem["PositionType"]);
                           DataRow dtGridRow = dtGridTable.NewRow();
                           dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                           dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                           dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                           string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                           dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                           string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                           SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                           SPQuery oquery1 = new SPQuery();
                           oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                           SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                           foreach (SPListItem ListItem in collectionitems)
                           {
                               // TaxonomyFieldValue BuValue = ListItem[MetadataField] as TaxonomyFieldValue;
                               string BuValue = Convert.ToString(ListItem[MetadataField]);
                               dtGridRow["BusinessUnit"] = BuValue;

                               dtGridRow["Approver"] = GetNextApprover(currapprover, value, BuValue);
                               
                               if (ListItem["Role"] != null)
                                   dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                               else
                                   dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                           }
                           dtGridTable.Rows.Add(dtGridRow);
                           ohash.Add(strReferenceNo, strReferenceNo);
                       }
                       //}
                   }
               }
               dtGridTable.DefaultView.Sort = "ID DESC";
               if (Sort)
               Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
               PendingApprovalGrid.DataSource = dtGridTable.DefaultView.ToTable();
               PendingApprovalGrid.DataBind();
           });
        }

        private void GetApprovedStatusDetails(string strRefno, string strUserName, DataTable dtGridTable,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno +
                   "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>" +
                   "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

               SPListItemCollection collitems = olist.GetItems(oquery);



               if (collitems != null && collitems.Count > 0)
               {
                   foreach (SPListItem listitem in collitems)
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       strReferenceNo = Convert.ToString(listitem["Title"]);
                       if (!ohash.Contains(strReferenceNo))
                       {
                           DataRow dtGridRow = dtGridTable.NewRow();
                           dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                           dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                           string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                           dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                           string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                           SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                           SPQuery oquery1 = new SPQuery();
                           oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                           SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                           foreach (SPListItem ListItem in collectionitems)
                           {
                               // TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;
                               string value = Convert.ToString(ListItem[MetadataField]);
                               dtGridRow["BusinessUnit"] = value;

                               dtGridRow["Approver"] = "HRServices";
                               if (ListItem["Role"] != null)
                                   dtGridRow["Role"] = Convert.ToString(ListItem["Role"]);
                               else
                                   dtGridRow["Role"] = Convert.ToString(ListItem["PositionTitle"]);
                           }
                           dtGridTable.Rows.Add(dtGridRow);
                           ohash.Add(strReferenceNo, strReferenceNo);
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

            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefno +
                   "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Rejected</Value></Eq></And></Where>" +
                   "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

               SPListItemCollection collitems = olist.GetItems(oquery);

               if (collitems != null && collitems.Count > 0)
               {
                   foreach (SPListItem listitem in collitems)
                   {
                       string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                       strReferenceNo = Convert.ToString(listitem["Title"]);
                       if (!ohash.Contains(strReferenceNo))
                       {
                           DataRow dtGridRow = dtGridTable.NewRow();
                           if (Convert.ToString(listitem["RejectedBy"]) != "")
                               dtGridRow["RejectedBy"] = GetUser(Convert.ToString(listitem["RejectedBy"])) + " (" + Convert.ToString(listitem["RejectedLevel"]) + ")";

                           dtGridRow["DateApproved"] = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                           dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                           string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/AppToHireReview.aspx?refno=" + strReferenceNo;
                           dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";
                           string PstDtlslstURL = HrWebUtility.GetListUrl("PositionDetails");
                           SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                           SPQuery oquery1 = new SPQuery();
                           oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

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
                           ohash.Add(strReferenceNo, strReferenceNo);
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
            strAuthor = tmparr[tmparr.Length - 1];
            using (HostingEnvironment.Impersonate())
            {
                using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
                {

                    PrincipalContext context1 = new PrincipalContext(ContextType.Domain);

                    string strUserEmailID = strAuthor.Substring(strAuthor.IndexOf('#') + 1);
                    strName = strUserEmailID;
                    string userWithoutDomain = strAuthor.Substring(0, strAuthor.IndexOf('@'));
                    string userName = userWithoutDomain.Substring(userWithoutDomain.IndexOf('#') + 1);

                    string strUserName = SPContext.Current.Web.CurrentUser.LoginName;
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
                        
                        string strUserName = SPContext.Current.Web.CurrentUser.LoginName;
                        strName = strUserName;
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
                LogUtility.LogError("AppToHireWorkflowApproval.DraftGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
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

                bool IsHRServiceUser = IsUserMemberOfGroup();
                if (IsHRServiceUser)
                {
                    GetRejectedStatusDetailsForHR(dtRejected,false);
                }
                GetHRManagerDetails(UserName, dtRejected,"Rejected");

                GetRejectedStatusDetailsByAuthor(UserName, dtRejected,false);
                DataView sortedView = new DataView(dtRejected);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                RejectedGrid.DataSource = sortedView;
                RejectedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("AppToHireWorkflowApproval.RejectedGrid_Sorting", ex.Message);
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

                bool IsHRServiceUser = IsUserMemberOfGroup();
                if (IsHRServiceUser)
                {
                    GetApprovedStatusDetailsForHR(dtApproved, false);
                }
                GetHRManagerDetails(UserName, dtApproved, "Approved");

                GetApprovedStatusDetailsByAuthor(UserName, dtApproved,false);
                DataView sortedView = new DataView(dtApproved);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                ApprovedGrid.DataSource = sortedView;
                ApprovedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("AppToHireWorkflowApproval.ApprovedGrid_Sorting", ex.Message);
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

                bool IsHRServiceUser = IsUserMemberOfGroup();
                if (IsHRServiceUser)
                {
                    GetPendingStatusDetailsForHR(dtPending, false);
                }
                GetHRManagerDetails(UserName, dtPending, "Pending");

                GetPendingStatusDetailsByAuthor(UserName, dtPending, false);
                DataView sortedView = new DataView(dtPending);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                PendingApprovalGrid.DataSource = sortedView;
                PendingApprovalGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("AppToHireWorkflowApproval.PendingApprovalGrid_Sorting", ex.Message);
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
