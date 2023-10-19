using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM.Core.Domain.Settings
{
    public partial class MongoDatabaseConfig
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public string ConnectionString => $"mongodb://{Host}:{Port}";

    }
}
