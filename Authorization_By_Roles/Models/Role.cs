namespace Authorization_By_Roles.Models
{
    public class Role
    {
        public string Name { get; set; }

        public Role(string name) => Name = name;
    }
}
