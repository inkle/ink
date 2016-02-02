# Writing with ink

## Introduction

**ink** is a scripting language built around the idea of marking up pure-text with flow in order to produce interactive scripts. 

At its most basic, it can be used to write a Choose Your Own-style story, or a branching dialogue tree. But its real strength is in writing dialogues with lots of options and lots of recombination of the flow. 

**ink** offers several features to enable non-techincal writers to branch often, and play out the consequences of those branches, in both minor and major ways, without fuss. 

The script aims to be clean and logically ordered, so branching dialogue can be tested "by eye". The flow is described in a declarative fashion where possible.

It's also designed with redrafting in mind; so editing a flow should be fast.

# Part One: The Basics

## 1) Content

### The simplest ink script

The most basic ink script is just text in a .ink file.

	Hello, world!
	
On running, this will output the content, and then stop.

Text on separate lines produces new paragraphs. The script:

	Hello, world!
	Hello?
	Hello, are you there?
	
produces output that looks the same.


### Comments 

By default, all text in your file will appear in the output content, unless specially marked up. 

The simplest mark-up is a comment. **ink** supports two kinds of comment. There's the kind used for someone reading the code, which the compiler ignores:

	"What do you make of this?" she asked. 
	
	// Something unprintable...
	
	"I couldn't possibly comment," I replied.
	
	/*
		... or an unlimited block of text
	*/

and there's the kind used for reminding the author what they need to do, that the compiler prints out during compilation:

	
	TODO: Write this section properly!

		
 
## 2) Choices 

Input is offered to the player via text choices. A text choice is indicated by an `*` character. 

If no other flow instructions are given, once made, the choice will flow into the next line of text.

	Hello world!
	*	Hello back!
		Nice to hear from you!
	
This produces the following game:

	Hello world 
	1: Hello back! 
	
	> 1
	Hello back!
	Nice to hear from you.	

By default, the text of a choice appears again, in the output. 
	
### Suppressing choice text 

Some games separate the text of a choice from its outcome. In **ink**, if the choice text is given in square brackets, the text of the choice will not be printed into response.

	Hello world!
	*	[Hello back!]
		Nice to hear from you!
	
produces
	
	Hello world 
	1: Hello back! 
	
	> 1
	Nice to hear from you.	

#### Advanced: mixing choice and output text

The square brackets in fact divide up the option content. What's before is printed in both choice and output; what's inside only in choice; and what's after, only in output. Effectively, they provide alternative ways for a line to end.

	Hello world!
	*	Hello [back!] right back to you!
		Nice to hear from you!
	
produces
	
	Hello world 
	1: Hello back!
	> 1
	Hello right back to you!
	Nice to hear from you.	
	
This is most useful when writing dialogue choices:

	"What that's?" my master asked.
	*	"I am somewhat tired[."]," I repeated.
		"Really," he responded. "How deleterious."

### Multiple Choices

To make choices really choices, we need to provide alternatives. We can do this simply by listing them:

	"What that's?" my master asked.
	*	"I am somewhat tired[."]," I repeated.
		"Really," he responded. "How deleterious."
	*	"Nothing, Monsieur!"[] I replied.
		"Very good, then."
	*  "I said, this journey is appalling[."] and I want no more of it."
		"Ah," he replied, not unkindly. "I see you are feeling frustrated. Tomorrow, things will improve."
 
This produces the following game:

	"What that's?" my master asked.
	
	1: "I am somewhat tired."
	2: "Nothing, Monsieur!"
	3: "I said, this journey is appalling."
	
	> 3
	"I said, this journey is appalling and I want no more of it."
	"Ah," he replied, not unkindly. "I see you are feeling frustrated. Tomorrow, things will improve."

The above syntax is enough to write a single set of choices. In a real game, we'll want to move the flow from one point to another based on what the player chooses. To do that, we need to introduce a bit more structure.

## 3) Knots

### Pieces of content are called knots 

To allow the game to branch we need to mark up sections of content with names (as an old-fashioned gamebook does with its 'Paragraph 18', and the like.) 

These sections are called "knots" and they're the fundamental structural unit of ink content.

### Writing a knot

The start of a knot is indicated by two or more equals signs, as follows.

	=== top_knot ===
	
(The equals signs on the end are optional; and the name needs to be a single word with no spaces.)

The start of a knot is a header; the content that follows will be inside that knot.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.

#### Advanced: a knottier "hello world"

Note that the game will automatically run the first knot it finds in a story if there is no "non-knot" content, so the simplest script is now:
	
	=== top_knot ===
	Hello world!

However, **ink** doesn't like loose ends, and produces a warning on compilation and/or run-time when it thinks this has happened. The script above produces this on compilation:

	WARNING: Apparent loose end exists where the flow runs out. Do you need a '-> DONE' statement, choice or divert? on line 3 of tests/test.ink

and this on running:

	Runtime error in tests/test.ink line 3: ran out of content. Do you need a '-> DONE' or '-> END'?
	
The following plays and compiles without error:

	=== top_knot ===
	Hello world!
	-> DONE
	
`-> DONE` is a marker for both the writer and the compiler; it means "the story flow intentionally ends here".

## 4) Diverts

### Knots divert to knots

You can tell the story to move from one knot to another using `->`, a "divert arrow". Diverts happen immediately without any user input.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.
	-> hurry_home 
	
	=== hurry_home === 
	We hurried home to Savile Row as fast as we could.
	
#### Diverts are invisible

Diverts are intended to be seamless and can even happen mid-sentence:

	=== hurry_home ===
	We hurried home to Savile Row -> as_fast_as_we_could
	
	=== as_fast_as_we_could ===
	as fast as we could.
	
produces the same line as above:
	
	We hurried home to Savile Row as fast as we could.

#### Glue
	
The default behaviour inserts line-breaks before every new line of content. In some cases, however, content must insist on not having a line-break, and it can do so using `<>`, or "glue".

	=== hurry_home ===
	We hurried home <> 
	-> to_savile_row 
	
	=== to_savile_row ===
	to Savile Row 
	-> as_fast_as_we_could
	
	=== as_fast_as_we_could ===
	<> as fast as we could.
	
also produces:

	We hurried home to Savile Row as fast as we could.

You can't use too much glue: multiple glues next to each other have no additional effect. (And there's no way to "negate" a glue; once a line is sticky, it'll stick.)

	
## 5) Branching The Flow

### Basic branching

