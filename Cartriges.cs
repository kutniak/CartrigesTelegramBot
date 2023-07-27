using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartrigesTelegramBot
{
    public class Cartrige
    {
        [Key]
        public int CartridgeId { get; set; }
        public string CartridgeName { get; set; }
        public int CartridgeCount { get; set; }
    }
}
