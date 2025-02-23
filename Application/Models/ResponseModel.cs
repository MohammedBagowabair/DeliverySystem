using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class ResponseModel
    {
        [Description("Response code. 0 is success, others are errors")]
        public int Code { get; set; }
        [Description("Response Message")]
        public string Message { get; set; }
        public Object Content { get; set; }

        public ResponseModel(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public ResponseModel(Object content) : this(0, "Success")
        {
            this.Content = content;
        }

        public ResponseModel(DeliveryCoreException e) : this(e.Code, e.Message)
        {
            this.Content = e.StackTrace;
        }

        public ResponseModel()
        {
        }

    }

}
