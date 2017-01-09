namespace NLog.Targets.Lumberjack
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal class LumberjackProtocol
    {
        private static readonly byte DataFrameByte = Encoding.ASCII.GetBytes("D")[0];
        private static readonly byte Version1Byte = Encoding.ASCII.GetBytes("1")[0];
        private static readonly byte WindowSizeFrameByte = Encoding.ASCII.GetBytes("W")[0];

        internal byte[] CreatePacket(IDictionary<string, object> data, int sequenceID)
        {
            using (var mem = new MemoryStream())
            {
                mem.WriteByte(Version1Byte);
                mem.WriteByte(DataFrameByte);

                var buff = new byte[8];
                buff[0] = (byte)(sequenceID >> 24);
                buff[1] = (byte)(sequenceID >> 16);
                buff[2] = (byte)(sequenceID >> 8);
                buff[3] = (byte)(sequenceID);
                buff[7] = (byte)(data.Count);
                mem.Write(buff, 0, 8);

                foreach (var property in data)
                    WriteKVP(mem, property.Key, property.Value as string ?? string.Empty);
                return mem.ToArray();
            }
        }

        internal byte[] CreateWindowSizePacket(int windowSize)
        {
            using (var mem = new MemoryStream())
            {
                mem.WriteByte(Version1Byte);
                mem.WriteByte(WindowSizeFrameByte);

                var buff = new byte[4];
                buff[0] = (byte)(windowSize >> 24);
                buff[1] = (byte)(windowSize >> 16);
                buff[2] = (byte)(windowSize >> 8);
                buff[3] = (byte)(windowSize);
                mem.Write(buff, 0, buff.Length);
                return mem.ToArray();
            }
        }

        private void WriteKVP(Stream stream, string key, string value)
        {
            var lenBuff = new byte[4];
            byte[] dataBuff;

            dataBuff = Encoding.UTF8.GetBytes(key);
            lenBuff[0] = (byte)(dataBuff.Length >> 24);
            lenBuff[1] = (byte)(dataBuff.Length >> 16);
            lenBuff[2] = (byte)(dataBuff.Length >> 8);
            lenBuff[3] = (byte)(dataBuff.Length);
            stream.Write(lenBuff, 0, lenBuff.Length);
            stream.Write(dataBuff, 0, dataBuff.Length);

            dataBuff = Encoding.UTF8.GetBytes(value);
            lenBuff[0] = (byte)(dataBuff.Length >> 24);
            lenBuff[1] = (byte)(dataBuff.Length >> 16);
            lenBuff[2] = (byte)(dataBuff.Length >> 8);
            lenBuff[3] = (byte)(dataBuff.Length);
            stream.Write(lenBuff, 0, lenBuff.Length);
            stream.Write(dataBuff, 0, dataBuff.Length);
        }
    }
}