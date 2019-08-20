# Viking.Pipeline
Declarative library for C# data processing.

This library enables you to create a declarative, functional description of how data flows through your application. 
Once set up, the pipeline will propagate any changes to any data to all appropriate stages in a lazy fashion.

To consume the data and react to changes, reactions can be created. Much like Events, the reaction methods will fire once new data is available.
Unlike Events, however, they are automatically detached from the pipeline once said reaction object is garbage collected.

Using the library is simple, and syntax is minimal for most operations. 
The following example sets up a pipeline which takes two changeable values and multiplies them together, 
then caches the value to prevent multiple recalculations, 
then adds a stage which uses the default equality comparison to determine if the downstream pipeline should be notified of changes,
then creates a reaction which will write out any changes to console.

```
var input1 = new AssignablePipelineStage<int>("input 1", 10);
var input2 = new AssignablePipelineStage<int>("input 2", 0);

var operation = PipelineOperation.Create("multiply", (a, b) => a * b, input1, input2);
var cache = operation.WithCache();
var equality = cache.WithEqualiyCheck();
var reaction = cache.CreateReaction(value => Console.WriteLine("New value is: " + value.ToString()));
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

The pipeline enables fairly simple extending: Simply create classes which extends IPipelineStage or IPipelineStage<T>, 
depending on if the stage has output or not.
All built-in functionality such as `WithCache()` or `CreateReaction()` will automatically be available, along with other neat methods.
A pipeline stage which conditionally stopped propagation based on a flag set on the stage itself would perhaps look like:

```
class PausingStage<T> : IPipelineStage<T>
{
  public PausingStage(IPipelineStage<T> stage) { Stage = stage; this.AddDependencies(stage); }
  private IPipelineStage<T> Stage { get; }
  public string Name => "Pauser for: " + Stage.Name;
  public bool IsPaused { get; set; }
  
  public T GetValue()=> Stage.GetValue();
  public void OnInvalidate(IPipelineInvalidator invalidator)
  {
    if(IsPaused)
      invalidator.Revalidate(this);
    else
      invalidator.InvalidateAllDependentStages(this);
  }
}
```

Apart from the easy extension, the pipeline library offers things like:
* Exception messages detailing what, where, and how exception appeared in the pipeline, along with all the preceding stages.
* Eliminating double-calls during propagation using topology sorting.
* Automatic cycle detection and exceptions.
* Automatic detection of concurrent propagations potentially conflicting with each other.
* Automatic handling of detachment on GC of objects. No more memory leaks (Or at least, you will have to work hard to get them)!




