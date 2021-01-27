using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

using System.Security.Cryptography;
using System.IO.Compression;

using System.Net;
using System.Net.Sockets;

namespace AEVIDomain
{
    // http://www.freeformatter.com/string-utilities.html !!!!!!!!!! nice utils


    //-----------------------------------------------------------------
    // NamedParameters
    //-----------------------------------------------------------------
    public class NamedParameters
    {
        public char Separator = ';';
        public char GroupNameSeparator = ':';
        public string DefaultGroup = "_";
        Hashtable _table = new Hashtable();
        SimpleList _groups = new SimpleList();

        public NamedParameters() { }
        public NamedParameters(string[] parameters)
        {
            if (parameters != null)
                foreach (string s in parameters) AddParameters(s);
        }

        public void AddParameters(string body)
        {
            // <string>UTA:POSId=123;Products=DIESEL,AD-BLUE(BULK),A-92,A-95,A-98,LPG;OLACurrency=EUR</string>
            if (string.IsNullOrEmpty(body)) return;
            int equIndex = body.IndexOf('=');
            if (equIndex <= 0) return;
            int prefixIndex = body.IndexOf(GroupNameSeparator);
            string groupName = DefaultGroup;
            if (prefixIndex > 0 && prefixIndex < equIndex)
            {
                groupName = body.Substring(0, prefixIndex);
                body = body.Substring(prefixIndex + 1);
            }
            TAG t = _table[groupName] as TAG;
            if (t == null) t = new TAG();
            foreach (string s in body.Split(Separator))
            {
                string[] sa = s.Split('=');
                if (sa.Length == 2) t[sa[0]] = sa[1];
            }
            _table[groupName] = t;
            _groups.Add(groupName);
        }
        public string GetGroups() { return _groups.ToString(); }
        public SimpleList GetGroupList() { return new SimpleList(_groups.ToString()); }

        public string Get(string parameter) { return Get(DefaultGroup, parameter); }
        public string Get(string group, string parameter)
        {
            TAG t = _table[group] as TAG;
            if (t == null) return null;
            return t[parameter];
        }
        public TAG GetTags(string group) { return _table[group] as TAG; }
    }

    //-----------------------------------------------------------------
    // 
    //-----------------------------------------------------------------
    public class ASyncTCPConn : IDisposable
    {
        TcpClient tcp_Client;
        NetworkStream io;
        bool isClosed = false;
        byte[] inBuf = new byte[8 * 1024];

        Queue inQueue = Queue.Synchronized(new Queue());
        Queue outQueue = Queue.Synchronized(new Queue());

        EndPoint remoteURL;
        EndPoint localURL;

