namespace Meckbaig.Cqrs.Dto.Abstractions;

/// <summary>
/// Interface for DTOs that represent entities that can be edited.
/// </summary>
public interface IEditDto : IBaseDto
{
	/// <summary>
	/// Gets the type of the validator for this DTO.
	/// </summary>
	static abstract Type GetValidatorType();
}
