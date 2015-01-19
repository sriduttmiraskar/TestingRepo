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
    public partial class NewHireReview : WebPartPage
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
                    DivSalaryPositionDetails.Visible = false;
                    DivWagedPositionDetails.Visible = false;
                    DivContractorPositionDetails.Visible = false;
                    DivExpatPositionDetails.Visible = false;
                    divSalaryRemunerationDetails.Visible = false;
                    divWagedRemunerationDetails.Visible = false;
                    divContractorJobDetails.Visible = false;
                    divExpatRemunerationDetails.Visible = false;

                    btnPDF.Visible = false;

                    if (strRefno != "")
                    {
                        lblRefNo.Text = strRefno;
                        string sError = VerifyUser(UserName, strRefno);
                        if (sError != "ACCESSDENIED")
                        {
                            bool IsHRServiceUser = IsUserMemberOfGroup();

                            //if (sError == "NOTCURRENTAPPROVER")
                            //{
                            //    btnApprove.Visible = false;
                            //    btnReject.Visible = false;
                            //    divComments.Visible = false;
                            //}
                            //else
                            //{
                            //    btnApprove.Visible = true;
                            //    if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) == "HRServices")
                            //        btnReject.Visible = false;
                            //    else
                            //        btnReject.Visible = true; 
                            //    divComments.Visible = true;
                            //}
                            // No idea why this below idiotic logic-if condition was written.
                            // Developers dont write comments and reviewers wont mind checking it!!!
                            // Commenting it unless any valid reason to uncomment it.

                            // If user is HR Services and workflow has not yet reached HR Services level.
                            // But no logic is written if user is HR Services and he is the approver and at the same level.
                            // Next if is written by Sri, commenting below idiotic "if"
                            //if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) != "HRServices")
                            //{
                            //    btnApprove.Visible = false;
                            //    btnPDF.Visible = false;
                            //    btnReject.Visible = false;
                            //    divComments.Visible = false;
                            //}

                            // If current user is HR Services and workflow is in HR Manager approval stage and user is current approver, 
                            // then show approve and reject button.
                            if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) == "HRManager" && sError == "")
                            {
                                btnApprove.Visible = true;
                                btnReject.Visible = true;
                                divComments.Visible = true;
                            }
                            else if (sError == "NOTCURRENTAPPROVER")
                            {
                                btnApprove.Visible = false;
                                btnPDF.Visible = false;
                                btnReject.Visible = false;
                                divComments.Visible = false;
                            }
                            if (IsHRServiceUser && Convert.ToString(ViewState["ApprovalStatus"]) == "HRServices")
                            {
                                btnApprove.Text = "Acknowledge";
                                btnReject.Visible = false;
                                divComments.Visible = false;
                                btnPDF.Visible = true;
                            }
                            
                            if (IsHRServiceUser && (Convert.ToString(ViewState["Status"]) == "Approved" || Convert.ToString(ViewState["Status"]) == "Rejected"))
                            {
                                btnPDF.Visible = true;
                            }

                            string strPositionType = "";
                            GetNewHireGeneralInfo(strRefno, ref strPositionType);
                            GetCommentHistory(strRefno);

                            if (string.Equals(strPositionType, "Salary"))
                            {
                                DivSalaryPositionDetails.Visible = true;
                                divSalaryRemunerationDetails.Visible = true;
                                GetSalaryPositionDetails(strRefno);
                                GetSalaryRenumerationDetails(strRefno);
                                GetOfferChecklist();

                            }
                            else if (string.Equals(strPositionType, "Waged"))
                            {
                                DivWagedPositionDetails.Visible = true;
                                divWagedRemunerationDetails.Visible = true;
                                GetWagedPositionDetails(strRefno);
                                GetWagedRenumerationDetails(strRefno);
                                GetOfferChecklist();
                            }
                            else if (string.Equals(strPositionType, "Contractor"))
                            {
                                DivContractorPositionDetails.Visible = true;
                                divContractorJobDetails.Visible = true;
                                DivHRManagerCheckList.Visible = false;
                                GetContractorPositionDetails(strRefno);
                                GetJobDetails(strRefno);
                                GetContractorRenumerationDetails(strRefno);
                            }
                            else if (string.Equals(strPositionType, "Expatriate"))
                            {
                                DivExpatPositionDetails.Visible = true;
                                divExpatRemunerationDetails.Visible = true;
                                GetPositionDetailsForExpat(strRefno);
                                GetExpatRenumerationDetails(strRefno);
                                GetExpatPersonalDetails(strRefno);
                                GetOfferChecklist();
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
                LogUtility.LogError("HRWebForms.NewHireReview.Page_Load", ex.Message);
            }
        }


        private string VerifyUser(string username, string refno)
        {
            string Error = string.Empty;
            string businessunit = string.Empty;
            string ApprovalStatus = string.Empty;
            string Status = string.Empty;
            string positiontype = string.Empty;
            chkbxLstExpat.Enabled = false;
            //string email = GetEmailFromAD(username);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               if (username != "")
               {
                   string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                   SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   SPQuery oquery = new SPQuery();
                   oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
                   SPListItemCollection collitems = olist.GetItems(oquery);
                   SPListItem listitem = collitems[0];
                   string value = Convert.ToString(listitem["BusinessUnit"]);

                   lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                   SPList olist2 = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   SPQuery oquery2 = new SPQuery();
                   oquery2.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
                   SPListItemCollection collitems3 = olist2.GetItems(oquery2);
                   SPListItem listitem2 = collitems3[0];
                   ApprovalStatus = Convert.ToString(listitem2["ApprovalStatus"]);
                   positiontype = Convert.ToString(listitem2["PositionType"]);
                   ViewState["PositionType"] = positiontype;
                   Status = Convert.ToString(listitem2["Status"]);

                   //For PDF Generation
                   ViewState["Status"] = Status;

                   btnApprove.Text = "Approve";
                   btnReject.Visible = true;

                   if (Status == "Approved" || Status == "Rejected")
                   {
                       bool IsHRServiceUser = IsUserMemberOfGroup();
                       if (IsHRServiceUser)
                       {
                           divComments.Visible = false;
                           btnPDF.Visible = true;
                       }
                       else
                       {
                           divComments.Visible = false;
                       }
                   }
                   if (Status == "Pending Approval")
                   {
                       ViewState["ApprovalStatus"] = ApprovalStatus;
                       bool IsHRServiceUser = IsUserMemberOfGroup();
                       if (IsHRServiceUser && ApprovalStatus == "HRServices")
                       {
                           Error = "";
                           btnApprove.Text = "Acknowledge";
                           btnReject.Visible = false;
                           divComments.Visible = false;
                           btnPDF.Visible = true;
                       }
                       // If current user is HR Services, its not the end of world!
                       //else if (IsHRServiceUser)
                       //{
                       //    Error = "";
                       //    btnApprove.Visible = false;
                       //    btnPDF.Visible = false;
                       //    divComments.Visible = false;
                       //}
                       else
                       {
                           if (value != "")
                           {
                               businessunit = value;
                               ViewState["BusinessUnit"] = businessunit;
                               string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");
                               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
                               SPQuery oquery1 = new SPQuery();
                               if (ApprovalStatus == "HRManager")
                               {
                                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                                   oquery1.Query = "<Where><And><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                                       "</Value></Eq><Eq><FieldRef Name='Approver'/><Value Type='User'>" + username +
                                                       "</Value></Eq></And></Where>";
                               }
                               /*else if (ApprovalStatus == "Vehicle")
                                   oquery1.Query = "<Where><And><Contains><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value +
                                                     "</Value></Contains><Eq><FieldRef Name='VehicleApprover'/><Value Type='User'>" + username +
                                                     "</Value></Eq></And></Where>";*/
                               if (oquery1.Query != "" && oquery1.Query != null)
                               {
                                   SPListItemCollection collitems1 = olist1.GetItems(oquery1);
                                   if (collitems1.Count > 0)
                                   {
                                       chkbxLstExpat.Enabled = true;
                                       Error = "";
                                       divComments.Visible = true;
                                   }
                                   else
                                   {
                                       Error = CheckIfAnyApproverOrInitiator(username, value);
                                   }
                               }
                               else
                               {
                                   Error = CheckIfAnyApproverOrInitiator(username, value);
                               }
                           }
                           else
                           {
                               Error = "ACCESSDENIED";
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

        private string CheckIfAnyApproverOrInitiator(string username, string value)
        {
            string Error = string.Empty;
            string lstURL1 = HrWebUtility.GetListUrl("NewHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);

               SPQuery oquery3 = new SPQuery();

               // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
               oquery3.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + value + "</Value></Eq></Where>";

               SPListItemCollection collitems2 = olist1.GetItems(oquery3);
               if (collitems2.Count > 0)
               {
                   if (Convert.ToString(collitems2[0]["Approver"]).Contains(username) ||
                       Convert.ToString(collitems2[0]["VehicleApprover"]).Contains(username))
                   {
                       Error = "NOTCURRENTAPPROVER";
                   }
                   else
                   {

                       string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                       SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery = new SPQuery();
                       oquery.Query = "<Where><And><Contains><FieldRef Name=\'Author\' /><Value Type=\"User\">" + username +
                                                           "</Value></Contains><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + lblRefNo.Text + "</Value></Eq></And></Where>";

                       SPListItemCollection collitems = olist.GetItems(oquery);
                       bool isUserHRService = IsUserMemberOfGroup();

                       if (collitems.Count > 0 || isUserHRService)
                       {
                           Error = "NOTCURRENTAPPROVER";
                       }
                       else
                       {
                           Error = "ACCESSDENIED";
                       }
                   }
               }
               else
               {
                   Error = "ACCESSDENIED";
               }
           });
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

        private void GetNewHireGeneralInfo(string strRefno, ref string strPositionType)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPListItemCollection collitems = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               collitems = olist.GetItems(oquery);
           });
            foreach (SPListItem listitem in collitems)
            {
                lblFirstName.Text = Convert.ToString(listitem["Title"]);
                ViewState["FirstName"] = lblFirstName.Text;
                lblLastName.Text = Convert.ToString(listitem["LastName"]);
                ViewState["LastName"] = lblLastName.Text;
                lblAddress.Text = Convert.ToString(listitem["Address"]);
                lblCity.Text = Convert.ToString(listitem["City"]);
                lblState.Text = Convert.ToString(listitem["State"]);
                lblPostCode.Text = Convert.ToString(listitem["PostCode"]);
                lblDate.Text = Convert.ToString(listitem["Date"]);
                if (listitem["AppToHireRefNo"] != null)
                {
                    lblAppToHireRefNo.Text = Convert.ToString(listitem["AppToHireRefNo"]);
                }
                else
                {
                    AppToHireRefNoText.Visible = false;
                    lblAppToHireRefNo.Visible = false;
                }
                lblPositiontype.Text = Convert.ToString(listitem["PositionType"]);
                strPositionType = lblPositiontype.Text;
                lblTypeOfRole.Text = Convert.ToString(listitem["Role"]);
            }
        }

        private void GetSalaryPositionDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = lblPositionTitle.Text;
                   lblCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                   lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                   lblWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);
                   lblReportsTo.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));
                   lblTypeofContract.Text = Convert.ToString(listitem["ContractType"]);

                   if (listitem["CommencementDate"] != null)
                       lblCommencementDate.Text = Convert.ToDateTime(listitem["CommencementDate"]).ToString("dd/MM/yyyy");

                   if (listitem["ProposedEndDate"] != null)
                       lblTermEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

                   lblNextSalaryReview.Text = Convert.ToString(listitem["NextSalaryReview"]);
                   lblWhowillsign.Text = GetUser(Convert.ToString(listitem["WhoSignLetter"]));
                   lblNotes.Text = Convert.ToString(listitem["Notes"]);
               }
           });
        }

        private void GetSalaryRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblGrade.Text = Convert.ToString(listitem["Grade"]);
                   lblFAR.Text = Convert.ToString(listitem["FAR"]);
                   lblSTI.Text = Convert.ToString(listitem["STI"]);
                   lblVehicle.Text = Convert.ToString(listitem["Vehicle"]);
                   ViewState["STI"] = lblSTI.Text;
                   ViewState["Vehicle"] = lblVehicle.Text;
                   lblIfOther.Text = Convert.ToString(listitem["OtherVehicleText"]);
                   lblRelocation.Text = Convert.ToString(listitem["Relocation"]);
                   lblRelocationDetails.Text = Convert.ToString(listitem["RelocationDetails"]);
               }
           });
        }

        private void GetWagedPositionDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblWagedPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = lblWagedPositionTitle.Text;
                   lblWagedCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                   lblWagedBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                   lblWagedWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblWagedSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);
                   lblWagedReportsto.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));

                   if (listitem["CommencementDate"] != null)
                       lblWagedCommencementDate.Text = Convert.ToDateTime(listitem["CommencementDate"]).ToString("dd/MM/yyyy");

                   if (listitem["ProposedEndDate"] != null)
                       lblWagedTermEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

                   lblWagedWhowillsign.Text = GetUser(Convert.ToString(listitem["WhoSignLetter"]));
                   lblWagedNotes.Text = Convert.ToString(listitem["Notes"]);
               }
           });
        }

        private void GetWagedRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblWagedPayLevel.Text = Convert.ToString(listitem["Level"]);
                   lblWagedRosterType.Text = Convert.ToString(listitem["RosterType"]);
                   lblWagedCrew.Text = Convert.ToString(listitem["Crew"]);
                   lblWagedShiftTeamLeader.Text = Convert.ToString(listitem["ShiftTeamLeader"]);
                   lblWagedAllowances.Text = Convert.ToString(listitem["Allowances"]);
                   lblWagedVehicle.Text = Convert.ToString(listitem["Vehicle"]);
                   ViewState["Vehicle"] = lblWagedVehicle.Text;

                   lblWagedIfOthers.Text = Convert.ToString(listitem["OtherVehicleText"]);
               }
           });
        }

        private void GetContractorPositionDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblContractorPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = lblContractorPositionTitle.Text;
                   lblContractorCompany.Text = Convert.ToString(listitem["CompanyTradingName"]);
                   lblContractorABN.Text = Convert.ToString(listitem["ABN"]);
                   lblContractorBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                   lblContractorWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblContractorSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);
                   lblContractorReportsto.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));
                   lblContractorCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                   lblContractorContractRate.Text = Convert.ToString(listitem["ContractRate"]);
                   lblContractorRateTypeField.Text = Convert.ToString(listitem["RateTypeField"]);

                   if (listitem["CommencementDate"] != null)
                       lblContractorContractStartDate.Text = Convert.ToDateTime(listitem["CommencementDate"]).ToString("dd/MM/yyyy");

                   if (listitem["ProposedEndDate"] != null)
                       lblContractorContractEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

                   lblContractorPaymentTerms.Text = Convert.ToString(listitem["PaymentTerms"]);
                   lblContractorIfother.Text = Convert.ToString(listitem["IfOtherSpecify"]);
                   lblContractorGST.Text = Convert.ToString(listitem["GST"]);
                   lblContractorWhoWillSign.Text = GetUser(Convert.ToString(listitem["WhoSignLetter"]));

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
               string app = SPContext.Current.Site.Protocol + "//" + SPContext.Current.Site.HostName;
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
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
                   cellName.Text = "<a target='_blank' href='" + app + url + "'>" + Convert.ToString(listitem["Name"]) + "</a>";
                   LinkButton lnkName = new LinkButton();
                   lnkName.Text = app + url;
                   row.Cells.Add(cellName);

                   TableCell celModified = new TableCell();
                   celModified.Text = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy");
                   row.Cells.Add(celModified);
                   tblAttachment.Rows.Add(row);
                   dtJobdetails.Rows.Add(new string[] { srType, Convert.ToString(listitem["Name"]), celModified.Text });

               }
           });
            ViewState["vwJobDetails"] = dtJobdetails;

        }

        private void GetContractorRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblContractorServicesToBe.Text = Convert.ToString(listitem["ServicesProvided"]);
               }
           });
        }

        private void GetPositionDetailsForExpat(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblExpatPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                   ViewState["PositionTitle"] = lblExpatPositionTitle.Text;
                   lblExpatCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                   lblExpatBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                   lblExpatWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblExpatSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);
                   lblExpatReportsto.Text = GetUser(Convert.ToString(listitem["ReportsTo"]));

                   if (listitem["CommencementDate"] != null)
                       lblExpatEffectiveDate.Text = Convert.ToDateTime(listitem["CommencementDate"]).ToString("dd/MM/yyyy");

                   lblExpatContractPeriod.Text = Convert.ToString(listitem["ContractPeriod"]);

                   if (listitem["ProposedEndDate"] != null)
                       lblExpatContractEndDate.Text = Convert.ToDateTime(listitem["ProposedEndDate"]).ToString("dd/MM/yyyy");

                   lblExpatNextSalaryReview.Text = Convert.ToString(listitem["NextSalaryReview"]);
                   lblExpatHomeLocation.Text = Convert.ToString(listitem["HomeLocation"]);
                   lblExpatWhowillsign.Text = GetUser(Convert.ToString(listitem["WhoSignLetter"]));
                   lblExpatNotes.Text = Convert.ToString(listitem["Notes"]);
               }
           });
        }

        private void GetExpatRenumerationDetails(string strRefno)
        {
            string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   lblExpatGrade.Text = Convert.ToString(listitem["Grade"]);
                   lblExpatFAR.Text = Convert.ToString(listitem["FAR"]);
                   lblExpatSTI.Text = Convert.ToString(listitem["STI"]);
                   ViewState["STI"] = lblExpatSTI.Text;
               }
           });
        }

        private void GetExpatPersonalDetails(string strRefno)
        {
            DataTable dtDependent = new DataTable();
            dtDependent.Columns.Add("Count");
            dtDependent.Columns.Add("Name");
            dtDependent.Columns.Add("DOB");


            string lstURL = HrWebUtility.GetListUrl("NewHirePersonnelDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oQuery = new SPQuery();
               oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               SPListItemCollection newHireItems = oList.GetItems(oQuery);
               foreach (SPListItem listitem in newHireItems)
               {
                   lblMaritalStatus.Text = Convert.ToString(listitem["MaritalStatus"]);
                   string strDependent = Convert.ToString(listitem["Dependent"]);
                   string strName = Convert.ToString(listitem["Name"]);
                   string strDOB = Convert.ToDateTime(listitem["DOB"]).ToString("dd/MM/yyyy");

                   dtDependent.Rows.Add(new string[] { strDependent, strName, strDOB });

               }

               if (dtDependent.Rows.Count > 0)
               {
                   DependentsTable.Rows.Clear();
                   AddDependentTableHeaders();
                   UpdateDependentsFromDataTable(dtDependent);
               }
           });
        }

        private void UpdateDependentsFromDataTable(DataTable dtDependent)
        {
            if (dtDependent != null)
            {
                if (dtDependent.Rows.Count > 0)
                {
                    for (int rowCnt = 0; rowCnt < dtDependent.Rows.Count; rowCnt++)
                    {
                        int cnt = rowCnt + 1;
                        TableRow tblRow = new TableRow();
                        tblRow.ID = "tblRow" + cnt;

                        TableCell tblcellDependcnt = new TableCell();
                        tblcellDependcnt.ID = "tblcellDependcnt" + cnt;


                        System.Web.UI.WebControls.Label lblDep = new System.Web.UI.WebControls.Label();
                        lblDep.ID = "lblDep" + cnt;
                        lblDep.CssClass = "span12";
                        lblDep.Attributes.CssStyle.Add("text-align", "center");

                        lblDep.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Count"]);
                        tblcellDependcnt.Controls.Add(lblDep);

                        TableCell tblcellName = new TableCell();
                        tblcellDependcnt.ID = "tblcellName" + cnt;

                        System.Web.UI.WebControls.Label txtName = new System.Web.UI.WebControls.Label();
                        txtName.ID = "txtName" + cnt;
                        txtName.CssClass = "span12";
                        txtName.Attributes.CssStyle.Add("text-align", "center");
                        txtName.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Name"]);
                        tblcellName.Controls.Add(txtName);

                        TableCell tblcellDOB = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;

                        System.Web.UI.WebControls.Label dtCntrl = new System.Web.UI.WebControls.Label();
                        string strDOB = Convert.ToString(dtDependent.Rows[cnt - 1]["DOB"]);

                        if (!string.IsNullOrEmpty(strDOB))
                            dtCntrl.Text = Convert.ToString(strDOB);
                        dtCntrl.CssClass = "span12";
                        dtCntrl.Attributes.CssStyle.Add("text-align", "center");
                        dtCntrl.ID = "dtCntrl" + cnt;
                        tblcellDOB.Controls.Add(dtCntrl);

                        tblRow.Cells.Add(tblcellDependcnt);
                        tblRow.Cells.Add(tblcellName);
                        tblRow.Cells.Add(tblcellDOB);

                        DependentsTable.Rows.Add(tblRow);
                    }
                }
            }
        }

        private void AddDependentTableHeaders()
        {
            TableHeaderRow HeadRw = new TableHeaderRow();
            HeadRw.Style.Add("width", "100%");
            TableHeaderCell tblCellDep = new TableHeaderCell();
            tblCellDep.Style.Add("width", "20%");
            TableHeaderCell tblCellName = new TableHeaderCell();
            tblCellName.Style.Add("width", "40%");
            TableHeaderCell tblCellDOB = new TableHeaderCell();
            tblCellDOB.Style.Add("width", "40%");


            tblCellDep.Text = "Dependent";
            tblCellName.Text = "Name";
            tblCellDOB.Text = "DOB";

            HeadRw.Cells.Add(tblCellDep);
            HeadRw.Cells.Add(tblCellName);
            HeadRw.Cells.Add(tblCellDOB);

            DependentsTable.Rows.Add(HeadRw);
        }

        private void GetCommentHistory(string strRefno)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("UserName", typeof(string)));
            dt.Columns.Add(new DataColumn("Comments", typeof(string)));


            string lstURL = HrWebUtility.GetListUrl("NewHireApprovalHistory");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   string strModified = Convert.ToDateTime(listitem["Modified"]).ToString("dd/MM/yyyy H:mm:ss");
                   string strAuthor = Convert.ToString(listitem["ApproverName"]) + " (" + Convert.ToString(listitem["ApproverStep"]) + ")";
                   string strComments = Convert.ToString(listitem["Comment"]);

                   dt.Rows.Add(new string[] { strModified, strAuthor, strComments });
               }
           });
            gdCommentHistory.DataSource = dt;
            gdCommentHistory.DataBind();
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateNewHireGeneralInfo("Approved");
                if (btnApprove.Text == "Acknowledge")
                    Response.Redirect("/people/Pages/HRWeb/NewHireReview.aspx?refno=" + lblRefNo.Text);
                else
                    Response.Redirect("/people/Pages/HRWeb/NewHireWorkflowApproval.aspx?refno=" + lblRefNo.Text);
            }
            catch (Exception ex)
            {
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.NewHireReview.btnApprove_Click", ex.Message);
            }
        }

        private void UpdateNewHireGeneralInfo(string status)
        {
            string strRefno = lblRefNo.Text.Trim();
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oQuery = new SPQuery();
               oQuery.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
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
                   else if (status == "Rejected")
                   {
                       item["Status"] = "Rejected";
                       if (currapprover != "HRServices")
                           item["RejectedBy"] = UserName;
                       else
                           item["RejectedBy"] = "HRServices";
                       item["RejectedLevel"] = currapprover;
                       nextapprover = GetNextApproverandSendEmail(currapprover, "Rejected");
                   }
                   item["ApprovalStatus"] = nextapprover;
                   item.Update();
                   if (currapprover == "HRManager")
                   {
                       UpdateOfferChecklists();
                   }
               }
           });
        }

        private string GetNextApproverandSendEmail(string currapprover, string status)
        {
            string nextapprover = string.Empty;
            string businessunit = Convert.ToString(ViewState["BusinessUnit"]);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               if (currapprover != "" && businessunit != "")
               {
                   string lstURL = HrWebUtility.GetListUrl("NewHireApprovalInfo");
                   SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                   string PositionType = Convert.ToString(ViewState["PositionType"]);
                   SPQuery oquery3 = new SPQuery();

                   // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                   oquery3.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit + "</Value></Eq></Where>";

                   SPListItemCollection collitems2 = olist.GetItems(oquery3);
                   if (collitems2.Count > 0)
                   {
                       SPListItem item = collitems2[0];
                       if (status == "Pending Approval")
                       {
                           if (currapprover == "HRManager")
                           {
                               if (Convert.ToString(ViewState["STI"]) == "Yes" || (Convert.ToString(ViewState["Vehicle"]) != "N/A" && Convert.ToString(ViewState["Vehicle"]) != ""))
                               {
                                   //nextapprover = "Vehicle";
                                   ViewState["VehicleApproverEmail"] = item["VehicleApprover"];
                               }
                               //else
                               //{
                               nextapprover = "HRServices";
                               ViewState["ApproverEmail"] = "HRServices";
                               //}
                           }
                           /*else if (currapprover == "Vehicle")
                           {
                               nextapprover = "HRServices";
                               ViewState["ApproverEmail"] = "HRServices";

                           }*/
                       }
                   }
               }
               SendEmail(status);
           });
            return nextapprover;
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateComment();
                UpdateNewHireGeneralInfo("Rejected");
                Response.Redirect("/people/Pages/HRWeb/NewHireReview.aspx?refno=" + lblRefNo.Text);
            }
            catch (Exception ex)
            {
                lblError.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.NewHireReview.btnReject_Click", ex.Message);
            }
        }

        private void UpdateComment()
        {
            string appno = lblRefNo.Text.Trim();
            string approveremail = UserName;
            string username = GetUserNameFromAD(approveremail);
            string approverid = UserName.Split('@')[0].Trim();
            string comment = txtComments.Text;
            string approverstep = Convert.ToString(ViewState["ApprovalStatus"]);

            string lstURL = HrWebUtility.GetListUrl("NewHireApprovalHistory");
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
                oQuery.Query = "<Query><Where><Eq><FieldRef Name='FormType' /><Value Type='Text'>NewHire</Value></Eq></Where></Query>";


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
                               "<FieldRef Name='HRManagerApprovalMessage' />");
                SPListItemCollection collListItems = lst.GetItems(oQuery);

                foreach (SPListItem itm in collListItems)
                {
                    if (Convert.ToString(itm["FormType"]) == "NewHire")
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
                        string url = site.Url + "/pages/hrweb/newhirereview.aspx?refno=" + strRefNo;
                        strFrom = Convert.ToString(itm["Title"]);
                        if (status == "Approved")
                        {
                            /*strTo = Convert.ToString(ViewState["Initiator"]);
                            strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                            strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo);*/
                        }
                        else if (status == "Rejected")
                        {
                            strTo = Convert.ToString(ViewState["Initiator"]);
                            strSubject = Convert.ToString(itm["RejectedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").
                                Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["RejectedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"])).
                                Replace("&lt;NAME&gt;", Convert.ToString(ViewState["FirstName"]) + " " + Convert.ToString(ViewState["LastName"]));
                        }
                        else
                        {
                            strTo = Convert.ToString(ViewState["ApproverEmail"]);
                            string[] tmparr = strTo.Split('|');
                            strTo = tmparr[tmparr.Length - 1];
                            strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").
                                Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));
                            strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"])).
                                Replace("&lt;NAME&gt;", Convert.ToString(ViewState["FirstName"]) + " " + Convert.ToString(ViewState["LastName"]));
                        }

                        if (strTo.Contains("#"))
                            strTo = strTo.Split('#')[1];

                        if (strTo.ToLower() == "hrservices")
                        {
                            string to = string.Empty;
                            strSubject = Convert.ToString(itm["ApprovedSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").
                                Replace("<POSTITLE>", Convert.ToString(ViewState["PositionTitle"]));

                            strMessage = Convert.ToString(itm["ApprovedMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                                Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", Convert.ToString(ViewState["PositionTitle"])).
                                Replace("&lt;NAME&gt;", Convert.ToString(ViewState["FirstName"]) + " " + Convert.ToString(ViewState["LastName"]));

                            //using (SPSite newSite = new SPSite(site.ID))
                            //{
                                //using (SPWeb newWeb = newSite.OpenWeb(web.ID))
                                //{
                                    /*SPGroup group = newWeb.Groups["HR Services"];
                                    foreach (SPUser user in group.Users)
                                    {
                                        to += ";" + user.Email;
                                    }*/
                            to += ";" + HrWebUtility.GetDistributionEmail("HR Services");
                                    string vehicleapprover = Convert.ToString(ViewState["VehicleApproverEmail"]);
                                    if (vehicleapprover.Contains("#"))
                                        vehicleapprover = vehicleapprover.Split('#')[1];
                                    to += ";" + vehicleapprover;
                                    to = to.TrimStart(';');

                                    string initiator = Convert.ToString(ViewState["Initiator"]);
                                    if (initiator.Contains("#"))
                                        initiator = initiator.Split('#')[1];
                                    to += ";" + initiator;

                                    string HRMgr = Convert.ToString(ViewState["HRManager"]);
                                    if (HRMgr.Contains("#"))
                                        HRMgr = HRMgr.Split('#')[1];
                                    to += ";" + HRMgr;

                                    strTo = to;
                                //}
                            //}

                        }

                        if (strTo != "")
                        {
                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");
                            string[] mailto = strTo.Split(';');
                            var distinctIDs = mailto.Distinct();
                            foreach (string s in distinctIDs)
                                if (s.Trim() != "") mailMessage.To.Add(s);
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
                    oItem["FormType"] = "NewHire";
                    oItem.Update();
                //}
            });
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

        private SPListItemCollection GetListData(string GetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblRefNo.Text.Trim();
            string lstURL = HrWebUtility.GetListUrl(GetListByName);
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oQuery = new SPQuery();
               oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               collectionItems = oList.GetItems(oQuery);
           });
            return collectionItems;
        }

        protected void btnPDF_Click(object sender, EventArgs e)
        {
            if (string.Equals(lblPositiontype.Text, "Salary", StringComparison.OrdinalIgnoreCase))
            {
                GenerateSalaryPDF();
            }
            else if (string.Equals(lblPositiontype.Text, "Waged", StringComparison.OrdinalIgnoreCase))
            {
                GenerateWagedPDF();
            }
            else if (string.Equals(lblPositiontype.Text, "Contractor", StringComparison.OrdinalIgnoreCase))
            {
                GenerateContractorPDF();
            }
            else if (string.Equals(lblPositiontype.Text, "Expatriate", StringComparison.OrdinalIgnoreCase))
            {
                GenerateExpatPDF();
            }


        }

        private void GenerateSalaryPDF()
        {
            string filename = "NewHire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
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

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk DateChnk = new Chunk("First Name: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblFirstName.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Last Name: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblLastName.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Address: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblAddress.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("City: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCity.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("State: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblState.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Post Code: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPostCode.Text, ddlFonts);
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

            PosTypeChnk = new Chunk("Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            if (lblAppToHireRefNo.Visible)
            {
                PosTypeChnk = new Chunk("App To Hire Ref No: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblAppToHireRefNo.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }

            PosTypeChnk = new Chunk("Position type: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositiontype.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Type Of Role: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeOfRole.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);

            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
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

            PosTypeChnk = new Chunk("Type of Contract: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeofContract.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Commencement Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblCommencementDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblCommencementDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Term End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblTermEndDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblTermEndDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Next Salary Review: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblNextSalaryReview.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Who will sign the letter: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWhowillsign.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Notes: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblNotes.Text, ddlFonts);
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
            header = new PdfPCell(new Phrase("Remuneration Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);

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

            PosTypeChnk = new Chunk("Relocation: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRelocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Relocation Details: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRelocationDetails.Text, ddlFonts);
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

            //CheckList 
            Paragraph positionHead = new Paragraph("            Offer Checklist", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);


            PdfPTable CheckBoxListTabel = new PdfPTable(2);
            float[] CheckBoxListTabelWidth = new float[] { 2.5f, 97.5f };
            CheckBoxListTabel.SetWidths(CheckBoxListTabelWidth);

            SPFile Chkedfile;
            SPFile NotChkedfile;
            Chkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_cross_small.jpg");
            byte[] imageBytes = Chkedfile.OpenBinary();
            iTextSharp.text.Image Chkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);

            NotChkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_blank_small.jpg");
            imageBytes = NotChkedfile.OpenBinary();
            iTextSharp.text.Image NotChkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);


            string strRefno = lblRefNo.Text;
            SPListItemCollection ChkListcollectionItems = GetListData("NewHireOfferChecklist", strRefno);
            if (ChkListcollectionItems != null && ChkListcollectionItems.Count > 0)
            {
                foreach (SPListItem ListItems in ChkListcollectionItems)
                {

                    if (Convert.ToString(ListItems["Immigration"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["ReferenceCheck"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["Resume"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["InterviewNotes"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["PsychometricTesting"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }

                }
            }
            pdfDoc.Add(CheckBoxListTabel);

            pdfDoc.Add(phEmpty);

            //Comment History
            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
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
                    PosTypevalcell = new PdfPCell(PosTypePh1);
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

            positionHead = new Paragraph("            Approval History", headFont);
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

        private void GenerateWagedPDF()
        {
            string filename = "NewHire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
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

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk DateChnk = new Chunk("First Name: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblFirstName.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Last Name: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblLastName.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Address: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblAddress.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("City: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCity.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("State: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblState.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Post Code: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPostCode.Text, ddlFonts);
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

            PosTypeChnk = new Chunk("Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            if (lblAppToHireRefNo.Visible)
            {
                PosTypeChnk = new Chunk("App To Hire Ref No: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblAppToHireRefNo.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }

            PosTypeChnk = new Chunk("Position type: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositiontype.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Type Of Role: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeOfRole.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);

            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedReportsto.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Commencement Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblWagedCommencementDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblWagedCommencementDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Term End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblWagedTermEndDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblWagedTermEndDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Who will sign the letter: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedWhowillsign.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Notes: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedNotes.Text, ddlFonts);
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
            header = new PdfPCell(new Phrase("Remuneration Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Pay Level: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedPayLevel.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Roster Type: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedRosterType.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Crew: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedCrew.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Shift Team Leader: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedShiftTeamLeader.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Allowances: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWagedAllowances.Text, ddlFonts);
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

            PosTypekVal = new Chunk(lblWagedIfOthers.Text, ddlFonts);
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

            //CheckList 
            Paragraph positionHead = new Paragraph("            Offer Checklist", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);


            PdfPTable CheckBoxListTabel = new PdfPTable(2);
            float[] CheckBoxListTabelWidth = new float[] { 2.5f, 97.5f };
            CheckBoxListTabel.SetWidths(CheckBoxListTabelWidth);

            SPFile Chkedfile;
            SPFile NotChkedfile;
            Chkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_cross_small.jpg");
            byte[] imageBytes = Chkedfile.OpenBinary();
            iTextSharp.text.Image Chkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);

            NotChkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_blank_small.jpg");
            imageBytes = NotChkedfile.OpenBinary();
            iTextSharp.text.Image NotChkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);


            string strRefno = lblRefNo.Text;
            SPListItemCollection ChkListcollectionItems = GetListData("NewHireOfferChecklist", strRefno);
            if (ChkListcollectionItems != null && ChkListcollectionItems.Count > 0)
            {
                foreach (SPListItem ListItems in ChkListcollectionItems)
                {

                    if (Convert.ToString(ListItems["Immigration"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["ReferenceCheck"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["Resume"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["InterviewNotes"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["PsychometricTesting"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }

                }
            }
            pdfDoc.Add(CheckBoxListTabel);

            pdfDoc.Add(phEmpty);

            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
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
                    PosTypevalcell = new PdfPCell(PosTypePh1);
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

            positionHead = new Paragraph("            Approval History", headFont);
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
            string filename = "NewHire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
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
            iTextSharp.text.Font cellFnt = iTextSharp.text.FontFactory.GetFont("Arial", 10f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.WHITE);

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk DateChnk = new Chunk("First Name: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblFirstName.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Last Name: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblLastName.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Address: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblAddress.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("City: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCity.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("State: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblState.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Post Code: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPostCode.Text, ddlFonts);
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

            PosTypeChnk = new Chunk("Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            if (lblAppToHireRefNo.Visible)
            {
                PosTypeChnk = new Chunk("App To Hire Ref No: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblAppToHireRefNo.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }

            PosTypeChnk = new Chunk("Position type: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositiontype.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Type Of Role: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeOfRole.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);


            iTextSharp.text.Font headFont = iTextSharp.text.FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);


            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);

            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Agency / Company / Trading Name: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorCompany.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("ABN: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorABN.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorReportsto.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract Rate (ex GST): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorContractRate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Rate Type Field: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorRateTypeField.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract Start Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblContractorContractStartDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblContractorContractStartDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblContractorContractEndDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblContractorContractEndDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Payment Terms: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorPaymentTerms.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("If other (specify): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorIfother.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("GST: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorGST.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Who will sign the letter: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorWhoWillSign.Text, ddlFonts);
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


            PosTypeChnk = new Chunk("Insurance: ", ddlLabelFonts);
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
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Name ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            tblAttach.AddCell(gridcell);

            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
            gridcell = new PdfPCell(PosTypePh1);
            gridcell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            tblAttach.AddCell(gridcell);

            DataTable dtJobDetails = (DataTable)ViewState["vwJobDetails"];
            if (dtJobDetails.Rows.Count > 0)
            {
                for (int count = 0; count <= dtJobDetails.Rows.Count - 1; count++)
                {
                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Type"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
                    tblAttach.AddCell(PosTypevalcell);

                    PosTypeChnk = new Chunk(" " + dtJobDetails.Rows[count]["Name"], ddlFonts);
                    PosTypePh1 = new Phrase(PosTypeChnk);
                    PosTypevalcell = new PdfPCell(PosTypePh1);
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

            PosTypeChnk = new Chunk("Services To Be Provided / Primary Objectives: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblContractorServicesToBe.Text, ddlFonts);
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
                    PosTypevalcell = new PdfPCell(PosTypePh1);
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

        private void GenerateExpatPDF()
        {
            string filename = "NewHire_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
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

            PdfPTable tblGeneralInfoLeft = new PdfPTable(2);
            float[] tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoLeft.SetWidths(tblGeneralInfoWidth);

            Chunk DateChnk = new Chunk("First Name: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblFirstName.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk PosTypeChnk = new Chunk("Last Name: ", ddlLabelFonts);
            Phrase PosTypePh1 = new Phrase(PosTypeChnk);
            PdfPCell PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            Chunk PosTypekVal = new Chunk(lblLastName.Text, ddlFonts);
            Phrase PosTypekValPh2 = new Phrase(PosTypekVal);
            PdfPCell PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Address: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblAddress.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("City: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCity.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("State: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblState.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Post Code: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPostCode.Text, ddlFonts);
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

            PosTypeChnk = new Chunk("Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            if (lblAppToHireRefNo.Visible)
            {
                PosTypeChnk = new Chunk("App To Hire Ref No: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblAppToHireRefNo.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblGeneralInfoRight.AddCell(PosTypealcell2);
            }

            PosTypeChnk = new Chunk("Position type: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPositiontype.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Type Of Role: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeOfRole.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PdfPCell rightCell = new PdfPCell(tblGeneralInfoRight);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl.AddCell(rightCell);

            Paragraph phEmpty = new Paragraph(" ");
            pdfDoc.Add(headerTbl);

            PdfPTable headerTbl1 = new PdfPTable(2);
            headerTbl1.SetWidths(headerWidth);

            PdfPTable tblPositionDet = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblPositionDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Position Title: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatPositionTitle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Cost Centre: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatCostCentre.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Reports to: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatReportsto.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Effective Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(Convert.ToDateTime(lblExpatEffectiveDate.Text).ToString("dd/MM/yyyy"), ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Contract Period (Years): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatContractPeriod.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Contract End Date: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            if (string.IsNullOrEmpty(lblExpatContractEndDate.Text))
                PosTypekVal = new Chunk("", ddlFonts);
            else
                PosTypekVal = new Chunk(Convert.ToDateTime(lblExpatContractEndDate.Text).ToString("dd/MM/yyyy"), ddlFonts);

            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("New Salary Review: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatNextSalaryReview.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Home Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatHomeLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);


            PosTypeChnk = new Chunk("Who will sign the letter: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatWhowillsign.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblPositionDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Notes: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblPositionDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExpatNotes.Text, ddlFonts);
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
            header = new PdfPCell(new Phrase("Remuneration Details", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);

            PdfPTable tblRenumeration = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblRenumeration.SetWidths(tblGeneralInfoWidth);

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

            header = new PdfPCell(new Phrase("Personal Details", headFont));
            header.Border = 0;
            header.Colspan = 2;
            header.HorizontalAlignment = 0;
            tblRenumeration.AddCell(header);

            PosTypeChnk = new Chunk("Marital Status: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblRenumeration.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblMaritalStatus.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblRenumeration.AddCell(PosTypealcell2);


            header = new PdfPCell(new Phrase("Dependents:", ddlLabelFonts));

            header.Border = 0;
            header.Colspan = 2;
            header.HorizontalAlignment = 0;
            tblRenumeration.AddCell(header);



            PdfPCell renumerationCell = new PdfPCell(tblRenumeration);
            renumerationCell.Border = 0;
            tblJobDetailsDet.AddCell(renumerationCell);

            //Dependents Tabel
            string Dependent = string.Empty;
            string Name = string.Empty;
            string DOB = string.Empty;



            PdfPTable VehicleTable = new PdfPTable(3);
            float[] VehicleTableWidth = new float[] { 25f, 40f, 35f };
            VehicleTable.SetWidths(VehicleTableWidth);

            Chunk VehicleChnk = new Chunk(" Dependent  ", legcellFnt);
            Phrase VehiclePh1 = new Phrase(VehicleChnk);
            PdfPCell Vehiclecell = new PdfPCell(VehiclePh1);
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            ////Vehiclecell.FixedHeight = 10f;
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" Name ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            ////Vehiclecell.FixedHeight = 10f;
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            VehicleChnk = new Chunk(" DOB ", legcellFnt);
            VehiclePh1 = new Phrase(VehicleChnk);
            Vehiclecell = new PdfPCell(VehiclePh1);
            //Vehiclecell.FixedHeight = 10f;
            Vehiclecell.BackgroundColor = new iTextSharp.text.BaseColor(60, 69, 79);
            VehicleTable.AddCell(Vehiclecell);

            string strRefno = lblRefNo.Text;

            SPListItemCollection VehiclecollectionItems = GetListData("NewHirePersonnelDetails", strRefno);
            if (VehiclecollectionItems != null && VehiclecollectionItems.Count > 0)
            {
                foreach (SPListItem ListItems in VehiclecollectionItems)
                {
                    Dependent = Convert.ToString(ListItems["Dependent"]);
                    Name = Convert.ToString(ListItems["Name"]);
                    DOB = Convert.ToDateTime(ListItems["DOB"]).ToString("dd/MM/yyyy");

                    VehicleChnk = new Chunk(Dependent, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    //Vehiclecell.FixedHeight = 10f;
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(Name, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    //Vehiclecell.FixedHeight = 10f;
                    VehicleTable.AddCell(Vehiclecell);

                    VehicleChnk = new Chunk(DOB, legddlFonts);
                    VehiclePh1 = new Phrase(VehicleChnk);
                    Vehiclecell = new PdfPCell(VehiclePh1);
                    //Vehiclecell.FixedHeight = 10f;
                    VehicleTable.AddCell(Vehiclecell);


                }
            }

            PdfPCell VehicleTabelcell = new PdfPCell(VehicleTable);
            tblJobDetailsDet.AddCell(VehicleTabelcell);

            //Dependent ends


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

            //CheckList 
            Paragraph positionHead = new Paragraph("            Offer Checklist", headFont);
            pdfDoc.Add(positionHead);
            pdfDoc.Add(phEmpty);


            PdfPTable CheckBoxListTabel = new PdfPTable(2);
            float[] CheckBoxListTabelWidth = new float[] { 2.5f, 97.5f };
            CheckBoxListTabel.SetWidths(CheckBoxListTabelWidth);

            SPFile Chkedfile;
            SPFile NotChkedfile;
            Chkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_cross_small.jpg");
            byte[] imageBytes = Chkedfile.OpenBinary();
            iTextSharp.text.Image Chkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);

            NotChkedfile = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR%20Web/Images/checked_blank_small.jpg");
            imageBytes = NotChkedfile.OpenBinary();
            iTextSharp.text.Image NotChkedlogo = iTextSharp.text.Image.GetInstance(imageBytes);

            SPListItemCollection ChkListcollectionItems = GetListData("NewHireOfferChecklist", strRefno);
            if (ChkListcollectionItems != null && ChkListcollectionItems.Count > 0)
            {
                foreach (SPListItem ListItems in ChkListcollectionItems)
                {

                    if (Convert.ToString(ListItems["Immigration"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Immigration Requirements completed", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["ReferenceCheck"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Reference Checks", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["Resume"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Resume/Application Form", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["InterviewNotes"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Interview Notes", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    if (Convert.ToString(ListItems["PsychometricTesting"]) == "Yes")
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = Chkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }
                    else
                    {
                        PdfPCell othersecondrowCell = new PdfPCell();
                        othersecondrowCell.Border = 0;
                        othersecondrowCell.Image = NotChkedlogo;
                        othersecondrowCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        othersecondrowCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                        othersecondrowCell = new PdfPCell(new Phrase("Psychometric Testing", ddlFonts));
                        othersecondrowCell.Border = 0;
                        CheckBoxListTabel.AddCell(othersecondrowCell);
                    }

                }
            }
            pdfDoc.Add(CheckBoxListTabel);







            PdfPTable pdfAppHistory = new PdfPTable(3);
            PosTypeChnk = new Chunk(" Date ", cellFnt);
            PosTypePh1 = new Phrase(PosTypeChnk);
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
                    PosTypevalcell = new PdfPCell(PosTypePh1);
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

            positionHead = new Paragraph("            Approval History", headFont);
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


                Chunk chunk = new Chunk("New Hire Request", hFonts);
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

        private void GetOfferChecklist()
        {
            string strRefno = lblRefNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireOfferChecklist", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                string Immigration = Convert.ToString(ListItems["Immigration"]);
                string ReferenceCheck = Convert.ToString(ListItems["ReferenceCheck"]);
                string Resume = Convert.ToString(ListItems["Resume"]);
                string InterviewNotes = Convert.ToString(ListItems["InterviewNotes"]);
                string PsychometricTesting = Convert.ToString(ListItems["PsychometricTesting"]);

                if (Immigration == "Yes")
                    chkbxLstExpat.Items[0].Selected = true;
                if (ReferenceCheck == "Yes")
                    chkbxLstExpat.Items[1].Selected = true;
                if (Resume == "Yes")
                    chkbxLstExpat.Items[2].Selected = true;
                if (InterviewNotes == "Yes")
                    chkbxLstExpat.Items[3].Selected = true;
                if (PsychometricTesting == "Yes")
                    chkbxLstExpat.Items[4].Selected = true;
            }
        }

        private void UpdateOfferChecklists()
        {

            string lstURL = HrWebUtility.GetListUrl("NewHireOfferChecklist");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oQuery = new SPQuery();
               string strAppHireRefNo = lblRefNo.Text;
               oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


               SPListItemCollection oItems = oList.GetItems(oQuery);
               SPListItem item = null;
               if (oItems != null && oItems.Count > 0)
               {
                   item = oItems[0];
               }
               else
               {
                   item = oList.AddItem();
               }
               item["Title"] = strAppHireRefNo;
               if (chkbxLstExpat.Items[0].Selected)
                   item["Immigration"] = "Yes";
               else
                   item["Immigration"] = "No";
               if (chkbxLstExpat.Items[1].Selected)
                   item["ReferenceCheck"] = "Yes";
               else
                   item["ReferenceCheck"] = "No";
               if (chkbxLstExpat.Items[2].Selected)
                   item["Resume"] = "Yes";
               else
                   item["Resume"] = "No";
               if (chkbxLstExpat.Items[3].Selected)
                   item["InterviewNotes"] = "Yes";
               else
                   item["InterviewNotes"] = "No";
               if (chkbxLstExpat.Items[4].Selected)
                   item["PsychometricTesting"] = "Yes";
               else
                   item["PsychometricTesting"] = "No";

               item.Update();

           });
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
    }
}
