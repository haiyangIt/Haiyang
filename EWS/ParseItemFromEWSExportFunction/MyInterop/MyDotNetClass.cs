using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MyInterop
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	/// 
	[Guid("3E0E2EB2-CC13-40fb-9346-34809CB2418C")]
	public interface IMyDotNetInterface
	{
		void ShowDialog ();
        void TestString([MarshalAs(UnmanagedType.LPWStr)]string name);
        void InputBytesAndString(byte[] array, int length);
        void GetProperty(IntPtr item);
        void GetProperties(IntPtr itemArray, int arrayLength);
        IChildInterface GetChildInterface(int index);
	}

    [Guid("27FFD310-5783-47B0-ABEF-D60E382B43D3")]
    public interface IChildInterface
    {
        void ShowDialog();
    }
	
	[ClassInterface(ClassInterfaceType.None)]
	[Guid("3A13EB34-3930-4e06-A3EB-10724CCC2F4B")]
	public class MyDotNetClass:IMyDotNetInterface
	{
		public MyDotNetClass ()
		{
		}

		public void ShowDialog ()
		{
			MessageBox.Show ("I am a  Managed DotNET C# COM Object Dialog");
		}




        public void InputBytesAndString(byte[] array, int length)
        {
            var test = array;
            var len = length;
        }


        public void TestString(string name)
        {
            var n = name;
            MessageBox.Show(name);
        }


        public void GetProperty(IntPtr itemPtr)
        {
            StringBuilder sb = new StringBuilder();
            E15PropertyItem item = (E15PropertyItem)Marshal.PtrToStructure(itemPtr, typeof(E15PropertyItem));
            sb.Append("item.tag:").AppendLine(item.nTag.ToString("X4"));
            sb.Append("item.nuseId:").AppendLine(item.nUsID.ToString("X4"));
            sb.Append("item.propertyTYpe:").AppendLine(item.PropertyType.ToString());
            sb.Append("item.length:").AppendLine(item.nLength.ToString());
            sb.Append("item.NameId.ulKind:").AppendLine(item.namedID.ulKind.ToString());
            MyGUID myguid = (MyGUID)Marshal.PtrToStructure(item.namedID.lpguid, typeof(MyGUID));
            Guid guid = new Guid((int)myguid.Data1, (short)myguid.Data2, (short)myguid.Data3, myguid.Data4);
            sb.Append("item.NameId.propertySet:").AppendLine(guid.ToString());
            if(item.namedID.ulKind == 0)
            {
                sb.Append("item.nameid.iid:").AppendLine(item.namedID.Kind.IID.ToString("X2"));
            }
            else
            {
                string name = (string)Marshal.PtrToStringUni(item.namedID.Kind.lpwstrName);

                sb.Append("item.nameid.name:").AppendLine(name);
            }

            byte[] values = new byte[item.nLength];
            Marshal.Copy(item.pValue, values, 0, item.nLength);
            sb.Append("item.values:");
            for (int index = 0; index < item.nLength; index++)
            {
                sb.Append(values[index].ToString("X2")).Append(" ");
            }
            sb.AppendLine("");
            MessageBox.Show(sb.ToString());
        }

        public void GetProperties(IntPtr itemArray, int arrayLength)
        {
            IntPtr temp = itemArray;
            for(int i = 0 ; i < arrayLength ; i++)
            {
                GetProperty(temp);
                temp = new IntPtr(temp.ToInt32() + Marshal.SizeOf(typeof(E15PropertyItem)));
            }
        }


        public IChildInterface GetChildInterface(int index)
        {
            return new MyChildInterface(index);
        }
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("52617946-C790-48A9-AD99-DB9D512B4837")]
    public class MyChildInterface : IChildInterface
    {
        private int _index = 0;
        public MyChildInterface(int index)
        {
            _index = index;
        }

        public void ShowDialog()
        {
            MessageBox.Show("index is " + _index);
        }
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct NameIDValue
    {

        /// LONG->int
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public int IID;

        /// LPWSTR->WCHAR*
        [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        public System.IntPtr lpwstrName;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct MAPINAMEID
    {

        /// LPGUID->GUID*
        public System.IntPtr lpguid;

        /// ULONG->unsigned int
        public uint ulKind;

        /// Anonymous_30d1fee8_2a01_4b2d_9a78_3c03898321ce
        public NameIDValue Kind;
    }

    public enum MAPIPropertyType
    {

        Unkown,

        BOOL,

        String,

        Int16,

        Int32,

        Int64,

        ByteArray,

        Double,

        DateTime,

        MVString,
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct E15PropertyItem
    {

        /// UINT->unsigned int
        public uint nTag;

        /// UINT->unsigned int
        public uint nUsID;

        /// _E15PropertyItem_PropertyType
        public MAPIPropertyType PropertyType;

        /// MAPINAMEID->_MAPINAMEID
        public MAPINAMEID namedID;

        /// int
        public int nLength;

        /// byte*
        public System.IntPtr pValue;
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct MyGUID
    {

        /// unsigned int
        public uint Data1;

        /// unsigned short
        public ushort Data2;

        /// unsigned short
        public ushort Data3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data4;
    }

}
