using System;
using System.Collections.Generic;
using System.Linq;
using Skybrud.Essentials.Time;
using Skybrud.Umbraco.Redirects.Extensions;
using Skybrud.Umbraco.Redirects.Import.Csv;
using Skybrud.Umbraco.Redirects.Models.Options;
using Umbraco.Web;

namespace Skybrud.Umbraco.Redirects.Models.Import.File
{
    public class CsvRedirectsFile : IRedirectsFile
    {
        public CsvRedirectsFile()
        {
            Seperator = CsvSeparator.Comma;
        }

        private readonly IRedirectPublishedContentFinder contentFinder;

        public CsvRedirectsFile(IRedirectPublishedContentFinder contentFinder)
        {
            this.contentFinder = contentFinder;
        }

        public string FileName { get; set; }

        public CsvSeparator Seperator { get; set; }

        public CsvFile File { get; private set; }

        public List<RedirectItem> Redirects { get; private set; }

        public List<ValidatedRedirectItem> ValidatedItems { get; private set; }

        /// <summary>
        /// Loads, parses and validates redirects
        /// </summary>
        public void Load()
        {
            File = CsvFile.Load(FileName, Seperator);

            File.Columns.AddColumn("Status");
            File.Columns.AddColumn("ErrorMessage");

            Redirects = File.Rows.Select(Parse).ToList();

            Validate();
        }

        /// <summary>
        /// Validates using task chains.
        /// </summary>
        private void Validate()
        {
            ValidatedItems = Redirects.Select(ValidateItems()).ToList();

            foreach (var item in ValidatedItems)
            {
                File.Rows[item.Index].AddCell(item.Status.ToString());
                File.Rows[item.Index].AddCell(string.Join(",", item.ValidationResults.Select(a => a.ErrorMessage)));
            }

            File.Save(FileName, Seperator);
        }

        private Func<RedirectItem, int, ValidatedRedirectItem> ValidateItems()
        {
            return (redirect, index) => RedirectItemValidationContext.Validate(index, redirect, Redirects.Where(a => !string.IsNullOrEmpty(a.LinkUrl) && !string.IsNullOrEmpty(a.Url)));
        }

        /// <summary>
        /// This is where a CsvRow gets parsed into a RedirectItem. The aim here is not to validate but 
        /// to get everything into a nicely typed model. It's not pretty mainly because of old skool 
        /// null checks.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private RedirectItem Parse(CsvRow row)
        {
            var redirectOptions = new AddRedirectOptions();

            redirectOptions.RootNodeId = 0;
            redirectOptions.IsPermanent = true;
            redirectOptions.IsRegex = false;
            redirectOptions.ForwardQueryString = true;

            var sourceUrlRaw = row.Cells[0] == null ? null : row.Cells[0].Value.Replace("\"",string.Empty).Trim();

            var sourceUrl = sourceUrlRaw.ToUri();

            if (sourceUrl != null)
            {
                //var lastSlash = sourceUrl.AbsolutePath.LastIndexOf('/');
                //var sourceUrlNoTrailingSlash = (lastSlash > 0) ? sourceUrl.AbsolutePath.Substring(0, lastSlash) : sourceUrl.AbsolutePath;

                redirectOptions.OriginalUrl = sourceUrl.AbsolutePath;

                //redirectOptions.QueryString = sourceUrl.Query.TrimStart('?');
            }

            var destinationUrlRaw = row.Cells[1] == null ? null : row.Cells[1].Value.Replace("\"", string.Empty).Trim();

            var destinationUrl = destinationUrlRaw.ToUri();

            RedirectDestinationType linkMode;
            var linkModeRaw = row.Cells[2].Value == null ? RedirectDestinationType.Url.ToString() : row.Cells[2].Value.Replace("\"", string.Empty).Trim();
            Enum.TryParse(linkModeRaw, out linkMode);

            if (destinationUrl != null)
            {
                var lastSlash = destinationUrl.AbsolutePath.LastIndexOf('/');
                var destUrlNoTrailingSlash = (lastSlash > 0) ? destinationUrl.AbsolutePath.Substring(0, lastSlash) : destinationUrl.AbsolutePath;

                var destinationUrlContent = contentFinder.Find(destUrlNoTrailingSlash);

                if (destinationUrlContent != null)
                {
                    redirectOptions.Destination = new RedirectDestination(destinationUrlContent.Id, Guid.Empty, destinationUrlContent.Url, linkMode);
                }
                else
                {
                    redirectOptions.Destination = new RedirectDestination(0, Guid.Empty, destinationUrl.AbsolutePath, linkMode);
                }
            }     

            var redirectItem = new RedirectItem(redirectOptions);

            return redirectItem;
        }
    }
}