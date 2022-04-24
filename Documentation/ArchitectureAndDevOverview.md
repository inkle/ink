# Architecture and Development

## Overview

The ink pipeline broadly consists of 3 stages:

 * The ink parser - interprets the ink text files to a hierarchy of objects
 * The runtime code generation
 * The runtime engine itself

The following is a fairly brief tour of these 3 stages, hopefully enough to give you a foundation for exploring the codebase yourself.

## Ink parser

The parser takes a root ink file (which may reference other ink files), and constructs a hierarchy of `Parsed.Object` objects that closely resemble the structure within the original ink files, as closely as possible to how it was written.

The parser is a hand-written recursive decent parser that inherits from `StringParser`, which allows a hierarchy of parse rules to be built using C# delegates. At the top level, `InkParser.cs` contains a method called `Parse()`, which returns a `Parsed.Story` object.

Within `Parse()`, rules evaluated, each of which may contain more rules. Rules are simply methods that return the result of parsing or `null` if parsing failed (though there are exceptions to this).

Crucially however, parse rules are wrapped in either `ParseObject(rule)` or `Parse<T>(rule)`. These methods know how to rewind the state of the parser when a rule fails. This is important, since the rule may get part way through before it realises that it's not parsing what it wanted. By way of illustration, here's the start of the parse rule for a stitch:

```csharp
protected object StitchDefinition()
{
    // Wrap the 'StitchDeclaration' rule in a Parse(...) call,
    // so that it will return to the correct point in the ink
    // file if parsing fails.
    var decl = Parse(StitchDeclaration);
    if (decl == null)
        return null;

    //... (continue parsing the StitchDefinition)
```

If a rule isn't wrapped in a `Parse` method, then it's an indication that rewinding definitely isn't strictly necessary within the scope, for example if a sub-rule is both optional and atomic. Or, when success and failure of the rule is handled manually.

## StringParser structuring

The base class `StringParser` contains a number of helper methods to help with parsing.

Methods like `ParseString`, `ParseSingleCharacter`, `ParseInt`, etc can be used to read text and values at the lowest level.

Higher level structuring methods can be used to compose rules together. For example:

```csharp
public List<object> OneOrMore(ParseRule rule)
```

...acts a bit like the `+` operator in regular expressions. So for example, in multiline conditionals, we have:

```csharp
List<object> multipleConditions = OneOrMore (SingleMultilineCondition)
```

Similarly, but even more powerfully, the `Interleave` method patterns of the form ABABA etc. Frequently, this is used to interleave some core content with whitespace. Here's an example that parses the arguments to a flow (e.g. knot, stitch or function), that is a series of identifiers separated by commas:

```csharp
var flowArguments = Interleave<FlowBase.Argument>(
    Spaced(FlowDeclArgument),
    Exclude (String(","))
);
```

The above example also demonstrates another concept: rule transformers and builders. In the above example, `Spaced` takes a rule, and produces a new rule that also allows for optional whitespace on either side of the content that is parsed.

The `Exclude` transformer takes a rule, and if it succeeds, prevents its result from being included in the `Interleave`'s returned list (to remove the commas from the parsed results).

The `String` rule is like `ParseString` except that it **constructs rules** rather than parsing immediately, which is useful for conveniently constructing declarative expressions like the one above.

Other transformers and structuring concepts exist, but hopefully this should give you enough of a taste to get started.

## Runtime code generation

The parsed hierarchy closely resembles the ink as it was originally written. However, the data that's loaded by the ink engine at runtime is very different. It's built out of smaller, more fundamental units, sort of like byte code, though not that low level. This content is exported to a JSON based format ready to be loaded by the runtime engine within the game. For more information on this format, [see the documentation](archive/ink_JSON_runtime_format.md).

In the runtime, there's no concept of Knots, Stitches, Weave, or other high level ink structures. Instead, the runtime consists mainly of general purpose `Runtime.Container` objects and content, inheriting from `Runtime.Object`.

These runtime objects are built by the `Parsed.Object` hierarchy, however not immediately. The constructors of `Parsed.Object` objects are kept as lightweight as possible, since the rewinding of the parser when rules fail can potentially cause them to be built and destroyed multiple times before a final successful hierarchy is produced.

Instead, the hard work of converting the parsed hierarchy into runtime code is saved for two steps that are done later:

 * Main code generation
 * Resolving of references

In most cases, a single `Parsed.Object` is converted to one or more `Runtime.Object`, through the following method:

```csharp
public override Runtime.Object GenerateRuntimeObject () {...}
```

At the top level, this code generation process is kicked off by the `Parsed.Story` in:

