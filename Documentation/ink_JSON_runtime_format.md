# ink's JSON runtime format

When ink is compiled to JSON, it is converted to a low level format for use by the runtime, and is made up of smaller, simpler building blocks. For an overview of the full pipeline, including a description of the runtime itself see the [Architecture and Development documentation](ArchitectureAndDevOverview.md).

## Top level

At the top level of the JSON file are two properties. `inkVersion` is an integer that denotes the format version, and `root`, which is the outer-most Container for the entire story. Additionally, there may also be a `listDefs` property, which contains the definitions of any lists used in the story.

```json
{
    "inkVersion": 21,
    "root": <root container>,
    "listDefs": <list definitions>
}
```
The `root` Container may have a named sub-container named `"global decl"`. This sub-container contains any global declarations that are a part of the story.

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

## List Definitions
The `listDefs` property at the top level of the JSON file contains the definitions for any lists defined in the story. The format of `listDefs` is as follows:
```json
{
    <list name>: {
        <entry name>: <entry value>,
        ...
    },
    ...
}
```
With `list name` and `entry name`s being `strings` and `entry value`s being `int`s

The property names at the top level of `listDefs` are the names of the lists, while the values are the contents of the list definition. The property names in the definitions are the names of the list items, while the values are the corresponding numerical values.

## Values

Values are the main content objects. The most useful for written content is the String Value, which is used for all the main text within the story flow.

String Values are represented in JSON by preceding the text in quotes by a leading `^` to distinguish it from many other object types which have special names (for example, control commands and native functions). The only exception is a newline, which can be simply written as `"\n"`.

Values may also be used in logic/calculations, for example with the `int` and `float` types.

Supported types:

* **string**: Represented with a leading `^` to differentiate from other string-based objects. e.g. `"^Hello world"` is used in JSON to represent the text `Hello world`, and `"^^ up there ^"` would be the text `^ up there ^`. No `^` is needed for a newline, so it's just `"\n"`.
* **int** and **float**: these are represented using their standard JSON counterparts. e.g. `5`, `5.6`.
* **boolean**: Represented with JSON `true` and `false`.
* **divert target**: represents a variable divert target, for example as used in the following ink:

        VAR x = -> somewhere

    Represented in runtime JSON as an object of the form: `{"^->": "path.to.target"}`

* **variable pointer**: used for references to variables, for example when declaring a function with the following ink:

        == function myFunction(ref x) ==

    Represented in runtime JSON as an object of the form: `{"^var": "varname", "ci": 0}`. Where `ci` is "context index", with the following possible values:

    * **-1** - default value, context yet to be determined.
    * **0**  - Variable is a global
    * **1 or more** - variable is a local/temporary in the callstack element with the given index.

* **list**: List values. The initial values of `list` variables are created like this in the `global decl` sub-container of `root`. If the list contains any objects, or does not have any origin lists, it is represented as follows:
    ```json
    {
        "list": {
            <qualified value name>: <numeric value>,
            ...
        }
    }
    ```
    With `qualified value name` being a dot-separated `string` containing the name of the origin list and the item name, and `numeric value` being the numeric value of the item from its origin list.

    If the list does not contain any objects, but does have origin lists, it is instead represented as follows:
    ```json
    {
        "list": {},
        "origins": [
            <origin list name>,
            ...
        ]
    }
    ```
    With `origin list name` being the name of one of the lists that are origins of this list.

### Deprecated Values

Legacy tags are recognized by the runtime engine and deserializer, but are not output by the serializer. They should therefore only appear in old runtime JSON files. They take the following form:
```json
{
    "#": <tag text>
}
```

