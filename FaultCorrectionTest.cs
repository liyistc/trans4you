using System;
using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions.ExpressionClass;
using System.IO;
using System.Linq;
using System.Diagnostics.Contracts;


//the namespace must be PAT.Lib, the class and method names can be arbitrary
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
      
    public struct Record
    {
        public List<int> eventList;
        public int item1;
        public int item2;
        public int item3;
    }
        
    public class  BusLog : ExpressionValue
    {
        private List<int> traceList;
        private List<int> busList;
        private List<int> userList;
        private List<Record> flagList;
        private List<Record> recordList;   
        private int busStates;    

        //private static readonly string[] onOrOff = {"alight","board"};
        //private static ulong AAACounter = 0;
        //private static ulong ABACounter = 0;
        //private static ulong ABBCounter = 0;
        //private static ulong ABCCounter = 0;
        static bool hasWrittenTitle = false;
    
        public BusLog(int numOfStates)
        {
            busStates = numOfStates;
            traceList = new List<int>();
            busList = new List<int>();
            userList = new List<int>();
            recordList = new List<Record>();
            flagList = new List<Record>();
        }
    
        public BusLog(int numOfStates,List<int> paraTraceList,List<int> paraBusList,List<int> paraUserList,List<Record> praraRecordList,List<Record> paraFlagList)
        {
            busStates = numOfStates;
            traceList = paraTraceList;
            busList = paraBusList;
            recordList = praraRecordList;
            flagList = paraFlagList;
            userList = paraUserList;
        }
              
        public int[] SetEvent(int e)
        {
            traceList.Add(e);
            if(e>=8000)
            {
                userList.Add(e);
            }
            else
            {
                busList.Add(e);
            }
            int[] returnedArray = new int[traceList.Count];
            traceList.CopyTo(returnedArray);
            return returnedArray;
        }
        
        public void AddFlagRecord(int item1,int item2,int item3)
        {
            Record newRecord;
            newRecord.eventList = new List<int>(traceList);
            newRecord.item1 = item1;
            newRecord.item2 = item2;
            newRecord.item3 = item3;
            flagList.Add(newRecord);
        }
                
        public void AddRecord(int item1,int item2,int item3)
        {
                Record newRecord;
                newRecord.eventList = new List<int>(traceList);
                newRecord.item1 = item1;
                newRecord.item2 = item2;
                newRecord.item3 = item3;
                recordList.Add(newRecord);
        }
        
        public void FlushEventLeft()
        {
            StreamWriter sw = null;
            StreamWriter sw2 = null;
            try
            {
                if(busList.Count != busStates || flagList.Count%2!=0
                || flagList.Count == 0  || recordList.Count == 0 )
                {
                    return;
                }
                                
                sw = new StreamWriter("buslog.txt",true);
                                
                int[] seqOfBus=FaultTolerance.GetBusSeq(recordList);
                
                if(!hasWrittenTitle)
                {
                    sw.WriteLine("flag,records,final,trace");
                    hasWrittenTitle = true;
                }
               
                //sw.Write("[Flag:");
                string flagStr = "";
                
                for(int i = 0; i < seqOfBus.Length; ++i)
                {
                   if(seqOfBus[i] == 0)
                   {
                       break;
                   }
                   List<Record> tmpList = flagList.FindAll(item => item.item1 == seqOfBus[i]);
                   if(tmpList.Count() ==0)
                   {
                       break;
                   }
                   sw.Write(seqOfBus[i] +"");
                   foreach(Record aRecord in tmpList)
                   {
                       flagStr += ((char)(aRecord.item2+64)).ToString();
                       sw.Write(((char)(aRecord.item2+64)).ToString());
                   }
                }
                //sw.Write("]");
                /*
                for(int i =0; i < flagList.Count; ++i)
                {
                    sw.Write("("+flagList[i].item1+",");
                    sw.Write((char)(flagList[i].item2+64)+",");
                    sw.Write(onOrOff[flagList[i].item3]+")");
                    if(i!= flagList.Count-1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write("]");
                */
                
                /*sw.Write(",[Origin:");
                
                for(int i = 0;i < recordList.Count; ++i)
                {
                    sw.Write("("+recordList[i].item1+",");
                    sw.Write((char)(recordList[i].item2+64)+",");
                    sw.Write(onOrOff[recordList[i].item3]+")");
                    if(i!= recordList.Count-1)
                    {
                        sw.Write(",");
                    }
                }
               
                sw.Write("]");*/
                
                //sw.Write(",[Records:");
                sw.Write(",");
                string recordsStr = "";
                for(int i = 0; i < seqOfBus.Length; ++i)
                {
                   if(seqOfBus[i] == 0)
                   {
                       break;
                   }
                   List<Record> tmpList = recordList.FindAll(item => item.item1 == seqOfBus[i]);
                   if(tmpList.Count() ==0)
                   {
                       break;
                   }
                   sw.Write(seqOfBus[i] +"");
                   foreach(Record aRecord in tmpList)
                   {
                       recordsStr += ((char)(aRecord.item2+64)).ToString();
                       sw.Write(((char)(aRecord.item2+64)).ToString());
                   }
                }
                //sw.Write("]");
                
                /*
                sw.Write(",[Transfer:");              
                foreach(FaultTolerance.ResultRecord aRecord in FaultTolerance.TransferRecords(recordList))
                {
                    sw.Write(aRecord.busNo + "");
                    foreach(FaultTolerance.TripRecord aTrip in aRecord.trips)
                    {
                        sw.Write((char)(aTrip.boardStop+64) + "");
                        sw.Write((char)(aTrip.alightStop+64) + "");
                    }
                }
                sw.Write("]");
                */
                
                //sw.Write(",[Final:");
                sw.Write(",");
                string finalStr = "";
                
                foreach(FaultTolerance.ResultRecord aRecord in FaultTolerance.FinalDecision(recordList))
                {
                    sw.Write(aRecord.busNo+"");
                    foreach(FaultTolerance.TripRecord aTrip in aRecord.trips)
                    {
                        finalStr += (char)((int)aTrip.boardStop+64) + "";
                        finalStr += (char)((int)aTrip.alightStop+64) + "";
                        sw.Write((char)((int)aTrip.boardStop+64) + "");
                        sw.Write((char)((int)aTrip.alightStop+64) + "");
                    }
                }
                      
                //sw.Write("]");
                sw.Write(",");
                //sw.Write(",[Trace:");
                foreach(int aEvent in traceList)
                {
                    int oneState=aEvent;
                    string str = "";
                    if(oneState >= 8000)
                    {
                        str += "u.";
                        if(oneState/1000 == 8)
                        {
                            str += "on.";
                        }
                        else
                        {
                            str += "off.";
                        }
                        oneState -= (oneState/1000)*1000;
                        str += oneState/100 + ".";
                        oneState -= (oneState/100)*100;
                        str += (char)(oneState + 'A' - 1) +"";
                    }
                    else
                    {
                        if(oneState/1000 ==1)
                        {
                            str += "get.";
                        }
                        else
                        {
                            str += "leave.";
                        }
                        oneState -= (oneState/1000)*1000;
                        str += oneState/100 +".";
                        oneState -= (oneState/100)*100;
                        str += (char)(oneState + 'A' - 1) +"";
                        
                    }
                    sw.Write(str + ",");
                    //sw.Write(str + "->");
                }
                sw.WriteLine("Skip");
                //sw.Write("Skip]");                
                sw.Flush();

                /*                
                if(flagStr == recordsStr && flagStr == finalStr)
                {
                    ++AAACounter;
                }
                
                if(flagStr == finalStr && flagStr != recordsStr)
                {
                    ++ABACounter;
                }
                
                if(flagStr != recordsStr && recordsStr == finalStr)
                {
                    ++ABBCounter;
                }
                
                if(flagStr != recordsStr && flagStr != finalStr && recordsStr != finalStr)
                {
                    ++ABCCounter;
                }
                
                sw2 = new StreamWriter("statistics.txt",false);
                sw2.WriteLine("AAACounter="+AAACounter);
                sw2.WriteLine("ABACounter="+ABACounter);
                sw2.WriteLine("ABBCounter="+ABBCounter);
                sw2.WriteLine("ABCCounter="+ABCCounter);
                */
               
            }
            catch
            {
            }
            finally
            {
                if(sw!=null)
                {
                    sw.Close();
                }
                if(sw2!=null)
                {
                    sw2.Close();
                }
            }
        }
    

        /// <summary>
        /// Please implement this method to return a deep clone of the current object
        /// </summary>
        /// <returns></returns>
        public override ExpressionValue GetClone()
        {
            List<int> hisTraceList = new List<int>(traceList);
            List<int> hisBusList = new List<int>(busList);
            List<int> hisUserList = new List<int>(userList);
            List<Record> hisRecordList = new List<Record>();
            List<Record> hisFlagList = new List<Record>();
            foreach(Record aRecord in recordList)
            {
                Record newRecord;
                newRecord.eventList = aRecord.eventList;
                newRecord.item1 = aRecord.item1;
                newRecord.item2 = aRecord.item2;
                newRecord.item3 = aRecord.item3;
                hisRecordList.Add(newRecord);
            }
            foreach(Record aRecord in flagList)
            {
                Record newRecord;
                newRecord.eventList = aRecord.eventList;
                newRecord.item1 = aRecord.item1;
                newRecord.item2 = aRecord.item2;
                newRecord.item3 = aRecord.item3;
                hisFlagList.Add(newRecord);
            }
            BusLog clonedBusLog = new BusLog(busStates,hisTraceList,hisBusList,hisUserList,hisRecordList,hisFlagList);
            return clonedBusLog;
        }


        /// <summary>
        /// Please implement this method to provide the string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "["+ ExpressionID + "]";
        }

        /// <summary>
        /// Please implement this method to provide the compact string representation of the datatype
        /// </summary>
        /// <returns></returns>
        public override string ExpressionID
        {
            get
            {
            string returnedString = "";
            
            if(recordList.Count ==0)
            {
                foreach(int aEvent in traceList)
                {
                    returnedString+= aEvent + "->";
                }
            }   
            else
            {
                List<int> tmpList = new List<int>();
                foreach(Record aRecord in recordList)
                {
                    foreach(int aEvent in aRecord.eventList)
                    {
                        if(!tmpList.Contains(aEvent))
                        {
                            returnedString += aEvent + "->";
                            tmpList.Add(aEvent);
                        }

                    }
                    
                    returnedString += "("+aRecord.item1+",";
                    returnedString += aRecord.item2+",";
                    returnedString += aRecord.item3+")";
                    returnedString += "->";
                    
                }    
                foreach(int aEvent in traceList)
                {
                    if(!tmpList.Contains(aEvent))
                    {
                        returnedString += aEvent+"->";
                    }
                }
            }
            
            return returnedString;
            }
        }
    }
    
    public class FaultTolerance
    {
        public enum BusStop{ini,A,B,C,D,E,F,G,H};
        private static readonly List<BusStop> BusOneRoute = new List<BusStop>{BusStop.A,BusStop.B,BusStop.C,BusStop.D};
        private static readonly List<BusStop> BusTwoRoute = new List<BusStop>{BusStop.A,BusStop.B,BusStop.C,BusStop.D};
        private static readonly List<BusStop> BusThreeRoute = new List<BusStop>{BusStop.B,BusStop.C,BusStop.E,BusStop.F};
        private static readonly List<BusStop> BusFourRoute = new List<BusStop>{BusStop.A,BusStop.B,BusStop.G,BusStop.H};     
        private static readonly List<BusStop>[] BusRoutes = {BusOneRoute,BusTwoRoute,BusThreeRoute,BusFourRoute};
        
        public struct OriginalRecord
        {
            public int busNo;
            public List<BusStop> route;
        }
        
        public struct TripRecord
        {
            public BusStop boardStop;
            public BusStop alightStop;
        }
        
        public struct ResultRecord
        {
            public int busNo;
            public HashSet<TripRecord> trips;
        }
           
        private static List<BusStop> GetSubRoute(int busNo,BusStop boardStop, BusStop alightStop)
        {
            int startIndex = 0, endIndex = 0;
            List<BusStop> route = BusRoutes[busNo-1];
            for(endIndex=0; endIndex < route.Count; ++endIndex)
            {
                if(route[endIndex]==boardStop)
                {
                    startIndex = endIndex;
                    continue;
                }
                if(route[endIndex]==alightStop)
                {
                    break;
                }
            }
            
            List<BusStop> returnedList = new List<BusStop>();
            for(int i = startIndex; i <= endIndex; ++i)
            {
                returnedList.Add(route[i]);
            }
            
            return returnedList;
        }
        
        public static List<ResultRecord> TransferRecords(List<Record> records)
        {
            List<ResultRecord> returnedRecords = new List<ResultRecord>();
            int[] seqOfBus = GetBusSeq(records);
            for(int i = 0; i < seqOfBus.Length; ++i)
            {
                if(seqOfBus[i]==0)
                {
                    break;
                }
                List<Record> tmpRecords = records.FindAll(item => item.item1 == seqOfBus[i]);
                if(tmpRecords != null && tmpRecords.Count%2==0)
                {
                    for(int j = 0; j < tmpRecords.Count - 1; j+=2)
                    {
                        AddTrip(returnedRecords, seqOfBus[i], (BusStop)tmpRecords[j].item2,(BusStop)tmpRecords[j+1].item2);
                    }
                    
                }
            }
            
            return returnedRecords;
        }
        
        public static List<ResultRecord> FinalDecision(List<Record> records)
        {
            List<ResultRecord> returnedRecords = new List<ResultRecord>();
            if(records.Count == 2)
            {
                AddTrip(returnedRecords,records[0].item1,(BusStop)records[0].item2,(BusStop)records[1].item2);
                return returnedRecords;
            }
            
            List<OriginalRecord> originalRecords = new List<OriginalRecord>(); 
            foreach(ResultRecord aRecord in TransferRecords(records))
            {
                foreach(TripRecord aTrip in aRecord.trips)
                {
                    originalRecords.Add(new OriginalRecord(){busNo = aRecord.busNo,route=GetSubRoute(aRecord.busNo,aTrip.boardStop,aTrip.alightStop)});
                }
            }
            
            //mind what should be compared here!
            for(int i = 0; i < originalRecords.Count-1; ++i)
            {
                for(int j = i+1; j < originalRecords.Count; ++j)
                {
                    if(originalRecords[i].busNo != originalRecords[j].busNo)
                    {
                        Analyze(originalRecords[i], originalRecords[j],returnedRecords);
                    }
                }
            }
            return returnedRecords;
        }
        
        private static void Analyze(OriginalRecord originalRecord1,OriginalRecord originalRecord2,List<ResultRecord> resultRecords)
        {
            if(!CanDo(originalRecord1.route,originalRecord2.route))
            {
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(originalRecord1.route.Count-1));
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(originalRecord2.route.Count-1)); 
                return;
            }
            
            IEnumerable<BusStop> intersection = originalRecord1.route.Intersect<BusStop>(originalRecord2.route);
            int numOfIntersection = intersection.Count();
            int index1=0,index2=0,count1=0,count2=0;
            
            index1 = originalRecord1.route.FindIndex(item => item == intersection.ElementAt(0));
            index2 = originalRecord2.route.FindIndex(item => item == intersection.ElementAt(0));
            count1 = originalRecord1.route.Count();
            count2 = originalRecord2.route.Count();
            
            //if first trip's head is longer than second and tail is the same long
            if(index1 > index2 && count1-index1 == count2-index2)
            {
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(originalRecord1.route.Count-1));
            }
            //if second trip's head is longer than first and tail is the same long
            else if(index2 > index1 && count1-index1 == count2-index2)
            {
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(originalRecord2.route.Count-1));
            }
            //if first trip's tail is longer than second and head is the same long
            else if(index1 == index2 && count1-index1 > count2-index2 )
            {
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(originalRecord1.route.Count-1));
            }
            //if second trip's tail is longer than second and head is the same long
            else if(index1 == index2 && count2-index2 > count1-index1 )
            {
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(originalRecord2.route.Count-1));
            }
            //if first trip's head is longer and second's tail is longer
            else if(index1 > index2 && count2-index2 > count1-index1)
            {
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(index1));
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(originalRecord2.route.Count-1));
            }
            //if second trip's head is longer and first's tail is longer
            else if(index2 > index1 && count1-index1 > count2-index2)
            {
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(index2));
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(originalRecord1.route.Count-1));
            }
            //if first trip's head and tail are all longer
            else if(index1 > index2 && count1-index1 > count2-index2)
            {
                AddTrip(resultRecords,originalRecord1.busNo,originalRecord1.route.ElementAt(0),originalRecord1.route.ElementAt(originalRecord1.route.Count-1));
            }
            //if second trip's head and tail are all longer
            else if(index2 > index1 && count2-index2 > count1-index1)
            {
                AddTrip(resultRecords,originalRecord2.busNo,originalRecord2.route.ElementAt(0),originalRecord2.route.ElementAt(originalRecord2.route.Count-1));
            }
            else
            {
            }
        }
        
        private static bool NeedDo(List<BusStop> route1, List<BusStop> route2)
        {
            IEnumerable<BusStop> intersection = route1.Intersect<BusStop>(route2);
            if(intersection.Count() >= 2)
            {
                return true;
            }
            if(intersection.Count() == 1)
            {
                int index1 = route1.FindIndex(item => item == intersection.ElementAt(0));
                int index2 = route2.FindIndex(item => item == intersection.ElementAt(0));
                if (route1.Count > index1 + 1 && route2.Count > index2 + 1)
                {
                    return true;
                }
                else if (route1.Count == index1 + 1 && route2.Count == index2 + 1)
                {
                    return true;
                }
                else
                {
                }
            }
            return false;
        }
        
        private static bool CanDo(List<BusStop> route1, List<BusStop> route2)
        {
            if(!NeedDo(route1,route2))
            {
                return false;
            }
            if(route1.Count == route2.Count)
            {
                IEnumerable<BusStop> intersection = route1.Intersect<BusStop>(route2);
                if(route1.Count==route2.Count)
                {
                    if(intersection.Count() == route1.Count || intersection.Count() == route1.Count -1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        private static void AddTrip(List<ResultRecord> records,int paraBusNo, BusStop paraBoardStop, BusStop paraAlightStop)
        {
            TripRecord tripRecord = new TripRecord(){boardStop = paraBoardStop,alightStop = paraAlightStop};
            if(records.FindIndex(item => item.busNo == paraBusNo) < 0)
            {
                ResultRecord resultRecord = new ResultRecord(){busNo = paraBusNo,trips = new HashSet<TripRecord>()};
                resultRecord.trips.Add(tripRecord);
                records.Add(resultRecord);
            }
            else
            {
                HashSet<TripRecord> trips = records.Find(item => item.busNo == paraBusNo).trips;
                trips.Add(tripRecord);
            }
        }
        
        
        public static int[] GetBusSeq(List<Record> recordList)
        {
            int[] seqOfBus={0,0,0,0};
            foreach(Record aRecord in recordList)
            {
                bool hasFound = false;
                int firstEmptyIndex = 0;
                for(int i = 0; i < 4; ++i)
                {
                    if(seqOfBus[i]==0)
                    {
                        firstEmptyIndex = i;
                        break;
                    }
                    if(seqOfBus[i]==aRecord.item1)
                    {
                        hasFound = true;
                    }
                }
                if(!hasFound)
                {
                    seqOfBus[firstEmptyIndex] = aRecord.item1;
                }
            }
            return seqOfBus;
        }
    }
}
