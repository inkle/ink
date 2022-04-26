# Architecture

_High-level explanation of the runtime architecture_

The general form of a compiled JSON story is an object that *must* contain :

``inkVersion`` : number : the ink version number. Current version is ``20``.

``root`` : array : the main story [Container](glossary.md#container)

``listDef`` : object (optional): a ListDefinition


The minimal compiled ink story : (corresponding to an empty file)
```json
{
 "inkVersion":20,
 "root":[[["done",{"#n":"g-0"}],null],"done",null],
 "listDefs":{}
}
```