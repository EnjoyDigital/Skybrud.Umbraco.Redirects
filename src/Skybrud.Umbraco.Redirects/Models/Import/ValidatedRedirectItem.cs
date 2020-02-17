using System.Collections.Generic;
using System.Linq;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    /// <summary>
    /// Represents a validated redirect item. It could be valid or invalid or partially valid (has warnings).
    /// </summary>
    public class ValidatedRedirectItem
    {
        public RedirectItem Item { get; set; }

        public IEnumerable<RedirectItemValidationResult> ValidationResults { get; set; }

        public ImportErrorLevel Status
        {
            get
            {
                if (ValidationResults.Any(a => a.Status == ImportErrorLevel.Error))
                {
                    return ImportErrorLevel.Error;
                }

                if (ValidationResults.Any(a => a.Status == ImportErrorLevel.Warning))
                {
                    return ImportErrorLevel.Warning;
                }

                return  ImportErrorLevel.Success;
            }
        }

        public bool IsValid
        {
            get { return ValidationResults.All(result => result.Status != ImportErrorLevel.Error); }
        }

        public int Index { get; set; }
    }
}