using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UserInfoSet = System.Collections.Generic.HashSet<Transport4YouSimulation.UserInfo>;

namespace Transport4YouSimulation
{

    class BusReportManager
    {

        public BusReportManager()
        {
        }

        public void ManageReport(BusReportMsg busReportMsg)
        {
            UserInfoSet userInfoSet = new UserInfoSet();

            //only registered user will be handled
            foreach (ulong userPhoneAddr in busReportMsg.cellPhoneAddrSet)
            {

                //he or she is a registered user 
                if (AccountManager.ContainsUser(userPhoneAddr))
                {
                    lock (AccountManager.UserBase[userPhoneAddr])
                    {
                        TripRecord aTripRecord = new TripRecord();
                        aTripRecord.busLine = busReportMsg.busLine;
                        aTripRecord.isBoard = busReportMsg.isBoard;
                        aTripRecord.stopName = busReportMsg.stopName;
                        aTripRecord.time = busReportMsg.time;
                        TripManager.AddTripRecord(userPhoneAddr, aTripRecord);
                        UserInfo aUserInfo;
                        AccountManager.QueryUser(userPhoneAddr, out aUserInfo);
                        userInfoSet.Add(aUserInfo);
                    }
                        Console.WriteLine("Server has received MSG. BUSLINE={0}, userId={1}\n", busReportMsg.busLine, userPhoneAddr);

                }
            }

            //Charge users
            AccountManager.ChargeUsers(busReportMsg,userInfoSet);
        }
    }
}
