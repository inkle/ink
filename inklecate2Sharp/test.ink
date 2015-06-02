~ var lunch = 1

== start ==
 You're at the start.
  ==> goto(==> lake, ==> forest)

== goto(firstPlace, secondPlace) ==
  ~ firstPlace()
  ~ secondPlace()

== lake ==
 You're at the lake.
 ~ eat()


== forest ==
 You're at the forest
 ~ eat()

== eat ==
 {lunch > 0:You have lunch.|Your stomach rumbles}
 ~ lunch = lunch - 1