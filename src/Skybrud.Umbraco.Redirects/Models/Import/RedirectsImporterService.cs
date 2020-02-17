using System;
using Skybrud.Umbraco.Redirects.Models.Import.Db;
using Skybrud.Umbraco.Redirects.Models.Import.File;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    public class ImportedEventArgs : EventArgs
    {
        public ImportedEventArgs(ImporterResponse response)
        {
            Response = response;
        }

        public ImporterResponse Response { get; private set; }
    }

    /// <summary>
    /// Umbraco style service that handles the imports and raises events
    /// </summary>
    public class RedirectsImporterService
    {
        public delegate void ImportedHandler(Object sender, ImportedEventArgs e);

        public event ImportedHandler Imported;

        public void RaiseEvent(ImporterResponse response)
        {
            var handler = Imported;

            if (handler != null)
            {
                var args = new ImportedEventArgs(response);

                handler(this, args);
            }
        }

        /// <summary>
        /// Imports a redirect file into the Skybrud redirect table
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual ImporterResponse Import(IRedirectsFile file)
        {
            var response = new ImporterResponse();
            
            file.Load();

            response.ImportedItems = file.ValidatedItems;
            response.File = file;

            var inserter = new SqlServerBulkCopy();

            inserter.BulkImport(file.ValidatedItems);

            RaiseEvent(response);

            return response;
        }
    }
}