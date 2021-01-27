using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Security.Cryptography;


//-----------------------------------------------------------------
// For WEB-AEVI Service
//-----------------------------------------------------------------

namespace AEVIDomain
{
    //-----------------------------------------------------------------
    // Request
    //
    // Action = “DeleteCard”
    // Data = “PAN”
    //
    // Action = “InsertCard”
    // Data = “PAN|RecValue|I1|I2|I3”
    //
    // Action = “UpdateCard”
    // Data = “PAN|RecValue|I1|I2|I3”
    //
    // 
    //-----------------------------------------------------------------

    public static class AEVI
    {
        public static void AEVITest()
        {
            // Инициализация каналов, тестовая среда
            string[] channelsArray = new string[] { "TestChannel;192.168.34.31:53105;userWEBTestServer;keyASDF8765" };
            MPChannels.Init(channelsArray);

            if (MPChannels.IsAvailable("TestChannel")) { }

            // Запрос
            string retCode, text;
            AEVI.Request("TestChannel", "DeleteCard", "77777", out retCode, out text);
            if (retCode != "0") { }
        }

        
        
        // всегда возвращает непустой resultCode
        public static bool Request(
            string channels,
            string action,
            string data,
            out string resultCode,
            out string text
            )
        {
         //   try
         //   {
                resultCode = text = null;
                if (string.IsNullOrEmpty(channels)) { resultCode = "5001"; text = "Empty request destination"; return false; }
                string[] clist = channels.Split(',');

                string answer;
                string packet = "R1|" + action + "|" + data;

                foreach (string ch in clist)
                {
                    if (!MPChannels.IsAvailable(ch)) continue;
                    answer = MPChannels.SendAndWaitString(ch, "AEVI.WEB", packet, 10 * 1000);
                    //if (answer == null) continue;
                    TAG t = new TAG(answer);
                    resultCode = t["RC"];
                    if (string.IsNullOrEmpty(resultCode)) continue;
                    text = t["Text"];
                    return true;
                }
                resultCode = "5002"; text = "No channel available";
         //   }
          //  catch(Exception ex) {log.Write(LogType.Error, "Request" + ex.Message);}
            return false;
        }
    }


    //-----------------------------------------------------------------
    // MP-Channels
    // externals - ManualTimer, CRCGenerator
    //-----------------------------------------------------------------
    public static class MPChannels
    {
        public static void Init(string[] channelsDescription)
        {
            MPChannelSettings.Init(channelsDescription);
            MPChannelsPool.Start();
            System.Threading.ThreadPool.QueueUserWorkItem(_cycleChannelsUp);
        }

        static void _cycleChannelsUp(object o)
        {
            ICollection collect = MPChannelSettings.GetChannelList();
            while (true)
            {
                foreach (object ch in collect)
                {
                    MPChannelSettings cs = ch as MPChannelSettings;
                    if (cs == null) { cs.Available = false; continue; }
                    string answer = SendAndWaitString(cs.Name, "Echo", "echoPattern", 10 * 1000);
                    if (answer == "echoPattern") cs.Available = true;
                    else cs.Available = false;
                }
                System.Threading.Thread.Sleep(10 * 1000);
            }
        }

        public static bool IsAvailable(string channelName)
        {
            MPChannelSettings cs = MPChannelSettings.Find(channelName);
            if (cs == null) return false;
            return cs.Available;
        }

        public static MPProtectedChannel GetChannel(string channelName, int waitTime) { return MPChannelsPool.GetChannel(channelName, waitTime); }
        public static void ReleaseChannel(MPProtectedChannel channel) { MPChannelsPool.ReleaseChannel(channel); }

        public static string SendAndWaitString(string channelName, string destId, object packet, int timeout)
        {
            byte[] answer;
            if (SendAndWaitAnswer(channelName, destId, packet, timeout, out answer) == false) return null;
            System.IO.BinaryReader r = new System.IO.BinaryReader(new System.IO.MemoryStream(answer));
            string s = r.ReadString();
            r.Close();
            return s;
        }

        public static bool SendAndWaitAnswer(string channelName, string destId, object packet, int timeout)
        {
            byte[] answer = null;
            return SendAndWaitAnswer(channelName, destId, packet, timeout, out answer);
        }

