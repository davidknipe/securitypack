using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.BaseLibrary.Scheduling;

namespace SecurityPack.PageTypeSecurityAddOn.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "SecurityPack : Page type security set up", Description = "Adds Change and Publish access to all page types that currently have Read access<hr/><strong>WARNING!</strong> This job may affect existing page security settings. It is intended to be used as a one off when first installing SecurityPack.PageTypeSecurityAddOn<hr/>", SortIndex = 9000)]
    public class SetupPageTypeSecurity : JobBase
    {
        private bool stopped = false;

        public SetupPageTypeSecurity()
            : base()
        {
            this.IsStoppable = true;
        }

        public override void Stop()
        {
            stopped = true;
            base.Stop();
        }

        public override string Execute()
        {
            try
            {
                int updatedCount = 0;

                //Iterate all page types
                foreach (var pt in EPiServer.DataAbstraction.PageType.List())
                {
                    //Set Change rights where Read rights already exist
                    bool changed = false;
                    AccessControlList update = pt.ACL.Copy();
                    foreach (var key in pt.ACL)
                    {
                        if (key.Value.Access.HasFlag(AccessLevel.Create) && !key.Value.Access.HasFlag(AccessLevel.Edit) && key.Value.Access.HasFlag(AccessLevel.Publish))
                        {
                            update[key.Key] = new AccessControlEntry(key.Key, AccessLevel.Create | AccessLevel.Edit | AccessLevel.Publish);
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        pt.ACL = update;
                        pt.Save();
                        this.OnStatusChanged("Applying updates for page type: '" + pt.Name + "'");
                        updatedCount++;
                    }
                    if (this.stopped)
                        break;
                }
                if (updatedCount > 0)
                {
                    return updatedCount.ToString() + " page types were updated";
                }
                else
                {
                    return "No page types were updated";
                }
            }
            catch (Exception ex)
            {
                return "EXCEPTION executing job: " + ex.ToString();
            }
        }
    }
}
