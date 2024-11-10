// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TouchPad.TouchPadHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util.TouchPad
{
  public class TouchPadHelper
  {
    internal static class TouchpadHelper
    {
      private const uint RIM_TYPEMOUSE = 0;
      private const uint RIM_TYPEKEYBOARD = 1;
      private const uint RIM_TYPEHID = 2;
      private const uint RIDEV_INPUTSINK = 256;
      private const uint RID_INPUT = 268435459;
      private const uint RIDI_PREPARSEDDATA = 536870917;
      private const uint RIDI_DEVICEINFO = 536870923;
      private const uint HIDP_STATUS_SUCCESS = 1114112;
      public const int WM_INPUT = 255;
      public const int RIM_INPUT = 0;
      public const int RIM_INPUTSINK = 1;

      [DllImport("User32", SetLastError = true)]
      private static extern uint GetRawInputDeviceList(
        [Out] TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST[] pRawInputDeviceList,
        ref uint puiNumDevices,
        uint cbSize);

      [DllImport("User32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool RegisterRawInputDevices(
        TouchPadHelper.TouchpadHelper.RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

      [DllImport("User32.dll", SetLastError = true)]
      private static extern uint GetRawInputData(
        IntPtr hRawInput,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize,
        uint cbSizeHeader);

      [DllImport("User32.dll", SetLastError = true)]
      private static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize);

      [DllImport("User32.dll", SetLastError = true)]
      private static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        ref TouchPadHelper.TouchpadHelper.RID_DEVICE_INFO pData,
        ref uint pcbSize);

      [DllImport("Hid.dll", SetLastError = true)]
      private static extern uint HidP_GetCaps(
        IntPtr PreparsedData,
        out TouchPadHelper.TouchpadHelper.HIDP_CAPS Capabilities);

      [DllImport("Hid.dll", CharSet = CharSet.Auto)]
      private static extern uint HidP_GetValueCaps(
        TouchPadHelper.TouchpadHelper.HIDP_REPORT_TYPE ReportType,
        [Out] TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS[] ValueCaps,
        ref ushort ValueCapsLength,
        IntPtr PreparsedData);

      [DllImport("Hid.dll", CharSet = CharSet.Auto)]
      private static extern uint HidP_GetUsageValue(
        TouchPadHelper.TouchpadHelper.HIDP_REPORT_TYPE ReportType,
        ushort UsagePage,
        ushort LinkCollection,
        ushort Usage,
        out uint UsageValue,
        IntPtr PreparsedData,
        IntPtr Report,
        uint ReportLength);

      public static bool Exists()
      {
        uint puiNumDevices = 0;
        uint cbSize = (uint) Marshal.SizeOf<TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST>();
        if (TouchPadHelper.TouchpadHelper.GetRawInputDeviceList((TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST[]) null, ref puiNumDevices, cbSize) != 0U)
          return false;
        TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST[] rawinputdevicelistArray = new TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST[(int) puiNumDevices];
        if ((int) TouchPadHelper.TouchpadHelper.GetRawInputDeviceList(rawinputdevicelistArray, ref puiNumDevices, cbSize) != (int) puiNumDevices)
          return false;
        foreach (TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST rawinputdevicelist in ((IEnumerable<TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST>) rawinputdevicelistArray).Where<TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST>((Func<TouchPadHelper.TouchpadHelper.RAWINPUTDEVICELIST, bool>) (x => x.dwType == 2U)))
        {
          uint pcbSize = 0;
          if (TouchPadHelper.TouchpadHelper.GetRawInputDeviceInfo(rawinputdevicelist.hDevice, 536870923U, IntPtr.Zero, ref pcbSize) == 0U)
          {
            TouchPadHelper.TouchpadHelper.RID_DEVICE_INFO pData = new TouchPadHelper.TouchpadHelper.RID_DEVICE_INFO()
            {
              cbSize = pcbSize
            };
            if (TouchPadHelper.TouchpadHelper.GetRawInputDeviceInfo(rawinputdevicelist.hDevice, 536870923U, ref pData, ref pcbSize) != uint.MaxValue && pData.hid.usUsagePage == (ushort) 13 && pData.hid.usUsage == (ushort) 5)
              return true;
          }
        }
        return false;
      }

      public static bool RegisterInput(IntPtr windowHandle)
      {
        return TouchPadHelper.TouchpadHelper.RegisterRawInputDevices(new TouchPadHelper.TouchpadHelper.RAWINPUTDEVICE[1]
        {
          new TouchPadHelper.TouchpadHelper.RAWINPUTDEVICE()
          {
            usUsagePage = (ushort) 13,
            usUsage = (ushort) 5,
            dwFlags = 0U,
            hwndTarget = windowHandle
          }
        }, 1U, (uint) Marshal.SizeOf<TouchPadHelper.TouchpadHelper.RAWINPUTDEVICE>());
      }

      public static TouchpadContact[] ParseInput(IntPtr lParam)
      {
        uint pcbSize1 = 0;
        uint cbSizeHeader = (uint) Marshal.SizeOf<TouchPadHelper.TouchpadHelper.RAWINPUTHEADER>();
        if (TouchPadHelper.TouchpadHelper.GetRawInputData(lParam, 268435459U, IntPtr.Zero, ref pcbSize1, cbSizeHeader) != 0U)
          return (TouchpadContact[]) null;
        IntPtr num1 = IntPtr.Zero;
        TouchPadHelper.TouchpadHelper.RAWINPUT structure;
        byte[] numArray1;
        try
        {
          num1 = Marshal.AllocHGlobal((int) pcbSize1);
          if ((int) TouchPadHelper.TouchpadHelper.GetRawInputData(lParam, 268435459U, num1, ref pcbSize1, cbSizeHeader) != (int) pcbSize1)
            return (TouchpadContact[]) null;
          structure = Marshal.PtrToStructure<TouchPadHelper.TouchpadHelper.RAWINPUT>(num1);
          byte[] numArray2 = new byte[(int) pcbSize1];
          Marshal.Copy(num1, numArray2, 0, numArray2.Length);
          numArray1 = new byte[(int) structure.Hid.dwSizeHid * (int) structure.Hid.dwCount];
          int srcOffset = (int) pcbSize1 - numArray1.Length;
          Buffer.BlockCopy((Array) numArray2, srcOffset, (Array) numArray1, 0, numArray1.Length);
        }
        finally
        {
          Marshal.FreeHGlobal(num1);
        }
        IntPtr num2 = Marshal.AllocHGlobal(numArray1.Length);
        Marshal.Copy(numArray1, 0, num2, numArray1.Length);
        IntPtr num3 = IntPtr.Zero;
        try
        {
          uint pcbSize2 = 0;
          if (TouchPadHelper.TouchpadHelper.GetRawInputDeviceInfo(structure.Header.hDevice, 536870917U, IntPtr.Zero, ref pcbSize2) != 0U)
            return (TouchpadContact[]) null;
          num3 = Marshal.AllocHGlobal((int) pcbSize2);
          TouchPadHelper.TouchpadHelper.HIDP_CAPS Capabilities;
          if ((int) TouchPadHelper.TouchpadHelper.GetRawInputDeviceInfo(structure.Header.hDevice, 536870917U, num3, ref pcbSize2) != (int) pcbSize2 || TouchPadHelper.TouchpadHelper.HidP_GetCaps(num3, out Capabilities) != 1114112U)
            return (TouchpadContact[]) null;
          ushort numberInputValueCaps = Capabilities.NumberInputValueCaps;
          TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS[] hidpValueCapsArray = new TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS[(int) numberInputValueCaps];
          if (TouchPadHelper.TouchpadHelper.HidP_GetValueCaps(TouchPadHelper.TouchpadHelper.HIDP_REPORT_TYPE.HidP_Input, hidpValueCapsArray, ref numberInputValueCaps, num3) != 1114112U)
            return (TouchpadContact[]) null;
          uint num4 = 0;
          TouchpadContactCreator touchpadContactCreator = new TouchpadContactCreator();
          List<TouchpadContact> touchpadContactList = new List<TouchpadContact>();
          foreach (TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS hidpValueCaps in (IEnumerable<TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS>) ((IEnumerable<TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS>) hidpValueCapsArray).OrderBy<TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS, ushort>((Func<TouchPadHelper.TouchpadHelper.HIDP_VALUE_CAPS, ushort>) (x => x.LinkCollection)))
          {
            uint UsageValue;
            if (TouchPadHelper.TouchpadHelper.HidP_GetUsageValue(TouchPadHelper.TouchpadHelper.HIDP_REPORT_TYPE.HidP_Input, hidpValueCaps.UsagePage, hidpValueCaps.LinkCollection, hidpValueCaps.Usage, out UsageValue, num3, num2, (uint) numArray1.Length) == 1114112U)
            {
              if (hidpValueCaps.LinkCollection == (ushort) 0)
              {
                if ((hidpValueCaps.UsagePage != (ushort) 13 || hidpValueCaps.Usage != (ushort) 86) && hidpValueCaps.UsagePage == (ushort) 13 && hidpValueCaps.Usage == (ushort) 84)
                  num4 = UsageValue;
              }
              else if (hidpValueCaps.UsagePage == (ushort) 13 && hidpValueCaps.Usage == (ushort) 81)
                touchpadContactCreator.ContactId = new int?((int) UsageValue);
              else if (hidpValueCaps.UsagePage == (ushort) 1 && hidpValueCaps.Usage == (ushort) 48)
                touchpadContactCreator.X = new int?((int) UsageValue);
              else if (hidpValueCaps.UsagePage == (ushort) 1 && hidpValueCaps.Usage == (ushort) 49)
                touchpadContactCreator.Y = new int?((int) UsageValue);
              TouchpadContact contact;
              if (touchpadContactCreator.TryCreate(out contact))
              {
                touchpadContactList.Add(contact);
                if ((long) touchpadContactList.Count < (long) num4)
                  touchpadContactCreator.Clear();
                else
                  break;
              }
            }
          }
          return touchpadContactList.ToArray();
        }
        finally
        {
          Marshal.FreeHGlobal(num2);
          Marshal.FreeHGlobal(num3);
        }
      }

      private struct RAWINPUTDEVICELIST
      {
        public IntPtr hDevice;
        public uint dwType;
      }

      private struct RAWINPUTDEVICE
      {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
      }

      private struct RAWINPUT
      {
        public TouchPadHelper.TouchpadHelper.RAWINPUTHEADER Header;
        public TouchPadHelper.TouchpadHelper.RAWHID Hid;
      }

      private struct RAWINPUTHEADER
      {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
      }

      private struct RAWHID
      {
        public uint dwSizeHid;
        public uint dwCount;
        public IntPtr bRawData;
      }

      private struct RID_DEVICE_INFO
      {
        public uint cbSize;
        public uint dwType;
        public TouchPadHelper.TouchpadHelper.RID_DEVICE_INFO_HID hid;
      }

      private struct RID_DEVICE_INFO_HID
      {
        public uint dwVendorId;
        public uint dwProductId;
        public uint dwVersionNumber;
        public ushort usUsagePage;
        public ushort usUsage;
      }

      private struct HIDP_CAPS
      {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
      }

      private enum HIDP_REPORT_TYPE
      {
        HidP_Input,
        HidP_Output,
        HidP_Feature,
      }

      private struct HIDP_VALUE_CAPS
      {
        public ushort UsagePage;
        public byte ReportID;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsAlias;
        public ushort BitField;
        public ushort LinkCollection;
        public ushort LinkUsage;
        public ushort LinkUsagePage;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsRange;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsStringRange;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsDesignatorRange;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsAbsolute;
        [MarshalAs(UnmanagedType.U1)]
        public bool HasNull;
        public byte Reserved;
        public ushort BitSize;
        public ushort ReportCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public ushort[] Reserved2;
        public uint UnitsExp;
        public uint Units;
        public int LogicalMin;
        public int LogicalMax;
        public int PhysicalMin;
        public int PhysicalMax;
        public ushort UsageMin;
        public ushort UsageMax;
        public ushort StringMin;
        public ushort StringMax;
        public ushort DesignatorMin;
        public ushort DesignatorMax;
        public ushort DataIndexMin;
        public ushort DataIndexMax;

        public ushort Usage => this.UsageMin;

        public ushort StringIndex => this.StringMin;

        public ushort DesignatorIndex => this.DesignatorMin;

        public ushort DataIndex => this.DataIndexMin;
      }
    }
  }
}