        public static bool SendAndWaitAnswer(string channelName, string destId, object packet, int timeout, out byte[] answer)
        {
            answer = null;
            try
            {
                MPProtectedChannel pc = GetChannel(channelName, timeout);
                if (pc == null) return false;
                if (pc.SendAndWaitAnswer(destId, packet, timeout, out answer) == false)
                {
                    pc.Close();
                    return false;
                }
                ReleaseChannel(pc);
                return true;
            }
            catch //(Exception e)
            {
                //string s = e.ToString();
                //CLog.Log("BAD error: " + s);
                //CLog.LogError("BAD error: " + s);
            }
            return false;
        }

        internal static void Log(string s) { }

    }

    //-----------------------------------------------------------------
    // Channel Settings
    //-----------------------------------------------------------------
    /*
    <MPChannels> 
        <!-- RemoteHostId;local_IP:port;MyId;key -->
        <string>CH1;localhost:27887;WEB-Server;ASDF8765</string>
    </MPChannels>
    */

    public class MPChannelSettings
    {
        static Hashtable list = Hashtable.Synchronized(new Hashtable());
        public static ICollection GetChannelList() { return list.Values; }
        public static MPChannelSettings Find(string channelName) { return (MPChannelSettings)list[channelName]; }

        MPChannelSettings() { }

        bool _available;
        public bool Available { get { return _available; } set { _available = value; } }

        string[] array;
        public string Name { get { return array[0]; } }
        public string Address { get { return array[1]; } }
        public string NameForConnect { get { return array[2]; } }
        public string KeyForSend { get { return GetDefaultKey(array[3]); } }
        public string KeyForReceive { get { return GetDefaultKey(array[3]); } }

        static string[] _ChannelsDescription;
        public static string[] GetChannelsDescription()
        {
            return _ChannelsDescription;
        }
        public static void Init(string[] channelsDescription)
        {
            _ChannelsDescription = channelsDescription;
            foreach (string s in GetChannelsDescription())
            {
                MPChannelSettings settings = BuildItem(s);
                if (settings != null) list[settings.Name] = settings;
            }
        }
        //-------------------------------------------------------------
        static MPChannelSettings BuildItem(string settings)
        {
            string[] sa = settings.Split(';');
            if (sa.Length < 4) return null;
            MPChannelSettings user = new MPChannelSettings();
            user.array = sa;
            return user;
        }

        static string GetDefaultKey(string s)
        {
            if (s == null) s = "+++";
            if (s.Length == 24) return s;
            return (s + "123456789-123456789-1234").Substring(0, 24);
        }
    }

    //-----------------------------------------------------------------
    // Channels Pool
    //-----------------------------------------------------------------
    public static class MPChannelsPool
    {
        static List<MPProtectedChannel> list = new List<MPProtectedChannel>();
        static object _locker = new object();
        static bool IsOpened;

        public static void Start()
        {
            IsOpened = true;
            System.Threading.ThreadPool.QueueUserWorkItem(_cycle);
        }

        public static void Stop()
        {
            lock (_locker)
            {
                IsOpened = false;
                foreach (MPProtectedChannel item in list) item.Close();
                list.Clear();
            }
        }

        static void _cycle(object o)
        {
            while (IsOpened)
            {
                try
                {
                    lock (_locker)
                        foreach (MPProtectedChannel item in list)
                        {
                            if (item.IsValid == false)
                            {
                                //MPChannels.Log(string.Format("Channel {0} removed from pool", item.Id));
                                item.Close();
                                list.Remove(item);
                                break;
                            }
                        }
                }
                catch { }
                System.Threading.Thread.Sleep(2 * 1000);
            }
        }

        static MPProtectedChannel GetChannel(string channelName, bool newRequest)
        {
            lock (_locker)
                foreach (MPProtectedChannel item in list)
                {
                    if (item == null) { list.Remove(item); return null; }
                    if (item.Connected && item.Name == channelName)
                    {
                        list.Remove(item);
                        //MPChannels.Log(string.Format("Channel {0} reallocated", item.Id));
                        return item;
                    }
                }
            if (newRequest == true)
            {
                MPProtectedChannel pc = MPProtectedChannel.CreateChannel(channelName);
                if (pc == null) return null;
                lock (_locker) { list.Add(pc); }
            }
            return null;
        }

