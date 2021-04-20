using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Orcus.Shared.NetSerializer.TypeSerializers;

namespace Orcus.Shared.NetSerializer
{
    /// <summary>
    ///     Used for serializing data. Taken from https://github.com/tomba/netserializer
    /// </summary>
    //https://github.com/tomba/netserializer
    public class Serializer
    {
        private readonly Dictionary<Type, ushort> _typeIdMap;

        private delegate void SerializerSwitch(Serializer serializer, Stream stream, object ob);

        private delegate void DeserializerSwitch(Serializer serializer, Stream stream, out object ob);

        private SerializerSwitch _serializerSwitch;
        private DeserializerSwitch _deserializerSwitch;

        private static readonly ITypeSerializer[] TypeSerializers =
        {
            new ObjectSerializer(),
            new PrimitivesSerializer(),
            new ArraySerializer(),
            new EnumSerializer(),
            new DictionarySerializer(),
            new GenericSerializer()
        };

        private readonly ITypeSerializer[] _userTypeSerializers;

        /// <summary>
        ///     Initialize NetSerializer
        /// </summary>
        /// <param name="rootTypes">Types to be (de)serialized</param>
        public Serializer(IEnumerable<Type> rootTypes)
            : this(rootTypes, new ITypeSerializer[0])
        {
        }

        /// <summary>
        ///     Initialize NetSerializer
        /// </summary>
        /// <param name="rootType">Type to be (de)serialized</param>
        public Serializer(Type rootType)
            : this(new[] {rootType}, new ITypeSerializer[0])
        {
        }

        /// <summary>
        ///     Initialize NetSerializer
        /// </summary>
        /// <param name="rootTypes">Types to be (de)serialized</param>
        /// <param name="userTypeSerializers">Array of custom serializers</param>
        public Serializer(IEnumerable<Type> rootTypes, ITypeSerializer[] userTypeSerializers)
        {
            if (userTypeSerializers.All(s => s is IDynamicTypeSerializer || s is IStaticTypeSerializer) == false)
                throw new ArgumentException(
                    "TypeSerializers have to implement IDynamicTypeSerializer or  IStaticTypeSerializer");

            _userTypeSerializers = userTypeSerializers;

            var typeDataMap = GenerateTypeData(rootTypes);

            GenerateDynamic(typeDataMap);

            _typeIdMap = typeDataMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TypeId);

#if GENERATE_DEBUGGING_ASSEMBLY
    // Note: GenerateDebugAssembly overwrites some fields from typeDataMap
			GenerateDebugAssembly(typeDataMap);
#endif
        }

        /// <summary>
        ///     Serialize the given object to the stream
        /// </summary>
        /// <param name="stream">The stream the data should be serialized to</param>
        /// <param name="data">The object which should get serialized</param>
        public void Serialize(Stream stream, object data)
        {
            _serializerSwitch(this, stream, data);
        }

        /// <summary>
        ///     Deserialize the object from the stream
        /// </summary>
        /// <param name="stream">The stream which contains the object</param>
        /// <returns>The deserialized object</returns>
        public object Deserialize(Stream stream)
        {
            object o;
            _deserializerSwitch(this, stream, out o);
            return o;
        }

