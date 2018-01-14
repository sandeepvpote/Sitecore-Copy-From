using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Pipelines;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using System.Collections;
using System.Collections.Generic;

namespace Sitecore.Foundation.CopyFrom.Repositories
{
    public class CopyItems
    {
        public void GetDestination(CopyItemsArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (!SheerResponse.CheckModified())
                return;
            Item obj = GetDatabase(args).Items[new ListString(args.Parameters["destination"], '|')[0]];
            UrlString urlString = new UrlString(this.GetDialogUrl());
            if (obj != null)
            {
                urlString.Append("fo", obj.ID.ToString());
                urlString.Append("sc_content", obj.Database.Name);
                urlString.Append("la", args.Parameters["language"]);
            }
            Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
            args.WaitForPostBack(false);
        }

        protected virtual string GetDialogUrl()
        {
            return "/sitecore/shell/Applications/Dialogs/Copy From.aspx";
        }

        public void CheckDestination(CopyItemsArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (!args.HasResult)
            {
                args.AbortPipeline();
            }
            else
            {
                Database database = GetDatabase(args);
                if (args.Result != null && args.Result.Length > 0 && args.Result != "undefined")
                {
                    if (!database.GetItem(args.Result).Access.CanCreate())
                    {
                        Context.ClientPage.ClientResponse.Alert("You do not have permission to create items here.");
                        args.AbortPipeline();
                        return;
                    }
                    args.Parameters["items"] = args.Result;
                }
                args.IsPostBack = false;
            }
        }

        public void CheckLanguage(CopyItemsArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (args.IsPostBack)
            {
                if (!(args.Result != "yes"))
                    return;
                args.AbortPipeline();
            }
            else
            {
                bool flag = false;
                foreach (Item obj in GetItems(args))
                {
                    if (obj.TemplateID == TemplateIDs.Language)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return;
                SheerResponse.Confirm("You are coping a language.\n\nA language item must have a name that is a valid ISO-code.\n\nPlease rename the copied item afterward.\n\nAre you sure you want to continue?");
                args.WaitForPostBack();
            }
        }

        public virtual void Execute(CopyItemsArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            Item target = GetDatabase(args).GetItem(args.Parameters["destination"], Language.Parse(args.Parameters["language"]));
            Assert.IsNotNull((object)target, args.Parameters["destination"]);
            ArrayList arrayList = new ArrayList();
            foreach (Item itemToCopy in GetItems(args))
            {
                if (itemToCopy != null)
                    arrayList.Add((object)this.CopyItem(target, itemToCopy));
            }
            args.Copies = arrayList.ToArray(typeof(Item)) as Item[];
        }

        protected virtual Item CopyItem(Item target, Item itemToCopy)
        {
            Assert.ArgumentNotNull((object)target, nameof(target));
            Assert.ArgumentNotNull((object)itemToCopy, nameof(itemToCopy));
            string str = target.Uri.ToString();
            string copyOfName = ItemUtil.GetCopyOfName(target, itemToCopy.Name);
            Item obj = Context.Workflow.CopyItem(itemToCopy, target, copyOfName);
            Log.Audit((object)this, "Copy item from: {0} to {1}", AuditFormatter.FormatItem(itemToCopy), AuditFormatter.FormatItem(obj), str);
            return obj;
        }

        protected static Database GetDatabase(CopyItemsArgs args)
        {
            string parameter = args.Parameters["database"];
            Database database = Factory.GetDatabase(parameter);
            Assert.IsNotNull((object)database, parameter);
            return database;
        }
        protected static List<Item> GetItems(CopyItemsArgs args)
        {
            List<Item> objList = new List<Item>();
            Database database = GetDatabase(args);
            foreach (string path in new ListString(args.Parameters["items"], '|'))
            {
                Item obj = database.GetItem(path, Language.Parse(args.Parameters["language"]));
                if (obj != null)
                    objList.Add(obj);
            }
            return objList;
        }
    }
}