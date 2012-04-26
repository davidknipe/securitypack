using System;
using System.Reflection;
using System.Web;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.UI;
using EPiServer.UI.Admin;
using EPiServer.UI.WebControls;

namespace SecurityPack.PageTypeSecurityAddOn.InitModules
{
    [InitializableModule]
    [ModuleDependency((typeof(EPiServer.Web.InitializationModule)))]
    public class AddEditACLOptionOnEditPageTypeBase : IInitializableHttpModule
    {
        public void InitializeHttpEvents(System.Web.HttpApplication application)
        {
            application.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
        }

        private void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            //Get the Request handler from the Context
            HttpContext context = ((HttpApplication)sender).Context;

            //Only try this if users are logged in and we have a handler
            if (context.Handler != null)
            {
                //Check if the request handler is of the right type (the built in or Composer page type editor)
                string baseTypeName = context.Handler.GetType().BaseType.FullName;
                if (baseTypeName == "EPiServer.UI.Admin.EditPageTypeBase" || baseTypeName == "Dropit.Extension.UI.Admin.ExtensionEditPageType")
                {
                    SystemPageBase pageBase = (SystemPageBase)context.Handler;
                    pageBase.InitComplete += pageBase_Init;
                }
            }
        }

        void pageBase_Init(object sender, EventArgs e)
        {
            //use a bit of reflection to find the PageTypeAccess property
            FieldInfo pageTypeAccessFieldInfo = Type.GetType(sender.GetType().BaseType.FullName).GetField("PageTypeAccess", BindingFlags.NonPublic | BindingFlags.Instance);

            if (pageTypeAccessFieldInfo != null)
            {
                object accessObj = pageTypeAccessFieldInfo.GetValue(sender);
                if (accessObj != null)
                {
                    MembershipAccessLevel access = (MembershipAccessLevel)accessObj;
                    access.AccessLevelValues = new AccessLevel[] { AccessLevel.Create, AccessLevel.Edit };
                }
            }
        }

        public void Initialize(EPiServer.Framework.Initialization.InitializationEngine context) { }

        public void Preload(string[] parameters) { }

        public void Uninitialize(EPiServer.Framework.Initialization.InitializationEngine context) { }
    }
}
