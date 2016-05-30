using Arcserve.Exchange.FastTransferUtil.Item;
using Arcserve.Exchange.FastTransferUtil.Item.PropValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Arcserve.Exchange.FastTransferUtil.CompoundFile.MsgStruct.Helper;
using Arcserve.Exchange.FastTransferUtil.FTStream;

namespace Arcserve.Exchange.FastTransferUtil.CompoundFile
{
    public abstract class BaseStruct : ICompareMsg
    {
        protected abstract string Name { get; }

        protected virtual bool isParser {
            get; set; }

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

        protected virtual Dictionary<uint, uint> NamedPropIdToChangedId
        {
            get
            {
                return this.ParentStruct.NamedPropIdToChangedId;
            }
        }

        #region Build Struct
        public virtual void AddProperties(IPropValue property)
        {
            if (property.PropInfo is NamePropInfo)
            {
                uint changedId = 0;
                var dic = NamedPropIdToChangedId;
                if (!dic.TryGetValue(property.PropTag.PropertyTag, out changedId))
                {
                    changedId = (uint)dic.Count + (uint)0x8000;
                    changedId = (changedId << 16) | property.PropTag.PropertyType;
                    dic.Add(property.PropTag.PropertyTag, changedId);
                }

                ModifyPropertyTag(property, changedId);

                NamedProperties[property.PropTag.PropertyId] = property;
            }
            Properties.AddProperty(property);
        }

        private void ModifyPropertyTag(IPropValue property, uint changedId)
        {
            property.PropTag = new PropertyTag((uint)changedId);
        }

        protected virtual void CreateStorageForBuild()
        {
            if(Storage == null)
            {
                ParentStruct.CreateStorageForBuild();
                CreateSelfStorageForBuild();
            }
        }

        protected virtual void CreateSelfStorageForBuild()
        {

        }

