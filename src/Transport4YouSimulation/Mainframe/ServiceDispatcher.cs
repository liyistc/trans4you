using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Transport4YouSimulation
{
    class ServiceDispatcher
    {
        private Thread dispatcherThread;
 

        public void ServiceDispatch(TcpClient tcpClient)
        {
            try
            {
                dispatcherThread = new Thread(ServiceThread);
                dispatcherThread.Start(tcpClient);
            }
            catch
            {
            }
        }

        public static void ServiceThread(object tcpClient)
        {
            NetworkStream stream = null;
            try
            {
                BinaryFormatter fmt = new BinaryFormatter();
                RequestInfo requestInfo = new RequestInfo();
                stream = ((TcpClient)tcpClient).GetStream();
                //lock (stream)
                {
                    //get the RequestInfo instance by Deserializing
                    requestInfo = (RequestInfo)fmt.Deserialize(stream);
                }
                switch (requestInfo.servicetype)
                {
                    case ServiceType.SERVICE_BUSREPORT:
                        BusReportMsg busReportMsg = (BusReportMsg)requestInfo.Content;
                        //transfer BusReportManager to process
                        BusReportManager busReportManger = new BusReportManager();
                        busReportManger.ManageReport(busReportMsg);
                        break;
                    case ServiceType.SERVICE_ROADCOND:
                        System.Windows.MessageBox.Show("SERVICE_ROADCOND");
                        break;
                    default:
                        break;
                }

            }
            catch
            {

            }
            finally
            {
                ((TcpClient)tcpClient).Close();
                if(stream != null)
                stream.Close();
            }
        }
    }
}
