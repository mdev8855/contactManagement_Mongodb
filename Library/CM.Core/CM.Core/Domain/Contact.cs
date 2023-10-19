using CM.Core.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace CM.Core.Domain
{
    public partial class Contact : BaseDocument
    {
        public required string Name { get; set; }

        [BsonExtraElements]
        public BsonDocument DynamicFields { get; set; }

        public List<string> Companies { get; set; } = new List<string>();

    }
}
