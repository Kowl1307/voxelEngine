namespace Tests.Runtime
{
    using NUnit.Framework;
    using UnityEngine;
    using Voxel_Engine.SVO;

    public class LinearOctreeTests
    {
        private LinearOctree<int> _octree;

        [SetUp]
        public void Setup()
        {
            _octree = new LinearOctree<int>();
        }

        [Test]
        public void TestEmptyOctreeReturnsDefault()
        {
            var result = _octree.GetValueAt(new Vector3Int(0, 0, 0));
            Assert.AreEqual(default(int), result);
        }

        [Test]
        public void TestInsertAndGetVoxel()
        {
            var pos = new Vector3Int(1, 2, 3);
            _octree.SetVoxel(pos, 42);

            var retrieved = _octree.GetValueAt(pos);
            Assert.AreEqual(42, retrieved);
        }

        [Test]
        public void TestGetTypeAtReturnsParentContent()
        {
            var posParent = new Vector3Int(0, 0, 0);
            var posChild = new Vector3Int(0, 0, 1);

            _octree.SetVoxel(posParent, 7);
            Assert.AreEqual(0, _octree.MortonEncode(posChild).Code&0b111000);
            Assert.AreEqual(20, _octree.MortonEncode(posChild).Depth);
            // Child nicht explizit gesetzt
            var retrieved = _octree.GetValueAt(posChild);
            Assert.AreEqual(7, retrieved);
        }

        [Test]
        public void TestBottomUpCollapseCollapsesChildren()
        {
            // Setze 8 Kinder mit demselben Wert um Collapse zu triggern
            var value = 123;

            var childCount = 0;
            for (var x = 8ul; x < 16; x++)
            {
                _octree.SetVoxel(new MortonCode(x, 1), value);
                childCount++;
            }

            var parentPosMorton = _octree.GetParentCode(new MortonCode(8,1));
            // Prüfe ob Parent-Knoten existiert und korrekt gesetzt wurde
            Assert.IsTrue(_octree._octree.ContainsKey(parentPosMorton));
            Assert.AreEqual(value, _octree._octree[parentPosMorton]);
            // Prüfe, dass Kinder entfernt wurden
            Assert.AreEqual(childCount, 8);
            var childrenFound = 0;
            for (var i = 0; i < 8; i++)
            {
                var childCode = _octree.GetChildCode(parentPosMorton, (ulong)i);
                if (_octree._octree.ContainsKey(childCode))
                    childrenFound++;
            }

            Assert.AreEqual(0, childrenFound);
        }
        
        [Test]
        public void TestBottomUpCollapseCollapsesChildrenForZero()
        {
            // Setze 8 Kinder mit demselben Wert um Collapse zu triggern
            var value = 123;

            var childCount = 0;
            for (var x = 0ul; x < 8; x++)
            {
                _octree.SetVoxel(new MortonCode(x, 0), value);
                childCount++;
            }

            var parentPosMorton = _octree.GetParentCode(new MortonCode(0, 0));
            // Prüfe ob Parent-Knoten existiert und korrekt gesetzt wurde
            Assert.IsTrue(_octree._octree.ContainsKey(parentPosMorton));
            Assert.AreEqual(value, _octree._octree[parentPosMorton]);
            // Prüfe, dass Kinder entfernt wurden
            Assert.AreEqual(childCount, 8);
            var childrenFound = 0;
            for (var i = 0; i < 8; i++)
            {
                var childCode = _octree.GetChildCode(parentPosMorton, (ulong)i);
                if (_octree._octree.ContainsKey(childCode))
                    childrenFound++;
            }

            Assert.AreEqual(0, childrenFound);
        }

        [Test]
        public void TestSetVoxelUpdatesValue()
        {
            var pos = new Vector3Int(5, 5, 5);
            _octree.SetVoxel(pos, 10);
            _octree.SetVoxel(pos, 20);

            var retrieved = _octree.GetValueAt(pos);
            Assert.AreEqual(20, retrieved);
        }
        
        [Test]
        public void TestMortonEncode()
        {
            var octree = new LinearOctree<int>();

            // Position (0,0,0) => Morton-Code sollte 0 sein
            Assert.AreEqual(0ul, octree.MortonEncode(0, 0, 0).Code);

            // Position (1,0,0) => 1 im X-Bit-Interleaving an der richtigen Stelle
            Assert.AreEqual(1ul, octree.MortonEncode(1, 0, 0).Code);

            // Position (0,1,0)
            Assert.AreEqual(2ul, octree.MortonEncode(0, 1, 0).Code);

            // Position (0,0,1)
            Assert.AreEqual(4ul, octree.MortonEncode(0, 0, 1).Code);

            // Position (1,1,1) Beispiel: Bits interleaved zu 7 (binär 111)
            Assert.AreEqual(7ul, octree.MortonEncode(1, 1, 1).Code);

            // Größeres Beispiel
            var code = octree.MortonEncode(2, 3, 1).Code;
            
            ulong expectedCode = 0;
            expectedCode |= (ulong)(2 & 1) << 0;
            expectedCode |= (ulong)(3 & 1) << 1;
            expectedCode |= (ulong)(1 & 1) << 2;
            expectedCode |= (ulong)((2 >> 1) & 1) << 3;
            expectedCode |= (ulong)((3 >> 1) & 1) << 4;
            expectedCode |= (ulong)((1 >> 1) & 1) << 5;
            Assert.AreEqual(expectedCode, code);
        }

        
        [Test]
        public void TestGetChildCode()
        {
            var octree = new LinearOctree<int>();

            ulong parentCode = 0b101010; // Beispiel-Morton-Code für den Parent
            var parentMorton = new MortonCode(parentCode, 1);
            // Teste alle 8 möglichen children
            for (ulong i = 0; i < 8; i++)
            {
                var childCode = octree.GetChildCode(parentMorton, i);
                // childCode sollte die unteren 3 Bits genau i sein
                var expectedChildBits = i & 0b111;
                var actualChildBits = childCode.Code & 0b111;
                Assert.AreEqual(expectedChildBits, actualChildBits);

                // childCode sollte die oberen Bits vom parentCode links verschoben enthalten
                var expectedParentBits = parentCode << 3;
                var actualParentBits = childCode.Code & (~0b111UL);
                Assert.AreEqual(expectedParentBits, actualParentBits);
            }
        }

    }
}