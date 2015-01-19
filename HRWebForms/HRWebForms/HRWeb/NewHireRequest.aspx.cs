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
    public partial class NewHireRequest : WebPartPage
    {

        string strRefno = string.Empty;
        string UserName = string.Empty;
        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                lblError.Text = string.Empty;
                divmain.Visible = true;
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


                if (ViewState["vwDependentTbl"] != null)
                {
                    UpdateDependentsFromVS();
                    AddNewRowToDependent();
                }
                else
                {
                    DependentsTabls.Rows.Clear();
                    AddHeaders();
                    AddNewRowToDependent();
                }

                if (!IsPostBack)
                {
                    bool bValid = false;
                    lblDateOfRequest.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    //ddlRef.Visible = false;
                    //lblNewHire.Visible = false;
                    //  dvAppToHire.Visible = false;
                    dvNewHire.Visible = false;
                    dvlblBU.Visible = false;
                    dvdrpBU.Visible = false;

                    dvlblContraBU.Visible = false;
                    dvdrpContraBU.Visible = false;

                    dvlblWagedBU.Visible = false;
                    dvdrpWagedBU.Visible = false;

                    dvlblExpatBU.Visible = false;
                    dvdrpExpatBU.Visible = false;

                    dvlblPostionType.Visible = false;
                    dvdrpPostionType.Visible = false;


                    if (Page.Request.QueryString["refno"] != null)
                    {
                        lblReferenceNo.Text = Page.Request.QueryString["refno"].ToUpper();
                        bValid = ValidateApplication();

                        if (bValid)
                        {
                            PopulateChoiceFields();
                            GetNewHireGeneralInfo();
                            //GetGeneralInfo();
                            string strPositiontype = lblPositionType.Text;
                            if (string.Equals(strPositiontype, "Salary", StringComparison.OrdinalIgnoreCase))
                            {
                                GetNewHireSalaryPositionDetails();
                                GetNewHireSalaryRemunerattionDetails();
                                //GetNewHireSalaryOfferChecklist();
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToSalTab();", true);

                            }
                            else if (string.Equals(strPositiontype, "Waged", StringComparison.OrdinalIgnoreCase))
                            {
                                GetNewHireWagedPositionDetails();
                                GetNewHireWagedRemunerattionDetails();
                                //GetNewHireWagedOfferChecklist();
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToWagedTab();", true);
                            }
                            else if (string.Equals(strPositiontype, "Contractor", StringComparison.OrdinalIgnoreCase))
                            {
                                GetNewHireContractorPositionDetails();
                                GetNewHireContractorRemunerattionDetails();
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToContraTab();", true);

                            }
                            else if (string.Equals(strPositiontype, "Expatriate", StringComparison.OrdinalIgnoreCase))
                            {
                                GetNewHireExpatPositionDetails();
                                GetNewHireExpatRemunerattionDetails();
                                GetNewHireExpatPersonnelDetails();
                                //GetNewHireExpatOfferChecklist();
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
                            }
                        }
                    }
                    else
                    {
                        PopulateChoiceFields();
                        bool IsHRServiceUser = IsUserMemberOfGroup();
                        if (IsHRServiceUser)
                        {
                            GetGeneralInfoForHRService();
                            bValid = true;
                        }
                        else //if (IsApprover())
                        {
                            string strStatus = GetGeneralInfo();

                            bValid = true;
                        }
                    }

                    if (!bValid)
                    {
                        lblError.Text = "The application number passed does not exist or has already been submitted.";
                        divmain.Visible = false;
                    }



                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.Page_Load", ex.Message);
                //lblError.Text = ex.InnerException.Message;
                divmain.Visible = false;
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
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
        
        private bool ValidateApplication()
        {
            bool bValid = false;
            if (lblReferenceNo.Text != "")
                strRefno = lblReferenceNo.Text;
            SPListItemCollection collectionItems = null;
            if (strRefno != "")
            {
                //SPList oList = SPContext.Current.Web.Lists[SetListByName];
                string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPQuery oQuery = new SPQuery();
                        oQuery.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                        collectionItems = oList.GetItems(oQuery);

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
                    });
            }

            return bValid;
        }
        
        private void AddDependentsFromVS()
        {

            DataTable dtDependent = (DataTable)ViewState["vwDependentTbl"];
            DependentsTabls.Rows.Clear();
            AddHeaders();

            if (dtDependent.Rows.Count > 0)
            {
                for (int rowCnt = 0; rowCnt < dtDependent.Rows.Count; rowCnt++)
                {
                    int cnt = rowCnt + 1;
                    TableRow tblRow = new TableRow();
                    tblRow.ID = "tblRow" + cnt;

                    TableCell tblcellDependcnt = new TableCell();
                    tblcellDependcnt.ID = "tblcellDependcnt" + cnt;
                    tblcellDependcnt.Width = 30;
                    /*TextBox txtDep = new TextBox();
                    txtDep.ID = "txtDep" + cnt;
                    txtDep.Attributes.CssStyle.Add("padding", "0px 0px");
                    txtDep.Attributes.CssStyle.Add("margin-top", "3px");
                    txtDep.Attributes.CssStyle.Add("margin-right", "3px");
                    txtDep.Attributes.CssStyle.Add("margin-left", "5px");
                    tblcellDependcnt.Controls.Add(txtDep);*/
                    System.Web.UI.WebControls.Label lblDep = new System.Web.UI.WebControls.Label();
                    lblDep.ID = "lblDep" + rowCnt;
                    // lblDep.Text = Convert.ToString("      " + rowCnt);
                    tblcellDependcnt.Controls.Add(lblDep);

                    TableCell tblcellName = new TableCell();
                    tblcellName.ID = "tblcellName" + cnt;
                    tblcellName.Width = 30;
                    TextBox txtName = new TextBox();
                    txtName.ID = "txtName" + cnt;
                    txtName.Attributes.CssStyle.Add("padding", "0px 0px");
                    txtName.Attributes.CssStyle.Add("margin-top", "3px");
                    txtName.Attributes.CssStyle.Add("margin-right", "3px");
                    txtName.Attributes.CssStyle.Add("margin-left", "5px");
                    tblcellName.Controls.Add(txtName);

                    TableCell tblcellDOB = new TableCell();
                    tblcellDOB.ID = "tblcellDOB" + cnt;
                    tblcellDOB.Width = 30;
                    DateTimeControl dtCntrl = new DateTimeControl();


                    dtCntrl.DateOnly = true;
                    dtCntrl.LocaleId = 2057;
                    dtCntrl.UseTimeZoneAdjustment = false;
                    dtCntrl.ID = "dtCntrl" + cnt;
                    dtCntrl.CssClassTextBox = "hr-web_DateTime";

                    tblcellDOB.Controls.Add(dtCntrl);

                    TableCell tblcellSave = new TableCell();
                    tblcellSave.ID = "tblcellSave" + cnt;
                    tblcellSave.Width = 30;

                    ImageButton imgbtnEditNewRowInsersion = new ImageButton();
                    imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
                    imgbtnEditNewRowInsersion.ID = "imgbtnEditNewRowInsersion" + cnt;
                    imgbtnEditNewRowInsersion.Click += imgbtnEditNewRowInsersion_Click;
                    imgbtnEditNewRowInsersion.ToolTip = "Add new row";
                    imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
                    imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");
                    //imgbtnEditNewRowInsersion.ValidationGroup = "EditAccommodationSave";
                    tblcellSave.Controls.Add(imgbtnEditNewRowInsersion);


                    ImageButton imgbtnDeleteRow = new ImageButton();
                    imgbtnDeleteRow.Attributes.Add("runat", "server");
                    imgbtnDeleteRow.ID = "imgbtnDeleteRow" + cnt;
                    imgbtnDeleteRow.Click += imgbtnDeleteRow_Click;
                    imgbtnDeleteRow.ToolTip = "Delete row";
                    imgbtnDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
                    //imgbtnDeleteRow.Attributes.CssStyle.Add("padding-left", "15px");

                    tblcellSave.Controls.Add(imgbtnDeleteRow);


                    tblRow.Cells.Add(tblcellDependcnt);
                    tblRow.Cells.Add(tblcellName);
                    tblRow.Cells.Add(tblcellDOB);
                    tblRow.Cells.Add(tblcellSave);

                    DependentsTabls.Rows.Add(tblRow);
                }
            }

        }
        
        private void UpdateDependents(bool bresult)
        {
            if (ViewState["vwDependentTbl"] != null)
            {
                DataTable dtDependent = (DataTable)ViewState["vwDependentTbl"];


                if (dtDependent.Rows.Count > 0)
                {
                    for (int rowCnt = 0; rowCnt < dtDependent.Rows.Count; rowCnt++)
                    {
                        int cnt = rowCnt + 1;
                        TableRow tblRow = new TableRow();
                        tblRow.ID = "tblRow" + cnt;

                        TableCell tblcellDependcnt = new TableCell();
                        tblcellDependcnt.ID = "tblcellDependcnt" + cnt;
                        /* TextBox txtDep = new TextBox();
                         txtDep.ID = "txtDep" + cnt;
                         txtDep.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Count"]);
                         tblcellDependcnt.Controls.Add(txtDep);*/


                        TableCell tblcellName = new TableCell();
                        tblcellDependcnt.ID = "tblcellName" + cnt;
                        TextBox txtName = new TextBox();
                        txtName.ID = "txtName" + cnt;
                        txtName.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Name"]);
                        tblcellName.Controls.Add(txtName);

                        TableCell tblcellDOB = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;
                        DateTimeControl dtCntrl = new DateTimeControl();

                        dtCntrl.DateOnly = true;
                        dtCntrl.LocaleId = 2057;
                        string strDOB = Convert.ToString(dtDependent.Rows[cnt - 1]["DOB"]);

                        if (!string.IsNullOrEmpty(strDOB))
                            dtCntrl.SelectedDate = Convert.ToDateTime(strDOB).Date;

                        // dtCntrl.UseTimeZoneAdjustment = false;
                        dtCntrl.ID = "dtCntrl" + cnt;
                        tblcellDOB.Controls.Add(dtCntrl);

                        TableCell tblcellSave = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;

                        ImageButton imgbtnEditNewRowInsersion = new ImageButton();
                        imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
                        imgbtnEditNewRowInsersion.ID = "imgbtnEditNewRowInsersion" + cnt;
                        imgbtnEditNewRowInsersion.Click += imgbtnEditNewRowInsersion_Click;
                        imgbtnEditNewRowInsersion.ToolTip = "Add new row";
                        imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
                        //imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "50px");
                        //imgbtnEditNewRowInsersion.ValidationGroup = "EditAccommodationSave";
                        tblcellSave.Controls.Add(imgbtnEditNewRowInsersion);

                        if (cnt != dtDependent.Rows.Count)
                        {
                            ImageButton imgbtnDeleteRow = new ImageButton();
                            imgbtnDeleteRow.Attributes.Add("runat", "server");
                            imgbtnDeleteRow.ID = "imgbtnDeleteRow" + cnt;
                            imgbtnDeleteRow.Click += imgbtnDeleteRow_Click;
                            imgbtnDeleteRow.ToolTip = "Delete row";
                            imgbtnDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
                            //imgbtnDeleteRow.Attributes.CssStyle.Add("padding-left", "15px");

                            tblcellSave.Controls.Add(imgbtnDeleteRow);
                        }

                        tblRow.Cells.Add(tblcellDependcnt);
                        tblRow.Cells.Add(tblcellName);
                        tblRow.Cells.Add(tblcellDOB);
                        tblRow.Cells.Add(tblcellSave);

                        DependentsTabls.Rows.Add(tblRow);
                    }
                }
            }
        }

        private void UpdateDependentsFromVS()
        {
            if (ViewState["vwDependentTbl"] != null)
            {
                DataTable dtDependent = (DataTable)ViewState["vwDependentTbl"];


                if (dtDependent.Rows.Count > 0)
                {
                    for (int rowCnt = 0; rowCnt < dtDependent.Rows.Count; rowCnt++)
                    {
                        int cnt = rowCnt + 1;
                        TableRow tblRow = new TableRow();
                        tblRow.ID = "tblRow" + cnt;

                        TableCell tblcellDependcnt = new TableCell();
                        tblcellDependcnt.ID = "tblcellDependcnt" + cnt;
                        /* TextBox txtDep = new TextBox();
                         txtDep.ID = "txtDep" + cnt;
                         txtDep.Attributes.CssStyle.Add("padding", "0px 0px");
                         txtDep.Attributes.CssStyle.Add("margin-top", "3px");
                         txtDep.Attributes.CssStyle.Add("margin-right", "3px");
                         txtDep.Attributes.CssStyle.Add("margin-left", "5px");
                         txtDep.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Count"]);
                         tblcellDependcnt.Controls.Add(txtDep);*/
                        System.Web.UI.WebControls.Label lblDep = new System.Web.UI.WebControls.Label();
                        lblDep.ID = "lblDep" + cnt;
                        lblDep.CssClass = "span12";
                        lblDep.Attributes.CssStyle.Add("text-align", "center");
                        //lblDep.Text = Convert.ToString("      " + rowCnt);
                        lblDep.Text = Convert.ToString(cnt);
                        tblcellDependcnt.Controls.Add(lblDep);


                        TableCell tblcellName = new TableCell();
                        tblcellDependcnt.ID = "tblcellName" + cnt;
                        TextBox txtName = new TextBox();
                        txtName.ID = "txtName" + cnt;
                        txtName.Attributes.CssStyle.Add("padding", "0px 0px");
                        txtName.Attributes.CssStyle.Add("margin-top", "3px");
                        txtName.Attributes.CssStyle.Add("margin-right", "3px");
                        txtName.Attributes.CssStyle.Add("margin-left", "5px");
                        txtName.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Name"]);
                        tblcellName.Controls.Add(txtName);

                        TableCell tblcellDOB = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;
                        DateTimeControl dtCntrl = new DateTimeControl();

                        dtCntrl.DateOnly = true;
                        dtCntrl.LocaleId = 2057;
                        string strDOB = Convert.ToString(dtDependent.Rows[cnt - 1]["DOB"]);

                        if (!string.IsNullOrEmpty(strDOB))
                            dtCntrl.SelectedDate = Convert.ToDateTime(strDOB).Date;


                        dtCntrl.CssClassTextBox = "hr-web_DateTime";

                        dtCntrl.UseTimeZoneAdjustment = false;
                        dtCntrl.ID = "dtCntrl" + cnt;
                        tblcellDOB.Controls.Add(dtCntrl);

                        TableCell tblcellSave = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;

                        ImageButton imgbtnEditNewRowInsersion = new ImageButton();
                        imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
                        imgbtnEditNewRowInsersion.ID = "imgbtnEditNewRowInsersion" + cnt;
                        imgbtnEditNewRowInsersion.Click += imgbtnEditNewRowInsersion_Click;
                        imgbtnEditNewRowInsersion.ToolTip = "Add new row";
                        imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
                        imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");
                        //imgbtnEditNewRowInsersion.ValidationGroup = "EditAccommodationSave";
                        tblcellSave.Controls.Add(imgbtnEditNewRowInsersion);


                        ImageButton imgbtnDeleteRow = new ImageButton();
                        imgbtnDeleteRow.Attributes.Add("runat", "server");
                        imgbtnDeleteRow.ID = "imgbtnDeleteRow" + cnt;
                        imgbtnDeleteRow.Click += imgbtnDeleteRow_Click;
                        imgbtnDeleteRow.ToolTip = "Delete row";
                        imgbtnDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
                        //imgbtnDeleteRow.Attributes.CssStyle.Add("padding-left", "15px");

                        tblcellSave.Controls.Add(imgbtnDeleteRow);



                        tblRow.Cells.Add(tblcellDependcnt);
                        tblRow.Cells.Add(tblcellName);
                        tblRow.Cells.Add(tblcellDOB);
                        tblRow.Cells.Add(tblcellSave);

                        DependentsTabls.Rows.Add(tblRow);
                    }
                }
            }
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
                        /* TextBox txtDep = new TextBox();
                         txtDep.ID = "txtDep" + cnt;
                         txtDep.Attributes.CssStyle.Add("padding", "0px 0px");
                         txtDep.Attributes.CssStyle.Add("margin-top", "3px");
                         txtDep.Attributes.CssStyle.Add("margin-right", "3px");
                         txtDep.Attributes.CssStyle.Add("margin-left", "5px");
                         txtDep.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Count"]);
                         tblcellDependcnt.Controls.Add(txtDep);*/

                        System.Web.UI.WebControls.Label lblDep = new System.Web.UI.WebControls.Label();
                        lblDep.ID = "lblDep" + cnt;
                        lblDep.CssClass = "span12";
                        lblDep.Attributes.CssStyle.Add("text-align", "center");

                        lblDep.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Count"]);
                        tblcellDependcnt.Controls.Add(lblDep);

                        TableCell tblcellName = new TableCell();
                        tblcellDependcnt.ID = "tblcellName" + cnt;
                        TextBox txtName = new TextBox();
                        txtName.ID = "txtName" + cnt;
                        txtName.Attributes.CssStyle.Add("padding", "0px 0px");
                        txtName.Attributes.CssStyle.Add("margin-top", "3px");
                        txtName.Attributes.CssStyle.Add("margin-right", "3px");
                        txtName.Attributes.CssStyle.Add("margin-left", "5px");
                        txtName.Text = Convert.ToString(dtDependent.Rows[cnt - 1]["Name"]);
                        tblcellName.Controls.Add(txtName);

                        TableCell tblcellDOB = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;
                        DateTimeControl dtCntrl = new DateTimeControl();

                        dtCntrl.DateOnly = true;
                        dtCntrl.LocaleId = 2057;
                        string strDOB = Convert.ToString(dtDependent.Rows[cnt - 1]["DOB"]);

                        if (!string.IsNullOrEmpty(strDOB))
                            dtCntrl.SelectedDate = Convert.ToDateTime(strDOB).Date;


                        dtCntrl.CssClassTextBox = "hr-web_DateTime";

                        dtCntrl.UseTimeZoneAdjustment = false;
                        dtCntrl.ID = "dtCntrl" + cnt;
                        tblcellDOB.Controls.Add(dtCntrl);

                        TableCell tblcellSave = new TableCell();
                        tblcellDependcnt.ID = "tblcellDOB" + cnt;

                        ImageButton imgbtnEditNewRowInsersion = new ImageButton();
                        imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
                        imgbtnEditNewRowInsersion.ID = "imgbtnEditNewRowInsersion" + cnt;
                        imgbtnEditNewRowInsersion.Click += imgbtnEditNewRowInsersion_Click;
                        imgbtnEditNewRowInsersion.ToolTip = "Add new row";
                        imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
                        imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");
                        //imgbtnEditNewRowInsersion.ValidationGroup = "EditAccommodationSave";
                        tblcellSave.Controls.Add(imgbtnEditNewRowInsersion);


                        ImageButton imgbtnDeleteRow = new ImageButton();
                        imgbtnDeleteRow.Attributes.Add("runat", "server");
                        imgbtnDeleteRow.ID = "imgbtnDeleteRow" + cnt;
                        imgbtnDeleteRow.Click += imgbtnDeleteRow_Click;
                        imgbtnDeleteRow.ToolTip = "Delete row";
                        imgbtnDeleteRow.ImageUrl = "../../Style%20Library/HR%20Web/Images/Delete.jpg";
                        //imgbtnDeleteRow.Attributes.CssStyle.Add("padding-left", "15px");

                        tblcellSave.Controls.Add(imgbtnDeleteRow);



                        tblRow.Cells.Add(tblcellDependcnt);
                        tblRow.Cells.Add(tblcellName);
                        tblRow.Cells.Add(tblcellDOB);
                        tblRow.Cells.Add(tblcellSave);

                        DependentsTabls.Rows.Add(tblRow);
                    }
                }
            }
            // AddNewRowToDependent();
            ViewState["vwDependentTbl"] = dtDependent;

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
            DataTable currentDataTable = (DataTable)ViewState["vwDependentTbl"];
            int itemscount = currentDataTable.Rows.Count;
            currentDataTable.Rows[rowid].Delete();
            ViewState["vwDependentTbl"] = currentDataTable;

            DependentsTabls.Rows.Clear();
            AddHeaders();
            UpdateDependentsFromVS();
            AddNewRowToDependent();
            Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
        }

        protected void btnAddRowDependents_Click(object sender, EventArgs e)
        {
            AddNewRowToDependent();
        }

        private void AddNewRowToDependent()
        {
            int rowCnt = DependentsTabls.Rows.Count;
            TableRow tblRow = new TableRow();
            tblRow.ID = "tblRow" + rowCnt;

            TableCell tblcellDependcnt = new TableCell();
            tblcellDependcnt.ID = "tblcellDependcnt" + rowCnt;
            tblcellDependcnt.Width = 30;

            System.Web.UI.WebControls.Label lblDep = new System.Web.UI.WebControls.Label();
            lblDep.ID = "lblDep" + rowCnt;
            lblDep.Text = Convert.ToString(rowCnt);

            lblDep.CssClass = "span12";
            lblDep.Attributes.CssStyle.Add("text-align", "center");

            /*TextBox txtDep = new TextBox();
            //txtDep.Width = 85;
            txtDep.ID = "txtDep" + rowCnt;
            txtDep.Attributes.CssStyle.Add("padding", "0px 0px");
            txtDep.Attributes.CssStyle.Add("margin-top", "3px");
            txtDep.Attributes.CssStyle.Add("margin-right", "1px");
            txtDep.Attributes.CssStyle.Add("margin-left", "5px");*/

            RequiredFieldValidator rftxtDep = new RequiredFieldValidator();
            rftxtDep.ForeColor = System.Drawing.Color.Red;
            rftxtDep.Text = "*";
            rftxtDep.ErrorMessage = "Dependent Is Empty";
            rftxtDep.ControlToValidate = "txtDep" + rowCnt;
            rftxtDep.ValidationGroup = "DependentsSave";

            tblcellDependcnt.Controls.Add(lblDep);


            TableCell tblcellName = new TableCell();
            tblcellName.ID = "tblcellName" + rowCnt;
            tblcellName.Width = 30;
            TextBox txtName = new TextBox();
            //txtName.Width = 85;
            txtName.ID = "txtName" + rowCnt;
            txtName.Attributes.CssStyle.Add("padding", "0px 0px");
            txtName.Attributes.CssStyle.Add("margin-top", "3px");
            txtName.Attributes.CssStyle.Add("margin-right", "1px");
            txtName.Attributes.CssStyle.Add("margin-left", "5px");

            RequiredFieldValidator rftxtName = new RequiredFieldValidator();
            rftxtName.ForeColor = System.Drawing.Color.Red;
            rftxtName.Text = "*";
            rftxtName.ErrorMessage = "Name Is Empty";
            rftxtName.ControlToValidate = "txtName" + rowCnt;
            rftxtName.ValidationGroup = "DependentsSave";

            tblcellName.Controls.Add(txtName);
            tblcellName.Controls.Add(rftxtName);

            TableCell tblcellDOB = new TableCell();
            tblcellDOB.ID = "tblcellDOB" + rowCnt;
            tblcellDOB.Width = 30;

            DateTimeControl dtCntrl = new DateTimeControl();

            dtCntrl.ID = "dtCntrl" + rowCnt;
            dtCntrl.DateOnly = true;
            dtCntrl.LocaleId = 2057;
            dtCntrl.UseTimeZoneAdjustment = false;
            dtCntrl.CssClassTextBox = "hr-web_DateTime";

            RequiredFieldValidator rfDOB = new RequiredFieldValidator();
            rfDOB.ForeColor = System.Drawing.Color.Red;
            rfDOB.Text = "*";
            rfDOB.ErrorMessage = "DOB Is Empty";
            //rfDOB.ControlToValidate = "dtCntrl" + rowCnt;
            rfDOB.ControlToValidate = "dtCntrl" + rowCnt + "$dtCntrl" + rowCnt + "Date";
            rfDOB.ValidationGroup = "DependentsSave";

            tblcellDOB.Controls.Add(dtCntrl);
            tblcellDOB.Controls.Add(rfDOB);

            TableCell tblcellSave = new TableCell();
            tblcellSave.ID = "tblcellSave" + rowCnt;

            ImageButton imgbtnEditNewRowInsersion = new ImageButton();
            imgbtnEditNewRowInsersion.Attributes.Add("runat", "server");
            imgbtnEditNewRowInsersion.ID = "imgbtnNewRowInsersion" + rowCnt;
            imgbtnEditNewRowInsersion.Click += imgbtnNewRowInsersion_Click;
            imgbtnEditNewRowInsersion.ToolTip = "Add new row";
            imgbtnEditNewRowInsersion.ImageUrl = "../../Style%20Library/HR%20Web/Images/ArrSave.jpg";
            imgbtnEditNewRowInsersion.Attributes.CssStyle.Add("padding-left", "15px");
            imgbtnEditNewRowInsersion.ValidationGroup = "DependentsSave";
            tblcellSave.Controls.Add(imgbtnEditNewRowInsersion);

            tblRow.Cells.Add(tblcellDependcnt);
            tblRow.Cells.Add(tblcellName);
            tblRow.Cells.Add(tblcellDOB);
            tblRow.Cells.Add(tblcellSave);

            DependentsTabls.Rows.Add(tblRow);
            /*DataTable dtDependent = new DataTable();
            if (ViewState["vwDependentTbl"] == null)
            {

                dtDependent.Columns.Add("Count");
                dtDependent.Columns.Add("Name");
                dtDependent.Columns.Add("DOB");
            }
            else
            {
                dtDependent = (DataTable)ViewState["vwDependentTbl"];
            }
            dtDependent.Rows.Add(new string[] { "", "", "" });

            ViewState["vwDependentTbl"] = dtDependent;*/

            // Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
        }

        private void imgbtnEditNewRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton SaveButton = (ImageButton)sender;
            string buttonid = SaveButton.ID.ToString();
            buttonid = buttonid.ToLower().Replace("imgbtneditnewrowinsersion", "");
            //buttonid = buttonid.Remove(0, 24);
            int rowid;
            int.TryParse(buttonid, out rowid);

            string strDep = "";
            string strName = "";
            DateTimeControl dtDOB = null;
            DataTable dtTable = new DataTable();
            if (ViewState["vwDependentTbl"] != null)
            {
                dtTable = (DataTable)ViewState["vwDependentTbl"];

                TableRow tblRow = DependentsTabls.Rows[rowid];
                /* TextBox txtDep = (TextBox)tblRow.FindControl("txtDep" + rowid);
                 strDep = txtDep.Text;*/
                System.Web.UI.WebControls.Label lblDep = (System.Web.UI.WebControls.Label)tblRow.FindControl("lblDep" + rowid);
                strDep = lblDep.Text;


                TextBox txtName = (TextBox)tblRow.FindControl("txtName" + rowid);
                strName = txtName.Text;
                dtDOB = (DateTimeControl)tblRow.FindControl("dtCntrl" + rowid);

                dtTable.Rows[rowid - 1]["Count"] = strDep;
                dtTable.Rows[rowid - 1]["Name"] = strName;
                dtTable.Rows[rowid - 1]["DOB"] = dtDOB.SelectedDate.Date;

            }

            DependentsTabls.Rows.Clear();

            AddHeaders();
            UpdateDependentsFromDataTable(dtTable);

            AddNewRowToDependent();
            ViewState["vwDependentTbl"] = dtTable;

            Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
        }
        
        private void imgbtnNewRowInsersion_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton SaveButton = (ImageButton)sender;
            string buttonid = SaveButton.ID.ToString();
            buttonid = buttonid.ToLower().Replace("imgbtnnewrowinsersion", "");
            //buttonid = buttonid.Remove(0, 24);
            int rowid;
            int.TryParse(buttonid, out rowid);

            string strDep = "";
            string strName = "";
            DateTimeControl dtDOB = null;
            DataTable dtTable = new DataTable();
            if (ViewState["vwDependentTbl"] == null)
            {
                dtTable.Columns.Add("Count");
                dtTable.Columns.Add("Name");
                dtTable.Columns.Add("DOB");

                TableRow tblRow = DependentsTabls.Rows[rowid];
                /*TextBox txtDep = (TextBox)tblRow.FindControl("txtDep" + rowid);
                strDep = txtDep.Text;*/
                System.Web.UI.WebControls.Label lblDep = (System.Web.UI.WebControls.Label)tblRow.FindControl("lblDep" + rowid);
                strDep = lblDep.Text;
                TextBox txtName = (TextBox)tblRow.FindControl("txtName" + rowid);
                strName = txtName.Text;
                dtDOB = (DateTimeControl)tblRow.FindControl("dtCntrl" + rowid);

                DataRow dr = dtTable.NewRow();
                dr["Count"] = strDep;
                dr["Name"] = strName;
                dr["DOB"] = dtDOB.SelectedDate.Date;


                dtTable.Rows.Add(dr);

            }
            else if (ViewState["vwDependentTbl"] != null)
            {
                dtTable = (DataTable)ViewState["vwDependentTbl"];

                TableRow tblRow = DependentsTabls.Rows[rowid];
                System.Web.UI.WebControls.Label lblDep = (System.Web.UI.WebControls.Label)tblRow.FindControl("lblDep" + rowid);
                strDep = lblDep.Text;
                TextBox txtName = (TextBox)tblRow.FindControl("txtName" + rowid);
                strName = txtName.Text;
                dtDOB = (DateTimeControl)tblRow.FindControl("dtCntrl" + rowid);

                DataRow dr = dtTable.NewRow();
                dr["Count"] = strDep;
                dr["Name"] = strName;
                dr["DOB"] = dtDOB.SelectedDate.Date;


                dtTable.Rows.Add(dr);
            }

            DependentsTabls.Rows.Clear();

            AddHeaders();
            UpdateDependentsFromDataTable(dtTable);


            AddNewRowToDependent();

            ViewState["vwDependentTbl"] = dtTable;


            Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
        }
        
        private void AddHeaders()
        {
            TableHeaderRow HeadRw = new TableHeaderRow();
            HeadRw.Style.Add("width", "100%");
            TableHeaderCell tblCellDep = new TableHeaderCell();
            tblCellDep.Style.Add("width", "15%");
            TableHeaderCell tblCellName = new TableHeaderCell();
            tblCellName.Style.Add("width", "30%");
            TableHeaderCell tblCellDOB = new TableHeaderCell();
            tblCellDOB.Style.Add("width", "40%");
            TableHeaderCell tblCellAdd = new TableHeaderCell();
            tblCellAdd.Style.Add("width", "15%");

            tblCellDep.Text = "Dependent";
            tblCellName.Text = "Name";
            tblCellDOB.Text = "DOB";

            HeadRw.Cells.Add(tblCellDep);
            HeadRw.Cells.Add(tblCellName);
            HeadRw.Cells.Add(tblCellDOB);
            HeadRw.Cells.Add(tblCellAdd);
            DependentsTabls.Rows.Add(HeadRw);
        }
        
        private SPListItemCollection SetListData(string SetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
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
                
        private string GetPositionType(string strRefNo)
        {
            string strPosType = "";

            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                        SPQuery oQuery = new SPQuery();
                        oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefNo + "</Value></Eq></Where>";
                        SPListItemCollection oItems = oList.GetItems(oQuery);
                        if (oItems != null && oItems.Count > 0)
                        {
                            foreach (SPListItem itm in oItems)
                            {

                                strPosType = Convert.ToString(itm["PositionType"]);
                            }
                        }
                    });
            return strPosType;
        }

        private bool IsApprover()
        {
            bool result = false;

            SPQuery appInfoQuery = new SPQuery();
            string appHireInfoLstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList appHireInfoList = SPContext.Current.Site.RootWeb.GetList(appHireInfoLstURL);
                        appInfoQuery.Query = "<Where><Or><Eq><FieldRef Name='Approver1'/><Value Type='User'>" + UserName +
                                                    "</Value></Eq><Eq><FieldRef Name='Approver2'/><Value Type='User'>" + UserName +
                                                    "</Value></Eq></Or></Where>";

                        SPListItemCollection appHireInfoItems = appHireInfoList.GetItems(appInfoQuery);
                        foreach (SPListItem item in appHireInfoItems)
                        {
                            result = true;

                        }
                    });
            return result;
        }

        private string GetGeneralInfo()
        {
            PopulateTaxonomy();
            AddBusinessUnitWAandLoc();

            string strStatus = "";
            SPQuery appInfoQuery = new SPQuery();
            string appHireInfoLstURL = HrWebUtility.GetListUrl("AppToHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList appHireInfoList = SPContext.Current.Site.RootWeb.GetList(appHireInfoLstURL);
                        appInfoQuery.Query = "<Where><Or><Eq><FieldRef Name='Approver1'/><Value Type='User'>" + UserName +
                                                    "</Value></Eq><Eq><FieldRef Name='Approver2'/><Value Type='User'>" + UserName +
                                                    "</Value></Eq></Or></Where>";

                        SPListItemCollection appHireInfoItems = appHireInfoList.GetItems(appInfoQuery);
                        foreach (SPListItem item in appHireInfoItems)
                        {
                            TaxonomyFieldValue value = item["BusinessUnit"] as TaxonomyFieldValue;
                            string appHirePosLstURL = HrWebUtility.GetListUrl("PositionDetails");
                            SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                            SPQuery appHirePosQuery = new SPQuery();
                            appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\'/><Value Type=\"Text\">" +
                                value.Label + "</Value></Eq></Where><OrderBy><FieldRef Name='Title' Ascending='False' /></OrderBy>";
                            SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                            foreach (SPListItem appHirePosItem in appHirePosItems)
                            {
                                string refno = Convert.ToString(appHirePosItem["Title"]);
                                string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
                                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                                SPQuery oQuery = new SPQuery();
                                oQuery.Query = "<Where><And><Eq><FieldRef Name=\'Status\'/><Value Type=\"Text\">Approved</Value></Eq>" +
                                    "<Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + refno + "</Value></Eq></And></Where>";
                                SPListItemCollection oItems = oList.GetItems(oQuery);
                                foreach (SPListItem item1 in oItems)
                                {
                                    bool IsProcessed = CheckNewHire(Convert.ToString(item1["Title"]));
                                    if (!IsProcessed)
                                    {
                                        ddlRef.Items.Add(Convert.ToString(item1["Title"]));
                                    }
                                }
                            }
                        }

                        if (ddlRef.Items != null && ddlRef.Items.Count > 1 && !string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
                        {
                            //ddlRef.Visible = true;
                            dvAppToHire.Visible = true;
                            dvlblBU.Visible = true;
                            dvlblPostionType.Visible = true;


                            //lblPositionType.Text = ddlRef.SelectedValue;
                            lblPositionType.Text = GetPositionType(ddlRef.SelectedItem.Text);
                            ddlPositionType.SelectedValue = lblPositionType.Text;
                            //lblReferenceNo.Text = ddlRef.SelectedItem.Text;

                            if (string.IsNullOrEmpty(lblReferenceNo.Text.Trim()))
                            {
                                SPSecurity.RunWithElevatedPrivileges(delegate()
                                {

                                    SPWeb web = SPContext.Current.Site.RootWeb;

                                    string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                                    SPList oList = web.GetList(lstURL);
                                    SPListItem listitem = oList.AddItem();
                                    web.AllowUnsafeUpdates = true;
                                    listitem.Update();
                                    //listitem["Title"] = "Ref No: AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');

                                    lblReferenceNo.Text = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    strRefno = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    listitem["RefNo"] = strRefno;

                                    if (ddlRef.Items.Count > 1 && !string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
                                        listitem["AppToHireRefNo"] = ddlRef.SelectedItem.Text;

                                    listitem.Update();
                                    web.AllowUnsafeUpdates = false;

                                });
                            }


                            if (string.Equals(lblPositionType.Text, "Salary", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToSalTab();", true);
                                GetSalaryPositionDetails(ddlRef.SelectedItem.Text);
                                GetSalaryRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Expatriate", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
                                GetExpatPositionDetails(ddlRef.SelectedItem.Text);
                                GetExpatRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Waged", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToWagedTab();", true);
                                GetWagedPositionDetails(ddlRef.SelectedItem.Text);
                                GetWagedRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Contractor", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToContraTab();", true);
                                GetContractorPositionDetails(ddlRef.SelectedItem.Text);
                                // GetcontrRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                        }
                        else
                        {
                            // ddlRef.Visible = false;

                            dvdrpPostionType.Visible = true;
                            if (string.IsNullOrEmpty(lblReferenceNo.Text.Trim()))
                            {
                                SPSecurity.RunWithElevatedPrivileges(delegate()
                                {

                                    SPWeb web = SPContext.Current.Site.RootWeb;

                                    string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                                    SPList oList = web.GetList(lstURL);
                                    SPListItem listitem = oList.AddItem();
                                    web.AllowUnsafeUpdates = true;
                                    listitem.Update();
                                    //listitem["Title"] = "Ref No: AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');

                                    lblReferenceNo.Text = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    strRefno = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    listitem["RefNo"] = strRefno;
                                    // listitem["AppToHireRefNo"] = ddlRef.SelectedItem.Text;

                                    listitem.Update();
                                    web.AllowUnsafeUpdates = false;

                                });
                            }
                            //lblNewHire.Visible = true;
                            dvNewHire.Visible = true;
                            //lblNewHire.Text = lblReferenceNo.Text;
                            dvdrpBU.Visible = true;
                            dvdrpContraBU.Visible = true;
                            dvdrpWagedBU.Visible = true;
                            dvdrpExpatBU.Visible = true;

                            //lblError.Text = "You do not have any submitted applications for hire";
                        }
                    });
            return strStatus;
        }

        private bool CheckNewHire(string refno)
        {
            bool processed = false;
            string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                        SPQuery oQuery = new SPQuery();
                        oQuery.Query = "<Where><Eq><FieldRef Name=\'AppToHireRefNo\'/><Value Type=\"Text\">" + refno + "</Value></Eq></Where>";
                        SPListItemCollection oItems = oList.GetItems(oQuery);
                        if (oItems != null && oItems.Count > 0)
                        {
                            processed = true;
                        }
                    });
            return processed;
        }

        private void GetGeneralInfoForHRService()
        {
            string lstURL = HrWebUtility.GetListUrl("AppToHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                        SPQuery oQuery = new SPQuery();
                        oQuery.Query = "<Where><Eq><FieldRef Name=\'Status\'/><Value Type=\"Text\">Approved</Value></Eq></Where>";
                        SPListItemCollection oItems = oList.GetItems(oQuery);
                        if (oItems != null && oItems.Count > 0)
                        {
                            foreach (SPListItem itm in oItems)
                            {
                                string strRefNo = Convert.ToString(itm["Title"]);
                                bool IsProcessed = CheckNewHire(strRefNo);
                                if (!IsProcessed)
                                {
                                    ddlRef.Items.Add(strRefNo);

                                }
                            }
                        }
                        PopulateTaxonomy();
                        AddBusinessUnitWAandLoc();

                        if (ddlRef.Items != null && ddlRef.Items.Count > 1 && !string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
                        {
                            dvAppToHire.Visible = true;
                            dvlblBU.Visible = true;
                            dvlblPostionType.Visible = true;
                            //lblPositionType.Text = ddlRef.SelectedValue;
                            lblPositionType.Text = GetPositionType(ddlRef.SelectedItem.Text);
                            ddlPositionType.SelectedValue = lblPositionType.Text;
                            //lblReferenceNo.Text = ddlRef.SelectedItem.Text;

                            if (string.IsNullOrEmpty(lblReferenceNo.Text.Trim()))
                            {
                                SPSecurity.RunWithElevatedPrivileges(delegate()
                                {

                                    SPWeb web = SPContext.Current.Site.RootWeb;

                                    string lstURL1 = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                                    SPList oList1 = web.GetList(lstURL1);
                                    SPListItem listitem = oList1.AddItem();
                                    web.AllowUnsafeUpdates = true;
                                    listitem.Update();
                                    //listitem["Title"] = "Ref No: AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');

                                    lblReferenceNo.Text = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    strRefno = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    listitem["RefNo"] = strRefno;

                                    if (ddlRef.Items.Count > 1 && !string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
                                        listitem["AppToHireRefNo"] = ddlRef.SelectedItem.Text;

                                    listitem.Update();
                                    web.AllowUnsafeUpdates = false;

                                });
                            }

                            if (string.Equals(lblPositionType.Text, "Salary", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToSalTab();", true);
                                GetSalaryPositionDetails(ddlRef.SelectedItem.Text);
                                GetSalaryRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Expatriate", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
                                GetExpatPositionDetails(ddlRef.SelectedItem.Text);
                                GetExpatRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Waged", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToWagedTab();", true);
                                GetWagedPositionDetails(ddlRef.SelectedItem.Text);
                                GetWagedRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                            else if (string.Equals(lblPositionType.Text, "Contractor", StringComparison.OrdinalIgnoreCase))
                            {
                                Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToContraTab();", true);
                                GetContractorPositionDetails(ddlRef.SelectedItem.Text);
                                // GetcontrRemunerattionDetails(ddlRef.SelectedItem.Text);
                            }
                        }
                        else
                        {
                            // ddlRef.Visible = false;
                            dvdrpPostionType.Visible = true;
                            if (string.IsNullOrEmpty(lblReferenceNo.Text.Trim()))
                            {
                                SPSecurity.RunWithElevatedPrivileges(delegate()
                                {

                                    SPWeb web = SPContext.Current.Site.RootWeb;

                                    string lstURL1 = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                                    SPList oList1 = web.GetList(lstURL1);
                                    SPListItem listitem = oList1.AddItem();
                                    web.AllowUnsafeUpdates = true;
                                    listitem.Update();
                                    //listitem["Title"] = "Ref No: AH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');

                                    lblReferenceNo.Text = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    strRefno = "NH" + Convert.ToString(listitem["ID"]).PadLeft(8, '0');
                                    listitem["RefNo"] = strRefno;
                                    // listitem["AppToHireRefNo"] = ddlRef.SelectedItem.Text;

                                    listitem.Update();
                                    web.AllowUnsafeUpdates = false;

                                });
                            }
                            //lblNewHire.Visible = true;
                            dvNewHire.Visible = true;
                            //lblNewHire.Text = lblReferenceNo.Text;
                            dvdrpBU.Visible = true;
                            dvdrpBU.Visible = true;
                            dvdrpContraBU.Visible = true;
                            dvdrpWagedBU.Visible = true;
                            dvdrpExpatBU.Visible = true;
                            //lblError.Text = "You do not have any submitted applications for hire";
                        }
                    });
        }

        private void GetNewHireGeneralInfo()
        {
            PopulateTaxonomy();
            AddBusinessUnitWAandLoc();

            if (strRefno == "")
                strRefno = lblReferenceNo.Text;

            string appHirePosLstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                        SPQuery appHirePosQuery = new SPQuery();
                        appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                        SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                        foreach (SPListItem listitem in appHirePosItems)
                        {
                            lblReferenceNo.Text = Convert.ToString(listitem["RefNo"]);
                            txtFirstName.Text = Convert.ToString(listitem["Title"]);
                            txtLastName.Text = Convert.ToString(listitem["LastName"]);
                            txtAddress.Text = Convert.ToString(listitem["Address"]);
                            txtCity.Text = Convert.ToString(listitem["City"]);
                            txtState.Text = Convert.ToString(listitem["State"]);
                            lblDateOfRequest.Text = Convert.ToDateTime(listitem["Date"]).Date.ToString("dd/MM/yyyy");
                            txtPostCode.Text = Convert.ToString(listitem["PostCode"]);
                            ddlTypeOfRole.SelectedValue = Convert.ToString(listitem["Role"]);

                            string strPositionType = Convert.ToString(listitem["PositionType"]);

                            lblPositionType.Text = strPositionType;
                            ddlPositionType.SelectedValue = strPositionType;

                            string strAppHire = Convert.ToString(listitem["AppToHireRefNo"]);
                            if (string.IsNullOrEmpty(strAppHire))
                            {
                                //lblNewHire.Visible = true;
                                //lblNewHire.Text = Convert.ToString(listitem["RefNo"]);
                                dvdrpBU.Visible = true;
                                dvdrpContraBU.Visible = true;
                                dvdrpExpatBU.Visible = true;
                                dvdrpWagedBU.Visible = true;
                                dvdrpPostionType.Visible = true;
                            }
                            else
                            {
                                dvlblBU.Visible = true;
                                dvlblContraBU.Visible = true;
                                dvlblWagedBU.Visible = true;
                                dvlblExpatBU.Visible = true;
                                dvlblPostionType.Visible = true;
                                ddlRef.Visible = true;
                                ddlRef.Items.Add(new ListItem(strAppHire, strPositionType));
                            }
                        }
                    });
        }
        
        private void GetNewHireSalaryPositionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            //dvlblBU.Visible = true;

            SPListItemCollection oListItems = GetListData("NewHirePositionDetails", strRefno);
            if (oListItems != null && oListItems.Count > 0)
            {
                foreach (SPListItem listitem in oListItems)
                {
                    SPListItemCollection oItems = null;
                    txtPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                    if (dvdrpBU.Visible)
                        ddlBusinessUnit.SelectedValue = Convert.ToString(listitem["BusinessUnit"]);
                    else
                        lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);


                    oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems != null && oItems.Count > 0)
                    {
                        DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                        ddlWorkArea.DataSource = dtWorkArea;
                        ddlWorkArea.DataValueField = "WorkArea";
                        ddlWorkArea.DataTextField = "WorkArea";
                        ddlWorkArea.DataBind();
                        ddlWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                    }

                    SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems1 != null && oItems1.Count > 0)
                    {
                        DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                        ddlSiteLocation.DataSource = dtLocation;
                        ddlSiteLocation.DataValueField = "Location";
                        ddlSiteLocation.DataTextField = "Location";
                        ddlSiteLocation.DataBind();
                        ddlSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                    }


                    if (listitem["ReportsTo"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ReportsToPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    ddlContractType.SelectedValue = Convert.ToString(listitem["ContractType"]);
                    txtCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                    CommencementDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["CommencementDate"]);
                    TermEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);


                    txtNewSalaryReview.Text = Convert.ToString(listitem["NextSalaryReview"]);

                    if (listitem["WhoSignLetter"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["WhoSignLetter"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        SignLetterPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    txtNotes.Text = Convert.ToString(listitem["Notes"]);
                }
            }

        }

        private void GetNewHireExpatPersonnelDetails()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;

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
                            ddlMaritalStatus.SelectedValue = Convert.ToString(listitem["MaritalStatus"]);
                            string strDependent = Convert.ToString(listitem["Dependent"]);
                            string strName = Convert.ToString(listitem["Name"]);
                            string strDOB = Convert.ToString(listitem["DOB"]);

                            dtDependent.Rows.Add(new string[] { strDependent, strName, strDOB });

                        }

                        if (dtDependent.Rows.Count > 0)
                        {
                            DependentsTabls.Rows.Clear();
                            AddHeaders();
                            UpdateDependentsFromDataTable(dtDependent);
                            AddNewRowToDependent();
                        }
                    });
        }

        private void GetNewHireSalaryRemunerattionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireRemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlGrade.SelectedValue = Convert.ToString(ListItems["Grade"]);
                ddlVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtFAR.Text = Convert.ToString(ListItems["FAR"]);
                //txtSTI.Text = Convert.ToString(ListItems["STI"]);
                ddlSTI.SelectedValue = Convert.ToString(ListItems["STI"]);
                txtIfOthers.Text = Convert.ToString(ListItems["IfOthersSpecify"]);
                ddlRelocation.SelectedValue = Convert.ToString(ListItems["Relocation"]);
                txtRelocationDet.Text = Convert.ToString(ListItems["RelocationDetails"]);
            }
        }
        /*private void GetNewHireSalaryOfferChecklist()
        {
            strRefno = ddlRef.SelectedItem.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireOfferChecklist", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                string strChkLst = Convert.ToString(ListItems["Checklist"]);

                foreach (ListItem itm in chkbxLstSalOffer.Items)
                {
                    if (string.Equals(strChkLst, itm.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        itm.Selected = true;
                    }
                }
            }
        }*/

        private void GetNewHireWagedPositionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;

            strRefno = lblReferenceNo.Text;
            //dvlblWagedBU.Visible = true;

            SPListItemCollection oListItems = GetListData("NewHirePositionDetails", strRefno);
            if (oListItems != null && oListItems.Count > 0)
            {
                foreach (SPListItem listitem in oListItems)
                {

                    txtWagedPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                    if (dvdrpWagedBU.Visible)
                        ddlWagedBusinessUnit.SelectedValue = Convert.ToString(listitem["BusinessUnit"]);
                    else
                        lblWagedBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                    txtWagedCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                    SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems != null && oItems.Count > 0)
                    {
                        DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                        ddlWagedWorkArea.DataSource = dtWorkArea;
                        ddlWagedWorkArea.DataValueField = "WorkArea";
                        ddlWagedWorkArea.DataTextField = "WorkArea";
                        ddlWagedWorkArea.DataBind();
                        ddlWagedWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                    }

                    SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems1 != null && oItems1.Count > 0)
                    {
                        DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                        ddlWagedSiteLocation.DataSource = dtLocation;
                        ddlWagedSiteLocation.DataValueField = "Location";
                        ddlWagedSiteLocation.DataTextField = "Location";
                        ddlWagedSiteLocation.DataBind();
                        ddlWagedSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                    }


                    if (listitem["ReportsTo"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ReportsToWagedPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }




                    WagedCommencementDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["CommencementDate"]);
                    WagedTermEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);




                    if (listitem["WhoSignLetter"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["WhoSignLetter"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        WagedSignPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    txtWagedNotes.Text = Convert.ToString(listitem["Notes"]);
                }
            }

        }
       
        private void GetNewHireWagedRemunerattionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireRemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlWagedLevel.SelectedValue = Convert.ToString(ListItems["Level"]);
                ddlWagedRosterType.SelectedValue = Convert.ToString(ListItems["RosterType"]);
                txtWagedCrew.Text = Convert.ToString(ListItems["Crew"]);
                //txtSTI.Text = Convert.ToString(ListItems["STI"]);
                txtWagedShiftTream.Text = Convert.ToString(ListItems["ShiftTeamLeader"]);
                txtWagedAllowances.Text = Convert.ToString(ListItems["Allowances"]);
                ddlWagedVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtWagedIfOther.Text = Convert.ToString(ListItems["IfOthersSpecify"]);

            }
        }
        /*private void GetNewHireWagedOfferChecklist()
        {
            strRefno = ddlRef.SelectedItem.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireOfferChecklist", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                string strChkLst = Convert.ToString(ListItems["Checklist"]);

                foreach (ListItem itm in chkbxLstWaged.Items)
                {
                    if (string.Equals(strChkLst, itm.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        itm.Selected = true;
                    }
                }
            }
        }*/

        private void GetNewHireContractorPositionDetails()
        {

            // strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            //dvlblContraBU.Visible = true;

            SPListItemCollection oListItems = GetListData("NewHirePositionDetails", strRefno);
            if (oListItems != null && oListItems.Count > 0)
            {
                foreach (SPListItem listitem in oListItems)
                {

                    //ddlRef.Items.Add

                    txtContractPosition.Text = Convert.ToString(listitem["PositionTitle"]);
                    txtContractCompany.Text = Convert.ToString(listitem["CompanyTradingName"]);
                    txtContractABN.Text = Convert.ToString(listitem["ABN"]);
                    if (dvdrpContraBU.Visible)
                        ddlContraBusinessUnit.SelectedValue = Convert.ToString(listitem["BusinessUnit"]);
                    else
                        lblContraBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);


                    SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems != null && oItems.Count > 0)
                    {
                        DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                        ddlContraWorkArea.DataSource = dtWorkArea;
                        ddlContraWorkArea.DataValueField = "WorkArea";
                        ddlContraWorkArea.DataTextField = "WorkArea";
                        ddlContraWorkArea.DataBind();
                        ddlContraWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                    }

                    SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems1 != null && oItems1.Count > 0)
                    {
                        DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                        ddlContraSiteLocation.DataSource = dtLocation;
                        ddlContraSiteLocation.DataValueField = "Location";
                        ddlContraSiteLocation.DataTextField = "Location";
                        ddlContraSiteLocation.DataBind();
                        ddlContraSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                    }


                    if (listitem["ReportsTo"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ReportsToContractorPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    txtContraCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                    txtContractRate.Text = Convert.ToString(listitem["ContractRate"]);
                    ddlRateTypeField.SelectedValue = Convert.ToString(listitem["RateTypeField"]);

                    ContraStartDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["CommencementDate"]);
                    ContraEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);
                    ddlContractPaymentTerms.SelectedValue = Convert.ToString(listitem["PaymentTerms"]);

                    txtContractOthers.Text = Convert.ToString(listitem["IfOtherSpecify"]);
                    ddlContractGST.SelectedValue = Convert.ToString(listitem["GST"]);


                    if (listitem["WhoSignLetter"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["WhoSignLetter"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ContractSignPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    // txtWagedNotes.Text = Convert.ToString(listitem["Notes"]);
                }
            }

        }
        
        private void GetNewHireContractorRemunerattionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireRemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                txtContraRoleStatement.Text = Convert.ToString(ListItems["ServicesProvided"]);


            }
        }

        private void GetNewHireExpatPositionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            //dvlblExpatBU.Visible = true;

            SPListItemCollection oListItems = GetListData("NewHirePositionDetails", strRefno);
            if (oListItems != null && oListItems.Count > 0)
            {
                foreach (SPListItem listitem in oListItems)
                {

                    txtExpatPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                    if (dvdrpExpatBU.Visible)
                        ddlExpatBusinessUnit.SelectedValue = Convert.ToString(listitem["BusinessUnit"]);
                    else
                        lblExpatBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);
                    txtexpatCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                    SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems != null && oItems.Count > 0)
                    {
                        DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                        ddlExpatWorkArea.DataSource = dtWorkArea;
                        ddlExpatWorkArea.DataValueField = "WorkArea";
                        ddlExpatWorkArea.DataTextField = "WorkArea";
                        ddlExpatWorkArea.DataBind();
                        ddlExpatWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                    }

                    SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", Convert.ToString(listitem["BusinessUnit"]));

                    if (oItems1 != null && oItems1.Count > 0)
                    {
                        DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                        ddlExpatSiteLocation.DataSource = dtLocation;
                        ddlExpatSiteLocation.DataValueField = "Location";
                        ddlExpatSiteLocation.DataTextField = "Location";
                        ddlExpatSiteLocation.DataBind();
                        ddlExpatSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                    }


                    if (listitem["ReportsTo"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ReportsToExpatPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }


                    txtContractPeriods.Text = Convert.ToString(listitem["ContractPeriod"]);
                    txtExpatNextReview.Text = Convert.ToString(listitem["NextSalaryReview"]);
                    txtExpatHomeLocation.Text = Convert.ToString(listitem["HomeLocation"]);

                    ExpatEffectiveTimeControl.SelectedDate = Convert.ToDateTime(listitem["CommencementDate"]);
                    ExpatContractDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);




                    if (listitem["WhoSignLetter"] != null)
                    {
                        string strpplpicker = string.Empty;
                        SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["WhoSignLetter"].ToString());
                        for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                        {
                            strpplpicker = strpplpicker + workers[coworker] + ",";
                        }
                        ExpatSignPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                    }

                    txtExpatNotes.Text = Convert.ToString(listitem["Notes"]);
                }
            }

        }
        
        private void GetNewHireExpatRemunerattionDetails()
        {
            //strRefno = ddlRef.SelectedItem.Text;
            strRefno = lblReferenceNo.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireRemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {

                ddlExpatGrade.SelectedValue = Convert.ToString(ListItems["Grade"]);
                txtExpatFAR.Text = Convert.ToString(ListItems["FAR"]);

                ddlExpatSTI.SelectedValue = Convert.ToString(ListItems["STI"]);
            }
        }

        /*private void GetNewHireExpatOfferChecklist()
        {
            strRefno = ddlRef.SelectedItem.Text;
            SPListItemCollection RemunarationDetailscollecItems = GetListData("NewHireOfferChecklist", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                string strChkLst = Convert.ToString(ListItems["Checklist"]);

                foreach (ListItem itm in chkbxLstExpat.Items)
                {
                    if (string.Equals(strChkLst, itm.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        itm.Selected = true;
                    }
                }
            }
        }*/

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
                    });
            return oList.GetItems(oQuery);
        }
        
        private SPListItemCollection GetListData(string GetListByName, string strRefno)
        {
            SPListItemCollection collectionItems = null;
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;
            SPWeb mySite = SPContext.Current.Web;
            //SPList oList = SPContext.Current.Web.Lists[GetListByName];
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

                   });
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
                        ddlPositionType.DataSource = AddTerms("Position Type", termGroup);
                        ddlPositionType.DataTextField = "Term";
                        ddlPositionType.DataValueField = "Term";
                        ddlPositionType.DataBind();

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

                        for (int i = 0; i < ddlPositionType.Items.Count; i++)
                        {
                            if (ddlPositionType.Items[i].Text == "Salary")
                                ddlPositionType.Items[i].Selected = true;
                        }
                        lblPositionType.Text = "Salary";
                        break;
                    case "Location Group":

                        break;
                    case "Organsiation Group":
                        /*  ddlBusinessUnit.DataSource = AddSubTerms("Group", termGroup, "SunRice");
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

        private void GetSalaryPositionDetails(string strRefNo)
        {
            dvlblBU.Visible = true;

            string appHirePosLstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                       SPQuery appHirePosQuery = new SPQuery();
                       appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefNo + "</Value></Eq></Where>";
                       SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                       foreach (SPListItem listitem in appHirePosItems)
                       {
                           txtPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                           lblBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                           SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", lblBusinessUnit.Text);

                           if (oItems != null && oItems.Count > 0)
                           {
                               DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                               ddlWorkArea.DataSource = dtWorkArea;
                               ddlWorkArea.DataValueField = "WorkArea";
                               ddlWorkArea.DataTextField = "WorkArea";
                               ddlWorkArea.DataBind();
                               ddlWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                           }

                           SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", lblBusinessUnit.Text);

                           if (oItems1 != null && oItems1.Count > 0)
                           {
                               DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                               ddlSiteLocation.DataSource = dtLocation;
                               ddlSiteLocation.DataValueField = "Location";
                               ddlSiteLocation.DataTextField = "Location";
                               ddlSiteLocation.DataBind();
                               ddlSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                           }


                           if (listitem["ReportsTo"] != null)
                           {
                               string strpplpicker = string.Empty;
                               SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                               for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                               {
                                   strpplpicker = strpplpicker + workers[coworker] + ",";
                               }
                               ReportsToPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                           }

                           txtCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                           CommencementDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedStartDate"]);
                           TermEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);
                           ddlTypeOfRole.SelectedValue = Convert.ToString(listitem["PositionType"]);

                       }
                   });
        }
        
        private void GetSalaryRemunerattionDetails(string strRefno)
        {

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
        
        private void GetWagedPositionDetails(string strRefNo)
        {
            dvlblWagedBU.Visible = true;
            string appHirePosLstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                       SPQuery appHirePosQuery = new SPQuery();
                       appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefNo + "</Value></Eq></Where>";
                       SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                       foreach (SPListItem listitem in appHirePosItems)
                       {
                           txtWagedPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                           lblWagedBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                           SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", lblWagedBusinessUnit.Text);

                           if (oItems != null && oItems.Count > 0)
                           {
                               DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                               ddlWagedWorkArea.DataSource = dtWorkArea;
                               ddlWagedWorkArea.DataValueField = "WorkArea";
                               ddlWagedWorkArea.DataTextField = "WorkArea";
                               ddlWagedWorkArea.DataBind();
                               ddlWagedWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                           }

                           SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", lblWagedBusinessUnit.Text);

                           if (oItems1 != null && oItems1.Count > 0)
                           {
                               DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                               ddlWagedSiteLocation.DataSource = dtLocation;
                               ddlWagedSiteLocation.DataValueField = "Location";
                               ddlWagedSiteLocation.DataTextField = "Location";
                               ddlWagedSiteLocation.DataBind();
                               ddlWagedSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                           }


                           if (listitem["ReportsTo"] != null)
                           {
                               string strpplpicker = string.Empty;
                               SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                               for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                               {
                                   strpplpicker = strpplpicker + workers[coworker] + ",";
                               }
                               ReportsToWagedPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                           }

                           txtWagedCostCentre.Text = Convert.ToString(listitem["CostCenter"]);

                           WagedCommencementDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedStartDate"]);
                           WagedTermEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);
                           ddlTypeOfRole.SelectedValue = Convert.ToString(listitem["PositionType"]);


                       }
                   });
        }
        
        private void GetWagedRemunerattionDetails(string strRefno)
        {

            SPListItemCollection RemunarationDetailscollecItems = GetListData("RemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlWagedLevel.SelectedValue = Convert.ToString(ListItems["Level"]);
                ddlVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);

                txtWagedIfOther.Text = Convert.ToString(ListItems["OtherVehicleText"]);
            }
        }
        
        private void GetContractorPositionDetails(string strRefNo)
        {
            dvlblContraBU.Visible = true;
            string appHirePosLstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                       SPQuery appHirePosQuery = new SPQuery();
                       appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefNo + "</Value></Eq></Where>";
                       SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                       foreach (SPListItem listitem in appHirePosItems)
                       {
                           txtContractPosition.Text = Convert.ToString(listitem["PositionTitle"]);
                           //txtContraRole.Text = Convert.ToString(listitem["Role"]);
                           lblContraBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                           SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", lblContraBusinessUnit.Text);

                           if (oItems != null && oItems.Count > 0)
                           {
                               DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                               ddlContraWorkArea.DataSource = dtWorkArea;
                               ddlContraWorkArea.DataValueField = "WorkArea";
                               ddlContraWorkArea.DataTextField = "WorkArea";
                               ddlContraWorkArea.DataBind();
                               ddlContraWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                           }

                           SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", lblContraBusinessUnit.Text);

                           if (oItems1 != null && oItems1.Count > 0)
                           {
                               DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                               ddlContraSiteLocation.DataSource = dtLocation;
                               ddlContraSiteLocation.DataValueField = "Location";
                               ddlContraSiteLocation.DataTextField = "Location";
                               ddlContraSiteLocation.DataBind();
                               ddlContraSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                           }


                           if (listitem["ReportsTo"] != null)
                           {
                               string strpplpicker = string.Empty;
                               SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                               for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                               {
                                   strpplpicker = strpplpicker + workers[coworker] + ",";
                               }
                               ReportsToContractorPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                           }

                           txtContraCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                           txtContractRate.Text = Convert.ToString(listitem["ContractRate"]);

                           ContraStartDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedStartDate"]);
                           ContraEndDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);
                           ddlTypeOfRole.SelectedValue = Convert.ToString(listitem["PositionType"]);


                       }
                   });
        }

        private void GetExpatPositionDetails(string strRefNo)
        {
            dvlblExpatBU.Visible = true;
            string appHirePosLstURL = HrWebUtility.GetListUrl("PositionDetails");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList appHirePosList = SPContext.Current.Site.RootWeb.GetList(appHirePosLstURL);

                       SPQuery appHirePosQuery = new SPQuery();
                       appHirePosQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefNo + "</Value></Eq></Where>";
                       SPListItemCollection appHirePosItems = appHirePosList.GetItems(appHirePosQuery);
                       foreach (SPListItem listitem in appHirePosItems)
                       {
                           txtExpatPositionTitle.Text = Convert.ToString(listitem["PositionTitle"]);
                           //txtContraRole.Text = Convert.ToString(listitem["Role"]);
                           lblExpatBusinessUnit.Text = Convert.ToString(listitem["BusinessUnit"]);

                           SPListItemCollection oItems = GetBusinessUnitWAandLoc("HrWebBusinessUnitWorkarea", lblExpatBusinessUnit.Text);

                           if (oItems != null && oItems.Count > 0)
                           {
                               DataTable dtWorkArea = oItems.GetDataTable().DefaultView.ToTable(true, "WorkArea");
                               ddlExpatWorkArea.DataSource = dtWorkArea;
                               ddlExpatWorkArea.DataValueField = "WorkArea";
                               ddlExpatWorkArea.DataTextField = "WorkArea";
                               ddlExpatWorkArea.DataBind();
                               ddlExpatWorkArea.SelectedValue = Convert.ToString(listitem["WorkArea"]);
                           }

                           SPListItemCollection oItems1 = GetBusinessUnitWAandLoc("HrWebBusinessUnitLocation", lblExpatBusinessUnit.Text);

                           if (oItems1 != null && oItems1.Count > 0)
                           {
                               DataTable dtLocation = oItems1.GetDataTable().DefaultView.ToTable(true, "Location");
                               ddlExpatSiteLocation.DataSource = dtLocation;
                               ddlExpatSiteLocation.DataValueField = "Location";
                               ddlExpatSiteLocation.DataTextField = "Location";
                               ddlExpatSiteLocation.DataBind();
                               ddlExpatSiteLocation.SelectedValue = Convert.ToString(listitem["SiteLocation"]);
                           }


                           if (listitem["ReportsTo"] != null)
                           {
                               string strpplpicker = string.Empty;
                               SPFieldMultiChoiceValue workers = new SPFieldMultiChoiceValue(listitem["ReportsTo"].ToString());
                               for (int coworker = 1; coworker < workers.Count; coworker = coworker + 2)
                               {
                                   strpplpicker = strpplpicker + workers[coworker] + ",";
                               }
                               ReportsToExpatPeopleEditor.CommaSeparatedAccounts = strpplpicker;
                           }

                           txtexpatCostCentre.Text = Convert.ToString(listitem["CostCenter"]);
                           // txtContractRate.Text = Convert.ToString(listitem["ContractRate"]);

                           ExpatEffectiveTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedStartDate"]);
                           ExpatContractDateTimeControl.SelectedDate = Convert.ToDateTime(listitem["ProposedEndDate"]);
                           ddlTypeOfRole.SelectedValue = Convert.ToString(listitem["PositionType"]);


                       }
                   });
        }
        
        private void GetExpatRemunerattionDetails(string strRefno)
        {

            SPListItemCollection RemunarationDetailscollecItems = GetListData("RemunerationDetails", strRefno);
            foreach (SPListItem ListItems in RemunarationDetailscollecItems)
            {
                ddlExpatGrade.SelectedValue = Convert.ToString(ListItems["Grade"]);
                //ddlVehicle.SelectedValue = Convert.ToString(ListItems["Vehicle"]);
                txtExpatFAR.Text = Convert.ToString(ListItems["FAR"]);
                //txtSTI.Text = Convert.ToString(ListItems["STI"]);
                ddlExpatSTI.SelectedValue = Convert.ToString(ListItems["STI"]);
                //txtIfOthers.Text = Convert.ToString(ListItems["OtherVehicleText"]);
            }
        }

        private void UpdateNewHireGeneralInfo(string strStatus)
        {


            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHireGeneralInfo");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                //string strAppHireRefNo = lblReferenceNo.Text;
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'RefNo\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);
                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }

                ////web.AllowUnsafeUpdates = true;

                listitem["Title"] = txtFirstName.Text.Trim();
                listitem["LastName"] = txtLastName.Text.Trim();
                listitem["Address"] = txtAddress.Text.Trim();

                listitem["City"] = txtCity.Text.Trim();
                listitem["State"] = txtState.Text.Trim();
                listitem["PostCode"] = txtPostCode.Text.Trim();

                if (!string.IsNullOrEmpty(lblDateOfRequest.Text.Trim()))
                    listitem["Date"] = Convert.ToDateTime(lblDateOfRequest.Text).ToString("dd/MM/yyyy");

                if (ddlRef.Items.Count > 1 && !string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
                    listitem["AppToHireRefNo"] = ddlRef.SelectedItem.Text;

                if (dvlblPostionType.Visible)
                    listitem["PositionType"] = lblPositionType.Text;
                else
                    listitem["PositionType"] = ddlPositionType.SelectedItem.Text;

                listitem["Role"] = ddlTypeOfRole.SelectedValue;
                listitem["Status"] = strStatus;
                listitem["ApprovalStatus"] = "HRManager";
                listitem["RefNo"] = lblReferenceNo.Text;
                listitem.Update();

                //web.AllowUnsafeUpdates = false;
            });
        }
        
        private void UpdatePositionDetailsList()
        {


            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");

                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                //string strAppHireRefNo = lblReferenceNo.Text;
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);
                SPWeb web = SPContext.Current.Web;
                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }

                //web.AllowUnsafeUpdates = true;
                //listitem["Title"] = lblReferenceNo.Text;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["PositionTitle"] = txtPositionTitle.Text;
                if (dvlblBU.Visible)
                    listitem["BusinessUnit"] = lblBusinessUnit.Text;
                else
                    listitem["BusinessUnit"] = ddlBusinessUnit.SelectedItem.Text;
                listitem["WorkArea"] = ddlWorkArea.SelectedValue;

                listitem["SiteLocation"] = ddlSiteLocation.SelectedValue;


                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                SPFieldUserValueCollection SignUserCollection = new SPFieldUserValueCollection();
                string[] SignUsersSeperated = SignLetterPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in SignUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        SignUserCollection.Add(UserName);
                    }
                }
                listitem["WhoSignLetter"] = SignUserCollection;
                listitem["Notes"] = txtNotes.Text;
                listitem["NextSalaryReview"] = txtNewSalaryReview.Text;
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtCostCentre.Text;
                listitem["ContractType"] = ddlContractType.SelectedValue;
                listitem["PositionType"] = lblPositionType.Text;

                if (!CommencementDateTimeControl.IsDateEmpty)
                    listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(CommencementDateTimeControl.SelectedDate);

                if (!TermEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(TermEndDateTimeControl.SelectedDate);

                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }
        
        private void UpdateRemunerationDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oQuery = new SPQuery();
                //string strAppHireRefNo = lblReferenceNo.Text;
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";

                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }

                //web.AllowUnsafeUpdates = true;
                //listitem["Title"] = lblReferenceNo.Text;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["Grade"] = ddlGrade.SelectedValue;
                listitem["FAR"] = txtFAR.Text;
                listitem["STI"] = ddlSTI.SelectedValue;
                listitem["Vehicle"] = ddlVehicle.SelectedValue;
                listitem["IfOthersSpecify"] = txtIfOthers.Text;
                listitem["Relocation"] = ddlRelocation.SelectedValue;
                listitem["RelocationDetails"] = txtRelocationDet.Text;
                listitem.Update();
                //web.AllowUnsafeUpdates = false;
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

        private void UpdateWagedDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oQuery = new SPQuery();
                //string strAppHireRefNo = lblReferenceNo.Text;
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }
                SPWeb web = SPContext.Current.Web;
                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["PositionTitle"] = txtWagedPositionTitle.Text;
                if (dvlblWagedBU.Visible)
                    listitem["BusinessUnit"] = lblWagedBusinessUnit.Text;
                else
                    listitem["BusinessUnit"] = ddlWagedBusinessUnit.SelectedItem.Text;

                listitem["WorkArea"] = ddlWagedWorkArea.SelectedValue;

                listitem["SiteLocation"] = ddlWagedSiteLocation.SelectedValue;


                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToWagedPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtWagedCostCentre.Text;
                listitem["PositionType"] = lblPositionType.Text;

                if (!WagedCommencementDateTimeControl.IsDateEmpty)
                    listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(WagedCommencementDateTimeControl.SelectedDate);


                if (!WagedTermEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(WagedTermEndDateTimeControl.SelectedDate);


                SPFieldUserValueCollection SignUserCollection = new SPFieldUserValueCollection();
                string[] SignUsersSeperated = WagedSignPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in SignUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        SignUserCollection.Add(UserName);
                    }
                }
                listitem["WhoSignLetter"] = SignUserCollection;
                listitem["Notes"] = txtWagedNotes.Text;

                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }
        
        private void UpdateWagedRemunerationDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oQuery = new SPQuery();
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }
                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["Level"] = ddlWagedLevel.SelectedValue;
                listitem["RosterType"] = ddlWagedRosterType.SelectedValue;
                listitem["Crew"] = txtWagedCrew.Text;
                listitem["ShiftTeamLeader"] = txtWagedShiftTream.Text;
                listitem["Allowances"] = txtWagedAllowances.Text;
                listitem["Vehicle"] = ddlWagedVehicle.SelectedValue;
                listitem["IfOthersSpecify"] = txtWagedIfOther.Text;

                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }

        private void UpdateContractPositionDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";

                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }
                SPWeb web = SPContext.Current.Web;
                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["PositionTitle"] = txtContractPosition.Text;
                listitem["CompanyTradingName"] = txtContractCompany.Text;
                listitem["ABN"] = txtContractABN.Text;
                // listitem["Role"] = txtContraRole.Text;
                if (dvlblContraBU.Visible)
                    listitem["BusinessUnit"] = lblContraBusinessUnit.Text;
                else
                    listitem["BusinessUnit"] = ddlContraBusinessUnit.SelectedItem.Text;
                listitem["WorkArea"] = ddlWorkArea.SelectedValue;

                listitem["SiteLocation"] = ddlContraSiteLocation.SelectedValue;


                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToContractorPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["CostCenter"] = txtContraCostCentre.Text;
                listitem["ContractRate"] = txtContractRate.Text;
                listitem["PositionType"] = lblPositionType.Text;
                listitem["PaymentTerms"] = ddlContractPaymentTerms.SelectedValue;
                listitem["IfOtherSpecify"] = txtContractOthers.Text;
                listitem["GST"] = ddlContractGST.SelectedValue;
                listitem["RateTypeField"] = ddlRateTypeField.SelectedValue;

                SPFieldUserValueCollection SignUserCollection = new SPFieldUserValueCollection();
                string[] SignUsersSeperated = ContractSignPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in SignUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        SignUserCollection.Add(UserName);
                    }
                }
                listitem["WhoSignLetter"] = SignUserCollection;
                if (!ContraStartDateTimeControl.IsDateEmpty)
                    listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ContraStartDateTimeControl.SelectedDate);


                if (!ContraEndDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ContraEndDateTimeControl.SelectedDate);


                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }
       
        private void UpdateContractRemunerationDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }

                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;

                listitem["ServicesProvided"] = txtContraRoleStatement.Text;

                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }

        private void UpdateExpatPositionDetailsList()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHirePositionDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }
                SPWeb web = SPContext.Current.Web;
                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["CostCenter"] = txtexpatCostCentre.Text;
                listitem["PositionTitle"] = txtExpatPositionTitle.Text;

                if (dvlblExpatBU.Visible)
                    listitem["BusinessUnit"] = lblExpatBusinessUnit.Text;
                else
                    listitem["BusinessUnit"] = ddlExpatBusinessUnit.SelectedItem.Text;

                listitem["WorkArea"] = ddlExpatWorkArea.SelectedValue;

                listitem["SiteLocation"] = ddlExpatSiteLocation.SelectedValue;


                SPFieldUserValueCollection ReportsToUserCollection = new SPFieldUserValueCollection();
                string[] reqdUsersSeperated = ReportsToExpatPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in reqdUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        ReportsToUserCollection.Add(UserName);
                    }
                }
                listitem["ReportsTo"] = ReportsToUserCollection;
                listitem["PositionType"] = lblPositionType.Text;
                listitem["ContractPeriod"] = txtContractPeriods.Text;
                listitem["NextSalaryReview"] = txtExpatNextReview.Text;
                listitem["HomeLocation"] = txtExpatHomeLocation.Text;

                if (!ExpatEffectiveTimeControl.IsDateEmpty)
                    listitem["CommencementDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ExpatEffectiveTimeControl.SelectedDate);

                if (!ExpatContractDateTimeControl.IsDateEmpty)
                    listitem["ProposedEndDate"] = SPUtility.CreateISO8601DateTimeFromSystemDateTime(ExpatContractDateTimeControl.SelectedDate);

                SPFieldUserValueCollection SignUserCollection = new SPFieldUserValueCollection();
                string[] SignUsersSeperated = ExpatSignPeopleEditor.CommaSeparatedAccounts.Split(',');
                foreach (string UserSeperated in SignUsersSeperated)
                {
                    if (!string.IsNullOrEmpty(UserSeperated))
                    {
                        SPUser User = web.SiteUsers[UserSeperated];
                        SPFieldUserValue UserName = new SPFieldUserValue(web, User.ID, User.LoginName);
                        SignUserCollection.Add(UserName);
                    }
                }
                listitem["WhoSignLetter"] = SignUserCollection;
                listitem["Notes"] = txtExpatNotes.Text;

                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }
       
        private void UpdateExpatRemunerationDetailsList()
        {



            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string lstURL = HrWebUtility.GetListUrl("NewHireRemunerationDetails");
                SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                SPQuery oQuery = new SPQuery();
                string strAppHireRefNo = lblReferenceNo.Text;
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strAppHireRefNo + "</Value></Eq></Where>";


                SPListItemCollection oItems = oList.GetItems(oQuery);

                SPListItem listitem = null;
                if (oItems != null && oItems.Count > 0)
                {
                    listitem = oItems[0];
                }
                else
                {
                    listitem = oList.AddItem();
                }
                //web.AllowUnsafeUpdates = true;
                listitem["Title"] = lblReferenceNo.Text;
                listitem["Grade"] = ddlExpatGrade.SelectedValue;
                listitem["FAR"] = txtExpatFAR.Text;
                listitem["STI"] = ddlExpatSTI.SelectedValue;
                //listitem["Vehicle"] = ddlVehicle.SelectedValue;
                // listitem["MaritalStatus"] = ddlMaritalStatus.SelectedValue;

                //  listitem["Allowances"] = txtLocationAllow.Text;


                listitem.Update();
                //web.AllowUnsafeUpdates = false;
            });
        }

        private void UpdateExpatPersonnelDetails()
        {
            if (strRefno == "")
                strRefno = lblReferenceNo.Text;

            if (ViewState["vwDependentTbl"] != null)
            {
                DataTable dtDependent = (DataTable)ViewState["vwDependentTbl"];

                string lstURL = HrWebUtility.GetListUrl("NewHirePersonnelDetails");
                SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList oList = SPContext.Current.Site.RootWeb.GetList(lstURL);

                       SPQuery oQuery = new SPQuery();
                       oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";

                       SPListItemCollection oItems = oList.GetItems(oQuery);
                       StringBuilder deletebuilder = BatchCommand(oList.ID.ToString(), oItems);
                       SPContext.Current.Site.RootWeb.ProcessBatchData(deletebuilder.ToString());

                       if (dtDependent.Rows.Count > 0)
                       {

                           for (int rowCnt = 0; rowCnt < dtDependent.Rows.Count; rowCnt++)
                           {
                               SPListItem listitem = oList.AddItem();
                               SPWeb web = SPContext.Current.Web;
                               //web.AllowUnsafeUpdates = true;

                               listitem["Title"] = lblReferenceNo.Text;
                               listitem["MaritalStatus"] = ddlMaritalStatus.SelectedValue;
                               listitem["Dependent"] = rowCnt + 1;
                               listitem["Name"] = Convert.ToString(dtDependent.Rows[rowCnt]["Name"]);
                               listitem["DOB"] = Convert.ToString(dtDependent.Rows[rowCnt]["DOB"]);

                               listitem.Update();
                               //web.AllowUnsafeUpdates = false;


                           }
                       }
                   });
            }
        }

        private bool ValidateSummary()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtLastName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(txtCity.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtState.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtPostCode.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlTypeOfRole.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtPositionTitle.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;



            if (CommencementDateTimeControl.IsDateEmpty)
                bresult = false;


            // if (TermEndDateTimeControl.IsDateEmpty)
            if (string.Equals(ddlTypeOfRole.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && TermEndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (SignLetterPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(ddlGrade.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtFAR.Text.Trim()))
                bresult = false;


            return bresult;

        }

        private bool ValidateWaged()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtLastName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(txtCity.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtState.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtPostCode.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlTypeOfRole.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedPositionTitle.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedCostCentre.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(ddlWagedWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToWagedPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;



            if (WagedCommencementDateTimeControl.IsDateEmpty)
                bresult = false;


            //if (WagedTermEndDateTimeControl.IsDateEmpty)
            if (string.Equals(ddlTypeOfRole.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && WagedTermEndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (WagedSignPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedLevel.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlWagedRosterType.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedCrew.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedShiftTream.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtWagedAllowances.Text.Trim()))
                bresult = false;


            return bresult;

        }

        private bool ValidateContractor()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtLastName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(txtCity.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtState.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtPostCode.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlTypeOfRole.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtContractCompany.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtContractABN.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlContraSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToContractorPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (string.IsNullOrEmpty(txtContraCostCentre.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtContractRate.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlRateTypeField.SelectedValue))
                bresult = false;

            if (ContraStartDateTimeControl.IsDateEmpty)
                bresult = false;

            //if (string.Equals(ddlTypeOfRole.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && ContraEndDateTimeControl.IsDateEmpty)
            if (ContraEndDateTimeControl.IsDateEmpty)
                bresult = false;

            if (ContractSignPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;




            Table tblAttachement = (Table)MyCustomControl.FindControl("tblAttachment");
            if (tblAttachement.Rows.Count <= 1)
                bresult = false;

            return bresult;

        }

        private bool ValidateExpat()
        {
            bool bresult = true;

            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtLastName.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
                bresult = false;


            if (string.IsNullOrEmpty(txtCity.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtState.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtPostCode.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlTypeOfRole.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtExpatPositionTitle.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatWorkArea.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(ddlExpatSiteLocation.SelectedValue))
                bresult = false;

            if (ReportsToExpatPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;

            if (ExpatEffectiveTimeControl.IsDateEmpty)
                bresult = false;

            if (string.IsNullOrEmpty(txtContractPeriods.Text.Trim()))
                bresult = false;

            //if (ExpatContractDateTimeControl.IsDateEmpty)
            if (string.Equals(ddlTypeOfRole.SelectedValue, "Fixed Term", StringComparison.OrdinalIgnoreCase) && ExpatContractDateTimeControl.IsDateEmpty)
                bresult = false;

            if (string.IsNullOrEmpty(txtExpatNextReview.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(txtExpatHomeLocation.Text.Trim()))
                bresult = false;

            /*if (SignLetterPeopleEditor.CommaSeparatedAccounts.Length <= 0)
                bresult = false;*/

            if (string.IsNullOrEmpty(ddlExpatGrade.SelectedValue))
                bresult = false;

            if (string.IsNullOrEmpty(txtExpatFAR.Text.Trim()))
                bresult = false;

            if (string.IsNullOrEmpty(ddlMaritalStatus.SelectedValue))
                bresult = false;


            return bresult;

        }

        protected void btnContractorSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateContractor())
                {
                    string bunit = string.Empty;
                    if (dvdrpContraBU.Visible)
                        bunit = ddlContraBusinessUnit.SelectedValue;
                    else
                        bunit = lblContraBusinessUnit.Text;
                    string Approver = GetApprover(bunit);
                    if (Approver != "")
                    {
                        UpdateNewHireGeneralInfo("Pending Approval");
                        UpdateContractPositionDetailsList();
                        UpdateContractRemunerationDetailsList();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the mandatory fields";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToContraTab();", true);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnContractorSubmit_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        protected void btnSalarySubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateSummary())
                {
                    string bunit = string.Empty;
                    if (dvdrpBU.Visible)
                        bunit = ddlBusinessUnit.SelectedValue;
                    else
                        bunit = lblBusinessUnit.Text;
                    string Approver = GetApprover(bunit);
                    if (Approver != "")
                    {
                        UpdateNewHireGeneralInfo("Pending Approval");
                        UpdatePositionDetailsList();
                        UpdateRemunerationDetailsList();
                        //UpdateOfferChecklists();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the mandatory fields";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToSalTab();", true);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnSalarySubmit_Click", ex.Message);
                //lblError.Text = "Unexpected error has occured. Please contact IT team.";
                lblError.Text = ex.Message;
                divmain.Visible = false;
            }
        }

        protected void btnWagedSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateWaged())
                {
                    string bunit = string.Empty;
                    if (dvdrpWagedBU.Visible)
                        bunit = ddlWagedBusinessUnit.SelectedValue;
                    else
                        bunit = lblWagedBusinessUnit.Text;
                    string Approver = GetApprover(bunit);
                    if (Approver != "")
                    {
                        UpdateNewHireGeneralInfo("Pending Approval");
                        UpdateWagedDetailsList();
                        UpdateWagedRemunerationDetailsList();
                        // UpdateWagedOfferChecklists();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the mandatory fields";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToWagedTab();", true);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnWagedSubmit_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        protected void btnExpatSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateExpat())
                {
                    string bunit = string.Empty;
                    if (dvdrpExpatBU.Visible)
                        bunit = ddlExpatBusinessUnit.SelectedValue;
                    else
                        bunit = lblExpatBusinessUnit.Text;
                    string Approver = GetApprover(bunit);
                    if (Approver != "")
                    {
                        UpdateNewHireGeneralInfo("Pending Approval");
                        UpdateExpatPositionDetailsList();
                        UpdateExpatRemunerationDetailsList();
                        // UpdateExpatOfferChecklists();
                        UpdateExpatPersonnelDetails();
                        SendEmail();
                        Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Submit");
                    }
                    else
                    {
                        lblError.Text = "The application cannot be submitted for processing as there are no approvers configured for the chosen business unit.";
                    }
                }
                else
                {
                    lblError.Text = "Please fill all the mandatory fields";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnExpatSubmit_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }

        }

        protected void btnSalarySave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text;
                UpdateNewHireGeneralInfo("Draft");
                UpdatePositionDetailsList();
                UpdateRemunerationDetailsList();
                // UpdateOfferChecklists();
                Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + refno + "&flow=Draft");
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnSalarySave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        protected void btnWagedSave_Click(object sender, EventArgs e)
        {
            try
            {
                string refno = lblReferenceNo.Text;
                UpdateNewHireGeneralInfo("Draft");
                UpdateWagedDetailsList();
                UpdateWagedRemunerationDetailsList();
                // UpdateWagedOfferChecklists();
                Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + refno + "&flow=Draft");

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnWagedSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        protected void btnContractorSave_Click(object sender, EventArgs e)
        {
            try
            {

                UpdateNewHireGeneralInfo("Draft");
                UpdateContractPositionDetailsList();
                UpdateContractRemunerationDetailsList();
                Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Draft");

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnContractorSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        protected void btnExpatSave_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateNewHireGeneralInfo("Draft");
                UpdateExpatPositionDetailsList();
                UpdateExpatRemunerationDetailsList();
                // UpdateExpatOfferChecklists();
                UpdateExpatPersonnelDetails();
                Server.Transfer("/people/Pages/HRWeb/NewHireStatus.aspx?refno=" + lblReferenceNo.Text + "&flow=Draft");

            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.btnExpatSave_Click", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
                divmain.Visible = false;
            }
        }

        private void SendEmail()
        {
            string strRefNo = lblReferenceNo.Text;

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
                                "<FieldRef Name='ApprovalMessage' />");
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

                        strTo = Convert.ToString(ViewState["ApproverEmail"]);
                        string positiontype = string.Empty;
                        string positiontitle = string.Empty;

                        if (dvlblPostionType.Visible)
                            positiontype = lblPositionType.Text;
                        else
                            positiontype = ddlPositionType.SelectedItem.Text;

                        if (positiontype == "Salary")
                        {
                            positiontitle = txtPositionTitle.Text.Trim();
                        }
                        else if (positiontype == "Waged")
                        {
                            positiontitle = txtWagedPositionTitle.Text.Trim();
                        }
                        else if (positiontype == "Contractor")
                        {
                            positiontitle = txtContractPosition.Text.Trim();
                        }
                        else if (positiontype == "Expatriate")
                        {
                            positiontitle = txtExpatPositionTitle.Text.Trim();
                        }


                        SmtpClient smtpClient = new SmtpClient();
                        smtpClient.Host = Convert.ToString(itm["EmailIP"]);
                        smtpClient.Port = 25;
                        //smtpClient.Host = "smtp.gmail.com";
                        string url = site.Url + "/pages/hrweb/newhirereview.aspx?refno=" + strRefNo;
                        strFrom = Convert.ToString(itm["Title"]);



                        string[] tmparr = strTo.Split('|');
                        strTo = tmparr[tmparr.Length - 1];
                        if (strTo.Contains("#"))
                            strTo = strTo.Split('#')[1];

                        strMessage = Convert.ToString(itm["ApprovalMessage"]).Replace("&lt;REFNO&gt;", strRefNo).
                            Replace("&lt;WORKFLOWPAGE&gt;", "<a href='" + url + "'>here</a>").Replace("&lt;POSTITLE&gt;", positiontitle).
                                Replace("&lt;NAME&gt;", txtFirstName.Text.Trim() + " " + txtLastName.Text.Trim());
                        strSubject = Convert.ToString(itm["ApprovalSubject"]).Replace("<REFNO>", strRefNo).Replace("\r\n", "").
                                Replace("<NAME>", txtFirstName.Text.Trim() + " " + txtLastName.Text.Trim()).Replace("<POSTITLE>", positiontitle);

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

        private string GetApprover(string businessunit)
        {
            string Approver = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("NewHireApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
                   {
                       SPList olist1 = SPContext.Current.Site.RootWeb.GetList(lstURL);
                       SPQuery oquery3 = new SPQuery();
                       // EQ operator should be used instead of Contains. Contains wont work properly in case of P&P related BUs
                       oquery3.Query = "<Where><Eq><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                           "</Value></Eq></Where>";
                       //oquery3.Query = "<Where><Contains><FieldRef Name=\'BusinessUnit\' /><Value Type=\"Text\">" + businessunit +
                       //    "</Value></Contains></Where>";
                       SPListItemCollection collitems2 = olist1.GetItems(oquery3);
                       if (collitems2.Count > 0)
                       {
                           Approver = Convert.ToString(collitems2[0]["Approver"]);
                           ViewState["ApproverEmail"] = Approver;
                       }
                   });
            return Approver;
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

        protected void ddlRef_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateChoiceFields();

            if (!string.Equals(ddlRef.SelectedValue, "New Hire", StringComparison.OrdinalIgnoreCase))
            {
                dvlblBU.Visible = true;
                dvdrpBU.Visible = false;

                dvlblContraBU.Visible = true;
                dvdrpContraBU.Visible = false;

                dvlblWagedBU.Visible = true;
                dvdrpWagedBU.Visible = false;

                dvlblExpatBU.Visible = true;
                dvdrpExpatBU.Visible = false;

                dvlblPostionType.Visible = true;
                dvdrpPostionType.Visible = false;

                if (ddlRef.Items != null && ddlRef.Items.Count > 1)
                {
                    lblPositionType.Text = GetPositionType(ddlRef.SelectedItem.Text);
                    // lblReferenceNo.Text = ddlRef.SelectedItem.Text;

                    if (string.Equals(lblPositionType.Text, "Salary", StringComparison.OrdinalIgnoreCase))
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToSalTab();", true);
                        GetSalaryPositionDetails(ddlRef.SelectedItem.Text);
                        GetSalaryRemunerattionDetails(ddlRef.SelectedItem.Text);
                    }
                    else if (string.Equals(lblPositionType.Text, "Expatriate", StringComparison.OrdinalIgnoreCase))
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToExpatTab();", true);
                        GetExpatPositionDetails(ddlRef.SelectedItem.Text);
                        GetExpatRemunerattionDetails(ddlRef.SelectedItem.Text);
                    }
                    else if (string.Equals(lblPositionType.Text, "Waged", StringComparison.OrdinalIgnoreCase))
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToWagedTab();", true);
                        GetWagedPositionDetails(ddlRef.SelectedItem.Text);
                        GetWagedRemunerattionDetails(ddlRef.SelectedItem.Text);
                    }
                    else if (string.Equals(lblPositionType.Text, "Contractor", StringComparison.OrdinalIgnoreCase))
                    {
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "MoveNextTab", "MoveToContraTab();", true);
                        GetContractorPositionDetails(ddlRef.SelectedItem.Text);
                        // GetcontrRemunerattionDetails(ddlRef.SelectedItem.Text);
                    }
                }
            }
            else
            {
                dvlblBU.Visible = false;
                dvdrpBU.Visible = true;

                dvlblContraBU.Visible = false;
                dvdrpContraBU.Visible = true;

                dvlblWagedBU.Visible = false;
                dvdrpWagedBU.Visible = true;

                dvlblExpatBU.Visible = false;
                dvdrpExpatBU.Visible = true;

                dvlblPostionType.Visible = false;
                dvdrpPostionType.Visible = true;
            }
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
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.ddlBusinessUnit_SelectedIndexChanged", ex.Message);
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
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
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.ddlWagedBusinessUnit_SelectedIndexChanged", ex.Message);
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
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.ddlContraBusinessUnit_SelectedIndexChanged", ex.Message);
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
                LogUtility.LogError("HRWebForms.HRWeb.NewHireRequest.ddlExpatBusinessUnit_SelectedIndexChanged", ex.Message);
                //lblError.Text ="Unexpected error has occured. Please contact IT team.";
                lblError.Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        protected void ddlPositionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblPositionType.Text = ddlPositionType.SelectedValue;
        }

    }

}