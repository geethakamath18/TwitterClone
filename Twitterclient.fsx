#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 

#load "Variables.fsx"
#load "Methods.fsx"
#load "Script.fsx"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

open Script.Script
open Variables.Variables

open Methods.Methods

open System.Collections.Generic
open Akka.FSharp
open System.Diagnostics


module Twitterclient = 

    let returnTweet(x:int, y:int) =
        let mutable tweet = ""
        let mutable hashtag = ""
        let mutable result = ""
        tweet <- tweetList.[x]
        hashtag <- hashTags.[y]
        result <- tweet+hashtag
        server.[0]<!ProcessTweet(tweet, hashtag)
        result

    let client (mailbox:Actor<_>) =

            let mutable t =""

            let mutable myID=0;

            let mutable myUserName = ""

            let mutable login = true

            let mutable myFeed= new List<string>();

            let mutable feedsInQueue = new List<string>();

            let rec loop () = actor {
                let! msg = mailbox.Receive () //Recieving messages from the mailbox

                match msg with
                |   Init(x:int, st:string) -> 
                    myID <-x;
                    myUserName <- st // generateUsername()

                |   TweetNow ->

                    let mutable randomNum1 = random.Next(0,tweetList.Length)
                    let mutable randomNum2 = random.Next(0, hashTags.Length)
                    t <- returnTweet(randomNum1,randomNum2)
                    // printfn "tweet retrieved is : %A" t
                    if tweetsTable.ContainsKey(myID) then
                        tweetsTable.[myID].Add(t);
                    else
                        let mutable tempList= new List<string>();
                        tempList.Add(t);
                        tweetsTable <- tweetsTable.Add(myID,tempList);

                    let mutable myFollowers = followersTable.[myID]
                    for i in [0 .. myFollowers.Count-1] do
                        clients.[myFollowers.[i]]<!AddToFeed(t, myID, "tweeted", "Group")

                |   SingleTweet ->
                    let mutable randomNum1 = random.Next(0,tweetList.Length)
                    let mutable randomNum2 = random.Next(0, hashTags.Length)
                    t <- returnTweet(randomNum1,randomNum2)
                    tweetsTable.[myID].Add(t);
                    let mutable myFollowers = followersTable.[myID]
                    for i in [0 .. myFollowers.Count-1] do
                        clients.[myFollowers.[i]]<!AddToFeed(t, myID, "tweeted", "Individual")

                |   RetweetNow(source:int) ->

                    let mutable tweetNumber = random.Next(0, tweetsTable.[source].Count)
                    tweetsTable.[myID].Add(tweetsTable.[source].[tweetNumber])
                    let mutable myFollowers = followersTable.[myID]
                    for i in [0 .. myFollowers.Count-1] do
                        clients.[myFollowers.[i]]<!AddToFeed(tweetsTable.[source].[tweetNumber], myID, "tweeted", "Group")

                |   MentionNow ->

                    let mutable possibleMentions = clone(clientList)
                    possibleMentions.Remove(myID)|> ignore
                    let mutable rand3 = random.Next(0, possibleMentions.Count)
                    let mutable mentionedUserID = possibleMentions.[rand3]
                    let mutable mentionedUserUsername = usernameIDMapping.[mentionedUserID]
                    let mutable randomNum1 = random.Next(0,tweetList.Length)
                    let mutable randomNum2 = random.Next(0, hashTags.Length)
                    let mutable mentionTweet = returnTweet(randomNum1,randomNum2)
                    mentionTweet <- string('@')+ string(mentionedUserUsername) + mentionTweet
                    tweetsTable.[myID].Add(mentionTweet);
                    clients.[mentionedUserID]<!AddToFeed(mentionTweet, myID, "mentioned", "Group")
                    // clients.[mentionedUserID]<!PrintMyFeed
                
                |   MentionSingleUser(mentUserID:int) ->

                    let mutable possibleMentions = clone(clientList)
                    possibleMentions.Remove(myID)|> ignore
                    let mutable rand3 = random.Next(0, possibleMentions.Count)
                    let mutable mentionedUserID = possibleMentions.[rand3]
                    let mutable mentionedUserUsername = usernameIDMapping.[mentionedUserID]
                    let mutable randomNum1 = random.Next(0,tweetList.Length)
                    let mutable randomNum2 = random.Next(0, hashTags.Length)
                    let mutable mentionTweet = returnTweet(randomNum1,randomNum2)
                    mentionTweet <- string('@')+ string(mentionedUserUsername) + mentionTweet
                    tweetsTable.[myID].Add(mentionTweet);
                    clients.[mentUserID]<!AddToFeed(mentionTweet, myID, "mentioned", "Individual")

                |   AddToFeed (tweet: string, from: int, action: string, actionType: string)->

                    match actionType with 
                    |   "Group" ->
                            myFeed.Add(tweet)   
                    |   "Individual" ->
                        if login then
                            myFeed.Add(tweet) 
                            printfn "In User %d's Feed: User %d %s %s" myID from action tweet   
                        else 
                            let combinedString = tweet+ ","+ string(from) + "," + action
                            feedsInQueue.Add(combinedString)

                    | _ -> printfn "nothing to do"
                |   PrintMyFeed ->
                    printfn " MY ID IS %d" myID
                    for i in myFeed do
                        printfn "%A\n" i
                
                |   Logout ->
                    login <- false 
                
                |   Login -> 
                    login <- true

                    for feed in feedsInQueue do
                        let mutable details = feed.Split(",")
                        let mutable from = int(details.[1])
                        printfn "In User %d'Feed : User %d %s %s" myID from details.[2] details.[0]
                        myFeed.Add(details.[0])
                    
                    feedsInQueue.RemoveAll |> ignore
                    
                    
                |_ -> printfn "unkown messageType recived"

                return! loop ()
            }
            loop ()