// Copyright (c) 2018-2021 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Networking
{
	/// <summary>
	/// Extension methods for <see cref="UnityWebRequest"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class UnityWebRequestExtensions
	{
		[Serializable]
		private struct ArrayProxy<TElement>
		{
			public TElement[] arr;
		}

		public static Task SendWebRequestAsync(this UnityWebRequest www, string tag, CancellationToken cancellationToken = default)
		{
			return SendWebRequestAsync<object>(www, tag, null, cancellationToken);
		}

		public static Task<TResponse> SendWebRequestAsync<TResponse>(this UnityWebRequest www, string tag, CancellationToken cancellationToken = default) where TResponse : class
		{
			return SendWebRequestAsync<TResponse>(www, tag, null, cancellationToken);
		}

		public static Task SendWebRequestNothrowAsync(this UnityWebRequest www, string tag, CancellationToken cancellationToken = default)
		{
			return SendWebRequestAsync<object>(www, tag, null, cancellationToken, false, true);
		}

		public static Task<TResponse> SendWebRequestNothrowAsync<TResponse>(this UnityWebRequest www, string tag, CancellationToken cancellationToken = default) where TResponse : class
		{
			return SendWebRequestAsync<TResponse>(www, tag, null, cancellationToken, false, true);
		}

		public static Task<TResponse> SendWebRequestAsync<TResponse>(this UnityWebRequest www, string tag, TResponse responseData, CancellationToken cancellationToken, bool throwOnError = true, bool loggingEnabled = true) where TResponse : class
		{
			if (www is null)
			{
				throw new ArgumentNullException(nameof(www));
			}

			var tcs = new TaskCompletionSource<TResponse>(www, TaskCreationOptions.RunContinuationsAsynchronously);

			if (cancellationToken.IsCancellationRequested)
			{
				if (throwOnError)
				{
					return Task.FromCanceled<TResponse>(cancellationToken);
				}
				else
				{
					return Task.FromResult<TResponse>(null);
				}
			}
			else
			{
				if (www.downloadHandler is null && typeof(TResponse) != typeof(object))
				{
					www.downloadHandler = new DownloadHandlerBuffer();
				}

				if (loggingEnabled)
				{
					LogRequest(tag, www);
				}

				var wwwOp = www.SendWebRequest();

				if (wwwOp != null)
				{
					if (cancellationToken.CanBeCanceled)
					{
						cancellationToken.Register(
							() =>
							{
								if (!tcs.Task.IsCompleted)
								{
									www.Abort();

									if (throwOnError)
									{
										tcs.TrySetCanceled(cancellationToken);
									}
									else
									{
										tcs.TrySetResult(null);
									}
								}
							},
							true);
					}

					wwwOp.completed += op =>
					{
						try
						{
							if (cancellationToken.IsCancellationRequested)
							{
								if (throwOnError)
								{
#if UNITY_EDITOR || DEBUG
									if (loggingEnabled)
									{
										LogCancelled(tag, www);
									}
#endif
									tcs.TrySetCanceled(cancellationToken);
								}
								else
								{
									LogCancelled(tag, www);
									tcs.TrySetResult(null);
								}
							}
							else if (www.result != UnityWebRequest.Result.Success)
							{
								if (throwOnError)
								{
#if UNITY_EDITOR || DEBUG
									if (loggingEnabled)
									{
										LogFailed(tag, www);
									}
#endif
									tcs.TrySetException(new UnityWebRequestException(www));
								}
								else
								{
									LogFailed(tag, www);
									tcs.TrySetResult(null);
								}
							}
							else
							{
								if (typeof(TResponse) == typeof(string))
								{
									tcs.TrySetResult(www.downloadHandler.text as TResponse);
								}
								else if (typeof(TResponse) == typeof(byte[]))
								{
									tcs.TrySetResult(www.downloadHandler.data as TResponse);
								}
								else if (typeof(TResponse) == typeof(Texture2D))
								{
									tcs.TrySetResult((www.downloadHandler as DownloadHandlerTexture)?.texture as TResponse);
								}
								else if (typeof(TResponse) == typeof(AudioClip))
								{
									tcs.TrySetResult((www.downloadHandler as DownloadHandlerAudioClip)?.audioClip as TResponse);
								}
								else if (typeof(TResponse) == typeof(AssetBundle))
								{
									tcs.TrySetResult((www.downloadHandler as DownloadHandlerAssetBundle)?.assetBundle as TResponse);
								}
								else if (typeof(TResponse) == typeof(object))
								{
									tcs.TrySetResult(null);
								}
								else
								{
									var responseText = www.downloadHandler.text;
#if UNITY_EDITOR || DEBUG
									if (loggingEnabled)
									{
										LogResponse(tag, www, responseText);
									}
#endif
									if (string.IsNullOrEmpty(responseText))
									{
										tcs.TrySetResult(null);
									}
									else if (typeof(TResponse).IsArray)
									{
										// NOTE: JsonUtility does not support root arrays. The workaround is to parse ArrayProxy<> instead.
										var jsonType = typeof(ArrayProxy<>).MakeGenericType(typeof(TResponse).GetElementType());
										var jsonFieldName = nameof(ArrayProxy<object>.arr);
										var json = $"{{\"{jsonFieldName}\":{responseText}}}";
										var rd = JsonUtility.FromJson(json, jsonType);
										var array = jsonType.GetField(jsonFieldName).GetValue(rd) as TResponse;
										tcs.TrySetResult(array);
									}
									else
									{
										if (responseData is null)
										{
											responseData = JsonUtility.FromJson<TResponse>(responseText);
										}
										else
										{
											JsonUtility.FromJsonOverwrite(responseText, responseData);
										}

										tcs.TrySetResult(responseData);
									}
								}
							}
						}
						catch (Exception e)
						{
							if (throwOnError)
							{
								tcs.TrySetException(e);
							}
							else
							{
								Debug.LogException(new UnityWebRequestException(www, e));
								tcs.TrySetResult(null);
							}
						}
					};
				}
				else
				{
					// NOTE: wwwOp might be null if the request is cancelled.
					if (throwOnError)
					{
						tcs.TrySetCanceled(cancellationToken);
					}
					else
					{
						tcs.TrySetResult(null);
					}
				}

				return tcs.Task;
			}
		}

		private static void LogRequest(string tag, UnityWebRequest www)
		{
			if (string.IsNullOrEmpty(tag))
			{
				Debug.Log($"<color=green>{www.method}</color> {www.url}");
			}
			else
			{
				Debug.Log($"[{tag}] <color=green>{www.method}</color> {www.url}");
			}
		}

		private static void LogResponse(string tag, UnityWebRequest www, string responseText)
		{
			if (string.IsNullOrEmpty(tag))
			{
				Debug.Log($"<color=green>{www.method}</color> {www.uri.GetLeftPart(UriPartial.Path)} response: {responseText}");
			}
			else
			{
				Debug.Log($"[{tag}] <color=green>{www.method}</color> {www.uri.GetLeftPart(UriPartial.Path)} response: {responseText}");
			}
		}

		private static void LogCancelled(string tag, UnityWebRequest www)
		{
			if (string.IsNullOrEmpty(tag))
			{
				Debug.LogWarning($"{www.url} cancelled");
			}
			else
			{
				Debug.LogWarning($"[{tag}] {www.url} cancelled");
			}
		}

		private static void LogFailed(string tag, UnityWebRequest www)
		{
			if (string.IsNullOrEmpty(tag))
			{
				Debug.LogError($"{www.url} failed ({www.result}): {www.error}");
			}
			else
			{
				Debug.LogError($"[{tag}] {www.url} failed ({www.result}): {www.error}");
			}
		}
	}
}
