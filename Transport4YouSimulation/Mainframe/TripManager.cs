using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Transport4YouSimulation
{

    public class TripManager
    {
        private static Dictionary<ulong, TripInfo> tripBase;

        public static Dictionary<ulong, TripInfo> TripBase
        {
            get
            {
                return tripBase;
            }
        }

        static TripManager()
        {
            LoadTripBase();
        }

        public static void AddTripRecord(ulong userPhoneAddr, TripRecord tripRecord)
        {
            lock (tripBase)
            {
                if (!tripBase.ContainsKey(userPhoneAddr))
                {
                    tripBase.Add(userPhoneAddr, new TripInfo() { userCellPhoneAddr = userPhoneAddr, tripRecords = new List<TripRecord>() });
                }
                tripBase[userPhoneAddr].tripRecords.Add(tripRecord);
                SaveTripBase();
            }
        }

        public static void SaveTripBase()
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(@"Trips2.txt", false);
                Dictionary<ulong, TripInfo>.Enumerator tripEnum = tripBase.GetEnumerator();
                while (tripEnum.MoveNext())
                {
                    sw.Write(tripEnum.Current.Value.userCellPhoneAddr + ",(");
                    for (int i = 0; i < tripEnum.Current.Value.tripRecords.Count; ++i)
                    {
                        sw.Write(tripEnum.Current.Value.tripRecords[i].time + ",");
                        sw.Write(tripEnum.Current.Value.tripRecords[i].busLine + ",");
                        sw.Write(tripEnum.Current.Value.tripRecords[i].isBoard + ",");
                        sw.Write(tripEnum.Current.Value.tripRecords[i].stopName + ",");
                        sw.Write(tripEnum.Current.Value.tripRecords[i].busId);
                        if (tripEnum.Current.Value.tripRecords.Count > 1 && i != tripEnum.Current.Value.tripRecords.Count - 1)
                        {
                            sw.Write(";");
                        }
                    }
                    sw.Write(")");
                    sw.Write("\xd\xa");

                }
                sw.Flush();
            }
            catch
            {
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

        public static void LoadTripBase()
        {
            StreamReader sr = null;
            string aTripLine = "";
            try
            {
                sr = new StreamReader(@"Trips.txt");
                tripBase = new Dictionary<ulong, TripInfo>();
                while ((aTripLine = sr.ReadLine()) != null)
                {
                    List<TripRecord> tripRecords = new List<TripRecord>();
                    //Console.WriteLine(aUser);
                    string userCellPhoneAddr = aTripLine.Substring(0, aTripLine.IndexOf(","));
                    string[] records = aTripLine.Substring(aTripLine.IndexOf("(") + 1, aTripLine.IndexOf(")") - aTripLine.IndexOf("(") - 1).Split(';');
                    TripRecord aTripRecord = new TripRecord();
                    foreach (string aRecord in records)
                    {
                        string[] recordItems = aRecord.Split(',');
                        aTripRecord.time = DateTime.Parse(recordItems.ElementAt(0));
                        aTripRecord.busLine = recordItems.ElementAt(1);
                        aTripRecord.isBoard = bool.Parse(recordItems.ElementAt(2));
                        aTripRecord.stopName = recordItems.ElementAt(3);
                        tripRecords.Add(aTripRecord);

                    }
                    TripInfo tripInfo = new TripInfo();
                    tripInfo.userCellPhoneAddr = ulong.Parse(userCellPhoneAddr);
                    tripInfo.tripRecords = tripRecords;
                    tripBase.Add(tripInfo.userCellPhoneAddr, tripInfo);
                }

            }
            catch
            {
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }
    }
}
