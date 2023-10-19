using CM.Core.Base;
using CM.Core.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM.Core.Domain
{

    public partial class DocumentSchema : BaseDocument
    {
        public required string CollectionName { get; set; }
        public required string FieldName { get; set; }     
        public required FieldDataType FieldType { get; set; } 
    }
}
