using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Smartstore;
using Smartstore.Web.Components;
using SmartStore.StrubeExport.Models;
using System;
using System.Collections.Generic;

namespace SmartStore.StrubeExport.Components
{
    /// <summary>
    /// Component to render profile configuration of strube xlsx export.
    /// </summary>
    public class StrubeXlsxConfigurationViewComponent : SmartViewComponent
    {
        public IViewComponentResult Invoke(object data)
        {
            var model = data as ProfileConfigurationModel;

            ViewBag.LanguageSeoCode = Services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();
            //ViewBag.AvailableCategories = model.DefaultGoogleCategory.HasValue()
            //    ? new List<SelectListItem> { new SelectListItem { Text = model.DefaultGoogleCategory, Value = model.DefaultGoogleCategory, Selected = true } }
            //    : new List<SelectListItem>();

            return View(model);
        }
    }
}

