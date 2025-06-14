using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Events;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;

public abstract class BaseEntity : IEntityWithId
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset LastModified { get; set; }

	private readonly List<BaseEvent> _domainEvents = [];

	public IReadOnlyCollection<BaseEvent> GetDomainEvents()
		=> _domainEvents.AsReadOnly();

	public void AddDomainEvent(BaseEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	public void RemoveDomainEvent(BaseEvent domainEvent)
	{
		_domainEvents.Remove(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}
