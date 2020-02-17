using System.Collections.Generic;
using System.Linq;

namespace Skybrud.Umbraco.Redirects.Models.Import.Validators
{
    public class DuplicateSourceUrlValidator : ValidatorBase
    {
        public override List<RedirectItemValidationResult> HandleValidation(RedirectItem redirectItem, IEnumerable<RedirectItem> otherRedirectItems)
        {
            var response = new RedirectItemValidationResult();
            var duplicateRedirect = otherRedirectItems.FirstOrDefault(item => item.Url == redirectItem.Url && item.QueryString == redirectItem.QueryString && item.Id != redirectItem.Id);

            if (duplicateRedirect != null)
            {                
                response.Status = ImportErrorLevel.Error;
                response.ErrorMessage = "This redirect has a duplicate row in the file with the same source URL. It has not been imported.";

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