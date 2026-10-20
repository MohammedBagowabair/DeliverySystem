namespace Domain.Exceptions
{
    public class ErrorCodes
    {
        //Auth
        public const int USER_NOT_FOUND_CODE = 9000;
        internal const string USER_NOT_FOUND_MESSAGE = "اسم المستخدم أو كلمة المرور غير صحيحة";

        public const int USER_PASSWORD_NOT_MATCHED_CODE = 9001;
        internal const string USER_PASSWORD_NOT_MATCHED_MESSAGE = "كلمة المرور غير متطابقة";

        public const int INVALID_USERNAME_OR_EMAIL_ADDRESS_CODE = 9002;
        internal const string INVALID_USERNAME_OR_EMAIL_ADDRESS_MESSAGE = "اسم المستخدم أو البريد الإلكتروني غير صالح";

        public const int USER_ACCOUNT_INACTIVE_CODE = 9003;
        internal const string USER_ACCOUNT_INACTIVE_MESSAGE = "حساب المستخدم غير نشط";

        public const int USER_IS_NOT_AUTHORIZED_CODE = 9004;
        internal const string USER_IS_NOT_AUTHORIZED_MESSAGE = "غير مصرح لك بالوصول إلى هذا المورد";

        public const int USER_ALREADY_EXISTS_CODE = 9005;
        internal const string USER_ALREADY_EXISTS_MESSAGE = "المستخدم موجود مسبقاً";

        public const int USER_PASSWORD_IS_NULL_CODE = 9006;
        internal const string USER_PASSWORD_IS_NULL_CODE_MESSAGE = "كلمة المرور مطلوبة";

        public const int USER_VERIFY_CODE_NOT_FOUND = 9007;
        internal const string USER_VERIFY_CODE_NOT_FOUND_MESSAGE = "رمز التحقق غير موجود";

        public const int USER_OTP_NOT_MATCHED_CODE = 908;
        internal const string USER_OTP_NOT_MATCHED_CODE_MESSAGE = "رمز التحقق غير صحيح";

        public const int ROLE_NAME_NOT_FOUND = 9009;
        internal const string ROLE_NAME_NOT_FOUND_MESSAGE = "الدور غير صالح";

        /// Database
        public const int DATABASE_UNIQUE_CONSTRAINT_VIOLATED_CODE = 5001;
        internal const string DATABASE_UNIQUE_CONSTRAINT_VIOLATED_MESSAGE = "القيمة موجودة مسبقاً (تكرار غير مسموح)";

        public const int DATABASE_RECORD_NOT_FOUND_CODE = 5002;
        internal const string DATABASE_RECORD_NOT_FOUND_MESSAGE = "السجل غير موجود";

        public const int NOT_ALL_RECORDS_DELETED_CODE = 5003;
        internal const string NOT_ALL_RECORDS_DELETED_MESSAGE = "تعذر حذف بعض السجلات";

        public const int UNABLE_TO_OPEN_FILE_NOT_FOUND_CODE = 5004;
        internal const string UNABLE_TO_OPEN_FILE_NOT_FOUND_MESSAGE = "تعذر فتح الملف. الملف غير موجود";

        public const int DB_UNABLE_TO_INSERT_RECORD_CODE = 5005;
        internal const string DB_UNABLE_TO_INSERT_RECORD_MESSAGE = "تعذر إضافة البيانات";

        public const int DB_UNABLE_TO_UPDATE_RECORD_CODE = 5006;
        internal const string DB_UNABLE_TO_UPDATE_RECORD_MESSAGE = "تعذر تحديث البيانات";

        public const int DB_UNABLE_TO_DELETE_RECORD_CODE = 5007;
        internal const string DB_UNABLE_TO_DELETE_RECORD_MESSAGE = "تعذر حذف البيانات";

        public const int DB_ENTITY_IS_INACTIVE_CODE = 5008;
        internal const string DB_ENTITY_IS_INACTIVE_MESSAGE = "{0}:{1} غير نشط.";

        public const int UNABLE_TO_SEND_WHATSAPP_OTP_CODE = 5009;
        internal const string UNABLE_TO_SEND_WHATSAPP_OTP_MESSAGE = "تعذر إرسال رمز واتساب";

        /// System
        public const int SYS_UNKNOWN_ERROR_CODE = 5999;
        public const string SYS_UNKNOWN_ERROR_MESSAGE = "حدث خطأ غير معروف";

        public const int USER_RESET_TOKEN_INVALID_CODE = 9010;
        public const string USER_RESET_TOKEN_INVALID_MESSAGE = "رابط إعادة تعيين كلمة المرور غير صالح أو منتهٍ";

        public const int USER_CURRENT_PASSWORD_INVALID_CODE = 9011;
        public const string USER_CURRENT_PASSWORD_INVALID_MESSAGE = "كلمة المرور الحالية غير صحيحة";

        public const int IMAGE_DIDNT_SAVE = 1012;
        internal const string IMAGE_DIDNT_SAVE_MESSAGE = "حدثت مشكلة أثناء حفظ الصورة";
    }
}
