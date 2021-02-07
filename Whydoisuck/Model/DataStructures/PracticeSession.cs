﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whydoisuck.Model.DataStructures
{
    public class PracticeSession : ISession
    {
        /// <summary>
        /// Level the session was played on
        /// </summary>
        [JsonProperty(PropertyName = "Level")] public Level Level { get; set; }
        /// <summary>
        /// True if the session was played from a startpos.
        /// </summary>
        [JsonProperty(PropertyName = "IsCopyRun")] public bool IsCopyRun { get; set; }
        /// <summary>
        /// Time the session was started at
        /// </summary>
        [JsonProperty(PropertyName = "StartTime")] public DateTime StartTime { get; set; }
        /// <summary>
        /// How long was the session
        /// </summary>
        [JsonProperty(PropertyName = "Duration")] public TimeSpan Duration { get; set; }
        /// <summary>
        /// All the attempts in the practice session
        /// </summary>
        [JsonProperty(PropertyName = "Attempts")] public List<PracticeAttempt> Attempts { get; set; }
    }
}
