namespace DVLD.CORE.Entities
{
    public class LicenseClass
    {
        public int LicenseClassID { set; get; }
        public string ClassName { set; get; } = null!;
        public string ClassDescription { set; get; } = null!;
        public byte MinimumAllowedAge { set; get; }
        public byte DefaultValidityLength { set; get; }
        public float ClassFees { set; get; }
    }
}