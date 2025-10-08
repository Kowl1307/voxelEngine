using NUnit.Framework;
using UnityEngine;
using Voxel_Engine.SVO;

namespace Tests.Runtime
{
    [TestFixture]
    public class SvoTests
    {
        private enum TestEnum
        {
            Green,
            Red,
            Blue
        };
    
        [SetUp]
        public void Setup()
        {
        
        }

        [Test]
        public void Test1()
        {
            Assert.AreEqual(0b1001000, IndexHelper.Interleave_Two0(6));
            
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {            
            Assert.AreEqual(0b001000000000, IndexHelper.Interleave_Two0(8));
            Assert.Pass();

        }
        
        [Test]
        public void Test3()
        {            
            Assert.AreEqual(0b001000000001, IndexHelper.Interleave_Two0(9));
            Assert.Pass();

        }
        
        [Test]
        public void TestInterleave3()
        {            
            Assert.AreEqual(0b011100100001, IndexHelper.Interleave3(6, 8, 9).Value);
            Assert.Pass();
        }

        [Test]
        public void TestOctalDigits()
        {
            //0b011 100 100 001
            var octalNumber = new OctalNumber() { Value = 0b011100100001 };
            Assert.AreEqual(3, octalNumber.GetOctalDigit(0));
            Assert.AreEqual(4, octalNumber.GetOctalDigit(1));
            Assert.AreEqual(4, octalNumber.GetOctalDigit(2));
            Assert.AreEqual(1, octalNumber.GetOctalDigit(3));
        }

        [Test]
        public void TestTreeConstruction()
        {
            var voxelPositions = new Vector3Int[] { new Vector3Int(0, 0, 0), new Vector3Int(63, 0, 0), new Vector3Int(20, 20, 0) };
            var voxelColors = new TestEnum[] { TestEnum.Green, TestEnum.Red, TestEnum.Blue };
            
            var tree = new SparseVoxelOctree<TestEnum>(64, voxelPositions, voxelColors);
            tree.Insert(new Vector3Int(25,13,5), TestEnum.Green);
            Assert.NotNull(tree.Root[0]);
        }
    }
}