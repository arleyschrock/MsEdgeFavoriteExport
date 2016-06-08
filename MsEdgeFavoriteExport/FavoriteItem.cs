using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsEdgeFavoriteExport
{
    public class FavoriteItem
    {
        public int Collection { get; set; }
        public int SchemaVersion { get; set; }
        public string ItemId { get; set; }
        public string ParentId { get; set; }
        public long OrderNumber { get; set; }
        public bool IsFolder { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public long DateUpdatedLow { get; set; }
        public int DateUpdatedHigh { get; set; }
        public string FaviconFileContent { get; set; }
    }

}
