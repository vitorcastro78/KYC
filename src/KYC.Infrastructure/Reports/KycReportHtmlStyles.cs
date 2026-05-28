namespace KYC.Infrastructure.Reports;

internal static class KycReportHtmlStyles
{
    public const string Css = """
        * { box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', system-ui, sans-serif;
            font-size: 11pt;
            line-height: 1.5;
            color: #212529;
            margin: 0;
            padding: 2rem 2.5rem;
        }
        header.doc-header {
            border-bottom: 3px solid #0d6efd;
            margin-bottom: 1.5rem;
            padding-bottom: 0.75rem;
        }
        header.doc-header h1 {
            font-size: 1.5rem;
            margin: 0 0 0.25rem;
            color: #0d6efd;
        }
        header.doc-header .meta { font-size: 0.85rem; color: #6c757d; }
        h2 {
            font-size: 1.15rem;
            margin: 1.5rem 0 0.75rem;
            color: #212529;
            border-bottom: 1px solid #dee2e6;
            padding-bottom: 0.25rem;
        }
        h3 { font-size: 1rem; margin: 1rem 0 0.5rem; color: #495057; }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 0.75rem 0 1rem;
            font-size: 0.95rem;
        }
        th, td {
            border: 1px solid #dee2e6;
            padding: 0.4rem 0.6rem;
            text-align: left;
        }
        th { background: #f8f9fa; font-weight: 600; }
        .alert-warning {
            background: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 0.25rem;
            padding: 0.75rem 1rem;
            margin: 0.75rem 0;
        }
        ul { margin: 0.5rem 0 1rem; padding-left: 1.25rem; }
        li { margin-bottom: 0.35rem; }
        .badge {
            display: inline-block;
            padding: 0.15rem 0.45rem;
            border-radius: 0.25rem;
            font-size: 0.8rem;
            font-weight: 600;
        }
        .badge-high { background: #f8d7da; color: #842029; }
        .badge-medium { background: #fff3cd; color: #664d03; }
        .badge-low { background: #d1e7dd; color: #0f5132; }
        .score-global { font-size: 1.1rem; font-weight: 700; }
        .footnote { font-size: 0.85rem; color: #6c757d; font-style: italic; }
        section.ai-summary {
            margin-top: 1.5rem;
            padding-top: 1rem;
            border-top: 2px dashed #adb5bd;
        }
        footer.doc-footer {
            margin-top: 2rem;
            padding-top: 0.75rem;
            border-top: 1px solid #dee2e6;
            font-size: 0.8rem;
            color: #6c757d;
        }
        @media print {
            body { padding: 1rem; }
        }
        """;
}
