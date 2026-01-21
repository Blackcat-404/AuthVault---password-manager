using System.Collections.Generic;

namespace PasswordManager.Application
{
    public class Result
    {
        protected readonly Dictionary<string, string> _errors = new();

        public bool Success => _errors.Count == 0;
        public IReadOnlyDictionary<string, string> Errors => _errors;

        public void AddError(string field, string message)
        {
            if (!_errors.ContainsKey(field))
                _errors[field] = message;
        }
    }
}
