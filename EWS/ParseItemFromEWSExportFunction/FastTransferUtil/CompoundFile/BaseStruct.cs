using FTStreamUtil.Item;
using FTStreamUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using FastTransferUtil.CompoundFile.MsgStruct.Helper;

namespace FastTransferUtil.CompoundFile
{
    public abstract class BaseStruct
    {
        protected abstract string Name { get; }

        protected BaseStruct(BaseStruct parentStruct)
        {
            ParentStruct = parentStruct;
        }

        public Props Properties = new Props();

        protected virtual Dictionary<int, IPropValue> NamedProperties
        {
            get
            {
                return this.ParentStruct.NamedProperties;
            }
        }

        public virtual void AddProperties(IPropValue property)
        {
            Properties.AddProperty(property);
            if (property.PropInfo is NamePropInfo)
            {
                NamedProperties[property.PropTag.PropertyId] = property;
            }
        }

        public void Build()
        {
            bool hasError = true;
            try
            {
                BuildEx();
                hasError = false;
            }
            finally
            {
                Release(hasError);
            }
            //var storage = Storage;
            //CompoundFileUtil.Instance.ReleaseComObj(ref storage);
        }

        protected virtual void BuildEx()
        {
            BuildProperties();
        }

        protected abstract void Release(bool hasError);

        protected BaseStruct ParentStruct
        {
            get; set;
        }

        internal virtual IStorage Storage
        {
            get; set;
        }

        protected virtual void BuildProperties()
        {
            IStream propertyStream = null;
            try
            {
                propertyStream = CreatePropertyStream();
                BuildHeader(propertyStream);
                foreach (IPropValue property in Properties.Properties.Values)
                {
                    BuildEachProperty(Storage, propertyStream, property);
                }
                propertyStream.Commit(0);
            }
            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(propertyStream);
            }
        }

        protected virtual void BuildEachProperty(IStorage storage, IStream propertyStream, IPropValue property)
        {
            WritePropertyInfoToPropertyStream(propertyStream, property);
            WritePropertyValueToStorage(storage, property);
        }

        public static int GetPropertyTag(IPropTag propertyTag)
        {
            int tag = propertyTag.PropertyTag;
            return GetPropertyTag(tag);
        }

        public static int GetPropertyTag(int tag)
        {
            int propId = tag >> 16;
            if ((tag & 0x9000) == 0x9000)
            {
                tag = propId << 16 | (int)PropertyType.PT_MV_UNICODE;
            }
            else if ((tag & 0x8000) == 0x8000)
            {
                tag = propId << 16 | (int)PropertyType.PT_UNICODE;
            }
            return tag;
        }

