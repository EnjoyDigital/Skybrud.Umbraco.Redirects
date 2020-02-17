using Skybrud.Umbraco.Redirects.Import.Csv;
using Skybrud.Umbraco.Redirects.Models.Import;
using Skybrud.Umbraco.Redirects.Models.Import.File;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Skybrud.Umbraco.Redirects.Controllers.Api
{
    public partial class RedirectsController : UmbracoAuthorizedApiController
    {
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
                    Content = new StringContent("File must be a valid CSV or Excel file")
                });
            }

            var uploadFolder = HttpContext.Current.Server.MapPath(FileUploadPath);
            Directory.CreateDirectory(uploadFolder);
            var provider = new CustomMultipartFormDataStreamProvider(uploadFolder);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            var file = result.FileData[0];
            var path = file.LocalFileName;
            var ext = path.Substring(path.LastIndexOf('.')).ToLower();

            if (ext != ".csv" && ext != ".xlsx")
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.UnsupportedMediaType,
                    Content = new StringContent("File must be a valid CSV or Excel file")
                });
            }

            var fileNameAndPath = HttpContext.Current.Server.MapPath(FileUploadPath + string.Format(FileName, DateTime.Now.Ticks));

            File.Copy(file.LocalFileName, fileNameAndPath, true);

            var importer = new RedirectsImporterService();

            IRedirectsFile redirectsFile;

            switch (ext)
            {
                default:
                    var csvFile = new CsvRedirectsFile(new RedirectPublishedContentFinder(UmbracoContext.ContentCache))
                    {
                        FileName = fileNameAndPath,
                        Seperator = CsvSeparator.Comma
                    };

                    redirectsFile = csvFile;

                    break;
            }

            var response = importer.Import(redirectsFile);

            using (var ms = new MemoryStream())
            {
                using (var outputFile = new FileStream(response.File.FileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[outputFile.Length];
                    outputFile.Read(bytes, 0, (int)outputFile.Length);
                    ms.Write(bytes, 0, (int)outputFile.Length);

                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                    httpResponseMessage.Content = new ByteArrayContent(bytes.ToArray());
                    httpResponseMessage.Content.Headers.Add("x-filename", "redirects.csv");
                    httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = "redirects.csv";
                    httpResponseMessage.StatusCode = HttpStatusCode.OK;

                    return httpResponseMessage;
                }
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
    }
}
