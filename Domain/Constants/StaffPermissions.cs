namespace Domain.Constants
{
    /// <summary>
    /// Feature permissions Admin can assign to Staff employees.
    /// Admin always has all permissions implicitly.
    /// Stored on User as comma-separated codes in Permissions.
    /// Also emitted as JWT role claims so [Authorize(Roles=...)] and AuthorizeView work.
    /// </summary>
    public static class StaffPermissions
    {
        public const string Orders = "Orders";
        public const string Drivers = "Drivers";
        public const string Customers = "Customers";
        public const string Reports = "Reports";
        public const string WorkDay = "WorkDay";
        public const string Years = "Years";
        public const string ViewFinance = "ViewFinance";
        /// <summary>When assigned to Staff, limits order views to the current business day only.</summary>
        public const string TodayOrdersOnly = "TodayOrdersOnly";

        public const string GroupModules = "Modules";
        public const string GroupRestrictions = "Restrictions";

        public static readonly string[] All =
        {
            Orders, Drivers, Customers, Reports, WorkDay, Years, ViewFinance, TodayOrdersOnly
        };

        /// <summary>Default set for a new Staff employee (operations without finance).</summary>
        public static readonly string[] DefaultStaff =
        {
            Orders, Drivers, Customers, Reports, WorkDay, Years
        };

        public static readonly IReadOnlyDictionary<string, string> LabelsAr =
            new Dictionary<string, string>
            {
                [Orders] = "الطلبات",
                [Drivers] = "الموصلون",
                [Customers] = "العملاء",
                [Reports] = "التقارير",
                [WorkDay] = "يوم العمل",
                [Years] = "السنوات السابقة",
                [ViewFinance] = "عرض الأرقام المالية",
                [TodayOrdersOnly] = "طلبات يوم العمل فقط (بدون سجل سابق)"
            };

        public static readonly IReadOnlyDictionary<string, string> DescriptionsAr =
            new Dictionary<string, string>
            {
                [Orders] = "عرض وإدارة الطلبات.",
                [Drivers] = "عرض وإدارة الموصلين وطلباتهم.",
                [Customers] = "عرض وإدارة العملاء.",
                [Reports] = "الوصول إلى شاشات التقارير.",
                [WorkDay] = "إغلاق يوم العمل والأرشيف اليومي.",
                [Years] = "أرشيف السنوات السابقة وإغلاق السنة.",
                [ViewFinance] = "إظهار الإيرادات والأرباح والمبالغ المالية.",
                [TodayOrdersOnly] = "تقييد الطلبات ليوم العمل الحالي فقط (يتطلب صلاحية الطلبات)."
            };

        public static readonly IReadOnlyDictionary<string, string> Groups =
            new Dictionary<string, string>
            {
                [Orders] = GroupModules,
                [Drivers] = GroupModules,
                [Customers] = GroupModules,
                [Reports] = GroupModules,
                [WorkDay] = GroupModules,
                [Years] = GroupModules,
                [ViewFinance] = GroupModules,
                [TodayOrdersOnly] = GroupRestrictions
            };

        /// <summary>Codes that must also be granted when a code is selected.</summary>
        public static readonly IReadOnlyDictionary<string, string[]> Implies =
            new Dictionary<string, string[]>
            {
                [TodayOrdersOnly] = new[] { Orders }
            };

        public static readonly PermissionPresetDefinition[] Presets =
        {
            new("DefaultStaff", "موظف افتراضي", "عمليات يومية بدون مالية", DefaultStaff),
            new("OrdersDesk", "مكتب الطلبات", "الطلبات فقط", new[] { Orders }),
            new("FieldOps", "تشغيل ميداني", "طلبات وموصلون وعملاء ويوم العمل", new[] { Orders, Drivers, Customers, WorkDay }),
            new("FinanceViewer", "عرض مالي", "التقارير والأرقام المالية مع الطلبات", new[] { Orders, Reports, ViewFinance }),
            new("FullStaff", "موظف كامل", "كل الصلاحيات ما عدا تقييد اليوم فقط", new[] { Orders, Drivers, Customers, Reports, WorkDay, Years, ViewFinance }),
        };

        public static string Serialize(IEnumerable<string>? codes)
        {
            if (codes == null) return string.Empty;
            var valid = codes
                .Select(c => c?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c) && All.Contains(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return string.Join(',', valid);
        }

        public static List<string> Parse(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string>();

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(c => All.Contains(c, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string NormalizeForRole(string? role, IEnumerable<string>? codes)
        {
            if (string.Equals(role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
                return Serialize(All);

            var list = codes?.ToList() ?? new List<string>();
            if (list.Count == 0)
                list.AddRange(DefaultStaff);

            ApplyImplies(list);
            return Serialize(list);
        }

        public static void ApplyImplies(List<string> list)
        {
            foreach (var pair in Implies)
            {
                if (!list.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                    continue;
                foreach (var required in pair.Value)
                {
                    if (!list.Contains(required, StringComparer.OrdinalIgnoreCase))
                        list.Add(required);
                }
            }
        }

        public static IReadOnlyList<PermissionCatalogItem> BuildCatalogItems()
        {
            return All.Select(code => new PermissionCatalogItem(
                code,
                LabelsAr.TryGetValue(code, out var label) ? label : code,
                DescriptionsAr.TryGetValue(code, out var desc) ? desc : string.Empty,
                Groups.TryGetValue(code, out var group) ? group : GroupModules,
                Implies.TryGetValue(code, out var implies) ? implies : Array.Empty<string>()
            )).ToList();
        }
    }

    public sealed record PermissionCatalogItem(
        string Code,
        string LabelAr,
        string DescriptionAr,
        string Group,
        IReadOnlyList<string> Implies);

    public sealed record PermissionPresetDefinition(
        string Id,
        string LabelAr,
        string DescriptionAr,
        IReadOnlyList<string> Permissions);
}
