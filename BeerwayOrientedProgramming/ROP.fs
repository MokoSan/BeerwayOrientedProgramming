module ROP

type Result<'TSuccess, 'TFailure> = 
    | Success of 'TSuccess
    | Failure of 'TFailure

let bind switchFunction input = 
    match input with
    | Success s -> switchFunction s
    | Failure f -> Failure f  

let (>>=) switchFunction twoTrackInput = 
    bind switchFunction twoTrackInput

let switch switchFunction1 switchFunction2 input = 
    match switchFunction1 input with
    | Success s -> switchFunction2 s 
    | Failure f -> Failure f

let (>=>) switchFunction1 switchFunction2 input = 
    switch switchFunction1 switchFunction2 input

let fail input =
    Failure input

let success input = 
    Success input 