        public static void ReleaseChannel(MPProtectedChannel channel)
        {
            //MPChannels.Log(string.Format("Channel {0} released (added to pool)", channel.Id));
            channel._updateTimers();
            lock (_locker) { list.Add(channel); }
        }
        public static void CloseChannel(MPProtectedChannel channel)
        {
            channel.Close();
        }

        public static MPProtectedChannel GetChannel(string channelName, int waitTime)
        {
            if (MPChannelSettings.Find(channelName) == null) return null;

            if (waitTime <= 0) waitTime = 20 * 1000;
            ManualTimer timer = new ManualTimer(waitTime);
            MPProtectedChannel pc = GetChannel(channelName, true);
            while (!timer.Timeout)
            {
                if (pc != null) return pc;
                pc = GetChannel(channelName, false);
                System.Threading.Thread.Sleep(200);
            }
            MPChannels.Log(string.Format("Cannot open channel to {0}", channelName));
            return null;
        }
    }

    //-----------------------------------------------------------------
    // Protected Channel
    //-----------------------------------------------------------------
    public class MPProtectedChannel
    {
        //-----------------------------------------------------
        static int _marker_counter = 0;
        static object _locker = new object();
        static int newMarker { get { lock (_locker) return ++_marker_counter; } }
        //-----------------------------------------------------
        enum State { Connecting, Connected, Closed }
        State state = State.Connecting;

        MPChannelSettings settings;
        public int marker;
        public string Id;
        public string Name { get { return settings.Name; } }

        const int SendByteTimeout = 15 * 1000;
        const int ReceiveByteTimeout = 40 * 1000;
        const int SendIdleTimeout = 60 * 1000;

        //const int NotUsedTimeout = 5 * 60 * 1000;
        const int NotUsedTimeout = 30 * 60 * 1000;

        public ManualTimer idleSendTimer;
        public ManualTimer stopTimer;

        public TcpClient client;

        BinaryReader channelStreamReader;
        BinaryWriter channelStreamWriter;
        SymmetricAlgorithm alg;

        MemoryStream packetBodyStream = new MemoryStream();
        BinaryReader packetBodyStreamReader;
        BinaryWriter packetBodyStreamWriter;

        Queue priorityQueue = Queue.Synchronized(new Queue());
        Queue deliveryQueue = Queue.Synchronized(new Queue());
        object workRequest;
        byte[] workAnswer;
        string workDestId;

        public static MPProtectedChannel CreateChannel(string channelName)
        {
            MPChannelSettings settings = MPChannelSettings.Find(channelName);
            if (settings == null) return null;
            return new MPProtectedChannel(settings);
        }

        MPProtectedChannel(MPChannelSettings settings)
        {
            this.settings = settings;
            marker = newMarker;
            Id = marker.ToString() + ":" + Name;
            state = State.Connecting;
            //MPChannels.Log(string.Format("*** Channel {0} created", Id));
            System.Threading.ThreadPool.QueueUserWorkItem(_cycle, null);
        }

        public void Close()
        {
            if (client != null)
                try
                {
                    MPChannels.Log(string.Format("*** Channel {0} closed", Id));
                    client.Client.Close();
                    client.Close();
                    packetBodyStream.Close();
                    alg.Clear();
                }
                catch { }
            state = State.Closed;
            client = null;
        }

        public bool IsValid { get { return state != State.Closed; } }
        public bool Connected { get { return state == State.Connected; } }

        void _cycle(object o)
        {
            try { _setup(); _cycle(); }
            catch { }
            Close();
        }

        static string GetHostName(string url)
        {
            string[] sa = url.Split(':');
            return sa[0];
        }
        static int GetPort(string url)
        {
            string[] sa = url.Split(':');
            return int.Parse(sa[1]);
        }

