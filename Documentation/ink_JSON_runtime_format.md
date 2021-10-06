# ink's JSON runtime format

When ink is compiled to JSON, it is converted to a low level format for use by the runtime, and is made up of smaller, simpler building blocks. For an overview of the full pipeline, including a description of the runtime itself see the [Architecture and Development documentation](ArchitectureAndDevOverview.md).

## Top level

At the top level of the JSON file are two properties. `inkVersion` is an integer that denotes the format version, and `root`, which is the outer-most Container for the entire story.

```json
{
    "inkVersion": 10,
    "root": <root container>
}
```

Broadly speaking, the entire format is composed of Containers, and individual sub-elements of the Story, within those Containers.

## Containers

There is only one type of generalised collection, and this is the **Container** - it's used throughout the engine. In JSON it's represented as an array type.

The root of a story is a container, and as a story is evaluated, the engine steps through the sub-elements of containers.

Although containers primarily behave like arrays, they also have additional attributes, including a way to reference named sub-elements that aren't included in the array itself. To support this behaviour, **the final element of the array is special**. The final element is either `null`, or it's an object (dictionary) that contains a combination of named sub-elements (for example, nested Containers), and optionally two other properties: `#f`, which is used to hold an integer of bit flags, and `#n`, which holds the name of the container itself, if that's not redundant due to being a named field of a parent container.

Possible flags used by `#f`:

 * **0x1** - Visits: The story should keep a record of the number of visits to this container.
 * **0x2** - Turns: The story should keep a record of the number of the turn index that this container was lasted visited.
 * **0x4** - CountStartOnly: For the above numbers, the story should only record changes when the story visits the very first subelement, rather than random entry at any point. Used to distinguish the different behaviour between knots and stitches (random access), versus gather points and choices (count start only).

Examples:

* `[5, 6, null]` - A Container with two integer values, and no additional attributes.

* `["^Hello world", {"#n": "hello"}]` - A Container named "hello" with the text object "Hello world".

* `["^test", {"subContainer": [5, 6, null], "#f": 3}]`

   A container with the text object "test", flags 1 and 2, and a nested container named "subContainer" that resembles the first example.

## Values

Values are the main content objects. The most useful for written content is the String Value, which is used for all the main text within the story flow.

String Values are represented in JSON by preceding the text in quotes by a leading `^` to distinguish it from many other object types which have special names (for example, control commands and native functions). The only exception is a newline, which can be simply written as `"\n"`.

Values may also be used in logic/calculations, for example with the `int` and `float` types.

Supported types:

* **string**: Represented with a leading `^` to differentiate from other string-based objects. e.g. `"^Hello world"` is used in JSON to represent the text `Hello world`, and `"^^ up there ^"` would be the text `^ up there ^`. No `^` is needed for a newline, so it's just `"\n"`.
* **int** and **float**: these are represented using their standard JSON counterparts. e.g. `5`, `5.6`.
* **divert target**: represents a variable divert target, for example as used in the following ink:

        VAR x = -> somewhere

    Represented in runtime JSON as an object of the form: `{"^->": "path.to.target"}`

* **variable pointer**: used for references to variables, for example when declaring a function with the following ink:

        == function myFunction(ref x) ==

    Represented in runtime JSON as an object of the form: `{"^var": "varname", "ci": 0}`. Where `ci` is "context index", with the following possible values:

    * **-1** - default value, context yet to be determined.
    * **0**  - Variable is a global
    * **1 or more** - variable is a local/temporary in the callstack element with the given index.

## Void

Represented by `"void"`, this is used to place an object on the evaluation stack when a function returns without a value.

## Control commands

Control commands are special instructions to the text engine to perform various actions. They are all represented by a particular text string:

