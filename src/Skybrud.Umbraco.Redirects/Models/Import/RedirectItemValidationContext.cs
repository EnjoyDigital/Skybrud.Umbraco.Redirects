using System.Collections.Generic;
using Skybrud.Umbraco.Redirects.Models.Import.Validators;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    /// <summary>
    /// Sets-up validators for redirect items imported from an external file
    /// </summary>
    public static class RedirectItemValidationContext
    {
        /// <summary>
        /// Validate redirect item. Uses a task chain to allow validators to be added easily.
        /// </summary>
        /// <param name="index">The index in the source document of the redirect</param>
        /// <param name="model">A redirect item</param>
        /// <param name="otherRedirectItems">All redirect items in the file</param>
        /// <returns></returns>
        public static ValidatedRedirectItem Validate(int index, RedirectItem model, IEnumerable<RedirectItem> otherRedirectItems)
        {
            var sourceUrlValidator = new SourceUrlValidator();

            var destinationUrlValidator = new DestinationUrlValidator();
            sourceUrlValidator.SetSuccessor(destinationUrlValidator);

            var redirectChainValidator = new RedirectChainValidator();
            destinationUrlValidator.SetSuccessor(redirectChainValidator);

            //var duplicateValidator = new DuplicateSourceUrlValidator();
            //redirectChainValidator.SetSuccessor(duplicateValidator);

            var results = sourceUrlValidator.HandleValidation(model, otherRedirectItems);

            var response = new ValidatedRedirectItem { Item = model, ValidationResults = results, Index = index };

            return response;
        }
    }
}