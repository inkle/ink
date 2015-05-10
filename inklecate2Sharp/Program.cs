using System;

namespace inklecate2Sharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			foreach (string arg in args) {
				Console.WriteLine (arg);
			}
		}
	}
}
