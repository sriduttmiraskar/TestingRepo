using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;

namespace HRWebForms.Features.ConfigMod
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("a486498f-f990-4e09-ab4f-334ee75b2997")]
    public class ConfigModEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWebApplication webApplication = properties.Feature.Parent as SPWebApplication;


            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPWebConfigModification mod = new SPWebConfigModification();

                mod.Path = "configuration/SharePoint/SafeMode/PageParserPaths";
                mod.Name = "PageParserPath[@VirtualPath='/*']";
                mod.Owner = "GrowerCustomPage";
                mod.Sequence = 0;
                mod.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
                mod.Value = "<PageParserPath VirtualPath='/*' CompilationMode='Always' AllowServerSideScript='true' IncludeSubFolders='true' />";

                webApplication.WebConfigModifications.Add(mod);
                webApplication.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                webApplication.Update();
            });

        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPWebApplication webApplication = properties.Feature.Parent as SPWebApplication;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                Collection<SPWebConfigModification> mods = webApplication.WebConfigModifications;
                int initialModificationsCount = mods.Count;

                for (int i = initialModificationsCount - 1; i >= 0; i--)
                {
                    if (mods[i].Owner == "GrowerCustomPage")
                    {
                        SPWebConfigModification modToRemove = mods[i];
                        mods.Remove(modToRemove);
                    }
                }

                if (initialModificationsCount > mods.Count)
                {
                    webApplication.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                    webApplication.Update();
                }

            });
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
