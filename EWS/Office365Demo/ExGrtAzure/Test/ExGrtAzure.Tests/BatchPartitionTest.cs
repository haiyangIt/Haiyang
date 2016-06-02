using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup;
using System.Collections.Generic;
using System.Diagnostics;
using Arcserve.Office365.Exchange.Util;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class BatchPartitionTest
    {
        List<List<Item>> arrays = null;
        [TestInitialize]
        public void InitValues()
        {
            arrays = InitArrays();
            int arraysTotalCount = 25;
        }
        
        public List<List<Item>> InitArrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(7);
            item1.array.AddRange(new List<Item>()
            {
                new Item(1,0), new Item(2,0),new Item(3,0),new Item(4,0),new Item(5,0),new Item(6,0),new Item(7,0)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(6);
            item2.array.AddRange(new List<Item>()
            {
                new Item(11,0), new Item(21,0),new Item(31,0),new Item(41,0),new Item(51,0),new Item(61,0)
            });


            EachPartition<Item> item3 = new EachPartition<Item>(4);
            item3.array.AddRange(new List<Item>()
            {
                new Item(111,0), new Item(211,0),new Item(311,0),new Item(411,0)
            });

            EachPartition<Item> item4 = new EachPartition<Item>(8);
            item4.array.AddRange(new List<Item>()
            {
                new Item(1111,0), new Item(2111,0),new Item(3111,0),new Item(4111,0),new Item(5111,0), new Item(6111,0),new Item(7111,0),new Item(8111,0)
            });

            return new List<List<Item>>() { item1, item2, item3, item4 };
        }


        public List<List<Item>> InitSmall4_22Arrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(7);
            item1.array.AddRange(new List<Item>()
            {
                new Item(1,0), new Item(2,0),new Item(3,0),new Item(4,0),new Item(5,0),new Item(6,0),new Item(7,0)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(6);
            item2.array.AddRange(new List<Item>()
            {
                new Item(11,0), new Item(21,0),new Item(31,0),new Item(41,0),new Item(51,0),new Item(61,0)
            });


            EachPartition<Item> item3 = new EachPartition<Item>(4);
            item3.array.AddRange(new List<Item>()
            {
                new Item(111,0), new Item(211,0),new Item(311,0),new Item(411,0)
            });

            EachPartition<Item> item4 = new EachPartition<Item>(5);
            item4.array.AddRange(new List<Item>()
            {
                new Item(1111,0), new Item(2111,0),new Item(3111,0),new Item(4111,0),new Item(5111,0)
            });

            return new List<List<Item>>() { item1, item2, item3, item4 };
        }

        public List<List<Item>> InitSmall2_13Arrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(7);
            item1.array.AddRange(new List<Item>()
            {
                new Item(1,0), new Item(2,0),new Item(3,0),new Item(4,0),new Item(5,0),new Item(6,0),new Item(7,0)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(6);
            item2.array.AddRange(new List<Item>()
            {
                new Item(11,0), new Item(21,0),new Item(31,0),new Item(41,0),new Item(51,0),new Item(61,0)
            });


            return new List<List<Item>>() { item1, item2 };
        }


        public List<List<Item>> InitSmall7_37Arrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(7);
            item1.array.AddRange(new List<Item>()
            {
                new Item(1,0), new Item(2,0),new Item(3,0),new Item(4,0),new Item(5,0),new Item(6,0),new Item(7,0)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(6);
            item2.array.AddRange(new List<Item>()
            {
                new Item(11,0), new Item(21,0),new Item(31,0),new Item(41,0),new Item(51,0),new Item(61,0)
            });


            EachPartition<Item> item3 = new EachPartition<Item>(4);
            item3.array.AddRange(new List<Item>()
            {
                new Item(111,0), new Item(211,0),new Item(311,0),new Item(411,0)
            });

            EachPartition<Item> item4 = new EachPartition<Item>(5);
            item4.array.AddRange(new List<Item>()
            {
                new Item(1111,0), new Item(2111,0),new Item(3111,0),new Item(4111,0),new Item(5111,0)
            });

            EachPartition<Item> item5 = new EachPartition<Item>(5);
            item5.array.AddRange(new List<Item>()
            {
                new Item(101,0), new Item(201,0),new Item(301,0),new Item(401,0),new Item(501,0)
            });

            EachPartition<Item> item6 = new EachPartition<Item>(5);
            item6.array.AddRange(new List<Item>()
            {
                new Item(102,0), new Item(202,0),new Item(302,0),new Item(402,0),new Item(502,0)
            });

            EachPartition<Item> item7 = new EachPartition<Item>(5);
            item7.array.AddRange(new List<Item>()
            {
                new Item(103,0), new Item(203,0),new Item(303,0),new Item(403,0),new Item(503,0)
            });


            return new List<List<Item>>() { item1, item2, item3, item4, item5, item6, item7 };
        }

        public List<List<Item>> InitLarge4_9Arrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(3);
            item1.array.AddRange(new List<Item>()
            {
                new Item("L1",0, false), new Item("L2",0, false),new Item("L3",0, false)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(2);
            item2.array.AddRange(new List<Item>()
            {
                new Item("L11",0, false), new Item("L21",0, false)
            });


            EachPartition<Item> item3 = new EachPartition<Item>(1);
            item3.array.AddRange(new List<Item>()
            {
                new Item("L111",0, false)
            });

            EachPartition<Item> item4 = new EachPartition<Item>(3);
            item4.array.AddRange(new List<Item>()
            {
                new Item("L1111",0, false), new Item("L2111",0, false),new Item("L3111",0, false)
            });

            return new List<List<Item>>() { item1, item2, item3, item4 };
        }

        public List<List<Item>> InitLarge10_23Arrays()
        {
            EachPartition<Item> item1 = new EachPartition<Item>(3);
            item1.array.AddRange(new List<Item>()
            {
                new Item("L1",0, false), new Item("L2",0, false),new Item("L3",0, false)
            });

            EachPartition<Item> item2 = new EachPartition<Item>(2);
            item2.array.AddRange(new List<Item>()
            {
                new Item("L11",0, false), new Item("L21",0, false)
            });


            EachPartition<Item> item3 = new EachPartition<Item>(1);
            item3.array.AddRange(new List<Item>()
            {
                new Item("L111",0, false)
            });

            EachPartition<Item> item4 = new EachPartition<Item>(3);
            item4.array.AddRange(new List<Item>()
            {
                new Item("L1111",0, false), new Item("L2111",0, false),new Item("L3111",0, false)
            });

            EachPartition<Item> item5 = new EachPartition<Item>(3);
            item5.array.AddRange(new List<Item>()
            {
                new Item("L101",0, false), new Item("L201",0, false),new Item("L301",0, false)
            });

            EachPartition<Item> item6 = new EachPartition<Item>(1);
            item6.array.AddRange(new List<Item>()
            {
                new Item("L11101",0, false)
            });

            EachPartition<Item> item7 = new EachPartition<Item>(2);
            item7.array.AddRange(new List<Item>()
            {
                new Item("L1101",0, false), new Item("L2101",0, false)
            });



            EachPartition<Item> item8 = new EachPartition<Item>(3);
            item8.array.AddRange(new List<Item>()
            {
                new Item("L1114",0, false), new Item("L2114",0, false),new Item("L3114",0, false)
            });

            EachPartition<Item> item9 = new EachPartition<Item>(3);
            item9.array.AddRange(new List<Item>()
            {
                new Item("L105",0, false), new Item("L205",0, false),new Item("L305",0, false)
            });

            EachPartition<Item> item10 = new EachPartition<Item>(2);
            item10.array.AddRange(new List<Item>()
            {
                new Item("L1106",0, false), new Item("L2106",0, false)
            });

            return new List<List<Item>>() { item1, item2, item3, item4, item5, item6, item7, item8, item9, item10 };
        }


        [TestMethod]
        public void MergeTwo22_9_3_5_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 3, 5);

                Assert.AreEqual(result.Count, 4);
                Assert.AreEqual(result[0].Count, 8);
                Assert.AreEqual(result[1].Count, 7);
                Assert.AreEqual(result[2].Count, 8);
                Assert.AreEqual(result[3].Count, 8);

                Assert.AreEqual(result[0][6].Index, "L11");
                Assert.AreEqual(result[1][2].Index, "0006");
                Assert.AreEqual(result[1][5].Index, "L2111");

                Assert.AreEqual(result[2][5].Index, "0051");
                Assert.AreEqual(result[3][5].Index, "3111");
                Assert.AreEqual(result[3][7].Index, "5111");
            }
        }

        [TestMethod]
        public List<int> GetRandomIndex(int count)
        {
            
            List<int> result = new List<int>(count);
            var random = new Random();
            var index = new int[count];
            for (int i = 0; i < count; i++)
            {
                index[i] = i;
            }

            for (int i = 0; i < count; i++)
            {
                var indexIndex = random.Next(i, count - 1);

                result.Add( index[indexIndex]);

                var temp = index[i];
                index[i] = index[indexIndex];
                index[indexIndex] = temp;
            }

            foreach(var i in result)
            {
                Debug.Write(i);
                Debug.Write(" ");
            }
            return result;
        }
        [TestMethod]
        public void MergeTwo22_9_3_5_Add_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; }, 40, 100, 3, 5);
            }
        }

        private void OutPutArray(List<List<Item>> smallArray, List<List<Item>> largeArray)
        {
            PartitionList<Item> test = new PartitionList<Item>();

            List<Item> allResult = new List<Item>();
            foreach (var item in smallArray)
            {
                foreach (var i in item)
                {
                    allResult.Add(i);
                }
            }

            foreach (var item in largeArray)
            {
                foreach (var i in item)
                {
                    allResult.Add(i);
                }
            }

            var randomListIndex = GetRandomIndex(allResult.Count);
            Debug.WriteLine(" ");
            foreach (var index in randomListIndex)
            {
                test.AddItem(allResult[index], allResult[index].IsSmall);
                Debug.Write(allResult[index].Index);
                Debug.Write(allResult[index].IsSmall ? "[S] " : "[L] ");
            }

            Debug.WriteLine(" ");
            Debug.WriteLine("finished result:");
            var result = test.GetFinishedPartitions();
            int count = 0;
            foreach (var item in result)
            {
                foreach (var i in item)
                {
                    Debug.Write(i.Index);
                    Debug.Write(i.IsSmall ? "[S] " : "[L] ");
                    count++;
                }
                Debug.WriteLine(" ");
            }

            Debug.WriteLine(" ");
            Debug.WriteLine("finished result:");
            var leftResult = test.GetLeftPartitions();
            foreach (var item in leftResult)
            {
                foreach (var i in item)
                {
                    Debug.Write(i.Index);
                    Debug.Write(i.IsSmall ? "[S] " : "[L] ");
                    count++;
                }
                Debug.WriteLine(" ");
            }
            Assert.AreEqual(allResult.Count, count);
        }

        [TestMethod]
        public void TestPartition()
        {
            int smallPartitionCount = 7;
            int largePartitionCount = 3;
            int maxRequestSize = 40;
            int batchAddCount = 30;
            CollectionPatitionUtil<Item> batchItemUtil = new CollectionPatitionUtil<Item>((item) => { return item.Size; },
                batchAddCount, maxRequestSize, smallPartitionCount, largePartitionCount);

            Random sizeRandom = new Random();
            sizeRandom.Next(0, maxRequestSize + 5);

            LinkedList<Item> items = new LinkedList<Item>();
            for (int i = 0; i < 90; i++)
            {
                if (i % 10 == 0)
                    Debug.WriteLine(" ");
                var item = new Item(i, sizeRandom.Next(0, maxRequestSize + 5));
                items.AddLast(item);
                Debug.Write(item.Index);
                Debug.Write("[");
                Debug.Write(item.Size.ToString("D2"));
                Debug.Write("]");
                Debug.Write(" ");

            }
            Debug.WriteLine(" ");
            Debug.WriteLine("-----------------------");

            foreach (var item in items)
            {
                IEnumerable<IEnumerable<Item>> result = null;
                if (batchItemUtil.AddItem(item, out result))
                {
                    Debug.WriteLine(string.Format("\r\n Finished Result:\r\n"));
                    foreach (var pa in result)
                    {
                        Debug.WriteLine("");
                        Debug.WriteLine("--");
                        foreach (var i in pa)
                        {
                            Debug.Write(i.Index);
                            Debug.Write("[");
                            Debug.Write(i.Size.ToString("D2"));
                            Debug.Write("]");
                        }
                    }
                }
            }

            var resultComplete = batchItemUtil.AddComplete();
            Debug.WriteLine(string.Format("\r\n Finished Complete Result:\r\n"));
            foreach (var pa in resultComplete)
            {
                Debug.WriteLine("");
                Debug.WriteLine("--");
                foreach (var i in pa)
                {
                    Debug.Write(i.Index);
                    Debug.Write("[");
                    Debug.Write(i.Size.ToString("D2"));
                    Debug.Write("]");
                }
            }

        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide10()
        {
            var partition = 10;
            var totalArray = 3;
            var lastArray = 2;
            var lastArrayCount = 5;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);

            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide5()
        {
            var partition = 5;
            var totalArray = 5;
            var lastArray = 4;
            var lastArrayCount = 5;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide3()
        {
            var partition = 3;
            var totalArray = 9;
            var lastArray = 8;
            var lastArrayCount = 1;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }
        [TestMethod]
        public void TestPartitionListMergeElementDivide7()
        {
            var partition = 7;
            var totalArray = 4;
            var lastArray = 3;
            var lastArrayCount = 4;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide4()
        {
            var partition = 4;
            var totalArray = 7;
            var lastArray = 6;
            var lastArrayCount = 1;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide6()
        {
            var partition = 6;
            var totalArray = 5;
            var lastArray = 4;
            var lastArrayCount = 1;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }


        [TestMethod]
        public void TestPartitionListMergeElementDivide11()
        {
            var partition = 11;
            var totalArray = 3;
            var lastArray = 2;
            var lastArrayCount = 3;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide16()
        {
            var partition = 16;
            var totalArray = 2;
            var lastArray = 1;
            var lastArrayCount = 9;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[i].Count, partition);
            }
        }

        [TestMethod]
        public void TestPartitionListMergeElementDivide30()
        {
            var partition = 30;
            var totalArray = 1;
            var lastArray = 0;
            var lastArrayCount = 25;
            PartitionList<Item> test = new PartitionList<Item>();
            var assetResult = test.RePartitionElements(arrays, partition);

            Assert.AreEqual(assetResult.Count, totalArray);
            Assert.AreEqual(assetResult[lastArray].Count, lastArrayCount);
            for (int i = 0; i < lastArray; i++)
            {
                Assert.AreEqual(assetResult[lastArray].Count, partition);
            }
        }


        [TestMethod]
        public void MergeTwo13_9_7_3Array()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 7, 3);

                Assert.AreEqual(result.Count, 3);
                Assert.AreEqual(result[0].Count, 10);
                Assert.AreEqual(result[1].Count, 9);
                Assert.AreEqual(result[2].Count, 3);
            }
        }

        [TestMethod]
        public void MergeTwo13_9_7_3_Add_OutPutArray()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 7;
            int largePartitionCount = 3;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; }, 40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }

        [TestMethod]
        public void MergeTwo13_9_6_3Array()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 6, 3);

                Assert.AreEqual(result.Count, 3);
                Assert.AreEqual(result[0].Count, 9);
                Assert.AreEqual(result[1].Count, 9);
                Assert.AreEqual(result[2].Count, 4);
            }
        }

        [TestMethod]
        public void MergeTwo13_9_6_3_Add_OutPutArray()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 6;
            int largePartitionCount = 3;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; }, 40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }

        [TestMethod]
        public void MergeTwo13_9_5_4Array()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 5, 4);

                Assert.AreEqual(result.Count, 3);
                Assert.AreEqual(result[0].Count, 9);
                Assert.AreEqual(result[1].Count, 9);
                Assert.AreEqual(result[2].Count, 4);
            }
        }

        [TestMethod]
        public void MergeTwo13_9_5_4_Add_OutPutArray()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 5;
            int largePartitionCount = 4;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }

        [TestMethod]
        public void MergeTwo13_9_6_2Array()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 6, 2);

                Assert.AreEqual(result.Count, 5);
                Assert.AreEqual(result[0].Count, 8);
                Assert.AreEqual(result[1].Count, 8);
                Assert.AreEqual(result[2].Count, 3);
                Assert.AreEqual(result[3].Count, 2);
                Assert.AreEqual(result[4].Count, 1);
            }
        }

        [TestMethod]
        public void MergeTwo13_9_6_2_Add_OutPutArray()
        {
            var smallArray = InitSmall2_13Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 6;
            int largePartitionCount = 2;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }

        [TestMethod]
        public void MergeTwo22_9_7_3Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            PartitionList<Item> test = new PartitionList<Item>();
            var result = test.MergeTwoCollections(smallArray, largeArray, 7, 3);

            Assert.AreEqual(result.Count, 4);
            Assert.AreEqual(result[0].Count, 10);
            Assert.AreEqual(result[1].Count, 10);
            Assert.AreEqual(result[2].Count, 10);
            Assert.AreEqual(result[3].Count, 1);
        }
        [TestMethod]
        public void MergeTwo22_9_7_3_Add_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 7;
            int largePartitionCount = 3;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }


        [TestMethod]
        public void MergeTwo22_9_6_3Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 6, 3);

                Assert.AreEqual(result.Count, 4);
                Assert.AreEqual(result[0].Count, 9);
                Assert.AreEqual(result[1].Count, 9);
                Assert.AreEqual(result[2].Count, 9);
                Assert.AreEqual(result[3].Count, 4);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_6_3_Add_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 6;
            int largePartitionCount = 3;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);

            }
        }

        [TestMethod]
        public void MergeTwo22_9_5_4Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 5, 4);

                Assert.AreEqual(result.Count, 4);
                Assert.AreEqual(result[0].Count, 9);
                Assert.AreEqual(result[1].Count, 9);
                Assert.AreEqual(result[2].Count, 6);
                Assert.AreEqual(result[3].Count, 7);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_5_4_Add_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 5;
            int largePartitionCount = 4;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_6_2Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 6, 2);

                Assert.AreEqual(result.Count, 5);
                Assert.AreEqual(result[0].Count, 8);
                Assert.AreEqual(result[1].Count, 8);
                Assert.AreEqual(result[2].Count, 8);
                Assert.AreEqual(result[3].Count, 6);
                Assert.AreEqual(result[4].Count, 1);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_6_2_Add_OutPutArray()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            int smallPartitionCount = 6;
            int largePartitionCount = 2;
            int maxCount = smallPartitionCount + largePartitionCount;
            {
                CollectionPatitionUtil<Item> b = new CollectionPatitionUtil<Item>((item) => { return item.Size; },  40, 100, smallPartitionCount, largePartitionCount);
                OutPutArray(smallArray, largeArray);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_3_5Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 3, 5);

                Assert.AreEqual(result.Count, 4);
                Assert.AreEqual(result[0].Count, 8);
                Assert.AreEqual(result[1].Count, 7);
                Assert.AreEqual(result[2].Count, 8);
                Assert.AreEqual(result[3].Count, 8);
            }
        }


        [TestMethod]
        public void MergeTwo22_9_4_5Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 4, 5);

                Assert.AreEqual(result.Count, 4);
                Assert.AreEqual(result[0].Count, 9);
                Assert.AreEqual(result[1].Count, 8);
                Assert.AreEqual(result[2].Count, 9);
                Assert.AreEqual(result[3].Count, 5);
            }
        }

        [TestMethod]
        public void MergeTwo22_9_30_10Array()
        {
            var smallArray = InitSmall4_22Arrays();
            var largeArray = InitLarge4_9Arrays();
            {
                PartitionList<Item> test = new PartitionList<Item>();
                var result = test.MergeTwoCollections(smallArray, largeArray, 30, 10);

                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].Count, 31);
            }
        }

        public class Item
        {
            public string Index;
            public int Size;
            public bool IsSmall;

            public Item(int index, int size, bool isSmall = true)
            {
                Index = index.ToString("D4");
                Size = size;
                IsSmall = isSmall;
            }

            public Item(string index, int size, bool isSmall = true)
            {
                Index = index;
                Size = size;
                IsSmall = isSmall;
            }

            public Item() { }
        }
    }
}
