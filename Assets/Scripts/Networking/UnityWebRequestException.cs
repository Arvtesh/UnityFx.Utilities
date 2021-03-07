// Copyright (c) 2018-2021 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Networking;

namespace UnityFx.Networking
{
	public class UnityWebRequestException : Exception
	{
		public long ResponseCode { get; }

		public UnityWebRequestException(UnityWebRequest www)
			: base(GetMessage(www))
		{
			ResponseCode = www.responseCode;
		}

		public UnityWebRequestException(UnityWebRequest www, Exception innerException)
			: base(GetMessage(www), innerException)
		{
			ResponseCode = www.responseCode;
		}

		private static string GetMessage(UnityWebRequest www)
		{
			if (www is null || www.result == UnityWebRequest.Result.Success || www.result == UnityWebRequest.Result.InProgress)
			{
				return nameof(UnityWebRequestException);
			}
			else if (string.IsNullOrEmpty(www.error))
			{
				if (www.downloadHandler is null || string.IsNullOrWhiteSpace(www.downloadHandler.text))
				{
					return $"WebRequest error {www.result} ({www.responseCode})";
				}
				else
				{
					return $"WebRequest error {www.result} ({www.responseCode}): {www.downloadHandler.text}";
				}
			}
			else
			{
				if (www.downloadHandler is null || string.IsNullOrWhiteSpace(www.downloadHandler.text))
				{
					return www.error;
				}
				else
				{
					return www.error + " " + www.downloadHandler.text;
				}
			}
		}
	}
}
