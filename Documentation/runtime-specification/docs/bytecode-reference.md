# Compiled JSON reference

_Glossary & Examples of each type of object & instructions the ink compiler will generate_

The [root container](architecture.md) should be parsed as a tree and each element interpreted as follow : (see [Glossary](glossary.md) for definitions)

* any number or boolean is a __Value__.
* any string that begins with a `^` is a __StringValue__. The initial `^` should be ignored.
* the special string `<>` is a __Glue__.