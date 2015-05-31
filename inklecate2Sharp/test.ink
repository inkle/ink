== test ==
Hello world {test}. {other:Other has been seen {other} times.}
 * loop  ==> test
 * other ==> other
 * done  => inner

= inner
  This is inner! Inner has been seen {test.inner} times.
  ==> test

== other ==
Other!
 ==> test