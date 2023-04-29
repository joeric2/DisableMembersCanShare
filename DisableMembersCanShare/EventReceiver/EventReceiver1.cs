using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System;
using System.Security.Permissions;
using System.Threading;

namespace DisableMembersCanShare.EventReceiver
{
    /// <summary>
    /// Web Events
    /// </summary>
    public class DisableMembersCanShareEventReceiver : SPSecurityEventReceiver
    {
        /// <summary>
        /// A group was updated.
        /// </summary>
        public override void GroupUpdated(SPSecurityEventProperties properties)
        {

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                SPDiagnosticsService.Local.WriteTrace(
                    0,
                    new SPDiagnosticsCategory("CustomEventReceivers", TraceSeverity.Verbose, EventSeverity.None),
                    TraceSeverity.Verbose,
                    "Event Receiver: DisableMembersCanShare begin.",
                    properties.GroupId,
                    properties.GroupName,
                    properties.GroupUserId,
                    properties.PrincipalId,
                    properties.Web.Url
                );

                try
                {
                    base.GroupUpdated(properties);

                    using (SPSite site = new SPSite(properties.SiteId))
                    {
                        using (SPWeb web = site.OpenWeb(properties.WebId))
                        {

                            SPGroup membersGroup = web.AssociatedMemberGroup;

                            SPDiagnosticsService.Local.WriteTrace(
                            0,
                            new SPDiagnosticsCategory("CustomEventReceivers", TraceSeverity.Verbose, EventSeverity.None),
                                TraceSeverity.Verbose,
                                "Group details for disabling SPGroup.AllowMembersEditMembership GroupID: {0}; GroupName: {1}; GroupUserId: {2};PrincipalId: {3}; WebUrl: {4}",
                                properties.GroupId,
                                properties.GroupName,
                                properties.GroupUserId,
                                properties.PrincipalId,
                                properties.Web.Url
                            );

                            //SPGroup thisGroup = web.Groups.GetByID(properties.GroupId);
                            SPGroup thisGroup = web.Site.RootWeb.SiteGroups.GetByID(properties.GroupId);


                            if ((null != membersGroup && membersGroup == thisGroup) || thisGroup.AllowMembersEditMembership)
                            {

                                if (web.MembersCanShare)
                                {
                                    SPDiagnosticsService.Local.WriteTrace(
                                    0,
                                    new SPDiagnosticsCategory("CustomEventReceivers", TraceSeverity.High, EventSeverity.None),
                                    TraceSeverity.High,
                                    "Disabling \"SPWeb.MembersCanShare\" in web {0}",
                                    web.Url
                                    );

                                    web.MembersCanShare = false;
                                    web.Update();
                                }

                                if (thisGroup.AllowMembersEditMembership)
                                {
                                    SPDiagnosticsService.Local.WriteTrace(
                                    0,
                                    new SPDiagnosticsCategory("CustomEventReceivers", TraceSeverity.High, EventSeverity.None),
                                    TraceSeverity.High,
                                    "Disabling \"SPGroup.AllowMembersEditMembership\" for group {0} in web {1}",
                                    thisGroup.Name,
                                    web.Url
                                    );

                                    //avoid save conflict during initial provisioning of groups...
                                    Thread.Sleep(500);

                                    thisGroup.AllowMembersEditMembership = false;
                                    thisGroup.Update();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SPDiagnosticsService.Local.WriteTrace(
                    0,
                    new SPDiagnosticsCategory("CustomEventReceivers", TraceSeverity.Unexpected, EventSeverity.None),
                    TraceSeverity.Unexpected,
                    "failed to restrict members can edit group membership with error: {0}\n{1}",
                    ex.Message,
                    ex.StackTrace
                    );
                }
            });
        }

    }
}