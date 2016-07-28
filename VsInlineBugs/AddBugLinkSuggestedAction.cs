using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace MooreAaronD.InlineBugs
{
    internal sealed class AddBugLinkSuggestedAction : ISuggestedAction
    {
        private readonly ITrackingSpan _span;
        private readonly ITextSnapshot _snapshot;

        
        private string ReplacementValue { get; }

        public AddBugLinkSuggestedAction(ITrackingSpan span)
        {
            _span = span;
            _snapshot = span.TextBuffer.CurrentSnapshot;
            string text = span.GetText(_snapshot);
            ReplacementValue = Regex.Replace(text, "TODO", "TODO [BUG:<ID>]", RegexOptions.IgnoreCase);
               

            DisplayText = string.Format("Link this todo to a work item //TODO [bug:id]");
        }

        public string DisplayText { get; }

        public string IconAutomationText => null;

        ImageMoniker ISuggestedAction.IconMoniker => default(ImageMoniker);

        public string InputGestureText => null;

        public bool HasActionSets => false;

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken) => null;

        public bool HasPreview => true;

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            var textBlock = new TextBlock();
            textBlock.Padding = new Thickness(5);
            textBlock.Inlines.Add(new Run() { Text = ReplacementValue });
            return Task.FromResult<object>(textBlock);
        }

        public void Dispose()
        {
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _span.TextBuffer.Replace(_span.GetSpan(_snapshot), ReplacementValue);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}