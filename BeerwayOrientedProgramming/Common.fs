namespace BeerwayOrientedProgramming

module Db = 

    open System.Linq

    open MongoDB.Bson
    open MongoDB.Driver
    open MongoDB.Driver.Builders
    open MongoDB.FSharp

    open BeerInfo

    type Configuration = { Id                 : BsonObjectId; 
                           MyPhoneNumber      : string; 
                           AccountSID         : string; 
                           AuthToken          : string; 
                           SendingPhoneNumber : string }

    [<Literal>]
    let ConnectionString = "your connection string goes here."

    [<Literal>]
    let DbName = "beerwayorientedprogramming"

    [<Literal>]
    let ConfigurationCollection = "configurations"

    let client = MongoClient(ConnectionString)
    let db     = client.GetDatabase(DbName)

    let generateBsonId() =
        BsonObjectId(ObjectId.GenerateNewId())

    let configuration = 
        let configurationCollection = 
            db.GetCollection<Configuration>(ConfigurationCollection)
        configurationCollection.Find(Builders.Filter.Empty).ToList().First()

    let getScrapeCollection ( breweryName : string ) =
        try
            db.GetCollection<BeerInfo>(breweryName)
        with
            | ex -> 
                db.CreateCollection(breweryName)
                db.GetCollection<BeerInfo>(breweryName)

    let getPreviousScrapeAndPersistNewBeerInfo ( newBeerInfo : BeerInfo ) 
        : BeerInfo option = 
        let breweryCollection = getScrapeCollection newBeerInfo.Name

        let previousScrape = 
            let result = 
                breweryCollection.Find(Builders.Filter.Empty)
                    .SortByDescending(fun b -> (b.TimeOfScrape :> obj))
                    .FirstOrDefault()
            if isNull ( result :> obj ) then None
            else Some result
        breweryCollection.InsertOne( newBeerInfo )
        previousScrape

module Error =

    type Error = 
        | ScrapeError  of string
        | CompareError of string
        | AlertError   of string

module Compare =

    open Error
    open BeerInfo
    open ROP
    open Db 

    let emptySet = Set.empty

    let compare( newBeerInfo : BeerInfo ) = 
        try
            let deserializedBeers = 
                Db.getPreviousScrapeAndPersistNewBeerInfo( newBeerInfo )
            let oldBeersAsSet = 
                if deserializedBeers.IsSome then 
                    Set< string >( deserializedBeers.Value.Beers )
                else emptySet

            let newBeersAsSet = Set< string >( newBeerInfo.Beers )
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
    open Db

    let stringifyDifferenceSetWithDetails ( difference : Set<string> ) : string =
        let concatenatedBeers = 
            difference
            |> String.concat ", "
        "New beers available! Including: " + concatenatedBeers 

    let alert( difference : Set<string> ) = 
        if difference.Count = 0 then Success "No Difference => No Text Sent"
        else
            try 
                TwilioClient.Init( configuration.AccountSID, configuration.AuthToken )
                let toPhoneNumber   = PhoneNumber configuration.MyPhoneNumber
                let sendPhoneNumber = PhoneNumber configuration.SendingPhoneNumber
                let message = MessageResource.Create( toPhoneNumber, 
                                                      null, 
                                                      sendPhoneNumber, 
                                                      null, 
                                                      stringifyDifferenceSetWithDetails
                                                        ( difference ))
                Success "New Message Sent Sucessfully"
            with
                | ex -> Failure ( AlertError ( ex.Message )) 