using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class LaterContext<TSelf>
    {
        public TimeSpan Delay, Cooldown = default;
        public Action<TSelf> Action;
        public Task Task = Task.CompletedTask;
        public Dictionary<string, object> Memory = new Dictionary<string, object>(8);

        public CancellationTokenSource TokenSource = new CancellationTokenSource();

        public LaterContext(TimeSpan tickrate, Action<TSelf> action)
        {
            Delay = tickrate;
            Action = action;
        }

        public Task InvokeLater()
        {
            if (!this.Task.IsCompleted) return this.Task;

            this.Task = Task.Run(async () =>
            {
                if (this.Delay != default)
                    await Task.Delay(this.Delay, TokenSource.Token);

                if (!TokenSource.Token.IsCancellationRequested)
                    this.Action.DynamicInvoke(this);

                if (this.Cooldown != default)
                    await Task.Delay(this.Cooldown, TokenSource.Token);
            });

            return this.Task;
        }
    }
    public class LaterContext : LaterContext<LaterContext>
    {
        public LaterContext(TimeSpan tickrate, Action<LaterContext> action) : base(tickrate, action) { }
    }

    public class ProgressContext<TSelf>
        : LaterContext<TSelf>, IDisposable
    {
        public int CursorX = -1, CursorY = -1;

        public float ProgressCurrent = -1, ProgressTotal = -1;
        public float ProgressPercent => (ProgressCurrent / ProgressTotal) * 100;

        public DateTime StartTime = DateTime.Now;

        public ProgressContext(TimeSpan tickrate, Action<TSelf> action) : base(tickrate, action) { }

        public virtual void AnnounceProgress(string msg)
        {

        }

        public virtual void Dispose()
        {
            TokenSource.Cancel();
        }
    }
    public class ProgressContext : ProgressContext<ProgressContext>
    {
        public ProgressContext(TimeSpan tickrate, Action<ProgressContext> action) : base(tickrate, action) { }

        public override void Dispose()
        {
            TokenSource.Cancel();
            this.Action(this);
        }
    }

    public class ProgressContextConsole : ProgressContext<ProgressContextConsole>
    {
        public ProgressContextConsole(TimeSpan tickrate, Action<ProgressContextConsole> action) : base(tickrate, action) { }

        public override void AnnounceProgress(string msg)
        {
            /* Console */
            if (this.CursorX < 0 || this.CursorY < 0)
            {
                this.CursorX = Console.CursorLeft;
                this.CursorY = Console.CursorTop;
                Console.WriteLine();
            }

            int prevcursorX = Console.CursorLeft;
            int prevcursorY = Console.CursorTop;
            Console.CursorLeft = this.CursorX;
            Console.CursorTop = this.CursorY;

            ConsoleColor prevfg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($" % ");
            Console.ForegroundColor = ProgressCurrent >= ProgressTotal ? ConsoleColor.Green : (prevfg == ConsoleColor.Gray ? ConsoleColor.DarkGray : (ConsoleColor)((byte)prevfg ^ 0x08));
            Console.Write($" ");
            Console.WriteLine(msg);
            Console.ForegroundColor = prevfg;

            Console.CursorLeft = prevcursorX;
            Console.CursorTop = prevcursorY;
        }

        public override void Dispose()
        {
            TokenSource.Cancel();
            this.Action(this);

            this.ProgressCurrent = 0;
            this.CursorX = -1;
            this.CursorY = -1;
        }
    }
}
