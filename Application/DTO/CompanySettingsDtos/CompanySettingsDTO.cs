namespace Application.DTO.CompanySettingsDtos
{
    public class CompanySettingsDTO
    {
        public int Id { get; set; }
        public decimal ExpensesShare { get; set; }
        public decimal ManagersShare { get; set; }
        public decimal ManagementShare { get; set; }
        public decimal CompanyReserveShare { get; set; }
    }
}
