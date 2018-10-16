using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luci.Services
{
    public static class ConfigHelper
    {

        public static IConfiguration _configuration { get; set; }

        public static void Start(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
    }
}
