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
    
    class TestDriver1:ITest
    {
        public bool test()
        {
            Console.WriteLine("Testing testfile1 and testfile2");
            TestFile1 obj1 = new TestFile1();
            TestFile2 obj2 = new TestFile2();
            Console.WriteLine("\nTesting for 1+4 and 5-3");
            Console.WriteLine("Expected Results are 5 and 2");
            int ans1 = obj1.add(1, 4);
            int ans2 = obj2.minus(5, 3);
            Console.WriteLine("Real Results are " + ans1 + " and " + ans2);
            if (ans1==5 && ans2==2)
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
            TestDriver1 t = new TestDriver1();
            if(t.test())
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
