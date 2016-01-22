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

### Comments 

By default, all text in your file will appear in the output content, unless specially marked up. The simplest mark-up is a comment. 

**ink** supports two kinds of comment. There's the kind used for someone reading the code, which the compiler ignores:

	"What do you make of this?" she asked. 
	
	// Something unprintable...
	
	"I couldn't possibly comment," I replied.
	
	/*
		... or an unlimited block of text, even
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
	= in_secont_class
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

You can use several logical tests on an option; if you do, they must all be true for the option to appear.

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

Sequences (the default):

	{I bought a coffee with my five-pound note.|I bought a second coffee for my friend.|I didn't have enough money to buy any more coffee.}
			
Cycles (marked with a `&`): 
	
	It was {&Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday} today.
	
Once-only lists (marked with a `!`):

	He told me a joke. {!I laughed politely.|I smiled.|I grimaced.|I promised myself to not react again.}
	
Shuffled lists (marked with a `~`):
	
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




:: BIG TODO -- explain the nesting of content




## 3) Tracking a Weave

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






# Variables and Logic


# Functions



### Comments

(Here maybe?)

### Labels

	This is the start
	
	* The first choice 		-> choiceResult1
	* The second choice 	-> choiceResult2
	
	- (choiceResult1) This is the result of the first choice.
		-> ending
	
	- (choiceResult2) This is the result of the second choice.
	// Automatically continues down to the end.
	
	- (ending) This is the end.
		-> DONE
		
### By default, ink has a forward flow

	This is the start
	
	* The first choice
	* The second choice 	-> ending
	
	- 	You chose the first option.
		Not what I would've done.
	-	(ending) This is the ending.
		-> DONE
		
(Explain `DONE`.)
		
### Knots

	-> start

	=== start ===
	This is the start.
	* The first choice		-> first_section
	* The second choice 	-> second_section

	=== first_section ===
	This is the first section.
	* Continue to second 	-> second_section
	* We're done 				-> last_section

	=== second_section ===
	This is the second section.
	* Go to first section 	-> first_section
	* Go to the end			-> last_section

	=== last_section ===
	This is the end!
	-> DONE

(Explanation should talk about knots in general, as well as the "once only" option default.)

### Inline conditionals: Have you seen this?

... (and adding conditions to options)


### Sequences

...


### Multi-line conditionals and sequences

...


### Knots v.s. stitches

...


### Weave in depth

#### Choice [] notation

#### Nesting, gathering

### Variables

... (and CONST)

### Simple expressions

...

### Functions

...

### Tunnels

...

### Default choices

...

### `TODO` statements

...

### `CHOICE_COUNT` and `TURNS_SINCE`

### Advanced constructs

#### Parameter passing

#### Variable diverts

(requirement of passing `-c` to the compiler)

#### Threads


