using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PAT.Common;
using PAT.Common.Ultility;
using OutOfMemoryException = PAT.Common.Classes.Expressions.ExpressionClass.OutOfMemoryException;
using PAT.Common.Classes.Assertion;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;

namespace Transport4YouSimulation
{
    public class PATService
    {
        private ModuleFacadeBase modulebase;

        public PATService()
        {
            modulebase = PAT.Common.Ultility.Ultility.LoadModule("CSP");
        }

        public List<string> RoutePlan(Dictionary<String,RoadStop> stops,Dictionary<String,Bus> buslines,string start,string dest,bool crossEnable)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"#import ""BusLine"";");
            //Current Stop
            sb.AppendLine("var current_stop = " + start + ";");
            //Current Bus
            sb.AppendLine("var temp = [-2];");
            sb.AppendLine("var<BusLine> current_bus = new BusLine(temp,-1);");
            //Enum all the stops
            sb.Append("enum {");
            for (int i = 0; i < stops.Keys.Count; i++)
            {
                if (i == 0)
                    sb.Append(stops.Keys.ElementAt<string>(i));
                else
                    sb.Append(", " + stops.Keys.ElementAt<string>(i));
            }

            sb.AppendLine("};");
            //List all the bus lines
            foreach (Bus busline in buslines.Values)
            {
                sb.Append("var s" + busline.BusLineName + " = [");
                for (int i = 0; i < busline.Stops.Count; i++)
                {
                    if(i==0)
                        sb.Append(busline.Stops.ElementAt<string>(i));
                    else
                        sb.Append(", " + busline.Stops.ElementAt<string>(i));
                }
                sb.AppendLine("];");
                sb.AppendLine("var<BusLine> " + busline.BusLineName + " = new BusLine(s" + busline.BusLineName + ","+busline.BusLineNo+");");
            }
            //Bus stop descriptions
            sb.AppendLine("takeBus(d) = case{");
            foreach (RoadStop rs in stops.Values)
            {
                sb.Append("  current_stop == " + rs.StopName + ": ");
                for (int i = 0; i < rs.BusLines.Count; i++)
                {
                    if (i != 0)
                        sb.Append("[]");                    
                    if (rs.BusLines.Count > 1)
                    {                        
                        sb.Append("[!" + rs.BusLines.ElementAt<string>(i) + ".IsRedundent(current_bus,current_stop)]");
                    }
                    sb.Append("Bus"+rs.BusLines.ElementAt<string>(i));
                }
                sb.AppendLine();
            }
            sb.AppendLine("};");
            //Bus line processes: Line name must be the format "Line$$$"
            foreach (Bus busline in buslines.Values)
            {
                if(crossEnable)
                    sb.AppendLine("Bus" + busline.BusLineName + " = TakeBus." + busline.BusLineNo + "{current_stop = " + busline.BusLineName + ".NextStop(current_stop);current_bus="+busline.BusLineName+ ";} -> plan;");
                else
                    sb.AppendLine("Bus" + busline.BusLineName + " = TakeBus." + busline.BusLineNo + "{current_stop = " + busline.BusLineName + ".NextStop(current_stop);current_bus=" + busline.BusLineName + ";} -> takeBus(0);");
            }
            //List all cross possibilities
            sb.AppendLine("crossRoad(d) = case{");
            foreach (RoadStop rs in stops.Values)
            {
                if (rs.Opposite != null)
                {
                    sb.AppendLine("  current_stop == " + rs.StopName + ": cross{current_stop = " + rs.Opposite.StopName + "} -> takeBus(0)");
                }
            }
            sb.AppendLine("};");
            //Goal
            sb.AppendLine("plan = takeBus(0) [] crossRoad(0);");
            //Enlarge the destination
            if (stops[dest].Opposite != null)
            {
                dest += "|| current_stop == " + stops[dest].Opposite.StopName;
            }
            sb.AppendLine("#define goal current_stop == " + dest +";");
            sb.AppendLine("#assert plan reaches goal;");

            //Console.Write(sb.ToString());
            return VerifyModel(sb.ToString());
        }

        public List<string> VerifyModel(string model)
        {                        
            SpecificationBase Spec = modulebase.ParseSpecification(model, "", "");
            List<string> RouteDescription = new List<string>();

            try
            {
                AssertionBase assertion = Spec.AssertionDatabase.Values.ElementAt(0);
                assertion.UIInitialize(null, FairnessType.NO_FAIRNESS, false, false, false, true, false, false);
                assertion.InternalStart();

                //if (assertion.VerificationOutput.VerificationResult.Equals(VerificationResultType.INVALID))
                //{
                //    RouteDescription.Add("NoSolution");
                //    return RouteDescription;
                //}
                foreach (ConfigurationBase step in assertion.VerificationOutput.CounterExampleTrace)
                {
                    if (step.Event != "init")
                    {
                        RouteDescription.Add(step.GetDisplayEvent());
                    }
                }
                return RouteDescription;
            }
            catch (RuntimeException ex)
            {
                System.Console.Out.WriteLine("Runtime exception occurred: " + ex.Message);
                if (ex is OutOfMemoryException)
                {
                    System.Console.Out.WriteLine(
                        "This error suggests your model is too big to be verified. Please make sure all your variables are bounded. You can use domain range values to check it, e.g., \"var x:{1..100}=0;\". Alternatively you can simplify your model by using fewer events, simplier processes, and smaller constants and variables.");
                }
                else
                {
                    System.Console.Out.WriteLine("Check your input model for the possiblity of errors.");
                }
            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine("Error occurred: " + ex.Message);
            }

            return null;
        }
    }
}
