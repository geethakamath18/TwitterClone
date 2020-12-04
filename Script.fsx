#r "nuget: FSharp.Json"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

#load "Variables.fsx"
#load "Methods.fsx"

open System
open FSharp.Json
open Akka.FSharp
open Akka.Actor
open System.Collections.Generic

open Methods.Methods
open Variables.Variables

module Script =
    let wait() = 
        System.Threading.Thread.Sleep(1000);

    let waitBitLonger() = 
        System.Threading.Thread.Sleep(2000);

    let createuser(numberOfUSers) = 
        server.[0]<!Start
        wait()
        printfn ""
        printfn "%d Users Created" numberOfUSers

        if printvalues then 
            for i in [0 .. numberOfUSers-1] do
                printfn "%s" usernameIDMapping.[i]

    let createFollowers() =

        server.[0]<!ConstructTables

        wait()
        if printvalues then 
            printfn ""
            printfn "UserID : number of followers %A " numberOfFollowers
            printfn ""
            printfn " UserID: followers %A  " followersTable
            printfn ""
            printfn "UserID : followingTable %A " followingTable

    let startTweet() = 
        wait()
        server.[0]<!StartTweeting

    let startRetweet() = 
        wait()
        server.[0] <! StartRetweeting

    let startMentioning() = 
        wait()
        server.[0] <! StartMentioning

    let tweetSingle() =
        let followingof1 = followingTable.[1]
        printfn ""
        printfn "List of user's user-1 is following %A" followingof1
        fo <- followingof1.[0]
        clients.[fo] <! SingleTweet

    let mentionUser() = 

        clients.[fo] <! MentionSingleUser(1)
        
        clients.[fo] <! MentionSingleUser(0)

    let deleteUser() =
        server.[0]<! DeleteUser
        
    let logoutUser()=
        clients.[1] <! Logout
   
    let demoOne(args) = 
 
        createuser(args)
        wait()
        createFollowers()
        wait()
        startTweet()
        wait()
        startRetweet()
        wait()
        startMentioning()
        wait()
        logout(1)
        wait()
        tweetSingle()
        wait()
        printfn "Since the user 1 is logged out you wont see User 1's feed with the tweet"
        printfn ""
        printfn "Press enter to for user 1 to login"
        System.Console.ReadLine() |> ignore
        // mentionUser()
        login(1)

    let demoTwo(args) = 

        createuser(args)
        waitBitLonger()
        createFollowers()
        waitBitLonger()
        startTweet()
        waitBitLonger()
        startRetweet()
        waitBitLonger()
        startMentioning()
        waitBitLonger()
        deleteUser()
        startMentioning()
