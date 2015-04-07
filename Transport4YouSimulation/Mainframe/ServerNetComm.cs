using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace Transport4YouSimulation
{
    class ServerNetComm
    {
        //instance of thread for listening
        private Thread listenThread;

        private bool isStopListen = false;
        //the instance of the componet dispatching request
        private ServiceDispatcher serviceDispatcher;

        //export this read-only property for outside to access its thread
        public Thread ListenThread
        {
            get
            {
                return listenThread;
            }
        }

        //flag of the time to stop the loop thread 
        public bool StopFlag
        {
            set
            {
                isStopListen = value;
            }
        }

        public ServerNetComm()
        {
            try
            {
                serviceDispatcher = new ServiceDispatcher();
                // start a thead to listen the port looply
                listenThread = new Thread(new ThreadStart(this.Listen));
                //start the thread
                listenThread.Start();
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void Listen()
        {
            TcpListener listener = null;
            try
            {
                //create a listenter to receive request on any IP with given port
                listener = new TcpListener(IPAddress.Any, 6666);
                //start to listen.This will not pend.
                listener.Start();

                //unless isStopListen is ture other wise this 
                while (!isStopListen)
                {
                    try
                    {
                        //wait for a new connect.This will pend.
                        TcpClient tcpClient = listener.AcceptTcpClient();
                        serviceDispatcher.ServiceDispatch(tcpClient);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("SocketException: {0}", e);
                        listenThread.Abort();
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e);
                        listenThread.Abort();
                        return;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                listenThread.Abort();
                return;
            }
            finally
            {
                //stop listening
                if (listener != null)
                {
                    listener.Stop();
                }
            }

        }

        public static void send(IPAddress ip,uint port, string content)
        {
            System.Windows.MessageBox.Show("sendmessage to "+ip.ToString()+":"+port.ToString()+",content="+content,"ServerNetComm.send()");
        }

    }
}
