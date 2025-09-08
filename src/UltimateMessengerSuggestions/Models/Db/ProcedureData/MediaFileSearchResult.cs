namespace UltimateMessengerSuggestions.Models.Db.ProcedureData;

/// TODO: remove
public class MediaFileSearchResult
{
	public int Id { get; set; }
	public string PublicId { get; set; }
	public string Description { get; set; }
	public string MediaType { get; set; }
	public string MediaUrl { get; set; }
	public string? VkConversation { get; set; }
	public long? VkMessageId { get; set; }
	public string? Discriminator { get; set; }
	public int? TagId { get; set; }
	public string? TagName { get; set; }

	public override string ToString()
	{
		return $"{Id} {Description} {TagName}";
	}
}
