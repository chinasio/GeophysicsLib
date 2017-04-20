using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeophysicsLib;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            double fv = IGRFMode.getF(2014, 1, 1, 11.5, 108.5, 1500);
            Console.WriteLine(fv);
        }
    }
}
