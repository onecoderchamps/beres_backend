
public interface IAuthService
{
    // Task<Object> RegisterGoogleAsync(string model,RegisterGoogleDto login);
    Task<Object> Aktifasi(string id);
    Task<Object> DeleteAccount(string id);

    Task<Object> UpdateProfile(string id, UpdateProfileDto item);
    Task<Object> UpdateUserProfile(string id, UpdateFCMProfileDto item);
    Task<Object> SendNotif(PayloadNotifSend payloadNotifSend);
}