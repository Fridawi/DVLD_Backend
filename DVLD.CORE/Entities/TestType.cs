namespace DVLD.CORE.Entities
{
    public class TestType
    {
        public int TestTypeID { set; get; }
        public string Title { set; get; } = null!;
        public string Description { set; get; } = null!;
        public float Fees { set; get; }
    }
}