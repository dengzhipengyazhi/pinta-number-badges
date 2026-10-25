# Changelog

All notable changes to this project are documented in this file.

The format is loosely based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2026-10-25

### Added
- **Fill** option: turn it off to draw a marker-style ring with a transparent
  interior instead of a filled badge.
- **Hand** option: wobbly hand-drawn circle (jittered path, slight rotation,
  overlapping stroke ends) plus a handwriting-style number rendered with a
  system handwriting font (Segoe Print / Bradley Hand / Comic Sans MS).
- Automatic ring width when `Fill` is off and the width is set to 0.
- The hand-drawn shape uses a deterministic seed, so the live preview and the
  stamped result are identical.

### Changed
- Bumped add-in version to `1.1`.
- Settings now persist the new `Fill` and `Hand` options.

## [1.0.0] - 2026-10-25

### Added
- Initial release: a Pinta tool that stamps numbered circular badges onto the
  current layer.
- Options: number, auto-increment, radius, outline width.
- Live preview on the scratch tool layer (excluded from save/export).
- Unlimited undo support (one history item per badge).
- Bilingual UI strings (English / Simplified Chinese).
