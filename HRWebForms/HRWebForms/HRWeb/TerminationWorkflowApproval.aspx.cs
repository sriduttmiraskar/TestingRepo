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
    public partial class TerminationWorkflowApproval : WebPartPage
    {
        string UserName = string.Empty;
        Hashtable oHash = new Hashtable();
        protected void page_load(object sender, EventArgs e)
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
                try
                {
                    
                    ShowWorkFlowItemsByUser(UserName);
                    // VerifyUser(UserName);
                    /* GetDraftStatusDetails();
                     GetPendingStatusDetails();
                     GetApprovedStatusDetails();
                     GetRejectedStatusDetails();*/
                }
                catch (Exception ex)
                {
                    LogUtility.LogError("TerminationWorkflowApproval.Page_Load", ex.Message);
                    WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
                }
            }
        } 

        private void ShowWorkFlowItemsByUser(string userName)
        {
             DataTable dtDraftTable = new DataTable();
            dtDraftTable.Columns.Add(new DataColumn("DateSubmitted"));
            dtDraftTable.Columns.Add(new DataColumn("Initiator"));
            dtDraftTable.Columns.Add(new DataColumn("FormNo"));
            dtDraftTable.Columns.Add(new DataColumn("BusinessUnit"));
            dtDraftTable.Columns.Add(new DataColumn("EmpName"));
            dtDraftTable.Columns.Add(new DataColumn("EmpNo"));
            dtDraftTable.Columns.Add(new DataColumn("Approver"));
            dtDraftTable.Columns.Add(new DataColumn("LastDay"));
            dtDraftTable.Columns.Add(new DataColumn("AcknowledgedOn"));
            dtDraftTable.Columns.Add(new DataColumn("ID"));
            DraftGrid.DataSource = dtDraftTable;
            DraftGrid.DataBind();

            DataTable dtPending = new DataTable();
            dtPending.Columns.Add(new DataColumn("DateSubmitted"));
            dtPending.Columns.Add(new DataColumn("Initiator"));
            dtPending.Columns.Add(new DataColumn("FormNo"));
            dtPending.Columns.Add(new DataColumn("BusinessUnit"));
            dtPending.Columns.Add(new DataColumn("EmpName"));
            dtPending.Columns.Add(new DataColumn("EmpNo"));
            dtPending.Columns.Add(new DataColumn("Approver"));
            dtPending.Columns.Add(new DataColumn("LastDay"));
            dtPending.Columns.Add(new DataColumn("AcknowledgedOn"));
            dtPending.Columns.Add(new DataColumn("ID"));
            PendingApprovalGrid.DataSource = dtPending;
            PendingApprovalGrid.DataBind();

            DataTable dtApproved = new DataTable();
            dtApproved.Columns.Add(new DataColumn("DateSubmitted"));
            dtApproved.Columns.Add(new DataColumn("Initiator"));
            dtApproved.Columns.Add(new DataColumn("FormNo"));
            dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
            dtApproved.Columns.Add(new DataColumn("EmpName"));
            dtApproved.Columns.Add(new DataColumn("EmpNo"));
            dtApproved.Columns.Add(new DataColumn("Approver"));
            dtApproved.Columns.Add(new DataColumn("LastDay"));
            dtApproved.Columns.Add(new DataColumn("AcknowledgedOn"));
            dtApproved.Columns.Add(new DataColumn("ID"));
            ApprovedGrid.DataSource = dtApproved;
            ApprovedGrid.DataBind();

            GetDraftRecordsForAuthor(dtDraftTable,userName,true);
            GetPendingRecordsForAuthor(dtPending, userName,true);

            
            
            bool IsHRMgrUser = CheckIfHRManager(userName);
            if (IsHRMgrUser)
            {
                GetPendingRecordsForHRMgr(dtPending, userName,true);
                GetApprovedRecordsForHRMgr(dtApproved, userName,true);
            }

            bool IsHRServiceUser = IsUserMemberOfGroup("HR Services");
            if (IsHRServiceUser)
            {
                GetPendingRecordsForHRService(dtPending, userName, true);
                GetApprovedRecordsForHRService(dtApproved, userName, true);
            }
            //bool IsISUser = CheckIfISUser(userName);
            bool IsISUser = IsUserMemberOfGroup("IS Group");
            if (IsISUser)
            {
                GetPendingRecordsForISUser(dtPending, userName,true);
                GetApprovedRecordsForISUser(dtApproved, userName,true);
            }
            //bool IsCCUser = CheckIfCCUser(userName);
            bool IsCCUser = IsUserMemberOfGroup("Credit Card");
            if (IsCCUser)
            {
                GetPendingRecordsForCCUser(dtPending, userName,true);
                GetApprovedRecordsForCCUser(dtApproved, userName,true);
            }
            bool IsProcurementUser = IsUserMemberOfGroup("Procurement");
            if (IsProcurementUser)
            {
                GetPendingRecordsForProcurementUser(dtPending, userName,true);
                GetApprovedRecordsForProcurementUser(dtApproved, userName,true);
            }
            //bool IsFinanceUser = CheckIfFinanceUser(userName);
            bool IsFinanceUser = IsUserMemberOfGroup("Finance");
            if (IsFinanceUser)
            {
                GetPendingRecordsForFinanceUser(dtPending, userName,true);
                GetApprovedRecordsForFinanceUser(dtApproved, userName,true);
            }
            //bool IsMarketingUser = CheckIfMarketingUser(userName);
            bool IsMarketingUser = IsUserMemberOfGroup("Marketing");
            if (IsMarketingUser)
            {
                GetPendingRecordsForMarketingUser(dtPending, userName,true);
                GetApprovedRecordsForMarketingUser(dtApproved, userName,true);
            }
            bool IsSiteAdmin = IsUserMemberOfGroup("Site Administration");
            if (IsSiteAdmin)
            {
                GetPendingRecordsForSAUser(dtPending, userName,true);
                GetApprovedRecordsForSAUser(dtApproved, userName,true);
            }
            GetApprovedRecordsForAuthor(dtApproved, userName,true);
        }

        private bool CheckIfHRManager(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"Text\">" + username + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private bool CheckIfInitiator(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Author\'/><Value Type=\"Text\">" + username + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                {
                    bValid = true;
                }
            });
            return bValid;
        }

        private bool CheckIfISUser(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">IS</Value>" +
                   "</Eq><Contains><FieldRef Name=\'Approver\'/><Value Type=\"User\">" +
                   username + "</Value></Contains></And></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private bool CheckIfCCUser(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">CreditCard</Value>" +
                   "</Eq><Contains><FieldRef Name=\'Approver\'/><Value Type=\"User\">" +
                   username + "</Value></Contains></And></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private bool CheckIfFinanceUser(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">Finance</Value>" +
                   "</Eq><Contains><FieldRef Name=\'Approver\'/><Value Type=\"User\">" +
                   username + "</Value></Contains></And></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private bool CheckIfMarketingUser(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">Marketing</Value>" +
                   "</Eq><Contains><FieldRef Name=\'Approver\'/><Value Type=\"User\">" +
                   username + "</Value></Contains></And></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private void GetPendingRecordsForHRService(DataTable dtPending, string strUserName, bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                /*oquery.Query = "<Where><And><Eq><FieldRef Name=\'ApprovalStatus\' /><Value Type=\"Text\">HRServices"+
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";*/

                oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='ApprovalStatus' />",
                                "<FieldRef Name='DateOfRequest' />",
                                "<FieldRef Name='HRServiceAckDate' />",
                                "<FieldRef Name='Author' />"
                                ); 

                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);
                        string AckDate = Convert.ToString(listitem["HRServiceAckDate"]);
                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        //string strAuth = Convert.ToString(listitem["Author"]);
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
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
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);

                            if (AckDate != "") dtGridRow["AcknowledgedOn"] = Convert.ToString(AckDate);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if(Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForHRService(DataTable dtApproved, string strUserName, bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                               "<FieldRef Name='Title' />",
                               "<FieldRef Name='ApprovalStatus' />",
                               "<FieldRef Name='DateOfRequest' />",
                               "<FieldRef Name='HRServiceAckDate' />",
                               "<FieldRef Name='Modified' />",
                               "<FieldRef Name='Author' />"
                               ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        if(Convert.ToString(listitem["DateOfRequest"])!="")
                            dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
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
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if(Convert.ToString(ListItem["LastDayAtWork"])!="")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

     
        private void GetPendingRecordsForHRMgr(DataTable dtPending, string strUserName,bool Sort)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"User\">" + strUserName + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                {
                    foreach (SPListItem listitem in collitems)
                    {
                        string value = Convert.ToString(listitem["BusinessUnit"]);
                        value = value.Split('|')[0];
                        string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                        SPQuery queryPostion = new SPQuery();
                        // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                        queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" +
                            value + "</Value></Eq></Where>";
                        SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion); 
                        if (collitemsPosition != null && collitemsPosition.Count > 0)
                        {
                            foreach (SPListItem itmPostion in collitemsPosition)
                            {
                                string strRefNo = Convert.ToString(itmPostion["Title"]);

                                string strEmpName = Convert.ToString(itmPostion["EmployeeName"]);
                                string strEmpNo = Convert.ToString(itmPostion["EmployeeNumber"]);
                                string strLastDay = string.Empty;
                                strLastDay = Convert.ToString(itmPostion["LastDayAtWork"]);
                                if(strLastDay!="")
                                    strLastDay = Convert.ToDateTime(strLastDay).ToString("dd/MM/yyyy");
                                string strAcknowledgedOn = Convert.ToString(itmPostion["Modified"]);

                                string lstURL2 = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
                                SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL2);
                                SPQuery oquery2 = new SPQuery();
                                oquery2.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefNo +
                                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                                    "<OrderBy><FieldRef Name='Date' Ascending='False'></FieldRef></OrderBy>";

                                oquery2.ViewFields = string.Concat("<FieldRef Name='ID' />",
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='ApprovalStatus' />",
                                "<FieldRef Name='DateOfRequest' />",
                                "<FieldRef Name='HRServiceAckDate' />",
                                "<FieldRef Name='Author' />"                               
                                ); 

                                SPListItemCollection collitems2 = olist2.GetItems(oquery2);

                                if (collitems2 != null && collitems2.Count > 0)
                                {
                                    foreach (SPListItem listitem2 in collitems2)
                                    {
                                        if (!oHash.Contains(strRefNo))
                                        {
                                            string currapprover = Convert.ToString(listitem2["ApprovalStatus"]);

                                            DataRow dtGridRow = dtPending.NewRow();
                                            dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem2["DateOfRequest"]).ToString("dd/MM/yyyy");
                                            dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem2["Author"]));
                                            dtGridRow["ID"] = Convert.ToString(listitem2["ID"]);
                                            if (Convert.ToString(listitem2["HRServiceAckDate"]) != "") dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem2["HRServiceAckDate"]);
                                            dtGridRow["LastDay"] = strLastDay;
                                            string url = "";
                                            
                                            dtGridRow["BusinessUnit"] = value;
                                            if (currapprover == "HRManager")
                                            {
                                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                                                //if(IsInitiator(strRefNo))
                                                    url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationRequest.aspx?refno=" + strRefNo;
                                                /*else
                                                    url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strRefNo;*/
                                            }
                                            else if (currapprover == "HRServices")
                                            {
                                                dtGridRow["Approver"] = "HR Services";
                                                url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strRefNo;
                                            }


                                            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefNo + "</a>";
                                            dtGridRow["EmpName"] = strEmpName;
                                            dtGridRow["EmpNo"] = strEmpNo;
                                            dtPending.Rows.Add(dtGridRow);
                                            oHash.Add(strRefNo, strRefNo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private bool IsInitiator(string RefNo)
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
                    RefNo + "</Value></Eq><Eq><FieldRef Name=\'Author\'/><Value Type=\"Text\">" + UserName +
                    "</Value></Eq></And><Eq><FieldRef Name=\'Status\'/><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>";
                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                    result = true;
            });
            return result;
        }

        private void GetApprovedRecordsForHRMgr(DataTable dtApproved, string strUserName,bool Sort)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"User\">" + strUserName + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                {
                    foreach (SPListItem listitem in collitems)
                    {
                        string value = Convert.ToString(listitem["BusinessUnit"]);
                        value = value.Split('|')[0];
                        string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList splstPosition = SPContext.Current.Site.RootWeb.GetList(lstURL);

                        SPQuery queryPostion = new SPQuery();
                        // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                        queryPostion.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" +
                            value + "</Value></Eq></Where>";
                        SPListItemCollection collitemsPosition = splstPosition.GetItems(queryPostion);
                        if (collitemsPosition != null && collitemsPosition.Count > 0)
                        {
                            foreach (SPListItem itmPostion in collitemsPosition)
                            {
                                string strRefNo = Convert.ToString(itmPostion["Title"]);
                                string strEmpName = Convert.ToString(itmPostion["EmployeeName"]);
                                string strEmpNo = Convert.ToString(itmPostion["EmployeeNumber"]);
                                

                                string lstURL2 = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
                                SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL2);
                                SPQuery oquery2 = new SPQuery();
                                oquery2.Query = "<Where><And><Eq><FieldRef Name=\'Title\' /><Value Type=\"Text\">" + strRefNo +
                                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>" +
                                    "<OrderBy><FieldRef Name='Date' Ascending='False'></FieldRef></OrderBy>";

                                oquery2.ViewFields = string.Concat("<FieldRef Name='ID' />",
                              "<FieldRef Name='Title' />",
                              "<FieldRef Name='ApprovalStatus' />",
                              "<FieldRef Name='DateOfRequest' />",
                              "<FieldRef Name='HRServiceAckDate' />",
                              "<FieldRef Name='Modified' />",
                              "<FieldRef Name='Author' />"                           
                              ); 


                                SPListItemCollection collitems2 = olist2.GetItems(oquery2);

                                if (collitems2 != null && collitems2.Count > 0)
                                {
                                    foreach (SPListItem listitem2 in collitems2)
                                    {
                                        if (!oHash.Contains(strRefNo))
                                        {
                                            string currapprover = Convert.ToString(listitem2["ApprovalStatus"]);

                                            DataRow dtGridRow = dtApproved.NewRow();
                                            dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem2["DateOfRequest"]).ToString("dd/MM/yyyy");
                                            dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem2["Author"]));

                                            dtGridRow["ID"] = Convert.ToString(listitem2["ID"]);
                                            string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strRefNo;
                                            dtGridRow["FormNo"] = "<a href=" + url + ">" + strRefNo + "</a>";
                                            dtGridRow["BusinessUnit"] = value;
                                            dtGridRow["Approver"] = "HR Services";
                                            dtGridRow["EmpName"] = strEmpName;
                                            if (Convert.ToString(itmPostion["LastDayAtWork"]) != "")
                                                dtGridRow["LastDay"] = Convert.ToDateTime(itmPostion["LastDayAtWork"]).ToString("dd/MM/yyyy");
                                            dtGridRow["EmpNo"] = Convert.ToString(itmPostion["EmployeeNumber"]);
                                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem2["HRServiceAckDate"]);
                                            dtApproved.Rows.Add(dtGridRow);
                                            oHash.Add(strRefNo, strRefNo);
                                        }
                                    }
                                }
                            }
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

        
        private void GetPendingRecordsForISUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'ISAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Author' />"                             
                             ); 

                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if(!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;

                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";

                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo,strReferenceNo);
                    }

                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForISUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'ISAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Modified' />",
                             "<FieldRef Name='Author' />"
                             ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }

                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        private void GetPendingRecordsForCCUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'CreditCardAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Author' />"
                             ); 

                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if(!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForCCUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'CreditCardAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Modified' />",
                             "<FieldRef Name='Author' />"
                             ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        private void GetPendingRecordsForProcurementUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'ProcurementAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='ApprovalStatus' />",
                                "<FieldRef Name='DateOfRequest' />",
                                "<FieldRef Name='HRServiceAckDate' />",
                                "<FieldRef Name='Author' />"
                                ); 

                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if(!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForProcurementUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'ProcurementAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Modified' />",
                             "<FieldRef Name='Author' />"
                             ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        private void GetPendingRecordsForFinanceUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'FinanceAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                             "<FieldRef Name='Title' />",
                             "<FieldRef Name='ApprovalStatus' />",
                             "<FieldRef Name='DateOfRequest' />",
                             "<FieldRef Name='HRServiceAckDate' />",
                             "<FieldRef Name='Author' />"
                             ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }

                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForFinanceUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'FinanceAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                            "<FieldRef Name='Title' />",
                            "<FieldRef Name='ApprovalStatus' />",
                            "<FieldRef Name='DateOfRequest' />",
                            "<FieldRef Name='HRServiceAckDate' />",
                            "<FieldRef Name='Modified' />",
                            "<FieldRef Name='Author' />"
                            ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {

                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }

                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        private void GetPendingRecordsForMarketingUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'MarketingAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                            "<FieldRef Name='Title' />",
                            "<FieldRef Name='ApprovalStatus' />",
                            "<FieldRef Name='DateOfRequest' />",
                            "<FieldRef Name='HRServiceAckDate' />",
                            "<FieldRef Name='Author' />"
                            ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForMarketingUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'MarketingAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                           "<FieldRef Name='Title' />",
                           "<FieldRef Name='ApprovalStatus' />",
                           "<FieldRef Name='DateOfRequest' />",
                           "<FieldRef Name='HRServiceAckDate' />",
                           "<FieldRef Name='Modified' />",
                           "<FieldRef Name='Author' />"
                           ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        private void GetPendingRecordsForSAUser(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'SiteAdminAckStatus\' /><Value Type=\"Text\">Pending</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                            "<FieldRef Name='Title' />",
                            "<FieldRef Name='ApprovalStatus' />",
                            "<FieldRef Name='DateOfRequest' />",
                            "<FieldRef Name='HRServiceAckDate' />",
                            "<FieldRef Name='Author' />"
                            );
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));

                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);
                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForSAUser(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'SiteAdminAckStatus\' /><Value Type=\"Text\">Approved</Value></Eq></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";
                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                           "<FieldRef Name='Title' />",
                           "<FieldRef Name='ApprovalStatus' />",
                           "<FieldRef Name='DateOfRequest' />",
                           "<FieldRef Name='HRServiceAckDate' />",
                           "<FieldRef Name='Modified' />",
                           "<FieldRef Name='Author' />"
                           );
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }


        private void GetDraftRecordsForAuthor(DataTable dtDraftTable, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Draft</Value></Eq></And></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                           "<FieldRef Name='Title' />",
                           "<FieldRef Name='ApprovalStatus' />",
                           "<FieldRef Name='DateOfRequest' />",
                           "<FieldRef Name='HRServiceAckDate' />",
                           "<FieldRef Name='Author' />"
                           ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtDraftTable.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationRequest.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtDraftTable.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtDraftTable.DefaultView.Sort = "ID DESC";
                if (Sort)
                Draftdir = System.Web.UI.WebControls.SortDirection.Descending;
                DraftGrid.DataSource = dtDraftTable.DefaultView.ToTable();
                DraftGrid.DataBind();
            });
        }

        private void GetPendingRecordsForAuthor(DataTable dtPending, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Pending Approval</Value></Eq></And></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                          "<FieldRef Name='Title' />",
                          "<FieldRef Name='ApprovalStatus' />",
                          "<FieldRef Name='DateOfRequest' />",
                          "<FieldRef Name='HRServiceAckDate' />",
                          "<FieldRef Name='Author' />"
                          ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);
                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtPending.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = string.Empty;
                        if (currapprover == "HRManager")
                            url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationRequest.aspx?refno=" + strReferenceNo;
                        else
                            url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;

                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
                        SPList olist1 = SPContext.Current.Site.RootWeb.GetList(PstDtlslstURL);
                        SPQuery oquery1 = new SPQuery();
                        oquery1.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strReferenceNo + "</Value></Eq></Where>";

                        SPListItemCollection collectionitems = olist1.GetItems(oquery1);

                        foreach (SPListItem ListItem in collectionitems)
                        {
                            /*TaxonomyFieldValue value = ListItem[MetadataField] as TaxonomyFieldValue;*/
                            string value = Convert.ToString(ListItem[MetadataField]);
                            dtGridRow["BusinessUnit"] = value;
                            if (currapprover == "HRManager")
                                dtGridRow["Approver"] = GetApprover(GetHRManager(value)) + " (HR Manager)";
                            else if (currapprover == "HRServices")
                                dtGridRow["Approver"] = "HR Services";
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtPending.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtPending.DefaultView.Sort = "ID DESC";
                if (Sort)
                Pendingdir = System.Web.UI.WebControls.SortDirection.Descending;
                PendingApprovalGrid.DataSource = dtPending.DefaultView.ToTable();
                PendingApprovalGrid.DataBind();
            });
        }

        private void GetApprovedRecordsForAuthor(DataTable dtApproved, string strUserName,bool Sort)
        {
            string strReferenceNo = "";

            string MetadataField = "BusinessUnit";
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><And><Eq><FieldRef Name=\'Author\' /><Value Type=\"User\">" + strUserName +
                    "</Value></Eq><Eq><FieldRef Name=\'Status\' /><Value Type=\"Text\">Approved</Value></Eq></And></Where>" +
                    "<OrderBy><FieldRef Name='ID' Ascending='False'></FieldRef></OrderBy>";

                oquery.ViewFields = string.Concat("<FieldRef Name='ID' />",
                          "<FieldRef Name='Title' />",
                          "<FieldRef Name='ApprovalStatus' />",
                          "<FieldRef Name='DateOfRequest' />",
                          "<FieldRef Name='HRServiceAckDate' />",
                          "<FieldRef Name='Author' />"
                          ); 
                SPListItemCollection collitems = olist.GetItems(oquery);

                foreach (SPListItem listitem in collitems)
                {
                    strReferenceNo = Convert.ToString(listitem["Title"]);

                    if (!oHash.Contains(strReferenceNo))
                    {
                        string currapprover = Convert.ToString(listitem["ApprovalStatus"]);

                        DataRow dtGridRow = dtApproved.NewRow();
                        dtGridRow["DateSubmitted"] = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");
                        dtGridRow["Initiator"] = GetUser(Convert.ToString(listitem["Author"]));
                        dtGridRow["ID"] = Convert.ToString(listitem["ID"]);

                        string url = SPContext.Current.Site.RootWeb.Url + "/Pages/HRWeb/TerminationReview.aspx?refno=" + strReferenceNo;
                        dtGridRow["FormNo"] = "<a href=" + url + ">" + strReferenceNo + "</a>";

                        string PstDtlslstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
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
                            dtGridRow["EmpName"] = Convert.ToString(ListItem["EmployeeName"]);
                            if (Convert.ToString(ListItem["LastDayAtWork"]) != "")
                                dtGridRow["LastDay"] = Convert.ToString(ListItem["LastDayAtWork"]);
                            dtGridRow["EmpNo"] = Convert.ToString(ListItem["EmployeeNumber"]);
                            dtGridRow["AcknowledgedOn"] = Convert.ToString(listitem["HRServiceAckDate"]);
                        }

                        dtApproved.Rows.Add(dtGridRow);
                        oHash.Add(strReferenceNo, strReferenceNo);
                    }
                }
                dtApproved.DefaultView.Sort = "ID DESC";
                if (Sort)
                Approveddir = System.Web.UI.WebControls.SortDirection.Descending;
                ApprovedGrid.DataSource = dtApproved.DefaultView.ToTable();
                ApprovedGrid.DataBind();
            });
        }

        
        private string GetHRManager(string businessunit)
        {
            string HRManager = string.Empty;
            string ApproverlstURL = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList applist = SPContext.Current.Site.RootWeb.GetList(ApproverlstURL);
               SPQuery appquery = new SPQuery();
               // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
               appquery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                   "</Value></Eq></Where>";

               SPListItemCollection appcollectionitems = applist.GetItems(appquery);

               foreach (SPListItem appListItem in appcollectionitems)
               {
                   HRManager = Convert.ToString(appListItem["HrManager"]);
               }
           });
            return HRManager;
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

        public static bool IsUserMemberOfGroup(string groupname)
        {
            bool result = false;
            SPUser user = SPContext.Current.Web.CurrentUser;
            if (!String.IsNullOrEmpty(groupname) && user != null)
            {
                foreach (SPGroup group in user.Groups)
                {
                    if (group.Name == groupname)
                    {
                        // found it
                        result = true;
                        break;
                    }
                }
            }

            return result;
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
                dtPending.Columns.Add(new DataColumn("EmpName"));
                dtPending.Columns.Add(new DataColumn("EmpNo"));
                dtPending.Columns.Add(new DataColumn("Approver"));
                dtPending.Columns.Add(new DataColumn("LastDay"));
                dtPending.Columns.Add(new DataColumn("AcknowledgedOn"));
                dtPending.Columns.Add(new DataColumn("ID"));
                PendingApprovalGrid.DataSource = dtPending;
                PendingApprovalGrid.DataBind();
                GetPendingRecordsForAuthor(dtPending, UserName, false);

                bool IsHRMgrUser = CheckIfHRManager(UserName);
                if (IsHRMgrUser)
                {
                    GetPendingRecordsForHRMgr(dtPending, UserName, false);
                } 
                bool IsHRServiceUser = IsUserMemberOfGroup("HR Services");
                if (IsHRServiceUser)
                {
                    GetPendingRecordsForHRService(dtPending, UserName, false);
                }

                
                //bool IsISUser = CheckIfISUser(userName);
                bool IsISUser = IsUserMemberOfGroup("IS Group");
                if (IsISUser)
                {
                    GetPendingRecordsForISUser(dtPending, UserName, false);
                }
                //bool IsCCUser = CheckIfCCUser(userName);
                bool IsCCUser = IsUserMemberOfGroup("Credit Card");
                if (IsCCUser)
                {
                    GetPendingRecordsForCCUser(dtPending, UserName, false);
                }
                bool IsProcurementUser = IsUserMemberOfGroup("Procurement");
                if (IsProcurementUser)
                {
                    GetPendingRecordsForProcurementUser(dtPending, UserName, false);
                }
                //bool IsFinanceUser = CheckIfFinanceUser(userName);
                bool IsFinanceUser = IsUserMemberOfGroup("Finance");
                if (IsFinanceUser)
                {
                    GetPendingRecordsForFinanceUser(dtPending, UserName, false);
                }
                //bool IsMarketingUser = CheckIfMarketingUser(userName);
                bool IsMarketingUser = IsUserMemberOfGroup("Marketing");
                if (IsMarketingUser)
                {
                    GetPendingRecordsForMarketingUser(dtPending, UserName, false);
                }
                bool IsSiteAdmin = IsUserMemberOfGroup("Site Administration");
                if (IsSiteAdmin)
                {
                    GetPendingRecordsForSAUser(dtPending, UserName, false);
                }
                DataView sortedView = new DataView(dtPending);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                PendingApprovalGrid.DataSource = sortedView;
                PendingApprovalGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TerminationWorkflowApproval.PendingApprovalGrid_Sorting", ex.Message);
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
                dtApproved.Columns.Add(new DataColumn("DateSubmitted"));
                dtApproved.Columns.Add(new DataColumn("Initiator"));
                dtApproved.Columns.Add(new DataColumn("FormNo"));
                dtApproved.Columns.Add(new DataColumn("BusinessUnit"));
                dtApproved.Columns.Add(new DataColumn("EmpName"));
                dtApproved.Columns.Add(new DataColumn("EmpNo"));
                dtApproved.Columns.Add(new DataColumn("Approver"));
                dtApproved.Columns.Add(new DataColumn("LastDay"));
                dtApproved.Columns.Add(new DataColumn("AcknowledgedOn"));
                dtApproved.Columns.Add(new DataColumn("ID"));
                ApprovedGrid.DataSource = dtApproved;
                ApprovedGrid.DataBind();

                bool IsHRServiceUser = IsUserMemberOfGroup("HR Services");
                if (IsHRServiceUser)
                {
                    GetApprovedRecordsForHRService(dtApproved, UserName,false);
                }

                bool IsHRMgrUser = CheckIfHRManager(UserName);
                if (IsHRMgrUser)
                {
                    GetApprovedRecordsForHRMgr(dtApproved, UserName, false);
                }
                //bool IsISUser = CheckIfISUser(UserName);
                bool IsISUser = IsUserMemberOfGroup("IS Group");
                if (IsISUser)
                {
                    GetApprovedRecordsForISUser(dtApproved, UserName, false);
                }
                //bool IsCCUser = CheckIfCCUser(UserName);
                bool IsCCUser = IsUserMemberOfGroup("Credit Card");
                if (IsCCUser)
                {
                    GetApprovedRecordsForCCUser(dtApproved, UserName, false);
                }
                bool IsProcurementUser = IsUserMemberOfGroup("Procurement");
                if (IsProcurementUser)
                {
                    GetApprovedRecordsForProcurementUser(dtApproved, UserName, false);
                }
                //bool IsFinanceUser = CheckIfFinanceUser(UserName);
                bool IsFinanceUser = IsUserMemberOfGroup("Finance");
                if (IsFinanceUser)
                {
                    GetApprovedRecordsForFinanceUser(dtApproved, UserName, false);
                }
                //bool IsMarketingUser = CheckIfMarketingUser(UserName);
                bool IsMarketingUser = IsUserMemberOfGroup("Marketing");
                if (IsMarketingUser)
                {
                    GetApprovedRecordsForMarketingUser(dtApproved, UserName, false);
                }
                bool IsSiteAdmin = IsUserMemberOfGroup("Site Administration");
                if (IsSiteAdmin)
                {
                    GetApprovedRecordsForSAUser(dtApproved, UserName, false);
                }
                GetApprovedRecordsForAuthor(dtApproved, UserName, false);

                DataView sortedView = new DataView(dtApproved);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                ApprovedGrid.DataSource = sortedView;
                ApprovedGrid.DataBind();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("TerminationWorkflowApproval.ApprovedGrid_Sorting", ex.Message);
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
                dtDraftTable.Columns.Add(new DataColumn("EmpName"));
                dtDraftTable.Columns.Add(new DataColumn("EmpNo"));
                dtDraftTable.Columns.Add(new DataColumn("Approver"));
                dtDraftTable.Columns.Add(new DataColumn("LastDay"));
                dtDraftTable.Columns.Add(new DataColumn("AcknowledgedOn"));
                dtDraftTable.Columns.Add(new DataColumn("ID"));
                DraftGrid.DataSource = dtDraftTable;
                DraftGrid.DataBind();

                GetDraftRecordsForAuthor(dtDraftTable, UserName, false);
                DataView sortedView = new DataView(dtDraftTable);
                sortedView.Sort = e.SortExpression + " " + SortDir;
                DraftGrid.DataSource = sortedView;
                DraftGrid.DataBind();

            }
            catch (Exception ex)
            {
                LogUtility.LogError("TerminationWorkflowApproval.DraftGrid_Sorting", ex.Message);
                WorkFlowlblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }
    }
}
