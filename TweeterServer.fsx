//Command to run file: dotnet fsi --langversion:preview experiment1.fsx 100 5
//Change numFollowers and numRetweets

#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 

#load "Variables.fsx"
#load "Methods.fsx"
#load "Script.fsx"
#load "Twitterclient.fsx"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

open Script.Script
open Variables.Variables

open Methods.Methods
open Twitterclient.Twitterclient

open System.Collections.Generic
open Akka.FSharp
open System.Diagnostics
// open BackendData

let mutable time =  System.Diagnostics.Stopwatch() 

module TweeterServer = 


    let master (mailbox: Actor<_>) = 
        let rec masterloop() = actor{
            let! msg =  mailbox.Receive ()
            match msg with
            |   Start ->
                printfn "Creating Users"
                for i in [0 .. numClients-1] do
                    let mutable name = generateUsername()
                    if uniqueUsername.Contains(name) then
                        while not(uniqueUsername.Contains(name)) do
                            name<- generateUsername()
                    uniqueUsername.Add(name);
                    // System.Threading.Thread.Sleep(3000);
                    usernameIDMapping <- usernameIDMapping.Add(i,name)
                    let properties = string(i)
                    let clientActor = spawn twitterSystem properties client
                    clients.Add(clientActor) 
                    clients.[i]<!Init(i, name)

            |   ConstructTables ->    
                printfn ""
                printfn "Generating Followers USing ZipF Distrubtuion"
                constructTableWithzipf()
                    // printfn "Done initializing"
            |   StartTweeting ->
                let mutable y = 0
                
                printfn ""
                printfn "Users starts Tweeting"

                while(y<5) do
                    for z in clientList do
                        clients.[z]<! TweetNow
                        let mutable n = clientTweet.[z];
                        // clientTweet.[z]<-n+1;
                        clientTweet<-clientTweet.Remove(z)
                        clientTweet<-clientTweet.Add(z, n+1)
                    y<-y+1

            |   DeleteUser ->
                let mutable rand = random.Next(0, clientList.Count)
                clientList.Remove(rand)|>ignore
                // printfn "Randomly chosen user is : %d" rand
                temp3<-followersTable.[rand] //Get zero's followers
                temp4<-followingTable.[rand]
                for i in temp3 do
                    followingTable.[i].Remove(rand)|>ignore
                for i in temp4 do    
                    followersTable.[i].Remove(rand)|>ignore
                followingTable<-followingTable.Remove(rand); //Erase 0
                followersTable<-followersTable.Remove(rand);
                printfn "user %d deleted his account" rand
            
            |   ProcessTweet (s:string, t:string) ->
                if hashtagTable.ContainsKey(t) then
                    hashtagTable.[t].Add(s)
                else 
                    let mutable tempString = new List<string>();
                    tempString.Add(s);
                    hashtagTable <- hashtagTable.Add(t,tempString)

            |   StartRetweeting ->
                let numRetweets = 3 // int(floor(0.02*float(numClients))) //2 percent of the clients will retweet
                let mutable k = 0;

                printfn ""
                printfn "Users starts Retweeting"
                while k<numRetweets do
                    let mutable rand1 = random.Next(0, clientList.Count) //Selecting random client who will retweet :tweeter

                    let mutable t = clone(clientList);
                    t.Remove(rand1)|>ignore
                    let mutable rand2 = random.Next(0, t.Count) //Selecting client whose tweet will be retweeted :source
                    clients.[clientList.[rand1]]<!RetweetNow(rand2)
                    k<-k+1

            |   StartMentioning ->

                let mutable rand4 = random.Next(0,clientList.Count)
                clients.[clientList.[rand4]]<!MentionNow

            |_ -> printfn "unkown messageType recived"
            return! masterloop ()
        }      
        masterloop()

    let main (args:string []) =

        let mutable demoType = args.[2]
        numClients<-(int) args.[1] // Setting the value of number of nodes
        // printfn "%d" numFollowers
        // numTweets<- (int)  args.[2] // Setting the value value for the topology
        for i in [0 .. numClients-1] do
            clientList.Add(i)
        wait()
        let twitterRef = spawn twitterSystem "twitter" master  // Spawning master actor  
        server.Add(twitterRef)
        match demoType with 
        |  "DemoOne" -> 
            
            demoOne(numClients)
            wait()
        | "DemoTwo" ->
            printvalues <- false
            printf "%A" clientList.Count
            time <- Stopwatch.StartNew()
            demoTwo(numClients)
            time.Stop()
            printfn "Total time taken is : %A" time.Elapsed.TotalMilliseconds
            wait()
            wait()
        | _ -> 
               printfn "Invalid Demo Type"
  
            
        // demoTwo(largenumber)
        printfn "Press Enter to exit"
        System.Console.ReadLine() |> ignore
        0
     
    let args = fsi.CommandLineArgs 
    match args.Length with //Checking number of parameters
        | 3 -> main args    
        | _ ->  failwith "You need to pass two parameters!"