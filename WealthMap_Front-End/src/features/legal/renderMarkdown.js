/**
 * Renders the subset of Markdown the two legal documents actually use.
 *
 * A parser dependency would be the obvious move, but this input is not
 * arbitrary: it is two files in this repository, written by us, changed rarely,
 * and covered by a check that every construct they contain is one of the cases
 * below. That is a much smaller problem than "parse Markdown", and it keeps the
 * bundle free of a library that would exist for two static pages.
 *
 * Supported: headings, paragraphs, unordered lists, tables, blockquotes,
 * horizontal rules, bold, inline code, and links. Anything else is passed
 * through as text rather than guessed at.
 *
 * Input is escaped before any markup is added, so nothing in the source can
 * inject an element. That is defence for its own sake — the source is ours —
 * but it costs one function and removes the question entirely.
 */

const escapeHtml = (text) =>
  text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')

/**
 * Bold, code and links, applied after escaping.
 *
 * Links are restricted to http(s), mailto and in-repo relative paths. A
 * javascript: href in our own documents would be a mistake rather than an
 * attack, but it would still be a live one.
 *
 * The href stops at the first closing parenthesis, so a URL containing one is
 * not supported — Markdown requires it escaped anyway, and neither document has
 * such a link. Handling nesting properly would mean tracking depth for a case
 * that does not arise.
 */
const inline = (text) => {
  let html = escapeHtml(text)

  html = html.replace(/`([^`]+)`/g, '<code>$1</code>')
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')

  html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, (match, label, href) => {
    const safe = /^(https?:\/\/|mailto:|\/|\.\/|#)/i.test(href)
    if (!safe) return label

    const external = /^https?:\/\//i.test(href)
    const attributes = external ? ' target="_blank" rel="noopener noreferrer"' : ''

    return `<a href="${href}"${attributes}>${label}</a>`
  })

  return html
}

const tableRowCells = (line) =>
  line
    .replace(/^\||\|$/g, '')
    .split('|')
    .map((cell) => cell.trim())

const isTableDivider = (line) => /^\|?[\s:|-]+\|[\s:|-]*$/.test(line) && line.includes('-')

export function renderMarkdown(source) {
  // The DRAFT marker is an HTML comment for whoever opens the file; the page
  // shows the same warning as a banner instead, so the comment is dropped.
  const lines = source.replace(/<!--[\s\S]*?-->/g, '').split('\n')

  const html = []
  let index = 0

  while (index < lines.length) {
    const line = lines[index]

    if (line.trim() === '') {
      index += 1
      continue
    }

    if (/^---+$/.test(line.trim())) {
      html.push('<hr>')
      index += 1
      continue
    }

    const heading = line.match(/^(#{1,4})\s+(.*)$/)
    if (heading) {
      const level = heading[1].length
      html.push(`<h${level}>${inline(heading[2])}</h${level}>`)
      index += 1
      continue
    }

    if (line.startsWith('> ')) {
      const quoted = []
      while (index < lines.length && lines[index].startsWith('>')) {
        quoted.push(lines[index].replace(/^>\s?/, ''))
        index += 1
      }
      // Blank lines inside a quote separate paragraphs, which both documents use.
      const paragraphs = quoted
        .join('\n')
        .split(/\n\s*\n/)
        .filter((block) => block.trim() !== '')
        .map((block) => `<p>${inline(block.trim().replace(/\n/g, ' '))}</p>`)
        .join('')
      html.push(`<blockquote>${paragraphs}</blockquote>`)
      continue
    }

    if (/^[-*]\s+/.test(line)) {
      const items = []
      while (index < lines.length && /^[-*]\s+/.test(lines[index])) {
        let item = lines[index].replace(/^[-*]\s+/, '')
        index += 1
        // A wrapped list item continues on an indented line.
        while (index < lines.length && /^\s{2,}\S/.test(lines[index])) {
          item += ' ' + lines[index].trim()
          index += 1
        }
        items.push(`<li>${inline(item)}</li>`)
      }
      html.push(`<ul>${items.join('')}</ul>`)
      continue
    }

    if (line.startsWith('|') && isTableDivider(lines[index + 1] ?? '')) {
      const headers = tableRowCells(line)
        .map((cell) => `<th>${inline(cell)}</th>`)
        .join('')

      index += 2

      const rows = []
      while (index < lines.length && lines[index].startsWith('|')) {
        const cells = tableRowCells(lines[index])
          .map((cell) => `<td>${inline(cell)}</td>`)
          .join('')
        rows.push(`<tr>${cells}</tr>`)
        index += 1
      }

      html.push(
        `<div class="legal-doc__table-scroll"><table>` +
          `<thead><tr>${headers}</tr></thead><tbody>${rows.join('')}</tbody>` +
          `</table></div>`
      )
      continue
    }

    // Anything else is a paragraph, running until a blank line.
    const paragraph = []
    while (
      index < lines.length &&
      lines[index].trim() !== '' &&
      !/^(#{1,4}\s|[-*]\s|>|\|)/.test(lines[index]) &&
      !/^---+$/.test(lines[index].trim())
    ) {
      paragraph.push(lines[index].trim())
      index += 1
    }
    html.push(`<p>${inline(paragraph.join(' '))}</p>`)
  }

  return html.join('\n')
}
