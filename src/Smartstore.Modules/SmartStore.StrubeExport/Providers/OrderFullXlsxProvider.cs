using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Smartstore.Collections;
using Smartstore.Core.Catalog;
using Smartstore.Core.DataExchange;
using Smartstore.Core.Logging;
//using Smartstore.Core.Plugins;
//using Smartstore.Services;
//using Smartstore.Services.Catalog;
//using Smartstore.Services.DataExchange.Export;
//using SmartStore.Services.Directory;
using Smartstore.Core.Localization;
using Smartstore.Core.Configuration;
using System.IO;
//using Smartstore.Core.Domain.Orders;
using OfficeOpenXml;
using Smartstore.StrubeExport.Models;
using Smartstore.Core.Security;
using Smartstore.Engine.Modularity;
using Smartstore.Core.DataExchange.Export;
using System.Threading.Tasks;
using System.Threading;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Widgets;
using Smartstore.Core.Data;

namespace Smartstore.StrubeExport.Providers
{
    /// <summary>
    /// Provider for Export Order Infos without Prices as XLSX File
    /// </summary>
    [SystemName("Strube.OrdersExportXLSX")]
    [FriendlyName("Strube Full Order xlsx-Export")]
    [Order(2)]
    [ExportFeatures(Features =
    ExportFeatures.CreatesInitialPublicDeployment |
    ExportFeatures.CanOmitGroupedProducts |
    ExportFeatures.CanProjectAttributeCombinations |
    ExportFeatures.CanProjectDescription |
    ExportFeatures.UsesSkuAsMpnFallback |
    ExportFeatures.OffersBrandFallback |
    ExportFeatures.UsesAttributeCombination |
    ExportFeatures.CanOmitCompletionMail)]
    public class OrderFullXlsxProvider: ExportProviderBase
    {
        //private readonly IEncryptionService _encryptionService;
        private readonly SmartDbContext _db;
        private readonly IEncryptor _encryptor;
        private readonly ISettingService _settingService;
        private readonly string _encryptionKey;

        //public OrderFullXlsxProvider(IEncryptionService encryptionService, ISettingService settingService)
        public OrderFullXlsxProvider(SmartDbContext db, IEncryptor encryptor,ISettingService settingService)
        {
            //_encryptionService = encryptionService;
            _db= db;
            _encryptor = encryptor;
            _settingService = settingService;
            var securitySettings = _settingService.GetSettingEntityByKeyAsync("securitysettings.encryptionkey").Result; // .GetSettings<SecuritySettings>();
            _encryptionKey = securitySettings.Value; //  .EncryptionKey;
        }

        public override ExportConfigurationInfo ConfigurationInfo => new()
        {
            ConfigurationWidget = new ComponentWidget(typeof(Components.StrubeXlsxConfigurationViewComponent)),
            ModelType = typeof(ProfileConfigurationModel)
        };

        public override ExportEntityType EntityType
        {
            get { return ExportEntityType.Order; }
        }

        public static string SystemName
        {
            get { return "Strube.OrdersExportXLSX"; }
        }

        public override string FileExtension
        {
            get { return "xlsx"; }
        }

        //public override ExportConfigurationInfo ConfigurationInfo => new ExportConfigurationInfo
        //{
        //    ConfigurationWidget = "~/Plugins/SmartStore.StrubeExport/Views/StrubeExport/ProfileConfiguration.cshtml",

        //    ModelType = typeof(ProfileConfigurationModel),
        //    Initialize = obj =>
        //    {
        //        var model = (obj as ProfileConfigurationModel);

        //        //model.LanguageSeoCode = _services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();

        //        //model.AvailableCategories = model.DefaultGoogleCategory.HasValue()
        //        //    ? new List<SelectListItem> { new SelectListItem { Text = model.DefaultGoogleCategory, Value = model.DefaultGoogleCategory, Selected = true } }
        //        //    : new List<SelectListItem>();
        //    }
        //};

