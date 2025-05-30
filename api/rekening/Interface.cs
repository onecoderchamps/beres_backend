public interface IRekeningService
{

    Task<object> getRekening();
    Task<object> SettingArisan();
    Task<object> SettingPatungan();
}