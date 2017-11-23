using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    class Program
    {
        delegate T CreateObject<out T>();

        private static object o = DateTime.Now;

        static void Main(string[] args)
        {

            int count = 1000000;
            object[] container = new object[count];
            Stopwatch w = new Stopwatch();
            w.Start();
            for (int i = 0; i < count; i++)
            {
                Type instance = typeof(MyClass);
                var methodInfo = instance.GetMethod("Print2");
                var cls = Activator.CreateInstance(instance);
                object o1 = methodInfo?.Invoke(cls, new object[] {"CCCCCCCCCCCCCCC"});
                container[i] = o1;
            }

            w.Stop();
            Console.WriteLine(w.Elapsed.ToString());
            Console.WriteLine("-------------------------------------");
            Thread.Sleep(1000);
            w.Reset();
            w.Start();

            for (int i = 0; i < count; i++)
            {
                Type instance = typeof(MyClass);
                var methodInfo = instance.GetMethod("Print2");
                var cls = GetCreator(instance)();
                var invoker = GetInvokers(instance, methodInfo);
                object o1 = invoker(cls, new object[] {"CCCCCCCCCCCCCCC"});
                container[i] = o1;
            }

            w.Stop();
            Console.WriteLine(w.Elapsed.ToString());

            Console.WriteLine("-------------------------------------");
            Thread.Sleep(1000);
            w.Reset();
            //w.Start();
            //Parallel.For(0, 10000000, (i) =>
            //{
            //    Type instance = typeof(MyClass);
            //    var methodInfo = instance.GetMethod("Print2");
            //    //Type instance = typeof(MyClass);
            //    //var methodInfo = instance.GetMethod("Print2");
            //    //var invokers = GetInvokers(instance, methodInfo);
            //    var cls = new MyClass();
            //    object o1 = cls.Print2("CCCCCCCCCCCCCCC");
            //});
            //w.Stop();
            //Console.WriteLine(w.Elapsed.ToString());

            Console.ReadLine();
        }

        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> TypeCache = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
        private static readonly ConcurrentDictionary<Type, Func<object>> MethodInfoCache = new ConcurrentDictionary<Type, Func<object>>();

        public static Func<object, object[], object> GetInvokers(Type instance, MethodInfo methodInfo)
        {
            if (instance == null || methodInfo == null)
            {
                return null;
            }
            if (!TypeCache.TryGetValue(methodInfo, out var invoker))
            {
                ParameterExpression instanceExpr = Expression.Parameter(typeof(object), "instance");
                ParameterExpression argsExpr = Expression.Parameter(typeof(object[]), "args");
                Expression[] args = methodInfo.GetParameters().Select((paramInfo, index) =>
                {
                    var indexExpression = Expression.ArrayAccess(argsExpr, Expression.Constant(index));
                    return paramInfo.ParameterType.IsValueType ? Expression.Convert(indexExpression, paramInfo.ParameterType) : Expression.TypeAs(indexExpression, paramInfo.ParameterType);
                }).ToArray();

                var cvtExpr = Expression.Convert(instanceExpr, instance);
                var callExpression = Expression.Call(cvtExpr, methodInfo, args);

                var lambda = Expression.Lambda<Func<object, object[], object>>(Expression.Convert(callExpression, typeof(object)), instanceExpr, argsExpr);
                invoker = lambda.Compile();
                TypeCache.AddOrUpdate(methodInfo, invoker, (k, v) => invoker);
            }
            return invoker;
        }

        public static Func<object> GetCreator(Type instance)
        {
            if (!MethodInfoCache.TryGetValue(instance, out Func<object> creator))
            {
                DynamicMethod dm = new DynamicMethod("", typeof(object), Type.EmptyTypes);
                ILGenerator il = dm.GetILGenerator();
                il.Emit(OpCodes.Newobj, instance.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);
                creator = (Func<object>) dm.CreateDelegate(typeof(Func<object>));
                MethodInfoCache.AddOrUpdate(instance, creator, (k, v) => creator);
            }
            return creator;
        }
    }

    public class MyClass
    {
        public MyClass()
        {
            
        }
        public MyClass(string a, int b, object o)
        {
            this.T = a;
        }

        public string T { get; set; }

        public void Print(string a, int b, ref object o)
        {
            
        }
        public int Print2(string a)
        {
            return 20;
        }
        public override string ToString()
        {
            return "My Class";
        }
    }
}