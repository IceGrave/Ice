using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ydis.Model.DataStructures;

namespace Ydis.Model.UserSettings
{
    public class SerializedSettings
    {
        /// <summary>
        /// Default folder for data saving
        /// </summary>
        [JsonProperty(PropertyName = "SavesPath")] public string SavesPath { get; set; } = Path.Combine(YDISSettings.AppData, "records");
        /// <summary>
        /// How many milliseconds there are between each scan of the game's memory
        /// </summary>
        [JsonProperty(PropertyName = "ScanPeriod")] public int ScanPeriod { get; set; } = YDISSettings.PERFORMANCE_PERIOD;
    }
}
