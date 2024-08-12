#if NET5_0_OR_GREATER
using System.Runtime.Loader;
#endif
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MenthaAssembly.IO
{
    public abstract class Codec
    {
        protected abstract void EncodeValue(Stream Stream, Type Type, object Value);

        protected abstract object DecodeValue(Stream Stream, Type Type);

        private static readonly Codec Default = new DefaultCodec();
        private static readonly Dictionary<Type, Codec> Instances;          // CodecType , Codec
        private static readonly Dictionary<Type, Codec> CacheMap = [];      // TargetType, Codec
        static Codec()
        {
            Instances = AppDomain.CurrentDomain.GetAssemblies()
                                               .TrySelectMany(i => i.GetTypes())
                                               .Where(IsInheritedCodec)
                                               .ToDictionary(t => t, t => (Codec)Activator.CreateInstance(t));

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

        }
        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs e)
        {
            foreach (Type t in e.LoadedAssembly.GetTypes().Where(IsInheritedCodec))
                Instances[t] = (Codec)Activator.CreateInstance(t);
        }

        private static readonly Type CodecType = typeof(Codec);
        private static bool IsInheritedCodec(Type Type)
            => CodecType.IsAssignableFrom(Type) && Type.IsClass && !Type.IsAbstract && Type.GetConstructor(Type.EmptyTypes) != null;

        /// <summary>
        /// Encodes the specified object using the default codec or the codec set from <see cref="CodecAttribute"/>.
        /// </summary>
        /// <param name="Stream">The specified stream for putting encoded data.</param>
        /// <param name="Value">The specified object to encode.</param>
        public static void Encode(Stream Stream, object Value)
        {
            Type Type = Value?.GetType();
            EncodeType(Stream, Type);

            if (Value is null)
                return;

            Codec Codec = GetCodec(Type);
            Codec.EncodeValue(Stream, Type, Value);
        }
        /// <summary>
        /// Encodes the specified object using the specified codec.
        /// </summary>
        /// <param name="Stream">The specified stream for putting encoded data.</param>
        /// <param name="Value">The specified object to encode.</param>
        /// <param name="Codec">The specified codec</param>
        public static void Encode(Stream Stream, object Value, Codec Codec)
        {
            Type Type = Value?.GetType();
            EncodeType(Stream, Type);

            if (Value is null)
                return;

            Codec.EncodeValue(Stream, Type, Value);
        }

        /// <summary>
        /// Decodes a object from the specified stream using the default codec or the codec set from <see cref="CodecAttribute"/>.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        public static object Decode(Stream Stream)
        {
            Type Type = DecodeType(Stream);
            if (Type is null)
                return null;

            Codec Codec = GetCodec(Type);
            return Codec.DecodeValue(Stream, Type);
        }
        /// <summary>
        /// Decodes a object from the specified stream using the specified decoder.
        /// </summary>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Codec"></param>
        public static object Decode(Stream Stream, Codec Codec)
        {
            Type Type = DecodeType(Stream);
            return Type is null ? null : Codec.DecodeValue(Stream, Type);
        }

        /// <summary>
        /// Decodes a object of the specified type from the specified stream using the default codec or the codec set from <see cref="CodecAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <param name="Stream">The specified stream.</param>
        public static T Decode<T>(Stream Stream)
        {
            Type Type = DecodeType(Stream),
                 ValueType = typeof(T);
            if (Type is null)
                return ValueType.IsStruct(false) ? throw new InvalidCastException($"The specified {ValueType.Name} type does not support null value.") :
                                                   default;

            if (!Type.IsBaseOn(ValueType))
                throw new InvalidCastException($"The decoded object ({Type.Name} type) can't be converted to the specified {ValueType.Name} type.");

            Codec Codec = GetCodec(Type);
            return (T)Codec.DecodeValue(Stream, Type);
        }
        /// <summary>
        /// Decodes a object of the specified type from the specified stream using the specified decoder.
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Codec">The specified codec</param>
        public static T Decode<T>(Stream Stream, Codec Codec)
        {
            Type Type = DecodeType(Stream),
                 ValueType = typeof(T);
            if (Type is null)
                return ValueType.IsStruct(false) ? throw new InvalidCastException($"The specified {ValueType.Name} type does not support null value.") :
                                                   default;

            object Result = Codec.DecodeValue(Stream, Type);
            if (Result is null)
                return ValueType.IsStruct(false) ? throw new InvalidCastException($"The specified {ValueType.Name} type does not support null value.") :
                                                   default;

            Type = Result.GetType();
            return !Type.IsBaseOn(ValueType)
                ? throw new InvalidCastException($"The decoded object ({Type.Name} type) can't be converted to the specified {ValueType.Name} type.")
                : (T)Result;
        }
        /// <summary>
        /// Decodes a object of the specified type from the specified stream using the specified decoder.
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <param name="Stream">The specified stream.</param>
        /// <param name="Codec">The specified codec</param>
        public static T Decode<T>(Stream Stream, Codec<T> Codec)
        {
            Type Type = DecodeType(Stream),
                 ValueType = typeof(T);
            return Type is null
                ? ValueType.IsStruct(false) ? throw new InvalidCastException($"The specified {ValueType.Name} type does not support null value.") :
                                                   default
                : (T)Codec.DecodeValue(Stream, Type);
        }

        private static Codec GetCodec(Type Type)
        {
            if (!CacheMap.TryGetValue(Type, out Codec Codec))
            {
                if (Type.GetCustomAttribute<CodecAttribute>() is CodecAttribute Attribute &&
                    !Instances.TryGetValue(Attribute.CodecType, out Codec))
                {
                    Codec = (Codec)Activator.CreateInstance(Type);
                    Instances[Type] = Codec;
                }

                CacheMap[Type] = Codec;
            }

            return Codec ?? Default;
        }

        protected static void EncodeType(Stream Stream, Type Type)
        {
            if (Type is null)
            {
                Stream.WriteStringAndLength(null);
                return;
            }

            Stream.WriteStringAndLength(Type.Assembly.GetName().Name, Encoding.UTF8);

            // Generic
            if (Type.IsGenericType)
            {
                // Define Type
                Type DefineType = Type.GetGenericTypeDefinition();
                Stream.WriteStringAndLength(DefineType.FullName);

                // Generic Types
                Type[] GenericTypes = Type.GetGenericArguments();
                Stream.Write(GenericTypes.Length);
                foreach (Type t in GenericTypes)
                    EncodeType(Stream, t);

                return;
            }

            Stream.WriteStringAndLength(Type.FullName);
        }
        protected static Type DecodeType(Stream Stream)
        {
            string AssemblyName = Stream.ReadStringAndLength(Encoding.UTF8);
            if (string.IsNullOrEmpty(AssemblyName))
                return null;

            Assembly Assembly =
#if NET5_0_OR_GREATER
                AssemblyLoadContext.All.SelectMany(i => i.Assemblies)
#else
                AppDomain.CurrentDomain.GetAssemblies()
#endif
                                       .FirstOrDefault(i => i.GetName().Name == AssemblyName);

            string CodecName = Stream.ReadStringAndLength(Encoding.UTF8);
            Type DefineType = Assembly.GetType(CodecName);

            // Generic
            if (DefineType.IsGenericType)
            {
                int GenericLength = Stream.Read<int>();
                Type[] GenericTypes = new Type[GenericLength];
                for (int i = 0; i < GenericLength; i++)
                    GenericTypes[i] = DecodeType(Stream);

                return DefineType.MakeGenericType(GenericTypes);
            }

            return DefineType;
        }

    }

    public abstract class Codec<T> : Codec
    {
        protected sealed override void EncodeValue(Stream Stream, Type Type, object Value)
        {
            if (Value is not T Data)
                throw new ArgumentException(nameof(Value));

            EncodeValue(Stream, Type, Data);
        }

        protected abstract void EncodeValue(Stream Stream, Type Type, T Value);

    }

}