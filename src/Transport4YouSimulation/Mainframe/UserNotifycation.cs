using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UserInfoSet = System.Collections.Generic.HashSet<Transport4YouSimulation.UserInfo>;

namespace Transport4YouSimulation
{
    class UserNotifycation
    {
        public static void SendNotify(UserInfo userInfo, string msg)
        {
            string sendout = userInfo.name +"("+ userInfo.cellPhoneNumber +")" +":"+ msg + "\n";
            //ServerNetComm.send(new System.Net.IPAddress(new byte[4]{127,0,0,1}),8888,sendout);
            if (PassengerBase.PassengerTable != null && PassengerBase.PassengerTable[userInfo.cellPhoneAddr] !=null)
            {
                PassengerBase.PassengerTable[userInfo.cellPhoneAddr].Message += sendout;
            }
        }
        public static void NotifyUsers(UserInfoSet userInfoSet,string msg)
        {
            foreach (UserInfo aUserInfo in userInfoSet)
            {
                if (PassengerBase.PassengerTable!=null)
                {
                    if(PassengerBase.PassengerTable.ContainsKey(aUserInfo.cellPhoneAddr))
                    {
                        SendNotify(aUserInfo, msg);
                    }
                }
            }
        }
        public void getAllRelatedUser(out UserInfoSet userInfoSet)
        {
            userInfoSet = new UserInfoSet();
        }

    }
}
