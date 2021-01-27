using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace AEVIDomain
{
  //  public struct EndPointData
  //  {
  //      public string host;
  //      public int port;
  //  }

    public class CUdpSender
    {
        string LogPath;
        string Host;
        int Port;

        //List<EndPointData> list_endpoint;

        public CUdpSender(string host, int port, string logpath)
        {
            try 
            {
                LogPath = logpath;
                Host = host;
                Port = port;
                //if(!string.IsNullOrEmpty(strendpoints))
               // {
               //     string[] ep = strendpoints.Split(';'); 
               //     parse_endpoint(ep); 
               // }
            }
            catch (Exception ex) {  }
        }

        static string SLMonths = "JanFebMarAprMayJunJulAugSepOctNovDec";

        public static string SYSLOGCreateTextMessage(int facility, int severity, DateTime tm, string host,
            string tag, string parameter, string text)
        {
            string time = string.Format("{0} {1} {2}", SLMonths.Substring((tm.Month - 1) * 3, 3),
                                        tm.Day.ToString().PadLeft(2), tm.ToString("HH:mm:ss"));
            return string.Format("<{0}>{1} {2} {3}: [{4}] {5}", facility * 8 + severity, time, host, tag,
                                    parameter, text);
        }

        static byte[] SYSLOGPrepareMessage(string message, Encoding encoding)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetBytes(message);
        }

        public void Send(int facility, string tag, string parameter, string text)
        {
            Log log = new Log(LogPath);
            try
            {
                string msg = SYSLOGCreateTextMessage(facility, 3, DateTime.Now, Environment.MachineName, 
                    tag, parameter, text);
                byte[] arr = SYSLOGPrepareMessage(msg, Encoding.UTF8);
                send(arr);
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
        }


        public void send (byte[] msg)
        {
            Log log = new Log(LogPath);
            IPEndPoint ipEndPoint;
            UdpClient udpClient = new UdpClient(); ;

            try
            {
               // foreach (EndPointData ep in list_endpoint)
               // {
                    udpClient = new UdpClient();
                    ipEndPoint = new IPEndPoint(IPAddress.Parse(Host), Port);

                    udpClient.Send(msg, msg.Length, ipEndPoint);

                    udpClient.Close();
              //  }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                log.Write(LogType.Error, "Incorrect port number");
                
            }
            catch (SocketException ex)
            {
                log.Write(LogType.Error, "Port is already in use");
               
            }
            catch (Exception ex)
            {
                log.Write(LogType.Error, ex.Message); 
                
            }
            finally { udpClient.Close(); }
        }

        /*private void parse_endpoint(string[] data)
        {
            Log log = new Log(LogPath);

            try
            {
                list_endpoint = new List<EndPointData>();
                EndPointData ep;

                if (data.Length > 0)
                {
                    foreach (string str in data)
                    {
                        string[] words = str.Split(':');

                        if (words.Length == 2)
                        {
                            ep = new EndPointData();

                            ep.host = words[0];
                            ep.port = 0;
                            int.TryParse(words[1], out ep.port);

                            list_endpoint.Add(ep);
                        }
                        else log.Write(LogType.Error, "Wrong number of parameters"); 
                        
                    }
                }
            }
            catch (Exception ex)
            {
                log.Write(LogType.Error, ex.Message); 
           }
        }*/
    }
}
