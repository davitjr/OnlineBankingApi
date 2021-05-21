using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    public class RestConfigRequest
    {
        public List<DigitalAccountRestConfigurationItem> ConfigurationItems { get; set; }
    }
}
