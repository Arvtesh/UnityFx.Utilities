// Copyright (c) 2018-2021 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFx.Tasks
{
	using Object = UnityEngine.Object;
	using AsyncOperation = UnityEngine.AsyncOperation;

	/// <summary>
	/// Extension methods for <see cref="AsyncOperation"/>-base classes.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class AsyncOperationExtensions
	{
		/// <summary>
		/// Creates a <see cref="Task"/> wrapper for <see cref="AsyncOperation"/>.
		/// </summary>
		/// <seealso cref="ToTask(AsyncOperation, CancellationToken)"/>
		public static Task ToTask(this AsyncOperation op)
		{
			return ToTask(op, CancellationToken.None);
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task"/> wrapper for <see cref="AsyncOperation"/>.
		/// </summary>
		/// <seealso cref="ToTask(AsyncOperation)"/>
		public static Task ToTask(this AsyncOperation op, CancellationToken cancellationToken)
		{
			if (op.isDone)
			{
				return Task.CompletedTask;
			}
			else if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled(cancellationToken);
			}
			else
			{
				var tcs = new TaskCompletionSource<object>(op);

				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.Register(() =>
					{
						tcs.TrySetCanceled(cancellationToken);
					});
				}

				op.completed += o =>
				{
					tcs.TrySetResult(null);
				};

				return tcs.Task;
			}
		}

		/// <summary>
		/// Returns the <see cref="AsyncOperation"/> awaiter. This method is intended for compiler use only.
		/// </summary>
		public static CompilerServices.AsyncOperationAwaiter GetAwaiter(this AsyncOperation op)
		{
			return new CompilerServices.AsyncOperationAwaiter(op);
		}

		/// <summary>
		/// Creates a <see cref="Task{TResult}"/> wrapper for <see cref="ResourceRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(ResourceRequest)"/>
		public static Task<Object> ToTask(this ResourceRequest op)
		{
			return ToTask<Object>(op, CancellationToken.None);
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task{TResult}"/> wrapper for <see cref="ResourceRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(ResourceRequest, CancellationToken)"/>
		public static Task<Object> ToTask(this ResourceRequest op, CancellationToken cancellationToken)
		{
			return ToTask<Object>(op, cancellationToken);
		}

		/// <summary>
		/// Creates a <see cref="Task{TResult}"/> wrapper <see cref="ResourceRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(ResourceRequest, CancellationToken)"/>
		public static Task<T> ToTask<T>(this ResourceRequest op) where T : Object
		{
			return ToTask<T>(op, CancellationToken.None);
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task{TResult}"/> wrapper for <see cref="ResourceRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(ResourceRequest)"/>
		public static Task<T> ToTask<T>(this ResourceRequest op, CancellationToken cancellationToken) where T : Object
		{
			if (op.isDone)
			{
				var asset = op.asset;

				if (asset)
				{
					return Task.FromResult((T)asset);
				}
				else
				{
					return Task.FromResult<T>(null);
				}
			}
			else if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<T>(cancellationToken);
			}
			else
			{
				var tcs = new TaskCompletionSource<T>(op, TaskCreationOptions.RunContinuationsAsynchronously);

				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.Register(() =>
					{
						tcs.TrySetCanceled(cancellationToken);
					});
				}

				op.completed += o =>
				{
					try
					{
						var asset = op.asset;

						if (asset)
						{
							tcs.TrySetResult((T)asset);
						}
						else
						{
							tcs.TrySetResult(null);
						}
					}
					catch (Exception e)
					{
						tcs.TrySetException(e);
					}
				};

				return tcs.Task;
			}
		}

		/// <summary>
		/// Returns the <see cref="ResourceRequest"/> awaiter. This method is intended for compiler use only.
		/// </summary>
		public static CompilerServices.ResourceRequestAwaiter GetAwaiter(this ResourceRequest op)
		{
			return new CompilerServices.ResourceRequestAwaiter(op);
		}

		/// <summary>
		/// Creates a <see cref="Task{TResult}"/> wrapper for <see cref="AssetBundleRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(AssetBundleRequest, CancellationToken)"/>
		public static Task<T> ToTask<T>(this AssetBundleRequest op) where T : Object
		{
			return ToTask<T>(op, CancellationToken.None);
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task{TResult}"/> wrapper for <see cref="AssetBundleRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(AssetBundleRequest)"/>
		public static Task<T> ToTask<T>(this AssetBundleRequest op, CancellationToken cancellationToken) where T : Object
		{
			if (op.isDone)
			{
				var asset = op.asset;

				if (asset)
				{
					return Task.FromResult((T)asset);
				}
				else
				{
					return Task.FromResult<T>(null);
				}
			}
			else if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<T>(cancellationToken);
			}
			else
			{
				var tcs = new TaskCompletionSource<T>(op, TaskCreationOptions.RunContinuationsAsynchronously);

				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.Register(() =>
					{
						tcs.TrySetCanceled(cancellationToken);
					});
				}

				op.completed += o =>
				{
					try
					{
						var asset = op.asset;

						if (asset)
						{
							tcs.TrySetResult((T)asset);
						}
						else
						{
							tcs.TrySetResult(null);
						}
					}
					catch (Exception e)
					{
						tcs.TrySetException(e);
					}
				};

				return tcs.Task;
			}
		}

		/// <summary>
		/// Returns the <see cref="AssetBundleRequest"/> awaiter. This method is intended for compiler use only.
		/// </summary>
		public static CompilerServices.AssetBundleRequestAwaiter GetAwaiter(this AssetBundleRequest op)
		{
			return new CompilerServices.AssetBundleRequestAwaiter(op);
		}

		/// <summary>
		/// Creates a <see cref="Task{TResult}"/> wrapper for <see cref="AssetBundleCreateRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask(AssetBundleCreateRequest, CancellationToken)"/>
		public static Task<AssetBundle> ToTask(this AssetBundleCreateRequest op)
		{
			return ToTask(op, CancellationToken.None);
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task{TResult}"/> wrapper for <see cref="AssetBundleCreateRequest"/>.
		/// </summary>
		/// <seealso cref="ToTask(AssetBundleCreateRequest)"/>
		public static Task<AssetBundle> ToTask(this AssetBundleCreateRequest op, CancellationToken cancellationToken)
		{
			if (op.isDone)
			{
				return Task.FromResult(op.assetBundle);
			}
			else if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<AssetBundle>(cancellationToken);
			}
			else
			{
				var tcs = new TaskCompletionSource<AssetBundle>(op, TaskCreationOptions.RunContinuationsAsynchronously);

				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.Register(() =>
					{
						tcs.TrySetCanceled(cancellationToken);
					});
				}

				op.completed += o =>
				{
					tcs.TrySetResult(op.assetBundle);
				};

				return tcs.Task;
			}
		}

		/// <summary>
		/// Returns the <see cref="AssetBundleCreateRequest"/> awaiter. This method is intended for compiler use only.
		/// </summary>
		public static CompilerServices.AssetBundleCreateRequestAwaiter GetAwaiter(this AssetBundleCreateRequest op)
		{
			return new CompilerServices.AssetBundleCreateRequestAwaiter(op);
		}
	}
}
