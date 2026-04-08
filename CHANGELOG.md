# Changelog

## 4.1.0

- Added `StringComparison` overload support to `GetSectionsNamed()` and `GetSettingsNamed()`, while keeping the default behavior as `StringComparison.OrdinalIgnoreCase`.
- Clarified the public documentation around create-or-get indexers and name matching behavior (see README).
- Fixed stream ownership inconsistencies:
  - `LoadFromStream()` now leaves caller-provided streams open.
  - `LoadFromBinaryStream()` now leaves caller-provided `BinaryReader` instances and streams open.
  - `SaveToBinaryStream()` now leaves caller-provided `BinaryWriter` instances open.
  - Stream-based load/save APIs now consistently leave caller-owned streams usable after the operation.
  - Binary/text stream internals now use explicit `leaveOpen` handling instead of relying on disposal side effects.
- Reduced allocations in textual stream save by writing through `StreamWriter` instead of building and encoding an intermediate byte buffer.
- Cached reflection metadata used for object mapping to avoid repeated member scans without changing observable behavior.
- Removed the deprecated `Configuration.StringRepresentation` property.
