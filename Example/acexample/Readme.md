# ActionQuery example

This example is a console application that defines a class (in host.cs) for hosting ActionQuery (`AC`) scripts and runs whatever you type in the console when prompted.

The type used for ResolverValue is in value.cs and is more complex than needed for an example (an object will do), but this better demonstrates how the values in the AC scripts may be more than simple numbers and strings and may contain additional data managed by the host's methods.

To run the example go to its directory and type `dotnet run`

If you get errors for missing stuff make sure to update submodules from your git (the ActionQuery library is used as a source fetched as a submodule in Src/ActionQuery).

## Example programs

```
Set('a',2), while(Get('a'), (Set('a',Add(Get('a'),-1)),Echo('A=',Get('a'))))
```
Demonstrates a cycle.

```
Set('a',1), if (Get('a'), Echo('a is truthy'), Echo('a is falsy'))
```

Try running this with a set to 0 to see the alternative result. Note that the `if` returns a value - the value returned by the part that is executed.

## Fiddle with the example

You can disable the tracing by setting `host.Trace = false;` in the Main method. You can still trace, but change the number of steps allowed - very useful if you fall in an endless loop for instance.

You can add more methods available to the script, by implementing them and registering them in the host's constructor. Note that the methods do not need to be only methods of the host class. Still it is unadvisable to spread them too much - never forget that you may need to take care about security in a real world scenario and allowing the script to call too many methods from too many places will make this task much harder.