Combining knots, options and diverts gives us the basic structure of a choose-your-own game.

	== paragraph 1 === 
	You stand by the wall of Analand, sword in hand.
	* [Open the gate] -> paragraph_2 
	* [Smash down the gate] -> paragraph_3
	* [Turn back and go home] -> paragraph_4

	=== paragraph_2 ===
	You open the gate, and step out onto the path. 
	
	...

### Branching and joining

Using diverts, the writer can branch the flow, and join it back up again, without showing the player that the flow has rejoined.

	=== back_in_london ===
	
	We arrived into London at 9.45pm exactly.
	
	*	"There is not a moment to lose!"[] I declared.
		-> hurry_outside 
		
	*	"Monsieur, let us savour this moment![] I declared.
		My master clouted me firmly around the head and dragged me out of the door. 
		-> dragged_outside 
	
	*	[We hurried home] -> hurry_outside
	
		
	=== hurry_outside ===
	We hurried home to Savile Row -> as_fast_as_we_could
	
	
	=== dragged_outside === 
	He insisted that we hurried home to Savile Row 
	-> as_fast_as_we_could


	=== as_fast_as_we_could === 
	<> as fast as we could.


### The story flow 

Knots and diverts combine to create the basic story flow of the game. This flow is "flat" - there's no call-stack, and diverts aren't "returned" from. 

In most ink scripts, the story flow starts at the top, bounces around in a spaghetti-like mess, and eventually, hopefully, reaches a `-> DONE`.

The very loose structure means writers can get on and write, branching and rejoining without worrying about the structure that they're creating as they go. There's no boiler-plate to creating new branches or diversions, and no need to track any state.

#### Advanced: Loops

You absolutely can use diverts to create looped content, and **ink** has several features to exploit this, including ways to make the content vary itself, and ways to control how often options can be chosen. 

See the sections on Varying Text and Conditional Options for more information.

Oh, and the following is legal and not a great idea:

	=== round ===
	and
	-> round

## 6) Includes and Stitches

### Knots can be subdivided

As stories get longer, they become more confusing to keep organised without some additional structure. 

Knots can include sub-sections called "stitches". These are marked using a single equals sign.

	=== the_orient_express ===
	= in_first_class 
		...
	= in_third_class
		...
	= in_the_guards_van 
		...
	= missed_the_train
		...

One could use a knot for a scene, for instance, and stitches for the events within the scene.
		
### Stitches have unique names		

