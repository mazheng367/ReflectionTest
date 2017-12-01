using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            int times = 1000000;
            Program program = new Program();
            object[] parameters = new object[] { new object(), new object(), new object() };
            program.Call(null, null, null); // force JIT-compile

            Stopwatch watch1 = new Stopwatch();
            watch1.Start();
            for (int i = 0; i < times; i++)
            {
                program.Call(parameters[0], parameters[1], parameters[2]);
            }
            watch1.Stop();
            Console.WriteLine(watch1.Elapsed + " (Directly invoke)");

            MethodInfo methodInfo = typeof(Program).GetMethod("Call");
            Stopwatch watch2 = new Stopwatch();
            watch2.Start();
            for (int i = 0; i < times; i++)
            {
                methodInfo.Invoke(program, parameters);
            }
            watch2.Stop();
            Console.WriteLine(watch2.Elapsed + " (Reflection invoke)");

            DynamicMethodExecutor executor = new DynamicMethodExecutor(methodInfo);
            Stopwatch watch3 = new Stopwatch();
            watch3.Start();
            for (int i = 0; i < times; i++)
            {
                executor.Execute(program, parameters);
            }
            watch3.Stop();
            Console.WriteLine(watch3.Elapsed + " (Dynamic executor)");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void Call(object o1, object o2, object o3)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("asdadsads").Append("asdasdasdasd");
        }
    }
}
