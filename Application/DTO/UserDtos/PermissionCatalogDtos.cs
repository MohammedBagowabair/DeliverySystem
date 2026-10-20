namespace Application.DTO.UserDtos
{
    public class PermissionCatalogDTO
    {
        public List<PermissionCatalogItemDTO> Permissions { get; set; } = new();
        public List<PermissionPresetDTO> Presets { get; set; } = new();
    }

    public class PermissionCatalogItemDTO
    {
        public string Code { get; set; } = string.Empty;
        public string LabelAr { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public List<string> Implies { get; set; } = new();
    }

    public class PermissionPresetDTO
    {
        public string Id { get; set; } = string.Empty;
        public string LabelAr { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateUserPermissionsDTO
    {
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateUserPermissionsResultDTO
    {
        public int UserId { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool RequiresReLogin { get; set; } = true;
        public string Message { get; set; } = "تم حفظ الصلاحيات. يجب على الموظف تسجيل الدخول من جديد لتطبيق التغييرات.";
    }
}
