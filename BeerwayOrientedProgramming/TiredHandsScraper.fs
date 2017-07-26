namespace BeerwayOrientedProgramming

module TiredHandsScraper =

    open System.Collections.Generic

    open FSharp.Data

    open BeerInfo
    open ROP
    open Error 

    [<Literal>]
    let TiredHandsUrl = "http://www.tiredhands.com/fermentaria/beers/" 

    [<Literal>]
    let ClassNameOfBeers = "menu-item-title"

    type HtmlProviderForTiredHands = HtmlProvider< TiredHandsUrl >
    let html                       = HtmlProviderForTiredHands.Load( TiredHandsUrl )

    let cleanupDataFromTheWebsite ( input : string ) = 
        input.Trim().Replace(":", " ")

    let beerFindingPredicate ( a : string, innerText : string ) : bool =
        a = ClassNameOfBeers && not ( innerText.Contains("*")) && not ( innerText.Contains( "Military" ))
        
    let getBeerNamesFromTiredHands() : List<string> =
        let results = 
            html.Html.Descendants ["div"]
            |> Seq.choose( fun x -> 
                x.TryGetAttribute("class")
                |> Option.map(fun a -> a.Value(), x.InnerText() ))
            |>  Seq.filter(fun ( a,  innerText ) -> beerFindingPredicate ( a, innerText ) ) 
            |> Seq.map(fun ( a, innerText ) -> cleanupDataFromTheWebsite innerText ) 
        List<string>( results )

    let scrape() =
        try
            let beers = getBeerNamesFromTiredHands()
            Success
                { Id           = Db.generateBsonId() 
                  Name         = "TiredHands" 
                  TimeOfScrape = System.DateTime.Now
                  Beers        = beers }
        with
            | ex -> Failure ( ScrapeError ( ex.Message )) 