A stitch can be diverted to using its "address".

	*	[Travel in third class]
		-> the_orient_express.in_third_class
	
	*	[Travel in the guard's van]
		-> the_orient_express.in_the_guards_van 
				
### The first stitch is the default

Diverting to a knot which contains stitches will divert to the first stitch in the knot. So:

	*	[Travel in first class]
		"First class, Monsieur. Where else?"
		-> the_orient_express

is the same as:

	*	[Travel in first class]
		"First class, Monsieur. Where else?"
		-> the_orient_express.in_first_class 
		
(...unless we move the order of the stitches around inside the knot!)

The first stitch can also be left unnamed.

	=== the_orient_express === 

	We boarded the train, but where?
	*	[First class] -> in_first_class
	*	[Second class] -> in_second_class

	= in_first_class 
		...
	= in_second_class
		...


### Local diverts 

From inside a knot, you don't need to use the full address for a stitch.

	-> the_orient_express

	=== the_orient_express ===
	= in_first_class 
		I settled my master.
		*	[Move to third class]
			-> in_third_class

	= in_third_class
		I put myself in third.

This means stitches and knots can't share names, but several knots can contain the same stitch name. (So both the Orient Express and the SS Mongolia can have first class.) 

The compiler will warn you if ambiguous names are used.

### Script files can combined

You can also split your content across multiple files, using an include statement.

	~ include newspaper.ink
	~ include cities/vienna.ink
	~ include journeys/orient_express.ink
	
Include statements should always go at the top of a file, and not inside knots.

There are no rules about what file a knot must be in to be diverted to. (In other words, separating files has no effect on the game's namespacing.)


## 8) Varying Choices 

### Choices can only be used once

By default, every choice in the game can only be chosen once. If you don't have loops in your story, you'll never notice this behaviour. But if you do use loops, you'll quickly notice your options disappearing...

	=== find_help ===
	
		You search desperately for a friendly face in the crowd. 
		*	The woman in the hat[?] pushes you roughly aside. -> find_help
		*	The man with the briefcase[?] looks disgusted as you stumble past him. -> find_help 
		
produces:

	You search desperately for a friendly face in the crowd.
	
	1: The woman in the hat?
	2: The man with the briefcase?
	
	> 1
	The woman in the hat pushes you roughly aside.
	You search desperately for a friendly face in the crowd.
	
	1: The man with the briefcase?
	
	> 

... and on the next loop you'll have no options left.

#### Fallback choices 

The above example stops where it does, because the next choice ends up in an "out of content" run-time error. 
	
	> 1
	The man with the briefcase looks disgusted as you stumble past him.
	You search desperately for a friendly face in the crowd.
	
	Runtime error in tests/test.ink line 6: ran out of content. Do you need a '-> DONE' or '-> END'?

We can resolve this with a 'fallback choice'. Fallback choices are never displayed to the player, but are 'chosen' by the game if no other options exist. 

A fallback choice is simply a "choice without choice text":

	*	-> out_of_options

We can use the square-bracket notation here as well:

	* [] Mulder never could explain how he got out of that burning box car. -> season_2

#### Example of a fallback choice

Adding this into the previous example gives us: 

	=== find_help ===
	
		You search desperately for a friendly face in the crowd. 
		*	The woman in the hat[?] pushes you roughly aside. -> find_help
		*	The man with the briefcase[?] looks disgusted as you stumble past him. -> find_help 
		*	[] But it is too late: you collapse onto the station platform. This is the end.
			-> DONE
	
and produces:

	You search desperately for a friendly face in the crowd.

	1: The woman in the hat?
	2: The man with the briefcase?

	> 1
	The woman in the hat pushes you roughly aside.
	You search desperately for a friendly face in the crowd.
	
	1: The man with the briefcase?

	> 1
	The man with the briefcase looks disgusted as you stumble past him.
	You search desperately for a friendly face in the crowd.
	But it is too late: you collapse onto the station platform. This is the end.


### Sticky choices

The 'once-only' behaviour is not always what we want, of course, so we have a second kind of choice: the "sticky" choice. A sticky choice is simply one that doesn't get used up, and is marked by a `+` bullet.

	=== homers_couch ===
		+	[Eat another donut]
			You eat another donut. -> homers_couch
		*	[Get off the couch] 
			You struggle up off the couch to go and compose epic poetry.
			-> DONE

### Conditional Choices

You can also turn choices on and off by hand. **ink** has quite a lot of logic available, but the simplest tests is "has the player seen a particular piece of content".

Every knot/stitch in the game has a unique address (so it can be diverted to), and we use the same address to test if that piece of content has been seen. 

	*	{ not visit_paris } 	[Go to Paris] -> visit_paris
	+ 	{ visit_paris 	 } 		[Return to Paris] -> visit_paris 

	*	{ visit_paris.met_estelle } [ Telephone Mme Estelle ] -> phone_estelle 
	
Note that the test `knot_name` is true if *any* stitch inside that knot has been seen.

Note also that conditionals don't override the once-only behaviour of options, so you'll still need sticky options for repeatable choices.

#### Advanced: multiple conditions

You can use several logical tests on an option; if you do, *all* the tests must all be passed for the option to appear.

	*	{ not visit_paris } 	[Go to Paris] -> visit_paris
	+ 	{ visit_paris } { not bored_of_paris } 
								[Return to Paris] -> visit_paris 



#### Advanced: knot/stitch labels are actually read counts

The test: 

	*	{seen_clue} [Accuse Mr Jefferson]

is actually testing an *integer* and not a true/false flag. A knot or stitch used this way is actually an integer variable containing the number of times the content at the address has been seen by the player. 

If it's non-zero, it'll return true in a test like the one above, but you can also be more specific as well:

	* {seen_clue > 3} [Flat-out arrest Mr Jefferson]

#### Advanced: more logic

**ink** supports a lot more logic and conditionality than covered here - see the section on 'variables and logic'.


## 9) Variable Text

### Text can vary

So far, all the content we've seen has been static, fixed pieces of text. But content can also vary at the moment of being printed. 

### Sequences, cycles and other lists

The simplest variations of text are from lists, which are selected from depending on some kind of rule. **ink** supports several types. Lists are written inside `{`...`}` curly brackets, with elements separated by `|` symbols (vertical divider lines).

These are only useful if a piece of content is visited more than once!

#### List types

**Sequences** (the default):

A sequence (or a "stopping list") is a list that tracks how many times its been seen, and each time, shows the next element along. When it runs out of new content it continues the show the final element.

	The radio hissed into life. {"Three!"|"Two!"|"One!"|There was the white noise racket of an explosion.|But it was just static.}

	{I bought a coffee with my five-pound note.|I bought a second coffee for my friend.|I didn't have enough money to buy any more coffee.}
			
**Cycles** (marked with a `&`): 
	
Cycles are like sequences, but they loop their content.
	
	It was {&Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday} today.
	

**Once-only** (marked with a `!`):
	
Once-only lists are like sequences, but when they run out of new content to display, they display nothing. (You can think of a once-only list as a sequence with a blank last entry.)
	
	He told me a joke. {!I laughed politely.|I smiled.|I grimaced.|I promised myself to not react again.}
	
**Shuffles** (marked with a `~`):
	
Shuffles produce randomised output.
	
	I tossed the coin. {~Heads|Tails}.

#### Features of Lists 

Lists can contain blank elements.

	I took a step forward. {!||||Then the lights went out. -> eek}

Lists can be nested.

	The Ratbear {&{wastes no time and |}swipes|scratches} {&at you|into your {&leg|arm|cheek}}.

Lists can include divert statements. 

	I {waited.|waited some more.|snoozed.|woke up and waited more.|gave up and left. -> leave_post_office}

They can also be used inside choice text:

	+ 	"Hello, {&Master|Monsieur Fogg|you|brown-eyes}!"[] I declared.
	
(...with one caveat; you can't start an option's text with a `{`, as it'll look like a conditional.)

#### Examples

Lists can be used inside loops to create the appearance of intelligent, state-tracking gameplay without particular effort.

Here's a one-knot version of whack-a-mole. Note we use once-only options, and a fallback, to ensure the mole doesn't move around, and the game will always end.

	=== whack_a_mole ===
		{I heft the hammer.|{~Missed!|Nothing!|No good. Where is he?|Ah-ha! Got him! -> DONE}}
		The {&mole|{&nasty|blasted|foul} {&creature|rodent}} is {in here somewhere|hiding somewhere|still at large|laughing at me|still unwhacked|doomed}. <>
		{!I'll show him!|But this time he won't escape!}
		* 	[{&Hit|Smash|Try} top-left] 	-> whack_a_mole
		*  [{&Whallop|Splat|Whack} top-right] -> whack_a_mole
		*  [{&Blast|Hammer} middle] -> whack_a_mole
		*  [{&Clobber|Bosh} bottom-left] 	-> whack_a_mole
		*  [{&Nail|Thump} bottom-right] 	-> whack_a_mole
		*  [] Then you collapse from hunger. The mole has defeated you! 
			-> DONE

produces the following 'game':

	I heft the hammer.
	The mole is in here somewhere. I'll show him!
	
	1: Hit top-left
	2: Whallop top-right
	3: Blast middle
	4: Clobber bottom-left
	5: Nail bottom-right
	
	> 1
	Missed!
	The nasty creature is hiding somewhere. But this time he won't escape!
	
	1: Splat top-right
	2: Hammer middle
	3: Bosh bottom-left
	4: Thump bottom-right
	
	> 4
	Nothing!
	The mole is still at large. 
	1: Whack top-right
	2: Blast middle
	3: Clobber bottom-left
	
	> 2
	Where is he?
	The blasted rodent is laughing at me. 
	1: Whallop top-right
	2: Bosh bottom-left

	> 1
	Ah-ha! Got him!
	

And here's a bit of lifestyle advice. Note the sticky choice - the lure of the television will never fade:

	=== turn_on_television === 
	I turned on the television {for the first time|for the second time|again|once more}, but there was {nothing good on, so I turned it off again|still nothing worth watching|even less to hold my interest than before|nothing but rubbish|a program about sharks and I don't like sharks|nothing on}.
	+	[Try it again]	 		-> turn_on_television
	*	[Go outside instead]	-> go_outside_instead
	
    === go_outside_instead ===
    -> DONE



#### Advanced: Multiline lists
**ink** has another format for making lists of varying content blocks, too. See the section on "multiline blocks" for details.



### Conditional Text

Text can also vary depending on logical tests, just as options can.

	{met_blofeld: "I saw him. Only for a moment." }

and

	"His real name was {met_blofeld.learned_his_name: Franz|a secret}."

These can appear as separate lines, or within a section of content. They can even be nested, so:

	{met_blofeld: "I saw him. Only for a moment. His real name was {met_blofeld.learned_his_name: Franz|kept a secret}." | "I missed him. Was he particularly evil?" }
	
can produce either:

	"I saw him. Only for a moment. His real name was Franz."

or:

	"I saw him. Only for a moment. His real name was kept a secret."
	
or: 

	"I missed him. Was he particularly evil?"

## 10) Game Queries

**ink** provides a few useful 'game level' queries about game state, for use in conditional logic. They're not quite parts of the language, but they're always available, and they can't be edited by the author. In a sense, they're the "standard library functions" of the language.

The convention is to name these in capital letters.

### CHOICE_COUNT

`CHOICE_COUNT` returns the number of options created so far in the current chunk. So for instance.

	*	{false} Option A
	* 	{true} Option B
	*  {CHOICE_COUNT() == 1} Option C

produces two options, B and C. This can be useful for controlling how many options a player gets on a turn. 

### TURNS_SINCE

`TURNS_SINCE` returns the number of moves (formally, player inputs) since a particular knot/stitch was last visited.

A value of 0 means "was seen as part of the current chunk". A value of -1 means "has never been seen". Any other positive value means it has been seen that many turns ago.

	*	{TURNS_SINCE(-> sleeping.intro) > 10} You are feeling tired... -> sleeping 
	* 	{TURNS_SINCE(-> laugh) == 0}  You try to stop laughing.

Note that the parameter passed to `TURNS_SINCE` is a "divert target", not simply the knot address itself (because the knot address is a number - the read count - not a location in the story...)

TODO: (requirement of passing `-c` to the compiler)

#### Advanced: more queries

You can make your own external functions, though the syntax is a bit different: see the section on functions below.


# Part 2: Weave

So far, we've been building branched stories in the simplest way, with "options" that link to "pages". 

But this requires us to uniquely name every destination in the story, which can slow down writing and discourage minor branching. 

**ink** has a much more powerful syntax available, designed for simplifying story flows which have an always-forwards direction (as most stories do, and most computer programs don't).

This format is called "weave", and its built out of the basic content/option syntax with two new features: the gather mark, `-`, and the nesting of choices and gathers.

## 1) Gathers

### Gather points gather the flow back together 

Let's go back to the first multi-choice example at the top of this document. 

	"What's that?" my master asked.
		*	"I am somewhat tired[."]," I repeated.
			"Really," he responded. "How deleterious."
		*	"Nothing, Monsieur!"[] I replied.
		*  "I said, this journey is appalling[."] and I want no more of it."
			"Ah," he replied, not unkindly. "I see you are feeling frustrated. Tomorrow, things will improve."
		
In a real game, all three of these options might well lead to the same conclusion - Monsieur Fogg leaves the room. We can do this using a gather, without the need to create any new knots, or add any diverts.

	"What that's?" my master asked.
		*	"I am somewhat tired[."]," I repeated.
			"Really," he responded. "How deleterious."
		*	"Nothing, Monsieur!"[] I replied.
			"Very good, then."
		*  "I said, this journey is appalling[."] and I want no more of it."
		"Ah," he replied, not unkindly. "I see you are feeling frustrated. Tomorrow, things will improve."

	-	With that Monsieur Fogg left the room.

This produces the following playthrough:

	"What that's?" my master asked.
	
	1: "I am somewhat tired."
	2: "Nothing, Monsieur!"
	3: "I said, this journey is appalling."
	
	> 1
	"I am somewhat tired," I repeated.
	"Really," he responded. "How deleterious."
	With that Monsieur Fogg left the room.
	
### Options and gathers form chains of content

We can string these gather-and-branch sections together to make branchy sequences that always run forwards.

	=== escape === 
	I ran through the forest, the dogs snapping at my heels.
	
		* 	I checked the jewels[] were still in my pocket, and the feel of them brought a spring to my step. <>
		
		*  I did not pause for breath[] but kept on running. <>

		*	I cheered with joy. <>
	
	- 	The road could not be much further! Mackie would have the engine running, and then I'd be safe.
	  
		*	I reached the road and looked about[]. And would you believe it?
		* 	I should interrupt to say Mackie is normally very reliable[]. He's never once let me down. Or rather, never once, previously to that night.
	
	-	The road was empty. Mackie was nowhere to be seen.

This is the most basic kind of weave. The rest of this section details  additional features that allow weaves to nest, contain side-tracks and diversions, divert within themselves, and above all, reference earlier choices to influence later ones.

#### The weave philsophy 

Weaves are more than just a convenient encapsulation of branching flow; they're also a way to author more robust content. The `escape` example above has already four possible routes through, and a more complex sequence might have lots and lots more. Using normal diverts, one has to check the links by chasing the diverts from point to point and it's easy for errors to creep in. 

With a weave, the flow is guaranteed to start at the top and "fall" to the bottom. Flow errors are impossible in a basic weave structure, and the output text can be easily skim read. That means there's no need to actually test all the branches in game to be sure they work as intended.

Weaves also allow for easy redrafting of choice-points; in particular, it's easy to break a sentence up and insert additional choices for variety or pacing reasons, without having to re-engineer any flow.
	

## 2) Nested Flow

The weaves shown above are quite simple, "flat" structures. Whatever the player does, they take the same number of turns to get from top to bottom. However, sometimes certain choices warrant a bit more depth or complexity. 

For that, we allow weaves to nest.

This section comes with a warning. Nested weaves are very powerful and very compact, but they can take a bit of getting used to! 

### Options can be nested

Consider the following scene:

	- 	"Well, Poirot? Murder or suicide?"
	*	"Murder!"
	* 	"Suicide!"
	-	Ms. Christie lowered her manuscript a moment. The rest of the writing group sat, open-mouthed.

The first choice presented is "Murder!" or "Suicide!". If Poirot declares a suicide, there's no more to do, but in the case of murder, there's a follow-up question needed - who does he suspect? 

We can add new options via a set of nested sub-choices. We tell the script that these new choices are "part of" another choice by using two asterisks, instead of just one. 


	- 	"Well, Poirot? Murder or suicide?"
		*	"Murder!"
		 	"And who did it?"
			* * 	"Detective-Inspector Japp!"
			* * 	"Captain Hastings!"
			* * 	"Myself!"
		* 	"Suicide!"
		-	Mrs. Christie lowered her manuscript a moment. The rest of the writing group sat, open-mouthed.
		
(Note that it's good style to also indent the lines to show the nesting, but the compiler doesn't mind.)

And should we want to add new sub-options to the other route, we do that in similar fashion.

	- 	"Well, Poirot? Murder or suicide?"
		*	"Murder!"
		 	"And who did it?"
			* * 	"Detective-Inspector Japp!"
			* * 	"Captain Hastings!"
			* * 	"Myself!"
		* 	"Suicide!"
			"Really, Poirot? Are you quite sure?"
			* * 	"Quite sure."
			* *		"It is perfectly obvious."
		-	Mrs. Christie lowered her manuscript a moment. The rest of the writing group sat, open-mouthed.

Now, that initial choice of accusation will lead to specific follow-up questions - but either way, the flow will come back together at the gather point, for Mrs. Christie's cameo appearance.

But what if we want a more extended sub-scene?

### Gather points can be nested too

Sometimes, it's not a question of expanding the number of options, but having more than one additional beat of story. We can do this by nesting gather points as well as options.

	- 	"Well, Poirot? Murder or suicide?"
			*	"Murder!"
			 	"And who did it?"
				* * 	"Detective-Inspector Japp!"
				* * 	"Captain Hastings!"
				* * 	"Myself!"
				- - 	"You must be joking!"
				* * 	"Mon ami, I am deadly serious."
				* *		"If only..."
			* 	"Suicide!"
				"Really, Poirot? Are you quite sure?"
				* * 	"Quite sure."
				* *		"It is perfectly obvious."
			-	Mrs. Christie lowered her manuscript a moment. The rest of the writing group sat, open-mouthed.

If the player chooses the "murder" option, they'll have two choices in a row on their sub-branch - a whole flat weave, just for them. 

#### Advanced: What gathers do

Gathers are, hopefully, work intuitively, but their behaviou is a little harder to put into words: in general, after an option has been taken, the story finds the next gather down that isn't on a lower level, and diverts to it. 

The basic idea is this: options separate the paths of the story, and gathers bring them back together. (Hence the name, "weave"!)


### You can nest as many levels are you like

Above, we used two levels of nesting; the main flow, and the sub-flow. But there's no limit to how many levels deep you can go.

	-	"Tell us a tale, Captain!"
		*	"Very well, you sea-dogs. Here's a tale..."
			* * 	"It was a dark and stormy night..." 
					* * * 	"...and the crew were restless..." 
							* * * *  "... and they said to their Captain..." 
									* * * * *		"...Tell us a tale Captain!"
		*	"No, it's past your bed-time."
 	-	To a man, the crew began to yawn.

After a while, this sub-nesting gets hard to read and manipulate, so it's good style to divert away to a new stitch if a side-choice goes unwieldy. 

But, in theory at least, you could write your entire story as a single weave.

### Example: a conversation with nested nodes

Here's a longer example:

	- I looked at Monsieur Fogg 
	*	... and I could contain myself no longer.
		'What is the purpose of our journey, Monsieur?'
		'A wager,' he replied.
		* * 	'A wager!'[] I returned.
				He nodded. 
				* * * 	'But surely that is foolishness!'
				* * *  'A most serious matter then!'
				- - - 	He nodded again.
				* * *	'But can we win?'
						'That is what we will endeavour to find out,' he answered.
				* * *	'A modest wager, I trust?'
						'Twenty thousand pounds,' he replied, quite flatly.
				* * * 	I asked nothing further of him then[.], and after a final, polite cough, he offered nothing more to me. <>
		* * 	'Ah[.'],' I replied, uncertain what I thought.
		- - 	After that, <>
	*	... but I said nothing[] and <> 
	- we passed the day in silence.
	- -> DONE

with a couple of possible playthroughs. A short one:

	I looked at Monsieur Fogg
	
	1: ... and I could contain myself no longer.
	2: ... but I said nothing

	> 2
	... but I said nothing and we passed the day in silence.

and a longer one:

	I looked at Monsieur Fogg
	
	1: ... and I could contain myself no longer.
	2: ... but I said nothing
	
	> 1
	... and I could contain myself no longer.
	'What is the purpose of our journey, Monsieur?'
	'A wager,' he replied.
	
	1: 'A wager!'
	2: 'Ah.'
	
	> 1
	'A wager!' I returned.
	He nodded.
	
	1: 'But surely that is foolishness!'
	2: 'A most serious matter then!'
	
	> 2
	'A most serious matter then!'
	He nodded again.
	
	1: 'But can we win?'
	2: 'A modest wager, I trust?'
	3: I asked nothing further of him then.
	
	> 2
	'A modest wager, I trust?'
	'Twenty thousand pounds,' he replied, quite flatly.
	After that, we passed the day in silence.

Hopefully, this demonstrates the philosophy laid out above: that weaves offer a compact way to offer a lot of branching, a lot of choices, but with the guarantee of getting from beginning to end!


## 3) Tracking a Weave

Sometimes, the weave structure is sufficient. But when it's not, we need a bit more control.

### Weaves are largely unaddressed

By default, lines of content in a weave don't have an address or label, which means they can't be diverted to, and they can't be tested for. In the most basic weave structure, choices vary the path the player takes through the weave and what they see, but once the weave is finished those choices and that path are forgotten.

But should we want to remember what the player has seen, we can - we add in labels where they're needed using the `(label_name)` syntax.

### Gathers and options can be labelled

Gather points at any nested level can be labelled using brackets.

	-  (top) 

Once labelled, gather points can be diverted to, or tested for in conditionals, just like knots and stitches. This means you can use previous decisions to alter later outcomes inside the weave, while still keeping all the advantages of a clear, reliable forward-flow.

Options can also be labelled, just like gather points, using brackets. Label brackets come before conditions in the line.

These addresses can be used in conditional tests, which can be useful for creating options unlocked by other options.

	=== meet_guard ===
	The guard frowns at you.
	
	* 	(greet) [Greet him]
		'Greetings.'
	*	(get_out) 'Get out of my way[.'],' you tell the guard.
	
	- 	'Hmm,' replies the guard.

	*	{greet} 	'Having a nice day?' // only if you greeted him
	
	* 	'Hmm?'[] you reply.
	
	*	{get_out} [Shove him aside] 	 // only if you threatened him
		You shove him sharply. He stares in reply, and draws his sword!
		-> fight_guard 			// this route diverts out of the weave

	-	'Mff,' the guard replies, and then offers you a paper bag. 'Toffee?'


