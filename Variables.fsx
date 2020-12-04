#r "nuget: FSharp.Json"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

open System
open FSharp.Json
open Akka.FSharp
open Akka.Actor
open System.Collections.Generic

module Variables =

    let hashTags = [" #goGators";" #fbf";" #tbt";" #ootd";" #dadStop";" #10yearschallenge"; " #dadJokes"; " #funny"; " #dadHumour"; " #humour"]

    let tweetList = ["How do you get a squirrel to like you? Act like a nut."; 
                     "Adam driver looks like someone tried to draw Keanu Reeves from memory"; 
                     "Ed Sheeran looks like an imaginary friend";
                     "Why don't eggs tell jokes? They'd crack each other up.";
                     "What do you call someone with no body and no nose? Nobody knows.";
                     "I'm on a seafood diet. I see food and I eat it.";
                     "I used to hate facial hair...but then it grew on me.";
                     "Don't trust atoms. They make up everything!";
                     "What did the fish say when he hit the wall? Dam.";
                     "I wouldn't buy anything with velcro. It's a total rip-off.";
                     "I ordered a chicken and an egg online. I’ll let you know.";
                     "How do you tell the difference between an alligator and a crocodile? You will see one later and one in a while.";
                     "Do I enjoy making courthouse puns? Guilty";]

    let alphabet = ['a' .. 'z']

    let underscore = '_'

    let  digits = [0 .. 9]

    let twitterSystem = System.create "twitterSystem" <| Configuration.defaultConfig()

    let mutable server =new List<IActorRef>()

    let mutable clients=new List<IActorRef>() 

    let mutable numClients = 0;

    let mutable numTweets = 0;

    let random = Random()

    let mutable numFollowing = 0;

    let mutable numFollowers = 0;

    let mutable r = 0;

    let mutable c = 0;

    let mutable p =0;
    
    let mutable printvalues = true
    
    let mutable temp3 = new List<int>();

    let mutable temp4 = new List<int>();

    //Creating tables
    let mutable followingTable = Map.empty<int, List<int>>

    let mutable followersTable = Map.empty<int, List<int>>

    let mutable tweetsTable = Map.empty<int, List<string>>

    let mutable hashtagTable = Map.empty<string, List<string>>

    let mutable clientList = new List<int>();

    let mutable clientTweet = Map.empty<int,int>

    let mutable uniqueUsername = new List<string>();

    let mutable usernameIDMapping = Map.empty<int, string> 

    type ActorMessageType = 
        |   Init of int * string
        |   StartTweeting
        |   ConstructTables
        |   DeleteUser
        |   TweetNow
        |   Start 
        |   ProcessTweet of string * string
        |   StartRetweeting
        |   RetweetNow of int
        |   StartMentioning
        |   MentionNow 
        |   AddToFeed of string * int * string * string
        |   PrintMyFeed
        |   Login 
        |   Logout
        |   SingleTweet
        |   MentionSingleUser of int
       
