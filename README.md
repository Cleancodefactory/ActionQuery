# ActionQuery

A little library for running simple scripts that command host objects in your code. The methods of the host object and other methods are called as instructions of the script program enabling one to create scripts that use them in certain pattern, feeding them with parameters and obtaining result(s).

## Purpose

Similar to ResolverExpression this library enables usage of simple scripts in configurations or from some other input that command host objects in the main code.

The need of such a miniature language arises when the programmer wants to create in C# a programmable "service" and do that programming without the need to recompile any code or even at run-time. Recompiling or dynamic compilation is a valid way to go, but it brings with it a lot of complexity and cannot be justified except in very small number of cases. As an alternative a simple to host mini language can do the job and command huge number of tasks defined in configurations or even received in network requests. All the functionality available for a script comes from the host which makes possible to secure the environment easily and in an easy to understand way that depends only on the host class - just do not make available to the script anything that can compromise the security.

## Documentation

_Will be here soon._

## Example

There is an example project in [Example/acexample](Example/acexample) that is using the ActionQuery from console application that allows you to enter mini programs and execute them (with tracing).

Please note that spaces and new lines are just ignored and unlike in the example the programs can be written in more relaxed fashion if they are loaded from a file or other source where they can be edited more conveniently.

Go to [example's readme](Example/acexample/Readme.md) for details about the example.
