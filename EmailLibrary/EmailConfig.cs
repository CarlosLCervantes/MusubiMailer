using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailLib
{
    public class EmailConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public int? Port { get; set; }
        public bool? UseSSL { get; set; }
        public string xmlFolder { get; set; }
    }
}
