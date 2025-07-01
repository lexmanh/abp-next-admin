namespace LINGYUN.Abp.LocalizationManagement;

public class AbpLocalizationManagementOptions
{
    /// <summary>
    /// Save localized text to the database
    /// </summary>
    public bool SaveStaticLocalizationsToDatabase { get; set; } = true;

    public AbpLocalizationManagementOptions()
    {
    }
}
