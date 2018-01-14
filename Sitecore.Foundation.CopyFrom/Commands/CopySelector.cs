using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework.Pipelines;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sitecore.Foundation.CopyFrom.Commands
{
    public class CopySelector : Command
    {
        public override void Execute(CommandContext context)
        {
            //selected item
            Item[] items = context.Items;

            Assert.ArgumentNotNull((object)items, nameof(items));
            if (items.Length == 0)
                return;
            Start("uiCopyItemFrom", (ClientPipelineArgs)new CopyItemsArgs(), items[0].Database, items);
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject((object)context, nameof(context));
            if (context.Items.Length != 1)
                return CommandState.Disabled;
            Item obj = context.Items[0];
            if (obj.Appearance.ReadOnly || !obj.Access.CanRead() || !context.Items[0].Access.CanWriteLanguage())
                return CommandState.Disabled;
            return base.QueryState(context);
        }

        private static NameValueCollection Start(string pipelineName, ClientPipelineArgs args, Database database, Item[] items)
        {
            Assert.ArgumentNotNullOrEmpty(pipelineName, nameof(pipelineName));
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.ArgumentNotNull((object)database, nameof(database));
            Assert.ArgumentNotNull((object)items, nameof(items));
            return Start(pipelineName, args, database, items, (NameValueCollection)null);
        }

        private static NameValueCollection Start(string pipelineName, ClientPipelineArgs args, Database database, Item[] items, NameValueCollection additionalParameters)
        {
            Assert.ArgumentNotNull((object)pipelineName, nameof(pipelineName));
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.ArgumentNotNull((object)database, nameof(database));
            Assert.ArgumentNotNull((object)items, nameof(items));
            Assert.ArgumentNotNullOrEmpty(pipelineName, nameof(pipelineName));
            NameValueCollection nameValueCollection = new NameValueCollection();
            string selectedItems = string.Join<ID>("|", ((IEnumerable<Item>)items).Select<Item, ID>((Func<Item, ID>)(item => item.ID)));
            string selectedItemsLanguage = items[0].Language.ToString();
            nameValueCollection.Add(nameof(database), database.Name);
            nameValueCollection.Add("language", selectedItemsLanguage);
            nameValueCollection.Add("destination", selectedItems);
            args.Parameters = nameValueCollection;
            if (additionalParameters != null)
            {
                foreach (string allKey in additionalParameters.AllKeys)
                    args.Parameters[allKey] = additionalParameters[allKey];
            }
            Sitecore.Context.ClientPage.Start(pipelineName, args);
            return nameValueCollection;
        }
    }
}