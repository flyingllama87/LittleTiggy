using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace LittleTiggy
{
    public static class GameOptionsFile
    {
        private static string optionsFilename = "options.json";

        public static void LoadOptions()
        {
#if ANDROID
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var optionsFilename = Path.Combine(path, GameOptionsFile.optionsFilename);
#endif

            if (File.Exists(optionsFilename))
            {
                using (StreamReader file = new StreamReader(optionsFilename))
                {
                    LittleTiggy.playerName = Convert.ToString(file.ReadLine());
                    LittleTiggy.gameDifficulty = (GameDifficulty)Convert.ToInt32(file.ReadLine());
                    LittleTiggy.gameTouchControlMethod = (GameTouchControlMethod)Convert.ToInt32(file.ReadLine());
                }
                LittleTiggy.bHasEnteredName = true;
                LittleTiggy.kbInput = LittleTiggy.playerName;
                Debug.WriteLine("Opened file!");
            }
            
        }

        public static void SaveOptions()
        {

#if ANDROID
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var optionsFilename = Path.Combine(path, GameOptionsFile.optionsFilename);
#endif

            using (StreamWriter file = new System.IO.StreamWriter(optionsFilename))
            {
                file.WriteLine(LittleTiggy.playerName);
                file.WriteLine((int)LittleTiggy.gameDifficulty);
                file.WriteLine((int)LittleTiggy.gameTouchControlMethod);
                Debug.WriteLine("Wrote to File!");
            }



            /*file = file.OpenFile(optionsFilename, FileMode.CreateNew);


            using (StreamWriter sw = new StreamWriter(fs))
            {
                
            }*/
        }
    }
}