### Scope

Inside the same block of weave, you can simply use the label name; from outside the block you need a path, either to a different stitch within the same knot:

	=== knot ===
	= stitch_one 
		- (gatherpoint) Some content.
	= stitch_two 
		*	{stitch_one.gatherpoint} Option

or pointing into another knot:

	=== knot_one ===
	-	(gather_one)
		* {knot_two.stitch_two.gather_two} Option
		
	=== knot_two ===
	= stitch_two 
		- (gather_two) 
			*	{knot_one.gather_one} Option
	

#### Advanced: all options can be labelled

In truth, all content in ink is a weave, even if there are no gathers in sight. That means you can label *any* option in the game with a bracket label, and then reference it using the addressing syntax. In particular, this means you can test *which* option a player took to reach a particular outcome.

	=== fight_guard ===
	...
	= throw_something 
	*	(rock) [Throw rock at guard] -> thrown
	* 	(sand) [Throw sand at guard] -> thrown

	= throw
	You hurl {throw_something.rock:a rock|a handful of sand} at the guard.
	

#### Advanced: Loops in a weave

Labelling allows us to create loops inside weaves. Here's a standard pattern for asking questions of an NPC.

	- (opts)
		*	'Can I get a uniform from somewhere?'[] you ask the cheerful guard.
			'Sure. In the locker.' He grins. 'Don't think it'll fit you, thought.'
		*	'Tell me about the security system.'
			'It's ancient,' the guard assures you. 'Old as coal.'
		*	'Are there dogs?'
			'Hundreds,' the guard answers, with a toothy grin. 'Hungry devils, too.'
		// We require the player to ask at least one question
		*	{loop} [Enough talking] 
			-> done
	- (loop) 
		// loop a few times before the guard gets bored
		{ -> opts | -> opts | }
		He scratches his head.
		'Well, can't stand around talking all day,' he declares. 
	- (done)
		You thank the guard, and move away. 





