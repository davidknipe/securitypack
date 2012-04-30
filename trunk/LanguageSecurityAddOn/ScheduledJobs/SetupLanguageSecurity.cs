using EPiServer.BaseLibrary.Scheduling;
using EPiServer.PlugIn;
using EPiServer.Security;
using System;
using System.Collections.Generic;

namespace SecurityPack.LanguageSecurityAddOn.ScheduledJobs
{
    [ScheduledPlugIn(
        DisplayName = "SecurityPack : Language security set up", 
        Description = "Adds Publish access to all languages that currently have Change access <hr/><strong>WARNING!</strong> This job may affect existing langauge security settings. It is intended to be used as a one off when first installing SecurityPack.LanguageSecurityAddon<hr/>", 
        SortIndex = 9001)]
    public class SetupLanguageSecurity : JobBase
    {
        private bool stopped = false;

        public SetupLanguageSecurity()
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
                List<string> updatedList = new List<string>();

                //Iterate all languages
                foreach (var lang in EPiServer.DataAbstraction.LanguageBranch.ListAll())
                {
                    //Add Publish rights where Read rights already exist
                    bool changed = false;
                    AccessControlList update = lang.ACL.Copy();
                    foreach (var key in lang.ACL)
                    {
                        if (key.Value.Access.HasFlag(AccessLevel.Edit) && !key.Value.Access.HasFlag(AccessLevel.Publish))
                        {
                            update[key.Key] = new AccessControlEntry(key.Key, AccessLevel.Edit | AccessLevel.Publish);
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        lang.ACL = update;
                        lang.Save();
                        this.OnStatusChanged("Applying updates for language: '" + lang.Name + "'");
                        updatedList.Add(lang.LanguageID);
                    }
                    if (this.stopped)
                        break;
                }
                if (updatedList.Count > 0)
                {
                    return "The following language branch(es) were updated: " + String.Join(", ", updatedList);
                }
                else
                {
                    return "No languages were updated";
                }
            }
            catch (Exception ex)
            {
                return "EXCEPTION executing job: " + ex.ToString();
            }
        }
    }}
