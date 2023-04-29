using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace DisableMembersCanShare.Features.DisableMembersCanShareWebFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("93ee7ec8-b057-4341-ba01-d4805d08c942")]
    public class DisableMembersCanShareWebFeatureEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                try
                {
                    SPWeb oweb = properties.Feature.Parent as SPWeb;
                    using (SPSite site = new SPSite(oweb.Site.ID))
                    {
                        using (SPWeb web = site.OpenWeb(oweb.ID))
                        {
                            if (web.MembersCanShare)
                            {
                                SPDiagnosticsService.Local.WriteTrace(
                                0,
                                new SPDiagnosticsCategory("CustomFeatureReceivers", TraceSeverity.High, EventSeverity.None),
                                TraceSeverity.High,
                                "Disabling \"SPWeb.MembersCanShare\" in web {0}",
                                web.Url
                                );

                                web.MembersCanShare = false;
                                web.Update();
                            }

                            SPGroup membersGroup = web.AssociatedMemberGroup;
                            if (null != membersGroup && membersGroup.AllowMembersEditMembership)
                            {
                                SPDiagnosticsService.Local.WriteTrace(
                                0,
                                new SPDiagnosticsCategory("CustomFeatureReceivers", TraceSeverity.High, EventSeverity.None),
                                TraceSeverity.High,
                                "Disabling \"SPGroup.AllowMembersEditMembership\" for group {0} in web {1}",
                                membersGroup.Name,
                                web.Url
                                );

                                membersGroup.AllowMembersEditMembership = false;
                                membersGroup.Update();
                            }
                        }
                    }
                    oweb.Dispose();
                }
                catch (Exception ex)
                {
                    SPDiagnosticsService.Local.WriteTrace(
                        0,
                        new SPDiagnosticsCategory("CustomFeatureReceivers", TraceSeverity.Unexpected, EventSeverity.None),
                        TraceSeverity.Unexpected,
                        "failed to restrict members can edit group membership with error: {0}",
                        ex.Message,
                        ex.StackTrace
                        );
                }
            });
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


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
