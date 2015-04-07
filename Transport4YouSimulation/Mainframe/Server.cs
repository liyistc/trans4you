using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Transport4YouSimulation
{
    public class Server
    {
        private static ServerNetComm serverNetComm;
        public static void Start()
        {
            try
            {
                //start to listen for receving request
                serverNetComm = new ServerNetComm();
                //TODO:这里仅仅是为了编码而模拟BUS自动向SERVER发消息，日后清除
                //BusNetComm busNetComm = new BusNetComm();

                //if the thread reports aborted we can know there are bugs in that field 
                //do
                //{
                //    if (serverNetComm.ListenThread.ThreadState == ThreadState.AbortRequested
                //        || serverNetComm.ListenThread.ThreadState == ThreadState.Aborted)
                //    {
                //        throw new Exception("serverNetComm.ListenThread stop abnomally");
                //    }
                //} while (serverNetComm.ListenThread.ThreadState == ThreadState.Running);

                //Environment.Exit(0);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                Environment.Exit(-1);
            }
        }

        public static void Stop()
        {
            serverNetComm.StopFlag = true;
        }

    }

}
