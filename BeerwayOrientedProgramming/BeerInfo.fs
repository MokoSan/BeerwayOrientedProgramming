module BeerInfo 

open System
open System.Collections.Generic
open MongoDB.Bson

type BeerInfo = 
    { Id           : BsonObjectId
      Name         : string 
      TimeOfScrape : DateTime 
      Beers        : List<string> } 