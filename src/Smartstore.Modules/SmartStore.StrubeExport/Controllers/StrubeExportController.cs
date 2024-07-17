using Microsoft.AspNetCore.Mvc;
using Smartstore.ComponentModel;
using Smartstore.Core;
using Smartstore.Core.Common.Services;
using Smartstore.Core.DataExchange.Export;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;
using Smartstore.StrubeExport.Models;
using Smartstore.StrubeExport.Settings;
using System;



namespace Smartstore.StrubeExport
{
    public class StrubeExportController : AdminController
    {
        private readonly ICommonServices _services;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly Lazy<IExportProfileService> _exportService;

        public StrubeExportController(
            ICommonServices services,
            IGenericAttributeService genericAttributeService,Lazy<IExportProfileService> exportProfileService)
        {
            _services = services;
            _genericAttributeService = genericAttributeService;
            _exportService = exportProfileService;
        }

        [SaveSetting]
        public ActionResult Configure(StrubeExportSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);



            return View(model);
        }


        [HttpPost]
        [SaveSetting]
        public ActionResult Configure(StrubeExportSettings settings, ConfigurationModel model) //, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }


            MiniMapper.Map(model, settings);
            return RedirectToPage("SmartStore.StrubeExport");
        }




    }
}