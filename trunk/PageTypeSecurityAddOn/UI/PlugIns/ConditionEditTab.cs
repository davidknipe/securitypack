using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.UI.Edit;
using EPiServer.UI.WebControls;
using EPiServer.Security;

namespace SecurityPack.PageTypeSecurityAddOn.UI.PlugIns
{
    /// <summary>
    /// Disable the Edit tab if a user does not have access to the page type
    /// </summary>
    [GuiPlugIn(Area = PlugInArea.EditPanel)]
    public class ConditionEditTab : ICustomPlugInLoader
    {
        public PlugInDescriptor[] List()
        {
            // hook LoadComplete-event on EditPanel page
            EditPanel editPanel = HttpContext.Current.Handler as EditPanel;

            if (null != editPanel)
            {
                editPanel.LoadComplete += new EventHandler(editPanel_LoadComplete);
            }

            //Never return a plugin - we don't want to add tabs.
            return new PlugInDescriptor[0] { };
        }

        protected void editPanel_LoadComplete(object sender, EventArgs e)
        {
            // find the TabStrip with id = "actionTab"
            TabStrip actionTabStrip = this.FindControl<TabStrip>(sender as Control, "actionTab");

            Tab actionTab = null;
            bool editTabFound = false;

            //Find the Edit tab
            if (actionTabStrip != null)
            {
                int editTabPlugInID = PlugInDescriptor.Load(typeof(EditPageControl)).ID;

                foreach (var item in actionTabStrip.Controls)
                {
                    actionTab = item as Tab;
                    if (actionTab != null && actionTab.PlugInID == editTabPlugInID && actionTab.Enabled == true)
                    {
                        //Check if the current user has access to this page type 
                        PageType pageType = PageType.Load((HttpContext.Current.Handler as EditPanel).CurrentPage.PageTypeID);
                        if (pageType.ACL.QueryDistinctAccess(PrincipalInfo.CurrentPrincipal, EPiServer.Security.AccessLevel.Edit) == false)
                        {
                            actionTab.Enabled = false;
                            editTabFound = true;
                        }
                        break;
                    }
                }

                //Check if we are attempting to select the edit tab, either by the DOPE menu. If so we need to stop this
                if (editTabFound && actionTab.Enabled == false)
                {
                    string selectedTabIndex = HttpContext.Current.Request.QueryString[actionTabStrip.SelectedTabQueryString];
                    if ((actionTabStrip.SelectedTabQueryString != null) && (selectedTabIndex != null) && selectedTabIndex == actionTab.Index.ToString())
                    {
                        //The first thing to do is to set the preview tab as the selected tab
                        actionTabStrip.SetSelectedTab("PreviewTab");

                        //Find the warning bar and show a warning message
                        HtmlGenericControl pageWarning = this.FindControl<HtmlGenericControl>(actionTabStrip.Parent, "PageWarning");
                        pageWarning.InnerHtml = pageWarning.InnerHtml + "<li>You cannot edit this page as you do not have edit access to the page template</li>";
                    }
                }
            }
        }

        // try to locate control of type T with ID==id
        // recurses into each childcontrol until first match is found
        protected T FindControl<T>(Control control, string id) where T : Control
        {
            T controlTest = control as T;

            if (null != controlTest && (null == id || controlTest.ID.Equals(id)))
                return controlTest;

            foreach (Control c in control.Controls)
            {
                controlTest = FindControl<T>(c, id);
                if (null != controlTest)
                    return controlTest;
            }

            return null;
        }

        protected T FindControl<T>(Control control) where T : Control
        {
            return FindControl<T>(control, null);
        }
    }
}
