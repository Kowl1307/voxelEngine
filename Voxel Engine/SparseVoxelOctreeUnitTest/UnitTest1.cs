using Voxel_Engine.SVO;

namespace SparseVoxelOctreeUnitTest;

public class Tests
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
        SparseVoxelOctree<TestEnum>.VectorToOctal();
        Assert.Pass();
    }
}