* `"ev"` - Begin logical evaluation mode. In evaluation mode, objects that are encountered are added to an evaluation stack, rather than simply echoed into the main text output stream. As they're pushed onto the stack, they may be processed by other commands, functions, etc.
* `"/ev"` - End logical evaluation mode. Future objects will be appended to the output stream rather than to the evaluation stack.
* `"out"` - The topmost object on the evaluation stack is popped and appended to the output stream (main story output).
* `"pop"` - Pops a value from the evaluation stack, *without* appending to the output stream.
* `"->->"` and `"~ret"` pop the callstack - used for returning from a tunnel or function respectively. They are specified independently for error checking, since the callstack is aware of whether each element was pushed as a tunnel or function in the first place.
* `"du"` - Duplicate the topmost object on the evaluation stack. Useful since some commands consume objects on the evaluation stack.
* `"str"` - Begin string evaluation mode. Adds a marker to the output stream, and goes into content mode (from evaluation mode). Must have already been in evaluation mode when this is encountered. See below for explanation.
* `"/str"` - End string evaluation mode. All content after the previous Begin marker is concatenated together, removed from the output stream, and appended as a string value to the evaluation stack. Re-enters evaluation mode immediately afterwards.
* `"nop"` - No-operation. Does nothing, but is useful as an addressable piece of content to divert to.
* `"choiceCnt"` - Pushes an integer with the current number of choices to the evaluation stack.
* `"turns"` - Pops from the evaluation stack, expecting to see a divert target for a knot, stitch, gather or choice. Pushes an integer with the number of turns since that target was last visited by the story engine.
* `"visit"` - Pushes an integer with the number of visits to the current container by the story engine.
* `"seq"` - Pops an integer, expected to be the number of elements in a sequence that's being entered. In return, it pushes an integer with the next sequence shuffle index to the evaluation stack. This shuffle index is derived from the number of elements in the sequence, the number of elements in it, and the story's random seed from when it was first begun.
* `"thread"` - Clones/starts a new thread, as used with the `<- knot` syntax in ink. This essentially clones the entire callstack, branching it.
* `"done"` - Tries to close/pop the active thread, otherwise marks the story flow safe to exit without a loose end warning.
* `"end"` - Ends the story flow immediately, closes all active threads, unwinds the callstack, and removes any choices that were previously created.

## Native functions

These are mathematical and logical functions that pop 1 or 2 arguments from the evaluation stack, evaluate the result, and push the result back onto the evaluation stack. The following operators are supported:

`"+"`, `"-"`, `"/"`, `"*"`, `"%"` (mod), `"_"` (unary negate), `"=="`, `">"`, `"<"`, `">="`, `"<="`, `"!="`, `"!"` (unary 'not'), `"&&"`, `"||"`, `"MIN"`, `"MAX"`

Booleans are supported only in the C-style - i.e. as integers where non-zero is treated as "true" and zero as "false". The true result of a boolean operation is pushed to the evaluation stack as `1`.

## Divert

Diverts can take the following forms:

* `{"->": "path.to.target"}` - a standard divert to content at a particular path.
* `{"->": "variableTarget", "var": true}` - as above, except that `var` specifies that the target is the name of a variable containing a *divert target* value.
* `{"f()": "path.to.func"}` - a function-call, which is defined as a divert that pushes an element to the callstack. Note that it doesn't necessarily correspond directly to an ink function, since choices use them internally too.
* `{"->t->": "path.tunnel"}` - a tunnel, which works similarly to a function call by pushing an element to the callstack. The only difference is that the callstack is aware of the type of element that was pushed, for error checking.
* `{"x()": "externalFuncName", "exArgs": 5}` - an external (game-side) function call, that optionally takes the specified number of arguments.

Additionally, a `"c"` property set to `true` indicates that the divert is conditional, and should therefore pop a value off the evaluation stack to determine whether the divert should actually happen.

## Variable assignment

Pops a value from the evaluation stack, and assigns it to a named variable, either globally or locally (in a `temp`, or a passed parameter). The `"re"` property being set indicates that it's a re-assignment rather than a brand new declaration.

Examples:

* `{"VAR=": "money", "re": true}` - Pop a value from the evaluation stack, and assign it to the already-declared global variable `money`.
* `{"temp=": "x"}` - Pop a value from the evaluation stack, and assign it to a newly declared temporary variable named `x`.

## Variable reference

Obtain the current value of a named variable, and push it to the evaluation stack.

Example:

