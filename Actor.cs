using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asyncMono
{

    abstract record SpriteHandlerMovment(TaskCompletionSource<object?> task);
    record Move(TaskCompletionSource<object?> task, Vector2 direction, Direction animationDirection, TimeSpan duration) : SpriteHandlerMovment(task);
    record SetPosition(TaskCompletionSource<object?> task, Vector2 position) : SpriteHandlerMovment(task);
    record SetVisible(TaskCompletionSource<object?> task, Boolean visibility) : SpriteHandlerMovment(task);

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    class Actor
    {
        public Vector2 Position => currentPosition;

        public AnimatedSprite? Sprite { get; set; }
        public bool visibile { get; set; }

        private Vector2 currentPosition;
        private Vector2 sourcePosition;
        private Vector2 targetPosition;

        private System.TimeSpan startTime;

        // holds movements that will run in the future
        private Queue<SpriteHandlerMovment> queue = new();

        // holds the current running movement
        private SpriteHandlerMovment? currentOrder;

        const float gridSize = 32;

        ///Tiles per seconds
        private const double Speed = 0.8;

        public Actor() { }
        public Actor(Vector2 vector2)
        {
            this.currentPosition = vector2;
        }


        public Task Up(int times = 1) => Move(new Vector2(0, -1) * times, Direction.Up, System.TimeSpan.FromSeconds(Speed) * times);
        public Task Down(int times = 1) => Move(new Vector2(0, +1) * times, Direction.Down, System.TimeSpan.FromSeconds(Speed) * times);
        public Task Left(int times = 1) => Move(new Vector2(-1, 0) * times, Direction.Left, System.TimeSpan.FromSeconds(Speed) * times);
        public Task Right(int times = 1) => Move(new Vector2(+1, 0) * times, Direction.Right, System.TimeSpan.FromSeconds(Speed) * times);
        public Task Move(Vector2 direction, Direction animationDirection, System.TimeSpan duration)
        {
            var newCompletionSource = new TaskCompletionSource<object?>();
            queue.Enqueue(new asyncMono.Move(newCompletionSource, direction, animationDirection, duration));
            return newCompletionSource.Task;
        }
        public Task SetPosition(Vector2 position)
        {
            var newCompletionSource = new TaskCompletionSource<object?>();
            queue.Enqueue(new asyncMono.SetPosition(newCompletionSource, position));
            return newCompletionSource.Task;
        }

        public Task SetVisibility(bool visible)
        {
            var newCompletionSource = new TaskCompletionSource<object?>();
            queue.Enqueue(new asyncMono.SetVisible(newCompletionSource, visible));
            return newCompletionSource.Task;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (!this.visibile)
                return;
            if (this.Sprite is null)
                return;
            spriteBatch.Draw(Sprite, this.Position * gridSize);
        }

        public void Update(GameTime time)
        {
            Sprite?.Update((float)time.ElapsedGameTime.TotalSeconds);

            if (currentOrder is null)
            {
                if (queue.TryDequeue(out currentOrder))
                {
                    if (currentOrder is Move move)
                    {

                        // setupd source and target
                        startTime = time.TotalGameTime;
                        sourcePosition = currentPosition;
                        targetPosition = currentPosition + move.direction;
                        Sprite?.Play(move.animationDirection.ToString().ToLower());
                    }
                    else if (currentOrder is SetPosition set)
                    {
                        // setup position
                        sourcePosition = set.position;
                        currentPosition = set.position;
                        targetPosition = set.position;
                    }
                    else if (currentOrder is SetVisible visibility)
                    {
                        // setup position
                        this.visibile = visibility.visibility;
                    }
                }
                else
                {
                    return; // we are in no animation and there is no in the cueue
                }
            }

            bool isAnimationFinished = false;

            if (currentOrder is Move currentMove)
            {
                var durationSinceStart = time.TotalGameTime - startTime;
                if (currentMove.duration.TotalSeconds > 0)
                {
                    double amount = durationSinceStart.TotalSeconds / currentMove.duration.TotalSeconds;
                    currentPosition = Lerp(ref sourcePosition, ref targetPosition, amount);
                }

                if (durationSinceStart >= currentMove.duration)
                {
                    isAnimationFinished = true;
                }
            }
            else if (currentOrder is SetPosition || currentOrder is SetVisible)
            {
                // the initilisation already set it.
                // noting more to do 
                isAnimationFinished = true;
            }
            if (isAnimationFinished)
            {
                // finishe and remove move so we can get the next
                currentOrder.task.SetResult(null);
                currentOrder = null;
                Sprite?.Play("idle");
            }
        }


        private Vector2 Lerp(ref Vector2 from, ref Vector2 to, double amount)
        {
            var fAmount = MathHelper.Clamp((float)amount, 0, 1);
            var x = MathHelper.Lerp(from.X, to.X, fAmount);
            var y = MathHelper.Lerp(from.Y, to.Y, fAmount);
            return new Vector2(x, y);
        }
    }
}
