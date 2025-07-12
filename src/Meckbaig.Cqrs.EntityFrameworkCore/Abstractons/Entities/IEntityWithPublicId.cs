namespace Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;

public interface IEntityWithPublicId
{
	string PublicId { get; set; }
}
