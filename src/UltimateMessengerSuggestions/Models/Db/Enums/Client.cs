namespace UltimateMessengerSuggestions.Models.Db.Enums;

/// <summary>
/// User client.
/// </summary>
public static class Client
{
	/// <summary>
	/// VKontakte.
	/// </summary>
	public const string Vk = "vk";

	/// <summary>
	/// Telegram Bot.
	/// </summary>
	public const string Telegram = "tg";

	/// <summary>
	/// Checks if the client is valid.
	/// </summary>
	/// <param name="client">Client name</param>
	/// <returns></returns>
	public static bool IsValid(string client)
	{
		return client == Vk || client == Telegram;
	}
}
