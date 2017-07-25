namespace BeerwayOrientedProgramming

module Error =

    type Error = 
        | ScrapeError  of string
        | CompareError of string
        | AlertError   of string

module Compare =

    open System.IO

    open Chiron
    open Chiron.Operators

    open Error
    open BeerInfo
    open ROP

    let serializeBeerInfo ( beerInfo : BeerInfo ) : string = 
        beerInfo 
        |> Json.serialize
        |> Json.formatWith JsonFormattingOptions.Pretty
        
    let deserializeBeerInfo ( jsonizedBeerInfo : string ) : BeerInfo = 
        jsonizedBeerInfo 
        |> Json.parse
        |> Json.deserialize

    let deserializePreviousScrape( filePath : string ) : BeerInfo option = 
        if File.Exists ( filePath ) then
            File.ReadAllText ( filePath )
            |> deserializeBeerInfo
            |> Some
        else
            None

    let emptySet = Set.empty

    let compare( newBeerInfo : BeerInfo ) = 
        try
            let deserializedBeers = deserializePreviousScrape("TiredHands.json")
            let oldBeersAsSet = 
                if deserializedBeers.IsSome then Set< string >( deserializedBeers.Value.Beers )
                else emptySet

            let newBeersAsSet = Set< string >( newBeerInfo.Beers )

            File.WriteAllText("TiredHands.json", serializeBeerInfo ( newBeerInfo ))

            let difference    = newBeersAsSet - oldBeersAsSet 
            Success ( difference )
        with
            | ex -> Failure ( CompareError( ex.Message ))

module Alert = 

    open Twilio
    open Twilio.Rest.Api.V2010.Account
    open Twilio.Types;

    open Error
    open ROP

    [<Literal>]
    let MyPhoneNumber = ""
    [<Literal>]
    let AccountSid = ""
    [<Literal>]
    let AuthToken = ""
    [<Literal>]
    let SendingPhoneNumberFromTwilio = ""

    let stringifyDifferenceSetWithDetails ( difference : Set<string> ) : string =
        let concatenatedBeers = 
            difference
            |> String.concat ", "
        "New beers available! Including: " + concatenatedBeers 

    let alert( difference : Set<string> ) = 
        if difference.Count = 0 then Success "No Difference => No Text Sent"
        else
            try 
                TwilioClient.Init( AccountSid, AuthToken )
                let toPhoneNumber   = PhoneNumber MyPhoneNumber
                let sendPhoneNumber = PhoneNumber SendingPhoneNumberFromTwilio
                let message = MessageResource.Create( toPhoneNumber, 
                                                      null, 
                                                      sendPhoneNumber, 
                                                      null, 
                                                      stringifyDifferenceSetWithDetails( difference ))
                Success "New Message Sent Sucessfully"
            with
                | ex -> Failure ( AlertError ( ex.Message )) 