// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using RestSharp;

namespace BuildDependency.Tools
{
	static class RestHelper
	{
		public static RestClient CreateRestClient(string baseUrl)
		{
			return new RestClient { BaseUrl = new Uri(baseUrl) };
		}

		public static T Execute<T>(RestClient client, IRestRequest request, bool throwException = true) where T : new()
		{
			var response = client.Execute<T>(request);
			if (response.ErrorException != null)
			{
				if (throwException)
				{
					const string message = "Error retrieving response.  Check inner details for more info.";
					throw new ApplicationException(message, response.ErrorException);
				}
				return default(T);
			}
			return response.Data;
		}

		public static T Execute<T>(string baseUrl, IRestRequest request, bool throwException = true) where T : new()
		{
			var client = CreateRestClient(baseUrl);
			return Execute<T>(client, request, throwException);
		}

	}
}
