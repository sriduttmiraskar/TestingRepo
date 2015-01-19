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

namespace HRWebForms.HRWeb
{
    public partial class TravelStatus : WebPartPage
    {

        string strRefno = string.Empty;
        string strflow = string.Empty;
        protected void page_load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    if (Page.Request.QueryString["refno"] != null)
                        strRefno = Page.Request.QueryString["refno"];

                    if (Page.Request.QueryString["flow"] != null)
                        strflow = Page.Request.QueryString["flow"];

                    if (strflow == "Submit")
                    {
                        lblMessage.Text = "Your application has been submitted for processing. Please click <a href='/people/Pages/HRWeb/TravelRequest.aspx'>here</a> to create a new Travel Authority.";
                    }
                    else if (strflow == "Draft")
                    {
                        lblMessage.Text = "Your application has been saved temporarily. Please click <a href='/people/Pages/HRWeb/TravelRequest.aspx'>here</a> to create a new Travel Authority.";
                    }
                }
            }
            catch
            {
            }
        }
    }
}