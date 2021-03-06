﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using JsonRPC;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;

namespace LittleTiggy
{
    public partial class LittleTiggy : Game
    {

        public static string LeaderBoardAPIEndpoint = "http://morganrobertson.net/LTLeaderBoard/api";
        // public static string LeaderBoardAPIEndpoint = "http://127.0.0.1:5000/api"; // Local flask test instance
        public static bool bDisableNetworkCalls = false;
        bool bGetScoresRequested = false;
        bool bGetScoresComplete = false;
        public static int apiTimeOut = 7500; // in milliseconds

        private BackgroundWorker BackgroundHTTPWorker = new BackgroundWorker();
        List<Tuple<string, int, string>> leaderBoardScores = new List<Tuple<string, int, string>>();

        private void BackgroundHTTPWorker_Initialise(LeaderBoardAPICall APICall)
        {
            BackgroundHTTPWorker.DoWork += new DoWorkEventHandler(BackgroundHTTPWorker_DoWork);
            if (APICall == LeaderBoardAPICall.AddScore)
            {
                BackgroundHTTPWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundHTTPWorker_AddScoreComplete);
            }
            else if (APICall == LeaderBoardAPICall.GetScores)
            {
                BackgroundHTTPWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundHTTPWorker_GetScoresComplete);
            }
        }

        private void BackgroundHTTPWorker_DoWork(object sender, DoWorkEventArgs eventArgs)
        {
            LeaderBoardClient LittleTiggyLBClient = (LeaderBoardClient)eventArgs.Argument;
            if (LittleTiggyLBClient.APICall == LeaderBoardAPICall.AddScore)
            {
                eventArgs.Result = LittleTiggyLBClient.AddScore();
            }
            else if (LittleTiggyLBClient.APICall == LeaderBoardAPICall.GetScores)
            {
                eventArgs.Result = LittleTiggyLBClient.GetScores();
                bGetScoresRequested = true;
            }

        }

        private void BackgroundHTTPWorker_AddScoreComplete(object sender, RunWorkerCompletedEventArgs eventArgs)
        {
            if (eventArgs.Result != null && (bool)eventArgs.Result)
            {
                Debug.WriteLine("AddScore BG worker completed");
            }
            BackgroundHTTPWorker.DoWork -= new DoWorkEventHandler(BackgroundHTTPWorker_DoWork);
            BackgroundHTTPWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BackgroundHTTPWorker_AddScoreComplete);
        }

        private void BackgroundHTTPWorker_GetScoresComplete(object sender, RunWorkerCompletedEventArgs eventArgs)
        {
            bGetScoresComplete = true;

            Debug.WriteLine("GetScores BG worker completed");

            BackgroundHTTPWorker.DoWork -= new DoWorkEventHandler(BackgroundHTTPWorker_DoWork);
            BackgroundHTTPWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BackgroundHTTPWorker_GetScoresComplete);

            leaderBoardScores = (List<Tuple<string, int, string>>)eventArgs.Result;

        }
    }

    public enum LeaderBoardAPICall
    {
        AddScore,
        GetScores
    }

    class LeaderBoardClient
    {
        public LeaderBoardAPICall APICall;
        public string name;
        public int score;
        public string difficulty;

        public LeaderBoardClient()
        {
            if (WebRequest.DefaultWebProxy != null)
            {
                WebRequest.DefaultWebProxy = null;
            }
        }


        public bool AddScore() //AddScore API call
        {
            try
            {
                using (Client rpcClient = new Client(LittleTiggy.LeaderBoardAPIEndpoint, LittleTiggy.apiTimeOut))
                {

                    Debug.WriteLine("API Endpoint:" + LittleTiggy.LeaderBoardAPIEndpoint);
                    Debug.WriteLine("Player name to send: " + this.name);

                    JArray parameters = JArray.Parse(@"['" + this.name + @"', '" + this.score + @"', '" + this.difficulty + "']");

                    Request request = rpcClient.NewRequest("app.AddScore", parameters);

                    GenericResponse response = rpcClient.Rpc(request);

                    if (response.Result != null)
                    {
                        JToken result = response.Result;
                        Debug.WriteLine(result);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Error in response! Error code: {0}", response.Error.Code);
                        return false;
                    }

                }
            }
            catch (System.Exception ex)
            {
                LittleTiggy.bDisableNetworkCalls = true;
                Debug.WriteLine("Unable to connect to API endpoint");
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public List<Tuple<string, int, string>> GetScores() // getScores API call
        {
            try
            {

                using (Client rpcClient = new Client(LittleTiggy.LeaderBoardAPIEndpoint, LittleTiggy.apiTimeOut))
                {

                    Debug.WriteLine("API Endpoint:" + LittleTiggy.LeaderBoardAPIEndpoint);

                    Request request = rpcClient.NewRequest("app.GetScores");

                    GenericResponse response = rpcClient.Rpc(request);

                    if (response.Result != null)
                    {
                        List<Tuple<string, int, string>> TBScores = new List<Tuple<string, int, string>>();
                        JToken result = response.Result;
                        Debug.Write(result.ToString());
                        JArray jArray = JsonConvert.DeserializeObject<JArray>(response.Result.ToString());
                        foreach (JObject jObject in jArray)
                        {
                            Tuple<string, int, string> score = new Tuple<string, int, string>(jObject["name"].ToString(), (int)jObject["score"], jObject["difficulty"].ToString());
                            Debug.Write(score.ToString());

                            TBScores.Add(score);
                            Debug.Write(name);
                        }

                        return TBScores;
                    }
                    else
                    {
                        Debug.WriteLine("Error in response! Error code: {0}", response.Error.Code);
                        Tuple<string, int, string>[] emptyTupleArray = new Tuple<string, int, string>[0];
                        List<Tuple<string, int, string>> emptyTupleList = new List<Tuple<string, int, string>>();
                        return emptyTupleList;
                    }

                }
            }
            catch (System.Exception ex)
            {
                LittleTiggy.bDisableNetworkCalls = true;
                Debug.WriteLine("Unable to connect to API endpoint");
                Debug.WriteLine(ex.ToString());
                List<Tuple<string, int, string>> emptyTupleList = new List<Tuple<string, int, string>>();
                return emptyTupleList;
            }
        }
    }


}
