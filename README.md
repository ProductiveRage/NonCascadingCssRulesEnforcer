# The Non-Cascading-CSS Rules Enforcer

In my blog post "[Non-cascading CSS: A revolution!](http://www.productiverage.com/noncascading-css-a-revolution)", I discussed a set of guidelines that were intended to make it easier to write maintainable style sheets for large projects (utilising the [LESS pre-processor](http://lesscss.org/)). It was inspired by the articles [www.lispcast.com/cascading-separation-abstraction](http://www.lispcast.com/cascading-separation-abstraction) and [37signals.com/svn/posts/3003-css-taking-control-of-the-cascade](https://signalvnoise.com/posts/3003-css-taking-control-of-the-cascade) and recommended the following rules:

1. A standard (html5-element-supporting) reset sheet is compulsory, only bare selectors may be specified in it
1. A single "common" or "theme" sheet will be included with a minimum of default styles, only bare selectors may be specified in it
1. No bare selectors may occur in the non-reset-or-theme rules (a bare selector may occur within a nested selector so long as child selectors are strictly used)
1. Stylesheets are broken out into separate files, each with a well-defined purpose
1. All files other than the reset and theme sheets should be wrapped in a html "scope-restricting tag"
1. No selector may be repeated in the rules
1. All measurements are described in pixels
1. Margins, where specified, must always be fully-defined
1. Border and Padding may not be combined with Width (unless "border-box" is used)

Built using the [CssParser](https://bitbucket.org/DanRoberts/cssparser) library, this project allows the above rules to be applied to style sheets - if you were so desiring, you could include this validation in your deployment process and get the warm fuzzy feeling that you were (according to my own humble opinion) making strides towards easy-to-maintain styling; no more should you worry in the future "if I change this rule here, what elements might it affect other than the ones that I intend it to?" (the two guidelines "Stylesheets are broken out into separate files, each with a well-defined purpose" and "No selector may be repeated in the rules" go a long way toward managing this).

The solution includes a **CSSMinifierIntegration** project which allows the rules validation to be easily tied into the [CssMinifier](https://bitbucket.org/DanRoberts/cssminifier) (which allows for on-the-fly style sheet loading, combining, LESS processing, minifying and caching) or it may be used in isolation - possibly as part of a one-off build step for a deployment or just as a linting pass, to highlight any guidelines that aren't met as "suggestions".

The **Tester (set me as Startup Project)** project is a Console Application that applies the rules to the style sheets from my [blog](http://www.productiverage.com). Currently they all pass (as you would hope!) and so running the application happily reports

> No rules were broken - hurrah!

but you could try tweaking the .less files to introduce some problems. For example, Content.less starts like this:

	html
	{
		div.Content
		{
			position: relative;
			margin: 0 0 16px 0;
			padding: 16px 48px 16px 16px;
			width: 100%;
			box-sizing: border-box;

If you removed the rule

	box-sizing: border-box;
	
and re-ran the console app, then you'd be presented with the warning

> Style block encountered that combines border and/or padding with width in Content.less (line 7)

This is because the default box model is a little nuts when it comes to combining width with padding and so it's not recommended - for example, if the space for a container is 300px and it has a 100% width applied and a padding of 10px then the combined width will be 320px (300px + 10px left border + 10px right border) and so it won't fit into the available space. In that case you may have wanted to somehow say that width should be "100% - 20px" and that the border will be 10px, but that's not possible. Using "box-sizing: border-box" changes the box model so that everything works more intuitively (the element will be 300px wide total, with a 280px wide content area within 10px-either-side padding).

Since browser support for "border-box" stopped being a problem (I think that IE7 was the limiting factor), many people simply specify "border-box" for all elements with a "\*" rule - if you're one of these people then you wouldn't want to apply the "Border and Padding may not be combined with Width" rule. And that's ok, each rule is a separate class in the **NonCascadingCSSRulesEnforcer** project (and many of them are configurable), so you may pick and choose what does or doesn't seem applicable to your own needs.