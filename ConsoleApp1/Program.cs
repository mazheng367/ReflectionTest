using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //object section = ConfigurationManager.GetSection("mySection");
            var m1 = Type.GetType("ConsoleApp1.MyClass,ConsoleApp1").GetMethod("Add");
            var m2 = Type.GetType("ConsoleApp1.MyClass2,ConsoleApp1").GetMethod("Add");

            Console.WriteLine(m1 == m2);

            //Console.WriteLine($"{type.FullName},{type.Assembly.GetName().Name}");
            Console.ReadLine();
        }
    }

    public enum MyEnum
    {
        SqlServer = 0,
        Oracle = 1
    }

    public class DbConfig : ConfigurationSection
    {
        [ConfigurationProperty("element")]
        public DbConfigElement Element
        {
            get => (DbConfigElement) base["element"];
            set => base["element"] = value;
        }

        [ConfigurationProperty("system")]
        public MyEnum System
        {
            get => (MyEnum) base["system"];
            set => base["system"] = value;
        }

        [ConfigurationProperty("days")]
        [TypeConverter(typeof(CusTypeConverter))]
        public int[] Days
        {
            get => (int[]) base["days"];
            set => base["days"] = value;
        }
    }

    public class Conv2 : TypeConverter
    {
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return base.CreateInstance(context, propertyValues);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            TypeNameElement element = new TypeNameElement();
            element.Type = Type.GetType(value.ToString());
            return element;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return base.CanConvertTo(ctx, type);
        }
    }

    public class CusTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {

            return value.ToString().Split(',').Select(item => int.Parse(item)).ToArray();
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }
    }

    public class DbConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("t1")]
        public TypeNameElement T1
        {
            get => (TypeNameElement) base["t1"];
            set => base["t1"] = value;
        }

        [ConfigurationProperty("t2")]
        public TypeNameElement T2
        {
            get => (TypeNameElement) base["t2"];
            set => base["t2"] = value;
        }

        [ConfigurationProperty("t3",DefaultValue = "")]
        public TypeNameElement T3
        {
            get => (TypeNameElement) base["t3"];
            set => base["t3"] = value;
        }
    }

    public class TypeNameElement : ConfigurationElement
    {
        [ConfigurationProperty("type")]
        [TypeConverter(typeof(TypeNameConverter))]
        public Type Type
        {
            get => (Type) base["type"];
            set => base["type"] = value;
        }
    }

    public class MyClass
    {
        public string Add()
        {
            return string.Empty;
        }
    }

    public class MyClass2
    {
        public string Add()
        {
            return string.Empty;
        }
    }
}
