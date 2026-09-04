using System;
using System.Collections.Generic;

namespace EduSense.UI.Services
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public IReadOnlyList<string> Errors { get; }

        public ApiException(int statusCode, IReadOnlyList<string> errors)
            : base(string.Join(" ", errors))
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
