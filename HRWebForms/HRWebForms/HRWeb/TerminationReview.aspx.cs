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
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace HRWebForms.HRWeb
{
    public partial class TerminationReview : WebPartPage
    {
        string UserName = string.Empty;
        string strRefno = "";

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

                lblTerminationRequest.Text = "";
                string strRefno = "";
                if (Page.Request.QueryString["refno"] != null)
                {
                    strRefno = Page.Request.QueryString["refno"];
                    ViewState["RefNo"] = strRefno;
                    lblReferenceNo.Text = "Ref No: " + Page.Request.QueryString["refno"];
                }

                if (!IsPostBack)
                {
                    bool bValid = false;
                    //dvNotification.Visible = false;
                    dvProcurement.Visible = false;
                    dvMarketing.Visible = false;
                    dvFinance.Visible = false;
                    dvHRServices.Visible = false;
                    dvCreditCard.Visible = false;
                    dvInformationTechonology.Visible = false;
                    dvSiteAdmin.Visible = false;
                    btnPDF.Visible = false;
                    dvdrpHRServices.Visible = false;
                    dvdrpHRServices.Attributes.Add("display", "none");
                    divlblHRServices.Visible = false;
                    divlblHRServices.Attributes.Add("display", "none");
                    btnAck.Visible = false;
                    dvTerminationMeeting.Visible = false;

                    if (strRefno != "")
                    {

                        bValid = ValidateApplication();
                        if (bValid)
                        {
                            GetTerminationGeneralInfo();
                            GetNotification();
                            GetTypeOfLeave();
                            GetCreditCard();
                            GetMarketing();
                            GetProcurement();
                            GetFinance();
                            GetISChecklist();
                            GetSiteAdmin();
                            GetSiteHRServices();
                            GetHRMeeting();
                            GetJobDetails(strRefno);

                            bool ISHRManager = IsHRManager();
                            if (ISHRManager)
                            {
                                if (lblMeetingComments.Text == "")
                                {
                                    dvProcurement.Visible = false;
                                    dvMarketing.Visible = false;
                                    dvFinance.Visible = false;
                                    dvCreditCard.Visible = false;
                                    dvSiteAdmin.Visible = false;
                                    dvTerminationMeeting.Visible = false;
                                    dvInformationTechonology.Visible = false;
                                }
                                else
                                {

                                    dvProcurement.Visible = true;
                                    dvMarketing.Visible = true;
                                    dvFinance.Visible = true;

                                    dvCreditCard.Visible = true;
                                    dvSiteAdmin.Visible = true;
                                    dvTerminationMeeting.Visible = true;
                                    dvInformationTechonology.Visible = true;
                                }
                            }

                            bool IsHRServiceUser = IsUserMemberOfGroup("HR Services");
                            if (IsHRServiceUser)
                            {
                                if (lblMeetingComments.Text == "")
                                {
                                    dvProcurement.Visible = false;
                                    dvMarketing.Visible = false;
                                    dvFinance.Visible = false;

                                    dvHRServices.Visible = false;
                                    dvHRServices.Attributes.Add("display", "none");

                                    dvCreditCard.Visible = false;
                                    dvSiteAdmin.Visible = false;
                                    dvInformationTechonology.Visible = false;
                                    btnPDF.Visible = false;
                                }
                                else
                                {

                                    dvProcurement.Visible = true;
                                    dvMarketing.Visible = true;
                                    dvFinance.Visible = true;

                                    dvHRServices.Visible = true;
                                    dvHRServices.Attributes.Add("display", "");

                                    dvCreditCard.Visible = true;
                                    dvSiteAdmin.Visible = true;
                                    dvInformationTechonology.Visible = true;
                                    btnPDF.Visible = true;

                                    dvTerminationMeeting.Visible = true;
                                }

                            }

                            bool bOtherApprover = false;


                            bool IsCCUser = IsUserMemberOfGroup("Credit Card");
                            if (IsCCUser)
                            {

                                dvCreditCard.Visible = true;
                                //dvTerminationMeeting.Visible = false;
                                if (!CCAckStatusApproved())
                                {
                                    btnAck.Visible = true;
                                    bOtherApprover = true;
                                }
                                //else
                                //btnAck.Visible = false;

                            }

                            bool IsProcurementUser = IsUserMemberOfGroup("Procurement");
                            if (IsProcurementUser)
                            {
                                dvProcurement.Visible = true;
                                //dvTerminationMeeting.Visible = false;
                                if (!ProcurementAckStatusApproved())
                                {
                                    btnAck.Visible = true;
                                    bOtherApprover = true;
                                }
                                //else
                                //btnAck.Visible = false;
                            }

                            bool IsFinanceUser = IsUserMemberOfGroup("Finance");
                            if (IsFinanceUser)
                            {
                                dvFinance.Visible = true;
                                //dvTerminationMeeting.Visible = false;
                                if (!FinanceAckStatusApproved())
                                {
                                    btnAck.Visible = true;
                                    bOtherApprover = true;
                                }
                                //else
                                // btnAck.Visible = false;
                            }

                            bool IsMarketingUser = IsUserMemberOfGroup("Marketing");
                            if (IsMarketingUser)
                            {
                                dvMarketing.Visible = true;
                                //dvTerminationMeeting.Visible = false;
                                if (!MarketingAckStatusApproved())
                                {
                                    btnAck.Visible = true;
                                    bOtherApprover = true;
                                }
                                //else
                                // btnAck.Visible = false;
                            }

                            bool IsISUser = IsUserMemberOfGroup("IS Group");
                            if (IsISUser)
                            {
                                dvInformationTechonology.Visible = true;
                                //dvTerminationMeeting.Visible = false;
                                if (!ISAckStatusApproved())
                                {
                                    btnAck.Visible = true;
                                    bOtherApprover = true;
                                }
                                if (lblMeetingComments.Text == "")
                                {
                                    dvProcurement.Visible = false;
                                    dvMarketing.Visible = false;
                                    dvFinance.Visible = false;
                                    dvCreditCard.Visible = false;
                                    dvSiteAdmin.Visible = false;
                                    dvInformationTechonology.Visible = false;
                                }

                                //else
                                //btnAck.Visible = false;
                            }

                            bool IsSiteAdmin = IsUserMemberOfGroup("Site Administration");
                            if (IsSiteAdmin)
                            {
                                dvSiteAdmin.Visible = true;
                                //dvTerminationMeeting.Visible = false;

                                if (!bOtherApprover)
                                    btnAck.Visible = false;

                                /* if (!SiteAdminAckStatusApproved())
                                     btnAck.Visible = true;
                                 else
                                     btnAck.Visible = false;*/
                            }



                            if (IsHRServiceUser)
                            {
                                if (lblMeetingComments.Text == "")
                                {
                                    dvTerminationMeeting.Visible = false;
                                    btnAck.Visible = false;
                                }
                                else
                                {
                                    dvTerminationMeeting.Visible = true;
                                    btnAck.Visible = true;
                                }

                                if (HRServiceAckStatusApproved())
                                {
                                    divlblHRServices.Visible = true;
                                    divlblHRServices.Attributes.Add("display", "");
                                    btnAck.Visible = false;
                                }
                                else
                                {
                                    dvdrpHRServices.Visible = true;
                                    dvdrpHRServices.Attributes.Add("display", "");
                                }

                            }

                            if (!bOtherApprover)
                            {
                                // If approval is pending with HR Services, then display text of the button as "HRS Acknowledge"
                                btnAck.Text = "HRS Acknowledge";
                            }
                            else
                            {
                                // If approval is pending with other dept than HR Services, then display text of the button as "Acknowledge"
                                btnAck.Text = "Acknowledge";
                            }
                            /* else
                             {
                                 SPUtility.HandleAccessDenied(new Exception("You don’t have access rights to see this content"));
                             }*/
                        }
                        else
                        {
                            SPUtility.HandleAccessDenied(new Exception("You don’t have access rights to see this content"));
                            //lblTerminationRequest.Text = "The application number passed does not exist or has already been submitted.";
                        }
                    }
                    else
                    {
                        lblTerminationRequest.Text = "Please pass the reference number.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.Page_Load", ex.Message);
                lblTerminationRequest.Text = ex.Message;
                //lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }

        }

        private bool IsInitiator()
        {
            bool result = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);

                SPQuery oquery = new SPQuery();

                oquery.Query = "<Where><And><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" +
                    strRefno + "</Value></Eq><Eq><FieldRef Name=\'Author\'/><Value Type=\"Text\">" + UserName +
                    "</Value></Eq></And></Where>";
                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                    result = true;
            });
            return result;
        }

        private bool IsHRManager()
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
                        if (string.Equals(txfBusinessUnit.Label, lblBusinessUnit.Text, StringComparison.OrdinalIgnoreCase))
                            result = true;
                    }
                }
            });
            return result;
        }

        private bool IsHRManager(string bunit)
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

                // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                oquery.Query = "<Where><And><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"Text\">" + UserName +
                    "</Value></Eq><Eq><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" + bunit +
                    "</Value></Eq></And></Where>";
                SPListItemCollection collitems = olist1.GetItems(oquery);
                if (collitems != null && collitems.Count > 0)
                {
                    result = true;
                }
            });
            return result;
        }

        private bool HRServiceAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["Status"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool CCAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["CreditCardAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool ProcurementAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["ProcurementAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool FinanceAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["FinanceAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool ISAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["ISAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool MarketingAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["MarketingAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool SiteAdminAckStatusApproved()
        {
            bool result = false;

            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                SPListItemCollection collitems = olist.GetItems(oquery);
                foreach (SPListItem listitem in collitems)
                {
                    string strStatus = Convert.ToString(listitem["SiteAdminAckStatus"]);
                    if (string.Equals(strStatus, "Approved", StringComparison.OrdinalIgnoreCase) || strStatus == "")
                    {
                        result = true;
                    }
                }
            });

            return result;
        }

        private bool ValidateApplication()
        {
            bool bValid = false;
            string bunit = string.Empty;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   bunit = Convert.ToString(listitem["BusinessUnit"]);

               }
               if (IsUserMemberOfGroup("HR Services") || IsUserMemberOfGroup("Credit Card") || IsUserMemberOfGroup("Procurement") ||
                   IsUserMemberOfGroup("Finance") || IsUserMemberOfGroup("Marketing") || IsUserMemberOfGroup("IS Group") ||
                   IsUserMemberOfGroup("Site Administration") || IsInitiator() || IsHRManager(bunit))
               {
                   bValid = true;
               }
           });
            return bValid;
        }

        private string GetUserType()
        {
            string strUserType = "";

            string lstURL1 = HrWebUtility.GetListUrl("HrWebHrBusinessUnitApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'HrManager\'/><Value Type=\"Text\">" + UserName + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist1.GetItems(oquery);
               if (collitems != null && collitems.Count > 0)
               {
                   strUserType = "HrManager";
               }
               else
               {
                   //HR Services
                   using (SPWeb web = SPControl.GetContextWeb(this.Context))
                   {
                       SPUser user = web.CurrentUser;

                       if (user.Groups.Cast<SPGroup>().Any(g => g.Name.Equals("HR Services")))
                       {
                           strUserType = "HrServices";
                       }
                   }
                   if (string.Equals(strUserType, "HrServices", StringComparison.OrdinalIgnoreCase))
                   {
                       //return strUserType;
                   }

                   //Business Checklist Approver By Individual
                   string otherApprovalInfoURL = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
                   SPList otherApprovalInfoList = SPContext.Current.Site.RootWeb.GetList(otherApprovalInfoURL);
                   SPQuery otherApprovalInfoListQuery = new SPQuery();
                   otherApprovalInfoListQuery.Query = "<Where><And><Eq><FieldRef Name=\'ApproverType\'/><Value Type=\"Text\">Individual</Value></Eq><Eq><FieldRef Name=\'Approver\'/><Value Type=\"Text\">" + UserName + "</Value></Eq></And></Where>";

                   SPListItemCollection otherApprovalInfoListCollitems = otherApprovalInfoList.GetItems(otherApprovalInfoListQuery);
                   if (otherApprovalInfoListCollitems != null && otherApprovalInfoListCollitems.Count > 0)
                   {

                       if (string.Equals(Convert.ToString(otherApprovalInfoListCollitems[0]["BusinessType"]), "CreditCard", StringComparison.OrdinalIgnoreCase))
                           strUserType = "CreditCardApprover";
                       else if (string.Equals(Convert.ToString(otherApprovalInfoListCollitems[0]["BusinessType"]), "Procurement", StringComparison.OrdinalIgnoreCase))
                           strUserType = "ProcurementApprover";
                       else if (string.Equals(Convert.ToString(otherApprovalInfoListCollitems[0]["BusinessType"]), "Finance", StringComparison.OrdinalIgnoreCase))
                           strUserType = "FinanceApprover";
                       else if (string.Equals(Convert.ToString(otherApprovalInfoListCollitems[0]["BusinessType"]), "Marketing", StringComparison.OrdinalIgnoreCase))
                           strUserType = "MarketingApprover";
                   }

                   //Business Checklist Approver By Group
                   otherApprovalInfoListQuery.Query = "<Where><Eq><FieldRef Name=\'ApproverType\'/><Value Type=\"Text\">SPGroup</Value></Eq></Where>";
                   otherApprovalInfoListCollitems = otherApprovalInfoList.GetItems(otherApprovalInfoListQuery);
                   if (otherApprovalInfoListCollitems != null && otherApprovalInfoListCollitems.Count > 0)
                   {
                       foreach (SPListItem Itm in otherApprovalInfoListCollitems)
                       {
                           string strApproverGroup = Convert.ToString(Itm["Approver"]);
                           using (SPWeb web = SPControl.GetContextWeb(this.Context))
                           {
                               SPUser user = web.CurrentUser;
                               if (user.Groups[strApproverGroup] != null)
                               {
                                   if (string.Equals(Convert.ToString(Itm["BusinessType"]), "CreditCard", StringComparison.OrdinalIgnoreCase))
                                       strUserType = "CreditCardApprover";
                                   else if (string.Equals(Convert.ToString(Itm["BusinessType"]), "Procurement", StringComparison.OrdinalIgnoreCase))
                                       strUserType = "ProcurementApprover";
                                   else if (string.Equals(Convert.ToString(Itm["BusinessType"]), "Finance", StringComparison.OrdinalIgnoreCase))
                                       strUserType = "FinanceApprover";
                                   else if (string.Equals(Convert.ToString(Itm["BusinessType"]), "Marketing", StringComparison.OrdinalIgnoreCase))
                                       strUserType = "MarketingApprover";
                               }
                           }
                       }

                   }

                   //Initiator
                   SPListItemCollection collectionItems = null;
                   if (strRefno != "")
                       collectionItems = SetListData("HrWebTerminationGeneralInfo", strRefno);
                   if (collectionItems != null && collectionItems.Count > 0)
                   {
                       foreach (SPListItem listitem in collectionItems)
                       {

                           string strUser = HrWebUtility.GetUser(Convert.ToString(listitem["Author"]));
                           UserName = HrWebUtility.GetUserByEmailID(UserName);
                           if (string.Equals(strUser, UserName, StringComparison.OrdinalIgnoreCase))
                           {
                               strUserType = "Initiator";
                           }
                       }
                   }
               }
           });
            return strUserType;
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

        private bool CheckIfSiteAdmin(string username)
        {
            bool bValid = false;
            string lstURL1 = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL1);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><And><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">SiteAdmin</Value>" +
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

        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
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

        private void GetNotification()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationNotification");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {

                   lblEmployeeName.Text = Convert.ToString(listitem["EmployeeName"]);
                   lblEmployeeNum.Text = Convert.ToString(listitem["EmployeeNumber"]);
                   lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                   if (string.Equals(Convert.ToString(listitem["IsMobilePurchaseRequired"]), "True"))
                       lblMobilePhone.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsMobilePurchaseRequired"]), "False"))
                       lblMobilePhone.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsImmigrationVisa"]), "True"))
                       lblImmigrationVisa.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsImmigrationVisa"]), "False"))
                       lblImmigrationVisa.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsNovatedLease"]), "True"))
                       lblInnovatedLease.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsNovatedLease"]), "False"))
                       lblInnovatedLease.Text = "No";
                   /* TaxonomyFieldValue txfBusinessUnit = listitem["BusinessUnit"] as TaxonomyFieldValue;
                    if (!string.IsNullOrEmpty(txfBusinessUnit.Label))
                        lblBusinessUnit.Text = txfBusinessUnit.Label;*/

                   lblWorkArea.Text = Convert.ToString(listitem["WorkArea"]);
                   lblSiteLocation.Text = Convert.ToString(listitem["SiteLocation"]);

                   if (listitem["LastDayAtWork"] != null)
                       lblLastDayWork.Text = Convert.ToDateTime(listitem["LastDayAtWork"]).ToString("dd/MM/yyyy");

                   if (listitem["PeriodOfServiceFrom"] != null)
                       lblPeriodOfService.Text = Convert.ToDateTime(listitem["PeriodOfServiceFrom"]).ToString("dd/MM/yyyy");

                   if (listitem["PeriodOfServiceTo"] != null)
                       lblPeriodOfServiceEndDate.Text = Convert.ToDateTime(listitem["PeriodOfServiceTo"]).ToString("dd/MM/yyyy");

                   if (listitem["Notes"] != null)
                       lblNotifyComments.Text = Convert.ToString(listitem["Notes"]);

                   /*   if (listitem["CreditCardAckDate"] != null)
                          lblCreditCardAckDate.Text = Convert.ToDateTime(listitem["CreditCardAckDate"]).ToString("dd/MM/yyyy");

                      if (listitem["CreditCardAckBy"] != null)
                          lblCreditCardAckName.Text = Convert.ToString(listitem["CreditCardAckBy"]);

                      if (listitem["ProcurementAckDate"] != null)
                          lblProcurementAckDate.Text = Convert.ToDateTime(listitem["ProcurementAckDate"]).ToString("dd/MM/yyyy");

                      if (listitem["ProcurementAckBy"] != null)
                          lblProcurementAckName.Text = Convert.ToString(listitem["ProcurementAckBy"]);

                      if (listitem["FinanceAckDate"] != null)
                          lblFinanceAckDate.Text = Convert.ToDateTime(listitem["FinanceAckDate"]).ToString("dd/MM/yyyy");

                      if (listitem["FinanceAckBy"] != null)
                          lblFinanceAckName.Text = Convert.ToString(listitem["FinanceAckBy"]);



                      /*TaxonomyFieldValue txfWorkArea = listitem["WorkArea"] as TaxonomyFieldValue;
                      if (!string.IsNullOrEmpty(txfWorkArea.Label))
                          drpdwnWorkArea.SelectedValue = txfWorkArea.TermGuid;*/





                   /*TaxonomyFieldValue txfSiteLocation = listitem["SiteLocation"] as TaxonomyFieldValue;
                   if (!string.IsNullOrEmpty(txfSiteLocation.Label))
                       drpdwnSiteLocation.SelectedValue = txfSiteLocation.TermGuid;*/


                   //object obj = listitem["BusinessUnit"];


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

        private void GetTerminationGeneralInfo()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
               oquery.ViewFields = string.Concat(
                                "<FieldRef Name='DateOfRequest' />",
                                "<FieldRef Name='PositionType' />",
                                "<FieldRef Name='HRServiceAckDate' />",
                                "<FieldRef Name='CreditCardAckDate' />",
                                "<FieldRef Name='FinanceAckDate' />",
                                "<FieldRef Name='ProcurementAckDate' />",
                                "<FieldRef Name='MarketingAckDate' />",
                                "<FieldRef Name='ISAckDate' />",
                                "<FieldRef Name='HRServiceAckBy' />",
                                "<FieldRef Name='CreditCardAckBy' />",
                                "<FieldRef Name='FinanceAckBy' />",
                                "<FieldRef Name='ProcurementAckBy' />",
                                "<FieldRef Name='MarketingAckBy' />",
                                "<FieldRef Name='ISAckBy' />",
                                "<FieldRef Name='Author' />");

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (listitem["DateOfRequest"] != null)
                       lblDateOfRequest.Text = Convert.ToDateTime(listitem["DateOfRequest"]).ToString("dd/MM/yyyy");

                   lblInitiator.Text = HrWebUtility.GetUser(Convert.ToString(listitem["Author"]));

                   lblPositionType.Text = Convert.ToString(listitem["PositionType"]);
                   /* TaxonomyFieldValue txfBusinessUnit = listitem["PositionType"] as TaxonomyFieldValue;
                    if (txfBusinessUnit.Label != null)
                        lblPositionType.Text = txfBusinessUnit.Label;*/

                   // if (string.Equals(strApproveType, "HrServices", StringComparison.OrdinalIgnoreCase))
                   // {

                   if (listitem["HRServiceAckDate"] != null)
                       lblHRServiceAckDate.Text = Convert.ToDateTime(listitem["HRServiceAckDate"]).ToString("dd/MM/yyyy");

                   lblHRServiceAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["HRServiceAckBy"]));

                   if (listitem["CreditCardAckDate"] != null)
                       lblCreditCardAckDate.Text = Convert.ToDateTime(listitem["CreditCardAckDate"]).ToString("dd/MM/yyyy");

                   lblCreditCardAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["CreditCardAckBy"]));

                   if (listitem["FinanceAckDate"] != null)
                       lblFinanceAckDate.Text = Convert.ToDateTime(listitem["FinanceAckDate"]).ToString("dd/MM/yyyy");

                   lblFinanceAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["FinanceAckBy"]));

                   if (listitem["ProcurementAckDate"] != null)
                       lblProcurementAckDate.Text = Convert.ToDateTime(listitem["ProcurementAckDate"]).ToString("dd/MM/yyyy");

                   lblProcurementAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["ProcurementAckBy"]));

                   if (listitem["MarketingAckDate"] != null)
                       lblMarketingAckDate.Text = Convert.ToDateTime(listitem["MarketingAckDate"]).ToString("dd/MM/yyyy");

                   lblMarketingAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["MarketingAckBy"]));


                   if (listitem["ISAckDate"] != null)
                       lblInfoAckDate.Text = Convert.ToDateTime(listitem["ISAckDate"]).ToString("dd/MM/yyyy");

                   lblInfoAckName.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["ISAckBy"]));


                   /*if (listitem["SiteAdminAckDate"] != null)
                       lblSiteAdminAckDate.Text = Convert.ToDateTime(listitem["SiteAdminAckDate"]).ToString("dd/MM/yyyy");

                   lblSiteAdminAckBy.Text = HrWebUtility.GetUserByEmailID(Convert.ToString(listitem["SiteAdminAckBy"]));*/

                   //}

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

                   /* if (string.Equals(Convert.ToString(listitem["IsProcessFinalPayment"]), "True"))
                        lblProcessFinalPay.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsProcessFinalPayment"]), "False"))
                        lblProcessFinalPay.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsTerminateSAPSystem"]), "True"))
                        lblTerminatSAP.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsTerminateSAPSystem"]), "False"))
                        lblTerminatSAP.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsKronosRemoved"]), "True"))
                        lblKronosAccess.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsKronosRemoved"]), "False"))
                        lblKronosAccess.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsTerminationPayed"]), "True"))
                        lblTerminationPay.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsTerminationPayed"]), "False"))
                        lblTerminationPay.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsDelimitMonitored"]), "True"))
                        lblDelimitDate.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsDelimitMonitored"]), "False"))
                        lblDelimitDate.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsPersonnelFileRemoved"]), "True"))
                        lblRemovePersonnel.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsPersonnelFileRemoved"]), "False"))
                        lblRemovePersonnel.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsHousingVehicleDeclared"]), "True"))
                        lblHousingSubsidy.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsHousingVehicleDeclared"]), "False"))
                        lblHousingSubsidy.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsVisaNotified"]), "True"))
                        lblVisaNotification.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsVisaNotified"]), "False"))
                        lblVisaNotification.Text = "No";*/

               }
           });
        }

        private void GetCreditCard()
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
                       lblCancelCredit.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsCancelCardAdvised"]), "False"))
                       lblCancelCredit.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsFinalClaimFormRecieved"]), "True"))
                       lblReceiptsReceived.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsFinalClaimFormRecieved"]), "False"))
                       lblReceiptsReceived.Text = "No";
               }
               if (lblCancelCredit.Text.ToLower() == "no" && lblReceiptsReceived.Text.ToLower() == "no")
               {
                   CreditAckDiv.Style["display"] = "none";
               }
           });
        }

        private void GetMarketing()
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

                   if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "True"))
                       lblRemoveEmployee.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "False"))
                       lblRemoveEmployee.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsPhotosRemoved"]), "True"))
                       lblRemovePhotos.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsPhotosRemoved"]), "False"))
                       lblRemovePhotos.Text = "No";
               }
               if (lblRemoveEmployee.Text.ToLower() == "no" && lblRemovePhotos.Text.ToLower() == "no")
               {
                   MarketingAckDiv.Style["display"] = "none";
               }
           });
        }

        private void GetProcurement()
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
                   if (string.Equals(Convert.ToString(listitem["IsCompanyVehicleReturned"]), "True"))
                       lblCompanyVehicle.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsCompanyVehicleReturned"]), "False"))
                       lblCompanyVehicle.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsVehicleKeysSet"]), "True"))
                       lblVehicleKeys.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsVehicleKeysSet"]), "False"))
                       lblVehicleKeys.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsFuelCard"]), "True"))
                       lblFuelCard.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsFuelCard"]), "False"))
                       lblFuelCard.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsVehicleConditionCompleted"]), "True"))
                       lblVehicleCondition.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsVehicleConditionCompleted"]), "False"))
                       lblVehicleCondition.Text = "No";
               }
               if (lblCompanyVehicle.Text.ToLower() == "no" && lblVehicleKeys.Text.ToLower() == "no" && lblFuelCard.Text.ToLower() == "no" &&
                lblVehicleCondition.Text.ToLower() == "no")
               {
                   ProcurementAckDiv.Style["display"] = "none";
               }
           });
        }

        private void GetFinance()
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

                   if (string.Equals(Convert.ToString(listitem["IsChequeSignatory"]), "True"))
                       lblChequeSignatory.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsChequeSignatory"]), "False"))
                       lblChequeSignatory.Text = "No";
               }
               if (lblChequeSignatory.Text.ToLower() == "no")
               {
                   FinanceAckDiv.Style["display"] = "none";
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
                       lblISLeeton.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsEquipmentsInLeeton"]), "False"))
                       lblISLeeton.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsComputerAccessRemoved"]), "True"))
                       lblRemoveAccess.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsComputerAccessRemoved"]), "False"))
                       lblRemoveAccess.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsMobileRecharged"]), "True"))
                       lblMobileCharger.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsMobileRecharged"]), "False"))
                       lblMobileCharger.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsMobilePurchased"]), "True"))
                       lblMobilePurchased.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsMobilePurchased"]), "False"))
                       lblMobilePurchased.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsElectronicEquipment"]), "True"))
                       lblElectronic.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsElectronicEquipment"]), "False"))
                       lblElectronic.Text = "No";


                   if (string.Equals(Convert.ToString(listitem["IsLaptopCollected"]), "True"))
                       lblLaptopCollected.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsLaptopCollected"]), "False"))
                       lblLaptopCollected.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsVoicemailChanged"]), "True"))
                       lblDisableVoicemail.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsVoicemailChanged"]), "False"))
                       lblDisableVoicemail.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "True"))
                       lblRemoveContacts.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsEmployeeRemoved"]), "False"))
                       lblRemoveContacts.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsAutomaticEmailSet"]), "True"))
                       lblAutomaticEmail.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsAutomaticEmailSet"]), "False"))
                       lblAutomaticEmail.Text = "No";

               }
           });
        }

        private void GetSiteAdmin()
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

                   if (string.Equals(Convert.ToString(listitem["IsSecurityCard"]), "True"))
                       lblSecurityCard.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsSecurityCard"]), "False"))
                       lblSecurityCard.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsOfficeKeys"]), "True"))
                       lblOfficeKeys.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsOfficeKeys"]), "False"))
                       lblOfficeKeys.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsLockerKeys"]), "True"))
                       lblLockerKey.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsLockerKeys"]), "False"))
                       lblLockerKey.Text = "No";

                   if (string.Equals(Convert.ToString(listitem["IsFOBPasses"]), "True"))
                       lblFobPasses.Text = "Yes";
                   else if (string.Equals(Convert.ToString(listitem["IsFOBPasses"]), "False"))
                       lblFobPasses.Text = "No";

               }
           });
        }

        private void GetSiteHRServices()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationHrServices");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();

                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";


                SPListItemCollection oItems = olist.GetItems(oQuery);



                if (oItems != null && oItems.Count > 0)
                {
                    foreach (SPListItem listitem in oItems)
                    {
                        if (string.Equals(Convert.ToString(listitem["IsProcessFinalPayment"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnFinalPayment.SelectedValue = "Yes";
                            lblFinalPayment.Text = "Yes";
                        }
                        else
                        {
                            drpdwnFinalPayment.SelectedValue = "No";
                            lblFinalPayment.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsTerminateSAPSystem"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnTerminateSAP.SelectedValue = "Yes";
                            lblPayrollSystem.Text = "Yes";
                        }
                        else
                        {
                            drpdwnTerminateSAP.SelectedValue = "No";
                            lblPayrollSystem.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsKronosRemoved"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnKronosRemoved.SelectedValue = "Yes";
                            lblKronosAccess.Text = "Yes";
                        }
                        else
                        {
                            drpdwnKronosRemoved.SelectedValue = "No";
                            lblKronosAccess.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsTerminationPayed"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnTerminationPay.SelectedValue = "Yes";
                            lblTerminationPay.Text = "Yes";
                        }
                        else
                        {
                            drpdwnTerminationPay.SelectedValue = "No";
                            lblTerminationPay.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsDelimitMonitored"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnDelimitDate.SelectedValue = "Yes";
                            lblDelimitDate.Text = "Yes";
                        }
                        else
                        {
                            drpdwnDelimitDate.SelectedValue = "No";
                            lblDelimitDate.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsPersonnelFileRemoved"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnRemoveFile.SelectedValue = "Yes";
                            lblRemovePersonal.Text = "Yes";
                        }
                        else
                        {
                            drpdwnRemoveFile.SelectedValue = "No";
                            lblRemovePersonal.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsHousingVehicleDeclared"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnHousing.SelectedValue = "Yes";
                            lblHousingSubsidy.Text = "Yes";
                        }
                        else
                        {
                            drpdwnHousing.SelectedValue = "No";
                            lblHousingSubsidy.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsVisaNotified"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            drpdwnVisaNotification.SelectedValue = "Yes";
                            lblVisaNotify.Text = "Yes";
                        }
                        else
                        {
                            drpdwnVisaNotification.SelectedValue = "No";
                            lblVisaNotify.Text = "No";
                        }


                    }
                }

            });

        }

        private void GetTypeOfLeave()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationLeave");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();

                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";


                SPListItemCollection oItems = olist.GetItems(oQuery);



                if (oItems != null && oItems.Count > 0)
                {
                    foreach (SPListItem listitem in oItems)
                    {
                        if (string.Equals(Convert.ToString(listitem["IsParentalLeave"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            lblParentalLeave.Text = "Yes";
                        }
                        else
                        {
                            lblParentalLeave.Text = "No";
                        }

                        if (string.Equals(Convert.ToString(listitem["IsLeaveWithoutPay"]), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            lblLeaveWithoutPay.Text = "Yes";
                        }
                        else
                        {
                            lblLeaveWithoutPay.Text = "No";
                        }

                        if (listitem["PeriodOfLeaveFrom"] != null)
                            lblPeriodOfLeave.Text = Convert.ToDateTime(listitem["PeriodOfLeaveFrom"]).ToString("dd/MM/yyyy");

                        if (listitem["PeriodOfLeaveTo"] != null)
                            lblPeriodOfLeaveEndDate.Text = Convert.ToDateTime(listitem["PeriodOfLeaveTo"]).ToString("dd/MM/yyyy");

                        lblTypeOfLeaveComments.Text = Convert.ToString(listitem["Comments"]);

                    }
                }

            });

        }

        private void GetHRMeeting()
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
                        lblExitInterview.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsExitInterview"]), "False"))
                        lblExitInterview.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsCompanyPropertyCollected"]), "True"))
                        lblPropertyCollected.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsCompanyPropertyCollected"]), "False"))
                        lblPropertyCollected.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsAgreementReiterate"]), "True"))
                        lblReiterateAgree.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsAgreementReiterate"]), "False"))
                        lblReiterateAgree.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsEmployeeContactsNotified"]), "True"))
                        lblNotifyContacts.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsEmployeeContactsNotified"]), "False"))
                        lblNotifyContacts.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsEmployeeAddressConfirmed"]), "True"))
                        lblConfirmEmployee.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsEmployeeAddressConfirmed"]), "False"))
                        lblConfirmEmployee.Text = "No";

                    if (string.Equals(Convert.ToString(listitem["IsServiceRequestCertificate"]), "True"))
                        lblCertificateService.Text = "Yes";
                    else if (string.Equals(Convert.ToString(listitem["IsServiceRequestCertificate"]), "False"))
                        lblCertificateService.Text = "No";

                    lblMeetingComments.Text = Convert.ToString(listitem["Comments"]);

                }
            });
        }

        private bool ApproverCreditCardAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["CreditCardAckStatus"]) == "Pending")
                   {
                       listitem["CreditCardAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["CreditCardAckBy"] = UserName;
                       listitem["CreditCardAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = true;
                   }
               }
           });
            return bProcessed;
        }

        private bool ApproverProcurementAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["ProcurementAckStatus"]) == "Pending")
                   {
                       listitem["ProcurementAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["ProcurementAckBy"] = UserName;
                       listitem["ProcurementAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = true;
                   }
               }
           });
            return bProcessed;
        }

        private bool ApproverMarketingAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["MarketingAckStatus"]) == "Pending")
                   {
                       listitem["MarketingAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["MarketingAckBy"] = UserName;
                       listitem["MarketingAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = true;
                   }
               }
           });
            return bProcessed;
        }

        private bool ApproverFinanceAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["FinanceAckStatus"]) == "Pending")
                   {
                       listitem["FinanceAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["FinanceAckBy"] = UserName;
                       listitem["FinanceAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = false;
                   }
               }
           });
            return bProcessed;
        }

        private bool ApproverISAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["ISAckStatus"]) == "Pending")
                   {
                       listitem["ISAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["ISAckBy"] = UserName;
                       listitem["ISAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = true;
                   }

               }
           });
            return bProcessed;
        }

        private bool ApproverSiteAdminAcknowledment()
        {
            bool bProcessed = false;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   if (Convert.ToString(listitem["SiteAdminAckStatus"]) == "Pending")
                   {
                       listitem["SiteAdminAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                       SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                       listitem["SiteAdminAckBy"] = UserName;
                       listitem["SiteAdminAckStatus"] = "Approved";
                       listitem.Update();
                       bProcessed = true;
                   }

               }
           });
            return bProcessed;
        }

        private void ApproverHRServicesAcknowledment()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();
            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);


               SPQuery oquery = new SPQuery();
               oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

               SPListItemCollection collitems = olist.GetItems(oquery);
               foreach (SPListItem listitem in collitems)
               {
                   listitem["HRServiceAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                   SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                   listitem["HRServiceAckBy"] = UserName;

                   listitem["HRServiceAckStatus"] = "Acknowledged";
                   listitem.Update();
               }
           });

        }

        protected void btnAck_Click(object sender, EventArgs e)
        {
            try
            {
                bool bProcessed = SetBCStatus();

                if (!bProcessed)
                {
                    bool IsHRServiceUser = IsUserMemberOfGroup("HR Services");
                    if (IsHRServiceUser)
                    {
                        if (ValidateBCStatus())
                        {
                            SetHRServiceStatus();
                        }
                        else
                        {
                            lblTerminationRequest.Text += "Please wait till all other approvers acknowledge";
                            GetJobDetails(strRefno);
                        }
                    }
                }
                else
                {
                    Response.Redirect("/people/Pages/HRWeb/TerminationWorkflowApproval.aspx");
                }

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.TerminationRequest.Page_Load", ex.Message);
                lblTerminationRequest.Text = ex.Message;
                //lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private bool SetBCStatus()
        {
            bool bCCProcessed = false;
            bool bProcProcessed = false;
            bool bMarkProcessed = false;
            bool bFinanceProcessed = false;
            bool bISProcessed = false;

            bool IsCCUser = IsUserMemberOfGroup("Credit Card");
            if (IsCCUser)
            {
                bCCProcessed = ApproverCreditCardAcknowledment();
            }

            bool IsProcurementUser = IsUserMemberOfGroup("Procurement");
            if (IsProcurementUser)
            {
                bProcProcessed = ApproverProcurementAcknowledment();
            }

            bool IsMarketingUser = IsUserMemberOfGroup("Marketing");
            if (IsMarketingUser)
            {
                bMarkProcessed = ApproverMarketingAcknowledment();
            }

            bool IsFinanceUser = IsUserMemberOfGroup("Finance");
            if (IsFinanceUser)
            {
                bFinanceProcessed = ApproverFinanceAcknowledment();
            }

            bool IsISUser = IsUserMemberOfGroup("IS Group");
            if (IsISUser)
            {
                bISProcessed = ApproverISAcknowledment();
            }

            bool IsAcknowledged = ValidateBCStatus();
            if (IsAcknowledged)
                SendEmail();

            return (bCCProcessed || bMarkProcessed || bProcProcessed || bISProcessed || bFinanceProcessed);
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
                oQuery.Query = "<Where><Eq><FieldRef Name=\'FormType\' /><Value Type=\"Text\">" + "Termination" +
            "</Value></Eq></Where>";
                oQuery.ViewFields = string.Concat(
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='EmailIP' />",
                                "<FieldRef Name='ApprovalSubject' />",
                                "<FieldRef Name='HRManagerApprovalMessage' />",
                                "<FieldRef Name='ApprovalMessage' />");
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


                    string to = string.Empty;
                    /*SPGroup group = web.Groups["HR Services"];
                    foreach (SPUser user in group.Users)
                    {
                        to += ";" + user.Email;
                    }*/
                    to += ";" + HrWebUtility.GetDistributionEmail("HR Services");
                    to = to.TrimStart(';');
                    strTo = to;


                    strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "");
                    strMessage = Convert.ToString(itm["HRManagerApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;NAME&gt;", lblEmployeeName.Text.Trim()).
                            Replace("&lt;BU&gt;", lblBusinessUnit.Text.Trim()).
                            Replace("&lt;LOCATION&gt;", lblSiteLocation.Text.Trim()).
                            Replace("&lt;TERMINATIONDATE&gt;", lblLastDayWork.Text.Trim());
                    // MailMessage mailMessage = new MailMessage(strFrom, strTo, strSubject, strMessage);

                    MailMessage mailMessage = new MailMessage();
                    string[] mailto = strTo.Split(';');
                    var distinctIDs = mailto.Distinct();
                    foreach (string s in distinctIDs)
                    {
                        if (s.Trim() != "")
                            mailMessage.To.Add(s);
                    }
                    mailMessage.From = new MailAddress(strFrom, "HR Forms - SunConnect");
                    mailMessage.Subject = strSubject;
                    mailMessage.Body = strMessage;

                    mailMessage.IsBodyHtml = true;
                    smtpClient.Send(mailMessage);

                    SaveEmailDetails(strFrom, strTo, strSubject, strMessage);
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
                oItem["FormType"] = "Termination";
                oItem.Update();
                //}
            });
        }

        private void SetHRServiceStatus()
        {
            SetTerminationHrServices();
            SetTerminationGeneralInfoList();
            Response.Redirect("/people/Pages/HRWeb/TerminationReview.aspx?refno=" + strRefno);
        }

        private void SetTerminationHrServices()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationHrServices");
            SPSecurity.RunWithElevatedPrivileges(delegate()
           {
               SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);

               SPQuery oQuery = new SPQuery();

               oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";


               SPListItemCollection oItems = olist.GetItems(oQuery);

               SPListItem listitem = null;
               if (oItems != null && oItems.Count > 0)
               {
                   listitem = oItems[0];
               }
               else
               {
                   listitem = olist.AddItem();
               }

               UpdateTerminationHrServices(listitem);
           });

        }

        private void UpdateTerminationHrServices(SPListItem listitem)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                listitem["Title"] = strRefno;

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

        private void SetTerminationGeneralInfoList()
        {

            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            SPWeb mySite = SPContext.Current.Web;
            SPListItemCollection collectionItems = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if (strRefno != "")
                    collectionItems = SetListData("HrWebTerminationGeneralInfo", strRefno);
                if (collectionItems != null && collectionItems.Count > 0)
                {
                    foreach (SPListItem listitem in collectionItems)
                    {

                        listitem["Status"] = "Approved";
                        listitem["HRServiceAckDate"] = DateTime.Now.ToString("dd/MM/yyyy");
                        SPFieldUserValue UserName = new SPFieldUserValue(mySite, mySite.CurrentUser.ID, mySite.CurrentUser.LoginName);
                        listitem["HRServiceAckBy"] = UserName;

                        listitem.Update();

                    }
                }


            });
        }

        protected void btnPDF_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateTerminationPDF();
            }
            catch (Exception ex)
            {
                lblTerminationRequest.Text = "An unexpected error has occurred. Please contact administrator";
                LogUtility.LogError("HRWebForms.TerminationReview.btnPDF_Click", ex.Message);
            }
        }

        private void GenerateTerminationPDF()
        {

            strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string Refno = strRefno;
            string filename = "Termination_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".pdf";
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

            Chunk DateChnk = new Chunk("Date: ", ddlLabelFonts);
            Phrase ValPh1 = new Phrase(DateChnk);
            PdfPCell DateChnvalcell = new PdfPCell(ValPh1);
            DateChnvalcell.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell);

            Chunk DateChnkVal = new Chunk(lblDateOfRequest.Text, ddlFonts);
            Phrase ValPh2 = new Phrase(DateChnkVal);
            PdfPCell DateChnvalcell2 = new PdfPCell(ValPh2);
            DateChnvalcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(DateChnvalcell2);

            Chunk InitiatorChnk = new Chunk("Initiator: ", ddlLabelFonts);
            Phrase InitiatorPh = new Phrase(InitiatorChnk);
            PdfPCell Initiatorcell = new PdfPCell(InitiatorPh);
            Initiatorcell.Border = 0;
            tblGeneralInfoLeft.AddCell(Initiatorcell);

            Chunk InitiatorkVal = new Chunk(lblInitiator.Text, ddlFonts);
            Phrase InititatorKValPh2 = new Phrase(InitiatorkVal);
            PdfPCell Initiatoralcell2 = new PdfPCell(InititatorKValPh2);
            Initiatoralcell2.Border = 0;
            tblGeneralInfoLeft.AddCell(Initiatoralcell2);


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

            PdfPCell leftCell = new PdfPCell(tblGeneralInfoLeft);
            leftCell.Border = 0;
            leftCell.Padding = 0f;
            headerTbl.AddCell(leftCell);

            PdfPTable tblGeneralInfoRight = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblGeneralInfoRight.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Ref No:", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(Refno, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblGeneralInfoRight.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk("", ddlFonts);
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

            PdfPTable tblNotification = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblNotification.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Employee Name: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblEmployeeName.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Employee Number: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblEmployeeNum.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Business Unit: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblBusinessUnit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Work Area: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblWorkArea.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Site Location: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSiteLocation.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Is mobile phone/equipment purchase required: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblMobilePhone.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Does this employee hold an Immigration Visa: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblImmigrationVisa.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Does this employee have a novated lease: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblNotification.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblInnovatedLease.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblNotification.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblNotification);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader.SetWidths(tblGeneralInfoWidth);

            PdfPCell header = new PdfPCell(new Phrase("Notification of Termination", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader);
            //pdfDoc.Add(phEmpty);

            headerTbl1.AddCell(leftCell);

            PdfPTable tblJobDetailsDet = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblJobDetailsDet.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Last Day of Work: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblLastDayWork.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblJobDetailsDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Period of Service: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPeriodOfService.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblJobDetailsDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPeriodOfServiceEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblJobDetailsDet.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblJobDetailsDet.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblNotifyComments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblJobDetailsDet.AddCell(PosTypealcell2);

            rightCell = new PdfPCell(tblJobDetailsDet);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl1.AddCell(rightCell);

            pdfDoc.Add(headerTbl1);

            PdfPTable headerTbl9 = new PdfPTable(2);
            headerTbl9.SetWidths(headerWidth);

            PdfPTable tblTypeofLeave = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 40f, 60f };
            tblTypeofLeave.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Is this Parental Leave: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTypeofLeave.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblParentalLeave.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTypeofLeave.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Leave without Pay: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTypeofLeave.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblLeaveWithoutPay.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTypeofLeave.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Period of leave: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTypeofLeave.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPeriodOfLeave.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTypeofLeave.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTypeofLeave.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPeriodOfLeaveEndDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTypeofLeave.AddCell(PosTypealcell2);



            PosTypeChnk = new Chunk("Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTypeofLeave.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTypeOfLeaveComments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTypeofLeave.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblTypeofLeave);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader8 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 150f };
            pdfPHeader8.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Type of Leave", headFont));
            header.Border = 0;
            pdfPHeader8.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader8.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader8);
            //pdfDoc.Add(phEmpty);

            headerTbl9.AddCell(leftCell);

            PdfPTable tblTypeofLeaveAck = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblTypeofLeaveAck.SetWidths(tblGeneralInfoWidth);

            rightCell = new PdfPCell(tblTypeofLeaveAck);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl9.AddCell(rightCell);

            pdfDoc.Add(headerTbl9);



            PdfPTable headerTbl3 = new PdfPTable(2);
            headerTbl3.SetWidths(headerWidth);

            PdfPTable tblCreditCard = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblCreditCard.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Cancel Credit Card – advise Amex Administrator to  cancel card: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblCreditCard.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCancelCredit.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblCreditCard.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Have all receipts been received to submit  final Amex claim form: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblCreditCard.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReceiptsReceived.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblCreditCard.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblCreditCard);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader2 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader2.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Credit Card", headFont));
            header.Border = 0;
            pdfPHeader2.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader2.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader2);
            //pdfDoc.Add(phEmpty);

            headerTbl3.AddCell(leftCell);

            rightCell = new PdfPCell(new Phrase("", ddlFonts));
            if (CreditAckDiv.Style["display"] != "none")
            {
                PdfPTable tblCreditCardAck = new PdfPTable(2);
                tblGeneralInfoWidth = new float[] { 60f, 40f };
                tblCreditCardAck.SetWidths(tblGeneralInfoWidth);

                PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblCreditCardAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblCreditCardAckDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblCreditCardAck.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblCreditCardAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblCreditCardAckName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblCreditCardAck.AddCell(PosTypealcell2);

                rightCell = new PdfPCell(tblCreditCardAck);
            }

            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl3.AddCell(rightCell);
            pdfDoc.Add(headerTbl3);

            PdfPTable headerTbl4 = new PdfPTable(2);
            headerTbl4.SetWidths(headerWidth);

            PdfPTable tblMarketing = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblMarketing.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Remove employee from websites SunRice/Careers/SunConnect: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblMarketing.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRemoveEmployee.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblMarketing.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Remove Photos from Corporate Affairs images directory: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblMarketing.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRemovePhotos.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblMarketing.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblMarketing);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader3 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader3.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Marketing", headFont));
            header.Border = 0;
            pdfPHeader3.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader3.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader3);
            //pdfDoc.Add(phEmpty);

            headerTbl4.AddCell(leftCell);

            rightCell = new PdfPCell(new Phrase("", ddlFonts));
            if (MarketingAckDiv.Style["display"] != "none")
            {
                PdfPTable tblMarketingAck = new PdfPTable(2);
                tblGeneralInfoWidth = new float[] { 60f, 40f };
                tblMarketingAck.SetWidths(tblGeneralInfoWidth);

                PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblMarketingAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblMarketingAckDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblMarketingAck.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblMarketingAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblMarketingAckName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblMarketingAck.AddCell(PosTypealcell2);

                rightCell = new PdfPCell(tblMarketingAck);
            }
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl4.AddCell(rightCell);
            pdfDoc.Add(headerTbl4);

            PdfPTable headerTbl5 = new PdfPTable(2);
            headerTbl5.SetWidths(headerWidth);

            PdfPTable tblProcurement = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblProcurement.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Company Vehicle Returned: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblProcurement.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCompanyVehicle.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblProcurement.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle keys x 2 sets: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblProcurement.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblVehicleKeys.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblProcurement.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Fuel Card: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblProcurement.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFuelCard.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblProcurement.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Vehicle condition report completed: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblProcurement.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblVehicleCondition.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblProcurement.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblProcurement);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader4 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader4.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Procurement", headFont));
            header.Border = 0;
            pdfPHeader4.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader4.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader4);
            //pdfDoc.Add(phEmpty);

            headerTbl5.AddCell(leftCell);

            rightCell = new PdfPCell(new Phrase("", ddlFonts));
            if (ProcurementAckDiv.Style["display"] != "none")
            {
                PdfPTable tblProcurementAck = new PdfPTable(2);
                tblGeneralInfoWidth = new float[] { 60f, 40f };
                tblProcurementAck.SetWidths(tblGeneralInfoWidth);

                PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblProcurementAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblProcurementAckDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblProcurementAck.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblProcurementAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblProcurementAckName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblProcurementAck.AddCell(PosTypealcell2);

                rightCell = new PdfPCell(tblProcurementAck);
            }
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl5.AddCell(rightCell);
            pdfDoc.Add(headerTbl5);

            PdfPTable headerTbl6 = new PdfPTable(2);
            headerTbl6.SetWidths(headerWidth);

            PdfPTable tblFinance = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblFinance.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Is the employee a Cheque Signatory: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblFinance.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblChequeSignatory.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblFinance.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblFinance);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader5 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader5.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Finance", headFont));
            header.Border = 0;
            pdfPHeader5.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader5.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader5);

            headerTbl6.AddCell(leftCell);

            rightCell = new PdfPCell(new Phrase("", ddlFonts));
            if (FinanceAckDiv.Style["display"] != "none")
            {
                PdfPTable tblFinanceAck = new PdfPTable(2);
                tblGeneralInfoWidth = new float[] { 60f, 40f };
                tblFinanceAck.SetWidths(tblGeneralInfoWidth);

                PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblFinanceAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblFinanceAckDate.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblFinanceAck.AddCell(PosTypealcell2);

                PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
                PosTypePh1 = new Phrase(PosTypeChnk);
                PosTypevalcell = new PdfPCell(PosTypePh1);
                PosTypevalcell.Border = 0;
                tblFinanceAck.AddCell(PosTypevalcell);

                PosTypekVal = new Chunk(lblFinanceAckName.Text, ddlFonts);
                PosTypekValPh2 = new Phrase(PosTypekVal);
                PosTypealcell2 = new PdfPCell(PosTypekValPh2);
                PosTypealcell2.Border = 0;
                tblFinanceAck.AddCell(PosTypealcell2);

                rightCell = new PdfPCell(tblFinanceAck);
            }
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl6.AddCell(rightCell);
            pdfDoc.Add(headerTbl6);

            pdfDoc.Add(phEmpty);

            PdfPTable headerTbl8 = new PdfPTable(2);
            headerTbl8.SetWidths(headerWidth);

            PdfPTable tblSiteAdmin = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblSiteAdmin.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Security Card: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblSiteAdmin.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblSecurityCard.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblSiteAdmin.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Office/Site Keys: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblSiteAdmin.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblOfficeKeys.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblSiteAdmin.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Locker Key: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblSiteAdmin.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblLockerKey.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblSiteAdmin.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("FOB Passes: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblSiteAdmin.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFobPasses.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblSiteAdmin.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Uniform Return: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblSiteAdmin.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblUniformReturn.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblSiteAdmin.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblSiteAdmin);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader7 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader7.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Site Administration", headFont));
            header.Border = 0;
            pdfPHeader7.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader7.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader7);
            //pdfDoc.Add(phEmpty);

            headerTbl8.AddCell(leftCell);

            PdfPTable tblSiteAdminAck = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblSiteAdminAck.SetWidths(tblGeneralInfoWidth);

            /* PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
             PosTypePh1 = new Phrase(PosTypeChnk);
             PosTypevalcell = new PdfPCell(PosTypePh1);
             PosTypevalcell.Border = 0;
             tblSiteAdminAck.AddCell(PosTypevalcell);

             PosTypekVal = new Chunk(lblSiteAdminAckDate.Text, ddlFonts);
             PosTypekValPh2 = new Phrase(PosTypekVal);
             PosTypealcell2 = new PdfPCell(PosTypekValPh2);
             PosTypealcell2.Border = 0;
             tblSiteAdminAck.AddCell(PosTypealcell2);

             PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
             PosTypePh1 = new Phrase(PosTypeChnk);
             PosTypevalcell = new PdfPCell(PosTypePh1);
             PosTypevalcell.Border = 0;
             tblSiteAdminAck.AddCell(PosTypevalcell);

             PosTypekVal = new Chunk(lblSiteAdminAckBy.Text, ddlFonts);
             PosTypekValPh2 = new Phrase(PosTypekVal);
             PosTypealcell2 = new PdfPCell(PosTypekValPh2);
             PosTypealcell2.Border = 0;
             tblSiteAdminAck.AddCell(PosTypealcell2);*/

            rightCell = new PdfPCell(tblSiteAdminAck);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl8.AddCell(rightCell);

            pdfDoc.Add(headerTbl8);

            PdfPTable headerTbl7 = new PdfPTable(2);
            headerTbl7.SetWidths(headerWidth);

            PdfPTable tblITChkList = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblITChkList.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Remove employee from email contact listing/folders/SunConnect Contacts listing: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRemoveContacts.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("All equipment to be returned to IS in Leeton: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblISLeeton.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Remove/Disable computer access: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRemoveAccess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Mobile Phone & Charger returned: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblMobileCharger.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Mobile Phone purchased and transferred into employee's name: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblMobilePurchased.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Any electronic equipment (ipad etc): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblElectronic.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Laptop Collected: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblLaptopCollected.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Disable employees voicemail: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDisableVoicemail.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Set automatic email notification to alert sender that the employee is no longer employed: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkList.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblAutomaticEmail.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkList.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblITChkList);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader6 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader6.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Information Technology Checklist", headFont));
            header.Border = 0;
            pdfPHeader6.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader6.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader6);
            //pdfDoc.Add(phEmpty);

            headerTbl7.AddCell(leftCell);

            PdfPTable tblITChkListAck = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblITChkListAck.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkListAck.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblInfoAckDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkListAck.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblITChkListAck.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblInfoAckName.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblITChkListAck.AddCell(PosTypealcell2);

            rightCell = new PdfPCell(tblITChkListAck);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl7.AddCell(rightCell);

            pdfDoc.Add(headerTbl7);

            PdfPTable headerTbl10 = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            headerTbl10.SetWidths(tblGeneralInfoWidth);

            PdfPTable tblTerminationMeeting = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblTerminationMeeting.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Exit Interview: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblExitInterview.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("All company property collected & actioned: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPropertyCollected.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Re-iterate confidentiality agreement: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblReiterateAgree.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Prepare to notify employees contacts(Customers/Suppliers): ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblNotifyContacts.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Confirm employee's address for future mailing of information: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblConfirmEmployee.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Certificate of Service request: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblCertificateService.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Address / Comments: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblTerminationMeeting.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblMeetingComments.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblTerminationMeeting.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblTerminationMeeting);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader9 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            pdfPHeader9.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("Termination Meeting", headFont));
            header.Border = 0;
            pdfPHeader9.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader9.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader9);


            headerTbl10.AddCell(leftCell);

            //PdfPTable tblTerminationMeetingAck = new PdfPTable(2);
            //tblGeneralInfoWidth = new float[] { 60f, 40f };
            //tblTerminationMeetingAck.SetWidths(tblGeneralInfoWidth);

            //rightCell = new PdfPCell(tblTerminationMeetingAck);
            //rightCell.Border = 0;
            //rightCell.Padding = 0f;
            //headerTbl10.AddCell(rightCell);

            pdfDoc.Add(headerTbl10);

            PdfPTable headerTbl2 = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 50f, 50f };
            headerTbl2.SetWidths(tblGeneralInfoWidth);

            PdfPTable tblHRServices = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 80f, 20f };
            tblHRServices.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Process Final Payment: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblFinalPayment.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Terminat from SAP Payroll System: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblPayrollSystem.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Kronos access removed: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblKronosAccess.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Termination pay provided: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblTerminationPay.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Delimit date monitoring: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblDelimitDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Remove personal file: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblRemovePersonal.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Housing subsidy/Motor vehicle Declaration: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblHousingSubsidy.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("457 Visa Notification to Immigration Department: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServices.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblVisaNotify.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServices.AddCell(PosTypealcell2);

            leftCell = new PdfPCell(tblHRServices);
            leftCell.Border = 0;
            leftCell.Padding = 0f;

            PdfPTable pdfPHeader1 = new PdfPTable(1);
            tblGeneralInfoWidth = new float[] { 100f };
            pdfPHeader1.SetWidths(tblGeneralInfoWidth);

            header = new PdfPCell(new Phrase("HR Services", headFont));
            header.Border = 0;
            pdfPHeader1.AddCell(header);
            header = new PdfPCell(new Phrase("", headFont));
            header.Border = 0;
            pdfPHeader1.AddCell(header);

            pdfDoc.Add(phEmpty);
            pdfDoc.Add(pdfPHeader1);
            //pdfDoc.Add(phEmpty);

            headerTbl2.AddCell(leftCell);

            PdfPTable tblHRServicesAck = new PdfPTable(2);
            tblGeneralInfoWidth = new float[] { 60f, 40f };
            tblHRServicesAck.SetWidths(tblGeneralInfoWidth);

            PosTypeChnk = new Chunk("Date Acknowledged: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServicesAck.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblHRServiceAckDate.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServicesAck.AddCell(PosTypealcell2);

            PosTypeChnk = new Chunk("Name: ", ddlLabelFonts);
            PosTypePh1 = new Phrase(PosTypeChnk);
            PosTypevalcell = new PdfPCell(PosTypePh1);
            PosTypevalcell.Border = 0;
            tblHRServicesAck.AddCell(PosTypevalcell);

            PosTypekVal = new Chunk(lblHRServiceAckName.Text, ddlFonts);
            PosTypekValPh2 = new Phrase(PosTypekVal);
            PosTypealcell2 = new PdfPCell(PosTypekValPh2);
            PosTypealcell2.Border = 0;
            tblHRServicesAck.AddCell(PosTypealcell2);

            rightCell = new PdfPCell(tblHRServicesAck);
            rightCell.Border = 0;
            rightCell.Padding = 0f;
            headerTbl2.AddCell(rightCell);

            pdfDoc.Add(headerTbl2);

            pdfDoc.Close();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

        }

        private bool ValidateBCStatus()
        {
            bool bValid = true;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text.Split(':')[1].Trim();

            string lsturl = HrWebUtility.GetListUrl("HrWebTerminationGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                       {
                           SPList olist = SPContext.Current.Site.RootWeb.GetList(lsturl);
                           SPQuery oquery = new SPQuery();
                           oquery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                           SPListItemCollection collitems = olist.GetItems(oquery);
                           string strCreditCardAckStatus = "";
                           string strProcurementAckStatus = "";
                           string strFinanceAckStatus = "";
                           string strMarketingAckStatus = "";
                           string strISAckStatus = "";

                           foreach (SPListItem listitem in collitems)
                           {
                               strCreditCardAckStatus = Convert.ToString(listitem["CreditCardAckStatus"]);
                               strProcurementAckStatus = Convert.ToString(listitem["ProcurementAckStatus"]);
                               strFinanceAckStatus = Convert.ToString(listitem["FinanceAckStatus"]);
                               strMarketingAckStatus = Convert.ToString(listitem["MarketingAckStatus"]);
                               strISAckStatus = Convert.ToString(listitem["ISAckStatus"]);

                           }

                           if (string.Equals(strCreditCardAckStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                               bValid = false;

                           if (string.Equals(strProcurementAckStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                               bValid = false;

                           if (string.Equals(strFinanceAckStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                               bValid = false;

                           if (string.Equals(strMarketingAckStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                               bValid = false;

                           if (string.Equals(strISAckStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                               bValid = false;
                       });

            return bValid;
        }



        public class pdfPagePaymentHistory : iTextSharp.text.pdf.PdfPageEventHelper
        {

            public override void OnStartPage(PdfWriter writer, Document doc)
            {

                PdfPTable headerTbl = new PdfPTable(3);

                headerTbl.TotalWidth = doc.PageSize.Width;
                string surl = SPContext.Current.Web.Url;


                SPFile file = SPContext.Current.Web.GetFile(SPContext.Current.Web.Url + "/Style%20Library/HR Web/Images/main-logo.png");
                byte[] imageBytes = file.OpenBinary();
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imageBytes);


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


                Chunk chunk = new Chunk("Termination Checklist", hFonts);
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
