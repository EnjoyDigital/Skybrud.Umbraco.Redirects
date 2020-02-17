using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    public interface IRedirectPublishedContentFinder
    {
        IPublishedContent Find(string url);
    }
}