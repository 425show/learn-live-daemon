﻿using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Helpers;


namespace learn_live_daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config = LoadAppSettings();
            if (config == null)
            {
                Console.WriteLine("Invalid appsettings.json file.");
                return;
            }
            var client = GetAuthenticatedGraphClient(config);
            var requestUserEmail = client.Users[config["targetUserId"]].Messages.Request().Skip(40);
            var results = requestUserEmail.GetAsync().Result;
            foreach (var message in results)
            {
                Console.WriteLine("");
                Console.WriteLine("Subject : " + message.Subject);
                Console.WriteLine("Received: " + message.ReceivedDateTime.ToString());
                Console.WriteLine("ID      : " + message.Id);
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestUserEmail.GetHttpRequestMessage().RequestUri);
        }
        private static IConfigurationRoot LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", false, true)
                                  .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["applicationSecret"]) ||
                    string.IsNullOrEmpty(config["tenantId"]) ||
                    string.IsNullOrEmpty(config["targetUserId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var authenticationProvider = CreateAuthenticationProvider(config);
            return new GraphServiceClient(authenticationProvider);
        }

        private static IAuthenticationProvider CreateAuthenticationProvider(IConfigurationRoot config)
        {
            var tenantId = config["tenantId"];
            var clientId = config["applicationId"];
            var clientSecret = config["applicationSecret"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithClientSecret(clientSecret)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray());
        }
    }
}
