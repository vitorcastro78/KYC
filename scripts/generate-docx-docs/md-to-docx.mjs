import fs from "fs";
import path from "path";
import {
  Document,
  Packer,
  Paragraph,
  TextRun,
  HeadingLevel,
  Table,
  TableRow,
  TableCell,
  WidthType,
  BorderStyle,
  ShadingType,
  ImageRun,
  PageBreak,
  AlignmentType,
  Header,
  Footer,
  PageNumber,
  NumberFormat,
} from "docx";

const HEADING_MAP = {
  1: HeadingLevel.HEADING_1,
  2: HeadingLevel.HEADING_2,
  3: HeadingLevel.HEADING_3,
  4: HeadingLevel.HEADING_4,
  5: HeadingLevel.HEADING_5,
  6: HeadingLevel.HEADING_6,
};

function parseInline(text) {
  const runs = [];
  const re = /(\*\*[^*]+\*\*|`[^`]+`|\[[^\]]+\]\([^)]+\))/g;
  let last = 0;
  let m;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) runs.push(new TextRun(text.slice(last, m.index)));
    const token = m[0];
    if (token.startsWith("**")) {
      runs.push(new TextRun({ text: token.slice(2, -2), bold: true }));
    } else if (token.startsWith("`")) {
      runs.push(new TextRun({ text: token.slice(1, -1), font: "Consolas", size: 20 }));
    } else if (token.startsWith("[")) {
      const linkMatch = /\[([^\]]+)\]\(([^)]+)\)/.exec(token);
      if (linkMatch) runs.push(new TextRun({ text: linkMatch[1], underline: {} }));
    }
    last = m.index + token.length;
  }
  if (last < text.length) runs.push(new TextRun(text.slice(last)));
  if (runs.length === 0) runs.push(new TextRun(text));
  return runs;
}

function isTableRow(line) {
  return line.trim().startsWith("|") && line.trim().endsWith("|");
}

function parseTableRow(line) {
  return line
    .trim()
    .slice(1, -1)
    .split("|")
    .map((c) => c.trim());
}

function isSeparatorRow(cells) {
  return cells.every((c) => /^:?-+:?$/.test(c.replace(/\s/g, "")));
}

function mdTableToDocx(rows) {
  if (rows.length === 0) return null;
  const header = rows[0];
  const body = rows.slice(1);
  const colCount = header.length;
  const tableRows = [];

  tableRows.push(
    new TableRow({
      tableHeader: true,
      children: header.map(
        (cell) =>
          new TableCell({
            shading: { type: ShadingType.CLEAR, fill: "1F4E79" },
            children: [
              new Paragraph({
                children: [new TextRun({ text: cell.replace(/\*\*/g, ""), bold: true, color: "FFFFFF" })],
              }),
            ],
          })
      ),
    })
  );

  for (const row of body) {
    while (row.length < colCount) row.push("");
    tableRows.push(
      new TableRow({
        children: row.slice(0, colCount).map(
          (cell) =>
            new TableCell({
              children: [new Paragraph({ children: parseInline(cell) })],
            })
        ),
      })
    );
  }

  return new Table({
    width: { size: 100, type: WidthType.PERCENTAGE },
    rows: tableRows,
  });
}