        /// <summary>
        ///     Deserialize the given byte array
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="bytes">The byte array which contains the serialized object</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(bytes, 0);
        }

        /// <summary>
        ///     Deserialize the given byte array, starting add the <see cref="index" />
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="bytes">The byte array which contains the serialized object</param>
        /// <param name="index">The start index</param>
        /// <returns>The deserialized object</returns>
        public T Deserialize<T>(byte[] bytes, int index)
        {
            object o;
            using (var ms = new MemoryStream(bytes, index, bytes.Length - index))
                _deserializerSwitch(this, ms, out o);
            return (T) o;
        }

        /// <summary>
        ///     Deserialize the given byte array
        /// </summary>
        /// <param name="bytes">The byte array which contains the serialized object</param>
        /// <returns>The deserialized object</returns>
        public object Deserialize(byte[] bytes)
        {
            return Deserialize(bytes, 0);
        }

        /// <summary>
        ///     Deserialize the given byte array, starting add the <see cref="index" />
        /// </summary>
        /// <param name="bytes">The byte array which contains the serialized object</param>
        /// <param name="index">The start index</param>
        /// <returns>The deserialized object</returns>
        public object Deserialize(byte[] bytes, int index)
        {
            object o;
            using (var ms = new MemoryStream(bytes, index, bytes.Length - index))
                _deserializerSwitch(this, ms, out o);
            return o;
        }

        /// <summary>
        ///     Serialize the object to a byte array
        /// </summary>
        /// <param name="data">The object to serialize</param>
        /// <returns>The serialized object</returns>
        public byte[] Serialize(object data)
        {
            using (var ms = new MemoryStream())
            {
                _serializerSwitch(this, ms, data);
                return ms.ToArray();
            }
        }

        public static byte[] FastSerialize<T>(T @object)
        {
            return new Serializer(typeof (T)).Serialize(@object);
        }

        public static T FastDeserialize<T>(byte[] data)
        {
            return new Serializer(typeof (T)).Deserialize<T>(data, 0);
        }

        public static T FastDeserialize<T>(byte[] data, int index)
        {
            return new Serializer(typeof (T)).Deserialize<T>(data, index);
        }

        private Dictionary<Type, TypeData> GenerateTypeData(IEnumerable<Type> rootTypes)
        {
            var map = new Dictionary<Type, TypeData>();
            var stack = new Stack<Type>(PrimitivesSerializer.GetSupportedTypes().Concat(rootTypes));

            stack.Push(typeof (object));

            // TypeID 0 is reserved for null
            ushort typeId = 1;

            while (stack.Count > 0)
            {
                var type = stack.Pop();

                if (map.ContainsKey(type))
                    continue;

                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (type.ContainsGenericParameters)
                    throw new NotSupportedException($"Type {type.FullName} contains generic parameters");

                var serializer = _userTypeSerializers.FirstOrDefault(h => h.Handles(type)) ??
                                 TypeSerializers.FirstOrDefault(h => h.Handles(type));

                if (serializer == null)
                    throw new NotSupportedException($"No serializer for {type.FullName}");

                foreach (var t in serializer.GetSubtypes(type))
                    stack.Push(t);

                TypeData typeData;

                var typeSerializer = serializer as IStaticTypeSerializer;
                if (typeSerializer != null)
                {
                    var sts = typeSerializer;

                    MethodInfo writer;
                    MethodInfo reader;

                    sts.GetStaticMethods(type, out writer, out reader);

                    Debug.Assert(writer != null && reader != null);

                    typeData = new TypeData(typeId++, writer, reader);
                }
                else
                {
                    var dynamicTypeSerializer = serializer as IDynamicTypeSerializer;
                    if (dynamicTypeSerializer != null)
                    {
                        var dts = dynamicTypeSerializer;

                        typeData = new TypeData(typeId++, dts);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                map[type] = typeData;
            }

            return map;
        }

        private void GenerateDynamic(Dictionary<Type, TypeData> map)
        {
            /* generate stubs */
            foreach (var kvp in map)
            {
                var type = kvp.Key;
                var td = kvp.Value;

                if (!td.IsGenerated)
                    continue;

                td.WriterMethodInfo = Helpers.GenerateDynamicSerializerStub(type);
                td.ReaderMethodInfo = Helpers.GenerateDynamicDeserializerStub(type);
            }

            var ctx = new CodeGenContext(map);

            /* generate bodies */

            foreach (var kvp in map)
            {
                var type = kvp.Key;
                var td = kvp.Value;

                if (!td.IsGenerated)
                    continue;

                var writerDm = (DynamicMethod) td.WriterMethodInfo;
                td.TypeSerializer.GenerateWriterMethod(type, ctx, writerDm.GetILGenerator());

                var readerDm = (DynamicMethod) td.ReaderMethodInfo;
                td.TypeSerializer.GenerateReaderMethod(type, ctx, readerDm.GetILGenerator());
            }

            var writer = (DynamicMethod) ctx.GetWriterMethodInfo(typeof (object));
            var reader = (DynamicMethod) ctx.GetReaderMethodInfo(typeof (object));

            _serializerSwitch = (SerializerSwitch) writer.CreateDelegate(typeof (SerializerSwitch));
            _deserializerSwitch = (DeserializerSwitch) reader.CreateDelegate(typeof (DeserializerSwitch));
        }

#if GENERATE_DEBUGGING_ASSEMBLY
		static void GenerateDebugAssembly(Dictionary<Type, TypeData> map)
		{
			var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("NetSerializerDebug"), AssemblyBuilderAccess.RunAndSave);
			var modb = ab.DefineDynamicModule("NetSerializerDebug.dll");
			var tb = modb.DefineType("NetSerializer", TypeAttributes.Public);

			/* generate stubs */
			foreach (var kvp in map)
			{
				var type = kvp.Key;
				var td = kvp.Value;

				if (!td.IsGenerated)
					continue;

				td.WriterMethodInfo = Helpers.GenerateStaticSerializerStub(tb, type);
				td.ReaderMethodInfo = Helpers.GenerateStaticDeserializerStub(tb, type);
			}

			var ctx = new CodeGenContext(map);

			/* generate bodies */

			foreach (var kvp in map)
			{
				var type = kvp.Key;
				var td = kvp.Value;

				if (!td.IsGenerated)
					continue;

				var writerMb = (MethodBuilder)td.WriterMethodInfo;
				td.TypeSerializer.GenerateWriterMethod(type, ctx, writerMb.GetILGenerator());

				var readerMb = (MethodBuilder)td.ReaderMethodInfo;
				td.TypeSerializer.GenerateReaderMethod(type, ctx, readerMb.GetILGenerator());
			}

			tb.CreateType();
			ab.Save("NetSerializerDebug.dll");
		}
#endif

        /* called from the dynamically generated code */
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private ushort GetTypeID(object ob)
        {
            ushort id;

            if (ob == null)
                return 0;

            var type = ob.GetType();

            if (_typeIdMap.TryGetValue(type, out id) == false)
                throw new InvalidOperationException($"Unknown type {type.FullName}");

            return id;
        }
    }
}