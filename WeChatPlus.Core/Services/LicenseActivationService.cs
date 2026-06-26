using System;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class LicenseActivationService
    {
        private readonly TrialLicenseService _licenseService;
        private readonly LicenseApiClient _apiClient;
        private readonly ILicenseActivationTransport _transport;

        public LicenseActivationService(
            TrialLicenseService licenseService,
            LicenseApiClient apiClient,
            ILicenseActivationTransport transport)
        {
            if (licenseService == null)
            {
                throw new ArgumentNullException("licenseService");
            }

            if (apiClient == null)
            {
                throw new ArgumentNullException("apiClient");
            }

            if (transport == null)
            {
                throw new ArgumentNullException("transport");
            }

            _licenseService = licenseService;
            _apiClient = apiClient;
            _transport = transport;
        }

        public LicenseActivationResult Activate(string licenseKey)
        {
            LicenseState current = _licenseService.GetOrCreateTrial();
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return Failure("License key is required.", current);
            }

            try
            {
                LicenseActivationRequest request = _apiClient.BuildActivationRequest(licenseKey, current);
                string responseJson = _transport.Send(request);
                LicenseActivationResponse response = LicenseActivationResponse.Parse(responseJson);
                LicenseState state = _licenseService.ApplyCloudActivation(response);
                if (!response.Ok)
                {
                    return Failure(string.IsNullOrWhiteSpace(response.Message) ? "Cloud activation failed." : response.Message, state);
                }

                return new LicenseActivationResult
                {
                    Success = true,
                    Message = string.IsNullOrWhiteSpace(response.Message) ? "Activated." : response.Message,
                    State = state
                };
            }
            catch (Exception ex)
            {
                return Failure("Cloud activation failed: " + ex.Message, _licenseService.GetOrCreateTrial());
            }
        }

        private static LicenseActivationResult Failure(string message, LicenseState state)
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = message ?? string.Empty,
                State = state
            };
        }
    }
}
