using System;

namespace clockify_to_any.core
{
    class ClockifyApiKeyProvider
    {
        private string[] Arguments { get; init; }

        public ClockifyApiKeyProvider(string[] args) => Arguments = args;

        public string GetApiKey()
        {
            string apiKey;
            string envVar = Environment.GetEnvironmentVariable("CAPI_KEY");

            if (Arguments.Length > 0)
            {
                Console.WriteLine("API Key megtalálva argumentumok között.");
                apiKey = Arguments[0];
            }
            else if (envVar != null)
            {
                Console.WriteLine("API Key a CAPI_KEY környezeti változóból.");
                apiKey = envVar;
            }
            else
            {
                throw new Exception("API Key nincs megadva.");
            }

            return apiKey;
        }
    }
}

