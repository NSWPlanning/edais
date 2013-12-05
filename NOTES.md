## Build

The solution is created using Visual Studio 2012 in C#. If you are building from vs2012 it is just a matter of opening the `ETH.sln` solution file and building it (F6).

Without VS2012, if you have the .net framework you can build it using MSBuild.exe found by default in:

    > "%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
The only argument required is the solution file `ETH.sln`

http://msdn.microsoft.com/en-us/library/vstudio/ms164311.aspx

Alternatively you can use xbuild for mono.
http://www.mono-project.com/Microsoft.Build

## Executable/Build output

The application will be built as ETH.exe and by default can be run from/found in `~\ETH\bin\Debug`.

## Running the test harness

Running the test harness without parameters or arguments will result in the generated help text listing each option.

## Listing available scenarios

    > eth -l

This will list all available test scenarios and the arguments needed for each one (if any).

When selecting a scenario to run, the brackets are not required but are just shown in the listing to indicate arguments for the scenarios.

    
## JSON results


    {"status": "ready", "endpoint": "http://localhost:8181/"}

    {"result": "pass"}
    {"result": "fail", "message": "blah"}

    {
        "scenario": "ScenarioName.Variation",
        "description": "whatever",
        "parameters": [
            {
                "name": "endpoint",
                "type": "url"
            },
            {
                "name": "something",
                "type": "string"
            }
        ]
    }

    [
        { "scenario": "ScenarioName.Variation", "result": "pass" },
        { "scenario": "ScenarioName.Variation2", "result": "fail", "message": "Some failure." }
    ]


## SSL Certificate Setup (windows)

Run in an ADMIN `cmd`, certificates will be created in the directory you're in.

    > "\Program Files (x86)\Windows Kits\8.0\bin\x64\makecert.exe" -n "CN=ethCA" -r -sv ethCA.pvk ethCA.cer -sr localmachine -ss My
    > "\Program Files (x86)\Windows Kits\8.0\bin\x64\makecert.exe" -sk ethSignedByCA -iv ethCA.pvk -n "CN=ethSignedByCA" -ic ethCA.cer ethSignedByCA.cer -sr localmachine -ss My

Open `MMC`. Add `Certificates (Local Computer)` snap-in.
Move `ethCA` certificate from the `Personal certificate` store to `Trusted Root Certification Authorities` store.
Get `ethSignedByCA` certificate thumbprint, and use in the command below.

    > netsh http add sslcert ipport=0.0.0.0:8181 certhash=<ethSignedByCA Thumbprint> appid={71b3e5bc-4bdb-457b-903b-357e7bb20995}

## Command line scenario examples


| `System Under Test`          |`Harness Role            `           |`Command Line Example`
|:-----------------------------|:------------------------------------|:-------------------------------------------------|
| EHC                          |Council System                       |eth -r test1 -t Council_EndToEnd.FromEHC_Accept  |
| EHC                          |Private Certifier System             |eth -r test1 -t PrivateCertifier_EndToEnd.ForEhc_Accept -a APP-00000001 test@localhost.test -c https://endpoint.svc
| EHC                          |Council & Private Certifier Systems  |N/A
| Council System               |EHC                                  |eth -r test1 -t EHC_EndToEnd.ToThirdParty_Accept -a APP-00000001 test@localhost.test -c https://endpoint.svc
| Council System               |Private Certifier System             |eth -r test1 -t PrivateCertifier_EndToEnd.ForEhc_Reject -m -s
| Council System               |EHC & Private Certifier System       |N/A
| Private Certifier System     |EHC                                  |eth -r test1 -t EHC_EndToEnd.ToThirdParty_Accept -a APP-00000001 test@localhost.test -c https://endpoint.svc -m --user buildaform_ehc --pass printer1978@
| Private Certifier System     |Council System                       |eth -r test1 -t Council_EndToEnd.FromCertifier_Accept
| Private Certifier System     |EHC & Council System                 |N/A
