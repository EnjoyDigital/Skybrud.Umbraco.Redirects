﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Skybrud.Umbraco.Redirects.Exceptions;
using Skybrud.Umbraco.Redirects.Import.Csv;
using Skybrud.Umbraco.Redirects.Models;
using Skybrud.WebApi.Json;
using Skybrud.WebApi.Json.Meta;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebApi;

namespace Skybrud.Umbraco.Redirects.Controllers.Api {
    
    [JsonOnlyConfiguration]
    public class RedirectsController : UmbracoAuthorizedApiController {

        private CultureInfo _culture;

        #region Properties

        protected RedirectsRepository Repository = new RedirectsRepository();

        /// <summary>
        /// Gets a reference to the culture of the authenticated user.
        /// </summary>
        public CultureInfo Culture {
            // TODO: Is the language reliable for determining the culture?
            get { return _culture ?? (_culture = new CultureInfo(Security.CurrentUser.Language)); }
        }

        #endregion

        #region Public API methods

        [HttpGet]
        public object GetDomains() {
            RedirectDomain[] domains = Repository.GetDomains();
            return new {
                total = domains.Length,
                data = domains
            };
        }

        /// <summary>
        /// Gets a list of root nodes based on the domains added to Umbraco. A root node will only be included in the
        /// list once - even if it has been assigned multiple domains.
        /// </summary>
        [HttpGet]
        public object GetRootNodes() {
            
            RedirectDomain[] domains = Repository.GetDomains();

            List<RedirectRootNode> temp = new List<RedirectRootNode>();

            foreach (RedirectDomain domain in domains.Where(x => x.RootNodeId > 0).DistinctBy(x => x.RootNodeId)) {
                
                // Get the root node from the content service
                IContent content = ApplicationContext.Services.ContentService.GetById(domain.RootNodeId);
                
                // Skip if not found via the content service
                if (content == null) continue;
                
                // Skip if the root node is located in the recycle bin
                if (content.Path.StartsWith("-1,-20,")) continue;
                
                // Append the root node to the result
                temp.Add(RedirectRootNode.GetFromContent(content));
            
            }

            return new {
                total = temp.Count,
                data = temp.OrderBy(x => x.Id)
            };
        
        }
        
