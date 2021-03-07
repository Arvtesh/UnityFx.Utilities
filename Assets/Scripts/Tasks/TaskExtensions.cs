using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFx.Tasks
{
	/// <summary>
	/// Extension methods for <see cref="Task"/> and <see cref="Task{TResult}"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class TaskExtensions
	{
		#region interface

		/// <summary>
		/// Creates a cancellable <see cref="Task"/> instance.
		/// </summary>
		public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
		{
			if (cancellationToken.CanBeCanceled)
			{
				var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

				cancellationToken.Register(() =>
				{
					tcs.TrySetCanceled(cancellationToken);
				});

				task.ContinueWith(completedTask =>
				{
					if (completedTask.IsCanceled)
					{
						tcs.TrySetCanceled();
					}
					else if (completedTask.IsFaulted)
					{
						tcs.TrySetException(completedTask.Exception.InnerException);
					}
					else
					{
						tcs.TrySetResult(null);
					}
				});

				return tcs.Task;
			}

			return task;
		}

		/// <summary>
		/// Creates a cancellable <see cref="Task{TResult}"/> instance.
		/// </summary>
		public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
		{
			if (cancellationToken.CanBeCanceled)
			{
				var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

				cancellationToken.Register(() =>
				{
					tcs.TrySetCanceled(cancellationToken);
				});

				task.ContinueWith(completedTask =>
				{
					if (completedTask.IsCanceled)
					{
						tcs.TrySetCanceled();
					}
					else if (completedTask.IsFaulted)
					{
						tcs.TrySetException(completedTask.Exception.InnerException);
					}
					else
					{
						tcs.TrySetResult(completedTask.Result);
					}
				});

				return tcs.Task;
			}

			return task;
		}

		/// <summary>
		/// Converts the specified async operation (task) to a <see cref="IEnumerator"/> instance that can be used in Unity coroutines.
		/// </summary>
		/// <param name="op">The source operation (task).</param>
		/// <returns>Returns enumerator that can be used in Unity coroutine.</returns>
		public static IEnumerator ToEnumerator(this IAsyncResult op)
		{
			if (op is IEnumerator e)
			{
				return e;
			}

			return new TaskEnumerator(op);
		}

		#endregion

		#region implementation

		private class TaskEnumerator : IEnumerator
		{
			private readonly IAsyncResult _task;

			public TaskEnumerator(IAsyncResult task)
			{
				_task = task;
			}

			public object Current
			{
				get
				{
					return null;
				}
			}

			public bool MoveNext()
			{
				return !_task.IsCompleted;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}

		#endregion
	}
}
