using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartStore.StrubeExport.Models;
using Smartstore.Core.DataExchange.Export;
using Smartstore.Engine.Modularity;
using Smartstore.Core.DataExchange;
using Smartstore.Core.Security;
using Smartstore.Core.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Widgets;

namespace SmartStore.StrubeExport.Providers
{
    /// <summary>
    /// Provider for Export Order Infos without Prices as CSV File
    /// </summary>

    [SystemName("Strube.OrdersExportCSV")]
    [FriendlyName("Strube Full Order csv-Export")]
    [Order(1)]
    [ExportFeatures(Features =
    ExportFeatures.CreatesInitialPublicDeployment |
    ExportFeatures.CanOmitGroupedProducts |
    ExportFeatures.CanProjectAttributeCombinations |
    ExportFeatures.CanProjectDescription |
    ExportFeatures.UsesSkuAsMpnFallback |
    ExportFeatures.OffersBrandFallback |
    ExportFeatures.UsesAttributeCombination |
    ExportFeatures.CanOmitCompletionMail)]
    public class OrderFullCsvProvider : ExportProviderBase
    {
        private readonly IEncryptor _encryptionService;
        private readonly ISettingService _settingService;
        private readonly string _encryptionKey;

        public OrderFullCsvProvider(IEncryptor encryptionService, ISettingService settingService)
        {
            _encryptionService = encryptionService;
            _settingService = settingService;
            var securitySettings = _settingService.GetSettingEntityByKeyAsync("securitysettings.encryptionkey").Result; 
            _encryptionKey = securitySettings.Value;
        }

        public override ExportEntityType EntityType
        {
            get { return ExportEntityType.Order; }
        }

        public static string SystemName
        {
            get { return "Strube.OrdersExportCSV"; }
        }

        public override string FileExtension
        {
            get { return "txt"; }
        }

        protected override async Task ExportAsync(ExportExecuteContext context, CancellationToken cancelToken)
        {
            dynamic currency = context.Currency;
            var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

            //Create Streamwriter
            StreamWriter _sw = new StreamWriter(context.DataStream);
            _sw.WriteLine(new OrderDetail().GetCSVHeader());
            // export the lines
            while (context.Abort == DataExchangeAbortion.None && await context.DataSegmenter.ReadNextSegmentAsync())
            {
                var segment = await context.DataSegmenter.GetCurrentSegmentAsync();
                foreach (dynamic order in segment)
                {
                    Order orderEntity = order.Entity;
                    List<OrderItem> orderItem = orderEntity.OrderItems.ToList();

                    if (context.Abort != DataExchangeAbortion.None)
                    {
                        break;
                    }

                    try
                    {
                        foreach (OrderItem item in orderItem)
                        {
                            OrderDetail orderDetail = new OrderDetail(item, config, _encryptionService, _encryptionKey);
                            _sw.WriteLine(orderDetail.GetCSVLine());
                            //Product itemProduct = item.Product;
                            //_sw.WriteLine(String.Format(_FormatString,
                            //    orderEntity.OrderGuid,
                            //    orderEntity.GetOrderNumber(),
                            //    orderEntity.CustomerOrderComment,
                            //    orderEntity.ShippingAddress.Company,
                            //    orderEntity.ShippingAddress.LastName,
                            //    orderEntity.ShippingAddress.FirstName,
                            //    orderEntity.ShippingAddress.Address1,
                            //    orderEntity.ShippingAddress.Address2,
                            //    orderEntity.ShippingAddress.ZipPostalCode,
                            //    orderEntity.ShippingAddress.City,
                            //    orderEntity.ShippingAddress.Country.Name,
                            //    itemProduct.Sku,
                            //    itemProduct.Name,
                            //    item.Quantity
                            //    )) ;

                            ++context.RecordsSucceeded;

                        }
                    }
                    catch (Exception ex)
                    {
                        context.RecordException(ex, orderEntity.Id);
                    }
                }

            }
            _sw.Flush();

        }


        public override ExportConfigurationInfo ConfigurationInfo => new()
        {
            ConfigurationWidget = new ComponentWidget(typeof(Components.StrubeXlsxConfigurationViewComponent)),
            ModelType = typeof(ProfileConfigurationModel)
        };

        //protected override void Export(ExportExecuteContext context)
        //{
        //    dynamic currency = context.Currency;
        //    //string _FormatString = "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13}";
        //    var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

        //    //Create Streamwriter
        //    StreamWriter _sw = new StreamWriter(context.DataStream);
        //    //Add Header Line
        //    //_sw.WriteLine(String.Format(_FormatString, 
        //    //    "ID", 
        //    //    "OrderId", 
        //    //    "Comment",
        //    //    "Company",
        //    //    "Name",
        //    //    "Surname",
        //    //    "Address1",
        //    //    "Address2",
        //    //    "Zip-Code",
        //    //    "City",
        //    //    "Country",
        //    //    "ItemId",
        //    //    "Description",
        //    //    "Count"));
        //    _sw.WriteLine(new OrderDetail().GetCSVHeader());
        //    // export the lines
        //    while (context.Abort==DataExchangeAbortion.None && context.DataSegmenter.ReadNextSegment())
        //    {
        //        var segment = context.DataSegmenter.CurrentSegment;
        //        foreach (dynamic order in segment)
        //        {
        //            Order orderEntity = order.Entity;
        //            List<OrderItem> orderItem = orderEntity.OrderItems.ToList();

        //            if (context.Abort!= DataExchangeAbortion.None)
        //            {
        //                break;
        //            }

        //            try
        //            {
        //                foreach (OrderItem item in orderItem)
        //                {
        //                    OrderDetail orderDetail = new OrderDetail(item,config,_encryptionService,_encryptionKey);
        //                    _sw.WriteLine(orderDetail.GetCSVLine());
        //                    //Product itemProduct = item.Product;
        //                    //_sw.WriteLine(String.Format(_FormatString,
        //                    //    orderEntity.OrderGuid,
        //                    //    orderEntity.GetOrderNumber(),
        //                    //    orderEntity.CustomerOrderComment,
        //                    //    orderEntity.ShippingAddress.Company,
        //                    //    orderEntity.ShippingAddress.LastName,
        //                    //    orderEntity.ShippingAddress.FirstName,
        //                    //    orderEntity.ShippingAddress.Address1,
        //                    //    orderEntity.ShippingAddress.Address2,
        //                    //    orderEntity.ShippingAddress.ZipPostalCode,
        //                    //    orderEntity.ShippingAddress.City,
        //                    //    orderEntity.ShippingAddress.Country.Name,
        //                    //    itemProduct.Sku,
        //                    //    itemProduct.Name,
        //                    //    item.Quantity
        //                    //    )) ;

        //                    ++context.RecordsSucceeded;

        //                }
        //            }
        //            catch(Exception ex)
        //            {
        //                context.RecordException(ex, orderEntity.Id);
        //            }
        //        }
        //    }
        //    _sw.Flush();
        //    //throw new NotImplementedException();
        //}

        //public override ExportConfigurationInfo ConfigurationInfo
        //{
        //    get { return null; }
        //}

    }
}