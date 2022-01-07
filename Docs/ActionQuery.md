# ActionQuery language (AQ) version 1.1

See also [creating a host](AQHost.md) for details on how to host ActionQuery scripts.

## New in v1.1

The host is required to support variables and syntax for accessing them is added:

`$var1` - gets the variable named `var1`

`$var1(100)` - sets the variable `var1` to 100.

Support for comments is added. The comments are started with the # sign and end with the end of the line. They can start at the beginning of a line or after the meaningful code on the line. 

```
# Set variable var1 to 100
$var1(100),
# Start decrementing it until we reach 0
while ( 
    $var1(Add($var1,-1)),
    # Display the current value of var1
    Echo($var1)
)
```
## Terminology

ActionQuery scripts are usually called shortly `AC scripts`.

`Parameter` is a set of named values the AC script can fetch through the host by using the parameter name in the source. E.g. `Echo(param1)` will call the host's function Echo with the value of `param1`. To obtain its value the AC will call the host's `EvalParam` method.

`Operators` are the built in AC operators - `if` and `while`.

`Host` is the application supplied class implementing the IActionQueryHost interface. The AC runner needs it to run scripts - every function in executed by calling the host to do it. Thus the `host` determines the actual set of available functions for the script. The scripts must be written for a known host - i.e. knowing what functions can be used in the script and what they do.
## Syntax - general structure

AC is the simplest (almost) possible language that can provide minimal acceptable comfort to people with habits built in virtually any programming platform. It consists of comma separated lists and function calls with arguments that are also comma separated lists. The available functions depend on the host over which the AC script is executed and will vary from context to context. Assuming that the functions exist a program in AQ can look like:

```
Echo( 'hello' ),
if ( Get('var1'),
     Echo('var1 contains a truthy value')
   )
```
or
```
$i(10),
while( Dec('i'),
    (
        Echo( 
            if ( IsEven($i)), 
                'i is even', 
                'i is odd' 
            )
        ),
        Echo( Concat('i=', $i))
    )
)
```

All function calls return a value, even the built-in constructs of the language `if` and `while` do return a value and can be used as arguments to another function (unlike `if` the `while` construct always returns null, so it is not useful that way, but technically it can be an argument).

