
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seebuff {
    public class BuffList {
        public int buffID { get; }
        public DateTime time { get; }
        public Dictionary<int, DateTime> zidian { get { return 字典; } set { 字典 = zidian; } }
        public  BuffList (int x, float y)
        {
            var time1 = DateTime.Now;
            this.buffID = x;
            this.time = time1.AddSeconds(y);
            字典.Add(this.buffID, this.time);
        }

        public static Dictionary<int, DateTime> 字典 = new Dictionary<int, DateTime>
        {
        };
    }
}
