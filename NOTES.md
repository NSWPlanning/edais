
# Use Cases

* Test run separation
* Retrieve list of tests
* Retrieve test results for run
* Run specific test
* Setup custom scenarios
* Test harness to signal to parent that we are listening/ready - stdout
* Signal fail or pass to parent - stdout + exit code
* Format for signaling - JSON


# Usages

## Running scenarios

    > eth.exe --run myid12 --test ProposeCreate.Valid1
    {"status": "ready", "endpoint": "http://localhost:8181/"}
    {"result": "pass"}

    > eth.exe --run myid12 --test ProposeCreate.Valid1
    {"status": "ready", "endpoint": "http://localhost:8181/"}
    {"result": "fail", "message": "Malformed XML at blah."}

    > eth.exe --run myid12 --test ProposeCreate.Valid1
    {"status": "ready", "endpoint": "http://localhost:8181/"}
    {"result": "fail", "message": "Timeout exceeded (5 seconds)."}

    > eth.exe --run myid12 --test ProposeCreateReceive.Valid --arg endpoint1:http://localhost:8181/Bap.svc
    {"result": "pass"}

## Introspecting test runs

    > eth.exe --run myid12 --results
    [
        {"scenario": "ProposeCreate.Valid1", "result": "pass"},
        {"scenario": "ProposeCreate.Valid2", "result": "fail", "message": "Fail message."}
    ]

    > eth.exe --run myid12 --clear
    {"status": "succeeded"}

    > eth.exe --run myid12 --clear
    {"status": "failed", "message": "Reason for failure."}

## Introspecting scenarios

    > eth.exe --list
    [
        {"scenario": "ProposeCreate.Valid1", "description": "Blah."},
        {"scenario": "ProposeCreate.Valid2", "description": "Beep."},
        {
            "scenario": "ProposeCreateReceive.Valid", 
            "description": "abc", 
            "parameters": [{"type": "url", "name": "endpoint1"}]
        }
    ]

    > eth.exe --info ProposeCreate.Valid1
    {"scenario": "ProposeCreate.Valid1", "description": "Blah."},

    
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

# Levels of testing

* HTTP
* SOAP / XML
* Application

# SSL Certificate Setup (windows)

Run in an ADMIN `cmd`, certificates will be created in the directory you're in.

    > "\Program Files (x86)\Windows Kits\8.0\bin\x64\makecert.exe" -n "CN=ethCA" -r -sv ethCA.pvk ethCA.cer -sr localmachine -ss My
    > "\Program Files (x86)\Windows Kits\8.0\bin\x64\makecert.exe" -sk ethSignedByCA -iv ethCA.pvk -n "CN=ethSignedByCA" -ic ethCA.cer ethSignedByCA.cer -sr localmachine -ss My

Open `MMC`. Add `Certificates (Local Computer)` snap-in.
Move `ethCA` certificate from the `Personal certificate` store to `Trusted Root Certification Authorities` store.
Get `ethSignedByCA` certificate thumbprint, and use in the command below.

    > netsh http add sslcert ipport=0.0.0.0:8181 certhash=<ethSignedByCA Thumbprint> appid={71b3e5bc-4bdb-457b-903b-357e7bb20995}