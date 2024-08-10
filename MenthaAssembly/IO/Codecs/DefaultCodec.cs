using MenthaAssembly.Reflection;
using MenthaAssembly.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MenthaAssembly.IO
{
    internal unsafe class DefaultCodec : Codec
    {
        private static readonly Type TypeOfType = typeof(Type).GetType();
        private static readonly Type StringType = ReflectionHelper.TypeAlias["string"];
        private static readonly Type ObjectType = ReflectionHelper.TypeAlias["object"];
        private static readonly Type IEnumerableType = typeof(IEnumerable);
        private static readonly Type IEnumerableGenericType = typeof(IEnumerable<>);
        private static readonly Type IDictionaryType = typeof(IDictionary);
        private static readonly Type IDictionaryGenericType = typeof(IDictionary<,>);

        protected override void EncodeValue(Stream Stream, Type Type, object Value)
        {
            // Type
            if (Value is Type TypeValue)
            {
                EncodeType(Stream, TypeValue);
                return;
            }

            // String
            if (Type == StringType)
            {
                Stream.WriteStringAndLength((string)Value);
                return;
            }

            // Struct
            if (Type.IsStruct(true))
            {
                using PinnedIntPtr Handle = new(Value);
                IntPtr pValue = Handle.DangerousGetHandle();
                Stream.Write(pValue, Marshal.SizeOf(Value));
                return;
            }

            // Array
            if (Type.IsArray && Value is Array Array)
            {
                Type ElementType = Type.GetElementType();

                // Dimension
                int Dimension = Array.Rank;
                Stream.Write(Dimension);

                // Lengths
                for (int i = 0; i < Dimension; i++)
                    Stream.Write(Array.GetLength(i));

                // Elements
                if (ElementType == typeof(object))
                {
                    foreach (object Element in Array)
                        Encode(Stream, Element);
                }
                else
                {
                    foreach (object Element in Array)
                        EncodeValue(Stream, ElementType, Element);
                }
                return;
            }

            // Constructor && Params
            _ = GetConstructor(Type, out ValueAccessor[] Params);

            // Collection of parameterless constructors
            if (Params.Length == 0)
            {
                // IDictionary
                if (Value is IDictionary Dictionary)
                {
                    int Count = Dictionary.Count;
                    Stream.Write(Count);

                    if (Count != 0)
                    {
                        Action<Stream, object> KeyEncoder = Encode,
                                               ValueEncoder = Encode;

                        if (ReflectionHelper.TryGetInheritedGenericInterfaceType(Type, IDictionaryGenericType, out Type Inherited))
                        {
                            Type[] GenericTypes = Inherited.GetGenericArguments();
                            if (GenericTypes.Length != 2)
                                throw new InvalidDataException($"The generic member length of this type {Inherited} is invalid.");

                            if (GenericTypes[0] != ObjectType)
                                KeyEncoder = (s, k) => EncodeValue(Stream, GenericTypes[0], k);

                            if (GenericTypes[1] != ObjectType)
                                ValueEncoder = (s, v) => EncodeValue(Stream, GenericTypes[1], v);
                        }

                        foreach (object Key in Dictionary.Keys)
                        {
                            KeyEncoder(Stream, Key);
                            ValueEncoder(Stream, Dictionary[Key]);
                        }
                    }
                }

                // IEnumerable
                else if (Value is IEnumerable Enumerable)
                {
                    int Count = Enumerable.Count();
                    Stream.Write(Count);

                    if (TryGetSpecifiedMethodWithSingleParameter(Type, "Add", out MethodInfo Add, out Type[] AddParams) ||  // IList、ICollection<>
                        TryGetSpecifiedMethodWithSingleParameter(Type, nameof(Queue.Enqueue), out Add, out AddParams) ||    // Queue
                        TryGetSpecifiedMethodWithSingleParameter(Type, nameof(Stack.Push), out Add, out AddParams) ||       // Push
                        TryGetSpecifiedMethodWithSingleParameter(Type, "TryAdd", out Add, out AddParams))                   // IProducerConsumerCollection<>
                    {
                        Action<Stream, object> Encoder = AddParams[0] == ObjectType ? Encode : (s, v) => EncodeValue(Stream, AddParams[0], v);
                        foreach (object Item in Enumerable)
                            Encoder(Stream, Item);
                    }
                }
            }

            // Public Properties
            PropertyInfo[] PublicProperties = Type.GetProperties(ReflectionHelper.PublicModifier)
                                                  .Where(i => i.CanRead && i.CanWrite && !Params.Any(p => p.Name == i.Name) && i.GetIndexParameters().Length == 0)
                                                  .ToArray();

            // Total Members Count
            Stream.Write(Params.Length + PublicProperties.Length);

            foreach (ValueAccessor Accessor in Params)
            {
                Stream.WriteStringAndLength(Accessor.Name);
                Encode(Stream, Accessor.GetValue(Value));
            }

            foreach (PropertyInfo Property in PublicProperties)
            {
                Stream.WriteStringAndLength(Property.Name);
                Encode(Stream, Property.GetValue(Value));
            }
        }

        protected override object DecodeValue(Stream Stream, Type Type)
        {
            // Type
            if (TypeOfType == Type)
                return DecodeType(Stream);

            // String
            if (Type == StringType)
                return Stream.ReadStringAndLength();

            // Struct
            if (Type.IsStruct(true))
            {
                object Struct = Activator.CreateInstance(Type);

                int Size = Marshal.SizeOf(Struct);
                byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
                try
                {
                    if (!Stream.ReadBuffer(Buffer, 0, Size))
                        throw new OutOfMemoryException();

                    using PinnedIntPtr Handle = new(Struct);
                    IntPtr pStruct = Handle.DangerousGetHandle();
                    Marshal.Copy(Buffer, 0, pStruct, Size);
                    return Struct;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(Buffer);
                }
            }

            // Array
            if (Type.IsArray)
            {
                Type ElementType = Type.GetElementType();

                int Dimension = Stream.Read<int>();

                int[] Lengths = new int[Dimension];
                for (int i = 0; i < Dimension; i++)
                    Lengths[i] = Stream.Read<int>();

                Array Array = Array.CreateInstance(ElementType, Lengths);
                Func<Stream, object> Decoder = ElementType == typeof(object) ? Decode :
                                                                               s => DecodeValue(s, ElementType);

                void FillMultiDimArray(int DimensionIndex, int[] Index)
                {
                    if (DimensionIndex == Dimension)
                    {
                        object Element = Decoder(Stream);
                        Array.SetValue(Element, Index);
                        return;
                    }

                    for (int i = 0; i < Lengths[DimensionIndex]; i++)
                    {
                        Index[DimensionIndex] = i;
                        FillMultiDimArray(DimensionIndex + 1, Index);
                    }
                }

                FillMultiDimArray(0, new int[Dimension]);
                return Array;
            }

            // Constructor && Params
            ConstructorInfo Constructor = GetConstructor(Type, out ValueAccessor[] Params);

            object Result = null;

            // IEnumerable of parameterless constructors
            if (Params.Length == 0 &&
                Type.IsBaseOn(IEnumerableType))
            {
                Result = Activator.CreateInstance(Type);
                int Count = Stream.Read<int>();
                if (TryGetSpecifiedMethod(Type, IDictionaryGenericType, nameof(IDictionary.Add), 2, out MethodInfo Add, out Type[] AddParams) ||    // IDictionary<,>、
                    TryGetSpecifiedMethodWithSingleParameter(Type, "Add", out Add, out AddParams) ||                                                // IList、ICollection<> ||                                   
                    TryGetSpecifiedMethodWithSingleParameter(Type, nameof(Queue.Enqueue), out Add, out AddParams) ||                                // Queue
                    TryGetSpecifiedMethodWithSingleParameter(Type, nameof(Stack.Push), out Add, out AddParams) ||                                   // Stack         
                    TryGetSpecifiedMethodWithSingleParameter(Type, "TryAdd", out Add, out AddParams))                                               // IProducerConsumerCollection<>
                {
                    int Length = AddParams.Length;
                    Func<Stream, object>[] Decoders = new Func<Stream, object>[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        int Index = i;
                        Decoders[i] = AddParams[i] == ObjectType ? Decode : s => DecodeValue(Stream, AddParams[Index]);
                    }

                    object[] Parameters = new object[Length];
                    for (int i = 0; i < Count; i++)
                    {
                        for (int j = 0; j < Length; j++)
                            Parameters[j] = Decoders[j](Stream);

                        Add.Invoke(Result, Parameters);
                    }
                }
            }

            int MemberCount = Stream.Read<int>();

            Dictionary<string, object> Members = new(MemberCount);
            for (int i = 0; i < MemberCount; i++)
                Members.Add(Stream.ReadStringAndLength(), Decode(Stream));

            if (Result is null)
            {
                object[] ConstructorParams = new object[Params.Length];
                for (int i = 0; i < Params.Length; i++)
                    ConstructorParams[i] = Members[Params[i].Name];

                Result = Constructor.Invoke(ConstructorParams);
            }

            // Fill Properties
            IEnumerable<PropertyInfo> PublicProperties = Type.GetProperties(ReflectionHelper.PublicModifier)
                                                             .Where(i => i.CanRead && i.CanWrite && !Params.Any(p => p.Name == i.Name) && i.GetIndexParameters().Length == 0);
            foreach (PropertyInfo Property in PublicProperties)
                if (Members.TryGetValue(Property.Name, out object obj))
                    Property.SetValue(Result, obj);

            return Result;
        }

        private static bool TryGetSpecifiedMethod(Type Type, Type GenericInterface, string Name, int ParamsLength, out MethodInfo Method, out Type[] ParamTypes)
        {
            if (ReflectionHelper.TryGetInheritedGenericInterfaceType(Type, GenericInterface, out Type Inherited))
            {
                ParamTypes = Inherited.GetGenericArguments();
                if (Inherited.TryGetMethod(Name, ParamTypes, out Method))
                    return true;
            }

            ParamTypes = new Type[ParamsLength];
            for (int i = 0; i < ParamsLength; i++)
                ParamTypes[i] = ObjectType;

            if (Type.TryGetMethod(Name, ParamTypes, out Method))
                return true;

            Method = null;
            ParamTypes = null;
            return false;
        }
        private static bool TryGetSpecifiedMethodWithSingleParameter(Type Type, string Name, out MethodInfo Method, out Type[] ParamTypes)
        {
            (MethodInfo, ParameterInfo[])[] Methods = Type.GetDeclaredMethods()
                                                          .Where(i => i.Name == Name)
                                                          .Select(i => (i, i.GetParameters()))
                                                          .Where(i => i.Item2.Length == 1)
                                                          .ToArray();

            if (ReflectionHelper.TryGetInheritedGenericInterfaceType(Type, IEnumerableGenericType, out Type Inherited))
            {
                ParamTypes = Inherited.GetGenericArguments();

                MethodInfo ObjectInfo = null;
                foreach ((MethodInfo Info, ParameterInfo[] Params) in Methods)
                {
                    if (Params[0].ParameterType == ParamTypes[0])
                    {
                        Method = Info;
                        return true;
                    }

                    if (Params[0].ParameterType == ObjectType)
                        ObjectInfo = Info;
                }

                if (ObjectInfo != null)
                {
                    Method = ObjectInfo;
                    ParamTypes = [ObjectType];
                    return true;
                }
            }
            else if (Methods.FirstOrDefault(i => i.Item2[0].ParameterType == ObjectType) is (MethodInfo Info, ParameterInfo[] Params))
            {
                Method = Info;
                ParamTypes = [ObjectType];
                return true;
            }

            Method = null;
            ParamTypes = null;
            return false;
        }

        private static ConstructorInfo GetConstructor(Type Type, out ValueAccessor[] Accessors)
        {
            ConstructorInfo[] Constructors = Type.GetConstructors(ReflectionHelper.PublicModifier | ReflectionHelper.InternalModifier);

            FieldInfo[] Fields = null;
            PropertyInfo[] Properties = null;
            ConstructorInfo ZeroParamConstructor = null;
            Dictionary<ConstructorInfo, ParameterInfo[]> ParamConstructors = [];
            try
            {
                // CodecConstructorAttribute
                for (int i = 0; i < Constructors.Length; i++)
                {
                    ConstructorInfo Constructor = Constructors[i];
                    ParameterInfo[] Params = Constructor.GetParameters();

                    if (Params.Length == 0)
                        ZeroParamConstructor = Constructor;
                    else
                        ParamConstructors[Constructor] = Params;

                    if (Constructor.GetCustomAttribute<CodecConstructorAttribute>() is CodecConstructorAttribute Attribute)
                        return TryGetConstructorParameters(Type, Params, Attribute, ref Properties, ref Fields, out Accessors) ? Constructor :
                               throw new InvalidDataException($"Not found the constructor parameter from the member names set by {nameof(CodecConstructorAttribute)}.");
                }

                // ZeroParamConstructor
                if (ZeroParamConstructor != null)
                {
                    Accessors = [];
                    return ZeroParamConstructor;
                }

                foreach (KeyValuePair<ConstructorInfo, ParameterInfo[]> item in ParamConstructors)
                    if (TryGetConstructorParameters(Type, item.Value, ref Properties, ref Fields, out Accessors))
                        return item.Key;
            }
            finally
            {
                ZeroParamConstructor = null;
                ParamConstructors.Clear();
            }

            throw new NotSupportedException("Not support this type of decoding.\r\nTo ensure decoding is possible, implement a custom codec.");
        }
        private static bool TryGetConstructorParameters(Type Type, ParameterInfo[] Params, ref PropertyInfo[] Properties, ref FieldInfo[] Fields, out ValueAccessor[] Accessors)
        {
            Fields ??= Type.GetFields(ReflectionHelper.AllModifier);
            Properties ??= Type.GetProperties(ReflectionHelper.AllModifier);

            int Length = Params.Length;
            Accessors = new ValueAccessor[Length];
            for (int i = 0; i < Length; i++)
            {
                ParameterInfo Param = Params[i];
                string Name = Param.Name.ToLower();
                Type ParamType = Param.ParameterType;
                if (Properties.FirstOrDefault(p => p.PropertyType.IsBaseOn(ParamType) && p.Name.ToLower() == Name) is PropertyInfo Property)
                {
                    Accessors[i] = new PropertyAccessor(Property);
                    continue;
                }

                if (Fields.FirstOrDefault(f => f.FieldType.IsBaseOn(ParamType) && VerifyBackingField(f, Name)) is FieldInfo Field)
                {
                    Accessors[i] = new FieldAccessor(Field);
                    continue;
                }

                Accessors = null;
                return false;
            }

            return true;
        }
        private static bool TryGetConstructorParameters(Type Type, ParameterInfo[] Params, CodecConstructorAttribute Attribute, ref PropertyInfo[] Properties, ref FieldInfo[] Fields, out ValueAccessor[] Accessors)
        {
            string[] MemberNames = Attribute.MemberNames;
            int Length = Params.Length;
            if (Length != MemberNames.Length)
            {
                Accessors = null;
                return false;
            }

            if (Length == 0)
            {
                Accessors = Array.Empty<ValueAccessor>();
                return true;
            }

            Properties ??= Type.GetProperties(ReflectionHelper.AllModifier);
            Fields ??= Type.GetFields(ReflectionHelper.AllModifier);

            Accessors = new ValueAccessor[Length];
            for (int i = 0; i < Length; i++)
            {
                string Name = MemberNames[i];
                Type ParamType = Params[i].ParameterType;
                if (Properties.FirstOrDefault(p => p.PropertyType.IsBaseOn(ParamType) && p.Name == Name) is PropertyInfo Property)
                {
                    Accessors[i] = new PropertyAccessor(Property);
                    continue;
                }

                if (Fields.FirstOrDefault(p => p.FieldType.IsBaseOn(ParamType) && p.Name == Name) is FieldInfo Field)
                {
                    Accessors[i] = new FieldAccessor(Field);
                    continue;
                }

                Accessors = null;
                return false;
            }

            return true;
        }

        private static bool VerifyBackingField(FieldInfo Field, string TargetName)
        {
            string FieldName = Field.Name.ToLower();
            return FieldName == TargetName || FieldName == $"<{TargetName}>k__backingfield"; //(FieldName.EndsWith("backingfield") && FieldName.StartsWith($"<{TargetName}>"));
        }

    }
}