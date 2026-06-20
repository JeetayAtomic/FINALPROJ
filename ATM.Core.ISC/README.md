        ATM.Core.ISC
        ============

        Usage:
        1) appsettings.json
        {
          "ISCServices": {
            "CustomerAPIBase": { "BaseUrl": "http://atm.supraesapp.com/Test_ATMCustomerAPI", "TimeoutSeconds": 60 },
            "BillAPIBase": { "BaseUrl": "http://atm.supraesapp.com/ATMBackendDEVAPI", "TimeoutSeconds": 120 }
          }
        }
            OR

        1) appsettings.json
        {
          "ServiceUrls": {
                "LoggingAPIBase": "http://atm.supraesapp.com/internal-errorlog-api",
                "CommonAPIBase": "http://atm.supraesapp.com/internal-common-api",
                "BillAPIBase": "http://atm.supraesapp.com/ATMBackendDEVAPI",
                "CustomerAPIBase": "http://atm.supraesapp.com/Test_ATMCustomerAPI",
                "PricingAPIBase": "http://atm.supraesapp.com/internal-pricing-api"
            }
        }

        2) Program.cs
        // opts is optional
        builder.Services.AddISCProxiesFromConfig(builder.Configuration, opts => { opts.RetryCount = 3; opts.TimeoutSeconds = 30; });

        3) Usage example (inject IISCClient):
            public class MyService {
              private readonly IISCClient _isc;
              public MyService(IISCClient isc) { _isc = isc; }

              // Sample Method
              public async Task Do() {
                var user = await _isc.PostAsync<GetSampleRequest, ATMResponse<GetSampleResponse>>("api/sample/get", new GetSampleRequest { Id = 1 },"CustomerAPIBase");
                // "CustomerAPIBase" is optional and can be used at constructor level to target specific service via global variable.
              }
            }
