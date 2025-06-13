using Meckbaig.Cqrs.EntityFrameworkCore.Enums;

namespace Meckbaig.Cqrs.EntityFrameworkCore.Attributes;

public class DatabaseRelationAttribute : Attribute
{
	public Relation Relation { get; set; }

	public DatabaseRelationAttribute(Relation relation)
	{
		Relation = relation;
	}
}
