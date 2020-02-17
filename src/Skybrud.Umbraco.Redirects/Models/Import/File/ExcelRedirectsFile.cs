using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FastExcel;
using Skybrud.Umbraco.Redirects.Extensions;
using Skybrud.Umbraco.Redirects.Import.Csv;
using Skybrud.Umbraco.Redirects.Models.Options;

namespace Skybrud.Umbraco.Redirects.Models.Import.File
{
    public class ExcelRedirectsFile : IRedirectsFile
    {
        public ExcelRedirectsFile()
        {
        }

        private readonly IRedirectPublishedContentFinder contentFinder;

        private FileInfo inputFile;

        public ExcelRedirectsFile(IRedirectPublishedContentFinder contentFinder)
        {
            this.contentFinder = contentFinder;
        }

        public string FileName { get; set; }

        public Worksheet File { get; private set; }

        public List<RedirectItem> Redirects { get; private set; }

        public List<ValidatedRedirectItem> ValidatedItems { get; private set; }

        /// <summary>
        /// Loads, parses and validates redirects
        /// </summary>
        public void Load()
        {
            inputFile = new FileInfo(FileName);

            Worksheet worksheet = null;

            using (FastExcel.FastExcel fastExcel = new FastExcel.FastExcel(inputFile, true))
            {
                File = fastExcel.Read(1);
                Redirects = File.Rows.Select(Parse).ToList();
            }

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
                //TODO: bit odd
                //File.AddValue(item.Index+1,5,item.Status.ToString());
                //File.AddValue(item.Index+1, 5, string.Join(",", item.ValidationResults.Select(a => a.ErrorMessage)));
            }

            //using (FastExcel.FastExcel fastExcel = new FastExcel.FastExcel(inputFile))
            //{
            //    fastExcel.Write(File);
            //}
        }

        private Func<RedirectItem, int, ValidatedRedirectItem> ValidateItems()
        {
            return (redirect, index) => RedirectItemValidationContext.Validate(index, redirect, Redirects.Where(a => !string.IsNullOrEmpty(a.LinkUrl) && !string.IsNullOrEmpty(a.Url)));
        }

        /// <summary>
        /// This is where an Excel Row gets parsed into a RedirectItem. The aim here is not to validate but 
        /// to get everything into a nicely typed model. It's not pretty mainly because of old skool 
        /// null checks.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private RedirectItem Parse(Row row)
        {
            var redirectOptions = new AddRedirectOptions();

            redirectOptions.RootNodeId = 0;
            redirectOptions.IsPermanent = true;
            redirectOptions.IsRegex = false;
            redirectOptions.ForwardQueryString = true;

            ParseSourceUrl(row.GetCellByColumnName("A").Value.ToString(), redirectOptions);

            var destinationUrlRaw = row.GetCellByColumnName("B").Value == null ? null : row.GetCellByColumnName("B").Value.ToString().Trim();

            var destinationUrl = destinationUrlRaw.ToUri();

            RedirectDestinationType linkMode;
            var linkModeRaw = row.GetCellByColumnName("C").Value == null ? RedirectDestinationType.Url.ToString() : row.GetCellByColumnName("C").Value.ToString().Trim();
            Enum.TryParse(linkModeRaw, out linkMode);

            redirectOptions.Destination.Type = linkMode;

            if (destinationUrl != null)
            {
                var urlContent = contentFinder.Find(destinationUrl.AbsolutePath);

                if (urlContent != null)
                {
                    redirectOptions.Destination = new RedirectDestination(urlContent.Id, Guid.Empty, urlContent.Url, linkMode);

                    //redirectOptions. = RedirectDestinationType.Content.ToString().ToLower();
                    //redirectOptions.LinkId = urlContent.Id;
                    //redirectOptions.LinkName = urlContent.Name;
                    //redirectOptions.LinkUrl = urlContent.Url;
                }
                else
                {
                    redirectOptions.Destination = new RedirectDestination(0, Guid.Empty, destinationUrl.AbsolutePath, linkMode);
                }
            }

            var redirectItem = new RedirectItem(redirectOptions);

            return redirectItem;
        }

        private static void ParseSourceUrl(string url, AddRedirectOptions redirectOptions)
        {
            var sourceUrlRaw = url == null
                ? null
                : url.Trim();

            var sourceUrl = sourceUrlRaw.ToUri();

            if (sourceUrl != null)
            {
                redirectOptions.OriginalUrl = sourceUrl.AbsolutePath;
            }
        }
    }
}