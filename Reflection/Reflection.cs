// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

#if WindowsCE
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ArrayCopier = System.Array;
#endif

namespace More
{
    // The FixedSerializationLength is -1 if the object's serialization length can change,
    // otherwise, it represents the objects fixed serialization length
    public interface ISerializer
    {
        UInt32 FixedSerializationLength();

        UInt32 SerializationLength();
        UInt32 Serialize(Byte[] bytes, UInt32 offset);

        UInt32 Deserialize(Byte[] bytes, UInt32 offset, UInt32 offsetLimit);

        void DataString(StringBuilder builder);
        void DataSmallString(StringBuilder builder);
    }
    public delegate ISerializer Deserializer(Byte[] bytes, Int32 offset, Int32 offsetLimit);

    // An IReflector is a serializer that serializes values directly to and from CSharp fields using reflection
    public interface IReflector
    {
        UInt32 FixedSerializationLength();

        UInt32 SerializationLength(Object instance);
        UInt32 Serialize(Object instance, Byte[] array, UInt32 offset);

        UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit);

        void DataString(Object instance, StringBuilder builder);
        void DataSmallString(Object instance, StringBuilder builder);
    }
    /*
    public interface IGenericReflector<T>
    {
        Int32 FixedSerializationLength();

        Int32 SerializationLength(T instance);
        Int32 Serialize(T instance, Byte[] array, Int32 offset);

        Int32 Deserialize(T instance, Byte[] array, Int32 offset, Int32 offsetLimit);

        void DataString(T instance, StringBuilder builder);
        void DataSmallString(T instance, StringBuilder builder);
    }
    */

    public interface IInstanceSerializer<T>
    {
        UInt32 SerializationLength(T instance);
        UInt32 Serialize(Byte[] bytes, UInt32 offset, T instance);
        UInt32 Deserialize(Byte[] bytes, UInt32 offset, UInt32 offsetLimit, out T instance);
        void DataString(T instance, StringBuilder builder);
        void DataSmallString(T instance, StringBuilder builder);
    }
    public class VoidInstanceSerializer : IInstanceSerializer<Object>
    {
        static VoidInstanceSerializer instance;
        public static VoidInstanceSerializer Instance
        {
            get
            {
                if (instance == null) instance = new VoidInstanceSerializer();
                return instance;
            }
        }
        private VoidInstanceSerializer() { }
        public UInt32 SerializationLength(Object instance)
        { return 0; }
        public UInt32 Serialize(byte[] bytes, uint offset, Object instance)
        { return offset; }
        public UInt32 Deserialize(byte[] bytes, uint offset, uint offsetLimit, out Object instance)
        { instance = null; return offset; }
        public void DataString(Object instance, StringBuilder builder) { }
        public void DataSmallString(Object instance, StringBuilder builder) { }
    }
    public abstract class FixedLengthInstanceSerializer<T> : IInstanceSerializer<T>
    {
        public readonly UInt32 fixedSerializationLength;
        protected FixedLengthInstanceSerializer()
        {
            this.fixedSerializationLength = FixedSerializationLength();
        }

        public abstract UInt32 FixedSerializationLength();
        public abstract void FixedLengthSerialize(Byte[] bytes, UInt32 offset, T instance);
        public abstract T FixedLengthDeserialize(Byte[] bytes, UInt32 offset);

        public UInt32 SerializationLength(T instance)
        {
            return fixedSerializationLength;
        }
        public UInt32 Serialize(byte[] bytes, UInt32 offset, T instance)
        {
            FixedLengthSerialize(bytes, offset, instance);
            return offset + fixedSerializationLength;
        }
        public UInt32 Deserialize(byte[] bytes, UInt32 offset, UInt32 offsetLimit, out T instance)
        {
            instance = FixedLengthDeserialize(bytes, offset);
            return offset + fixedSerializationLength;
        }
        public abstract void DataString(T instance, StringBuilder builder);
        public abstract void DataSmallString(T instance, StringBuilder builder);

        public T[] FixedLengthDeserializeArray(Byte[] bytes, UInt32 offset, UInt32 elementCount)
        {
            T[] array = new T[elementCount];
            for (uint i = 0; i < array.Length; i++)
            {
                array[i] = FixedLengthDeserialize(bytes, offset);
                offset += fixedSerializationLength;
            }
            return array;
        }
        public void FixedLengthDeserializeArray(Byte[] bytes, UInt32 offset, T[] array)
        {
            for (uint i = 0; i < array.Length; i++)
            {
                array[i] = FixedLengthDeserialize(bytes, offset);
                offset += fixedSerializationLength;
            }
        }
    }


    public class InstanceSerializerAdapter<T> : ISerializer
    {
        readonly IInstanceSerializer<T> serializer;
        public T instance;
        public InstanceSerializerAdapter(IInstanceSerializer<T> serializer, T instance)
        {
            this.serializer = serializer;
            this.instance = instance;
        }
        public UInt32 FixedSerializationLength() { return UInt32.MaxValue; }
        public UInt32 SerializationLength() { return serializer.SerializationLength(instance); }
        public UInt32 Serialize(byte[] bytes, UInt32 offset) { return serializer.Serialize(bytes, offset, instance); }
        public UInt32 Deserialize(byte[] bytes, UInt32 offset, UInt32 offsetLimit)
        { return serializer.Deserialize(bytes, offset, offsetLimit, out instance); }
        public void DataString(StringBuilder builder) { serializer.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { serializer.DataSmallString(instance, builder); }
    }
    public class FixedLengthInstanceSerializerAdapter<T> : ISerializer
    {
        readonly FixedLengthInstanceSerializer<T> serializer;
        public T instance;
        public FixedLengthInstanceSerializerAdapter(FixedLengthInstanceSerializer<T> serializer, T instance)
        {
            this.serializer = serializer;
            this.instance = instance;
        }
        public UInt32 FixedSerializationLength() { return serializer.fixedSerializationLength; }
        public UInt32 SerializationLength() { return serializer.fixedSerializationLength; }
        public UInt32 Serialize(byte[] bytes, UInt32 offset)
        {
            serializer.FixedLengthSerialize(bytes, offset, instance);
            return offset + serializer.fixedSerializationLength;
        }
        public UInt32 Deserialize(byte[] bytes, UInt32 offset, UInt32 offsetLimit)
        {
            instance = serializer.FixedLengthDeserialize(bytes, offset);
            return offset + serializer.fixedSerializationLength;
        }
        public void DataString(StringBuilder builder) { serializer.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { serializer.DataSmallString(instance, builder); }
    }
    public class FixedLengthInstanceSerializerToReflectorAdapter<T> : IReflector
    {
        readonly FixedLengthInstanceSerializer<T> serializer;
        public FixedLengthInstanceSerializerToReflectorAdapter(FixedLengthInstanceSerializer<T> serializer)
        {
            this.serializer = serializer;
        }
        public UInt32 FixedSerializationLength() { return serializer.FixedSerializationLength(); }
        public UInt32 SerializationLength(Object instance)
        {
            return serializer.SerializationLength((T)instance);
        }
        public UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            return serializer.Serialize(array, offset, (T)instance);
        }
        public UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            T output;
            offset = serializer.Deserialize(array, offset, offsetLimit, out output);
            instance = output;
            return offset;
        }
        public void DataString(Object instance, StringBuilder builder)
        {
            serializer.DataString((T)instance, builder);
        }
        public void DataSmallString(Object instance, StringBuilder builder)
        {
            serializer.DataSmallString((T)instance, builder);
        }
    }





    //public delegate T FixedLengthDeserializer<T>(Byte[] bytes, Int32 offset);
    //public delegate T DynamicLengthDeserializer<T>(Byte[] bytes, Int32 offset, Int32 offsetLimit, out Int32 newOffset);

    /*
    public class FixedLengthReflectorInstanceSerializer<T> : FixedLengthInstanceSerializer<T>
    {
        readonly IReflector reflector;
        FixedLengthDeserializer<T> deserializer;
        readonly Int32 fixedSerializationLength;
        public FixedLengthReflectorInstanceSerializer(IReflector reflector, FixedLengthDeserializer<T> deserializer)
        {
            this.reflector = reflector;
            this.deserializer = deserializer;
            fixedSerializationLength = reflector.FixedSerializationLength();
            if (fixedSerializationLength == UInt32.MaxValue) throw new InvalidOperationException(
                 "This class is only for reflectors with FixedSerializationLength");
        }
        public int FixedSerializationLength()                          { return fixedSerializationLength; }
        public void Serialize(byte[] bytes, int offset, T instance)    { reflector.Serialize(instance, bytes, offset); }
        public void DataString(T instance, StringBuilder builder)      { reflector.DataString(instance, builder); }
        public void DataSmallString(T instance, StringBuilder builder) { reflector.DataSmallString(instance, builder); }
        public T Deserialize(byte[] array, int offset)
        {
            return deserializer(array, offset);
        }
    }
    public abstract class DynamicLengthSerializer<T> : IDynamicLengthSerializer<T>
    {
        readonly IReflector reflector;
        public DynamicLengthSerializer(IReflector reflector)
        {
            this.reflector = reflector;
        }
        public int SerializationLength(T instance)                 { return reflector.SerializationLength(instance); }
        public int Serialize(byte[] array, int offset, T instance) { return reflector.Serialize(instance, array, offset); }
        public void DataString(T instance, StringBuilder builder)
        {
            reflector.DataString(instance, builder);
        }
        public void DataSmallString(T instance, StringBuilder builder)
        {
            reflector.DataSmallString(instance, builder);
        }

        public abstract int Deserialize(byte[] array, int offset, int offsetLimit, out T outInstance);
    }
    */

    public static class DataStringBuilder
    {
        /*
        public static String DataString(ISerializer serializer)
        {
            StringBuilder builder = new StringBuilder();
            serializer.DataString(builder);
            return builder.ToString();
        }
        public static String DataSmallString(ISerializer serializer)
        {
            StringBuilder builder = new StringBuilder();
            serializer.DataSmallString(builder);
            return builder.ToString();
        }
        public static String DataString(IReflector reflector, Object instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataString(instance, builder);
            return builder.ToString();
        }
        public static String DataSmallString(IReflector reflector, Object instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataSmallString(instance, builder);
            return builder.ToString();
        }
        public static String DataString<T>(IInstanceSerializer<T> instanceSerializer, T instance)
        {
            StringBuilder builder = new StringBuilder();
            instanceSerializer.DataString(instance, builder);
            return builder.ToString();
        }
        public static String DataSmallString<T>(IInstanceSerializer<T> instanceSerializer, T instance)
        {
            StringBuilder builder = new StringBuilder();
            instanceSerializer.DataSmallString(instance, builder);
            return builder.ToString();
        }
        */
        public static String DataString(this ISerializer serializer, StringBuilder builder)
        {
            builder.Length = 0;
            serializer.DataString(builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
        public static String DataSmallString(this ISerializer serializer, StringBuilder builder)
        {
            builder.Length = 0;
            serializer.DataSmallString(builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
        public static String DataString(this IReflector reflector, Object instance, StringBuilder builder)
        {
            builder.Length = 0;
            reflector.DataString(instance, builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
        public static String DataSmallString(this IReflector reflector, Object instance, StringBuilder builder)
        {
            builder.Length = 0;
            reflector.DataSmallString(instance, builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
        public static String DataString<T>(this IInstanceSerializer<T> instanceSerializer, T instance, StringBuilder builder)
        {
            builder.Length = 0;
            instanceSerializer.DataString(instance, builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
        public static String DataSmallString<T>(this IInstanceSerializer<T> instanceSerializer, T instance, StringBuilder builder)
        {
            builder.Length = 0;
            instanceSerializer.DataSmallString(instance, builder);
            String dataString = builder.ToString();
            builder.Length = 0;
            return dataString;
        }
    }

    public class ReflectorToSerializerAdapater : ISerializer
    {
        protected readonly IReflector reflector;
        readonly Object instance;
        readonly UInt32 fixedSerializationLength;

        public ReflectorToSerializerAdapater(IReflector reflector, Object instance)
        {
            this.reflector = reflector;
            this.instance = instance;
            this.fixedSerializationLength = reflector.FixedSerializationLength();
        }
        public UInt32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public UInt32 SerializationLength()
        {
            return reflector.SerializationLength(instance);
        }
        public UInt32 Serialize(Byte[] array, UInt32 offset)
        {
            return reflector.Serialize(instance, array, offset);
        }
        public UInt32 Deserialize(Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            return reflector.Deserialize(instance, array, offset, offsetLimit);
        }
        public void DataString(StringBuilder builder) { reflector.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { reflector.DataSmallString(instance, builder); }
    }
    public class ReflectorToInstanceSerializerAdapater<T> : IInstanceSerializer<T>
    {
        public delegate T Constructor();

        protected readonly IReflector reflector;
        readonly Constructor constructor;
        public ReflectorToInstanceSerializerAdapater(IReflector reflector, Constructor constructor)
        {
            this.reflector = reflector;
            this.constructor = constructor;
        }
        public uint SerializationLength(T instance)
        {
            return reflector.SerializationLength(instance);
        }
        public uint Serialize(byte[] bytes, uint offset, T instance)
        {
            return reflector.Serialize(instance, bytes, offset);
        }
        public uint Deserialize(byte[] bytes, uint offset, uint offsetLimit, out T instance)
        {
            instance = constructor();
            return reflector.Deserialize(instance, bytes, offset, offsetLimit);
        }
        public void DataString(T instance, StringBuilder builder) { reflector.DataString(instance, builder); }
        public void DataSmallString(T instance, StringBuilder builder) { reflector.DataSmallString(instance, builder); }
    }
    public class Reflectors : IReflector
    {
        public readonly IReflector[] reflectors;
        public readonly UInt32 fixedSerializationLength;

        public Reflectors(params IReflector[] reflectors)
        {
            this.reflectors = reflectors;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                UInt32 fieldFixedSerializationLength = reflectors[i].FixedSerializationLength();
                if (fieldFixedSerializationLength == UInt32.MaxValue)
                {
                    this.fixedSerializationLength = UInt32.MaxValue; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        //
        // Create Reflectors with circular references
        //
        public delegate IReflector ReflectorCreator(Reflectors theseReflectors);
        public Reflectors(IReflector[] reflectors, ReflectorCreator nullReflectorCreator)
        {
            this.reflectors = reflectors;
            this.fixedSerializationLength = UInt32.MaxValue;

            Boolean foundNull = false;

            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector reflector = reflectors[i];
                if (reflector == null)
                {
                    if (foundNull) throw new InvalidOperationException("This constructor requires one and only one IReflector to be null but found more than one");
                    foundNull = true;
                    reflector = nullReflectorCreator(this);
                    if (reflector == null) throw new InvalidOperationException("The null reflector creator you provided returned null");
                    reflectors[i] = reflector;
                }
            }

            if (!foundNull) throw new InvalidOperationException("This constructor requires that one of the reflectors is null but none were null");
        }

        public UInt32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public UInt32 SerializationLength(Object instance)
        {
            if (fixedSerializationLength != UInt32.MaxValue) return fixedSerializationLength;

            UInt32 length = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Deserialize(instance, array, offset, offsetLimit);
            }
            return offset;
        }
        public void DataString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataString(instance, builder);
            }
        }
        public void DataSmallString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataSmallString(instance, builder);
            }
        }
    }


    /*
    public class ListReflector<T>
    {
        public delegate UInt32 Length();
        public delegate Array GetArray();

        readonly Length elementCountMethod;
        readonly IReflector[] elementReflectors;

        UInt32 fixedElementSerializationLength;

        public ListReflector(Length elementCountMethod, params IReflector[] elementReflectors)
        {
            if (elementCountMethod == null) throw new ArgumentNullException("elementCountMethod");
            if (elementReflectors == null) throw new ArgumentNullException("elementReflectors");

            this.elementCountMethod = elementCountMethod;
            this.elementReflectors = elementReflectors;

            this.fixedElementSerializationLength = 0;
            for (int i = 0; i < elementReflectors.Length; i++)
            {
                UInt32 fieldFixedSerializationLength = elementReflectors[i].FixedSerializationLength();
                if (fieldFixedSerializationLength == UInt32.MaxValue)
                {
                    this.fixedElementSerializationLength = UInt32.MaxValue; // length is not fixed
                    return;
                }
                this.fixedElementSerializationLength += fieldFixedSerializationLength;
            }

        }
        public UInt32 FixedSerializationLength()
        {
            return UInt32.MaxValue;
        }
        public UInt32 SerializationLength(Object instance)
        {
            UInt32 elementCount = elementCountMethod();
            if(fixedElementSerializationLength != UInt32.MaxValue)
            {
                return elementCount * fixedElementSerializationLength;
            }







            UInt32 length = 0;
            for(int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Deserialize(instance, array, offset, offsetLimit);
            }
            return offset;
        }
        public void DataString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataString(instance, builder);
            }
        }
        public void DataSmallString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataSmallString(instance, builder);
            }
        }
    }
    */

    public class VoidSerializer : ISerializer
    {
        private static VoidSerializer instance = null;
        public static VoidSerializer Instance
        {
            get
            {
                if (instance == null) instance = new VoidSerializer();
                return instance;
            }
        }
        private VoidSerializer() { }
        public UInt32 FixedSerializationLength() { return 0; }

        public UInt32 SerializationLength() { return 0; }
        public UInt32 Serialize(Byte[] array, UInt32 offset) { return offset; }

        public UInt32 Deserialize(Byte[] array, UInt32 offset, UInt32 offsetLimit) { return offset; }

        public void DataString(StringBuilder builder) { builder.Append("<void>"); }
        public void DataSmallString(StringBuilder builder) { builder.Append("<void>"); }
    }
    public class VoidReflector : IReflector
    {
        private static VoidReflector instance = null;
        private static IReflector[] reflectorsArrayInstance = null;
        private static Reflectors reflectorsInstance = null;
        public static VoidReflector Instance
        {
            get
            {
                if (instance == null) instance = new VoidReflector();
                return instance;
            }
        }
        public static IReflector[] ReflectorsArray
        {
            get
            {
                if (reflectorsArrayInstance == null) reflectorsArrayInstance = new IReflector[] { Instance };
                return reflectorsArrayInstance;
            }
        }
        public static Reflectors Reflectors
        {
            get
            {
                if (reflectorsInstance == null) reflectorsInstance = new Reflectors(ReflectorsArray);
                return reflectorsInstance;
            }
        }
        private VoidReflector() { }
        public UInt32 FixedSerializationLength() { return 0; }
        public UInt32 SerializationLength(Object instance) { return 0; }
        public UInt32 Serialize(Object instance, Byte[] array, UInt32 offset) { return offset; }
        public UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit) { return offset; }
        public void DataString(Object instance, StringBuilder builder) { builder.Append("<void>"); }
        public void DataSmallString(Object instance, StringBuilder builder) { builder.Append("<void>"); }
    }
    public class SubclassSerializer : ISerializer
    {
        readonly IReflector[] reflectors;
        protected readonly UInt32 fixedSerializationLength;

        public SubclassSerializer(Reflectors reflectors)
        {
            this.reflectors = reflectors.reflectors;
            this.fixedSerializationLength = reflectors.fixedSerializationLength;
        }
        public UInt32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public UInt32 SerializationLength()
        {
            if (fixedSerializationLength != UInt32.MaxValue) return fixedSerializationLength;

            UInt32 length = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(this);
            }
            return length;
        }
        public UInt32 Serialize(Byte[] array, UInt32 offset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Serialize(this, array, offset);
            }
            return offset;
        }
        public UInt32 Deserialize(Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(this, array, offset, offsetLimit);
            }
            return offset;
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataString(this, builder);
            }
            builder.Append("]");
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataSmallString(this, builder);
            }
            builder.Append("]");
        }
    }


    public interface ISerializerCreator
    {
        ISerializer CreateSerializer();
    }
    public class SerializerFromObjectAndReflectors : ISerializer
    {
        readonly Object instance;
        protected readonly IReflector[] reflectors;
        protected readonly UInt32 fixedSerializationLength;

        public SerializerFromObjectAndReflectors(Object instance, Reflectors reflectors)
        {
            this.instance = instance;
            this.reflectors = reflectors.reflectors;
            this.fixedSerializationLength = reflectors.fixedSerializationLength;
        }
        public UInt32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public UInt32 SerializationLength()
        {
            if (fixedSerializationLength != UInt32.MaxValue) return fixedSerializationLength;

            UInt32 length = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public UInt32 Serialize(Byte[] array, UInt32 offset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Serialize(instance, array, offset);
            }
            return offset;
        }
        public UInt32 Deserialize(Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(instance, array, offset, offsetLimit);
            }
            return offset;
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append(instance.GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataString(instance, builder);
            }
            builder.Append("]");
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append(instance.GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataSmallString(instance, builder);
            }
            builder.Append("]");
        }
    }
    public abstract class ClassFieldReflector : IReflector
    {
        public readonly FieldInfo fieldInfo;
        protected ClassFieldReflector(Type classThatHasThisField, String fieldName)
        {
            this.fieldInfo = classThatHasThisField.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (this.fieldInfo == null) throw new InvalidOperationException(String.Format(
                    "The class you provided '{0}' does not have the field name you provided '{1}'",
                    classThatHasThisField.Name, fieldName));
        }
        protected ClassFieldReflector(Type classThatHasThisField, String fieldName, Type expectedFieldType)
            : this(classThatHasThisField, fieldName)
        {
            if (fieldInfo.FieldType != expectedFieldType)
                throw new InvalidOperationException(String.Format(
                    "In the class '{0}' field '{1}', you specified the expected type to be '{2}' but it is actually '{3}",
                    classThatHasThisField.Name, fieldName, expectedFieldType.FullName, fieldInfo.FieldType.FullName));
        }
        public abstract UInt32 FixedSerializationLength();
        public abstract UInt32 SerializationLength(Object instance);
        public abstract UInt32 Serialize(Object instance, Byte[] array, UInt32 offset);
        public abstract UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit);
        public abstract void DataString(Object instance, StringBuilder builder);
        public virtual void DataSmallString(Object instance, StringBuilder builder)
        {
            DataString(instance, builder);
        }
    }
    public class ClassFieldReflectors<FieldType> : ClassFieldReflector /* where FieldType : new() */
    {
        private IReflector[] fieldReflectors;
        private UInt32 fixedSerializationLength;

        public ClassFieldReflectors(Type classThatHasThisField, String fieldName, Reflectors fieldReflectors)
            : base(classThatHasThisField, fieldName)
        {
            this.fieldReflectors = fieldReflectors.reflectors;
            this.fixedSerializationLength = fieldReflectors.fixedSerializationLength;
        }
        public override UInt32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public override UInt32 SerializationLength(Object instance)
        {
            if (fixedSerializationLength != UInt32.MaxValue) return fixedSerializationLength;

            Object structInstance = fieldInfo.GetValue(instance);

            UInt32 length = 0;
            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                length += fieldReflectors[i].SerializationLength(structInstance);
            }
            return length;
        }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            Object structInstance = (instance == null) ? null : fieldInfo.GetValue(instance);

            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                IReflector serializer = fieldReflectors[i];
                offset = serializer.Serialize(structInstance, array, offset);
            }
            return offset;
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            Object structObject = FormatterServices.GetUninitializedObject(typeof(FieldType));
            //FieldType structObject = new FieldType();

            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                IReflector serializer = fieldReflectors[i];
                offset = serializer.Deserialize(structObject, array, offset, offsetLimit);
            }

            fieldInfo.SetValue(instance, structObject);

            return offset;
        }
        public override void DataString(Object instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            Object structInstance = fieldInfo.GetValue(instance);

            if (structInstance == null)
            {
                builder.Append("<null>");
                return;
            }
            builder.Append("{");
            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");

                fieldReflectors[i].DataString(structInstance, builder);
            }
            builder.Append("}");
        }
        public override void DataSmallString(Object instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            Object structInstance = fieldInfo.GetValue(instance);

            if (structInstance == null)
            {
                builder.Append("<null>");
                return;
            }
            builder.Append("{");
            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");

                fieldReflectors[i].DataSmallString(structInstance, builder);
            }
            builder.Append("}");
        }
    }
    public class PartialByteArraySerializer : ISerializer
    {
        private static PartialByteArraySerializer nullInstance = null;
        public static PartialByteArraySerializer Null
        {
            get
            {
                if (nullInstance == null) nullInstance = new PartialByteArraySerializer(null, 0, 0);
                return nullInstance;
            }
        }

        public Byte[] bytes;
        public UInt32 offset, length;
        public PartialByteArraySerializer()
        {
        }
        public PartialByteArraySerializer(Byte[] bytes, UInt32 offset, UInt32 length)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.length = length;
            if (offset + length > bytes.Length) throw new ArgumentOutOfRangeException();
        }
        public UInt32 FixedSerializationLength()
        {
            return UInt32.MaxValue; // length is 
        }
        public UInt32 SerializationLength()
        {
            if (bytes == null) return 0;
            return length;
        }
        public UInt32 Serialize(Byte[] array, UInt32 offset)
        {
            if (this.bytes == null) return offset;
            ArrayCopier.Copy(this.bytes, this.offset, array, offset, this.length);
            return offset + this.length;
        }
        public UInt32 Deserialize(Byte[] array, UInt32 offset, UInt32 offsetLimit)
        {
            UInt32 length = offsetLimit - offset;

            if (length <= 0)
            {
                this.bytes = null;
                this.offset = 0;
                this.length = 0;
                return offset;
            }

            this.bytes = new Byte[length];
            ArrayCopier.Copy(array, offset, this.bytes, 0, length);

            this.offset = 0;
            this.length = length;

            return offset + length;
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append((bytes == null) ? "<null>" : bytes.ToHexString((Int32)offset, (Int32)length));
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append((bytes == null) ? "<null>" : ((bytes.Length <= 10) ?
                BitConverter.ToString(bytes) : String.Format("[{0} bytes]", length)));
        }
    }

    /*
    public class SerializableDataFieldReflector : ClassFieldReflector
    {
        public SerializableDataFieldReflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName)
        {
            if (!typeof(ISerializer).IsAssignableFrom(fieldInfo.FieldType))
            {
                throw new InvalidOperationException(String.Format(
                    "A generic reflector serializer can only be used on fields that implement the ISerializer interface.  The field you are using '{0} {1}' does not implement this interface",
                    fieldInfo.FieldType.Name, fieldInfo.Name));
            }
        }
        public override UInt32 FixedSerializationLength()
        {
            return -1; // There's not really a way to tell if this type has a fixed length
        }
        private ISerializer GetValue(ISerializer instance)
        {
            if (instance == null) throw new InvalidOperationException(String.Format("The Serializer Class '{0}' cannot be null", instance.GetType().Name));

            ISerializer value = (ISerializer)fieldInfo.GetValue(instance);

            if (value == null) throw new InvalidOperationException(String.Format("The value of field '{0} {1}' cannot be null for any serialization methods using this serializer",
                fieldInfo.FieldType.Name, fieldInfo.Name));

            return value;
        }
        public override UInt32 SerializationLength(Object instance)
        {
            return GetValue(instance).SerializationLength();
        }
        public override UInt32 Serialize(Object instance, Byte[] array, UInt32 offset)
        {
            return GetValue(instance).Serialize(array, offset);
        }
        public override UInt32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            return GetValue(instance).Deserialize(array, offset, maxOffset);
        }
        public override String ToNiceString(Object instance)
        {
            return GetValue(instance).ToNiceString();
        }
        public override String ToNiceSmallString(Object instance)
        {
            return GetValue(instance).ToNiceSmallString();
        }
    }
    */
}
