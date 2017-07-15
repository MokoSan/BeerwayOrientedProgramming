namespace BeerwayOrientedProgramming

module BeerwayOrientedProgramming =

    open ROP

    open TiredHandsScraper
    open Compare
    open Alert

    let pipeline = scrape >=> compare >=> alert 

    [<EntryPoint>]
    let main argv =
        printfn "%A" ( pipeline() )
        0 // return an integer exit code
