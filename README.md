# DelegateDecompiler #

Proof-of-concept utility library that decompiles delegates (`Action`, `Func<T>`, etc.) into LINQ expression trees (`Expression<Action>`, `Expression<Func<T>>`, etc.).


### Short demonstration ###

```csharp
interface IFoo
{
    IBar Bar { get; }
}

interface IBar
{
    string Property { get; set; }
}

// This is the lambda we're going to decompile from IL:
int message = "Hello world.";
Action<IFoo> action = foo => foo.Bar.Property = message;

// Decompile it into an expression tree:
Expression expr = action.Decompile();

// Replace property accessor methods with more semantic property expressions:
expr = new SpecialNameMethodReplacer().Visit(expr);

// Evaluate captured variables to their value:
expr = new PartialEvaluator().Visit(expr);

// Show the result:
Console.WriteLine(expr);  // => foo => (foo.Bar.Property = "Hello world!")
```


### Limitations ###

* Multi-statement methods are not yet supported.

* Conditionals, branches, and exception handling are not supported (and probably won't be).

* Currently, this library only targets the full .NET Framework. (Targeting .NET Standard 2.0 should be possible with only a few minor modifications. Earlier .NET Standard versions likely cannot be supported due to missing APIs.)