Legacy tags exist only for backwards compatibility, and should be ignored in favour of dynamic tags, which use the `"#"` and `"/#"` control commands (see [Control Commands](#control_commands)). Tag objects still exist within the runtime, but only for flattening tags down when evaluating `ControlCommand.EndString` (`"/str"`).

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
* `"turn"` - Pushes an integer with the current turn number to the evaluation stack.
* `"turns"` - Pops from the evaluation stack, expecting to see a divert target for a knot, stitch, gather, or choice. Pushes an integer with the number of turns since that target was last visited by the story engine.
* `"visit"` - Pushes an integer with the number of visits to the current container by the story engine.
* `"seq"` - Pops an integer, expected to be the number of elements in a sequence that's being entered. In return, it pushes an integer with the next sequence shuffle index to the evaluation stack. This shuffle index is derived from the number of elements in the sequence, the number of elements in it, and the story's random seed from when it was first begun.
* `"thread"` - Clones/starts a new thread, as used with the `<- knot` syntax in ink. This essentially clones the entire callstack, branching it.
* `"done"` - Tries to close/pop the active thread, otherwise marks the story flow safe to exit without a loose end warning.
* `"end"` - Ends the story flow immediately, closes all active threads, unwinds the callstack, and removes any choices that were previously created.
* `"readc"` - Pops from the evaluation stack, expecting to see a divert target for a knot, stitch, gather, or choice. Pushes an integer with the number of times that target has been visited by the story engine.
* `"rnd"` - Pops two values from the evaluation stack, expecting to see two `int`s. Generates a random value between the two integers, with the first value popped as the maximum and the second as the minimum, and pushes the random value to the stack.
* `"srnd"` - Pops one value from the evaluation stack, expecting to see an `int`. Sets the seed for the random generator to the given value and resets any additional randomness factors, then pushes `void` to the stack.
* `"listInt"` - Pops two values from the evaluation stack, expecting to see an `int` and then a `string`. Pushes a `list` containing only the list item with the given value (from the `int`) from the specified list (from the `string`).
* `"range"` - Pops three values from the evaluation stack. The first two are either `int`s or `list`s, the third is a `list`. If the first argument is a `list`, the value of the lowest item will be used. If the second argument is a `list`, the value of the highest item will be used. Generates a `list` containing every item from the third value that is between the bounds of the first and second values, inclusive. 
* `"lrnd"` - Pops one value from the evaluation stack, expecting to see a `list`. Pushes a `list` containing one random item from the argument to the stack.
* `"#"` - Adds a marker to the output stream to indicate that the following `string` values belong to a tag, until a `"/#"` or another `"#"` is found. If another `"#"` marker is found, it marks the beginning of a new tag.
* `"/#"` - Adds a marker to the output stream indicating that the `string` values between it and the preceding `"#"` are all part of a tag. Should only be encountered in string evaluation when generating text for a choice. In that case, the `string` values in the output stream since the last `"#"` are removed and added as a tag to the choice.

## Native functions

These are mathematical and logical functions that pop 1 or 2 arguments from the evaluation stack, evaluate the result, and push the result back onto the evaluation stack. Arguments are popped as a group, so the bottommost value is the first argument and the topmost is the second argument. Types are coerced so that both arguments are of the same type, see [Type coercion](#type-coercion). This is why `bool`s are not listed as an argument type.

The following operators are supported:

| Operator | Supported Types | No. Arguments | Effects |
| -------- | --------------- | ------------- | ----- |
| `"+"`    | `int`, `float`, `string`, `list` | 2 | Addition on `int`s and `float`s, concatination on `string`s, set union on `list`s. |
| `"-"`    | `int`, `float`, `list` | 2 | Subtraction on `int`s and `float`s, set difference on `list`s. |
| `"*"`    | `int`, `float` | 2 | Standard multiplication. |
| `"/"`    | `int`, `float` | 2 | Standard division. |
| `"%"`    | `int`, `float` | 2 | Modulo operator (`fmod` for `float`s). |
| `"_"`    | `int`, `float` | 1 | Negation. |
| `"=="`   | `int`, `float`, `string`, `list`, `divert target` | 2 | Equal. *Returns a `bool`.*  |
| `"!="`   | `int`, `float`, `string`, `list`, `divert target` | 2 | Not Equal. *Returns a `bool`.* |
| `">"`    | `int`, `float`, `list` | 2 | Greater than. [See below for lists](#comparison-operators-and-lists). *Returns a `bool`.* |
| `"<"`    | `int`, `float`, `list` | 2 | Less than. [See below for lists](#comparison-operators-and-lists). *Returns a `bool`.* |
| `">="`   | `int`, `float`, `list` | 2 | Greater than or equals. [See below for lists](#comparison-operators-and-lists). *Returns a `bool`.* |
| `"<="`   | `int`, `float`, `list` | 2 | Less than or equals. [See below for lists](#comparison-operators-and-lists). *Returns a `bool`.* |
| `"!"`    | `int`, `float`, `list` | 1 | Unary not. Returns `true` for `int`s and `float`s if the argument is equal to 0. Returns 1 for `list`s if the provided list has at least one element, otherwise returns 0. |
| `"&&"`   | `int`, `float`, `list` | 2 | Logical and. Returns `true` if both arguments are truthy. For `int`s and `float`s, this means nonzero. For `list`s, this means that the list contains at least one element. |
| `"\|\|"`   | `int`, `float`, `list` | 2 | Logical or. Returns `true` if either argument is truthy.
| `"MIN"`  | `int`, `float` | 2 | Minimum. Returns the lowest of the two arguments. |
| `"MAX"`  | `int`, `float` | 2 | Maximum. Returns the highest of the two arguments. |
| `"POW"`  | `int`, `float` | 2 | Exponentiation. Returns the first argument raised to the power of the second argument. Always casts to floats to handle negative exponents. |
| `"FLOOR"`| `int`, `float` | 1 | Floor. Returns the argument rounded *down* to the nearest whole number. |
| `"CEILING"` | `int`, `float` | 1 | Ceiling. Returns the argument rounded *up* to the nearest whole number. |
| `"INT"` | `int`, `float` | 1 | Returns the argument as an `int`, rounded towards zero. |
| `"FLOAT"` | `int`, `float` | 1 | Returns the argument as a `float`. |
| `"?"`    | `string`, `list` | 2 | If `string`: Returns `true` if the second argument is a substring of the first argument. If `list`: Returns `true` if the first argument contains every element of the second argument. If either list is empty, returns `false`. |
| `"!?"`   | `string`, `list` | 2 | If `string`: Returns `true` if the second argument is *not* a substring of the first argument. If `list`: returns `true` if the first argument does *not* contain every element in the second agument. If either list is empty, returns `true`. |
| `"L^"`   | `list`           | 2 | Intersection. Returns a list that contains only elements that are in *both* of the argument lists. Represented as `"^"` within the runtime, but not the JSON representation in order to not be confused with strings. |
| `"LIST_MIN"` | `list`       | 1 | Returns a list containing only the minimum item in the list. |
| `"LIST_MAX"` | `list`       | 1 | Returns a list containing only the maximum item in the list. |
| `"LIST_ALL"` | `list`       | 1 | Returns a list with every item from the original list definition to which the argument belongs. |
| `"LIST_COUNT"` | `list`     | 1 | Returns the number of elements in the argument. |
| `"LIST_VALUE"` | `list`     | 1 | Returns the value of the maximum item in the list. |
| `"LIST_INVERT"` | `list`    | 1 | Returns a list containing every item from the original list definition that is not in the argument list. |

### Comparison operators and Lists
When comparing lists, comparison operators (excluding equality and inequality, so `">"`, `"<"`, `">="`, `"<="`) compare the highest and lowest values in the lists.

`">"` returns `true` if the *minimum* value in the first argument is larger than the *maximum* value in the second argument.
`"<"` does the same, but with *maximum* value of the first argument being compared with the *minimum* value of the second argument.

`">="` and `"<="` both check if their relation holds true with the minimum and maximum values of both lists.
`">="` returns `true` if the *minimum* value of the first argument is greater than or equal to the *minimum* value of the second argument, *and* if the *maximum* value of the first argument is greater than or equal to the *maximum* value of the second argument.
`"<="` does the same, but checking if the values in the first argument are less than or equal to the values in the second argument.

### Type coercion
When a native function is called, its arguments may be coerced. This is done so that binary operations only have one type for their arguments and for specific unary operators.

Types exist in the following hierarchy:
`bool` < `int` < `float` < `list` < `string`
If a native function is called with two different types, the lower type in the hierarchy is coerced to the higher type. `list`s are an exception to this. If one of the arguments is a `list`, the other argument must be another `list` or an `int`, otherwise an unrecoverable error occurs.

Diverts and variable pointers are not a part of this hierarchy.

Coercions work as follows:
* `bool` - `bool` is *always* coerced to another type, usually `int`.
  * `int` or `float`: `true` becomes `1` and `false` becomes `0`.
* `int`
  * `bool`: `0` becomes `false`, all other values become `true`.
  * `float`: Remains the same value, but a `float`.
  * `string`: Becomes a `string` representation of the value.
  * `list`: Becomes a `list` containing only the item that is equivalent to the value of the `int` from the definition of the `list`.
* `float`
  * `bool`: `0` becomes `false`, all other values become `true`.
  * `int`: Becomes an int, rounded towards 0.
  * `string`: Becomes a `string` representation of the value, serialized according to .NET's InvariantCulture format.
* `list`
  * `int` or `float`: If `list` is empty, becomes `0`. Otherwise becomes the value of the maximum item in the list.
  * `string`: If `list` is empty, becomes `""`. Otherwise becomes the full dot-seperated name of the maximum item in the list.
* `string`
  * `int`: Tries to parse the `string` value as an `int`.
  * `float`: Tries to parse the `string` value as a `float`, deserialized according to .NET's InvariantCulture format and Float number style.

Any types not listed cannot be coerced. Diverts and variable pointers can't be coerced to or from at all.

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
If the reference is to the name of an item from a list, a list containing only the named list item will be pushed. If multiple lists have items with the same name, a fully-qualified name must be used.

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

```json
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

## Glue

Glue is represented as `"<>"`. Glue removes all whitespace from the end of the output stream until no newlines remain. Any whitespace before the first newline remains as is. Glue also causes all future whitespace to not be appended to the output stream until non-whitespace text is encountered.
