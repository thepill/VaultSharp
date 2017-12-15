﻿using System;
using Newtonsoft.Json;
using VaultSharp.Backends.System;
using Xunit;

namespace VaultSharp.Samples
{
    class Program
    {
        private static IVaultClient UnauthenticatedVaultClient;

        private static string ResponseContent;

        public static void Main(string[] args)
        {
            var settings = new VaultClientSettings();
            settings.VaultServerUriWithPort = "http://localhost:8200";

            settings.AfterApiResponseAction = r => ResponseContent = r.Content.ReadAsStringAsync().Result;

            UnauthenticatedVaultClient = new VaultClient(settings);

            RunAllSamples();

            Console.ReadLine();
        }

        private static void RunAllSamples()
        {
            RunSystemBackendSamples();
            RunAuthBackendSamples();
            RunSecretBackendSamples();
        }

        private static void RunAuthBackendSamples()
        {
        }

        private static void RunSecretBackendSamples()
        {
        }

        private static void RunSystemBackendSamples()
        {
            var exception = Assert.ThrowsAsync<Exception>(() => UnauthenticatedVaultClient.V1.System.GetSealStatusAsync()).Result;
            Assert.Contains("not yet initialized", exception.Message);

            var initStatus = UnauthenticatedVaultClient.V1.System.GetInitStatusAsync().Result;
            Assert.False(initStatus);

            var initOptions = new InitOptions
            {
                SecretShares = 10,
                SecretThreshold = 5
            };

            var masterCredentials = UnauthenticatedVaultClient.V1.System.InitAsync(initOptions).Result;
            DisplayJson(masterCredentials);

            Assert.Equal(initOptions.SecretShares, masterCredentials.MasterKeys.Length);
            Assert.Equal(initOptions.SecretShares, masterCredentials.Base64MasterKeys.Length);

            initStatus = UnauthenticatedVaultClient.V1.System.GetInitStatusAsync().Result;
            DisplayJson(initStatus);

            Assert.True(initStatus);

            var sealStatus = UnauthenticatedVaultClient.V1.System.GetSealStatusAsync().Result;
            DisplayJson(sealStatus);

            Assert.True(sealStatus.Sealed);
        }

        private static void DisplayJson<T>(T value)
        {
            string line = "============";
            Console.WriteLine(typeof(T).Name);

            Console.WriteLine(line + line);
            Console.WriteLine(ResponseContent);
            Console.WriteLine(JsonConvert.SerializeObject(value));
            Console.WriteLine(line + line);
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
