using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Shell.Framework;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using System;

namespace Sitecore.Foundation.CopyFrom.Dailogs
{
    public class CopySelectorForm : DialogForm
    {

        protected DataContext DataContext;

        protected TreeviewEx Treeview;

        protected Literal CopyFromItemPath;

        protected Literal CopyToItemPath;

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            Dispatcher.Dispatch(message, this.GetCurrentItem(message));
            base.HandleMessage(message);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
                return;
            this.DataContext.GetFromQueryString();
            Context.ClientPage.ServerProperties["id"] = (object)WebUtil.GetQueryString("fo");
            Item folder = this.DataContext.GetFolder();
            Assert.IsNotNull((object)folder, "Item not found");

            this.CopyToItemPath.Text = "Copy selected item to: " + this.ShortenPath(folder.Paths.Path);

            // Set this dynamically based on the current item.
            //this.DataContext.Root = "{00000000-0000-0000-0000-00000000}";
        }


        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)args, nameof(args));
            string str = this.CopyFromItemPath.Value;
            if (str.Length == 0)
                SheerResponse.Alert("Select an item.");
            Item root = this.DataContext.GetRoot();
            if (root != null && root.ID != root.Database.GetRootItem().ID)
                str = FileUtil.MakePath(root.Paths.Path, str, '/');
            //Object to copy
            Item obj1 = this.DataContext.GetItem(str);
            if (obj1 == null)
            {
                SheerResponse.Alert("The target item could not be found.");
            }
            else
            {
                string serverProperty = Context.ClientPage.ServerProperties["id"] as string;
                if (!string.IsNullOrEmpty(serverProperty))
                {
                    // Copy to this object
                    Item obj2 = Context.ContentDatabase.Items[serverProperty];
                    if (obj2 == null)
                    {
                        SheerResponse.Alert("The source item could not be found.");
                        return;
                    }
                    if (obj1.ID.ToString() == obj2.ID.ToString())
                    {
                        SheerResponse.Alert("Select a different item.");
                        return;
                    }
                    if (obj1.Paths.LongID.StartsWith(obj2.Paths.LongID, StringComparison.InvariantCulture))
                    {
                        SheerResponse.Alert("An item cannot be copied below itself.");
                        return;
                    }
                }
                if (!obj1.Access.CanCreate())
                {
                    SheerResponse.Alert("The item cannot be copied to this location because\nyou do not have Create permission.");
                }
                else
                {
                    SheerResponse.SetDialogValue(obj1.ID.ToString());
                    base.OnOK(sender, args);
                }
            }
        }

        protected void SelectTreeNode()
        {
            Item selectionItem = this.Treeview.GetSelectionItem();
            if (selectionItem == null)
                return;


            this.CopyFromItemPath.Value =  this.ShortenPath(selectionItem.Paths.Path);
            this.CopyFromItemPath.Text = "Copy this item: " +  this.ShortenPath(selectionItem.Paths.Path);
        }

        private Item GetCurrentItem(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            string index = message["id"];
            Item folder = this.DataContext.GetFolder();
            Language language = Context.Language;
            if (folder != null)
                language = folder.Language;
            if (!string.IsNullOrEmpty(index))
                return Context.ContentDatabase.Items[index, language];
            return folder;
        }

        private string ShortenPath(string path)
        {
            Assert.ArgumentNotNull((object)path, nameof(path));
            Item root = this.DataContext.GetRoot();
            if (root != null && root.ID != root.Database.GetRootItem().ID)
            {
                string path1 = root.Paths.Path;
                if (path.StartsWith(path1, StringComparison.InvariantCulture))
                    path = StringUtil.Mid(path, path1.Length);
            }
            return path;
        }
    }
}
