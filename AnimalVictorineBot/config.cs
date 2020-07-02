using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalVictorineBot
{

    public class appconfig
    {
        public app_config[] config { get; set; }
    }
    public class app_config
    {
        public string token { get; set; }
        public string channelId { get; set; }
    }
}
