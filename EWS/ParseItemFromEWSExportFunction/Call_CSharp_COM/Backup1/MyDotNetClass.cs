using System;
using System.Runtime.InteropServices;
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
	}
}
