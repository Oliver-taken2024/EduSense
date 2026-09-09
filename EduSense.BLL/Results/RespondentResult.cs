using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.BLL.Results
{
    public enum RespondentResultStatus
    {
        Success,
        TokenNotFound,
        SurveyExpired
    }

    // Generisk resultat-wrapper: håller ihop status + payload istället för
    // att servicen kastar exceptions för flöden som inte är valideringsfel.
    public class RespondentResult<T>
    {
        public RespondentResultStatus Status { get; }
        public T? Value { get; }
        private RespondentResult(RespondentResultStatus status, T? value)
        {
            Status = status;
            Value = value;
        }

        public static RespondentResult<T> Success(T value) =>
            new(RespondentResultStatus.Success, value);

        public static RespondentResult<T> Failure(RespondentResultStatus status) =>
            new(status, default);
    }
}
