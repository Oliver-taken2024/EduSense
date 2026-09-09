namespace EduSense.Shared
{
    public class UserInfoDto
    {
        public required string Username { get; set; }
        public required string DisplayName { get; set; }
        public required List<string> Roles { get; set; }
    }
}
