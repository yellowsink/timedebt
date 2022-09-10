namespace TimeDebt;

public static class TimeDebt
{
	/// <summary>
	/// Tracks time taken by an action as it attempts to run it repeatedly
	/// </summary>
	/// <param name="targetRate">How many times per second we are aiming to run the function</param>
	/// <param name="timed">The function being ran and timed</param>
	/// <param name="loopPredicate">Tests if to continue looping or not</param>
	/// <param name="skipAct">An action to run when a tick is skipped. Leave null to disable skipping</param>
	/// <param name="preTimed">A function to run before skipping and before the timed func</param>
	public static void TrackTime(double  targetRate,     Action<long> timed, Predicate<int> loopPredicate,
								 Action? skipAct = null, Action?      preTimed = null)
	{
		var iterTicks = (long) (10_000_000 / targetRate);
		var startTime = DateTime.UtcNow.Ticks;
		var timeDebt  = 0L; // the namesake is here :p

		for (var loopCount = 1; loopPredicate(loopCount); loopCount++)
		{
			preTimed?.Invoke();

			if (timeDebt > iterTicks && skipAct != null)
			{
				timeDebt -= iterTicks;
				skipAct();
				continue;
			}

			timed(timeDebt);

			// measure current time
			var current = DateTime.UtcNow.Ticks - startTime;

			// the full amount of time we are behind by
			var amountBehind = current - (loopCount * iterTicks) + timeDebt;

			// timeDebt has been accounted for, reset it!
			timeDebt = 0;

			// ideally how long we need to wait for (how far *ahead* we are if you will)
			var waitTime = iterTicks - amountBehind;

			if (waitTime > 0)
				Thread.Sleep(new TimeSpan(waitTime));
			else
				// if we can't fully make up time try to do it later
				timeDebt += -waitTime;
		}
	}

	/// <summary>
	/// Tracks time taken by an action as it attempts to run it repeatedly
	/// </summary>
	/// <param name="targetRate">How many times per second we are aiming to run the function</param>
	/// <param name="timed">The function being ran and timed</param>
	/// <param name="loopPredicate">Tests if to continue looping or not</param>
	/// <param name="skipAct">An action to run when a tick is skipped. Leave null to disable skipping</param>
	/// <param name="preTimed">A function to run before skipping and before the timed func</param>
	public static async Task TrackTimeAsync(double targetRate, Func<long, Task> timed, Predicate<int> loopPredicate,
											Func<Task>? skipAct = null, Func<Task>? preTimed = null)
	{
		var iterTicks = (long) (10_000_000 / targetRate);
		var startTime = DateTime.UtcNow.Ticks;
		var timeDebt  = 0L; // the namesake is here :p

		for (var loopCount = 1; loopPredicate(loopCount); loopCount++)
		{
			if (preTimed != null)
				await preTimed();

			if (timeDebt > iterTicks && skipAct != null)
			{
				timeDebt -= iterTicks;
				await skipAct();
				continue;
			}

			await timed(timeDebt);

			// measure current time
			var current = DateTime.UtcNow.Ticks - startTime;

			// the full amount of time we are behind by
			var amountBehind = current - (loopCount * iterTicks) + timeDebt;

			// timeDebt has been accounted for, reset it!
			timeDebt = 0;

			// ideally how long we need to wait for (how far *ahead* we are if you will)
			var waitTime = iterTicks - amountBehind;

			if (waitTime > 0)
				Thread.Sleep(new TimeSpan(waitTime));
			else
				// if we can't fully make up time try to do it later
				timeDebt += -waitTime;
		}
	}

	/// <summary>
	/// Tracks time taken by an action as it attempts to run it repeatedly
	/// </summary>
	/// <param name="targetRate">How many times per second we are aiming to run the function</param>
	/// <param name="timed">The function being ran and timed</param>
	/// <param name="loopPredicate">Tests if to continue looping or not</param>
	/// <param name="cToken">A cancellation token which allows you to end the loop early</param>
	/// <param name="skipAct">An action to run when a tick is skipped. Leave null to disable skipping</param>
	/// <param name="preTimed">A function to run before skipping and before the timed func</param>
	public static void TrackTime(double            targetRate, Action<long> timed, Predicate<int> loopPredicate,
								 CancellationToken cToken,     Action?      skipAct = null, Action? preTimed = null)
		=> TrackTime(targetRate,
					 timed,
					 lCount => !cToken.IsCancellationRequested && loopPredicate(lCount),
					 skipAct,
					 preTimed);

	/// <summary>
	/// Tracks time taken by an action as it attempts to run it repeatedly
	/// </summary>
	/// <param name="targetRate">How many times per second we are aiming to run the function</param>
	/// <param name="timed">The function being ran and timed</param>
	/// <param name="loopPredicate">Tests if to continue looping or not</param>
	/// <param name="cToken">A cancellation token which allows you to end the loop early</param>
	/// <param name="skipAct">An action to run when a tick is skipped. Leave null to disable skipping</param>
	/// <param name="preTimed">A function to run before skipping and before the timed func</param>
	public static Task TrackTimeAsync(double targetRate, Func<long, Task> timed, Predicate<int> loopPredicate,
									  CancellationToken cToken, Func<Task>? skipAct = null, Func<Task>? preTimed = null)
		=> TrackTimeAsync(targetRate,
						  timed,
						  lCount => !cToken.IsCancellationRequested && loopPredicate(lCount),
						  skipAct,
						  preTimed);
}