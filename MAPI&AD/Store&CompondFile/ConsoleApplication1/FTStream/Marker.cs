using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1.FTStream
{
    public class Marker
    {
        public static readonly UInt32 StartTopFld = 0x40090003;
        public static readonly UInt32 EndFolder = 0x400B0003;
        public static readonly UInt32 StartSubFld = 0x400A0003;

        public static readonly UInt32 StartMessage = 0x400C0003;
        public static readonly UInt32 EndMessage = 0x400D0003;
        public static readonly UInt32 StartFAIMsg = 0x40100003;
        public static readonly UInt32 StartEmbed = 0x40010003;
        public static readonly UInt32 EndEmbed = 0x40020003;
        public static readonly UInt32 StartRecip = 0x40030003;
        public static readonly UInt32 EndToRecip = 0x40040003;
        public static readonly UInt32 NewAttach = 0x40000003;
        public static readonly UInt32 EndAttach = 0x400E0003;

        public static readonly UInt32 IncrSyncChg = 0x40120003;
        public static readonly UInt32 IncrSyncChgPartial = 0x407D0003;
        public static readonly UInt32 IncrSyncDel = 0x40130003;
        public static readonly UInt32 IncrSyncEnd = 0x40140003;
        public static readonly UInt32 IncrSyncRead = 0x402F0003;
        public static readonly UInt32 IncrSyncStateBegin = 0x403A0003;
        public static readonly UInt32 IncrSyncStateEnd = 0x403B0003;
        public static readonly UInt32 IncrSyncProgressMode = 0x4074000B;
        public static readonly UInt32 IncrSyncProgressPerMsg = 0x4075000B;
        public static readonly UInt32 IncrSyncMessage = 0x40150003;
        public static readonly UInt32 IncrSyncGroupInfo = 0x407B0102;

        public static readonly UInt32 FXErrorInfo = 0x40180003;

        private static HashSet<UInt32> MarkerHash;
        private static void InitMark()
        {
            MarkerHash = new HashSet<UInt32>();

            MarkerHash.Add(StartTopFld);
            MarkerHash.Add(EndFolder);
            MarkerHash.Add(StartSubFld);

            MarkerHash.Add(StartMessage);
            MarkerHash.Add(EndMessage);
            MarkerHash.Add(StartFAIMsg);
            MarkerHash.Add(StartEmbed);
            MarkerHash.Add(EndEmbed);
            MarkerHash.Add(StartRecip);
            MarkerHash.Add(EndToRecip);
            MarkerHash.Add(NewAttach);
            MarkerHash.Add(EndAttach);

            MarkerHash.Add(IncrSyncChg);
            MarkerHash.Add(IncrSyncChgPartial);
            MarkerHash.Add(IncrSyncDel);
            MarkerHash.Add(IncrSyncEnd);
            MarkerHash.Add(IncrSyncRead);
            MarkerHash.Add(IncrSyncStateBegin);
            MarkerHash.Add(IncrSyncStateEnd);
            MarkerHash.Add(IncrSyncProgressMode);
            MarkerHash.Add(IncrSyncProgressPerMsg);
            MarkerHash.Add(IncrSyncMessage);
            MarkerHash.Add(IncrSyncGroupInfo);

            MarkerHash.Add(FXErrorInfo);
        }

        private static Marker _instance = new Marker();


        public static bool JudgeIsMarker(UInt32 marker)
        {
            if (MarkerHash == null)
                InitMark();
            return MarkerHash.Contains(marker);
        }

        public static bool IsSpecificMarker(IMarker marker, UInt32 markerValue)
        {
            return ((PTypInteger32)marker).Value == markerValue;
        }

        public static bool TryCreateMarker(byte[] buffer, ref int pos, out IMarker marker)
        {
            return PTypInteger32.JudgeIsMarker(buffer, ref pos, out marker);
        }
    }
}
