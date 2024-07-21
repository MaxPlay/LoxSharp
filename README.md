# LoxSharp

Implementation C# of the language "Lox" designed by Robert Nystrom for the book [Crafting Interpreters](https://craftinginterpreters.com/).

## Structure

The application is split in three parts:
- **LoxSharp** - The project that creates the console application. It depends on *LoxSharp.Core*.
- **LoxSharp.Core** - Based 100% on Part II of the book Crafting Interpreters. It's a recreation of the entire code presented there except for the code generator which is found in *Tool.GenerateAst*.
- **Tool.GenerateAst** - This is the code generator used to generate the Expr and Stmt classes and interfaces. In contrast to the one presented in the book (which is very simple and basically hard coded) this one uses a json based configuration to generate the code.

## Differences between the book and the code

Since this is C# and I don't like boxing, the whole "memory" stuff is wrapped in a variant type called [RuntimeValue](LoxSharp.Core/RuntimeValue.cs). Instead of using the basic object datatype, this type is a struct, so we pass it by ref or out parameters and all the value types it can hold are either contained in the type itself or can be placed into "object", because they are classes.

Due to the nature of this, the code has some more checking against types and null going on and I am obviously using modern C# features like explicit nullable types and pattern matching.

## License

See [LICENSE.txt](LICENSE.txt).