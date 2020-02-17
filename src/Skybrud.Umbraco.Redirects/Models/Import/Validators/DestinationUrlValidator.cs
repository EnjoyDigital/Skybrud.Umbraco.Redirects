using System.Collections.Generic;

namespace Skybrud.Umbraco.Redirects.Models.Import.Validators
{
    public class DestinationUrlValidator : ValidatorBase
    {
        public override List<RedirectItemValidationResult> HandleValidation(RedirectItem redirectItem, IEnumerable<RedirectItem> otherRedirectItems)
        {
            var response = new RedirectItemValidationResult();

            if (string.IsNullOrEmpty(redirectItem.LinkUrl))
            {
                response.Status = ImportErrorLevel.Error;
                response.ErrorMessage = "No destination URL was provided or is in the wrong format";

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