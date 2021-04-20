/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orcus.Shared.NetSerializer
{
    public sealed class TypeData
    {
        public TypeData(ushort typeId, IDynamicTypeSerializer serializer)
        {
            TypeId = typeId;
            TypeSerializer = serializer;

            NeedsInstanceParameter = true;
        }

        public TypeData(ushort typeId, MethodInfo writer, MethodInfo reader)
        {
            TypeId = typeId;
            WriterMethodInfo = writer;
            ReaderMethodInfo = reader;

            NeedsInstanceParameter = writer.GetParameters().Length == 3;
        }

        public readonly ushort TypeId;
        public bool IsGenerated => TypeSerializer != null;
        public readonly IDynamicTypeSerializer TypeSerializer;
        public MethodInfo WriterMethodInfo;
        public MethodInfo ReaderMethodInfo;

        public bool NeedsInstanceParameter { get; private set; }
    }

    public sealed class CodeGenContext
    {
        private readonly Dictionary<Type, TypeData> _typeMap;

        public CodeGenContext(Dictionary<Type, TypeData> typeMap)
        {
            _typeMap = typeMap;

            var td = _typeMap[typeof (object)];
            SerializerSwitchMethodInfo = td.WriterMethodInfo;
            DeserializerSwitchMethodInfo = td.ReaderMethodInfo;
        }

        public MethodInfo SerializerSwitchMethodInfo { get; private set; }
        public MethodInfo DeserializerSwitchMethodInfo { get; private set; }

        public MethodInfo GetWriterMethodInfo(Type type)
        {
            return _typeMap[type].WriterMethodInfo;
        }

        public MethodInfo GetReaderMethodInfo(Type type)
        {
            return _typeMap[type].ReaderMethodInfo;
        }

        public bool IsGenerated(Type type)
        {
            return _typeMap[type].IsGenerated;
        }

        public IDictionary<Type, TypeData> TypeMap => _typeMap;

        private bool CanCallDirect(Type type)
        {
            // We can call the (De)serializer method directly for:
            // - Value types
            // - Array types
            // - Sealed types with static (De)serializer method, as the method will handle null
            // Other reference types go through the (De)serializerSwitch

            bool direct;

            if (type.IsValueType || type.IsArray)
                direct = true;
            else if (type.IsSealed && IsGenerated(type) == false)
                direct = true;
            else
                direct = false;

            return direct;
        }

        public TypeData GetTypeData(Type type)
        {
            return _typeMap[type];
        }

        public TypeData GetTypeDataForCall(Type type)
        {
            bool direct = CanCallDirect(type);
            if (!direct)
                type = typeof (object);

            return GetTypeData(type);
        }
    }
}