* `{"VAR?": "danger"}` - Get an existing global or temporary variable named `danger` and push its value to the evaluation stack.

## Read count

Obtain the read count of a particular named knot, stitch, choice or gather. Note that this is implemented as a Variable Reference with particular flag in the C# ink runtime.

Example:

* `{"CNT?": "the_hall.light_switch"}` - gets the read count of the container at the given path. For example, it might be a stitch named `light_switch` in a knot called `the_hall`.


## ChoicePoint

Generates an instance of a `Choice`. Its exact behaviour depends on its flags. It doesn't contain any text itself, since choice text is generated at runtime and added to the evaluation stack. When a ChoicePoint is encountered, it pops content off the evaluation stack according to its flags, which indicate which texts are needed.

A ChoicePoint object's structure in JSON is:

```json
{
    "*": "path.when.chosen",
    "flg": 18
}
```

The path when chosen is the target path of a Container of content, and is assigned when calling `ChooseChoiceIndex`.

The `flg` field is a bitfield of flags:

 * **0x1 - Has condition?**: Set if the story should pop a value from the evaluation stack in order to determine whether a choice instance should be created at all.
 * **0x2 - Has start content?** - According to square bracket notation, is there any leading content before any square brackets? If so, this content should be popped from the evaluation stack.
 * **0x4 - Has choice-only content?** - According to square bracket notation, is there any content between the square brackets? If so, this content should be popped from the evaluation stack.
 * **0x8 - Is invisible default?** - When this is enabled, the choice isn't provided to the game (isn't presented to the player), and instead is automatically followed if there are no other choices generated.
 * **0x10 - Once only?** - Defaults to true. This is the difference between the `*` and `+` choice bullets in ink. If once only (`*`), the choice is only displayed if its target container's read count is zero.

Example of the full JSON output, including the ChoicePoint object, when generating an actual ink choice from `* Hello[.], world.`. Most of the complexity is derived from the fact that content can be dynamic, and the square bracket notation that requires repetition.

```jsonc
// Outer container
[

  // Evaluate choice text.
  // Starts by calling a "function" labelled
  // 's', which is the start content for the choice.
  // We use a small Container so that it can be
  // be re-used, and so the visit counts will be correct.
  "ev",
  "str",
  {
    "f()": ".^.s"
  },
  "/str",

  // Evaluate content inside square brackets (simply '.')
  "str",
  "^.",
  "/str",

  // Evaluation of choice text complete
  "/ev",

  // ChoicePoint object itself:
  //  - linked to own container named 'c'
  //  - Flags 22 are:
  //     * 0x2  - has start content
  //     * 0x4  - has choice-only content
  //     * 0x10 - once only
  {
    "*": ".^.c",
    "flg": 22
  },

  // Named content from outer container - 's' and 'c'
  {
    // Inner container for start content is labelled 's'
    "s": [
      "^Hello",
      null
    ],

    // Inner container for content when choice is chosen
    // First repeats the start content ('s'),
    // before continuing.
    "c": [
      {
        "f()": "0.0.s"
      },
      "^, world.",
      "\n",
      "\n",

      // Container has all three counting flags:
      //  - Visits are counted
      //  - Turns-since is counted
      //  - Counted from start only
      {
        "#f": 7
      }
    ]
  }
]
```

## Paths

Paths won't ever appear on their own in a Container, but are used by various objects (for example, see Diverts) to reference content within the hierarchy.

Paths are a dot-separated syntax:

    path.to.target

Where each element of the path references a sub-object, drilling down into the hierarchy.

However, paths can have several element types between the dots:

 * **Names** - to reference particular knots, stitches, gathers and named choices. These specify a named content object within a Container.
 * **Indices** - integers that specify the index of a content object within the ordered array section of a Container.
 * **Parent** - Denoted with a `^`. (Similar to using ".." in a file system.)

Relative paths *lead* with a dot rather than starting with a name or index.

Examples:

* `building.entrance.3.0` - the first element of a Container at the fourth element of a Container named `entrance` within a Container named `building` of the root Container.
* `.^.1` - the second element of the parent Container.

