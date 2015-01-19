
using Microsoft.SharePoint;
using System;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HRWebForms.ControlTemplates.HRWebForms
{

    public partial class UploadJobUserControl : UserControl
    {

        string strRefno = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string tmpref = ((Label)this.Parent.FindControl("lblReferenceNo")).Text;
                if (tmpref != "")
                {
                    strRefno = tmpref;
                    //strRefno = ((Label)this.Parent.FindControl("lblReferenceNo")).Text;
                   /* string[] strSplit = tmpref.Split(':');
                    if (strSplit.Length > 0)
                        strRefno = tmpref.Split(':')[1].Trim();
                    else
                        strRefno = tmpref;*/

                }
                UpdateFileUpload();
                DataTable dt = (DataTable)ViewState["AttachmentInfo"];
                if (dt != null)
                {
                    PopulateAttachmentTable(dt);
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("HRWebForms.HRWeb.AppToHireRequest.Page_Load", ex.Message);
                ((Label)this.Parent.FindControl("lblError")).Text = "Unexpected error has occured. Please contact IT team.";
            }
        }

        private void AddHeaderToTable()
        {
            TableRow tRow = new TableRow();
            TableCell tCellchk = new TableCell();
            tCellchk.Width = 5;
            tRow.Cells.Add(tCellchk);

            TableCell tCell = new TableCell();
            tCell.Attributes.Add("font-weight", "bold");
            tCell.Text = "File Type";
            tCell.Width = 20;
            tRow.Cells.Add(tCell);

            TableCell tCell1 = new TableCell();
            tCell1.Attributes.Add("font-weight", "bold");
            tCell1.Text = "File Name";
            tCell1.Width = 55;
            tRow.Cells.Add(tCell1);

            TableCell tCell2 = new TableCell();
            tCell2.Attributes.Add("font-weight", "bold");
            tCell2.Text = "Modified";
            tCell2.Width = 20;
            tRow.Cells.Add(tCell2);

            TableCell tCell3 = new TableCell();
            tCell3.Text = "ID";
            tCell3.Visible = false;
            tRow.Cells.Add(tCell3);

            tblAttachment.Rows.Add(tRow);
        }

        private void UpdateFileUpload()
        {
            tblAttachment.Rows.Clear();
            AddHeaderToTable();

            Label lblReferenceNo = this.Parent.FindControl("lblReferenceNo") as Label;
            strRefno = lblReferenceNo.Text;
            if (strRefno != "")
            {
                //strRefno = strRefno.Split(':')[1].Trim();
                SPList oList = SPContext.Current.Web.Lists["JobDetails"];
                SPQuery oQuery = new SPQuery();
                oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + strRefno + "</Value></Eq></Where>";
                oQuery.RowLimit = 100;
                SPListItemCollection collectionItems = oList.GetItems(oQuery);
                for (int cnt = 0; cnt <= collectionItems.Count - 1; cnt++)
                {

                    TableRow tRow = new TableRow();
                    TableCell tCellchk = new TableCell();
                    CheckBox cb = new CheckBox();
                    cb.ID = "DynamicChkbox" + cnt;

                    tCellchk.Controls.Add(cb);
                    tRow.Cells.Add(tCellchk);

                    TableCell tCell = new TableCell();
                    tCell.Text = Convert.ToString(collectionItems[cnt]["Type"]);
                    tRow.Cells.Add(tCell);

                    TableCell tCell1 = new TableCell();
                    Label lblName = new Label();
                    lblName.ID = "lblName" + cnt;
                    lblName.Text = Convert.ToString(collectionItems[cnt]["Name"]);
                    tCell1.Controls.Add(lblName);
                    tRow.Cells.Add(tCell1);

                    TableCell tCell2 = new TableCell();
                    DateTime validDate = (DateTime)collectionItems[cnt]["Modified"];
                    tCell2.Text = validDate.ToString("dd/MM/yyyy");
                    tRow.Cells.Add(tCell2);

                    TableCell tCell3 = new TableCell();
                    tCell3.Text = Convert.ToString(collectionItems[cnt]["ID"]);
                    tCell3.Visible = false;
                    tRow.Cells.Add(tCell3);

                    tblAttachment.Rows.Add(tRow);
                }
            }
        }



        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                //for (int cnt = tblAttachment.Rows.Count; cnt >= 0; cnt--)
                for (int cnt = 1; cnt < tblAttachment.Rows.Count; cnt++)
                {
                    CheckBox chkboxControl = (CheckBox)tblAttachment.Rows[cnt].Cells[0].FindControl("DynamicChkbox" + (cnt - 1).ToString());

                    if (chkboxControl != null)
                    {
                        if (chkboxControl.Checked)
                        {
                            string id = tblAttachment.Rows[cnt].Cells[4].Text;
                            if (id != "")
                            {
                                SPSecurity.RunWithElevatedPrivileges(delegate()
                                {
                                    SPWeb web = SPContext.Current.Web;
                                    SPDocumentLibrary oLibrary = web.Lists["JobDetails"] as SPDocumentLibrary;
                                    SPQuery oQuery = new SPQuery();
                                    oQuery.Query = "<Where><Eq><FieldRef Name=\'ID\'/><Value Type=\"Number\">" + id + "</Value></Eq></Where>";
                                    oQuery.RowLimit = 100;
                                    SPListItemCollection collectionItems = oLibrary.GetItems(oQuery);
                                    if (collectionItems != null && collectionItems.Count > 0)
                                    {
                                        SPListItem item = collectionItems[0];
                                        web.AllowUnsafeUpdates = true;
                                        item.Delete();
                                    }
                                    //item.Update();
                                });

                            }
                        }
                    }
                }
                UpdateFileUpload();
            }
            catch (Exception ex)
            {
                LogUtility.LogError("UploadJobUserControl.btnAdd_Click", ex.Message);
                ((Label)this.Parent.FindControl("lblError")).Text = ex.Message;
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Label lblReferenceNo = this.Parent.FindControl("lblReferenceNo") as Label;
                strRefno = lblReferenceNo.Text;
                if (HrWebFileUpload.HasFile)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SPWeb web = SPContext.Current.Web;
                        SPDocumentLibrary oLibrary = web.Lists["JobDetails"] as SPDocumentLibrary;

                        Stream fStream = HrWebFileUpload.PostedFile.InputStream;

                        byte[] contents = new byte[fStream.Length];

                        fStream.Read(contents, 0, (int)fStream.Length);

                        string[] filename = HrWebFileUpload.FileName.Split('.');
                        string fileextn = "."+filename[filename.Length - 1];
                        string sfile = string.Empty;
                        for (int i = 0; i < filename.Length - 1; i++)
                        {
                            sfile += filename[i];
                        }

                        string fileUrl = oLibrary.RootFolder.Url + "/" + sfile + "_" + strRefno+fileextn;

                        bool IsOverwriteFile = true;
                        SPFile file = oLibrary.RootFolder.Files.Add(fileUrl, fStream, IsOverwriteFile);

                        SPListItem item = file.Item;
                        item["Title"] = strRefno;
                        item.Update();
                        file.Update();
                        fStream.Close();
                    });
                    UpdateFileUpload();
                }
            }
            catch (Exception ex)
            {
                LogUtility.LogError("UploadJobUserControl.btnAdd_Click", ex.Message);
                ((Label)this.Parent.FindControl("lblError")).Text = "Unexpected error has occured. Please contact IT team.";

            }

        }

        private void PopulateAttachmentTable(DataTable dt)
        {
            for (int cnt = tblAttachment.Rows.Count - 1; cnt == 1; cnt++)
            {
                tblAttachment.Rows.RemoveAt(cnt);
            }

            for (int cnt = 0; cnt < dt.Rows.Count; cnt++)
            {
                TableRow tRow = new TableRow();
                TableCell tCellchk = new TableCell();
                CheckBox cb = new CheckBox();
                cb.ID = "DynamicChkbox" + cnt;

                tCellchk.Controls.Add(cb);
                tRow.Cells.Add(tCellchk);

                TableCell tCell = new TableCell();

                tCell.Text = Convert.ToString(dt.Rows[cnt]["FileType"]);
                tRow.Cells.Add(tCell);

                TableCell tCell1 = new TableCell();
                tCell1.Text = Convert.ToString(dt.Rows[cnt]["FileName"]);
                tRow.Cells.Add(tCell1);

                TableCell tCell2 = new TableCell();

                tCell2.Text = Convert.ToString(dt.Rows[cnt]["ModifiedDate"]);
                tRow.Cells.Add(tCell2);

                TableCell tCell3 = new TableCell();
                tCell3.Text = Convert.ToString(dt.Rows[cnt]["PostedFile"]);
                tCell3.Visible = false;
                tRow.Cells.Add(tCell3);

                tblAttachment.Rows.Add(tRow);
            }
        }
    }
}
