/*
 * An example which proves how wrong the 'node is cancer' article is wrong about
 * event loops. Well, the Fibonacci calculation is not executed in the
 * event loop, but in a different thread, but the api exposed in the loop
 * class makes it easy to utilize the thread pool.
 * */
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using LibuvSharp;
using LibuvSharp.Threading;

namespace Test
{
	class MainClass
	{
		public static BigInteger Fibonacci(int n)
		{
			switch (n) {
			case 0:
				return new BigInteger(0);
			case 1:
				return new BigInteger(1);
			default:
				return Fibonacci(n - 1) + Fibonacci(n - 2);
			}
		}

		public static void Main(string[] args)
		{
			var stdin = new Pipe();
			stdin.Open((IntPtr)0);
			stdin.Read(Encoding.ASCII, (str) => {
				str = str.TrimEnd(new char[] { '\r', '\n' });
				if (str.StartsWith("fib ")) {
					int n;
					if (!int.TryParse(str.Substring("fib ".Length), out n)) {
						Console.WriteLine("Supply an integer to the fib command");
						return;
					}
					TimeSpan span;
					BigInteger res;
					Loop.Default.QueueUserWorkItem(() => {
						var now = DateTime.Now;
						res = Fibonacci(n);
						span = DateTime.Now - now;
					}, () => {
						Console.WriteLine("{0}: fib({1}) = {2}", span, n, res);
					});
				} else if (str == "quit") {
					stdin.Close();
				} else if (str == "help") {
					Console.WriteLine("Available commands: ");
					Console.WriteLine("fib <n:int> - start a thread which calculates fib");
					Console.WriteLine("help - displays help");
					Console.WriteLine("quit - quits the program");
				} else {
					Console.WriteLine("Uknown command");
				}
			});
			stdin.Resume();
			Loop.Default.Run();
		}
	}
}
