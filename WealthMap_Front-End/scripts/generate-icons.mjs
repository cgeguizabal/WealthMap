/**
 * Builds the favicon and the installed-app icons from the two source marks.
 *
 *   node scripts/generate-icons.mjs
 *
 * Run it whenever `src/assets/Icon/*.png` changes. The outputs land in `public/`
 * and are committed — this is not part of the build, because regenerating
 * identical binaries on every `npm run build` would churn the diff for nothing.
 *
 * WHY THIS EXISTS AT ALL
 *
 * The source marks are 768×630 with transparent backgrounds. Neither property
 * survives contact with a launcher:
 *
 *   - Icons must be square. A non-square manifest icon gets letterboxed by the
 *     OS, usually onto whatever background it feels like.
 *   - iOS renders transparency in a home-screen icon as solid black, so the
 *     light mark — navy lettering — would be navy on black.
 *
 * So each output is the artwork centred on an opaque square, at the size the
 * platform asks for. Nothing is cropped and nothing is stretched.
 *
 * There is no image library here on purpose: this needs one composite of one
 * PNG format, and a dependency that ships a native binary is a poor trade for
 * a file that runs twice a year. It handles 8-bit RGBA non-interlaced PNG,
 * which is what the sources are, and says so loudly if given anything else.
 */

import { readFileSync, writeFileSync, mkdirSync } from 'node:fs'
import { deflateSync, inflateSync } from 'node:zlib'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const here = dirname(fileURLToPath(import.meta.url))
const root = join(here, '..')

// ── PNG ────────────────────────────────────────────────────────────

const SIGNATURE = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10])

const crcTable = Array.from({ length: 256 }, (_, n) => {
  let c = n
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1
  return c >>> 0
})

const crc32 = (buffer) => {
  let c = 0xffffffff
  for (const byte of buffer) c = crcTable[(c ^ byte) & 0xff] ^ (c >>> 8)
  return (c ^ 0xffffffff) >>> 0
}

/** Undoes one scanline filter. The five are defined in the PNG spec, §9.2. */
function unfilter(type, line, previous, bpp) {
  const out = Buffer.alloc(line.length)

  for (let i = 0; i < line.length; i++) {
    const raw = line[i]
    const a = i >= bpp ? out[i - bpp] : 0
    const b = previous ? previous[i] : 0
    const c = previous && i >= bpp ? previous[i - bpp] : 0

    let value
    switch (type) {
      case 0: value = raw; break
      case 1: value = raw + a; break
      case 2: value = raw + b; break
      case 3: value = raw + ((a + b) >> 1); break
      case 4: {
        // Paeth: pick whichever neighbour the gradient points at.
        const p = a + b - c
        const pa = Math.abs(p - a)
        const pb = Math.abs(p - b)
        const pc = Math.abs(p - c)
        value = raw + (pa <= pb && pa <= pc ? a : pb <= pc ? b : c)
        break
      }
      default: throw new Error(`Unknown filter type ${type}`)
    }

    out[i] = value & 0xff
  }

  return out
}

function decode(file) {
  const buffer = readFileSync(file)

  if (!buffer.subarray(0, 8).equals(SIGNATURE)) throw new Error(`${file} is not a PNG`)

  let offset = 8
  let header = null
  const data = []

  while (offset < buffer.length) {
    const length = buffer.readUInt32BE(offset)
    const type = buffer.toString('ascii', offset + 4, offset + 8)
    const body = buffer.subarray(offset + 8, offset + 8 + length)

    if (type === 'IHDR') {
      header = {
        width: body.readUInt32BE(0),
        height: body.readUInt32BE(4),
        depth: body[8],
        colour: body[9],
        interlace: body[12]
      }
    } else if (type === 'IDAT') data.push(body)
    else if (type === 'IEND') break

    offset += 12 + length
  }

  if (!header) throw new Error(`${file} has no IHDR`)

  if (header.depth !== 8 || header.colour !== 6 || header.interlace !== 0)
    throw new Error(
      `${file} is depth ${header.depth}, colour type ${header.colour}, ` +
      `interlace ${header.interlace}. This script only handles 8-bit RGBA, ` +
      `non-interlaced — re-export it that way or reach for a real image library.`)

  const raw = inflateSync(Buffer.concat(data))
  const bpp = 4
  const stride = header.width * bpp
  const pixels = Buffer.alloc(header.height * stride)

  let previous = null
  for (let y = 0; y < header.height; y++) {
    const start = y * (stride + 1)
    const line = raw.subarray(start + 1, start + 1 + stride)
    const decoded = unfilter(raw[start], line, previous, bpp)

    decoded.copy(pixels, y * stride)
    previous = decoded
  }

  return { width: header.width, height: header.height, pixels }
}

function encode({ width, height, pixels }) {
  const stride = width * 4
  const raw = Buffer.alloc(height * (stride + 1))

  // Filter 0 (None) on every line. The marks are large flat areas, so deflate
  // does the work regardless and this keeps the encoder honest.
  for (let y = 0; y < height; y++) {
    raw[y * (stride + 1)] = 0
    pixels.copy(raw, y * (stride + 1) + 1, y * stride, (y + 1) * stride)
  }

  const chunk = (type, body) => {
    const out = Buffer.alloc(12 + body.length)
    out.writeUInt32BE(body.length, 0)
    out.write(type, 4, 'ascii')
    body.copy(out, 8)
    out.writeUInt32BE(crc32(out.subarray(4, 8 + body.length)), 8 + body.length)
    return out
  }

  const ihdr = Buffer.alloc(13)
  ihdr.writeUInt32BE(width, 0)
  ihdr.writeUInt32BE(height, 4)
  ihdr[8] = 8    // depth
  ihdr[9] = 6    // RGBA
  ihdr[10] = 0   // deflate
  ihdr[11] = 0   // adaptive filtering
  ihdr[12] = 0   // no interlace

  return Buffer.concat([
    SIGNATURE,
    chunk('IHDR', ihdr),
    chunk('IDAT', deflateSync(raw, { level: 9 })),
    chunk('IEND', Buffer.alloc(0))
  ])
}