        public ASyncTCPConn(TcpClient client)
        {
            tcp_Client = client;
            //client.ReceiveTimeout = 5 * 60 * 1000;
            io = client.GetStream();
            remoteURL = client.Client.RemoteEndPoint;
            localURL = client.Client.LocalEndPoint;
            BeginRead();
        }
        public void Dispose() { Close(); }
        public void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                try
                {
                    tcp_Client.Client.Close();
                    tcp_Client.Close();
                    io = null;
                    tcp_Client = null;
                }
                catch { }
            }
        }
        public bool IsClosed { get { return isClosed; } }
        public bool Connected { get { return !isClosed; } }

        public EndPoint RemoteEndPoint { get { return remoteURL; } }
        public EndPoint LocalEndPoint { get { return localURL; } }

        //---------------------------------------------
        private void BeginRead()
        {
            io.BeginRead(inBuf, 0, inBuf.Length, ReadCallback, null);
        }
        private void ReadCallback(IAsyncResult result)
        {
            //byte[] xxx = result.AsyncState as byte[];
            int read;
            try { read = io.EndRead(result); }
            catch
            {
                //An error has occured when reading
                Close();
                return;
            }
            if (read == 0)
            {
                //The connection has been closed.
                Close();
                return;
            }
            try
            {
                byte[] buf = new byte[read];
                System.Buffer.BlockCopy(inBuf, 0, buf, 0, read);
                //Array.Copy(inBuf, buf, read);
                inQueue.Enqueue(buf);
                BeginRead();
            }
            catch { Close(); }
        }
        //---------------------------------------------
        object locker = new object();
        private void BeginWrite()
        {
            lock (locker)
            {
                if (outQueue.Count > 0)
                {
                    byte[] buf = outQueue.Dequeue() as byte[];
                    io.BeginWrite(buf, 0, buf.Length, WriteCallback, null);
                }
            }
        }
        private void WriteCallback(IAsyncResult result)
        {
            try
            {
                io.EndWrite(result);
                BeginWrite();
            }
            catch { /* Close(); */ }
        }
        //---------------------------------------------
        public bool Available
        {
            get
            {
                try { return inQueue.Count > 0; }
                catch { return false; }
            }
        }
        public byte[] Read()
        {
            if (Available)
                try { return (byte[])inQueue.Dequeue(); }
                catch { }
            return null;
        }
        public void Write(byte[] bytes)
        {
            try
            {
                outQueue.Enqueue(bytes);
                BeginWrite();
            }
            catch { /* Close(); */ }
        }
    }



    //-----------------------------------------------------------------------------------
    /*
    // example from Inet
    public class AsyncTcpClient____
    {
        private IPAddress[] addresses;
        private int port;
        private System.Threading.WaitHandle addressesSet;
        private TcpClient tcpClient;
        private int failedConnectionCount;

        /// <summary>
        /// Construct a new client from a known IP Address
        /// </summary>
        /// <param name="address">The IP Address of the server</param>
        /// <param name="port">The port of the server</param>
        public AsyncTcpClient____(IPAddress address, int port)
            : this(new[] { address }, port)
        {
        }

        /// <summary>
        /// Construct a new client where multiple IP Addresses for
        /// the same client are known.
        /// </summary>
        /// <param name="addresses">The array of known IP Addresses</param>
        /// <param name="port">The port of the server</param>
        public AsyncTcpClient____(IPAddress[] addresses, int port)
            : this(port)
        {
            this.addresses = addresses;
        }

        /// <summary>
        /// Construct a new client where the address or host name of
        /// the server is known.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server</param>
        /// <param name="port">The port of the server</param>
        public AsyncTcpClient____(string hostNameOrAddress, int port)
            : this(port)
        {
            //addressesSet = new AutoResetEvent(false);
            Dns.BeginGetHostAddresses(hostNameOrAddress, GetHostAddressesCallback, null);
        }

        /// <summary>
        /// Private constuctor called by other constuctors
        /// for common operations.
        /// </summary>
        /// <param name="port"></param>
        private AsyncTcpClient____(int port)
        {
            if (port < 0)
                throw new ArgumentException();
            this.port = port;
            this.tcpClient = new TcpClient();
            this.Encoding = Encoding.Default;
        }

        /// <summary>
        /// The endoding used to encode/decode string when sending and receiving.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Attempts to connect to one of the specified IP Addresses
        /// </summary>
        public void Connect()
        {
            if (addressesSet != null)
                //Wait for the addresses value to be set
                addressesSet.WaitOne();
            //Set the failed connection count to 0
            //Interlocked.Exchange(ref failedConnectionCount, 0);
            //Start the async connect operation
            tcpClient.BeginConnect(addresses, port, ConnectCallback, null);
        }

        /// <summary>
        /// Writes a string to the network using the defualt encoding.
        /// </summary>
        /// <param name="data">The string to write</param>
        /// <returns>A WaitHandle that can be used to detect
        /// when the write operation has completed.</returns>
        public void Write(string data)
        {
            byte[] bytes = Encoding.GetBytes(data);
            Write(bytes);
        }

        /// <summary>
        /// Writes an array of bytes to the network.
        /// </summary>
        /// <param name="bytes">The array to write</param>
        /// <returns>A WaitHandle that can be used to detect
        /// when the write operation has completed.</returns>
        public void Write(byte[] bytes)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            //Start async write operation
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
        }

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.EndWrite(result);
        }

        /// <summary>
        /// Callback for Connect operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                tcpClient.EndConnect(result);
            }
            catch
            {
                //Increment the failed connection count in a thread safe way
                //Interlocked.Increment(ref failedConnectionCount);
                if (failedConnectionCount >= addresses.Length)
                {
                    //We have failed to connect to all the IP Addresses
                    //connection has failed overall.
                    return;
                }
            }

            //We are connected successfully.
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            //Now we are connected start asyn read operation.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ReadCallback(IAsyncResult result)
        {
            int read;
            NetworkStream networkStream;
            try
            {
                networkStream = tcpClient.GetStream();
                read = networkStream.EndRead(result);
            }
            catch
            {
                //An error has occured when reading
                return;
            }

            if (read == 0)
            {
                //The connection has been closed.
                return;
            }

            byte[] buffer = result.AsyncState as byte[];
            string data = this.Encoding.GetString(buffer, 0, read);
            //Do something with the data object here.
            //Then start reading from the network again.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        /// <summary>
        /// Callback for Get Host Addresses operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void GetHostAddressesCallback(IAsyncResult result)
        {
            addresses = Dns.EndGetHostAddresses(result);
            //Signal the addresses are now set
            //((AutoResetEvent)addressesSet).Set();
        }
    }
    */

    //-----------------------------------------------------------------
    // CRC Calculations
    //-----------------------------------------------------------------
    public static class CRCGenerator
    {
        // Calculates a 16-bit CRC for size bytes of data in buffer using the CCITT polynomial
        // x**16 + x**12 + x**5 + 1
        //
        // CRC16( "\1\2\3\4", 0x1234 ) = 0xCB7C
        //
        static UInt16[] crc16Table = {
			0x00000, 0x01189, 0x02312, 0x0329B, 0x04624, 0x057AD, 0x06536, 0x074BF, 0x08C48, 0x09DC1, 0x0AF5A, 0x0BED3, 0x0CA6C, 0x0DBE5, 0x0E97E, 0x0F8F7, 0x01081, 0x00108, 0x03393, 0x0221A, 0x056A5, 0x0472C, 0x075B7, 0x0643E, 0x09CC9, 0x08D40, 0x0BFDB, 0x0AE52, 0x0DAED, 0x0CB64, 0x0F9FF, 0x0E876, 0x02102, 0x0308B, 0x00210, 0x01399, 0x06726, 0x076AF, 0x04434, 0x055BD, 0x0AD4A, 0x0BCC3, 0x08E58, 0x09FD1, 0x0EB6E, 0x0FAE7, 0x0C87C, 0x0D9F5, 0x03183, 0x0200A, 0x01291, 0x00318, 0x077A7, 0x0662E, 0x054B5, 0x0453C, 0x0BDCB, 0x0AC42, 0x09ED9, 0x08F50, 0x0FBEF, 0x0EA66, 0x0D8FD, 0x0C974, 0x04204, 0x0538D, 0x06116, 0x0709F, 0x00420, 0x015A9, 0x02732, 0x036BB, 0x0CE4C, 0x0DFC5, 0x0ED5E, 0x0FCD7, 0x08868, 0x099E1, 0x0AB7A, 0x0BAF3, 0x05285, 0x0430C, 0x07197, 0x0601E, 0x014A1, 0x00528, 0x037B3, 0x0263A, 0x0DECD, 0x0CF44, 0x0FDDF, 0x0EC56, 0x098E9, 0x08960, 0x0BBFB, 0x0AA72, 0x06306, 0x0728F, 0x04014, 0x0519D, 0x02522, 0x034AB, 0x00630, 0x017B9, 0x0EF4E, 0x0FEC7, 0x0CC5C, 0x0DDD5, 0x0A96A, 0x0B8E3, 0x08A78, 0x09BF1, 0x07387, 0x0620E, 0x05095, 0x0411C, 0x035A3, 0x0242A, 0x016B1, 0x00738, 0x0FFCF, 0x0EE46, 0x0DCDD, 0x0CD54, 0x0B9EB, 0x0A862, 0x09AF9, 0x08B70, 0x08408, 0x09581, 0x0A71A, 0x0B693, 0x0C22C, 0x0D3A5, 0x0E13E, 0x0F0B7, 0x00840, 0x019C9, 0x02B52, 0x03ADB, 0x04E64, 0x05FED, 0x06D76, 0x07CFF, 0x09489, 0x08500, 0x0B79B, 0x0A612, 0x0D2AD, 0x0C324, 0x0F1BF, 0x0E036, 0x018C1, 0x00948, 0x03BD3, 0x02A5A, 0x05EE5, 0x04F6C, 0x07DF7, 0x06C7E, 0x0A50A, 0x0B483, 0x08618, 0x09791, 0x0E32E, 0x0F2A7, 0x0C03C, 0x0D1B5, 0x02942, 0x038CB, 0x00A50, 0x01BD9, 0x06F66, 0x07EEF, 0x04C74, 0x05DFD, 0x0B58B, 0x0A402, 0x09699, 0x08710, 0x0F3AF, 0x0E226, 0x0D0BD, 0x0C134, 0x039C3, 0x0284A, 0x01AD1, 0x00B58, 0x07FE7, 0x06E6E, 0x05CF5, 0x04D7C, 0x0C60C, 0x0D785, 0x0E51E, 0x0F497, 0x08028, 0x091A1, 0x0A33A, 0x0B2B3, 0x04A44, 0x05BCD, 0x06956, 0x078DF, 0x00C60, 0x01DE9, 0x02F72, 0x03EFB, 0x0D68D, 0x0C704, 0x0F59F, 0x0E416, 0x090A9, 0x08120, 0x0B3BB, 0x0A232, 0x05AC5, 0x04B4C, 0x079D7, 0x0685E, 0x01CE1, 0x00D68, 0x03FF3, 0x02E7A, 0x0E70E, 0x0F687, 0x0C41C, 0x0D595, 0x0A12A, 0x0B0A3, 0x08238, 0x093B1, 0x06B46, 0x07ACF, 0x04854, 0x059DD, 0x02D62, 0x03CEB, 0x00E70, 0x01FF9, 0x0F78F, 0x0E606, 0x0D49D, 0x0C514, 0x0B1AB, 0x0A022, 0x092B9, 0x08330, 0x07BC7, 0x06A4E, 0x058D5, 0x0495C, 0x03DE3, 0x02C6A, 0x01EF1, 0x00F78
		};

        public static UInt16 CRC16(byte b, UInt16 crc)
        {
            return (UInt16)((crc >> 8) ^ crc16Table[(byte)(crc ^ b)]);
        }
        public static UInt16 CRC16(Byte[] data, int startIndex, int length, UInt16 crc)
        {
            for (int i = 0; i < length; i++)
                crc = (UInt16)((crc >> 8) ^ crc16Table[(crc ^ data[startIndex + i]) & 0xff]);
            return crc;
        }

        //------------------------------------------------------
        // CRC32( "\1\2\3\4", 0x12345678 ) = 0x19517C1F

        static UInt32[] crc32Table = { /* CRC polynomial 0xEDB88320 */
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
			0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
			0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
			0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
			0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
			0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
			0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
			0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
			0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
			0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
			0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
			0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
			0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
			0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
			0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
			0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
			0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
			0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
			0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
			0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
			0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
			0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
			0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
			0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
			0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
			0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
			};

        public static UInt32 CRC32(byte b, UInt32 crc)
        {
            return (UInt32)((crc >> 8) ^ crc32Table[(byte)(crc ^ b)]);
        }
        public static UInt32 CRC32(byte[] data, int startIndex, int length, UInt32 crc)
        {
            for (int i = 0; i < length; i++)
                crc = (UInt32)((crc >> 8) ^ crc32Table[(crc ^ data[startIndex + i]) & 0xff]);
            return crc;
        }
        public static UInt32 CRC32(byte[] data, UInt32 crc) { return CRC32(data, 0, data.Length, crc); }

    }

    //-----------------------------------------------------------------
    // Timer
    //-----------------------------------------------------------------
    public class ManualTimer
    {
        int ticks;
        int interval;

        public ManualTimer() : this(0) { }
        public ManualTimer(int milliseconds) { Set(milliseconds); }
        public void Set(int milliseconds) { ticks = (System.Environment.TickCount + (interval = milliseconds)); }
        public void Reset() { Set(interval); }
        public bool Timeout { get { return (System.Environment.TickCount - ticks) >= 0; } }
    }

    //-----------------------------------------------------------------
    // IO
    //-----------------------------------------------------------------
    public static class XIO
    {
        public static uint Read7BitEncodedUInt32(Stream s)
        {
            uint result = 0;
            int i;
            while (true)
            {
                i = s.ReadByte();
                if (i == -1) throw new EndOfStreamException();
                result = result * 128 + (uint)(i & 0x7F);
                if ((i & 0x80) == 0) return result;
            }
        }

        public static void Write7BitEncodedUInt32(Stream s, uint num)
        {
            while (true)
            {
                s.WriteByte((byte)(num % 128));
                num /= 128;
                if (num == 0) break;
            }
        }

        public static int Read7BitEncodedInt32(Stream s) { return (int)Read7BitEncodedUInt32(s); }
        public static void Write7BitEncodedInt32(Stream s, int num) { Write7BitEncodedUInt32(s, (uint)num); }

        public static string buildStrFromByteAnswer(byte[] answer)
        {
            BinaryReader r = new BinaryReader(new MemoryStream(answer));
            return r.ReadString();
        }

    }

    //-----------------------------------------------------------------
    // CByteArray
    //-----------------------------------------------------------------
    public class CByteArray : Stream
    {
        public bool AutoExpand = true;

        byte[] _buffer;
        public byte[] Buffer
        {
            get { return _buffer; }
            //set { _buffer = value; }
        }

        int _dataLength = 0;
        int _curPos = 0;

        //--------------------------------------------------
        public CByteArray()
            : this(256)
        {
        }
        public CByteArray(int bufSize)
        {
            _buffer = new Byte[bufSize];
        }
        public CByteArray(byte[] buf, bool copy)
        {
            if (copy) { _buffer = new byte[buf.Length]; Write(buf); }
            else { _buffer = buf; _dataLength = _buffer.Length; }
            Rewind();
        }
        public CByteArray(byte[] buf, int offset, int length)
        {
            _buffer = new byte[length];
            Write(buf, offset, length);
            Rewind();
        }

        //-----------------------------------------------------------------
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("CByteArray " + _curPos.ToString() + "/"
                + _dataLength.ToString() + "/" + Capacity.ToString() + "\n");
            for (int i = 0; i < _dataLength; i++) sb.Append(Convert.ToString(_buffer[i], 16) + " ");
            sb.Append("\n");
            return sb.ToString();
        }
        public override void Flush() { }
        public override void Close() { Clear(); }
        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return true; } }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int point;
            switch (origin)
            {
                case SeekOrigin.Begin: point = 0; break;
                case SeekOrigin.Current: point = _curPos; break;
                default: point = _dataLength; break;
            }
            return Position = point + offset;
        }
        //--------------------------------------------------
        public int Capacity
        {
            get { return _buffer.Length; }
            // unconditionallly set length, possible cut data
            set
            {
                if (value < 0) value = 0;
                Array.Resize<byte>(ref _buffer, value);
                if (_dataLength > value) _dataLength = value;
                if (_curPos > _dataLength) _curPos = _dataLength;
            }
        }

        public void Compress() { if (_dataLength != _buffer.Length) Array.Resize<byte>(ref _buffer, _dataLength); }
        public int BytesAvailable { get { return _dataLength - _curPos; } }

        void _increaseDataLength(int newLength)
        {
            // only for increase (if takes)
            if (newLength > _dataLength)
            {
                _dataLength = newLength;
                if (newLength > _buffer.Length)
                {
                    // resize
                    if (!AutoExpand) throw new ArgumentException("Cannot expand buffer");
                    Array.Resize<byte>(ref _buffer, newLength + newLength);
                }
            }
        }

        public int DataLength // same as Length but type=int
        {
            get { return _dataLength; }
            set
            {
                if (value < 0) value = 0;
                if (value <= _dataLength)
                {
                    // cut value
                    _dataLength = value;
                    if (_curPos > _dataLength) _curPos = _dataLength;
                }
                else _increaseDataLength(value);
            }

        }
        public override long Length { get { return (long)_dataLength; } }
        public override long Position
        {
            get { return (long)_curPos; }
            set
            {
                if (value < 0) _curPos = 0;
                else if (value <= _dataLength) _curPos = (int)value;
                else _curPos = _dataLength;
            }
        }

        public void Clear() { _dataLength = _curPos = 0; }
        public void Clear0() { _dataLength = _curPos = 0; Array.Clear(Buffer, 0, Buffer.Length); }
        public void Rewind() { _curPos = 0; }
        public void Append() { _curPos = _dataLength; }

        public override void SetLength(long value)
        {
            if (value < 0) value = 0;
            if (value <= _dataLength)
            {
                // cut value
                _dataLength = (int)value;
                if (_curPos > _dataLength) _curPos = _dataLength;
            }
            else _increaseDataLength((int)value);
        }

        //-----------------------------------------------------------------
        public byte this[int i]
        {
            get { if (i >= _dataLength) throw new ArgumentOutOfRangeException(); return _buffer[i]; }
            set { if (i >= _dataLength) throw new ArgumentOutOfRangeException(); _buffer[i] = value; }
        }

        //-----------------------------------------------------------------
        public override void Write(byte[] src, int offset, int nbytes)
        {
            _increaseDataLength(_curPos + nbytes);
            System.Buffer.BlockCopy(src, offset, _buffer, _curPos, nbytes);
            _curPos += nbytes;
        }
        public override void WriteByte(byte b)
        {
            _increaseDataLength(_curPos + 1);
            _buffer[_curPos++] = b;
        }

        public void Write(UInt16 w)
        {
            _increaseDataLength(_curPos + 2);
            _buffer[_curPos++] = (byte)w;
            _buffer[_curPos++] = (byte)(w >> 8);
        }
        public void Write(Int16 w)
        {
            Write((UInt16)w);
        }
        public void Write(uint d)
        {
            _increaseDataLength(_curPos + 4);
            _buffer[_curPos++] = (byte)d;
            _buffer[_curPos++] = (byte)(d >> 8);
            _buffer[_curPos++] = (byte)(d >> 16);
            _buffer[_curPos++] = (byte)(d >> 24);
        }
        public void Write(int w)
        {
            Write((uint)w);
        }
        public void Write(byte[] src)
        {
            Write(src, 0, src.Length);
        }

        public void Write7BitEncoded(uint value)
        {
            while (true)
            {
                uint remainder = value % 128;
                value /= 128;
                if (value == 0) { WriteByte((byte)(remainder)); return; }
                WriteByte((byte)(remainder | 0x80));
            }
        }
        public void Write7BitEncoded(int value)
        {
            Write7BitEncoded((uint)value);
        }

        /// <summary>
        /// Encode and write string with length preceeding
        /// </summary>
        /// <param name="str"></param>
        /// <param name="codePage"></param>
        public void Write(string str, int codePage)
        {
            if (str == null) WriteByte(0);
            else
            {
                Write7BitEncoded(str.Length);
                Write(Encoding.GetEncoding(codePage).GetBytes(str));
            }
        }
        public void Write(string str)
        {
            Write(str, 0);
        }
        /// <summary>
        /// ncode and write string of fixed size
        /// </summary>
        /// <param name="size"></param>
        /// <param name="str"></param>
        /// <param name="codePage"></param>
        public void WriteFixedString(int size, string str, int codePage)
        {
            if (size < 0) size = str.Length;
            if (str.Length != size)
            {
                if (str.Length < size) str = str.PadRight(size, '\0');
                else str = str.Substring(0, size);
            }
            Write(Encoding.GetEncoding(codePage).GetBytes(str));

        }
        public void WriteFixedString(int size, string str)
        {
            WriteFixedString(size, str, 0);
        }
        public void WriteFixedString(string str, int codePage)
        {
            WriteFixedString(-1, str, codePage);
        }
        public void WriteFixedString(string str)
        {
            WriteFixedString(-1, str, 0);
        }

        //-----------------------------------------------------------------
        public static CByteArray operator +(CByteArray ba, byte b) { ba.WriteByte(b); return ba; }
        public static CByteArray operator +(CByteArray ba, int b) { ba.Write(b); return ba; }
        public static CByteArray operator +(CByteArray ba, uint b) { ba.Write(b); return ba; }
        public static CByteArray operator +(CByteArray ba, Int16 b) { ba.Write(b); return ba; }
        public static CByteArray operator +(CByteArray ba, UInt16 b) { ba.Write(b); return ba; }
        public static CByteArray operator +(CByteArray ba, string b) { ba.Write(b); return ba; }

        //-----------------------------------------------------------------
        public override int Read(byte[] dest, int offset, int nbytes)
        {
            if (nbytes > BytesAvailable) nbytes = BytesAvailable;
            System.Buffer.BlockCopy(_buffer, _curPos, dest, offset, nbytes);
            _curPos += nbytes;
            return nbytes;
        }
        public override int ReadByte()
        {
            return _curPos >= _dataLength ? -1 : _buffer[_curPos++];
        }

        public UInt16 ReadUInt16()
        {
            if (BytesAvailable < 2) throw new EndOfStreamException();
            _curPos += 2;
            return BitConverter.ToUInt16(_buffer, _curPos - 2);
        }
        public Int16 ReadInt16()
        {
            return (Int16)ReadUInt16();
        }
        public uint ReadUInt32()
        {
            if (BytesAvailable < 4) throw new EndOfStreamException();
            _curPos += 4;
            return BitConverter.ToUInt32(_buffer, _curPos - 4);
        }
        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public uint Read7BitEncodedUInt32()
        {
            uint result = 0;
            int ch;
            uint k = 1;
            while (true)
            {
                ch = ReadByte();
                if (ch == -1) throw new EndOfStreamException();
                result += k * (uint)(ch & 0x7F);
                k *= 128;
                if ((ch & 0x80) == 0) return result;
            }
        }
        public int Read7BitEncodedInt32()
        {
            return (int)Read7BitEncodedUInt32();
        }

        /// <summary>
        /// Read encoded string of fixed length
        /// </summary>
        /// <param name="length"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public string ReadFixedString(int length, int codePage)
        {
            if (BytesAvailable < length) throw new EndOfStreamException();
            _curPos += length;
            return Encoding.GetEncoding(codePage).GetString(_buffer, _curPos - length, length);
        }
        public string ReadFixedString(int length)
        {
            return ReadFixedString(length, 0);
        }
        /// <summary>
        /// Read encoded string with length preceeding
        /// </summary>
        /// <param name="length"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public string ReadString(int codePage)
        {
            int length = Read7BitEncodedInt32();
            return length == 0 ? string.Empty : ReadFixedString(length, codePage);
        }
        public string ReadString()
        {
            return ReadString(0);
        }

        //-----------------------------------------------------------------
        public bool WriteTo(CByteArray destArray, int nbytes) { return Copy(this, destArray, nbytes); }
        public bool ReadFrom(CByteArray srcArray, int nbytes) { return Copy(srcArray, this, nbytes); }
        public static bool Copy(CByteArray srcArray, CByteArray destArray, int nbytes)
        {
            if (srcArray.BytesAvailable < nbytes) return false;
            destArray.Write(srcArray._buffer, srcArray._curPos, nbytes);
            srcArray._curPos += nbytes;
            return true;
        }

        public void ShiftRight(int nbytes)
        {
            // does not change Position
            int nbcopy = BytesAvailable;
            _increaseDataLength(_dataLength + nbytes);
            System.Buffer.BlockCopy(_buffer, _curPos, _buffer, _curPos + nbytes, nbcopy);
        }
        public void Insert(byte[] src, int offset, int nbytes)
        {
            ShiftRight(nbytes);
            Write(src, offset, nbytes);
        }
        public void Delete(int nbytes)
        {
            if (nbytes > BytesAvailable) nbytes = BytesAvailable;
            System.Buffer.BlockCopy(_buffer, _curPos + nbytes, _buffer, _curPos, BytesAvailable - nbytes);
            _dataLength -= nbytes;
        }

        //-----------------------------------------------------------------
        // Implement Collection (IEnumerable, IEnumerator) //: IEnumerable<byte>, IEnumerator<byte>
        //-----------------------------------------------------------------
        /*
                int _enumPos = -1;

                // IEnumerable<byte> : IEnumerable
                public IEnumerator<byte> GetEnumerator() { return this; }
                IEnumerator IEnumerable.GetEnumerator() { return this; } // base interface

                // IEnumerator<byte> : IEnumerator, IDisposable
                object IEnumerator.Current { get { return (object)this[_enumPos]; } } // base interface
                public byte Current { get { return this[_enumPos]; } }
                public void Reset() { _enumPos = -1; }
                public bool MoveNext() { return ++_enumPos < dataLength; }

         */
    }

    //-----------------------------------------------------------------
    // Extended MemoryStream
    //-----------------------------------------------------------------
    public class MemoryStreamEx : MemoryStream
    {
        public MemoryStreamEx() : base() { }
        public MemoryStreamEx(int startCapacity) : base(startCapacity) { }
        public MemoryStreamEx(byte[] array) : base(array) { }
        public MemoryStreamEx(byte[] array, int offset, int length) : base(array, offset, length) { }

        //-----------------------------------------------------------------
        public void Clear() { Position = 0; SetLength(0); }
        public void Rewind() { Position = 0; }
        public void Compress() { Capacity = (int)Length; }
        public int BytesAvailable { get { return Position < Length ? (int)(Length - Position) : 0; } }
        public int DataLength { get { return (int)Length; } set { SetLength(value); } }
        public int Pos { get { return (int)Position; } set { Position = value; } }

        //-----------------------------------------------------------------
        public void Write(int num, int bytes) { while ((bytes--) > 0) { WriteByte((byte)num); num >>= 8; } }
        public void Write(UInt16 num) { Write((int)num, 2); }
        public void Write(Int16 num) { Write((int)num, 2); }
        public void Write(uint num) { Write((int)num, 4); }
        public void Write(int num) { Write(num, 4); }

        public void Write(byte[] src) { Write(src, 0, src.Length); }

        public void Write7BitEncoded(uint value)
        {
            while (true)
            {
                uint remainder = value % 128;
                value /= 128;
                if (value == 0) { WriteByte((byte)(remainder)); return; }
                WriteByte((byte)(remainder | 0x80));
            }
        }
        public void Write7BitEncoded(int value) { Write7BitEncoded((uint)value); }

        // Encode and write string with 7bit-enc-length preceeding
        public void Write(string str, int codePage)
        {
            if (str == null) WriteByte(0);
            else
            {
                Write7BitEncoded(str.Length);
                Write(Encoding.GetEncoding(codePage).GetBytes(str));
            }
        }
        public void Write(string str) { Write(str, 0); }

        public void WriteFixedString(int size, string str, int codePage)
        {
            if (size < 0) size = str.Length;
            if (str.Length != size)
            {
                if (str.Length < size) str = str.PadRight(size, '\0');
                else str = str.Substring(0, size);
            }
            Write(Encoding.GetEncoding(codePage).GetBytes(str));

        }
        public void WriteFixedString(int size, string str) { WriteFixedString(size, str, 0); }
        public void WriteFixedString(string str, int codePage) { WriteFixedString(-1, str, codePage); }
        public void WriteFixedString(string str) { WriteFixedString(-1, str, 0); }

        public void WriteZString(string str)
        {
            WriteFixedString(str);
            WriteByte(0);
        }

        public void WriteLine(string str) { WriteFixedString(str); WriteFixedString("\r\n"); }
        public void ShiftRight(int nbytes)
        {
            // does not change Position
            SetLength(Length + nbytes);
            System.Buffer.BlockCopy(GetBuffer(), (int)Position, GetBuffer(), (int)Position + nbytes, BytesAvailable);
        }
        public void Insert(byte[] src, int offset, int nbytes)
        {
            ShiftRight(nbytes);
            Write(src, offset, nbytes);
        }
        public void Delete(int nbytes)
        {
            if (nbytes > BytesAvailable) nbytes = BytesAvailable;
            System.Buffer.BlockCopy(GetBuffer(), (int)Position + nbytes, GetBuffer(), (int)Position, BytesAvailable - nbytes);
            SetLength(Length - nbytes);
        }

        //-----------------------------------------------------------------
        public UInt16 ReadUInt16()
        {
            if (BytesAvailable < 2) throw new EndOfStreamException();
            Pos += 2;
            return BitConverter.ToUInt16(GetBuffer(), Pos - 2);
        }
        public Int16 ReadInt16() { return (Int16)ReadUInt16(); }
        public uint ReadUInt32()
        {
            if (BytesAvailable < 4) throw new EndOfStreamException();
            Pos += 4;
            return BitConverter.ToUInt32(GetBuffer(), Pos - 4);
        }
        public int ReadInt32() { return (int)ReadUInt32(); }

        public uint Read7BitEncodedUInt32()
        {
            uint result = 0;
            int ch;
            uint k = 1;
            while (true)
            {
                ch = ReadByte();
                if (ch == -1) throw new EndOfStreamException();
                result += k * (uint)(ch & 0x7F);
                k *= 128;
                if ((ch & 0x80) == 0) return result;
            }
        }
        public int Read7BitEncodedInt32() { return (int)Read7BitEncodedUInt32(); }
        public string ReadFixedString(int length, int codePage)
        {
            if (BytesAvailable < length) throw new EndOfStreamException();
            Pos += length;
            return Encoding.GetEncoding(codePage).GetString(GetBuffer(), Pos - length, length);
        }
        public string ReadFixedString(int length) { return ReadFixedString(length, 0); }
        public string ReadString(int codePage)
        {
            int length = Read7BitEncodedInt32();
            return length == 0 ? string.Empty : ReadFixedString(length, codePage);
        }
        public string ReadString() { return ReadString(0); }

        public string ReadZString()
        {
            int b;
            StringBuilder bld = new StringBuilder();
        Loop:
            b = ReadByte();
            if (b <= 0) return bld.ToString();
            bld.Append((char)b);
            goto Loop;
        }
        public string ReadLine()
        {
            int b;
            StringBuilder bld = new StringBuilder();
        Loop:
            b = ReadByte();
            if (b < 0)
            {
                if (bld.Length == 0) return null;
                return bld.ToString();
            }
            if (b == '\r') goto Loop;
            if (b == '\n') return bld.ToString();
            bld.Append((char)b);
            goto Loop;
        }
    }

    //-----------------------------------------------------------------
    // resizeble version (non-fixed Capacity)
    public class MemoryNFStream : MemoryStreamEx
    {
        public MemoryNFStream(byte[] array)
            : base(array.Length)
        {
            Write(array, 0, array.Length);
            Position = 0;
        }
        public MemoryNFStream(byte[] array, int offset, int length)
            : base(length)
        {
            Write(array, offset, length);
            Position = 0;
        }
    }
    //-----------------------------------------------------------------
    // Cyclic Buffer
    //-----------------------------------------------------------------
    public class CyclicBuffer
    {
        private byte[] buffer;
        private int readPos;
        private int writePos;
        private int dataLength;
        private int size;

        public CyclicBuffer(int size)
        {
            buffer = new byte[this.size = size];
        }

        public int DataLength { get { return dataLength; } }
        public bool DataAvailable { get { return dataLength > 0; } }
        public int Size { get { return size; } }

        public int Read(byte[] buf, int offset, int length)
        {
            if (length > dataLength) length = dataLength;
            if (length <= 0) return 0;
            int restPart = size - readPos; // tile size
            if (length <= restPart)
            {
                System.Buffer.BlockCopy(buffer, readPos, buf, offset, length);
                readPos += length;
            }
            else
            {
                System.Buffer.BlockCopy(buffer, readPos, buf, offset, restPart);
                System.Buffer.BlockCopy(buffer, 0, buf, offset + restPart, readPos = length - restPart);
            }
            if (readPos >= size) readPos = 0;
            dataLength -= length;
            return length;
        }
        public int Write(byte[] buf, int offset, int length)
        {
            if (length > (size - dataLength)) length = (size - dataLength);
            if (length <= 0) return 0;
            int restPart = size - writePos;
            if (length <= restPart)
            {
                System.Buffer.BlockCopy(buf, offset, buffer, writePos, length);
                writePos += length;
            }
            else
            {
                System.Buffer.BlockCopy(buf, offset, buffer, writePos, restPart);
                System.Buffer.BlockCopy(buf, offset + restPart, buffer, 0, writePos = length - restPart);
            }
            if (writePos >= size) readPos = 0;
            dataLength += length;
            return length;
        }
    }

    //-----------------------------------------------------------------
    // XMLFileIO
    //-----------------------------------------------------------------
    public static class XMLFileIO
    {
        public static bool Save(string fileName, object obj)
        {
            try
            {
                TextWriter tw = new StreamWriter(fileName);
                //XmlSerializer xs = new XmlSerializer(typeof(Config));
                Type t = obj.GetType();
                XmlSerializer xs = new XmlSerializer(obj.GetType());
                xs.Serialize(tw, obj);
                tw.Close();
                return true;
            }
            catch { return false; }
            //			WebBrowser b = new WebBrowser();
            //			b.Navigate(AppDomain.CurrentDomain.BaseDirectory + "//" + name);
        }

        public static object Load(string fileName, Type type)
        {
            TextReader tr = null;
            try
            {
                tr = new StreamReader(fileName);
                XmlSerializer xs = new XmlSerializer(type);
                object obj = (object)xs.Deserialize(tr);
                tr.Close();
                return obj;
            }
            catch (Exception e) { if (tr != null) tr.Close(); return e.Message; }
        }
    }

    //-----------------------------------------------------------------
    // Random
    //-----------------------------------------------------------------
    public static class UTRandom
    {
        static Random _rand = new Random((int)(System.Environment.TickCount ^ 0xD56A3B44));
        public static int Generate() { return _rand.Next(); }
        public static byte[] GenerateRandom(int length)
        {
            byte[] b = new byte[length];
            _rand.NextBytes(b);
            return b;
        }
    }
    // ?? new Random().NextBytes(dataArray);


    //-----------------------------------------------------------------
    // Parser
    //-----------------------------------------------------------------
    public static class Parser
    {
        public static double ParseDouble(string num)
        {
            double ret;
            if (double.TryParse(num, out ret) == false)
                if (double.TryParse(num.Replace('.', ','), out ret) == false) throw new FormatException();
            return ret;
        }
        public static decimal ParseDecimal(string num)
        {
            decimal ret;
            if (decimal.TryParse(num, out ret) == false)
                if (decimal.TryParse(num.Replace('.', ','), out ret) == false) throw new FormatException();
            return ret;
        }

        public static double Round(double num, int digits)
        {
            string format = "{0:F" + digits.ToString() + "}";
            return double.Parse(string.Format(format, num + 0.00001));
        }
        public static string Combine(char separator, params object[] args)
        {
            //if( args.Length == 1 ) return args[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i != 0) sb.Append(separator);
                if (args[i] != null) sb.Append(args[i].ToString());
            }
            return sb.ToString();
        }
        public static string Combine(params object[] args) { return Combine(';', args); }

        public static string AppendToSList(string list, char separator, string arg)
        {
            if (arg == null) arg = "";
            if (string.IsNullOrEmpty(list)) list = arg;
            else
            {
                list += separator;
                list += arg;
            }
            return list;
        }

        public static string GetNotNull(string str) { return str == null ? "" : str; }

        public static Hashtable SplitStringAs(char separator, string source, string pattern)
        {
            Hashtable result = new Hashtable();
            if (source != null && !string.IsNullOrEmpty(pattern))
            {
                string[] parray = pattern.Split(separator);
                string[] sarray = source.Split(separator);
                int len = parray.Length < sarray.Length ? parray.Length : sarray.Length;
                for (int i = 0; i < len; i++) result[parray[i]] = sarray[i];
            }
            return result;
        }

        public static string ToStr(object num)
        {
            string s = num.ToString().Replace(',', '.');
            if (s.Contains(".")) s = s.TrimEnd('0');
            if (s.EndsWith(".")) s = s.Replace(".", "");
            return s;
        }

        public static decimal ParseFirstDec(string num)
        {
            if (string.IsNullOrEmpty(num)) return 0;
            if (num.Length == 1) return ParseDecimal(num);
            int sign = 1;
            if (num[0] == '-') { sign = -1; num = num.Substring(1); }
            decimal result = ParseDecimal(num.Substring(1)) / (int)Math.Pow(10, num[0] - '0');
            return result * sign;
        }

        static string QP(decimal value)
        {
            if (value == 0) return "00";
            string sign = "";
            if (value < 0) { value *= -1; sign = "-"; }
            int i = 4;
            string s = ((long)(decimal.Round(value * 10000, 0))).ToString();
            if (s.EndsWith("0")) { s = s.Substring(0, s.Length - 1); i--; }
            if (s.EndsWith("0")) { s = s.Substring(0, s.Length - 1); i--; }
            if (s.EndsWith("0")) { s = s.Substring(0, s.Length - 1); i--; }
            if (s.EndsWith("0")) { s = s.Substring(0, s.Length - 1); i--; }
            return sign + i.ToString() + s;
        }
        static string QP(string value) { return QP(Parser.ParseDecimal(value)); }

    }

    //-----------------------------------------------------------------
    // Ini file support
    //-----------------------------------------------------------------
    public class IniSettings
    {
        Hashtable table;

        public IniSettings()
        {
            table = new Hashtable();
        }

        public object Get(string group, string key)
        {
            //			Hashtable gr = (Hashtable)table[group];
            //			return gr == null ? null : gr[key];
            return table[group + "^" + key];
        }

        public void Set(string group, string key, object value)
        {
            table[group + "^" + key] = value;
        }

        public object this[string key]
        {
            get { return Get("", (string)key); }
            set { Set("", key, value); }
        }

        public int Load(string fullName)
        {
            string[] fileText;
            try { fileText = File.ReadAllLines(fullName); }
            catch { return -1; }

            string group = "";
            for (int i = 0; i < fileText.Length; i++)
            {
                string s = fileText[i].Trim();
                if (s.Length == 0 || s.StartsWith(";")) continue;
                if (s.StartsWith("[") && s.EndsWith("]"))
                {
                    group = s.Substring(1, s.Length - 2).Trim(); // select text between [...]
                }
                else
                {
                    string[] values = s.Split('=');
                    if (values.Length < 2) return i + 1;
                    values[0] = values[0].Trim();
                    values[1] = values[1].Trim();
                    if (values[0] == "") return i + 1; // не допускаем конструкций типа '= 345'
                    table[group + "." + values[0]] = values[1];
                }
            }
            return 0;
        }
        /*
                public static void Save(string fullName, Hashtable tbl)
                {
                    ICollection keys = tbl.Keys;
                    string[] array = new string[keys.Count];
                    int i = 0;
                    foreach (object o in keys)
                    {
                        string s = (string)o;
                        array[i] = s + " = " + tbl[s];
                        i++;
                    }
                    File.WriteAllLines(fullName, array);
                }
         */
    }

    //-----------------------------------------------------------------
    // Encryption
    //-----------------------------------------------------------------
    public static class Crypto
    {
        public static void Encrypt(Stream ins, Stream outs, SymmetricAlgorithm alg, int tmpBufLen)
        {
            // TODO: wrong implementation, use OUTS for file only
            byte[] buf = new byte[tmpBufLen]; //This is intermediate storage for the encryption.
            CryptoStream encStream = new CryptoStream(outs, alg.CreateEncryptor(), CryptoStreamMode.Write);
            int len;
            while ((len = ins.Read(buf, 0, tmpBufLen)) > 0) encStream.Write(buf, 0, len);
            encStream.Close();
        }

        public static void Decrypt(Stream ins, Stream outs, SymmetricAlgorithm alg, int tmpBufLen)
        {
            byte[] buf = new byte[tmpBufLen]; //This is intermediate storage for the encryption.
            CryptoStream encStream = new CryptoStream(ins, alg.CreateDecryptor(), CryptoStreamMode.Read);
            int len;
            while ((len = encStream.Read(buf, 0, tmpBufLen)) > 0) outs.Write(buf, 0, len);
            encStream.Close();
        }

        public static void CryptFile(bool encrypt, String inName, String outName, SymmetricAlgorithm alg)
        {
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            if (encrypt) Encrypt(fin, fout, alg, 2048);
            else Decrypt(fin, fout, alg, 2048);
            fin.Close();
            fout.Close();
        }
    }

    //-----------------------------------------------------------------
    // Compression
    //-----------------------------------------------------------------
    public static class Compression
    {
        public static void Compress(Stream ins, Stream outs, int tmpBufLen)
        {
            byte[] buf = new byte[tmpBufLen]; //This is intermediate storage for the encryption.
            GZipStream encStream = new GZipStream(outs, CompressionMode.Compress);
            int len;
            while ((len = ins.Read(buf, 0, tmpBufLen)) > 0) encStream.Write(buf, 0, len);
            encStream.Close();
        }

        public static void Decompress(Stream ins, Stream outs, int tmpBufLen)
        {
            byte[] buf = new byte[tmpBufLen]; //This is intermediate storage for the encryption.
            GZipStream encStream = new GZipStream(ins, CompressionMode.Decompress);
            int len;
            while ((len = encStream.Read(buf, 0, tmpBufLen)) > 0) outs.Write(buf, 0, len);
            encStream.Close();
        }

        public static void CompressFile(bool compress, String inName, String outName)
        {
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            if (compress) Compress(fin, fout, 2048);
            else Decompress(fin, fout, 2048);
            fin.Close();
            fout.Close();
        }

        //-----------------------------------------------------------------
        public static byte[] Compress(byte[] buffer, int offset, int length)
        {
            //MemoryStream ms = new MemoryStream(buf, offset,length);
            MemoryStream outs = new MemoryStream();
            GZipStream encStream = new GZipStream(outs, CompressionMode.Compress, true);
            encStream.Write(buffer, offset, length);
            encStream.Close();

            int pos = (int)outs.Position;
            byte[] buf = new byte[pos];
            Buffer.BlockCopy(outs.GetBuffer(), 0, buf, 0, pos);
            outs.Close();

            return buf;
        }
    }

    //-----------------------------------------------------------------
    // Log
    //-----------------------------------------------------------------
    public class XLog
    {
        //object _locker = new object();
        Queue _outQueue = Queue.Synchronized(new Queue());

        public string DirName = ".";
        public string NamePrefix = "";
        public string NameSuffix = ".log";

        public bool CopyToConsole = true;
        public bool AutoAddDate = false;
        public bool AutoAddTime = true;

        public int Type;

        public virtual string GetName()
        {
            return NamePrefix + DateTime.Now.ToString("yyyy-MM-dd") + NameSuffix;
        }


        public XLog()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_thread);
        }
        void _thread(object o)
        {
            while (true)
            {
                try { _outputMsg((string)_outQueue.Dequeue()); }
                catch { }
                System.Threading.Thread.Sleep(200);
            }
        }

        void _outputMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            if (!Directory.Exists(DirName)) Directory.CreateDirectory(DirName);

            try { System.IO.File.AppendAllText(Path.Combine(DirName, GetName()), msg, Encoding.UTF8); }
            catch { }
        }

        public string Log(string msg) { return Log(false, msg); }

        public string LogLine(string msg) { return Log(true, msg); }
        public string LogLine(string format, object obj) { return Log(true, string.Format(format, obj)); }
        public string LogLine(string format, object obj1, object obj2) { return Log(true, string.Format(format, obj1, obj2)); }
        public string LogLine(string format, object obj1, object obj2, object obj3) { return Log(true, string.Format(format, obj1, obj2, obj3)); }
        public string LogLine(string format, params object[] obj) { return Log(true, string.Format(format, obj)); }

        public string Log(bool newLine, string msg)
        {
            if (AutoAddTime) msg = DateTime.Now.ToString("HH:mm:ss ") + msg;
            if (AutoAddDate) msg = DateTime.Now.ToString("yyyy-MM-dd ") + msg;
            if (newLine) msg += System.Environment.NewLine;
            _outQueue.Enqueue(msg);
            if (CopyToConsole) Console.Write(msg);
            return msg;
        }
    }

    //-----------------------------------------------------------------
    // SimpleList
    //-----------------------------------------------------------------
    public class SimpleList
    {
        // cannot contain ""
        string Source;
        string separator;

        public SimpleList(string source, string separator)
        {
            Source = source == null ? "" : source;
            this.separator = separator;
        }
        public SimpleList(string source) : this(source, ",") { }
        public SimpleList() : this(null, ",") { }
        public SimpleList(string[] args) : this() { Add(args); }

        public void Clear() { Source = ""; }
        public bool IsEmpty { get { return Source == ""; } }
        public override string ToString() { return Source; }

        public bool Contains(string value)
        {
            if (string.IsNullOrEmpty(value) || Source == "") return false;
            if (Source == value) return true;
            if (Source.Contains(separator + value + separator)) return true;
            if (Source.StartsWith(value + separator)) return true;
            if (Source.EndsWith(separator + value)) return true;
            return false;
        }
        public string[] Split() { return Source == "" ? new string[0] : Source.Split(separator[0]); }
        public string Add(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains(separator)) return Add(value.Split(separator[0]));
                if (!Contains(value))
                    if (Source == "") Source = value;
                    else { Source += separator; Source += value; }
            }
            return Source;
        }
        public string AddDuplicated(string value)
        {
            if (Source == "") Source = value;
            else { Source += separator; Source += value; }
            return Source;
        }

        public string Add(string[] args)
        {
            for (int i = 0; i < args.Length; i++) Add(args[i]);
            return Source;
        }
        public int Counter { get { return Split().Length; } }

        public string this[int i]
        {
            get { return Split()[i]; }
            set { if (string.IsNullOrEmpty(value)) Remove(i); else { string[] a = Split(); a[i] = value; Clear(); Add(a); } }
        }
        public int GetIndexOf(string value)
        {
            if (string.IsNullOrEmpty(value)) return -1;
            string[] a = Split();
            for (int i = 0; i < a.Length; i++) if (a[i] == value) return i;
            return -1;
        }
        public string Remove(int element)
        {
            string[] a = Split();
            Clear();
            for (int i = 0; i < a.Length; i++) if (i != element) Add(a[i]);
            return Source;
        }
        public string Remove(string element)
        {
            string[] a = Split();
            Clear();
            for (int i = 0; i < a.Length; i++) if (a[i] != element) Add(a[i]);
            return Source;
        }
    }

    //-----------------------------------------------------------------
    // Convert
    //-----------------------------------------------------------------
    public static class StringUtils
    {
        public static string Reverse(string s)
        {
            StringBuilder bld = new StringBuilder();
            int i = s.Length;
            while (i-- > 0) bld.Append(s[i]);
            return bld.ToString();
        }

        public static string BASE_02 = "01";
        public static string BASE_10 = "0123456789";
        public static string BASE_T2 = "0123456789=";
        public static string BASE_16 = "0123456789ABCDEF";
        public static string BASE_62 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public static string BASE_64 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";
        public static string BASE_74 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!*+,-./:=?^_";
        public static string BASE_96 = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        public static string BASE_128 = "\x0\x1\x2\x3\x4\x5\x6\x7\x8\x9\xA\xB\xC\xD\xE\xF\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public static string ChangeBase(string str, string srcBase, string destBase)
        {
            try
            {
                if (string.IsNullOrEmpty(str)) return str;
                int len1 = srcBase.Length;
                int len2 = destBase.Length;
                string result = "";
                int index, div;

            Loop:
                int num = 0;
                string im = "";
                int len = str.Length;
                if (len == 0) return result;
                for (int i = 0; i < len; i++)
                {
                    num *= len1;
                    index = srcBase.IndexOf(str[i]);
                    if (index < 0) return null;
                    num += index;
                    div = num / len2;
                    if (div == 0 && im.Length == 0) { }
                    else im += srcBase[div].ToString();
                    num %= len2;
                }
                result = destBase[num].ToString() + result;
                str = im;
                goto Loop;
            }
            catch { }
            return null;
        }

        public static string GetDefaultValue(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
                if (args[i] != null)
                {
                    string s = args[i].ToString();
                    if (!string.IsNullOrEmpty(s)) return s;
                }
            return null;
        }

        public static string Bin2Hex(byte[] buf) { return Bin2Hex(buf, 0, buf.Length); }
        public static string Bin2Hex(byte[] buf, int offset, int len)
        {
            StringBuilder bld = new StringBuilder();
            for (int i = offset; i < len; i++) bld.AppendFormat("{0:X2}", (int)buf[i]);
            return bld.ToString();
        }
        public static byte[] Hex2Bin(string s)
        {
            s = s.ToUpper();
            if (s.Length % 2 == 1) s += "0";
            byte[] b = new byte[s.Length / 2];
            for (int i = 0; i < b.Length; i++) b[i] = (byte)("0123456789ABCDEF".IndexOf(s[i * 2]) * 16 + "0123456789ABCDEF".IndexOf(s[i * 2 + 1]));
            return b;
        }
        public static string ToAscii(byte[] buf, int offset, int len)
        {
            StringBuilder bld = new StringBuilder();
            for (int i = offset; i < len; i++)
            {
                if (buf[i] < 0x20 || buf[i] > '~') bld.AppendFormat("[{0:X}]", (int)buf[i]);
                else bld.Append((char)buf[i]);
            }
            return bld.ToString();
        }
        public static int Count(string s, char c)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int n = 0;
            for (int i = 0; i < s.Length; i++) if (s[i] == c) n++;
            return n;
        }
        public static bool ListContains(string list, string value, string separator)
        {
            if (list == null || value == null) return false;
            if (list == value) return true;
            if (list.Contains(separator + value + separator)) return true;
            if (list.StartsWith(value + separator)) return true;
            if (list.EndsWith(separator + value)) return true;
            return false;
        }
        public static bool ValueMatched(string list, string value, string separator)
        {
            if (string.IsNullOrEmpty(list)) return true; // All values
            if (list == "*") return true;
            return ListContains(list, value, separator);
        }

        public static string Combine(char separator, params object[] args)
        {
            //if( args.Length == 1 ) return args[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i != 0) sb.Append(separator);
                if (args[i] != null) sb.Append(args[i].ToString());
            }
            return sb.ToString();
        }
        public static string Combine(params object[] args) { return Combine(';', args); }

        public static string AppendToSList(string list, char separator, string arg)
        {
            if (arg == null) arg = "";
            if (string.IsNullOrEmpty(list)) list = arg;
            else
            {
                list += separator;
                list += arg;
            }
            return list;
        }

        public static string GetNotNull(string str) { return str == null ? "" : str; }

        public static Hashtable SplitStringAs(char separator, string source, string pattern)
        {
            Hashtable result = new Hashtable();
            if (source != null && !string.IsNullOrEmpty(pattern))
            {
                string[] parray = pattern.Split(separator);
                string[] sarray = source.Split(separator);
                int len = parray.Length < sarray.Length ? parray.Length : sarray.Length;
                for (int i = 0; i < len; i++) result[parray[i]] = sarray[i];
            }
            return result;
        }

        public static string ToStr(object num)
        {
            string s = num.ToString().Replace(',', '.');
            if (s.Contains(".")) s = s.TrimEnd('0');
            if (s.EndsWith(".")) s = s.Replace(".", "");
            return s;
        }

        //---------------------------------------------------------
        public static string BuildTAG(string tagName, string value)
        {
            if (value == null) return "";
            return string.Format("<{0}={1}>", tagName, value);
        }
        public static void DeleteTAG(ref string source, string tagName)
        {
            if (!string.IsNullOrEmpty(source)) SetTAGValue(ref source, tagName, null);
        }
        public static string GetTAGValue(string source, string tagName)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string tagId = "<" + tagName + "=";
            if (!source.Contains(tagId)) return null;
            string[] a = source.Split('>');
            foreach (string s in a) if (s.StartsWith(tagId)) return s.Substring(tagId.Length);
            return null;
        }
        public static void SetTAGValue(ref string source, string tagName, string newValue)
        {
            if (source == null) source = "";
            string oldtag = BuildTAG(tagName, GetTAGValue(source, tagName));
            string newtag = BuildTAG(tagName, newValue);
            if (oldtag != "" && source.Contains(oldtag)) source = source.Replace(oldtag, newtag);
            else source += newtag;
        }
        public static bool TAGExists(string source, string tagName)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return source.Contains("<" + tagName + "=");
        }
    }

    public static class zTAG
    {
        public static string Build(string tagName, string value)
        {
            if (value == null) return "";
            return string.Format("<{0}={1}>", tagName, value);
        }
        public static void Delete(ref string source, string tagName)
        {
            if (!string.IsNullOrEmpty(source)) SetValue(ref source, tagName, null);
        }
        public static string GetValue(string source, string tagName)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string tagId = "<" + tagName + "=";
            if (!source.Contains(tagId)) return null;
            string[] a = source.Split('>');
            foreach (string s in a) if (s.StartsWith(tagId)) return s.Substring(tagId.Length);
            return null;
        }
        public static void SetValue(ref string source, string tagName, string newValue)
        {
            if (source == null) source = "";
            string oldtag = Build(tagName, GetValue(source, tagName));
            string newtag = Build(tagName, newValue);
            if (oldtag != "" && source.Contains(oldtag)) source = source.Replace(oldtag, newtag);
            else source += newtag;
        }
        public static bool Exists(string source, string tagName)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return source.Contains("<" + tagName + "=");
        }
        public static void Move(ref string source, ref string dest, string tagName)
        {
            SetValue(ref dest, tagName, GetValue(source, tagName)); Delete(ref source, tagName);
        }
    }

    public class zTAG_Bag_Old
    {
        public string source;
        public zTAG_Bag_Old() { source = ""; }
        public zTAG_Bag_Old(string src)
        {
            if (string.IsNullOrEmpty(src)) src = "";
            source = src;
        }

        public static string Build(string tagName, string value)
        {
            if (value == null) return "";
            return string.Format("<{0}={1}>", tagName, value);
        }
        public void Delete(string tagName)
        {
            if (!string.IsNullOrEmpty(source)) SetValue(tagName, null);
        }
        public string GetValue(string tagName)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string tagId = "<" + tagName + "=";
            int pos = source.IndexOf(tagId);
            if (pos < 0) return null;
            int endpos = source.IndexOf(">", pos);
            if (pos < 0) return null;
            pos += tagId.Length;
            return source.Substring(pos, endpos - pos);

            //if (!source.Contains(tagId)) return null;
            //string[] a = source.Split('>');
            //foreach (string s in a) if (s.StartsWith(tagId)) return s.Substring(tagId.Length);
            //return null;
        }
        public string GetTag(string tagName) { return Build(tagName, GetValue(tagName)); }

        public void SetValue(string tagName, string newValue)
        {
            if (source == null) source = "";
            string oldtag = Build(tagName, GetValue(tagName));
            string newtag = Build(tagName, newValue);
            if (oldtag != "" && source.Contains(oldtag)) source = source.Replace(oldtag, newtag);
            else source += newtag;
        }
        public void SetValue1st(string tagName, string newValue)
        {
            if (source == null) source = "";
            string oldtag = Build(tagName, GetValue(tagName));
            string newtag = Build(tagName, newValue);
            if (oldtag != "" && source.Contains(oldtag)) source = source.Replace(oldtag, newtag);
            else source = newtag + source;
        }
        public bool Exists(string tagName)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return source.Contains("<" + tagName + "=");
        }
        public void Rename(string tagName, string newName)
        {
            if (Exists(tagName))
            {
                string value = GetValue(tagName);
                Delete(tagName);
                SetValue(newName, value);
            }
        }
    }

    public class TAG
    {
        public static void Test()
        {
            TAG t = new TAG();
            t.SetValue("1", "asdf");
            t.SetValue("2", "asdf%zzz");
            t.SetValue("3", "asdf");
            t.SetValue("4", "asdf");
            t.SetValue("5", "asdf");

            string s;
            s = t.GetValue("1");
            s = t.GetValue("2");
            t.SetValue("3", "%\n;;;asdf");
            s = t.GetValue("3");
            s = t.GetValue("4");
            s = t.GetValue("5");
            s = t.GetValue("6");
            s = t.GetValue("7");
            s = t.GetValue("8");
            s = t.GetValue("9");
        }

        public bool EmptyForbidden = true;
        public string this[string s]
        {
            get { return GetValue(s); }
            set { SetValue(s, value); }
        }

        public override string ToString() { return Source; }
        static string tagEsc_BadValues = "%<>;|\r\n\'\"";
        static string tagEsc_Replaces = "p(),!rnqb";

        public string Source;
        public TAG() { Source = ""; }
        public TAG(string src)
        {
            if (string.IsNullOrEmpty(src)) src = "";
            Source = src;
        }

        static string compress(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                int ind = tagEsc_BadValues.IndexOf(c);
                if (ind >= 0) { sb.Append('%'); sb.Append(tagEsc_Replaces[ind]); }
                else sb.Append(c);
            }
            return sb.ToString();
        }
        static string decompress(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.Contains("%")) return value;
            StringBuilder sb = new StringBuilder();
            bool esc = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (esc)
                {
                    int ind = tagEsc_Replaces.IndexOf(c);
                    if (ind < 0) return null; // Error
                    sb.Append(tagEsc_BadValues[ind]);
                    esc = false;
                }
                else
                    if (c == '%') esc = true;
                    else sb.Append(c);
            }
            if (esc) return null;
            return sb.ToString();
        }

        public static string Build(string tagName, string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return string.Format("<{0}={1}>", tagName, compress(value));
        }

        public void Delete(string tagName)
        {
            if (!string.IsNullOrEmpty(Source)) SetValue(tagName, null);
        }
        public string GetValue(string tagName)
        {
            if (string.IsNullOrEmpty(Source)) return null;
            string tagId = "<" + tagName + "=";
            int pos = Source.IndexOf(tagId);
            if (pos < 0) return null;
            int endpos = Source.IndexOf(">", pos);
            if (endpos <= 0) return null;
            pos += tagId.Length;
            return decompress(Source.Substring(pos, endpos - pos));
        }
        public string GetNZValue(string tagName) { string v = GetValue(tagName); return v == null ? "" : v; }
        public string GetTag(string tagName) { return Build(tagName, GetValue(tagName)); }

        public void SetValue(string tagName, string newValue)
        {
            if (Source == null) Source = "";
            string oldtag = Build(tagName, GetValue(tagName));
            string newtag = Build(tagName, newValue);
            if (oldtag != "" && Source.Contains(oldtag)) Source = Source.Replace(oldtag, newtag);
            else Source += newtag;
        }
        public void SetValue1st(string tagName, string newValue)
        {
            if (Source == null) Source = "";
            string oldtag = Build(tagName, GetValue(tagName));
            string newtag = Build(tagName, newValue);
            if (oldtag != "" && Source.Contains(oldtag)) Source = Source.Replace(oldtag, newtag);
            else Source = newtag + Source;
        }
        public bool Exists(string tagName)
        {
            if (string.IsNullOrEmpty(Source)) return false;
            return Source.Contains("<" + tagName + "=");
        }
        public void Rename(string tagName, string newName)
        {
            if (Exists(tagName))
            {
                string value = GetValue(tagName);
                Delete(tagName);
                SetValue(newName, value);
            }
        }
        public int GetInt(string tagName)
        {
            string val = GetValue(tagName);
            if (string.IsNullOrEmpty(val)) return -1;
            int i;
            if (int.TryParse(val, out i) == false) return -1;
            return i;
        }
        public void SetValue(string tagName, int val) { SetValue(tagName, val.ToString()); }
        public decimal GetDecimal(string tagName)
        {
            string val = GetValue(tagName);
            if (string.IsNullOrEmpty(val)) return -1;
            decimal i;
            if (decimal.TryParse(val.Replace('.', ','), out i) == false) return -1;
            return i;
        }
        public void SetValue(string tagName, decimal val) { SetValue(tagName, ToStr(val)); }
        public static string ToStr(object num)
        {
            string s = num.ToString().Replace(',', '.');
            if (s.Contains(".")) s = s.TrimEnd('0');
            if (s.EndsWith(".")) s = s.Replace(".", "");
            return s;
        }
        public SimpleList GetTags()
        {
            SimpleList list = new SimpleList();
            string[] da = Source.Split('<');
            foreach (string s in da)
            {
                int ind = s.IndexOf('=');
                if (ind > 0) list.Add(s.Substring(0, ind));
            }
            return list;
        }
    }

    
    /*
	public class TCPSelfRestoringConnection
	{
		string addr;
		int port;
		System.Threading.WaitCallback onConnect;

		bool isClosed;
		public TcpClient Client;

		public int pauseTicks = 10 * 1000;

		public TCPSelfRestoringConnection(string addr, int port, System.Threading.WaitCallback onConnect)
		{
			this.addr = addr;
			this.port = port;
			this.onConnect = onConnect;

			System.Threading.ThreadPool.QueueUserWorkItem(cycle);
		}

		public void Close()
		{
			isClosed = true;
			try
			{
				Client.Client.Close();
				Client.Close();
			}
			catch { }
		}

		void cycle(object o)
		{
			while (!isClosed)
			{
				try
				{
					Client = new TcpClient(addr, port);
					onConnect(this);
				}
				catch
				{
					System.Threading.Thread.Sleep(pauseTicks);
				}
			}
		}
	}
     */

    //-----------------------------------------------------------------
    // Ini file support
    //-----------------------------------------------------------------
    public static class FileUtils
    {
        public static void Save(MemoryStreamEx ms, string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            fs.Write(ms.GetBuffer(), 0, ms.DataLength);
            fs.Close();
        }

        public static int ScanDir(string path, string pattern, bool subDirs, bool trace, System.Threading.WaitCallback callBack)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!Directory.Exists(di.FullName)) return -1;
            FileInfo[] files = di.GetFiles(pattern, subDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            int nfiles = 0;
            foreach (FileInfo fi in files)
            {
                if (trace) Console.WriteLine(fi.FullName);
                callBack(fi);
                nfiles++;
            }
            if (trace) Console.WriteLine("Complete {0} files", nfiles);
            return nfiles;
        }
        public static string MakeGZFileCopy(string dir, string fname, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) pattern = "!TM14!_!N!";
            try
            {
                string name = Path.GetFileName(fname);

                if (!string.IsNullOrEmpty(pattern))
                {
                    pattern = pattern.Replace("!N!", name);
                    pattern = pattern.Replace("!TM14!", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    name = pattern;
                }

                string newName = Path.Combine(dir, name + ".gz");
                Directory.CreateDirectory(dir);
                File.Delete(newName);
                Compression.CompressFile(true, Path.GetFullPath(fname), Path.GetFullPath(newName));
                return newName;
            }
            catch { }
            return null;
        }
    }

    //-----------------------------------------------------------------
    // Ini file support
    //-----------------------------------------------------------------
    public static class IPUtils
    {
        private static void _ConnectCallback(IAsyncResult ar)
        {
            TcpClient client = null;
            try
            {
                object[] state = (object[])ar.AsyncState;
                client = state[0] as TcpClient;
                client.EndConnect(ar); // Complete the connection.
                System.Threading.Thread.Sleep(1000);
                if (state[1] != null) return;
            }
            catch { }
            if (client != null)
            {
                try { client.Client.Close(); }
                catch { }
                try { client.Close(); }
                catch { }
            }
        }

        public static TcpClient Connect(int timeout, string hostName, int port)
        {
            TcpClient client = new TcpClient();
            object[] state = new object[2];
            state[0] = client;
            client.BeginConnect(hostName, port, new AsyncCallback(_ConnectCallback), state);
            ManualTimer timer = new ManualTimer(timeout);
            while (!client.Connected && !timer.Timeout) System.Threading.Thread.Sleep(2);
            if (client.Connected)
            {
                state[1] = new object();
                return client;
            }
            try { client.Client.Close(); }
            catch { }
            try { client.Close(); }
            catch { }
            return null;
        }

        public static IPAddress GetAddr(string url)
        {
            string[] sa = url.Split(':');
            return sa[0] == "0.0.0.0" ? IPAddress.Any : Dns.GetHostEntry(sa[0]).AddressList[0];
        }
        public static string GetHostName(string url)
        {
            string[] sa = url.Split(':');
            return sa[0];
        }
        public static int GetPort(string url)
        {
            string[] sa = url.Split(':');
            return int.Parse(sa[1]);
        }
        public static IPEndPoint GetEndPoint(string url)
        {
            return new IPEndPoint(GetAddr(url), GetPort(url));
        }

        public static bool Matched(string ip, string pattern)
        {
            return ip.StartsWith(pattern);
        }
        public static bool Matched2List(string ip, string[] patterns)
        {
            foreach (string allowedAddress in patterns) if (Matched(ip, allowedAddress)) return true;
            return false;
        }
        public static bool Matched2List(string ip, string patterns)
        {
            string[] splitted = ip.Split('.', ':');
            if (splitted.Length < 4) return false;
            SimpleList list = new SimpleList(patterns);
            if (list.Contains(Parser.Combine('.', splitted[0], splitted[1], splitted[2], splitted[3]))) return true;
            if (list.Contains(Parser.Combine('.', splitted[0], splitted[1], splitted[2], "0"))) return true;
            if (list.Contains(Parser.Combine('.', splitted[0], splitted[1], "0", "0"))) return true;
            return false;
        }

        public static UInt32 IPv4ToUInt32(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return 0;

            int n = StringUtils.Count(ip, ':');
            if (n > 1) return 0; // IPv6 not supported
            if (n == 1) ip = ip.Substring(0, ip.IndexOf(':'));

            IPAddress ipa;
            if (IPAddress.TryParse(ip, out ipa) == false) return 0; // error parsing 

            byte[] ipBytes = ipa.GetAddressBytes();
            return (((UInt32)ipBytes[0]) << 24) + (((UInt32)ipBytes[1]) << 16) + (((UInt32)ipBytes[2]) << 8) + (((UInt32)ipBytes[3]));
        }

        public static bool IPIsPrivate(string ip)
        {
            /*10.0.0.0/8
            172.16.0.0/12
            192.168.0.0/16
            127.0.0.0/8
            169.254.0.0/16 
            */
            if (string.IsNullOrEmpty(ip)) return true;
            return ip.StartsWith("10.") || ip.StartsWith("172.16.") || ip.StartsWith("192.168.") || ip.StartsWith("127.") || ip.StartsWith("169.254.");
        }
    }

    //----------------------------------------------------------------------------
    // MyGuid
    //----------------------------------------------------------------------------
    public static class AGuid
    {
        public static void I2B(UInt32 n, byte[] buf, int offset) { for (int i = 0; i < 4; i++) { buf[offset + i] = (byte)n; n >>= 4; } }
        public static byte[] GetBytes(string k1, string k2, string k3, string k4)
        {
            byte[] buf = new byte[16];
            I2B(CRCGenerator.CRC32(Encoding.ASCII.GetBytes(k1), 1), buf, 0);
            I2B(CRCGenerator.CRC32(Encoding.ASCII.GetBytes(k2), 1), buf, 4);
            I2B(CRCGenerator.CRC32(Encoding.ASCII.GetBytes(k3), 1), buf, 8);
            I2B(CRCGenerator.CRC32(Encoding.ASCII.GetBytes(k4), 1), buf, 12);
            return buf;
        }
        public static string GetString(string k1, string k2, string k3, string k4) { return AsHexString(GetBytes(k1, k2, k3, k4)); }
        public static string AsHexString(byte[] buf)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buf) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
        public static byte[] FromHexString(string str)
        {
            if ((str.Length & 1) != 0) str = "0" + str;
            byte[] buf = new byte[str.Length / 2];
            for (int i = 0; i < buf.Length; i++) buf[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            return buf;
        }
    }

    //----------------------------------------------------------------------------
    // Codecs
    //----------------------------------------------------------------------------

    public static class Codecs
    {
        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        /*
        Note two things, first I am using ASCII encoding, which should cover most folks. Just in case though, System.Text has encodings for the various flavors of UTF as well as Unicode. Just choose the appropriate encoding method for your need.
        Second, I made the class static because I was using a console app for my test harness. While it could be static in your class, there’s no reason it has to be. Your choice. 
        OK, we’ve got the string encoded, at some point we’re going to want to decode it. We essentially do the reverse of encoding, we call the FromBase64String and pass in our encoded string, which returns a byte array. We then call the AsciiEncoding GetString to convert our byte array to a string. Here’s a small method to Decode your Base64 strings.
        */
        static public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
        static public string DecodeFrom64_852(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.GetEncoding(852).GetString(encodedDataAsBytes);
            return returnValue;
        }

        /*
        public void SendAuthRequestUTD(String Racun, int SeqNumber)
        {
            var enc = Encoding.GetEncoding(852);
            byte[] byteXml = enc.GetBytes(Racun);
            var myBE = new Base64Encoder(byteXml);
            char[] myEncoded = myBE.GetEncoded();
            var myStr = new StringBuilder();
            myStr.Append(myEncoded);
            var sHelp = myStr.ToString();
            try
            {
                saleRequestSended = true;
                communicator.SendSaleAuthorisationRequestMellon(sHelp, SeqNumber);
            }
            catch (Exception ex)
            {
            }
        }
        */



        //-----------------------------------------------------------------
        static readonly byte[] UUEncMap = new byte[]
        {
          0x60, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
          0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
          0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
          0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
          0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
          0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
          0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F
        };

        static readonly byte[] UUDecMap = new byte[]
        {
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
          0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
          0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
          0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
          0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static void UUDecode(System.IO.Stream input, System.IO.Stream output)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (output == null)
                throw new ArgumentNullException("output");

            long len = input.Length;
            if (len == 0)
                return;

            long didx = 0;
            int nextByte = input.ReadByte();
            while (nextByte >= 0)
            {
                // get line length (in number of encoded octets)
                int line_len = UUDecMap[nextByte];

                // ascii printable to 0-63 and 4-byte to 3-byte conversion
                long end = didx + line_len;
                byte A, B, C, D;
                if (end > 2)
                {
                    while (didx < end - 2)
                    {
                        A = UUDecMap[input.ReadByte()];
                        B = UUDecMap[input.ReadByte()];
                        C = UUDecMap[input.ReadByte()];
                        D = UUDecMap[input.ReadByte()];

                        output.WriteByte((byte)(((A << 2) & 255) | ((B >> 4) & 3)));
                        output.WriteByte((byte)(((B << 4) & 255) | ((C >> 2) & 15)));
                        output.WriteByte((byte)(((C << 6) & 255) | (D & 63)));
                        didx += 3;
                    }
                }

                if (didx < end)
                {
                    A = UUDecMap[input.ReadByte()];
                    B = UUDecMap[input.ReadByte()];
                    output.WriteByte((byte)(((A << 2) & 255) | ((B >> 4) & 3)));
                    didx++;
                }

                if (didx < end)
                {
                    B = UUDecMap[input.ReadByte()];
                    C = UUDecMap[input.ReadByte()];
                    output.WriteByte((byte)(((B << 4) & 255) | ((C >> 2) & 15)));
                    didx++;
                }

                // skip padding
                do
                {
                    nextByte = input.ReadByte();
                }
                while (nextByte >= 0 && nextByte != '\n' && nextByte != '\r');

                // skip end of line
                do
                {
                    nextByte = input.ReadByte();
                }
                while (nextByte >= 0 && (nextByte == '\n' || nextByte == '\r'));
            }
        }

        public static void UUEncode(System.IO.Stream input, System.IO.Stream output)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (output == null)
                throw new ArgumentNullException("output");

            long len = input.Length;
            if (len == 0)
                return;

            int sidx = 0;
            int line_len = 45;
            byte[] nl = Encoding.ASCII.GetBytes(Environment.NewLine);

            byte A, B, C;
            // split into lines, adding line-length and line terminator
            while (sidx + line_len < len)
            {
                // line length
                output.WriteByte(UUEncMap[line_len]);

                // 3-byte to 4-byte conversion + 0-63 to ascii printable conversion
                for (int end = sidx + line_len; sidx < end; sidx += 3)
                {
                    A = (byte)input.ReadByte();
                    B = (byte)input.ReadByte();
                    C = (byte)input.ReadByte();

                    output.WriteByte(UUEncMap[(A >> 2) & 63]);
                    output.WriteByte(UUEncMap[(B >> 4) & 15 | (A << 4) & 63]);
                    output.WriteByte(UUEncMap[(C >> 6) & 3 | (B << 2) & 63]);
                    output.WriteByte(UUEncMap[C & 63]);
                }

                // line terminator
                for (int idx = 0; idx < nl.Length; idx++)
                    output.WriteByte(nl[idx]);
            }

            // line length
            output.WriteByte(UUEncMap[len - sidx]);

            // 3-byte to 4-byte conversion + 0-63 to ascii printable conversion
            while (sidx + 2 < len)
            {
                A = (byte)input.ReadByte();
                B = (byte)input.ReadByte();
                C = (byte)input.ReadByte();

                output.WriteByte(UUEncMap[(A >> 2) & 63]);
                output.WriteByte(UUEncMap[(B >> 4) & 15 | (A << 4) & 63]);
                output.WriteByte(UUEncMap[(C >> 6) & 3 | (B << 2) & 63]);
                output.WriteByte(UUEncMap[C & 63]);
                sidx += 3;
            }

            if (sidx < len - 1)
            {
                A = (byte)input.ReadByte();
                B = (byte)input.ReadByte();

                output.WriteByte(UUEncMap[(A >> 2) & 63]);
                output.WriteByte(UUEncMap[(B >> 4) & 15 | (A << 4) & 63]);
                output.WriteByte(UUEncMap[(B << 2) & 63]);
                output.WriteByte(UUEncMap[0]);
            }
            else if (sidx < len)
            {
                A = (byte)input.ReadByte();

                output.WriteByte(UUEncMap[(A >> 2) & 63]);
                output.WriteByte(UUEncMap[(A << 4) & 63]);
                output.WriteByte(UUEncMap[0]);
                output.WriteByte(UUEncMap[0]);
            }

            // line terminator
            for (int idx = 0; idx < nl.Length; idx++)
                output.WriteByte(nl[idx]);
        }
    }

}
