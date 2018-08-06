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
            builder.Add(T("Settings"),
                menu => menu.Add(
                    T("App Releases"),
                    "1",
                    item => item.Action("Index", "Admin", new { area = "CodeSanook.AppRelease" }))
                );
        }
    }
}