# Glossary

_A glossary of terms used by the runtime_

* __Container__
: the base element of JSON compiled stories. A Container may be __named__. A Container may contain other Containers (named or anonymous).

* __Control Command__
: a built-in instruction destined to be read and executed by the interpreter

* __Choice Point__
: the generator for a single choice. Its content (text) and behaviour are defined at runtime

* __Divert__
: a generalisation of the divert concept found in ink source. See [Type of Diverts](diverts.md)

* __Glue__
: an invisible textual value akin to an unbreakable space

* __List__
: what exactly is a LIST is an open question fairly debated on the community discord.

* __List item__
: Lists "contain" List items.

* __Native Function Call__
: an operation performed internally on behalf of the interpreter. See [Native functions](native-functions.md)

* __StringValue__
: a textual value

* __Value__
: a integer, float or boolean value. In the context of a boolean operation, any non-zero value is considered truthy.

* __Variable__
: a reference to a slot in the evaluation stack. It may contains a Value, a StringValue or a Divert. It may be global or temporary. See [Variable Assignment](variables.md)

* __Void__
: a special Value returned by function calls that don't return anything