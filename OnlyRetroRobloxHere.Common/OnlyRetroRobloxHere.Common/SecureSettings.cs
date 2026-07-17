using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnlyRetroRobloxHere.Common;

public class SecureSettings
{
	private static SecureSettings? _default;

	public static SecureSettings Default => _default ?? (_default = Load());

	public static bool Initialized => _default != null;

	public string RobloxCookie { get; set; } = "";

	public string AssetUrl { get; set; } = "";

    private static string? DecryptData(byte[] encryptedData)
	{
		try
		{
			byte[] bytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(bytes);
		}
		catch (CryptographicException value)
		{
			Logger.Instance.Error($"Failed to decrypt SecureSettings: {value}");
			return null;
		}
	}

	private byte[] EncryptData()
	{
		string s = JsonSerializer.Serialize(this);
		return ProtectedData.Protect(Encoding.UTF8.GetBytes(s), null, DataProtectionScope.CurrentUser);
	}

	private static SecureSettings Load()
	{
		if (!File.Exists(PathHelper.SecureSettings))
		{
			return new SecureSettings();
		}
		string text = DecryptData(File.ReadAllBytes(PathHelper.SecureSettings));
		if (text == null)
		{
			return new SecureSettings();
		}
		try
		{
			return JsonSerializer.Deserialize<SecureSettings>(text);
		}
		catch (JsonException value)
		{
			Logger.Instance.Error($"Failed to deserialize SecureSettings: {value}");
			return new SecureSettings();
		}
	}

	public static void Save()
	{
		if (Initialized)
		{
			Default.SaveInternal();
		}
	}

	private void SaveInternal()
	{
		File.WriteAllBytes(PathHelper.SecureSettings, EncryptData());
	}
}