#### Advanced: diverting to options

Options can also be diverted to: but the divert goes to the output of having chosen that choice, *as though the choice had been chosen*. So the content printed will ignore square bracketed text, and if the option is once-only, it will be marked as used up.

	- (opts)
	*	[Pull a face]
		You pull a face, and the soldier comes at you! -> shove

	*	(shove) [Shove the guard aside] You shove the guard to one side, but he comes back swinging.

	*	{shove} [Grapple and fight] -> fight_the_guard
	
	- 	-> opts

produces: 

	1: Pull a face
	2: Shove the guard aside
	
	> 1
	You pull a face, and the soldier comes at you! You shove the guard to one side, but he comes back swinging.
	
	1: Grapple and fight
	
	>
	
#### Advanced: Gathers directly after an option

The following is valid, and frequently useful.

	*	"Are you quite well, Monsieur?"[] I asked.
		- - (quitewell) "Quite well," he replied. 
	*	"How did you do at the crossword, Monsieur?"[] I asked.
		-> quitewell 
	*	I said nothing[] and neither did my Master.
	-	We feel into companionable silence once more.

Note the level 2 gather point directly below the first option: there's nothing to gather here, really, but it gives us a handy place to divert the second option to. 






# Part 3: Variables and Logic

So far we've made conditional text, and conditional choices, using tests based on what content the player has seen so far. 

