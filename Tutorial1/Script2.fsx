#r @"C:\MyCode1\FSharp1\Tut1\Tutorial1\packages\FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"

open FSharp.Data

let queryServer uri queryParams = 
    try
        let response = Http.Request(uri, query=queryParams, silentHttpErrors = true)
        Some response 
    with
    | :? System.Net.WebException as ex -> None

let sendAlert uri message = 
    // send alert via email, say
    printfn "Error for %s. Message=%O" uri message

let checkServer (uri,queryParams) = 
    match queryServer uri queryParams with
    | Some response -> 
        printfn "Response for %s is %O" uri response.StatusCode 
        if (response.StatusCode <> 200) then
            sendAlert uri response.StatusCode 
    | None -> 
        sendAlert uri "No response"

// test the sites    
let google = "http://google.com", ["q","fsharp"]
let bad = "http://example.bad", []

[google;bad]
|> List.iter checkServer 
