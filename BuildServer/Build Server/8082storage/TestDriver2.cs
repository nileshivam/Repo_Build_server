using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTest
{
    interface ITest
    {
        bool test();
    }

    class TestDriver2 : ITest
    {
        public bool test()
        {
            Console.WriteLine("Testing testfile1 and testfile2");
            TestFile3 obj1 = new TestFile3();
            TestFile4 obj2 = new TestFile4();
            Console.WriteLine("\nTesting for 5*4 and 9/3");
            Console.WriteLine("Expected Results are 20 and 3");
            int ans1 = obj1.mul(5, 4);
            int ans2 = obj2.div(9, 3);
            Console.WriteLine("Real Results are " + ans1 + " and " + ans2);
            if (ans1 ==20 && ans2 == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Main(String[] args)
        {
            Console.WriteLine("-------------- Test Harness Running ---------");
            TestDriver2 t = new TestDriver2();
            if (t.test())
            {
                Console.WriteLine("-------------- Test Passed ---------");
            }
            else
            {
                Console.WriteLine("-------------- Test Failed ---------");
            }
        }

    }
}