export function markdownToChildren(md) {
  const lines = md.replace(/\r\n/g, "\n").split("\n");
  const children = [];
  let i = 0;

  while (i < lines.length) {
    const line = lines[i];

    if (line.trim() === "---") {
      children.push(new Paragraph({ children: [new PageBreak()] }));
      i++;
      continue;
    }

    if (line.startsWith("```")) {
      const lang = line.slice(3).trim();
      const codeLines = [];
      i++;
      while (i < lines.length && !lines[i].startsWith("```")) {
        codeLines.push(lines[i]);
        i++;
      }
      i++;
      const codeText = codeLines.join("\n");
      children.push(
        new Paragraph({
          spacing: { before: 120, after: 120 },
          shading: { type: ShadingType.CLEAR, fill: "F5F5F5" },
          children: [
            new TextRun({
              text: codeText,
              font: "Consolas",
              size: 18,
            }),
          ],
        })
      );
      continue;
    }

    if (isTableRow(line)) {
      const tableRows = [];
      while (i < lines.length && isTableRow(lines[i])) {
        const cells = parseTableRow(lines[i]);
        if (!isSeparatorRow(cells)) tableRows.push(cells);
        i++;
      }
      const table = mdTableToDocx(tableRows);
      if (table) children.push(table);
      children.push(new Paragraph({ text: "" }));
      continue;
    }

    const headingMatch = /^(#{1,6})\s+(.*)$/.exec(line);
    if (headingMatch) {
      const level = headingMatch[1].length;
      children.push(
        new Paragraph({
          heading: HEADING_MAP[level],
          children: parseInline(headingMatch[2]),
        })
      );
      i++;
      continue;
    }

    if (/^>\s/.test(line)) {
      const quoteLines = [];
      while (i < lines.length && /^>\s?/.test(lines[i])) {
        quoteLines.push(lines[i].replace(/^>\s?/, ""));
        i++;
      }
      children.push(
        new Paragraph({
          indent: { left: 720 },
          spacing: { before: 80, after: 80 },
          children: parseInline(quoteLines.join(" ")),
        })
      );
      continue;
    }

    if (/^[-*]\s/.test(line.trim()) || /^\d+\.\s/.test(line.trim())) {
      const items = [];
      while (i < lines.length) {
        const l = lines[i].trim();
        if (/^[-*]\s/.test(l) || /^\d+\.\s/.test(l)) {
          const text = l.replace(/^[-*]\s/, "").replace(/^\d+\.\s/, "");
          items.push(
            new Paragraph({
              bullet: { level: 0 },
              children: parseInline(text),
            })
          );
          i++;
        } else if (l === "") {
          i++;
          break;
        } else break;
      }
      children.push(...items);
      continue;
    }

    if (/^-\s\[[ xX]\]/.test(line.trim())) {
      const checked = /^-\s\[[xX]\]/.test(line.trim());
      const text = line.trim().replace(/^-\s\[[ xX]\]\s*/, "");
      children.push(
        new Paragraph({
          children: [
            new TextRun({ text: checked ? "☑ " : "☐ ", font: "Segoe UI Symbol" }),
            ...parseInline(text),
          ],
        })
      );
      i++;
      continue;
    }

    if (line.trim() === "") {
      i++;
      continue;
    }

    children.push(new Paragraph({ children: parseInline(line) }));
    i++;
  }

  return children;
}

export function createCoverPage(title, subtitle, meta = []) {
  return [
    new Paragraph({ spacing: { before: 2400 } }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      children: [new TextRun({ text: "KYC AI Platform", size: 28, color: "1F4E79" })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 400 },
      children: [new TextRun({ text: title, bold: true, size: 44, color: "1F4E79" })],
    }),
    ...(subtitle
      ? [
          new Paragraph({
            alignment: AlignmentType.CENTER,
            spacing: { before: 200 },
            children: [new TextRun({ text: subtitle, size: 24, italics: true, color: "666666" })],
          }),
        ]
      : []),
    ...meta.flatMap(([label, value]) => [
      new Paragraph({
        alignment: AlignmentType.CENTER,
        spacing: { before: 120 },
        children: [
          new TextRun({ text: `${label}: `, bold: true, size: 22 }),
          new TextRun({ text: value, size: 22 }),
        ],
      }),
    ]),
    new Paragraph({ children: [new PageBreak()] }),
  ];
}

export function createStandardDocument({ title, subtitle, meta, mdPath, repoRoot }) {
  const md = fs.readFileSync(mdPath, "utf8");
  const body = markdownToChildren(md);
  const cover = createCoverPage(title, subtitle, meta);

  return new Document({
    creator: "KYC Platform",
    title,
    description: subtitle || title,
    styles: {
      default: {
        document: {
          run: { font: "Calibri", size: 22 },
        },
      },
    },
    sections: [
      {
        properties: {
          page: {
            margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 },
          },
        },
        headers: {
          default: new Header({
            children: [
              new Paragraph({
                alignment: AlignmentType.RIGHT,
                children: [new TextRun({ text: title, size: 18, color: "888888", italics: true })],
              }),
            ],
          }),
        },
        footers: {
          default: new Footer({
            children: [
              new Paragraph({
                alignment: AlignmentType.CENTER,
                children: [
                  new TextRun({ text: "Página ", size: 18 }),
                  new TextRun({ children: [PageNumber.CURRENT], size: 18 }),
                  new TextRun({ text: " de ", size: 18 }),
                  new TextRun({ children: [PageNumber.TOTAL_PAGES], size: 18 }),
                ],
              }),
            ],
          }),
        },
        children: [...cover, ...body],
      },
    ],
  });
}

export async function writeDocument(doc, outputPath) {
  const buffer = await Packer.toBuffer(doc);
  fs.mkdirSync(path.dirname(outputPath), { recursive: true });
  fs.writeFileSync(outputPath, buffer);
  console.log(`  ✓ ${outputPath}`);
}

export function addImageSection(imagePath, caption, width = 500, height = 280) {
  if (!fs.existsSync(imagePath)) {
    return [
      new Paragraph({
        children: [new TextRun({ text: `[Imagem não encontrada: ${caption}]`, italics: true, color: "CC0000" })],
      }),
    ];
  }
  const data = fs.readFileSync(imagePath);
  return [
    new Paragraph({
      spacing: { before: 200, after: 80 },
      children: [new TextRun({ text: caption, bold: true, size: 22 })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      children: [
        new ImageRun({
          type: "png",
          data,
          transformation: { width, height },
        }),
      ],
    }),
  ];
}
