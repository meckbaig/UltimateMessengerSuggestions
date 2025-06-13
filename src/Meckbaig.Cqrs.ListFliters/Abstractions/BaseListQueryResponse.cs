using Meckbaig.Cqrs.Abstractons;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace Meckbaig.Cqrs.ListFliters.Abstractions
{
	public abstract class BaseListQueryResponse<TResult> : BaseResponse where TResult : IBaseDto
	{
		/// <summary>
		/// Gets or sets items of the response.
		/// </summary>
		public virtual required IList<TResult> Items { get; set; }
	}
}
