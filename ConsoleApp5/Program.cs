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

            Type instance = typeof(MyClass);
            var methodInfo = instance.GetMethod("Print2");
            var cls1 = Activator.CreateInstance(instance);

            Stopwatch w1 = new Stopwatch();
            w1.Start();
            for (int i = 0; i < count; i++)
            {
                object o1 = methodInfo?.Invoke(cls1, new object[] {"CCCCCCCCCCCCCCC"});
                container[i] = o1;
            }

            w1.Stop();
            Console.WriteLine(w1.Elapsed.ToString());
            Console.WriteLine("-------------------------------------");
            Thread.Sleep(1000);

            var creator = GetCreator(instance);
            var invoker = GetInvokers(instance, methodInfo);

            var cls2 = creator();

            Stopwatch w2 = new Stopwatch();
            w2.Start();

            for (int i = 0; i < count; i++)
            {
                //Type instance = typeof(MyClass);
                //var methodInfo = instance.GetMethod("Print2");

                //var creator = GetCreator(instance);
                //var invoker = GetInvokers(instance, methodInfo);
                //var cls2 = creator();
                object o1 = invoker(cls2, new Object[] {"CCCCCCCCCCCCCCC"});
                container[i] = o1;
            }

            w2.Stop();
            Console.WriteLine(w2.Elapsed.ToString());

            //Console.WriteLine("------------------多线程环境-------------------");
            //Console.WriteLine("-------------------------------------");
            //Thread.Sleep(1000);
            //w.Reset();
            //w.Start();
            //Parallel.For(0, count, (i) =>
            //{
            //    Type instance = typeof(MyClass);
            //    var methodInfo = instance.GetMethod("Print2");
            //    var cls = Activator.CreateInstance(instance);
            //    object o1 = methodInfo?.Invoke(cls, new object[] { "CCCCCCCCCCCCCCC" });
            //    container[i] = o1;
            //});
            //w.Stop();
            //Console.WriteLine(w.Elapsed.ToString());
            //Console.WriteLine("-------------------------------------");
            //w.Reset();
            //w.Start();
            //Parallel.For(0, count, (i) =>
            //{
            //    Type instance = typeof(MyClass);
            //    var methodInfo = instance.GetMethod("Print2");
            //    var cls = GetCreator(instance)();
            //    var invoker = GetInvokers(instance, methodInfo);
            //    object o1 = invoker(cls, new object[] { "CCCCCCCCCCCCCCC" });
            //    container[i] = o1;
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

                var cvtExpr = Expression.TypeAs(instanceExpr, instance);
                var callExpression = Expression.Call(cvtExpr, methodInfo, args);

                var lambda = Expression.Lambda<Func<object, object[], object>>(Expression.Convert(callExpression, typeof(object)), instanceExpr, argsExpr);
                invoker = lambda.Compile();
                TypeCache.TryAdd(methodInfo, invoker);
            }
            return invoker;
        }

        public static Func<object> GetCreator(Type instance)
        {
            if (!MethodInfoCache.TryGetValue(instance, out Func<object> creator))
            {
                DynamicMethod dm = new DynamicMethod("CreateObject", typeof(object), Type.EmptyTypes);
                ILGenerator il = dm.GetILGenerator();
                il.Emit(OpCodes.Newobj, instance.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);
                creator = (Func<object>) dm.CreateDelegate(typeof(Func<object>));
                MethodInfoCache.TryAdd(instance, creator);
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

        public string Print2(string a)
        {
            //StringBuilder builder = new StringBuilder(a);
            //builder.Append("asdasdasdasd");
            //return builder.ToString();
            return null;
        }

        public override string ToString()
        {
            return "My Class";
        }
    }
}