using System;

namespace SmartStore.StrubeExport.Models
{
    [CustomModelPart]
    [Serializable]
    public class ProfileConfigurationModel
    {
        [LocalizedDisplay("Plugins.SmartStore.StrubeExport.ExportShipAddress")]
        public bool ExportShipAddress { get; set; } = true;

        [LocalizedDisplay("Plugins.SmartStore.StrubeExport.SuppressPrice")]
        public bool SuppressPrice { get; set; } = false;

        [LocalizedDisplay("Plugins.SmartStore.StrubeExport.SuppressBank")]
        public bool SuppressBank { get; set; } = false;

    }
}