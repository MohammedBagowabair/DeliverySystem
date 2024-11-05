namespace Domain.Exceptions
{
    public class ErrorCodes
    {
        //Auth
        public const int USER_NOT_FOUND_CODE = 9000;
        internal const string USER_NOT_FOUND_MESSAGE = "Invalid username or password";

        public const int USER_PASSWORD_NOT_MATCHED_CODE = 9001;
        internal const string USER_PASSWORD_NOT_MATCHED_MESSAGE = "Password is not matched";

        public const int ADMIN_INVALID_OTP_CODE = 9002;
        internal const string ADMIN_INVALID_OTP_MESSAGE = "Invalid One Time Password (OTP)";

        public const int INVALID_USERNAME_OR_EMAIL_ADDRESS_CODE = 9002;
        internal const string INVALID_USERNAME_OR_EMAIL_ADDRESS_MESSAGE = "Invalid username or email address";

        public const int USER_ACCOUNT_INACTIVE_CODE = 9003;
        internal const string USER_ACCOUNT_INACTIVE_MESSAGE = "User account is inactive";

        public const int USER_IS_NOT_AUTHORIZED_CODE = 9004;
        internal const string USER_IS_NOT_AUTHORIZED_MESSAGE = "User is not authorized to access the resource";

        public const int USER_ALREADY_EXISTS_CODE = 9005;
        internal const string USER_ALREADY_EXISTS_MESSAGE = "User is already exists";

        public const int USER_PASSWORD_IS_NULL_CODE = 9006;
        internal const string USER_PASSWORD_IS_NULL_CODE_MESSAGE = "User Password is Null";

        public const int USER_VERIFY_CODE_NOT_FOUND = 9007;
        internal const string USER_VERIFY_CODE_NOT_FOUND_MESSAGE = "User OTP Not Found!";

        public const int USER_OTP_NOT_MATCHED_CODE = 908;
        internal const string USER_OTP_NOT_MATCHED_CODE_MESSAGE = "User OTP is Not Correct!";

        public const int ROLE_NAME_NOT_FOUND = 9009;
        internal const string ROLE_NAME_NOT_FOUND_MESSAGE = "InValid Role";
        ///
        /// Database
        /// 
        public const int DATABASE_UNIQUE_CONSTRAINT_VIOLATED_CODE = 5001;
        internal const string DATABASE_UNIQUE_CONSTRAINT_VIOLATED_MESSAGE = "Unique constraint vaiolated";

        public const int DATABASE_RECORD_NOT_FOUND_CODE = 5002;
        internal const string DATABASE_RECORD_NOT_FOUND_MESSAGE = "Record is not found";

        public const int NOT_ALL_RECORDS_DELETED_CODE = 5003;
        internal const string NOT_ALL_RECORDS_DELETED_MESSAGE = "Some records can't be deleted";

        public const int UNABLE_TO_OPEN_FILE_NOT_FOUND_CODE = 5004;
        internal const string UNABLE_TO_OPEN_FILE_NOT_FOUND_MESSAGE = "Unable to open file. File not found";

        public const int DB_UNABLE_TO_INSERT_RECORD_CODE = 5005;
        internal const string DB_UNABLE_TO_INSERT_RECORD_MESSAGE = "Unable to insert data";

        public const int DB_UNABLE_TO_UPDATE_RECORD_CODE = 5006;
        internal const string DB_UNABLE_TO_UPDATE_RECORD_MESSAGE = "Unable to update data";

        public const int DB_UNABLE_TO_DELETE_RECORD_CODE = 5007;
        internal const string DB_UNABLE_TO_DELETE_RECORD_MESSAGE = "Unable to delete data";

        public const int DB_ENTITY_IS_INACTIVE_CODE = 5008;
        internal const string DB_ENTITY_IS_INACTIVE_MESSAGE = "{0}:{1} is inacive.";

        public const int UNABLE_TO_SEND_WHATSAPP_OTP_CODE = 5009;
        internal const string UNABLE_TO_SEND_WHATSAPP_OTP_MESSAGE = "Unable To Send WhatsApp OTP Code";


        ///System
        ///


        public const int SYS_UNKNOWN_ERROR_CODE = 5999;
        internal const string SYS_UNKNOWN_ERROR_MESSAGE = "Unknow Error";


        public const int IMAGE_DIDNT_SAVE = 1012;
        internal const string IMAGE_DIDNT_SAVE_MESSAGE = "Proplem in saving the image in the directory";

    }
}
