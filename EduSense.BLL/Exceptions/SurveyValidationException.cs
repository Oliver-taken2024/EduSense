using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.BLL.Exceptions
{
    public class SurveyValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public SurveyValidationException(IReadOnlyList<string> errors)
            : base(string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
