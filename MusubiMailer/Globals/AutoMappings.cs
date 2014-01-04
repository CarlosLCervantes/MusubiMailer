using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;

namespace MusubiMailer
{
    public class AutoMappings
    {
        public static void Initialize()
        {
            Mapper.CreateMap<DAL.Email, EmailFileRecord>();
            Mapper.CreateMap<EmailFileRecord, DAL.Email>();
        }
    }
}
