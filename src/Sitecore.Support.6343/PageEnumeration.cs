using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.XA.Feature.CreativeExchange.Enums;
using Sitecore.XA.Feature.CreativeExchange.Models.Export;
using Sitecore.XA.Feature.CreativeExchange.Models.Import;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Export.Export;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Export.PageProcessing;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Export.PageValidation;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.Import;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.PageProcessing;
using Sitecore.XA.Feature.CreativeExchange.Services.Export;
using Sitecore.XA.Feature.CreativeExchange.Storage;
using Sitecore.XA.Foundation.Multisite;
using Sitecore.XA.Foundation.SitecoreExtensions.Repositories;

namespace Sitecore.Support.XA.Feature.CreativeExchange.Services.Export
{
    public class PageEnumerator : Sitecore.XA.Feature.CreativeExchange.Services.Export.PageEnumerator
    {
        public PageEnumerator(ExportOptions exportOptions):base(exportOptions)
        {
            //fix 6343
            Database ContentDatabase = Context.ContentDatabase ?? Context.Database;
            base.HomeItem =ContentDatabase.GetItem(HomeItem.ID, base.StartItem.Language);
        }
    }
}

namespace Sitecore.Support.XA.Feature.CreativeExchange.Services.Export
{
 
        public class PageEnumeration : Sitecore.XA.Feature.CreativeExchange.Pipelines.Export.Export.PageEnumeration
    {
            public override void Process(ExportArgs args)
            {
            //fix 6343
                var pages = new Sitecore.Support.XA.Feature.CreativeExchange.Services.Export.PageEnumerator(args.ExportContext.ExportOptions)
                    .Enumerate()
                    .Where(page => base.Valid(page, args));

            foreach (var exportedPage in pages)
            {
                ProcessPage(exportedPage, args);
            }
            }

            private void ProcessPage(ExportedPage exportedPage, ExportArgs args)
            {
                ISiteInfoResolver siteInfoResolver = Sitecore.XA.Foundation.IoC.ServiceLocator.Current.Resolve<ISiteInfoResolver>();
                string virtualFolder = siteInfoResolver.GetSiteInfo(exportedPage.Item).VirtualFolder;

                var pageProcessingArgs = new PageProcessingArgs
                {
                    PageContext = new PageContext
                    {
                        ExportContext = args.ExportContext,
                        HttpContextData = args.HttpContextData,
                        ExportedPage = exportedPage,
                        HomeFullPath = siteInfoResolver.GetStartPath(exportedPage.Item),
                        VirtualFolders = !string.IsNullOrWhiteSpace(virtualFolder) ? new List<string> { virtualFolder } : new List<string>()
                    },
                    LinkProcessor = args.LinkProcessor,
                    CreativeExchangeExportStorage = args.CreativeExchangeExportStorage,
                    Messages = args.Messages
                };
                CorePipeline.Run("ceExport.pageProcessing", pageProcessingArgs);
            }
        }


    }

