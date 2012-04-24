using System;
using System.Collections.Generic;
using EPiServer;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Security;

namespace SecurityPack.PageTypeSecurityAddOn.UI.PlugIns
{
    [PagePlugIn]
    public class ConditionContextMenuEditControls
    {
        public static void Initialize(int bitflags)
        {
            EPiServer.PageBase.PageSetup += new EPiServer.PageSetupEventHandler(PageBase_PageSetup);
        }

        static void PageBase_PageSetup(EPiServer.PageBase sender, EPiServer.PageSetupEventArgs e)
        {
            sender.PreRender += new EventHandler(sender_PreRender);
        }

        static void sender_PreRender(object sender, EventArgs e)
        {
            var page = (sender as EPiServer.PageBase);
            if (page != null && page.ContextMenu != null)
            {
                bool hasAccessToPageType = PageType.Load(page.CurrentPage.PageTypeID).ACL.QueryDistinctAccess(AccessLevel.Edit);
                if (hasAccessToPageType == false)
                {
                    List<String> contextMenuKeys = new List<String> { "DopeEdit", "QuickEdit", "ExtEditOnPage" };
                    RightClickMenuItem item;
                    foreach (string key in contextMenuKeys)
                    {
                        item = page.ContextMenu.Menu.Items[key];
                        if (item != null)
                        {
                            item.EnabledScript = "false;";
                        }
                    }
                }
            }
        }
    }
}
