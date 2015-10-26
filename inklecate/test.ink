VAR x = 0
Start: {x}
Pre-increment
~ increment(x)
Post-increment. Now: {x}.
DONE

== function increment(ref r) ==
Incrementing: {r}.
~ r = r + 1
Incremented to: {r}.
~ return

/*
VAR result = 0
~ factorialByRef(result, 5)
{ result }


== function factorialByRef(ref r, n) ==
{ r == 0:
    ~ r = 1
}
{ n > 1:
    ~ r = r * n
    ~ factorialByRef(r, n-1)
}
~ return
*/


/*
~ temp x = 3
{x}

== knot ==
Not hello world.
VAR x = 5
-> DONE
*/