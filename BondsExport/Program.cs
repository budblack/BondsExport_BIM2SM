using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BondsExport
{
    class Program 
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                BE be = new BE(args[0]);
            }
            else
            {
                BE be = new BE(@"D:\测试\Test.smwu");
            }
        }
    }
}
