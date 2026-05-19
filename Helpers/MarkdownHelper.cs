using Markdig;
using Microsoft.AspNetCore.Html;

namespace MyApp.Helpers;

public static class MarkdownHelper
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static IHtmlContent ToHtml(string? markdown) =>
        new HtmlString(Markdown.ToHtml(markdown ?? string.Empty, Pipeline));
}
