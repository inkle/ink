# 使用 Ink 进行写作

## 声明

简体中文教程是从英文原文翻译而来。可能会存在版本滞后性，或与英文版有出入，请与英文版为准。

翻译时使用了 DeepSeek、ChatGPT 与 DeepL 辅助。

我可以确保每句翻译我都校正过。

译者：王洛木 (Nomo_Wang@outlook.com)，如果您有关于翻译的问题要提，且不想要占用 Discussion 资源，可以发邮件给译者。

翻译时的 Ink 版本：1.2.0

翻译最后更新时间：2025 年 07 月 01 日

已全部翻译完毕。如果您在阅读时发现问题，请你用上面的联系方式联系译者反馈，感谢您的阅读～

## 此外
根据语义，将部分专有词汇进行翻译：

| 原文          | 翻译   |
| ----------- | ---- |
| Knot        | 结点   |
| Divert      | 转向   |
| Branch      | 分支   |
| Stitch      | 针脚   |
| Weave       | 织体   |
| Gather      | 收束   |
| Scope       | 界限   |
| Tunnel      | 隧道   |
| Thread      | 缝合线  |
| Side-Thread | 旁缝合线 |
| Flags       | 标志   |
| Nodes       | 小节   |

## 内容目录
<details>
  <summary>内容目录</summary>

