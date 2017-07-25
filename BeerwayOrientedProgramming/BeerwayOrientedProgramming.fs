namespace BeerwayOrientedProgramming

module BeerwayOrientedProgramming =

    open ROP

    open Compare
    open Alert

    open Quartz
    open Quartz.Impl

    let tiredHandsPipeline = TiredHandsScraper.scrape >=> compare >=> alert 
    let breweryPipelines = [ tiredHandsPipeline ]

    type Job () =
        interface IJob with
            member x.Execute(context: IJobExecutionContext) =
                breweryPipelines
                |> List.iter(fun b -> printfn "%A" ( b() )) 

    [<EntryPoint>]
    let main argv =
        let schedulerFactory = StdSchedulerFactory()
        let scheduler = schedulerFactory.GetScheduler()
        let job = JobBuilder.Create<Job>().Build()
        let trigger =
            TriggerBuilder.Create()
                .WithSimpleSchedule(fun x ->
                    x.WithIntervalInSeconds(2400).RepeatForever() |> ignore)
                .Build()
        scheduler.Start()
        scheduler.ScheduleJob(job, trigger) |> ignore
        0