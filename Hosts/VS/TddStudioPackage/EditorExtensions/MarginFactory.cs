﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using R4nd0mApps.TddStud10.Hosts.VS.TddStudioPackage.Extensions.Editor;

#if DONT_COMPILE

TODO:
- Unit Test
  x Glyph Factory - we will wait till the remaining come online
  x Canvas code - WPF code cannot be tested easily
  v DataStore code
  v Margin code
  v Margin Glyph painter
  - TestMarkerTagger code
-------------------
- Cannot check by str = "Discover Unit Tests" in datastore events
- Change in eventing infra 
  - RunStartEA, RunErrorEA, RunEndEA - make i tconsistent with runexecutor
  - RunErrorEA contains RSR
  - RunEndEA contains RSR and data
  - Engine steps don't update datastore
- datastore entities must be non-null always
- For new runs - we should merge right - when is the right time to pull that in?
- Engine events wire up 
  - Custom Trigger mechanism with 3 goals: exception in one handler should not affect the others
  - Combine attach/detach between EnginerLoader and TddStudi10Runner
  - Move to disposable model where we detach on dispose.
  - Get methods to attach from outside - dont expose events.
  - EngineHost, RunState, DataStore, ConsoleApp, [TBD:ToolWindow], etc.
- Move to async tagging
- Use SyncContext in Package class also
- SnapshotlineRange - tagger implementation assumes we we ask line by line and not for spans across multiplelines
- getMarkerTags - can detect sequence points but we arent instrumenting it.

- Spec questions
  - How do we deal with edited text [a] edit on a given line [b] shift lines up/down] 
  - if a sequence point is changed, its coverage data should be unknown: how does ncruch handle this?

- Infra questions
  - Should the tagger be disposable, if so who will call the IDispose?
  - Should I not be unsubscribing from the eventhanders in Margin
  - NormalizedSnapshotSpanCollection
    - Normalized means [a] sorted [b] overlaps combined [c] but not necessarily consecutive
    - Arg to GetTags - will never contain SnapshotSpan spanning multiple TextSnapshotLine-s
  - Span == Eucleadean Line Segment
  - SnapshotSpan == Span but within a text snapshot [not neessarily in the same text line]
  - SnapshotPoint == Point in a SnapshotSpan, also in a TextSnapshotLine
  - In Margin.TagsChangedEventArgs we are refetching all tags is that OK?
  - Ok to hold reference to ITextSnapshotLine in Tag?
  - Jared thinks LayoutEvent is too costly - what is the option?
  - s.Start.GetContainingLine().LineNumber - in GetTags is obviously not valid - as the snap could be the entire document.

==================

datastore
state [potentially corresponding to >1 state per line]


http://stackoverflow.com/questions/17167423/creating-a-tagger-that-has-more-than-one-tag-type-for-vs-extension/24923127#24923127	
https://github.com/qwertie/Loyc/blob/master/Visual%20Studio%20Integration/LoycExtensionForVs/SampleLanguage.cs



states
- unittest start
- unknown coverage
- uncovered
- partially covered failing tests
- partially covered all passing tests
- fully covered failing tests
- fully covered all passing tests
- test failure origin

Articles:
all of noahric blogs looks like
http://chrisparnin.github.io/articles/2013/09/using-tagging-and-adornments-for-better-todos-in-visual-studio/

Potentials:
https://github.com/EWSoftware/VSSpellChecker


===================

  mouse
- https://github.com/tunnelvisionlabs/InheritanceMargin/blob/f9f47148c7eb3de15fc92ca2ff372d266af63d4f/Tvl.VisualStudio.InheritanceMargin/InheritanceGlyphFactory.cs
  - command bindings on glyph
- [Export(typeof(IGlyphMouseProcessorProvider))]


selective of margin

blogs.msdn.com vs editor 

myltiple tag attr
- https://github.com/adamdriscoll/poshtools/blob/18eee4842c5643385bdd8db148b42d48d867c74e/ReplWindow/Repl/Margin/GlyphPrompts.cs

- getting access to service provider
    [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))]
    internal IServiceProvider _serviceProvider = null;

- move tddpackageextension one level up - refactor the projects
- keyboard input
- implement sort/remove using in fsharppowertools
- compress datastore size - esp the last one where unit tests are repeated


glyphs 
- test start, red/green, exception point 
- debug first failing tests, debug all tests
toolwindow - build breaks, list of test, error details
click on icon takes to toolwindow
misc
- c#, vb, fsharp
- signing
- solution folder
- IN_TDDSTUDIO + move tddstudio
- disable tdd should show '?' in the status bar
2 x katas
perf
- disable components
- fail gracefully on large projects
- vsvim, xunit
diagnostics
-------------------------------------
- incremental build [per assembly pipeline]
- nunit support
- cpp
- sublime text

#endif

namespace R4nd0mApps.TddStud10.Hosts.VS.EditorExtensions
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginConstants.Name)]
    [Order(After = PredefinedMarginNames.Outlining)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        [Import]
        private IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new Margin(
                textViewHost.TextView,
                aggregatorFactory.CreateTagAggregator<TestMarkerTag>(textViewHost.TextView.TextBuffer));
        }
    }
}