        public void Build()
        {
            bool hasError = true;
            try
            {
                CreateStorageForBuild();
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

        public static uint GetPropertyTag(IPropTag propertyTag)
        {
            uint tag = propertyTag.PropertyTag;
            return GetPropertyTag(tag);
        }

        public static uint GetPropertyTag(uint tag)
        {
            ushort propId = (ushort)(tag >> 16);
            if ((tag & 0x9000) == 0x9000)
            {
                tag = (uint)(propId << 16) | (uint)PropertyType.PT_MV_UNICODE;
            }
            else if ((tag & 0x8000) == 0x8000)
            {
                tag = (uint)(propId << 16 )| (uint)PropertyType.PT_UNICODE;
            }
            return tag;
        }

        internal static string GetPropertyStreamName(IPropTag propTag)
        {
            return string.Format("__substg1.0_{0:X8}", GetPropertyTag(propTag));
        }

        internal static string GetMvVarPropertyStreamName(IPropTag propTag, int index)
        {
            return string.Format("__substg1.0_{0:X8}-{1:X8}", GetPropertyTag(propTag), index);
        }

        private void WritePropertyValueToStorage(IStorage storage, IPropValue property)
        {
            if (property is VarPropValue || PropertyTag.IsGuidType(property.PropTag) || property is SpecialVarBaseProperty)
            {
                var streamName = GetPropertyStreamName(property.PropTag); // string.Format("__substg1.0_{0:X8}", GetPropertyTag(property.PropTag));
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
                    var streamName = GetPropertyStreamName(property.PropTag); //string.Format("__substg1.0_{0:X8}", GetPropertyTag(property.PropTag));
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
                        string valueStreamName = GetMvVarPropertyStreamName(property.PropTag, itemIndex); // string.Format("__substg1.0_{0:X8}-{1:X8}", GetPropertyTag(property.PropTag), itemIndex);
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
                    var streamName = GetPropertyStreamName(property.PropTag); // string.Format("__substg1.0_{0:X8}", property.PropTag.PropertyTag);
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

        protected virtual int ModifyPropertyLength(IPropValue property)
        {
            return property.PropValue.BytesCountForMsg;
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
                var valueSize = ModifyPropertyLength(property);
                //var valueSize = property.PropValue.BytesCountForMsg;
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
            return CompoundFileUtil.Instance.GetChildStream(GetPropertyHeaderStreamName(), true, Storage);
        }
        #endregion

        protected string GetPropertyHeaderStreamName()
        {
            return "__properties_version1.0";
        }

        #region Parser CompoundFile

        public virtual void Parser()
        {
            bool hasError = true;
            isParser = true;
            try {
                ParserEx();
                hasError = false;
            }
            finally
            {
                Release(hasError);
            }
        }

        protected virtual void ParserEx()
        {
            GetStorageForParser();
            ParseProperty();
        }

        protected abstract void GetStorageForParser();

        protected virtual void ParseProperty()
        {
            IStream propertyHeaderStream = null;
            try {
                propertyHeaderStream = CompoundFileUtil.Instance.GetChildStream(GetPropertyHeaderStreamName(), false, Storage);
                int readCount = 0;
                ParserHeader(propertyHeaderStream, ref readCount);
                ParserProperties(Storage, propertyHeaderStream, ref readCount);
            }
            finally
            {
                CompoundFileUtil.Instance.ReleaseComObj(propertyHeaderStream);
            }
        }

        protected virtual void ParserHeader(IStream propertyHeaderStream, ref int readCount) { }

        protected virtual void ParserProperties(IStorage storage, IStream propertyHeaderStream, ref int readCount)
        {
            STATSTG stat;
            propertyHeaderStream.Stat(out stat, 1);
            int count = (int)stat.cbSize;

            while (readCount < count)
            {
                var tag = propertyHeaderStream.ReadInt32(ref readCount);
                var flag = propertyHeaderStream.ReadInt32(ref readCount);
                var value = propertyHeaderStream.ReadInt64(ref readCount);
                IPropValueForParser property = SpecialPropertyUtil.Instance.CreateNewPropValue(tag, value);

                property.Parse(Storage);
                Properties.AddProperty(property);
            }
        }
        #endregion


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

        public virtual bool Compare(object other, StringBuilder result, int indent)
        {
            SetMessage(result, indent, "{0} Compare", Name);
            
            if (!(other is BaseStruct))
            {
                SetMessage(result, indent, "{0} other is not BaseStruct", Name);
                return false;
            }

            var otherStruct = other as BaseStruct;
            var isSame = Properties.Compare(otherStruct.Properties, result, indent + 2);
            SetMessage(result, indent, "{0} Compare complete {1}.", Name, isSame);
            return isSame;
        }

        public static void SetMessage(StringBuilder sb, int indent, string format, params object[] args)
        {
            sb.Append(" ".PadLeft(indent)).AppendFormat(format, args).AppendLine();
        }
        
    }

    public interface ICompareMsg
    {
        bool Compare(object other, StringBuilder result, int indent);
    }

    public class Props : ICompareMsg
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

        internal IPropValue GetProperty(uint tag)
        {
            tag = BaseStruct.GetPropertyTag(tag);
            return Properties[(uint)tag];
        }

        internal bool ContainProperty(int tag)
        {
            return Properties.ContainsKey((uint)tag);
        }

        public bool Compare(object other, StringBuilder result, int indent)
        {
            if (!(other is Props))
            {
                BaseStruct.SetMessage(result, indent, "other is not Props.");
                return false;
            }

            var otherProperties = other as Props;
            HashSet<uint> sour = new HashSet<uint>();
            HashSet<uint> des = new HashSet<uint>(otherProperties.Properties.Keys);

            HashSet<uint> notSame = new HashSet<uint>();

            foreach(uint key in Properties.Keys)
            {
                if (des.Contains(key))
                {
                    var valueSour = Properties[key];
                    var valueDes = otherProperties.Properties[key];

                    bool isSame = true;
                    if (((IFTTreeNode)valueSour).BytesCount != ((IFTTreeNode)valueDes).BytesCount || ((IFTTreeNode)valueSour).Bytes.Length != ((IFTTreeNode)valueDes).Bytes.Length)
                    {
                        isSame = false;
                    }
                    else
                    {
                        var count = ((IFTTreeNode)valueSour).Bytes.Length;
                        for (int i = 0; i < count; i++)
                        {
                            if(((IFTTreeNode)valueSour).Bytes[i] != ((IFTTreeNode)valueDes).Bytes[i])
                            {
                                isSame = false;
                                break;
                            }

                        }
                    }

                    if (!isSame)
                    {
                        notSame.Add(key);
                    }

                    des.Remove(key);
                }
                else
                {
                    sour.Add(key);
                }
            }

            if (notSame.Count == 0 && sour.Count == 0 && des.Count == 0)
            {
                BaseStruct.SetMessage(result, indent, "All property is same.");
                return true;
            }

            var newIndent = indent + 2;
            BaseStruct.SetMessage(result, indent, "Following is not same property.");
            foreach (var notSameKey in notSame)
            {
                BaseStruct.SetMessage(result, newIndent, "left :{0}.", SpecialPropertyUtil.Instance.GetString((ISpecialValue)Properties[notSameKey]));
                BaseStruct.SetMessage(result, newIndent, "right:{0}.", SpecialPropertyUtil.Instance.GetString((ISpecialValue)otherProperties.Properties[notSameKey]));
                result.AppendLine();
            }

            BaseStruct.SetMessage(result, indent, "Following is not in left.");
            foreach (var sourExtra in sour)
            {
                BaseStruct.SetMessage(result, newIndent, "{0}.", SpecialPropertyUtil.Instance.GetString((ISpecialValue)Properties[sourExtra]));
                result.AppendLine();
            }

            BaseStruct.SetMessage(result, indent, "Following is not in right.");
            foreach (var desExtra in des)
            {
                BaseStruct.SetMessage(result, newIndent, "{0}.", SpecialPropertyUtil.Instance.GetString((ISpecialValue)otherProperties.Properties[desExtra]));
                result.AppendLine();
            }
            return false;
        }
    }
}
