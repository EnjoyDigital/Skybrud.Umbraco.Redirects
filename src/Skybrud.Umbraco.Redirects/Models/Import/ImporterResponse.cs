using System.Collections.Generic;
using System.Linq;
using Skybrud.Umbraco.Redirects.Models.Import.File;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    public class ImporterResponse
    {
        public ImporterResponse()
        {
            ImportedItems = Enumerable.Empty<ValidatedRedirectItem>();
        }

        public IRedirectsFile File { get; set; }

        public IEnumerable<ValidatedRedirectItem> ImportedItems { get; set;  }
    }
}