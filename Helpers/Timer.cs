using System;
using System.Diagnostics;
using System.Threading;

namespace AppoMobi.Specials;

public class TimerElapsedEventArgs : EventArgs
{
	public TimerElapsedEventArgs(TimeSpan elapsed)
	{
		Elapsed = elapsed;
	}

	public TimeSpan Elapsed { get; private set; }
}

public class Timer : IDisposable
{
	private readonly int _interval; // Interval in microseconds
	private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
	private long _nextTickTime;
	private bool _enabled;
	private readonly object _lock = new object();
	private readonly SynchronizationContext _synchronizationContext;
	private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);

	public Timer(double milliseconds)
	{
		_interval = (int)(milliseconds * 1000.0);
		_nextTickTime = _stopwatch.ElapsedTicks + (long)(_interval * (Stopwatch.Frequency / 1000000.0));
		_synchronizationContext = SynchronizationContext.Current;
	}

	public event EventHandler<TimerElapsedEventArgs> Elapsed;

	public bool Enabled
	{
		get { return _enabled; }
		set
		{
			if (_enabled != value)
			{
				lock (_lock)
				{
					_enabled = value;

					if (_enabled)
					{
						Start();
					}
					else
					{
						Stop();
					}
				}
			}
		}
	}

	public double IntervalMilliseconds
	{
		get { return _interval / 1000.0; }
	}

	private void OnElapsed(TimeSpan elapsed)
	{
		if (_synchronizationContext != null)
		{
			_synchronizationContext.Post(state => Elapsed?.Invoke(this, new TimerElapsedEventArgs((TimeSpan)state)), elapsed);
		}
		else
		{
			Elapsed?.Invoke(this, new TimerElapsedEventArgs(elapsed));
		}
	}

	private void TimerLoop()
	{
		while (_enabled)
		{
			long currentTime;
			lock (_lock)
			{
				currentTime = _stopwatch.ElapsedTicks;

				if (currentTime >= _nextTickTime)
				{
					// Perform the tick action
					OnElapsed(_stopwatch.Elapsed - TimeSpan.FromTicks((long)(_interval * (Stopwatch.Frequency / 1000000.0))));

					_nextTickTime += (long)(_interval * (Stopwatch.Frequency / 1000000.0));
				}
			}

			// Wait until the next tick time
			long delayTicks = _nextTickTime - currentTime;
			if (delayTicks > 0)
			{
				// Calculate the delay time in milliseconds
				int delay = (int)((delayTicks * 1000) / Stopwatch.Frequency);
				if (delay > 0)
				{
					// Use a manual reset event to wait for the next tick time
					_resetEvent.Wait(delay);
				}
			}
		}
	}

	public void Start()
	{
		if (!_enabled)
		{
			lock (_lock)
			{
				_enabled = true;

				var timerThread = new Thread(TimerLoop);
				timerThread.IsBackground = true;
				timerThread.Start();
			}
		}
	}

	public void Stop()
	{
		if (_enabled)
		{
			lock (_lock)
			{
				_enabled = false;
				_nextTickTime = 0;

				// Signal the reset event to unblock the timer loop
				_resetEvent.Set();
			}
		}
	}

	public void Dispose()
	{
		Stop();

		// Dispose of the reset event
		_resetEvent.Dispose();
	}
}