        [HttpGet]
        public object GetRedirects(int page = 1, int limit = 20, string type = null, string text = null, int? rootNodeId = null) {
            try {
                return Repository.GetRedirects(page, limit, type, text, rootNodeId);
            } catch (RedirectsException ex) {
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        public object GetRedirectsForContent(int contentId) {

            try {
                
                // Get a reference to the content item
                IContent content = ApplicationContext.Services.ContentService.GetById(contentId);

                // Trigger an exception if the content item couldn't be found
                if (content == null) throw new RedirectsException(HttpStatusCode.NotFound, Localize("redirects/errorContentNoRedirects"));
                
                // Generate the response
                return JsonMetaResponse.GetSuccess(new {
                    content = new {
                        id = content.Id,
                        name = content.Name
                    },
                    redirects = Repository.GetRedirectsByContentId(contentId)
                });
            
            } catch (RedirectsException ex) {
                
                // Generate the error response
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            
            }
        
        }

        [HttpGet]
        public object GetRedirectsForMedia(int contentId) {

            try {

                // Get a reference to the media item
                IMedia media = ApplicationContext.Services.MediaService.GetById(contentId);

                // Trigger an exception if the media item couldn't be found
                if (media == null) throw new RedirectsException(HttpStatusCode.NotFound, Localize("redirects/errorMediaNoRedirects"));

                // Generate the response
                return JsonMetaResponse.GetSuccess(new {
                    media = new {
                        id = media.Id,
                        name = media.Name
                    },
                    redirects = Repository.GetRedirectsByMediaId(contentId)
                });
            
            } catch (RedirectsException ex) {

                // Generate the error response
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            
            }

        }

        [HttpGet]
        public object AddRedirect(int rootNodeId, string url, string linkMode, int linkId, string linkUrl, string linkName = null, bool permanent = true, bool regex = false, bool forward = false) {

            try {
                
                // Some input validation
                if (String.IsNullOrWhiteSpace(url)) throw new RedirectsException(Localize("redirects/errorNoUrl"));
                if (String.IsNullOrWhiteSpace(linkUrl)) throw new RedirectsException(Localize("redirects/errorNoDestination"));
                if (String.IsNullOrWhiteSpace(linkMode)) throw new RedirectsException(Localize("redirects/errorNoDestination"));

                // Parse the link mode
                RedirectLinkMode mode;
                switch (linkMode) {
                    case "content": mode = RedirectLinkMode.Content; break;
                    case "media": mode = RedirectLinkMode.Media; break;
                    case "url": mode = RedirectLinkMode.Url; break;
                    default: throw new RedirectsException(Localize("redirects/errorUnknownLinkMode"));
                }

                // Initialize a new link item
                RedirectLinkItem destination = new RedirectLinkItem(linkId, linkName, linkUrl, mode);

                // Add the redirect
                RedirectItem redirect =  Repository.AddRedirect(rootNodeId, url, destination, permanent, regex, forward);

                // Return the redirect
                return redirect;

            } catch (RedirectsException ex) {

                // Generate the error response
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            
            }

        }

        [HttpGet]
        public object EditRedirect(int rootNodeId, string redirectId, string url, string linkMode, int linkId, string linkUrl, string linkName = null, bool permanent = true, bool regex = false, bool forward = false) {

            try {

                // Get a reference to the redirect
                RedirectItem redirect = Repository.GetRedirectById(redirectId);
                if (redirect == null) throw new RedirectNotFoundException();

                // Some input validation
                if (String.IsNullOrWhiteSpace(url)) throw new RedirectsException(Localize("redirects/errorNoUrl"));
                if (String.IsNullOrWhiteSpace(linkUrl)) throw new RedirectsException(Localize("redirects/errorNoDestination"));
                if (String.IsNullOrWhiteSpace(linkMode)) throw new RedirectsException(Localize("redirects/errorNoDestination"));

                // Parse the link mode
                RedirectLinkMode mode;
                switch (linkMode) {
                    case "content": mode = RedirectLinkMode.Content; break;
                    case "media": mode = RedirectLinkMode.Media; break;
                    case "url": mode = RedirectLinkMode.Url; break;
                    default: throw new RedirectsException(Localize("redirects/errorUnknownLinkMode"));
                }

                // Initialize a new link item
                RedirectLinkItem destination = new RedirectLinkItem(linkId, linkName, linkUrl, mode);

                // Split the URL and query string
                string[] urlParts = url.Split('?');
                url = urlParts[0].TrimEnd('/');
                string query = urlParts.Length == 2 ? urlParts[1] : "";

                // Update the properties of the redirect
                redirect.RootNodeId = rootNodeId;
                redirect.Url = url;
                redirect.QueryString = query;
                redirect.Link = destination;
                redirect.IsPermanent = permanent;
				redirect.IsRegex = regex;
                redirect.ForwardQueryString = forward;
                
                // Save/update the redirect
                Repository.SaveRedirect(redirect);

                // Return the redirect
                return redirect;

            } catch (RedirectsException ex) {

                // Generate the error response
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            
            }

        }

        /// <summary>
        /// Deletes the redirect with the specified <paramref name="redirectId"/>.
        /// </summary>
        /// <param name="redirectId">The ID of the redirect.</param>
        [HttpGet]
        public object DeleteRedirect(string redirectId) {

            try {

                // Get a reference to the redirect
                RedirectItem redirect = Repository.GetRedirectById(redirectId);
                if (redirect == null) throw new RedirectNotFoundException();

                // Delete the redirect
                Repository.DeleteRedirect(redirect);

                // Return the redirect
                return redirect;

            } catch (RedirectsException ex) {

                // Generate the error response
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, ex.Message));
            
            }

        }

        private const string FileUploadPath = "~/App_Data/TEMP/FileUploads/";
        private const string FileName = "redirects{0}.csv";

        [HttpPost]
        public async Task<HttpResponseMessage> Import()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.UnsupportedMediaType,
                    Content = new StringContent("File must be a valid CSV file")
                });
            }

            var uploadFolder = HttpContext.Current.Server.MapPath(FileUploadPath);
            Directory.CreateDirectory(uploadFolder);
            var provider = new CustomMultipartFormDataStreamProvider(uploadFolder);
            var result = await Request.Content.ReadAsMultipartAsync(provider);
            var file = result.FileData[0];
            var path = file.LocalFileName;
            var ext = path.Substring(path.LastIndexOf('.')).ToLower();

            if (ext != ".csv")
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.UnsupportedMediaType,
                    Content = new StringContent("File must be a valid CSV file")
                });
            }

            var fileNameAndPath = HttpContext.Current.Server.MapPath(FileUploadPath + string.Format(FileName, DateTime.Now.Ticks));

            System.IO.File.Copy(file.LocalFileName, fileNameAndPath, true);

            try
            {
                var importer = new RedirectsImporter(UmbracoContext);

                importer.Import(fileNameAndPath, CsvSeparator.Comma);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message, ex);
            }
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            }
        }

        #endregion

        #region Private helper methods

        private string Localize(string key) {
            return Services.TextService.Localize(key, Culture);
        }

        #endregion

    }

}