        private void WritePropertyValueToStorage(IStorage storage, IPropValue property)
        {
            if (property is VarPropValue || PropertyTag.IsGuidType(property.PropTag) || property is SpecialVarBaseProperty)
            {
                var streamName = string.Format("__substg1.0_{0:X8}", GetPropertyTag(property.PropTag));
                IStream varPropStream = null;
                try
                {
                    varPropStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);
                    varPropStream.Write(ModifyPropertyValue(property));
                    varPropStream.Commit(0);
                }
                finally
                {
                    CompoundFileUtil.Instance.ReleaseComObj(varPropStream);
                }
            }
            else if (property is MvPropValue)
            {
                if (PropertyTag.IsMultiVarType((ushort)property.PropTag.PropertyTag)) // MVVarValue
                {
                    // write length stream
                    var streamName = string.Format("__substg1.0_{0:X8}", GetPropertyTag(property.PropTag));
                    IStream propStream = null;
                    try
                    {
                        propStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);

                        if (property.PropTag.PropertyTag == (int)PropertyType.PT_MV_BINARY)
                        {
                            foreach (var item in (MvVarSizeValue)property.PropValue)
                            {
                                MvVarSizeItem mvItem = item as MvVarSizeItem;

                                propStream.Write(mvItem.GetValueLength());
                                propStream.WriteZero(4);
                            }
                        }
                        else if (property.PropTag.PropertyTag == (int)PropertyType.PT_MV_STRING8 || ((property.PropTag.PropertyTag & 0x9000) == 0x9000))
                        {
                            foreach (var item in (MvVarSizeValue)property.PropValue)
                            {
                                MvVarSizeItem mvItem = item as MvVarSizeItem;
                                propStream.Write(mvItem.GetValueLength());
                            }
                        }
                        else
                            throw new ArgumentException("Mv propertyTag is wrong.");

                        propStream.Commit(0);
                    }
                    finally
                    {
                        CompoundFileUtil.Instance.ReleaseComObj(propStream);
                    }

                    // write value stream
                    int itemIndex = 0;
                    foreach (var item in (MvVarSizeValue)property.PropValue)
                    {
                        MvVarSizeItem mvItem = item as MvVarSizeItem;
                        string valueStreamName = string.Format("__substg1.0_{0:X8}-{1:X8}", GetPropertyTag(property.PropTag), itemIndex);
                        IStream valueStream = null;
                        try
                        {
                            valueStream = CompoundFileUtil.Instance.GetChildStream(valueStreamName, true, Storage);
                            valueStream.Write(mvItem.GetValueBytes());
                            valueStream.Commit(0);
                        }
                        finally
                        {
                            CompoundFileUtil.Instance.ReleaseComObj(valueStream);
                        }
                        itemIndex++;
                    }
                }
                else
                {
                    var streamName = string.Format("__substg1.0_{0:X8}", property.PropTag.PropertyTag);
                    IStream propStream = null;
                    try
                    {
                        propStream = CompoundFileUtil.Instance.GetChildStream(streamName, true, storage);
                        propStream.Write(((MvFixedSizeValue)property.PropValue).GetItemValueByte());
                        propStream.Commit(0);
                    }
                    finally
                    {
                        CompoundFileUtil.Instance.ReleaseComObj(propStream);
                    }
                }
            }
        }

        protected virtual byte[] ModifyPropertyValue(IPropValue property)
        {
            return property.PropValue.BytesForMsg;
        }

        protected virtual void WritePropertyInfoToPropertyStream(IStream propertyStream, IPropValue property)
        {
            if ((property is FixPropValue && !PropertyTag.IsGuidType(property.PropTag)) || (property is SpecialFixProperty))
            {
                // 1.2.1 Set fixed data
                // 1.2.1.1 Set Tag
                propertyStream.Write(property.PropTag.Bytes);
                // 1.2.1.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.1.3 Set Value
                var value = ModifyPropertyValue(property);
                propertyStream.Write(value);
                propertyStream.WriteZero(8 - value.Length);
            }
            else if((property is SpecialFixProperty) && property.PropTag.PropertyType == (short)PropertyType.PT_OBJECT)
            {
                // 1.2.1 Set fixed data
                // 1.2.1.1 Set Tag
                propertyStream.Write(property.PropTag.Bytes);
                // 1.2.1.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.1.3 Set Value
                var value = ModifyPropertyValue(property);
                propertyStream.Write(value);
                propertyStream.WriteZero(8 - value.Length);
            }
            else if (property is VarPropValue || PropertyTag.IsGuidType(property.PropTag) || property is SpecialVarBaseProperty)
            {
                // 1.2.2 Set variable data
                // 1.2.2.1 Set Tag
                var tag = GetPropertyTag(property.PropTag);
                propertyStream.Write(tag);
                // 1.2.2.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.2.3 Set Size
                var valueSize = property.PropValue.BytesCountForMsg;
                //if (property.PropValue is VarStringValue)
                //{

                //    if (valueSize == 0)
                //        propertyStream.Write((Int32)2);
                //    else
                //        propertyStream.Write(valueSize);
                //}
                //else
                //{
                propertyStream.Write(valueSize);
                //}
                // 1.2.2.4 Set Reserve , todo if contain embed message, it must set 0x01, if OLE, set 0x04;
                propertyStream.WriteZero(4);
            }
            else if (property is MvPropValue)
            {
                // 1.2.3 Set variable data
                // 1.2.3.1 Set Tag
                var tag = GetPropertyTag(property.PropTag);
                propertyStream.Write(tag);
                // 1.2.3.2 Set Flag
                propertyStream.Write((Int32)0x06);
                // 1.2.3.3 Set Size, todo must check the length is all value's bytes total length;
                if (PropertyTag.IsMultiVarType((ushort)property.PropTag.PropertyType))
                {
                    propertyStream.Write((Int32)(((ISizeValue)property.PropValue).ItemCount) * 4);
                }
                else
                {
                    propertyStream.Write((Int32)(((ISizeValue)property.PropValue).ItemCount) * PropertyTag.GetFixPropertyTypeLength((ushort)property.PropTag.PropertyType));
                }
                // 1.2.3.4 Set Reserve , todo if contain embed message, it must set 0x01, if OLE, set 0x04;
                propertyStream.WriteZero(4);
            }
        }

        protected virtual void BuildHeader(IStream propertyStream)
        {
            throw new NotImplementedException();
        }

        protected virtual IStream CreatePropertyStream()
        {
            return CompoundFileUtil.Instance.GetChildStream("__properties_version1.0", true, Storage);
        }

        private string _string;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(64);
            var item = this;
            while (item != null)
            {
                sb.Insert(0, item.Name);
                sb.Insert(0, "->");
                item = item.ParentStruct;
            }
            return sb.ToString();
        }
    }

    public class Props
    {
        public Dictionary<uint, IPropValue> Properties = new Dictionary<uint, IPropValue>();
        public void AddProperty(IPropValue property)
        {
            var tag = BaseStruct.GetPropertyTag(property.PropTag);

            if (property is ISpecialValue && Properties.ContainsKey((uint)tag))
            {
                return;    
            }
            Properties[(uint)tag] = property;
        }

        internal IPropValue GetProperty(int tag)
        {
            tag = BaseStruct.GetPropertyTag(tag);
            return Properties[(uint)tag];
        }

        internal bool ContainProperty(int tag)
        {
            return Properties.ContainsKey((uint)tag);
        }
    }
}
