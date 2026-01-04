using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerConsumerClean
{
    static class Supervisor
    {
        static readonly object _lock = new object();
        static readonly Queue<int> _buffer = new Queue<int>();

        static int _bufferSize = 50;
        static int _activeProducers = 0;
        static bool _producersStopped = false;

        public static void ProducerStarts()
        {
            lock (_lock) { _activeProducers++; }
        }

        public static void ProducerStops()
        {
            lock (_lock)
            {
                _activeProducers--;
                if (_activeProducers <= 0)
                {
                    _producersStopped = true;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        public static void Produce(int number)
        {
            lock (_lock)
            {
                while (_buffer.Count >= _bufferSize)
                    Monitor.Wait(_lock);

                _buffer.Enqueue(number);
                Monitor.PulseAll(_lock);
            }
        }

        public static bool TryConsume(out int value)
        {
            lock (_lock)
            {
                while (_buffer.Count == 0)
                {
                    if (_producersStopped)
                    {
                        value = default;
                        return false;
                    }
                    Monitor.Wait(_lock);
                }

                value = _buffer.Dequeue();
                Monitor.PulseAll(_lock);
                return true;
            }
        }
    }

    class Producer
    {
        readonly int _from, _to;

        public Producer(int from, int to)
        {
            _from = from; _to = to;
        }

        public void Make()
        {
            Supervisor.ProducerStarts();
            try
            {
                for (int i = _from; i <= _to; i++)
                    if (IsPrime(i))
                        Supervisor.Produce(i);
            }
            finally
            {
                Supervisor.ProducerStops();
            }
        }

        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            int limit = (int)Math.Sqrt(n);
            for (int i = 3; i <= limit; i += 2)
                if (n % i == 0) return false;

            return true;
        }
    }

    class Consumer
    {
        readonly ConsoleColor _color;
        static readonly object _consoleLock = new object();

        public Consumer(ConsoleColor color)
        {
            _color = color;
        }

        public void Consume()
        {
            while (Supervisor.TryConsume(out int x))
            {
                lock (_consoleLock)
                {
                    Console.ForegroundColor = _color;
                    Console.WriteLine(x);
                }
            }

            lock (_consoleLock)
            {
                Console.ResetColor();
                Console.WriteLine("No more producer -> consumer stopped");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var p1 = new Producer(1000, 2000);
            var p2 = new Producer(2001, 3000);
            var p3 = new Producer(3001, 4000);
            var p4 = new Producer(4001, 5000);

            var c1 = new Consumer(ConsoleColor.Blue);
            var c2 = new Consumer(ConsoleColor.Yellow);

            var t1 = new Thread(p1.Make);
            var t2 = new Thread(p2.Make);
            var t3 = new Thread(p3.Make);
            var t4 = new Thread(p4.Make);

            var t5 = new Thread(c1.Consume);
            var t6 = new Thread(c2.Consume);

            t1.Start(); t2.Start(); t3.Start(); t4.Start();
            t5.Start(); t6.Start();

            t1.Join(); t2.Join(); t3.Join(); t4.Join();
            t5.Join(); t6.Join();

            Console.ResetColor();
            Console.WriteLine("All threads finished.");
            Console.ReadKey();
        }
    }
}
