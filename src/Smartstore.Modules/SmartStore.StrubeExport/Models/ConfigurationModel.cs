using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Smartstore.StrubeExport.Models
{
    public class ConfigurationModel : ModelBase
    {


        [LocalizedDisplay("Plugins.SmartStore.StrubeExport.MyFirstSetting")]
        public string MyFirstSetting { get; set; }



    }


}