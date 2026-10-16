namespace SlideNarrator.DAL.Entities;

/// <summary>Vai trò phân quyền: Admin, Presenter, Viewer.</summary>
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<UserAccount> Users { get; set; } = new List<UserAccount>();
}
