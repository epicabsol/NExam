NExam
=====

NExam is a prototype for how I would like a testing and mocking library to work. I made it specifically out of the frustration of trying to read tests written using modern 'fluent' testing & mocking APIs.

So, I give you: NExam: Testing For The Rest Of Us

Respository Overview
--------------------

You'll find a couple of projects here:
 - `NExam` is the library for testing. It tests static methods that take a single `ITestContext` parameter and have the `[Test]` attribute applied.
 - `NExam.Submarine` is the library for mocking. It exposes the `Substitute<T>` class, which can be cast to the type `T` that it is substituting for.
 - `NExam.Pull` is the example of how the NExam projects can be used. The name is a pun on the word 'example'.

Design Philosophy
-----------------

I created NExam because I was frustrated with how incredibly hard it was to read unit tests for the popular unit testing frameworks, which implement a 'fluent' style of naming. I was staring at perfectly valid C# code and could not for the life of me comprehend what was going on.

So, here are the general API design principles:

**Simple & Straightforward Naming**  
Class names should be noun phrases, and method names should be verb phrases. Clearly express what a class represents and what a method does.

**No Invokation Context Funkiness**  
Methods should not do some completely different behavior just because they are called in a certain context.

**No Fake Global Extension Methods**
Extension methods that extend all types `T` are quite annoying, but I draw the line when they throw an exception when they aren't used in their very limited scope.

Let's look at a couple of examples that we would like to stray away from while designing the NExam library:

```
.Returns(...)
```
What the heck is this? Terrible naming (what does this method do? Hint: It doesn't just 'return', like the name would have you think), and it's available for any type `T` but throws an exception unless it's called on the result of a mockable method.  
This is especially cryptic when you see it called on a method that returns primitive type - the developer is left wondering if `int` got crazy new methods since they last checked.

```
.Should().Be(...)
```
What the heck is this? What does the `Should()` method do? What type does it return? No idea. `Be`? Really? What is a method called `Be` supposed to do? Just exist?

```
.InOrder(() =>
{ ... }
```
Again, `InOrder` isn't a verb and doesn't tell us what this method does. It raises questions like, 'how could this lambda I'm giving *not* be in order?' But that's just the tip of the iceberg: the bigger issue is that inside the given lambda, mocked methods avtually behave completely different from how they would otherwise behave.

```
<MockedMethod>(Do(...))
```
This turns my head inside-out, which is exactly the opposite of what reading code should feel like. Not only does this not run the mocked method like normal, the `Do(...)` doesn't actually *do* the lambda it's been given! That's right, the mocked method here behaves differently based on how its parameter was produced! This goes against the orthogonality that makes C-style languages so intuitive: a value is completely independent of how it was derived. A `0` is a `0`, and it has to behave identically regardless of whether the value came from a literal, a constant, a parameter, or a function call!

Design note: Ideally we would be able to implicitly and explicitly cast from `Substitute<T>` to `T` when `T` is an interface. However, C# explicitly disallows this.

Design note: Ideally we would be able to use `Substitute<T>` with classes as `T`, but unfortunately, the `ImpromptuInterface` package we're using to implement dynamic interface implementation doesn't support dynamic subclassing.