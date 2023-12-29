using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AppoMobi.Specials;

public class Tasks
{
    private Timer _timer;

    protected static Dictionary<string, RunningTimer> Timers { get; } = new();

    /// <summary>
    /// In MS
    /// </summary>
    /// <param name="interval"></param>
    protected void SetTimer(double ms)
    {
        _timer = new Timer(ms);
        _timer.Elapsed += NotifyTimerElapsed;
        _timer.Enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="task"></param>
    public static void StartDelayedAsync(TimeSpan delay, Func<Task> task)
    {
        StartDelayedAsync(delay, default, task);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="task"></param>
    public static void StartDelayedAsync(TimeSpan delay, CancellationToken cancellationToken, Func<Task> task)
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
                await task();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Start action with delay. Beware this is not garanteed to run on background thread at all.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    public static void StartDelayed(TimeSpan delay, Action action)
    {
        StartDelayed(delay, default, action);
    }

    /// <summary>
    /// Start action with delay. Beware this is not garanteed to run on background thread at all.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="action"></param>
    public static void StartDelayed(TimeSpan delay, CancellationToken cancellationToken, Action action)
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
                action();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }).ConfigureAwait(false);

    }

    public static void StartTimerAsync(TimeSpan when, Func<Task<bool>> task)
    {
        StartTimerAsync(when, default, task);
    }

    public static void StartTimerAsync(TimeSpan when, CancellationToken cancellationToken, Func<Task<bool>> task)
    {
        var repeatDelayMs = when.TotalMilliseconds;
        var id = Guid.NewGuid().ToString();

        async void TimerAction(object state)
        {
            var wrapper = state as RunningTimer;

            if (wrapper.IsExecuting)
                return;

            bool ret;
            wrapper.IsExecuting = true;

            try
            {
                ret = await task();
            }
            catch (Exception e) //we could land here upon cancellation..
            {
                Console.WriteLine(e);
                ret = false; //remove timer
            }


            try
            {
                var myself = Timers[id];
                if (myself != null) myself.Executed++;
                //Debug.WriteLine($"[StartTimer] Exec timer {id} {myself.Executed}");
                var needCancel = false;
                if (cancellationToken != default)
                    if (cancellationToken.IsCancellationRequested)
                        needCancel = true;

                if (!ret || repeatDelayMs < 1 || needCancel)
                    //todo kill timer
                    if (myself != null && myself.Timer != null)
                    {
                        myself.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        myself.Timer.Dispose();
                        Timers[myself.Id] = null;
#if DEBUG
                        //Console.WriteLine($"[StartTimer] Stopped timer {id}");
#endif
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                wrapper.IsExecuting = false;
            }
        }

        var logTimer = new RunningTimer
        {
            Id = id,
            RepeatingDelay = when,
            StartDelay = when
        };
        Timers[id] = logTimer;

        // Register a callback to dispose the timer if the cancellationToken is canceled before it starts.
        if (cancellationToken != default)
            cancellationToken.Register(() =>
            {
                logTimer.Timer?.Change(Timeout.Infinite, Timeout.Infinite);
                logTimer.Timer?.Dispose();
                Timers[id] = null;
            });

        logTimer.Timer =
            new System.Threading.Timer(TimerAction, logTimer, logTimer.StartDelay, logTimer.RepeatingDelay);
        //Debug.WriteLine($"[BackgroundTaskQueue] Added task on timer {id}");

#if DEBUG
        //Console.WriteLine($"[StartTimer] Added task on timer delay {logTimer.StartDelay} repeat {logTimer.RepeatingDelay} id  {id}");
#endif
    }


    public void StartTimerOnce(TimeSpan when, Action action)
    {
        OnElapsed = action;
        SetTimer(when.TotalMilliseconds);
    }

    public event Action OnElapsed;

    private void NotifyTimerElapsed(object source, TimerElapsedEventArgs timerElapsedEventArgs)
    {
        _timer.Enabled = false;
        _timer.Elapsed -= NotifyTimerElapsed;
        _timer.Dispose();
        _timer = null;

        OnElapsed?.Invoke();

        OnElapsed = null;
    }

    [DebuggerDisplay("Every {RepeatingDelay.TotalSeconds} sec(s), Id = {Id}")]
    public class RunningTimer
    {
        public string Id { get; set; }
        public TimeSpan StartDelay { get; set; }
        public TimeSpan RepeatingDelay { get; set; }
        public System.Threading.Timer Timer { get; set; }
        public ulong Executed { get; set; }
        public bool IsExecuting { get; set; }
    }
}