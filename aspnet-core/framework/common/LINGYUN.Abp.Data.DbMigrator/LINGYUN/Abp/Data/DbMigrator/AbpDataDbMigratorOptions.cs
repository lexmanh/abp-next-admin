namespace LINGYUN.Abp.Data.DbMigrator;

public class AbpDataDbMigratorOptions
{
    /// <summary>
    /// Allow seed data when migrating the database.
    /// </summary>
    public bool AllowSeedData { get; set; } = true;

    public AbpDataDbMigratorOptions()
    {
    }
}