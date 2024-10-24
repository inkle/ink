# 使用 Ink 进行写作
<details>
  <summary>内容目录</summary>

- [使用 Ink 进行写作](#使用-ink-进行写作)
	- [声明](#声明)
	- [介绍](#介绍)
- [第 1 部分：基础｜Part One: The Basics](#第-1-部分基础part-one-the-basics)
	- [1) 内容｜Content](#1-内容content)
		- [最简单的 ink 脚本｜Hello, World!](#最简单的-ink-脚本hello-world)
		- [注释｜Comments](#注释comments)
		- [标签｜Tags](#标签tags)
	- [2) 选择｜Choices](#2-选择choices)
		- [不输出选择文本｜Suppressing choice text](#不输出选择文本suppressing-choice-text)
			- [进阶：混合选项与输出文本｜Advanced: mixing choice and output text](#进阶混合选项与输出文本advanced-mixing-choice-and-output-text)
		- [多样化的选择｜Multiple Choices](#多样化的选择multiple-choices)
	- [3) 结点｜Knots](#3-结点knots)
		- [内容的片段被称为结点｜Pieces of content are called knots](#内容的片段被称为结点pieces-of-content-are-called-knots)
		- [撰写一个结点｜Writing a knot](#撰写一个结点writing-a-knot)
			- [进阶：一个结点更复杂的“你好世界”｜Advanced: a knottier "hello world"](#进阶一个结点更复杂的你好世界advanced-a-knottier-hello-world)
	- [4) 分道｜Diverts](#4-分道diverts)
		- [从结点分道到结点｜Knots divert to knots](#从结点分道到结点knots-divert-to-knots)
			- [分道是不可见的｜Diverts are invisible](#分道是不可见的diverts-are-invisible)
			- [胶合｜Glue](#胶合glue)
	- [5) 为故事流程进行分支｜Branching The Flow](#5-为故事流程进行分支branching-the-flow)
		- [基本分支｜Basic branching](#基本分支basic-branching)
		- [分支与合并｜Branching and joining](#分支与合并branching-and-joining)
		- [故事流｜The story flow](#故事流the-story-flow)
			- [进阶：循环｜Advanced: Loops](#进阶循环advanced-loops)
	- [6) 包含和接缝｜Includes and Stitches](#6-包含和接缝includes-and-stitches)
		- [结点可以是次级分道｜Knots can be subdivided](#结点可以是次级分道knots-can-be-subdivided)
		- [接缝需要有独一无二的名称｜Stitches have unique names](#接缝需要有独一无二的名称stitches-have-unique-names)
		- [默认为第一个接缝｜The first stitch is the default](#默认为第一个接缝the-first-stitch-is-the-default)
		- [内部分道｜Local diverts](#内部分道local-diverts)
		- [脚本文件可组合｜Script files can be combined](#脚本文件可组合script-files-can-be-combined)
	- [7) 可变选项｜Varying Choices](#7-可变选项varying-choices)
		- [选项只能被使用一次｜Choices can only be used once](#选项只能被使用一次choices-can-only-be-used-once)
			- [后备选项｜Fallback choices](#后备选项fallback-choices)
			- [后备选项示例｜Example of a fallback choice](#后备选项示例example-of-a-fallback-choice)
		- [粘滞选项｜Sticky choices](#粘滞选项sticky-choices)
		- [条件选项｜Conditional Choices](#条件选项conditional-choices)
			- [进阶：多重条件｜Advanced: multiple conditions](#进阶多重条件advanced-multiple-conditions)
			- [逻辑运算符：AND 和 OR｜Logical operators: AND and OR](#逻辑运算符and-和-orlogical-operators-and-and-or)
			- [进阶：结点与接缝的实际阅读次数｜Advanced: knot/stitch labels are actually read counts](#进阶结点与接缝的实际阅读次数advanced-knotstitch-labels-are-actually-read-counts)
			- [进阶：更多逻辑｜Advanced: more logic](#进阶更多逻辑advanced-more-logic)
	- [8) 可变文本｜Variable Text](#8-可变文本variable-text)
		- [文本是可以变更的｜Text can vary](#文本是可以变更的text-can-vary)
		- [序列、循环以及其他类型的替文｜Sequences, cycles and other alternatives](#序列循环以及其他类型的替文sequences-cycles-and-other-alternatives)
			- [替文的类型｜Types of alternatives](#替文的类型types-of-alternatives)
			- [替文的特点｜Features of Alternatives](#替文的特点features-of-alternatives)
			- [示例｜Examples](#示例examples)
			- [另行参见：多行替文｜Sneak Preview: Multiline alternatives](#另行参见多行替文sneak-preview-multiline-alternatives)
		- [条件文本｜Conditional Text](#条件文本conditional-text)
	- [9) 游戏查询和函数｜Game Queries and Functions](#9-游戏查询和函数game-queries-and-functions)
		- [选项计数函数｜CHOICE\_COUNT()](#选项计数函数choice_count)
		- [总回合计数函数｜TURNS()](#总回合计数函数turns)
		- [分道计数函数｜TURNS\_SINCE(-\> knot)](#分道计数函数turns_since--knot)
			- [功能预览：在功能中使用分道计数函数｜Sneak preview: using TURNS\_SINCE in a function](#功能预览在功能中使用分道计数函数sneak-preview-using-turns_since-in-a-function)
		- [种子随机函数｜SEED\_RANDOM()](#种子随机函数seed_random)
			- [进阶：更多查询｜Advanced: more queries](#进阶更多查询advanced-more-queries)
- [第 2 部分：织体｜Part 2: Weave](#第-2-部分织体part-2-weave)
	- [1) 收束｜Gathers](#1-收束gathers)
		- [收束点将故事流收束到一起｜Gather points gather the flow back together](#收束点将故事流收束到一起gather-points-gather-the-flow-back-together)
		- [内容链的选项和收束｜Options and gathers form chains of content](#内容链的选项和收束options-and-gathers-form-chains-of-content)
			- [织体的理念（方法论）｜The weave philosophy](#织体的理念方法论the-weave-philosophy)
	- [2) 嵌套故事流｜Nested Flow](#2-嵌套故事流nested-flow)
		- [选项可以嵌套｜Options can be nested](#选项可以嵌套options-can-be-nested)
		- [收束点也可以嵌套｜Gather points can be nested too](#收束点也可以嵌套gather-points-can-be-nested-too)
			- [进阶：收束这个操作做了什么｜Advanced: What gathers do](#进阶收束这个操作做了什么advanced-what-gathers-do)
		- [你可以根据你自己的需要设置多层嵌套｜You can nest as many levels are you like](#你可以根据你自己的需要设置多层嵌套you-can-nest-as-many-levels-are-you-like)
		- [示例：用嵌套节点写的对话｜Example: a conversation with nested nodes](#示例用嵌套节点写的对话example-a-conversation-with-nested-nodes)
	- [3) 追踪织体｜Tracking a Weave](#3-追踪织体tracking-a-weave)
		- [织体是庞大且没有地址索引的｜Weaves are largely unaddressed](#织体是庞大且没有地址索引的weaves-are-largely-unaddressed)
		- [收束和选项也可以打标签｜Gathers and options can be labelled](#收束和选项也可以打标签gathers-and-options-can-be-labelled)
		- [界限｜Scope](#界限scope)
			- [进阶：所有的选项都可以打标签｜Advanced: all options can be labelled](#进阶所有的选项都可以打标签advanced-all-options-can-be-labelled)
			- [进阶：在织体里循环｜Advanced: Loops in a weave](#进阶在织体里循环advanced-loops-in-a-weave)
			- [进阶：分道指向到选项｜Advanced: diverting to options](#进阶分道指向到选项advanced-diverting-to-options)
			- [进阶：在一个选项后直接收束｜Advanced: Gathers directly after an option](#进阶在一个选项后直接收束advanced-gathers-directly-after-an-option)
- [第 3 部分：变量和逻辑｜Part 3: Variables and Logic](#第-3-部分变量和逻辑part-3-variables-and-logic)
	- [1) 全局变量｜Global Variables](#1-全局变量global-variables)
		- [定义全局变量｜Defining Global Variables](#定义全局变量defining-global-variables)
		- [使用全局变量｜Using Global Variables](#使用全局变量using-global-variables)
			- [进阶：将分道存储为变量｜Advanced: storing diverts as variables](#进阶将分道存储为变量advanced-storing-diverts-as-variables)
			- [进阶：全局变量是对外可见的｜Advanced: Global variables are externally visible](#进阶全局变量是对外可见的advanced-global-variables-are-externally-visible)
		- [打印输出变量｜Printing variables](#打印输出变量printing-variables)
		- [叠加态字符串｜Evaluating strings](#叠加态字符串evaluating-strings)
	- [2) 逻辑｜Logic](#2-逻辑logic)
		- [数学｜Mathematics](#数学mathematics)
			- [指定范围的随机整数函数｜RANDOM(min, max)](#指定范围的随机整数函数randommin-max)
			- [进阶：数值类型是隐藏但存在的｜Advanced: numerical types are implicit](#进阶数值类型是隐藏但存在的advanced-numerical-types-are-implicit)
			- [进阶：自定义变量类型｜Advanced: INT(), FLOOR() and FLOAT()](#进阶自定义变量类型advanced-int-floor-and-float)
		- [字符串查询｜String queries](#字符串查询string-queries)
	- [3) 条件代码块（如果，否则）｜Conditional blocks (if/else)](#3-条件代码块如果否则conditional-blocks-ifelse)
		- [一个简单的“如果”｜A simple 'if'](#一个简单的如果a-simple-if)
		- [扩展判断条件代码块（如果、或者、否则）｜Extended if/else if/else blocks](#扩展判断条件代码块如果或者否则extended-ifelse-ifelse-blocks)
		- [开关代码块｜Switch blocks](#开关代码块switch-blocks)
			- [Example: context-relevant content](#example-context-relevant-content)
		- [Conditional blocks are not limited to logic](#conditional-blocks-are-not-limited-to-logic)
		- [多行代码块｜Multiline blocks](#多行代码块multiline-blocks)
			- [Advanced: modified shuffles](#advanced-modified-shuffles)
	- [4) Temporary Variables](#4-temporary-variables)
		- [Temporary variables are for scratch calculations](#temporary-variables-are-for-scratch-calculations)
		- [Knots and stitches can take parameters](#knots-and-stitches-can-take-parameters)
			- [Example: a recursive knot definition](#example-a-recursive-knot-definition)
			- [Advanced: sending divert targets as parameters](#advanced-sending-divert-targets-as-parameters)
	- [5) 函数｜Functions](#5-函数functions)
		- [Defining and calling functions](#defining-and-calling-functions)
		- [Functions don't have to return anything](#functions-dont-have-to-return-anything)
		- [Functions can be called inline](#functions-can-be-called-inline)
			- [Examples](#examples)
			- [Example: turning numbers into words](#example-turning-numbers-into-words)
		- [Parameters can be passed by reference](#parameters-can-be-passed-by-reference)
	- [6) 常量｜Constants](#6-常量constants)
		- [Global Constants](#global-constants)
	- [7) Advanced: Game-side logic](#7-advanced-game-side-logic)
- [第 4 部分：进阶流程控制｜Part 4: Advanced Flow Control](#第-4-部分进阶流程控制part-4-advanced-flow-control)
	- [1) Tunnels](#1-tunnels)
		- [Tunnels run sub-stories](#tunnels-run-sub-stories)
			- [Advanced: Tunnels can return elsewhere](#advanced-tunnels-can-return-elsewhere)
			- [Advanced: Tunnels use a call-stack](#advanced-tunnels-use-a-call-stack)
	- [2) Threads](#2-threads)
		- [Threads join multiple sections together](#threads-join-multiple-sections-together)
		- [Uses of threads](#uses-of-threads)
		- [When does a side-thread end?](#when-does-a-side-thread-end)
		- [Using `-> DONE`](#using---done)
			- [Example: adding the same choice to several places](#example-adding-the-same-choice-to-several-places)
			- [Example: organisation of wide choice points](#example-organisation-of-wide-choice-points)
- [第 5 部分：进阶状态追踪｜Part 5: Advanced State Tracking](#第-5-部分进阶状态追踪part-5-advanced-state-tracking)
			- [Note: New feature alert!](#note-new-feature-alert)
	- [1) Basic Lists](#1-basic-lists)
	- [2) Reusing Lists](#2-reusing-lists)
		- [States can be used repeatedly](#states-can-be-used-repeatedly)
			- [List values can share names](#list-values-can-share-names)
			- [Advanced: a LIST is actually a variable](#advanced-a-list-is-actually-a-variable)
	- [3) List Values](#3-list-values)
		- [Converting values to numbers](#converting-values-to-numbers)
		- [Converting numbers to values](#converting-numbers-to-values)
		- [Advanced: defining your own numerical values](#advanced-defining-your-own-numerical-values)
	- [4) Multivalued Lists](#4-multivalued-lists)
		- [Lists are boolean sets](#lists-are-boolean-sets)
			- [Assiging multiple values](#assiging-multiple-values)
			- [Adding and removing entries](#adding-and-removing-entries)
		- [Basic Queries](#basic-queries)
			- [Testing for emptiness](#testing-for-emptiness)
			- [Testing for exact equality](#testing-for-exact-equality)
			- [Testing for containment](#testing-for-containment)
			- [Warning: no lists contain the empty list](#warning-no-lists-contain-the-empty-list)
			- [Example: basic knowledge tracking](#example-basic-knowledge-tracking)
			- [Example: a doctor's surgery](#example-a-doctors-surgery)
			- [Advanced: nicer list printing](#advanced-nicer-list-printing)
			- [Lists don't need to have multiple entries](#lists-dont-need-to-have-multiple-entries)
		- [The "full" list](#the-full-list)
			- [Advanced: "refreshing" a list's type](#advanced-refreshing-a-lists-type)
			- [Advanced: a portion of the "full" list](#advanced-a-portion-of-the-full-list)
		- [Example: Tower of Hanoi](#example-tower-of-hanoi)
	- [5) Advanced List Operations](#5-advanced-list-operations)
		- [Comparing lists](#comparing-lists)
			- ["Distinctly bigger than"](#distinctly-bigger-than)
			- ["Definitely never smaller than"](#definitely-never-smaller-than)
			- [Health warning!](#health-warning)
		- [Inverting lists](#inverting-lists)
			- [Footnote](#footnote)
		- [Intersecting lists](#intersecting-lists)
	- [6) Multi-list Lists](#6-multi-list-lists)
		- [Lists to track objects](#lists-to-track-objects)
		- [Lists to track multiple states](#lists-to-track-multiple-states)
			- [How does this affect queries?](#how-does-this-affect-queries)
	- [7) Long example: crime scene](#7-long-example-crime-scene)
	- [8) Summary](#8-summary)
		- [Flags](#flags)
		- [State machines](#state-machines)
		- [Properties](#properties)
- [第 6 部分：标识符中的国际字符支持｜Part 6: International character support in identifiers](#第-6-部分标识符中的国际字符支持part-6-international-character-support-in-identifiers)
		- [Supported Identifier Characters](#supported-identifier-characters)

</details>

## 声明

简体中文教程是从英文原文翻译而来。可能会存在版本滞后性，故入与英文版有出入，请与英文版为准。

部分不重要的段落由 ChatGPT 或 DeepL 组合翻译。

重要的部分完全由人工翻译。

译者：王洛木 (Nomo_Wang@outlook.com)，如果您有关于翻译的问题要提，且不想要占用 Discussion 资源，可以发邮件给译者。

翻译时的 Ink 版本：1.2.0

翻译最后更新时间：2024年10月2日

翻译仍在更新。

## 介绍

**Ink** 是一种脚本语言，围绕着用流程标记纯文本以生成交互式脚本的理念而构建。

它的基本功能是编写“选择你自己的”故事或分支对话树。但它真正的优势在于能够编写包含大量选项和复杂流程重组的对话。

**Ink** 提供了一些功能，使非专业作家能够频繁进行分支，并以轻重缓急的方式演绎这些分支的后果，毫不费力。

脚本力求简洁、逻辑清晰，因此可以通过“用眼睛”测试分支对话。在可能的情况下，流程以声明的方式进行描述。

它的设计也考虑到了重写的问题，因此编辑流程应该快速便捷。

# 第 1 部分：基础｜Part One: The Basics

## 1) 内容｜Content

### 最简单的 ink 脚本｜Hello, World!

最基础的 ink 脚本就是在 .ink 文件中输入文本就行了。

	你好，世界！

在运行时，这会直接输出文本，然后就停止了。

另起一行就可以新建段落。就像这个脚本：

	你好，世界！
	你好？
	哈喽，你在那里么？

输出结果与上面的脚本是一样的，所见即所得。

### 注释｜Comments

默认情况下，文件中的所有文本都将出现在输出内容中，除非特别标注。

最简单的标记就是注释。**Ink** 支持两种注释。一种是供阅读代码的人使用的注释，编译器会忽略它：

	“你怎么看？”她问到。

	// 这行以双斜杠开始的内容不会被打印输出……（译者注：也就是单行注释，可以直接跟在某行文本之后。）

	“我不可能发表评论。”我回复道。

	/*
		……夹在本段落上下的两个标记符号之间部分可以写无限长的注释，包括换行。
	*/


另外还有一种是用来提醒作者需要做什么用的，编译器会在编译时打印出来：

（译者注：TODO 后面的冒号要使用半角冒号，也就是英文冒号，然后后面打个空格）

	TODO: 这个段落应该写……

### 标签｜Tags

引擎运行时，游戏中的文本内容会以“原样”显示。但有时在一行内容上标注额外信息，告诉游戏如何处理该内容也是很有用的。

**Ink** 提供了一个简单的系统，可以用标签标记内容行。

	一行普通的游戏文本。# 颜色-蓝色
	A line of normal game-text. # colour it blue

这些标签不会显示在主文本流中，但可以被游戏读取，并根据需要使用。请参阅 [《运行您的 Ink》](RunningYourInk_Simplified-Chinese.md#marking-up-your-ink-content-with-tags) 获取更多信息。

## 2) 选择｜Choices

玩家可通过文本选项来进行输入。文本选项用 "*"字符表示。

如果没有给出其他流程指示，一旦做出选择，就会根据选项进入下一行文本。

	你好世界！
	*	你也好！
		见到你真是太好了！

上面的这个脚本会在游戏中这样输出：

	你好世界！
	1: 你也好！

	> 1
	你也好！
	见到你真是太好了！

在默认情况下，选项的文本会在输出中再显示一次。

### 不输出选择文本｜Suppressing choice text

有些游戏将选择文本与结果分开。在 **Ink** 中，如果文本选项的文本写在方括号中，则该文本不会被打印到响应中。


	你好世界！
	*	[你也好！]
		见到你真是太好了！

输出结果：

	你好世界！
	1: 你也好！

	> 1
	见到你真是太好了！

#### 进阶：混合选项与输出文本｜Advanced: mixing choice and output text

可以使用方括号来分割选项文本在输出中的范围：
*	方括号 前面的 内容会在选项和输出中都发引出来
*	方括号 内部的 部分只会显示在选项内
*	方括号 后面的 则只会打印在输出的内容里。

这可以为故事线提供不同的结尾方式。

比如下面这个脚本：

	你好世界！
	*	你好
	*	你[也好！]也一样！
		见到你真是太好了！

输出的结果是：

	你好世界！
	1: 你也好！
	> 1
	你也一样！
	见到你真是太好了！

这在编写对话选择的时候很实用，下面是一个脚本示例：

	“你说什么？”我的老大问我。
	*	“我有点累了[。”]，老大……”我重复着。
		“这样啊。”他回应道：“那休息一下吧。”

这将输出：

	“你说什么？”我的老大问我。
	1: “我有点累了。”
	> 1
	“我有点累了，老大……”我重复着。
	“这样啊。”他回应道：“那休息一下吧。”

### 多样化的选择｜Multiple Choices

为了让选择更真实，我们需要提供一些替代选项。只需列出备选方案即可：

	“你说什么？”我的老大问我。
	*	“我有点累了[。”]，老大……”我重复着。
		“这样啊。”他回应道：“那休息一下吧。”
    *	“没事的老大！”[]我说。
		“很好，那继续吧。”
	*	“我说，这次的冒险真的很可怕[……”]，我真的不想再继续了……
		“啊……别这样。”他安慰着我：“看起来你现在有些累了。明天，事情一定会有所好转的。”

上面这段脚本的游戏结果如下：

	“你说什么？”我的老大问我。

	1: “我有点累了。”
    2: “没事的老大！”
	3: “我说，这次的冒险真的很可怕……”

	> 3
	“我说，这次的冒险真的很可怕，我真的不想再继续了……
	“啊……别这样。”他安慰着我：“看起来你现在有些累了。明天，事情一定会有所好转的。”

上述语法足以编写一组选项。在真正的游戏中，我们需要根据玩家的选择将流程从一个点移动到另一个点。为此，我们需要引入更多的结构。

## 3) 结点｜Knots

### 内容的片段被称为结点｜Pieces of content are called knots

为了让游戏能够分支，我们需要用名称来标记内容的不同部分（就像老式游戏本中的 “第 18 段”之类的）。

这些 部分 (Section) 就被称为“结点”（Knots），是 Ink 内容的基本结构单元。

### 撰写一个结点｜Writing a knot

结点的起点用两个或以上的等号表示，如下几行均为符合规范的结点起点（每行一个例子）：

	=== top_knot ===
	=== top_knot
	==top_knot

需要注意的是：
*	区分大小写。
*	末尾的等号是可选项。
*	无法使用连字符 "-"。还有其他一些特殊的标点符号也无法使用。在出现不可使用的标点符号时，编辑器会报错提醒。
*	可以使用数字开头，但是不可以使用纯数字。
*	中间不可以有空格，不然一来结点名会被截取到空格前，二来跳转箭头也无法指向对应结点。
*	可以使用英文字符以外的字符，但是不推荐。一是因为截止至翻译为止，使用了英文字符以外的字符不会被 Inky 用蓝色标记；而是因为很有可能部分字符会导致结点失效。所以不建议使用英文与数字以外的字符。

等号这一行就是该结点的标题（当然等号和空格不会算在内）。在这下面的内容都在这个结点内。

	=== back_in_london ===
	
	我们于晚上 9 点 45 分准时到达伦敦。

#### 进阶：一个结点更复杂的“你好世界”｜Advanced: a knottier "hello world"

在启动 Ink 文件时，结点以外的内容会自动运行。但节点不会。因此，如果你开始使用节点来管理内容，就需要告诉游戏该去哪里。我们可以使用分道箭头 `->`来做到这一点，下一部分将对此进行详细介绍。

这是一个简单的结点跳转脚本：

	-> top_knot

	=== top_knot ===
	你好世界！

不过，**Ink** 不喜欢“开放式”结局（这里说的意思是：需要有一个标记来告诉 Ink 结点结束了），当它认为出现这种情况时，会在编译和或运行时发出警告。上面的脚本就会在编译时发出这样的警告：

	WARNING: Apparent loose end exists where the flow runs out. Do you need a '-> END' statement, choice or divert? on line 3 of tests/test.ink

	警告：显然在流程结束的地方少写了点什么。是否需要 "->END" 语句、选项或跳转？此问题发生在 tests/test.ink 的第 3 行。

（译者注：程序内的文本已经在翻译了……所以这里这里姑且先保留原本的报错，以便对照查询。下同。）

在运行时的报错则是这样的：

	Runtime error in tests/test.ink line 3: ran out of content. Do you need a '-> DONE' or '-> END'?

	在运行到 tests/test.ink 的第 3 行时出现错误：没有更多内容了。或许您需要 "-> DONE" 或者 "-> END"？

下面的这个脚本则不会在游玩或者编译时出现问题：

	=== top_knot ===
	你好世界！
	-> END

`-> END` 是一个同时给写作者和编译器用的标记，表示 "故事流程现在应该停止"。

## 4) 分道｜Diverts

### 从结点分道到结点｜Knots divert to knots

您可以使用“分道箭头”`->`来让故事从一个结点分道到另一个结。无需任何用户输入，分道会立即发生。

	=== back_in_london ===

	我们于晚上 9 点 45 分准时到达伦敦。
	-> hurry_home

	=== hurry_home ===
	我们以最快的速度赶回萨维尔街。

#### 分道是不可见的｜Diverts are invisible

分道甚至可以在句子中可以无缝衔接：

	=== hurry_home ===
	我们赶回萨维尔街， -> as_fast_as_we_could

	=== as_fast_as_we_could ===
	用我们最快的速度。

这将会输出：

	我们赶回萨维尔街，用我们最快的速度。

#### 胶合｜Glue

脚本在另起一行的时候默认会有一个不可见的换行符。但是在某些情况下，您可能不希望您的文本换行，但是在脚本里又需要换行来写。那么这时就可以使用 `<>` 或 "glue"来实现。

	=== hurry_home ===
	我们赶回<>
	-> to_savile_row

	=== to_savile_row ===
	萨维尔街，
	-> as_fast_as_we_could

	=== as_fast_as_we_could ===
	<>用我们最快的速度。

输出是这样的：

	我们赶回萨维尔街，用我们最快的速度。

您最好不要使用多个胶合：多个相邻的胶合语法不会产生额外的效果。（并且也没有办法“屏蔽”胶合；一旦一行被胶合起来，就无法再拆分开。）

## 5) 为故事流程进行分支｜Branching The Flow

### 基本分支｜Basic branching

将结点、选项和分道结合起来，就形成了的自助游戏 (choose-your-own game) 的基本结构。

	=== paragraph_1 ===
	你站在安纳兰德城墙边，手持长剑。
	* [打开大门] -> paragraph_2
	* [砸了那个大门] -> paragraph_3
	* [打道回府] -> paragraph_4

	=== paragraph_2 ===
	你打开了大门，踏上了门里那条小路。

	...

### 分支与合并｜Branching and joining

利用分道，作者可以将故事流分支，然后再次合并起来，且不会让玩家看到流程已经重新连接。

	=== back_in_london ===

	我们于晚上 9 点 45 分准时到达伦敦。

	*	“要没时间了！”我大喊。
		-> hurry_outside

	*	"老大，时间还够呢！"[] 我说。
		老大用力拍了拍我的头，把我拽出了门。
		-> dragged_outside

	*	[我们立刻向家里赶去]， -> hurry_outside

	

	=== hurry_outside ===

	我们赶回萨维尔街，-> as_fast_as_we_could


	=== dragged_outside ===
	他坚持要我们赶回萨维尔街的家，
	-> as_fast_as_we_could


	=== as_fast_as_we_could ===
	<>用我们最快的速度。


### 故事流｜The story flow

结点和分道相结合就形成了游戏的基本故事流程。但这种流程是“扁平”的——既没有调用堆栈，分道也不会从某处“折返”。

在大多数水墨脚本中，故事流程从顶部开始，像意大利面条一样乱蹦乱跳，最终，希望能到达"->结束"。
在大部分 Ink 脚本中，故事流从顶部开始，然后就像是一盘意面一样，最终到达一个 `-> END`。

这种松散的结构方式可以让作者轻松的续写、分支或合并，也不必担心他们在写作过程中就要想好要创建的结构。而在创建新的分支或分流时，既然不需要任何模板，也不需要跟踪任何状态。

#### 进阶：循环｜Advanced: Loops

您可以使用分道来创建循环内容，**Ink** 有多种可以利用这一点的功能，包括使内容自行变化的方法，以及控制选项选择频率的方法。

更多信息请参阅这些章节：
*	[可变文本｜Variable Text](#8-可变文本variable-text)
*	[条件选项｜Conditional Choices](#条件选项conditional-choices)

另外，下列内容符合规范但是并不好：

	=== round ===
	然后
	-> round

（译者注：上面这是一个无限循环。）

## 6) 包含和接缝｜Includes and Stitches

### 结点可以是次级分道｜Knots can be subdivided

随着故事越来越长，如果没有一些额外的结构，就会变得越来越难以组织。

结点可以包括一种被称为“接缝” (Stitches) 的子部分。这些接缝使用一个等号标记。

	=== the_orient_express ===
	= in_first_class
		...
	= in_third_class
		...
	= in_the_guards_van
		...
	= missed_the_train
		...

例如，可以结点来指定一个场景，然后用接缝来表示场景中的事件。

### 接缝需要有独一无二的名称｜Stitches have unique names

接缝可以使用它的“地址”(Address) 来进行分道。

	*	[乘坐三等座]
		-> the_orient_express.in_third_class

	*	[乘坐警卫间]
		-> the_orient_express.in_the_guards_van

### 默认为第一个接缝｜The first stitch is the default

转到包含接缝的结点时，将转到结点中的第一个接缝。所以：

	*	[乘坐一等座]
		"先生，一等座还有空位么？"
		-> The_orient_express

与下面这个脚本是一样的：

	*	[乘坐一等座]
		"先生，一等座还有空位么？"
		-> the_orient_express.in_first_class

（……除非我们在结点内移动了接缝的顺序！）


您也可以在结点内的那些接缝上方加入任何内容。然而你需要记得为接缝进行分道。因为引擎在有接缝前有内容的时候*不会*自动进入第一个接缝，举个例子：

	=== the_orient_express ===

	已经上了火车了，但是坐到哪里呢？
	*	[一等座] -> in_first_class
	*	[二等座] -> in_second_class

	= in_first_class
		...
	= in_second_class
		...


### 内部分道｜Local diverts

如果你要在结点内进行分道，那么您不需要使用完整的地址就可以进行内部接缝。

	-> the_orient_express

	=== the_orient_express ===
	= in_first_class
		我安顿好了我的老大。
		*	[去三等座]
			-> in_third_class

	= in_third_class
		我把我自己安排在三等座。

这意味着接缝和结点不能共用名称，但是如果相同名称的接缝分别属于不同的结点则可以使用。(因此，"东方快车”和“蒙古号”这两个结点里面都可以包含叫“一等座”的接缝。）

如果使用了模棱两可的名称，编译器会发出警告。

### 脚本文件可组合｜Script files can be combined

您还可以可以把您的脚本内容拆分到多个文件中，只需要使用“包含声明”`INCLUDE` 就可以了。

	INCLUDE newspaper.ink
	INCLUDE cities/vienna.ink
	INCLUDE journeys/orient_express.ink

包含语句应始终放在文件头，而不是在结点内。

把文件分割开不会影响到分道跳转。（换句话说，只要你在文件头声明过了要用到的文件，那么就可以进行跨文件分道。）

## 7) 可变选项｜Varying Choices

### 选项只能被使用一次｜Choices can only be used once

默认情况下，游戏中的每个选择都只能被选择一次。如果你的故事中没有循环，你就不会注意到这种行为。但如果你使用了循环，你很快就会发现你的选项消失了……

	=== find_help ===
		你在人群中拼命地寻找着友善的面孔。
		*	那个戴帽子的女人[？]粗暴地把你推到了一边。-> find_help
		*	那个拿公文包的男人[？]一脸嫌弃地看着你然后走开了。-> find_help

输出结果：

	你在人群中拼命地寻找着友善的面孔。
	1: 那个戴帽子的女人？
	2: 那个拿公文包的男人？

	> 1
	那个戴帽子的女人粗暴地把你推到了一边。
	你在人群中拼命地寻找着友善的面孔。

	1: 那个拿公文包的男人？

	>

……然后你就发现什么选项都没剩下了。

#### 后备选项｜Fallback choices

上面的示例到此为止，因为下一个选择会导致在运行时出现“内容不足”的错误。

	> 1
	那个拿公文包的男人一脸嫌弃地看着你然后走开了。
	你在人群中拼命地寻找着友善的面孔。

	Runtime error in tests/test.ink line 6: ran out of content. Do you need a '-> DONE' or '-> END'?

	在运行到 tests/test.ink 的第 6 行时出现错误：内容不足。您需要 "-> DONE" 还是 "-> END"？

我们可以用“后备选项”来解决这个问题。后备选项并不会显示给玩家，而是当玩家没有别的选项的时候就会自动选择它。

后备选项写起来很简单，就是“没有选择文本的选项”：

	*	-> out_of_options

此外，我们还可以稍微滥用一下这个语法，使用“空接箭头”来做一个带有内容的默认的选择：

	* 	->
		穆德始终无法解释他是如何从着火的车厢里逃出来的。-> season_2

#### 后备选项示例｜Example of a fallback choice

将其与前面的例子相加，就得出了结果：

	=== find_help ===

		你在人群中拼命地寻找着友善的面孔。
		*	那个戴帽子的女人[？]粗暴地把你推到了一边。-> find_help
		*	那个拿公文包的男人[？]一脸嫌弃地看着你然后走开了。-> find_help
		*	->
			但为时已晚：你倒在了列车站台上。这就是结局。
			-> END

这将输出：

	你在人群中拼命地寻找着友善的面孔。
	1: 那个戴帽子的女人？
	2: 那个拿公文包的男人？

	> 1
	那个戴帽子的女人粗暴地把你推到了一边。
	你在人群中拼命地寻找着友善的面孔。

	1: 那个拿公文包的男人？

	> 1
	那个拿公文包的男人一脸嫌弃地看着你然后走开了。
	你在人群中拼命地寻找着友善的面孔。
	但为时已晚：你倒在了列车站台上。这就是结局。

### 粘滞选项｜Sticky choices

当然，“一次性”的行为并不总是我们想要的，所以我们还有第二种选择：“粘滞”选择。粘滞就是不会被用完的选择，选一次之后还能再选，它用 "+"标记。

	=== homers_couch ===
		+	[吃另一个甜甜圈]
			你吃了另一个甜甜圈。 -> homers_couch
		*	[从沙发上起来]
			你挣扎着从沙发上站起来，去创作史诗。
			-> END

后备选项也可以是粘滞选项：

	=== conversation_loop
		*	[谈论最近的天气] -> chat_weather
		*	[谈论孩子们的事情] -> chat_children
		+	-> sit_in_silence_again

### 条件选项｜Conditional Choices

您还可以手动打开或关闭选择。**Ink** 有很多可用的逻辑，但最简单的检测是“玩家是否看过某个特定内容”。

游戏中的每个结点与接缝都有一个唯一的地址（这样它就可以被分道到），我们使用相同的地址来检测该内容是否被查看过。

	*	{ not visit_paris } 	[去巴黎] -> visit_paris
	+ 	{ visit_paris 	 } 		[回到巴黎] -> visit_paris
	*	{ visit_paris.met_estelle } [致电艾斯特尔女士] -> phone_estelle

需要注意的是：如果要检测的 `knot_name`（结点名）内含有接缝的话，则需要看完*所有的*接缝后，返回的结果才是“ture”（是、真）。

还要注意的是，条件选项也是一次性选项，因此你仍然需要将其标识为粘滞选项才可进行重复选择。

#### 进阶：多重条件｜Advanced: multiple conditions

您可以在一个选项上使用多个逻辑检测；如果这样做的话，那么*所有的*检测都必须通过之后，对应的选项才会出现。

	*	{ not visit_paris } 	[去巴黎] -> visit_paris
  	+ 	{ visit_paris } { not bored_of_paris }		[回到巴黎] -> visit_paris

#### 逻辑运算符：AND 和 OR｜Logical operators: AND and OR

上述“多重条件”实际上只是带有普通 AND 运算符条件编程。Ink 支持常用的 `and`（和、也、并且，也可以写成 `&&`）还有 `or` （或、或者，也可以写成 `||`），也支持半角括号。

	*	{ not (visit_paris or visit_rome) && (visit_london || visit_new_york) } [等等，到底要去哪儿？我有点糊涂了。] -> visit_someplace

译者注：上方的示例条件部分翻译过来：
*	“伪代码”：{非 (visit_paris 或 visit_rome) 且 (visit_london 或着 visit_new_york)}
*	人话：没有 访问过巴黎 或者 访问过罗马，且 访问过伦敦 或者 访问过纽约

对于非程序员来说，假定 `X` 和 `Y` 是两个结点，那么 `X and Y` 就表示 `X` 和 `Y` 都必须为真。`X or Y` 表示二者之一或二者皆是。我们没有 `xor`（“异或”，即当两两数值相同时为否，而数值不同时为真。）。

您也可以使用标准的 `!` 来表示 `not`，不过有时会让编译器感到困惑，因为它认为 `{!text}` 是本文接下来会提到的一种“一次性替文”。我们建议使用 `not` 因为布尔检测很令人头大。（译者注：此外，非程序员会相对难以理解布尔运算。所以此处建议不引入运算符 `!`）

#### 进阶：结点与接缝的实际阅读次数｜Advanced: knot/stitch labels are actually read counts

这是检测：

	*	{seen_clue} [指责杰斐逊先生]

这实际上是在检测一个*整数*，而不是在检测一个是与否的标志。以这种方式使用的结点或接缝实际上是在设置一个整数变量，其中包含玩家看到该地址内容的次数。

如果它不为零，就会在类似上面的检测中返回 `true`，但也可以更具体一些：

	* {seen_clue > 3} [直接逮捕杰斐逊先生]


#### 进阶：更多逻辑｜Advanced: more logic

**Ink** 支持的逻辑和条件性远不止这些，请参阅[变量和逻辑](#第-3-部分变量和逻辑part-3-variables-and-logic)部分。


## 8) 可变文本｜Variable Text

### 文本是可以变更的｜Text can vary

到目前为止，我们看到的所有内容都是静态、固定的文本。但是，内容也可以在打印输出时发生变化。

### 序列、循环以及其他类型的替文｜Sequences, cycles and other alternatives

最简单的可变文本就是替文，它依据某些规则进行选择。**Ink** 支持多种类型的替文。替文写在 `{`...`}` 这样的花括号内，各个替文元素之间使用半角分隔符 `|` 隔开。

只有当一个内容片段被多次访问时，这些替文才会有效！

#### 替文的类型｜Types of alternatives

**序列**（默认替文类型）：

序列（或称 "倒数区块"）是一组会跟踪它自己被查看了多少次，并在每次观看时显示下一个元素的替文元素组。当其中的替文元素用完时，它会保持显示最后一个元素：

	无线电嘶嘶作响。{"三！"|"二！"|"一！"|*传来一声巨大的白噪音，如同炸雷。*|但那只是静电噪声。}
	
	{我用五英镑纸币买了一杯咖啡，又给朋友买了第二杯。｝

	{我用我的五英镑钞票买了一杯咖啡|我为我的朋友买了第二杯咖啡。|我没有钱没更多咖啡了。}

**循环**（使用 `&` 标记）：

循环就像序列一样，但是它会循环它的内容：

	今天是{&星期一|星期二|星期三|星期四|星期五|星期六|星期天}。

**一次性**（使用 `!` 标记）：

一次性替文和序列提问类似，但是当它们没有新内容可以显示的时候就什么也不现实。（你可以把这个想象成最后一条内容为空的序列替文）。

	他跟我开了个玩笑。{!我礼貌性地笑了一下。|我微笑了一下。|我苦笑了一下。|我向我自己保证我不会再有反应了。}

**乱序**（使用 `~` 标记）：

乱序会产生随机输出。

	我跑了一枚硬币。{~正面|反面}。

#### 替文的特点｜Features of Alternatives

替文可以包含空白元素：

	我向前走了一步。{!||||然后灯灭了。-> eek}

替文可以套娃：

	鼠熊{&{&一下子就|}挠|抓}{&伤了你|到了你的{&腿|胳膊|脸颊}}。

替文可以嵌套分道声明：

	我{就这么等着。|继续等着。|都等睡着了。|都睡醒了还没有等到。|放弃并离开了。-> leave_post_office}

也可以在选项中使用替文：

  	+	“你好，{&老大|福格先生|天气不错|棕色眼睛的朋友}！”[]我问候道。

（……但有一点要注意；你不能使用 `{` 这个符号来作为一个选项的文本，因为它看起来像一个表达式。）

（……但是注意事项也有关于注意事项的注意事项，如果您在 `{` 之前下一个转译空格 `\ `，那么 Ink 就会将那个花括号识别为文本了。）

	+	\ {&他们向沙地进发|他们向沙漠出发|一行人沿着老路向南。}

#### 示例｜Examples

替文可以在循环中使用，从而不费吹灰之力就能创造出智能的、紧跟游戏状态的演出。

这是一个单结点版本的打地鼠游戏。请注意，在这个脚本中我们只使用了一次性选择的选项，还有后备选项，以确保地鼠永远不会移动，游戏永远会结束。

	=== whack_a_mole ===
		{我一锤子砸下去。|{~没打着！|啥也没！|啊，它去哪了？|啊哈！打中了！-> END}}
		这{~讨厌的|该死的|可恶的}{~东西|啮齿动物}仍然{在什么地方|藏在某处|逍遥在外|在什么地方嘲笑我|没有被敲死|还没有完犊子}。<>
		{!头套给丫薅掉！|必须打它脸！}
		*	[{&打|击打|试试}左上角]-> whack_a_mole
		*	[{&敲|锤|砸}右上角]-> whack_a_mole
		*	[{&猛击|锤击}中间]-> whack_a_mole
		*	[{&埋伏|奇袭}左下角]-> whack_a_mole
		*	[{&钉打|重击}右下角]-> whack_a_mole
		*	->
				然后你就被“累鼠”了。地鼠打败了你！
				-> END

这个“游戏”的实况是这样的：

	我一锤子砸下去。
	这讨厌的东西仍然在什么地方。头套给丫薅掉！

	1: 打左上角
	2: 敲右上角
	3: 猛击中间
	4: 埋伏左下角
	5: 钉打右下角

	> 1
	没打着！
	这该死的啮齿动物仍然在什么地方。必须打它脸！

	1: 捶右上角
	2: 锤击中间
	3: 奇袭左下角
	4: 重击右下角

	> 4
	啥也没！
	这可恶的东西仍然逍遥法外。

	1: 砸右上角
	2: 猛击中间
	3: 埋伏左下角

	> 2

	啊，它去哪了？
	这讨厌的东西仍然在什么地方嘲笑我。

	1: 敲右上角
	2: 奇袭左下角

	> 1
	啊哈！打中了！

这有一个关于游戏生命周期的建议：注意活用粘滞选项——无尽的的电视诱惑：

And here's a bit of lifestyle advice. Note the sticky choice - the lure of the television will never fade:

	=== turn_on_television ===
	我{第一次|第二次|又|再一次}打开电视，但是{没有什么有意思的，所以我又把它关掉了|仍然没有什么值得一看的|这次的东西甚至更让我没兴趣了|啥也没，都是乐色|这次是一个关于鲨鱼的节目，我不喜欢鲨鱼}。

	+	[要不，再看看别的？]-> turn_on_television
	*	[还是出去逛逛吧]-> go_outside_instead

    === go_outside_instead ===
    -> END



#### 另行参见：多行替文｜Sneak Preview: Multiline alternatives
**Ink** 还有另一种格式来制作替换内容块用的替文。详见 [多行代码块](#多行代码块multiline-blocks)。



### 条件文本｜Conditional Text

文本也可以像选项一样根据逻辑检测的结果不同而变化。

	{met_blofeld: “我看见他了。只有那么一瞬间。”}

还有

	“他的名字是{met_blofeld.learned_his_name: 弗朗茨|个秘密}。”

它们可以作为单独一行的出现，也可以出现在内容的某个部分中。它们甚至可以嵌套，例如：

	{met_blofeld: “我看见他了。只有那么一瞬间。他的真名{met_blofeld.learned_his_name: 是弗朗茨|还需要保密}。”|“我想他了。他很邪恶么？”

这可能会输出一下结果：

	“我看见他了。只有那么一瞬间。他的真名是弗朗茨。”

或：

	“我看见他了。只有那么一瞬间。他的真名还需要保密。”

或者：

	“我想他了。他很邪恶么？”

## 9) 游戏查询和函数｜Game Queries and Functions

**Ink** 提供了关于游戏状态的一些非常有用的“游戏等级”查询，这可以用于逻辑条件。它们并不完全是本编程语言的一部分，但它们总是可用的，而且作者无法对它们进行编辑。从某种意义上说，它们是本编程语言语言的“标准函数库”。

命名惯例是使用大写字母。

### 选项计数函数｜CHOICE_COUNT()

`CHOICE_COUNT` 会返回当前块目前已创建了的选项的个数。例如：

	*	{false} 选项 A
	*	{ture} 选项 B
	*	{CHOICE_COUNT() == 1} 选项 C

这回生成两个选项，B 和 C。这对于控制玩家在一个回合内有多少个选项是很有用的。

### 总回合计数函数｜TURNS()

`TURNS()` 这个函数会返回游戏开始后的游戏回合数。

### 分道计数函数｜TURNS_SINCE(-> knot)

`TURNS_SINCE` 返回自上次访问某个结点或接缝之后，玩家操作了多少次。（玩家操作在形式上来说就是玩家的交互输入）。

值为 0 就表示“你目前正在你所检测的结点或接缝中使用这个函数”。值为 -1 就表示那个要检测的结点或接缝还从来没有被看过。其它任何的正值都表示你要检测的内容在多少个回合之前出现过了。

	*	{TURNS_SINCE(-> sleeping.intro) > 10} 你感到疲乏……-> sleeping
  	* 	{TURNS_SINCE(-> laugh) == 0} 你尝试不再笑。

请注意：传递参数给 `TURNS_SINCE` 的是具体的“分道目标”，而不是简单的结点地址本身（因为结点地址在程序那边是一串数字，是一个读数，而不是一个故事中的某个位置）

TODO: （向编译器传递 `-c` 的要求）
（译者注：上面这个 TODO 是 Ink 的开发者给他们自己写的版本计划。）

#### 功能预览：在功能中使用分道计数函数｜Sneak preview: using TURNS_SINCE in a function

`TURNS_SINCE(->x) == 0` 检测是一个非常有用的函数，通常值得将其单独包装成一个 Ink 的功能。

	=== function came_from(-> x)
		~ return TURNS_SINCE(x) == 0

[函数](#5-函数functions)这个章节对此处的语法概述会讲的更清楚一些。简单来说，上面的这句语法可以让写出一些以下的内容：

	*	{came_from(->  nice_welcome)} ‘来到这让我很开心！’
	*	{came_from(->  nasty_welcome)} ‘咱还是快一些吧。’

……这可以让游戏对玩家*刚才*看到的内容作出反应。

### 种子随机函数｜SEED_RANDOM()

处于测试的目的，通常需要固定的随机数生成器，以便每次游戏都能产生相同的结果。您可以通过给随机数系统“设定种子号 (Seeding)”来做到这一点。

	~ SEED_RANDOM(235)

您传给种子函数的种子号是有您任意指定的，但是如果提供了相同的数字就会产生结果相同的序列。所以为了产生不同的随机序列，您需要提供不同的种子号。

#### 进阶：更多查询｜Advanced: more queries

您也可以创建您自己的外部函数，但是语法会略有不同，详情请见后文中的[函数](#5-函数functions)章节。

# 第 2 部分：织体｜Part 2: Weave

到目前为止，我们一直在用最简单的方式构建分支故事，即通过“选项 (Options)”链接到“页面 (Pages)”。

但这要求我们对故事中的每个目的地都进行唯一命名，这可能会减慢写作速度，并阻碍小分支的出现。

**Ink** 有一种功能更强大的语法，专门用于简化始终向前的故事流（大多数故事都是这样，而大多数计算机程序则不是）。

这种格式就被称为“织体 (Weave)”，它在基本内容和选项语法的基础上增加了两个新功能：收束 (Gather)，`-`，还有选择与收束的嵌套。

## 1) 收束｜Gathers

### 收束点将故事流收束到一起｜Gather points gather the flow back together

让我们回到本文开头的第一个多选示例。

	“你说什么？”我的老大问我。
	*	“我有点累了[。”]，老大……”我重复着。
		“这样啊。”他回应道：“那休息一下吧。”
    *	“没事的老大！”[]我说。
		“很好，那继续吧。”
	*	“我说，这次的冒险真的很可怕[……”]，我真的不想再继续了……
		“啊……别这样。”他安慰着我：“看起来你现在有些累了。明天，事情一定会有所好转的。”

在实际游戏中，这三个选项都可能导致相同的结果——福格先生离开了房间。我们可以用“收束”而无需创建任何新的结点或分道而完成这一点。

	“你说什么？”我的老大问我。
	*	“我有点累了[。”]，老大……”我重复着。
		“这样啊。”他回应道：“那休息一下吧。”
    *	“没事的老大！”[]我说。
		“很好，那继续吧。”
	*	“我说，这次的冒险真的很可怕[……”]，我真的不想再继续了……
		“啊……别这样。”他安慰着我：“看起来你现在有些累了。明天，事情一定会有所好转的。”

	-	说完，福格先生离开了房间。

这可能会输出以下的游玩路径：

	“你说什么？”我的老大问我。

	1: “我有点累了。”
    2: “没事的老大！”
	3: “我说，这次的冒险真的很可怕……”

	> 1
	“我有点累了，老大……”我重复着。
	“这样啊。”他回应道：“那休息一下吧。”
	说完，福格先生离开了房间。

### 内容链的选项和收束｜Options and gathers form chains of content

我们可以将这些收束和分支部分串联起来，以使得故事分支继续推进。

	=== escape ===
	我在森林里奔跑，狗在我身后追赶。

		*	我检查了一下珠宝[是否还在。]，它们还在我的口袋里，它们的触感给了我慰籍，让我的脚步跟踩了弹簧一样跑的更快了。<>

		*	不要停下来啊！[]继续向前奔跑。<>

		*	我高兴地欢呼起来。<>

  	-	路已经不远了！麦基会发动引擎，然后我就安全了。
		
		*	我走到路上，四处张望[]。你敢信吗？
		*	我要插一句，麦基通常都非常可靠[]。他从没让我失望过。或者说，在那天晚上之前，他从没让我失望过。

	-	路上空无一人。麦基不见踪影。

这是组织织体最基本的方式。本节的其余部分将详细介绍一些附加功能，这些功能可以用来制作织体嵌套、内容的旁道 (Side-Track) 和分道 (Diversions)、在自身内部进行分道，最重要的是，还根据前面的选择来影响后面的内容。

#### 织体的理念（方法论）｜The weave philosophy

织体不仅是对分支的方便封装，也是编写更经得起推敲的内容的一种方法。上面的 `escape` 示例就已经有四种可能的路径了，而更复杂的序列可能会有更多更多的路径。如果使用普通的分道，就必须挨个检查结点链接，这样很容易出现错误。

在织体中，流程保证从顶部开始，然后一路走到底。在基本的织体结构中，流程错误是不可能发生的，而且输出文本可以很容易地略读。
这就意味着无需在游戏中实际测试所有分支也能确保它们按预期运行。

织体还可以方便地重新起草选择点，特别是那些出于多样性或节奏的选择。它很容易将句子拆开并插入额外的选项，而无需重新设计任何流程。

## 2) 嵌套故事流｜Nested Flow

上图中的织体是非常简单的“扁平”结构。无论玩家做什么，从开头到结尾都需要相同的回合数。然后有时某些选择应当需要更多的深度或复杂性。

为此，我们允许织体嵌套。

本节有一个警告。嵌套编织功能强大，结构紧凑，但需要一点时间来适应！

### 选项可以嵌套｜Options can be nested

请看下面的场景：

	-	“波洛？你认为这是谋杀还是自杀？”
	*	“谋杀！”
	*	“自杀！”
	-	克里斯蒂女士稍后放下了手稿。写作小组的其他成员坐在一旁，张大了嘴巴。

第一个选择是“谋杀！”或“自杀！”。如果波洛宣布是自杀，那就没什么可做的了，但如果是谋杀，就需要追问——他怀疑谁？

我们可以通过一组嵌套的子选项来添加新的选项。我们可以用两个星号而不是一个星号来表示这些新选项是另一个选项的“一部分”。

	-	“波洛？你认为这是谋杀还是自杀？”
	*	“谋杀！”
		“那你认为是谁干的呢？”
		**	“贾普探长！”
		**	“黑斯廷斯上尉！”
		**	“就是我！”
	*	“自杀！”
	-	克里斯蒂女士稍后放下了手稿。写作小组的其他成员坐在一旁，张大了嘴巴。

（注意，使用缩进来显示嵌套也是一种好的风格，这会便于作者审阅，但编译器并不会介意）。

如果我们想在另一条路径上添加新的子选项，也可以用类似的方法来实现。

	-	“波洛？你认为这是谋杀还是自杀？”
	*	“谋杀！”
		“那你认为是谁干的呢？”
		**	“贾普探长！”
		**	“黑斯廷斯上尉！”
		**	“就是我！”
	*	“自杀！”
		“真的么，波洛？你确定么？”
		**	“非常确定。”
		**	“这是显而易见的。”
	-	克里斯蒂女士稍后放下了手稿。写作小组的其他成员坐在一旁，张大了嘴巴。

现在，最初的指控选择将引出具体的后续问题——但无论如何，流程都将在克里斯蒂夫人的出场时收束到一起。

但是，如果我们想要一个更长的分镜头呢？

### 收束点也可以嵌套｜Gather points can be nested too

有时，问题不在于选项数量的增加，而在于故事节点的增加。我们可以通过嵌套收束点和选项来实现这一点。

	-	“波洛？你认为这是谋杀还是自杀？”
	*	“谋杀！”
		“那你认为是谁干的呢？”
		**	“贾普探长！”
		**	“黑斯廷斯上尉！”
		**	“就是我！”
		--	“你一定是在开玩笑！”
		**	“我的朋友，我是认真的。”
		**	“只是……”
	*	“自杀！”
		“真的么，波洛？你确定么？”
		**	“非常确定。”
		**	“这是显而易见的。”
	-	克里斯蒂女士稍后放下了手稿。写作小组的其他成员坐在一旁，张大了嘴巴。

如果玩家选择了“谋杀”选项，他们的这条子分支上就会连续出现两个选择——只属于这条子分支的扁平织体。

#### 进阶：收束这个操作做了什么｜Advanced: What gathers do

收束是直观的，但它们的行为却很难用语言表达：一般来说，在一个选项被选中后，故事会找到下一个收束点并向其分道而去。

基本原理是：选项将故事情节分开，而收束则将它们重新聚拢。（这一套下来得名“织体”）

### 你可以根据你自己的需要设置多层嵌套｜You can nest as many levels are you like

上面，我们使用了两层嵌套：主流程和子流程。但是，我们并没有限制嵌套的深度。

	-	“跟我们讲个故事吧，队长！”
		*	“好吧，你们这些‘海狗’。我还真有个故事……”
			**	“那是一个风雨交加的漆黑夜晚……”
				***	“……船员们都很不安……”
					****	“……他们对船长说……”
						*****	“船长给我们讲个故事吧！”
		*	“不行，你们该上床睡觉了。”
	-	船员们打起了哈欠。

过一段时间后，这种嵌套就会变得难以阅读和操作，因此，如果嵌套会变得臃肿的话，将其分道到一个新的接缝会是一个好的操作。

但至少在理论上，你可以把整个故事只写成一个织体。

### 示例：用嵌套节点写的对话｜Example: a conversation with nested nodes

这个示例有点长：

	-	我看着福格先生
	*	……我再也控制不住我自己了。
		“我们此行的目的是什么，先生？”
		“为了打个赌。”他说。
		**	“打个赌！”[]我重复着。
			他点点头。
			***	“但这真的很蠢！”
			***	“这也太糟了吧！”
			---	他又点了点头。
			***	“那我们能赢么？”
				“这正是我们要努力查明的。”他回答道。
			***	“赌注应该不大吧？”
				“两万英镑。”他斩钉截铁地回答道。
			***	我没什么想问的了。
				他最后礼貌地咳嗽了一声后，也没有再说什么。<>
		**	“啊？”[]我不敢相信。
		--	在那之后，<>
	*	……但我什么也没有说。<>
	-	我们在沉默中度过了一天。
	-	->	END

有几种可能的玩法。一个短的：

	我看着福格先生
	
	1: ……我再也控制不住我自己了。
	2: ……但我什么也没有说。

	> 2
	……但我什么也没有说。我们在沉默中度过了一天。

一个长点的：

		我看着福格先生
	
	1: ……我再也控制不住我自己了。
	2: ……但我什么也没有说。

	> 1
	……我再也控制不住我自己了。
	“我们此行的目的是什么，先生？”
	“为了打个赌。”他说。

	1: “打个赌！”
	2: “啊？”

	> 1
	“打个赌！”我重复着。
	他点点头。
	
	1: “但这真的很蠢！”
	2: “这也太糟了吧！”

	> 2
	“这也太糟了吧！”
	他又点了点头。

	1: “那我们能赢么？”
	2: “赌注应该不大吧？”				
	3: 我没什么想问的了。

	> 2
	“赌注应该不大吧？”
	“两万英镑。”他斩钉截铁地回答道。
	在那之后，我们在沉默中度过了一天。

希望这能证明上文所阐述的理念：编织提供了一种紧凑的方式，可以提供很多分支、很多选择，但又能保证一定可以从开头走到结尾！


## 3) 追踪织体｜Tracking a Weave

有时只使用织体这种结构就足够了。但如果不够，我们就需要更多的控制。

### 织体是庞大且没有地址索引的｜Weaves are largely unaddressed

默认情况下，织体结构中的内容行都没有地址或标签，这意味着它们无法被分道到其他地方，也就无法进行测试。
在最基本的织体结构中，玩家的选择会改变织体的路径和他们所看到的内容，但一旦织体看完了，这些选择和路径就会被遗忘。

不过如果我们想记住玩家看过的内容，也是可以的——我们可以使用 `(label_name)` 语法在需要的地方添加标签。

### 收束和选项也可以打标签｜Gathers and options can be labelled

任何嵌套层的收束点都可以用括号标注。就像：

  	-	(top)

一旦贴上标签，收束点就可以像结点和接缝一样被分道或是测试。这意味着您可以利用之前的决定来改变织体中的后续结果，同时继续保持清晰、可靠且继续发展等织体的所有优点。

选项也可以用括号来打标签，就像收束点一样。但是标签括号需要写在每个选项的文本之前。

这些地址可以在条件测试中使用，对于创建被其他选项解锁的选项时非常有用。

	=== meet_guard ===
	警卫皱着眉头看着你。

	*	(greet)	[向他打招呼]
		“你好啊。”
	*	(get_out)	“让一下。”[]你和警卫说道。

	-	“嗯……”警卫应了一声。

	*	{greet}	“今天过得怎么样？”	// 当你选择了向他打招呼的时候
		“还不赖。”

	*	“怎么了？”[]你有些好奇。

	*	{get_out}	[把他推到一边]	// 当你选择了威胁他时
		你把他粗暴地推到一边，他瞪着眼睛看着你，然后拔出了剑！
		->	fight_guard	// 这个路径将分道出这个织体

	-	“哦……”警卫回应着，然后递给你一个小纸袋。“太妃糖？”

### 界限｜Scope

在同一织体块内，您可以简单地使用标签名称；而在织体块外，您需要一个路径，或者是通往同一结点的不同接缝的路径：

	=== knot ===
	= stitch_one
		-	(gatherpoint)	一些内容。
	= stitch_two
		*	{stitch_one.gatherpoint}	选项

或者指向另一个结点里：

	=== knot_one ===
	-	(gather_one)
  		*	{knot_two.stitch_two.gather_two}	选项

	=== knot_two ===
	= stitch_two
		- (gather_two)
			*	{knot_one.gather_one} 	选项

#### 进阶：所有的选项都可以打标签｜Advanced: all options can be labelled

事实上，Ink 里所有的内容都是织体，即使看不到任何收束。这意味着你可以用括号标注游戏中的*任何*选项，然后用寻址语法引用它。这意味着你可以测试玩家是通过*哪一个*选项得出特定结果的。

	=== fight_guard ===
	……
	= throw_something
	*	(rock) [朝警卫扔石头] -> throw
	* 	(sand) [朝警卫扔沙子] -> throw

	= throw
	你朝警卫扔了{throw_something.rock:一块石头|一把沙子}。

#### 进阶：在织体里循环｜Advanced: Loops in a weave

标签可以让我们在编制织体的过程中创建循环。下面是向 NPC 提问的标准模式。

	- (opts)
		*	“我能从哪里拿一套制服吗？”[]你问那个开朗的警卫。
			“当然可以，就在那个柜子里。”他咧嘴一笑。
		*	“告诉我安保系统的情况。”
			“‘它’相当古老，”警卫向你保证：“就像一块煤炭。”
		*	“有狗么？”
			“很多。”警卫咧嘴一笑回答道：“饿的跟魔鬼一样。”
		//	我们需要玩家询问至少一个问题
		*	{loop}	[没什么想说的了]
			->	done

	- (loop)
		//	在警卫厌烦之前询问几次
		{ -> opts | -> opts | }
		他挠挠头。
		“好了，咱不能一天到晚就站着说话了吧？”他说道。
	- (done)
		你谢过了警卫，然后离开了。

#### 进阶：分道指向到选项｜Advanced: diverting to options

选项也可以被分道指向：但会直接分道到该选项的输出，*就像选择了该选项一样*。因此，打印的内容将忽略方括号内的文字，如果该选项只能使用一次，它将被标记为次数用尽。

	- (opts)
	*	[向警卫做鬼脸]
		你做了个鬼脸，于是警卫向你冲过来了！	-> shove

	*	(shove) [推搡警卫]你推了一把警卫，但是他很快就摆正了重心。
	
	*	{shove} [跟他打架]	-> fight_the_guard

	-	-> opts

输出：

	1: 向警卫做鬼脸
	2: 推搡警卫

	> 1
	你做了个鬼脸，于是警卫向你冲过来了！你推了一把警卫，但是他很快就摆正了重心。

	1: 跟他打架

	>

#### 进阶：在一个选项后直接收束｜Advanced: Gathers directly after an option

以下内容不仅有效，而且经常使用。

	*	“您还好么，先生？”[]我问到。
		--	(quitewell)	“挺好的。”他回应道。
	*	“填字游戏做得怎么样了，先生？”[]我问到。
		-> quitewell
	*	我什么也没说[]，我的老大也什么都没说。
  	-	我们彼此再次陷入了沉默。

注意上方示例中的二级收束点：这里其实真没什么好收束的，但是它为我们提供了一个方便的地方来分道第二个选项。

# 第 3 部分：变量和逻辑｜Part 3: Variables and Logic

到目前为止，我们已经可以制作了条件文本和条件选择，并根据玩家目前所看到的内容进行了检测。

此外，**Ink** 还支持临时和全局变量，可存储数字和内容数据，甚至故事流程命令。在逻辑方面，**Ink** 功能齐全，还包含一些额外的结构，有助于更好地组织分支故事中复杂的逻辑。

## 1) 全局变量｜Global Variables

这是最强大的一种变量，也可以说是对故事最有用的一种变量，是用来存储有关游戏状态的一些独特属性的变量——从主人公口袋里的钱的数量到代表主人公精神状态的值等，不一而足。

这种变量被称为“全局变量”，因为它可以从故事中的任何地方访问——既可以设置，也可以读取。（从传统上来说，程序设计会尽量避免这种情况的发生，因为这会让程序的一部分与另一部分无关。但故事就是故事，而故事都是关于后果的：比如《赌城之旅》的故事也不会一直就在那个赌场里耗着对吧）。

### 定义全局变量｜Defining Global Variables

全局变量可以通过 `VAR` 语句在任何地方定义。全局变量应有一个初始值，该值定义了变量的类型--整数、浮点数（十进制）、内容或故事地址。

	VAR knowledge_of_the_cure = false
	VAR players_name = "Emilia"
	VAR number_of_infected_people = 521
	VAR current_epilogue = -> they_all_die_of_the_plague

### 使用全局变量｜Using Global Variables

我们可以通过测试全局变量来控制选项，并提供条件文本，这与我们之前看到的方法类似。

	=== the_train ===
		火车颠簸得嘎嘎作响。{ mood > 0: 不过，我的心情还是很积极的，并不在意这零星的颠簸|我忍无可忍了}。
		*	{ not knows_about_wager }	“先生，我们为什么要旅行？”[]我问到。
		*	{ knows_about_wager }	我认真思考着我们奇怪的冒险[]，这件事真的可行吗？

#### 进阶：将分道存储为变量｜Advanced: storing diverts as variables

“分道”语句本身实际上也是一种值，可以被存储、更改和转道。

	VAR 	current_epilogue = -> everybody_dies

	=== continue_or_quit ===
	是现在就放弃，还是继续努力拯救你的王国？
	*  [继续努力！]		-> more_hopeless_introspection
	*  [放弃了]		-> current_epilogue

#### 进阶：全局变量是对外可见的｜Advanced: Global variables are externally visible

全局变量可以在运行时和剧情中访问或修改，这在更广泛的程度上为游戏和剧情之间的联结提供了一种很好的方式。

“**Ink** 层”通常是存储游戏变量的好地方；无需考虑保存和加载问题，而且故事本身也能对当前值做出反应。

### 打印输出变量｜Printing variables

变量的值可以使用跟序列和条件文本类似的行内语法打印为内容的一部分：

	VAR friendly_name_of_player = "杰基"
	VAR age = 23

	我的名字是金·帕斯帕特奥特，但是我的朋友都叫我{friendly_name_of_player}。我{age}岁了。

这对调试很有用。有关基于逻辑和变量的更复杂打印输出，请参阅[函数](#5-函数functions)章节。

### 叠加态字符串｜Evaluating strings

也许你会注意到，上面我们提到的变量可以包含“内容”，而不是“字符串”。这是故意的，因为使用 Ink 定义的字符串可以包含 Ink 本身，尽管它的值总是字符串。

	VAR a_colour = ""

	~ a_colour = "{~红|蓝|绿|黄}"

	{a_colour}

这样就会在调用 `a_color` 的时候产生红色、蓝色、绿色或黄色中的某一种。

但要注意，像这样的内容一旦被观测，其值就会被“粘住”（就像薛定谔的猫被观测后就会坍缩为某个状态）下面是例子：

	歹徒打中了你，你眼冒{a_colour}和{a_colour}的星星。
	The goon hits you, and sparks fly before you eyes, {a_colour} and {a_colour}.

……这样写就不会产生非常有趣的效果。上面的结果只会“让你眼冒同一种颜色的星星”（如果您真的希望这样做，我们也十分建议使用文本相关的功能，也就是替文来打印输出颜色！）。

这也就是为什么我们不推荐：

	VAR a_colour = "{~red|blue|green|yellow}"

因为它是全局变量，会直接影响到整个游戏。


## 2) 逻辑｜Logic

显然，我们的全局变量并不打算成为常量，因此我们需要一种语法来更改它们。

由于默认情况下，**Ink** 脚本中的任何文本都会直接打印输出到屏幕上，因此我们使用一个标记符号来表示某一行内容的目的是进行一些数字运算，我们使用 `~` 标记。

以下语句都可以为变量赋值：

	=== set_some_variables ===
		~ knows_about_wager = true
		~ x = (x * x) - (y * y) + c
		~ y = 2 * x * y

检测条件可以这样写：

	{ x == 1.2 }
	{ x / 2 > 4 }
	{ y - 1 <= x * x }

### 数学｜Mathematics

**Ink**支持四种基本数学运算（`+`、`-`、`*` 和 `/`），以及返回整除后余数的 `%`（或 `mod`）。此外还有 POW 可以来表示幂的运算：

	{POW(3, 2)} 的结果是 9.
	{POW(16, 0.5)} 的结果是 4.

如果需要进行更复杂的操作，可以编写函数（必要时可以使用递归），或调用外部游戏代码函数（以进行更高级的操作）。

#### 指定范围的随机整数函数｜RANDOM(min, max)

如果需要，墨水可以使用 RANDOM 函数生成随机整数。RANDOM 就像一个骰子（Shai子、Tou子，无所谓你知道是什么就行。🎲），因此最小值和最大值都是包含在内的。

	~ temp dice_roll = RANDOM(1, 6)

	~ temp lazy_grading_for_test_paper = RANDOM(30, 75)

	~ temp number_of_heads_the_serpent_has = RANDOM(3, 8)

可为随机数生成器添加种子以进行测试，请参阅上文的“游戏查询和功能”部分。

译者注：这个随机整数函数语法必定是

	~ temp <变量名称> = RANDOM(min,max)

那个 `temp` 改不成别的。

#### 进阶：数值类型是隐藏但存在的｜Advanced: numerical types are implicit

运算结果，尤其是除法运算的结果，是根据输入的类型进行类型化的。因此，整数除法返回整数结果，而浮点除法返回浮点结果。

	~ x = 2 / 3
	~ y = 7 / 3
	~ z = 1.2 / 0.5

这会使得 `x` 为 0，`y` 为 2，`z` 为 2.4。

#### 进阶：自定义变量类型｜Advanced: INT(), FLOOR() and FLOAT()

如果不想使用上面那种自动但是隐藏的类型，或想对变量进行取舍，则可以直接将其转换为指定类型。

| 代码 | 类型 | 备注 |
| - | - | - |
| INT() | 整数 | 向零取整，正数取整后会小于等于原来的数，负数反之 |
| FLOOR() | 整数 | 向下取整，取整后的数小于或等于原来的数 |
| FLOAT() | 浮点数 | 双精度二进制浮点数，说人话就是带有小数的数据 |

	{INT(3.2)} 是 3.
	{FLOOR(4.8)} 是 4.
	{INT(-4.8)} 是 -4.
	{FLOOR(-4.8)} 是 -5.

	{FLOAT(4)} 嗯……还是 4.

译者注：截止至翻译更新时还没有向上取整。

### 字符串查询｜String queries

奇怪的是，作为一款文本引擎，**Ink** 却并没有太多字符串处理功能：因为我们假定任何需要进行的字符串转换的都将由游戏代码（或许还有外部函数）来处理。 但我们支持三种基本查询：相等、不相等和子字符串（我们用 `?` 来查询，原因会在稍后的章节中阐明）。

以下的每行内容都会返回“真”：

	{ "Yes, please." == "Yes, please." }
	{ "No, thank you." != "Yes, please." }
	{ "Yes, please" ? "ease" }


## 3) 条件代码块（如果，否则）｜Conditional blocks (if/else)

前面我们已经看到条件代码块可以用于控制选项和故事内容；现在介绍 **Ink** 提供的与普通 if/else-if/else 结构相当的结构。

### 一个简单的“如果”｜A simple 'if'

if 语法查询从开始到当前所产生的所有文本、选项还有结果。用两个花括号 `{`……`}` 括起来的内容为要判断的内容。

	{ x > 0:
		~ y = x - 1
	}

译者注：上面这个翻译成自然语言是：如果 x > 0，就运算 y = x - 1

然后，可以添加“否则”(else)，并提供其他条件：

	{ x > 0:
		~ y = x - 1
	- else:
		~ y = x + 1
	}

译者注：这个翻译成自然语言是：如果 x > 0，就运算 y = x - 1。否则运算 y = x + 1。`else` 前面的 `-` 是必要的。

### 扩展判断条件代码块（如果、或者、否则）｜Extended if/else if/else blocks

上述语法实际上是一种更通用结构的特殊情况，类似于其他语言的 "switch "语句。下面例子中单独的 `-` 开头意味着新的 `if` 判断，作为一个简单的判断来说，只是把判断条件写到了下一行：

	{
		- x > 0:
			~ y = x - 1
		- else:
			~ y = x + 1
	}

译者注：翻译为自然语言：如果 x 大于 0，那么运算 y = x - 1。否则运算 y = x + 1

使用这种结构，我们还可以实现“或者 (else-if)”：

	{
		- x == 0:
			~ y = 0
		- x > 0:
			~ y = x - 1
		- else:
			~ y = x + 1
	}

（请注意：和其他地方一样，空格纯粹是为了便于阅读，没有任何语法意义。）

译者注：翻译为自然语言：如果 x 等于 0，那么 y 等于 0；或者如果 x 大于 0，运算 y = x - 1。否则，运算 y = x + 1

译者再注：作为条件语句，if（如果）肯定是要有的；然后 if-else（或者）是可以没有或者有多个的；else（否则）可以没有，但是有的话只能有一个。“或者”这个用法是有先后顺序的，以写在前面的为先。

### 开关代码块｜Switch blocks

还有一个开关代码块示例：

	{ x:
	- 0: 	zero
	- 1: 	one
	- 2: 	two
	- else: lots
	}

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

### 多行代码块｜Multiline blocks

还有一类多行代码块是对上述替代系统的扩展。下面这些都是有效的，并能实现您所期望的功能：


 	// Sequence: go through the alternatives, and stick on last
	{ stopping:
		-	I entered the casino.
		-  I entered the casino again.
		-  Once more, I went inside.
	}

	// Shuffle: show one at random
	At the table, I drew a card. <>
	{ shuffle:
		- 	Ace of Hearts.
		- 	King of Spades.
		- 	2 of Diamonds.
			'You lose this time!' crowed the croupier.
	}

	// Cycle: show each in turn, and then cycle
	{ cycle:
		- I held my breath.
		- I waited impatiently.
		- I paused.
	}

	// Once: show each, once, in turn, until all have been shown
	{ once:
		- Would my luck hold?
		- Could I win the hand?
	}

#### Advanced: modified shuffles

The shuffle block above is really a "shuffled cycle"; in that it'll shuffle the content, play through it, then reshuffle and go again.

There are two other versions of shuffle:

`shuffle once` which will shuffle the content, play through it, and then do nothing.

	{ shuffle once:
	-	The sun was hot.
	- 	It was a hot day.
	}

`shuffle stopping` will shuffle all the content (except the last entry), and once its been played, it'll stick on the last entry.

	{ shuffle stopping:
	- 	A silver BMW roars past.
	-	A bright yellow Mustang takes the turn.
	- 	There are like, cars, here.
	}


## 4) Temporary Variables

### Temporary variables are for scratch calculations

Sometimes, a global variable is unwieldy. **Ink** provides temporary variables for quick calculations of things.

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

The value in a temporary variable is thrown away after the story leaves the stitch in which it was defined.

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


... and you'll need to use parameters if you want to pass a temporary value from one stitch to another!

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
		-> END


(In fact, this kind of definition is useful enough that **Ink** provides a special kind of knot, called, imaginatively enough, a `function`, which comes with certain restrictions and can return a value. See the section below.)


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

...but note the `->` in the `generic_sleep` definition: that's the one case in **Ink** where a parameter needs to be typed: because it's too easy to otherwise accidentally do the following:

	=== sleeping_in_hut ===
		You lie down and close your eyes.
		-> generic_sleep (waking_in_the_hut)

... which sends the read count of `waking_in_the_hut` into the sleeping knot, and then attempts to divert to it.





## 5) 函数｜Functions

The use of parameters on knots means they are almost functions in the usual sense, but they lack one key concept - that of the call stack, and the use of return values.

**Ink** includes functions: they are knots, with the following limitations and features:

A function:
- cannot contain stitches
- cannot use diverts or offer choices
- can call other functions
- can include printed content
- can return a value of any type
- can recurse safely

(Some of these may seem quite limiting, but for more story-oriented call-stack-style features, see the section on [Tunnels](#1-tunnels).)

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

The following example is long, but appears in pretty much every inkle game to date. (Recall that a hyphenated line inside multiline curly braces indicates either "a condition to test" or, if the curly brace began with a variable, "a value to compare against".)

    === function print_num(x) ===
    {
        - x >= 1000:
            {print_num(x / 1000)} thousand { x mod 1000 > 0:{print_num(x mod 1000)}}
        - x >= 100:
            {print_num(x / 100)} hundred { x mod 100 > 0:and {print_num(x mod 100)}}
        - x == 0:
            zero
        - else:
            { x >= 20:
                { x / 10:
                    - 2: twenty
                    - 3: thirty
                    - 4: forty
                    - 5: fifty
                    - 6: sixty
                    - 7: seventy
                    - 8: eighty
                    - 9: ninety
                }
                { x mod 10 > 0:<>-<>}
            }
            { x < 10 || x > 20:
                { x mod 10:
                    - 1: one
                    - 2: two
                    - 3: three
                    - 4: four
                    - 5: five
                    - 6: six
                    - 7: seven
                    - 8: eight
                    - 9: nine
                }
            - else:
                { x:
                    - 10: ten
                    - 11: eleven
                    - 12: twelve
                    - 13: thirteen
                    - 14: fourteen
                    - 15: fifteen
                    - 16: sixteen
                    - 17: seventeen
                    - 18: eighteen
                    - 19: nineteen
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




##  6) 常量｜Constants


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
	{
        -  secret_agent_location == suitcase_location:
		The secret agent grabs the suitcase!
		~ suitcase_location = HELD_BY_AGENT

	-  secret_agent_location < suitcase_location:
		The secret agent moves forward.
		~ secret_agent_location++
	}

Constants are simply a way to allow you to give story states easy-to-understand names.

## 7) Advanced: Game-side logic

There are two core ways to provide game hooks in the **Ink** engine. External function declarations in ink allow you to directly call C# functions in the game, and variable observers are callbacks that are fired in the game when ink variables are modified. Both of these are described in [Running your ink](RunningYourInk.md).

# 第 4 部分：进阶流程控制｜Part 4: Advanced Flow Control

## 1) Tunnels

The default structure for **Ink** stories is a "flat" tree of choices, branching and joining back together, perhaps looping, but with the story always being "at a certain place".

But this flat structure makes certain things difficult: for example, imagine a game in which the following interaction can happen:

	=== crossing_the_date_line ===
	*	"Monsieur!"[] I declared with sudden horror. "I have just realised. We have crossed the international date line!"
	-	Monsieur Fogg barely lifted an eyebrow. "I have adjusted for it."
	*	I mopped the sweat from my brow[]. A relief!
	* 	I nodded, becalmed[]. Of course he had!
	*  I cursed, under my breath[]. Once again, I had been belittled!

...but it can happen at several different places in the story. We don't want to have to write copies of the content for each different place, but when the content is finished it needs to know where to return to. We can do this using parameters:

	=== crossing_the_date_line(-> return_to) ===
	...
	-	-> return_to

	...

	=== outside_honolulu ===
	We arrived at the large island of Honolulu.
	- (postscript)
		-> crossing_the_date_line(-> done)
	- (done)
		-> END

	...

	=== outside_pitcairn_island ===
	The boat sailed along the water towards the tiny island.
	- (postscript)
		-> crossing_the_date_line(-> done)
	- (done)
		-> END

Both of these locations now call and execute the same segment of storyflow, but once finished they return to where they need to go next.

But what if the section of story being called is more complex - what if it spreads across several knots? Using the above, we'd have to keep passing the 'return-to' parameter from knot to knot, to ensure we always knew where to return.

So instead, **Ink** integrates this into the language with a new kind of divert, that functions rather like a subroutine, and is called a 'tunnel'.

### Tunnels run sub-stories

The tunnel syntax looks like a divert, with another divert on the end:

	-> crossing_the_date_line ->

This means "do the crossing_the_date_line story, then continue from here".

Inside the tunnel itself, the syntax is simplified from the parameterised example: all we do is end the tunnel using the `->->` statement which means, essentially, "go on".

	=== crossing_the_date_line ===
	// this is a tunnel!
	...
	- 	->->

Note that tunnel knots aren't declared as such, so the compiler won't check that tunnels really do end in `->->` statements, except at run-time. So you will need to write carefully to ensure that all the flows into a tunnel really do come out again.

Tunnels can also be chained together, or finish on a normal divert:

	...
	// this runs the tunnel, then diverts to 'done'
	-> crossing_the_date_line -> done
	...

	...
	//this runs one tunnel, then another, then diverts to 'done'
	-> crossing_the_date_line -> check_foggs_health -> done
	...

Tunnels can be nested, so the following is valid:

	=== plains ===
	= night_time
		The dark grass is soft under your feet.
		+	[Sleep]
			-> sleep_here -> wake_here -> day_time
	= day_time
		It is time to move on.

	=== wake_here ===
		You wake as the sun rises.
		+	[Eat something]
			-> eat_something ->
		+	[Make a move]
		-	->->

	=== sleep_here ===
		You lie down and try to close your eyes.
		-> monster_attacks ->
		Then it is time to sleep.
		-> dream ->
		->->

... and so on.


#### Advanced: Tunnels can return elsewhere

Sometimes, in a story, things happen. So sometimes a tunnel can't guarantee that it will always want to go back to where it came from. **Ink** supplies a syntax to allow you to "returning from a tunnel but actually go somewhere else" but it should be used with caution as the possibility of getting very confused is very high indeed.

Still, there are cases where it's indispensable:

	=== fall_down_cliff 
	-> hurt(5) -> 
	You're still alive! You pick yourself up and walk on.
	
	=== hurt(x)
		~ stamina -= x 
		{ stamina <= 0:
			->-> youre_dead
		}
	
	=== youre_dead
	Suddenly, there is a white light all around you. Fingers lift an eyepiece from your forehead. 'You lost, buddy. Out of the chair.'
	 
And even in less drastic situations, we might want to break up the structure:
 
	-> talk_to_jim ->
 
	 === talk_to_jim
	 - (opts) 	
		*	[ Ask about the warp lacelles ] 
			-> warp_lacells ->
		*	[ Ask about the shield generators ] 
			-> shield_generators ->	
		* 	[ Stop talking ]
			->->
	 - -> opts 

	 = warp_lacells
		{ shield_generators : ->-> argue }
		"Don't worry about the warp lacelles. They're fine."
		->->

	 = shield_generators
		{ warp_lacells : ->-> argue }
		"Forget about the shield generators. They're good."
		->->
	 
	 = argue 
	 	"What's with all these questions?" Jim demands, suddenly. 
	 	...
	 	->->

#### Advanced: Tunnels use a call-stack

Tunnels are on a call-stack, so can safely recurse.


## 2) Threads

So far, everything in ink has been entirely linear, despite all the branching and diverting. But it's actually possible for a writer to 'fork' a story into different sub-sections, to cover more possible player actions.

We call this 'threading', though it's not really threading in the sense that computer scientists mean it: it's more like stitching in new content from various places.

Note that this is definitely an advanced feature: the engineering stories becomes somewhat more complex once threads are involved!

### Threads join multiple sections together

Threads allow you to compose sections of content from multiple sources in one go. For example:

    == thread_example ==
    I had a headache; threading is hard to get your head around.
    <- conversation
    <- walking


    == conversation ==
    It was a tense moment for Monty and me.
     * "What did you have for lunch today?"[] I asked.
        "Spam and eggs," he replied.
     * "Nice weather, we're having,"[] I said.
        "I've seen better," he replied.
     - -> house

    == walking ==
    We continued to walk down the dusty road.
     * [Continue walking]
        -> house

    == house ==
    Before long, we arrived at his house.
    -> END

It allows multiple sections of story to combined together into a single section:

    I had a headache; threading is hard to get your head around.
    It was a tense moment for Monty and me.
    We continued to walk down the dusty road.
    1: "What did you have for lunch today?"
    2: "Nice weather, we're having,"
    3: Continue walking

On encountering a thread statement such as `<- conversation`, the compiler will fork the story flow. The first fork considered will run the content at `conversation`, collecting up any options it finds. Once it has run out of flow here it'll then run the other fork.

All the content is collected and shown to the player. But when a choice is chosen, the engine will move to that fork of the story and collapse and discard the others.

Note that global variables are *not* forked, including the read counts of knots and stitches.

### Uses of threads

In a normal story, threads might never be needed.

But for games with lots of independent moving parts, threads quickly become essential. Imagine a game in which characters move independently around a map: the main story hub for a room might look like the following:

	CONST HALLWAY = 1
	CONST OFFICE = 2

	VAR player_location = HALLWAY
	VAR generals_location = HALLWAY
	VAR doctors_location = OFFICE

	== run_player_location
		{
			- player_location == HALLWAY: -> hallway
		}

	== hallway ==
		<- characters_present(HALLWAY)
		*	[Drawers]	-> examine_drawers
		* 	[Wardrobe] -> examine_wardrobe
		*  [Go to Office] 	-> go_office
		-	-> run_player_location
	= examine_drawers
		// etc...

	// Here's the thread, which mixes in dialogue for characters you share the room with at the moment.

	== characters_present(room)
		{ generals_location == room:
			<- general_conversation
		}
		{ doctors_location == room:
			<- doctor_conversation
		}
		-> DONE

	== general_conversation
		*	[Ask the General about the bloodied knife]
			"It's a bad business, I can tell you."
		-	-> run_player_location

	== doctor_conversation
		*	[Ask the Doctor about the bloodied knife]
			"There's nothing strange about blood, is there?"
		-	-> run_player_location



Note in particular, that we need an explicit way to return the player who has gone down a side-thread to return to the main flow. In most cases, threads will either need a parameter telling them where to return to, or they'll need to end the current story section.


### When does a side-thread end?

Side-threads end when they run out of flow to process: and note, they collect up options to display later (unlike tunnels, which collect options, display them and follow them until they hit an explicit return, possibly several moves later).

Sometimes a thread has no content to offer - perhaps there is no conversation to have with a character after all, or perhaps we have simply not written it yet. In that case, we must mark the end of the thread explicitly.

If we didn't, the end of content might be a story-bug or a hanging story thread, and we want the compiler to tell us about those.

### Using `-> DONE`

In cases where we want to mark the end of a thread, we use `-> DONE`: meaning "the flow intentionally ends here". If we don't, we might end up with a warning message - we can still play the game, but it's a reminder that we have unfinished business.

The example at the start of this section will generate a warning; it can be fixed as follows:

    == thread_example ==
    I had a headache; threading is hard to get your head around.
    <- conversation
    <- walking
    -> DONE

The extra DONE tells ink that the flow here has ended and it should rely on the threads for the next part of the story.

Note that we don't need a `-> DONE` if the flow ends with options that fail their conditions. The engine treats this as a valid, intentional, end of flow state.

**You do not need a `-> DONE` after an option has been chosen**. Once an option is chosen, a thread is no longer a thread - it is simply the normal story flow once more.

Using `-> END` in this case will not end the thread, but the whole story flow. (And this is the real reason for having two different ways to end flow.)


#### Example: adding the same choice to several places

Threads can be used to add the same choice into lots of different places. When using them this way, it's normal to pass a divert as a parameter, to tell the story where to go after the choice is done.

	=== outside_the_house
	The front step. The house smells. Of murder. And lavender.
	- (top)
		<- review_case_notes(-> top)
		*	[Go through the front door]
			I stepped inside the house.
			-> the_hallway
		* 	[Sniff the air]
			I hate lavender. It makes me think of soap, and soap makes me think about my marriage.
			-> top

	=== the_hallway
	The hallway. Front door open to the street. Little bureau.
	- (top)
		<- review_case_notes(-> top)
		*	[Go through the front door]
			I stepped out into the cool sunshine.
			-> outside_the_house
		* 	[Open the bureau]
			Keys. More keys. Even more keys. How many locks do these people need?
			-> top

	=== review_case_notes(-> go_back_to)
	+	{not done || TURNS_SINCE(-> done) > 10}
		[Review my case notes]
		// the conditional ensures you don't get the option to check repeatedly
	 	{I|Once again, I} flicked through the notes I'd made so far. Still not obvious suspects.
	- 	(done) -> go_back_to

Note this is different than a tunnel, which runs the same block of content but doesn't give a player a choice. So a layout like:

	<- childhood_memories(-> next)
	*	[Look out of the window]
	 	I daydreamed as we rolled along...
	 - (next) Then the whistle blew...

might do exactly the same thing as:

	*	[Remember my childhood]
		-> think_back ->
	*	[Look out of the window]
		I daydreamed as we rolled along...
	- 	(next) Then the whistle blew...

but as soon as the option being threaded in includes multiple choices, or conditional logic on choices (or any text content, of course!), the thread version becomes more practical.


#### Example: organisation of wide choice points

A game which uses ink as a script rather than a literal output might often generate very large numbers of parallel choices, intended to be filtered by the player via some other in-game interaction - such as walking around an environment. Threads can be useful in these cases simply to divide up choices.

```
=== the_kitchen
- (top)
	<- drawers(-> top)
	<- cupboards(-> top)
	<- room_exits
= drawers (-> goback)
	// choices about the drawers...
	...
= cupboards(-> goback)
	// choices about cupboards
	...
= room_exits
	// exits; doesn't need a "return point" as if you leave, you go elsewhere
	...
```

# 第 5 部分：进阶状态追踪｜Part 5: Advanced State Tracking

Games with lots of interaction can get very complex, very quickly and the writer's job is often as much about maintaining continuity as it is about content.

This becomes particularly important if the game text is intended to model anything - whether it's a game of cards, the player's knowledge of the gameworld so far, or the state of the various light-switches in a house.

**Ink** does not provide a full world-modelling system in the manner of a classic parser IF authoring language - there are no "objects", no concepts of "containment" or being "open" or "locked". However, it does provide a simple yet powerful system for tracking state-changes in a very flexible way, to enable writers to approximate world models where necessary.

#### Note: New feature alert!

This feature is very new to the language. That means we haven't begun to discover all the ways it might be used - but we're pretty sure it's going to be useful! So if you think of a clever usage we'd love to know!


## 1) Basic Lists

The basic unit of state-tracking is a list of states, defined using the `LIST` keyword. Note that a list is really nothing like a C# list (which is an array).

For instance, we might have:

	LIST kettleState = cold, boiling, recently_boiled

This line defines two things: firstly three new values - `cold`, `boiling` and `recently_boiled` - and secondly, a variable, called `kettleState`, to hold these states.

We can tell the list what value to take:

	~ kettleState = cold

We can change the value:

	*	[Turn on kettle]
		The kettle begins to bubble and boil.
		~ kettleState = boiling

We can query the value:

	*	[Touch the kettle]
		{ kettleState == cold:
			The kettle is cool to the touch.
		- else:
		 	The outside of the kettle is very warm!
		}

For convenience, we can give a list a value when it's defined using a bracket:

	LIST kettleState = cold, (boiling), recently_boiled
	// at the start of the game, this kettle is switched on. Edgy, huh?

...and if the notation for that looks a bit redundant, there's a reason for that coming up in a few subsections time.



## 2) Reusing Lists

The above example is fine for the kettle, but what if we have a pot on the stove as well? We can then define a list of states, but put them into variables - and as many variables as we want.

	LIST daysOfTheWeek = Monday, Tuesday, Wednesday, Thursday, Friday
	VAR today = Monday
	VAR tomorrow = Tuesday

### States can be used repeatedly

This allows us to use the same state machine in multiple places.

	LIST heatedWaterStates = cold, boiling, recently_boiled
	VAR kettleState = cold
	VAR potState = cold

	*	{kettleState == cold} [Turn on kettle]
		The kettle begins to boil and bubble.
		~ kettleState = boiling
	*	{potState == cold} [Light stove]
	 	The water in the pot begins to boil and bubble.
	 	~ potState = boiling

But what if we add a microwave as well? We might want start generalising our functionality a bit:

	LIST heatedWaterStates = cold, boiling, recently_boiled
	VAR kettleState = cold
	VAR potState = cold
	VAR microwaveState = cold

	=== function boilSomething(ref thingToBoil, nameOfThing)
		The {nameOfThing} begins to heat up.
		~ thingToBoil = boiling

	=== do_cooking
	*	{kettleState == cold} [Turn on kettle]
		{boilSomething(kettleState, "kettle")}
	*	{potState == cold} [Light stove]
		{boilSomething(potState, "pot")}
	*	{microwaveState == cold} [Turn on microwave]
		{boilSomething(microwaveState, "microwave")}

or even...

	LIST heatedWaterStates = cold, boiling, recently_boiled
	VAR kettleState = cold
	VAR potState = cold
	VAR microwaveState = cold

	=== cook_with(nameOfThing, ref thingToBoil)
	+ 	{thingToBoil == cold} [Turn on {nameOfThing}]
	  	The {nameOfThing} begins to heat up.
		~ thingToBoil = boiling
		-> do_cooking.done

	=== do_cooking
	<- cook_with("kettle", kettleState)
	<- cook_with("pot", potState)
	<- cook_with("microwave", microwaveState)
	- (done)

Note that the "heatedWaterStates" list is still available as well, and can still be tested, and take a value.

#### List values can share names

Reusing lists brings with it ambiguity. If we have:

	LIST colours = red, green, blue, purple
	LIST moods = mad, happy, blue

	VAR status = blue

... how can the compiler know which blue you meant?

We resolve these using a `.` syntax similar to that used for knots and stitches.

	VAR status = colours.blue

...and the compiler will issue an error until you specify.

Note the "family name" of the state, and the variable containing a state, are totally separate. So

	{ statesOfGrace == statesOfGrace.fallen:
		// is the current state "fallen"
	}

... is correct.


#### Advanced: a LIST is actually a variable

One surprising feature is the statement

	LIST statesOfGrace = ambiguous, saintly, fallen

actually does two things simultaneously: it creates three values, `ambiguous`, `saintly` and `fallen`, and gives them the name-parent `statesOfGrace` if needed; and it creates a variable called `statesOfGrace`.

And that variable can be used like a normal variable. So the following is valid, if horribly confusing and a bad idea:

	LIST statesOfGrace = ambiguous, saintly, fallen

	~ statesOfGrace = 3.1415 // set the variable to a number not a list value

...and it wouldn't preclude the following from being fine:

	~ temp anotherStateOfGrace = statesOfGrace.saintly




## 3) List Values

When a list is defined, the values are listed in an order, and that order is considered to be significant. In fact, we can treat these values as if they *were* numbers. (That is to say, they are enums.)

	LIST volumeLevel = off, quiet, medium, loud, deafening
	VAR lecturersVolume = quiet
	VAR murmurersVolume = quiet

	{ lecturersVolume < deafening:
		~ lecturersVolume++

		{ lecturersVolume > murmurersVolume:
			~ murmurersVolume++
			The murmuring gets louder.
		}
	}

The values themselves can be printed using the usual `{...}` syntax, but this will print their name.

	The lecturer's voice becomes {lecturersVolume}.

### Converting values to numbers

The numerical value, if needed, can be got explicitly using the LIST_VALUE function. Note the first value in a list has the value 1, and not the value 0.

	The lecturer has {LIST_VALUE(deafening) - LIST_VALUE(lecturersVolume)} notches still available to him.

### Converting numbers to values

You can go the other way by using the list's name as a function:

	LIST Numbers = one, two, three
	VAR score = one
	~ score = Numbers(2) // score will be "two"

### Advanced: defining your own numerical values

By default, the values in a list start at 1 and go up by one each time, but you can specify your own values if you need to.

	LIST primeNumbers = two = 2, three = 3, five = 5

If you specify a value, but not the next value, ink will assume an increment of 1. So the following is the same:

	LIST primeNumbers = two = 2, three, five = 5


## 4) Multivalued Lists

The following examples have all included one deliberate untruth, which we'll now remove. Lists - and variables containing list values - do not have to contain only one value.

### Lists are boolean sets

A list variable is not a variable containing a number. Rather, a list is like the in/out nameboard in an accommodation block. It contains a list of names, each of which has a room-number associated with it, and a slider to say "in" or "out".

Maybe no one is in:

	LIST DoctorsInSurgery = Adams, Bernard, Cartwright, Denver, Eamonn

Maybe everyone is:

	LIST DoctorsInSurgery = (Adams), (Bernard), (Cartwright), (Denver), (Eamonn)

Or maybe some are and some aren't:

	LIST DoctorsInSurgery = (Adams), Bernard, (Cartwright), Denver, Eamonn

Names in brackets are included in the initial state of the list.

Note that if you're defining your own values, you can place the brackets around the whole term or just the name:

	LIST primeNumbers = (two = 2), (three) = 3, (five = 5)

#### Assiging multiple values

We can assign all the values of the list at once as follows:

	~ DoctorsInSurgery = (Adams, Bernard)
	~ DoctorsInSurgery = (Adams, Bernard, Eamonn)

We can assign the empty list to clear a list out:

	~ DoctorsInSurgery = ()


#### Adding and removing entries

List entries can be added and removed, singly or collectively.

	~ DoctorsInSurgery = DoctorsInSurgery + Adams
 	~ DoctorsInSurgery += Adams  // this is the same as the above
	~ DoctorsInSurgery -= Eamonn
	~ DoctorsInSurgery += (Eamonn, Denver)
	~ DoctorsInSurgery -= (Adams, Eamonn, Denver)

Trying to add an entry that's already in the list does nothing. Trying to remove an entry that's not there also does nothing. Neither produces an error, and a list can never contain duplicate entries.


### Basic Queries

We have a few basic ways of getting information about what's in a list:

	LIST DoctorsInSurgery = (Adams), Bernard, (Cartwright), Denver, Eamonn

	{LIST_COUNT(DoctorsInSurgery)} 	//  "2"
	{LIST_MIN(DoctorsInSurgery)} 		//  "Adams"
	{LIST_MAX(DoctorsInSurgery)} 		//  "Cartwright"
	{LIST_RANDOM(DoctorsInSurgery)} 	//  "Adams" or "Cartwright"

#### Testing for emptiness

Like most values in ink, a list can be tested "as it is", and will return true, unless it's empty.

	{ DoctorsInSurgery: The surgery is open today. | Everyone has gone home. }

#### Testing for exact equality

Testing multi-valued lists is slightly more complex than single-valued ones. Equality (`==`) now means 'set equality' - that is, all entries are identical.

So one might say:

	{ DoctorsInSurgery == (Adams, Bernard):
		Dr Adams and Dr Bernard are having a loud argument in one corner.
	}

If Dr Eamonn is in as well, the two won't argue, as the lists being compared won't be equal - DoctorsInSurgery will have an Eamonn that the list (Adams, Bernard) doesn't have.

Not equals works as expected:

	{ DoctorsInSurgery != (Adams, Bernard):
		At least Adams and Bernard aren't arguing.
	}

#### Testing for containment

What if we just want to simply ask if Adams and Bernard are present? For that we use a new operator, `has`, otherwise known as `?`.

	{ DoctorsInSurgery ? (Adams, Bernard):
		Dr Adams and Dr Bernard are having a hushed argument in one corner.
	}

And `?` can apply to single values too:

	{ DoctorsInSurgery has Eamonn:
		Dr Eamonn is polishing his glasses.
	}

We can also negate it, with `hasnt` or `!?` (not `?`). Note this starts to get a little complicated as

	DoctorsInSurgery !? (Adams, Bernard)

does not mean neither Adams nor Bernard is present, only that they are not *both* present (and arguing).

#### Warning: no lists contain the empty list

Note that the test 

	SomeList ? ()

will always return false, regardless of whether `SomeList` itself is empty. In practice this is the most useful default, as you'll often want to do tests like:

	SilverWeapons ? best_weapon_to_use 
	
to fail if the player is empty-handed.

#### Example: basic knowledge tracking

The simplest use of a multi-valued list is for tracking "game flags" tidily.

	LIST Facts = (Fogg_is_fairly_odd), 	first_name_phileas, (Fogg_is_English)

	{Facts ? Fogg_is_fairly_odd:I smiled politely.|I frowned. Was he a lunatic?}
	'{Facts ? first_name_phileas:Phileas|Monsieur}, really!' I cried.

In particular, it allows us to test for multiple game flags in a single line.

	{ Facts ? (Fogg_is_English, Fogg_is_fairly_odd):
		<> 'I know Englishmen are strange, but this is *incredible*!'
	}


#### Example: a doctor's surgery

We're overdue a fuller example, so here's one.

	LIST DoctorsInSurgery = (Adams), Bernard, Cartwright, (Denver), Eamonn

	-> waiting_room

	=== function whos_in_today()
		In the surgery today are {DoctorsInSurgery}.

	=== function doctorEnters(who)
		{ DoctorsInSurgery !? who:
			~ DoctorsInSurgery += who
			Dr {who} arrives in a fluster.
		}

	=== function doctorLeaves(who)
		{ DoctorsInSurgery ? who:
			~ DoctorsInSurgery -= who
			Dr {who} leaves for lunch.
		}

	=== waiting_room
		{whos_in_today()}
		*	[Time passes...]
			{doctorLeaves(Adams)} {doctorEnters(Cartwright)} {doctorEnters(Eamonn)}
			{whos_in_today()}

This produces:

	In the surgery today are Adams, Denver.

	> Time passes...

	Dr Adams leaves for lunch. Dr Cartwright arrives in a fluster. Dr Eamonn arrives in a fluster.

	In the surgery today are Cartwright, Denver, Eamonn.

#### Advanced: nicer list printing

The basic list print is not especially attractive for use in-game. The following is better:

	=== function listWithCommas(list, if_empty)
	    {LIST_COUNT(list):
	    - 2:
	        	{LIST_MIN(list)} and {listWithCommas(list - LIST_MIN(list), if_empty)}
	    - 1:
	        	{list}
	    - 0:
				{if_empty}
	    - else:
	      		{LIST_MIN(list)}, {listWithCommas(list - LIST_MIN(list), if_empty)}
	    }

	LIST favouriteDinosaurs = (stegosaurs), brachiosaur, (anklyosaurus), (pleiosaur)

	My favourite dinosaurs are {listWithCommas(favouriteDinosaurs, "all extinct")}.

It's probably also useful to have an is/are function to hand:

	=== function isAre(list)
		{LIST_COUNT(list) == 1:is|are}

	My favourite dinosaurs {isAre(favouriteDinosaurs)} {listWithCommas(favouriteDinosaurs, "all extinct")}.

And to be pendantic:

	My favourite dinosaur{LIST_COUNT(favouriteDinosaurs) != 1:s} {isAre(favouriteDinosaurs)} {listWithCommas(favouriteDinosaurs, "all extinct")}.


#### Lists don't need to have multiple entries

Lists don't *have* to contain multiple values. If you want to use a list as a state-machine, the examples above will all work - set values using `=`, `++` and `--`; test them using `==`, `<`, `<=`, `>` and `>=`. These will all work as expected.

### The "full" list

Note that `LIST_COUNT`, `LIST_MIN` and `LIST_MAX` are refering to who's in/out of the list, not the full set of *possible* doctors. We can access that using

	LIST_ALL(element of list)

or

	LIST_ALL(list containing elements of a list)

	{LIST_ALL(DoctorsInSurgery)} // Adams, Bernard, Cartwright, Denver, Eamonn
	{LIST_COUNT(LIST_ALL(DoctorsInSurgery))} // "5"
	{LIST_MIN(LIST_ALL(Eamonn))} 				// "Adams"

Note that printing a list using `{...}` produces a bare-bones representation of the list; the values as words, delimited by commas.

#### Advanced: "refreshing" a list's type

If you really need to, you can make an empty list that knows what type of list it is.

	LIST ValueList = first_value, second_value, third_value
	VAR myList = ()

	~ myList = ValueList()

You'll then be able to do:

	{ LIST_ALL(myList) }

#### Advanced: a portion of the "full" list

You can also retrieve just a "slice" of the full list, using the `LIST_RANGE` function. There are two formulations, both valid:

	LIST_RANGE(list_name, min_integer_value, max_integer_value)

and

	LIST_RANGE(list_name, min_value, max_value)
	
Min and max values here are inclusive. If the game can’t find the values, it’ll get as close as it can, but never go outside the range. So for example:

	{LIST_RANGE(LIST_ALL(primeNumbers), 10, 20)} 

will produce 
	
	11, 13, 17, 19



### Example: Tower of Hanoi

To demonstrate a few of these ideas, here's a functional Tower of Hanoi example, written so no one else has to write it.


	LIST Discs = one, two, three, four, five, six, seven
	VAR post1 = ()
	VAR post2 = ()
	VAR post3 = ()

	~ post1 = LIST_ALL(Discs)

	-> gameloop

	=== function can_move(from_list, to_list) ===
	    {
	    -   LIST_COUNT(from_list) == 0:
	        // no discs to move
	        ~ return false
	    -   LIST_COUNT(to_list) > 0 && LIST_MIN(from_list) > LIST_MIN(to_list):
	        // the moving disc is bigger than the smallest of the discs on the new tower
	        ~ return false
	    -   else:
	    	 // nothing stands in your way!
	        ~ return true

	    }

	=== function move_ring( ref from, ref to ) ===
	    ~ temp whichRingToMove = LIST_MIN(from)
	    ~ from -= whichRingToMove
	    ~ to += whichRingToMove

	== function getListForTower(towerNum)
	    { towerNum:
	        - 1:    ~ return post1
	        - 2:    ~ return post2
	        - 3:    ~ return post3
	    }

	=== function name(postNum)
	    the {postToPlace(postNum)} temple

	=== function Name(postNum)
	    The {postToPlace(postNum)} temple

	=== function postToPlace(postNum)
	    { postNum:
	        - 1: first
	        - 2: second
	        - 3: third
	    }

	=== function describe_pillar(listNum) ==
	    ~ temp list = getListForTower(listNum)
	    {
	    - LIST_COUNT(list) == 0:
	        {Name(listNum)} is empty.
	    - LIST_COUNT(list) == 1:
	        The {list} ring lies on {name(listNum)}.
	    - else:
	        On {name(listNum)}, are the discs numbered {list}.
	    }


	=== gameloop
	    Staring down from the heavens you see your followers finishing construction of the last of the great temples, ready to begin the work.
	- (top)
	    +  [ Regard the temples]
	        You regard each of the temples in turn. On each is stacked the rings of stone. {describe_pillar(1)} {describe_pillar(2)} {describe_pillar(3)}
	    <- move_post(1, 2, post1, post2)
	    <- move_post(2, 1, post2, post1)
	    <- move_post(1, 3, post1, post3)
	    <- move_post(3, 1, post3, post1)
	    <- move_post(3, 2, post3, post2)
	    <- move_post(2, 3, post2, post3)
	    -> DONE

	= move_post(from_post_num, to_post_num, ref from_post_list, ref to_post_list)
	    +   { can_move(from_post_list, to_post_list) }
	        [ Move a ring from {name(from_post_num)} to {name(to_post_num)} ]
	        { move_ring(from_post_list, to_post_list) }
	        { stopping:
	        -   The priests far below construct a great harness, and after many years of work, the great stone ring is lifted up into the air, and swung over to the next of the temples.
	            The ropes are slashed, and in the blink of an eye it falls once more.
	        -   Your next decree is met with a great feast and many sacrifices. After the funeary smoke has cleared, work to shift the great stone ring begins in earnest. A generation grows and falls, and the ring falls into its ordained place.
	        -   {cycle:
	            - Years pass as the ring is slowly moved.
	            - The priests below fight a war over what colour robes to wear, but while they fall and die, the work is still completed.
	            }
	        }
	    -> top



## 5) Advanced List Operations

The above section covers basic comparisons. There are a few more powerful features as well, but - as anyone familiar with mathematical   sets will know - things begin to get a bit fiddly. So this section comes with an 'advanced' warning.

A lot of the features in this section won't be necessary for most games.

### Comparing lists

We can compare lists less than exactly using `>`, `<`, `>=` and `<=`. Be warned! The definitions we use are not exactly standard fare. They are based on comparing the numerical value of the elements in the lists being tested.

#### "Distinctly bigger than"

`LIST_A > LIST_B` means "the smallest value in A is bigger than the largest values in B": in other words, if put on a number line, the entirety of A is to the right of the entirety of B. `<` does the same in reverse.

#### "Definitely never smaller than"

`LIST_A >= LIST_B` means - take a deep breath now - "the smallest value in A is at least the smallest value in B, and the largest value in A is at least the largest value in B". That is, if drawn on a number line, the entirety of A is either above B or overlaps with it, but B does not extend higher than A.

Note that `LIST_A > LIST_B` implies `LIST_A != LIST_B`, and `LIST_A >= LIST_B` allows `LIST_A == LIST_B` but precludes `LIST_A < LIST_B`, as you might hope.

#### Health warning!

`LIST_A >= LIST_B` is *not* the same as `LIST_A > LIST_B or LIST_A == LIST_B`.

The moral is, don't use these unless you have a clear picture in your mind.

### Inverting lists

A list can be "inverted", which is the equivalent of going through the accommodation in/out name-board and flipping every switch to the opposite of what it was before.

	LIST GuardsOnDuty = (Smith), (Jones), Carter, Braithwaite

	=== function changingOfTheGuard
		~ GuardsOnDuty = LIST_INVERT(GuardsOnDuty)


Note that `LIST_INVERT` on an empty list will return a null value, if the game doesn't have enough context to know what invert. If you need to handle that case, it's safest to do it by hand:

	=== function changingOfTheGuard
		{!GuardsOnDuty: // "is GuardsOnDuty empty right now?"
			~ GuardsOnDuty = LIST_ALL(Smith)
		- else:
			~ GuardsOnDuty = LIST_INVERT(GuardsOnDuty)
		}

#### Footnote

The syntax for inversion was originally `~ list` but we changed it because otherwise the line

	~ list = ~ list

was not only functional, but actually caused list to invert itself, which seemed excessively perverse.

### Intersecting lists

The `has` or `?` operator is, somewhat more formally, the "are you a subset of me" operator, ⊇, which includes the sets being equal, but which doesn't include if the larger set doesn't entirely contain the smaller set.

To test for "some overlap" between lists, we use the overlap operator, `^`, to get the *intersection*.

	LIST CoreValues = strength, courage, compassion, greed, nepotism, self_belief, delusions_of_godhood
	VAR desiredValues = (strength, courage, compassion, self_belief )
	VAR actualValues =  ( greed, nepotism, self_belief, delusions_of_godhood )

	{desiredValues ^ actualValues} // prints "self_belief"

The result is a new list, so you can test it:

	{desiredValues ^ actualValues: The new president has at least one desirable quality.}

	{LIST_COUNT(desiredValues ^ actualValues) == 1: Correction, the new president has only one desirable quality. {desiredValues ^ actualValues == self_belief: It's the scary one.}}




## 6) Multi-list Lists


So far, all of our examples have included one large simplification, again - that the values in a list variable have to all be from the same list family. But they don't.

This allows us to use lists - which have so far played the role of state-machines and flag-trackers - to also act as general properties, which is useful for world modelling.

This is our inception moment. The results are powerful, but also more like "real code" than anything that's come before.

### Lists to track objects

For instance, we might define:

	LIST Characters = Alfred, Batman, Robin
	LIST Props = champagne_glass, newspaper

	VAR BallroomContents = (Alfred, Batman, newspaper)
	VAR HallwayContents = (Robin, champagne_glass)

We could then describe the contents of any room by testing its state:

	=== function describe_room(roomState)
		{ roomState ? Alfred: Alfred is here, standing quietly in a corner. } { roomState ? Batman: Batman's presence dominates all. } { roomState ? Robin: Robin is all but forgotten. }
		<> { roomState ? champagne_glass: A champagne glass lies discarded on the floor. } { roomState ? newspaper: On one table, a headline blares out WHO IS THE BATMAN? AND *WHO* IS HIS BARELY-REMEMBERED ASSISTANT? }

So then:

	{ describe_room(BallroomContents) }

produces:

	Alfred is here, standing quietly in a corner. Batman's presence dominates all.

	On one table, a headline blares out WHO IS THE BATMAN? AND *WHO* IS HIS BARELY-REMEMBERED ASSISTANT?

While:

	{ describe_room(HallwayContents) }

gives:

	Robin is all but forgotten.

	A champagne glass lies discarded on the floor.

And we could have options based on combinations of things:

	*	{ currentRoomState ? (Batman, Alfred) } [Talk to Alfred and Batman]
		'Say, do you two know each other?'

### Lists to track multiple states

We can model devices with multiple states. Back to the kettle again...

	LIST OnOff = on, off
	LIST HotCold = cold, warm, hot

	VAR kettleState = (off, cold) // we need brackets because it's a proper, multi-valued list now

	=== function turnOnKettle() ===
	{ kettleState ? hot:
		You turn on the kettle, but it immediately flips off again.
	- else:
		The water in the kettle begins to heat up.
		~ kettleState -= off
		~ kettleState += on
		// note we avoid "=" as it'll remove all existing states
	}

	=== function can_make_tea() ===
		~ return kettleState ? (hot, off)

These mixed states can make changing state a bit trickier, as the off/on above demonstrates, so the following helper function can be useful.

 	=== function changeStateTo(ref stateVariable, stateToReach)
 		// remove all states of this type
 		~ stateVariable -= LIST_ALL(stateToReach)
 		// put back the state we want
 		~ stateVariable += stateToReach

 which enables code like:

 	~ changeState(kettleState, on)
 	~ changeState(kettleState, warm)


#### How does this affect queries?

The queries given above mostly generalise nicely to multi-valued lists

    LIST Letters = a,b,c
    LIST Numbers = one, two, three

    VAR mixedList = (a, three, c)

	{LIST_ALL(mixedList)}   // a, one, b, two, c, three
    {LIST_COUNT(mixedList)} // 3
    {LIST_MIN(mixedList)}   // a
    {LIST_MAX(mixedList)}   // three or c, albeit unpredictably

    {mixedList ? (a,b) }        // false
    {mixedList ^ LIST_ALL(a)}   // a, c

    { mixedList >= (one, a) }   // true
    { mixedList < (three) }     // false

	{ LIST_INVERT(mixedList) }            // one, b, two


## 7) Long example: crime scene

Finally, here's a long example, demonstrating a lot of ideas from this section in action. You might want to try playing it before reading through to better understand the various moving parts.

	-> murder_scene

	// Helper function: popping elements from lists
	=== function pop(ref list)
	   ~ temp x = LIST_MIN(list) 
	   ~ list -= x 
	   ~ return x
	
	//
	//  System: items can have various states
	//  Some are general, some specific to particular items
	//
	

	LIST OffOn = off, on
	LIST SeenUnseen = unseen, seen
	
	LIST GlassState = (none), steamed, steam_gone
	LIST BedState = (made_up), covers_shifted, covers_off, bloodstain_visible
	
	//
	// System: inventory
	//
	
	LIST Inventory = (none), cane, knife
	
	=== function get(x)
	    ~ Inventory += x
	
	//
	// System: positioning things
	// Items can be put in and on places
	//
	
	LIST Supporters = on_desk, on_floor, on_bed, under_bed, held, with_joe
	
	=== function move_to_supporter(ref item_state, new_supporter) ===
	    ~ item_state -= LIST_ALL(Supporters)
	    ~ item_state += new_supporter
	
	
	// System: Incremental knowledge.
	// Each list is a chain of facts. Each fact supersedes the fact before 
	//
	
	VAR knowledgeState = ()
	
	=== function reached (x) 
	   ~ return knowledgeState ? x 
	
	=== function between(x, y) 
	   ~ return knowledgeState? x && not (knowledgeState ^ y)
	
	=== function reach(statesToSet) 
	   ~ temp x = pop(statesToSet)
	   {
	   - not x: 
	      ~ return false 
	
	   - not reached(x):
	      ~ temp chain = LIST_ALL(x)
	      ~ temp statesGained = LIST_RANGE(chain, LIST_MIN(chain), x)
	      ~ knowledgeState += statesGained
	      ~ reach (statesToSet) 	// set any other states left to set
	      ~ return true  	       // and we set this state, so true
	 
	    - else:
	      ~ return false || reach(statesToSet) 
	    }	
	
	//
	// Set up the game
	//
	
	VAR bedroomLightState = (off, on_desk)
	
	VAR knifeState = (under_bed)
	
	
	//
	// Knowledge chains
	//
	
	
	LIST BedKnowledge = neatly_made, crumpled_duvet, hastily_remade, body_on_bed, murdered_in_bed, murdered_while_asleep
	
	LIST KnifeKnowledge = prints_on_knife, joe_seen_prints_on_knife,joe_wants_better_prints, joe_got_better_prints
	
	LIST WindowKnowledge = steam_on_glass, fingerprints_on_glass, fingerprints_on_glass_match_knife
	
	
	//
	// Content
	//
	
	=== murder_scene ===
	    The bedroom. This is where it happened. Now to look for clues.
	- (top)
	    { bedroomLightState ? seen:     <- seen_light  }
	    <- compare_prints(-> top)

    *   (dobed) [The bed...]
        The bed was low to the ground, but not so low something might not roll underneath. It was still neatly made.
        ~ reach (neatly_made)
        - - (bedhub)
        * *     [Lift the bedcover]
                I lifted back the bedcover. The duvet underneath was crumpled.
                ~ reach (crumpled_duvet)
                ~ BedState = covers_shifted
        * *     (uncover) {reached(crumpled_duvet)}
                [Remove the cover]
                Careful not to disturb anything beneath, I removed the cover entirely. The duvet below was rumpled.
                Not the work of the maid, who was conscientious to a point. Clearly this had been thrown on in a hurry.
                ~ reach (hastily_remade)
                ~ BedState = covers_off
        * *     (duvet) {BedState == covers_off} [ Pull back the duvet ]
                I pulled back the duvet. Beneath it was a sheet, sticky with blood.
                ~ BedState = bloodstain_visible
                ~ reach (body_on_bed)
                Either the body had been moved here before being dragged to the floor - or this is was where the murder had taken place.
        * *     {BedState !? made_up} [ Remake the bed ]
                Carefully, I pulled the bedsheets back into place, trying to make it seem undisturbed.
                ~ BedState = made_up
        * *     [Test the bed]
                I pushed the bed with spread fingers. It creaked a little, but not so much as to be obnoxious.
        * *     (darkunder) [Look under the bed]
                Lying down, I peered under the bed, but could make nothing out.

        * *     {TURNS_SINCE(-> dobed) > 1} [Something else?]
                I took a step back from the bed and looked around.
                -> top
        - -     -> bedhub

    *   {darkunder && bedroomLightState ? on_floor && bedroomLightState ? on}
        [ Look under the bed ]
        I peered under the bed. Something glinted back at me.
        - - (reaching)
        * *     [ Reach for it ]
                I fished with one arm under the bed, but whatever it was, it had been kicked far enough back that I couldn't get my fingers on it.
                -> reaching
        * *     {Inventory ? cane} [Knock it with the cane]
                -> knock_with_cane

        * *     {reaching > 1 } [ Stand up ]
                I stood up once more, and brushed my coat down.
                -> top

    *   (knock_with_cane) {reaching && TURNS_SINCE(-> reaching) >= 4 &&  Inventory ? cane } [Use the cane to reach under the bed ]
        Positioning the cane above the carpet, I gave the glinting thing a sharp tap. It slid out from the under the foot of the bed.
        ~ move_to_supporter( knifeState, on_floor )
        * *     (standup) [Stand up]
                Satisfied, I stood up, and saw I had knocked free a bloodied knife.
                -> top

        * *     [Look under the bed once more]
                Moving the cane aside, I looked under the bed once more, but there was nothing more there.
                -> standup

    *   {knifeState ? on_floor} [Pick up the knife]
        Careful not to touch the handle, I lifted the blade from the carpet.
        ~ get(knife)

    *   {Inventory ? knife} [Look at the knife]
        The blood was dry enough. Dry enough to show up partial prints on the hilt!
        ~ reach (prints_on_knife)

    *   [   The desk... ]
        I turned my attention to the desk. A lamp sat in one corner, a neat, empty in-tray in the other. There was nothing else out.
        Leaning against the desk was a wooden cane.
        ~ bedroomLightState += seen

        - - (deskstate)
        * *     (pickup_cane) {Inventory !? cane}  [Pick up the cane ]
                ~ get(cane)
              I picked up the wooden cane. It was heavy, and unmarked.

        * *    { bedroomLightState !? on } [Turn on the lamp]
                -> operate_lamp ->

        * *     [Look at the in-tray ]
                I regarded the in-tray, but there was nothing to be seen. Either the victim's papers were taken, or his line of work had seriously dried up. Or the in-tray was all for show.

        + +     (open)  {open < 3} [Open a drawer]
                I tried {a drawer at random|another drawer|a third drawer}. {Locked|Also locked|Unsurprisingly, locked as well}.

        * *     {deskstate >= 2} [Something else?]
                I took a step away from the desk once more.
                -> top

        - -     -> deskstate

    *     {(Inventory ? cane) && TURNS_SINCE(-> deskstate) <= 2} [Swoosh the cane]
        I was still holding the cane: I gave it an experimental swoosh. It was heavy indeed, though not heavy enough to be used as a bludgeon.
        But it might have been useful in self-defence. Why hadn't the victim reached for it? Knocked it over?

    *   [The window...]
        I went over to the window and peered out. A dismal view of the little brook that ran down beside the house.

        - - (window_opts)
        <- compare_prints(-> window_opts)
        * *     (downy) [Look down at the brook]
                { GlassState ? steamed:
                    Through the steamed glass I couldn't see the brook. -> see_prints_on_glass -> window_opts
                }
                I watched the little stream rush past for a while. The house probably had damp but otherwise, it told me nothing.
        * *     (greasy) [Look at the glass]
                { GlassState ? steamed: -> downy }
                The glass in the window was greasy. No one had cleaned it in a while, inside or out.
        * *     { GlassState ? steamed && not see_prints_on_glass && downy && greasy }
                [ Look at the steam ]
                A cold day outside. Natural my breath should steam. -> see_prints_on_glass ->
        + +     {GlassState ? steam_gone} [ Breathe on the glass ]
                I breathed gently on the glass once more. { reached (fingerprints_on_glass): The fingerprints reappeared. }
                ~ GlassState = steamed

        + +     [Something else?]
                { window_opts < 2 || reached (fingerprints_on_glass) || GlassState ? steamed:
                    I looked away from the dreary glass.
                    {GlassState ? steamed:
                        ~ GlassState = steam_gone
                        <> The steam from my breath faded.
                    }
                    -> top
                }
                I leant back from the glass. My breath had steamed up the pane a little.
               ~ GlassState = steamed

        - -     -> window_opts

    *   {top >= 5} [Leave the room]
        I'd seen enough. I {bedroomLightState ? on:switched off the lamp, then} turned and left the room.
        -> joe_in_hall

    -   -> top
	
	
	= operate_lamp
	    I flicked the light switch.
	    { bedroomLightState ? on:
	        <> The bulb fell dark.
	        ~ bedroomLightState += off
	        ~ bedroomLightState -= on
	    - else:
	        { bedroomLightState ? on_floor: <> A little light spilled under the bed.} { bedroomLightState ? on_desk : <> The light gleamed on the polished tabletop. }
	        ~ bedroomLightState -= off
	        ~ bedroomLightState += on
	    }
	    ->->
	
	
	= compare_prints (-> backto)
	    *   { between ((fingerprints_on_glass, prints_on_knife),     fingerprints_on_glass_match_knife) } 
	[Compare the prints on the knife and the window ]
	        Holding the bloodied knife near the window, I breathed to bring out the prints once more, and compared them as best I could.
	        Hardly scientific, but they seemed very similar - very similiar indeed.
	        ~ reach (fingerprints_on_glass_match_knife)
	        -> backto
	
	= see_prints_on_glass
	    ~ reach (fingerprints_on_glass)
	    {But I could see a few fingerprints, as though someone hadpressed their palm against it.|The fingerprints were quite clear and well-formed.} They faded as I watched.
	    ~ GlassState = steam_gone
	    ->->
	
	= seen_light
	    *   {bedroomLightState !? on} [ Turn on lamp ]
	        -> operate_lamp ->
	
	    *   { bedroomLightState !? on_bed  && BedState ? bloodstain_visible }
	        [ Move the light to the bed ]
	        ~ move_to_supporter(bedroomLightState, on_bed)
	
	        I moved the light over to the bloodstain and peered closely at it. It had soaked deeply into the fibres of the cotton sheet.
	        There was no doubt about it. This was where the blow had been struck.
	        ~ reach (murdered_in_bed)
	
	    *   { bedroomLightState !? on_desk } {TURNS_SINCE(-> floorit) >= 2 }
	        [ Move the light back to the desk ]
	        ~ move_to_supporter(bedroomLightState, on_desk)
	        I moved the light back to the desk, setting it down where it had originally been.
	    *   (floorit) { bedroomLightState !? on_floor && darkunder }
	        [Move the light to the floor ]
	        ~ move_to_supporter(bedroomLightState, on_floor)
	        I picked the light up and set it down on the floor.
	    -   -> top
	
	=== joe_in_hall
	    My police contact, Joe, was waiting in the hall. 'So?' he demanded. 'Did you find anything interesting?'
	- (found)
	    *   {found == 1} 'Nothing.'
	        He shrugged. 'Shame.'
	        -> done
	    *   { Inventory ? knife } 'I found the murder weapon.'
	        'Good going!' Joe replied with a grin. 'We thought the murderer had gotten rid of it. I'll bag that for you now.'
	        ~ move_to_supporter(knifeState, with_joe)
	
	    *   {reached(prints_on_knife)} { knifeState ? with_joe }
	        'There are prints on the blade[.'],' I told him.
	        He regarded them carefully.
	        'Hrm. Not very complete. It'll be hard to get a match from these.'
	        ~ reach (joe_seen_prints_on_knife)
	    *   { reached((fingerprints_on_glass_match_knife, joe_seen_prints_on_knife)) }
	        'They match a set of prints on the window, too.'
	        'Anyone could have touched the window,' Joe replied thoughtfully. 'But if they're more complete, they should help us get a decent match!'
	        ~ reach (joe_wants_better_prints)
	    *   { between(body_on_bed, murdered_in_bed)}
	        'The body was moved to the bed at some point[.'],' I told him. 'And then moved back to the floor.'
	        'Why?'
	        * *     'I don't know.'
	                Joe nods. 'All right.'
	        * *     'Perhaps to get something from the floor?'
	                'You wouldn't move a whole body for that.'
	        * *     'Perhaps he was killed in bed.'
	                'It's just speculation at this point,' Joe remarks.
	    *   { reached(murdered_in_bed) }
	        'The victim was murdered in bed, and then the body was moved to the floor.'
	        'Why?'
	        * *     'I don't know.'
	                Joe nods. 'All right, then.'
	        * *     'Perhaps the murderer wanted to mislead us.'
	                'How so?'
	            * * *   'They wanted us to think the victim was awake[.'], I replied thoughtfully. 'That they were meeting their attacker, rather than being stabbed in their sleep.'
	            * * *   'They wanted us to think there was some kind of struggle[.'],' I replied. 'That the victim wasn't simply stabbed in their sleep.'
	            - - -   'But if they were killed in bed, that's most likely what happened. Stabbed, while sleeping.'
	                    ~ reach (murdered_while_asleep)
	        * *     'Perhaps the murderer hoped to clean up the scene.'
	                'But they were disturbed? It's possible.'
	
	    *   { found > 1} 'That's it.'
	        'All right. It's a start,' Joe replied.
	        -> done
	    -   -> found
	-   (done)
	    {
	    - between(joe_wants_better_prints, joe_got_better_prints):
	        ~ reach (joe_got_better_prints)
	        <> 'I'll get those prints from the window now.'
	    - reached(joe_seen_prints_on_knife):
	        <> 'I'll run those prints as best I can.'
	    - else:
	        <> 'Not much to go on.'
	    }
	    -> END



## 8) Summary

To summarise a difficult section, **Ink**'s list construction provides:

### Flags
* 	Each list entry is an event
* 	Use `+=` to mark an event as having occurred
*  	Test using `?` and `!?`

Example:

	LIST GameEvents = foundSword, openedCasket, metGorgon
	{ GameEvents ? openedCasket }
	{ GameEvents ? (foundSword, metGorgon) }
	~ GameEvents += metGorgon

### State machines
* 	Each list entry is a state
*  Use `=` to set the state; `++` and `--` to step forward or backward
*  Test using `==`, `>` etc

Example:

	LIST PancakeState = ingredients_gathered, batter_mix, pan_hot, pancakes_tossed, ready_to_eat
	{ PancakeState == batter_mix }
	{ PancakeState < ready_to_eat }
	~ PancakeState++

### Properties
*	Each list is a different property, with values for the states that property can take (on or off, lit or unlit, etc)
* 	Change state by removing the old state, then adding in the new
*  Test using `?` and `!?`

Example:

	LIST OnOffState = on, off
	LIST ChargeState = uncharged, charging, charged

	VAR PhoneState = (off, uncharged)

	*	{PhoneState !? uncharged } [Plug in phone]
		~ PhoneState -= LIST_ALL(ChargeState)
		~ PhoneState += charging
		You plug the phone into charge.
	*	{ PhoneState ? (on, charged) } [ Call my mother ]




# 第 6 部分：标识符中的国际字符支持｜Part 6: International character support in identifiers

By default, ink has no limitations on the use of non-ASCII characters inside the story content. However, a limitation currently exsits
on the characters that can be used for names of constants, variables, stictches, diverts and other named flow elements (a.k.a. *identifiers*).

Sometimes it is inconvenient for a writer using a non-ASCII language to write a story because they have to constantly switch to naming identifiers in ASCII and then switching back to whatever language they are using for the story. In addition, naming identifiers in the author's own language could improve the overal readibility of the raw story format.

In an effort to assist in the above scenario, ink *automatically* supports a list of pre-defined non-ASCII character ranges that can be used as identifiers. In general, those ranges have been selected to include the alpha-numeric subset of the official unicode character range, which would suffice for naming identifiers. The below section gives more detailed information on the non-ASCII characters that ink automatically supports.

### Supported Identifier Characters

The support for the additional character ranges in ink is currently limited to a predefined set of character ranges.

Below is a listing of the currently supported identifier ranges.

 - **Arabic**

   Enables characters for languages of the Arabic family and is a subset of the official *Arabic* unicode range `\u0600`-`\u06FF`.


 - **Armenian**

   Enables characters for the Armenian language and is a subset of the official *Armenian* unicode range `\u0530`-`\u058F`.


 - **Cyrillic**

   Enables characters for languages using the Cyrillic alphabet and is a subset of the official *Cyrillic* unicode range `\u0400`-`\u04FF`.


 - **Greek**

   Enables characters for languages using the Greek alphabet and is a subset of the official *Greek and Coptic* unicode range `\u0370`-`\u03FF`.


 - **Hebrew**

   Enables characters in Hebrew using the Hebrew alphabet and is a subset of the official *Hebrew* unicode range `\u0590`-`\u05FF`.


 - **Latin Extended A**

   Enables an extended character range subset of the Latin alphabet - completely represented by the official *Latin Extended-A* unicode range `\u0100`-`\u017F`.


 - **Latin Extended B**

   Enables an extended character range subset of the Latin alphabet - completely represented by the official *Latin Extended-B* unicode range `\u0180`-`\u024F`.

- **Latin 1 Supplement**

   Enables an extended character range subset of the Latin alphabet - completely represented by the official *Latin 1 Supplement* unicode range `\u0080` - `\u00FF`.


**NOTE!** ink files should be saved in UTF-8 format, which ensures that the above character ranges are supported.

If a particular character range that you would like to use within identifiers isn't supported, feel free to open an [issue](/inkle/ink/issues/new) or [pull request](/inkle/ink/pulls) on the main ink repo.
