# Viking.Pipeline
Declarative library for C# data processing.

This library enables you to create a declarative, functional description of how data flows through your application. 
Once set up, the pipeline will propagate any changes to any data to all appropriate stages in a lazy fashion.

To consume the data and react to changes, reactions can be created. Much like Events, the reaction methods will fire once new data is available.

## Features
The pipeline library offers things like:
* Automatic propagation of updated values.
* Eliminating double-calls during propagation using topology sorting.
* Automatic cycle detection and exceptions.
* Exception messages detailing what, where, and how exception appeared in the pipeline, along with all the preceding stages.
* Automatic detection of concurrent propagations potentially conflicting with each other.
* Automatic handling of detachment on GC of pipeline objects. No more memory leaks like in Events*!
* Different types of transaction and concurrency control on pipelines.

_\* Subject to the shenananigans programmers sometimes do because we just love to make our own lives misserable_

## Getting Started
Want to try Viking.Pipeline out? Just include the NuGet package **Viking.Pipeline** in your project and you're ready to go!

The following classes and functions should get you off to a good start:
* __AssignablePipelineStage__ - Probably the most common way to publish values to the pipeline which can change over time.
* __[any struct or class].AsPipelineConstant()__ - Publishes a constant value to the pipeline.
* __PipelineOperations.Create("name", [function], [input(s)])__ - Create a function taking one or more inputs and outputting one value.
* __[any pipeline stage].WithCache()__ - Caches the output of the previous pipeline stage. Recomputation of the value will only be done if something has changed upstream.
* __[any pipeline stage].CreateReaction(...)__ - Define a function which will be called with the results of the previous pipeline stage if it changes as a result of some changed input value or otherwise.
* __PipelineTransaction__ - Perform multiple changes atomically with only one propagation through the pipeline.

## Simple Examples
Using the library is simple, and syntax is minimal for most operations. 
The following example sets up a pipeline which
1. takes two changeable values and multiplies them together, 
2. then caches the value to prevent multiple recalculations, 
3. then adds a stage which uses the default equality comparison to determine if the downstream pipeline should be notified of changes,
4. then creates a reaction which will write out any changes to console.

```
var input1 = new AssignablePipelineStage<int>("input 1", 10);
var input2 = new AssignablePipelineStage<int>("input 2", 0);

var operation = PipelineOperation.Create("multiply", (a, b) => a * b, input1, input2);
var cache = operation.WithCache();
var equality = cache.WithEqualiyCheck();
var reaction = cache.CreateReaction(value => Console.WriteLine("New value is: " + value.ToString()));

input2.SetValue(1); // Will write the value 10 to the console.
```

This can also be written in a slightly more fluid way if you don't care about the interim steps:

```
var input1 = new AssignablePipelineStage<int>("input 1", 10);
var input2 = new AssignablePipelineStage<int>("input 2", 0);

var reaction = 
  PipelineOperation.Create("multiply", (a, b) => a * b, input1, input2)
  .WithCache()
  .WithEqualityCheck()
  .CreateReaction(value => Console.WriteLine("New value is: " + value.ToString()));
```

When multiple values must be changed, the following pattern can be used:

```
// The below demostrates how multiple values can be changed without triggering a propagation for each change.
new PipelineTransaction()
  .Update(input1, 5) // Adds an update of the first assignable value
  .Update(input2, 2) // Adds an update of the second assignable value
  .Commit(); // Trigger an atomic update of values in the transaction.
```

## Easy Extension
Are you missing that one feature in the pipeline? Not to worry, extending it is simple!

Simply create classes which implements the very small interfaces `IPipelineStage` or `IPipelineStage<T>`, 
depending on if the stage has output or not.
All built-in functionality such as `WithCache()` or `CreateReaction()` will automatically be available, along with __many__ other neat methods.
A pipeline stage which conditionally stopped propagation based on a flag set on the stage itself would perhaps look like:

```
class PausingStage<T> : IPipelineStage<T>
{
  public PausingStage(IPipelineStage<T> stage) { Stage = stage; this.AddDependencies(stage); }
  
  private IPipelineStage<T> Stage { get; }
  public string Name => "Pauser for: " + Stage.Name;
  public bool IsPaused { get; set; }
  
  public T GetValue() => Stage.GetValue();
  public void OnInvalidate(IPipelineInvalidator invalidator)
  {
    if(IsPaused)
      invalidator.Revalidate(this);
    else
      invalidator.InvalidateAllDependentStages(this);
  }
}
```




