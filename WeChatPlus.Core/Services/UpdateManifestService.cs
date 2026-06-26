using System;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class UpdateManifestService
    {
        public static UpdateManifestResult Load(
            string cloudManifestUrl,
            string localManifestPath,
            IUpdateManifestTransport transport)
        {
            Exception cloudError = null;
            if (!string.IsNullOrWhiteSpace(cloudManifestUrl) && transport != null)
            {
                try
                {
                    string cloudJson = transport.DownloadString(cloudManifestUrl);
                    return Success("cloud", "Cloud update manifest loaded.", UpdateManifest.Parse(cloudJson));
                }
                catch (Exception ex)
                {
                    cloudError = ex;
                }
            }

            if (!string.IsNullOrWhiteSpace(localManifestPath) && File.Exists(localManifestPath))
            {
                string localJson = File.ReadAllText(localManifestPath);
                string source = cloudError == null ? "local" : "local-fallback";
                string message = cloudError == null
                    ? "Local update manifest loaded."
                    : "Cloud update manifest unavailable, using local manifest: " + cloudError.Message;
                return Success(source, message, UpdateManifest.Parse(localJson));
            }

            string failureMessage = cloudError == null
                ? "Update manifest unavailable."
                : "Update manifest unavailable: " + cloudError.Message;
            return new UpdateManifestResult
            {
                Loaded = false,
                Source = "none",
                Message = failureMessage,
                Manifest = new UpdateManifest()
            };
        }

        private static UpdateManifestResult Success(string source, string message, UpdateManifest manifest)
        {
            return new UpdateManifestResult
            {
                Loaded = true,
                Source = source,
                Message = message,
                Manifest = manifest ?? new UpdateManifest()
            };
        }
    }
}
