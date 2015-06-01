~ var x = 3.1415926
{x}

== test ==
Hello world.
 * choice 1     { x >= 3 }
    Chosen 1.
 * choice 2 ==> out    { x > 3 }
 * choice 3
    Chosen 3.
- 
  Done.

== out ==
 Out!