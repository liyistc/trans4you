using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Diagnostics.Contracts;

namespace Transport4YouSimulation
{
    public static class ServerConfig
    {
        private static string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "serverconfig.ini";
        private static IniFile configFile = new IniFile(configFilePath);

        static ServerConfig()
        {
            if (!File.Exists(configFilePath))
            {
                StreamWriter sw = new StreamWriter(File.Create(configFilePath));
                sw.WriteLine("[ServerTest]");
                sw.WriteLine("T_COST=2");
                sw.WriteLine("T_VAL=2");
                sw.Flush();
                sw.Close();
            }
        }

        public static void ModifyT_COST(uint cost)
        {
            configFile.IniWriteValue("ServerTest", "T_COST", cost.ToString());
        }

        public static void ModifyT_VAL(uint minutes)
        {
            configFile.IniWriteValue("ServerTest", "T_VAL", minutes.ToString());
        }

        //[Pure]
        public static uint GetT_COST()
        {
            try
            {
                return uint.Parse(configFile.IniReadValue("ServerTest", "T_COST"));
            }
            catch
            {
                return 2;
            }
        }

        //[Pure]
        public static uint GetT_VAL()
        {
            try
            {
                return uint.Parse(configFile.IniReadValue("ServerTest", "T_VAL"));
            }
            catch
            {
                return 2;
            }
        }
    }
}
