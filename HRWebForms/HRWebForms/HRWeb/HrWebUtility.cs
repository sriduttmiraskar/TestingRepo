using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace HRWebForms.HRWeb
{
    static class HrWebUtility
    {
        public static string GetUser(string strAuthor)
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

        public static string GetUserByEmailID(string strAuthor)
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

        public static string GetDistributionEmail(string group)
        {
            string DistributionEmail = string.Empty;
            string lstURL = HrWebUtility.GetListUrl("HrWebTerminationOtherApprovalInfo");
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPList olist = SPContext.Current.Site.RootWeb.GetList(lstURL);
                SPQuery oquery = new SPQuery();
                oquery.Query = "<Where><Eq><FieldRef Name=\'BusinessType\'/><Value Type=\"Text\">" + group + "</Value></Eq></Where>";
                oquery.RowLimit = 100;
                SPListItemCollection collitems = olist.GetItems(oquery);
                SPListItem listitem = collitems[0];

                DistributionEmail = Convert.ToString(listitem["DistributionEmail"]);
            });
            return DistributionEmail;
        }

        public static string GetListUrl(string listname)
        {
            string listurl = string.Empty;
            SPList olist = SPContext.Current.Web.Lists["HRWebListUrl"];
            SPQuery oQuery = new SPQuery();
            oQuery.Query = "<Where><Eq><FieldRef Name=\'Title\'/><Value Type=\"Text\">" + listname + "</Value></Eq></Where>";
            oQuery.ViewFields = string.Concat(
                                "<FieldRef Name='Title' />",
                                "<FieldRef Name='ListURL' />");


            SPListItemCollection oItems = olist.GetItems(oQuery);
            if (oItems != null && oItems.Count > 0)
            {
                listurl = Convert.ToString(oItems[0]["ListURL"]);
            }
            return listurl;
        }
    }
}
