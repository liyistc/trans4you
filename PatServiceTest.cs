using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PAT.Common;
using PAT.Common.Ultility;
using OutOfMemoryException = PAT.Common.Classes.Expressions.ExpressionClass.OutOfMemoryException;
using PAT.Common.Classes.Assertion;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;
using System.Diagnostics;

namespace PAT.Lib
{
    /// <summary>
    /// The math library that can be used in your model.
    /// all methods should be declared as public static.
    /// 
    /// The parameters must be of type "int", or "int array"
    /// The number of parameters can be 0 or many
    /// 
    /// The return type can be bool, int or int[] only.
    /// 
    /// The method name will be used directly in your model.
    /// e.g. call(max, 10, 2), call(dominate, 3, 2), call(amax, [1,3,5]),
    /// 
    /// Note: method names are case sensetive
    /// </summary>
    public class PatServiceTest : ExpressionValue
    {
        private static int testCaseCounter=0;
        //private static Transport4YouSimulation.PATService patService;
        public static readonly string[] StopTable = { "TerminalA", "Stop5", "Stop7","Stop9","Stop58","Stop31","Stop33","Stop53","Stop57","TerminalC","Stop56",
                                                    "Stop52","Stop32","Stop30","Stop59","Stop10","Stop8","Stop6","Stop13","Stop22","Stop40","Stop42",
                                                    "Stop43","Stop41","Stop23","Stop14","Stop25","Stop39","Stop37","TerminalD","Stop36","Stop38",
                                                    "Stop24", "Stop15", "Stop19", "Stop20", "Stop47", "Stop46", "Stop21", "Stop18", "TerminalB","Stop17",
                                                    "Stop61", "Stop29", "Stop45", "Stop49", "Stop48", "Stop44", "Stop28", "Stop60", "Stop16","Stop12",
                                                    "Stop27", "Stop55", "Stop51", "Stop50", "Stop54", "Stop26", "Stop11", "Stop35", "Stop34"};
        public PatServiceTest()
        {
            //TestDataLoader.InitBus();
            //patService = new Transport4YouSimulation.PATService();
  
        }

        public void TestMethod(int startIndex, int endIndex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter("planLog.txt", true);
                string startPoint = StopTable[startIndex];
                string endPoint = StopTable[endIndex];
                string result = ExecuteCommandSync(System.Environment.CurrentDirectory, "QuickGraph.exe", startPoint + " " + endPoint);
                string[] cells1 = result.Split(',');
                string result2 = ExecuteCommandSync(System.Environment.CurrentDirectory, "RoutePlanningConsole.exe", startPoint + " " + endPoint);
                string[] cells2 = result2.Split(',');
                string passInfo = "";
                if(Convert.ToInt32(cells2[0]) <= Convert.ToInt32(cells1[0]))
                {
                    passInfo = "pass";
                }
                else
                {
                    passInfo = "fail";
                }

                sw.WriteLine((++testCaseCounter)+": "+passInfo + "; From "+startPoint + " to "+endPoint+
                    "; PAT RoutePlan Length: " + cells2[0] + "; " + "Dijkstra Aloghrithm Length: "+
                    cells1[0] +"; PAT actionList: "+cells2[1] +"; Dijkstra Aloghrithm actionList: "+cells1[1]);
                
            }
            catch
            {
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }

        }

        public string ExecuteCommandSync(string path,string fullname,string para)
        {
            string result = "";
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.

                Process proc = new Process();

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                proc.StartInfo.WorkingDirectory = path;
                proc.StartInfo.FileName = fullname;
                proc.StartInfo.Arguments = para;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                // Do not create the black window.
                proc.StartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                proc.Start();
                proc.WaitForExit();
                // Get the output into a string
                result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
            }
            catch (Exception objException)
            {
                // Log the exception
            }
            return result;
        }


        /// <summary>
        /// Please implement this method to provide the string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }


        /// <summary>
        /// Please implement this method to return a deep clone of the current object
        /// </summary>
        /// <returns></returns>
        public override ExpressionValue GetClone()
        {
            return this;
        }


        /// <summary>
        /// Please implement this method to provide the compact string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ExpressionID
        {
            get { return ""; }
        }

    }



}