**ink** also supports variables, both temporary and global, storing numerical and content data, or even story flow commands. It is fully-featured in terms of logic, and contains a few additional structures to help keep the often complex logic of a branching story better organised.


## 1) Global Variables

The most powerful kind of variable, and arguably the most useful for a story, is a variable to store some unique property about the state of the game - anything from the amount of money in the protagonist's pocket, to a value representing the protagonist's state of mind. 

This kind of variable is called "global" because it can be accessed from anywhere in the story - both set, and read from. (Traditionally, programming tries to avoid this kind of thing, as it allows one part of a program to mess with another, unrelated part. But a story is a story, and stories are all about consequences: what happens in Vegas rarely stays there.)

### Defining Global Variables

Global variables can be defined anywhere, via a `VAR` statement. They should be given an initial value, which defines what type of variable they are - integer, floating point (decimal), content, or a story address.

	VAR knowledge_of_the_cure = false
	VAR players_name = "Emilia"
	VAR number_of_infected_people = 521
	VAR current_epilogue = "-> they_all_die_of_the_plague"

### Using Global Variables

We can test global variables to control options, and provide conditional text, in a similar way to what we have previously seen.

	=== the_train ===
		The train jolted and rattled. { mood > 0:I was feeling positive enough, however, and did not mind the odd bump|It was more than I could bear}.
		*	{ not knows_about_wager } 'But, Monsieur, why are we travelling?'[] I asked.
		* 	{ knows_about_wager} I contemplated our strange adventure[]. Would it be possible?

#### Advanced: storing diverts as variables

A "divert" statement is actually a type of value in itself, and can be stored, altered, and diverted to. 

	VAR 	current_epilogue = -> everybody_dies 
	
	=== continue_or_quit ===
	Give up now, or keep trying to save your Kingdom?
	*  [Keep trying!] 	-> more_hopeless_introspection
	*  [Give up] 		-> epilogue


#### Advanced: Global variables are externally visible

Global variables can be accessed, and altered, from the runtime as well from the story, so provide the best way to communicate between the wider game and the story. 

The **ink** layer is often be a good place to store gameplay-variables; there's no save/load issues to consider, and the story itself can react to the current values. 



### Printing variables

The value of a variable can be printed as content using an inline syntax similar to sequences, and conditional text:

	VAR friendly_name_of_player = "Jackie"
	VAR age = 23

	"My name is Jean Passepartout, but my friend's call me {friendly_name_of_player}. I'm {age} years old."
	
This can be useful in debugging. For more complex printing based on logic and variables, see the section on functions.

## 2) Logic

Obviously, our global variables are not intended constant, so we need a syntax for altering them. 

Since by default, any text in an **ink** script is printed out directly to the screen, we use a markup symbol to indicate that a line of content is intended meant to be doing some numerical work, we use the `~` mark. 

The following statements all assign values to variables:

	
	=== set_some_variables ===
		~ knows_about_wager = true	
		~ x = (x * x) - (y * y) + c
		~ y = 2 * x * y

### Mathematics
	
**ink** supports the four basic mathematical operations (`+`, `-`, `*` and `/`), as well as `%` (or `mod`), which returns the remainder after integer division. 

If more complex operations are required, one can write functions (for recursive formulas and the like), or call out to external, game-code functions (for anything more advanced). 

#### Advanced: numerical types are implicit

Results of operations - in particular, for division - are typed based on the type of the input. So integer division returns integer, but floating point division returns floating point results. 

	~ x = 2 / 3
	~ y = 7 / 3
	~ z = 1.2 / 0.5
	
assigns x to be 0, y to be 2 and z to be 0.6.

## 3) Conditional blocks (if/else)

We've seen conditionals used to control options and story content; **ink** also provides an equivalent of the normal if/else if/else structure. 

### A simple 'if'

