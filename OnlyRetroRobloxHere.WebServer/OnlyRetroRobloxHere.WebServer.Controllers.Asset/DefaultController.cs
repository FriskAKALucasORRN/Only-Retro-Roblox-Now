using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlyRetroRobloxHere.Common;

namespace OnlyRetroRobloxHere.WebServer.Controllers.Asset;

[ApiController]
[Route("Asset")]
[Route("Asset/Default.ashx")]
[Route("Data/Get.ashx")]
public class DefaultController : ControllerBase
{
	private const ulong PlaceId = 1818uL;

	private readonly ILogger<DefaultController> _logger;

	public DefaultController(ILogger<DefaultController> logger)
	{
		_logger = logger;
	}

	private static string? GetAssetFileWithId(ulong id)
	{
		string path = Path.Combine(PathHelper.Clients, Config.Instance.Client.ClientName, "assets");
		Directory.CreateDirectory(path);
		Directory.CreateDirectory(PathHelper.Character);
		string searchPattern = $"{id}.*";
		string[] files = Directory.GetFiles(path, searchPattern);
		if (files.Length != 0)
		{
			return files[0];
		}
		files = Directory.GetFiles(PathHelper.Character, searchPattern);
		if (files.Length != 0)
		{
			return files[0];
		}
		foreach (string assetPackDirectory in Common.AssetPackDirectories)
		{
			if (Directory.Exists(assetPackDirectory))
			{
				files = Directory.GetFiles(assetPackDirectory, searchPattern);
				if (files.Length != 0)
				{
					return files[0];
				}
			}
		}
		return null;
	}

	private static string ProcessScript(string scriptText, ulong id)
	{
		scriptText = scriptText.Replace("{experimentalplayerlistenabled}", Config.Instance.User.Launch.ExperimentalPlayerlistEnabled.ToString().ToLowerInvariant());
		if (Config.Instance.Client.SignAssetScripts)
		{
			scriptText = ScriptSigner.Sign(scriptText, id);
		}
		return scriptText;
	}

	private static bool IsRedirectStatusCode(HttpStatusCode code)
	{
		if (code == HttpStatusCode.Found || (uint)(code - 307) <= 1u)
		{
			return true;
		}
		return false;
	}

	private async Task<IActionResult> RedirectToRoblox()
	{
        string baseUrl = SecureSettings.Default.AssetUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://assetdelivery.roblox.com/v1/asset/";
        }

        string url = $"{baseUrl}{base.Request.QueryString}";

        Logger.Instance.Info($"Asset URL: {url}");
        if (Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens && string.IsNullOrEmpty(SecureSettings.Default.RobloxCookie))
		{
			return File(await Common.HttpClient.GetByteArrayAsync(url), "application/octet-stream");
		}
		if (Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens || !string.IsNullOrEmpty(SecureSettings.Default.RobloxCookie))
		{
			HttpResponseMessage httpResponseMessage = await Common.HttpClientAssetDelivery.GetAsync(url);
			if (!IsRedirectStatusCode(httpResponseMessage.StatusCode) && !httpResponseMessage.IsSuccessStatusCode)
			{
				Logger.Instance.Warn($"Asset delivery secure: got {httpResponseMessage.StatusCode}");
				return Redirect(url);
			}
			if (!httpResponseMessage.Headers.TryGetValues("Location", out IEnumerable<string> values))
			{
				Logger.Instance.Warn("Asset delivery secure: could not find Location header");
				return Redirect(url);
			}
			string text = values.First();
			if (Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens)
			{
				return File(await Common.HttpClientAssetDelivery.GetByteArrayAsync(text), "application/octet-stream");
			}
			return Redirect(text);
		}
		return Redirect(url); //Dear matt, WHY SO MANY IF STATEMENTS? I HATE THEM. -Sincerely, Frisk/Lucas
    }

	[HttpGet]
	[HttpHead]
	public async Task<IActionResult> Get([FromQuery] ulong id = 0uL, [FromQuery] ulong assetVersionId = 0uL, [FromQuery] int version = 0)
	{
		if (assetVersionId != 0L && AssetVersionIdHelper.Map.ContainsKey(assetVersionId))
		{
			id = AssetVersionIdHelper.Map[assetVersionId];
		}
		if (id == 0L || version != 0)
		{
			return await RedirectToRoblox();
		}
		if (id == 1818)
		{
			if (Config.Instance.User.Launch.SelectedMap == null)
			{
				return StatusCode(500);
			}
			string text = Path.Combine(Utils.GetMapsDirectory(), Config.Instance.User.Launch.SelectedMap);
			if (!System.IO.File.Exists(text))
			{
				return StatusCode(500);
			}
			byte[] array = System.IO.File.ReadAllBytes(text);
			if (text.EndsWith(".gz"))
			{
				if (!Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens)
				{
					base.Response.Headers.Add("Content-Encoding", "gzip");
				}
				else
				{
					array = GZipStream.UncompressBuffer(array);
				}
			}
			return File(array, "application/octet-stream");
		}
		LocalAssetHelper.AssetResult assetData = LocalAssetHelper.Instance.GetAssetData(id);
		if (assetData.Success)
		{
			byte[] array2 = assetData.File;
			if (assetData.Compressed)
			{
				if (!Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens)
				{
					base.Response.Headers.Add("Content-Encoding", "gzip");
				}
				else
				{
					array2 = GZipStream.UncompressBuffer(array2);
				}
			}
			if (assetData.IsLua)
			{
				string value = ProcessScript(Encoding.UTF8.GetString(array2), id);
				return Ok(value);
			}
			return File(array2, "application/octet-stream");
		}
		string assetFileWithId = GetAssetFileWithId(id);
		if (assetFileWithId == null)
		{
			return await RedirectToRoblox();
		}
		if (assetFileWithId.EndsWith(".lua"))
		{
			string value2 = ProcessScript(System.IO.File.ReadAllText(assetFileWithId), id);
			return Ok(value2);
		}
		byte[] array3 = System.IO.File.ReadAllBytes(assetFileWithId);
		if (assetFileWithId.EndsWith(".gz"))
		{
			if (!Config.Instance.Client.ClientWillDieIfAHttpRedirectHappens)
			{
				base.Response.Headers.Add("Content-Encoding", "gzip");
			}
			else
			{
				array3 = GZipStream.UncompressBuffer(array3);
			}
		}
		return File(array3, "application/octet-stream");
	}
}
