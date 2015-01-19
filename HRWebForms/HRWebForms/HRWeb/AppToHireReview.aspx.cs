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
    public partial class AppToHireReview : WebPartPage
    {
        string UserName = string.Empty;

        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                lblError.Text = string.Empty;
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
                    strRefno = Page.Request.QueryString["refno"];

                if (!IsPostBack)
                {
                    dvSalaryRenum.Visible = false;
                    dvContractor.Visible = false;
                    dvWageed.Visible = false;
                    dvExpat.Visible = false;
                    btnPDF.Visible = false;

                    if (strRefno != "")
                    {
                        lblRefNo.Text = strRefno; 

                        string sError = VerifyUser(UserName, strRefno);
                        if (sError != "ACCESSDENIED")
                        {
                            bool IsHRServiceUser = IsUserMemberOfGroup();
                            if (sError == "NOTCURRENTAPPROVER")
                            {
                                btnApprove.Visible = false;
                                btnReject.Visible = false;
                                //txtComments.Enabled = false;
                                divComments.Visible = false;
                            }
                            else
                            {
                                btnApprove.Visible = true;
                                
                                if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) == "HRServices")
                                    btnReject.Visible = false;
                                else
                                    btnReject.Visible = true;
                                //txtComments.Enabled = true;
                                divComments.Visible = true;
                            }

                            if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) != "HRServices")
                            {
                                //btnApprove.Visible = false;
                                btnPDF.Visible = false;
                                //btnReject.Visible = false;
                                SuccessfulApplicantEdit.Visible = false;
                                SuccessfulApplicantRead.Visible = false;
                                //divComments.Visible = false;
                            }
                            if (Convert.ToString(ViewState["ApprovalStatus"]) != "")
                            {
                                if (!IsCurrentApprover(Convert.ToString(ViewState["PositionType"]), Convert.ToString(ViewState["ApprovalStatus"]), Convert.ToString(ViewState["BusinessUnit"]), UserName))
                                {
                                    if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) == "HRServices" && Convert.ToString(ViewState["Status"]) != "Rejected")
                                    {
                                        btnApprove.Visible = true;
                                    }
                                    else
                                    {
                                        btnApprove.Visible = false;
                                        btnReject.Visible = false;
                                        //txtComments.Enabled = false;
                                        divComments.Visible = false;
                                    }
                                }
                            }

                            if (IsHRServiceUser && (Convert.ToString(ViewState["Status"]) == "Approved" || Convert.ToString(ViewState["Status"]) == "Rejected"))
                            {
                                btnPDF.Visible = true;
                                SuccessfulApplicantRead.Visible = true;
                            }
                            string strPositionType = "";
                            GetGeneralInfo(strRefno, ref strPositionType);
                            GetJobDetails(strRefno);

                            GetCommentHistory(strRefno);
                            GetSuccessfulApplicantDetails(strRefno);


                            if (string.Equals(strPositionType, "Salary"))
                            {
                                //dvSalaryRenum.Attributes.Add("display", "inline");
                                dvSalaryRenum.Visible = true;
                                GetPositionDetails(strRefno);
                                GetSalaryRenumerationDetails(strRefno);

                            }
                            else if (string.Equals(strPositionType, "Contractor"))
                            {
                                dvContractor.Visible = true;
                                GetPositionDetailsForContractor(strRefno);
                                GetContractorRenumerationDetails(strRefno);
                            }
                            else if (string.Equals(strPositionType, "Expatriate"))
                            {
                                dvExpat.Visible = true;
                                GetPositionDetailsForExpat(strRefno);
                                GetExpatRenumerationDetails(strRefno);
                            }
                            else if (string.Equals(strPositionType, "Waged"))
                            {
                                dvWageed.Visible = true;
                                GetPositionDetails(strRefno);
                                GetWagedRenumerationDetails(strRefno);
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
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.AppToHireReview.Page_Load", ex.Message);
            }
        }

        private string VerifyUser(string username, string refno)
        {
            string Error = string.Empty;
            string businessunit = string.Empty;
            string ApprovalStatus = string.Empty;
            string Status = string.Empty;
            string positiontype = string.Empty;
            //string email = GetEmailFromAD(username);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               if (username != "")
               {
                   string lstURL = HrWebUtility.GetListUrl("PositionDetails");

                   SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   SPQuery oquery = new SPQuery();
                   oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
                   oquery.RowLimit = 100;
                   SPListItemCollection collitems = olist.GetItems(oquery);
                   SPListItem listitem = collitems[0];
                   //TaxonomyFieldValue value = listitem["BusinessUnit"] as TaxonomyFieldValue;
                   string value = Convert.ToString(listitem["BusinessUnit"]);
                   businessunit = value;
                   lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
                   SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
                   oquery.RowLimit = 100;
                   SPListItemCollection collitems3 = olist2.GetItems(oquery);
                   SPListItem listitem2 = collitems3[0];
                   ApprovalStatus = Convert.ToString(listitem2["ApprovalStatus"]);
                   positiontype = Convert.ToString(listitem2["PositionType"]);

                   Status = Convert.ToString(listitem2["Status"]);

                   ViewState["PositionType"] = positiontype;
                   ViewState["ApprovalStatus"] = ApprovalStatus;
                   ViewState["BusinessUnit"] = businessunit;
                   ViewState["Status"] = Status;

                   btnApprove.Text = "Approve";
                   btnReject.Visible = true;
                   btnBack.Visible = false;
                   if (Status == "Approved" || Status == "Rejected")  
                   {
                       bool IsHRServiceUser = IsUserMemberOfGroup();
                       if (IsHRServiceUser)
                       {
                           SuccessfulApplicantEdit.Visible = false;
                           SuccessfulApplicantRead.Visible = true;
                           divComments.Visible = false;
                           btnPDF.Visible = true;
                       }
                       else
                       {
                           SuccessfulApplicantEdit.Visible = false;
                           SuccessfulApplicantRead.Visible = false;
                           divComments.Visible = false;
                       }
                   }
                   if (Status == "Pending Approval")
                   {
                       
                       bool IsCurrentUserApprover = IsCurrentApprover(positiontype,ApprovalStatus,value,username);
                       bool IsHRServiceUser = IsUserMemberOfGroup();
                       btnBack.Visible = false;
                       if (IsHRServiceUser && ApprovalStatus == "HRServices")
                       {
                           Error = "";
                           btnApprove.Text = "Acknowledge";
                           btnReject.Visible = false;
                           SuccessfulApplicantEdit.Visible = true;
                           SuccessfulApplicantRead.Visible = false;
                           divComments.Visible = false;
                           btnPDF.Visible = true;
                       }
                       else if (IsHRServiceUser)
                       {
                           Error = "";
                           btnApprove.Visible = false;
                           btnPDF.Visible = false;
                           btnReject.Visible = false;
                           SuccessfulApplicantEdit.Visible = false;
                           SuccessfulApplicantRead.Visible = false;
                           divComments.Visible = false;
                           if (IsCurrentUserApprover)
                           {
                               btnBack.Visible = true;
                           }
                       }
                       else
                       {
                           SuccessfulApplicantEdit.Visible = false;
                           SuccessfulApplicantRead.Visible = false;
                           if (value != "")
                           {
                               if (IsCurrentUserApprover)
                               {
                                   Error = "";
                                   divComments.Visible = true;
                                   btnBack.Visible = true;
                               }
                               else
                               {
                                   string lstURL1 = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
                                   SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                                   SPQuery oquery3 = new SPQuery();
                                   if (positiontype == "Waged")
                                   {
                                       // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                                       oquery3.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                           "</Value></Eq><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Waged</Value></Eq></And></Where>";
                                   }
                                   else
                                   {
                                       oquery3.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                            "</Value></Eq><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Salary</Value></Eq></And></Where>";
                                   }
                                   oquery3.ViewFields = string.Concat(
                                       "<FieldRef Name='Approver1' />",
                                       "<FieldRef Name='Approver2' />",
                                       "<FieldRef Name='Approver3' />",
                                       "<FieldRef Name='Approver4' />",
                                       "<FieldRef Name='Approver5' />",
                                       "<FieldRef Name='Approver6' />",
                                       "<FieldRef Name='Approver7' />");
                                   oquery3.RowLimit = 100;
                                   SPListItemCollection collitems2 = olist1.GetItems(oquery3);
                                   if (collitems2.Count > 0)
                                   {
                                       if (Convert.ToString(collitems2[0]["Approver1"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver2"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver3"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver4"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver5"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver6"]).Contains(username) ||
                                           Convert.ToString(collitems2[0]["Approver7"]).Contains(username))
                                       {
                                           Error = "NOTCURRENTAPPROVER";
                                       }
                                       else
                                       {
                                           //Error = "ACCESSDENIED";
                                           Error = ISInitiator(username);
                                       }
                                   }
                                   else
                                   {
                                       //Error = "ACCESSDENIED";
                                       Error = ISInitiator(username);
                                   }
                               }
                           }
                           else
                           {
                               //Error = "ACCESSDENIED";
                               Error = ISInitiator(username);
                           }
                       }
                   }
                   else
                   {
                       Error = "NOTCURRENTAPPROVER";
                   }
               }
           });
            return Error;
        }

        private bool IsCurrentApprover(string positiontype,string ApprovalStatus, string value, string username)
        {
            bool bValid = false;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL9 = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
                SPList olist9 = SPContext.Current.Site.RootWeb.GetList(lstURL9);
                SPQuery oquery9 = new SPQuery();


                if (positiontype == "Waged")
                {
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    oquery9.Query = "<Where><And><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                       "</Value></Eq><Eq><FieldRef Name='" + ApprovalStatus + "'/><Value Type='User'>" + username +
                                       "</Value></Eq></And><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Waged</Value></Eq></And></Where>";
                }
                else
                {
                    // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                    oquery9.Query = "<Where><And><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                       "</Value></Eq><Eq><FieldRef Name='" + ApprovalStatus + "'/><Value Type='User'>" + username +
                                       "</Value></Eq></And><Eq><FieldRef Name='PositionType'/><Value Type='Text'>Salary</Value></Eq></And></Where>";
                }
                oquery9.RowLimit = 100;
                SPListItemCollection collitems9 = olist9.GetItems(oquery9);
                if (collitems9.Count > 0)
                {
                    bValid = true;
                }
            });
            return bValid;
        }

        private string ISInitiator(string username)
        {
            string Error = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
            SPQuery oquery = new SPQuery();
            oquery.Query = "<Where><And><Contains><FieldRef Name=\'Author\' /><Value Type=\"User\">" + username +
                                                "</Value></Contains><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + lblRefNo.Text + "</Value></Eq></And></Where>";

            SPListItemCollection collitems = olist.GetItems(oquery);
            if (collitems.Count > 0)
            {
                Error = "NOTCURRENTAPPROVER";
            }
            else
            {
                Error = "ACCESSDENIED";
            }
            return Error;
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
            }
            return strName;
        }

        private string GetEmailFromAD(string username)
        {
            string[] tmparr = username.Split('|');
            username = tmparr[tmparr.Length - 1];
            using (var context = new System.DirectoryServices.AccountManagement.PrincipalContext(ContextType.Domain))
            {
                PrincipalContext context1 = new PrincipalContext(ContextType.Domain);
                UserPrincipal foundUser = UserPrincipal.FindByIdentity(context1, username);
                if (foundUser != null)
                {
                    return foundUser.EmailAddress;
                }
                else
                {
                    return "";
                }
            }
        }

        
        private void GetGeneralInfo(string strRefno, ref string strPositionType)
        {
            SPListItemCollection collitems = null;
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               collitems = olist.GetItems(oquery);
           });
            foreach (SPListItem listitem in collitems)
            {

                lblDate.Text = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");

                /*TaxonomyFieldValue value = listitem["PositionType"] as TaxonomyFieldValue;
                if (value != null)
                {
                    lblPositionType.Text = value.Label;
                    strPositionType = value.Label;
                }*/

                lblPositionType.Text = Convert.ToString(listitem["PositionType"]);
                strPositionType = lblPositionType.Text;

                /*TaxonomyFieldValue value1 = listitem["PositionReason"] as TaxonomyFieldValue;
                if (value1 != null)
                    lblReasonPositionRqd.Text = value1.Label;*/

                lblReasonPositionRqd.Text = Convert.ToString(listitem["PositionReason"]);

                //lblReplacePosition.Text = GetUser(Convert.ToString(listitem["ReplacementFor"]));
                lblReplacePosition.Text = Convert.ToString(listitem["ReplacementFor"]);
                lblRequiredBy.Text = HrWebUtility.GetUser(Convert.ToString(listitem["RequiredBy"]));
                lblcomments.Text = Convert.ToString(listitem["Comments"]);

                if (string.Equals(Convert.ToString(listitem["IsBudgetedPosition"]), "True"))
                    lblBudgetPosition.Text = "Yes";
                //ddlBudgetPosition.SelectedItem.Text = "Yes";
                else if (string.Equals(Convert.ToString(listitem["IsBudgetedPosition"]), "False"))
                    lblBudgetPosition.Text = "No";

                if (string.Equals(Convert.ToString(listitem["IsIncreaseInStaffing"]), "True"))
                    lblStaffingLevel.Text = "Yes";
                else if (string.Equals(Convert.ToString(listitem["IsIncreaseInStaffing"]), "False"))
                    lblStaffingLevel.Text = "No";

                lblDetails.Text = Convert.ToString(listitem["Details"]);

                /*TaxonomyFieldValue value2 = listitem["RecruitmentProcess"] as TaxonomyFieldValue;
                if (value2 != null)
                    lblRecruitmentProcess.Text = value2.Label;*/

                lblRecruitmentProcess.Text = Convert.ToString(listitem["RecruitmentProcess"]);
                string status = Convert.ToString(listitem["Status"]);

            }


        }

        private void GetPositionDetails(string strRefno)
        {

            string lstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblContractRateHeader.Visible = false;
                   lblContractRate.Visible = false;
                   lblPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = Convert.ToString(listitem["PositionTitle"]);
                   lblSAPPositionNo.Text = Convert.ToString(listitem["SAPPositionNo"]);


                   /*TaxonomyFieldValue value = listitem["BusinessUnit"] as TaxonomyFieldValue;
                   if (value != null)
                       lblBusinessUnit.Text = value.Label;*/

                   lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                   lblWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);

                   lblReportsTo.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));

                   lblCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                   /*TaxonomyFieldValue value3 = listitem["PositionType"] as TaxonomyFieldValue;
                   if (value3 != null)
                       lblTypeofPosition.Text = value3.Label;*/

                   lblTypeofPosition.Text = Convert.ToString(listitem["PositionType"]);

                   if (listitem["ProposedStartDate"] != null)
                       lblProStartDate.Text = Convert.ToDateTime(listitem["ProposedStartDate"]).ToString("dd/MM/yyyy");

                   if (listitem["ProposedEndDate"] != null)
                       lblFixedEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

               }
           });

        }

        private void GetPositionDetailsForExpat(string strRefno)
        {

            string lstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblContractRateHeader.Visible = false;
                   lblContractRate.Visible = false;
                   lblSAPPositionHeader.Visible = false;
                   lblSAPPositionNo.Visible = false;

                   lblPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = Convert.ToString(listitem["PositionTitle"]);
                   /*TaxonomyFieldValue value = listitem["BusinessUnit"] as TaxonomyFieldValue;
                   if (value != null)
                       lblBusinessUnit.Text = value.Label;

                   TaxonomyFieldValue value1 = listitem["WorkArea"] as TaxonomyFieldValue;
                   if (value1 != null)
                       lblWorkArea.Text = value1.Label;

                   TaxonomyFieldValue value2 = listitem["SiteLocation"] as TaxonomyFieldValue;
                   if (value2 != null)
                       lblSiteLocation.Text = value2.Label;*/

                   /*TaxonomyFieldValue value = listitem["BusinessUnit"] as TaxonomyFieldValue;
                   if (value != null)
                       lblBusinessUnit.Text = value.Label;*/

                   lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);


                   lblWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);

                   lblReportsTo.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));

                   lblCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                   /*TaxonomyFieldValue value3 = listitem["PositionType"] as TaxonomyFieldValue;
                   if (value3 != null)
                       lblTypeofPosition.Text = value3.Label;*/

                   lblTypeofPosition.Text = Convert.ToString(listitem["PositionType"]);

                   if (listitem["ProposedStartDate"] != null)
                       lblProStartDate.Text = Convert.ToDateTime(listitem["ProposedStartDate"]).ToString("dd/MM/yyyy");

                   if (listitem["ProposedEndDate"] != null)
                       lblFixedEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

               }
           });
        }

        private void GetPositionDetailsForContractor(string strRefno)
        {

            string lstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblPostionHeader.Text = "Role:";
                   lblPositionTitle.Text = Convert.ToString(listitem["Role"]);
                   ViewState["PositionTitle"] = Convert.ToString(listitem["Role"]);
                   // lblSAPPositionNo.Text = Convert.ToString(listitem["SAPPositionNo"]);
                   lblSAPPositionHeader.Visible = false;
                   lblSAPPositionNo.Visible = false;

                   /* TaxonomyFieldValue value = listitem["BusinessUnit"] as TaxonomyFieldValue;
                    if (value != null)
                        lblBusinessUnit.Text = value.Label;*/

                   lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                   lblWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);

                   // lblDate.Text = Convert.ToDateTime(listitem["DateOfRequest"]).ToString();

                   lblReportsTo.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));

                   // lblCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                   lblCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                   lblContractRate.Text = Convert.ToString(listitem["ContractRate"]);

                   lblTypePositionHeader.Text = "Type of Contract<br>Agreement:";

                   /*TaxonomyFieldValue value3 = listitem["PositionType"] as TaxonomyFieldValue;
                   if (value3 != null)
                       lblTypeofPosition.Text = value3.Label;*/

                   lblTypeofPosition.Text = Convert.ToString(listitem["PositionType"]);



                   lblProStartDateHeader.Text = "Effective Date:";
                   lblFixedTermHeader.Text = "Contract End Date:";
                   lblProStartDate.Text = Convert.ToDateTime(listitem["ProposedStartDate"]).ToString("dd/MM/yyyy");
                   if(Convert.ToString(listitem["ProposedEndDate"])!="")
                    lblFixedEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");



               }
           });

        }

        private void GetSalaryRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblGrade.Text = Convert.ToString(listitem["Grade"]);
                   lblVehicle.Text = Convert.ToString(listitem["Vehicle"]);
                   lblFAR.Text = Convert.ToString(listitem["FAR"]);
                   lblSTI.Text = Convert.ToString(listitem["STI"]);
                   lblIfOther.Text = Convert.ToString(listitem["OtherVehicleText"]);

                   ViewState["STI"] = Convert.ToString(listitem["STI"]);
                   ViewState["Vehicle"] = Convert.ToString(listitem["Vehicle"]);

               }
           });

        }

        private void GetWagedRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {

                   lblWagedLevel.Text = Convert.ToString(listitem["Level"]);

                   /*TaxonomyFieldValue value3 = listitem["ShiftRotation"] as TaxonomyFieldValue;
                   if (value3 != null)
                       lblShiftLocation.Text = value3.Label;*/

                   lblShiftLocation.Text = Convert.ToString(listitem["ShiftRotation"]);

                   lblWagedVehicle.Text = Convert.ToString(listitem["Vehicle"]);
                   lblWagedIfAny.Text = Convert.ToString(listitem["OtherVehicleText"]);
                   ViewState["STI"] = Convert.ToString(listitem["STI"]);
                   ViewState["Vehicle"] = Convert.ToString(listitem["Vehicle"]);


               }

           });
        }

        private void GetExpatRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("RemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblExpatGrade.Text = Convert.ToString(listitem["Grade"]);
                   lblExpatVehicle.Text = Convert.ToString(listitem["Vehicle"]);
                   lblExpatFAR.Text = Convert.ToString(listitem["FAR"]);
                   lblExpatSTI.Text = Convert.ToString(listitem["STI"]);
                   /*lblExpatUtilities.Text = Convert.ToString(listitem["Utilities"]);
                   lblExpatRelocation.Text = Convert.ToString(listitem["Relocation"]);*/
                   lblExpatIfAny.Text = Convert.ToString(listitem["OtherVehicleText"]);
                   ViewState["STI"] = Convert.ToString(listitem["STI"]);
                   ViewState["Vehicle"] = Convert.ToString(listitem["Vehicle"]);

               }

           });
        }

        private void GetContractorRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("ContractRoleStatement");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblContractDelivery.Text = Convert.ToString(listitem["RoleStatement"]);


               }
           });

        }

        private void GetJobDetails(string strRefno)
        {

            DataTable dtJobdetails = new DataTable();
            dtJobdetails.Columns.Add("Type");
            dtJobdetails.Columns.Add("Name");
            dtJobdetails.Columns.Add("Modified");
            string lstURL = HrWebUtility.GetListUrl("JobDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               //string app = SPContext.Current.Site.RootWeb.Url;
               string app = SPContext.Current.Site.Protocol + "//" + SPContext.Current.Site.HostName;
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.RowLimit = 100;
               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   string url = listitem.File.ServerRelativeUrl;
                   string srType = Convert.ToString(listitem["Type"]);
                   TableRow row = new TableRow();
                   TableCell emptyType = new TableCell();
                   emptyType.Text = "";
                   row.Cells.Add(emptyType);

                   TableCell cellType = new TableCell();
                   cellType.Text = srType;
                   row.Cells.Add(cellType);

                   TableCell cellName = new TableCell();
                   //  cellName.Text = Convert.ToString(listitem["Name"]);

                   cellName.Text = "<a target='_blank' href='" + app + url + "'>" + Convert.ToString(listitem["Name"]) + "</a>";
                   LinkButton lnkName = new LinkButton();
                   lnkName.Text = app + url;
                   //cellName.Controls.Add(lnkName);
                   row.Cells.Add(cellName);

                   TableCell celModified = new TableCell();
                   celModified.Text = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                   row.Cells.Add(celModified);
                   tblAttachment.Rows.Add(row);
                   dtJobdetails.Rows.Add(new string[] { srType, Convert.ToString(listitem["Name"]), celModified.Text });

                   //txtMeetingComments.Text = Convert.ToString(listitem["Comments"]);

               }
               ViewState["vwJobDetails"] = dtJobdetails;

           });
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
           });

        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateGeneralInfo("Approved");
                SetSuccessfulApplicationList();
                //bool IsHRServiceUser = IsUserMemberOfGroup();
                if (btnApprove.Text=="Acknowledge")
                    Response.Redirect("/people/Pages/HRWeb/AppToHireReview.aspx?refno=" + lblRefNo.Text);
                else
                    Response.Redirect("/people/Pages/HRWeb/AppToHireWorkflowApproval.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.AppToHireReview.btnApprove_Click", ex.Message);
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateGeneralInfo("Back");
                Response.Redirect("/people/Pages/HRWeb/AppToHireWorkflowApproval.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.AppToHireReview.btnBack_Click", ex.Message);
            }

        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateGeneralInfo("Rejected");
                Response.Redirect("/people/Pages/HRWeb/AppToHireReview.aspx?refno=" + lblRefNo.Text);
            }
            catch (Exception ex)
            {
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.AppToHireReview.btnReject_Click", ex.Message);
            }
        }

        private void UpdateGeneralInfo(string status)
        {
            string strRefno = lblRefNo.Text.Trim();
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oQuery = new SPQuery();
               oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oQuery.RowLimit = 100;
               SPListItemCollection collectionItems = olist.GetItems(oQuery);
               if (collectionItems.Count > 0)
               {
                   SPListItem item = collectionItems[0];
                   string currapprover = Convert.ToString(ViewState["ApprovalStatus"]);
                   string initiator = Convert.ToString(item["Author"]);
                   string[] tmparr = initiator.Split('|');
                   initiator = tmparr[tmparr.Length - 1];
                   ViewState["Initiator"] = initiator;
                   string nextapprover = "";
                   if (currapprover == "HRServices" && status == "Approved")
                   {
                       item["Status"] = "Approved";
                       nextapprover = GetNextApproverandSendEmail(currapprover, "Approved");
                   }
                   else if (status == "Approved")
                   {
                       item["Status"] = "Pending Approval";
                       nextapprover = GetNextApproverandSendEmail(currapprover, "Pending Approval");
                   }
                   else if (status == "Back")
                   {
                       item["Status"] = "Draft";
                       nextapprover = "";
                       SendEmail(status);
                   }
                   else if (status == "Rejected")
                   {
                       item["Status"] = "Rejected";
                       if (currapprover != "HRServices")
                           item["RejectedBy"] = UserName;
                       else
                           item["RejectedBy"] = "HRServices";
                       item["RejectedLevel"] = GetRejectedLevel(currapprover);
                       nextapprover = GetNextApproverandSendEmail(currapprover, "Rejected");
                   }
                   item["ApprovalStatus"] = nextapprover;
                   item.Update();
               }
           });
        }

        private string GetRejectedLevel(string approver)
        {
            string rapprover = string.Empty;
            if (approver == "Approver1")
                rapprover = "Level 1";
            else if (approver == "Approver2")
                rapprover = "Level 2";
            else if (approver == "Approver3")
                rapprover = "Level 3";
            else if (approver == "Approver4")
                rapprover = "Level 4";
            else if (approver == "Approver5")
                rapprover = "Level 5";
            else if (approver == "Approver6")
                rapprover = "Level 6";
            else if (approver == "Approver")
                rapprover = "Level 7";
            else if (approver == "HRServices")
                rapprover = "HR Services";
            return rapprover;
        }

        private string GetNextApproverandSendEmail(string currapprover, string status)
        {
            string nextapprover = string.Empty;
            string businessunit = Convert.ToString(ViewState["BusinessUnit"]);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {

               if (currapprover != "" && businessunit != "")
               {
                   string lstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
                   SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   string PositionType = Convert.ToString(ViewState["PositionType"]);
                   SPQuery oquery3 = new SPQuery();
                   if (PositionType == "Waged")
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
                   SPListItemCollection collitems2 = olist.GetItems(oquery3);
                   if (collitems2.Count > 0)
                   {
                       SPListItem item = collitems2[0];

                       ViewState["HRManager"] = Convert.ToString(item["Approver1"]);

                       if (currapprover == "Approver1" && Convert.ToString(item["Approver2"]) != "")
                       {
                           nextapprover = "Approver2";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver2"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver3"]) != "")
                       {
                           nextapprover = "Approver3";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver3"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver4"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver4";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver4"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver5"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver5";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver5"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver6"]) != "")
                       {
                           nextapprover = "Approver6";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver1" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver3"]) != "")
                       {
                           nextapprover = "Approver3";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver3"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver4"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver4";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver4"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver5"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver5";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver5"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver6"]) != "")
                       {
                           nextapprover = "Approver6";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver2" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver4"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver4";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver4"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver5"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver5";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver5"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver6"]) != "")
                       {
                           nextapprover = "Approver6";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver3" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["Approver5"]) != "" && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != "")))
                       {
                           nextapprover = "Approver5";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver5"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["Approver6"]) != "")
                       {
                           nextapprover = "Approver6";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver4" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver5" && Convert.ToString(item["Approver6"]) != "")
                       {
                           nextapprover = "Approver6";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver6"];
                       }
                       else if (currapprover == "Approver5" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver5" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver5" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver5" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver6" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) != Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver6" && Convert.ToString(item["Approver7"]) != "" && (Convert.ToString(item["Approver7"]) == Convert.ToString(item["Approver5"]) && (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))))
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver6" && Convert.ToString(item["Approver7"]) != "")
                       {
                           nextapprover = "Approver7";
                           ViewState["ApproverEmail"] = collitems2[0]["Approver7"];
                       }
                       else if (currapprover == "Approver6" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }
                       else if (currapprover == "Approver7" && Convert.ToString(item["HRServices"]) != "")
                       {
                           nextapprover = "HRServices";
                           ViewState["ApproverEmail"] = collitems2[0]["HRServices"];
                       }

                   }
               }
               SendEmail(status);
           });
            return nextapprover;
        }

        private void UpdateComment()
        {
            string appno = lblRefNo.Text.Trim();
            string approveremail = UserName;
            string username = GetUserNameFromAD(approveremail);
            string approverid = UserName.Split('@')[0].Trim();
            string comment = txtComments.Text;
            string approverstep = Convert.ToString(ViewState["ApprovalStatus"]);

            string lstURL = HrWebUtility.GetListUrl("AppToHireCommentsHistory"); 
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
               
                   SPListItem listitem = oList.AddItem();
                   listitem["Title"] = appno;
                   listitem["ApproverID"] = approverid;
                   listitem["ApproverName"] = username;
                   listitem["ApproverEmail"] = approveremail;
                   listitem["ApproverStep"] = approverstep;
                   listitem["Comment"] = comment;
                   listitem.Update();
               
           });
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

        private string GetFirstLevelApprover()
        {
            string strFirstApprover = string.Empty;

            string businessunit = Convert.ToString(ViewState["BusinessUnit"]);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {

               if (businessunit != "")
               {
                   string lstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
                   SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   string PositionType = Convert.ToString(ViewState["PositionType"]);
                   SPQuery oquery = new SPQuery();
                   if (PositionType == "Waged")
                   {
                       // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                            "</Value></Eq><Eq><FieldRef Name=\'PositionType\' /><Value Type=\"Text\">Waged</Value></Eq></And></Where>";
                   }
                   else
                   {
                       // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                       oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                            "</Value></Eq><Eq><FieldRef Name=\'PositionType\' /><Value Type=\"Text\">Salary</Value></Eq></And></Where>";
                   }
                   oquery.ViewFields = string.Concat(
                       "<FieldRef Name='Approver1' />");
                   oquery.RowLimit = 100;
                   SPListItemCollection collitems = olist.GetItems(oquery);
                   if (collitems.Count > 0)
                   {
                       SPListItem item = collitems[0];

                       strFirstApprover = Convert.ToString(item["Approver1"]);

                   }
               }
           });

            return strFirstApprover;
        }

        private void SendEmail(string status)
        {
            string strRefNo = lblRefNo.Text;
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
                               "<FieldRef Name='ApprovalMessage' />",
                               "<FieldRef Name='ApprovedSubject' />",
                               "<FieldRef Name='ApprovedMessage' />",
                               "<FieldRef Name='RejectedSubject' />",
                               "<FieldRef Name='RejectedMessage' />",
                               "<FieldRef Name='RevertedSubject' />",
                               "<FieldRef Name='RevertedMessage' />",
                               "<FieldRef Name='HRManagerApprovalMessage' />");
                oQuery.RowLimit = 100;
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
                        string url = site.Url + "/pages/hrweb/apptohirereview.aspx?refno=" + strRefNo;
                        strFrom = Convert.ToString(itm["Title"]);
                        if (status == "Approved")
                        {
                            //strTo = Convert.ToString(ViewState["Initiator"]);
                            strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            //strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo);
                        }
                        else if (status == "Rejected")
                        {
                            strTo = Convert.ToString(ViewState["Initiator"]);
                            strSubject = Convert.ToString(itm["RejectedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["RejectedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"]));
                        }
                        else if (status == "Back")
                        {
                            string strFirstApr = GetFirstLevelApprover();
                            if (strFirstApr.Contains("#"))
                                strFirstApr = strFirstApr.Split('#')[1];
                            strTo = Convert.ToString(ViewState["Initiator"])+";"+strFirstApr;                            
                            strSubject = Convert.ToString(itm["RevertedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["RevertedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"]));
                        }
                        else
                        {
                            strTo = Convert.ToString(ViewState["ApproverEmail"]);
                            string[] tmparr = strTo.Split('|');
                            strTo = tmparr[tmparr.Length - 1];
                            strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"]));
                        }

                        if (strTo.Contains("#"))
                            strTo = strTo.Split('#')[1];

                        if (strTo.ToLower() == "hrservices")
                        {
                            string to = string.Empty;
                            strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["HRManagerApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"]));

                            using (SPSite newSite = new SPSite(site.ID))
                            {
                                using (SPWeb newWeb = newSite.OpenWeb(web.ID))
                                {
                                    to += ";" + HrWebUtility.GetDistributionEmail("HR Services");
                                    to = to.TrimStart(';');
                                    if (lblPositionType.Text.Trim() == "Contractor")
                                    {
                                        to += ";" + HrWebUtility.GetDistributionEmail("Procurement-ApptoHire");
                                        to = to.TrimStart(';');
                                    }
                                    /*SPGroup group = newWeb.Groups["HR Services"];
                                    foreach (SPUser user in group.Users)
                                    {
                                        to += ";" + user.Email;
                                    }
                                    to = to.TrimStart(';');

                                    if (lblPositionType.Text.Trim() == "Contractor")
                                    {
                                        SPGroup group1 = newWeb.Groups["Procurement"];
                                        foreach (SPUser user in group1.Users)
                                        {
                                            to += ";" + user.Email;
                                        }
                                        to = to.TrimStart(';');
                                    }*/

                                    string initiator = Convert.ToString(ViewState["Initiator"]);
                                    if (initiator.Contains("#"))
                                        initiator = initiator.Split('#')[1];
                                    to += ";" + initiator;

                                    string HRMgr = Convert.ToString(ViewState["HRManager"]);
                                    if (HRMgr.Contains("#"))
                                        HRMgr = HRMgr.Split('#')[1];
                                    to += ";" + HRMgr;

                                    strTo = to;
                                }
                            }
                        }

                        if (strTo != "")
                        {
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
                        }
                        break;
                    }
                }
            });
        }

        private void SaveEmailDetails(string strFrom, string strTo, string strSubject, string strMessage)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    /*SPSite oSite = SPContext.Current.Site;
                    using (SPWeb oWeb = oSite.OpenWeb())
                    {*/
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
            catch 
            { }
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

        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblRefNo.Text.Trim();
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

        private void UpdateSuccessfulApplicationList(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["SuccessfulApplicantName"] = txtSuccessfulApplicantName.Text;
                listitem["Position"] = txtPosition.Text;
                listitem["SAPNumber"] = txtSAPNumber.Text;

                if (!CommencementDateTimeControl.IsDateEmpty)
                    listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(CommencementDateTimeControl.SelectedDate);

                listitem.Update();
            });
        }

        private SPListItemCollection GetListData(string GetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblRefNo.Text.Trim();
            SPWeb mySite = SPContext.Current.Web;
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

        private void GetSuccessfulApplicantDetails(string strRefno)
        {
            if (strRefno == "")
                strRefno = lblRefNo.Text.Trim();
            SPListItemCollection SuccessfulApplicationcollecItems = GetListData("SuccessfulApplication", strRefno);
            foreach (SPListItem ListItems in SuccessfulApplicationcollecItems)
            {
                txtSuccessfulApplicantName.Text = Convert.ToString(ListItems["SuccessfulApplicantName"]);
                txtPosition.Text = Convert.ToString(ListItems["Position"]);
                txtSAPNumber.Text = Convert.ToString(ListItems["SAPNumber"]);

                CommencementDateTimeControl.SelectedDate = Convert.ToDateTime(ListItems["CommencementDate"]);

                lblSAName.Text = Convert.ToString(ListItems["SuccessfulApplicantName"]);
                lblSAPos.Text = Convert.ToString(ListItems["Position"]);
                lblSASAP.Text = Convert.ToString(ListItems["SAPNumber"]);

                if (ListItems["CommencementDate"] != null)
                    lblSACommDate.Text = Convert.ToDateTime(ListItems["CommencementDate"]).ToString("dd/MM/yyyy");
            }
        }


        private void SetSuccessfulApplicationList()
        {
            string strRefno = "";
            SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (strRefno == "")
                        strRefno = lblRefNo.Text.Trim();
                    SPListItemCollection collectionItems = SetListData("SuccessfulApplication", strRefno);
                    if (collectionItems != null && collectionItems.Count > 0)
                    {
                        foreach (SPListItem listitem in collectionItems)
                        {
                            UpdateSuccessfulApplicationList(listitem);
                        }
                    }
                    else
                    {
                        SPList oList = SPContext.Current.Web.Lists["SuccessfulApplication"];
                        SPListItem listitem = oList.AddItem();
                        listitem["Title"] = strRefno;
                        UpdateSuccessfulApplicationList(listitem);

                    }
                });
        }

        protected void btnPDF_Click(object sender, EventArgs e)
        {
            if (string.Equals(lblPositionType.Text, "Waged", StringComparison.OrdinalIgnoreCase))
            {
                GenerateWagedPDF();
            }
            else if (string.Equals(lblPositionType.Text, "Salary", StringComparison.OrdinalIgnoreCase))
            {
                GenerateSalaryPDF();
            }
            else if (string.Equals(lblPositionType.Text, "Expatriate", StringComparison.OrdinalIgnoreCase))
            {
                GenerateExpatPDF();
            }
            else if (string.Equals(lblPositionType.Text, "Contractor", StringComparison.OrdinalIgnoreCase))
            {
                GenerateContractorPDF();
            }


        }

        private void GenerateWagedPDF()
        {
            string filename = "Apptohire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk RefChunk = new Chunk("Reference Number: ", ddlLabelFonts);
            Phrase RefValPh1 = new Phrase(RefChunk);
            PdfPCell RefChnvalcell = new PdfPCell(RefValPh1);
            RefChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell);

            Chunk RefChnkVal = new Chunk(lblRefNo.Text, ddlFonts);
            Phrase RefValPh2 = new Phrase(RefChnkVal);
            PdfPCell RefChnvalcell2 = new PdfPCell(RefValPh2);
            RefChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell2);

            Chunk DateChnk = new Chunk("Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblDate.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Position Type: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblPositionType.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Reason Position Required: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReasonPositionRqd.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Replacement for Position Held by: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReplacePosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Budgeted Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBudgetPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Is this an increase in staffing levels: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblStaffingLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Recruitment Process: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRecruitmentProcess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Details: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDetails.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PdfPCell leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);


            PdfPTable tblGeneralInfoRight = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoRight.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Required By: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRequiredBy.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblcomments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            bool IsHRServiceUser = IsUserMemberOfGroup();

            if (IsHRServiceUser && SuccessfulApplicantRead.Visible)
            {
                PdfPCell Emptycell = new PdfPCell(new Phrase("   ", headFont));
                Emptycell.Colspan = 2;
                Emptycell.HorizontalAlignment = 0;
                Emptycell.Border = 0;
                tblGeneralInfoRight.AddCell(Emptycell);

                PdfPCell cell = new PdfPCell(new Phrase("Successful Applicant Details", headFont));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                cell.Border = 0;
                tblGeneralInfoRight.AddCell(cell);


                tblGeneralInfoRight.AddCell(Emptycell);

                PosTypeChnk = new Chunk("Successful Applicant Name : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Position : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAPos.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("SAP Number : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSASAP.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Commencement Date : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSACommDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }


            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);



            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);



            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            //float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("SAP Position No: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSAPPositionNo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReportsTo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Type of Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeofPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Proposed Start Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblProStartDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Fixed Term End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFixedEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblPositionDet);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            //  iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            PdfPTable pdfPHeader = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader.SetWidths(tblGeneralInfoWidth);

            PdfPCell header = new PdfPCell(new Phrase("Position Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);
            header = new PdfPCell(new Phrase("Job Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);


            PosTypeChnk = new Chunk("Attached updated Role Statement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PdfPTable tblAttach = new PdfPTable(3);
            PosTypeChnk = new Chunk(" FileType ", cellFnt);
            tblGeneralInfoWidth = new float[] { 25f, 50f, 25f };
            tblAttach.SetWidths(tblGeneralInfoWidth);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Name ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            DataTable dtJobDetails = (DataTable)ViewState["vwJobDetails"];
            if (dtJobDetails.Rows.Count > 0)
            {
                for (int count = 0; count <= dtJobDetails.Rows.Count - 1; count++)
                {
                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Type"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Name"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Modified"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    tblAttach.AddCell(PosTypevalcell);

                }
            }

            PdfPCell attachCell = new PdfPCell(tblAttach);
            tblJobDetailsDet.AddCell(attachCell);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);
            //tblRenumeration.AddCell(phEmpty);
            //tblRenumeration.AddCell(phEmpty);
            PosTypeChnk = new Chunk(" ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(" ", ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Level: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Shift Rotation: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblShiftLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedVehicle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("If other (specify): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedIfAny.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PdfPCell renumerationCell = new PdfPCell(tblRenumeration);
            renumerationCell.Border = 0;
            tblJobDetailsDet.AddCell(renumerationCell);


            leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            rightCell = new PdfPCell(tblJobDetailsDet);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl1.AddCell(rightCell);

            pdfDoc.Add(headerTbl1);

            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("                 Approval History", headFont);
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
        private void GenerateSalaryPDF()
        {
            string filename = "Apptohire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk RefChunk = new Chunk("Reference Number: ", ddlLabelFonts);
            Phrase RefValPh1 = new Phrase(RefChunk);
            PdfPCell RefChnvalcell = new PdfPCell(RefValPh1);
            RefChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell);

            Chunk RefChnkVal = new Chunk(lblRefNo.Text, ddlFonts);
            Phrase RefValPh2 = new Phrase(RefChnkVal);
            PdfPCell RefChnvalcell2 = new PdfPCell(RefValPh2);
            RefChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell2);

            Chunk DateChnk = new Chunk("Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblDate.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Position Type: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblPositionType.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Reason Position Required: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReasonPositionRqd.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Replacement for Position Held by: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReplacePosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Budgeted Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBudgetPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Is this an increase in staffing levels: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblStaffingLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Recruitment Process: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRecruitmentProcess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Details: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDetails.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PdfPCell leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);


            PdfPTable tblGeneralInfoRight = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoRight.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Required By: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRequiredBy.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblcomments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            bool IsHRServiceUser = IsUserMemberOfGroup();

            if (IsHRServiceUser && SuccessfulApplicantRead.Visible)
            {
                PdfPCell Emptycell = new PdfPCell(new Phrase("   ", headFont));
                Emptycell.Colspan = 2;
                Emptycell.HorizontalAlignment = 0;
                Emptycell.Border = 0;
                tblGeneralInfoRight.AddCell(Emptycell);

                PdfPCell cell = new PdfPCell(new Phrase("Successful Applicant Details", headFont));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                cell.Border = 0;
                tblGeneralInfoRight.AddCell(cell);


                tblGeneralInfoRight.AddCell(Emptycell);

                PosTypeChnk = new Chunk("Successful Applicant Name : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Position : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAPos.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("SAP Number : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSASAP.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Commencement Date : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSACommDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }

            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);

            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            //float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("SAP Position No: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSAPPositionNo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReportsTo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Type of Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeofPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Proposed Start Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblProStartDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Fixed Term End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFixedEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblPositionDet);
            leftCell.Border = 0;
            leftCell.Padding = 0f;



            PdfPTable pdfPHeader = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader.SetWidths(tblGeneralInfoWidth);

            PdfPCell header = new PdfPCell(new Phrase("Position Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);
            header = new PdfPCell(new Phrase("Job Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);


            PosTypeChnk = new Chunk("Attached updated Role Statement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PdfPTable tblAttach = new PdfPTable(3);
            tblGeneralInfoWidth = new float[] { 25f, 50f, 25f };
            tblAttach.SetWidths(tblGeneralInfoWidth);
            PosTypeChnk = new Chunk(" FileType ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Name ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);


            DataTable dtJobDetails = (DataTable)ViewState["vwJobDetails"];
            if (dtJobDetails.Rows.Count > 0)
            {
                for (int count = 0; count <= dtJobDetails.Rows.Count - 1; count++)
                {
                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Type"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Name"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Modified"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    tblAttach.AddCell(PosTypevalcell);

                }
            }

            PdfPCell attachCell = new PdfPCell(tblAttach);
            tblJobDetailsDet.AddCell(attachCell);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);
            //tblRenumeration.AddCell(phEmpty);
            //tblRenumeration.AddCell(phEmpty);
            //tblRenumeration.AddCell(phEmpty);
            PosTypeChnk = new Chunk(" ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(" ", ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Grade: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblGrade.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("FAR: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFAR.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("STI: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSTI.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblVehicle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("If other (specify): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblIfOther.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PdfPCell renumerationCell = new PdfPCell(tblRenumeration);
            renumerationCell.Border = 0;
            tblJobDetailsDet.AddCell(renumerationCell);


            leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            rightCell = new PdfPCell(tblJobDetailsDet);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl1.AddCell(rightCell);

            pdfDoc.Add(headerTbl1);

            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("                 Approval History", headFont);
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
        private void GenerateExpatPDF()
        {
            string filename = "Apptohire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk RefChunk = new Chunk("Reference Number: ", ddlLabelFonts);
            Phrase RefValPh1 = new Phrase(RefChunk);
            PdfPCell RefChnvalcell = new PdfPCell(RefValPh1);
            RefChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell);

            Chunk RefChnkVal = new Chunk(lblRefNo.Text, ddlFonts);
            Phrase RefValPh2 = new Phrase(RefChnkVal);
            PdfPCell RefChnvalcell2 = new PdfPCell(RefValPh2);
            RefChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell2);

            Chunk DateChnk = new Chunk("Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblDate.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Position Type: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblPositionType.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Reason Position Required: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReasonPositionRqd.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Replacement for Position Held by: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReplacePosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Budgeted Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBudgetPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Is this an increase in staffing levels: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblStaffingLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Recruitment Process: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRecruitmentProcess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Details: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDetails.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PdfPCell leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);


            PdfPTable tblGeneralInfoRight = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoRight.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Required By: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRequiredBy.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblcomments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            bool IsHRServiceUser = IsUserMemberOfGroup();

            if (IsHRServiceUser && SuccessfulApplicantRead.Visible)
            {
                PdfPCell Emptycell = new PdfPCell(new Phrase("   ", headFont));
                Emptycell.Colspan = 2;
                Emptycell.HorizontalAlignment = 0;
                Emptycell.Border = 0;
                tblGeneralInfoRight.AddCell(Emptycell);

                PdfPCell cell = new PdfPCell(new Phrase("Successful Applicant Details", headFont));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                cell.Border = 0;
                tblGeneralInfoRight.AddCell(cell);


                tblGeneralInfoRight.AddCell(Emptycell);

                PosTypeChnk = new Chunk("Successful Applicant Name : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Position : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAPos.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("SAP Number : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSASAP.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Commencement Date : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSACommDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }


            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);



            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);



            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            //float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            /* PosTypeChnk = new Chunk("SAP Position No: ", ddlLabelFonts);
             PosTypePh1 = new Phrase(PosTypeChnk);
             PosTypevalcell = new PdfPCell(PosTypePh1);
             PosTypevalcell.Border = 0;
             tblPositionDet.AddCell(PosTypevalcell);

             PosTypekVal = new Chunk(lblSAPPositionNo.Text, ddlFonts);
             PosTypekValPh2 = new Phrase(PosTypekVal);
             PosTypealcell2 = new PdfPCell(PosTypekValPh2);
             PosTypealcell2.Border = 0;
             tblPositionDet.AddCell(PosTypealcell2);*/

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReportsTo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Type of Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeofPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Proposed Start Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblProStartDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Fixed Term End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFixedEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblPositionDet);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            //  iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            PdfPTable pdfPHeader = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader.SetWidths(tblGeneralInfoWidth);

            PdfPCell header = new PdfPCell(new Phrase("Position Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);
            header = new PdfPCell(new Phrase("Job Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);


            PosTypeChnk = new Chunk("Attached updated Role Statement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PdfPTable tblAttach = new PdfPTable(3);
            tblGeneralInfoWidth = new float[] { 25f, 50f, 25f };
            tblAttach.SetWidths(tblGeneralInfoWidth);
            PosTypeChnk = new Chunk(" FileType ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Name ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);


            DataTable dtJobDetails = (DataTable)ViewState["vwJobDetails"];

            if (dtJobDetails != null && dtJobDetails.Rows.Count > 0)
            {
                for (int count = 0; count <= dtJobDetails.Rows.Count - 1; count++)
                {
                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Type"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Name"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Modified"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    tblAttach.AddCell(PosTypevalcell);

                }
            }

            PdfPCell attachCell = new PdfPCell(tblAttach);
            tblJobDetailsDet.AddCell(attachCell);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);
            //tblRenumeration.AddCell(phEmpty);
            //tblRenumeration.AddCell(phEmpty);

            //tblRenumeration.AddCell(phEmpty);
            PosTypeChnk = new Chunk(" ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(" ", ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Grade: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatGrade.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("FAR: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatFAR.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("STI: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatSTI.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Utilities: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            /*PosTypekVal = new Chunk(lblExpatUtilities.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Relocation: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);*/

            PosTypekVal = new Chunk(lblExpatRelocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatVehicle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("If other (specify): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatIfAny.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PdfPCell renumerationCell = new PdfPCell(tblRenumeration);
            renumerationCell.Border = 0;
            tblJobDetailsDet.AddCell(renumerationCell);


            leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            rightCell = new PdfPCell(tblJobDetailsDet);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl1.AddCell(rightCell);

            pdfDoc.Add(headerTbl1);

            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("                 Approval History", headFont);
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
        private void GenerateContractorPDF()
        {
            string filename = "Apptohire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
            Document pdfDoc = new Document(new iTextSharp.text.Rectangle(325f, 144f), 10, 10, 120, 10);
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

            PdfWriter pdfwriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfPagePaymentHistory page = new pdfPagePaymentHistory();
            pdfwriter.PageEvent = page;
            pdfDoc.Open();

            PdfPTable headerTbl = new PdfPTable(2);

            float[] headerWidth = new float[] { 50f, 50f };
            headerTbl.SetWidths(headerWidth);

            iTextSharp.text.Font ddlLabelFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font ddlFonts = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 8f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk RefChunk = new Chunk("Reference Number: ", ddlLabelFonts);
            Phrase RefValPh1 = new Phrase(RefChunk);
            PdfPCell RefChnvalcell = new PdfPCell(RefValPh1);
            RefChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell);

            Chunk RefChnkVal = new Chunk(lblRefNo.Text, ddlFonts);
            Phrase RefValPh2 = new Phrase(RefChnkVal);
            PdfPCell RefChnvalcell2 = new PdfPCell(RefValPh2);
            RefChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(RefChnvalcell2);

            Chunk DateChnk = new Chunk("Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblDate.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Position Type: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblPositionType.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Reason Position Required: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReasonPositionRqd.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Replacement for Position Held by: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReplacePosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Budgeted Position: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBudgetPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Is this an increase in staffing levels: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblStaffingLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Recruitment Process: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRecruitmentProcess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Details: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDetails.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PdfPCell leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);


            PdfPTable tblGeneralInfoRight = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoRight.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Required By: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRequiredBy.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblcomments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);


            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            bool IsHRServiceUser = IsUserMemberOfGroup();

            if (IsHRServiceUser && SuccessfulApplicantRead.Visible)
            {
                PdfPCell Emptycell = new PdfPCell(new Phrase("   ", headFont));
                Emptycell.Colspan = 2;
                Emptycell.HorizontalAlignment = 0;
                Emptycell.Border = 0;
                tblGeneralInfoRight.AddCell(Emptycell);

                PdfPCell cell = new PdfPCell(new Phrase("Successful Applicant Details", headFont));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                cell.Border = 0;
                tblGeneralInfoRight.AddCell(cell);


                tblGeneralInfoRight.AddCell(Emptycell);

                PosTypeChnk = new Chunk("Successful Applicant Name : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Position : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSAPos.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("SAP Number : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSASAP.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Commencement Date : ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblSACommDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }
            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);



            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);



            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            //float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Role: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            /* PosTypeChnk = new Chunk("SAP Position No: ", ddlLabelFonts);
             PosTypePh1 = new Phrase(PosTypeChnk);
             PosTypevalcell = new PdfPCell(PosTypePh1);
             PosTypevalcell.Border = 0;
             tblPositionDet.AddCell(PosTypevalcell);

             PosTypekVal = new Chunk(lblSAPPositionNo.Text, ddlFonts);
             PosTypekValPh2 = new Phrase(PosTypekVal);
             PosTypealcell2 = new PdfPCell(PosTypekValPh2);
             PosTypealcell2.Border = 0;
             tblPositionDet.AddCell(PosTypealcell2);*/

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReportsTo.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Type of Contract Agreement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeofPosition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract Rate: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractRate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Effective Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblProStartDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFixedEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblPositionDet);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            // iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            PdfPTable pdfPHeader = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader.SetWidths(tblGeneralInfoWidth);

            PdfPCell header = new PdfPCell(new Phrase("Position Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);
            header = new PdfPCell(new Phrase("Job Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);


            PosTypeChnk = new Chunk("Attached updated Role Statement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PdfPTable tblAttach = new PdfPTable(3);
            tblGeneralInfoWidth = new float[] { 25f, 50f, 25f };
            tblAttach.SetWidths(tblGeneralInfoWidth);
            PosTypeChnk = new Chunk(" FileType ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Name ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            tblAttach.AddCell(gridcell);

            DataTable dtJobDetails = (DataTable)ViewState["vwJobDetails"];
            if (dtJobDetails.Rows.Count > 0)
            {
                for (int count = 0; count <= dtJobDetails.Rows.Count - 1; count++)
                {
                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Type"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Name"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Modified"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    tblAttach.AddCell(PosTypevalcell);

                }
            }

            PdfPCell attachCell = new PdfPCell(tblAttach);
            tblJobDetailsDet.AddCell(attachCell);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);
            //tblRenumeration.AddCell(phEmpty);
            //tblRenumeration.AddCell(phEmpty);

            //tblRenumeration.AddCell(phEmpty);
            PosTypeChnk = new Chunk(" ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(" ", ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract Deliverables / Role Statement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractDelivery.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            /*PosTypeChnk = new Chunk("FAR: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatFAR.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("STI: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatSTI.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Utilities: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatUtilities.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Relocation: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatRelocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatVehicle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("If other (specify): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatIfAny.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);*/

            PdfPCell renumerationCell = new PdfPCell(tblRenumeration);
            renumerationCell.Border = 0;
            tblJobDetailsDet.AddCell(renumerationCell);


            leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            rightCell = new PdfPCell(tblJobDetailsDet);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl1.AddCell(rightCell);

            pdfDoc.Add(headerTbl1);

            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);

            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" UserName ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Comments ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            //PosTypevalcell.Border = 0;
            pdfAppHistory.AddCell(gridcell);

            if (gdCommentHistory.Rows.Count > 0)
            {
                for (int cnt = 0; cnt <= gdCommentHistory.Rows.Count - 1; cnt++)
                {


                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[0].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(gdCommentHistory.Rows[cnt].Cells[1].Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);
                    System.Web.UI.WebControls.Label lblSummary = (System.Web.UI.WebControls.Label)gdCommentHistory.Rows[cnt].FindControl("lblComments");

                    PosTypeChnk = new Chunk(lblSummary.Text, ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    //PosTypevalcell.Border = 0;
                    pdfAppHistory.AddCell(PosTypevalcell);
                }
            }

            Paragraph positionHead = new Paragraph("                 Approval History", headFont);
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

        public class pdfPagePaymentHistory : iTextSharp.text.pdf.PdfPageEventHelper
        {

            //override the OnStartPage event handler to add our header
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


                Chunk chunk = new Chunk("Application To Hire", hFonts);
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
