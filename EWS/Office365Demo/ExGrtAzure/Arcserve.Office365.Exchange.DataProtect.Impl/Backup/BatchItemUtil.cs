using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup
{
    public class BatchItemUtil<T>
    {
        public BatchItemUtil(Func<T, int> funcGetSize)
        {
            FuncGetSize = funcGetSize;
            PartitionListCount = MaxBatchAddCount / BatchRequestMaxCount;
            if (BatchRequestMaxCount != SmallCountInPartition + LargeCountInPartition)
                throw new ArgumentException("the config is not right. small count add large count must equal maxBatchCount.");
        }

        public BatchItemUtil(Func<T, int> funcGetSize, int maxBatchAddCount, int requestMaxSize, int smallCountInPartition, int largeCountInPartition)
        {
            FuncGetSize = funcGetSize;
            PartitionListCount = MaxBatchAddCount / BatchRequestMaxCount;

            BatchRequestMaxCount = smallCountInPartition + largeCountInPartition;
            MaxBatchAddCount = maxBatchAddCount;
            RequestMaxSize = requestMaxSize;
            SmallCountInPartition = smallCountInPartition;
            LargeCountInPartition = largeCountInPartition;
        }

        private int PartitionListCount = 0;
        public static int BatchRequestMaxCount = CloudConfig.Instance.BatchExportImportItemMaxCount;
        public static int MaxBatchAddCount = CloudConfig.Instance.BatchExportImportMaxAddCount;
        public static int RequestMaxSize = CloudConfig.Instance.BatchExportImportItemMaxSizeForSingleMB;

        public static int SmallCountInPartition = CloudConfig.Instance.BatchExportImportSmallCountInPartition;
        public static int LargeCountInPartition = CloudConfig.Instance.BatchExportImportLargeCountInPartition;

        private int TotalCount = 0;
        private Func<T, int> FuncGetSize;

        PartitionList<T> PartitionLists = new PartitionList<T>();
        private int TotalSize = 0;
        private List<T> Collections = new List<T>(100);
        private List<T> LargestCollection = new List<T>(10);
        public bool AddItem(T item, out IEnumerable<IEnumerable<T>> items)
        {
            int size = FuncGetSize(item);

            if (size > RequestMaxSize)
            {
                LargestCollection.Add(item);
                items = null;
                return false;
            }

            TotalCount += 1;
            TotalSize += size;
            Collections.Add(item);
            if (TotalCount >= MaxBatchAddCount)
            {
                return GetPartitions(out items);
            }
            items = null;
            return false;
        }

        private bool GetPartitions(out IEnumerable<IEnumerable<T>> items)
        {
            if (TotalCount == 0)
            {
                items = new List<List<T>>(0);
                return false;
            }

            int avgSize = TotalSize / (TotalCount * SmallCountInPartition / (LargeCountInPartition + SmallCountInPartition));

            Debug.Write(string.Format("\r\nTotalSize:{1},TotalCount:{2}, AvgSize: {0}\r\n ", avgSize, TotalSize, TotalCount));

            foreach (var ele in Collections)
            {
                int eleSize = FuncGetSize(ele);
                bool isSmall = eleSize <= avgSize;
                PartitionLists.AddItem(ele, isSmall);
            }

            var result = PartitionLists.GetFinishedPartitions();
            Collections.Clear();
            if (LargestCollection.Count > 0)
            {
                foreach (var largeItem in LargestCollection)
                {
                    result.Add(new List<T>(1) { largeItem });
                }
                LargestCollection.Clear();
            }
            items = result;
            TotalSize = 0;
            TotalCount = 0;
            return true;
        }

        public IEnumerable<IEnumerable<T>> AddComplete()
        {
            IEnumerable<IEnumerable<T>> items;
            GetPartitions(out items);
            return items;
        }


        public static List<List<T>> Convert(List<EachPartition<T>> obj)
        {
            List<List<T>> result = new List<List<T>>(obj.Count);
            foreach (var item in obj)
            {
                result.Add(item);
            }
            return result;
        }

    }

    public enum ListStatus : byte
    {
        LackSmall,
        LackLarge,
        Complete
    }


    public class EachPartition<TElement> : IEnumerable<TElement>
    {
        public List<TElement> array = new List<TElement>();
        public int Count
        {
            get
            {
                return array.Count;
            }
        }
        public int MaxCount = 0;
        int TotalCount = 0;
        public EachPartition(int maxCount)
        {
            MaxCount = maxCount;
        }
        public List<EachPartition<TElement>> Parent = null;
        public void AddItem(TElement item)
        {
            array.Add(item);
            TotalCount++;
            if (TotalCount == MaxCount)
            {
                FinishEvent.Invoke(this, null);
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return array.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }

        public event EventHandler FinishEvent;

        public static implicit operator List<TElement>(EachPartition<TElement> obj)
        {
            return obj.array;
        }
    }

    public class PartitionList<TElement>
    {
        List<EachPartition<TElement>> SmallCollections = new List<EachPartition<TElement>>();
        List<EachPartition<TElement>> LargeCollections = new List<EachPartition<TElement>>();
        bool isNeedNewSmall = true;
        bool isNeedNewLarge = true;

        int totalCount = 0;

        public void AddItem(TElement item, bool isSmall)
        {
            var result = GetUnFinishResult(isSmall);
            result.AddItem(item);
            totalCount++;
        }

        private EachPartition<TElement> GetUnFinishResult(bool isSmall)
        {
            if (isSmall)
            {
                return GetCollection(SmallCollections, BatchItemUtil<TElement>.SmallCountInPartition, isNeedNewSmall);
            }
            else
            {
                return GetCollection(LargeCollections, BatchItemUtil<TElement>.LargeCountInPartition, isNeedNewLarge);
            }
        }

        private EachPartition<TElement> GetCollection(List<EachPartition<TElement>> collections, int maxCount, bool isNeedNew)
        {
            if (isNeedNew)
            {
                EachPartition<TElement> partition = new EachPartition<TElement>(maxCount);
                collections.Add(partition);
                partition.FinishEvent += Partition_FinishEvent;
                partition.Parent = collections;
                isNeedNew = false;

            }
            return collections[collections.Count - 1];
        }

        private void Partition_FinishEvent(object sender, EventArgs e)
        {
            EachPartition<TElement> partition = (EachPartition<TElement>)sender;
            if (partition.Parent == SmallCollections)
                isNeedNewSmall = true;
            else
                isNeedNewLarge = true;
        }

        public List<List<TElement>> GetFinishedPartitions()
        {
            var result = MergeTwoCollections(BatchItemUtil<TElement>.Convert(SmallCollections), BatchItemUtil<TElement>.Convert(LargeCollections),
                BatchItemUtil<TElement>.SmallCountInPartition, BatchItemUtil<TElement>.LargeCountInPartition);
            SmallCollections.Clear();
            LargeCollections.Clear();
            return result;
        }

        private void ReBuildSmallAndLargePartion()
        {

        }

        public List<List<TElement>> GetLeftPartitions()
        {
            var result = MergeTwoCollections(BatchItemUtil<TElement>.Convert(SmallCollections), BatchItemUtil<TElement>.Convert(LargeCollections), BatchItemUtil<TElement>.SmallCountInPartition, BatchItemUtil<TElement>.LargeCountInPartition);
            SmallCollections.Clear();
            LargeCollections.Clear();
            return result;
        }

        public List<List<TElement>> RePartitionElements(List<List<TElement>> source, int listCount)
        {
            List<List<TElement>> des = new List<List<TElement>>();

            List<TElement> desItem = new List<TElement>(listCount);
            int desItemIndex = 0;
            int sourItemIndex = 0;
            int desItemLeft = 0;
            int sourItemLeft = 0;
            int copyLength = 0;
            foreach (var sourItem in source)
            {
                int sourItemCount = sourItem.Count;
                sourItemIndex = 0;
                while (sourItemIndex < sourItemCount)
                {
                    desItemLeft = listCount - desItemIndex;
                    sourItemLeft = sourItemCount - sourItemIndex;
                    copyLength = desItemLeft < sourItemLeft ? desItemLeft : sourItemLeft;
                    desItem.AddRange(sourItem.GetRange(sourItemIndex, copyLength));//.Skip(sourItemIndex).Take(copyLength));
                    desItemIndex += copyLength;
                    sourItemIndex += copyLength;

                    if (desItemIndex >= listCount)
                    {
                        des.Add(desItem);
                        desItem = new List<TElement>(listCount);
                        desItemIndex = 0;
                    }
                }
            }

            if (desItem.Count > 0)
            {
                des.Add(desItem);
            }
            return des;
        }

        public List<List<TElement>> MergeTwoCollections(List<List<TElement>> smallElement, List<List<TElement>> largeElement, int smallElementCapacity, int largeElementCapacity)
        {

            var listCapacity = smallElementCapacity + largeElementCapacity;
            if (largeElement.Count == 0)
            {
                return RePartitionElements(smallElement, listCapacity);
            }

            if (smallElement.Count == 0)
            {
                return RePartitionElements(largeElement, largeElementCapacity);
            }

            var result = new List<List<TElement>>(smallElement.Count);

            smallElement = RePartitionElements(smallElement, smallElementCapacity);
            largeElement = RePartitionElements(largeElement, largeElementCapacity);

            var count = smallElement.Count < largeElement.Count ? smallElement.Count : largeElement.Count;

            int index = 0;
            while (index < count)
            {
                smallElement[index].AddRange(largeElement[index]);
                index++;
            }

            result.AddRange(smallElement.GetRange(0, count));

            if (smallElement.Count == largeElement.Count)
            {
                return result;
            }

            if (index < smallElement.Count)
            {
                var temp = new List<List<TElement>>(smallElement.Count - index);
                temp.AddRange(smallElement.GetRange(index, smallElement.Count - index));
                result.AddRange(RePartitionElements(temp, listCapacity));
                return result;
            }
            else
            {
                result.AddRange(largeElement.GetRange(index, largeElement.Count - index));
                return result;
            }
        }
    }
}