        void _setup()
        {
            client = new TcpClient(GetHostName(settings.Address), GetPort(settings.Address));

            MPChannels.Log(string.Format("*** Channel {0} connected {1}->{2}",
                Id, client.Client.LocalEndPoint, client.Client.RemoteEndPoint));

            client.SendTimeout = SendByteTimeout;
            client.ReceiveTimeout = ReceiveByteTimeout;

            channelStreamReader = new BinaryReader(client.GetStream());
            channelStreamWriter = new BinaryWriter(client.GetStream());
            alg = TripleDES.Create();
            alg.Key = Encoding.ASCII.GetBytes(settings.KeyForSend);

            packetBodyStreamReader = new BinaryReader(packetBodyStream);
            packetBodyStreamWriter = new BinaryWriter(packetBodyStream);

            channelStreamWriter.Write("PCh"); // Just HeaderId
            channelStreamWriter.Write(""); // Settings
            channelStreamWriter.Write(settings.NameForConnect);
            channelStreamReader.ReadString(); // OK

            stopTimer = new ManualTimer(NotUsedTimeout);
            idleSendTimer = new ManualTimer(SendIdleTimeout);

            state = State.Connected;
        }

        public void _updateTimers()
        {
            stopTimer.Reset();
            idleSendTimer.Reset();
        }

        void _cycle()
        {
            while (client.Connected && !stopTimer.Timeout)
            {
                if (idleSendTimer.Timeout)
                {
                    if (sendCryptedPacketAndWaitAnswer("SYS0", "Idle") == false) break;
                    idleSendTimer.Reset();
                    //MPChannels.Log(string.Format("Channel {0} idle", Id));
                    continue;
                }

                if (workRequest != null)
                {
                    if (sendCryptedPacketAndWaitAnswer(workDestId, workRequest) == false) break;
                    //workAnswer = new byte[packetBodyStream.Length];
                    //Array.Copy(packetBodyStream.GetBuffer(), packetBodyStream.Position, (byte[])workAnswer, 0, packetBodyStream.Length);
                    long dataLength = packetBodyStream.Length - packetBodyStream.Position;
                    byte[] tmp = new byte[dataLength];
                    Array.Copy(packetBodyStream.GetBuffer(), packetBodyStream.Position, tmp, 0, dataLength);
                    workAnswer = tmp;
                    workRequest = null;
                    _updateTimers();
                    continue;
                }

                if (priorityQueue.Count > 0)
                {
                    if (sendCryptedPacketAndWaitAnswer("SYSPq", priorityQueue.Dequeue()) == false) break;
                    _updateTimers();
                    continue;
                }

                if (deliveryQueue.Count > 0)
                {
                    if (sendCryptedPacketAndWaitAnswer("SYSDq", deliveryQueue.Dequeue()) == false) break;
                    _updateTimers();
                    continue;
                }

                System.Threading.Thread.Sleep(200);
            }
        }

        static bool _appendPacketBody(BinaryWriter writer, object o)
        {
            if (o.GetType() == typeof(string)) writer.Write((string)o);
            else if (o.GetType() == typeof(byte[])) writer.Write((byte[])o);
            else if (o.GetType() == typeof(MemoryStream))
            {
                int i;
                while ((i = ((MemoryStream)o).ReadByte()) >= 0) writer.Write((byte)i);
            }
            else return false;
            return true;
        }

        int packetCounter = 0;
        bool sendCryptedPacket(string destinationId, object packet)
        {
            //MPChannels.Log(string.Format("<{0},{1}/{2} {3}", Id, packetCounter, destinationId, packet));
            packetBodyStream.SetLength(0);
            packetBodyStreamWriter.Write(packetCounter++);
            packetBodyStreamWriter.Write(destinationId);
            if (_appendPacketBody(packetBodyStreamWriter, packet) == false) return false;

            packetBodyStream.Position = 0;
            MPPCSecurity.EncryptAndWritePacket(channelStreamWriter, packetBodyStream, alg);
            return true;
        }

        bool receiveCryptedAnswer(int counter, string destinationId)
        {
            // receive packet
            // compare 'counter' with received (if != -1)
            // compare 'destinationId' with received (if != null)
            // on return: packetBodyStream contains packet, starting from current position
            packetBodyStream.SetLength(0);
            if (MPPCSecurity.ReadAndDecryptPacket(channelStreamReader, packetBodyStream, alg) == false) return false;
            packetBodyStream.Position = 0;
            int R_counter = packetBodyStreamReader.ReadInt32();
            string R_destId = packetBodyStreamReader.ReadString();

            //string tmp = Encoding.ASCII.GetString( packetBodyStream.GetBuffer(), (int)packetBodyStream.Position, (int)packetBodyStream.Length);
            //MPChannels.Log(string.Format(">{0},{1}/{2} {3}", Id, R_counter, R_destId, tmp));

            if (counter != -1) if (counter != R_counter) return false;
            if (destinationId != null) if (destinationId != R_destId) return false;
            return true;
        }

