using System;
using System.Threading;

using Rhino.Runtime.RhinoAccounts;

namespace Crash.Handlers.Authentication
{

	public static class ClientAuthentication
	{

		public static async Task<Tuple<IOpenIDConnectToken, IOAuth2Token>> GetRhinoToken(string pluginId, string pluginSecret, CancellationToken token)
		{
			Tuple<IOpenIDConnectToken, IOAuth2Token> authTokens = null;

			if (string.IsNullOrEmpty(pluginId) || string.IsNullOrEmpty(pluginSecret))
				return authTokens;

			await RhinoAccountsManager.ExecuteProtectedCodeAsync(async (SecretKey secretKey) =>
			{
				authTokens = RhinoAccountsManager.TryGetAuthTokens(pluginId, secretKey);

				if (authTokens == null)
				{
					authTokens = await RhinoAccountsManager.GetAuthTokensAsync(
						pluginId,
						pluginSecret,
						secretKey,
						token
					);
				}
			});

			return authTokens;
		}

	}

}

