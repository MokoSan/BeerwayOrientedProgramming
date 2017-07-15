module BeerInfo 

open System

open Chiron
open Chiron.Operators

type BeerInfo = { TimeOfScrape : DateTime; Beers : string list } with
    static member ToJson( beerInfo : BeerInfo ) =
        Json.write "TimeOfScrape" beerInfo.TimeOfScrape
        *> Json.write "Beers" beerInfo.Beers

    static member FromJson( _ : BeerInfo ) =
        fun timeOfScrape beers -> { TimeOfScrape = timeOfScrape; Beers = beers }
        <!> Json.read "TimeOfScrape" 
        <*> Json.read "Beers"