module BeerInfo 

open System

open Chiron
open Chiron.Operators

type BeerInfo = { Name : string; TimeOfScrape : DateTime; Beers : string list } with
    static member ToJson( beerInfo : BeerInfo ) =
        Json.write "Name" beerInfo.Name
        *> Json.write "TimeOfScrape" beerInfo.TimeOfScrape
        *> Json.write "Beers" beerInfo.Beers

    static member FromJson( _ : BeerInfo ) =
        fun name timeOfScrape beers 
            -> { Name = name; TimeOfScrape = timeOfScrape; Beers = beers }
        <!> Json.read "Name" 
        <*> Json.read "TimeOfScrape"
        <*> Json.read "Beers"