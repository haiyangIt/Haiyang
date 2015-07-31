using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MyInterop
{
    //[Guid("898C03F3-6571-4DB2-AE14-3E1D0DE2D6FC")]
    public interface ISuperInterface
    {
        int Add(int i, int j);
    }

    [Guid("795A6100-78C6-4799-B9F2-6D135F41AB17")]
    public interface IChildrenInterface : ISuperInterface
    {
        int Multi(int i, int j);
    }


    [ClassInterface(ClassInterfaceType.None)]
    [Guid("77D6A2D3-A61C-4B6F-B5C0-7C09E0419DE6")]
    public class TestCom : IChildrenInterface
    {
        public int Multi(int i, int j)
        {
            return i * j;
        }

        public int Add(int i, int j)
        {
            return i + j;
        }
    }
}
