using Domain.Exceptions;

namespace Application.Common.Models
{
    public class ApiResultModel<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Content { get; set; }
        public object Error { get; set; }


        public ApiResultModel(int code, string message, T content = default)
        {
            Code = code;
            Message = message;
            Content = content;
        }

        public ApiResultModel(T contet) : this(0, "Success", contet)
        {

        }

        public ApiResultModel()
        {

        }

        public ApiResultModel(Exception exception)
        {
            Code = 500;
            Message = ApiResultModel<T>.HandleInnerExceptions(exception);
            if (exception is DeliveryCoreException ex)
            {
                Code = ex.Code;
                Error = (exception as DeliveryCoreException).Data;
            }
        }

        private static string? HandleInnerExceptions(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            return $"{exception.Message}{(!string.IsNullOrEmpty(ApiResultModel<T>.HandleInnerExceptions(exception.InnerException)) ? $"{Environment.NewLine}{ApiResultModel<T>.HandleInnerExceptions(exception.InnerException)}" : "")}";
        }
    }
}
