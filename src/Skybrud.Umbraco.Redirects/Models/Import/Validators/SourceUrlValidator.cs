using System.Collections.Generic;

namespace Skybrud.Umbraco.Redirects.Models.Import.Validators
{
    /// <summary>
    /// Checks that the redirect has a source URL
    /// </summary>
    public class SourceUrlValidator : ValidatorBase
    {
        /// <summary>
        /// Handles validation for Source Url
        /// </summary>
        /// <param name="redirectItem">A redirect item</param>
        /// <param name="otherRedirectItems">All redirect items in the file</param>
        /// <returns></returns>
        public override List<RedirectItemValidationResult> HandleValidation(RedirectItem redirectItem, IEnumerable<RedirectItem> otherRedirectItems)
        {
            var response = new RedirectItemValidationResult();
            
            if (string.IsNullOrEmpty(redirectItem.Url))         
            {
                response.Status = ImportErrorLevel.Error;
                response.ErrorMessage = "No source URL was provided or is in the wrong format";

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