// ── Composition ────────────────────────────────────────────────────

const clamp = (v, lo, hi) => (v < lo ? lo : v > hi ? hi : v)

/** Bilinear, because nearest-neighbour turns the arrow's diagonal into stairs. */
function sample(source, x, y) {
  const x0 = clamp(Math.floor(x), 0, source.width - 1)
  const y0 = clamp(Math.floor(y), 0, source.height - 1)
  const x1 = clamp(x0 + 1, 0, source.width - 1)
  const y1 = clamp(y0 + 1, 0, source.height - 1)
  const fx = x - x0
  const fy = y - y0

  const at = (px, py) => (py * source.width + px) * 4
  const out = [0, 0, 0, 0]

  for (let c = 0; c < 4; c++) {
    const top = source.pixels[at(x0, y0) + c] * (1 - fx) + source.pixels[at(x1, y0) + c] * fx
    const bottom = source.pixels[at(x0, y1) + c] * (1 - fx) + source.pixels[at(x1, y1) + c] * fx
    out[c] = top * (1 - fy) + bottom * fy
  }

  return out
}

/**
 * The artwork centred on an opaque square.
 *
 * @param coverage how much of the square's width the artwork may occupy. The
 *   rest is padding — generous for the tile look, and much larger for maskable
 *   icons, where the OS crops to a circle and anything outside it is lost.
 */
function compose(source, size, background, coverage) {
  const pixels = Buffer.alloc(size * size * 4)

  for (let i = 0; i < size * size; i++) {
    pixels[i * 4] = background[0]
    pixels[i * 4 + 1] = background[1]
    pixels[i * 4 + 2] = background[2]
    pixels[i * 4 + 3] = 255
  }

  // Contained, not cropped: the mark is wider than it is tall, so width binds.
  const scale = Math.min(
    (size * coverage) / source.width,
    (size * coverage) / source.height)

  const drawWidth = Math.round(source.width * scale)
  const drawHeight = Math.round(source.height * scale)
  const left = Math.round((size - drawWidth) / 2)
  const top = Math.round((size - drawHeight) / 2)

  for (let y = 0; y < drawHeight; y++) {
    for (let x = 0; x < drawWidth; x++) {
      const [r, g, b, a] = sample(
        source,
        (x + 0.5) / scale - 0.5,
        (y + 0.5) / scale - 0.5)

      const alpha = a / 255
      if (alpha <= 0) continue

      const target = ((top + y) * size + (left + x)) * 4

      // Source-over onto the opaque background, so the result has no alpha for
      // a launcher to reinterpret as black.
      pixels[target] = Math.round(r * alpha + pixels[target] * (1 - alpha))
      pixels[target + 1] = Math.round(g * alpha + pixels[target + 1] * (1 - alpha))
      pixels[target + 2] = Math.round(b * alpha + pixels[target + 2] * (1 - alpha))
    }
  }

  return { width: size, height: size, pixels }
}

// ── The icons ──────────────────────────────────────────────────────

const hex = (value) => [
  parseInt(value.slice(1, 3), 16),
  parseInt(value.slice(3, 5), 16),
  parseInt(value.slice(5, 7), 16)
]

// Straight from _tokens.scss. The tile on screen and the tile on a home screen
// should not be two different shades of the same idea.
const SURFACE = hex('#FFFFFF')      // --tile-surface, light
const BRAND_NAVY = hex('#212F46')   // --brand-navy

const light = decode(join(root, 'src/assets/Icon/Icon_LightMode.png'))
const dark = decode(join(root, 'src/assets/Icon/Icon_DarkMode.png'))

const outputs = [
  // Installed app. The light mark on white: it has to read on a launcher
  // wallpaper of any colour, and iOS will not honour transparency anyway.
  ['icon-192.png', light, 192, SURFACE, 0.76],
  ['icon-512.png', light, 512, SURFACE, 0.76],
  ['apple-touch-icon.png', light, 180, SURFACE, 0.76],

  // Maskable: Android crops to a circle of 80% diameter and keeps whatever
  // survives. The mark is wider than tall, so its diagonal is what has to fit —
  // hence far more padding than the plain icons above.
  ['icon-maskable-512.png', light, 512, SURFACE, 0.58],

  // Browser tab. The dark mark on navy rather than on transparency: a tab strip
  // can be light or dark, and near-white lettering on a light strip is a blank
  // square. The navy makes it legible either way.
  ['favicon-32.png', dark, 32, BRAND_NAVY, 0.8],
  ['favicon-64.png', dark, 64, BRAND_NAVY, 0.8],
  ['favicon-180.png', dark, 180, BRAND_NAVY, 0.8]
]

mkdirSync(join(root, 'public'), { recursive: true })

for (const [name, source, size, background, coverage] of outputs) {
  const png = encode(compose(source, size, background, coverage))
  writeFileSync(join(root, 'public', name), png)
  console.log(`${name.padEnd(24)} ${size}×${size}  ${(png.length / 1024).toFixed(1)} KB`)
}
