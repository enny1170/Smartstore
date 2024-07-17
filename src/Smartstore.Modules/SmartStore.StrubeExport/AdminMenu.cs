using Smartstore.Collections;
using Smartstore.Core.Content.Menus;
using Smartstore.Web.Rendering.Builders;

namespace Smartstore.StrubeExport
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("Strube Export")
                .Icon("fa-seedling","bi")
                .ResKey("Plugins.FriendlyName.SmartStore.StrubeExport")
                .Action("Configure", "Strube Export", new {  area = "Admin" })
                .AsItem();

            pluginsNode.Prepend(menuItem);
        }

    }
}