The if syntax takes its cue from the other conditionals used so far, with the `{`...`}` syntax indicating that something is being tested.

	{ x > 0:
		~ y = x - 1
	}

Else conditions can be provided:

	{ x > 0:
		~ y = x - 1:
	- else:
		~ y = x + 1;
	}
	
### Extended if/else if/else blocks

The above syntax is actually a specific case of a more general structure, something like a "switch" statement of another language:

	{
		- x > 0: 
			~ y = x - 1
		- else:	
			~ y = x + 1
	}

And using this form we can include 'else-if' conditions:

	{ 
		- x == 0:
			~ y = 0
		- x > 0:
			~ y = x - 1:
		- else:
			~ y = x + 1;
	}

(Note, as with everything else, the white-space is purely for readability and has no syntactic meaning.)
	
#### Example: context-relevant content

Note these tests don't have to be variable-based and can use read-counts, just as other conditionals can, and the following construction is quite frequent, as a way of saying "do some content which is relevant to the current game state":

	=== dream ===
		{
			- visited_snakes && not dream_about_snakes:
				~ fear++
				-> dream_about_snakes

			- visited_poland && not dream_about_polish_beer:
				~ fear--
				-> dream_about_polish_beer 

			- else:
				// breakfast-based dreams have no effect
				-> dream_about_marmalade
		}	

The syntax has the advantage of being easy to extend, and prioritise.

	

### Conditional blocks are not limited to logic

Conditional blocks can be used to control story content as well as logic:

	I stared at Monsieur Fogg.
	{ know_about_wager:
		<> "But surely you are not serious?" I demanded.
	- else:
		<> "But there must be a reason for this trip," I observed.
	}
	He said nothing in reply, merely considering his newspaper with as much thoroughness as entomologist considering his latest pinned addition.

You can even put options inside conditional blocks:

	{ door_open:
		* 	I strode out of the compartment[] and I fancied I heard my master quietly tutting to himself. 			-> go_outside 
	- else:
		*	I asked permission to leave[] and Monsieur Fogg looked surprised. 	-> open_door 
		* 	I stood and went to open the door[]. Monsieur Fogg seemed untroubled by this small rebellion. -> open_door
	}

...but note that the lack of weave-syntax and nesting in the above example isn't accidental: to avoid confusing the various kinds of nesting at work, you aren't allowed to include gather points inside conditional blocks.



## 4) Temporary Variables

### Temporary variables are for scratch calculations

Sometimes, a global variable is unwieldy. **ink** provides temporary variables for quick calculations of things.  

	=== near_north_pole ===
		~ temp number_of_warm_things = 0
		{ blanket:
			~ number_of_warm_things++
		}
		{ ear_muffs:
			~ number_of_warm_things++
		}
		{ gloves:
			~ number_of_warm_things++
		}
		{ number_of_warm_things > 2:
			Despite the snow, I felt incorrigibly snug.
		- else:
			That night I was colder than I have ever been.
		}

The value in a temporary variable is thrown away after the story leaves the knot in which it was defined. 

TODO: check this is actually true

### Knots and stitches can take parameters

A particularly useful form of temporary variable is a parameter. Any knot or stitch can be given a value as a parameter.
		
	*	[Accuse Hasting]
			-> accuse("Hastings")
	*	[Accuse Mrs Black]
			-> accuse("Claudia")
	*	[Accuse myself] 
			-> accuse("myself")
			
	=== accuse(who) ===
		"I accuse {who}!" Poirot declared.
		"Really?" Japp replied. "{who == "myself":You did it?|{who}?}"
		"And why not?" Poirot shot back. 	
		
	
	

#### Example: a recursive knot definition

Temporary variables are safe to use in recursion (unlike globals), so the following will work.

	-> add_one_to_one_hundred(0, 1)

	=== add_one_to_one_hundred(total, x) ===
		~ total = total + x
		{ x == 100:
			-> finished(total)
		- else:
			-> add_one_to_one_hundred(total, x + 1)
		}	
		
	=== finished(total) ===
		"The result is {total}!" you announce.
		Gauss stares at you in horror.
		-> DONE
	

(In fact, this kind of definition is useful enough that **ink** provides a special kind of knot, called, imaginatively enough, a `function`, which comes with certain restrictions and can return a value. See the section below.)


#### Advanced: sending divert targets as parameters

Knot/stitch addresses are a type of value, indicated by a `->` character, and can be stored and passed around. The following is therefore legal, and often useful:

	=== sleeping_in_hut ===
		You lie down and close your eyes.
		-> generic_sleep (-> waking_in_the_hut)

	===	 generic_sleep (-> waking)
		You sleep perchance to dream etc. etc.
		-> waking

	=== waking_in_the_hut
		You get back to your feet, ready to continue your journey.
		
...but note the `->` in the `generic_sleep` definition: that's the one case in **ink** where a parameter needs to be typed: because it's too easy to otherwise accidentally do the following:

	=== sleeping_in_hut ===
		You lie down and close your eyes.
		-> generic_sleep (waking_in_the_hut)
	
... which sends the read count of `waking_in_the_hut` into the sleeping knot, and then attempts to divert to it.





## 5) Functions

The use of parameters on knots means they are almost functions in the usual sense, but they lack one key concept - that of the call stack, and the use of return values. 

**ink** includes functions: they are knots, with the following limitations and features:

A function:
- cannot contain stitches
- cannot use diverts or offer choices
- can call other functions
- can include printed content 
- can return a value of any type
- can recurse safely

(Some of these may seem quite limiting, but for more story-oriented call-stack-style features, see the section of Tunnels.)

Return values are provided via the `~ return` statement. 

### Defining and calling functions

To define a function, simply declare a knot to be one:
	
	=== function say_yes_to_everything ===
		~ return true
	
	=== function lerp(a, b, k) ===
		~ return ((b - a) * k) + a

Functions are called by name, and with brackets, even if they have no parameters:

	~ x = lerp(2, 8, 0.3)
	
	*	{say_yes_to_everything()} 'Yes.' 

As in any other language, a function, once done, returns the flow to wherever it was called from - and despite not being allowed to divert the flow, functions can still call other functions.

	=== function say_no_to_nothing === 
		~ return say_yes_to_everything()

### Functions don't have to return anything

A function does not need to have a return value, and can simply do something that is worth packaging up:

	=== function harm(x) ===
		{ stamina < x:
			~ stamina = 0
		- else:
			~ stamina = stamina - x
		}

...though remember a function cannot divert, so while the above prevents a negative Stamina value, it won't kill a player who hits zero.

### Functions can be called inline

