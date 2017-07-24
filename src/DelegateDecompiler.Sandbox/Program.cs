using System;
using System.Linq.Expressions;

namespace LambdaDecompiler.Sandbox
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            int n = args.Length - 18;
            Action<IFoo> action = foo => foo.Property = n % -1;

            var postProcessors = new ExpressionVisitor[]
            {
                new SpecialNameMethodReplacer(),
                new PartialEvaluator(),
            };

            Expression expr = action.Decompile();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Decompiled as:");
            Console.ResetColor();
            Console.WriteLine(expr);
            Console.WriteLine();

            foreach (var postProcessor in postProcessors)
            {
                expr = postProcessor.Visit(expr);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("After post-processing by " + postProcessor.GetType().Name + ":");
                Console.ResetColor();
                Console.WriteLine(expr);
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Final result:");
            Console.ResetColor();
            Console.WriteLine(expr);
        }
    }
}

public interface IFoo
{
    int Property { get; set; }
    IBar Bar { get; set; }
}

public interface IBar
{
    event Action<IFoo> Event;
}
