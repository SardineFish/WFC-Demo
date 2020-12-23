using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SardineFish.Utils
{
    public class ParallelCoroutineRunner
    {
        private List<CoroutineRunner> runners = new List<CoroutineRunner>(16);

        public bool Aborted { get; private set; } = false;
        public bool Completed { get; private set; } = false;

        public bool Running => !Aborted && !Completed;

        public bool Tick()
        {
            if (Aborted)
                return false;
            
            var keepRunning = false;
            for (var i = 0; i < runners.Count; i++)
            {
                if (!runners[i].Running)
                    continue;

                keepRunning |= runners[i].Tick();
                
                if (Aborted)
                    return false;
            }

            if (!keepRunning)
            {
                Completed = true;
                return false;
            }

            return true;
        }

        public void Append(IEnumerator coroutine)
        {
            if (Completed || Aborted)
            {
                Debug.LogWarning("Attempt to add coroutine to terminated runner.");
                return;
            }
            runners.Add(new CoroutineRunner(coroutine));
        }

        public void Abort()
        {
            if (!Completed)
            {
                Aborted = true;
                foreach(var runner in runners)
                    runner.Abort();
            }
        }
    }
    public class CoroutineRunner
    {
        Stack<IEnumerator> runStack = new Stack<IEnumerator>();
        private IEnumerator iterator;
        private bool shouldPop = false;
        
        public bool Aborted { get; private set; } 
        public bool Completed { get; private set; }

        public bool Running => !Aborted && !Completed;

        public event Action OnAbort;

        public CoroutineRunner(IEnumerator coroutine)
        {
            runStack.Push(coroutine);
            shouldPop = true;
        }

        public bool Tick()
        {
            if (Aborted)
            {
                Completed = false;
                return false;
            }

            do
            {
                if (shouldPop)
                {
                    iterator = runStack.Pop();
                    shouldPop = false;
                }

                for (var state = iterator.MoveNext(); state; state = iterator.MoveNext())
                {
                    if (iterator.Current is null)
                    {
                        return true;
                    }
                    else if (iterator.Current is IEnumerator next)
                    {
                        runStack.Push(iterator);
                        runStack.Push(next);
                        shouldPop = true;
                        break;
                    }
                }

                shouldPop = true;
            } while (runStack.Count > 0);

            Completed = true;
            Aborted = false;
            return false;
        }

        public void Run()
        {
            while (Tick()) ;
        }

        public void Abort()
        {
            Aborted = true;
            OnAbort?.Invoke();
        }
        
        
        public static void Run(IEnumerator coroutine)
        {
            Stack<IEnumerator> runStack = new Stack<IEnumerator>();
            runStack.Push(coroutine);
            while (runStack.Count > 0)
            {
                var iterator = runStack.Pop();
                for (var state = iterator.MoveNext(); state; state = iterator.MoveNext())
                {
                    if(iterator.Current is null)
                        continue;
                    else if (iterator.Current is IEnumerator next)
                    {
                        runStack.Push(iterator);
                        runStack.Push(next);
                        break;
                    }
                }
            }
        }

        public static IEnumerator RunProgressive(IEnumerator coroutine)
        {
            Stack<IEnumerator> runStack = new Stack<IEnumerator>();
            runStack.Push(coroutine);
            while (runStack.Count > 0)
            {
                var iterator = runStack.Pop();
                for (var state = iterator.MoveNext(); state; state = iterator.MoveNext())
                {
                    if (iterator.Current is null)
                    {
                        yield return null;
                    }
                    else if (iterator.Current is IEnumerator next)
                    {
                        runStack.Push(iterator);
                        runStack.Push(next);
                        break;
                    }
                }
            }
        }
        
        public static IEnumerator All(IEnumerable<IEnumerator> coroutines)    
        {
            var list = new List<IEnumerator>();
            list.Clear();
            list.AddRange(coroutines.Select(RunProgressive));

            bool keepRunning = true;
            while (keepRunning)
            {
                keepRunning = false;
                foreach (var coroutine in list)
                {
                    keepRunning |= coroutine.MoveNext();
                }

                if (!keepRunning)
                    break;
                yield return null;
            }
            
        }
    }
}