        protected override async Task ExportAsync(ExportExecuteContext context, CancellationToken cancelToken)
        {
            dynamic currency = context.Currency;
            OrderDetails orderDetails = new OrderDetails();
            var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

            while (context.Abort == DataExchangeAbortion.None && await context.DataSegmenter.ReadNextSegmentAsync())
            {
                var segment = await context.DataSegmenter.GetCurrentSegmentAsync();

                foreach (dynamic order in segment)
                {
                    Order orderEntity = order.Entity;
                    //try to load related objects
                    _db.Entry(orderEntity).Collection(o => o.OrderItems).Load();
                    _db.Entry(orderEntity).Reference(o => o.ShippingAddress).Load();
                    _db.Entry(orderEntity.ShippingAddress).Reference(s=> s.Country).Load();
                    _db.Entry(orderEntity).Reference(o=>o.BillingAddress).Load();
                    _db.Entry(orderEntity.BillingAddress).Reference(s=>s.Country).Load();
                    List<OrderItem> orderItem = orderEntity.OrderItems.ToList();
                    //loading Product data
                    foreach (var item in orderItem)
                    {
                        _db.Entry(item).Reference(i => i.Product).Load();
                    }

                    if (context.Abort != DataExchangeAbortion.None)
                    {
                        break;
                    }

                    try
                    {
                        foreach (OrderItem item in orderItem)
                        {
                            OrderDetail tmp = new OrderDetail(item, config, _encryptor, _encryptionKey);
                            orderDetails.Add(tmp);
                            ++context.RecordsSucceeded;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.RecordException(ex, orderEntity.Id);
                    }
                }
            }
            //create ExcelPackage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(context.DataStream))
            {
                var workSheet = excelPackage.Workbook.Worksheets.Add(
                    DateTime.Now.Year.ToString() +
                    "_" +
                    DateTime.Now.Month.ToString() +
                    "_" +
                    DateTime.Now.Day.ToString() +
                    "_" +
                    DateTime.Now.Hour.ToString() +
                    "_" +
                    DateTime.Now.Minute.ToString());
                workSheet.Cells["A1"].LoadFromCollection<OrderDetail>(orderDetails, true, OfficeOpenXml.Table.TableStyles.Medium13);
                excelPackage.Workbook.Properties.Company = "Strube D&S GmbH";
                excelPackage.Workbook.Properties.Author = "Strube Web Shop";
                excelPackage.Save();
            }


        }


        //protected override void Export(ExportExecuteContext context)
        //{
        //    dynamic currency = context.Currency;
        //    OrderDetails orderDetails = new OrderDetails();
        //    var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();
        //    // convert the lines
        //    while (context.Abort == DataExchangeAbortion.None && context.DataSegmenter.ReadNextSegment())
        //    {
        //        var segment = context.DataSegmenter.CurrentSegment;
        //        foreach (dynamic order in segment)
        //        {
        //            Order orderEntity = order.Entity;
        //            List<OrderItem> orderItem = orderEntity.OrderItems.ToList();

        //            if (context.Abort != DataExchangeAbortion.None)
        //            {
        //                break;
        //            }

        //            try
        //            {
        //                foreach (OrderItem item in orderItem)
        //                {
        //                    OrderDetail tmp = new OrderDetail(item,config,_encryptionService,_encryptionKey);
        //                    orderDetails.Add(tmp);
        //                    ++context.RecordsSucceeded;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                context.RecordException(ex, orderEntity.Id);
        //            }
        //        }
        //    }
        //    //create ExcelPackage
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (ExcelPackage excelPackage = new ExcelPackage(context.DataStream))
        //    {
        //        var workSheet = excelPackage.Workbook.Worksheets.Add(
        //            DateTime.Now.Year.ToString() +
        //            "_" +
        //            DateTime.Now.Month.ToString() +
        //            "_" +
        //            DateTime.Now.Day.ToString() +
        //            "_" +
        //            DateTime.Now.Hour.ToString() +
        //            "_" +
        //            DateTime.Now.Minute.ToString());
        //        workSheet.Cells["A1"].LoadFromCollection<OrderDetail>(orderDetails, true, OfficeOpenXml.Table.TableStyles.Medium13);
        //        excelPackage.Workbook.Properties.Company = "Strube D&S GmbH";
        //        excelPackage.Workbook.Properties.Author = "Strube Web Shop";
        //        excelPackage.Save();
        //    }

        //}

        //public override ExportConfigurationInfo ConfigurationInfo
        //{
        //    get { return null; }
        //}

        // override after execution here
        public override Task OnExecutedAsync(ExportExecuteContext context, CancellationToken cancelToken = default)
        {
            return base.OnExecutedAsync(context, cancelToken);
        }
    }
}