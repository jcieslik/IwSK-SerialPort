using System;

namespace RS485_Model.Model
{
    public class ModbusFrame
    {
        public byte Address { get; set; }

        public byte Function { get; set; }

        public byte[] Data { get; set; }

        public byte[] ToAscii()
        {
            int frameSize = 9 + Data.Length * 2;
            byte[] frame = new byte[frameSize];
            frame[0] = Convert.ToByte(':');
            WriteByteHex(frame, Address, 1);
            WriteByteHex(frame, Function, 3);
            int index = 5;
            foreach (byte dataByte in Data)
            {
                WriteByteHex(frame, dataByte, index);
                index += 2;
            }
            byte lrc = ModbusChecksums.Lrc(Address, Function, Data);
            WriteByteHex(frame, lrc, frameSize - 4);
            frame[frameSize - 2] = Convert.ToByte('\r');
            frame[frameSize - 1] = Convert.ToByte('\n');
            return frame;
        }

        public byte[] ToRtu()
        {
            int frameSize = 4 + Data.Length;
            byte[] frame = new byte[frameSize];
            frame[0] = Address;
            frame[1] = Function;
            Data.CopyTo(frame, 2);
            ushort crc = ModbusChecksums.Crc16(frame, 0, 2 + Data.Length);
            frame[frameSize - 2] = (byte)crc;
            frame[frameSize - 1] = (byte)(crc >> 8);
            return frame;
        }

        public static ModbusFrame FromAscii(byte[] byteFrame)
        {
            if (byteFrame.Length < 9 || byteFrame.Length % 2 != 1)
            {
                throw new InvalidFrameException("Nieprawidłowa długość ramki");
            }
            if (Convert.ToChar(byteFrame[0]) != ':')
            {
                throw new InvalidFrameException("Ramka musi zaczynać się od znaku :");
            }
            if (Convert.ToChar(byteFrame[byteFrame.Length - 2]) != '\r' ||
                Convert.ToChar(byteFrame[byteFrame.Length - 1]) != '\n')
            {
                throw new InvalidFrameException("Ramka musi kończyć się znakami CR LF");
            }
            ModbusFrame objFrame = new ModbusFrame();
            objFrame.Address = ReadByteHex(byteFrame, 1);
            objFrame.Function = ReadByteHex(byteFrame, 3);
            int dataLength = (byteFrame.Length - 9) / 2;
            objFrame.Data = new byte[dataLength];
            for (int i = 0, j = 5; i < dataLength; i++, j += 2)
            {
                objFrame.Data[i] = ReadByteHex(byteFrame, j);
            }
            byte frameLrc = ReadByteHex(byteFrame, byteFrame.Length - 4);
            byte calculatedLrc = ModbusChecksums.Lrc(objFrame.Address, objFrame.Function, objFrame.Data);
            if (frameLrc != calculatedLrc)
            {
                throw new InvalidFrameException("Nieprawidłowa suma kontrolna");
            }
            return objFrame;
        }

        public static ModbusFrame FromRtu(byte[] byteFrame)
        {
            if (byteFrame.Length < 4)
            {
                throw new InvalidFrameException("Nieprawidłowa długość ramki");
            }
            ModbusFrame objFrame = new ModbusFrame();
            objFrame.Address = byteFrame[0];
            objFrame.Function = byteFrame[1];
            int dataLength = byteFrame.Length - 4;
            objFrame.Data = new byte[dataLength];
            Array.Copy(byteFrame, 2, objFrame.Data, 0, dataLength);
            int frameCrc = byteFrame[byteFrame.Length - 2] + (byteFrame[byteFrame.Length - 1] << 8);
            int calculatedCrc = ModbusChecksums.Crc16(byteFrame, 0, byteFrame.Length - 2);
            if (frameCrc != calculatedCrc)
            {
                throw new InvalidFrameException("Nieprawidłowa suma kontrolna");
            }
            return objFrame;
        }

        private static void WriteByteHex(byte[] frame, byte byteToWrite, int where)
        {
            string hexString = byteToWrite.ToString("X2");
            frame[where] = (byte)hexString[0];
            frame[where + 1] = (byte)hexString[1];
        }

        private static byte ReadByteHex(byte[] frame, int where)
        {
            char c1 = Convert.ToChar(frame[where]);
            char c2 = Convert.ToChar(frame[where + 1]);
            string hexString = new string(new char[] { c1, c2 });
            return Convert.ToByte(hexString, 16);
        }
    }
}
