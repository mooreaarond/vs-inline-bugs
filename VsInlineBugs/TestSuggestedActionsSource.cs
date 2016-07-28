using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MooreAaronD.InlineBugs
{
    class TestSuggestedActionsSource : ISuggestedActionsSource
    {
        private readonly TestSuggestedActionsSourceProvider _factory;
        private readonly ITextBuffer _textBuffer;
        private readonly ITextView _textView;

        internal static readonly Regex TodoNoBugRegex = new Regex(@"//\s*(?<todo>todo)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal static readonly Regex TodoWithBugRegex = new Regex(@"//\s*TODO\s*\[*BUG(\s*:\s*|\s*)\d+\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TestSuggestedActionsSource(TestSuggestedActionsSourceProvider testSuggestedActionsSourceProvider, ITextView textView, ITextBuffer textBuffer)
        {
            _factory = testSuggestedActionsSourceProvider;
            _textBuffer = textBuffer;
            _textView = textView;
        }

        #pragma warning disable 0067
        public event EventHandler<EventArgs> SuggestedActionsChanged;
        #pragma warning restore 0067

        public void Dispose()
        {
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            SnapshotSpan lineSnapshot;
            if (!TryGetTodoLineSnapshotSpan(out lineSnapshot))
                return Enumerable.Empty<SuggestedActionSet>();

            ITrackingSpan todoTrackingSpan = range.Snapshot.CreateTrackingSpan(lineSnapshot, SpanTrackingMode.EdgeInclusive);
            var addBugLinkAction = new AddBugLinkSuggestedAction(todoTrackingSpan);
            return new SuggestedActionSet[] { new SuggestedActionSet(new ISuggestedAction[] { addBugLinkAction }) };
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                SnapshotSpan lineSnapshot;
                return TryGetTodoLineSnapshotSpan(out lineSnapshot);
            });
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }

        private bool TryGetTodoLineSnapshotSpan(out SnapshotSpan line)
        {
            line = default(SnapshotSpan);

            ITextViewLine textViewLine = _textView.Caret.ContainingTextViewLine;
            if (textViewLine == null)
                return false;

            string text = textViewLine.Extent.GetText() ?? string.Empty;
            Match matchNoBug = TestSuggestedActionsSource.TodoNoBugRegex.Match(text);
            Match matchWithBug = TestSuggestedActionsSource.TodoWithBugRegex.Match(text);

            if (!matchNoBug.Success || matchWithBug.Success)
                return false;

            line = textViewLine.Extent;
            return true;
        }

        private bool TryGetWordUnderCaret(out TextExtent wordExtent)
        {
            ITextCaret caret = _textView.Caret;
            SnapshotPoint point;

            if (caret.Position.BufferPosition > 0)
            {
                point = caret.Position.BufferPosition - 1;
            }
            else
            {
                wordExtent = default(TextExtent);
                return false;
            }

            ITextStructureNavigator navigator = _factory.NavigatorService.GetTextStructureNavigator(_textBuffer);

            wordExtent = navigator.GetExtentOfWord(point);
            return true;
        }
    }
}
