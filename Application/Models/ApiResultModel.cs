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

        public ApiResultModel(T contet) : this(0, "نجاح", contet)
        {

        }

        public ApiResultModel()
        {

        }

        public ApiResultModel(Exception exception)
        {
            if (exception is DeliveryCoreException domain)
            {
                Code = domain.Code;
                Message = domain.Message;
                Error = domain.Data;
            }
            else
            {
                Code = ErrorCodes.SYS_UNKNOWN_ERROR_CODE;
                Message = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE;
            }
        }
    }
}