- [使用 Ink 进行写作](#使用-ink-进行写作)
	- [声明](#声明)
	- [此外](#此外)
	- [内容目录](#内容目录)
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
	- [4) 转向｜Diverts](#4-转向diverts)
		- [从结点转向到结点｜Knots divert to knots](#从结点转向到结点knots-divert-to-knots)
			- [转向是不可见的｜Diverts are invisible](#转向是不可见的diverts-are-invisible)
			- [胶合｜Glue](#胶合glue)
	- [5) 为故事流程进行分支｜Branching The Flow](#5-为故事流程进行分支branching-the-flow)
		- [基本分支｜Basic branching](#基本分支basic-branching)
		- [分支与合并｜Branching and joining](#分支与合并branching-and-joining)
		- [故事流｜The story flow](#故事流the-story-flow)
			- [进阶：循环｜Advanced: Loops](#进阶循环advanced-loops)
	- [6) 包含和针脚｜Includes and Stitches](#6-包含和针脚includes-and-stitches)
		- [结点可以是次级转向｜Knots can be subdivided](#结点可以是次级转向knots-can-be-subdivided)
		- [针脚需要有独一无二的名称｜Stitches have unique names](#针脚需要有独一无二的名称stitches-have-unique-names)
		- [默认为第一个针脚｜The first stitch is the default](#默认为第一个针脚the-first-stitch-is-the-default)
		- [内部转向｜Local diverts](#内部转向local-diverts)
		- [脚本文件可组合｜Script files can be combined](#脚本文件可组合script-files-can-be-combined)
	- [7) 可变选项｜Varying Choices](#7-可变选项varying-choices)
		- [选项只能被使用一次｜Choices can only be used once](#选项只能被使用一次choices-can-only-be-used-once)
			- [后备选项｜Fallback choices](#后备选项fallback-choices)
			- [后备选项示例｜Example of a fallback choice](#后备选项示例example-of-a-fallback-choice)
		- [粘滞选项｜Sticky choices](#粘滞选项sticky-choices)
		- [条件选项｜Conditional Choices](#条件选项conditional-choices)
			- [进阶：多重条件｜Advanced: multiple conditions](#进阶多重条件advanced-multiple-conditions)
			- [逻辑运算符：AND 和 OR｜Logical operators: AND and OR](#逻辑运算符and-和-orlogical-operators-and-and-or)
			- [进阶：结点与针脚的实际阅读次数｜Advanced: knot/stitch labels are actually read counts](#进阶结点与针脚的实际阅读次数advanced-knotstitch-labels-are-actually-read-counts)
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
		- [转向计数函数｜TURNS\_SINCE(-\> knot)](#转向计数函数turns_since--knot)
			- [功能预览：在功能中使用转向计数函数｜Sneak preview: using TURNS\_SINCE in a function](#功能预览在功能中使用转向计数函数sneak-preview-using-turns_since-in-a-function)
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
		- [示例：用嵌套小节编写的对话｜Example: a conversation with nested nodes](#示例用嵌套小节编写的对话example-a-conversation-with-nested-nodes)
	- [3) 追踪织体｜Tracking a Weave](#3-追踪织体tracking-a-weave)
		- [织体是庞大且没有地址索引的｜Weaves are largely unaddressed](#织体是庞大且没有地址索引的weaves-are-largely-unaddressed)
		- [收束和选项也可以打标签｜Gathers and options can be labelled](#收束和选项也可以打标签gathers-and-options-can-be-labelled)
		- [界限｜Scope](#界限scope)
			- [进阶：所有的选项都可以打标签｜Advanced: all options can be labelled](#进阶所有的选项都可以打标签advanced-all-options-can-be-labelled)
			- [进阶：在织体里循环｜Advanced: Loops in a weave](#进阶在织体里循环advanced-loops-in-a-weave)
			- [进阶：转向指向到选项｜Advanced: diverting to options](#进阶转向指向到选项advanced-diverting-to-options)
			- [进阶：在一个选项后直接收束｜Advanced: Gathers directly after an option](#进阶在一个选项后直接收束advanced-gathers-directly-after-an-option)
- [第 3 部分：变量和逻辑｜Part 3: Variables and Logic](#第-3-部分变量和逻辑part-3-variables-and-logic)
	- [1) 全局变量｜Global Variables](#1-全局变量global-variables)
		- [定义全局变量｜Defining Global Variables](#定义全局变量defining-global-variables)
		- [使用全局变量｜Using Global Variables](#使用全局变量using-global-variables)
			- [进阶：将转向存储为变量｜Advanced: storing diverts as variables](#进阶将转向存储为变量advanced-storing-diverts-as-variables)
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
			- [示例：与背景相关的内容｜Example: context-relevant content](#示例与背景相关的内容example-context-relevant-content)
		- [条件块代码块不仅限于逻辑｜Conditional blocks are not limited to logic](#条件块代码块不仅限于逻辑conditional-blocks-are-not-limited-to-logic)
		- [多行代码块｜Multiline blocks](#多行代码块multiline-blocks)
			- [进阶：修改洗牌随机｜Advanced: modified shuffles](#进阶修改洗牌随机advanced-modified-shuffles)
	- [4) 临时变量｜Temporary Variables](#4-临时变量temporary-variables)
		- [临时变量用于临时计算｜Temporary variables are for scratch calculations](#临时变量用于临时计算temporary-variables-are-for-scratch-calculations)
		- [结点和针脚可接收参数｜Knots and stitches can take parameters](#结点和针脚可接收参数knots-and-stitches-can-take-parameters)
			- [示例：定义一个递归结点｜Example: a recursive knot definition](#示例定义一个递归结点example-a-recursive-knot-definition)
			- [进阶：将转向目标作为参数来传递｜Advanced: sending divert targets as parameters](#进阶将转向目标作为参数来传递advanced-sending-divert-targets-as-parameters)
	- [5) 函数｜Functions](#5-函数functions)
		- [定义和调用函数｜Defining and calling functions](#定义和调用函数defining-and-calling-functions)
		- [函数不一定非要有个返回值｜Functions don't have to return anything](#函数不一定非要有个返回值functions-dont-have-to-return-anything)
		- [函数可以直接在同一行内被调用｜Functions can be called inline](#函数可以直接在同一行内被调用functions-can-be-called-inline)
			- [Examples](#examples)
			- [示例：将数字转化为文字｜Example: turning numbers into words](#示例将数字转化为文字example-turning-numbers-into-words)
		- [参数可以通过引用来传递｜Parameters can be passed by reference](#参数可以通过引用来传递parameters-can-be-passed-by-reference)
	- [6) 常量｜Constants](#6-常量constants)
		- [全局常量｜Global Constants](#全局常量global-constants)
	- [7) 进阶：游戏端逻辑｜Advanced: Game-side logic](#7-进阶游戏端逻辑advanced-game-side-logic)
- [第 4 部分：进阶流程控制｜Part 4: Advanced Flow Control](#第-4-部分进阶流程控制part-4-advanced-flow-control)
	- [1) 隧道｜Tunnels](#1-隧道tunnels)
		- [隧道运行子故事｜Tunnels run sub-stories](#隧道运行子故事tunnels-run-sub-stories)
			- [进阶：隧道可以返回到其它位置｜Advanced: Tunnels can return elsewhere](#进阶隧道可以返回到其它位置advanced-tunnels-can-return-elsewhere)
			- [进阶：隧道是使用调用栈的｜Advanced: Tunnels use a call-stack](#进阶隧道是使用调用栈的advanced-tunnels-use-a-call-stack)
	- [2) 缝合线｜Threads](#2-缝合线threads)
		- [缝合线把多个部分合并到一起｜Threads join multiple sections together](#缝合线把多个部分合并到一起threads-join-multiple-sections-together)
		- [缝合线的用法｜Uses of threads](#缝合线的用法uses-of-threads)
		- [何时结束一个旁缝合线？｜When does a side-thread end?](#何时结束一个旁缝合线when-does-a-side-thread-end)
		- [使用 `-> DONE`｜Using `-> DONE`](#使用---doneusing---done)
			- [示例：在多个位置添加相同选项｜Example: adding the same choice to several places](#示例在多个位置添加相同选项example-adding-the-same-choice-to-several-places)
			- [示例：大规模选项的组织管理｜Example: organisation of wide choice points](#示例大规模选项的组织管理example-organisation-of-wide-choice-points)
- [第 5 部分：进阶状态追踪｜Part 5: Advanced State Tracking](#第-5-部分进阶状态追踪part-5-advanced-state-tracking)
			- [注意：新功能提醒！｜Note: New feature alert!](#注意新功能提醒note-new-feature-alert)
	- [1) 基础列表｜Basic Lists](#1-基础列表basic-lists)
	- [2) 复用列表｜Reusing Lists](#2-复用列表reusing-lists)
		- [状态是可以被重复使用的｜States can be used repeatedly](#状态是可以被重复使用的states-can-be-used-repeatedly)
			- [列表的值可以共享名称｜List values can share names](#列表的值可以共享名称list-values-can-share-names)
			- [进阶：LIST 本质上是一个变量｜Advanced: a LIST is actually a variable](#进阶list-本质上是一个变量advanced-a-list-is-actually-a-variable)
	- [3) 值的顺序｜List Values](#3-值的顺序list-values)
		- [将值转换为数字｜Converting values to numbers](#将值转换为数字converting-values-to-numbers)
		- [将数字转换为值｜Converting numbers to values](#将数字转换为值converting-numbers-to-values)
		- [高级：自定义数值映射｜Advanced: defining your own numerical values](#高级自定义数值映射advanced-defining-your-own-numerical-values)
	- [4) 多值列表｜Multivalued Lists](#4-多值列表multivalued-lists)
		- [列表的本质是布尔集合｜Lists are boolean sets](#列表的本质是布尔集合lists-are-boolean-sets)
			- [批量赋值｜Assiging multiple values](#批量赋值assiging-multiple-values)
			- [添加或删除条目｜Adding and removing entries](#添加或删除条目adding-and-removing-entries)
		- [基本查询｜Basic Queries](#基本查询basic-queries)
			- [空值检测｜Testing for emptiness](#空值检测testing-for-emptiness)
			- [精确相等性测试｜Testing for exact equality](#精确相等性测试testing-for-exact-equality)
			- [包含性测试｜Testing for containment](#包含性测试testing-for-containment)
			- [注意：空列表不被任何列表包含｜Warning: no lists contain the empty list](#注意空列表不被任何列表包含warning-no-lists-contain-the-empty-list)
			- [示例：基础信息追踪｜Example: basic knowledge tracking](#示例基础信息追踪example-basic-knowledge-tracking)
			- [示例：医生门诊系统｜Example: a doctor's surgery](#示例医生门诊系统example-a-doctors-surgery)
			- [进阶：优化列表显示｜Advanced: nicer list printing](#进阶优化列表显示advanced-nicer-list-printing)
			- [列表不是必须包含多个值｜Lists don't need to have multiple entries](#列表不是必须包含多个值lists-dont-need-to-have-multiple-entries)
		- [“完整”的列表｜The "full" list](#完整的列表the-full-list)
			- [进阶：“重置”列表的类型｜Advanced: "refreshing" a list's type](#进阶重置列表的类型advanced-refreshing-a-lists-type)
			- [进阶：获取“完整”列表的子集｜Advanced: a portion of the "full" list](#进阶获取完整列表的子集advanced-a-portion-of-the-full-list)
		- [示例：汉诺塔｜Example: Tower of Hanoi](#示例汉诺塔example-tower-of-hanoi)
	- [5) 进阶列表操作｜Advanced List Operations](#5-进阶列表操作advanced-list-operations)
		- [比较列表｜Comparing lists](#比较列表comparing-lists)
			- [“严格的大于”](#严格的大于)
			- [“绝不会小于”](#绝不会小于)
			- [健康忠告！](#健康忠告)
		- [反转列表](#反转列表)
			- [脚注](#脚注)
		- [交集列表](#交集列表)
	- [6) 多列表列表（表中表）](#6-多列表列表表中表)
		- [用列表追踪物品](#用列表追踪物品)
		- [用列表追踪多重状态](#用列表追踪多重状态)
			- [这对查询有何影响？](#这对查询有何影响)
	- [7) 长示例：犯罪现场](#7-长示例犯罪现场)
	- [8) 总结](#8-总结)
		- [标志（Flags）](#标志flags)
		- [状态机（State machines）](#状态机state-machines)
		- [属性（Properties）](#属性properties)
- [第 6 部分：标识符中的国际字符支持｜Part 6: International character support in identifiers](#第-6-部分标识符中的国际字符支持part-6-international-character-support-in-identifiers)
		- [支持的标识符字符](#支持的标识符字符)

</details>


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

译者注：关于结点还有以后的函数和列表等部分的命名
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

在启动 Ink 文件时，结点以外的内容会自动运行。但结点不会。因此，如果你开始使用结点来管理内容，就需要告诉游戏该去哪里。我们可以使用转向箭头 `->`来做到这一点，下一部分将对此进行详细介绍。

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

## 4) 转向｜Diverts

### 从结点转向到结点｜Knots divert to knots

您可以使用“转向箭头”`->`来让故事从一个结点转向到另一个结。无需任何用户输入，转向会立即发生。

	=== back_in_london ===

	我们于晚上 9 点 45 分准时到达伦敦。
	-> hurry_home

	=== hurry_home ===
	我们以最快的速度赶回萨维尔街。

#### 转向是不可见的｜Diverts are invisible

转向甚至可以在句子中可以无缝衔接：

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

将结点、选项和转向结合起来，就形成了的自助游戏 (choose-your-own game) 的基本结构。

	=== paragraph_1 ===
	你站在安纳兰德城墙边，手持长剑。
	* [打开大门] -> paragraph_2
	* [砸了那个大门] -> paragraph_3
	* [打道回府] -> paragraph_4

	=== paragraph_2 ===
	你打开了大门，踏上了门里那条小路。

	...

### 分支与合并｜Branching and joining

利用转向，作者可以将故事流分支，然后再次合并起来，且不会让玩家看到流程已经重新连接。

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

结点和转向相结合就形成了游戏的基本故事流程。但这种流程是“扁平”的——既没有调用堆栈，转向也不会从某处“折返”。

在大多数水墨脚本中，故事流程从顶部开始，像意大利面条一样乱蹦乱跳，最终，希望能到达"->结束"。
在大部分 Ink 脚本中，故事流从顶部开始，然后就像是一盘意面一样，最终到达一个 `-> END`。

这种松散的结构方式可以让作者轻松的续写、分支或合并，也不必担心他们在写作过程中就要想好要创建的结构。而在创建新的分支或分流时，既然不需要任何模板，也不需要跟踪任何状态。

#### 进阶：循环｜Advanced: Loops

您可以使用转向来创建循环内容，**Ink** 有多种可以利用这一点的功能，包括使内容自行变化的方法，以及控制选项选择频率的方法。

更多信息请参阅这些章节：
*	[可变文本｜Variable Text](#8-可变文本variable-text)
*	[条件选项｜Conditional Choices](#条件选项conditional-choices)

另外，下列内容符合规范但是并不好：

	=== round ===
	然后
	-> round

（译者注：上面这是一个无限循环。）

## 6) 包含和针脚｜Includes and Stitches

### 结点可以是次级转向｜Knots can be subdivided

随着故事越来越长，如果没有一些额外的结构，就会变得越来越难以组织。

结点可以包括一种被称为“针脚” (Stitches) 的子部分。这些针脚使用一个等号标记。

	=== the_orient_express ===
	= in_first_class
		...
	= in_third_class
		...
	= in_the_guards_van
		...
	= missed_the_train
		...

例如，可以结点来指定一个场景，然后用针脚来表示场景中的事件。

### 针脚需要有独一无二的名称｜Stitches have unique names

针脚可以使用它的“地址”(Address) 来进行转向。

	*	[乘坐三等座]
		-> the_orient_express.in_third_class

	*	[乘坐警卫间]
		-> the_orient_express.in_the_guards_van

### 默认为第一个针脚｜The first stitch is the default

转到包含针脚的结点时，将转到结点中的第一个针脚。所以：

	*	[乘坐一等座]
		"先生，一等座还有空位么？"
		-> The_orient_express

与下面这个脚本是一样的：

	*	[乘坐一等座]
		"先生，一等座还有空位么？"
		-> the_orient_express.in_first_class

（……除非我们在结点内移动了针脚的顺序！）


您也可以在结点内的那些针脚上方加入任何内容。然而你需要记得为针脚进行转向。因为引擎在有针脚前有内容的时候*不会*自动进入第一个针脚，举个例子：

	=== the_orient_express ===

	已经上了火车了，但是坐到哪里呢？
	*	[一等座] -> in_first_class
	*	[二等座] -> in_second_class

	= in_first_class
		...
	= in_second_class
		...


### 内部转向｜Local diverts

如果你要在结点内进行转向，那么您不需要使用完整的地址就可以进行内部针脚。

	-> the_orient_express

	=== the_orient_express ===
	= in_first_class
		我安顿好了我的老大。
		*	[去三等座]
			-> in_third_class

	= in_third_class
		我把我自己安排在三等座。

这意味着针脚和结点不能共用名称，但是如果相同名称的针脚分别属于不同的结点则可以使用。(因此，"东方快车”和“蒙古号”这两个结点里面都可以包含叫“一等座”的针脚。）

如果使用了模棱两可的名称，编译器会发出警告。

### 脚本文件可组合｜Script files can be combined

您还可以可以把您的脚本内容拆分到多个文件中，只需要使用“包含声明”`INCLUDE` 就可以了。

	INCLUDE newspaper.ink
	INCLUDE cities/vienna.ink
	INCLUDE journeys/orient_express.ink

包含语句应始终放在文件头，而不是在结点内。

把文件分割开不会影响到转向跳转。（换句话说，只要你在文件头声明过了要用到的文件，那么就可以进行跨文件转向。）

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

游戏中的每个结点与针脚都有一个唯一的地址（这样它就可以被转向到），我们使用相同的地址来检测该内容是否被查看过。

	*	{ not visit_paris } 	[去巴黎] -> visit_paris
	+ 	{ visit_paris 	 } 		[回到巴黎] -> visit_paris
	*	{ visit_paris.met_estelle } [致电艾斯特尔女士] -> phone_estelle

需要注意的是：如果要检测的 `knot_name`（结点名）内含有针脚的话，则需要看完*所有的*针脚后，返回的结果才是“ture”（是、真）。

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

#### 进阶：结点与针脚的实际阅读次数｜Advanced: knot/stitch labels are actually read counts

这是检测：

	*	{seen_clue} [指责杰斐逊先生]

这实际上是在检测一个*整数*，而不是在检测一个是与否的标志。以这种方式使用的结点或针脚实际上是在设置一个整数变量，其中包含玩家看到该地址内容的次数。

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

替文可以嵌套转向声明：

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

### 转向计数函数｜TURNS_SINCE(-> knot)

`TURNS_SINCE` 返回自上次访问某个结点或针脚之后，玩家操作了多少次。（玩家操作在形式上来说就是玩家的交互输入）。

值为 0 就表示“你目前正在你所检测的结点或针脚中使用这个函数”。值为 -1 就表示那个要检测的结点或针脚还从来没有被看过。其它任何的正值都表示你要检测的内容在多少个回合之前出现过了。

	*	{TURNS_SINCE(-> sleeping.intro) > 10} 你感到疲乏……-> sleeping
  	* 	{TURNS_SINCE(-> laugh) == 0} 你尝试不再笑。

请注意：传递参数给 `TURNS_SINCE` 的是具体的“转向目标”，而不是简单的结点地址本身（因为结点地址在程序那边是一串数字，是一个读数，而不是一个故事中的某个位置）

TODO: （向编译器传递 `-c` 的要求）
（译者注：上面这个 TODO 是 Ink 的开发者给他们自己写的版本计划。）

#### 功能预览：在功能中使用转向计数函数｜Sneak preview: using TURNS_SINCE in a function

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

在实际游戏中，这三个选项都可能导致相同的结果——福格先生离开了房间。我们可以用“收束”而无需创建任何新的结点或转向而完成这一点。

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

这是组织织体最基本的方式。本节的其余部分将详细介绍一些附加功能，这些功能可以用来制作织体嵌套、内容的旁道 (Side-Track) 和转向 (Diversions)、在自身内部进行转向，最重要的是，还根据前面的选择来影响后面的内容。

#### 织体的理念（方法论）｜The weave philosophy

织体不仅是对分支的方便封装，也是编写更经得起推敲的内容的一种方法。上面的 `escape` 示例就已经有四种可能的路径了，而更复杂的序列可能会有更多更多的路径。如果使用普通的转向，就必须挨个检查结点链接，这样很容易出现错误。

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

有时，问题不在于选项数量的增加，而在于故事要点的增加。我们可以通过嵌套收束点和选项来实现这一点。

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

收束是直观的，但它们的行为却很难用语言表达：一般来说，在一个选项被选中后，故事会找到下一个收束点并向其转向而去。

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

过一段时间后，这种嵌套就会变得难以阅读和操作，因此，如果嵌套会变得臃肿的话，将其转向到一个新的针脚会是一个好的操作。

但至少在理论上，你可以把整个故事只写成一个织体。

### 示例：用嵌套小节编写的对话｜Example: a conversation with nested nodes

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

默认情况下，织体结构中的内容行都没有地址或标签，这意味着它们无法被转向到其他地方，也就无法进行测试。
在最基本的织体结构中，玩家的选择会改变织体的路径和他们所看到的内容，但一旦织体看完了，这些选择和路径就会被遗忘。

不过如果我们想记住玩家看过的内容，也是可以的——我们可以使用 `(label_name)` 语法在需要的地方添加标签。

### 收束和选项也可以打标签｜Gathers and options can be labelled

任何嵌套层的收束点都可以用括号标注。就像：

  	-	(top)

一旦贴上标签，收束点就可以像结点和针脚一样被转向或是测试。这意味着您可以利用之前的决定来改变织体中的后续结果，同时继续保持清晰、可靠且继续发展等织体的所有优点。

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
		->	fight_guard	// 这个路径将转向出这个织体

	-	“哦……”警卫回应着，然后递给你一个小纸袋。“太妃糖？”

### 界限｜Scope

在同一织体块内，您可以简单地使用标签名称；而在织体块外，您需要一个路径，或者是通往同一结点的不同针脚的路径：

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

#### 进阶：转向指向到选项｜Advanced: diverting to options

选项也可以被转向指向：但会直接转向到该选项的输出，*就像选择了该选项一样*。因此，打印的内容将忽略方括号内的文字，如果该选项只能使用一次，它将被标记为次数用尽。

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

注意上方示例中的二级收束点：这里其实真没什么好收束的，但是它为我们提供了一个方便的地方来转向第二个选项。

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

#### 进阶：将转向存储为变量｜Advanced: storing diverts as variables

“转向”语句本身实际上也是一种值，可以被存储、更改和转道。

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
	- 0: 	零
	- 1: 	一
	- 2: 	二
	- else: 许多
	}

#### 示例：与背景相关的内容｜Example: context-relevant content

请注意，这些测试并不一定要基于变量，也可以使用阅读次数，就像其他条件一样，下面的结构也很常见，是“做一些与当前游戏状态相关的内容”的一种表达方式：

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

这种语法的优点是易于扩展和方便确定优先级。

译者注：`++` 的意思是 +1，`--` 的意思是 -1。

### 条件块代码块不仅限于逻辑｜Conditional blocks are not limited to logic

条件块代码块同样可用于控制故事内容和逻辑：

	我盯着福格先生。
	{ know_about_wager:
		<> "但你不是认真的吧？" 我问到。
	- else:
		<> "但这次旅行一定是有原因的，"我确信。
	}
他什么也没有回答，只是像个在研究新品种的昆虫学家一样，死死地盯着他的报纸。

你甚至可以把选项放在条件代码块中：

	{ door_open:
		*	我大步走出车厢[]，我仿佛听到老大在悄悄地自言自语。	-> go_outside
	- else:
		*	我请求离开[]，福格先生一脸惊讶。	-> open_door
		* 	我站起来去开门[]。福格先生似乎并没有被这小小的叛逆举动所困扰。	-> open_door
	}

……但请注意，上述示例中缺少织体语法和嵌套并不是偶然的：这是为了避免混淆各种嵌套。所以无法在条件块中包含收束点。

### 多行代码块｜Multiline blocks

还有一类多行代码块是对上述替文系统的扩展。下面这些都是有效的，并能实现您所期望的功能：

	//	序列：按顺序替换后备选项，最后确定
	{ stopping:
		-	我进入了赌场
		-	我又进入了赌场。
		-	再一次，我进来了。
	}

 	//	洗牌随机：随机抽取一个来显示，抽完所有结果后重抽
	在桌子上，我抽了一张牌。<>
	{ shuffle:
		-	红桃 A
		-	黑桃 K
		-	方片 2
			“你这把不走运啊！”荷官嚷嚷着。
	}

	//	循环：挨个显示，然后再重头
	{ cycle:
		-	我屏住呼吸。
		-	我不耐烦地等待着。
		-	我停顿了一下。
	}

	//	一次性：每个结果在一回游戏里只会抽到一次，抽完了就没有了。
	{ once:
		-	我的运气能保持住吗？
  		-	我能赢吗？
	}

译者注：上面说到的这些方案写法上来说像是某种判定条件，但实际上您可认为是一种“叫对名字就可以放出来的咒语”。只要按照上面的格式正确拼写，就可以使用了。

#### 进阶：修改洗牌随机｜Advanced: modified shuffles

上面提到的洗牌随机实际上是一个“洗牌随机并循环”；即它会将内容洗牌随机后输出一遍。然后再把所有选项洗牌随机后，再输出一遍。

所以还有两个经过修改的洗牌随机：

`shuffle once` 这个可以将内容洗牌后输出。但是输出完了之后就不会再收回并重新洗牌，所以用完就没有内容了。

	{ shuffle once:
	-	太阳真大。
	- 	好热的一天。
	}

`shuffle stopping` 将对所有内容进行洗牌（最后一条除外），一旦输出完毕，就会停留在最后一条上。

译者注：最后一条不参与洗牌。所以并不是最后一条输出什么就停留在什么上，而一定是写在最后的那一条被固定。

	{ shuffle stopping:
	-	一辆银色宝马轰鸣而过。
	-	一辆亮黄色的野马在转弯
	-	这里有很多车
	}

## 4) 临时变量｜Temporary Variables

### 临时变量用于临时计算｜Temporary variables are for scratch calculations

有时，全局变量会显得笨重。**Ink** 提供了临时变量，方便进行一些快速计算。

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
			尽管下着雪，但我却感到无比温暖。
		- else:
			那一晚是我人生中最冷的一晚。
		}

临时变量的值在故事离开定义它的针脚 (Stitch) 后会被丢弃。

### 结点和针脚可接收参数｜Knots and stitches can take parameters

临时变量的一种特别有用形式是参数。任何结点或针脚都可以接收一个参数值。

	*	[指控海斯廷斯]
			-> accuse("海斯廷斯")
	*	[指控布莱克夫人]
			-> accuse("莱克夫人")
	*	[指控我自己]
			-> accuse("自己")

	=== accuse(who) ===
		“我指控{who}！” 波洛宣布道。
		“真的吗？” 贾普问道。 “{who == "myself":是你做的？|你{who}？}”
		“怎么会不是呢？” 波洛反问道。

……如果想从一个针脚传递临时值到另一个针脚时，就需要使用参数！

#### 示例：定义一个递归结点｜Example: a recursive knot definition

在递归中使用临时变量是安全的（与全局变量不同），因此以下代码将正常运行。

	-> add_one_to_one_hundred(0, 1)

	=== add_one_to_one_hundred(total, x) ===
		~ total = total + x
		{ x == 100:
			-> finished(total)
		- else:
			-> add_one_to_one_hundred(total, x + 1)
		}

	=== finished(total) ===
		“结果是 {total}！” 你宣布。
		高斯惊恐地盯着你。
		-> END

（事实上，因为这种定义足够有用，所以 **Ink** 提供了一种特殊的结点类型，称为“函数 (Function)”，对它进行一些限制，就可以返回一个值。详见函数章节。）

#### 进阶：将转向目标作为参数来传递｜Advanced: sending divert targets as parameters

结点和针脚的地址是一种值，用 `->` 字符表示，可以被存储和传递。因此以下代码是合规的，常用且非常有用：

	=== sleeping_in_hut ===
		你躺下并闭上了眼睛。
		-> generic_sleep (-> waking_in_the_hut)

	=== generic_sleep (-> waking)
		你睡着了，也许会做梦等等等等。
		-> waking

	=== waking_in_the_hut
		你站起身来，准备继续你的旅程。

ChatGPT 解析：

这段 Ink 代码的运行方式如下：

1.	进入 sleeping_in_hut：
	*	读者来到 sleeping_in_hut 结点，描述告诉他们：“你躺下并闭上了眼睛。”
	*	然后，代码使用 -> generic_sleep (-> waking_in_the_hut) 将控制权转到 generic_sleep 结点，同时将 waking_in_the_hut 这个跳转目标作为参数传递给 generic_sleep。
2.	进入 generic_sleep 并使用参数：
	*	进入 generic_sleep 后，读者看到“你睡着了，也许会做梦等等等等。”这部分描述。
	*	此外，generic_sleep 中 -> waking 的跳转实际上会指向传入的参数 waking_in_the_hut，即在 generic_sleep 结点完成后，将控制权转移到 waking_in_the_hut。
3.	进入 waking_in_the_hut：
	*	最后，代码跳转到 waking_in_the_hut，这里的描述告诉读者：“你站起身来，准备继续你的旅程。”这完成了这段代码的流程。

总结：这种结构的目的是让 generic_sleep 结点可以根据传入的参数跳转到不同的“醒来”位置，使其能够在不同场景中复用，增强代码的灵活性。

译者注：这段说人话的意思就是，临时使用上方结点给出的转向参数替换下方的转向来做到临时接入不同的结点。

……请注意 `generic_sleep` 定义中的 `->`：这是 **Ink** 中唯一一个需要将参数类型化的情况，因为否则很容易犯如下错误：

	=== sleeping_in_hut ===
		你躺下并闭上了眼睛。
		-> generic_sleep (waking_in_the_hut)

……这将会让 waking_in_the_hut 的读取计数传递到 sleeping 结点，然后试图转向跳转到它。

## 5) 函数｜Functions

在结点上使用参数会使他们几乎等同于通常意义下的函数，但是它们缺少一个关键概念——调用栈和返回值。

**Ink** 包含了这样的功能：它们是结点，但是具有以下的限制和特性：

一个函数：
- 不能包含针脚 (Stitchs)
- 不能使用转向或提供选择
- 可以调用其他函数
- 可以包含已打印输出的内容
- 可以返回任何类型的值
- 可以安全地递归

（这些限制看起来或许有些严格，所以如果需要更多面向故事的调用栈风格的功能，请查看[隧道](#1-隧道tunnels)部分。）

返回值通过 `~ return` 语句提供。

### 定义和调用函数｜Defining and calling functions

要定义一个函数，只需要将一个结点声明为函数即可：

	=== function say_yes_to_everything ===
		~ return true

	=== function lerp(a, b, k) ===
		~ return ((b - a) * k) + a

译者注：就像上面这样，以 "function" 开头并空一格写上函数名就可以了。

函数通过名称和括号调用，哪怕它们并没有参数：

	~ x = lerp(2, 8, 0.3)

	*	{say_yes_to_everything()} 'Yes.'

与其他编程语言蕾丝，一个函数再一次执行完毕后，要将流程返回到调用它的位置——尽管函数不能进行转向，但是仍然函数仍然可以调用其它函数。

	=== function say_no_to_nothing ===
		~ return say_yes_to_everything()

### 函数不一定非要有个返回值｜Functions don't have to return anything

一个函数不一定需要一个返回值，可以让函数仅仅只是执行一些操作：

	=== function harm(x) ===
		{ stamina < x:
			~ stamina = 0
		- else:
			~ stamina = stamina - x
		}

……要记得函数是不能进行转向的，所以上面这些代码虽然可以防止耐力值 (Stamina) 变为负数，但是不会让耐力归零的玩家死亡。

### 函数可以直接在同一行内被调用｜Functions can be called inline

函数不仅可以在 `~` 行内调用，还可以在内容中直接调用。在这种情况下，如果函数有返回值，那么这个返回值就回被打印输出（当然也有可能输出其他内容。）如果没有任何返回值，那么就不会打印输出任何内容。

默认情况下，内容是“胶合”在一起的，所以以下代码：

	福格先生看起来{describe_health(health)}。

	=== function describe_health(x) ===
	{
	- x == 100:
		~ return "轻松愉快"
	- x > 75:
		~ return "略显疲惫"
	- x > 45:
		~ return "有些颓丧"
	- else:
		~ return "神情恍惚"
	}

会输出：

	福格先生看起来精神恍惚。

#### Examples

举个实例，您可以写这样的东西：

	=== function max(a,b) ===
		{ a < b:
			~ return b
		- else:
			~ return a
		}

	=== function exp(x, e) ===
		// 返回 x 的 e 次幂，其中 e 是整数
		{ e <= 0:
			~ return 1
		- else:
			~ return x * exp(x, e - 1)
		}

然后：

	2^5 和 3^3 中的最大值是 {max(exp(2,5), exp(3,3))}.
输出：

	2^5 和 3^3 中的最大值是 32。


#### 示例：将数字转化为文字｜Example: turning numbers into words

一下示例虽然较长，但几乎可以出现在每个 Inkle 游戏中。（请记得，带有连字符的行出现在多行大括号中时，表示为“要测试的条件”；如果大括号以变量开头，则表示“要比较的值”。）

    === function print_num(x) ===
    {
        - x >= 1000:
            {print_num(x / 1000)} 一千 { x mod 1000 > 0:{print_num(x mod 1000)}}
        - x >= 100:
            {print_num(x / 100)} 一百 { x mod 100 > 0:and {print_num(x mod 100)}}
        - x == 0:
            零
        - else:
            { x >= 20:
                { x / 10:
                    - 2: 二十
                    - 3: 三十
                    - 4: 四十
                    - 5: 五十
                    - 6: 六十
                    - 7: 七十
                    - 8: 八十
                    - 9: 九十
                }
                { x mod 10 > 0:<>-<>}
            }
            { x < 10 || x > 20:
                { x mod 10:
                    - 1: 一
                    - 2: 二
                    - 3: 三
                    - 4: 四
                    - 5: 五
                    - 6: 六
                    - 7: 七
                    - 8: 八
                    - 9: 九
                }
            - else:
                { x:
                    - 10: 十
                    - 11: 十一
                    - 12: 十二
                    - 13: 十三
                    - 14: 十四
                    - 15: 十五
                    - 16: 十六
                    - 17: 十七
                    - 18: 十八
                    - 19: 十九
                }
            }
    }

有了上面的函数，咱们就可以使用这样的功能：

	~ price = 15

	我从口袋里掏出{print_num(price)}枚硬币，慢慢地数着。
	“哦，算了，”商人回答道，“我只要一半。”然后她拿走了{print_num(price / 2)}枚，把剩下的硬币推回给我。

### 参数可以通过引用来传递｜Parameters can be passed by reference

函数的参数也可以通过“引用”来传递，这意味着函数可以直接修改被传入的变量，而不是创建一个临时变量来保存该值。

举个例子，大部分的 **Inkle** 故事都可以包含：

	=== function alter(ref x, k) ===
		~ x = x + k

那么像这样的行：

	~ gold = gold + 7
	~ health = health - 4

就可以写成：

	~ alter(gold, 7)
	~ alter(health, -4)

这种写法可以增加易读性，并且（更实用的是）它们可以在一行内就完成，从而实现更紧凑的代码。

	*	我吃了一块饼干[]之后，觉得精神焕发。{alter(health, 2)}
	*	我给了福格先生一块饼干[]，他一口吞了下去，一点也不优雅。{alter(foggs_health, 1)}
	-	<> 然后，我们继续赶路了。

将简单的操作封装进函数还有一个方便的好处，就是可以在需要的时候加入调试信息。

##  6) 常量｜Constants

### 全局常量｜Global Constants

交互式故事通常依赖于状态指示器来跟踪某些高级流程所处的阶段。有很多方法可以实现这一点，但最方便的方法是使用常量。

有时，将常量定义为字符串是很方便的，因为这样可以将它们打印出来，用于游戏展示或调试的目的。

	CONST HASTINGS = "黑斯廷斯"
	CONST POIROT = "波洛"
	CONST JAPP = "贾普"

	VAR current_chief_suspect = HASTINGS

	=== review_evidence ===
		{ found_japps_bloodied_glove:
			~ current_chief_suspect = POIROT
		}
		当前的怀疑对象：{current_chief_suspect}

有时候，为一些常量赋值也很实用：

	CONST PI = 3.14
	CONST VALUE_OF_TEN_POUND_NOTE = 10

有时，数字常量还可以用在其他地方，下面的例子就是用数字来代替位置：

	CONST LOBBY = 1
	CONST STAIRCASE = 2
	CONST HALLWAY = 3

	CONST HELD_BY_AGENT = -1

	VAR secret_agent_location = LOBBY
	VAR suitcase_location = HALLWAY

	=== report_progress ===
	{
        -  secret_agent_location == suitcase_location:
		特工抓住了手提箱！
		~ suitcase_location = HELD_BY_AGENT

	-  secret_agent_location < suitcase_location:
		特工向前走去。
		~ secret_agent_location++
	}

上面这个例子中，常量只是为了给故事的状态赋予易于理解的名称。

## 7) 进阶：游戏端逻辑｜Advanced: Game-side logic

在 Ink 引擎中提供游戏钩子有两种核心方法：
*	外部函数声明：在 Ink 中可以声明外部函数，允许你直接调用游戏中的 C# 函数。
*	变量观察器：当 Ink 中的变量被修改时，触发游戏中的回调函数。

这两种方法的详细描述见 [Running your ink](RunningYourInk.md).

# 第 4 部分：进阶流程控制｜Part 4: Advanced Flow Control

## 1) 隧道｜Tunnels

**Ink** 的默认结构是一颗“扁平”的选择树，分叉、合并、或者循环……但是故事始终处于“某个位置”。

这种扁平的结构有时会让某些情景变得复杂：
举个例子，设想一个游戏中可能会出现一下互动：

	=== crossing_the_date_line ===
	* “先生！”[] 我惊呼，“我刚刚意识到，咱们已经穿越了国际日期变更线！”
	- 福格先生只是微微抬了一下眉毛。“我已经考虑到了。”
	* 我擦了擦额头上的冷汗[]，顿时松了一口气！
	* 我点了点头，心情平静下来[]。他当然已经准备好了！
	* 我低声咒骂了一句[]。我又一次被轻视了！

但这个交互可能发生在故事的不同位置。我们不希望为每个位置都重复写一份相同的内容。但在内容结束时，程序需要知道返回到哪里。我们可以通过参数来实现这一点：

	=== crossing_the_date_line(-> return_to) ===
	...
	- -> return_to

	...

	=== outside_honolulu ===
	我们到达了檀香山这座大岛。
	- (postscript)
		-> crossing_the_date_line(-> done)
	- (done)
		-> END

	...

	=== outside_pitcairn_island ===
	船沿着水面驶向那个小小的皮特凯恩岛。
	- (postscript)
		-> crossing_the_date_line(-> done)
	- (done)
		-> END

现在，这两个位置都调用并执行了相同的一段故事流程，但在完成后，它们会返回到各自需要前往的下一步。

然而，如果被调用的故事段更加复杂——比如它跨越了多个结点 (knots) 怎么办？按照上述方法，我们不得不在结点之间不断传递“返回位置”的参数，以确保每次都知道返回到哪里。

译者注：以上的示例是表示，如果不使用“隧道”的写法，要怎么在转向到另一个地方并执行完毕后转向回原来的位置。接下来才要说 **Ink** 为这个操作提供的“隧道”语法。

为了解决这一问题，**Ink** 将这一功能集成到了语言本身，提供了一种新类型的转向 (Divert)，其功能类似于子流程，被称为“隧道(Tunnel)”。

### 隧道运行子故事｜Tunnels run sub-stories

隧道的语法看起来就像是一个转向，只是在分到的最后再另一个转向：

	-> crossing_the_date_line ->

上面这个就表示“执行 crossing_the_date_line 的内容，然后从这里继续”。

在隧道内部，其语法相比参数化的示例更加简化：我们只需使用 `->->` 声明来结束隧道。这句话的意思基本上是“继续”。

	=== crossing_the_date_line ===
	// 这是一个隧道！	
	...
	- 	->->

请注意，隧道结点并不以特殊的方式声明，因此编译器并不会在编译时检查隧道是否确实以 `->->` 语句结束，这种检查只会在运行时进行。因此，你需要仔细检查，以确保所有进入了隧道的流程都能再正确的返回出来。

隧道可以串联在一起，也可以使用普通转向结束：

	...
	// 运行隧道后跳转到 'done'
	-> crossing_the_date_line -> done
	...

	...
	// 运行一个隧道，然后运行另一个隧道，最后跳转到 'done'
	-> crossing_the_date_line -> check_foggs_health -> done
	...

隧道可以嵌套使用，所以下面的例子也是支持的。

	=== plains ===
	= night_time
		你脚下黑色的草地非常柔软。
		+	[Sleep]
			-> sleep_here -> wake_here -> day_time
	= day_time
		是时候动身了。

	=== wake_here ===
		太阳升起，你醒来了。
		+	[吃点什么]
			-> eat_something ->
		+	[出发]
		-	->->

	=== sleep_here ===
		你躺下来，试图闭上眼睛。
		-> monster_attacks ->
		是时候睡觉了。
		-> dream ->
		->->

……大概就是这样。

#### 进阶：隧道可以返回到其它位置｜Advanced: Tunnels can return elsewhere

有时，在故事中，事情可能不会像是预期一样发生。所以有时候隧道也无法保证它总是能返回到它之前的位置。所以为了解决这种情况，**Ink** 提供了一种语法，允许你“从隧道返回，但实际上去往其它的地方。”不过这种功能应当谨慎使用，毕竟这很容易导致逻辑混乱。

当然，在某些情况下，这种灵活性是必不可少的。

	=== fall_down_cliff 
	-> hurt(5) -> 
	你还活着！你站了起来继续前进。
	
	=== hurt(x)
		~ stamina -= x 
		{ stamina <= 0:
			->-> youre_dead
		}
	
	=== youre_dead
	突然，周围一片白光。有人伸手摘下你额头上的目镜。‘你输了，伙计。离开椅子吧。

即使故事情节没有生死攸关的紧张感，我们也可以通过灵活的跳转机制来调整叙事的流程结构：
 
	-> talk_to_jim ->
 
	 === talk_to_jim
	 - (opts) 	
		*	[询问关于超空间装置的事] 
			-> warp_lacells ->
		*	[询问关于护盾发生器的事] 
			-> shield_generators ->	
		* 	[停止交谈]
			->->
	 - -> opts 

	 = warp_lacells
		{ shield_generators : ->-> argue }
		“别担心超空间装置，它们没问题。”
		->->

	 = shield_generators
		{ warp_lacells : ->-> argue }
		“忘了护盾发生器吧，它们一切正常。”
		->->
	 
	 = argue 
		“问这么多问题干什么？”吉姆突然质问道。
		...
	 	->->

译者注：上面的这个例子会在问完一个问题要问另一个的时候进入 'argue'

#### 进阶：隧道是使用调用栈的｜Advanced: Tunnels use a call-stack

隧道是基于调用栈的，因此可以安全地递归调用。

## 2) 缝合线｜Threads

到目前为止，尽管 **Ink** 中有大量分支和跳转，但一切都是线性的。然而，作者实际上可以将故事“分叉”为不同的子部分，以涵盖更多可能的玩家行为。

我们称这种机制为“缝合线 (Thread)”，尽管它并不完全符合计算机科学中“线程（也是 Thread）”的定义：因为这更像是从不同地方“缝合线”新内容到当前故事中。

需要注意的是，这是一个高级功能：一旦涉及缝合线，故事的设计会变得更加复杂！

### 缝合线把多个部分合并到一起｜Threads join multiple sections together

缝合线操作允许你将多个来源的内容一次性组合成一个部分，例如：

    == thread_example ==
	我有点头疼；缝合线操作实在是有点难以理解。
    <- conversation
    <- walking

    == conversation ==
	对于蒙蒂和我来说，这真是一个紧张的时刻。
    *	“你今天午餐吃了什么？”[]我问道。
		“午餐肉和鸡蛋，”他回答。
    *	“天气不错啊，”[] 我说道。
		“见过更好的，”他回答。
    -	-> house

    == walking ==
	我们继续沿着尘土飞扬的道路走。
    *	[继续走]
    	-> house

    == house ==
	不久后，我们到达了他的房子。
	-> END

这就让故事的多个部分组合到一起成了一个独立的部分：

	我有点头疼；缝合线操作实在是有点难以理解。
	对于蒙蒂和我来说，这真是一个紧张的时刻。
	我们继续沿着尘土飞扬的道路走。
    1: “你今天午餐吃了什么？”
    2: “天气不错啊，”
    3: 继续走

当遇到类似 `<- conversation` 这样的缝合线语句时，编译器就会将故事流分叉过来。首个缝口 (fork) 将运行 `conversation` 中的内容，并收集其中的所有选项。一旦该缝口 (fork) 的内容结束，编译器将继续运行其他缝口 (fork) 。

所有的内容都会收集并展示给玩家。但当玩家选择之后，引擎就会跳转到那个分叉后折叠并丢弃其它分叉。

另外需要注意的是，全局变量*不会*被分叉，包括结点和针脚的读取计数。

### 缝合线的用法｜Uses of threads

在常规故事中，可能永远不需要用到缝合线。

但对于拥有大量独立移动部件的游戏，缝合线很快会变得不可或缺。想象一个角色在地图上独立移动的游戏：某个房间的主故事结点可能如下所示：

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
		*	[抽屉]	-> examine_drawers
		* 	[衣柜] -> examine_wardrobe
		*  [前往办公室] 	-> go_office
		-	-> run_player_location
	= examine_drawers
		// 等等……

	// 这里是缝合线，它会混入当前同房间角色的对话

	== characters_present(room)
		{ generals_location == room:
			<- general_conversation
		}
		{ doctors_location == room:
			<- doctor_conversation
		}
		-> DONE

	== general_conversation
		*	[询问将军关于带血的刀]
			“这事可不简单，我告诉你。”
		-	-> run_player_location

	== doctor_conversation
		*	[询问医生关于带血的刀]
			“血迹有什么好奇怪的？”
		-	-> run_player_location

特别要注意：我们需要明确的方法让进入旁缝合线的玩家返回主流程。大多数情况下，缝合线要么需要将参数告知返回位置，要么需要直接结束当前故事段落。

### 何时结束一个旁缝合线？｜When does a side-thread end?

当旁缝合线无流程可处理时便会结束：需注意，它们会暂存选项稍后显示（这与隧道不同，隧道会收集选项、立即显示并持续跟进，直到遇到明确的返回指令，该过程可能跨越多个步骤）。

有时，一个缝合线没有内容可提供——也许是与某个角色的对话已经结束，也许是我们还没有写完。在这种情况下，我们必须明确标记缝合线的结束。

如果我们不这样做，内容的结尾可能是一个故事漏洞或悬而未决的故事缝合线，我们希望编译器能告诉我们这些情况。

### 使用 `-> DONE`｜Using `-> DONE`

当需要显式标记缝合线已终结时，使用 `-> DONE` 指令：意为"流程在此处主动终止"。若未标记，可能触发警告——游戏虽可继续运行，但会提醒存在未闭合的的叙事单元。

本节开头的示例会生成警告，修正方式如下：

    == thread_example ==
    I had a headache; threading is hard to get your head around.
    <- conversation
    <- walking
    -> DONE

此处添加的 `DONE` 会告知 ink 引擎：当前流程已终结，后续故事应依赖其他部分推进。

请注意：若流程因条件分支未满足而自然终止，则无需 `-> DONE`。引擎会将其视为合规的流程终止状态。

**当玩家做出选择后，无需再使用`-> DONE`**。因为一旦选项被选定，该旁缝合线则立即脱离跳转出了缝合线，重新融入主线叙事流。

**在此场景中使用`-> END`不会终止当前缝合线，而是会直接终结整个叙事流**（这也正是我们需要两种不同流程终止方式的根本原因）。


#### 示例：在多个位置添加相同选项｜Example: adding the same choice to several places

缝合线可用于在多个不同位置复用相同的选项。这种用法通常需要传入转向作为参数，以指定选项执行完毕后故事应跳转的位置。

	=== outside_the_house
	门前台阶。屋子里飘出混杂薰衣草香气的凶案气息。
	- (top)
		<- review_case_notes(-> top)
		*	[进入正门]
			我迈步走进屋内。
			-> the_hallway
		* 	[嗅闻空气]
			我讨厌薰衣草。它让我想起肥皂，而肥皂让我想起我的婚姻。
			-> top

	=== the_hallway
	门厅。正门通向街道，角落摆着小柜子。
	- (top)
		<- review_case_notes(-> top)
		*	[走出正门]
			我踏入门外凉爽的阳光中。
			-> outside_the_house
		* 	[打开柜子]
			钥匙。更多的钥匙。甚至还有钥匙。这家人到底需要多少把锁？
			-> top

	=== review_case_notes(-> go_back_to)
	+	{not done || TURNS_SINCE(-> done) > 10}
		[查阅案件笔记]
		// 使用条件判断以确保不会频繁出现该选项
	 	{我|又一次，我} 快速翻看目前的调查记录。依然没有明显嫌疑人。
	- 	(done) -> go_back_to

需注意这与隧道的区别：隧道会执行相同内容块但不提供玩家选择权。例如以下两种写法效果相同：

	<- childhood_memories(-> next)
	*	[望向窗外]
	 	车轮滚动中，我陷入恍惚……
	 - (next) 直到汽笛声响起……

大致上是等价于：

	*	[回忆童年]
		-> think_back ->
	*	[望向窗外]
		车轮滚动中，我陷入恍惚……
	- 	(next) T直到汽笛声响起……

不过，当需要复用的选项包含多重选择、条件分支逻辑（或任何文本内容！）时，缝合线方案就会显示出其真正的优势。

#### 示例：大规模选项的组织管理｜Example: organisation of wide choice points

当游戏将 ink 脚本作为底层逻辑而非直接输出时，常会遇到需要生成大量并行选项的情况——这些选项通常需要通过游戏内交互（如环境探索）进行筛选。此时，缝合线就能有效发挥模块化分割的作用。

```
=== the_kitchen
- (top)
	<- drawers(-> top)
	<- cupboards(-> top)
	<- room_exits
= drawers (-> goback)
	// 抽屉相关选项……
	...
= cupboards(-> goback)
	// 有关橱柜的选项
	...
= room_exits
	// 出口；不需要"返回点"，因为离开就意味着前往其他地方
	...
```

# 第 5 部分：进阶状态追踪｜Part 5: Advanced State Tracking

交互密集的游戏会迅速变得异常复杂，作者的工作不仅关乎内容创作，同样需要维护叙事连贯性。


当游戏文本需要为任何事物建模时，这一点都尤为重要——无论是卡牌游戏规则、玩家当前对游戏世界的认知，还是房屋内各类电灯开关的状态。

**Ink** 并未像传统交互小说创作语言那样提供完整的世界建模系统——这里既没有"对象"概念，也不支持"容器关系"或"开启与锁定"状态。但它通过一套简洁而强大的系统，以高度灵活的方式追踪状态变化，使作者能在必要时构建近似的世界模型。

#### 注意：新功能提醒！｜Note: New feature alert!

该功能是语言中的全新特性。这意味着我们尚未发掘其所有可能的用途——但我们非常确定它将会很有用！如果您想到了任何巧妙的用法，我们很乐意听取您的建议！

## 1) 基础列表｜Basic Lists

状态追踪的基本单位是状态列表，使用 LIST 关键字定义。请注意，此列表与 C# 中的列表（即数组）完全不同。

举个例子，假定：

	LIST kettleState = cold, boiling, recently_boiled

这行代码定义了两项内容：首先是三个新状态值——`cold`（冷）、`boiling`（沸腾）和 `recently_boiled`（刚煮沸）——其次是一个名为`kettleState`的变量，用于存储这些状态。

然后，我们可以指定列表的初始值：

	~ kettleState = cold

可以改变状态的值：

	*	[打开水壶]
		水壶开始冒泡沸腾。
		~ kettleState = boiling

可以查询当前状态：

	*	[触摸水壶]
		{ kettleState == cold:
			水壶摸起来凉凉的。
		- else:
		 	水壶外壁非常烫！
		}

为方便起见，可以在一开始定义列表时就用括号指定初始值：

	LIST kettleState = cold, (boiling), recently_boiled
	// 游戏开始时，这个水壶就是开着的，嘻嘻。

……如果这种语法看起来有点多余，我们将在后续小节解释原因。

## 2) 复用列表｜Reusing Lists

上述水壶的例子已经足够，但如果炉子上还有个锅呢？所以我们可以先定义一个状态列表，然后将其赋值给任意数量的变量。

	LIST daysOfTheWeek = Monday, Tuesday, Wednesday, Thursday, Friday
	VAR today = Monday
	VAR tomorrow = Tuesday

译者注：总结来说就是用这个语法一次性创建多个变量，并使用一个带名字的容器给这批内容装起来。

### 状态是可以被重复使用的｜States can be used repeatedly

这样我们就可以在多个地方复用同一个状态机器。

	LIST heatedWaterStates = cold, boiling, recently_boiled	// 创建水的状态列表：冷的、沸腾、刚煮沸
	VAR kettleState = cold	//	水壶是冷的
	VAR potState = cold	// 锅是冷的

	*	{kettleState == cold} [打开水壶]	//	如果水壶是冷的
		水壶开始沸腾冒泡。
		~ kettleState = boiling	// 将水壶设为沸腾
	*	{potState == cold} [点燃炉灶]
	 	锅里的水开始沸腾冒泡。
	 	~ potState = boiling	//	将锅设为沸腾

但如果再加个微波炉呢？那我们可能需要稍微做点功能泛化：

	LIST heatedWaterStates = cold, boiling, recently_boiled	// 与上面的列表相同
	VAR kettleState = cold	//	水壶是冷的
	VAR potState = cold	// 锅是冷的
	VAR microwaveState = cold	//	微波炉也是冷的

	=== function boilSomething(ref thingToBoil, nameOfThing)	// 函数：煮沸某物（参数 要煮沸的物品， 物品名称）
		那个{nameOfThing}开始加热了。
		~ thingToBoil = boiling	// 设定要煮沸的物品状态为煮沸

	=== do_cooking	// 进行烹饪
	*	{kettleState == cold} [打开水壶]	// 水壶是冷的
		{boilSomething(kettleState, "kettle")}	// 调用上面煮沸某物的函数（水壶状态，水壶）
	*	{potState == cold} [点燃炉灶]	// 灶台是冷的
		{boilSomething(potState, "pot")}	// 调用上面的函数，由函数把它的状态改为“煮沸”
	*	{microwaveState == cold} [打开微波炉]	//	微波炉是冷的
		{boilSomething(microwaveState, "microwave")}	// 同理

甚至可以……
	LIST heatedWaterStates = cold, boiling, recently_boiled
	VAR kettleState = cold
	VAR potState = cold
	VAR microwaveState = cold

	//	上面还是那个列表和初始状态

	=== cook_with(nameOfThing, ref thingToBoil)	// 用某物煮沸（物品名称，参数 要煮的东西）
	+ 	{thingToBoil == cold} [打开{nameOfThing}]	// 某个要煮的东西是冷的，打开对应的物品名称
	  	那个{nameOfThing}开始加热了。	// 那个“物品名称”开始加热了。
		~ thingToBoil = boiling	// 把要煮的东西状态设定为沸腾
		-> do_cooking.done	//	转到 do_cooking 结点中的 done 针脚

	=== do_cooking	// 烹饪（上面要煮的东西与器皿的对应关系在这里）
	<- cook_with("kettle", kettleState)	//
	<- cook_with("pot", potState)
	<- cook_with("microwave", microwaveState)
	- (done)

注意："加热水状态"这个列表仍然可用，仍然可以被检测和赋值。

#### 列表的值可以共享名称｜List values can share names

复用列表会带来命名歧义。如果我们有：

	LIST colours = red, green, blue, purple	// 列表 颜色：红、绿、蓝、紫
	LIST moods = mad, happy, blue	// 列表 情绪：愤怒、开心、忧郁
	//	译者注：英语里，blue 可以同时翻译为“蓝色”或“忧郁”，就和中文中的一词多义一样。

	VAR status = blue	// 设定 状态：blue

……那编译器怎么知道您指的是哪个列表中的 blue？

所以我们通过使用类似结点和针脚中用到的 `.` 语法来结局这个问题。

	VAR status = colours.blue

……编译器会明确要求您指明您在使用哪个列表中的哪个状态，不然就会一直报错。

注意：状态组的"家族名"（合集名称）与包含状态的变量是完全独立的。因此

	{ statesOfGrace == statesOfGrace.fallen:	
		// 检查 恩典状态 是否为：恩典状态.堕落
	}

……它也是合规的。

#### 进阶：LIST 本质上是一个变量｜Advanced: a LIST is actually a variable

有个令人惊讶的特性是

	LIST statesOfGrace = ambiguous, saintly, fallen

这个语句实际上同时完成了两件事：它创建了三个值，`ambiguous`, `saintly` 和 `fallen`，然后声明了一个名为 `statesOfGrace` 的普通变量

这意味着这个变量可以像普通变量一样被重新赋值。所以以下写法极易造成混淆且不推荐，但语法上是合规的：

	LIST statesOfGrace = ambiguous, saintly, fallen

	~ statesOfGrace = 3.1415 // 将变量赋值为了一个数字而不是列表中的某个值

……但这并不影响以下用法的正确性：

	~ temp anotherStateOfGrace = statesOfGrace.saintly




## 3) 值的顺序｜List Values

当定义一个列表时，列出的值必然是有顺序的，这个顺序也是有意义的。实际上，我们可以把这些值当作数字来处理（也就是说，它们本质上是枚举类型）。

	LIST volumeLevel = off, quiet, medium, loud, deafening	// 创建一个“音量级别”的列表，列表里有“关闭”、“安静”、“中等”、“响亮”、“震耳欲聋”
	VAR lecturersVolume = quiet	//	创建一个“讲师音量”的变量，并设定为“安静”
	VAR murmurersVolume = quiet	//	创建一个“窃窃私语音量”的变量，并设定为“安静”

	{ lecturersVolume < deafening:	// 如果“讲师音量”小于“震耳欲聋”
		~ lecturersVolume++	// 那就就提高一级“讲师音量”

		{ lecturersVolume > murmurersVolume:	// 如果“讲师音量”大于“窃窃私语音量”
			~ murmurersVolume++	// 那么提高一级“窃窃私语音量”
			窃窃私语声变得更大了。
		}
	}

这些值本身可以通过常规的 `{某种判定条件}` 语法输出，但将直接显示其名称。

	讲师的声音变得{lecturersVolume}。

### 将值转换为数字｜Converting values to numbers

如需获取数值，可使用 LIST_VALUE 函数显式转换。但请注意，列表中第一个值的数值会记录为 1（而非 0）。

	讲师还有{LIST_VALUE(deafening) - LIST_VALUE(lecturersVolume)}档音量可以调。

### 将数字转换为值｜Converting numbers to values

您可以通过将列表名称作为函数来使用以进行反向转换：

	LIST Numbers = one, two, three	// 创建一个名为“数字”的列表，里面有“一”、“二”、“三”	
	VAR score = one	//	创建一个名叫“得分”的变量
	~ score = Numbers(2) // 设定“得分”为“数字”列表中的第2个值，这样之后，“得分”的值就会是”二“。

### 高级：自定义数值映射｜Advanced: defining your own numerical values

默认情况下，列表中的值从1开始依次递增，但您也可以根据需要指定自定义数值。

	LIST primeNumbers = two = 2, three = 3, five = 5	// 创建一个”质数“列表，并使得“二”的顺序为 2，“三”的顺序为 3，但“五”的顺序为 5。

如果为某个值指定了数值但未指定下一个值的数值，ink 将默认按上一个值继续递增1号。因此以下定义和上方的例子是等效的：

	LIST primeNumbers = two = 2, three, five = 5	// 这其中，“三”没有被手动制定为第 3 个，但是由于前一个被指定为第 2 个，所以这里自动递增 1 号即为 3。

## 4) 多值列表｜Multivalued Lists

以下示例均包含一处刻意添加的不实信息，我们现在现予以修正。列表（以及包含列表值的变量）并非只能存储单一值。

### 列表的本质是布尔集合｜Lists are boolean sets

列表变量不是包含数字的变量。实际上，列表就像宿舍楼里的出入登记板。它包含一系列名字，每个名字都关联着一个房间号，并通过一个只有开或关状态的滑块来标记"有人"或"外出"状态。

可能没任何人在：

	LIST DoctorsInSurgery = Adams, Bernard, Cartwright, Denver, Eamonn	// “外科医生”列表，那些值都是人名

可能所有人都在：

	LIST DoctorsInSurgery = (Adams), (Bernard), (Cartwright), (Denver), (Eamonn)

或者可能有些人在有些人不在：

	LIST DoctorsInSurgery = (Adams), Bernard, (Cartwright), Denver, Eamonn

括号中的名字会被包含在列表初始状态。
（译者注：简单来说，加了括号就是在列表初始化的时候一定在列表。加了括号就是创建了这么个值，但是一开始并不在列表中。）

注意：当你自定义数值时，你可以用括号来包裹整个条目或仅包裹名称：

LIST primeNumbers = (two = 2), (three) = 3, (five = 5)	// “质数”列表

#### 批量赋值｜Assiging multiple values

我们可以一次性为列表赋值多个值：

	~ DoctorsInSurgery = (Adams, Bernard)
	~ DoctorsInSurgery = (Adams, Bernard, Eamonn)

也可以赋个空值来清空列表：

	~ DoctorsInSurgery = ()


#### 添加或删除条目｜Adding and removing entries
列表的条目可以单独或批量地添加或移除。

	~ DoctorsInSurgery = DoctorsInSurgery + Adams
 	~ DoctorsInSurgery += Adams  // 这与上一行是等效的。
	~ DoctorsInSurgery -= Eamonn
	~ DoctorsInSurgery += (Eamonn, Denver)
	~ DoctorsInSurgery -= (Adams, Eamonn, Denver)

尝试添加已存在的条目不会产生任何效果。尝试移除不存在的条目也不会有任何效果。但是前面提到的这两种操作也不会报错，列表永远不会包含重读的条目。


### 基本查询｜Basic Queries

我们有以下几种基本方式来获取列表信息：

	LIST DoctorsInSurgery = (Adams), Bernard, (Cartwright), Denver, Eamonn	// “外科医生”列表，可能有些人在有些人不在。

	{LIST_COUNT(DoctorsInSurgery)} 	//  查询现在列表中有几个值："2"
	{LIST_MIN(DoctorsInSurgery)} 		//  查询排序值最小的是谁："Adams"
	{LIST_MAX(DoctorsInSurgery)} 		//  查询排序值最大的是谁："Cartwright"
	{LIST_RANDOM(DoctorsInSurgery)} 	//  随机一个值出来："Adams" 或 "Cartwright"

#### 空值检测｜Testing for emptiness

和 Ink 中的大多数值一样，列表可以直接作为条件测试，非空时会返回 true。

	{DoctorsInSurgery: 今日门诊开放。 | 所有人都回家了。}	// 如果“外科医生”列表不为空则显示前半句，空则显示后半句。

#### 精确相等性测试｜Testing for exact equality

测试多值列表比单值列表稍微复杂些。想等运算符（`==`）在这种情况下表示“集合相等”——也就是说，所有的条目必须完全相同。

所以我们可以这样：

	{ DoctorsInSurgery == (Adams, Bernard):
		亚当斯医生和伯纳德医生正在角落里大声争吵。
	}

如果埃蒙 (Eamonn) 医生也在场，两人就不会争吵，因为比较的两个列表不相等——DoctorsInSurgery 列表中有埃蒙，而 (Adams, Bernard) 列表中没有。

下面是按照预期运行的不等运算符：

	{ DoctorsInSurgery != (Adams, Bernard):	// 如果“外科医生”列表不等于“Adams, Bernard”
		至少亚当斯和伯纳德没在争吵。
	}

#### 包含性测试｜Testing for containment

那如果我们只是想知道亚当斯和伯纳德医当时是否在场呢？这个时候我们就可以使用新的运算符 `has`，也可以写作 `?`。

	{ DoctorsInSurgery ? (Adams, Bernard):
		Dr Adams and Dr Bernard are having a hushed argument in one corner.
	}

`?` 运算符同时适用于单值列表。

	{ DoctorsInSurgery has Eamonn:	// 如果“外科医生”列表包含 "Eamonn"
		埃蒙医生正在擦拭他的眼镜。
	}

我们也可以用否定形式，也就是 `hasnt` 或 `!?`（而不是 `?`），来查询列表是否不包含要查询的内容中的某一项。也就是说：

	DoctorsInSurgery !? (Adams, Bernard)	// 查询“外科医生”列表中是否不包含 "Adams" 或 "Bernard" 中的一个或更多。

这并不意味着亚当斯和伯纳德都不在场，仅表示他们不会同时在场（并发生争执）。

#### 注意：空列表不被任何列表包含｜Warning: no lists contain the empty list

所以如果你想要测试：

	SomeList ? ()	// 查询“某个列表”是否包含 “<什么也没有>”

无论 `SomeList` 本身的值是否为空，这个查询都会返回 false，这种设计在实际应用中最合理，比如这样的检测：

	SilverWeapons ? best_weapon_to_use	// 查询“银制武器”列表中是否有 best_weapon_to_use（最好的武器）
	
那如果“best_weapon_to_use”（最好的武器）是空的，则返回失败。

#### 示例：基础信息追踪｜Example: basic knowledge tracking

多值列表在游戏中最简单的用途就是整洁地追踪“游戏标记”：

	LIST Facts = (Fogg_is_fairly_odd), first_name_phileas, (Fogg_is_English)	
	// 创建“事实”列表，初始化“福格是个相当古怪的人（并留在列表中）”，“名叫菲利亚斯”，“福格是英国人”

	{Facts ? Fogg_is_fairly_odd:我礼貌地笑了笑。|他是个疯子吗？}
	//	查询 Fogg_is_fairly_odd 是否在“事实”列表中。

	“{Facts ? first_name_phileas:斐利亚斯|先生}，真的！”我喊道。
	// 查询 first_name_phileas 是否在“事实”列表中。

特别是，这个语法还允许我们在一行中就测试多个标志。

	{ Facts ? (Fogg_is_English, Fogg_is_fairly_odd):
		<> ”我知道英国人很奇怪，但这也太*不可思议*了！“
	}

#### 示例：医生门诊系统｜Example: a doctor's surgery

让我们来看一个完整示例：

	LIST DoctorsInSurgery = (Adams), Bernard, Cartwright, (Denver), Eamonn
	// 创建一个”外科医生“列表，初始化：亚当斯（留在列表）, 伯纳德, 卡特赖特, 丹佛（留在列表）, 埃蒙
	
	-> waiting_room

	=== function whos_in_today()
		今日坐诊医生有：{DoctorsInSurgery}。

	=== function doctorEnters(who)
		{ DoctorsInSurgery !? who:
			~ DoctorsInSurgery += who
			{who} 医生匆匆赶到诊室。
		}
		//	函数功能结点，当调用 doctorEnters(who) 时添加这个医生并输出”一个"{who} 医生匆匆赶到诊室。“

	=== function doctorLeaves(who)
		{ DoctorsInSurgery ? who:
			~ DoctorsInSurgery -= who
			{who} 医生外出午餐。
		}
		//	函数功能结点，当调用 doctorLeaves(who) 时移除这个医生并输出”一个"{who} 医生外出午餐。“

	=== waiting_room
		{whos_in_today()}
		*	[时间流逝……]
			{doctorLeaves(Adams)} {doctorEnters(Cartwright)} {doctorEnters(Eamonn)}
			{whos_in_today()}
			// 在这里为之前结点中的 "who" 赋值并调用函数。

这将会输出：

	今日坐诊医生有：Adams, Denver。

	> 时间流逝...

	Adams 医生外出午餐。Cartwright 医生匆匆赶到诊室。Eamonn 医生匆匆赶到诊室。

	今日坐诊医生有：Cartwright, Denver, Eamonn。

#### 进阶：优化列表显示｜Advanced: nicer list printing
基础的列表在游戏中可能实用，所以可以这样来优化一下：

	=== function listWithCommas(list, if_empty)
		// 功能函数 listWithCommas，预留 list 和 if_empty 变量

		{LIST_COUNT(list):
		//	判断列表中有几个值
		- 2:
			{LIST_MIN(list)} 和 {listWithCommas(list - LIST_MIN(list), if_empty)}
			//	如果有 2 个，那么就先说“<列表中已存在的值中排序值最小的那个值>和<列表中去掉最小排序值之后的新列表的值>”，然后递归再调用自身，直到这个满足终止条件（0、1 或 2 个）后调用 if_empty 的替代文本。
		- 1:
			{list}
			//	只有 1 个就直接报出恐龙名字
		- 0:
			{if_empty}
			//	没有（为 0 个）就显示 {if_empty} 的内容
		- else:
			{LIST_MIN(list)}、{listWithCommas(list - LIST_MIN(list), if_empty)}
			//	和上面的 2 是一样的，只是 2 一开始就达到了递归的终止条件，这里可能比 2 更多，所以输出的时候考虑了语言逻辑而进行了调整。
		}

	LIST favouriteDinosaurs = (stegosaurs), brachiosaur, (anklyosaurus), (pleiosaur)
	// 在这里创建了”我最喜欢的恐龙“列表，并初始化：<那些恐龙的名字>

	我最喜欢的恐龙是{listWithCommas(favouriteDinosaurs, "已全部灭绝")}。
	//	调用 listWithCommas 函数，并传递参数“我最喜欢的恐龙列表给“listWithCommas”，并将“已全部灭绝”这个替代文本代入 if_empty

再配上一个单复数判断函数：
（译者注：英语单数用 is，多个用 are，但是翻译过来都是“是”，就像汉语中的“一个”和“一群”的区别，请举一反三的应用）

	=== function isAre(list)
		{LIST_COUNT(list) == 1:是一个(is)|是一群(are)}

	最喜欢的恐龙{isAre(favouriteDinosaurs)}{listWithCommas(favouriteDinosaurs, "已全部灭绝")}。

再严谨一些的话（名次单复数的区别，举一反三地实用即可）：

	我最喜欢的恐龙{LIST_COUNT(favouriteDinosaurs) != 1:们}{isAre(favouriteDinosaurs)}{listWithCommas(favouriteDinosaurs, “已全部灭绝”)}。

	My favourite dinosaur{LIST_COUNT(favouriteDinosaurs) != 1:s} {isAre(favouriteDinosaurs)} {listWithCommas(favouriteDinosaurs,s “已全部灭绝”)}.


#### 列表不是必须包含多个值｜Lists don't need to have multiple entries

列表不一定需要包含多个值。如果要将列表用作状态机，上述所有示例仍然适用——你可以继续使用 `=`、`++` 和 `--` 设置值；使用 `==`、`<`、`<=`、`>` 和 `>=` 进行判定测试。这些操作都将按预期工作。

### “完整”的列表｜The "full" list

注意：`LIST_COUNT`，`LIST_MIN` 和 `LIST_MAX` 参考的是当前列表中的内容，而非所有有可能的医生名单。要访问完整列表，可以使用：

	LIST_ALL(<列表中的元素>)

或

	LIST_ALL(<包含列表元素的列表>)

	{LIST_ALL(DoctorsInSurgery)} // Adams, Bernard, Cartwright, Denver, Eamonn
	{LIST_COUNT(LIST_ALL(DoctorsInSurgery))} // "5"
	{LIST_MIN(LIST_ALL(Eamonn))} 				// "Adams"

请注意：使用 `{<要判定的内容>}` 打印列表会产生最基本的列表表示形式，即用逗号分隔的值。

#### 进阶：“重置”列表的类型｜Advanced: "refreshing" a list's type

如果您需要的话，您可以创建一个没有内容的空列表（这个列表只是一个类型为列表的空列表）。

	LIST ValueList = first_value, second_value, third_value
	// 整一个有三个值的 ValueList
	VAR myList = ()
	//	整一个名叫 myList 的变量，里面是空的。
	
	~ myList = ValueList()
	// 把 ValueList 的值填入 myList

这之后可以执行：

	{ LIST_ALL(myList) }
	//	 列出完整的 myList

#### 进阶：获取“完整”列表的子集｜Advanced: a portion of the "full" list

使用 `LIST_RANGE` 函数可以获取完整列表的特定区间，有两种等效写法：

	LIST_RANGE(list_name, min_integer_value, max_integer_value)
	// LIST_RANGE(想截取的列表名称，你想取得的那个区间的最小排序值的整数值，你想取得的那个区间的最大排序值的整数值)

或者
	{LIST_RANGE(LIST_ALL(primeNumbers), 10, 20)} 
	// LIST_RANGE(想截取的列表名称，你想取得的那个区间的最小排序值的整数值，你想取得的那个区间的最大排序值的整数值，你想取得的那个区间的最小值值本身，你想取得的那个区间的最大值值本身)
	
其中最小值和最大值都是包含的。如果找不到精确匹配值，系统会返回最接近但不超出范围的数值。例如：

	{LIST_RANGE(LIST_ALL(质数列表), 10, 20)} 
	//	从完整的质数列表中截取，最小截到 10，最大截到 20

将输出：

	11, 13, 17, 19



### 示例：汉诺塔｜Example: Tower of Hanoi

为了展示其中的一些想法，这里有一个功能性的汉诺塔示例，写这个示例的目的是为了让其他人不用再写了。
（译者注：汉诺塔是一种经典益智游戏，如果不清楚这是什么请用搜索引擎查一下，规则相当简单，这里不再赘述。）

	LIST Discs = 一, 二, 三, 四, 五, 六, 七	//	创建列表“圆盘”，并初始化一二三四五六七。
	VAR post1 = ()	// 创建变量“柱子1”
	VAR post2 = ()	// 创建变量“柱子2”
	VAR post3 = ()	// 创建变量“柱子3”

	~ post1 = LIST_ALL(Discs)	// 在 柱子1 上按列表 Discs（圆盘）的顺序放上所有的 7 个圆盘

	-> gameloop
	// 转向 游戏循环

	=== function can_move(from_list, to_list) ===
	// 功能函数 can_move（检查是否可以移动），等待参数 from_list（来源柱）和 to_list（目标柱）
	    {
	    -   LIST_COUNT(from_list) == 0:
		// 如果待会 来源柱 中的值为 0（也就是没有圆盘）
	        ~ return false
			// 返回 false
	    -   LIST_COUNT(to_list) > 0 && LIST_MIN(from_list) > LIST_MIN(to_list):
		// 如果待会 目标柱 同时大于 0 和 来源柱 中最小的排序值，而这两个值又大于 目标柱 中的最小排序值
		// 实际来说就是要移动的圆盘比目标柱最上面的圆盘大
	        ~ return false
			// 返回 false
	    -   else:
		//其他情况都可以移动！
	        ~ return true
			// 返回 true
	    }

	=== function move_ring( ref from, ref to ) ===
	// 功能函数 move_ring（移动圆盘），传参 来源，传参 目标
	    ~ temp whichRingToMove = LIST_MIN(from)	// 将 from（来源）列表中最小排序值代入临时变量 whichRingToMove（移动哪号圆盘）
	    ~ from -= whichRingToMove	// 从 来源 列表中移除 whichRingToMove
	    ~ to += whichRingToMove	// 在 目标 列表中增加 whichRingToMove


	== function getListForTower(towerNum)
	// 功能参数 获取柱子列表（等待传入柱子编号）
	    { towerNum:
	        - 1:    ~ return post1	// 是 1 就返回 柱子1
	        - 2:    ~ return post2	// 是 2 就……
	        - 3:    ~ return post3	// ……
	    }

	=== function name(postNum)
	// 功能函数 柱子名转换（小写），等待 postNum 参数
	    那{postToPlace(postNum)}座

	=== function Name(postNum)
	// 功能函数 柱子名转换（首字母大写），等待 postNum 参数
	    那{postToPlace(postNum)}座

	=== function postToPlace(postNum)
	// 功能函数 编号到次序转换
	    { postNum:
	        - 1: 第一
	        - 2: 第二
	        - 3: 第三
	    }

	=== function describe_pillar(listNum) ==
	// 功能函数 描述柱子，等待 listNum 参数
	    ~ temp list = getListForTower(listNum)
		//	将 获取柱子列表（柱子编号）填入 临时参数 list
	    {
	    - LIST_COUNT(list) == 0:	// 如果 list 最小排序值为 0
	        {Name(listNum)}是空的。
	    - LIST_COUNT(list) == 1:	// 如果 list 最小排序值为 1
	        只有{list}号圆盘在{name(listNum)}上。
	    - else:	// 其他情况
	        在{name(listNum)}上，摆放着{list}号圆盘。
	    }


	=== gameloop
	从天上俯瞰，你看到你的追随者们正在准备开始完成最后一座大神庙的建设。
	- (top)
	    +  [查看圣殿]
        	你依次检视每座圣殿。每座圣殿上都堆叠着石环。{describe_pillar(1)} {describe_pillar(2)} {describe_pillar(3)}
	    <- move_post(1, 2, post1, post2)
	    <- move_post(2, 1, post2, post1)
	    <- move_post(1, 3, post1, post3)
	    <- move_post(3, 1, post3, post1)
	    <- move_post(3, 2, post3, post2)
	    <- move_post(2, 3, post2, post3)
		// move_post 函数在下一针脚
	    -> DONE

	= move_post(from_post_num, to_post_num, ref from_post_list, ref to_post_list)
	// 移动柱子（并代入了对应的参数）
	    +   { can_move(from_post_list, to_post_list) }
	        [将圆盘从{name(from_post_num)}移动到{name(to_post_num)}。]
	        { move_ring(from_post_list, to_post_list) }
	        { stopping:	// 按顺序显示下方文本并停在最后一项
	        -	下方的祭司们建造了巨大的吊架，经过多年的努力，巨大的石环被吊起，缓缓移向下一座圣殿。
				绳索被斩断，转瞬间石环便稳稳落下。
	        -   你的谕令引发了盛大的庆典和祭祀。当葬仪的烟雾散去，移动石环的工程郑重展开。一代人成长又逝去，石环终于归位。
	        -   { cycle:	// 循环显示下方文本
	            - 石环在岁月流转中缓慢移动。
	            - 祭司们为袍服颜色爆发战争，虽死伤无数，工程却仍在继续。
	            }
	        }
	    -> top


## 5) 进阶列表操作｜Advanced List Operations

前文已涵盖基础的比较操作。除此之外还有一些更强大的功能，但正如熟悉数学集合的人所知——事情开始变得有些复杂了。因此本节内容标注为"进阶"警示。

本节的多数功能对大多数游戏开发并非必需。

### 比较列表｜Comparing lists 

我们可以使用 >、<、>= 和 <= 来比较列表大小。需要注意！这里使用的定义并不完全符合常见标准，它们是基于被比较列表中元素的数值来进行比较的。

#### “严格的大于”

`LIST_A > LIST_B` 的含义是：“A 中的最小值大于 B 中的最大值”。换句话说，如果放在数轴上，A 的全部内容都在 B 的全部内容的右侧。`<` 则是相反的比较。

#### “绝不会小于”

`LIST_A >= LIST_B` 的含义是——（请你做好心理准备……）——“A 中的最小值至少与 B 中的最小值相等，且 A 中的最大值至少与 B 中的最大值相等”。换句话说，如果画在数轴上，A 的整体要么在 B 之上，要么与 B 重叠，但 B 不会高于 A。

需要注意的是，`LIST_A > LIST_B` 意味着 `LIST_A != LIST_B`，而 `LIST_A >= LIST_B` 则允许 `LIST_A == LIST_B` 但会排除 `LIST_A < LIST_B`，这也许正如你所希望的那样。

#### 健康忠告！

`LIST_A >= LIST_B` 并*不*等同于 `LIST_A > LIST_B` 或 `LIST_A == LIST_B`。

这个道理是：为了您的脑细胞着想，除非你在脑中有非常清晰的理解，否则不要使用这些比较。

### 反转列表

列表可以被“反转”，就像是住宿登记处的进出名牌板，将每个开关都翻转成相反的状态。

	LIST GuardsOnDuty = (Smith), (Jones), Carter, Braithwaite

	=== function changingOfTheGuard
		~ GuardsOnDuty = LIST_INVERT(GuardsOnDuty)

请注意，如果对一个空列表使用 `LIST_INVERT`，而游戏又没有足够的上下文来确定到底要反转什么内容，那么它将返回空值。如果需要处理这种情况，最安全的做法是手动处理：

	=== function changingOfTheGuard
		{!GuardsOnDuty: // 查询 GuardsOnDuty 列表现在是不是空的
			~ GuardsOnDuty = LIST_ALL(Smith)
		- else:
			~ GuardsOnDuty = LIST_INVERT(GuardsOnDuty)
		}

#### 脚注

在 Ink 诞生之初时，反转的语法最初是 `~ list`，但后来更改了，否则以下这行

	~ list = ~ list

不仅可以正常运行，而且会真的让 list 自我反转，这看起来过于反常。

（译者注：这是一个已经不再可用的语法，图一乐看看就行。）

### 交集列表

`has` 或 `?` 运算符用通俗的语言来表述就是“你是我的子集吗”运算符，也就是“⊇”的意思，它包含集合相等的情况，但当大集合未完全包含小集合时则不成立。

若要检测两个列表是否“有交集”，可以使用重叠运算符 `^` 来获取*交集*。

	LIST CoreValues = strength, courage, compassion, greed, nepotism, self_belief, delusions_of_godhood
	VAR desiredValues = (strength, courage, compassion, self_belief )
	VAR actualValues =  ( greed, nepotism, self_belief, delusions_of_godhood )

	{desiredValues ^ actualValues} // 这会输出 "self_belief"

译者注：两个 LIST 之间似乎不能直接比较，他们这里也是把 LIST 里的值搞到了两个 VAR 里再进行比较的。

结果是一个新的列表，因此可以进行判定：

	{desiredValues ^ actualValues: 新总统至少有一个值得称道的品质。}	// 如果两个列表有交集。

	{LIST_COUNT(desiredValues ^ actualValues) == 1: 更正，新总统实际上只有一个值得称道的品质。{desiredValues ^ actualValues == self_belief: 而且是那个最可怕的品质。}}	// 如果两个列表只有一个交集就“吃了吐”。如果那个交集完全等于 self_belief 则输出……


## 6) 多列表列表（表中表）

到目前为止，我们在所有示例中都使用了一个简化假设：列表变量中的值必须全部来自同一个列表族。但其实并不需要。（译者注：这是说，前面的例子最多只创建了一个列表，但其实 LIST 并不是只能有一个）

这使得列表除了用作状态机和标志追踪器外，还可以用来作为通用属性，非常适合用于建模你的世界。

这就是我们的“盗梦空间”时刻（从这里开始进入更复杂、强大但真实的“嵌套”世界（嵌套状态、嵌套结构、嵌套复杂性）。这种结果非常强大，但也更接近“真正的代码”，比之前讲过的任何内容都更像。

### 用列表追踪物品

举个例子，我们可以定义：

	LIST Characters = Alfred, Batman, Robin	// 角色列表
	LIST Props = champagne_glass, newspaper	// 道具列表：香槟杯、报纸

	VAR BallroomContents = (Alfred, Batman, newspaper)
	VAR HallwayContents = (Robin, champagne_glass)

接着，我们可以通过状态检测来描述房间内的内容：

	=== function describe_room(roomState)
		{ roomState ? Alfred: 阿尔弗雷德正静静地站在角落里。}{ roomState ? Batman: 蝙蝠侠的存在让所有人都感到压迫。}{ roomState ? Robin: 罗宾几乎被遗忘。}
		<> { roomState ? champagne_glass: 地板上丢这一个香槟杯。}{ roomState ? newspaper: 桌子上的头条新闻用超大的字号写着“谁是蝙蝠侠？他那几乎被遗忘的助手又是*谁*？”}

那么：

	{ describe_room(BallroomContents) }

就会输出：

	阿尔弗雷德正静静地站在角落里。蝙蝠侠的存在让所有人都感到压迫。罗宾几乎被遗忘。
	
	桌子上的头条新闻用超大的字号写着“谁是蝙蝠侠？他那几乎被遗忘的助手又是谁？”

而：

	{ describe_room(HallwayContents) }

则会输出：

	Robin 几乎被遗忘。

	地板上丢弃着一个香槟杯。

我们还可以基于组合状态来提供选项：

	*	{ currentRoomState ? (Batman, Alfred) } [与阿尔弗雷德和蝙蝠侠对话]
		“嘿，你们两个互相认识吗？”

### 用列表追踪多重状态

我们还可以用它来建模具有多个状态的设备。回到水壶那个例子……

	LIST OnOff = on, off
	LIST HotCold = cold, warm, hot

	VAR kettleState = (off, cold) // 这回这里需要一个括号，因为他们现在是一个真正的多值列表了

	=== function turnOnKettle() ===
	{ kettleState ? hot:
		你打开水壶，但它立刻又跳闸关闭。
	- else:
		水壶里的水开始加热。
		~ kettleState -= off
		~ kettleState += on
		// 注意不要使用“=”赋值，而应该使用操作列表的办法，否则会移除所有已存在的状态，也就是直接覆盖掉了列表而替换成了一个值。
	}

	=== function can_make_tea() ===
		~ return kettleState ? (hot, off)

这种混合状态会让状态变更稍微复杂，如上面 on/off 的示例所示，因此以下辅助函数会很有用：

 	=== function changeStateTo(ref stateVariable, stateToReach)
 		~ stateVariable -= LIST_ALL(stateToReach)	// 移除此类别的所有状态
 		~ stateVariable += stateToReach	// 添加需要到达的那个状态
		// 译者注：相当于是先清除，表中所有状态，再重新给定对应状态，避免了手动开关导致遗漏了某些值的状态。

这样就可以写出如下代码：

 	~ changeState(kettleState, on)
 	~ changeState(kettleState, warm)


#### 这对查询有何影响？

上述查询基本可以自然地以此类推到多值列表上：

    LIST Letters = a,b,c
    LIST Numbers = one, two, three

    VAR mixedList = (a, three, c)

	{LIST_ALL(mixedList)}   // a, one, b, two, c, three
    {LIST_COUNT(mixedList)} // 3
    {LIST_MIN(mixedList)}   // a
    {LIST_MAX(mixedList)}   // three 或 c，结果不固定
    {mixedList ? (a,b) }        // false
    {mixedList ^ LIST_ALL(a)}   // a, c

    { mixedList >= (one, a) }   // true
    { mixedList < (three) }     // false

	{ LIST_INVERT(mixedList) }            // one, b, two


## 7) 长示例：犯罪现场

最后，这里给出一个长示例以展示本节中许多概念在实际中的运作方式。建议在阅读之前把这段代码复制到 Ink 中先试玩一下，以便更好理解地理解各个环节的运作。

（译者注：这一节的代码翻译由 ChatGPT 完成。已在 Ink 中验证其可靠性。）

	-> murder_scene

	// 辅助函数：从列表中弹出元素
	=== function pop(ref list)
	~ temp x = LIST_MIN(list) 
	~ list -= x 
	~ return x

	//
	//  系统：物品可具有不同状态
	//  有些是通用状态，有些是特定物品的专有状态
	//

	LIST OffOn = off, on
	LIST SeenUnseen = unseen, seen

	LIST GlassState = (none), steamed, steam_gone
	LIST BedState = (made_up), covers_shifted, covers_off, bloodstain_visible

	//
	// 系统：库存
	//

	LIST Inventory = (none), cane, knife

	=== function get(x)
		~ Inventory += x

	//
	// 系统：物品位置管理
	// 物品可被放入或放置在不同位置
	//

	LIST Supporters = on_desk, on_floor, on_bed, under_bed, held, with_joe

	=== function move_to_supporter(ref item_state, new_supporter) ===
		~ item_state -= LIST_ALL(Supporters)
		~ item_state += new_supporter


	// 系统：递增式知识管理
	// 每个列表都是一条事实链，每个事实会取代之前的事实
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
		~ reach (statesToSet) 	// 设置列表中剩余状态
		~ return true  	        // 成功设置该状态，返回 true
	
		- else:
		~ return false || reach(statesToSet) 
		}	

	//
	// 游戏初始化
	//

	VAR bedroomLightState = (off, on_desk)

	VAR knifeState = (under_bed)


	//
	// 知识链
	//

	LIST BedKnowledge = neatly_made, crumpled_duvet, hastily_remade, body_on_bed, murdered_in_bed, murdered_while_asleep

	LIST KnifeKnowledge = prints_on_knife, joe_seen_prints_on_knife, joe_wants_better_prints, joe_got_better_prints

	LIST WindowKnowledge = steam_on_glass, fingerprints_on_glass, fingerprints_on_glass_match_knife

	//
	// 内容
	//

	=== murder_scene ===
		卧室。这就是案发地。现在该寻找线索了。
	- (top)
		{ bedroomLightState ? seen:     <- seen_light  }
		<- compare_prints(-> top)

	*   (dobed) [床……]
		床离地不高，但也不至于什么都滚不进去。它依旧被整齐地铺好。
		~ reach (neatly_made)
		- - (bedhub)
		* *     [掀开被子]
				我掀开了被子。被褥已经被压皱。
				~ reach (crumpled_duvet)
				~ BedState = covers_shifted
		* *     (uncover) {reached(crumpled_duvet)}
				[拿掉被子]
				小心翼翼地，我完全移开了被子，下方的被褥一片凌乱。
				这并非一位尽职的女仆所为，显然是匆忙间丢上的。
				~ reach (hastily_remade)
				~ BedState = covers_off
		* *     (duvet) {BedState == covers_off} [拉开被褥]
				我拉开了被褥，下面的床单上粘着血迹。
				~ BedState = bloodstain_visible
				~ reach (body_on_bed)
				不是尸体先被移到这里，就是这里正是案发地。
		* *     {BedState !? made_up} [重新整理床铺]
				我小心翼翼地把床单铺回原状，试图让它看起来毫无动过的痕迹。
				~ BedState = made_up
		* *     [测试床铺]
				我张开手指按了按床，床吱呀作响，但声响并不大。
		* *     (darkunder) [查看床下]
				我躺下来，往床下看去，但什么都看不清。

		* *     {TURNS_SINCE(-> dobed) > 1} [看看别处？]
				我从床边退后一步，环顾四周。
				-> top
		- -     -> bedhub

	*   {darkunder && bedroomLightState ? on_floor && bedroomLightState ? on}
		[查看床下]
		我往床下看去，有什么东西在闪光。
		- - (reaching)
		* *     [伸手去拿]
				我伸手到床下去够，但无论那是什么，已经被踢得太远够不到。
				-> reaching
		* *     {Inventory ? cane} [用手杖够]
				-> knock_with_cane

		* *     {reaching > 1 } [站起来]
				我再次站起身，拍了拍大衣。
				-> top

	*   (knock_with_cane) {reaching && TURNS_SINCE(-> reaching) >= 4 &&  Inventory ? cane } [用手杖够床下的东西]
		我用手杖对准地毯轻轻一挑，闪光的东西从床脚滑了出来。
		~ move_to_supporter( knifeState, on_floor )
		* *     (standup) [站起来]
				我满意地站起身，看到被挑出来的是一把带血的刀。
				-> top

		* *     [再次查看床下]
				我移开手杖，再次查看床下，但那里已经没有其他东西。
				-> standup

	*   {knifeState ? on_floor} [捡起刀]
		我小心翼翼地避开刀柄，将刀从地毯上拾起。
		~ get(knife)

	*   {Inventory ? knife} [查看刀]
		刀上的血迹已经干了，足够显露出刀柄上的部分指纹！
		~ reach (prints_on_knife)

	*   [书桌……]
		我把注意力转向书桌。一盏台灯放在一角，另一角是空空的收纳盘，桌面没有其他东西。
		一根木手杖斜靠在桌边。
		~ bedroomLightState += seen

		- - (deskstate)
		* *     (pickup_cane) {Inventory !? cane}  [捡起手杖]
				~ get(cane)
			我捡起了这根木手杖，它很沉，却没有任何标记。

		* *    { bedroomLightState !? on } [打开台灯]
				-> operate_lamp ->

		* *     [查看收纳盘]
				我看了看收纳盘，但里面什么都没有。要么是死者的文件被拿走了，要么他根本没什么业务，又或只是摆设。

		+ +     (open)  {open < 3} [打开抽屉]
				我{随便抽开一个|又拉开另一个|拉开第三个}抽屉，{锁着|也是锁着|果然也是锁着}。

		* *     {deskstate >= 2} [看看别处？]
				我再次从桌边退后一步。
				-> top

		- -     -> deskstate

	*     {(Inventory ? cane) && TURNS_SINCE(-> deskstate) <= 2} [挥动手杖]
		我仍握着手杖，轻轻挥了挥。它确实很沉，但不足以当作钝器使用。
		不过若是自卫时用上倒也合适。可死者当时为什么没有抓起它？或者碰倒它？
		
	*   [窗户……]
		我走到窗户旁，往外看去，只能见到房子旁潺潺流过的小溪。

		- - (window_opts)
		<- compare_prints(-> window_opts)
		* *     (downy) [往下看小溪]
				{ GlassState ? steamed:
					透过被雾气笼罩的玻璃，我看不清小溪。 -> see_prints_on_glass -> window_opts
				}
				我看着那条小溪匆匆流过。这栋房子大概有点潮湿，但除此之外，这景象并没有告诉我什么。
		* *     (greasy) [查看玻璃]
				{ GlassState ? steamed: -> downy }
				窗户上的玻璃很脏。里面外面都没人清理过。
		* *     { GlassState ? steamed && not see_prints_on_glass && downy && greasy }
				[查看雾气]
				外面很冷，自然我的呼吸会在玻璃上起雾。 -> see_prints_on_glass ->
		+ +     {GlassState ? steam_gone} [对着玻璃哈气]
				我轻轻对着玻璃哈了口气。{ reached (fingerprints_on_glass): 指纹又重新显现出来。 }
				~ GlassState = steamed

		+ +     [看看别处？]
				{ window_opts < 2 || reached (fingerprints_on_glass) || GlassState ? steamed:
					我从昏暗的玻璃上移开了视线。
					{GlassState ? steamed:
						~ GlassState = steam_gone
						<> 我呼出的雾气渐渐散去。
					}
					-> top
				}
				我从玻璃上靠了回去，我的呼吸在玻璃上凝起了一层雾。
			~ GlassState = steamed

		- -     -> window_opts

	*   {top >= 5} [离开房间]
		我看得够多了。我{bedroomLightState ? on:关掉了台灯，然后}转身离开了房间。
		-> joe_in_hall

	-   -> top


	= operate_lamp
		我按下了灯的开关。
		{ bedroomLightState ? on:
			<> 灯泡熄灭了。
			~ bedroomLightState += off
			~ bedroomLightState -= on
		- else:
			{ bedroomLightState ? on_floor: <> 灯光透过床下洒出一丝微光。} { bedroomLightState ? on_desk : <> 灯光在抛光的桌面上闪烁着光芒。 }
			~ bedroomLightState -= off
			~ bedroomLightState += on
		}
		->->


	= compare_prints (-> backto)
		*   { between ((fingerprints_on_glass, prints_on_knife), fingerprints_on_glass_match_knife) } 
	[对比刀上的指纹和窗户上的指纹]
			我拿着带血的刀靠近窗户，对着玻璃哈了口气让指纹再次显现，尽力进行对比。
			虽说这并不科学，但它们看起来非常相似——非常相似。
			~ reach (fingerprints_on_glass_match_knife)
			-> backto

	= see_prints_on_glass
		~ reach (fingerprints_on_glass)
		{但我能看见一些指纹，就像有人用手掌按过似的。|指纹非常清晰完整。} 当我注视时，它们渐渐消散。
		~ GlassState = steam_gone
		->->

	= seen_light
		*   {bedroomLightState !? on} [打开台灯]
			-> operate_lamp ->

		*   { bedroomLightState !? on_bed  && BedState ? bloodstain_visible }
			[把灯移到床上]
			~ move_to_supporter(bedroomLightState, on_bed)

			我把灯移到血迹处仔细观察。血已经渗透进棉质床单的纤维中。
			毫无疑问，凶手是在这里行凶的。
			~ reach (murdered_in_bed)

		*   { bedroomLightState !? on_desk } {TURNS_SINCE(-> floorit) >= 2 }
			[把灯移回桌子]
			~ move_to_supporter(bedroomLightState, on_desk)
			我把灯移回桌子，放回它原来的位置。
		*   (floorit) { bedroomLightState !? on_floor && darkunder }
			[把灯移到地上]
			~ move_to_supporter(bedroomLightState, on_floor)
			我把灯拾起，放到了地上。
		-   -> top

	=== joe_in_hall
		我的警察联系人乔正站在走廊里等我。“怎么样？”他问道，“你发现了什么有趣的东西吗？”
	- (found)
		*   {found == 1} “没有。”
			他耸了耸肩：“可惜。”
			-> done
		*   { Inventory ? knife } “我找到了凶器。”
			“干得好！”乔笑着回答，“我们以为凶手已经处理掉了它。我现在帮你封存起来。”
			~ move_to_supporter(knifeState, with_joe)

		*   {reached(prints_on_knife)} { knifeState ? with_joe }
			“刀上有指纹。”我告诉他。
			他仔细查看。
			“唔，不太完整，要比对起来有点困难。”
			~ reach (joe_seen_prints_on_knife)
		*   { reached((fingerprints_on_glass_match_knife, joe_seen_prints_on_knife)) }
			“刀上的指纹和窗户上的指纹是同一人留下的。”
			“谁都可能碰过窗户。”乔若有所思地回答，“但如果窗户上的指纹更完整，或许能帮我们找到匹配！”
			~ reach (joe_wants_better_prints)
		*   { between(body_on_bed, murdered_in_bed)}
			“尸体曾被移到床上，然后又被移回地面。”我告诉他。
			“为什么？”
			* *     “我不知道。”
					乔点点头：“好吧。”
			* *     “可能是为了从地上拿东西？”
					“没必要为了拿东西而搬动整具尸体。”
			* *     “可能是死在床上的。”
					“现在说什么都是猜测。”乔说。
		*   { reached(murdered_in_bed) }
			“受害者是在床上被谋杀的，随后尸体被移到了地上。”
			“为什么？”
			* *     “我不知道。”
					乔点点头：“好吧。”
			* *     “可能凶手想误导我们。”
					“怎么误导？”
				* * *   “想让我们以为受害者是清醒着遇害的。”我若有所思地回答，“好像他是见到了凶手才被杀。”
				* * *   “想让我们以为曾经发生过搏斗。”我回答，“让我们以为他不是在睡梦中被杀的。”
				- - -   “但如果真是在床上被杀，那很可能他是在睡觉时被刺杀的。”
						~ reach (murdered_while_asleep)
			* *     “可能凶手想清理现场。”
					“然后被打断了？也有可能。”

		*   { found > 1} “就这些。”
			“好吧，总算是个开始。”乔回答。
			-> done
		-   -> found
	-   (done)
		{
		- between(joe_wants_better_prints, joe_got_better_prints):
			~ reach (joe_got_better_prints)
			<> “我现在去把窗户上的指纹提取下来。”
		- reached(joe_seen_prints_on_knife):
			<> “我会尽量比对这些指纹。”
		- else:
			<> “线索不多。”
		}
		-> END


## 8) 总结

现在，我们来总结一下这个困难的章节，**Ink**的列表构造提供了：

### 标志（Flags）
*	每个列表条目是一个事件
*	使用 `+=` 来标记事件已发生
*	使用 `?` 和 `!?` 进行测试

示例：

	LIST GameEvents = foundSword, openedCasket, metGorgon
	{ GameEvents ? openedCasket }
	{ GameEvents ? (foundSword, metGorgon) }
	~ GameEvents += metGorgon

### 状态机（State machines）
*	每个列表条目是一个状态
*	使用 `=` 设置状态；使用 `++` 和 `--` 前进或后退
*	使用 `==`、`>` 等进行判定

示例：

	LIST PancakeState = ingredients_gathered, batter_mix, pan_hot, pancakes_tossed, ready_to_eat
	{ PancakeState == batter_mix }
	{ PancakeState < ready_to_eat }
	~ PancakeState++

### 属性（Properties）
*	每个列表是不同的属性，包含该属性可取的状态值（on/off，lit/unlit 等）
*	通过先移除旧状态，再添加新状态来改变状态
*	使用 `?` 和 `!?` 进行判定

示例：

	LIST OnOffState = on, off
	LIST ChargeState = uncharged, charging, charged

	VAR PhoneState = (off, uncharged)

	*	{ PhoneState !? uncharged } [插上手机充电]
		~ PhoneState -= LIST_ALL(ChargeState)
		~ PhoneState += charging
		你将手机插上开始充电。
	*	{ PhoneState ? (on, charged) } [给妈妈打电话]


# 第 6 部分：标识符中的国际字符支持｜Part 6: International character support in identifiers

默认情况下，Ink 在故事内容中使用非 ASCII 字符没有任何限制。然而，目前对常量、变量、针脚（Stitch）、转向（Divert）以及其他具名流程元素（即 标识符）的命名字符存在限制。

对于使用非 ASCII 语言（可以简单认为是 26 个英文字母和常见标点符号内）写作的作者来说，这意味着他们在编写故事时需要不断在 ASCII 命名与故事语言之间切换，十分不便。此外，使用作者本身语言为标识符命名，也有助于提升原始故事格式的整体可读性。

为帮助解决上述问题，Ink 自动支持一系列预定义的可用于标识符的非 ASCII 字符范围。一般来说，这些范围包含了官方 Unicode 字符范围中字母数字的子集，足以用于标识符命名。以下部分给出了 Ink 自动支持的非 ASCII 可用字符的详细信息。


### 支持的标识符字符

Ink 对额外字符范围的支持目前仅限于预定义的一组字符范围。

以下是当前支持的标识符字符范围列表：

- **阿拉伯语（Arabic）**
    
    启用阿拉伯语系语言的字符，是官方 *Arabic* Unicode 范围 `\u0600-\u06FF` 的子集。
    
- **亚美尼亚语（Armenian）**
    
    启用亚美尼亚语言的字符，是官方 *Armenian* Unicode 范围 `\u0530-\u058F` 的子集。
    
- **西里尔字母（Cyrillic）**
    
    启用使用西里尔字母语言的字符，是官方 *Cyrillic* Unicode 范围 `\u0400-\u04FF` 的子集。
    
- **希腊语（Greek）**
    
    启用使用希腊字母语言的字符，是官方 *Greek and Coptic* Unicode 范围 `\u0370-\u03FF` 的子集。
    
- **希伯来语（Hebrew）**
    
    启用使用希伯来字母语言的希伯来语字符，是官方 *Hebrew* Unicode 范围 `\u0590-\u05FF` 的子集。
    
- **拉丁字母扩展 A（Latin Extended A）**
    
    启用拉丁字母扩展范围的字符，完整对应官方 *Latin Extended-A* Unicode 范围 `\u0100-\u017F`。
    
- **拉丁字母扩展 B（Latin Extended B）**
    
    启用拉丁字母扩展范围的字符，完整对应官方 *Latin Extended-B* Unicode 范围 `\u0180-\u024F`。
    
- **拉丁字母补充（Latin 1 Supplement）**
    
    启用拉丁字母扩展范围的字符，完整对应官方 *Latin 1 Supplement* Unicode 范围 `\u0080 - \u00FF`。

**注意！** Ink 文件应以 UTF-8 格式保存，以确保支持上述字符范围。

果您希望在标识符中使用的特定但目前尚未支持的字符范围，欢迎在 Ink 主代码库提交 [issue](/inkle/ink/issues/new) 或提交 [pull request](/inkle/ink/pulls)。