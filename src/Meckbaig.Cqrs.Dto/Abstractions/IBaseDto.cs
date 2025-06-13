namespace Meckbaig.Cqrs.Dto.Abstractions;

/// <summary>
/// Interface for DTOs.
/// </summary>
public interface IBaseDto
{
	/// <summary>
	/// Gets the type of the origin entity for this DTO.
	/// </summary>
	static abstract Type GetOriginType();
}