```csharp
public Runtime.Story ExportRuntime()
```

Once the full runtime hierarchy has been constructed, references are resolved. This has to be done in a separate pass, since the target of references (such as divert targets - knot names etc) may not exist yet until the full hierarchy exists.

Each `Parsed.Object` can implement:

```csharp
public override void ResolveReferences(Story context)
```

...in order to participate in this process. For example in `Divert.cs`, the method contains this (as well as other things):

```csharp
if (targetContent) {
    runtimeDivert.targetPath = targetContent.runtimePath;
}
```

## Runtime ink engine


_Note: This section of the documentation is incomplete. We're working on a formal specification for the runtime ink engine, stay tuned!_

As mentioned above, the runtime code is built out of smaller, simpler, objects compared to the ink as it's parsed directly.

All the higher level structures, including the story itself, any knots and stitches, and even choices, are built out of containers. Within the containers, content is iterated through sequentially, and appended to the output.

This structure is loaded by the ink engine in a [JSON based format](archive/ink_JSON_runtime_format.md).

### Containers

The `Runtime.Container` is general purpose, and can work as either than array or a dictionary, or both. Therefore, it can have ordered (indexed content) that's designed to be iterated through sequentially, and can also have named content, that's designed to be accessed by a string key.

For example, a `Parsed.Choice` compiles down to a `Runtime.Container` that contains, amongst other things, the initial sequential content that forms the text of the choice, the minimal `Runtime.ChoicePoint` itself, as well as a named sub-container that contains the content that is run when the choice is picked by the player.

### Content

As with ink itself, the runtime engine's fundamental unit is content, rather than code. As such, a simple "Hello world" ink file would be compiled down to a single `Runtime.Container` with a single `Runtime.StringValue` in it (as well as a terminator).

Each piece of content that is encountered is appended to the `outputStream` within the `Runtime.Story`.

### Control commands

Alongside the content that's designed to be seen by the player, additional commands are used to control the flow and evaluation of the content. Some examples, all of which are in the enum `Runtime.ControlCommand.CommandType`:

* `EvalStart` and `EvalEnd`: the content objects between are appended to the `evaluationStack` rather than the `outputStream`. As functions and operators are encountered, they pop values off the stack, process them, and push a value back on the stack. Meanwhile, `EvalOutput` pops a value off the evaluation stack, and pushes it onto the output stream.
* `ChoiceCount`: how many choices have been produced in the current turn? Push the value to the output stream (or evaluation stack if in evaluation mode).
* `Done`: Indicates that the current flow is safe to end.

### Story

Some important and useful features of the main runtime engine in `Story.cs`:

 * `Continue()` is the top level point where iteration of the content happens, and has this rough structure internally:

```csharp
while( Step () || TryFollowDefaultInvisibleChoice() ) {}
```

* `Step()` iterates through a single element of content, and returns `false` if it runs out of content.
* `PerformLogicAndFlowControl(contentObject)` is called from `Step`, and handles the majority of the non-content objects such Diverts, Control Commands, etc.

## Compiler development and debugging tips

While testing modifications to the compiler, it's useful to run the **InkTestBed** project. Inside this project `InkTestBed.cs` contains a suite of useful tools.

The main entry point is `Run()`. By default it simply calls `Play()`, which will load up a pre-existing `test.ink` (that you can put your test ink in). `Play()` has a basic choice loop that also serves as a good introduction to some of the built in convenience functions.

However if you want to automatate the testing of a particular flow, you could write something like:

```csharp
void Run ()
{
    CompileFile();

    ContinueMaximally ();
    Choose(0);

    ContinueMaximally ();
    Choose(1);

    ContinueMaximally ();
}
```

`BuildStringOfHierarchy()` is a method in `Runtime.Story` that's useful when debugging. If you add it as a Watch expression while debugging the ink engine, you can see a representation of the runtime hierarchy, as well as an arrow that points at where execution currently is in the hierarchy. For example, the following ink:

    This is a test.
    * [A choice]
    - The end -> DONE

...is at the time of writing represented, somewhat verbosely, as:

    [
        [
            This is a test.
    ,
            [
                EvalStart,  <---
                BeginString,
                A choice,
                EndString,
                EvalEnd,
                Choice: ->  line 2
                -- named: --
                [ (c)
                    Divert (line 3)
                ]
            ]
            -- named: --
            [ (g-0)
                The end
    ,
                Done
            ]
        ],
        End
    ]

The `<---` is the pointer to the current object being evaluated.

In this representation, containers are represented as:

    [
        ...content...
        -- named: --
        ...named content...
    ]
