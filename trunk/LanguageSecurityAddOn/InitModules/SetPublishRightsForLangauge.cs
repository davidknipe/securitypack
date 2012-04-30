using System;
using System.Reflection;
using System.Web;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.UI;
using EPiServer.UI.Admin;
using EPiServer.UI.WebControls;
using System.Web.UI;
using EPiServer.UI.Edit;
using EPiServer.DataAbstraction;

namespace SecurityPack.LanguageSecurityAddOn.InitModules
{
    [InitializableModule]
    [ModuleDependency((typeof(EPiServer.Web.InitializationModule)))]
    public class SetPublishRightsForLangauge : IInitializableHttpModule
    {
        public void InitializeHttpEvents(System.Web.HttpApplication application)
        {
            application.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
        }

        private void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            //Get the Request handler from the Context
            HttpContext context = ((HttpApplication)sender).Context;

            //Only try this if we have a handler
            if (context.Handler != null && context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                //Check if the request handler is of the right type
                if (context.Handler.GetType().IsSubclassOf(typeof(EPiServer.UI.Edit.EditPanel)))
                {
                    SystemPageBase pageBase = (EditPanel)context.Handler;
                    pageBase.PreRender += pageBase_PreRender;
                }
            }
        }

        void pageBase_PreRender(object sender, EventArgs e)
        {
            EPiServer.UI.Edit.EditPanel editPanel = (EPiServer.UI.Edit.EditPanel)sender;

            EditPageButtonControl editPageButtonControl = FindControl<EditPageButtonControl>(editPanel);

            if (editPageButtonControl != null && editPageButtonControl.Page != null)
            {
                //Check if the user has Publish access to the page language
                LanguageBranch pageLang = LanguageBranch.Load(editPanel.CurrentPage.LanguageID);
                if (pageLang.ACL.QueryDistinctAccess(PrincipalInfo.CurrentPrincipal, AccessLevel.Publish) == false)
                {
                    // The user does not have Publish access to the lanaguage so remove any publish 
                    // rights, this is temporary since the ACL is not saved with the page
                    AccessControlList newList = editPanel.CurrentPage.ACL.Copy();
                    foreach (var key in editPanel.CurrentPage.ACL)
                    {
                        if (key.Value.Access.HasFlag(AccessLevel.Publish))
                        {
                            AccessLevel level = key.Value.Access;
                            level = level ^ AccessLevel.Publish;
                            newList[key.Key] = new AccessControlEntry(key.Key, level);
                        }
                    }
                    editPanel.CurrentPage.ACL = newList;
                }
            }
        }

        public void Initialize(EPiServer.Framework.Initialization.InitializationEngine context) { }

        public void Preload(string[] parameters) { }

        public void Uninitialize(EPiServer.Framework.Initialization.InitializationEngine context) { }

        // try to locate control of type T with ID==id
        // recurses into each childcontrol until first match is found
        protected T FindControl<T>(Control control, string id) where T : Control
        {
            try
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
            catch
            {
                return null;
            }
        }

        protected T FindControl<T>(Control control) where T : Control
        {
            return FindControl<T>(control, null);
        }
    }
}
