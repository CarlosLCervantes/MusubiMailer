using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusubiMailer
{
    public class EmailFileRecord
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ExtraData { get; set; }

        public EmailFileRecord(string fname, string lname, string address, string extraData)
        {
            this.FirstName = fname;
            this.LastName = lname;
            this.Address = address;
            this.ExtraData = extraData;
        }
    }
}
