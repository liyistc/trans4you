//#define CONTRACTS_FULL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Diagnostics.Contracts;
using UserInfoSet = System.Collections.Generic.HashSet<Transport4YouSimulation.UserInfo>;

namespace Transport4YouSimulation
{
   

    public class AccountManager
    {
        public static readonly string NO_CREDITCARD = "NIL";

        private static Dictionary<ulong, UserInfo> userBase;

        //[ContractInvariantMethod]
        //void AccountInvariant()
        //{
        //    //invariant
        //    Contract.Invariant(ServerConfig.GetT_COST() > 0 && ServerConfig.GetT_VAL() > 0, "T_COST AND T_VAL is illegal!");
        //}

        public static Dictionary<ulong, UserInfo> UserBase
        {
            get
            {
                return userBase;
            }
        }

        static AccountManager()
        {
            loadUserBase();
        }

        //[Pure]
        public static bool ContainsUser(ulong userCellPhoneAddr)
        {
            return userBase.ContainsKey(userCellPhoneAddr);
        }

        public static void QueryUser(ulong userCellPhoneAddr, out UserInfo userInfo)
        {
            if (userBase.TryGetValue(userCellPhoneAddr, out userInfo))
            {
                return;
            }
            else
            {
                throw new Exception();
            }

        }

        public static bool IsNewUserInfoLegal(UserInfo userInfo)
        {
            
            if(userBase.ContainsKey(userInfo.cellPhoneAddr))
            {
                return false;
            }

            return true;
        }

        public static void ModifyUser(UserInfo userInfo)
        { 
            ////pre-conditions
            //Contract.Requires(userInfo != null, "user info is null!");
            //Contract.Requires(userInfo.name.Length > 0, "user name length is zero!");
            //Contract.Requires(userInfo.cellPhoneNumber.Length > 0, "user cellphone number length is zero!");
            //Contract.Requires(ContainsUser(userInfo.cellPhoneAddr), "try to access a unregistered user!");
            ////post-conditions
            //Contract.Ensures(userBase[userInfo.cellPhoneAddr] == userInfo, "modification failed!");
            
            if (ContainsUser(userInfo.cellPhoneAddr))
            {
                lock (userBase)
                {
                    userBase[userInfo.cellPhoneAddr] = userInfo;
                    saveUserBase();
                }
            }
            
        }

        public static void ChargeUsers(BusReportMsg  busReportMsg, UserInfoSet userInfoSet)
        {
            if (busReportMsg.isBoard)
            {
                foreach (UserInfo aUserInfo in userInfoSet)
                {
                    TimeSpan ts = DateTime.Now.Subtract(aUserInfo.ticketBeginTime);
                    //if the user hold a valid ticket(within 2 hours but for simulation just one minute)
                    if (ts.TotalMinutes <= ServerConfig.GetT_VAL()  && ts.TotalMinutes>=0 && aUserInfo.hasTicket)
                    {
                        //if the user own a e-ticket, won't charge him/she
                        UserNotifycation.SendNotify(aUserInfo,"You have boarded on bus " + busReportMsg.busLine+" at "+busReportMsg.stopName +". You are not charged as you are holding a valid ticket.");
                        return;
                    }
                    else
                    {
                        //pay by credit card
                        if (aUserInfo.isPayByCreditCard)
                        {
                            UserInfo newUserInfo = aUserInfo;
                            newUserInfo.ticketBeginTime = DateTime.Now;
                            newUserInfo.hasTicket = true;
                            ModifyUser(newUserInfo);
                            UserNotifycation.SendNotify(newUserInfo,
                                "You have boarded on bus " + busReportMsg.busLine + " at " + busReportMsg.stopName+ ". You have been charged by your credit card and you will own an e-ticket for " + ServerConfig.GetT_VAL() + " hours.");
                        }
                        //pay by account balance
                        else
                        {
                            UserInfo newUserInfo = aUserInfo;
                            newUserInfo.hasTicket = false;
                            if ((newUserInfo.balance -= (int)ServerConfig.GetT_COST()) <= 0)
                            {
                                ModifyUser(newUserInfo);
                                //balance is less than 0, the user should be notified
                                UserNotifycation.SendNotify(newUserInfo, "You have boarded on bus " + busReportMsg.busLine + " at " + busReportMsg.stopName + ". You have been charged " + ServerConfig.GetT_COST() + " EUR and please add value!");
                            }
                            else
                            {
                                ModifyUser(newUserInfo);
                                UserNotifycation.SendNotify(newUserInfo, "You have boarded on bus " + busReportMsg.busLine + " at " + busReportMsg.stopName + ". You have been charged " + ServerConfig.GetT_COST() + " EUR.");
                            }
                        }

                    }

                }
            }

        }

        public static bool AddUser(ulong userCellPhoneAddr, UserInfo userInfo)
        {
            if (IsNewUserInfoLegal(userInfo))
            {
                lock (userBase)
                {
                    userBase.Add(userCellPhoneAddr, userInfo);
                    saveUserBase();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DeleteUser(ulong userCellPhoneAddr, UserInfo userInfo)
        {
            if (ContainsUser(userCellPhoneAddr))
            {
                lock (userBase)
                {
                    userBase.Remove(userCellPhoneAddr);
                    saveUserBase();
                }
            }
        }

        public static void loadUserBase()
        {
            StreamReader sr = null;
            string aUser = "";
            try
            {
                sr = new StreamReader(@"Users.txt");
                userBase = new Dictionary<ulong, UserInfo>();
                while ((aUser = sr.ReadLine()) != null)
                {
                    //Console.WriteLine(aUser);
                    string[] infos = aUser.Split(',');
                    UserInfo aUseInfo = new UserInfo();
                    aUseInfo.cellPhoneAddr = ulong.Parse(infos.ElementAt(0));
                    aUseInfo.name = infos.ElementAt(1);
                    aUseInfo.cellPhoneNumber = infos.ElementAt(2);
                    aUseInfo.hasTicket = bool.Parse(infos.ElementAt(3));
                    aUseInfo.ticketBeginTime = DateTime.Parse(infos.ElementAt(4));
                    aUseInfo.isPayByCreditCard = bool.Parse(infos.ElementAt(5));
                    aUseInfo.creditCardNumber = infos.ElementAt(6).ToString();
                    aUseInfo.balance = int.Parse(infos.ElementAt(7));
                    userBase.Add(aUseInfo.cellPhoneAddr, aUseInfo);
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

        public static void saveUserBase()
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(@"Users2.txt", false);
                Dictionary<ulong,UserInfo>.Enumerator userEnum= userBase.GetEnumerator();
                while (userEnum.MoveNext())
                {
                    sw.Write(userEnum.Current.Value.cellPhoneAddr + ",");
                    sw.Write(userEnum.Current.Value.name + ",");
                    sw.Write(userEnum.Current.Value.cellPhoneNumber + ",");
                    sw.Write(userEnum.Current.Value.hasTicket + ",");
                    sw.Write(userEnum.Current.Value.ticketBeginTime + ",");
                    sw.Write(userEnum.Current.Value.isPayByCreditCard + ",");
                    sw.Write(userEnum.Current.Value.creditCardNumber + ",");
                    sw.Write(userEnum.Current.Value.balance);
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
    }
}
