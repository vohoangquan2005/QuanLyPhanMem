using Model;
using System;

namespace Services
{
    public class ErrorService : IErrorService
    {
        public Error CreateError(string message, string stackTrace)
        {
            return new Error
            {
                Message = message,
                StackTrace = stackTrace,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}