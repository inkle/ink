# Compiled JSON reference

_Glossary & Examples of each type of object & instructions the ink compiler will generate_

The [root container](architecture.md) should be parsed as a tree and each element interpreted as follow : (see [Glossary](glossary.md) for definitions)

* any number or boolean is a __Value__.
* any string that begins with a `^` is a __StringValue__. The initial `^` should be ignored.
* the special string `<>` is a __Glue__.
* strings matching exactly a __Control Command__ is such.
* strings matching exactly a __Native Function Call__ is such.
    * Exception : `L^` matches `^` (list intersect operation)
* `"void"` is the special Value __Void__
* __TODO__ : named/flagged content
* any array is a __Container__ to be parsed recursively.
* any `null` is kept as is.
