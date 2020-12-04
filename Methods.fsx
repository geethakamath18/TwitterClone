#r "nuget: FSharp.Json"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

#load "Variables.fsx"

open System
open FSharp.Json
open Akka.FSharp
open Akka.Actor
open System.Collections.Generic

open Variables.Variables

module Methods =
    let mutable fo = 0 
    let mutable rank = 0
    let mutable numberOfFollowers = Map.empty<int, int>
    
    let mutable rand = 0
    let mutable temp2 = new List<int>()

    let clone(l: List<int>)=
        let res= List<int>();
        for j in l do
            res.Add(j)
        res

    let generateUsername() =
        let mutable username ="";
        let mutable mid = random.Next(0,2) //Existence of underscore in the username
        let mutable j =0;
        while(j<5) do
            let randomLetter= alphabet.[random.Next(0,alphabet.Length)]
            username <-username + string(randomLetter)
            j<-j+1
        if mid=1 then
            username <- username + string(underscore)
        j<-0
        while(j<2) do
            let randomNumber= digits.[random.Next(0,digits.Length)]
            username <-username + string(randomNumber)
            j<-j+1
        username

    let constructTables() = 
        for i in [0 .. numClients-1] do
            clientTweet <- clientTweet.Add(i,0)

        for i in [0 .. numClients-1] do
            let mutable emptyList= new List<string>();
            tweetsTable <- tweetsTable.Add(i, emptyList) 

        for i in [0 .. numClients-1] do
            let mutable emptyList= new List<int>();
            followersTable<- followersTable.Add(i, emptyList) 
            followingTable<- followingTable.Add(i, emptyList) 

        for i in [0 .. numClients-1] do
            let mutable temp1 = new List<int>();             
            let mutable t = clone(clientList);
            t.RemoveAt(i);
            for j in [0 .. numFollowers-1] do
                let mutable temp2 = new List<int>();
                r <- t.[random.Next(0,t.Count)]
                if temp1.Contains(r) then
                    while((temp1.Contains(r))) do
                         r <- t.[random.Next(0,t.Count)]
                temp1.Add(r)
                if followersTable.ContainsKey(r) then
                    temp2 <- followersTable.[r]
                    if not(temp2.Contains(i)) then
                        followersTable.[r].Add(i)                            
                    else
                        p<-p+1; //DO NOTHING
                else 
                    temp2.Add(i)
                    followersTable <- followersTable.Add(r,temp2);
            followingTable <- followingTable.Add(i,temp1);
        
    let zipfDistrubtuion() = 

        let uppercap = float(numClients) * 0.7 |> int 
        for i in [0 .. numClients-1] do 

            rank <- i+2 
            if i < uppercap then
                numFollowers <- ceil( float(numClients)/ float(rank) )|> int
                numberOfFollowers <- numberOfFollowers.Add(i, numFollowers)
            else 
                numFollowers <- 1 
                numberOfFollowers <- numberOfFollowers.Add(i, numFollowers)


    let initializeTables() = 

        for i in [0 .. numClients-1] do
            let mutable emptyList= new List<int>();

            followersTable<- followersTable.Add(i, emptyList) 
            followingTable<- followingTable.Add(i, emptyList) 


    
    let constructTableWithzipf() = 
        zipfDistrubtuion()
        initializeTables()
        for i in [0 .. numClients-1] do
            clientTweet <- clientTweet.Add(i,0)

        for i in [0 .. numClients-1] do
            let mutable emptyList= new List<string>();
            tweetsTable <- tweetsTable.Add(i, emptyList)

        for i in [0 .. numClients-1] do 

            let totalFollowers = numberOfFollowers.[i] 
            let cloneOfClientList = clone(clientList)

            cloneOfClientList.Remove(i) |> ignore 

            let mutable temp1 = new List<int>(); 
            let mutable fol = 0

            for j in [0 .. totalFollowers-1] do 

                rand <- random.Next(0, cloneOfClientList.Count)
                fol <- cloneOfClientList.[rand]
                temp1.Add(fol)
                cloneOfClientList.Remove(fol)  |> ignore

                temp2 <- followingTable.[fol]
                temp2.Add(i)
                followingTable <- followingTable.Add(fol, temp2)

            followersTable <- followersTable.Add(i, temp1)
                    
    let logout(userId) = 
    
        clients.[userId] <!Logout

    let login(userId) = 
        clients.[userId] <! Login
        printfn "User %d logged in" userId


