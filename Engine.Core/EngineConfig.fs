﻿namespace R4nd0mApps.TddStud10.Engine.Core

open System.Runtime.Serialization

[<AllowNullLiteral>]
[<DataContract>]
type EngineConfig() = 
    let mutable snapShotRoot = @"%temp%\_tdd"
    
    [<DataMember(IsRequired = false)>]
    member __.SnapShotRoot 
        with get () = snapShotRoot
        and set value = snapShotRoot <- value