using System;
using System.Reflection;
using System.Web;
using EPiServer.Framework;
using EPiServer.Security;
using EPiServer.UI;
using EPiServer.UI.WebControls;

namespace SecurityPack.LanguageSecurityAddOn.InitModules
{
    [InitializableModule]
    [ModuleDependency((typeof(EPiServer.Web.InitializationModule)))]
    public class ConditionPublishControls : IInitializableHttpModule
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
                if(context.Handler.GetType().BaseType == typeof(EPiServer.UI.Admin.EditLanguageBranch))
                {
                    SystemPageBase pageBase = (SystemPageBase)context.Handler;
                    pageBase.InitComplete += pageBase_Init;
                }
            }
        }

        void pageBase_Init(object sender, EventArgs e)
        {
            //use a bit of reflection to find the LanguageAccess property
            FieldInfo langBranchAccessFieldInfo = Type.GetType(sender.GetType().BaseType.FullName).GetField("LanguageAccess", BindingFlags.NonPublic | BindingFlags.Instance);

            if (langBranchAccessFieldInfo != null)
            {
                object accessObj = langBranchAccessFieldInfo.GetValue(sender);
                if (accessObj != null)
                {
                    MembershipAccessLevel access = (MembershipAccessLevel)accessObj;
                    access.AccessLevelValues = new AccessLevel[] { AccessLevel.Edit, AccessLevel.Publish };
                }
            }
        }

        public void Initialize(EPiServer.Framework.Initialization.InitializationEngine context) { }

        public void Preload(string[] parameters) { }

        public void Uninitialize(EPiServer.Framework.Initialization.InitializationEngine context) { }
    }
}
