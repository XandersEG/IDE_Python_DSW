namespace IDEPython.DTOs.Models
{
    public class Student : User
    {
        public List<Course> Courses { get; set; } = new();

        public Student(String firstName, String secondName, String lastName1, String lastName2, String email)
        {
            this.FirstName = firstName;
            SecondName = secondName;
            LastName1 = lastName1;
            LastName2 = lastName2;
            Email = email;
        }
    }
}
