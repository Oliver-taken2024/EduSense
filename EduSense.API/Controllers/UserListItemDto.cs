namespace EduSense.API.Controllers
{
    internal class UserListItemDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public required List<string> Roles { get; set; }
    }
}