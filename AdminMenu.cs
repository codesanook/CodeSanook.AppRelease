using CodeSanook.AppRelease.Controllers;
using CodeSanook.Common.Modules;
using CodeSanook.Common.Web;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace CodeSanook.Swagger
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(item => item
                    .Caption(T("App Release"))
                    .Position("11")
                    .Action(
                        nameof(AppInfoController.Index),
                        MvcHelper.GetControllerName<AppInfoController>(),
                        new { area = ModuleHelper.GetModuleName<AppInfoController>() }
                    )
                );
        }
    }
}