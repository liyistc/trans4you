using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Transport4YouSimulation
{
    public class BusNetComm
    {
        /*
        private bool isStopSend= false;
        public bool StopFlag
        {
            set
            {
                isStopSend = value;
            }
        }*/

        public BusNetComm()
        {
            /*
            try
            {
                for (int i = 1; i < 4; ++i)
                {
                    BusReportMsg busReportMsg = new BusReportMsg();
                    busReportMsg.time = DateTime.Now;
                    busReportMsg.isBoard = true;
                    busReportMsg.busLine =  (UInt32)i;
                    busReportMsg.cellPhoneAddrSet = new HashSet<ulong>(){10000L+(ulong)i};
                    (new Thread(this.SendMsg)).Start(busReportMsg);
                }
            }
            catch
            {
            }*/
        }
        public void SendMsg(BusReportMsg busReportMsg)
        {

            BinaryFormatter fmt = new BinaryFormatter();

            //do
            {
                try
                {
                    //TODO:这个在将来要移除
                    //Thread.Sleep(2000);

                    TcpClient client = new TcpClient();
                    client.Connect("127.0.0.1", 6666);
                    NetworkStream stream = client.GetStream();
                    RequestInfo requestInfo = new RequestInfo();
                    requestInfo.servicetype = ServiceType.SERVICE_BUSREPORT;
                    requestInfo.Content = busReportMsg;

                    //byte[] bytes = Encoding.Unicode.GetBytes(busReportMsg.ToString());
                    lock (stream)
                    {
                        //stream.Write(bytes, 0, bytes.Length);
                        fmt.Serialize(stream,requestInfo);
                    }
                    Console.WriteLine("bus NO.{0} has sent MSG to server", busReportMsg.busLine);
                    stream.Close();
                    client.Close();
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                    //break;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                    //break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                    //break;
                }

            } //while (!isStopSend);

        }
    }
}
