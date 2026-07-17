using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OnlyRetroRobloxHere.Common;

namespace OnlyRetroRobloxHere.WebServer;

internal static class AssetDelivery
{
	private static string CsrfToken;

	public static IEnumerable<AssetDeliveryBatchRequest> ConstructBatchRequest(IEnumerable<ulong> ids)
	{
		List<AssetDeliveryBatchRequest> list = new List<AssetDeliveryBatchRequest>();
		foreach (ulong item2 in ids.Distinct())
		{
			AssetDeliveryBatchRequest item = new AssetDeliveryBatchRequest
			{
				AssetId = item2,
				RequestId = item2.ToString()
			};
			list.Add(item);
		}
		return list;
	}

	private static HttpRequestMessage ConstructHttpRequestMessage(string body)
	{
		return new HttpRequestMessage
		{
			Method = HttpMethod.Post,
            RequestUri = new Uri("https://assetdelivery.roblox.com/v2/assets/batch"),
            Headers = 
			{
				{ "user-agent", "Roblox/WinInet" },
				{
					"Cookie",
					".ROBLOSECURITY=" + SecureSettings.Default.RobloxCookie
				}
			},
			Content = new StringContent(body, Encoding.UTF8, "application/json")
		};
	}

	public static async Task<IEnumerable<AssetInformation>> BatchRequest(IEnumerable<AssetDeliveryBatchRequest> request)
	{
		string body = JsonSerializer.Serialize(request);
		for (int i = 1; i <= 5; i++)
		{
			try
			{
				HttpRequestMessage request2 = ConstructHttpRequestMessage(body);
				HttpResponseMessage httpResponseMessage = await Common.HttpClient.SendAsync(request2);
				if (httpResponseMessage.StatusCode == HttpStatusCode.Forbidden)
				{
					if (httpResponseMessage.Headers.TryGetValues("x-csrf-token", out IEnumerable<string> values))
					{
						CsrfToken = values.First();
						continue;
					}
					break;
				}
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					Logger.Instance.Error($"Got unexpected status code, try {i}: {httpResponseMessage.StatusCode}");
					break;
				}
				return JsonSerializer.Deserialize<AssetInformation[]>(await httpResponseMessage.Content.ReadAsStringAsync());
			}
			catch (Exception value)
			{
				Logger.Instance.Warn($"Got exception, try {i}: {value}");
				continue;
			}
		}
		Logger.Instance.Error("Failed to get asset information! Either ran out of retries or something unexpected occured...");
		return Array.Empty<AssetInformation>();
	}

	public static async Task<IEnumerable<AssetInformation>> BatchRequest(IEnumerable<ulong> ids)
	{
		return await BatchRequest(ConstructBatchRequest(ids));
	}

	static AssetDelivery()
	{
		CsrfToken = "";
	}
}
