using System.Collections.Generic;
using System.Linq;

namespace Skybrud.Umbraco.Redirects.Models.Import.Validators
{
    /// <summary>
    /// Detects redirect chains and loops in the file. Chains are treated as warnings.
    /// They get imported but return with warnings on RedirectItemValidationResult. Loops are 
    /// not imported as they cause bugs and are treated as errors
    /// </summary>
    public class RedirectChainValidator : ValidatorBase
    {
        public override List<RedirectItemValidationResult> HandleValidation(RedirectItem redirectItem, IEnumerable<RedirectItem> otherRedirectItems)
        {
            var response = new RedirectItemValidationResult();

            var linkUrl = string.Empty;

            if (!string.IsNullOrEmpty(redirectItem.LinkUrl))
            {
                linkUrl = redirectItem.LinkUrl;
            }
        
            var destinationRedirect = otherRedirectItems.FirstOrDefault(item => item.Url == linkUrl && item.QueryString == redirectItem.QueryString && item.Id != redirectItem.Id);

            if (destinationRedirect != null)
            {
                if (destinationRedirect.LinkUrl == redirectItem.Url)
                {
                    response.Status = ImportErrorLevel.Error;
                    response.ErrorMessage = string.Format("This redirect would create a redirect loop as another redirect exists with the URL ({0}) in the file. It has not been imported.", redirectItem.LinkUrl);

                    ErrorsResult.Add(response);

                    return ErrorsResult;
                }

                response.Status = ImportErrorLevel.Warning;
                response.ErrorMessage = string.Format("This redirect links to the URL ({0}) in the file. This will result in a redirect chain", redirectItem.LinkUrl);

                ErrorsResult.Add(response);

                return ErrorsResult;
            }                     

            if (Successor != null)
            {
                return Successor.HandleValidation(redirectItem, otherRedirectItems);
            }

            ErrorsResult.Add(response);

            return ErrorsResult;
        }
    }
}