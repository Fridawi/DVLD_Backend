namespace DVLD.CORE.Entities
{
    public class Person
    {
        public int PersonID { set; get; }
        public string FirstName { set; get; } = null!;
        public string SecondName { set; get; } = null!;
        public string? ThirdName { set; get; }
        public string LastName { set; get; } = null!;
        public string FullName
        {
            get
            {
                string[] names = { FirstName, SecondName, ThirdName!, LastName };
                return string.Join(" ", names.Where(n => !string.IsNullOrWhiteSpace(n)));
            }
        }
        public string NationalNo { set; get; } = null!;
        public DateTime DateOfBirth { set; get; }
        public short Gendor { set; get; }
        public string Address { set; get; } = null!;
        public string Phone { set; get; } = null!;
        public string? Email { set; get; }
        public int NationalityCountryID { set; get; }
        public Country CountryInfo { set; get; } = null!;
        public string? ImagePath { set; get; }
    }
}
