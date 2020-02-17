using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    /// <summary>
    /// This class only really exists so I can test the looking up of Umbraco nodes without loads of mocking 
    /// out the Umbraco Context which is a faff
    /// </summary>
    public class RedirectPublishedContentFinder : IRedirectPublishedContentFinder
    {
        private readonly IPublishedContentCache _publishedContentCache;

        public RedirectPublishedContentFinder(IPublishedContentCache publishedContentCache)
        {
            _publishedContentCache = publishedContentCache;
        }

        public IPublishedContent Find(string url)
        {
            return _publishedContentCache.GetByRoute(url);
        }
    }
}