        bool sendCryptedPacketAndWaitAnswer(string destinationId, object packet)
        {
            if (sendCryptedPacket(destinationId, packet) == false) return false;
            if (receiveCryptedAnswer(packetCounter - 1, destinationId) == false) return false;
            return true;
        }

        // external, for separate thread
        // no exception
        public bool SendAndWaitAnswer(string destId, object packet, int timeout)
        {
            if (timeout <= 0) timeout = 20 * 1000;
            ManualTimer timer = new ManualTimer(timeout);
            workAnswer = null;
            workDestId = destId;
            workRequest = packet;
            while (workAnswer == null && this.IsValid && !timer.Timeout) System.Threading.Thread.Sleep(200);
            if (workAnswer != null) return true;
            Close();
            return false;
        }

        public bool SendAndWaitAnswer(string destId, object packet, int timeout, out byte[] answer)
        {
            bool res = SendAndWaitAnswer(destId, packet, timeout);
            answer = workAnswer;
            return res;
        }

        public void Release()
        {
            MPChannels.ReleaseChannel(this);
        }


    }

    //-----------------------------------------------------------------
    // Packets
    //-----------------------------------------------------------------
    public static class MPPCSecurity
    {
        static Random rnd = new Random();
        static int GetRandom() { return rnd.Next(); }

        const int MaxInPacketLength = 1024 * 6;
        const uint CRCSeed = 0xEE609AF7;

        public static bool ReadAndDecryptPacket(BinaryReader srcStreamReader, MemoryStream destStream, SymmetricAlgorithm alg)
        {
            destStream.SetLength(0);

            //int tmout = srcStreamReader.BaseStream.ReadTimeout;
            if (srcStreamReader.ReadChar() != 'P') return false;
            int len = srcStreamReader.ReadInt32();
            if (len > MaxInPacketLength) return false;
            int iv = srcStreamReader.ReadInt32() - 200;
            byte[] buf = srcStreamReader.ReadBytes(len - 8);
            uint crc = srcStreamReader.ReadUInt32() - 200;

            if (CRCGenerator.CRC32(buf, 0, buf.Length, CRCSeed) != crc) return false;

            alg.IV = Encoding.ASCII.GetBytes(iv.ToString("X8"));
            MemoryStream tmpStream = new MemoryStream(buf);
            Crypto.Decrypt(tmpStream, destStream, alg, 64);

            destStream.Position = 0;
            return true;
        }

        public static bool EncryptAndWritePacket(BinaryWriter destStreamWriter, MemoryStream srcStream, SymmetricAlgorithm alg)
        {
            if (srcStream.Length == 0) return true;

            int iv = GetRandom();
            alg.IV = Encoding.ASCII.GetBytes(iv.ToString("X8"));

            MemoryStream tmpStream = new MemoryStream();
            CryptoStream encStream = new CryptoStream(tmpStream, alg.CreateEncryptor(), CryptoStreamMode.Write);
            encStream.Write(srcStream.GetBuffer(), 0, (int)srcStream.Length);
            encStream.FlushFinalBlock();

            destStreamWriter.Write('P');
            destStreamWriter.Write((Int32)(tmpStream.Length + 8));
            destStreamWriter.Write(iv + 200);

            int offset = 0;
            while (offset < tmpStream.Length)
            {
                int l = (int)tmpStream.Length - offset;
                if (l > 4096) l = 4096;
                destStreamWriter.Write(tmpStream.GetBuffer(), offset, l);
                destStreamWriter.BaseStream.Flush();
                offset += l;
            }

            destStreamWriter.Write(CRCGenerator.CRC32(tmpStream.GetBuffer(), 0, (int)tmpStream.Length, CRCSeed) + 200);

            encStream.Close();
            return true;
        }
    }


}