Functions can be called on `~` content lines, but can also be called during a piece of content. In this context, the return value, if there is one, is printed (as well as anything else the function wants to print.) If there is no return value, nothing is printed.

Content is, by default, 'glued in', so the following:

	Monsieur Fogg was looking {describe_health(health)}.
	
	=== function describe_health(x) ===
	{ 
	- x == 100:
		~ return "spritely"
	- x > 75:
		~ return "chipper"
	- x > 45:
		~ return "somewhat flagging"
	- else:
		~ return "despondent"
	}

produces:
	
	Monsieur Fogg was looking despondent.
	
#### Examples

For instance, you might include:

	=== function max(a,b) ===
		{ a < b:
			~ return b
		- else:
			~ return a
		}

	=== function exp(x, e) ===
		// returns x to the power e where e is an integer
		{ e <= 0:
			~ return 1
		- else:	
			~ return x * exp(x, e - 1)
		}

Then:

	The maximum of 2^5 and 3^3 is {max(exp(2,5), exp(3,3))}. 
		
produces:

	The maximum of 2^5 and 3^3 is 32.


#### Example: turning numbers into words

The following example is long, but appears in pretty much every inkle game to date:

	=== function print_num(x) ===
	{ 
		- x >= 1000:
			{print_num(x / 1000)} <> thousand { x mod 1000 > 0: {print_num(x mod 1000)} } 
		- x >= 100:
			{print_num(x / 100)} <> hundred { x mod 100 > 0: and {print_num(x mod 100)} } 
		- else:
			{ x >= 20:
				{ 
				- x / 10 == 2:
					twenty
				- x / 10 == 3:
					thirty
				- x / 10 == 4:
					forty
				- x / 10 == 5:
					fifty
				- x / 10 == 6:
					sixty
				- x / 10 == 7:
					seventy
				- x / 10 == 8:
					eighty
				- x / 10 == 9:
					ninety
				}
				{ x mod 10 > 0:
					<>-
				}
			}
			{ x < 10 || x > 20:
				{
				- x mod 10 == 1:
					<>one
				- x mod 10 == 2:
					<>two
				- x mod 10 == 3:
					<>three
				- x mod 10 == 4:
					<>four        
				- x mod 10 == 5:
					<>five
				- x mod 10 == 6:
					<>six
				- x mod 10 == 7:
					<>seven
				- x mod 10 == 8:
					<>eight
				- x mod 10 == 9:
					<>nine
				}
			- else:		
				{ 
				- x == 10:
					ten
				- x == 11:
					eleven       
				- x == 12:
					twelve
				- x == 13:
					thirteen
				- x == 14:
					fourteen
				- x == 15:
					fifteen
				- x == 16:
					sixteen      
				- x == 17:
					seventeen
				- x == 18:
					eighteen
				- x == 19:
					nineteen
				}
			}
	}
	
which enables us to write things like:

	~ price = 15
	
	I pulled out {print_num(price)} coins from my pocket and slowly counted them. 
	"Oh, never mind," the trader replied. "I'll take half." And she took {print_num(price / 2)}, and pushed the rest back over to me.



### Parameters can be passed by reference
	
Function parameters can also be passed 'by reference', meaning that the function can actually alter the the variable being passed in, instead of creating a temporary variable with that value. 

For instance, most **inkle** stories include the following:

	=== function alter(ref x, k) ===
		~ x = x + k
	
Lines such as:

	~ gold = gold + 7
	~ health = health - 4
	
then become:

	~ alter(gold, 7)
	~ alter(health, -4)
	
which are slightly easier to read, and (more usefully) can be done inline for maximum compactness.

	*	I ate a biscuit[] and felt refreshed. {alter(health, 2)}
	* 	I gave a biscuit to Monsieur Fogg[] and he wolfed it down most undecorously. {alter(foggs_health, 1)}
	-	<> Then we continued on our way.

Wrapping up simple operations in function can also provide a simple place to put debugging information, if required.




##  6) Constants


### Global Constants

Interactive stories often rely on state machines, tracking what stage some higher level process has reached. There are lots of ways to do this, but the most conveninent is to use constants.

Sometimes, it's convenient to define constants to be strings, so you can print them out, for gameplay or debugging purposes.

	CONST HASTINGS = "Hastings"
	CONST POIROT = "Poirot"
	CONST JAPP = "Japp"
	
	VAR current_chief_suspect = HASTINGS
	
	=== review_evidence ===
		{ found_japps_bloodied_glove:
			~ current_chief_suspect = POIROT
		}
		Current Suspect: {current_chief_suspect}
	
Sometimes giving them values is useful:

	CONST PI = 3.14
	CONST VALUE_OF_TEN_POUND_NOTE = 10
	
And sometimes the numbers are useful in other ways:

	CONST LOBBY = 1
	CONST STAIRCASE = 2
	CONST HALLWAY = 3
	
	CONST HELD_BY_AGENT = -1
	
	VAR secret_agent_location = LOBBY
	VAR suitcase_location = HALLWAY
	
	=== report_progress ===
	{  secret_agent_location = suitcase_location:
		The secret agent grabs the suitcase!
		~ suitcase_location = HELD_BY_AGENT  
		
	-  secret_agent_location < suitcase_location:
		The secret agent moves forward.
		~ secret_agent_location++
	}
	
Constants are simply a way to allow you to give story states easy-to-understand names.  

## 7) Advanced: Game-side logic 

You can also provide additional functions via C# which the ink calls out to. There are two ways to provide delegate callbacks in the ink engine:

### Function bindings

You can bind an external script to an **ink** function, to then be used in the ink script. 

To do this, first declare an external function using something like this at the top of one of your ink files, in global scope:

	EXTERNAL multiply(x,y)

Then before calling your story, set up the bound function:

    story.BindExternalFunction ("multiply", (int arg1, float arg2) => {
        return arg1 * arg2;
    });  

There are convenience overloads for BindExternalFunction, for arity <= 3, for both Funcs and Actions (as well as a general purpose BindExternalFunctionGeneral that takes an object array for > 3 parameters.)

You can then call that function within the ink:

	3 times 4 is {multiply(3, 4)}.

The types you can use are int, float, bool (automatically converted from ink’s ints) and string.

### Variable observers 

You can also passively set the game to watch for changes in the state of any **ink** variable, by creating a variable observer: 

    story.ObserveVariable ("health", (string varName, object newValue) => {
        SetHealthInUI((int)newValue);
    });  

The reason we pass varName in is so that you can have a single observer function that observes multiple variables if you like.



# Part 4: Advanced Flow Control



## ) Tunnels

## ) Threads








