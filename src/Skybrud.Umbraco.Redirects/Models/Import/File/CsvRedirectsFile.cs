using System;
using System.Collections.Generic;
using System.Linq;
using Skybrud.Essentials.Time;
using Skybrud.Umbraco.Redirects.Extensions;
using Skybrud.Umbraco.Redirects.Import.Csv;
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
            var redirectItemRow = new RedirectItemRow();

            redirectItemRow.UniqueId = Guid.NewGuid().ToString();
            redirectItemRow.RootNodeId = 0;
            redirectItemRow.IsPermanent = true;
            redirectItemRow.IsRegex = false;
            redirectItemRow.ForwardQueryString = false;
            redirectItemRow.Created = EssentialsDateTime.CurrentUnixTimestamp;
            redirectItemRow.Updated = EssentialsDateTime.CurrentUnixTimestamp;

            var sourceUrlRaw = row.Cells[0] == null ? null : row.Cells[0].Value.Replace("\"",string.Empty).Trim();

            var sourceUrl = sourceUrlRaw.ToUri();

            if (sourceUrl != null)
            {
                var lastSlash = sourceUrl.AbsolutePath.LastIndexOf('/');
                var sourceUrlNoTrailingSlash = (lastSlash > 0) ? sourceUrl.AbsolutePath.Substring(0, lastSlash) : sourceUrl.AbsolutePath;

                redirectItemRow.Url = sourceUrlNoTrailingSlash;

                redirectItemRow.QueryString = sourceUrl.Query.TrimStart('?');
            }

            var destinationUrlRaw = row.Cells[1] == null ? null : row.Cells[1].Value.Replace("\"", string.Empty).Trim();

            var destinationUrl = destinationUrlRaw.ToUri();

            if (destinationUrl != null)
            {
                redirectItemRow.LinkUrl = destinationUrl.AbsolutePath;
            }

            RedirectLinkMode linkMode;
            var linkModeRaw = row.Cells[2].Value == null ? RedirectLinkMode.Url.ToString() : row.Cells[2].Value.Replace("\"", string.Empty).Trim();
            Enum.TryParse(linkModeRaw, out linkMode);

            redirectItemRow.LinkMode = linkMode.ToString().ToLower();

            if (destinationUrl != null)
            {
                var lastSlash = destinationUrl.AbsolutePath.LastIndexOf('/');
                var destUrlNoTrailingSlash = (lastSlash > 0) ? destinationUrl.AbsolutePath.Substring(0, lastSlash) : destinationUrl.AbsolutePath;

                var urlContent = contentFinder.Find(destUrlNoTrailingSlash);

                if (urlContent != null)
                {
                    redirectItemRow.LinkMode = RedirectLinkMode.Content.ToString().ToLower();
                    redirectItemRow.LinkId = urlContent.Id;
                    redirectItemRow.LinkName = urlContent.Name;
                    redirectItemRow.LinkUrl = urlContent.Url;
                }
                else
                {
                    redirectItemRow.LinkUrl = destinationUrl.AbsolutePath;
                }
            }     

            var redirectItem = new RedirectItem(redirectItemRow);

            return redirectItem;
        }
    }
}