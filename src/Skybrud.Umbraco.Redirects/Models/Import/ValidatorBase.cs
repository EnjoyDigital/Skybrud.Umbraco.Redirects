using System.Collections.Generic;

namespace Skybrud.Umbraco.Redirects.Models.Import
{
    /// <summary>
    /// Base validator class. All validators should implement HandleValidation
    /// </summary>
    public abstract class ValidatorBase
    {
        protected ValidatorBase Successor { get; private set; }

        protected List<RedirectItemValidationResult> ErrorsResult { get; set; }

        protected ValidatorBase()
        {
            ErrorsResult = new List<RedirectItemValidationResult>();
        }

        public abstract List<RedirectItemValidationResult> HandleValidation(RedirectItem model, IEnumerable<RedirectItem> otherRedirectItems);

        public void SetSuccessor(ValidatorBase successor)
        {
            this.Successor = successor;
        }
    }
}