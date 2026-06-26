namespace WeChatPlus.Core.Services
{
    public interface ILicenseActivationTransport
    {
        string Send(LicenseActivationRequest request);
    }
}
