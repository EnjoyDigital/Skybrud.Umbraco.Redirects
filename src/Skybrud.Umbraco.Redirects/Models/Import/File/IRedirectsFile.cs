using System.Collections.Generic;

namespace Skybrud.Umbraco.Redirects.Models.Import.File
{
    public interface IRedirectsFile
    {
        string FileName { get; }

        void Load();



        List<RedirectItem> Redirects { get; }

        List<ValidatedRedirectItem> ValidatedItems { get; }
    }
}