public interface IRekeningService
{

    Task<object> getRekening();
    Task<object> SettingArisan();
    Task<object> SettingPatungan();
    Task<object> SettingIuranBulanan();
    Task<object> SettingIuranTahunan();
    Task<object> Banner();
}