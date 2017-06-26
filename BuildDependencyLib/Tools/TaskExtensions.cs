// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Threading.Tasks;

namespace BuildDependency
{
	/// <summary>
	/// Provides synchronous extension methods for tasks.
	/// </summary>
	public static class TaskExtensions
	{
		// Original code comes from
		// https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Tasks/SynchronousTaskExtensions.cs

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		public static void WaitAndUnwrapException(this Task task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		public static T WaitAndUnwrapException<T>(this Task<T> task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));

			return task.GetAwaiter().GetResult();
		}
	}
}
