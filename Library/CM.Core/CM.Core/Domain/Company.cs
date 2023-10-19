using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CM.Core.Base;

namespace CM.Core.Domain
{
    public partial class Company : BaseDocument
    {
        public required string Name { get; set; }
        public int NumberOfEmployees { get; set; }

        [BsonExtraElements]
        public BsonDocument DynamicFields { get; set; }

    }
}
