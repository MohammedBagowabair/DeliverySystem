namespace Domain.Exceptions
{
    public class DeliveryCoreException(int errorCode) : Exception
    {
        private readonly string _message = errorCode switch
        {
            0 => string.Empty,
            ErrorCodes.DATABASE_UNIQUE_CONSTRAINT_VIOLATED_CODE => ErrorCodes.DATABASE_UNIQUE_CONSTRAINT_VIOLATED_MESSAGE,
            ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE => ErrorCodes.DATABASE_RECORD_NOT_FOUND_MESSAGE,
            ErrorCodes.NOT_ALL_RECORDS_DELETED_CODE => ErrorCodes.NOT_ALL_RECORDS_DELETED_MESSAGE,
            ErrorCodes.USER_ALREADY_EXISTS_CODE => ErrorCodes.USER_ALREADY_EXISTS_MESSAGE,
            ErrorCodes.USER_NOT_FOUND_CODE => ErrorCodes.USER_NOT_FOUND_MESSAGE,
            ErrorCodes.UNABLE_TO_OPEN_FILE_NOT_FOUND_CODE => ErrorCodes.UNABLE_TO_OPEN_FILE_NOT_FOUND_MESSAGE,
            ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE => ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_MESSAGE,
            ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE => ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_MESSAGE,
            ErrorCodes.DB_UNABLE_TO_DELETE_RECORD_CODE => ErrorCodes.DB_UNABLE_TO_DELETE_RECORD_MESSAGE,
            ErrorCodes.USER_PASSWORD_IS_NULL_CODE => ErrorCodes.USER_PASSWORD_IS_NULL_CODE_MESSAGE,
            ErrorCodes.USER_VERIFY_CODE_NOT_FOUND => ErrorCodes.USER_VERIFY_CODE_NOT_FOUND_MESSAGE,
            ErrorCodes.USER_OTP_NOT_MATCHED_CODE => ErrorCodes.USER_OTP_NOT_MATCHED_CODE_MESSAGE,
            ErrorCodes.USER_ACCOUNT_INACTIVE_CODE => ErrorCodes.USER_ACCOUNT_INACTIVE_MESSAGE,
            ErrorCodes.ROLE_NAME_NOT_FOUND => ErrorCodes.ROLE_NAME_NOT_FOUND_MESSAGE,
            ErrorCodes.DB_ENTITY_IS_INACTIVE_CODE => ErrorCodes.DB_ENTITY_IS_INACTIVE_MESSAGE,
            ErrorCodes.INVALID_USERNAME_OR_EMAIL_ADDRESS_CODE => ErrorCodes.INVALID_USERNAME_OR_EMAIL_ADDRESS_MESSAGE,
            //ErrorCodes.EVENT_NOT_FOUND_CODE => ErrorCodes.EVENT_NOT_FOUND_MESSAGE,
            //ErrorCodes.TICKET_NOT_FOUND_CODE => ErrorCodes.TICKET_NOT_FOUND_MESSAGE,
            //ErrorCodes.PERMISSION_DENIED_CODE => ErrorCodes.PERMISSION_DENIED_MESSAGE,
            ErrorCodes.IMAGE_DIDNT_SAVE => ErrorCodes.IMAGE_DIDNT_SAVE_MESSAGE,

            _ => ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE,
        };
        public int Code { get; private set; } = errorCode;
        public new object Data { get; set; }

        public override string Message => _message;
        public DeliveryCoreException(int errorCode, object data) : this(errorCode)
        {
            Data = data;
        }
        public DeliveryCoreException(string message, int errorCode, object data) : this(errorCode)
        {
            _message = $"{_message}{(!string.IsNullOrEmpty(message) ? $"{Environment.NewLine}{message}" : "")}";
            Data = data;
        }

        public DeliveryCoreException(int errorCode, object data, params object[] args) : this(errorCode)
        {
            _message = string.Format(_message, args);
            Data = data;
        }
    }
}