Multiple statements can be grouped as one by enclosing them in `( )` brackets (see the while's body in the above example). Such a group returns the value of its last member, while the rest are dumped - i.e. they are executed, but their return values are lost, only the last one is returned. For example if we assume that `Echo` outputs on the console all its arguments:

```
Echo('a','b'.'c')
```

will output 

```
a
b
c
```

but

```
Echo(('a','b','c'))
```

will output only

```
c
```

Regardless of the implementation of the function in the host.

> So the general structure of an AQ program is

```
program := statement *{, }
statement := literal | parameter | functioncall | varsetget | comment
functioncall = function ( statement *{, statement})
varsetget := $varname | $varname ( statement )
comment := # ... <end of line>
```

All the functions except `if` and `while` are provided by the host and can accept any number of arguments (by default up to 16 - a sanity limit imposed by the library). The function implementations will receive the arguments as an array and can chose what to do with them and throw an exception if they require certain number(s) of arguments, but receive less or more than necessary.

## Types

The ActionQuery works with a single type provided by the host implementation. An `object` will do, but it is most often more convenient to use something more complex that enables the host and everything else in the project to keep some other supporting information with the value. Basically the type used has to be something that can contain at least strings, integers, floating point numbers and booleans. One cannot specify any other types as literals in the language, but this is not a reason to not support such types packed in the AC parameter type if they are needed in the project. Such "advanced" types will pass through the script from function to function and if the host provides functions for their detailed manipulation it will be possible to deal with them in the AC scripts. For instance set of functions working with dictionaries will enable writing scripts  working with dictionaries and so on. This cannot go to far though, the AC script is rather limited language and there is a reasonable limit to teh complexity of the scripts.

The only direct interactions (not through the host supplied functions) of the language with the values are delegated to mandatory methods the host must implement. The ActionQuery asks host to convert literals and to determine if a value is truthy or falsy, to resolve parameters, to get/set variables, everything else is up to the set of functions provided to the language and is thus a matter of host design and purpose as defined by the programmer.

## Special literals

### Strings

The strings are enclosed in single quotes `' '` and if a quote has to be included inside the string it has to be escaped with backslash `'\''`.

Valid string literals:

```
'abcd'
'abc defg'
'1234 , sdf; '
'O\'Neill'
```

### Numbers

Both integer and floating point decimal numbers with optional sign can be specified as literals. If the number contains a dot it is treated as floating point, if it doesn't it is teated as an integer. Floating point exponents are not supported as literals.

Valid number literals:

```
123
0344
-34
+45.345
34.54
0.545
```

Floating point number literals cannot be specified by omitting everything before the decimal sign - 0 has to be used e.g. `0.234` is valid, but `.545` is not.

### Booleans and null

Tree more literals are accepted:

`true` - boolean true
`false` - boolean false
`null` - null value

### Functions and their names

The functions provided by the host are recognized thanks to the function call syntax: `identifier(` comma separated list of arguments `)`. The identifier can be any ASCII combination starting with alpha character or underscore `_` and containing further alpha, numeric characters and also the characters `_`, `.` (dot), and dash `-`. The names are case sensitive and the names `if` and `while` are reserved by the language for the corresponding language constructs. Further semi-reserved words are:

> `collect`, `foreach`, `for`, `do`, `spread`

There is no explicit reservation for these keywords, but there are listed as names for probable future extensions and should be avoided in order to avoid future incompatibility. If you need them for clarity use Capitalized versions.

The arguments of the functions can be any valid statements, including nested function calls and even language constructs, because the latter behave as functions.

### External parameters

Any identifier that is also a valid function name can serve as name of an external parameter. One can have both external parameter and a function with the same name, they will be recognized based on the syntax - parameters are not followed by `( )` brackets.

The external parameters supply through the host a value provided from the outside world in some way. The host implements a method that is called when a parameter has to be resolved, how the parameter is resolved is up to the host and one can implement it in any way suitable for the needs of the context in which the AC script is used.

### Variables

From version 1.1 the host is required to support variables. To this end the hosts must implement the two new interface methods GetVar and SetVar.

Variable names follow the same rules as the function names and there is no problem to have variables with names same as some functions - the syntax will make mistakes impossible.

To read a variable it is placed without following `( )` brackets and prefixed with the `$` symbol. E.g. `$var1` fetches the value of var1.

To set a variable it is prefixed with `$` and followed by `( )` brackets with an expression inside. E.g. `$var1(10)` sets var1 to 10, `$var1($var2)` sets var1 to the value of var2, `$var1(Add(1,5))` sets var 1 to the sum of 1 and 5 and so on.

Unset variables should return `null`. Hosts can implement this differently if it serves a useful purpose, but without a compelling reason hosts SHOULD not throw exceptions when an unset variable is requested. Returning null is the recommended way to implement this.
### Program flow constructs

Currently there are only two constructs in the ActionQuery language - `if` and `while`. They act similar to functions, but unlike the case of a function, some of their arguments are not executed depending on a condition or are executed multiple times.

`if ( condition, then-branch [, else-branch] )`

The `if` construct, as shown above, can have 2 or 3 arguments. Any other number will cause compile-time syntax error. The `if` executes its condition argument and if its value is truthy executes its second argument, if it is falsy it executes the 3-d (if provided). 

`if` returns a value - the value returned by the executed argument. If the else-branch is omitted and the condition is falsy it returns `null`. This means that `if` can be used as an argument to another function or flow construct. The nesting is not explicitly limited. Obviously function-like usage is best suited for `if` constructs with 3 arguments, but if the functions provided by the host are aware of the `null` behavior in the other case it is also Ok to use it without an else-branch.

_If you wonder about it_: The difference between the language `if` construct and a function that attempts to do the same is that the argument that does not match the condition is not executed at all, while a function provided by the host will only take care for the result, but will not be able to stop the execution of any of its args.

`while ( condition, body )`

The `while` construct makes possible to make program cycles. The body of the construct is executed as long as the condition is not a falsy value. So the body and the condition are repeatedly executed until the condition becomes falsy.

To avoid endless loops the AQ runner (the object representing the compiled program) supports an optional parameter that specifies a hard limit for the steps the execution can take. It is recommended to always set a limit when executing AC scripts unless the specific context assumes rare and extremely careful writing of the script (and even then a very high limit will not be a bad idea).

The while also returns a value, but it is always `null`. Future extensions may offer a cycle construct of another kind that allows for something else, but while will behave that way, because any other behavior can easily become a source for mistakes.

`Future extensions` of flow control are unlikely, but if the demand for them grows they can still appear after careful consideration. The argument against that is that such extensions fall in two unwanted categories: 

- Function like constructs that assume at abstract level the existence of collections/iterable values. Going that way will lead to a number of additions to support the concept and will effectively extend the syntax of the language drastically. The language is not well suited for dealing with types on that level and to make it such the extensions have to be big enough that the result will hardly bare any resemblance with the minimal syntax we want to have. 

- Function like constructs that do not behave as functions at all. Doing this without introduction of a lot more features will be difficult to use and like in the previous case will cause the language to grow outside of the intended complexity envelope.

### Compound statements

If one needs to put more than a single function call, literal or parameter as argument somewhere where only one should be given, multiple statements of any kind can be specified in `( )` brackets. A compound statement returns the value of its last statement, all the others are lost/dumped, but they are still executed.

Compound operators are most useful in `if` and `while` constructs, but can be used on any level (including global) where multiple statements are needed.

For example:

```
while ( Decrement('i'),
    (
        Echo('i=', Get('i)),
        Func1('some literal', someparam),
        if (Get('b'),
            (
                Echo('in an if\'s then'),
                Echo('still in the if\'s then')
            )
        )
    )
)
```

### Global behavior

The global body of a script can be thought as an argument list of a function. The list of their values is returned to the caller who executes the compiled script over a host. These values may or may not be used depending on the needs and reasons for the execution of the script.

This is mostly matter of creating and using a host, i.e. usage of ActionQuery from your C# code which is discussed in [Creating a host](AQHost.md). However, if we ignore the details, multiple statements can be used on global level as long as they are separated with commas. If the result is important for the AQ invoker, depending on which execution method is used `Execute` or `ExecuteScalar` it may be needed to enclose some or all the statements as a compound statement. Still, this is something that depends on the usage and should be defined for the specific scenario for which the specific scripts are written, together with all the available functions.

As mentioned previously, it is recommended to use `ExecuteScalar` and expect a single value result from the script at most. Any other data that has to reach the outside world from the script can be passed by functions provided by the host. This makes it easier to follow (function names hint at the purpose) and less likely to mess up the result - with multiple results the order of the pieces of code in the script and their number (at global level) will be critical and this is obviously something that can be messed up much more easily compared to calls of special functions feeding certain external functionality.

## Usage of AQ script from the perspective of the ActionQuery script writer.

ActionQuery scripts should be supported by a project that is usually complex and flexible. Such a project will provide certain customization points where AC scripts can be attached to control what is happening. Each point will come with a context that provides a set of functions that can be used in the script. Very often such projects will also provide libraries of functions usable across all the contexts in all customization points to do basic stuff which does not depend on the particular context.

The AC script editor should write the script(s) by using the provided functions, including them in flow constructs as necessary.

Except functions a customization point will most likely define for the script(s) a number of available parameters that can be referred in the ActionQuery script together with information what data they supply. The parameters can be configuration values, values of dynamic variables describing the current state of the context and can be implemented in any way suitable. The only requirement is to provide them through the host packed in the type used by the AC script in the point's context (see [Creating a host](AQHost.md) for the other side of the matter).

A host can choose to implement the variables available to the script in a shared way. I.e. the variables can be accessed by application code and impact its function in certain way. Be sure to synchronize the access to them if it can be done from different threads. While this approach can be tempting sometimes it is rarely justified and using dedicated functions is still the recommended approach.

## Justification for the ActionQuery existence and implementation

This section is for those who wonder why ActionQuery might be useful and why in this form. 

The first thing that comes to mind to a C# experienced programmer for the scenarios targeted by ActionQuery is probably using Expressions. They can be constructed in various ways, including parsing a language of sorts. While this is a valid way to achieve similar results it is complicated and imposing limitations in order to keep it simple is still not a simple task - both for planning and for implementation. The intrinsic strict types used by .NET expressions makes them a perfect way to specify something external inside the C# code, but the reverse scenario - specifying internal matter from the outside is possible, but much more difficult both for implementation and for expression writing. Just consider the implicit strict typing that alone will require a complex expression syntax that can easily reach C# levels.

So, if you want, for example, to be able to write in C# something that will become SQL like in Entity Framework it is natural to use them, the overall task may not be a small one, but is natural to proceed this way. There are a number of scenarios in which they solve similar problems - provide not just the result, but also additional information or a structure that can be executed in a way decided by a code external for the scenario. Still, try to do the reverse - provide instructions for the C# code you write from the outside and Expressions rise the bar not only for the implementation of the code that consumes them, but also for those who will write them and will need to be aware of the types and to some extent of the way the code functions inside the application. Add the need to do this at run-time or even only without rebuilding and one has a lot of work to do and a lot of details and possibilities to take care of. Such needs often lead to exploding in size configurations or if Expressions are employed the aim to simplify the writing for the people who will write them from the outside, the limitations that need to be applied negate the very usefulness of the feature.

ActionQuery works with a single variant type capable of holding the basic values and depend on conversion built-in in the functions or conversion functions available for the script. The simple context - a set of parameters and available functions makes writing scripts almost entirely a matter of describing to the editors what these mean and do in the particular context without need to know anything more. While ,NET Expressions can easily get out of hand and dig deeper into the application than intended, it is easy to limit the ActionQuery script only to an easy to define set of options - no chance to execute something in a scope where it can invoke something that should not be invoked and no need to convert fully fledged language logic to another language/execution code just to make sure the context is well isolated from anything dangerous.

So, it is easy, then why not host some well-known language - Javascript for example? Doing so is also a valid way to go, but the question one should consider in such a case is what is the price of such an approach? Fully capable scripting languages require complex contexts and serious effort to provide them. They will usually need their own thread, memory and thread(s), the calls from and to them should be proxied, types implicitly converted, very likely they will impose the need to link the calls in and out of them to the host asynchronously and take care to marshal any incompatible data. In the end such scripting languages are too powerful and will enable the editors to go farther than it was ever intended for the scenarios where they are used, sometimes for good, but most often with unexpected and dangerous results, such as moving logic that should be inside the application to some script intended for small tasks, building frameworks that lead to splitting core logic between the C# code and a number of scripts initially intended for small customizations or query-like functions. Time constraints can force one to leave it so and allow further spread of core logic that will soon put the stability and consistency of the project in jeopardy.

> ActionQuery is about minimal amount of learning, limiting what the writers can do, by providing them with everything they can use, context specialization, preventing code explosion and making the hosting a simple task that can be used almost everywhere without the need to support heavy and needy script engines. It is also called a query for a reason - it does not have the usual for many languages operators or the option to create procedures or classes. ActionQuery is still more an algorithmic language, but emphasizes nested function calls enough to make an ActionQuery program closer to a query in some predicate language than to a program in a fully-fledged algorithmic language. The idea behind it is to provide something convenient enough, but also naturally limiting the writer to a single task - the one required by the context, without temptations for complex solutions which would be out of place there. The hosting is very simple, because the script runs in the calling thread, without a heap, depends on the host for any illusion of variables and any other feature.

> To achieve its goals ActionQuery hosting has to be controlled by those who implement the host. Instead of growing the AC program too much, which would certainly happen if it was possible to define functions inside it, any such need has to be backed by the host and thus approved and implemented by the host developers and not by the script writers. This is simple to implement on one hand and keeps the control over the project development into the right hands as well.

## Caching

The compiled programs (runner objects created by the compiler) are context independent and can be preserved for multiple executions without recompilation. They can be theoretically even executed over different hosts, but the hosts must be compatible (provide same functions adn features).